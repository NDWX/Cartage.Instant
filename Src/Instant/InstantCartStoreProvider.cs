using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pug.Extensions;

using Application = Pug.Application;

namespace Pug.Cartage.Instant
{
	public class InstantCartStoreProvider : ICartInfoStoreProvider
	{
		Application.IApplicationUserSessionProvider userSessionProvider;

		public InstantCartStoreProvider(Application.IApplicationUserSessionProvider userSessionProvider)
		{
			this.userSessionProvider = userSessionProvider;

		}
		
		private Dictionary<string, TransientCart> GetCartStore()
		{
			Application.ApplicationUserSession userSession = userSessionProvider.CurrentSession;

			Dictionary<string, TransientCart> carts = userSession.Get<Dictionary<string, TransientCart>>("Cartage.Instant");

			if (carts == null)
			{
				carts = new Dictionary<string, TransientCart>();
				userSessionProvider.CurrentSession.Set<Dictionary<string, TransientCart>>("Cartage.Instant", carts);
			}
			return carts;
		}

		TransientCart GetCart(string cart)
		{
			Dictionary<string, TransientCart> carts = GetCartStore();

			TransientCart cartStore = carts[cart];

			if (cartStore == null)
			{
				throw new CartNotFound(cart);
			}

			return cartStore;
		}

		public bool CartExists(string identifier)
		{
			Dictionary<string, TransientCart> carts = GetCartStore();

			return carts.ContainsKey(identifier);
		}

		public void RegisterCart(string identifier)
		{
			Dictionary<string, TransientCart> carts = GetCartStore();
			
			if (carts.ContainsKey(identifier))
				throw new CartExists(identifier);			

			TransientCart cartStore = new TransientCart(identifier, userSessionProvider.CurrentSession.Identifier);

			carts.Add(identifier, cartStore);
		}

		string GetNewLineIdentifier()
		{
			byte[] binarySeed = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());

			string lineIdentifier = Pug.Base32.From(binarySeed).Replace("=", string.Empty);

			return lineIdentifier;
		}

		#region ICartInfoStoreProvider Members

		public string AddItem(string cart, string productCode, decimal quantity, IDictionary<string, string> attributes)
		{
			TransientCart cartStore = GetCart(cart);

			string lineIdentifier = GetNewLineIdentifier();

			CartLine newLine = new CartLine(lineIdentifier, productCode, attributes, quantity);
			cartStore.Lines.Add(lineIdentifier, newLine);

			return lineIdentifier;
		}

		public void DeleteLine(string cart, string identifier)
		{
			TransientCart cartStore = GetCart(cart);

			cartStore.Lines.Remove(identifier);
		}

		public ICollection<CartInfo> GetCarts(Range<DateTime> creationPeriod, Range<DateTime> modificationPeriod)
		{
			Application.ApplicationUserSession userSession = userSessionProvider.CurrentSession;
			Dictionary<string, TransientCart> carts = userSession.Get<Dictionary<string, TransientCart>>("Cartage.Instant");

			var filteredCarts = from cart in carts.Values 
								where (creationPeriod != null && cart.CreationTimestamp.IsWithin(creationPeriod)) &&
										(modificationPeriod != null && cart.ModificationTimestamp.IsWithin(modificationPeriod)) 
								select new CartInfo(cart.Identifier, cart.CreationTimestamp, cart.CreateUser, cart.ModificationTimestamp, cart.LastModifyUser);

			return filteredCarts.ToArray();
		}

		public CartInfo GetInfo(string identifier)
		{
			TransientCart cartStore = GetCart(identifier);

			return new CartInfo(identifier, cartStore.CreationTimestamp, cartStore.CreateUser, cartStore.ModificationTimestamp, cartStore.LastModifyUser);
		}

		public CartLine GetLine(string cart, string identifier)
		{
			TransientCart cartStore = GetCart(cart);

			if (!cartStore.Lines.ContainsKey(identifier))
				return null;

			return cartStore.Lines[identifier];
		}

		public ICollection<CartLine> GetLines(string cart)
		{
			TransientCart cartStore = GetCart(cart);

			return cartStore.Lines.Values.ToArray();
		}

		public void UpdateLine(string cart, string identifier, decimal quantity, IDictionary<string, string> attributes)
		{
			TransientCart cartStore = GetCart(cart);

			if (!cartStore.Lines.ContainsKey(identifier))
				throw new IndexOutOfRangeException("Line identifier is not known.");

			CartLine cartLine = cartStore.Lines[identifier];

			cartStore.Lines[identifier] = new CartLine(identifier, cartLine.ProductCode, attributes, quantity);
		}

		public void Clear(string cart)
		{
			TransientCart cartStore = GetCart(cart);
			cartStore.Lines.Clear();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion
	}
}

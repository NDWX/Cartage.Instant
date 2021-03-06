using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pug.Extensions;
using Application = Pug.Application;

namespace Pug.Cartage.Instant
{
	internal class InstantCartStore : ICartInfoStore
	{
		Application.IApplicationUserSessionProvider userSessionProvider;

		internal InstantCartStore(Application.IApplicationUserSessionProvider userSessionProvider)
		{
			this.userSessionProvider = userSessionProvider;
		}

		string GetNewLineIdentifier()
		{
			byte[] binarySeed = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());

			string lineIdentifier = Pug.Base32.From(binarySeed).Replace("=", string.Empty);

			return lineIdentifier;
		}

		private Dictionary<string, TransientCart> GetCartStore()
		{
			Application.ApplicationUserSession userSession = userSessionProvider.CurrentSession;

			Dictionary<string, TransientCart> carts = userSession.Get<Dictionary<string, TransientCart>>("Cartage.Instant");

			if (carts == null)
			{
				throw new MissingCartStore();
			}

			return carts;
		}

		TransientCart _GetCart(string cart)
		{
			Dictionary<string, TransientCart> carts = GetCartStore();

			TransientCart cartStore = carts[cart];

			if (cartStore == null)
			{
				throw new CartNotFound(cart);
			}

			return cartStore;
		}

		//public string AddItem(string cart, string productCode, decimal quantity, IDictionary<string, string> attributes)
		//{
		//    TransientCart cartStore = __GetCart(cart);

		//    string lineIdentifier = GetNewLineIdentifier();

		//    IDictionary<string, ICartLineAttributeInfo> lineAttributes = new Dictionary<string, ICartLineAttributeInfo>();

		//    foreach( string attribute in attributes.Keys)
		//        lineAttributes.Add(attribute, new CartLineAttributeInfo(attribute, attributes[attribute]));

		//    TransientCartLine newLine = new TransientCartLine(new CartLineInfo(lineIdentifier, productCode, quantity), lineAttributes);
		//    cartStore.Lines.Add(lineIdentifier, newLine);

		//    return lineIdentifier;
		//}

		//public ICartLine GetLine(string cart, string identifier)
		//{
		//    TransientCart cartStore = GetCart(cart);

		//    if (!cartStore.Lines.ContainsKey(identifier))
		//        return null;

		//    return cartStore.Lines[identifier];
		//}

		//public ICollection<ICartLine> GetLines(string cart)
		//{
		//    TransientCart cartStore = GetCart(cart);

		//    return cartStore.Lines.Values.ToArray();
		//}

		//public void UpdateLine(string cart, string identifier, decimal quantity, IDictionary<string, string> attributes)
		//{
		//    TransientCart cartStore = __GetCart(cart);

		//    if (!cartStore.Lines.ContainsKey(identifier))
		//        throw new IndexOutOfRangeException("Line identifier is not known.");

		//    ICartLine cartLine = cartStore.Lines[identifier];

		//    IDictionary<string, ICartLineAttributeInfo> lineAttributes = new Dictionary<string, ICartLineAttributeInfo>();

		//    foreach (string attribute in attributes.Keys)
		//        lineAttributes.Add(attribute, new CartLineAttributeInfo(attribute, attributes[attribute]));

		//    cartStore.Lines[identifier] = new TransientCartLine(new CartLineInfo(identifier, cartLine.Info.ProductCode, quantity), lineAttributes);
		//}

		//public void Clear(string cart)
		//{
		//    TransientCart cartStore = __GetCart(cart);
		//    cartStore.Lines.Clear();
		//}

		#region ICartInfoStoreProvider Members

		public bool CartExists(string identifier)
		{
			Dictionary<string, TransientCart> carts = GetCartStore();

			return carts.ContainsKey(identifier);
		}

		public void RegisterCart(string identifier, string user)
		{
			Dictionary<string, TransientCart> carts = GetCartStore();

			if (carts.ContainsKey(identifier))
				throw new CartExists(identifier);

			TransientCart cartStore = new TransientCart(identifier, userSessionProvider.CurrentSession.Identifier);

			carts.Add(identifier, cartStore);
		}

		public void DeleteCart(string identifier, string user)
		{
			GetCartStore().Remove(identifier);
		}

		public void DeleteLineAttribute(string cart, string line, string name, string user)
		{
			((TransientCartLine)_GetCart(cart).Lines[line]).DeleteAttribute(name);
		}

		public void DeleteLines(string cart, string user)
		{
			TransientCart cartStore = _GetCart(cart);
			cartStore.Lines.Clear();
		}

		public void DeleteLine(string cart, string identifier, string user)
		{
			TransientCart cartStore = _GetCart(cart);

			cartStore.Lines.Remove(identifier);
		}

		public ICollection<ICartInfo> GetCarts(Range<DateTime> creationPeriod, Range<DateTime> modificationPeriod)
		{
			Application.ApplicationUserSession userSession = userSessionProvider.CurrentSession;
			Dictionary<string, TransientCart> carts = userSession.Get<Dictionary<string, TransientCart>>("Cartage.Instant");

			var filteredCarts = from cart in carts.Values
								where (creationPeriod != null && cart.Info.Created.IsWithin(creationPeriod)) &&
										(modificationPeriod != null && cart.Info.LastModified.IsWithin(modificationPeriod))
								select new CartInfo(cart.Info.Identifier, cart.Info.Created, cart.Info.CreateUser, cart.Info.LastModified, cart.Info.LastModifyUser, cart.Info.IsFinalized);

			return filteredCarts.ToArray();
		}

		public ICartInfo GetCart(string identifier)
		{
			return _GetCart(identifier).Info;
		}

		public ICartLineInfo GetLine(string cart, string identifier)
		{
			TransientCart _cart = _GetCart(cart);

			if (!_cart.Lines.ContainsKey(identifier))
				throw new UnknownCartLine();

			return _cart.Lines[identifier].Info;
		}

		public IDictionary<string, ICartLineAttributeInfo> GetLineAttributes(string cart, string identifier)
		{
			TransientCart _cart = _GetCart(cart);

			if (!_cart.Lines.ContainsKey(identifier))
				throw new UnknownCartLine();

			return _cart.Lines[identifier].Attributes;
		}

		public ICollection<ICartLineInfo> GetLines(string cart)
		{
			return (from ICartLine line in _GetCart(cart).Lines.Values select line.Info).ToArray();
		}

		public void InsertLine(string cart, string identifier, string productCode, decimal quantity, string user)
		{
			_GetCart(cart).Lines.Add(identifier, new TransientCartLine(new CartLineInfo(identifier, productCode, quantity), new Dictionary<string, ICartLineAttributeInfo>()));
		}

		public void InsertLineAttribute(string cart, string line, string name, string value, string user)
		{
			TransientCart _cart = _GetCart(cart);

			if (!_cart.Lines.ContainsKey(line))
				throw new UnknownCartLine();

			((TransientCartLine)_cart.Lines[line]).SetAttribute(name, value);
		}

		public bool LineExists(string cart, string identifier)
		{
			return _GetCart(cart).Lines.ContainsKey(identifier);
		}

		public void SetLineAttribute(string cart, string line, string name, string value, string user)
		{
			TransientCart _cart = _GetCart(cart);

			if (!_cart.Lines.ContainsKey(line))
				throw new UnknownCartLine();

			((TransientCartLine)_cart.Lines[line]).SetAttribute(name, value);
		}

		public void UpdateLine(string cart, string identifier, decimal quantity, string user)
		{
			TransientCart _cart = _GetCart(cart);

			if (!_cart.Lines.ContainsKey(identifier))
				throw new UnknownCartLine();

			ICartLineInfo lineInfo = _cart.Lines[identifier].Info;

			((TransientCartLine)_cart.Lines[identifier]).Info = new CartLineInfo(identifier, lineInfo.ProductCode, quantity);
		}

		public void SetCartFinalized(string cart, string user)
		{
			TransientCart _cart = _GetCart(cart);

			ICartInfo info = _cart.Info;

			_cart.Info = new CartInfo(info.Identifier, info.Created, info.CreateUser, DateTime.Now, user, true);
		}

		public void BeginTransaction()
		{
		}

		public void CommitTransaction()
		{
		}

		public void EnlistInTransaction(System.Transactions.Transaction transaction)
		{
		}

		public void RollbackTransaction()
		{
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			userSessionProvider.CurrentSession.Remove<Dictionary<string, TransientCart>>("Cartage.Instant");
		}

		#endregion

	}
}

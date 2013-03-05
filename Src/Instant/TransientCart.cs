using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pug.Extensions;

using Application = Pug.Application;

namespace Pug.Cartage.Instant
{
	internal class TransientCart
	{
		ICartInfo info;
		Dictionary<string, ICartLine> lines;

		public TransientCart(string identifier, string createUser)
		{
			info = new CartInfo(identifier, DateTime.Now, createUser, DateTime.Now, createUser);

			lines = new Dictionary<string, ICartLine>();
		}

		public ICartInfo Info
		{
			get
			{
				return this.info;
			}
			internal set
			{
				this.info = value;
			}
		}

		public Dictionary<string, ICartLine> Lines
		{
			get
			{
				return lines;
			}
		}
	}
}

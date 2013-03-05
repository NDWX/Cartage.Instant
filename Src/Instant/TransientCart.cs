using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Application = Pug.Application;

namespace Pug.Cartage.Instant
{
	internal class TransientCart
	{
		string identifier;
		DateTime creationTimestamp, modificationTimestamp;
		string createUser, lastModifyUser;

		Dictionary<string, CartLine> lines;

		public TransientCart(string identifier, string createUser)
		{
			this.identifier = identifier;
			this.createUser = createUser;
			this.creationTimestamp = DateTime.Now;

			lines = new Dictionary<string, CartLine>();
		}

		public string Identifier
		{
			get
			{
				return identifier;
			}
		}
		public DateTime CreationTimestamp
		{
			get
			{
				return creationTimestamp;
			}
		}
		public string CreateUser
		{
			get
			{
				return createUser;
			}
		}
		public DateTime ModificationTimestamp
		{
			get
			{
				return modificationTimestamp;
			}
			set
			{
				modificationTimestamp = value;
			}
		}
		public string LastModifyUser
		{
			get
			{
				return lastModifyUser;
			}
			set
			{
				lastModifyUser = value;
			}
		}
		public Dictionary<string, CartLine> Lines
		{
			get
			{
				return lines;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pug.Extensions;
using Application = Pug.Application;

namespace Pug.Cartage.Instant
{
	internal class TransientCartLine : ICartLine
	{
		ICartLineInfo info;
		IDictionary<string, ICartLineAttributeInfo> attributes;

		public TransientCartLine(ICartLineInfo info, IDictionary<string, ICartLineAttributeInfo> attributes)
		{
			this.info = info;
			this.attributes = attributes;
		}

		public ICartLineInfo Info
		{
			get
			{
				return info;
			}
			internal set
			{
				info = value;
			}
		}

		public IDictionary<string, ICartLineAttributeInfo> Attributes
		{
			get
			{
				return attributes.ReadOnly();
			}
		}

		internal ICartLineAttributeInfo this[string name]
		{
			get
			{
				return this.attributes[name];
			}
		}

		internal void SetAttribute(string name, string value)
		{
			this.attributes[name] = new CartLineAttributeInfo(name, value);
		}

		internal void DeleteAttribute(string name)
		{
			this.attributes.Remove(name);
		}
	}
}

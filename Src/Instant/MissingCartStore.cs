using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pug.Extensions;
using Application = Pug.Application;

namespace Pug.Cartage.Instant
{
	public class MissingCartStore : Exception
	{
		public MissingCartStore()
			: base()
		{
		}

		public MissingCartStore(string message)
			: base(message)
		{
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pug.Extensions;
using Application = Pug.Application;

namespace Pug.Cartage.Instant
{
	internal class DummyDataStoreProvider : Pug.Application.Data.IDataAccessProvider
	{

		public Application.Data.DataExceptionHandler DataExceptionHandler
		{
			get { throw new NotImplementedException(); }
		}

		public System.Data.Common.DbProviderFactory DbProviderFactory
		{
			get { throw new NotImplementedException(); }
		}
	}
}

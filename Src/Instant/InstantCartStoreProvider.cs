using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pug.Extensions;

using Application = Pug.Application;

namespace Pug.Cartage.Instant
{
	public class InstantCartStoreProvider : Pug.Application.Data.ApplicationData<ICartInfoStore>
	{
		Application.IApplicationUserSessionProvider userSessionProvider;

		public InstantCartStoreProvider(Application.IApplicationUserSessionProvider userSessionProvider) 
			: base(string.Empty, new DummyDataStoreProvider())
		{
			this.userSessionProvider = userSessionProvider;
		}

		public override ICartInfoStore GetSession()
		{
			return new InstantCartStore(userSessionProvider);
		}
	}
}
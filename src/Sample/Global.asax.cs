using System;
using System.Web;
using JsonRpcHandler;
using JsonRpcHandler.Configuration;

namespace Sample
{
	public class Global : HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
			LambdaRpcConfiguration cfg = new LambdaRpcConfiguration()
				.Register<Service>("stringEcho", x => x.StringEcho(null))
				.Register<Service>("boolEcho", x => x.BoolEcho(false))
				.Register<Service>("arrayEcho", x => x.ArrayEcho(null))
				.Register<Service>("objectEcho", x => x.ObjectEcho(null))
				.Register<Service>("jObjectEcho", x => x.JObjectEcho(null))
				.Register<Service>("noParams", x => x.NoParams())
				.Register<Service>("numberEcho", x => x.NumberEcho(0));
			JsonRpcHttpHandler.SetConfiguration(cfg);
		}

		protected void Session_Start(object sender, EventArgs e) {}

		protected void Application_BeginRequest(object sender, EventArgs e) {}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {}

		protected void Application_Error(object sender, EventArgs e) {}

		protected void Session_End(object sender, EventArgs e) {}

		protected void Application_End(object sender, EventArgs e) {}
	}
}
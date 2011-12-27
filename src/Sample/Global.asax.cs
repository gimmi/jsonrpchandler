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
			JsonRpcHttpHandler.RpcConfiguration = new LambdaRpcConfiguration()
				.Register<Service>("Service.echo", x => x.Echo(null));
		}

		protected void Session_Start(object sender, EventArgs e) {}

		protected void Application_BeginRequest(object sender, EventArgs e) {}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {}

		protected void Application_Error(object sender, EventArgs e) {}

		protected void Session_End(object sender, EventArgs e) {}

		protected void Application_End(object sender, EventArgs e) {}
	}
}
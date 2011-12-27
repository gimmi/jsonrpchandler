using System;
using System.IO;
using System.Web;
using JsonRpcHandler.Configuration;
using JsonRpcHandler.ObjectFactory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonRpcHandler
{
	public class JsonRpcHttpHandler : IHttpHandler
	{
		public static IRpcConfiguration RpcConfiguration = new ExceptionRpcConfiguration();
		public static IObjectFactory ObjectFactory = new ActivatorObjectFactory();

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var request = new HttpRequest {
				HttpMethod = context.Request.HttpMethod,
				ContentType = context.Request.ContentType,
				Content = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding)
			};

			var response = new HttpResponse {
				Content = new StreamWriter(context.Response.OutputStream, context.Response.ContentEncoding)
			};

			Handle(request, response, new JsonRpcHandler(new ParametersParser(), RpcConfiguration, ObjectFactory, new MethodInvoker()));

			context.Response.Status = response.Status;
			context.Response.ContentType = response.ContentType;
		}

		public void Handle(HttpRequest req, HttpResponse resp, JsonRpcHandler jsonRpcHandler)
		{
			if(req.HttpMethod != "POST")
			{
				resp.Status = "405 Method Not Allowed";
				return;
			}
			if(req.ContentType != "application/json")
			{
				resp.Status = "415 Unsupported Media Type";
				return;
			}
			JToken json;
			try
			{
				json = JToken.ReadFrom(new JsonTextReader(req.Content));
			}
			catch(Exception e)
			{
				resp.Status = "400 Bad Request";
				resp.ContentType = "text/plain";
				resp.Content.WriteLine(e);
				return;
			}
			resp.Status = "200 OK";
			resp.ContentType = "application/json";
			using(var jsonWriter = new JsonTextWriter(resp.Content))
			{
				new JsonSerializer().Serialize(jsonWriter, jsonRpcHandler.Handle(json));
			}
		}
	}
}
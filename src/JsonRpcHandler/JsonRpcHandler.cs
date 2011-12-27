using System;
using System.Collections.Generic;
using System.Reflection;
using JsonRpcHandler.MethodResolver;
using JsonRpcHandler.ObjectFactory;
using Newtonsoft.Json.Linq;

namespace JsonRpcHandler
{
	public class JsonRpcHandler
	{
		private readonly IMethodResolver _methodResolver;
		private readonly IObjectFactory _objectFactory;
		private readonly MethodInvoker _methodInvoker;
		private readonly ParametersParser _parametersParser;

		public JsonRpcHandler(ParametersParser parametersParser, IMethodResolver methodResolver, IObjectFactory objectFactory, MethodInvoker methodInvoker)
		{
			_parametersParser = parametersParser;
			_methodResolver = methodResolver;
			_objectFactory = objectFactory;
			_methodInvoker = methodInvoker;
		}

		public virtual JToken Handle(JToken req)
		{
			return (req.Type == JTokenType.Array ? HandleBatchRequest(req) : HandleSingleRequest(req));
		}

		private JToken HandleBatchRequest(IEnumerable<JToken> reqs)
		{
			var resps = new JArray();
			foreach(JToken req in reqs)
			{
				resps.Add(HandleSingleRequest(req));
			}
			return resps;
		}

		private T GetPropertyValue<T>(string name, JToken reqToken, T def = default(T))
		{
			JToken val;
			if(reqToken.Type == JTokenType.Object && ((JObject)reqToken).TryGetValue(name, out val))
			{
				return val.Value<T>();
			}
			return def;
		}

		private JToken HandleSingleRequest(JToken reqToken)
		{
			int? id = null;
			try
			{
				id = GetPropertyValue<int?>("id", reqToken);
				MethodInfo methodInfo = _methodResolver.GetMethodInfo(GetPropertyValue<string>("method", reqToken));
				object[] parameters = _parametersParser.Parse(methodInfo.GetParameters(), GetPropertyValue("params", reqToken, new JArray()));
				object instance = _objectFactory.Resolve(methodInfo.DeclaringType);
				JToken result;
				try
				{
					result = _methodInvoker.Invoke(methodInfo, instance, parameters);
				}
				finally
				{
					_objectFactory.Release(instance);
				}
				return new JObject {
					{ "jsonrpc", "2.0" },
					{ "id", id },
					{ "result", result }
				};
			}
			catch(Exception e)
			{
				var error = new JObject {
					{ "code", -32603 },
					{ "message", e.Message }
				};
				return new JObject {
					{ "jsonrpc", "2.0" },
					{ "id", id },
					{ "error", error }
				};
			}
		}
	}
}
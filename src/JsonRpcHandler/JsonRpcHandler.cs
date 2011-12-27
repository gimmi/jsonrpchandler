using System;
using System.Collections.Generic;
using System.Reflection;
using JsonRpcHandler.Configuration;
using JsonRpcHandler.ObjectFactory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JsonRpcHandler
{
	public class JsonRpcHandler
	{
		private readonly IRpcConfiguration _rpcConfiguration;
		private readonly IObjectFactory _objectFactory;
		private readonly MethodInvoker _methodInvoker;
		private readonly ParametersParser _parametersParser;

		public JsonRpcHandler(ParametersParser parametersParser, IRpcConfiguration rpcConfiguration, IObjectFactory objectFactory, MethodInvoker methodInvoker)
		{
			_parametersParser = parametersParser;
			_rpcConfiguration = rpcConfiguration;
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

		private JToken HandleSingleRequest(JToken request)
		{
			var response = new JObject {
				{ "jsonrpc", "2.0" },
				{ "id", GetPropertyValue<int?>("id", request) }
			};
			try
			{
				var methodName = GetPropertyValue<string>("method", request);
				Type type = _rpcConfiguration.GetMethodType(methodName);
				MethodInfo methodInfo = _rpcConfiguration.GetMethodInfo(methodName);
				JToken result;
				result = Handle(request, type, methodInfo);
				response.Add("result", result);
			}
			catch(Exception e)
			{
				response.Add("error", new JObject {
					{ "code", -32603 },
					{ "message", e.Message }
				});
			}
			return response;
		}

		private JToken Handle(JToken request, Type type, MethodInfo methodInfo)
		{
			var jsonSerializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			object[] parameters = _parametersParser.Parse(methodInfo.GetParameters(), GetPropertyValue("params", request, new JArray()), jsonSerializer);
			object instance = _objectFactory.Resolve(type);
			JToken result;
			try
			{
				result = _methodInvoker.Invoke(methodInfo, instance, parameters, jsonSerializer);
			}
			finally
			{
				_objectFactory.Release(instance);
			}
			return result;
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
	}
}
using System;
using System.Collections.Generic;
using System.Reflection;
using JsonRpcHandler.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JsonRpcHandler
{
	public class JsonRpcHandler
	{
		private readonly IRpcConfiguration _rpcConfiguration;
		private readonly MethodInvoker _methodInvoker;
		private readonly RpcHandlerInterceptor _rpcHandlerInterceptor;
		private readonly ParametersParser _parametersParser;

		public JsonRpcHandler(ParametersParser parametersParser, IRpcConfiguration rpcConfiguration, MethodInvoker methodInvoker, RpcHandlerInterceptor rpcHandlerInterceptor)
		{
			_parametersParser = parametersParser;
			_rpcConfiguration = rpcConfiguration;
			_methodInvoker = methodInvoker;
			_rpcHandlerInterceptor = rpcHandlerInterceptor;
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
				MethodInfo method = _rpcConfiguration.GetMethodInfo(methodName);
				JToken result = null;
				_rpcHandlerInterceptor.Invoke(type, method, delegate (object instance, JsonSerializer jsonSerializer) {
					result = Handle(type, method, request, jsonSerializer, instance);
				});
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

		private JToken Handle(Type type, MethodInfo methodInfo, JToken request, JsonSerializer jsonSerializer, object instance)
		{
			jsonSerializer = jsonSerializer ?? new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			instance = instance ?? Activator.CreateInstance(type);
			object[] parameters = _parametersParser.Parse(methodInfo.GetParameters(), GetPropertyValue("params", request, new JArray()), jsonSerializer);
			return _methodInvoker.Invoke(methodInfo, instance, parameters, jsonSerializer);
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
using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonRpcHandler
{
	public class ParametersParser
	{
		public virtual object[] Parse(ParameterInfo[] parameterInfos, JToken data, JsonSerializer jsonSerializer)
		{
			switch(data.Type)
			{
				case JTokenType.Array:
					return ParseByPosition(parameterInfos, (JArray)data, jsonSerializer);
				case JTokenType.Object:
					return ParseByName(parameterInfos, (JObject)data, jsonSerializer);
				default:
					throw new Exception(string.Format("Cannot extract parameters from a {0}", data.Type));
			}
		}

		public virtual object[] ParseByPosition(ParameterInfo[] parameterInfos, JArray data, JsonSerializer jsonSerializer)
		{
			if(parameterInfos.Length != data.Count)
			{
				throw new Exception(string.Format("Method expect {0} parameter(s), but passed {1} parameter(s)", parameterInfos.Length, data.Count));
			}
			var parameters = new object[parameterInfos.Length];
			for(int i = 0; i < parameterInfos.Length; i++)
			{
				parameters[i] = Deserialize(parameterInfos[i], data[i], jsonSerializer);
			}
			return parameters;
		}

		public virtual object[] ParseByName(ParameterInfo[] parameterInfos, JObject data, JsonSerializer jsonSerializer)
		{
			var parameters = new object[parameterInfos.Length];
			for(int i = 0; i < parameterInfos.Length; i++)
			{
				JToken value;
				if(data.TryGetValue(parameterInfos[i].Name, out value))
				{
					parameters[i] = Deserialize(parameterInfos[i], value, jsonSerializer);
				}
				else
				{
					parameters[i] = GetDefaultValue(parameterInfos[i].ParameterType);
				}
			}
			return parameters;
		}

		private static object GetDefaultValue(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}

		private object Deserialize(ParameterInfo parameterInfo, JToken value, JsonSerializer jsonSerializer)
		{
			return jsonSerializer.Deserialize(new JTokenReader(value), parameterInfo.ParameterType);
		}
	}
}
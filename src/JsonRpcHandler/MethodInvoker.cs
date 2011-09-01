using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonRpcHandler
{
	public class MethodInvoker
	{
		private readonly JsonSerializer _jsonSerializer;

		public MethodInvoker(JsonSerializer jsonSerializer)
		{
			_jsonSerializer = jsonSerializer;
		}

		public virtual JToken Invoke(MethodInfo methodInfo, object instance, object[] parameters)
		{
			object result = methodInfo.Invoke(instance, parameters);
			using(var writer = new JTokenWriter())
			{
				_jsonSerializer.Serialize(writer, result);
				return writer.Token;
			}
		}
	}
}
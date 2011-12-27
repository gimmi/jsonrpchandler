using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonRpcHandler
{
	public class MethodInvoker
	{
		public virtual JToken Invoke(MethodInfo methodInfo, object instance, object[] parameters, JsonSerializer jsonSerializer)
		{
			object result = methodInfo.Invoke(instance, parameters);
			using(var writer = new JTokenWriter())
			{
				jsonSerializer.Serialize(writer, result);
				return writer.Token;
			}
		}
	}
}
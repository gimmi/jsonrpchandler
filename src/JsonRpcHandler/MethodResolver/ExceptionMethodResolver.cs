using System;
using System.Reflection;

namespace JsonRpcHandler.MethodResolver
{
	public class ExceptionMethodResolver : IMethodResolver
	{
		public MethodInfo Resolve(string name)
		{
			throw new Exception(typeof(JsonRpcHttpHandler).FullName + " not configured.");
		}
	}
}
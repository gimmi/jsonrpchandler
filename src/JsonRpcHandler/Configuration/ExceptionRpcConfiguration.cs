using System;
using System.Reflection;

namespace JsonRpcHandler.Configuration
{
	public class ExceptionRpcConfiguration : IRpcConfiguration
	{
		public MethodInfo GetMethodInfo(string name)
		{
			throw new Exception(typeof(JsonRpcHttpHandler).FullName + " not configured.");
		}

		public Type GetMethodType(string name)
		{
			throw new Exception(typeof(JsonRpcHttpHandler).FullName + " not configured.");
		}
	}
}
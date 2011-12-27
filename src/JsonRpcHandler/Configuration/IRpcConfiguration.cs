using System;
using System.Reflection;

namespace JsonRpcHandler.Configuration
{
	public interface IRpcConfiguration
	{
		MethodInfo GetMethodInfo(string name);
		Type GetMethodType(string name);
	}
}
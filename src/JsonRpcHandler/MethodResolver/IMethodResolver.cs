using System;
using System.Reflection;

namespace JsonRpcHandler.MethodResolver
{
	public interface IMethodResolver
	{
		MethodInfo GetMethodInfo(string name);
		Type GetMethodType(string name);
	}
}
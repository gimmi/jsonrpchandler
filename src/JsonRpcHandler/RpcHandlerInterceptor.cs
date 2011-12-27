using System;
using System.Reflection;

namespace JsonRpcHandler
{
	public delegate void RpcHandlerInterceptor(Type type, MethodInfo method, RpcHandlerInvoker invoker);
}
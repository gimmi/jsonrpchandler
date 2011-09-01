using System.Reflection;

namespace JsonRpcHandler.MethodResolver
{
	public interface IMethodResolver
	{
		MethodInfo Resolve(string name);
	}
}
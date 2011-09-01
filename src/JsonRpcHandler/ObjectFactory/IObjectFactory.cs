using System;

namespace JsonRpcHandler.ObjectFactory
{
	public interface IObjectFactory
	{
		object Resolve(Type type);
		void Release(object instance);
	}
}
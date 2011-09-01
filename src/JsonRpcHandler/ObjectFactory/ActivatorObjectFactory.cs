using System;

namespace JsonRpcHandler.ObjectFactory
{
	public class ActivatorObjectFactory : IObjectFactory
	{
		public virtual object Resolve(Type type)
		{
			return Activator.CreateInstance(type);
		}

		public virtual void Release(object instance) {}
	}
}
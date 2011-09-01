using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonRpcHandler.MethodResolver
{
	public class LambdaMethodResolver : IMethodResolver
	{
		private readonly IDictionary<string, MethodInfo> _methodInfos = new Dictionary<string, MethodInfo>();

		public MethodInfo Resolve(string name)
		{
			if(_methodInfos.ContainsKey(name))
			{
				return _methodInfos[name];
			}
			throw new Exception(string.Format("Method {0} not registered", name));
		}

		public LambdaMethodResolver Register(string name, Expression<Action> expression)
		{
			return Register(name, (LambdaExpression)expression);
		}

		public LambdaMethodResolver Register<T>(string name, Expression<Action<T>> expression)
		{
			return Register(name, (LambdaExpression)expression);
		}

		public LambdaMethodResolver Register<T, TResult>(string name, Expression<Func<T, TResult>> expression)
		{
			return Register(name, (LambdaExpression)expression);
		}

		public LambdaMethodResolver Register(string name, LambdaExpression expression)
		{
			var outermostExpression = expression.Body as MethodCallExpression;
			if(outermostExpression == null)
			{
				throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
			}
			_methodInfos.Add(name, outermostExpression.Method);
			return this;
		}
	}
}
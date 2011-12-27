using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonRpcHandler.Configuration
{
	public class LambdaRpcConfiguration : IRpcConfiguration
	{
		private readonly IDictionary<string, MethodMetadata> _methodInfos = new Dictionary<string, MethodMetadata>();

		public MethodInfo GetMethodInfo(string name)
		{
			return GetMethodMetadata(name).MethodInfo;
		}

		public Type GetMethodType(string name)
		{
			return GetMethodMetadata(name).Type;
		}

		private MethodMetadata GetMethodMetadata(string name)
		{
			if (!_methodInfos.ContainsKey(name))
			{
				throw new Exception(string.Format("Method {0} not registered", name));
				
			}
			return _methodInfos[name];
		}

		public LambdaRpcConfiguration Register<T>(string name, Expression<Action<T>> expression)
		{
			return Register(name, typeof(T), expression);
		}

		public LambdaRpcConfiguration Register<T, TResult>(string name, Expression<Func<T, TResult>> expression)
		{
			return Register(name, typeof(T), expression);
		}

		public LambdaRpcConfiguration Register(string name, Type type, LambdaExpression expression)
		{
			var outermostExpression = expression.Body as MethodCallExpression;
			if(outermostExpression == null)
			{
				throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
			}
			_methodInfos.Add(name, new MethodMetadata { Type = type, MethodInfo = outermostExpression.Method });
			return this;
		}

		private class MethodMetadata
		{
			public Type Type;
			public MethodInfo MethodInfo;
		}
	}
}
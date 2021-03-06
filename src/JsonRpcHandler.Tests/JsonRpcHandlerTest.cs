﻿using System;
using System.Reflection;
using JsonRpcHandler.Configuration;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;
using SharpTestsEx;

namespace JsonRpcHandler.Tests
{
	[TestFixture]
	public class JsonRpcHandlerTest
	{
		private JsonRpcHandler _target;
		private IRpcConfiguration _rpcConfiguration;
		private ParametersParser _parametersParser;
		private MethodInvoker _methodInvoker;
		private RpcHandlerInterceptor _rpcHandlerInterceptor;

		[SetUp]
		public void SetUp()
		{
			_rpcConfiguration = MockRepository.GenerateStub<IRpcConfiguration>();
			_parametersParser = MockRepository.GenerateStub<ParametersParser>();
			_methodInvoker = MockRepository.GenerateStub<MethodInvoker>();
			_rpcHandlerInterceptor = MockRepository.GenerateStub<RpcHandlerInterceptor>();
			_target = new JsonRpcHandler(_parametersParser, _rpcConfiguration, _methodInvoker, _rpcHandlerInterceptor);
		}

		[Test]
		public void Should_return_error_when_method_cannot_be_found()
		{
			_rpcConfiguration.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Throw(new Exception("method not found"));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'method not found' } }");
		}

		[Test]
		public void Should_return_error_when_parameters_cannot_be_parsed()
		{
			_rpcConfiguration.Stub(x => x.GetMethodType(Arg<string>.Is.Anything)).Return(typeof(Object));
			_rpcConfiguration.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Return(typeof(Object).GetMethod("ToString"));
			_parametersParser.Stub(x => x.Parse(Arg<ParameterInfo[]>.Is.Anything, Arg<JToken>.Is.Anything, Arg<JsonSerializer>.Is.Anything)).Throw(new Exception("cannot parse params"));
			_rpcHandlerInterceptor.Stub(x => x.Invoke(Arg<Type>.Is.Anything, Arg<MethodInfo>.Is.Anything, Arg<RpcHandlerInvoker>.Is.Anything)).Do(new RpcHandlerInterceptor((type, method, invoker) => invoker.Invoke()));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'cannot parse params' } }");
		}

		[Test]
		public void Should_return_error_when_service_instance_cannot_be_resolved()
		{
			_rpcConfiguration.Stub(x => x.GetMethodType(Arg<string>.Is.Anything)).Return(typeof(Object)); 
			_rpcConfiguration.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Return(typeof(Object).GetMethod("ToString"));
			_parametersParser.Stub(x => x.Parse(Arg<ParameterInfo[]>.Is.Anything, Arg<JToken>.Is.Anything, Arg<JsonSerializer>.Is.Anything)).Return(new object[0]);
			_rpcHandlerInterceptor.Stub(x => x.Invoke(Arg<Type>.Is.Anything, Arg<MethodInfo>.Is.Anything, Arg<RpcHandlerInvoker>.Is.Anything)).Do(new RpcHandlerInterceptor(delegate {
				throw new Exception("cannot resolve service");
			}));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'cannot resolve service' } }");
		}

		[Test]
		public void Should_return_error_method_invocation_throws_error()
		{
			_rpcConfiguration.Stub(x => x.GetMethodType(Arg<string>.Is.Anything)).Return(typeof(Object));
			_rpcConfiguration.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Return(typeof(Object).GetMethod("ToString"));
			_parametersParser.Stub(x => x.Parse(Arg<ParameterInfo[]>.Is.Anything, Arg<JToken>.Is.Anything, Arg<JsonSerializer>.Is.Anything)).Return(new object[0]);
			_rpcHandlerInterceptor.Stub(x => x.Invoke(Arg<Type>.Is.Anything, Arg<MethodInfo>.Is.Anything, Arg<RpcHandlerInvoker>.Is.Anything)).Do(new RpcHandlerInterceptor((type, method, invoker) => invoker.Invoke()));
			_methodInvoker.Stub(x => x.Invoke(Arg<MethodInfo>.Is.Anything, Arg<object>.Is.Anything, Arg<Object[]>.Is.Anything, Arg<JsonSerializer>.Is.Anything)).Throw(new Exception("invocation error"));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'invocation error' } }");
		}

		[Test]
		public void Should_return_inner_error_when_invocation_throws_targetinvocationexception()
		{
			_rpcConfiguration.Stub(x => x.GetMethodType(Arg<string>.Is.Anything)).Return(typeof(Object));
			_rpcConfiguration.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Return(typeof(Object).GetMethod("ToString"));
			_parametersParser.Stub(x => x.Parse(Arg<ParameterInfo[]>.Is.Anything, Arg<JToken>.Is.Anything, Arg<JsonSerializer>.Is.Anything)).Return(new object[0]);
			_rpcHandlerInterceptor.Stub(x => x.Invoke(Arg<Type>.Is.Anything, Arg<MethodInfo>.Is.Anything, Arg<RpcHandlerInvoker>.Is.Anything)).Do(new RpcHandlerInterceptor((type, method, invoker) => invoker.Invoke()));
			_methodInvoker.Stub(x => x.Invoke(Arg<MethodInfo>.Is.Anything, Arg<object>.Is.Anything, Arg<Object[]>.Is.Anything, Arg<JsonSerializer>.Is.Anything)).Throw(new TargetInvocationException("reflection exception", new Exception("invocation error")));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'invocation error' } }");
		}

		[Test]
		public void Should_process_correct_request()
		{
			MethodInfo methodInfo = typeof(Object).GetMethod("ToString");
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			var instance = new object();
			var parameters = new object[0];

			_rpcConfiguration.Stub(x => x.GetMethodInfo("MethodName")).Return(methodInfo);
			_rpcConfiguration.Stub(x => x.GetMethodType("MethodName")).Return(typeof(Object));
			_parametersParser.Stub(x => x.Parse(Arg<ParameterInfo[]>.Is.Equal(parameterInfos), Arg<JToken>.Is.Equal(JToken.Parse("[ 1, 2, 3 ]")), Arg<JsonSerializer>.Is.Anything)).Return(parameters);
			_rpcHandlerInterceptor.Stub(x => x.Invoke(Arg<Type>.Is.Anything, Arg<MethodInfo>.Is.Anything, Arg<RpcHandlerInvoker>.Is.Anything)).Do(new RpcHandlerInterceptor((type, method, invoker) => invoker.Invoke(instance)));
			_methodInvoker.Stub(x => x.Invoke(Arg<MethodInfo>.Is.Equal(methodInfo), Arg<object>.Is.Equal(instance), Arg<ParameterInfo[]>.Is.Equal(parameters), Arg<JsonSerializer>.Is.Anything)).Return(JToken.Parse("456"));

			EvaluateBatch("{ id: 123, method: 'MethodName', params: [ 1, 2, 3 ] }", "{ jsonrpc: '2.0', id: 123, result: 456 }");
		}

		private void Evaluate(string req, string resp)
		{
			req = req.Replace('\'', '"');
			resp = resp.Replace('\'', '"');

			_target.Handle(JToken.Parse(req)).ToString().Should().Be.EqualTo(JToken.Parse(resp).ToString());
		}

		private void EvaluateBatch(string req, string resp)
		{
			Evaluate(req, resp);
			Evaluate("[" + req + "]", "[" + resp + "]");
		}
	}
}
using System;
using System.Reflection;
using JsonRpcHandler.MethodResolver;
using JsonRpcHandler.ObjectFactory;
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
		private IMethodResolver _methodResolver;
		private ParametersParser _parametersParser;
		private ActivatorObjectFactory _objectFactory;
		private MethodInvoker _methodInvoker;

		[SetUp]
		public void SetUp()
		{
			_methodResolver = MockRepository.GenerateStub<IMethodResolver>();
			_parametersParser = MockRepository.GenerateStub<ParametersParser>((JsonSerializer)null);
			_objectFactory = MockRepository.GenerateStub<ActivatorObjectFactory>();
			_methodInvoker = MockRepository.GenerateStub<MethodInvoker>((JsonSerializer)null);
			_target = new JsonRpcHandler(_parametersParser, _methodResolver, _objectFactory, _methodInvoker);
		}

		[Test]
		public void Should_return_error_when_method_cannot_be_found()
		{
			_methodResolver.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Throw(new Exception("method not found"));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'method not found' } }");
		}

		[Test]
		public void Should_return_error_when_parameters_cannot_be_parsed()
		{
			_methodResolver.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Return(typeof(Object).GetMethod("ToString"));
			_parametersParser.Stub(x => x.Parse(Arg<ParameterInfo[]>.Is.Anything, Arg<JToken>.Is.Anything)).Throw(new Exception("cannot parse params"));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'cannot parse params' } }");
		}

		[Test]
		public void Should_return_error_when_service_instance_cannot_be_resolved()
		{
			_methodResolver.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Return(typeof(Object).GetMethod("ToString"));
			_parametersParser.Stub(x => x.Parse(Arg<ParameterInfo[]>.Is.Anything, Arg<JToken>.Is.Anything)).Return(new object[0]);
			_objectFactory.Stub(x => x.Resolve(Arg<Type>.Is.Anything)).Throw(new Exception("cannot resolve service"));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'cannot resolve service' } }");
		}

		[Test]
		public void Should_return_error_method_invocation_throws_error()
		{
			_methodResolver.Stub(x => x.GetMethodInfo(Arg<string>.Is.Anything)).Return(typeof(Object).GetMethod("ToString"));
			_parametersParser.Stub(x => x.Parse(Arg<ParameterInfo[]>.Is.Anything, Arg<JToken>.Is.Anything)).Return(new object[0]);
			_objectFactory.Stub(x => x.Resolve(Arg<Type>.Is.Anything)).Return(new object());
			_methodInvoker.Stub(x => x.Invoke(Arg<MethodInfo>.Is.Anything, Arg<object>.Is.Anything, Arg<Object[]>.Is.Anything)).Throw(new Exception("invocation error"));

			EvaluateBatch("{ method: 'NotExistingMethod' }", "{ jsonrpc: '2.0', id: null, error: { code: -32603, message: 'invocation error' } }");
		}

		[Test]
		public void Should_process_correct_request()
		{
			Type type = typeof(Object);
			MethodInfo methodInfo = type.GetMethod("ToString");
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			var instance = new object();
			var parameters = new object[0];

			_methodResolver.Stub(x => x.GetMethodInfo("MethodName")).Return(methodInfo);
			_methodResolver.Stub(x => x.GetMethodType("MethodName")).Return(type);
			_parametersParser.Stub(x => x.Parse(parameterInfos, JToken.Parse("[ 1, 2, 3 ]"))).Return(parameters);
			_objectFactory.Stub(x => x.Resolve(type)).Return(instance);
			_methodInvoker.Stub(x => x.Invoke(methodInfo, instance, parameters)).Return(JToken.Parse("456"));

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
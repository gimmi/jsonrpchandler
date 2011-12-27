using System;
using JsonRpcHandler.Configuration;
using NUnit.Framework;
using SharpTestsEx;

namespace JsonRpcHandler.Tests.Configuration
{
	[TestFixture]
	public class LambdaRpcConfigurationTest
	{
		private LambdaRpcConfiguration _target;

		[SetUp]
		public void SetUp()
		{
			_target = new LambdaRpcConfiguration();
		}

		[Test]
		public void Should_return_defined_type()
		{
			_target.Register<TestClass>("Service.toString", x => x.ToString());
			_target.GetMethodInfo("Service.toString").Should().Be.SameInstanceAs(typeof(object).GetMethod("ToString"));
			_target.GetMethodInfo("Service.toString").Should().Not.Be.SameInstanceAs(typeof(TestClass).GetMethod("ToString"));
			_target.GetMethodType("Service.toString").Should().Be.EqualTo(typeof(TestClass));
		}

		[Test]
		public void Should_register_methods_with_no_parameters_and_no_return_value()
		{
			_target.Register<TestClass>("Service.noParNoRet", x => x.NoParNoRet());
			_target.GetMethodInfo("Service.noParNoRet").Should().Be.SameInstanceAs(typeof(TestClass).GetMethod("NoParNoRet"));
		}

		[Test]
		public void Should_register_methods_with_no_parameters_and_return_value()
		{
			_target.Register<TestClass>("Service.noParWithRet", x => x.NoParWithRet());
			_target.GetMethodInfo("Service.noParWithRet").Should().Be.SameInstanceAs(typeof(TestClass).GetMethod("NoParWithRet"));
		}

		[Test]
		public void Should_register_methods_with_parameters_and_no_return_value()
		{
			_target.Register<TestClass>("Service.withParNoRet", x => x.WithParNoRet(default(int), default(string)));
			_target.GetMethodInfo("Service.withParNoRet").Should().Be.SameInstanceAs(typeof(TestClass).GetMethod("WithParNoRet"));
		}

		[Test]
		public void Should_register_methods_with_parameters_and_return_value()
		{
			_target.Register<TestClass>("Service.withParWithRet", x => x.WithParWithRet(default(int), default(string)));
			_target.GetMethodInfo("Service.withParWithRet").Should().Be.SameInstanceAs(typeof(TestClass).GetMethod("WithParWithRet"));
		}

		[Test]
		public void Should_throw_exception_when_method_not_registered()
		{
			Executing.This(() => _target.GetMethodInfo("NotRegistered")).Should().Throw<Exception>().And.ValueOf.Message.Should().Be.EqualTo("Method NotRegistered not registered");
		}

		public class TestClass
		{
			public void NoParNoRet() {}

			public int NoParWithRet()
			{
				return 0;
			}

			public void WithParNoRet(int p1, string p2) {}

			public string WithParWithRet(int p1, string p2)
			{
				return null;
			}

			public override string ToString()
			{
				return "";
			}
		}
	}
}
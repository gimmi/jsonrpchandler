using System;
using JsonRpcHandler.MethodResolver;
using NUnit.Framework;
using SharpTestsEx;

namespace JsonRpcHandler.Tests.MethodResolver
{
	[TestFixture]
	public class LambdaMethodResolverTest
	{
		private LambdaMethodResolver _target;

		[SetUp]
		public void SetUp()
		{
			_target = new LambdaMethodResolver();
		}

		[Test]
		public void Should_register_methods_with_no_parameters_and_no_return_value()
		{
			_target.Register<TestClass>("Service.noParNoRet", x => x.NoParNoRet());
			_target.Resolve("Service.noParNoRet").Should().Be.SameInstanceAs(typeof(TestClass).GetMethod("NoParNoRet"));
		}

		[Test]
		public void Should_register_methods_with_no_parameters_and_return_value()
		{
			_target.Register<TestClass>("Service.noParWithRet", x => x.NoParWithRet());
			_target.Resolve("Service.noParWithRet").Should().Be.SameInstanceAs(typeof(TestClass).GetMethod("NoParWithRet"));
		}

		[Test]
		public void Should_register_methods_with_parameters_and_no_return_value()
		{
			_target.Register<TestClass>("Service.withParNoRet", x => x.WithParNoRet(default(int), default(string)));
			_target.Resolve("Service.withParNoRet").Should().Be.SameInstanceAs(typeof(TestClass).GetMethod("WithParNoRet"));
		}

		[Test]
		public void Should_register_methods_with_parameters_and_return_value()
		{
			_target.Register<TestClass>("Service.withParWithRet", x => x.WithParWithRet(default(int), default(string)));
			_target.Resolve("Service.withParWithRet").Should().Be.SameInstanceAs(typeof(TestClass).GetMethod("WithParWithRet"));
		}

		[Test]
		public void Should_throw_exception_when_method_not_registered()
		{
			Executing.This(() => _target.Resolve("NotRegistered")).Should().Throw<Exception>().And.ValueOf.Message.Should().Be.EqualTo("Method NotRegistered not registered");
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
		}
	}
}
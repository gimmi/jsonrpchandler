﻿using System;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpTestsEx;

namespace JsonRpcHandler.Tests
{
	[TestFixture]
	public class ParameterValuesParserTest
	{
		private ParametersParser _target;

		[SetUp]
		public void SetUp()
		{
			_target = new ParametersParser();
		}

		[Test]
		public void Should_parse_positional_arguments()
		{
			object[] actual = _target.ParseByPosition(GetType().GetMethod("ExampleMethod").GetParameters(), new JArray("v1", 123, true), new JsonSerializer());
			actual.Should().Have.SameSequenceAs(new object[] { "v1", 123, true });
		}

		[Test]
		public void Should_throw_exception_when_positional_argument_count_does_not_match()
		{
			Executing.This(() => _target.ParseByPosition(GetType().GetMethod("ExampleMethod").GetParameters(), new JArray("v1", 123), new JsonSerializer())).Should().Throw<Exception>().And.Exception.Message.Should().Be.EqualTo("Method expect 3 parameter(s), but passed 2 parameter(s)");
		}

		[Test]
		public void Should_parse_named_arguments()
		{
			object[] actual = _target.ParseByName(GetType().GetMethod("ExampleMethod").GetParameters(), new JObject { { "p3", true }, { "p2", 123 }, { "p1", "v1" } }, new JsonSerializer());
			actual.Should().Have.SameSequenceAs(new object[] { "v1", 123, true });
		}

		[Test]
		public void Should_use_type_default_value_when_named_arguments_are_missing()
		{
			object[] actual = _target.ParseByName(GetType().GetMethod("ExampleMethod").GetParameters(), new JObject(), new JsonSerializer());
			actual[0].Should().Be.Null();
			actual[1].Should().Be.EqualTo(0);
			actual[2].Should().Be.EqualTo(false);
		}

		[Test]
		public void Should_handle_parameters_of_jtoken_type_as_expected()
		{
			var jObject = new JObject { { "prop", new JValue("val") } };
			var jArray = new JArray(new JValue(1));
			object[] actual = _target.ParseByPosition(GetType().GetMethod("MethodWithJTokenParam").GetParameters(), new JArray(jObject, jArray), new JsonSerializer());
			actual.Length.Should().Be.EqualTo(2);
			JToken.DeepEquals((JToken)actual[0], jObject).Should().Be.True();
			JToken.DeepEquals((JToken)actual[1], jArray).Should().Be.True();
		}

		public void ExampleMethod(string p1, int p2, bool p3) {}
		public void MethodWithJTokenParam(JObject p1, JArray p2) {}
	}
}
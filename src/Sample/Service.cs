﻿using System;
using Newtonsoft.Json.Linq;

namespace Sample
{
	public class Service
	{
		public string StringEcho(string par)
		{
			return par;
		}

		public double NumberEcho(double par)
		{
			return par;
		}

		public bool BoolEcho(bool par)
		{
			return par;
		}

		public int[] ArrayEcho(int[] ints)
		{
			return ints;
		}

		public ExampleClass ObjectEcho(ExampleClass obj)
		{
			return obj;
		}

		public JObject JObjectEcho(JObject obj)
		{
			return obj;
		}

		public bool NoParams()
		{
			return true;
		}

		public string Exception()
		{
			throw new ApplicationException("An error occured");
		}

		#region Nested type: ExampleClass

		public class ExampleClass
		{
			public string StringValue;
			public double NumberValue;
			public bool BoolValue;
		}

		#endregion
	}
}
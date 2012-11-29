using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class EventArgs
	{
		private static readonly string TestArgument = "Foo";

		[Test]
		public void ConstructorTest()
		{
			EventArgs<string> argsUnderTest = new EventArgs<string>(TestArgument);
			Assert.AreEqual(TestArgument, argsUnderTest.Argument);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class CancelEventArgs : TestFixtureBase
	{
		[Test]
		public void ConstructorTest()
		{
			string TestArgument = "Foo";
			CancelEventArgs<string> argsUnderTest = new CancelEventArgs<string>(TestArgument);
			Assert.AreEqual(TestArgument, argsUnderTest.Argument);
		}
	}
}

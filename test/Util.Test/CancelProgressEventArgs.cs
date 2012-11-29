using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class CancelProgressEventArgsTests
	{
		private static readonly float TestPercentComplete = .1f;

		[Test]
		public void ConstructorTest()
		{
			CancelProgressEventArgs argsUnderTest = new CancelProgressEventArgs(TestPercentComplete);
			Assert.AreEqual(TestPercentComplete, argsUnderTest.PercentComplete);
		}
	}
}

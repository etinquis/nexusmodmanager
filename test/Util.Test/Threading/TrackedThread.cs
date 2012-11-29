using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Nexus.Client.Util.Threading;

namespace Util.Test.Threading
{
	[TestFixture]
	class TrackedThreadTests : TestFixtureBase
	{
		private void TestFunc()
		{
			
		}

		private void TestParameterizedFunc(object param)
		{
			
		}

		[Test]
		public void ConstructorTest()
		{
			TrackedThread threadUnderTest = new TrackedThread(TestFunc);
			Assert.IsNotNull(threadUnderTest.Thread);
		}

		[Test]
		public void RunThreadTest()
		{
			TrackedThread threadUnderTest = new TrackedThread(TestFunc);
			threadUnderTest.Start();

			threadUnderTest.Thread.Join();
			Assert.AreEqual(0, TrackedThreadManager.Threads.Count());
		}

		[Test]
		public void RunThreadParameterizedTest()
		{
			TrackedThread threadUnderTest = new TrackedThread(TestParameterizedFunc);
			threadUnderTest.Start("Foo");

			threadUnderTest.Thread.Join();
			Assert.AreEqual(0, TrackedThreadManager.Threads.Count());
		}

		[TearDown]
		public void Cleanup()
		{
			while(TrackedThreadManager.Threads.Count() > 0)
			{
				TrackedThreadManager.RemoveThread(TrackedThreadManager.Threads.First());
			}
		}
	}
}

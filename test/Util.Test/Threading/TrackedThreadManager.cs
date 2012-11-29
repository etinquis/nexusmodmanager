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
	class TrackedThreadManagerTests : TestFixtureBase
	{
		private class MockTrackedThread : TrackedThread
		{
			private static void Run()
			{
				
			}

			public MockTrackedThread() : base(Run)
			{
			}
		}

		[Test]
		public void ThreadPropertyTest()
		{
			Assert.AreEqual(0, TrackedThreadManager.Threads.Count());
		}

		[Test]
		public void AddThreadTest()
		{
			var mock = new MockTrackedThread();
			TrackedThreadManager.AddThread(mock);
			Assert.AreEqual(1, TrackedThreadManager.Threads.Count());
			var thread = TrackedThreadManager.Threads[0];
			Assert.AreEqual(mock, thread);

			TrackedThreadManager.RemoveThread(mock);
		}

		[Test]
		public void RemoveThreadTest()
		{
			var mock = new MockTrackedThread();
			TrackedThreadManager.AddThread(mock);
			TrackedThreadManager.RemoveThread(mock);
			Assert.AreEqual(0, TrackedThreadManager.Threads.Count());
		}
	}
}

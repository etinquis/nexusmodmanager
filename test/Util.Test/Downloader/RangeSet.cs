using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Downloader;

namespace Util.Test.Downloader
{
	[TestFixture]
	class RangeSetTests : TestFixtureBase
	{
		[Test]
		public void ConstructorTest()
		{
			var rangeSetUnderTest = new RangeSet();
			Assert.AreEqual(0, rangeSetUnderTest.Count());
		}

		[Test]
		public void AddRangeTest()
		{
			var rangeSetUnderTest = CreateTestRangeSet();
			var range = new Range(0, 1);
			rangeSetUnderTest.AddRange(range);
			Assert.AreEqual(1, rangeSetUnderTest.Count());
			Assert.AreEqual(range, rangeSetUnderTest.First());
		}
		[Test]
		public void RemoveRangeTest()
		{
			var rangeSetUnderTest = CreateTestRangeSet();
			var range = new Range(0, 1);
			rangeSetUnderTest.AddRange(range);

			Assert.AreEqual(1, rangeSetUnderTest.Count());
			rangeSetUnderTest.RemoveRange(range);
			Assert.AreEqual(0, rangeSetUnderTest.Count());
		}

		private RangeSet CreateTestRangeSet()
		{
			return new RangeSet();
		}
	}
}

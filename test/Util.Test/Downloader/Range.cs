using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Downloader;

namespace Util.Test.Downloader
{
	[TestFixture]
	class RangeTests : TestFixtureBase
	{
		private int TestRangeStart = 882;
		private int TestRangeEnd = 1259;

		private int ValidRangeItem = 1000;
		private int InvalidRangeItemZero = 0;
		private int InvalidRangeItemHigh = int.MaxValue;
		private int InvalidRangeItemLow = int.MinValue;

		string ExpectedRangeString = "0-1";

		[Test]
		public void ConstructorTest()
		{
			Range rangeUnderTest = new Range(0,1);
			Assert.AreEqual(0, rangeUnderTest.StartByte);
			Assert.AreEqual(1, rangeUnderTest.EndByte);
			Assert.AreEqual(2, rangeUnderTest.Size);
		}

		[Test]
		public void ContainsValidTest()
		{
			var rangeUnderTest = CreateTestRange();
			Assert.IsTrue(rangeUnderTest.Contains(ValidRangeItem));
		}
		[Test]
		public void ContainsInvalidLowTest()
		{
			var rangeUnderTest = CreateTestRange();
			Assert.IsFalse(rangeUnderTest.Contains(InvalidRangeItemLow));
		}
		[Test]
		public void ContainsInvalidHighTest()
		{
			var rangeUnderTest = CreateTestRange();
			Assert.IsFalse(rangeUnderTest.Contains(InvalidRangeItemHigh));
		}
		[Test]
		public void ContainsInvalidZeroTest()
		{
			var rangeUnderTest = CreateTestRange();
			Assert.IsFalse(rangeUnderTest.Contains(InvalidRangeItemZero));
		}

		[Test]
		public void IntersectedWithTest(
				[Random(1,500,1)] int arbitraryValue1,
				[Random(1, 500, 1)] int arbitraryValue2
			)
		{
			var rangeUnderTest = CreateTestRange();
			var intersectingRange = new Range(TestRangeStart - arbitraryValue1, TestRangeStart + arbitraryValue2);
			Assert.IsTrue(rangeUnderTest.IntersectsWith(intersectingRange));
		}
		[Test]
		public void DoesNotIntersectsWithTest()
		{
			var rangeUnderTest = CreateTestRange();
			var nonIntersectingRange = new Range(TestRangeStart - 500, TestRangeStart - 200);
			Assert.IsFalse(rangeUnderTest.IntersectsWith(nonIntersectingRange));
		}
		[Test]
		public void IsAdjacentToTest()
		{
			var rangeUnderTest = CreateTestRange();
			var adjacentRange = new Range(TestRangeStart - 214, TestRangeStart - 1);
			Assert.IsTrue(rangeUnderTest.IsAdjacentTo(adjacentRange));
		}
		[Test]
		public void IsNotAdjacentToTest()
		{
			var rangeUnderTest = CreateTestRange();
			var adjacentRange = new Range(TestRangeStart - 214, TestRangeStart);
			Assert.IsFalse(rangeUnderTest.IsAdjacentTo(adjacentRange));
		}
		[Test]
		public void IsSuperRangeOf()
		{
			var rangeUnderTest = CreateTestRange();
			var subRange = new Range(TestRangeStart + 10, TestRangeStart + 50);
			Assert.IsTrue(rangeUnderTest.IsSuperRangeOf(subRange));
		}
		[Test]
		public void IsNotSuperRangeOf()
		{
			var rangeUnderTest = CreateTestRange();
			var subRange = new Range(TestRangeStart -1, TestRangeStart + 50);
			Assert.IsFalse(rangeUnderTest.IsSuperRangeOf(subRange));
		}
		[Test]
		public void IsSubRangeOf()
		{
			var rangeUnderTest = CreateTestRange();
			var superRange = new Range(TestRangeStart - 10, TestRangeEnd + 50);
			Assert.IsTrue(rangeUnderTest.IsSubRangeOf(superRange));
		}
		[Test]
		public void IsNotSubRangeOf()
		{
			var rangeUnderTest = CreateTestRange();
			var notSuperRange = new Range(TestRangeStart - 1, TestRangeStart + 50);
			Assert.IsFalse(rangeUnderTest.IsSubRangeOf(notSuperRange));
		}

		[Test]
		public void MergeTest()
		{
			int expectedRangeStart = TestRangeStart - 124;
			int expectedRangeEnd = TestRangeEnd;

			var rangeUnderTest = CreateTestRange();
			var mergeRange = new Range(expectedRangeStart, TestRangeStart + 14);

			rangeUnderTest.Merge(mergeRange);
			Assert.AreEqual(expectedRangeStart, rangeUnderTest.StartByte);
			Assert.AreEqual(expectedRangeEnd, rangeUnderTest.EndByte);
		}
		[Test]
		public void InvalidMergeTest()
		{
			var rangeUnderTest = CreateTestRange();
			var mergeRange = new Range(TestRangeStart - 50, TestRangeStart - 20);

			Assert.Throws(Is.InstanceOf<Exception>(), () => rangeUnderTest.Merge(mergeRange));
		}
		[Test]
		public void SuperRangeMergeTest()
		{
			int expectedRangeStart = TestRangeStart;
			int expectedRangeEnd = TestRangeEnd;

			var rangeUnderTest = CreateTestRange();
			var mergeRange = new Range(TestRangeStart + 1, TestRangeEnd - 1);

			rangeUnderTest.Merge(mergeRange);
			Assert.AreEqual(expectedRangeStart, rangeUnderTest.StartByte);
			Assert.AreEqual(expectedRangeEnd, rangeUnderTest.EndByte);
		}
		[Test]
		public void SubRangeMergeTest()
		{
			int expectedRangeStart = TestRangeStart - 1;
			int expectedRangeEnd = TestRangeEnd + 1;

			var rangeUnderTest = CreateTestRange();
			var mergeRange = new Range(expectedRangeStart, expectedRangeEnd);

			rangeUnderTest.Merge(mergeRange);
			Assert.AreEqual(expectedRangeStart, rangeUnderTest.StartByte);
			Assert.AreEqual(expectedRangeEnd, rangeUnderTest.EndByte);
		}

		[Test]
		public void EqualsRangeTest()
		{
			var rangeUnderTest = CreateTestRange();
			var equalRange = CreateTestRange();

			Assert.IsTrue(rangeUnderTest.Equals(equalRange));
		}
		[Test]
		public void DoesNotEqualRangeTest()
		{
			var rangeUnderTest = CreateTestRange();
			var notEqualRange = new Range(TestRangeStart - 1, TestRangeEnd);

			Assert.IsFalse(rangeUnderTest.Equals(notEqualRange));
		}
		[Test]
		public void EqualsIntsTest()
		{
			var rangeUnderTest = CreateTestRange();
			var equalRange = CreateTestRange();

			Assert.IsTrue(rangeUnderTest.Equals(equalRange.StartByte, equalRange.EndByte));
		}
		[Test]
		public void DoesNotEqualIntsTest()
		{
			var rangeUnderTest = CreateTestRange();
			var notEqualRange = new Range(TestRangeStart - 1, TestRangeEnd);

			Assert.IsFalse(rangeUnderTest.Equals(notEqualRange.StartByte, notEqualRange.EndByte));
		}

		[Test]
		public void ToStringTest()
		{
			Range rangeUnderTest = new Range(0, 1);
			Assert.AreEqual(ExpectedRangeString, rangeUnderTest.ToString());
		}
		[Test]
		public void ParseTest()
		{
			Range rangeUnderTest = Range.Parse(ExpectedRangeString);
			Assert.AreEqual(0, rangeUnderTest.StartByte);
			Assert.AreEqual(1, rangeUnderTest.EndByte);
		}

		private Range CreateTestRange()
		{
			return new Range(TestRangeStart, TestRangeEnd);
		}
	}
}

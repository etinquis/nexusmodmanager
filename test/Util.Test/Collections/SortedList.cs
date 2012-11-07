using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nexus.Client.Util.Collections;
using NUnit.Framework;

namespace Util.Test.Collections
{
	[TestFixture]
	internal class SortedListTests : ListTestBase
	{
		[Test]
		public void EnumerableConstructorTest()
		{
			var listUnderTest = new SortedList<string>(TestItems);

			Assert.IsTrue(ListsAreEqualBesidesOrder(TestItems, listUnderTest));
		}
		[Test]
		public void ComparerConstructorTest()
		{
			var listUnderTest = new SortedList<string>(StringComparer.OrdinalIgnoreCase);

			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(listUnderTest.Contains(DifferentCase));
		}
		[Test]
		public void EnumerableComparerConstructorTest()
		{
			var listUnderTest = new SortedList<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(listUnderTest.Contains(DifferentCase));
		}
		[Test]
		public void NullEnumerableNullComparerConstructorTest()
		{
			Assert.Throws<ArgumentException>(() => new SortedList<Action>(new Action[] { () => { } }, null));
		}
		[Test]
		public void EnumerableNullComparerConstructorTest()
		{
			Assert.Throws<ArgumentException>(() => new SortedList<Action>(new Action[] { () => { } }, null));
		}
		[Test]
		public void EnumerableConstructorNonIComparableTypeTest()
		{
			Assert.Throws<ArgumentException>(() => new SortedList<Action>(new Action[] {() => { }}));
		}

		[Test]
		public void RemoveAtTest()
		{
			var listUnderTest = CreateTestList();
			var beforeCount = listUnderTest.Count;

			listUnderTest.RemoveAt(ExistingMatchIndex);
			Assert.AreEqual(beforeCount - 1, listUnderTest.Count);
		}
		[Test]
		public void RemoveAtInvalidIndexHighTest()
		{
			var listUnderTest = CreateTestList();

			Assert.Throws<ArgumentOutOfRangeException>(() => listUnderTest.RemoveAt(InvalidIndexHigh));
		}
		[Test]
		public void RemoveAtInvalidIndexLowTest()
		{
			var listUnderTest = CreateTestList();

			Assert.Throws<ArgumentOutOfRangeException>(() => listUnderTest.RemoveAt(InvalidIndexLow));
		}

		[Test]
		public void InsertTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws<InvalidOperationException>(() => listUnderTest.Insert(0, NonexistantMatch));
		}

		[Test]
		public void ClearTest()
		{
			var listUnderTest = CreateTestList();
			
			listUnderTest.Clear();
			Assert.AreEqual(0, listUnderTest.Count);
		}

		[Test]
		public void CopyToTest()
		{
			var listUnderTest = CreateTestList();
			string[] actual = new string[listUnderTest.Count];
			listUnderTest.CopyTo(actual, 0);

			ListsAreEqualBesidesOrder(TestItems, actual);
		}

		[Test]
		public void AddTest()
		{
			var listUnderTest = CreateTestList();
			
			Assert.IsFalse(listUnderTest.Contains(NonexistantMatch));
			listUnderTest.Add(NonexistantMatch);
			Assert.IsTrue(listUnderTest.Contains(NonexistantMatch));
		}
		[Test]
		public void AddNullTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsFalse(listUnderTest.Contains(null));
			listUnderTest.Add(null);
			Assert.IsTrue(listUnderTest.Contains(null));
		}

		[Test]
		public void RemoveTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsTrue(listUnderTest.Contains(ExistingMatch));
			Assert.IsTrue(listUnderTest.Remove(ExistingMatch));
			Assert.IsFalse(listUnderTest.Contains(ExistingMatch));
		}
		[Test]
		public void RemoveNonexistantTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsFalse(listUnderTest.Contains(NonexistantMatch));
			Assert.IsFalse(listUnderTest.Remove(NonexistantMatch));
			Assert.IsFalse(listUnderTest.Contains(NonexistantMatch));
		}

		[Test]
		public void ThisGetterTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(ExistingMatch, listUnderTest[TestItemsSorted.IndexOf(ExistingMatch, StringComparer.Ordinal)]);
		}
		[Test]
		public void ThisGetterInvalidIndexHighTest()
		{
			var listUnderTest = CreateTestList();
			string x;
			Assert.Throws<ArgumentOutOfRangeException>(() => x = listUnderTest[InvalidIndexHigh]);
		}
		[Test]
		public void ThisGetterInvalidIndexLowTest()
		{
			var listUnderTest = CreateTestList();
			string x;
			Assert.Throws<ArgumentOutOfRangeException>(() => x = listUnderTest[InvalidIndexLow]);
		}
		[Test]
		public void ThisSetterTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws<InvalidOperationException>(() => listUnderTest[ExistingMatchIndex] = NonexistantMatch);
		}

		[Test]
		public void ComparerPropertyTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsFalse(listUnderTest.Contains(DifferentCase));
			listUnderTest.Comparer = StringComparer.OrdinalIgnoreCase;
			Assert.IsTrue(listUnderTest.Contains(DifferentCase));
		}

		[Test]
		public void IndexOfTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(TestItemsSorted.IndexOf(ExistingMatch, StringComparer.Ordinal), listUnderTest.IndexOf(ExistingMatch));
		}

		[Test]
		public void IsReadOnlyTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsFalse(listUnderTest.IsReadOnly);
		}
		
		private SortedList<string> CreateTestList()
		{
			return new SortedList<string>(TestItems);
		}
	}
}

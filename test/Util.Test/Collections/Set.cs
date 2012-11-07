using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Collections;

namespace Util.Test.Collections
{
	[TestFixture]
	internal class SetTests : ListTestBase
	{
		[Test]
		public void DefaultConstructorTest()
		{
			const int expectedCount = 0;

			var setUnderTest = new Set<string>();
			Assert.AreEqual(expectedCount, setUnderTest.Count);
			Assert.AreEqual(default(string), setUnderTest.ElementAtOrDefault(0));
		}

		[Test]
		public void EnumerableConstructorTest()
		{
			const int expectedCount = 3;
			var setUnderTest = new Set<string>(Numbers);
			Assert.AreEqual(expectedCount, setUnderTest.Count);

			for (int i = 0; i < Numbers.Count(); i++)
			{
				Assert.AreEqual(Numbers[i], setUnderTest[i]);
			}
			Assert.AreEqual(default(string), setUnderTest.ElementAtOrDefault(3));
		}

		[Test]
		public void CopySetConstructorTest()
		{
			Set<string> expected = CreateTestSet();
			Set<string> copiedSet = new Set<string>(expected);

			for (int i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i], copiedSet[i]);
			}
		}

		[Test]
		public void ComparerConstructorTest()
		{
			var setUnderTest = new Set<string>(StringComparer.OrdinalIgnoreCase);
			setUnderTest.Add(ExistingMatch);
			Assert.IsTrue(setUnderTest.Contains(DifferentCase));
		}

		[Test]
		public void FindPredicateTest()
		{
			var setUnderTest = CreateTestSet();
			var actual = setUnderTest.Find(str => str.Equals(ExistingMatch));

			Assert.AreEqual(ExistingMatch, actual);
		}

		[Test]
		public void ToArrayTest()
		{
			var setUnderTest = new Set<string>(Numbers);

			Assert.AreEqual(Numbers, setUnderTest.ToArray());
		}

		[Test]
		public void AddTest()
		{
			var setUnderTest = CreateTestSet();
			setUnderTest.Add(NonexistantMatch);
			Assert.IsTrue(setUnderTest.Contains(NonexistantMatch));
		}
		[Test]
		public void AddNullTest()
		{
			var setUnderTest = CreateTestSet();
			setUnderTest.Add(null);
			Assert.IsTrue(setUnderTest.Contains(null));
		}

		[Test]
		public void AddRangeTest()
		{
			var extraItems = new string[] {"Four", "Five", "Six"};
			var setUnderTest = new Set<string>(Numbers);
			setUnderTest.AddRange(extraItems);

			foreach (var item in Numbers)
			{
				Assert.IsTrue(setUnderTest.Contains(item));
			}
			foreach (var item in extraItems)
			{
				Assert.IsTrue(setUnderTest.Contains(item));
			}
		}

		[Test]
		public void SortNoComparerTest()
		{
			var setUnderTest = new Set<string>(CaseSignificantSortable);

			setUnderTest.Sort();

			for (int i = 0; i < CaseSignificantExpectedSorted.Count(); i++)
			{
				Assert.AreEqual(CaseSignificantExpectedSorted[i], setUnderTest[i]);
			}
		}

		[Test]
		public void SortWithComparerTest()
		{
			var setUnderTest = new Set<string>(CaseInsignificantSortable, StringComparer.OrdinalIgnoreCase);

			setUnderTest.Sort();

			for (int i = 0; i < CaseInsignificantExpectedSorted.Count(); i++)
			{
				Assert.AreEqual(CaseInsignificantExpectedSorted[i], setUnderTest[i]);
			}
		}

		[Test]
		public void ForEachTest()
		{
			var expected = new List<string>(Numbers);
			var setUnderTest = new Set<string>(Numbers);

			var result = new List<string>();
			Action<string> action = result.Add;

			setUnderTest.ForEach(action);

			for (int i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i], result[i]);
			}
		}

		[Test]
		public void IndexOfExistingTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.AreEqual(ExistingMatchIndex, setUnderTest.IndexOf(ExistingMatch));
		}

		[Test]
		public void IndexOfExistingWithIndexTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.AreEqual(NonexistantMatchExpectedIndex, setUnderTest.IndexOf(ExistingMatch, ExistingMatchIndex + 1));
		}

		[Test]
		public void IndexOfExistingWithValidIndexTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.AreEqual(ExistingMatchIndex, setUnderTest.IndexOf(ExistingMatch, 0));
		}

		[Test]
		public void IndexOfNegOneIndexTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.Throws<ArgumentOutOfRangeException>(() => setUnderTest.IndexOf(ExistingMatch, InvalidIndexLow));
		}

		[Test]
		public void IndexOfOutOfBoundsHighIndexTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.Throws<ArgumentOutOfRangeException>(() => setUnderTest.IndexOf(ExistingMatch, InvalidIndexHigh));
		}

		[Test]
		public void IndexOfNullTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.AreEqual(NonexistantMatchExpectedIndex, setUnderTest.IndexOf(null));
		}

		[Test]
		public void IndexOfNonexistantTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.AreEqual(NonexistantMatchExpectedIndex, setUnderTest.IndexOf(NonexistantMatch));
		}

		[Test]
		public void IndexOfComparerTest()
		{
			var setUnderTest = new Set<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			var actual = setUnderTest.IndexOf(DifferentCase, 0);

			Assert.AreEqual(ExistingMatchIndex, actual);
		}

		[Test]
		public void IndexOfComparerNonexistantTest()
		{
			var setUnderTest = new Set<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			var actual = setUnderTest.IndexOf(NonexistantMatch, 0);

			Assert.AreEqual(NonexistantMatchExpectedIndex, actual);
		}

		[Test]
		public void InsertTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.Throws<InvalidOperationException>(() => setUnderTest.Insert(0, NonexistantMatch));
		}

		[Test]
		public void CopyToTest()
		{
			var setUnderTest = CreateTestSet();
			string[] actual = new string[setUnderTest.Count];
			setUnderTest.CopyTo(actual, 0);
			Assert.IsTrue(ListsAreEqual(actual, setUnderTest));
		}
		[Test]
		public void CopyToIndexTest()
		{
			var setUnderTest = CreateTestSet();
			string[] actual = new string[setUnderTest.Count + 1];
			setUnderTest.CopyTo(actual, 1);
			Assert.IsTrue(ListsAreEqual(TestItems, actual.Skip(1).ToList()));
		}

		[Test]
		public void RemoveAtTest()
		{
			var setUnderTest = CreateTestSet();
			setUnderTest.RemoveAt(ExistingMatchIndex);

			Assert.AreNotEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void RemoveAtTestInvalidIndexHighTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.Throws<ArgumentOutOfRangeException>(()=>setUnderTest.RemoveAt(InvalidIndexHigh));
		}
		[Test]
		public void RemoveAtTestInvalidIndexLowTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.Throws<ArgumentOutOfRangeException>(() => setUnderTest.RemoveAt(InvalidIndexLow));
		}

		[Test]
		public void RemoveExistingTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.IsTrue(setUnderTest.Remove(ExistingMatch));
			Assert.AreNotEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void RemoveNonExistantTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.IsFalse(setUnderTest.Remove(NonexistantMatch));
			Assert.AreEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void RemoveNullTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.IsFalse(setUnderTest.Remove(null));
		}

		[Test]
		public void ThisGetterTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.AreEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void ThisGetterInvalidIndexHighTest()
		{
			var setUnderTest = CreateTestSet();
			string x;
			Assert.Throws<ArgumentOutOfRangeException>(() => x = setUnderTest[InvalidIndexHigh]);
		}
		[Test]
		public void ThisGetterInvalidIndexLowTest()
		{
			var setUnderTest = CreateTestSet();
			string x;
			Assert.Throws<ArgumentOutOfRangeException>(() => x = setUnderTest[InvalidIndexLow]);
		}

		[Test]
		public void ThisSetterTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.Throws<InvalidOperationException>(() => setUnderTest[ExistingMatchIndex] = NonexistantMatch);
		}

		[Test]
		public void ClearTest()
		{
			var setUnderTest = CreateTestSet();
			setUnderTest.Clear();
			
			Assert.AreEqual(0, setUnderTest.Count);
		}

		[Test]
		public void ReadOnlyTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.AreEqual(false, setUnderTest.IsReadOnly);
		}

		[Test]
		public void IListAddTest()
		{
			IList setUnderTest = CreateTestSet();
			setUnderTest.Add(NonexistantMatch);

			Assert.IsTrue(setUnderTest.Contains(NonexistantMatch));
		}
		[Test]
		public void IListAddNonTValueTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.Throws<InvalidOperationException>(() => setUnderTest.Add(12345));
		}

		[Test]
		public void IListContainsTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.IsTrue(((IList)setUnderTest).Contains(ExistingMatch));
		}
		[Test]
		public void IListContainsNonTValueTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.IsFalse(((IList)setUnderTest).Contains(12345));
		}

		[Test]
		public void IListIndexOfTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.AreEqual(ExistingMatchIndex, ((IList)setUnderTest).IndexOf(ExistingMatch));
		}
		[Test]
		public void IListIndexOfNonTValueTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.AreEqual(NonexistantMatchExpectedIndex, setUnderTest.IndexOf(12345));
		}

		[Test]
		public void IListInsertTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.Throws<InvalidOperationException>(() => setUnderTest.Insert(ExistingMatchIndex, NonexistantMatch));
		}
		[Test]
		public void IListInsertNonTValueTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.Throws<InvalidOperationException>(() => ((IList)setUnderTest).Insert(ExistingMatchIndex, 12345));
		}

		[Test]
		public void IListFixedSizeTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.IsFalse(setUnderTest.IsFixedSize);
		}

		[Test]
		public void IListRemoveTest()
		{
			IList setUnderTest = CreateTestSet();
			setUnderTest.Remove(ExistingMatch);
			Assert.AreNotEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void IListRemoveNonTValueTest()
		{
			var setUnderTest = CreateTestSet();
			Assert.DoesNotThrow(() => ((IList) setUnderTest).Remove(12345));
		}

		[Test]
		public void IListThisGetterTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.AreEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void IListThisSetterTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.Throws<InvalidOperationException>(() => setUnderTest[ExistingMatchIndex] = NonexistantMatch);
		}
		[Test]
		public void IListThisSetterNonTValueTest()
		{
			IList setUnderTest = CreateTestSet();
			Assert.Throws<InvalidOperationException>(() => setUnderTest[ExistingMatchIndex] = 12345);
		}

		[Test]
		public void ICollectionIsSyncronizedTest()
		{
			ICollection setUnderTest = CreateTestSet();
			Assert.AreEqual(false, setUnderTest.IsSynchronized);
		}
		[Test]
		public void ICollectionSyncRootTest()
		{
			ICollection setUnderTest = CreateTestSet();
			Assert.IsNotNull(setUnderTest.SyncRoot);
		}

		[Test]
		public void ICollectionCopyToTest()
		{
			ICollection setUnderTest = CreateTestSet();
			string[] actual = new string[setUnderTest.Count];
			setUnderTest.CopyTo(actual, 0);

			Assert.AreEqual(TestItems, actual);
		}
		[Test]
		public void ICollectionCopyToIndexTest()
		{
			ICollection setUnderTest = CreateTestSet();
			string[] actual = new string[setUnderTest.Count + 1];
			setUnderTest.CopyTo(actual, 1);

			Assert.AreEqual(TestItems, actual.Skip(1));
		}
		[Test]
		public void ICollectionCopyToInvalidIndexHighTest()
		{
			ICollection setUnderTest = CreateTestSet();
			string[] actual = new string[1];
			Assert.Throws<ArgumentException>(() => setUnderTest.CopyTo(actual, InvalidIndexHigh));
		}
		[Test]
		public void ICollectionCopyToInvalidIndexLowTest()
		{
			ICollection setUnderTest = CreateTestSet();
			string[] actual = new string[1];
			Assert.Throws<ArgumentOutOfRangeException>(() => setUnderTest.CopyTo(actual, InvalidIndexLow));
		}

		private void TestActionAgainst(Action<Set<string>> actionUnderTest, Action<Set<string>> verificationAction)
		{
			var listUnderTest = CreateTestSet();
			var expectedList = CreateTestSet();

			actionUnderTest(listUnderTest);
			verificationAction(expectedList);

			Assert.IsTrue(SetsAreEqual(expectedList, listUnderTest));
		}

		protected  virtual Set<string> CreateTestSet()
		{
			return new Set<string>() {"Foo", "Bar", "Baz"};
		}

		private bool SetsAreEqual(Set<string> expected, Set<string> actual)
		{
			bool result = true;

			result &= expected.Count == actual.Count;
			if (result == false) return false;

			for(int i = 0; i < expected.Count; i++)
			{
				result &= expected.ElementAt(i) == actual.ElementAt(i);
			}

			result &= expected.Reverse() == actual.Reverse();

			result &= expected.Contains(ExistingMatch) == actual.Contains(ExistingMatch);
			result &= expected.Contains(NonexistantMatch) == actual.Contains(NonexistantMatch);

			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Collections;

namespace Util.Test.Collections
{
	[TestFixture]
	class ReorderableStackTests : ListTestBase
	{
		[Test]
		public virtual void DefaultConstructorTest()
		{
			const int expectedCount = 0;

			var stackUnderTest = new ReorderableStack<string>();
			Assert.AreEqual(expectedCount, stackUnderTest.Count);
			Assert.AreEqual(default(string), stackUnderTest.ElementAtOrDefault(0));
		}

		[Test]
		public virtual void EnumerableConstructorTest()
		{
			const int expectedCount = 3;
			var stackUnderTest = new ReorderableStack<string>(Numbers);
			Assert.AreEqual(expectedCount, stackUnderTest.Count);

			for (int i = 0; i < Numbers.Count(); i++)
			{
				Assert.AreEqual(Numbers[i], stackUnderTest[i]);
			}
			Assert.AreEqual(default(string), stackUnderTest.ElementAtOrDefault(3));
		}

		[Test]
		public virtual void CopySetConstructorTest()
		{
			ReorderableStack<string> expected = CreateTestStack();
			ReorderableStack<string> copiedSet = new ReorderableStack<string>(expected);

			for (int i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i], copiedSet[i]);
			}
		}

		[Test]
		public virtual void ComparerConstructorTest()
		{
			var stackUnderTest = new ReorderableStack<string>(StringComparer.OrdinalIgnoreCase);
			stackUnderTest.Push(ExistingMatch);
			Assert.IsTrue(stackUnderTest.Contains(DifferentCase));
		}

		[Test]
		public virtual void FindPredicateTest()
		{
			var stackUnderTest = CreateTestStack();
			var actual = stackUnderTest.Find(str => str.Equals(ExistingMatch));

			Assert.AreEqual(ExistingMatch, actual);
		}

		[Test]
		public virtual void ToArrayTest()
		{
			var stackUnderTest = new ReorderableStack<string>(Numbers);

			Assert.AreEqual(Numbers, stackUnderTest.ToArray());
		}

		[Test]
		public virtual void PushTest()
		{
			var stackUnderTest = CreateTestStack();
			stackUnderTest.Push(NonexistantMatch);
			Assert.IsTrue(stackUnderTest.Contains(NonexistantMatch));
		}
		[Test]
		public virtual void PushNullTest()
		{
			var stackUnderTest = CreateTestStack();
			stackUnderTest.Push(null);
			Assert.IsTrue(stackUnderTest.Contains(null));
		}
		
		[Test]
		public virtual void IndexOfExistingTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.AreEqual(ExistingMatchIndex, stackUnderTest.IndexOf(ExistingMatch));
		}

		[Test]
		public virtual void IndexOfExistingWithIndexTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.AreEqual(NonexistantMatchExpectedIndex, stackUnderTest.IndexOf(NonexistantMatch));
		}
		
		[Test]
		public virtual void IndexOfNullTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.AreEqual(NonexistantMatchExpectedIndex, stackUnderTest.IndexOf(null));
		}

		[Test]
		public virtual void IndexOfNonexistantTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.AreEqual(NonexistantMatchExpectedIndex, stackUnderTest.IndexOf(NonexistantMatch));
		}

		[Test]
		public virtual void IndexOfComparerTest()
		{
			var stackUnderTest = new Nexus.Client.Util.Collections.ReorderableStack<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			var actual = stackUnderTest.IndexOf(DifferentCase);

			Assert.AreEqual(ExistingMatchIndex, actual);
		}

		[Test]
		public void IndexOfComparerNonexistantTest()
		{
			var stackUnderTest = new Nexus.Client.Util.Collections.ReorderableStack<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			var actual = stackUnderTest.IndexOf(NonexistantMatch);

			Assert.AreEqual(NonexistantMatchExpectedIndex, actual);
		}

		[Test]
		public virtual void CopyToTest()
		{
			var stackUnderTest = CreateTestStack();
			string[] actual = new string[stackUnderTest.Count];
			stackUnderTest.CopyTo(actual, 0);
			Assert.IsTrue(ListsAreEqual(actual, stackUnderTest));
		}
		[Test]
		public virtual void CopyToIndexTest()
		{
			var stackUnderTest = CreateTestStack();
			string[] actual = new string[stackUnderTest.Count + 1];
			stackUnderTest.CopyTo(actual, 1);
			Assert.IsTrue(ListsAreEqual(TestItems, actual.Skip(1).ToList()));
		}

		[Test]
		public void RemoveAtTest()
		{
			var stackUnderTest = CreateTestStack();
			stackUnderTest.RemoveAt(ExistingMatchIndex);

			Assert.AreNotEqual(ExistingMatch, stackUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void RemoveAtTestInvalidIndexHighTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.Throws<ArgumentOutOfRangeException>(() => stackUnderTest.RemoveAt(InvalidIndexHigh));
		}
		[Test]
		public void RemoveAtTestInvalidIndexLowTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.Throws<ArgumentOutOfRangeException>(() => stackUnderTest.RemoveAt(InvalidIndexLow));
		}

		[Test]
		public virtual void RemoveExistingTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.IsTrue(stackUnderTest.Remove(ExistingMatch));
			Assert.AreNotEqual(ExistingMatch, stackUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public virtual void RemoveNonExistantTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.IsFalse(stackUnderTest.Remove(NonexistantMatch));
			Assert.AreEqual(ExistingMatch, stackUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public virtual void RemoveNullTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.IsFalse(stackUnderTest.Remove(null));
		}

		[Test]
		public virtual void ThisGetterTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.AreEqual(ExistingMatch, stackUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void ThisGetterInvalidIndexHighTest()
		{
			var stackUnderTest = CreateTestStack();
			string x;
			Assert.Throws<ArgumentOutOfRangeException>(() => x = stackUnderTest[InvalidIndexHigh]);
		}
		[Test]
		public virtual void ThisGetterInvalidIndexLowTest()
		{
			var stackUnderTest = CreateTestStack();
			string x;
			Assert.Throws<ArgumentOutOfRangeException>(() => x = stackUnderTest[InvalidIndexLow]);
		}

		[Test]
		public void ThisSetterTest()
		{
			var stackUnderTest = CreateTestStack();
			stackUnderTest[ExistingMatchIndex] = NonexistantMatch;
			Assert.AreEqual(NonexistantMatch, stackUnderTest[ExistingMatchIndex]);
		}

		[Test]
		public virtual void ClearTest()
		{
			var stackUnderTest = CreateTestStack();
			stackUnderTest.Clear();

			Assert.AreEqual(0, stackUnderTest.Count);
		}

		[Test]
		public virtual void ReadOnlyTest()
		{
			var stackUnderTest = CreateTestStack();
			Assert.AreEqual(false, stackUnderTest.IsReadOnly);
		}

		[Test]
		public virtual void PeekTest()
		{
			var stackUnderTest = CreateTestStack();
			foreach (var testItem in TestItems.Reverse())
			{
				Assert.AreEqual(testItem, stackUnderTest.Peek());
				stackUnderTest.Pop();
			}
		}

		[Test]
		public virtual void PopTest()
		{
			var stackUnderTest = CreateTestStack();
			for(int i = 0; i < stackUnderTest.Count; i++)
			{
				var item = stackUnderTest.Pop();
				Assert.AreEqual(TestItems.Reverse().ElementAt(i), item);
			}
		}

		[Test]
		public virtual void PushRangeTest()
		{
			var stackUnderTest = new ReorderableStack<string>();
			stackUnderTest.PushRange(TestItems);
			Assert.AreEqual(TestItems.Last(), stackUnderTest.Peek());
			Assert.AreEqual(TestItems.Length, stackUnderTest.Count);
		}

		[Test]
		public virtual void IndexOfWithComparisonTest()
		{
			Comparison<string> comparison = (s, s1) => s.Length == s1.Length && !s.Equals(s1) ? 0 : String.Compare(s.ToUpper(), s1, System.StringComparison.Ordinal);
			var stackUnderTest = new ReorderableStack<string>(comparison);
			stackUnderTest.Push(ExistingMatch);
			int index = stackUnderTest.IndexOf(ExistingMatch);

			Assert.AreNotEqual(ExistingMatchIndex, index);
		}

		[Test]
		public virtual void IListTInsertTest()
		{
			var stackUnderTest = CreateTestStack();
			((IList<string>)stackUnderTest).Insert(stackUnderTest.Count - 1, NonexistantMatch);
			stackUnderTest.Pop();
			Assert.AreEqual(NonexistantMatch, stackUnderTest.Peek());
		}

		protected virtual ReorderableStack<string> CreateTestStack()
		{
			return new Nexus.Client.Util.Collections.ReorderableStack<string>(TestItems);
		}
	}
}

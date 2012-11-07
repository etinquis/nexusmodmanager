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
		public void DefaultConstructorTest()
		{
			const int expectedCount = 0;

			var setUnderTest = new ReorderableStack<string>();
			Assert.AreEqual(expectedCount, setUnderTest.Count);
			Assert.AreEqual(default(string), setUnderTest.ElementAtOrDefault(0));
		}

		[Test]
		public void EnumerableConstructorTest()
		{
			const int expectedCount = 3;
			var setUnderTest = new ReorderableStack<string>(Numbers);
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
			ReorderableStack<string> expected = CreateTestStack();
			ReorderableStack<string> copiedSet = new ReorderableStack<string>(expected);

			for (int i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i], copiedSet[i]);
			}
		}

		[Test]
		public void ComparerConstructorTest()
		{
			var setUnderTest = new ReorderableStack<string>(StringComparer.OrdinalIgnoreCase);
			setUnderTest.Push(ExistingMatch);
			Assert.IsTrue(setUnderTest.Contains(DifferentCase));
		}

		[Test]
		public void FindPredicateTest()
		{
			var setUnderTest = CreateTestStack();
			var actual = setUnderTest.Find(str => str.Equals(ExistingMatch));

			Assert.AreEqual(ExistingMatch, actual);
		}

		[Test]
		public void ToArrayTest()
		{
			var setUnderTest = new ReorderableStack<string>(Numbers);

			Assert.AreEqual(Numbers, setUnderTest.ToArray());
		}

		[Test]
		public void PushTest()
		{
			var setUnderTest = CreateTestStack();
			setUnderTest.Push(NonexistantMatch);
			Assert.IsTrue(setUnderTest.Contains(NonexistantMatch));
		}
		[Test]
		public void PushNullTest()
		{
			var setUnderTest = CreateTestStack();
			setUnderTest.Push(null);
			Assert.IsTrue(setUnderTest.Contains(null));
		}
		
		[Test]
		public void IndexOfExistingTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.AreEqual(ExistingMatchIndex, setUnderTest.IndexOf(ExistingMatch));
		}

		[Test]
		public void IndexOfExistingWithIndexTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.AreEqual(NonexistantMatchExpectedIndex, setUnderTest.IndexOf(NonexistantMatch));
		}
		
		[Test]
		public void IndexOfNullTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.AreEqual(NonexistantMatchExpectedIndex, setUnderTest.IndexOf(null));
		}

		[Test]
		public void IndexOfNonexistantTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.AreEqual(NonexistantMatchExpectedIndex, setUnderTest.IndexOf(NonexistantMatch));
		}

		[Test]
		public void IndexOfComparerTest()
		{
			var setUnderTest = new Nexus.Client.Util.Collections.ReorderableStack<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			var actual = setUnderTest.IndexOf(DifferentCase);

			Assert.AreEqual(ExistingMatchIndex, actual);
		}

		[Test]
		public void IndexOfComparerNonexistantTest()
		{
			var setUnderTest = new Nexus.Client.Util.Collections.ReorderableStack<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			var actual = setUnderTest.IndexOf(NonexistantMatch);

			Assert.AreEqual(NonexistantMatchExpectedIndex, actual);
		}

		[Test]
		public void CopyToTest()
		{
			var setUnderTest = CreateTestStack();
			string[] actual = new string[setUnderTest.Count];
			setUnderTest.CopyTo(actual, 0);
			Assert.IsTrue(ListsAreEqual(actual, setUnderTest));
		}
		[Test]
		public void CopyToIndexTest()
		{
			var setUnderTest = CreateTestStack();
			string[] actual = new string[setUnderTest.Count + 1];
			setUnderTest.CopyTo(actual, 1);
			Assert.IsTrue(ListsAreEqual(TestItems, actual.Skip(1).ToList()));
		}

		[Test]
		public void RemoveAtTest()
		{
			var setUnderTest = CreateTestStack();
			setUnderTest.RemoveAt(ExistingMatchIndex);

			Assert.AreNotEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void RemoveAtTestInvalidIndexHighTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.Throws<ArgumentOutOfRangeException>(() => setUnderTest.RemoveAt(InvalidIndexHigh));
		}
		[Test]
		public void RemoveAtTestInvalidIndexLowTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.Throws<ArgumentOutOfRangeException>(() => setUnderTest.RemoveAt(InvalidIndexLow));
		}

		[Test]
		public void RemoveExistingTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.IsTrue(setUnderTest.Remove(ExistingMatch));
			Assert.AreNotEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void RemoveNonExistantTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.IsFalse(setUnderTest.Remove(NonexistantMatch));
			Assert.AreEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void RemoveNullTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.IsFalse(setUnderTest.Remove(null));
		}

		[Test]
		public void ThisGetterTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.AreEqual(ExistingMatch, setUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public void ThisGetterInvalidIndexHighTest()
		{
			var setUnderTest = CreateTestStack();
			string x;
			Assert.Throws<ArgumentOutOfRangeException>(() => x = setUnderTest[InvalidIndexHigh]);
		}
		[Test]
		public void ThisGetterInvalidIndexLowTest()
		{
			var setUnderTest = CreateTestStack();
			string x;
			Assert.Throws<ArgumentOutOfRangeException>(() => x = setUnderTest[InvalidIndexLow]);
		}

		[Test]
		public void ThisSetterTest()
		{
			var setUnderTest = CreateTestStack();
			setUnderTest[ExistingMatchIndex] = NonexistantMatch;
			Assert.AreEqual(NonexistantMatch, setUnderTest[ExistingMatchIndex]);
		}

		[Test]
		public void ClearTest()
		{
			var setUnderTest = CreateTestStack();
			setUnderTest.Clear();

			Assert.AreEqual(0, setUnderTest.Count);
		}

		[Test]
		public void ReadOnlyTest()
		{
			var setUnderTest = CreateTestStack();
			Assert.AreEqual(false, setUnderTest.IsReadOnly);
		}

		protected virtual Nexus.Client.Util.Collections.ReorderableStack<string> CreateTestStack()
		{
			return new Nexus.Client.Util.Collections.ReorderableStack<string>(TestItems);
		}
	}
}

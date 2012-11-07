using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Collections;

namespace Util.Test.Collections
{
	[TestFixture]
	internal class SortedThreadSafeObservableCollectionTests : ThreadSafeObservableListTests
	{
		[Test]
		public override void InsertTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), () => listUnderTest.Insert(ExistingMatchIndex, ExistingMatch));
		}
		[Test]
		public override void InsertInvalidIndexHighTest()
		{
			var listUnderTest = CreateTestList();
			listUnderTest.Insert(InvalidIndexHigh, NonexistantMatch);
			Assert.AreEqual(TestItems.Length + 1, listUnderTest.Count);
		}
		[Test]
		public override void InsertInvalidIndexLowTest()
		{
			var listUnderTest = CreateTestList();
			listUnderTest.Insert(InvalidIndexHigh, NonexistantMatch);
			Assert.AreEqual(TestItems.Length + 1, listUnderTest.Count);
		}

		[Test]
		public override void ThisSetterTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<InvalidOperationException>(), () => listUnderTest[ExistingMatchIndex] = NonexistantMatch);
		}

		[Test]
		public override void MoveTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<InvalidOperationException>(), () => listUnderTest.Move(0, 1));
		}

		[Test]
		public override void CopyToTest()
		{
			var listUnderTest = CreateTestList();
			var actual = new string[listUnderTest.Count];
			listUnderTest.CopyTo(actual, 0);
			Assert.IsTrue(ListsAreEqual(TestItemsSorted, actual));
		}


		[Test]
		public override void IEnumerableTGetEnumeratorTest()
		{
			IEnumerable<string> listUnderTest = CreateTestList();
			var enumerator = listUnderTest.GetEnumerator();

			int count = 0;
			while (enumerator.MoveNext())
			{
				Assert.AreEqual(TestItemsSorted[count], enumerator.Current);
				count++;
			}
			Assert.AreEqual(TestItemsSorted.Length, count);
		}

		[Test]
		public override void IEnumerableGetEnumeratorTest()
		{
			IEnumerable listUnderTest = CreateTestList();
			var enumerator = listUnderTest.GetEnumerator();

			int count = 0;
			while (enumerator.MoveNext())
			{
				Assert.AreEqual(TestItemsSorted[count], enumerator.Current);
				count++;
			}
			Assert.AreEqual(TestItemsSorted.Length, count);
		}

		[Test]
		public override void IndexOfExistingMatchTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(TestItemsSorted.IndexOf(ExistingMatch, StringComparer.Ordinal), listUnderTest.IndexOf(ExistingMatch));
		}

		[Test]
		public override void ThisGetterTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(ExistingMatch, listUnderTest[TestItemsSorted.IndexOf(ExistingMatch, StringComparer.Ordinal)]);
		}

		protected override ThreadSafeObservableList<string> CreateTestList()
		{
			return new SortedThreadSafeObservableCollection<string>(TestItems, StringComparer.OrdinalIgnoreCase);
		}
	}
}

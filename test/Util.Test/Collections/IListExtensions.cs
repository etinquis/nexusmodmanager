using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Collections;

namespace Util.Test.Collections
{
	[TestFixture]
	class IListExtensionsTests : ListTestBase
	{
		[Test]
		public void FindExistingMatchTest()
		{
			IList<string> listUnderTest = CreateTestList();
			string actualMatch = listUnderTest.Find(str => str.Equals(ExistingMatch));

			Assert.AreEqual(ExistingMatch, actualMatch);
		}

		[Test]
		public void FindNonexistantMatchTest()
		{
			IList<string> listUnderTest = CreateTestList();
			string actualMatch = listUnderTest.Find(str => str.Equals(NonexistantMatch));

			Assert.AreEqual(default(string), actualMatch);
		}

		[Test]
		public void SwapTest()
		{
			TestActionAgainst(list=>list.Swap(0,1),
				list=>
					{
						var tmp = list[0];
						list[0] = list[1];
						list[1] = tmp;
					});
		}
		[Test]
		public void SwapTwiceAsIdentityTest()
		{
			TestActionAgainst(list =>
				                  {
					                  list.Swap(0, 1);
					                  list.Swap(0, 1);
				                  },
			                  list => { });
		}
		[Test]
		public void SwapOrderDoesntMatterTest()
		{
			TestActionAgainst(list => list.Swap(0, 1),
			                  list => list.Swap(1, 0));
		}
		
		[Test]
		public void ContainsExistingMatchPredicateTest()
		{
			var listUnderTest = CreateTestList();

			Assert.IsTrue(listUnderTest.Contains(str=>str.Equals(ExistingMatch)));
		}
		[Test]
		public void ContainsExistingMatchStringTest()
		{
			var listUnderTest = CreateTestList();

			Assert.IsTrue(listUnderTest.Contains(ExistingMatch, StringComparer.Ordinal));
		}
		[Test]
		public void ContainsExistingMatchStringIgnoreCaseTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsTrue(listUnderTest.Contains(DifferentCase, StringComparer.OrdinalIgnoreCase));
		}
		[Test]
		public void ContainsNonExistantMatchStringTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsFalse(listUnderTest.Contains(NonexistantMatch, StringComparer.Ordinal));
		}
		[Test]
		public void ContainsNonExistantMatchStringIngoreCaseTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsFalse(listUnderTest.Contains(NonexistantMatch, StringComparer.OrdinalIgnoreCase));
		}
		[Test]
		public void ContainsNonExistantMatchPredicateTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsFalse(listUnderTest.Contains(str=>str.Equals(NonexistantMatch)));
		}

		[Test]
		public void RemoveExistingTest()
		{
			TestActionAgainst(list => list.Remove(ExistingMatch, StringComparer.Ordinal),
							  list => list.Remove(ExistingMatch));
		}
		[Test]
		public void RemoveExistingIgnoreCaseTest()
		{
			TestActionAgainst(list => list.Remove(DifferentCase, StringComparer.OrdinalIgnoreCase),
							  list => list.Remove(ExistingMatch));
		}
		[Test]
		public void RemoveNonExistantTest()
		{
			TestActionAgainst(list => list.Remove(NonexistantMatch, StringComparer.Ordinal),
							  list => list.Remove(NonexistantMatch));
		}
		[Test]
		public void RemoveNonExistantIgnoreCaseTest()
		{
			TestActionAgainst(list => list.Remove(NonexistantMatch, StringComparer.OrdinalIgnoreCase),
							  list => list.Remove(NonexistantMatch));
		}

		[Test]
		public void RemoveAllTest()
		{
			TestActionAgainst(list => list.RemoveAll(str => str.StartsWith("B")),
							  list =>
								  {
									  list.Remove("Bar");
									  list.Remove("Baz");
								  });
		}

		[Test]
		public void RemoveRangeTest()
		{
			TestActionAgainst(list => list.RemoveRange(new string[] {"Foo", "Bar"}),
			                  list =>
				                  {
					                  list.Remove("Foo");
					                  list.Remove("Bar");
				                  });
		}
		[Test]
		public void RemoveRangeIgnoreCaseTest()
		{
			TestActionAgainst(list => list.RemoveRange(new string[] {"foo", "bar"}, StringComparer.OrdinalIgnoreCase),
			                  list =>
				                  {
					                  list.Remove("Foo");
					                  list.Remove("Bar");
				                  });
		}

		[Test]
		public void IndexOfExistingMatchPredicateTest()
		{
			int expected = ExistingMatchIndex;
			int actual = CreateTestList().IndexOf(str => str.Equals(ExistingMatch));

			Assert.AreEqual(expected, actual);
		}
		[Test]
		public void IndexOfExistingIgnoreCaseTest()
		{
			int expected = ExistingMatchIndex;
			int actual = CreateTestList().IndexOf(DifferentCase, StringComparer.OrdinalIgnoreCase);

			Assert.AreEqual(expected, actual);
		}
		[Test]
		public void IndexOfNonexistantTest()
		{
			int expected = NonexistantMatchExpectedIndex;
			int actual =  CreateTestList().IndexOf(NonexistantMatch);
			Assert.AreEqual(expected, actual);
		}
		[Test]
		public void IndexOfNonexistantIgnoreCaseTest()
		{
			int expected = NonexistantMatchExpectedIndex;
			int actual = CreateTestList().IndexOf(NonexistantMatch, StringComparer.OrdinalIgnoreCase);
			Assert.AreEqual(expected, actual);
		}
		[Test]
		public void IndexOfNonexistantPredicateTest()
		{
			int expected = NonexistantMatchExpectedIndex;
			int actual = CreateTestList().IndexOf(str => str.Equals(NonexistantMatch));
			Assert.AreEqual(expected, actual);
		}
		
		[Test]
		public void IsNullOrEmptyNullTest()
		{
			IList<string> nullList = null;
			Assert.IsTrue(nullList.IsNullOrEmpty());
		}
		[Test]
		public void IsNullOrEmptyEmptyTest()
		{
			IList<string> emptyList = new List<string>();
			Assert.IsTrue(emptyList.IsNullOrEmpty());
		}
		[Test]
		public void IsNullOrEmptyFullTest()
		{
			IList<string> fullList = CreateTestList();
			Assert.IsFalse(fullList.IsNullOrEmpty());
		}
		
		private bool ListsAreEqual(IList<string> expected, IList<string> actual)
		{
			bool result = true;

			result &= expected.Count == actual.Count;
			if (result == false) return false;

			for(int i = 0; i < expected.Count; i++)
			{
				result &= expected[i] == actual[i];
			}

			return result;
		}

		private void TestActionAgainst(Action<IList<string>> actionUnderTest, Action<IList<string>> verificationAction)
		{
			var listUnderTest = CreateTestList();
			var expectedList = CreateTestList();

			actionUnderTest(listUnderTest);
			verificationAction(expectedList);

			Assert.IsTrue(ListsAreEqual(expectedList, listUnderTest));
		}

		private IList<string> CreateTestList()
		{
			return new List<string> {"Foo", "Bar", "Baz"};
		}
	}
}

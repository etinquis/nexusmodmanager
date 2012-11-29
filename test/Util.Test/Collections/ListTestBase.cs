using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util.Test.Collections
{
	abstract class ListTestBase : TestFixtureBase
	{
		protected const string ExistingMatch = "Foo";
		protected const int ExistingMatchIndex = 0;
		protected const string DifferentCase = "foo";
		protected const string NonexistantMatch = "NotValid";
		protected const int NonexistantMatchExpectedIndex = -1;

		protected const int InvalidIndexHigh = int.MaxValue;
		protected const int InvalidIndexLow = -1;

		protected string[] TestItems = { "Foo", "Bar", "Baz" };
		protected string[] TestItemsSorted = { "Bar", "Baz", "Foo" };
		protected string[] Numbers = { "One", "Two", "Three" };
		protected string[] CaseSignificantSortable = { "Abc", "Def", "abc", "Xyz" };
		protected string[] CaseSignificantExpectedSorted = { "abc", "Abc", "Def", "Xyz" };
		protected string[] CaseInsignificantSortable = { "def", "abc", "xyz" };
		protected string[] CaseInsignificantExpectedSorted = { "abc", "def", "xyz" };

		protected virtual bool ListsAreEqual(IList<string> expected, IList<string> actual)
		{
			bool result = true;

			result &= expected.Count == actual.Count;
			if (result == false) return false;

			for (int i = 0; i < expected.Count; i++)
			{
				result &= expected.ElementAt(i) == actual.ElementAt(i);
			}
			
			result &= expected.Contains(ExistingMatch) == actual.Contains(ExistingMatch);
			result &= expected.Contains(NonexistantMatch) == actual.Contains(NonexistantMatch);

			return result;
		}

		protected bool ListsAreEqualBesidesOrder(IList<string> expected, IList<string> actual)
		{
			bool result = true;

			result &= expected.Count == actual.Count;
			if (result == false) return false;

			for (int i = 0; i < expected.Count; i++)
			{
				result &= actual.Contains(expected.ElementAt(i));
			}

			result &= expected.Contains(ExistingMatch) == actual.Contains(ExistingMatch);
			result &= expected.Contains(NonexistantMatch) == actual.Contains(NonexistantMatch);

			return result;
		}
	}
}

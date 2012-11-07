using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Collections;
using Util.Test.Collections.Generic_Interfaces;

namespace Util.Test.Collections
{
	[TestFixture]
	class ObservableSetTests : SetTests
	{
		[Test]
		public void DefaultConstructorTest()
		{
			const int expectedCount = 0;

			var setUnderTest = new ObservableSet<string>();
			Assert.AreEqual(expectedCount, setUnderTest.Count);
			Assert.AreEqual(default(string), setUnderTest.ElementAtOrDefault(0));
		}

		[Test]
		public void EnumerableConstructorTest()
		{
			const int expectedCount = 3;
			var setUnderTest = new ObservableSet<string>(Numbers);
			Assert.AreEqual(expectedCount, setUnderTest.Count);

			for (int i = 0; i < Numbers.Count(); i++)
			{
				Assert.AreEqual(Numbers[i], setUnderTest[i]);
			}
			Assert.AreEqual(default(string), setUnderTest.ElementAtOrDefault(3));
		}
		
		[Test]
		public void EnumerableComparerConstructorTest()
		{
			var setUnderTest = new ObservableSet<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			Assert.IsTrue(ListsAreEqual(TestItems, setUnderTest));
			Assert.IsTrue(setUnderTest.Contains(DifferentCase));
		}

		[Test]
		public void CopySetConstructorTest()
		{
			Set<string> expected = CreateTestSet();
			ObservableSet<string> copiedSet = new ObservableSet<string>(expected);

			Assert.IsTrue(ListsAreEqual(expected, copiedSet));
		}

		[Test]
		public void ComparerConstructorTest()
		{
			var setUnderTest = new ObservableSet<string>(StringComparer.OrdinalIgnoreCase);
			setUnderTest.Add(ExistingMatch);
			Assert.IsTrue(setUnderTest.Contains(DifferentCase));
		}

		[Test]
		public void AddCollectionChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Add(ExistingMatch), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ExistingMatch)));
		}
		[Test]
		public void AddExistingCollectionChangedEventIgnoredTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Add(ExistingMatch)));
		}
		[Test]
		public void RemoveCollectionChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Remove(ExistingMatch), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ExistingMatch)));
		}
		[Test]
		public void RemoveNonExistantCollectionChangedEventIgnoredTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Remove(ExistingMatch)));
		}
		[Test]
		public void RemoveAtCollectionChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.RemoveAt(0), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ExistingMatch)));
		}
		[Test]
		public void ClearCollectionChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Clear(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
		}
		[Test]
		public void ClearEmptyCollectionChangedEventIgnoredTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			Assert.IsTrue(setUnderTest.Count == 0);
			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Clear()));
		}
		
		[Test]
		public void AddCountPropertyChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Add(ExistingMatch)));
		}
		[Test]
		public void AddExistingCountPropertyChangedEventIgnoredTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Add(ExistingMatch)));
		}
		[Test]
		public void RemoveCountPropertyChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Remove(ExistingMatch)));
		}
		[Test]
		public void RemoveNonExistantCountPropertyChangedEventIgnoredTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set=>set.Remove(NonexistantMatch)));
		}
		public void RemoveAtCountPropertyChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.RemoveAt(0)));
		}
		[Test]
		public void ClearCountPropertyChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			setUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Clear()));
		}
		[Test]
		public void ClearEmptySetCountPropertyChangedEventIgnoredTest()
		{
			var setUnderTest = new ObservableSet<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ObservableSet<string>>(setUnderTest);

			Assert.IsTrue(setUnderTest.Count == 0);
			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Clear()));
		}

		protected override Set<string> CreateTestSet()
		{
			return new ObservableSet<string>(TestItems);
		}
	}
}

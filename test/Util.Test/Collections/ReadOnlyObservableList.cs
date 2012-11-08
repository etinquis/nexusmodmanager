using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Nexus.Client.Util.Collections;
using Util.Test.Collections.Generic_Interfaces;

namespace Util.Test.Collections
{
	[TestFixture]
	class ReadOnlyObservableListTests : ListTestBase
	{
		[Test]
		public virtual void ConstructorListNotNotifyColleectionChangedTest()
		{
			Assert.Throws(Is.InstanceOf<Exception>(), () => new ReadOnlyObservableList<string>(TestItems));
		}
		[Test]
		public virtual void ConstructorListNotNotifyPropertyChangedTest()
		{
			var mock = new Mock<INotifyCollectionChanged>();
			var mock2 = mock.As<IList<string>>();

			Assert.Throws(Is.InstanceOf<Exception>(), () => new ReadOnlyObservableList<string>(mock2.Object));
		}

		[Test]
		public virtual void IndexOfTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(ExistingMatchIndex, listUnderTest.IndexOf(ExistingMatch));
		}

		[Test]
		public virtual void ICollectionRemoveTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), () => ((ICollection<string>)listUnderTest).Remove(NonexistantMatch));
		}
		[Test]
		public virtual void IListTRemoveAtTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), ()=> ((IList<string>)listUnderTest).RemoveAt(0));
		}
		[Test]
		public virtual void IListTInsertTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(),
			              () => ((IList<string>) listUnderTest).Insert(ExistingMatchIndex, NonexistantMatch));
		}
		[Test]
		public virtual void ICollectionClearTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), ()=> ((ICollection<string>)listUnderTest).Clear());
		}

		[Test]
		public virtual void ThisGetterTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(ExistingMatch, listUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public virtual void IListTThisSetterTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), () => ((IList<string>)listUnderTest)[ExistingMatchIndex] = NonexistantMatch);
		}
		[Test]
		public virtual void ICollectionTAddTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), () => ((ICollection<string>)listUnderTest).Add(NonexistantMatch));
		}

		[Test]
		public virtual void ContainsTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsTrue(listUnderTest.Contains(ExistingMatch));
		}

		[Test]
		public virtual void CopyToTest()
		{
			var listUnderTest = CreateTestList();
			var actual = new string[listUnderTest.Count];

			listUnderTest.CopyTo(actual, 0);
			Assert.IsTrue(ListsAreEqual(listUnderTest, actual));
		}

		[Test]
		public virtual void IsReadOnlyTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsTrue(listUnderTest.IsReadOnly);
		}

		public virtual void EnumeratorTest()
		{
			var listUnderTest = CreateTestList();
			IEnumerator<string> enumerator = listUnderTest.GetEnumerator();

			int i = 0;
			while(enumerator.MoveNext())
			{
				Assert.AreEqual(TestItems[i], enumerator.Current);
			}
		}

		[Test]
		public virtual void IEnumerableEnumeratorTest()
		{
			IEnumerable listUnderTest = CreateTestList();
			IEnumerator enumerator = listUnderTest.GetEnumerator();

			int i = 0;
			while (enumerator.MoveNext())
			{
				Assert.AreEqual(TestItems[i++], enumerator.Current as string);
			}
		}

		[Test]
		public virtual void AddCollectionChangedEventRaisedByProxyTest()
		{
			var internalList = new ObservableCollection<string>(TestItems);
			var listUnderTest = new ReadOnlyObservableList<string>(internalList);

			var collectionChangeTester = new INotifyCollectionChangedTester<ReadOnlyObservableList<string>>(listUnderTest);
			Assert.IsTrue(collectionChangeTester.DoesActionRaiseEventSynchronous((list)=>internalList.Add(NonexistantMatch), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, NonexistantMatch)));
		}
		[Test]
		public virtual void AddPropertyChangedEventRaisedByProxyTest()
		{
			var internalList = new ObservableCollection<string>(TestItems);
			var listUnderTest = new ReadOnlyObservableList<string>(internalList);

			var propertyChangeTester = new INotifyPropertyChangedTester<ReadOnlyObservableList<string>>(listUnderTest);
			Assert.IsTrue(propertyChangeTester.DoesActionRaiseEventSynchronous((list) => internalList.Add(NonexistantMatch), new PropertyChangedEventArgs("Count")));
		}

		[Test]
		public virtual void RemoveCollectionChangedEventRaisedByProxyTest()
		{
			var internalList = new ObservableCollection<string>(TestItems);
			var listUnderTest = new ReadOnlyObservableList<string>(internalList);

			var collectionChangeTester = new INotifyCollectionChangedTester<ReadOnlyObservableList<string>>(listUnderTest);
			Assert.IsTrue(collectionChangeTester.DoesActionRaiseEventSynchronous((list) => internalList.Remove(ExistingMatch), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ExistingMatch)));
		}
		[Test]
		public virtual void RemovePropertyChangedEventRaisedByProxyTest()
		{
			var internalList = new ObservableCollection<string>(TestItems);
			var listUnderTest = new ReadOnlyObservableList<string>(internalList);

			var propertyChangeTester = new INotifyPropertyChangedTester<ReadOnlyObservableList<string>>(listUnderTest);
			Assert.IsTrue(propertyChangeTester.DoesActionRaiseEventSynchronous((list) => internalList.Remove(ExistingMatch), new PropertyChangedEventArgs("Count")));
		}

		private ReadOnlyObservableList<string> CreateTestList()
		{
			return new ReadOnlyObservableList<string>(new ObservableCollection<string>(TestItems));
		}
	}
}

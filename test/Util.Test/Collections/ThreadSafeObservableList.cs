using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Nexus.Client.Util.Collections;
using Util.Test.Collections.Generic_Interfaces;

namespace Util.Test.Collections
{
	[TestFixture]
	internal class ThreadSafeObservableListTests : ListTestBase
	{
		[Test]
		public virtual void DefaultConstructorTest()
		{
			ThreadSafeObservableList<string> listUnderTest = new ThreadSafeObservableList<string>();
			Assert.AreEqual(0, listUnderTest.Count);
		}
		[Test]
		public virtual void EnumerableConstructorTest()
		{
			ThreadSafeObservableList<string> listUnderTest = new ThreadSafeObservableList<string>(TestItems);
			Assert.AreEqual(TestItems.Count(), listUnderTest.Count);
		}
		[Test]
		public virtual void ComparerConstructorTest()
		{
			ThreadSafeObservableList<string> listUnderTest = new ThreadSafeObservableList<string>(StringComparer.OrdinalIgnoreCase);
			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(listUnderTest.Contains(DifferentCase));
		}
		[Test]
		public virtual void EnumerableComparerConstructorTest()
		{
			ThreadSafeObservableList<string> listUnderTest = new ThreadSafeObservableList<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			Assert.AreEqual(TestItems.Count(), listUnderTest.Count);
			Assert.IsTrue(listUnderTest.Contains(DifferentCase));
		}

		[Test]
		public virtual void IndexOfExistingMatchTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(ExistingMatchIndex, listUnderTest.IndexOf(ExistingMatch));
		}
		[Test]
		public virtual void IndexOfNonexistantMatchTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(NonexistantMatchExpectedIndex, listUnderTest.IndexOf(NonexistantMatch));
		}

		[Test]
		public virtual void InsertTest()
		{
			var listUnderTest = CreateTestList();
			listUnderTest.Insert(0, NonexistantMatch);
			Assert.AreEqual(0, listUnderTest.IndexOf(NonexistantMatch));
		}
		[Test]
		public virtual void InsertInvalidIndexHighTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws<ArgumentOutOfRangeException>(() => listUnderTest.Insert(InvalidIndexHigh, NonexistantMatch));
		}
		[Test]
		public virtual void InsertInvalidIndexLowTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws<ArgumentOutOfRangeException>(() => listUnderTest.Insert(InvalidIndexLow, NonexistantMatch));
		}

		[Test]
		public virtual void ClearTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsNotEmpty(listUnderTest);
			listUnderTest.Clear();
			Assert.IsEmpty(listUnderTest);
		}

		[Test]
		public virtual void CopyToTest()
		{
			var listUnderTest = CreateTestList();
			var actual = new string[listUnderTest.Count];
			listUnderTest.CopyTo(actual, 0);
			Assert.IsTrue(ListsAreEqual(TestItems, actual));
		}

		[Test]
		public virtual void IsReadOnlyTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsFalse(listUnderTest.IsReadOnly);
		}

		[Test]
		public virtual void RemoveTest()
		{
			var listUnderTest = CreateTestList();
			Assert.IsTrue(listUnderTest.Contains(ExistingMatch));
			listUnderTest.Remove(ExistingMatch);
			Assert.IsFalse(listUnderTest.Contains(ExistingMatch));
		}

		[Test]
		public virtual void IEnumerableTGetEnumeratorTest()
		{
			IEnumerable<string> listUnderTest = CreateTestList();
			var enumerator = listUnderTest.GetEnumerator();

			int count = 0;
			while(enumerator.MoveNext())
			{
				Assert.AreEqual(TestItems[count], enumerator.Current);
				count++;
			}
			Assert.AreEqual(TestItems.Length, count);
		}

		[Test]
		public virtual void IEnumerableGetEnumeratorTest()
		{
			IEnumerable listUnderTest = CreateTestList();
			var enumerator = listUnderTest.GetEnumerator();

			int count = 0;
			while (enumerator.MoveNext())
			{
				Assert.AreEqual(TestItems[count], enumerator.Current);
				count++;
			}
			Assert.AreEqual(TestItems.Length, count);
		}

		[Test]
		public virtual void RemoveAtTest()
		{
			var listUnderTest = CreateTestList();
			listUnderTest.RemoveAt(ExistingMatchIndex);
			Assert.AreNotEqual(ExistingMatch, listUnderTest.ElementAt(ExistingMatchIndex));
		}

		[Test]
		public virtual void AddCollectionChangedEventRaisedTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Add(ExistingMatch), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ExistingMatch)));
		}
		[Test]
		public virtual void RemoveCollectionChangedEventRaisedTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Remove(ExistingMatch), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ExistingMatch)));
		}
		[Test]
		public virtual void RemoveNonExistantCollectionChangedEventIgnoredTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Remove(ExistingMatch)));
		}
		[Test]
		public virtual void RemoveAtCollectionChangedEventRaisedTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.RemoveAt(0), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ExistingMatch)));
		}
		[Test]
		public virtual void ClearCollectionChangedEventRaisedTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Clear(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
		}
		[Test]
		public virtual void ClearEmptyCollectionChangedEventIgnoredTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			Assert.IsTrue(listUnderTest.Count == 0);
			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Clear()));
		}

		[Test]
		public virtual void AddCountPropertyChangedEventRaisedTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Add(ExistingMatch)));
		}
		[Test]
		public virtual void RemoveCountPropertyChangedEventRaisedTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Remove(ExistingMatch)));
		}
		[Test]
		public virtual void RemoveNonExistantCountPropertyChangedEventIgnoredTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			listUnderTest.Add(ExistingMatch);
			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Remove(NonexistantMatch)));
		}
		[Test]
		public virtual void RemoveAtCountPropertyChangedEventRaisedTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.RemoveAt(0)));
		}
		[Test]
		public virtual void ClearCountPropertyChangedEventRaisedTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Clear()));
		}
		[Test]
		public virtual void ClearEmptySetCountPropertyChangedEventIgnoredTest()
		{
			var listUnderTest = new ThreadSafeObservableList<string>();
			var collectionChangedTester = new INotifyCollectionChangedTester<ThreadSafeObservableList<string>>(listUnderTest);

			Assert.IsTrue(listUnderTest.Count == 0);
			Assert.IsFalse(collectionChangedTester.DoesActionRaiseEventSynchronous(set => set.Clear()));
		}

		[Test]
		public virtual void ThisSetterTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(ExistingMatch, listUnderTest[ExistingMatchIndex]);
			listUnderTest[ExistingMatchIndex] = NonexistantMatch;
			Assert.AreEqual(NonexistantMatch, listUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public virtual void ThisGetterTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(ExistingMatch, listUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public virtual void ThisGetterNonExistantTest()
		{
			var listUnderTest = CreateTestList();
			string x;
			Assert.Throws(Is.InstanceOf<Exception>(), () => x = listUnderTest[NonexistantMatchExpectedIndex]);
		}

		[Test]
		public virtual void MoveTest()
		{
			var listUnderTest = CreateTestList();
			listUnderTest.Move(ExistingMatchIndex, ExistingMatchIndex + 1);
			Assert.AreEqual(ExistingMatch, listUnderTest[ExistingMatchIndex + 1]);
			Assert.AreNotEqual(ExistingMatch, listUnderTest[ExistingMatchIndex]);
		}
		[Test]
		public virtual void MoveToInvalidIndexLowTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), () => listUnderTest.Move(ExistingMatchIndex, InvalidIndexLow));
		}
		[Test]
		public virtual void MoveToInvalidIndexHighTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), () => listUnderTest.Move(ExistingMatchIndex, InvalidIndexHigh));
		}
		[Test]
		public virtual void MoveFromInvalidIndexLowTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), () => listUnderTest.Move(InvalidIndexLow, ExistingMatchIndex));
		}
		[Test]
		public virtual void MoveFromInvalidIndexHighTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws(Is.InstanceOf<Exception>(), () => listUnderTest.Move(InvalidIndexHigh, ExistingMatchIndex));
		}

		[Test]
		public virtual void RemoveAtThreadedTest(
			[Random(0,int.MaxValue, 5)] int randomSeed, 
			[Random(100, 5000, 5)] int listLength,
			[Random(2, 50, 5)] int numThreads)
		{
			Random random = new Random(randomSeed);

			while((listLength % numThreads) != 0) listLength++;

			int deleteCountPerThread = listLength/numThreads;

			var listUnderTest = new ThreadSafeObservableList<int>();
			for(int i = 0; i < listLength; i++)
			{
				listUnderTest.Add(i);
			}

			Action test = delegate()
				{
					int myCount = deleteCountPerThread;
					while (myCount > 0)
					{
						int removeIndex;
						lock(random)
						{
							removeIndex = random.Next(myCount--);
						}
						listUnderTest.RemoveAt(removeIndex);
					}
				};

			ThreadedTester[] testers = new ThreadedTester[numThreads];
			for(int i = 0; i < numThreads; i++)
			{
				testers[i] = new ThreadedTester(test);
			}

			for(int i = 0; i < numThreads; i++)
			{
				testers[i].Start();
			}

			for (int i = 0; i < numThreads; i++)
			{
				testers[i].Join();
			}

			for (int i = 0; i < numThreads; i++)
			{
				if(testers[i].Exception != null)
				{
					Assert.Fail("Tester {0} failed with exception: {1}", i, testers[i].Exception.Message);
				}
			}
		}

		protected virtual ThreadSafeObservableList<string> CreateTestList()
		{
			return new ThreadSafeObservableList<string>(TestItems);
		}
	}

	class ThreadedTester
	{
		public Action TestAction { get; private set; }
		public Exception Exception { get; private set; }
		private Thread _thread;

		public ThreadedTester(Action testAction)
		{
			TestAction = testAction;
			_thread = new Thread(RunFunc);
		}

		public void Start()
		{
			_thread.Start();
		}

		public void Join()
		{
			_thread.Join();
		}

		private void RunFunc()
		{
			try
			{
				TestAction();
			}
			catch (Exception ex)
			{
				Exception = ex;
				throw;
			}
		}
	}
}

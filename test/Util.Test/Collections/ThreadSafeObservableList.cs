using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Nexus.Client.Util.Collections;

namespace Util.Test.Collections
{
	[TestFixture]
	class ThreadSafeObservableListTests : ListTestBase
	{
		[Test]
		public void DefaultConstructorTest()
		{
			ThreadSafeObservableList<string> listUnderTest = new ThreadSafeObservableList<string>();
			Assert.AreEqual(0, listUnderTest.Count);
		}
		[Test]
		public void EnumerableConstructorTest()
		{
			ThreadSafeObservableList<string> listUnderTest = new ThreadSafeObservableList<string>(TestItems);
			Assert.AreEqual(TestItems.Count(), listUnderTest.Count);
		}
		[Test]
		public void ComparerConstructorTest()
		{
			ThreadSafeObservableList<string> listUnderTest = new ThreadSafeObservableList<string>(StringComparer.OrdinalIgnoreCase);
			listUnderTest.Add(ExistingMatch);
			Assert.IsTrue(listUnderTest.Contains(DifferentCase));
		}
		[Test]
		public void EnumerableComparerConstructorTest()
		{
			ThreadSafeObservableList<string> listUnderTest = new ThreadSafeObservableList<string>(TestItems, StringComparer.OrdinalIgnoreCase);
			Assert.AreEqual(TestItems.Count(), listUnderTest.Count);
			Assert.IsTrue(listUnderTest.Contains(DifferentCase));
		}

		[Test]
		public void IndexOfExistingMatchTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(ExistingMatchIndex, listUnderTest.IndexOf(ExistingMatch));
		}
		[Test]
		public void IndexOfNonexistantMatchTest()
		{
			var listUnderTest = CreateTestList();
			Assert.AreEqual(NonexistantMatchExpectedIndex, listUnderTest.IndexOf(NonexistantMatch));
		}

		[Test]
		public void InsertTest()
		{
			var listUnderTest = CreateTestList();
			listUnderTest.Insert(0, NonexistantMatch);
			Assert.AreEqual(0, listUnderTest.IndexOf(NonexistantMatch));
		}
		[Test]
		public void InsertInvalidIndexHighTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws<ArgumentOutOfRangeException>(() => listUnderTest.Insert(InvalidIndexHigh, NonexistantMatch));
		}
		[Test]
		public void InsertInvalidIndexLowTest()
		{
			var listUnderTest = CreateTestList();
			Assert.Throws<ArgumentOutOfRangeException>(() => listUnderTest.Insert(InvalidIndexLow, NonexistantMatch));
		}

		[Test]
		public void RemoveAtTest()
		{
			var listUnderTest = CreateTestList();
			listUnderTest.RemoveAt(ExistingMatchIndex);
			Assert.AreNotEqual(ExistingMatch, listUnderTest.ElementAt(ExistingMatchIndex));
		}

		[Test]
		public void RemoveAtThreadedTest(
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

		private ThreadSafeObservableList<string> CreateTestList()
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

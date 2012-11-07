using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Collections;

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
			setUnderTest.CollectionChanged +=
				delegate(object sender, NotifyCollectionChangedEventArgs args)
					{
						if (args.Action == NotifyCollectionChangedAction.Add && args.NewItems.Count == 1)
						{
							Assert.Pass();
						}
					};
			setUnderTest.Add(ExistingMatch);

			Assert.Fail();
		}
		[Test]
		public void RemoveCollectionChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			setUnderTest.Add(ExistingMatch);

			setUnderTest.CollectionChanged +=
				delegate(object sender, NotifyCollectionChangedEventArgs args)
				{
					if (args.Action == NotifyCollectionChangedAction.Remove && args.OldItems.Count == 1)
					{
						Assert.Pass();
					}
				};
			setUnderTest.Remove(ExistingMatch);
			Assert.Fail();
		}
		[Test]
		public void RemoveAtCollectionChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			setUnderTest.Add(ExistingMatch);

			setUnderTest.CollectionChanged +=
				delegate(object sender, NotifyCollectionChangedEventArgs args)
				{
					if (args.Action == NotifyCollectionChangedAction.Remove && args.OldItems.Count == 1)
					{
						Assert.Pass();
					}
				};
			setUnderTest.RemoveAt(0);
			Assert.Fail();
		}
		[Test]
		public void ClearCollectionChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			setUnderTest.Add(ExistingMatch);
			setUnderTest.Add(NonexistantMatch);

			setUnderTest.CollectionChanged +=
				delegate(object sender, NotifyCollectionChangedEventArgs args)
				{
					if (args.Action == NotifyCollectionChangedAction.Reset)
					{
						Assert.Pass();
					}
				};
			setUnderTest.Clear();
			Assert.Fail();
		}
		
		[Test]
		public void AddCountPropertyChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			(setUnderTest as INotifyPropertyChanged).PropertyChanged +=
				delegate(object sender, PropertyChangedEventArgs args)
				{
					if (args.PropertyName == "Count")
					{
						Assert.Pass();
					}
				};
			setUnderTest.Add(ExistingMatch);
			Assert.Fail();
		}
		[Test]
		public void RemoveCountPropertyChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			setUnderTest.Add(ExistingMatch);
			(setUnderTest as INotifyPropertyChanged).PropertyChanged +=
				delegate(object sender, PropertyChangedEventArgs args)
				{
					if (args.PropertyName == "Count")
					{
						Assert.Pass();
					}
				};
			setUnderTest.Remove(ExistingMatch);
			Assert.Fail();
		}
		public void RemoveAtCountPropertyChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			setUnderTest.Add(ExistingMatch);
			(setUnderTest as INotifyPropertyChanged).PropertyChanged +=
				delegate(object sender, PropertyChangedEventArgs args)
				{
					if (args.PropertyName == "Count")
					{
						Assert.Pass();
					}
				};
			setUnderTest.RemoveAt(0);
			Assert.Fail();
		}
		[Test]
		public void ClearCountPropertyChangedEventRaisedTest()
		{
			var setUnderTest = new ObservableSet<string>();
			setUnderTest.Add(ExistingMatch);
			(setUnderTest as INotifyPropertyChanged).PropertyChanged +=
				delegate(object sender, PropertyChangedEventArgs args)
				{
					if (args.PropertyName == "Count")
					{
						Assert.Pass();
					}
				};
			setUnderTest.Clear();
			Assert.Fail();
		}
		[Test]
		public void PropertyChangedRemoveHandlerTest()
		{
			var setUnderTest = new ObservableSet<string>();
			PropertyChangedEventHandler failHandler = 
				delegate(object sender, PropertyChangedEventArgs args)
				{
					Assert.Fail();
				};

			(setUnderTest as INotifyPropertyChanged).PropertyChanged += failHandler;
			(setUnderTest as INotifyPropertyChanged).PropertyChanged -= failHandler;

			setUnderTest.Add(ExistingMatch);
		}

		protected override Set<string> CreateTestSet()
		{
			return new ObservableSet<string>(TestItems);
		}
	}
}

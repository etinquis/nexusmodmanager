using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;
using Util.Test.Collections.Generic_Interfaces;

namespace Util.Test
{
	[TestFixture]
	class ObservableObjectTests
	{
		private class TestObservableObject : ObservableObject
		{
			private string _testString;
			public string SetPropertyIfChangedTestProperty
			{
				get { return _testString; }
				set { SetPropertyIfChanged(ref _testString, value, () => SetPropertyIfChangedTestProperty); }
			}

			public bool CheckIfChanged<T>(T oldValue, T newValue)
			{
				return base.CheckIfChanged(oldValue, newValue);
			}

			public void OnPropertyChanged()
			{
				OnPropertyChanged(()=>SetPropertyIfChangedTestProperty);
			}
		}

		[Test]
		public void SetPropertyIfChangedNotifyPropertyChangedEventRaisedTest()
		{
			TestObservableObject objectUnderTest = new TestObservableObject();
			INotifyPropertyChangedTester<TestObservableObject> propertyChangedTester =
				new INotifyPropertyChangedTester<TestObservableObject>(objectUnderTest);

			Assert.IsTrue(
				propertyChangedTester.DoesActionRaiseEventSynchronous(
					(obj) => obj.SetPropertyIfChangedTestProperty = "foo",
					new PropertyChangedEventArgs("SetPropertyIfChangedTestProperty")));
		}

		[Test]
		public void SetPropertyIfChangedNotifyPropertyChangedEventIgnoredTest()
		{
			TestObservableObject objectUnderTest = new TestObservableObject();
			INotifyPropertyChangedTester<TestObservableObject> propertyChangedTester =
				new INotifyPropertyChangedTester<TestObservableObject>(objectUnderTest);
			objectUnderTest.SetPropertyIfChangedTestProperty = "foo";

			Assert.IsFalse(
				propertyChangedTester.DoesActionRaiseEventSynchronous(
					(obj) => obj.SetPropertyIfChangedTestProperty = "foo",
					new PropertyChangedEventArgs("SetPropertyIfChangedTestProperty")));
		}

		[Test]
		public void OnPropertyChangedTest()
		{
			TestObservableObject objectUnderTest = new TestObservableObject();
			INotifyPropertyChangedTester<TestObservableObject> propertyChangedTester =
				new INotifyPropertyChangedTester<TestObservableObject>(objectUnderTest);
			
			Assert.IsTrue(
				propertyChangedTester.DoesActionRaiseEventSynchronous(
					(obj) => obj.OnPropertyChanged(),
					new PropertyChangedEventArgs("SetPropertyIfChangedTestProperty")));
		}

		[Test]
		public void CheckIfChangedTest()
		{
			TestObservableObject objectUnderTest = new TestObservableObject();

			Assert.IsTrue(objectUnderTest.CheckIfChanged("Test", "test"));
		}
		[Test]
		public void CheckIfChangedNullTest()
		{
			TestObservableObject objectUnderTest = new TestObservableObject();

			Assert.IsFalse(objectUnderTest.CheckIfChanged<string>(null, null));
		}
	}
}

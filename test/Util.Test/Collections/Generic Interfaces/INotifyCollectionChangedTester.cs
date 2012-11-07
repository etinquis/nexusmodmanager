using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Util.Test.Collections.Generic_Interfaces
{
	class INotifyCollectionChangedTester<TObjectUnderTest> where TObjectUnderTest : INotifyCollectionChanged
	{
		private TObjectUnderTest ObjectUnderTest { get; set; }
		private bool TestSuccess { get; set; }

		private NotifyCollectionChangedEventArgs ExpectedArgs { get; set; }

		public INotifyCollectionChangedTester(TObjectUnderTest objectUnderTest)
		{
			ObjectUnderTest = objectUnderTest;
		}

		private void NotifyAndPassTest(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (ExpectedArgs != null)
			{
				TestSuccess = ExpectedArgs.Action == args.Action;
			}
			else
			{
				TestSuccess = true;
			}
		}

		public bool DoesActionRaiseEventSynchronous(Action<TObjectUnderTest> testAction, NotifyCollectionChangedEventArgs expectedArgs = null)
		{
			TestSuccess = false;
			ExpectedArgs = expectedArgs;

			ObjectUnderTest.CollectionChanged += NotifyAndPassTest;
			testAction(ObjectUnderTest);
			ObjectUnderTest.CollectionChanged -= NotifyAndPassTest;

			return TestSuccess;
		}
	}
}

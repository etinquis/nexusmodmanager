using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Util.Test.Collections.Generic_Interfaces
{
	internal class INotifyPropertyChangedTester<TObjectUnderTest> where TObjectUnderTest : INotifyPropertyChanged
	{
		private TObjectUnderTest ObjectUnderTest { get; set; }
		private bool TestSuccess { get; set; }

		private PropertyChangedEventArgs ExpectedArgs { get; set; }

		public INotifyPropertyChangedTester(TObjectUnderTest objectUnderTest)
		{
			ObjectUnderTest = objectUnderTest;
		}

		private void NotifyAndPassTest(object sender, PropertyChangedEventArgs args)
		{
			if(ExpectedArgs != null)
			{
				if(ExpectedArgs.PropertyName == args.PropertyName)
				{
					TestSuccess = true;
				}
			}
			else
			{
				TestSuccess = true;
			}
		}

		public bool DoesActionRaiseEventSynchronous(Action<TObjectUnderTest> testAction, PropertyChangedEventArgs expectedArgs = null)
		{
			TestSuccess = false;
			ExpectedArgs = expectedArgs;

			ObjectUnderTest.PropertyChanged += NotifyAndPassTest;
			testAction(ObjectUnderTest);
			ObjectUnderTest.PropertyChanged -= NotifyAndPassTest;

			return TestSuccess;
		}
	}
}

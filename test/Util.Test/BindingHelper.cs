using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class BindingHelperTests
	{
		private class TestObjectSource : ObservableObject
		{
			private string _testString;
			public string TestStringProperty
			{
				get { return _testString; }
				set { SetPropertyIfChanged(ref _testString, value, () => TestStringProperty); }
			}

			private bool _testBool;
			public bool TestBoolProperty
			{
				get { return _testBool; }
				set { SetPropertyIfChanged(ref _testBool, value, () => TestBoolProperty); }
			}
		}

		[Test, Ignore]
		public void CreateFullBindingTest()
		{
			CheckBox checkbox = new CheckBox();
			TestObjectSource source = new TestObjectSource();
			Form frm = new Form();
			frm.Controls.Add(checkbox);

			Binding result = BindingHelper.CreateFullBinding<CheckBox, TestObjectSource>(checkbox, x => x.Checked, source, x => x.TestBoolProperty);

			checkbox.CausesValidation = true;
			source.TestBoolProperty = true;
			frm.ValidateChildren();

			Assert.AreEqual(source.TestBoolProperty, checkbox.Checked);
		}
	}
}

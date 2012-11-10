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
		private IEnumerable<Control> ControlValueSource
		{
			get
			{
				Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(
					typeof (System.Security.Permissions.UIPermissionAttribute));

				Mock<Control> mockControl = new Mock<Control>();
				mockControl.Setup(ctrl => ctrl.Text).Returns("TestName");

				yield return mockControl.Object;
			}
		}

		private IEnumerable<Expression<Func<string>>> ControlPropertyValueSource
		{
			get
			{
				Control control = null;
				yield return () => control.Text;
			}
		}

		private IEnumerable<object> ObjectValueSource
		{
			get
			{
				yield return new {Name = string.Empty};
			}
		}

		private IEnumerable<Expression<Func<string>>> ObjectPropertyValueSource
		{
			get
			{
				var y = new {Name = string.Empty};
				yield return () => y.Name;
			}
		}

		[Test]
		public void CreateManualBindingControlExpObjectExpTest()
		{
			Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(
					typeof(System.Security.Permissions.UIPermissionAttribute));

			Mock<Control> mockControl = new Mock<Control>();
			mockControl.SetupProperty(ctrl => ctrl.Text);

			Control control = mockControl.Object;

			TextBox textBox = new TextBox();
			var objectSource = new TestObjectSource();

			Binding manualBinding = BindingHelper.CreateFullBinding(textBox, () => textBox.Text, objectSource,
			                                                          () => objectSource.StringProperty);

			manualBinding.ReadValue();
			Assert.AreEqual(objectSource.StringProperty, control.Text);
		}

		private class TestObjectSource
		{
			public string StringProperty
			{
				get { return "Foo"; }
				set
				{
					
				}
			}
		}
	}
}

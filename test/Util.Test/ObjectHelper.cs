using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class ObjectHelperTests
	{
		public int IntTestProperty
		{
			get { return 1; }
		}

		public object ObjectTestProperty
		{
			get
			{
				return new object();
			}
		}

		[Test]
		public void GetPropertyNameFuncNoParams()
		{
			Expression<Func<int>> nonUnaryExpression = () => IntTestProperty;
			Assert.IsFalse(nonUnaryExpression.Body is UnaryExpression);
			Assert.AreEqual("IntTestProperty", ObjectHelper.GetPropertyName(nonUnaryExpression));
		}
		[Test]
		public void GetPropertyNameFuncTUnaryTest()
		{
			Expression<Func<object>> unaryExpression = () => IntTestProperty;
			Assert.IsTrue(unaryExpression.Body is UnaryExpression);
			Assert.AreEqual("IntTestProperty", ObjectHelper.GetPropertyName(unaryExpression));
		}

		[Test]
		public void GetPropertyNameFuncTObjTest()
		{
			Expression<Func<ObjectHelperTests, object>> nonUnaryExpression = x => x.ObjectTestProperty;
			Assert.IsFalse(nonUnaryExpression.Body is UnaryExpression);
			Assert.AreEqual("ObjectTestProperty", ObjectHelper.GetPropertyName(nonUnaryExpression));
		}
		[Test]
		public void GetPropertyNameFuncTObjUnaryTest()
		{
			Expression<Func<ObjectHelperTests, object>> unaryExpression = x => x.IntTestProperty;
			Assert.IsTrue(unaryExpression.Body is UnaryExpression);
			Assert.AreEqual("IntTestProperty", ObjectHelper.GetPropertyName(unaryExpression));
		}
	}
}

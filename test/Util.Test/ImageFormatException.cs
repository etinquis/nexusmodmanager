using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class ImageFormatExceptionTests
	{
		private const string TestMessage = "Foo";
		private static readonly ImageFormat TestFormat = ImageFormat.Png;

		[Test]
		public void ConstructorTest()
		{
			ImageFormatException exceptionUnderTest = new ImageFormatException(TestMessage, TestFormat);
			Assert.AreEqual(TestMessage, exceptionUnderTest.Message);
			Assert.AreEqual(TestFormat, exceptionUnderTest.Format);
		}
	}
}

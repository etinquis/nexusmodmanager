using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class TextUtilTests
	{
		private static readonly string TestString
			= "The quick brown fox jumped over the lazy dog.";

		private static readonly byte[] TestStringBytesUTF8
			= Encoding.UTF8.GetBytes(TestString);

		private static readonly byte[] TestStringBytesAscii
			= Encoding.ASCII.GetBytes(TestString);

		private static readonly string TestMultiLineString
			= "The quick brown fox jumped over" + Environment.NewLine
			  + "the lazy dog.";

		private static readonly string[] ExpectedMultiLineResult
			= TestMultiLineString.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

		private static readonly byte[] TestMultiLineStringBytesUTF8
			= Encoding.UTF8.GetBytes(TestMultiLineString);

		private static readonly byte[] TestMultiLineStringBytesAscii
			= Encoding.ASCII.GetBytes(TestMultiLineString);

		[Test]
		public void ByteToStringUTF8Test()
		{
			Assert.AreEqual(TestString, TextUtil.ByteToString(TestStringBytesUTF8));
		}
		[Test]
		public void ByteToStringAsciiTest()
		{
			Assert.AreEqual(TestString, TextUtil.ByteToString(TestStringBytesAscii));
		}
		[Test]
		public void ByteToStringNullBytesTest()
		{
			Assert.Throws(Is.InstanceOf<Exception>(), () => TextUtil.ByteToString(null));
		}

		[Test]
		public void ByteToStringLinesOneLineTest()
		{
			Assert.AreEqual(new[] { TestString }, TextUtil.ByteToStringLines(TestStringBytesAscii));
		}
		[Test]
		public void ByteToStringLinesNullBytesTest()
		{
			Assert.Throws(Is.InstanceOf<Exception>(), () => TextUtil.ByteToStringLines(null));
		}
		[Test]
		public void ByteToStringLinesMultiLinesTest()
		{
			Assert.AreEqual(ExpectedMultiLineResult, TextUtil.ByteToStringLines(TestMultiLineStringBytesAscii));
		}
	}
}

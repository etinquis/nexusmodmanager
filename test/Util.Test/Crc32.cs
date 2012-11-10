using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class Crc32Tests
	{
		private const int CrcHashSize = 32;

		private static readonly string TestString
			= "The quick brown fox jumped over the lazy dog.";
		private static readonly byte[] ExpectedCrc
			= new byte[] { 130, 163, 70, 66 };

		private static readonly uint ExpectedCrcValue = 2191738434;

		[Test]
		public void ConstructorTest()
		{
			Crc32 hashUnderTest = new Crc32();
			Assert.AreEqual(CrcHashSize, hashUnderTest.HashSize);
		}

		[Test]
		public void ComputeHashTest()
		{
			Crc32 hashUnderTest = new Crc32();
			byte[] result = hashUnderTest.ComputeHash(Encoding.UTF8.GetBytes(TestString));
			
			Assert.AreEqual(ExpectedCrc, result.Reverse());
		}

		[Test]
		public void ComputeHashCrcValueTest()
		{
			Crc32 hashUnderTest = new Crc32();
			hashUnderTest.ComputeHash(Encoding.UTF8.GetBytes(TestString));

			Assert.AreEqual(ExpectedCrcValue, hashUnderTest.CrcValue);
		}

		[Test]
		public void ComputeHashNullByteArrayTest()
		{
			Crc32 hashUnderTest = new Crc32();
			Assert.Throws(Is.InstanceOf<Exception>(), () => hashUnderTest.ComputeHash((byte[])null));
		}
		[Test]
		public void ComputeHashInvalidStartTest()
		{
			Crc32 hashUnderTest = new Crc32();
			Assert.Throws(Is.InstanceOf<Exception>(), () => hashUnderTest.ComputeHash(Encoding.UTF8.GetBytes(TestString), -1, 1));
		}
		[Test]
		public void ComputeHashTooBigTest()
		{
			Crc32 hashUnderTest = new Crc32();
			byte[] bytes = Encoding.UTF8.GetBytes(TestString);
			Assert.Throws(Is.InstanceOf<Exception>(), () => hashUnderTest.ComputeHash(bytes, 0, bytes.Length + 1));
		}
		[Test]
		public void ComputeHashInvalidSizeTest()
		{
			Crc32 hashUnderTest = new Crc32();
			Assert.Throws(Is.InstanceOf<Exception>(), () => hashUnderTest.ComputeHash(Encoding.UTF8.GetBytes(TestString), 0, -20));
		}
	}
}

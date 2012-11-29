using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Downloader;

namespace Util.Test.Downloader
{
	[TestFixture]
	class FileBlockTests : FileWriter
	{
		public FileBlockTests() : base(string.Empty, string.Empty) { }

		private byte[] TestData = new byte[] { 0,0,0,0,0,0,0,0,0 };

		[Test]
		public void ConstructorTest()
		{
			var blockUnderTest = new FileBlock(0, TestData);
			Assert.AreEqual(0, blockUnderTest.StartPosition);
			Assert.AreEqual(TestData, blockUnderTest.Data);
		}

		[Test]
		public void CompareToTest()
		{
			var blockUnderTest = new FileBlock(0, TestData);
			var otherBlock = new FileBlock(0, TestData);

			Assert.AreEqual(0, blockUnderTest.CompareTo(otherBlock));
		}
		[Test]
		public void CompareToUnequalTest()
		{
			var blockUnderTest = new FileBlock(0, TestData);
			var otherBlock = new FileBlock(1, new byte[]{});

			Assert.AreNotEqual(0, blockUnderTest.CompareTo(otherBlock));
		}
		[Test, Ignore]
		public void CompareToDifferentDataTest()
		{
			var blockUnderTest = new FileBlock(0, TestData);
			var otherBlock = new FileBlock(0, new byte[] { });

			Assert.AreNotEqual(0, blockUnderTest.CompareTo(otherBlock));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Downloader;

namespace Util.Test.Downloader
{
	[TestFixture]
	class CompletedDownloadEventArgsTests : TestFixtureBase
	{
		private const string TestFileName = "Foo";

		[Test]
		public void ConstructorTest()
		{
			CompletedDownloadEventArgs argsUnderTest = new CompletedDownloadEventArgs(true, TestFileName);
			Assert.IsTrue(argsUnderTest.GotEntireFile);
			Assert.AreEqual(TestFileName, argsUnderTest.SavedFileName);
		}
	}
}

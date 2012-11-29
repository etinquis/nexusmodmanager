using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Downloader;
using TestBed.IO;

namespace Util.Test.Downloader
{
	[TestFixture]
	class FileMetadataTests : TestFixtureBase
	{
		private const string ExpectedFileName = "Enhanced Blood Textures 2_5-60-2-5.rar";
		private const bool ExpectedIsHtml = false;
		private const int ExpectedLength = 35423672;

		[Test]
		public void ConstructorTest()
		{
			FileMetadata objectUnderTest = new FileMetadata();
			Assert.AreEqual(null, objectUnderTest.SuggestedFileName);
			Assert.AreEqual(false, objectUnderTest.IsHtml);
			Assert.AreEqual(0, objectUnderTest.Length);
			Assert.AreEqual(false, objectUnderTest.NotFound);
			Assert.AreEqual(false, objectUnderTest.Exists);
			Assert.AreEqual(false, objectUnderTest.SupportsResume);
		}

		[Test]
		public void ConstructorHeaderCollectionTest()
		{
			FileMetadata objectUnderTest = new FileMetadata(CreateTestHeaderCollection());
			Assert.AreEqual(ExpectedFileName, objectUnderTest.SuggestedFileName);
			Assert.AreEqual(ExpectedIsHtml, objectUnderTest.IsHtml);
			Assert.AreEqual(ExpectedLength, objectUnderTest.Length);
		}

		private WebHeaderCollection CreateTestHeaderCollection()
		{
			var headerCollection = new WebHeaderCollection();
			TextReader reader = FileLoader.LoadFileText("webresponses/skyrimnexusfiletest.response");
			string line;
			while(reader.Peek() != -1)
			{
				line = reader.ReadLine();
				string[] kvp = line.Split(':');
				if(kvp.Count() == 2)
				{
					headerCollection.Add(kvp[0], kvp[1]);
				}
			}

			return headerCollection;
		}
	}
}

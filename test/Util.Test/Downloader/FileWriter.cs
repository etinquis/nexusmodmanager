using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util.Downloader;

namespace Util.Test.Downloader
{
	[TestFixture]
	class FileWriterTests : TestFixtureBase
	{
		[Test]
		public void ConstructorTest()
		{
			FileWriter writerUnderTest = new FileWriter(string.Empty, string.Empty);
		}
	}
}

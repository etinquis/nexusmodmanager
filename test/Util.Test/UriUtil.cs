using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class UriUtilTests
	{
		private const string EXPECTED_URL = "http://google.com/";

		[Test]
		public void BuildUriTest()
		{
			string testUrl = "http://google.com";
			Uri result = UriUtil.BuildUri(testUrl);
			Assert.AreEqual(EXPECTED_URL, result.ToString());
		}

		[Test]
		public void BuildUriNoHttpTest()
		{
			string testUrl = "google.com";
			Uri result = UriUtil.BuildUri(testUrl);
			Assert.AreEqual(EXPECTED_URL, result.ToString());
		}

		[Test]
		public void BuildUriInvalidTest()
		{
			string testUrl = "notanurl!";
			Uri result = UriUtil.BuildUri(testUrl);
			Assert.AreEqual(null, result);
		}

		[Test]
		public void TryBuildUriTest()
		{
			string testUrl = "http://google.com";
			Uri result;
			Assert.IsTrue(UriUtil.TryBuildUri(testUrl, out result));
			Assert.AreEqual(EXPECTED_URL, result.ToString());
		}

		[Test]
		public void TryBuildUriNoHttpTest()
		{
			string testUrl = "google.com";
			Uri result;
			Assert.IsTrue(UriUtil.TryBuildUri(testUrl, out result));
			Assert.AreEqual(EXPECTED_URL, result.ToString());
		}

		[Test]
		public void TryBuildUriInvalidTest()
		{
			string testUrl = "notanurl!";
			Uri result;
			Assert.IsFalse(UriUtil.TryBuildUri(testUrl, out result));
			Assert.AreEqual(null, result);
		}
	}
}

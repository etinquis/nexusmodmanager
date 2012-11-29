using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nexus.Client.Util;
using TestBed.IO;

namespace Util.Test
{
	[TestFixture]
	class ExtendedImageTests : TestFixtureBase
	{
		private static byte[] TestImageBmpBytes = FileLoader.LoadFileBinary("images/test.bmp");
		private static byte[] TestImageTiffBytes = FileLoader.LoadFileBinary("images/test.tif");
		private static byte[] TestImageGifBytes = FileLoader.LoadFileBinary("images/test.gif");
		private static byte[] TestImagePngBytes = FileLoader.LoadFileBinary("images/test.png");
		private static byte[] TestImageJpgBytes = FileLoader.LoadFileBinary("images/test.jpg");
		[Test]
		public void ConstructorTest()
		{
			ExtendedImage imageUnderTest = new ExtendedImage(TestImageBmpBytes);
			Assert.AreEqual(TestImageBmpBytes, imageUnderTest.Data);
			Assert.IsNotNull(imageUnderTest.Image);
		}

		[Test]
		public void GetExtensionBmpTest()
		{
			ExtendedImage imageUnderTest = new ExtendedImage(TestImageBmpBytes);
			Assert.AreEqual(".bmp", imageUnderTest.GetExtension());
		}
		[Test]
		public void GetExtensionTiffTest()
		{
			ExtendedImage imageUnderTest = new ExtendedImage(TestImageTiffBytes);
			Assert.AreEqual(".tif", imageUnderTest.GetExtension());
		}
		[Test]
		public void GetExtensionGifTest()
		{
			ExtendedImage imageUnderTest = new ExtendedImage(TestImageGifBytes);
			Assert.AreEqual(".gif", imageUnderTest.GetExtension());
		}
		[Test]
		public void GetExtensionPngTest()
		{
			ExtendedImage imageUnderTest = new ExtendedImage(TestImagePngBytes);
			Assert.AreEqual(".png", imageUnderTest.GetExtension());
		}
		[Test]
		public void GetExtensionJpgTest()
		{
			ExtendedImage imageUnderTest = new ExtendedImage(TestImageJpgBytes);
			Assert.AreEqual(".jpg", imageUnderTest.GetExtension());
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Nexus.Client.Util;

namespace Util.Test
{
	[TestFixture]
	class TraceUtilTests
	{
		private const string NoExceptionString = "\tNO EXCEPTION.";
		private static readonly Exception TestException = new NotImplementedException();

		private static readonly string ExpectedExceptionString
			= "Exception: " + Environment.NewLine
			  + "Message: " + Environment.NewLine
			  + "\t" + TestException.Message + Environment.NewLine
			  + "Full Trace: " + Environment.NewLine
			  + "\t" + TestException.ToString() + Environment.NewLine;

		private static readonly BadImageFormatException BadFormatException = new BadImageFormatException();

		private static readonly string ExpectedBadFormatExceptionString
			= "Exception: " + Environment.NewLine
			  + "Message: " + Environment.NewLine
			  + "\t" + BadFormatException.Message + Environment.NewLine
			  + "Full Trace: " + Environment.NewLine
			  + "\t" + BadFormatException.ToString() + Environment.NewLine
			  + "File Name:\t" + BadFormatException.FileName + Environment.NewLine
			  + "Fusion Log:\t" + BadFormatException.FusionLog + Environment.NewLine;

		private static readonly Exception InnerException = new Exception();
		private static readonly Exception NestedException = new Exception(string.Empty, InnerException);

		private static readonly string ExpectedNestedExceptionString
			= "Exception: " + Environment.NewLine
			  + "Message: " + Environment.NewLine
			  + "\t" + NestedException.Message + Environment.NewLine
			  + "Full Trace: " + Environment.NewLine
			  + "\t" + NestedException.ToString() + Environment.NewLine
			  + "Inner Exception:" + Environment.NewLine
			  + InnerException.ToString() + Environment.NewLine;

		[Test]
		public void TraceExceptionTest()
		{
			Mock<TraceListener> mockListener = new Mock<TraceListener>(MockBehavior.Strict);
			mockListener.Setup(
				listener =>
				listener.TraceEvent(It.IsAny<TraceEventCache>(), It.IsAny<string>(), It.IsAny<TraceEventType>(), It.IsAny<int>(), It.IsAny<string>())).Callback(Assert.Pass);
			Trace.Listeners.Add(mockListener.Object);
			TraceUtil.TraceException(TestException);

			Assert.Fail();
		}

		[Test]
		public void CreateTraceExceptionStringTest()
		{
			Assert.AreEqual(ExpectedExceptionString, TraceUtil.CreateTraceExceptionString(TestException));
		}
		[Test]
		public void CreateTraceExceptionStringNullExceptionTest()
		{
			Assert.AreEqual(NoExceptionString, TraceUtil.CreateTraceExceptionString(null));
		}

		[Test]
		public void CreateTraceExceptionStringBadFormatExceptionTest()
		{
			Assert.AreEqual(ExpectedBadFormatExceptionString, TraceUtil.CreateTraceExceptionString(BadFormatException));
		}

		[Test]
		public void CreateTraceExceptionInnerExceptionTest()
		{
			Assert.AreEqual(ExpectedNestedExceptionString, TraceUtil.CreateTraceExceptionString(NestedException));
		}
	}
}

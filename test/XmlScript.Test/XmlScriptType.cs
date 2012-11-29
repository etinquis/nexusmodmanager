using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Nexus.Client.ModManagement.Scripting;
using Nexus.Client.ModManagement.Scripting.XmlScript;

namespace XmlScript.Test
{
	[TestFixture]
	public class XmlScriptTypeTests
	{
		private static readonly string MinimumViableScriptString
			= @"<config xmlns:x='http://www.w3.org/2001/XMLSchema-instance' x:noNamespaceSchemaLocation='xmlscript5.00000'>
			<moduleName>XMLScript 5.0 Test</moduleName>
		</config>
		";

		private static readonly StringReader MinimumViableScriptReader = new StringReader(MinimumViableScriptString);

		private static readonly XElement MinimumViableScriptXElement = XElement.Load(MinimumViableScriptReader);

		[Test]
		public void ConstructorTest()
		{
			XmlScriptType typeUnderTest = new XmlScriptType();
			Assert.AreEqual("XmlScript", typeUnderTest.TypeId);
			Assert.AreEqual("XML Script", typeUnderTest.TypeName);
			Assert.AreEqual(new[] { "script.xml", "ModuleConfig.xml" }, typeUnderTest.FileNames);
		}

		[Test]
		public void LoadScriptMinimumViableTest()
		{
			XmlScriptType typeUnderTest = new XmlScriptType();
			IScript script = typeUnderTest.LoadScript(MinimumViableScriptString);
			Assert.IsNotNull(script);
		}

		[Test]
		public void IsXmlScriptValidTest()
		{
			XmlScriptType typeUnderTest = new XmlScriptType();
			Assert.IsTrue(typeUnderTest.IsXmlScriptValid(MinimumViableScriptXElement));
		}

		[Test]
		public void ValidateXmlScriptTest()
		{
			XmlScriptType typeUnderTest = new XmlScriptType();
			typeUnderTest.ValidateXmlScript(MinimumViableScriptXElement);
		}

		[Test]
		public void ValidateScriptTest()
		{
			XmlScriptType typeUnderTest = new XmlScriptType();
			IScript script = typeUnderTest.LoadScript(MinimumViableScriptString);
			typeUnderTest.ValidateScript(script);
		}

		[Test]
		public void GetXmlScriptSchema50Test()
		{
			XmlScriptType typeUnderTest = new XmlScriptType();
			Assert.IsNotNull(typeUnderTest.GetXmlScriptSchema(new Version("5.0")));
		}
		[Test]
		public void GetXmlScriptSchema00Test()
		{
			XmlScriptType typeUnderTest = new XmlScriptType();
			Assert.Throws(Is.InstanceOf<Exception>(), ()=>typeUnderTest.GetXmlScriptSchema(new Version("0.0")));
		}
	}
}

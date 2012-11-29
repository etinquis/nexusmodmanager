using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestBed.IO
{
	public static class FileLoader
	{
		private static readonly string TestFileRoot = 
			Path.Combine(Environment.CurrentDirectory, "../../../objects/");
		private static readonly string RootFilePath = Environment.CurrentDirectory;

		public static Stream OpenFile(string fileName)
		{
			return new FileStream(
				Path.Combine(TestFileRoot, fileName),
				FileMode.Open
				);
		}

		public static byte[] LoadFileBinary(string fileName)
		{
			var stream = new MemoryStream();
			OpenFile(fileName).CopyTo(stream);
			return stream.ToArray();
		}

		public static TextReader LoadFileText(string fileName)
		{
			return new StreamReader(OpenFile(fileName));
		}
	}
}

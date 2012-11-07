using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestBed.Fuzzing
{
	public class FuzzerFactory
	{
		public IFuzzer<string> CreateStringFuzzer(int randomSeed)
		{
			return new StringFuzzer(randomSeed);
		}

		public IFuzzer<Uri> CreateNxmUriFuzzer(int randomSeed)
		{
			return new NxmUriFuzzer(randomSeed);
		}
	}
}

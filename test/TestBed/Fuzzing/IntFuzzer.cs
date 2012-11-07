using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestBed.Fuzzing
{
	class IntFuzzer : Fuzzer<int>
	{
		public IntFuzzer(int randomSeed) : base(randomSeed)
		{
		}

		protected override int GenerateFuzzValue(int seed)
		{
			return seed;
		}
	}
}

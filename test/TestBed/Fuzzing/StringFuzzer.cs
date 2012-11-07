using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestBed.Fuzzing
{
	class StringFuzzer : Fuzzer<string>
	{
		private const int MAX_CHARS = 1000;

		public StringFuzzer(int randomSeed) : base(randomSeed)
		{
			
		}

		protected override string  GenerateFuzzValue(int seed)
		{
			int charCount = seed % MAX_CHARS;
			Random charGen = new Random(seed);

			int[] chars = new int[charCount];

			StringBuilder builder = new StringBuilder();

			for(int i = 0; i < charCount; i++)
			{
				chars[i] = charGen.Next(Char.MaxValue);
				builder.Append(Convert.ToChar(chars[i]));
			}

			return builder.ToString();
		}
}
}

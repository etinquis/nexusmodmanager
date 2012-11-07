using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestBed.Fuzzing
{
	public interface IFuzzer<TParam>
	{
		IEnumerable<TParam> FuzzValues { get; }
	}

	abstract class Fuzzer<TParam> : IFuzzer<TParam>
	{
		private Random Random { get; set; }

		public Fuzzer(int randomSeed)
		{
			Random = new Random(randomSeed);
		}

		public IEnumerable<TParam> FuzzValues
		{
			get
			{
				while(true)
				{
					yield return GenerateFuzzValue(Random.Next());
				}
			}
		}

		protected abstract TParam GenerateFuzzValue(int seed);
	}
}

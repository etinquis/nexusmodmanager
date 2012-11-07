using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestBed.Fuzzing
{
	class NxmUriFuzzer : Fuzzer<Uri>
	{
		private List<string> Games = new List<string>(new [] {"Skyrim", "Oblivion", "Morrowind", "Fallout3", "NewVegas", "DragonAge", "MountAndBlade", "Witcher", "Neverwinter", "WorldOfTanks", "Grimrock", "DarkSouls", "XCom"});  

		public NxmUriFuzzer(int seed) : base(seed)
		{
			
		}

		protected override Uri GenerateFuzzValue(int seed)
		{
			Random rand = new Random(seed);

			string game = Games[rand.Next(Games.Count)];
			string modId = rand.Next(2000).ToString();

			return new Uri(string.Format("nxm://{0}/mods/{1}", game, modId));
		}
	}
}

using System;
using System.Collections.Generic;

namespace ThunderRoad
{
	// Token: 0x0200032C RID: 812
	public static class LevelOptionExtensions
	{
		// Token: 0x060025DF RID: 9695 RVA: 0x00104798 File Offset: 0x00102998
		public static string Get(this LevelOption option)
		{
			if (LevelOptionExtensions._cache == null)
			{
				LevelOptionExtensions._cache = new Dictionary<LevelOption, string>();
			}
			string value;
			if (LevelOptionExtensions._cache.TryGetValue(option, out value) && !string.IsNullOrEmpty(value))
			{
				return value;
			}
			value = option.ToString();
			if (string.IsNullOrEmpty(value))
			{
				throw new Exception(string.Format("LevelOption {0} returned from cache is empty", option));
			}
			LevelOptionExtensions._cache[option] = value;
			return value;
		}

		// Token: 0x040025D0 RID: 9680
		private static Dictionary<LevelOption, string> _cache;
	}
}

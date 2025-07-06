using System;

namespace ThunderRoad.Pools
{
	// Token: 0x02000493 RID: 1171
	public interface IKey<T> : IEquatable<T>
	{
		// Token: 0x060032B1 RID: 12977
		string GetKeyString();
	}
}

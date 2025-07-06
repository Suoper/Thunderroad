using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002EC RID: 748
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Misc/PrefabSpawner.html")]
	[AddComponentMenu("ThunderRoad/Levels/Spawners/Prefab Spawner")]
	public class PrefabSpawner : MonoBehaviour
	{
		// Token: 0x060023EE RID: 9198 RVA: 0x000F5CEA File Offset: 0x000F3EEA
		protected void Start()
		{
			if (this.spawnOnStart)
			{
				this.Spawn();
			}
		}

		// Token: 0x060023EF RID: 9199 RVA: 0x000F5CFC File Offset: 0x000F3EFC
		public void Spawn()
		{
			Transform cachedTransform = base.transform;
			if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && this.platform.HasFlag(PrefabSpawner.Platform.Windows))
			{
				Catalog.InstantiateAsync(this.address, cachedTransform.position, cachedTransform.rotation, cachedTransform, null, "PrefabSpawner");
				return;
			}
			if (Application.platform == RuntimePlatform.Android && this.platform.HasFlag(PrefabSpawner.Platform.Android))
			{
				Catalog.InstantiateAsync(this.address, cachedTransform.position, cachedTransform.rotation, cachedTransform, null, "PrefabSpawner");
			}
		}

		// Token: 0x04002319 RID: 8985
		public string address;

		// Token: 0x0400231A RID: 8986
		public bool spawnOnStart = true;

		// Token: 0x0400231B RID: 8987
		public PrefabSpawner.Platform platform = PrefabSpawner.Platform.Windows | PrefabSpawner.Platform.Android;

		// Token: 0x020009DA RID: 2522
		[Flags]
		public enum Platform
		{
			// Token: 0x04004628 RID: 17960
			Windows = 1,
			// Token: 0x04004629 RID: 17961
			Android = 2
		}
	}
}

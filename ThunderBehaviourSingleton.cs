using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000373 RID: 883
	public class ThunderBehaviourSingleton<T> : ThunderBehaviour where T : ThunderBehaviour
	{
		// Token: 0x17000286 RID: 646
		// (get) Token: 0x060029FB RID: 10747 RVA: 0x0011D29B File Offset: 0x0011B49B
		public static T Instance
		{
			get
			{
				return ThunderBehaviourSingleton<T>._instance;
			}
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x060029FC RID: 10748 RVA: 0x0011D2A2 File Offset: 0x0011B4A2
		public static bool HasInstance
		{
			get
			{
				return ThunderBehaviourSingleton<T>._instance != null;
			}
		}

		// Token: 0x060029FD RID: 10749 RVA: 0x0011D2B4 File Offset: 0x0011B4B4
		private void Awake()
		{
			if (ThunderBehaviourSingleton<T>._instance != null)
			{
				Debug.LogError("Two singleton of type " + base.GetType().Name + " are present. New one will replace the other");
			}
			ThunderBehaviourSingleton<T>._instance = (this as T);
			this.OnSetInstance();
		}

		// Token: 0x060029FE RID: 10750 RVA: 0x0011D308 File Offset: 0x0011B508
		protected virtual void OnSetInstance()
		{
		}

		// Token: 0x060029FF RID: 10751 RVA: 0x0011D30A File Offset: 0x0011B50A
		private void OnDestroy()
		{
			ThunderBehaviourSingleton<T>.InstanceDestroyed onInstanceDestroyed = this.OnInstanceDestroyed;
			if (onInstanceDestroyed != null)
			{
				onInstanceDestroyed();
			}
			ThunderBehaviourSingleton<T>._instance = default(T);
		}

		// Token: 0x14000141 RID: 321
		// (add) Token: 0x06002A00 RID: 10752 RVA: 0x0011D328 File Offset: 0x0011B528
		// (remove) Token: 0x06002A01 RID: 10753 RVA: 0x0011D360 File Offset: 0x0011B560
		public event ThunderBehaviourSingleton<T>.InstanceDestroyed OnInstanceDestroyed;

		// Token: 0x040027C5 RID: 10181
		private static T _instance;

		// Token: 0x02000A8B RID: 2699
		// (Invoke) Token: 0x0600468B RID: 18059
		public delegate void InstanceDestroyed();
	}
}

using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x0200026A RID: 618
	public class GolemSpawner : ThunderBehaviour
	{
		// Token: 0x06001BC2 RID: 7106 RVA: 0x000B7E6C File Offset: 0x000B606C
		private void Start()
		{
			if (this.spawnOnStart)
			{
				this.SpawnGolem();
			}
		}

		// Token: 0x06001BC3 RID: 7107 RVA: 0x000B7E7C File Offset: 0x000B607C
		public void SpawnGolem()
		{
			GolemSpawner.SpawnGolem(this.golemAddress, this.actionOnSpawn, base.transform.position, base.transform.rotation, base.transform, this.arenaCrystalRandomizer, this);
		}

		// Token: 0x06001BC4 RID: 7108 RVA: 0x000B7EB4 File Offset: 0x000B60B4
		public static void SpawnGolem(string address, GolemSpawner.SpawnAction spawnAction, Vector3 position, Quaternion rotation, Transform parent, WeakPointRandomizer arenaCrystalRandomizer, GolemSpawner spawner)
		{
			Catalog.InstantiateAsync(address, position, rotation, parent, delegate(GameObject golemPrefab)
			{
				Golem golem = golemPrefab.GetComponentInChildren<Golem>();
				if (spawner != null)
				{
					spawner.golem = golem;
				}
				golem.transform.rotation = Quaternion.FromToRotation(golem.transform.up, Vector3.up) * golem.transform.rotation;
				golem.spawner = spawner;
				golem.characterController.enableOverlapRecovery = false;
				golem.characterController.radius = 0.01f;
				golem.arenaCrystalRandomizer = arenaCrystalRandomizer;
				if (golem.crystals.IsNullOrEmpty())
				{
					return;
				}
				if (spawnAction == GolemSpawner.SpawnAction.Disable)
				{
					golem.gameObject.SetActive(false);
					return;
				}
				spawner.EnableGolem();
				if (spawnAction == GolemSpawner.SpawnAction.Wake)
				{
					golem.RandomizeCrystalProtection();
					golem.TargetPlayer();
					golem.SetAwake(true);
				}
			}, "GolemSpawner");
		}

		// Token: 0x06001BC5 RID: 7109 RVA: 0x000B7EFC File Offset: 0x000B60FC
		public void EnableGolem()
		{
			if (!this.golem.gameObject.activeInHierarchy)
			{
				this.golem.gameObject.SetActive(true);
			}
			this.golem.characterController.enableOverlapRecovery = false;
			this.golem.characterController.radius = 0.01f;
			this.golem.RandomizeCrystalProtection();
			this.golem.TargetPlayer();
		}

		// Token: 0x06001BC6 RID: 7110 RVA: 0x000B7F68 File Offset: 0x000B6168
		public void WakeGolem()
		{
			if (this.golem == null)
			{
				Debug.LogError("No golem to wake!");
				return;
			}
			if (!this.golem.gameObject.activeInHierarchy)
			{
				this.EnableGolem();
			}
			this.golem.SetAwake(true);
		}

		// Token: 0x06001BC7 RID: 7111 RVA: 0x000B7FA8 File Offset: 0x000B61A8
		public void StunGolem()
		{
			if (this.golem == null || !this.golem.gameObject.activeInHierarchy)
			{
				Debug.LogError("No golem to stun!");
				return;
			}
			this.golem.Stun(this.golem.activeCrystalConfig.arenaCrystalMaxStun);
		}

		// Token: 0x06001BC8 RID: 7112 RVA: 0x000B7FFB File Offset: 0x000B61FB
		public void StunGolem(float time = 0f)
		{
			if (this.golem == null || !this.golem.gameObject.activeInHierarchy)
			{
				Debug.LogError("No golem to stun!");
				return;
			}
			this.golem.Stun(time);
		}

		// Token: 0x06001BC9 RID: 7113 RVA: 0x000B8034 File Offset: 0x000B6234
		public void DefeatGolem()
		{
			if (this.golem == null)
			{
				Debug.LogError("No golem to defeat!");
				return;
			}
			this.golem.Defeat();
		}

		// Token: 0x06001BCA RID: 7114 RVA: 0x000B805C File Offset: 0x000B625C
		public void StartWakeSequence(int num)
		{
			switch (num)
			{
			case 0:
			{
				UnityEvent unityEvent = this.onStartWakeFull;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
				return;
			}
			case 1:
			{
				UnityEvent unityEvent2 = this.onStartWakeShortA;
				if (unityEvent2 == null)
				{
					return;
				}
				unityEvent2.Invoke();
				return;
			}
			case 2:
			{
				UnityEvent unityEvent3 = this.onStartWakeShortB;
				if (unityEvent3 == null)
				{
					return;
				}
				unityEvent3.Invoke();
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x04001AA0 RID: 6816
		public string golemAddress;

		// Token: 0x04001AA1 RID: 6817
		public WeakPointRandomizer arenaCrystalRandomizer;

		// Token: 0x04001AA2 RID: 6818
		public bool spawnOnStart = true;

		// Token: 0x04001AA3 RID: 6819
		public GolemSpawner.SpawnAction actionOnSpawn;

		// Token: 0x04001AA4 RID: 6820
		[Header("Events")]
		public UnityEvent onStartWakeFull;

		// Token: 0x04001AA5 RID: 6821
		public UnityEvent onStartWakeShortA;

		// Token: 0x04001AA6 RID: 6822
		public UnityEvent onStartWakeShortB;

		// Token: 0x04001AA7 RID: 6823
		public UnityEvent onGolemAwaken;

		// Token: 0x04001AA8 RID: 6824
		public UnityEvent onGolemDefeat;

		// Token: 0x04001AA9 RID: 6825
		public UnityEvent onGolemKill;

		// Token: 0x04001AAA RID: 6826
		public UnityEvent onGolemStun;

		// Token: 0x04001AAB RID: 6827
		public UnityEvent onCrystalGrabbed;

		// Token: 0x04001AAC RID: 6828
		public UnityEvent onCrystalUnGrabbed;

		// Token: 0x04001AAD RID: 6829
		[NonSerialized]
		public Golem golem;

		// Token: 0x020008D8 RID: 2264
		public enum SpawnAction
		{
			// Token: 0x040042DB RID: 17115
			None,
			// Token: 0x040042DC RID: 17116
			Disable,
			// Token: 0x040042DD RID: 17117
			Wake
		}
	}
}

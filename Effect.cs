using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000285 RID: 645
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/Effect.html")]
	public class Effect : ThunderBehaviour
	{
		// Token: 0x06001E5E RID: 7774 RVA: 0x000CED47 File Offset: 0x000CCF47
		public virtual void Play()
		{
		}

		// Token: 0x06001E5F RID: 7775 RVA: 0x000CED49 File Offset: 0x000CCF49
		public virtual void Stop()
		{
		}

		// Token: 0x06001E60 RID: 7776 RVA: 0x000CED4B File Offset: 0x000CCF4B
		public virtual void End(bool loopOnly = false)
		{
		}

		// Token: 0x06001E61 RID: 7777 RVA: 0x000CED4D File Offset: 0x000CCF4D
		public virtual void SetIntensity(float value, bool loopOnly = false)
		{
			this.effectIntensity = value;
		}

		// Token: 0x06001E62 RID: 7778 RVA: 0x000CED56 File Offset: 0x000CCF56
		public virtual void SetSpeed(float value, bool loopOnly = false)
		{
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x000CED58 File Offset: 0x000CCF58
		public virtual void SetHaptic(HapticDevice hapticDevice, GameData.HapticClip hapticClipFallBack)
		{
		}

		// Token: 0x06001E64 RID: 7780 RVA: 0x000CED5A File Offset: 0x000CCF5A
		public virtual void SetMainGradient(Gradient gradient)
		{
		}

		// Token: 0x06001E65 RID: 7781 RVA: 0x000CED5C File Offset: 0x000CCF5C
		public virtual void SetSecondaryGradient(Gradient gradient)
		{
		}

		// Token: 0x06001E66 RID: 7782 RVA: 0x000CED5E File Offset: 0x000CCF5E
		public virtual void SetSource(Transform transform)
		{
		}

		// Token: 0x06001E67 RID: 7783 RVA: 0x000CED60 File Offset: 0x000CCF60
		public virtual void SetTarget(Transform transform)
		{
		}

		// Token: 0x06001E68 RID: 7784 RVA: 0x000CED62 File Offset: 0x000CCF62
		public virtual void SetSize(float value)
		{
			this.effectSize = value;
		}

		// Token: 0x06001E69 RID: 7785 RVA: 0x000CED6B File Offset: 0x000CCF6B
		public virtual void SetMesh(Mesh mesh)
		{
		}

		// Token: 0x06001E6A RID: 7786 RVA: 0x000CED6D File Offset: 0x000CCF6D
		public virtual void SetRenderer(Renderer renderer, bool secondary)
		{
		}

		// Token: 0x06001E6B RID: 7787 RVA: 0x000CED6F File Offset: 0x000CCF6F
		public virtual void SetCollider(Collider collider)
		{
		}

		// Token: 0x06001E6C RID: 7788 RVA: 0x000CED71 File Offset: 0x000CCF71
		public virtual void SetNoise(bool noise)
		{
		}

		// Token: 0x06001E6D RID: 7789 RVA: 0x000CED73 File Offset: 0x000CCF73
		private void OnDestroy()
		{
			if (this.isPooled && !GameManager.isQuitting)
			{
				Debug.LogWarning("Effect " + base.name + " has been destroyed but it should not!");
			}
		}

		// Token: 0x06001E6E RID: 7790 RVA: 0x000CED9E File Offset: 0x000CCF9E
		public virtual void CollisionStay(Vector3 position, Quaternion rotation, float speed)
		{
			base.transform.SetPositionAndRotation(position, rotation);
			this.SetSpeed(speed, true);
		}

		// Token: 0x06001E6F RID: 7791 RVA: 0x000CEDB5 File Offset: 0x000CCFB5
		public virtual void CollisionStay(Vector3 position, Quaternion rotation, float speed, float intensity)
		{
			base.transform.SetPositionAndRotation(position, rotation);
			this.SetSpeed(speed, true);
			this.SetIntensity(intensity, true);
		}

		// Token: 0x06001E70 RID: 7792 RVA: 0x000CEDD5 File Offset: 0x000CCFD5
		public virtual void CollisionStay(float speed)
		{
			this.SetSpeed(speed, true);
		}

		// Token: 0x06001E71 RID: 7793 RVA: 0x000CEDDF File Offset: 0x000CCFDF
		public virtual void CollisionStay(float speed, float intensity)
		{
			this.SetSpeed(speed, true);
			this.SetIntensity(intensity, true);
		}

		// Token: 0x06001E72 RID: 7794 RVA: 0x000CEDF1 File Offset: 0x000CCFF1
		public virtual void Despawn()
		{
		}

		// Token: 0x06001E73 RID: 7795 RVA: 0x000CEDF4 File Offset: 0x000CCFF4
		protected void InvokeDespawnCallback()
		{
			Effect.DespawnCallback despawnCallback = this.despawnCallback;
			if (despawnCallback != null)
			{
				despawnCallback(this);
			}
			if (this.despawnCallback != null)
			{
				Delegate[] invocationList = this.despawnCallback.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					Effect.DespawnCallback eventDelegate = invocationList[i] as Effect.DespawnCallback;
					if (eventDelegate != null)
					{
						try
						{
							eventDelegate(this);
						}
						catch (Exception e)
						{
							Debug.LogError(string.Format("Error during DespawnCallback event: {0}", e));
						}
					}
				}
			}
			this.despawnCallback = null;
		}

		// Token: 0x04001CDB RID: 7387
		public Effect.DespawnCallback despawnCallback;

		// Token: 0x04001CDC RID: 7388
		[NonSerialized]
		public bool isPooled;

		// Token: 0x04001CDD RID: 7389
		[NonSerialized]
		public bool isOutOfPool;

		// Token: 0x04001CDE RID: 7390
		[NonSerialized]
		public EffectModule module;

		// Token: 0x04001CDF RID: 7391
		[NonSerialized]
		public LightVolumeReceiver lightVolumeReceiver;

		// Token: 0x04001CE0 RID: 7392
		[NonSerialized]
		public EffectInstance containingInstance;

		// Token: 0x04001CE1 RID: 7393
		public Effect.Step step;

		// Token: 0x04001CE2 RID: 7394
		public int stepCustomHashId;

		// Token: 0x04001CE3 RID: 7395
		public float effectIntensity;

		// Token: 0x04001CE4 RID: 7396
		public float effectSpeed;

		// Token: 0x04001CE5 RID: 7397
		protected float effectSize = 1f;

		// Token: 0x0200092B RID: 2347
		// (Invoke) Token: 0x060042B1 RID: 17073
		public delegate void DespawnCallback(Effect effect);

		// Token: 0x0200092C RID: 2348
		public enum Step
		{
			// Token: 0x040043E0 RID: 17376
			Start,
			// Token: 0x040043E1 RID: 17377
			Loop,
			// Token: 0x040043E2 RID: 17378
			End,
			// Token: 0x040043E3 RID: 17379
			Custom
		}
	}
}

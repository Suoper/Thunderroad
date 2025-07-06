using System;
using System.Collections.Generic;
using ThunderRoad.Reveal;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200028D RID: 653
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectReveal.html")]
	public class EffectReveal : Effect
	{
		// Token: 0x06001ECD RID: 7885 RVA: 0x000D2208 File Offset: 0x000D0408
		public override void Play()
		{
			if (this.currentSize == 0f)
			{
				Debug.LogError("Reveal size is set to 0!");
				return;
			}
			this.playTime = Time.time;
			Transform t = base.transform;
			Vector3 direction = (this.applyOn == EffectReveal.Direction.Target) ? (-t.forward) : t.forward;
			if (base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(RevealMaskProjection.ProjectAsync(t.position + -direction * this.offsetDistance, direction, t.up, this.depth, this.currentSize, this.maskTexture, this.currentChannelMultiplier, this.revealMaterialControllers, this.revealData, new RevealMaskProjection.OnCompleted(this.OnProjectCompleted)));
			}
		}

		// Token: 0x06001ECE RID: 7886 RVA: 0x000D22C8 File Offset: 0x000D04C8
		protected void OnProjectCompleted(float overTime)
		{
			if (overTime > 0f)
			{
				if (this.collisionHandler.isRagdollPart)
				{
					this.collisionHandler.ragdollPart.ragdoll.creature.updateReveal = true;
				}
				else if (this.collisionHandler.isItem)
				{
					this.collisionHandler.item.updateReveal = true;
				}
			}
			base.Invoke("Despawn", overTime + 1f);
		}

		// Token: 0x06001ECF RID: 7887 RVA: 0x000D2337 File Offset: 0x000D0537
		public override void Stop()
		{
			if (this.step == Effect.Step.Loop)
			{
				this.Despawn();
			}
		}

		// Token: 0x06001ED0 RID: 7888 RVA: 0x000D2348 File Offset: 0x000D0548
		public override void End(bool loopOnly = false)
		{
			if (this.step == Effect.Step.Loop)
			{
				this.Despawn();
			}
		}

		// Token: 0x06001ED1 RID: 7889 RVA: 0x000D235C File Offset: 0x000D055C
		public override void SetIntensity(float value, bool loopOnly = false)
		{
			base.SetIntensity(value, loopOnly);
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				this.currentSize = Mathf.Lerp(this.minSize, this.maxSize, value);
				this.currentChannelMultiplier = Vector4.Lerp(this.minChannelMultiplier, this.maxChannelMultiplier, value);
			}
		}

		// Token: 0x06001ED2 RID: 7890 RVA: 0x000D23B0 File Offset: 0x000D05B0
		public override void Despawn()
		{
			if (Application.isPlaying)
			{
				EffectModuleReveal.Despawn(this);
				base.InvokeDespawnCallback();
			}
		}

		// Token: 0x04001DA3 RID: 7587
		public Texture maskTexture;

		// Token: 0x04001DA4 RID: 7588
		public EffectReveal.Direction applyOn = EffectReveal.Direction.Target;

		// Token: 0x04001DA5 RID: 7589
		public float depth = 1.2f;

		// Token: 0x04001DA6 RID: 7590
		public float offsetDistance = 0.05f;

		// Token: 0x04001DA7 RID: 7591
		public float minSize = 0.05f;

		// Token: 0x04001DA8 RID: 7592
		public float maxSize = 0.1f;

		// Token: 0x04001DA9 RID: 7593
		public Vector4 minChannelMultiplier = Vector4.one;

		// Token: 0x04001DAA RID: 7594
		public Vector4 maxChannelMultiplier = Vector4.one;

		// Token: 0x04001DAB RID: 7595
		[NonSerialized]
		public float playTime;

		// Token: 0x04001DAC RID: 7596
		[NonSerialized]
		public float currentSize;

		// Token: 0x04001DAD RID: 7597
		[NonSerialized]
		public Vector4 currentChannelMultiplier;

		// Token: 0x04001DAE RID: 7598
		public CollisionHandler collisionHandler;

		// Token: 0x04001DAF RID: 7599
		public List<RevealMaterialController> revealMaterialControllers;

		// Token: 0x04001DB0 RID: 7600
		public RevealData[] revealData;

		// Token: 0x02000932 RID: 2354
		public enum Direction
		{
			// Token: 0x040043F9 RID: 17401
			Source,
			// Token: 0x040043FA RID: 17402
			Target
		}
	}
}

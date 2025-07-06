using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000291 RID: 657
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/HingeEffect.html")]
	[AddComponentMenu("ThunderRoad/Effects/Hinge effect")]
	public class HingeEffect : ThunderBehaviour
	{
		// Token: 0x06001F01 RID: 7937 RVA: 0x000D3B89 File Offset: 0x000D1D89
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x06001F02 RID: 7938 RVA: 0x000D3B96 File Offset: 0x000D1D96
		protected override void ManagedOnEnable()
		{
			if (!this.loaded)
			{
				this.Load(this.effectId, this.minTorque, this.maxTorque);
			}
		}

		// Token: 0x06001F03 RID: 7939 RVA: 0x000D3BB8 File Offset: 0x000D1DB8
		protected override void ManagedOnDisable()
		{
			EffectInstance effectInstance = this.effectInstance;
			if (effectInstance != null)
			{
				effectInstance.Stop(0);
			}
			this.loaded = false;
		}

		// Token: 0x06001F04 RID: 7940 RVA: 0x000D3BD4 File Offset: 0x000D1DD4
		public virtual void Load(string effectId, float minTorque, float maxTorque)
		{
			if (this.joint != null)
			{
				this.jointRb = this.joint.GetComponent<Rigidbody>();
				this.minTorque = minTorque;
				this.maxTorque = maxTorque;
				if (this.effectInstance == null)
				{
					EffectData effectData = Catalog.GetData<EffectData>(effectId, true);
					this.effectInstance = effectData.Spawn(base.transform.position, base.transform.rotation, base.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					Item item;
					if (this.jointRb.TryGetComponent<Item>(out item))
					{
						this.effectInstance.source = item;
					}
				}
				this.effectInstance.SetIntensity(0f);
				this.effectInstance.Play(0, false, false);
				this.loaded = true;
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06001F05 RID: 7941 RVA: 0x000D3C9C File Offset: 0x000D1E9C
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001F06 RID: 7942 RVA: 0x000D3CA0 File Offset: 0x000D1EA0
		protected internal override void ManagedUpdate()
		{
			if (this.loaded)
			{
				this.currentIntensity = Mathf.InverseLerp(this.minTorque, this.maxTorque, this.jointRb.angularVelocity.magnitude);
				this.effectInstance.SetIntensity(this.currentIntensity);
			}
		}

		// Token: 0x04001DFF RID: 7679
		public string effectId;

		// Token: 0x04001E00 RID: 7680
		public float minTorque = 5f;

		// Token: 0x04001E01 RID: 7681
		public float maxTorque = 12f;

		// Token: 0x04001E02 RID: 7682
		public HingeJoint joint;

		// Token: 0x04001E03 RID: 7683
		public float currentIntensity;

		// Token: 0x04001E04 RID: 7684
		protected EffectInstance effectInstance;

		// Token: 0x04001E05 RID: 7685
		protected Rigidbody jointRb;

		// Token: 0x04001E06 RID: 7686
		protected bool loaded;
	}
}

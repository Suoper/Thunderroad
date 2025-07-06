using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002A1 RID: 673
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/SpringForceEffect")]
	[AddComponentMenu("ThunderRoad/Effects/Spring force effect")]
	public class SpringForceEffect : MonoBehaviour
	{
		// Token: 0x06001F79 RID: 8057 RVA: 0x000D633D File Offset: 0x000D453D
		private void OnEnable()
		{
			if (this.effectInstance != null)
			{
				this.effectInstance.Play(0, false, false);
				this.active = true;
			}
		}

		// Token: 0x06001F7A RID: 8058 RVA: 0x000D635C File Offset: 0x000D455C
		private void OnDisable()
		{
			if (this.effectInstance != null)
			{
				this.effectInstance.Stop(0);
			}
			this.active = false;
		}

		// Token: 0x06001F7B RID: 8059 RVA: 0x000D637C File Offset: 0x000D457C
		public virtual void Awake()
		{
			if (this.springJoint != null)
			{
				EffectData effectData = Catalog.GetData<EffectData>(this.effectId, true);
				if (effectData != null)
				{
					this.effectInstance = effectData.Spawn(base.transform.position, base.transform.rotation, base.transform, null, false, null, false, 1f, 1f, Array.Empty<Type>());
					this.effectInstance.SetIntensity(0f);
					this.effectInstance.Play(0, false, false);
					this.active = true;
				}
			}
		}

		// Token: 0x06001F7C RID: 8060 RVA: 0x000D6408 File Offset: 0x000D4608
		protected virtual void Update()
		{
			if (this.active)
			{
				this.currentIntensity = Mathf.InverseLerp(this.minForce, this.maxForce, this.springJoint.currentForce.magnitude) * Mathf.InverseLerp(this.connectedBodyMinSpeed, this.connectedBodyMaxSpeed, this.springJoint.connectedBody.velocity.magnitude);
				this.effectInstance.SetIntensity(this.currentIntensity);
			}
		}

		// Token: 0x04001EA1 RID: 7841
		public SpringJoint springJoint;

		// Token: 0x04001EA2 RID: 7842
		public string effectId;

		// Token: 0x04001EA3 RID: 7843
		public float minForce = 1f;

		// Token: 0x04001EA4 RID: 7844
		public float maxForce = 5f;

		// Token: 0x04001EA5 RID: 7845
		public float connectedBodyMinSpeed = 0.5f;

		// Token: 0x04001EA6 RID: 7846
		public float connectedBodyMaxSpeed = 3f;

		// Token: 0x04001EA7 RID: 7847
		public float currentIntensity;

		// Token: 0x04001EA8 RID: 7848
		protected EffectInstance effectInstance;

		// Token: 0x04001EA9 RID: 7849
		protected bool active;
	}
}

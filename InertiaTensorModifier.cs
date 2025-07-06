using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000347 RID: 839
	public class InertiaTensorModifier : MonoBehaviour
	{
		// Token: 0x0600273A RID: 10042 RVA: 0x0010F14C File Offset: 0x0010D34C
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (this.referenceCollider == null)
			{
				Transform customInertiaTensorTransform = base.transform.Find("InertiaTensorCollider");
				if (customInertiaTensorTransform)
				{
					this.referenceCollider = customInertiaTensorTransform.GetComponent<CapsuleCollider>();
				}
				else
				{
					customInertiaTensorTransform = new GameObject("InertiaTensorCollider").transform;
					customInertiaTensorTransform.SetParent(base.transform, false);
				}
				if (this.referenceCollider == null)
				{
					this.referenceCollider = customInertiaTensorTransform.gameObject.AddComponent<CapsuleCollider>();
					(this.referenceCollider as CapsuleCollider).radius = 0.05f;
					(this.referenceCollider as CapsuleCollider).direction = 2;
				}
			}
			this.referenceCollider.enabled = false;
			this.referenceCollider.isTrigger = true;
		}

		// Token: 0x0600273B RID: 10043 RVA: 0x0010F219 File Offset: 0x0010D419
		protected void Awake()
		{
			this.physicBody = base.gameObject.GetPhysicBody();
			this.referenceCollider.gameObject.layer = 2;
		}

		// Token: 0x0600273C RID: 10044 RVA: 0x0010F23D File Offset: 0x0010D43D
		private void Start()
		{
			if (this.applyCustomInertiaTensorOnStart)
			{
				this.SetCustomInertiaTensor();
			}
		}

		// Token: 0x0600273D RID: 10045 RVA: 0x0010F250 File Offset: 0x0010D450
		public void CalculateFromInertiaTensorCollider()
		{
			if (this.physicBody == null)
			{
				this.physicBody = base.gameObject.GetPhysicBody();
			}
			if (!this.referenceCollider)
			{
				Debug.LogWarning("Cannot calculate custom inertia tensor because no custom collider has been set on " + base.name);
				this.physicBody.ResetInertiaTensor();
				return;
			}
			this.referenceCollider.isTrigger = true;
			Collider[] rbColliders = this.physicBody.gameObject.GetComponentsInChildren<Collider>();
			int colliderCount = rbColliders.Length;
			for (int i = 0; i < colliderCount; i++)
			{
				Collider rbCollider = rbColliders[i];
				if (!rbCollider.isTrigger)
				{
					rbCollider.enabled = false;
				}
			}
			this.referenceCollider.enabled = true;
			this.referenceCollider.isTrigger = false;
			this.physicBody.ResetInertiaTensor();
			this.customInertiaTensor = this.physicBody.inertiaTensor;
			this.customInertiaTensorRotation = this.physicBody.inertiaTensorRotation;
			for (int j = 0; j < colliderCount; j++)
			{
				rbColliders[j].enabled = true;
			}
			this.referenceCollider.isTrigger = true;
			this.referenceCollider.enabled = false;
		}

		// Token: 0x0600273E RID: 10046 RVA: 0x0010F364 File Offset: 0x0010D564
		public void SetCustomInertiaTensor()
		{
			if (this.customInertiaTensor == Vector3.zero)
			{
				this.CalculateFromInertiaTensorCollider();
			}
			if (!this.physicBody)
			{
				this.physicBody = base.gameObject.GetPhysicBody();
			}
			this.physicBody.inertiaTensor = this.customInertiaTensor;
			this.physicBody.inertiaTensorRotation = this.customInertiaTensorRotation;
		}

		// Token: 0x0600273F RID: 10047 RVA: 0x0010F3C9 File Offset: 0x0010D5C9
		public virtual void ResetInertiaTensor()
		{
			if (!this.physicBody)
			{
				this.physicBody = base.gameObject.GetPhysicBody();
			}
			this.physicBody.ResetInertiaTensor();
		}

		// Token: 0x0400265C RID: 9820
		public Collider referenceCollider;

		// Token: 0x0400265D RID: 9821
		public Vector3 customInertiaTensor;

		// Token: 0x0400265E RID: 9822
		public Quaternion customInertiaTensorRotation;

		// Token: 0x0400265F RID: 9823
		public bool applyCustomInertiaTensorOnStart = true;

		// Token: 0x04002660 RID: 9824
		protected PhysicBody physicBody;
	}
}

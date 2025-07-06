using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200027C RID: 636
	[RequireComponent(typeof(Ragdoll))]
	public class RagdollMassScalar : MonoBehaviour
	{
		// Token: 0x06001DB9 RID: 7609 RVA: 0x000CA941 File Offset: 0x000C8B41
		private void Awake()
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		// Token: 0x06001DBA RID: 7610 RVA: 0x000CA950 File Offset: 0x000C8B50
		private void OnValidate()
		{
			this.standing.OnValidate(base.transform);
			this.handled.OnValidate(base.transform);
			this.ragdolled.OnValidate(base.transform);
		}

		// Token: 0x06001DBB RID: 7611 RVA: 0x000CA985 File Offset: 0x000C8B85
		private void OnTransformChildrenChanged()
		{
			this.standing.ChildrenChanged(base.transform);
			this.handled.ChildrenChanged(base.transform);
			this.ragdolled.ChildrenChanged(base.transform);
		}

		// Token: 0x06001DBC RID: 7612 RVA: 0x000CA9BA File Offset: 0x000C8BBA
		private void OnDrawGizmos()
		{
			this.standing.UpdateMasses();
			this.handled.UpdateMasses();
			this.ragdolled.UpdateMasses();
		}

		// Token: 0x06001DBD RID: 7613 RVA: 0x000CA9E0 File Offset: 0x000C8BE0
		public static float GetTotalMass<T>(List<T> bodies, Func<T, float> getPartMass)
		{
			float total = 0f;
			foreach (T body in bodies)
			{
				total += getPartMass(body);
			}
			return total;
		}

		// Token: 0x06001DBE RID: 7614 RVA: 0x000CAA38 File Offset: 0x000C8C38
		public static void ScaleMass<T>(List<T> bodies, float scale, Func<T, float> getPartMass, Action<T, float> setPartMass)
		{
			foreach (T body in bodies)
			{
				setPartMass(body, getPartMass(body) * scale);
			}
		}

		// Token: 0x04001C3F RID: 7231
		public RagdollMassScalar.PhysicBodyScalar standing = new RagdollMassScalar.PhysicBodyScalar();

		// Token: 0x04001C40 RID: 7232
		public RagdollMassScalar.HandledScalar handled = new RagdollMassScalar.HandledScalar();

		// Token: 0x04001C41 RID: 7233
		public RagdollMassScalar.RagdolledScalar ragdolled = new RagdollMassScalar.RagdolledScalar();

		// Token: 0x02000919 RID: 2329
		[Serializable]
		public abstract class MassScalar<T>
		{
			// Token: 0x06004276 RID: 17014 RVA: 0x0018D404 File Offset: 0x0018B604
			public void PopulateBodies(Transform ragdoll)
			{
				this.bodies = new List<T>();
				foreach (RagdollPart part in ragdoll.GetComponentsInChildren<RagdollPart>())
				{
					this.bodies.Add(this.GetBodyFromPart(part));
				}
			}

			// Token: 0x06004277 RID: 17015
			public abstract T GetBodyFromPart(RagdollPart part);

			// Token: 0x06004278 RID: 17016
			public abstract void SetMass(T t, float mass);

			// Token: 0x06004279 RID: 17017
			public abstract float GetMass(T t);

			// Token: 0x0600427A RID: 17018
			public abstract void DefaultMassesFallback();

			// Token: 0x0600427B RID: 17019 RVA: 0x0018D448 File Offset: 0x0018B648
			public void OnValidate(Transform ragdoll)
			{
				if (this.blockValidate)
				{
					return;
				}
				this.blockValidate = true;
				this.PopulateBodies(ragdoll);
				if (Application.isPlaying)
				{
					return;
				}
				if (this.totalMass < 0f)
				{
					this.totalMass = RagdollMassScalar.GetTotalMass<T>(this.bodies, new Func<T, float>(this.GetMass));
					this.lastTotalMass = this.totalMass;
				}
				if (this.lastTotalMass < 0f)
				{
					this.lastTotalMass = this.totalMass;
				}
				if (!this.totalMass.IsApproximately(this.lastTotalMass))
				{
					RagdollMassScalar.ScaleMass<T>(this.bodies, this.totalMass / this.lastTotalMass, new Func<T, float>(this.GetMass), new Action<T, float>(this.SetMass));
					this.lastTotalMass = this.totalMass;
				}
				this.blockValidate = false;
			}

			// Token: 0x0600427C RID: 17020 RVA: 0x0018D51C File Offset: 0x0018B71C
			public void ChildrenChanged(Transform ragdoll)
			{
				this.PopulateBodies(ragdoll);
			}

			// Token: 0x0600427D RID: 17021 RVA: 0x0018D528 File Offset: 0x0018B728
			public void UpdateMasses()
			{
				this.blockValidate = true;
				float partsMass = RagdollMassScalar.GetTotalMass<T>(this.bodies, new Func<T, float>(this.GetMass));
				if (!partsMass.IsApproximately(this.totalMass))
				{
					this.totalMass = partsMass;
				}
				if (this.totalMass < 0f)
				{
					this.DefaultMassesFallback();
				}
				this.blockValidate = false;
			}

			// Token: 0x0400438D RID: 17293
			[Delayed]
			public float totalMass = -1f;

			// Token: 0x0400438E RID: 17294
			[HideInInspector]
			[NonSerialized]
			protected float lastTotalMass = -1f;

			// Token: 0x0400438F RID: 17295
			protected List<T> bodies;

			// Token: 0x04004390 RID: 17296
			protected bool blockValidate;
		}

		// Token: 0x0200091A RID: 2330
		[Serializable]
		public class PhysicBodyScalar : RagdollMassScalar.MassScalar<PhysicBody>
		{
			// Token: 0x0600427F RID: 17023 RVA: 0x0018D5A2 File Offset: 0x0018B7A2
			public override PhysicBody GetBodyFromPart(RagdollPart part)
			{
				return part.GetPhysicBody();
			}

			// Token: 0x06004280 RID: 17024 RVA: 0x0018D5AA File Offset: 0x0018B7AA
			public override float GetMass(PhysicBody t)
			{
				return t.mass;
			}

			// Token: 0x06004281 RID: 17025 RVA: 0x0018D5B2 File Offset: 0x0018B7B2
			public override void SetMass(PhysicBody t, float mass)
			{
				t.mass = mass;
			}

			// Token: 0x06004282 RID: 17026 RVA: 0x0018D5BB File Offset: 0x0018B7BB
			public override void DefaultMassesFallback()
			{
			}
		}

		// Token: 0x0200091B RID: 2331
		[Serializable]
		public class HandledScalar : RagdollMassScalar.MassScalar<RagdollPart>
		{
			// Token: 0x06004284 RID: 17028 RVA: 0x0018D5C5 File Offset: 0x0018B7C5
			public override RagdollPart GetBodyFromPart(RagdollPart part)
			{
				return part;
			}

			// Token: 0x06004285 RID: 17029 RVA: 0x0018D5C8 File Offset: 0x0018B7C8
			public override float GetMass(RagdollPart t)
			{
				return t.handledMass;
			}

			// Token: 0x06004286 RID: 17030 RVA: 0x0018D5D0 File Offset: 0x0018B7D0
			public override void SetMass(RagdollPart t, float mass)
			{
				t.handledMass = mass;
			}

			// Token: 0x06004287 RID: 17031 RVA: 0x0018D5DC File Offset: 0x0018B7DC
			public override void DefaultMassesFallback()
			{
				this.blockValidate = true;
				this.totalMass = 0f;
				foreach (RagdollPart part in this.bodies)
				{
					part.handledMass = part.GetPhysicBody().mass;
					this.totalMass += part.GetPhysicBody().mass;
				}
				this.blockValidate = false;
			}
		}

		// Token: 0x0200091C RID: 2332
		[Serializable]
		public class RagdolledScalar : RagdollMassScalar.MassScalar<RagdollPart>
		{
			// Token: 0x06004289 RID: 17033 RVA: 0x0018D674 File Offset: 0x0018B874
			public override RagdollPart GetBodyFromPart(RagdollPart part)
			{
				return part;
			}

			// Token: 0x0600428A RID: 17034 RVA: 0x0018D677 File Offset: 0x0018B877
			public override float GetMass(RagdollPart t)
			{
				return t.ragdolledMass;
			}

			// Token: 0x0600428B RID: 17035 RVA: 0x0018D67F File Offset: 0x0018B87F
			public override void SetMass(RagdollPart t, float mass)
			{
				t.ragdolledMass = mass;
			}

			// Token: 0x0600428C RID: 17036 RVA: 0x0018D688 File Offset: 0x0018B888
			public override void DefaultMassesFallback()
			{
			}
		}
	}
}

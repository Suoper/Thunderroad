using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002E7 RID: 743
	[AddComponentMenu("ThunderRoad/Levels/Spawners/Pierced Spawner")]
	public class PiercedSpawner : ThunderBehaviour
	{
		// Token: 0x17000231 RID: 561
		// (get) Token: 0x060023C3 RID: 9155 RVA: 0x000F4E8F File Offset: 0x000F308F
		// (set) Token: 0x060023C4 RID: 9156 RVA: 0x000F4E97 File Offset: 0x000F3097
		public Item SpawnedItem { get; private set; }

		// Token: 0x060023C5 RID: 9157 RVA: 0x000F4EA0 File Offset: 0x000F30A0
		public List<ValueDropdownItem<string>> GetAllWeapons()
		{
			List<ValueDropdownItem<string>> weapons = new List<ValueDropdownItem<string>>();
			foreach (ItemData data in Catalog.GetDataList<ItemData>())
			{
				if (data.type == ItemData.Type.Weapon)
				{
					weapons.Add(new ValueDropdownItem<string>(data.id, data.id));
				}
			}
			return weapons;
		}

		// Token: 0x060023C6 RID: 9158 RVA: 0x000F4F14 File Offset: 0x000F3114
		public List<ValueDropdownItem<string>> GetAllDamagers()
		{
			List<ValueDropdownItem<string>> damagers = new List<ValueDropdownItem<string>>();
			if (this.pierceWeaponData == null)
			{
				this.pierceWeaponData = Catalog.GetData<ItemData>(this.pierceWeaponID, true);
			}
			foreach (ItemData.Damager damager in this.pierceWeaponData.damagers)
			{
				damagers.Add(new ValueDropdownItem<string>(damager.transformName, damager.transformName));
			}
			return damagers;
		}

		// Token: 0x060023C7 RID: 9159 RVA: 0x000F4FA0 File Offset: 0x000F31A0
		public void Spawn(bool findPiecePoint = false)
		{
			if (findPiecePoint)
			{
				this.foundPiercePoint = false;
			}
			this.FindPiercePoint();
			base.StartCoroutine(this.SpawnCoroutine(null));
		}

		// Token: 0x060023C8 RID: 9160 RVA: 0x000F4FC0 File Offset: 0x000F31C0
		protected void Setup()
		{
			this.FindPiercePoint();
			this.pierceWeaponData = Catalog.GetData<ItemData>(this.pierceWeaponID, true);
		}

		// Token: 0x060023C9 RID: 9161 RVA: 0x000F4FDC File Offset: 0x000F31DC
		protected void FindPiercePoint()
		{
			if (this.foundPiercePoint)
			{
				return;
			}
			this.colliderPiercePoint = base.transform.position + base.transform.forward * this.raycastDistance;
			this.distanceToPiercePoint = this.raycastDistance;
			RaycastHit[] hits = Physics.RaycastAll(base.transform.position, base.transform.forward, this.raycastDistance, this.raycastLayerMask, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < hits.Length; i++)
			{
				bool hitEnd = hits[i].collider == this.endPierceCollider;
				if (hitEnd || hits[i].distance < this.distanceToPiercePoint)
				{
					this.colliderPiercePoint = hits[i].point;
					this.distanceToPiercePoint = hits[i].distance;
					this.hitCollider = hits[i].collider;
					if (hitEnd)
					{
						break;
					}
				}
			}
			this.lastCalcPiercePointStart = base.transform.position;
			this.lastCalcPiercePointRotat = base.transform.eulerAngles;
			if (Application.isPlaying)
			{
				this.foundPiercePoint = true;
			}
		}

		// Token: 0x060023CA RID: 9162 RVA: 0x000F50FE File Offset: 0x000F32FE
		protected void Start()
		{
			if (this.parentSpawner != null)
			{
				this.parentSpawner.onSpawnEvent.AddListener(new UnityAction<Item>(this.ParentSpawned));
				return;
			}
			if (this.spawnOnStart)
			{
				this.Spawn(false);
			}
		}

		// Token: 0x060023CB RID: 9163 RVA: 0x000F513C File Offset: 0x000F333C
		public void ParentSpawned(Item item)
		{
			this.parentSpawner.onSpawnEvent.RemoveListener(new UnityAction<Item>(this.ParentSpawned));
			RaycastHit[] hits = Physics.RaycastAll(base.transform.position, base.transform.forward, this.raycastDistance, this.raycastLayerMask, QueryTriggerInteraction.Ignore);
			float closestDistance = float.PositiveInfinity;
			Vector3 closestPoint = Vector3.zero;
			Collider closestCollider = null;
			for (int i = 0; i < hits.Length; i++)
			{
				if (!(hits[i].GetPhysicBody() != item.physicBody) && (closestCollider == null || hits[i].distance < closestDistance))
				{
					closestDistance = hits[i].distance;
					closestPoint = hits[i].point;
					closestCollider = hits[i].collider;
				}
			}
			if (closestCollider != null)
			{
				this.colliderPiercePoint = closestPoint;
				this.endPierceCollider = closestCollider;
				this.foundPiercePoint = true;
			}
			base.StartCoroutine(this.SpawnCoroutine(item));
		}

		// Token: 0x060023CC RID: 9164 RVA: 0x000F523C File Offset: 0x000F343C
		protected IEnumerator SpawnCoroutine(Item parentSpawned)
		{
			PiercedSpawner.<>c__DisplayClass36_0 CS$<>8__locals1 = new PiercedSpawner.<>c__DisplayClass36_0();
			CS$<>8__locals1.<>4__this = this;
			this.Setup();
			this.FindPiercePoint();
			if (this.pierceWeaponData == null)
			{
				Debug.LogError("No pierce weapon could spawn! Data is missing or wrong ID!");
				yield break;
			}
			CS$<>8__locals1.spawnWaiting = 1;
			CS$<>8__locals1.pierceWeapon = null;
			CS$<>8__locals1.piercer = null;
			CS$<>8__locals1.kinematicBodies = new List<PhysicBody>();
			CS$<>8__locals1.parentItem = base.GetComponentInParent<Item>();
			this.pierceWeaponData.SpawnAsync(delegate(Item weapon)
			{
				for (int c = 0; c < weapon.collisionHandlers.Count; c++)
				{
					CollisionHandler handler = weapon.collisionHandlers[c];
					for (int d = 0; d < handler.damagers.Count; d++)
					{
						CS$<>8__locals1.piercer = handler.damagers[d];
						if (CS$<>8__locals1.piercer.name != CS$<>8__locals1.<>4__this.pierceDamagerName)
						{
							CS$<>8__locals1.piercer = null;
						}
						if (CS$<>8__locals1.piercer != null)
						{
							break;
						}
					}
					if (CS$<>8__locals1.piercer != null)
					{
						break;
					}
				}
				weapon.transform.MoveAlign(CS$<>8__locals1.piercer.transform, CS$<>8__locals1.<>4__this.transform, null);
				if (CS$<>8__locals1.parentItem != null)
				{
					weapon.IgnoreItemCollision(CS$<>8__locals1.parentItem, true);
				}
				weapon.physicBody.isKinematic = true;
				CS$<>8__locals1.kinematicBodies.Add(weapon.physicBody);
				CS$<>8__locals1.pierceWeapon = weapon;
				CS$<>8__locals1.<>4__this.SpawnedItem = weapon;
				int spawnWaiting2 = CS$<>8__locals1.spawnWaiting;
				CS$<>8__locals1.spawnWaiting = spawnWaiting2 - 1;
			}, null, null, null, true, null, Item.Owner.None);
			for (int i = 0; i < this.childsToPierce.Count; i++)
			{
				int spawnWaiting = CS$<>8__locals1.spawnWaiting;
				CS$<>8__locals1.spawnWaiting = spawnWaiting + 1;
				ItemSpawner itemSpawner = this.childsToPierce[i];
				itemSpawner.Spawn(false, true, false, null, -1, true);
				itemSpawner.onSpawnEvent.AddListener(new UnityAction<Item>(CS$<>8__locals1.<SpawnCoroutine>g__EndSpawnHandler|1));
			}
			while (CS$<>8__locals1.spawnWaiting > 0)
			{
				yield return null;
			}
			if (CS$<>8__locals1.pierceWeapon == null)
			{
				Debug.LogError("No pierce weapon could spawn!");
				yield break;
			}
			float raycastLength = Vector3.Distance(base.transform.position, this.colliderPiercePoint) + this.targetPierceDepth;
			RaycastHit[] backwardsHits = Physics.RaycastAll(this.colliderPiercePoint, -base.transform.forward, raycastLength, this.raycastLayerMask, QueryTriggerInteraction.Ignore);
			Dictionary<Transform, PiercedSpawner.PierceInfo> pierceInfos = new Dictionary<Transform, PiercedSpawner.PierceInfo>();
			RaycastHit piercerHit = default(RaycastHit);
			int hitsCount = backwardsHits.Length;
			for (int j = 0; j < hitsCount; j++)
			{
				RaycastHit hit = backwardsHits[j];
				if (hit.collider.GetPhysicBody() == CS$<>8__locals1.pierceWeapon.physicBody)
				{
					if (hit.distance < piercerHit.distance)
					{
						piercerHit = hit;
					}
				}
				else if (hit.distance <= raycastLength - this.targetPierceDepth)
				{
					Transform hitTransform = PiercedSpawner.PierceInfo.HitTransform(hit);
					PiercedSpawner.PierceInfo pierceInfo;
					if (pierceInfos.TryGetValue(hitTransform, out pierceInfo))
					{
						pierceInfo.TryUpdateExit(hit.point, hit.collider);
					}
					else
					{
						pierceInfos.Add(hitTransform, new PiercedSpawner.PierceInfo(base.transform, hit, false));
					}
				}
			}
			RaycastHit[] forwardHits = Physics.RaycastAll(base.transform.position, base.transform.forward, raycastLength, this.raycastLayerMask, QueryTriggerInteraction.Ignore);
			hitsCount = forwardHits.Length;
			for (int k = 0; k < hitsCount; k++)
			{
				RaycastHit hit2 = forwardHits[k];
				PiercedSpawner.PierceInfo pierceInfo2;
				if (!pierceInfos.TryGetValue(PiercedSpawner.PierceInfo.HitTransform(hit2), out pierceInfo2))
				{
					pierceInfo2 = new PiercedSpawner.PierceInfo(base.transform, hit2, true);
					pierceInfos.Add(pierceInfo2.objectTransform, pierceInfo2);
				}
				else
				{
					pierceInfo2.TryUpdateEnter(hit2.point, hit2.collider);
				}
				if (hit2.collider == this.endPierceCollider)
				{
					pierceInfo2.endTarget = true;
					Vector3 exitPoint = (k < forwardHits.Length - 1) ? forwardHits[k + 1].point : (hit2.point + base.transform.forward * this.targetPierceDepth);
					pierceInfo2.TryUpdateExit(exitPoint, null);
				}
			}
			if (pierceInfos.Count == 0)
			{
				foreach (PhysicBody physicBody in CS$<>8__locals1.kinematicBodies)
				{
					physicBody.isKinematic = false;
				}
				if (CS$<>8__locals1.parentItem != null)
				{
					CS$<>8__locals1.pierceWeapon.IgnoreItemCollision(CS$<>8__locals1.parentItem, false);
				}
				if (this.despawnIfNoPierce)
				{
					CS$<>8__locals1.pierceWeapon.Despawn();
				}
				UnityEvent unityEvent = this.onFailPierceEvent;
				if (unityEvent != null)
				{
					unityEvent.Invoke();
				}
				yield break;
			}
			float pierceDepthUsed = 0f;
			float pierceDepthRemaining = CS$<>8__locals1.piercer.penetrationDepth;
			List<PiercedSpawner.PierceInfo> sortedPierceInfos = (from pi in pierceInfos.Values
			orderby Vector3.Distance(pi.enterPoint, CS$<>8__locals1.<>4__this.transform.position) descending
			select pi).ToList<PiercedSpawner.PierceInfo>();
			Vector3 closestEnterPoint = sortedPierceInfos[0].enterPoint;
			for (int l = sortedPierceInfos.Count - 1; l >= 0; l--)
			{
				PiercedSpawner.PierceInfo pierceInfo3 = sortedPierceInfos[l];
				if (pierceDepthRemaining > 0f || this.overridePierceDepth)
				{
					float realThickness = pierceInfo3.thickness;
					if (!this.overridePierceDepth)
					{
						realThickness = Mathf.Min(realThickness, pierceDepthRemaining);
					}
					pierceDepthUsed += realThickness;
					pierceDepthRemaining -= realThickness;
					pierceInfo3.piercedDepth = realThickness;
					if (CS$<>8__locals1.piercer.ForcePierce(out pierceInfo3.pierceCollision, pierceInfo3.enterPoint, pierceInfo3.enterCollider, piercerHit.collider, null, null, -1f, null, Damager.PenetrationConditions.DataAllowsPierce | Damager.PenetrationConditions.HasDepth))
					{
						CollisionInstance collisionInstance = pierceInfo3.pierceCollision;
						bool hitRagdoll = pierceInfo3.pierceCollision.damageStruct.hitRagdollPart != null;
						if (this.ragdollDamageSpeed > 0f && hitRagdoll)
						{
							DamagerData damagerData = CS$<>8__locals1.piercer.data;
							collisionInstance.impactVelocity = CS$<>8__locals1.piercer.transform.forward * this.ragdollDamageSpeed;
							collisionInstance.damageStruct.damage = (collisionInstance.damageStruct.baseDamage = damagerData.velocityDamageCurve.Evaluate(collisionInstance.impactVelocity.magnitude));
							CollisionInstance collisionInstance2 = collisionInstance;
							collisionInstance2.damageStruct.damage = collisionInstance2.damageStruct.damage * damagerData.throwedMultiplier;
							CollisionInstance collisionInstance3 = collisionInstance;
							collisionInstance3.damageStruct.damage = collisionInstance3.damageStruct.damage * damagerData.GetTier(CS$<>8__locals1.piercer.collisionHandler).damageMultiplier;
							CollisionInstance collisionInstance4 = collisionInstance;
							collisionInstance4.damageStruct.damage = collisionInstance4.damageStruct.damage * collisionInstance.damageStruct.materialModifier.damageMultiplier;
							CollisionInstance collisionInstance5 = collisionInstance;
							collisionInstance5.damageStruct.damage = collisionInstance5.damageStruct.damage * collisionInstance.damageStruct.hitRagdollPart.data.damageMultiplier;
							collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.Damage(pierceInfo3.pierceCollision);
						}
						if (this.spawnEffects)
						{
							collisionInstance.TryPlayEffects();
						}
						if (this.overridePierceDepth)
						{
							pierceInfo3.updateLimit = true;
						}
					}
					if (pierceInfo3.endTarget)
					{
						pierceDepthRemaining = 0f;
					}
				}
				if (!pierceInfo3.moveable || pierceInfo3.endTarget || (pierceDepthRemaining == 0f && !this.overridePierceDepth))
				{
					sortedPierceInfos.RemoveAt(l);
				}
			}
			Vector3 targetDepthPoint = this.colliderPiercePoint + base.transform.forward * this.targetPierceDepth;
			Vector3 resultDepthPoint = (this.endPierceCollider == null || this.overridePierceDepth) ? targetDepthPoint : Vector3.MoveTowards(closestEnterPoint, targetDepthPoint, CS$<>8__locals1.piercer.penetrationDepth);
			sortedPierceInfos = (from pi in sortedPierceInfos
			orderby Vector3.Distance(pi.exitPoint, CS$<>8__locals1.<>4__this.colliderPiercePoint)
			select pi).ToList<PiercedSpawner.PierceInfo>();
			for (int m = 0; m < sortedPierceInfos.Count; m++)
			{
				PiercedSpawner.PierceInfo pierceInfo4 = sortedPierceInfos[m];
				if (this.alignChildItems && this.endPierceCollider != null)
				{
					Vector3 prevEnter = (m == 0) ? this.colliderPiercePoint : sortedPierceInfos[m - 1].enterPoint;
					pierceInfo4.objectTransform.position += prevEnter - pierceInfo4.exitPoint;
				}
				if (pierceInfo4.updateLimit)
				{
					SoftJointLimit softJointLimit = pierceInfo4.pierceCollision.damageStruct.penetrationJoint.linearLimit;
					softJointLimit.limit = this.targetPierceDepth + (pierceInfo4.endTarget ? 0f : this.externalPierceDepthAllowed);
					pierceInfo4.pierceCollision.damageStruct.penetrationJoint.linearLimit = softJointLimit;
				}
			}
			CS$<>8__locals1.pierceWeapon.transform.MoveAlign(CS$<>8__locals1.piercer.transform, resultDepthPoint, base.transform.rotation, null);
			foreach (PhysicBody physicBody2 in CS$<>8__locals1.kinematicBodies)
			{
				physicBody2.isKinematic = false;
			}
			if (CS$<>8__locals1.parentItem != null)
			{
				CS$<>8__locals1.pierceWeapon.IgnoreItemCollision(CS$<>8__locals1.parentItem, false);
			}
			UnityEvent<Item> unityEvent2 = this.onPierceEvent;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke(CS$<>8__locals1.pierceWeapon);
			}
			yield break;
		}

		// Token: 0x040022D9 RID: 8921
		[Tooltip("Should the item spawn on start, or only when prompted?")]
		public bool spawnOnStart = true;

		// Token: 0x040022DA RID: 8922
		[Tooltip("If the pierced spawner doesn't manage to pierce anything, should the pierce weapon despawn?")]
		public bool despawnIfNoPierce = true;

		// Token: 0x040022DB RID: 8923
		public UnityEvent<Item> onPierceEvent = new UnityEvent<Item>();

		// Token: 0x040022DC RID: 8924
		public UnityEvent onFailPierceEvent = new UnityEvent();

		// Token: 0x040022DD RID: 8925
		[Tooltip("If set to false, the pierce will not spawn any effects.")]
		public bool spawnEffects = true;

		// Token: 0x040022DE RID: 8926
		[Tooltip("If the weapon hits this collider, it should stop piercing at this point and go no deeper!")]
		public string pierceWeaponID;

		// Token: 0x040022DF RID: 8927
		[Tooltip("Pick which damager should do the piercing. This lines up with the pierce transform's name.")]
		public string pierceDamagerName;

		// Token: 0x040022E0 RID: 8928
		[Tooltip("Sets what the 'impact velocity' of the collision should be, if this hits a ragdoll. Any value greater than 0 makes the pierce deal damage to ragdolls it pierces.")]
		public float ragdollDamageSpeed;

		// Token: 0x040022E1 RID: 8929
		[Header("Pierce end")]
		[Tooltip("If the weapon hits this collider, it should stop piercing at this point and go no deeper!")]
		public Collider endPierceCollider;

		// Token: 0x040022E2 RID: 8930
		[NonSerialized]
		public Collider hitCollider;

		// Token: 0x040022E3 RID: 8931
		[Header("Raycasting")]
		public LayerMask raycastLayerMask = 218250337;

		// Token: 0x040022E4 RID: 8932
		public float raycastDistance;

		// Token: 0x040022E5 RID: 8933
		[Header("Depth")]
		[Range(0f, 10f)]
		public float targetPierceDepth;

		// Token: 0x040022E6 RID: 8934
		public bool overridePierceDepth;

		// Token: 0x040022E7 RID: 8935
		[Range(0f, 10f)]
		public float externalPierceDepthAllowed;

		// Token: 0x040022E8 RID: 8936
		[Header("Child pierceables")]
		public bool alignChildItems;

		// Token: 0x040022E9 RID: 8937
		public List<ItemSpawner> childsToPierce;

		// Token: 0x040022EA RID: 8938
		public ItemSpawner parentSpawner;

		// Token: 0x040022EB RID: 8939
		protected bool foundPiercePoint;

		// Token: 0x040022EC RID: 8940
		protected ItemData pierceWeaponData;

		// Token: 0x040022ED RID: 8941
		protected Vector3 lastCalcPiercePointStart;

		// Token: 0x040022EE RID: 8942
		protected Vector3 lastCalcPiercePointRotat;

		// Token: 0x040022EF RID: 8943
		protected Vector3 colliderPiercePoint;

		// Token: 0x040022F0 RID: 8944
		protected float distanceToPiercePoint;

		// Token: 0x020009D0 RID: 2512
		protected class PierceInfo
		{
			// Token: 0x170005A2 RID: 1442
			// (get) Token: 0x06004487 RID: 17543 RVA: 0x001927B9 File Offset: 0x001909B9
			public Vector3 enterPoint
			{
				get
				{
					if (this.enterLocal == null)
					{
						return this.objectTransform.position;
					}
					return this.objectTransform.TransformPoint(this.enterLocal.Value);
				}
			}

			// Token: 0x170005A3 RID: 1443
			// (get) Token: 0x06004488 RID: 17544 RVA: 0x001927EA File Offset: 0x001909EA
			public bool skewer
			{
				get
				{
					return this.enterCollider != null && this.exitCollider != null;
				}
			}

			// Token: 0x170005A4 RID: 1444
			// (get) Token: 0x06004489 RID: 17545 RVA: 0x00192808 File Offset: 0x00190A08
			public Vector3 exitPoint
			{
				get
				{
					if (this.exitLocal == null)
					{
						return this.enterPoint;
					}
					return this.objectTransform.TransformPoint(this.exitLocal.Value);
				}
			}

			// Token: 0x170005A5 RID: 1445
			// (get) Token: 0x0600448A RID: 17546 RVA: 0x00192834 File Offset: 0x00190A34
			public float thickness
			{
				get
				{
					if (this.exitLocal == null)
					{
						return 0f;
					}
					return Vector3.Distance(this.enterPoint, this.exitPoint);
				}
			}

			// Token: 0x0600448B RID: 17547 RVA: 0x0019285C File Offset: 0x00190A5C
			public static Transform HitTransform(RaycastHit hit)
			{
				if (hit.rigidbody != null)
				{
					return hit.rigidbody.transform;
				}
				if (!(hit.articulationBody != null))
				{
					return hit.collider.transform;
				}
				return hit.articulationBody.transform;
			}

			// Token: 0x0600448C RID: 17548 RVA: 0x001928B0 File Offset: 0x00190AB0
			public PierceInfo(Transform piercer, RaycastHit hit, bool forward = true)
			{
				this.pierceReference = piercer;
				this.objectTransform = PiercedSpawner.PierceInfo.HitTransform(hit);
				this.moveable = (hit.rigidbody != null || hit.articulationBody != null);
				Vector3 pointLocal = this.objectTransform.InverseTransformPoint(hit.point);
				if (forward)
				{
					this.enterLocal = new Vector3?(pointLocal);
					this.enterCollider = hit.collider;
					return;
				}
				this.exitLocal = new Vector3?(pointLocal);
				this.exitCollider = hit.collider;
			}

			// Token: 0x0600448D RID: 17549 RVA: 0x00192944 File Offset: 0x00190B44
			public void TryUpdateEnter(Vector3 point, Collider collider)
			{
				if (this.enterLocal == null || this.pierceReference.InverseTransformPoint(point).z < this.pierceReference.InverseTransformPoint(this.enterPoint).z)
				{
					this.enterLocal = new Vector3?(this.objectTransform.InverseTransformPoint(point));
					this.enterCollider = collider;
				}
			}

			// Token: 0x0600448E RID: 17550 RVA: 0x001929A8 File Offset: 0x00190BA8
			public void TryUpdateExit(Vector3 point, Collider collider)
			{
				if (this.exitLocal == null || this.pierceReference.InverseTransformPoint(point).z < this.pierceReference.InverseTransformPoint(this.exitPoint).z)
				{
					this.exitLocal = new Vector3?(this.objectTransform.InverseTransformPoint(point));
					this.exitCollider = collider;
				}
			}

			// Token: 0x040045FA RID: 17914
			public Transform pierceReference;

			// Token: 0x040045FB RID: 17915
			public Transform objectTransform;

			// Token: 0x040045FC RID: 17916
			public bool endTarget;

			// Token: 0x040045FD RID: 17917
			public bool moveable;

			// Token: 0x040045FE RID: 17918
			public Vector3? enterLocal;

			// Token: 0x040045FF RID: 17919
			public Collider enterCollider;

			// Token: 0x04004600 RID: 17920
			public Vector3? exitLocal;

			// Token: 0x04004601 RID: 17921
			public Collider exitCollider;

			// Token: 0x04004602 RID: 17922
			public float piercedDepth;

			// Token: 0x04004603 RID: 17923
			public CollisionInstance pierceCollision;

			// Token: 0x04004604 RID: 17924
			public bool updateLimit;
		}
	}
}

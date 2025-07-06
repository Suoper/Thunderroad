using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000252 RID: 594
	public class CreatureAbilityHitbox : MonoBehaviour
	{
		// Token: 0x1700018D RID: 397
		// (get) Token: 0x060019DD RID: 6621 RVA: 0x000AC3DB File Offset: 0x000AA5DB
		public bool active
		{
			get
			{
				return this.hitCollider.enabled;
			}
		}

		// Token: 0x060019DE RID: 6622 RVA: 0x000AC3E8 File Offset: 0x000AA5E8
		private void Start()
		{
			this.hitCollider = base.GetComponent<Collider>();
			this.hitCollider.enabled = false;
			this.creature = base.GetComponentInParent<Creature>();
		}

		// Token: 0x060019DF RID: 6623 RVA: 0x000AC40E File Offset: 0x000AA60E
		private void CheckInitCreatureAffectedList<T>(Dictionary<Creature, T> dict) where T : new()
		{
			if (dict == null || !dict.ContainsKey(this.creature))
			{
				if (dict == null)
				{
					dict = new Dictionary<Creature, T>();
				}
				dict.Add(this.creature, Activator.CreateInstance<T>());
			}
		}

		// Token: 0x060019E0 RID: 6624 RVA: 0x000AC442 File Offset: 0x000AA642
		public void EnableHitBox(CreatureAbility ability)
		{
			this.ability = ability;
			this.hitCollider.enabled = true;
			this.CheckInitCreatureAffectedList<List<PhysicBody>>(CreatureAbilityHitbox.affectedBodies);
			this.CheckInitCreatureAffectedList<List<Creature>>(CreatureAbilityHitbox.damagedCreatures);
		}

		// Token: 0x060019E1 RID: 6625 RVA: 0x000AC46D File Offset: 0x000AA66D
		public void DisableHitBox()
		{
			this.ability = null;
			this.hitCollider.enabled = false;
			CreatureAbilityHitbox.affectedBodies.Clear();
			CreatureAbilityHitbox.damagedCreatures.Clear();
		}

		// Token: 0x060019E2 RID: 6626 RVA: 0x000AC498 File Offset: 0x000AA698
		private void OnTriggerEnter(Collider other)
		{
			PhysicBody pb = other.GetPhysicBody();
			if (pb != null)
			{
				Item item = pb.gameObject.GetComponentInParent<Item>();
				if (item != null)
				{
					if (item.breakable != null && this.ability.breakBreakables)
					{
						item.breakable.Break();
					}
					return;
				}
				CreatureAbilityHitbox.<>c__DisplayClass15_0 CS$<>8__locals1;
				CS$<>8__locals1.hitCreature = null;
				RagdollPart part;
				if (pb.gameObject.TryGetComponent<RagdollPart>(out part))
				{
					CS$<>8__locals1.hitCreature = part.ragdoll.creature;
				}
				else
				{
					Creature creature = pb.gameObject.GetComponentInParent<Creature>();
					if (creature != null)
					{
						CS$<>8__locals1.hitCreature = creature;
					}
				}
				if (CS$<>8__locals1.hitCreature == null || CS$<>8__locals1.hitCreature == this.creature)
				{
					return;
				}
				if (this.damager && !CreatureAbilityHitbox.damagedCreatures[this.creature].Contains(CS$<>8__locals1.hitCreature))
				{
					CS$<>8__locals1.hitCreature.Damage(this.ability.contactDamage);
					CreatureAbilityHitbox.damagedCreatures[this.creature].Add(CS$<>8__locals1.hitCreature);
				}
				foreach (CreatureAbilityHitbox.InflictedStatus status in this.applyStatuses)
				{
					if (!CS$<>8__locals1.hitCreature.HasStatus(status.data))
					{
						CS$<>8__locals1.hitCreature.Inflict(status.data, this, status.duration, null, true);
					}
				}
				if (!this.forcer)
				{
					return;
				}
				Vector3 force = (CS$<>8__locals1.hitCreature.transform.position - this.hitCollider.bounds.center).normalized * this.ability.contactForce;
				if (CS$<>8__locals1.hitCreature.isPlayer)
				{
					if (!CreatureAbilityHitbox.affectedBodies[this.creature].Contains(CS$<>8__locals1.hitCreature.currentLocomotion.physicBody))
					{
						if (this.ability.forceUngrip)
						{
							this.<OnTriggerEnter>g__Ungrab|15_0(Side.Left, ref CS$<>8__locals1);
							this.<OnTriggerEnter>g__Ungrab|15_0(Side.Right, ref CS$<>8__locals1);
						}
						CS$<>8__locals1.hitCreature.currentLocomotion.physicBody.AddForce(CS$<>8__locals1.hitCreature.locomotion.isGrounded ? force.ToXZ() : force, ForceMode.VelocityChange);
						CreatureAbilityHitbox.affectedBodies[this.creature].Add(CS$<>8__locals1.hitCreature.currentLocomotion.physicBody);
						return;
					}
				}
				else
				{
					CS$<>8__locals1.hitCreature.ragdoll.SetState(Ragdoll.State.Destabilized);
					foreach (RagdollPart rPart in CS$<>8__locals1.hitCreature.ragdoll.parts)
					{
						if (!CreatureAbilityHitbox.affectedBodies[this.creature].Contains(rPart.physicBody))
						{
							rPart.physicBody.AddForce(force, ForceMode.VelocityChange);
							CreatureAbilityHitbox.affectedBodies[this.creature].Add(rPart.physicBody);
						}
					}
				}
			}
		}

		// Token: 0x060019E3 RID: 6627 RVA: 0x000AC7B8 File Offset: 0x000AA9B8
		private void OnTriggerExit(Collider other)
		{
			if (!this.ability.multiHit)
			{
				return;
			}
			PhysicBody pb = other.GetPhysicBody();
			if (pb != null && CreatureAbilityHitbox.affectedBodies[this.creature].Contains(pb))
			{
				CreatureAbilityHitbox.affectedBodies[this.creature].Remove(pb);
				Creature exitCreature = null;
				RagdollPart part;
				if (pb.gameObject.TryGetComponent<RagdollPart>(out part))
				{
					exitCreature = part.ragdoll.creature;
				}
				else
				{
					Creature creature = pb.gameObject.GetComponentInParent<Creature>();
					if (creature != null)
					{
						exitCreature = creature;
					}
				}
				if (exitCreature != null)
				{
					if (CreatureAbilityHitbox.affectedBodies[this.creature].Contains(exitCreature.currentLocomotion.physicBody))
					{
						return;
					}
					foreach (RagdollPart ragdollPart in this.creature.ragdoll.parts)
					{
						if (CreatureAbilityHitbox.affectedBodies[this.creature].Contains(ragdollPart.physicBody))
						{
							return;
						}
					}
					CreatureAbilityHitbox.damagedCreatures.Remove(exitCreature);
				}
			}
		}

		// Token: 0x060019E6 RID: 6630 RVA: 0x000AC920 File Offset: 0x000AAB20
		[CompilerGenerated]
		private void <OnTriggerEnter>g__Ungrab|15_0(Side side, ref CreatureAbilityHitbox.<>c__DisplayClass15_0 A_2)
		{
			RagdollHand hand = A_2.hitCreature.GetHand(side);
			if (hand == null)
			{
				return;
			}
			if (hand.climb != null)
			{
				hand.climb.UnGrip();
			}
			Handle grabbedHandle = hand.grabbedHandle;
			UnityEngine.Object x;
			if (grabbedHandle == null)
			{
				x = null;
			}
			else
			{
				PhysicBody physicBodyInParent = grabbedHandle.GetPhysicBodyInParent();
				if (physicBodyInParent == null)
				{
					x = null;
				}
				else
				{
					GameObject gameObject = physicBodyInParent.gameObject;
					if (gameObject == null)
					{
						x = null;
					}
					else
					{
						RagdollPart component = gameObject.GetComponent<RagdollPart>();
						x = ((component != null) ? component.ragdoll : null);
					}
				}
			}
			CreatureAbility creatureAbility = this.ability;
			UnityEngine.Object y;
			if (creatureAbility == null)
			{
				y = null;
			}
			else
			{
				Creature creature = creatureAbility.creature;
				y = ((creature != null) ? creature.ragdoll : null);
			}
			if (x == y)
			{
				hand.UnGrab(false);
			}
		}

		// Token: 0x040018AD RID: 6317
		public bool forcer = true;

		// Token: 0x040018AE RID: 6318
		public bool damager = true;

		// Token: 0x040018AF RID: 6319
		public List<CreatureAbilityHitbox.InflictedStatus> applyStatuses = new List<CreatureAbilityHitbox.InflictedStatus>();

		// Token: 0x040018B0 RID: 6320
		[NonSerialized]
		public Collider hitCollider;

		// Token: 0x040018B1 RID: 6321
		protected Creature creature;

		// Token: 0x040018B2 RID: 6322
		protected CreatureAbility ability;

		// Token: 0x040018B3 RID: 6323
		protected static Dictionary<Creature, List<PhysicBody>> affectedBodies = new Dictionary<Creature, List<PhysicBody>>();

		// Token: 0x040018B4 RID: 6324
		protected static Dictionary<Creature, List<Creature>> damagedCreatures = new Dictionary<Creature, List<Creature>>();

		// Token: 0x02000892 RID: 2194
		[Serializable]
		public class InflictedStatus
		{
			// Token: 0x17000529 RID: 1321
			// (get) Token: 0x060040AE RID: 16558 RVA: 0x00189372 File Offset: 0x00187572
			private List<ValueDropdownItem<string>> GetAllStatuses
			{
				get
				{
					return Catalog.GetDropdownAllID<StatusData>("None");
				}
			}

			// Token: 0x04004205 RID: 16901
			public string data;

			// Token: 0x04004206 RID: 16902
			public float duration = 3f;
		}
	}
}

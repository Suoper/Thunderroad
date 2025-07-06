using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using ThunderRoad.Skill.Spell;
using ThunderRoad.Skill.SpellMerge;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000263 RID: 611
	[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Beam config")]
	public class GolemBeam : GolemAbility
	{
		// Token: 0x06001B76 RID: 7030 RVA: 0x000B584B File Offset: 0x000B3A4B
		private List<ValueDropdownItem<string>> GetAllSpellIDs()
		{
			return Catalog.GetDropdownAllID<SpellData>("None");
		}

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06001B77 RID: 7031 RVA: 0x000B5857 File Offset: 0x000B3A57
		public override bool HeadshotInterruptable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06001B78 RID: 7032 RVA: 0x000B585C File Offset: 0x000B3A5C
		public override bool OverrideLook
		{
			get
			{
				GolemBeam.State state = this.state;
				if ((state == GolemBeam.State.Deploying || state == GolemBeam.State.Firing) && this.golem != null)
				{
					LookMode lookMode = this.golem.lookMode;
					return lookMode == LookMode.HorizontalSweep || lookMode == LookMode.VerticalSweep;
				}
				return false;
			}
		}

		// Token: 0x06001B79 RID: 7033 RVA: 0x000B58A4 File Offset: 0x000B3AA4
		public override bool Allow(GolemController golem)
		{
			return base.Allow(golem) && Time.time - GolemBeam.lastBeamTime > this.beamCooldownDuration && golem.IsSightable(golem.attackTarget, this.beamMaxDistance, this.beamStartMaxAngle);
		}

		// Token: 0x06001B7A RID: 7034 RVA: 0x000B58DC File Offset: 0x000B3ADC
		public override void Begin(GolemController golem)
		{
			base.Begin(golem);
			GolemBeam.lastBeamTime = Time.time;
			if (this.chargeEffectData == null)
			{
				this.chargeEffectData = Catalog.GetData<EffectData>(this.chargeEffectID, true);
			}
			if (this.beamEffectData == null)
			{
				this.beamEffectData = Catalog.GetData<EffectData>(this.beamEffectID, true);
			}
			if (this.deflectPoint == null)
			{
				this.deflectPoint = golem.transform.FindOrAddTransform("DeflectPoint", Vector3.zero, null, null);
			}
			if (this.leaveMoltenTrail && this.moltenBeamSpell == null)
			{
				this.moltenBeamSpell = (Catalog.GetData<SpellData>(this.moltenBeamSpellId, true) as SpellMergeMoltenBeam);
			}
			golem.weakpoints.Add(golem.headCrystalBody.transform);
			this.beamTargetLostDuration = 0f;
			this.sweepMode = (LookMode)UnityEngine.Random.Range(0, 3);
			LookMode lookMode = this.sweepMode;
			if (lookMode > LookMode.HorizontalSweep)
			{
				if (lookMode == LookMode.VerticalSweep)
				{
					this.sweepSide = false;
				}
			}
			else
			{
				this.sweepSide = (UnityEngine.Random.Range(0, 2) != 0);
			}
			float duration = UnityEngine.Random.Range(this.beamingMinMaxDuration.x, this.beamingMinMaxDuration.y);
			golem.Deploy(duration, new Action(this.OnDeployStart), new Action(this.OnDeployed), new Action(this.OnDeployEnd));
		}

		// Token: 0x06001B7B RID: 7035 RVA: 0x000B5A30 File Offset: 0x000B3C30
		public void OnDeployStart()
		{
			this.state = GolemBeam.State.Deploying;
			this.golem.lookMode = this.sweepMode;
			this.lockSweep = true;
			this.golem.headLookSpeedMultiplier = ((this.sweepMode == LookMode.Follow) ? 0.5f : 1f);
			this.BeamCharge();
		}

		// Token: 0x06001B7C RID: 7036 RVA: 0x000B5A81 File Offset: 0x000B3C81
		public void OnDeployed()
		{
			if (this.state != GolemBeam.State.Deploying)
			{
				return;
			}
			this.lockSweep = false;
			this.state = GolemBeam.State.Firing;
			this.BeamStartLoop();
		}

		// Token: 0x06001B7D RID: 7037 RVA: 0x000B5AA0 File Offset: 0x000B3CA0
		public void OnDeployEnd()
		{
			base.End();
		}

		// Token: 0x06001B7E RID: 7038 RVA: 0x000B5AA8 File Offset: 0x000B3CA8
		public override void OnCycle(float delta)
		{
			base.OnCycle(delta);
			if (this.golem.IsSightable(this.golem.attackTarget, this.beamMaxDistance, this.beamAngleHardMax))
			{
				this.beamTargetLostDuration = 0f;
				return;
			}
			this.beamTargetLostDuration += delta;
			if (this.beamTargetLostDuration > this.beamStopDelayTargetLost)
			{
				base.End();
			}
		}

		// Token: 0x06001B7F RID: 7039 RVA: 0x000B5B10 File Offset: 0x000B3D10
		public override void OnUpdate()
		{
			base.OnUpdate();
			switch (this.state)
			{
			case GolemBeam.State.Deploying:
			case GolemBeam.State.Finished:
				break;
			case GolemBeam.State.Firing:
				this.BeamUpdate();
				break;
			default:
				return;
			}
		}

		// Token: 0x06001B80 RID: 7040 RVA: 0x000B5B44 File Offset: 0x000B3D44
		public override void LookAt()
		{
			base.LookAt();
			this.golem.headIktarget.rotation = Quaternion.LookRotation(this.golem.lookingTarget.position - this.golem.transform.position, Vector3.up);
			Vector3 targetPosition = this.golem.lookingTarget.position + ((this.golem.lookMode == LookMode.VerticalSweep) ? this.golem.headIktarget.transform.up : this.golem.headIktarget.transform.right) * ((this.sweepSide ? this.headSweepRange : (-this.headSweepRange)) * this.headSweepRangeMultiplier);
			this.golem.headIktarget.transform.position = Vector3.MoveTowards(this.golem.headIktarget.transform.position, targetPosition, this.golem.headLookSpeed * this.golem.headLookSpeedMultiplier * Time.deltaTime);
			if (!this.lockSweep && this.golem.headIktarget.position.PointInRadius(targetPosition, 0.1f))
			{
				this.sweepSide = !this.sweepSide;
			}
			this.golem.eyeTransform.rotation = Quaternion.LookRotation(this.golem.headIktarget.position - this.golem.eyeTransform.transform.position, Vector3.up);
		}

		// Token: 0x06001B81 RID: 7041 RVA: 0x000B5CD0 File Offset: 0x000B3ED0
		public void BeamUpdate()
		{
			bool deflected = false;
			if (this.golem.WithinForwardCone(this.golem.attackTarget, 1000f, this.beamAngleHardMax))
			{
				this.FireBeam(this.golem.eyeTransform.position, this.radius, this.golem.eyeTransform.forward, this.hitRange, this.beamRaycastMask, true, out deflected);
			}
			else
			{
				base.End();
			}
			if (deflected || this.deflectedBeamEffect == null)
			{
				return;
			}
			this.deflectedBeamEffect.Stop(0);
			this.deflectedBeamEffect = null;
		}

		// Token: 0x06001B82 RID: 7042 RVA: 0x000B5D64 File Offset: 0x000B3F64
		public void FireBeam(Vector3 start, float radius, Vector3 direction, float beamLength, LayerMask mask, bool tryDeflect, out bool deflected)
		{
			deflected = false;
			RaycastHit hit;
			if (!Physics.SphereCast(start, radius, direction, out hit, beamLength, mask, QueryTriggerInteraction.Ignore))
			{
				return;
			}
			PhysicBody hitPb = hit.GetPhysicBody();
			if (hitPb != null)
			{
				Creature hitCreature;
				RagdollPart part;
				if (hitPb == Player.local.locomotion.physicBody)
				{
					hitCreature = Player.currentCreature;
				}
				else if (hitPb.gameObject.TryGetComponent<RagdollPart>(out part))
				{
					hitCreature = part.ragdoll.creature;
				}
				else
				{
					hitPb.gameObject.TryGetComponent<Creature>(out hitCreature);
				}
				if (hitCreature != null)
				{
					if (Time.time >= hitCreature.lastDamageTime + this.damagePeriodTime)
					{
						hitCreature.Damage(this.damagePerSecond * this.damagePeriodTime);
						foreach (Golem.InflictedStatus status in this.appliedStatuses)
						{
							hitCreature.Inflict(status.data, this, status.duration, status.parameter, true);
						}
					}
					hitCreature.AddForce(this.golem.eyeTransform.forward * this.appliedForce, this.appliedForceMode, 1f, null);
					return;
				}
				Item item;
				if (hitPb.gameObject.TryGetComponent<Item>(out item))
				{
					if (item.breakable != null)
					{
						item.breakable.Break();
					}
					else if (tryDeflect && item.IsHeldByPlayer)
					{
						Vector3 normal = this.GetDeflectNormal(beamLength, item, hit.normal);
						Vector3 deflectDirection = Vector3.Reflect(direction, normal);
						this.lastDeflectDirection = ((Time.unscaledTime - this.lastDeflect > 0.5f) ? deflectDirection : Vector3.Slerp(this.lastDeflectDirection, deflectDirection, Time.unscaledDeltaTime * 5f));
						this.lastDeflect = Time.unscaledTime;
						deflected = true;
						this.deflectPoint.SetParent(hit.collider.transform);
						this.deflectPoint.SetPositionAndRotation(hit.point, Quaternion.LookRotation(this.lastDeflectDirection));
						if (this.deflectedBeamEffect == null)
						{
							this.deflectedBeamEffect = this.beamEffectData.Spawn(this.deflectPoint, null, true, null, false, 1f, 1f, new Type[]
							{
								typeof(EffectModuleAudio)
							});
							this.deflectedBeamEffect.Play(0, false, false);
						}
						bool flag;
						this.FireBeam(hit.point, radius, this.lastDeflectDirection, beamLength, mask, false, out flag);
					}
				}
				this.deflectPoint.SetParent(null);
				SimpleBreakable breakable = hitPb.rigidBody.GetComponentInParent<SimpleBreakable>();
				if (breakable != null)
				{
					breakable.Break();
					return;
				}
			}
			else if (this.leaveMoltenTrail)
			{
				this.TrySpawnTrail(hit, this.golem.eyeTransform.forward);
			}
		}

		// Token: 0x06001B83 RID: 7043 RVA: 0x000B6030 File Offset: 0x000B4230
		public virtual void BeamCharge()
		{
			this.chargeEffect = this.chargeEffectData.Spawn(this.golem.eyeTransform, true, null, false);
			this.chargeEffect.Play(0, false, false);
		}

		// Token: 0x06001B84 RID: 7044 RVA: 0x000B6060 File Offset: 0x000B4260
		public virtual void BeamStartLoop()
		{
			if (this.stopChargeOnShoot && this.chargeEffect != null)
			{
				this.chargeEffect.End(false, -1f);
				this.chargeEffect = null;
			}
			EffectInstance effectInstance = this.beamEffect;
			if (effectInstance != null)
			{
				effectInstance.End(false, -1f);
			}
			this.beamEffect = this.beamEffectData.Spawn(this.golem.eyeTransform, true, null, false);
			this.beamEffect.Play(0, false, false);
			if (this.beamEffect != null)
			{
				foreach (EffectParticle effectParticle in this.beamEffect.effects.OfType<EffectParticle>())
				{
					effectParticle.rootParticleSystem.collision.collidesWith = this.beamRaycastMask;
					foreach (EffectParticleChild effectParticleChild in effectParticle.childs)
					{
						effectParticleChild.particleSystem.collision.collidesWith = this.beamRaycastMask;
					}
				}
			}
		}

		// Token: 0x06001B85 RID: 7045 RVA: 0x000B6190 File Offset: 0x000B4390
		public override void OnEnd()
		{
			base.OnEnd();
			GolemBeam.lastBeamTime = Time.time;
			this.state = GolemBeam.State.Finished;
			if (this.golem.isDeployed)
			{
				this.golem.StopDeploy();
			}
			this.golem.headLookSpeedMultiplier = 1f;
			this.golem.weakpoints.Remove(this.golem.headCrystalBody.transform);
			if (this.chargeEffect != null)
			{
				this.chargeEffect.End(false, -1f);
				this.chargeEffect = null;
			}
			if (this.beamEffect != null)
			{
				this.beamEffect.End(false, -1f);
				this.beamEffect = null;
			}
			if (this.deflectedBeamEffect != null)
			{
				this.deflectedBeamEffect.End(false, -1f);
				this.deflectedBeamEffect = null;
			}
		}

		// Token: 0x06001B86 RID: 7046 RVA: 0x000B6260 File Offset: 0x000B4460
		private Vector3 GetDeflectNormal(float beamLength, Item item, Vector3 defaultNormal)
		{
			List<RaycastHit> hits = new List<RaycastHit>();
			for (int i = 0; i < 3; i++)
			{
				RaycastHit hit;
				if (this.Raycast(Quaternion.AngleAxis(120f * (float)i, Vector3.forward) * Vector3.up * 0.01f, beamLength, out hit))
				{
					Rigidbody rigidbody = hit.rigidbody;
					UnityEngine.Object x;
					if (rigidbody == null)
					{
						x = null;
					}
					else
					{
						CollisionHandler component = rigidbody.GetComponent<CollisionHandler>();
						x = ((component != null) ? component.item : null);
					}
					if (x == item)
					{
						hits.Add(hit);
					}
				}
			}
			int count = hits.Count;
			if (count == 0)
			{
				return defaultNormal;
			}
			if (count == 3)
			{
				Plane plane = new Plane(hits[0].point, hits[1].point, hits[2].point);
				if (Vector3.Dot(plane.normal, this.golem.eyeTransform.forward) > 0f)
				{
					plane.Flip();
				}
				return plane.normal;
			}
			Vector3 average = defaultNormal;
			for (int j = 0; j < hits.Count; j++)
			{
				average += hits[j].normal;
			}
			return average / (float)(hits.Count + 1);
		}

		// Token: 0x06001B87 RID: 7047 RVA: 0x000B639C File Offset: 0x000B459C
		private bool Raycast(Vector3 offset, float distance, out RaycastHit hit)
		{
			return Physics.Raycast(this.golem.eyeTransform.position + this.golem.eyeTransform.TransformPoint(offset), this.golem.eyeTransform.forward, out hit, distance, this.beamRaycastMask, QueryTriggerInteraction.Ignore);
		}

		// Token: 0x06001B88 RID: 7048 RVA: 0x000B63F4 File Offset: 0x000B45F4
		public void TrySpawnTrail(RaycastHit hit, Vector3 direction)
		{
			if (hit.rigidbody != null)
			{
				return;
			}
			float timeSinceLastSpawn = Time.time - this.lastTrailSpawn;
			float sqrDistance = (this.lastFlameWall != null) ? (this.lastFlameWall.transform.position - hit.point).sqrMagnitude : float.PositiveInfinity;
			if (this.lastFlameWall != null && timeSinceLastSpawn < this.moltenBeamSpell.trailMaxDelay && (timeSinceLastSpawn < this.moltenBeamSpell.trailMinDelay || sqrDistance < this.moltenBeamSpell.trailMinDistance * this.moltenBeamSpell.trailMinDistance) && sqrDistance < this.moltenBeamSpell.trailMaxDistance * this.moltenBeamSpell.trailMaxDistance)
			{
				return;
			}
			this.lastFlameWall = this.moltenBeamSpell.SpawnTrail(hit.point, Quaternion.LookRotation(Vector3.Cross(hit.normal, UnityEngine.Random.onUnitSphere), hit.normal));
			this.lastFlameWall.ignorePlayer = false;
			this.lastFlameWall.statusData = Catalog.GetData<StatusData>(this.appliedStatuses[0].data, true);
			this.lastFlameWall.statusDuration = this.appliedStatuses[0].duration;
			this.lastFlameWall.heatPerSecond = this.appliedStatuses[0].parameter;
			this.lastTrailSpawn = Time.time;
		}

		// Token: 0x04001A1E RID: 6686
		public static float lastBeamTime;

		// Token: 0x04001A1F RID: 6687
		[Header("Beam")]
		public float hitRange = 30f;

		// Token: 0x04001A20 RID: 6688
		public float radius = 1f;

		// Token: 0x04001A21 RID: 6689
		public float headSweepRange = 5f;

		// Token: 0x04001A22 RID: 6690
		public bool lockSweep;

		// Token: 0x04001A23 RID: 6691
		public LayerMask beamRaycastMask;

		// Token: 0x04001A24 RID: 6692
		public float beamMaxDistance = 50f;

		// Token: 0x04001A25 RID: 6693
		public float beamStartMaxAngle = 30f;

		// Token: 0x04001A26 RID: 6694
		public float beamAngleSoftMax = 60f;

		// Token: 0x04001A27 RID: 6695
		public float beamAngleHardMax = 80f;

		// Token: 0x04001A28 RID: 6696
		[Header("Effects")]
		public string chargeEffectID = "";

		// Token: 0x04001A29 RID: 6697
		public string beamEffectID = "";

		// Token: 0x04001A2A RID: 6698
		public bool stopChargeOnShoot;

		// Token: 0x04001A2B RID: 6699
		[Header("Impact")]
		public float damagePerSecond = 5f;

		// Token: 0x04001A2C RID: 6700
		public float damagePeriodTime = 0.1f;

		// Token: 0x04001A2D RID: 6701
		public bool blockable = true;

		// Token: 0x04001A2E RID: 6702
		public bool deflectOnBlock = true;

		// Token: 0x04001A2F RID: 6703
		public float appliedForce;

		// Token: 0x04001A30 RID: 6704
		public float blockedPushForce;

		// Token: 0x04001A31 RID: 6705
		public ForceMode appliedForceMode;

		// Token: 0x04001A32 RID: 6706
		public List<Golem.InflictedStatus> appliedStatuses = new List<Golem.InflictedStatus>();

		// Token: 0x04001A33 RID: 6707
		[Header("Timing")]
		public float beamCooldownDuration = 20f;

		// Token: 0x04001A34 RID: 6708
		public float beamStopDelayTargetLost = 3f;

		// Token: 0x04001A35 RID: 6709
		public Vector2 beamingMinMaxDuration = new Vector2(5f, 10f);

		// Token: 0x04001A36 RID: 6710
		[Header("Trail")]
		public bool leaveMoltenTrail;

		// Token: 0x04001A37 RID: 6711
		public string moltenBeamSpellId = "PlasmaBeam";

		// Token: 0x04001A38 RID: 6712
		[NonSerialized]
		public GolemBeam.State state;

		// Token: 0x04001A39 RID: 6713
		[NonSerialized]
		public EffectData chargeEffectData;

		// Token: 0x04001A3A RID: 6714
		[NonSerialized]
		public EffectData beamEffectData;

		// Token: 0x04001A3B RID: 6715
		[NonSerialized]
		public SpellMergeMoltenBeam moltenBeamSpell;

		// Token: 0x04001A3C RID: 6716
		[NonSerialized]
		public bool sweepSide;

		// Token: 0x04001A3D RID: 6717
		protected float headSweepRangeMultiplier = 1f;

		// Token: 0x04001A3E RID: 6718
		protected EffectInstance chargeEffect;

		// Token: 0x04001A3F RID: 6719
		protected EffectInstance beamEffect;

		// Token: 0x04001A40 RID: 6720
		protected EffectInstance deflectedBeamEffect;

		// Token: 0x04001A41 RID: 6721
		protected Transform deflectPoint;

		// Token: 0x04001A42 RID: 6722
		protected float lastTrailSpawn;

		// Token: 0x04001A43 RID: 6723
		protected FlameWall lastFlameWall;

		// Token: 0x04001A44 RID: 6724
		protected float beamTargetLostDuration;

		// Token: 0x04001A45 RID: 6725
		protected LookMode sweepMode;

		// Token: 0x04001A46 RID: 6726
		private float lastDeflect;

		// Token: 0x04001A47 RID: 6727
		private Vector3 lastDeflectDirection;

		// Token: 0x020008CF RID: 2255
		public enum State
		{
			// Token: 0x040042BD RID: 17085
			Deploying,
			// Token: 0x040042BE RID: 17086
			Firing,
			// Token: 0x040042BF RID: 17087
			Finished
		}
	}
}

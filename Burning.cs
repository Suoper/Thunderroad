using System;
using System.Collections.Generic;
using ThunderRoad.Skill;
using ThunderRoad.Skill.Spell;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000236 RID: 566
	public class Burning : Status
	{
		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060017D9 RID: 6105 RVA: 0x0009EF9C File Offset: 0x0009D19C
		public float CharAmount
		{
			get
			{
				Creature creature = this.entity as Creature;
				if (creature == null || !creature.isPlayer)
				{
					return Mathf.Clamp01(Mathf.Max(this.isIgnited ? ((Time.time - this.igniteTime) / this.data.fullCharTime) : 0f, this.entity.GetVariable<float>("CharAmount")));
				}
				return 0f;
			}
		}

		// Token: 0x060017DA RID: 6106 RVA: 0x0009F008 File Offset: 0x0009D208
		public static FloatHandler GetHeatGainHandler(ThunderEntity entity)
		{
			FloatHandler handler;
			if (!entity.TryGetVariable<FloatHandler>("HeatGainMult", out handler))
			{
				return entity.SetVariable<FloatHandler>("HeatGainMult", new FloatHandler());
			}
			return handler;
		}

		// Token: 0x060017DB RID: 6107 RVA: 0x0009F038 File Offset: 0x0009D238
		public static FloatHandler GetHeatLossHandler(ThunderEntity entity)
		{
			FloatHandler handler;
			if (!entity.TryGetVariable<FloatHandler>("HeatLossMult", out handler))
			{
				return entity.SetVariable<FloatHandler>("HeatLossMult", new FloatHandler());
			}
			return handler;
		}

		// Token: 0x060017DC RID: 6108 RVA: 0x0009F066 File Offset: 0x0009D266
		public override void Spawn(StatusData data, ThunderEntity entity)
		{
			base.Spawn(data, entity);
			this.data = (data as StatusDataBurning);
			this.heatGainMult = Burning.GetHeatGainHandler(entity);
			this.heatLossMult = Burning.GetHeatLossHandler(entity);
		}

		// Token: 0x060017DD RID: 6109 RVA: 0x0009F094 File Offset: 0x0009D294
		public override bool AddHandler(object handler, float duration = float.PositiveInfinity, object parameter = null, bool playEffect = true)
		{
			WaterHandler waterHandler = this.entity.waterHandler;
			if (waterHandler != null && waterHandler.inWater)
			{
				return false;
			}
			if (parameter is float)
			{
				float heat = (float)parameter;
				this.AddHeat(heat, true);
				duration = float.PositiveInfinity;
				return base.AddHandler(handler, duration, parameter, playEffect);
			}
			return false;
		}

		// Token: 0x060017DE RID: 6110 RVA: 0x0009F0E9 File Offset: 0x0009D2E9
		protected override object GetValue()
		{
			return this.entity.GetVariable<float>("Heat");
		}

		// Token: 0x060017DF RID: 6111 RVA: 0x0009F100 File Offset: 0x0009D300
		public override void FirstApply()
		{
			base.FirstApply();
			Creature creature = this.entity as Creature;
			if (creature != null)
			{
				this.data.SpawnEffects(creature, this.data.smokingMainEffectData, this.data.smokingLimbEffectData, out this.smokeEffects);
				for (int i = 0; i < this.smokeEffects.Count; i++)
				{
					EffectInstance effectInstance = this.smokeEffects[i];
					if (effectInstance != null)
					{
						effectInstance.SetIntensity(this.Intensity);
					}
					if (effectInstance != null)
					{
						effectInstance.Play(0, false, false);
					}
				}
				creature.ragdoll.OnStateChange -= this.OnStateChange;
				creature.ragdoll.OnStateChange += this.OnStateChange;
			}
			EffectInstance effectInstance2 = this.effectInstance;
			if (effectInstance2 != null)
			{
				effectInstance2.SetIntensity(this.Intensity);
			}
			this.lastTick = Time.time + UnityEngine.Random.Range(-this.data.burnDelay, this.data.burnDelay);
		}

		// Token: 0x060017E0 RID: 6112 RVA: 0x0009F200 File Offset: 0x0009D400
		private void OnStateChange(Ragdoll.State previousState, Ragdoll.State newState, Ragdoll.PhysicStateChange physicsChange, EventTime time)
		{
			try
			{
				Creature creature = this.entity as Creature;
				if (creature != null && physicsChange != Ragdoll.PhysicStateChange.None && newState != Ragdoll.State.Disabled)
				{
					bool isStart = time == EventTime.OnStart;
					if (this.smokeEffects != null)
					{
						for (int i = 0; i < this.smokeEffects.Count; i++)
						{
							this.ReparentEffect(this.smokeEffects[i], creature, isStart);
						}
					}
					if (this.flameEffects != null)
					{
						for (int j = 0; j < this.flameEffects.Count; j++)
						{
							this.ReparentEffect(this.flameEffects[j], creature, isStart);
						}
					}
				}
			}
			catch (NullReferenceException exception)
			{
				Debug.LogWarning("Warning: Status Effect threw exception during creature state change.");
				Debug.LogException(exception);
			}
		}

		// Token: 0x060017E1 RID: 6113 RVA: 0x0009F2B4 File Offset: 0x0009D4B4
		public void ReparentEffect(EffectInstance effect, Creature creature, bool isStart)
		{
			if (creature.GetRendererForVFX() && effect != null)
			{
				effect.SetRenderer(isStart ? null : creature.GetRendererForVFX(), false);
			}
			if (effect != null)
			{
				effect.SetParent(isStart ? null : creature.ragdoll.rootPart.meshBone.transform, false);
			}
		}

		// Token: 0x060017E2 RID: 6114 RVA: 0x0009F30C File Offset: 0x0009D50C
		public override void FullRemove()
		{
			base.FullRemove();
			if (this.smokeEffects != null)
			{
				for (int i = 0; i < this.smokeEffects.Count; i++)
				{
					EffectInstance effectInstance = this.smokeEffects[i];
					if (effectInstance != null)
					{
						effectInstance.End(false, -1f);
					}
				}
				this.smokeEffects = null;
			}
			if (this.flameEffects != null)
			{
				for (int j = 0; j < this.flameEffects.Count; j++)
				{
					EffectInstance effectInstance2 = this.flameEffects[j];
					if (effectInstance2 != null)
					{
						effectInstance2.SetParent(null, false);
					}
					if (effectInstance2 != null)
					{
						effectInstance2.End(false, -1f);
					}
				}
				this.flameEffects.Clear();
			}
			Creature creature = this.entity as Creature;
			if (creature != null)
			{
				creature.ragdoll.OnStateChange -= this.OnStateChange;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060017E3 RID: 6115 RVA: 0x0009F3DC File Offset: 0x0009D5DC
		// (set) Token: 0x060017E4 RID: 6116 RVA: 0x0009F408 File Offset: 0x0009D608
		public float Heat
		{
			get
			{
				object value = this.value;
				if (value is float)
				{
					return (float)value;
				}
				return 0f;
			}
			set
			{
				this.entity.SetVariable<float>("Heat", Mathf.Clamp(value, 0f, this.MaxHeat));
				this.OnValueChange();
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060017E5 RID: 6117 RVA: 0x0009F432 File Offset: 0x0009D632
		public float MaxHeat
		{
			get
			{
				return this.data.maxHeat;
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060017E6 RID: 6118 RVA: 0x0009F43F File Offset: 0x0009D63F
		public float Intensity
		{
			get
			{
				return this.Heat / this.MaxHeat;
			}
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060017E7 RID: 6119 RVA: 0x0009F44E File Offset: 0x0009D64E
		public bool ShouldIgnite
		{
			get
			{
				return this.Heat >= this.MaxHeat;
			}
		}

		// Token: 0x060017E8 RID: 6120 RVA: 0x0009F464 File Offset: 0x0009D664
		public void AddHeat(float heat, bool onValueChange = true)
		{
			if (this.isIgnited)
			{
				Creature creature = this.entity as Creature;
				if (creature != null && !creature.isPlayer && !creature.isKilled)
				{
					return;
				}
			}
			float heatToAdd = (heat > 0f) ? (heat * this.heatGainMult) : (heat * this.heatLossMult);
			this.entity.SetVariable<float>("Heat", (float current) => Mathf.Clamp(current + heatToAdd, 0f, this.MaxHeat));
			if (onValueChange)
			{
				this.OnValueChange();
			}
		}

		// Token: 0x060017E9 RID: 6121 RVA: 0x0009F4F6 File Offset: 0x0009D6F6
		public override void Transfer(ThunderEntity other)
		{
			other.Inflict(this.data, this, float.PositiveInfinity, this.Heat, true);
		}

		// Token: 0x060017EA RID: 6122 RVA: 0x0009F518 File Offset: 0x0009D718
		public override void Update()
		{
			base.Update();
			Creature creature = this.entity as Creature;
			if (creature != null && this.CharAmount == 1f && creature.isKilled && !this.isFrozen)
			{
				this.FullyChar(creature);
			}
			if (!this.isIgnited && this.ShouldIgnite)
			{
				this.Ignite();
				this.isIgnited = true;
			}
			if (this.isIgnited && this.CharAmount < 1f)
			{
				this.entity.SetVariable<float>("CharAmount", new Func<float, float>(this.CharEntity));
				this.heatGainMult.Add("CharAmount", this.data.charResistanceCurve.Evaluate(this.CharAmount));
			}
			Creature creature2 = this.entity as Creature;
			float num;
			if (creature2 != null)
			{
				if (creature2.isPlayer)
				{
					num = this.data.heatReductionPerSecondPlayer;
					goto IL_F8;
				}
				if (creature2.isKilled)
				{
					num = this.data.heatReductionPerSecondKilled;
					goto IL_F8;
				}
			}
			num = this.data.heatReductionPerSecond;
			IL_F8:
			float heatReduction = num;
			this.AddHeat(-heatReduction * Time.deltaTime, true);
			WaterHandler waterHandler = this.entity.waterHandler;
			if (waterHandler != null && waterHandler.inWater)
			{
				this.Heat = 0f;
			}
			if (this.Heat == 0f && Time.time - this.startTime > 1f)
			{
				base.ClearHandlers();
				return;
			}
			EffectInstance effectInstance = this.effectInstance;
			if (effectInstance != null)
			{
				effectInstance.SetIntensity(this.Intensity);
			}
			if (!this.isIgnited && this.smokeEffects != null)
			{
				for (int i = 0; i < this.smokeEffects.Count; i++)
				{
					EffectInstance effectInstance2 = this.smokeEffects[i];
					if (effectInstance2 != null)
					{
						effectInstance2.SetIntensity(this.Intensity);
					}
				}
			}
			if (!this.isIgnited)
			{
				return;
			}
			if (Burning.spread != null)
			{
				this.Spread();
			}
			if (Time.time - this.lastTick < this.data.burnDelay)
			{
				return;
			}
			this.lastTick = Time.time;
			this.Burn((float)this.handlers.Count);
		}

		// Token: 0x060017EB RID: 6123 RVA: 0x0009F725 File Offset: 0x0009D925
		private float CharEntity(float current)
		{
			return current + Time.deltaTime * 1f / this.data.fullCharTime;
		}

		// Token: 0x060017EC RID: 6124 RVA: 0x0009F740 File Offset: 0x0009D940
		public void FullyChar(Creature creature)
		{
			this.isFrozen = true;
			if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration, BuildSettings.ContentFlagBehaviour.Discard))
			{
				return;
			}
			creature.ragdoll.SetState(Ragdoll.State.Destabilized, true);
			creature.RunAfter(delegate()
			{
				if (this.data.freezeOnFullChar)
				{
					creature.ragdoll.SetState(Ragdoll.State.Frozen);
					if (this.data.weakenJointsOnFullChar && GameManager.CheckContentActive(BuildSettings.ContentFlag.Dismemberment, BuildSettings.ContentFlagBehaviour.Discard))
					{
						creature.ragdoll.EnableCharJointBreakForce(this.data.fullCharCharJointMultiplier);
						creature.ragdoll.OnGrabEvent += this.Grabbed;
						creature.ragdoll.OnTelekinesisGrabEvent += this.TKGrabbed;
						creature.ragdoll.OnUngrabEvent += this.Released;
						creature.ragdoll.OnTelekinesisReleaseEvent += this.TKReleased;
						creature.OnDespawnEvent += base.<FullyChar>g__CreatureDespawn|1;
					}
				}
			}, 0.5f, false);
		}

		// Token: 0x060017ED RID: 6125 RVA: 0x0009F7A2 File Offset: 0x0009D9A2
		private void Grabbed(RagdollHand ragdollHand, HandleRagdoll handleRagdoll)
		{
			handleRagdoll.ragdollPart.ragdoll.EnableCharJointBreakForce(this.data.fullCharCharJointMultiplier);
		}

		// Token: 0x060017EE RID: 6126 RVA: 0x0009F7BF File Offset: 0x0009D9BF
		private void TKGrabbed(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll)
		{
			handleRagdoll.ragdollPart.ragdoll.EnableCharJointBreakForce(this.data.fullCharCharJointMultiplier);
		}

		// Token: 0x060017EF RID: 6127 RVA: 0x0009F7DC File Offset: 0x0009D9DC
		private void TKReleased(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll, bool lastHandler)
		{
			handleRagdoll.ragdollPart.ragdoll.EnableCharJointBreakForce(this.data.fullCharCharJointMultiplier);
		}

		// Token: 0x060017F0 RID: 6128 RVA: 0x0009F7F9 File Offset: 0x0009D9F9
		private void Released(RagdollHand ragdollHand, HandleRagdoll handleRagdoll, bool lastHandler)
		{
			handleRagdoll.ragdollPart.ragdoll.EnableCharJointBreakForce(this.data.fullCharCharJointMultiplier);
		}

		// Token: 0x060017F1 RID: 6129 RVA: 0x0009F818 File Offset: 0x0009DA18
		public virtual void Burn(float multiplier)
		{
			Creature creature = this.entity as Creature;
			if (creature != null && !creature.isKilled)
			{
				creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Fire, (creature.isPlayer ? this.data.damagePerTickPlayer : this.data.damagePerTick) * multiplier)
				{
					isStatus = true
				}, null, null)
				{
					skipVignette = true
				});
			}
		}

		// Token: 0x060017F2 RID: 6130 RVA: 0x0009F884 File Offset: 0x0009DA84
		public virtual bool CanSpread(ThunderEntity eachEntity)
		{
			Creature creature = eachEntity as Creature;
			return creature != null && !creature.isPlayer && creature != this.entity;
		}

		// Token: 0x060017F3 RID: 6131 RVA: 0x0009F8B4 File Offset: 0x0009DAB4
		public virtual void Spread()
		{
			foreach (Creature creature in Creature.InRadiusNaive(this.entity.Center, Burning.spread.radius, new Func<Creature, bool>(this.CanSpread), null))
			{
				creature.Inflict(this.data, this, float.PositiveInfinity, this.Heat * (creature.isKilled ? Burning.spread.heatTransferToDeadEnemy : Burning.spread.heatTransfer) * Time.deltaTime * Burning.spread.rangeCurve.Evaluate(1f - (this.entity.Center - creature.Center).magnitude / Burning.spread.radius), true);
			}
		}

		// Token: 0x060017F4 RID: 6132 RVA: 0x0009F9AC File Offset: 0x0009DBAC
		public virtual void Ignite()
		{
			Burning.<>c__DisplayClass43_0 CS$<>8__locals1 = new Burning.<>c__DisplayClass43_0();
			CS$<>8__locals1.<>4__this = this;
			ThunderEntity entity = this.entity;
			CS$<>8__locals1.creature = (entity as Creature);
			if (CS$<>8__locals1.creature == null)
			{
				return;
			}
			EventManager.InvokeIgnite(CS$<>8__locals1.creature);
			if (!CS$<>8__locals1.creature.isPlayer)
			{
				if (SkillDiscombobulate.CreatureStunned(CS$<>8__locals1.creature))
				{
					SkillDiscombobulate.BrainToggle(CS$<>8__locals1.creature, true, false);
				}
				CS$<>8__locals1.creature.brain.Stop();
				CS$<>8__locals1.creature.OnDamageEvent += CS$<>8__locals1.<Ignite>g__OnDamage|3;
				CS$<>8__locals1.creature.OnKillEvent += CS$<>8__locals1.<Ignite>g__OnKill|2;
				BrainModuleFear fear = CS$<>8__locals1.creature.brain.instance.GetModule<BrainModuleFear>(true);
				if (fear != null && fear.isCowering)
				{
					fear.StopPanic();
				}
				bool fireDeathAnim = false;
				if (GameManager.CheckContentActive(BuildSettings.ContentFlag.Fright, BuildSettings.ContentFlagBehaviour.Discard))
				{
					if (!CS$<>8__locals1.creature.isKilled)
					{
						BrainModuleSpeak speak = CS$<>8__locals1.creature.brain.instance.GetModule<BrainModuleSpeak>(false);
						if (speak != null)
						{
							speak.StopSpeak(true);
							speak.Play(BrainModuleSpeak.hashFalling, true, true, -1);
						}
					}
					fireDeathAnim = CS$<>8__locals1.creature.canPlayDynamicAnimation;
				}
				Ragdoll.State state = CS$<>8__locals1.creature.ragdoll.state;
				if (state == Ragdoll.State.Standing || state == Ragdoll.State.NoPhysic)
				{
					if (fireDeathAnim)
					{
						CS$<>8__locals1.creature.ragdoll.forcePhysic.Add(this);
						CS$<>8__locals1.creature.RunAfter(delegate()
						{
							CS$<>8__locals1.creature.StopAnimation(true);
							AnimationData.Clip[] picked;
							CS$<>8__locals1.creature.PlayAnimation(CS$<>8__locals1.<>4__this.data.heatAnimationData, out picked, null);
							MonoBehaviour creature2 = CS$<>8__locals1.creature;
							Action action;
							if ((action = CS$<>8__locals1.<>9__4) == null)
							{
								action = (CS$<>8__locals1.<>9__4 = delegate()
								{
									CS$<>8__locals1.creature.SetAnimatorHeightRatio(0.2f);
									CS$<>8__locals1.creature.animator.SetFloat(Burning.Injured, 1f);
								});
							}
							creature2.RunAfter(action, picked[0].animationClip.length * picked[0].animationSpeed * 0.35f, false);
							MonoBehaviour creature3 = CS$<>8__locals1.creature;
							Action action2;
							if ((action2 = CS$<>8__locals1.<>9__5) == null)
							{
								action2 = (CS$<>8__locals1.<>9__5 = delegate()
								{
									CS$<>8__locals1.creature.Kill(new CollisionInstance(new DamageStruct(DamageType.Fire, 1000f), null, null));
								});
							}
							creature3.RunAfter(action2, picked[0].animationClip.length * picked[0].animationSpeed * 0.85f, false);
						}, UnityEngine.Random.Range(0f, this.data.animationStartTimeMax), false);
						CS$<>8__locals1.creature.OnDespawnEvent += CS$<>8__locals1.<Ignite>g__ResetLaying|1;
					}
					else
					{
						CS$<>8__locals1.creature.ragdoll.SetState(Ragdoll.State.Destabilized);
					}
				}
				if (this.data.allowVisualCharring && GameManager.CheckContentActive(BuildSettings.ContentFlag.Burns, BuildSettings.ContentFlagBehaviour.Discard))
				{
					CS$<>8__locals1.creature.GetOrAddComponent<CharBehaviour>().Init(this.data);
				}
			}
			this.igniteTime = Time.time;
			if (this.effectInstance != null)
			{
				EffectInstance smokeEffect = this.effectInstance;
				this.effectInstance.onEffectFinished += delegate(EffectInstance _)
				{
					smokeEffect.Despawn();
				};
				this.effectInstance.End(false, -1f);
			}
			if (this.smokeEffects != null)
			{
				for (int i = 0; i < this.smokeEffects.Count; i++)
				{
					EffectInstance effectInstance = this.smokeEffects[i];
					if (effectInstance != null)
					{
						effectInstance.End(false, -1f);
					}
				}
				this.smokeEffects = null;
			}
			EffectData igniteEffectData = this.data.igniteEffectData;
			Creature creature = this.entity as Creature;
			EffectInstance effectInstance2 = igniteEffectData.Spawn(((creature != null) ? creature.ragdoll.targetPart.transform : null) ?? this.entity.RootTransform, true, null, false);
			effectInstance2.SetSource(this.entity.RootTransform);
			effectInstance2.SetTarget(this.entity.RootTransform);
			effectInstance2.Play(0, false, false);
			if (!this.data.SpawnEffects(this.entity, this.data.burningMainEffectData, this.data.burningLimbEffectData, out this.flameEffects))
			{
				return;
			}
			for (int j = 0; j < this.flameEffects.Count; j++)
			{
				EffectInstance effectInstance3 = this.flameEffects[j];
				if (effectInstance3 != null)
				{
					effectInstance3.Play(0, false, false);
				}
			}
		}

		// Token: 0x04001725 RID: 5925
		protected new StatusDataBurning data;

		// Token: 0x04001726 RID: 5926
		public static SkillConflagration spread;

		// Token: 0x04001727 RID: 5927
		public List<EffectInstance> smokeEffects;

		// Token: 0x04001728 RID: 5928
		public List<EffectInstance> flameEffects;

		// Token: 0x04001729 RID: 5929
		public FloatHandler heatGainMult;

		// Token: 0x0400172A RID: 5930
		public FloatHandler heatLossMult;

		// Token: 0x0400172B RID: 5931
		protected float igniteTime;

		// Token: 0x0400172C RID: 5932
		protected float lastTick;

		// Token: 0x0400172D RID: 5933
		public bool isIgnited;

		// Token: 0x0400172E RID: 5934
		public bool isFrozen;

		// Token: 0x0400172F RID: 5935
		private static readonly int Injured = Animator.StringToHash("Injured");
	}
}

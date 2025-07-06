using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200022C RID: 556
	public class MeleeStanceData : StanceIdlesData<MeleeIdle>
	{
		// Token: 0x0600177C RID: 6012 RVA: 0x0009D837 File Offset: 0x0009BA37
		protected void SetOpenNoMovement()
		{
			this.SetNewestNoMovement(this.openingAttacks, ref this.openersLastCount);
		}

		// Token: 0x0600177D RID: 6013 RVA: 0x0009D84B File Offset: 0x0009BA4B
		protected void SetChainNoMovement()
		{
			this.SetNewestNoMovement(this.chainAttacks, ref this.chainsLastCount);
		}

		// Token: 0x0600177E RID: 6014 RVA: 0x0009D85F File Offset: 0x0009BA5F
		protected void SetRiposteNoMovement()
		{
			this.SetNewestNoMovement(this.ripostes, ref this.ripostesLastCount);
		}

		// Token: 0x0600177F RID: 6015 RVA: 0x0009D873 File Offset: 0x0009BA73
		protected void SetNewestNoMovement(List<AttackMotion> list, ref int lastCount)
		{
			if (lastCount == 0 && list.Count > 1)
			{
				lastCount = list.Count;
			}
			if (lastCount < list.Count)
			{
				list[list.Count - 1].allowPlayAndMove = false;
			}
			lastCount = list.Count;
		}

		// Token: 0x06001780 RID: 6016 RVA: 0x0009D8B0 File Offset: 0x0009BAB0
		public void AutoConfigAllOpeningAttacks(Creature creature)
		{
			MeleeStanceData.<>c__DisplayClass21_0 CS$<>8__locals1 = new MeleeStanceData.<>c__DisplayClass21_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.creature = creature;
			CS$<>8__locals1.<AutoConfigAllOpeningAttacks>g__RecursivePlay|0(0);
		}

		// Token: 0x06001781 RID: 6017 RVA: 0x0009D8CB File Offset: 0x0009BACB
		private IEnumerator TrackHandAndRoot(Creature creature, AttackMotion attackMotion)
		{
			MeleeStanceData.<>c__DisplayClass22_0 CS$<>8__locals1;
			CS$<>8__locals1.creature = creature;
			CS$<>8__locals1.startPosition = CS$<>8__locals1.creature.transform.position;
			CS$<>8__locals1.forwardStart = CS$<>8__locals1.creature.transform.forward;
			float rightHandMaxDistance = MeleeStanceData.<TrackHandAndRoot>g__GetHandRange|22_1(Side.Right, ref CS$<>8__locals1);
			float leftHandMaxDistance = MeleeStanceData.<TrackHandAndRoot>g__GetHandRange|22_1(Side.Left, ref CS$<>8__locals1);
			if (attackMotion.sweep)
			{
				if (attackMotion.attackSide == Interactable.HandSide.Both)
				{
					attackMotion.minRange = Mathf.Max(rightHandMaxDistance, leftHandMaxDistance);
				}
				else if (attackMotion.attackSide == Interactable.HandSide.Right)
				{
					attackMotion.minRange = rightHandMaxDistance;
				}
				else
				{
					attackMotion.minRange = leftHandMaxDistance;
				}
			}
			while (CS$<>8__locals1.creature.isPlayingDynamicAnimation)
			{
				rightHandMaxDistance = Mathf.Max(rightHandMaxDistance, MeleeStanceData.<TrackHandAndRoot>g__GetHandRange|22_1(Side.Right, ref CS$<>8__locals1));
				leftHandMaxDistance = Mathf.Max(leftHandMaxDistance, MeleeStanceData.<TrackHandAndRoot>g__GetHandRange|22_1(Side.Left, ref CS$<>8__locals1));
				yield return Yielders.EndOfFrame;
			}
			float previousRange = attackMotion.maxRange;
			if (attackMotion.attackSide == Interactable.HandSide.Both)
			{
				attackMotion.maxRange = Mathf.Max(rightHandMaxDistance, leftHandMaxDistance);
			}
			else if (attackMotion.attackSide == Interactable.HandSide.Right)
			{
				attackMotion.maxRange = rightHandMaxDistance;
			}
			else
			{
				attackMotion.maxRange = leftHandMaxDistance;
			}
			attackMotion.maxRange /= CS$<>8__locals1.creature.transform.lossyScale.y;
			if (!attackMotion.sweep)
			{
				attackMotion.minRange = attackMotion.maxRange;
			}
			Debug.Log(string.Format("Configured vs actual diff: {0}", attackMotion.maxRange - previousRange));
			yield break;
		}

		// Token: 0x06001782 RID: 6018 RVA: 0x0009D8E4 File Offset: 0x0009BAE4
		public void NormalizeAllAttackTimings()
		{
			foreach (StanceNode node in this.AllStanceNodes())
			{
				if (!(node.animationClip == null))
				{
					AttackMotion attackMotion = node as AttackMotion;
					if (attackMotion != null)
					{
						attackMotion.start /= attackMotion.animationClip.length;
						attackMotion.peak /= attackMotion.animationClip.length;
						attackMotion.end /= attackMotion.animationClip.length;
					}
				}
			}
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06001783 RID: 6019 RVA: 0x0009D98C File Offset: 0x0009BB8C
		[JsonIgnore]
		public Dictionary<string, GuardPose> guardPoseIDs
		{
			get
			{
				if (this._guardPoseIDs == null)
				{
					this._guardPoseIDs = new Dictionary<string, GuardPose>();
					this._guardPoseIDs.Add(this.upperLeftGuard.id, this.upperLeftGuard);
					this._guardPoseIDs.Add(this.upperRightGuard.id, this.upperRightGuard);
					this._guardPoseIDs.Add(this.midLeftGuard.id, this.midLeftGuard);
					this._guardPoseIDs.Add(this.neutralGuard.id, this.neutralGuard);
					this._guardPoseIDs.Add(this.midRightGuard.id, this.midRightGuard);
					this._guardPoseIDs.Add(this.lowerLeftGuard.id, this.lowerLeftGuard);
					this._guardPoseIDs.Add(this.lowerRightGuard.id, this.lowerRightGuard);
				}
				return this._guardPoseIDs;
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x06001784 RID: 6020 RVA: 0x0009DA7C File Offset: 0x0009BC7C
		[JsonIgnore]
		public Dictionary<int, GuardPose> AnimationClipToGuardPose
		{
			get
			{
				if (this._animationClipToGuardPose == null)
				{
					try
					{
						this._animationClipToGuardPose = new Dictionary<int, GuardPose>();
						this._animationClipToGuardPose.Add(this.upperLeftGuard.animationClip.GetInstanceID(), this.upperLeftGuard);
						this._animationClipToGuardPose.Add(this.upperRightGuard.animationClip.GetInstanceID(), this.upperRightGuard);
						this._animationClipToGuardPose.Add(this.midLeftGuard.animationClip.GetInstanceID(), this.midLeftGuard);
						this._animationClipToGuardPose.Add(this.neutralGuard.animationClip.GetInstanceID(), this.neutralGuard);
						this._animationClipToGuardPose.Add(this.midRightGuard.animationClip.GetInstanceID(), this.midRightGuard);
						this._animationClipToGuardPose.Add(this.lowerLeftGuard.animationClip.GetInstanceID(), this.lowerLeftGuard);
						this._animationClipToGuardPose.Add(this.lowerRightGuard.animationClip.GetInstanceID(), this.lowerRightGuard);
					}
					catch (Exception e)
					{
						Debug.LogError("Error in animationClipToGuardPose" + e.Message + e.StackTrace);
						this._animationClipToGuardPose = new Dictionary<int, GuardPose>();
					}
				}
				return this._animationClipToGuardPose;
			}
		}

		// Token: 0x06001785 RID: 6021 RVA: 0x0009DBC8 File Offset: 0x0009BDC8
		protected override StanceData CreateNew()
		{
			return new MeleeStanceData();
		}

		// Token: 0x06001786 RID: 6022 RVA: 0x0009DBCF File Offset: 0x0009BDCF
		public override IEnumerable<StanceNode> AllStanceNodes()
		{
			foreach (StanceNode node in base.AllStanceNodes())
			{
				yield return node;
			}
			IEnumerator<StanceNode> enumerator = null;
			int num;
			for (int i = 0; i < this.openingAttacks.Count; i = num + 1)
			{
				yield return this.openingAttacks[i];
				num = i;
			}
			for (int i = 0; i < this.chainAttacks.Count; i = num + 1)
			{
				yield return this.chainAttacks[i];
				num = i;
			}
			yield return this.upperLeftGuard;
			yield return this.upperRightGuard;
			yield return this.midLeftGuard;
			yield return this.neutralGuard;
			yield return this.midRightGuard;
			yield return this.lowerLeftGuard;
			yield return this.lowerRightGuard;
			for (int i = 0; i < this.ripostes.Count; i = num + 1)
			{
				yield return this.ripostes[i];
				num = i;
			}
			yield break;
			yield break;
		}

		// Token: 0x06001787 RID: 6023 RVA: 0x0009DBE0 File Offset: 0x0009BDE0
		protected override StanceData MakeFilteredClone(ItemModuleAI mainHand, ItemModuleAI offHand, int difficulty = 0)
		{
			MeleeStanceData newData = (MeleeStanceData)base.MakeFilteredClone(mainHand, offHand, difficulty);
			newData.alternateAttackSides = this.alternateAttackSides;
			newData.chainAttacksOnBody = this.chainAttacksOnBody;
			newData.defenseSettings = this.defenseSettings;
			newData.upperLeftGuard = this.upperLeftGuard.Copy<GuardPose>();
			newData.upperRightGuard = this.upperRightGuard.Copy<GuardPose>();
			newData.midLeftGuard = this.midLeftGuard.Copy<GuardPose>();
			newData.neutralGuard = this.neutralGuard.Copy<GuardPose>();
			newData.midRightGuard = this.midRightGuard.Copy<GuardPose>();
			newData.lowerLeftGuard = this.lowerLeftGuard.Copy<GuardPose>();
			newData.lowerRightGuard = this.lowerRightGuard.Copy<GuardPose>();
			newData.openingAttacks = new List<AttackMotion>();
			newData.chainAttacks = new List<AttackMotion>();
			newData.ripostes = new List<AttackMotion>();
			foreach (ValueTuple<int, AttackMotion> element in Utils.CombinedEnumerable<AttackMotion>(new IEnumerable<AttackMotion>[]
			{
				this.openingAttacks,
				this.chainAttacks,
				this.ripostes
			}))
			{
				AttackMotion attackMotion = element.Item2;
				if (!base.TooDifficult(difficulty, attackMotion.difficulty) && attackMotion.attackMotion != ItemModuleAI.AttackType.None)
				{
					bool rightHandGood = mainHand != null && mainHand.AttackTypeMatches(attackMotion.attackMotion);
					bool leftHandGood = offHand != null && offHand.AttackTypeMatches(attackMotion.attackMotion);
					if (rightHandGood || leftHandGood)
					{
						switch (attackMotion.attackSide)
						{
						case Interactable.HandSide.Both:
							if (!rightHandGood)
							{
								continue;
							}
							if (!leftHandGood)
							{
								continue;
							}
							break;
						case Interactable.HandSide.Right:
							if (!rightHandGood)
							{
								continue;
							}
							break;
						case Interactable.HandSide.Left:
							if (!leftHandGood)
							{
								continue;
							}
							break;
						}
						AttackMotion copy = attackMotion.Copy<AttackMotion>();
						switch (element.Item1)
						{
						case 0:
							newData.openingAttacks.Add(copy);
							break;
						case 1:
							newData.chainAttacks.Add(copy);
							break;
						case 2:
							newData.ripostes.Add(copy);
							break;
						}
					}
				}
			}
			return newData;
		}

		// Token: 0x06001789 RID: 6025 RVA: 0x0009DEC0 File Offset: 0x0009C0C0
		[CompilerGenerated]
		internal static float <TrackHandAndRoot>g__GetDirectionDistance|22_0(Vector3 position, Vector3 direction, ref MeleeStanceData.<>c__DisplayClass22_0 A_2)
		{
			Vector3 offset = position.ToXZ() - A_2.startPosition.ToXZ();
			return Vector3.Dot(direction, Vector3.Project(offset, direction));
		}

		// Token: 0x0600178A RID: 6026 RVA: 0x0009DEF1 File Offset: 0x0009C0F1
		[CompilerGenerated]
		internal static float <TrackHandAndRoot>g__GetHandRange|22_1(Side side, ref MeleeStanceData.<>c__DisplayClass22_0 A_1)
		{
			return MeleeStanceData.<TrackHandAndRoot>g__GetDirectionDistance|22_0(A_1.creature.GetHand(side).transform.position, A_1.forwardStart, ref A_1);
		}

		// Token: 0x040016E9 RID: 5865
		public bool alternateAttackSides;

		// Token: 0x040016EA RID: 5866
		public MeleeStanceData.BodyChainBehaviour chainAttacksOnBody;

		// Token: 0x040016EB RID: 5867
		public List<AttackMotion> openingAttacks = new List<AttackMotion>();

		// Token: 0x040016EC RID: 5868
		public List<AttackMotion> chainAttacks = new List<AttackMotion>();

		// Token: 0x040016ED RID: 5869
		public BrainModuleDefense.StanceDefenseSettings defenseSettings;

		// Token: 0x040016EE RID: 5870
		public GuardPose upperLeftGuard = new GuardPose
		{
			id = "GuardUpperLeft"
		};

		// Token: 0x040016EF RID: 5871
		public GuardPose upperRightGuard = new GuardPose
		{
			id = "GuardUpperRight"
		};

		// Token: 0x040016F0 RID: 5872
		public GuardPose midLeftGuard = new GuardPose
		{
			id = "GuardMidLeft"
		};

		// Token: 0x040016F1 RID: 5873
		public GuardPose neutralGuard = new GuardPose
		{
			id = "GuardNeutral"
		};

		// Token: 0x040016F2 RID: 5874
		public GuardPose midRightGuard = new GuardPose
		{
			id = "GuardMidRight"
		};

		// Token: 0x040016F3 RID: 5875
		public GuardPose lowerLeftGuard = new GuardPose
		{
			id = "GuardLowerLeft"
		};

		// Token: 0x040016F4 RID: 5876
		public GuardPose lowerRightGuard = new GuardPose
		{
			id = "GuardLowerRight"
		};

		// Token: 0x040016F5 RID: 5877
		public List<AttackMotion> ripostes = new List<AttackMotion>();

		// Token: 0x040016F6 RID: 5878
		private int openersLastCount;

		// Token: 0x040016F7 RID: 5879
		private int chainsLastCount;

		// Token: 0x040016F8 RID: 5880
		private int ripostesLastCount;

		// Token: 0x040016F9 RID: 5881
		protected Dictionary<string, GuardPose> _guardPoseIDs;

		// Token: 0x040016FA RID: 5882
		protected Dictionary<int, GuardPose> _animationClipToGuardPose;

		// Token: 0x02000842 RID: 2114
		public enum BodyChainBehaviour
		{
			// Token: 0x040040FF RID: 16639
			Disabled,
			// Token: 0x04004100 RID: 16640
			UseChance,
			// Token: 0x04004101 RID: 16641
			Always
		}
	}
}

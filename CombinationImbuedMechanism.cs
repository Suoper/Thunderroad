using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002BB RID: 699
	public class CombinationImbuedMechanism : ThunderBehaviour
	{
		// Token: 0x060021E1 RID: 8673 RVA: 0x000E978C File Offset: 0x000E798C
		private void Awake()
		{
			EventManager.onLevelUnload += this.ReleaseEffects;
			this.combination = new Dictionary<ColliderGroup, string>();
			for (int i = 0; i < this.imbueCombination.Length; i++)
			{
				CombinationImbuedMechanism.ImbueCombination temp = this.imbueCombination[i];
				this.combination.Add(temp.colliderGroup, temp.imbueId);
			}
			this.currentIndex = 0;
			this.isDone = false;
		}

		// Token: 0x060021E2 RID: 8674 RVA: 0x000E97FC File Offset: 0x000E79FC
		public void OnImbue(ColliderGroup colliderGroup)
		{
			if (this.isDone)
			{
				return;
			}
			if (this.isDone)
			{
				return;
			}
			if (this.isReseting)
			{
				return;
			}
			string imbueId;
			if (this.combination.TryGetValue(colliderGroup, out imbueId) && !colliderGroup.imbue.spellCastBase.id.Equals(imbueId))
			{
				this.Failure(colliderGroup.colliders[0].transform);
				return;
			}
			if (this.isOrderedConbination && this.imbueCombination[this.currentIndex].colliderGroup != colliderGroup)
			{
				this.Failure(colliderGroup.colliders[0].transform);
				return;
			}
			colliderGroup.imbue.spellCastBase.imbueEffect.blockPoolSteal = true;
			this.currentIndex++;
			if (this.currentIndex == this.imbueCombination.Length)
			{
				this.Success(colliderGroup.colliders[0].transform);
			}
		}

		// Token: 0x060021E3 RID: 8675 RVA: 0x000E98ED File Offset: 0x000E7AED
		private void ReleaseEffects(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			EventManager.onLevelUnload -= this.ReleaseEffects;
			this.InstantReset();
		}

		// Token: 0x060021E4 RID: 8676 RVA: 0x000E9906 File Offset: 0x000E7B06
		public void ResetMechanism()
		{
			if (this.isReseting)
			{
				return;
			}
			this.isReseting = true;
			if (this.resetCoroutine != null)
			{
				base.StopCoroutine(this.resetCoroutine);
			}
			this.resetCoroutine = base.StartCoroutine(this.ResetCoroutine());
		}

		// Token: 0x060021E5 RID: 8677 RVA: 0x000E993E File Offset: 0x000E7B3E
		private void Success(Transform imbueOrb)
		{
			this.isDone = true;
			UnityEvent<Transform> unityEvent = this.onSuccess;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(imbueOrb);
		}

		// Token: 0x060021E6 RID: 8678 RVA: 0x000E9958 File Offset: 0x000E7B58
		private void Failure(Transform imbueOrb)
		{
			this.isDone = true;
			UnityEvent<Transform> unityEvent = this.onFailure;
			if (unityEvent != null)
			{
				unityEvent.Invoke(imbueOrb);
			}
			if (this.autoResetOnFailure)
			{
				this.ResetMechanism();
			}
		}

		// Token: 0x060021E7 RID: 8679 RVA: 0x000E9981 File Offset: 0x000E7B81
		private IEnumerator ResetCoroutine()
		{
			while (this.IsCasterImbueing())
			{
				yield return null;
			}
			this.InstantReset();
			yield break;
		}

		// Token: 0x060021E8 RID: 8680 RVA: 0x000E9990 File Offset: 0x000E7B90
		public void InstantReset()
		{
			for (int i = 0; i < this.imbueCombination.Length; i++)
			{
				Imbue imbue = this.imbueCombination[i].colliderGroup.imbue;
				if (imbue != null)
				{
					SpellCastCharge spellCastBase = imbue.spellCastBase;
					if (((spellCastBase != null) ? spellCastBase.imbueEffect : null) != null)
					{
						imbue.spellCastBase.imbueEffect.blockPoolSteal = false;
					}
					imbue.SetEnergyInstant(0f);
				}
			}
			this.currentIndex = 0;
			this.isDone = false;
			this.isReseting = false;
		}

		// Token: 0x060021E9 RID: 8681 RVA: 0x000E9A18 File Offset: 0x000E7C18
		private bool IsCasterImbueing()
		{
			PlayerHand handLeft = Player.local.handLeft;
			bool flag;
			if (handLeft == null)
			{
				flag = true;
			}
			else
			{
				RagdollHand ragdollHand = handLeft.ragdollHand;
				bool? flag2;
				if (ragdollHand == null)
				{
					flag2 = null;
				}
				else
				{
					SpellCaster caster = ragdollHand.caster;
					flag2 = ((caster != null) ? new bool?(caster.isFiring) : null);
				}
				bool? flag3 = flag2;
				bool flag4 = true;
				flag = !(flag3.GetValueOrDefault() == flag4 & flag3 != null);
			}
			if (flag)
			{
				PlayerHand handRight = Player.local.handRight;
				bool flag5;
				if (handRight == null)
				{
					flag5 = true;
				}
				else
				{
					RagdollHand ragdollHand2 = handRight.ragdollHand;
					bool? flag6;
					if (ragdollHand2 == null)
					{
						flag6 = null;
					}
					else
					{
						SpellCaster caster2 = ragdollHand2.caster;
						flag6 = ((caster2 != null) ? new bool?(caster2.isFiring) : null);
					}
					bool? flag3 = flag6;
					bool flag4 = true;
					flag5 = !(flag3.GetValueOrDefault() == flag4 & flag3 != null);
				}
				if (flag5)
				{
					return false;
				}
			}
			for (int i = 0; i < this.imbueCombination.Length; i++)
			{
				ColliderGroup colliderGroup = this.imbueCombination[i].colliderGroup;
				if (colliderGroup.imbue.spellCastBase != null)
				{
					SpellCaster spellCaster = colliderGroup.imbue.spellCastBase.spellCaster;
					if (!(spellCaster == null) && !spellCaster.imbueObjects.IsNullOrEmpty())
					{
						int count = spellCaster.imbueObjects.Count;
						for (int j = 0; j < count; j++)
						{
							if (spellCaster.imbueObjects[j].colliderGroup == colliderGroup)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x040020CB RID: 8395
		public CombinationImbuedMechanism.ImbueCombination[] imbueCombination;

		// Token: 0x040020CC RID: 8396
		public bool isOrderedConbination;

		// Token: 0x040020CD RID: 8397
		public bool autoResetOnFailure;

		// Token: 0x040020CE RID: 8398
		public UnityEvent<Transform> onSuccess;

		// Token: 0x040020CF RID: 8399
		public UnityEvent<Transform> onFailure;

		// Token: 0x040020D0 RID: 8400
		private Dictionary<ColliderGroup, string> combination;

		// Token: 0x040020D1 RID: 8401
		private int currentIndex;

		// Token: 0x040020D2 RID: 8402
		private bool isDone;

		// Token: 0x040020D3 RID: 8403
		private bool isReseting;

		// Token: 0x040020D4 RID: 8404
		private Coroutine resetCoroutine;

		// Token: 0x02000989 RID: 2441
		[Serializable]
		public struct ImbueCombination
		{
			// Token: 0x040044E5 RID: 17637
			public ColliderGroup colliderGroup;

			// Token: 0x040044E6 RID: 17638
			public string imbueId;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000257 RID: 599
	[RequireComponent(typeof(Animator))]
	public class FaceAnimator : ThunderBehaviour
	{
		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06001A32 RID: 6706 RVA: 0x000AEED7 File Offset: 0x000AD0D7
		public static FaceAnimator.Expression[] expressionList
		{
			get
			{
				if (FaceAnimator._expressionList.IsNullOrEmpty())
				{
					FaceAnimator._expressionList = (FaceAnimator.Expression[])Enum.GetValues(typeof(FaceAnimator.Expression));
				}
				return FaceAnimator._expressionList;
			}
		}

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06001A33 RID: 6707 RVA: 0x000AEF03 File Offset: 0x000AD103
		// (set) Token: 0x06001A34 RID: 6708 RVA: 0x000AEF0B File Offset: 0x000AD10B
		public bool animated { get; protected set; }

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06001A35 RID: 6709 RVA: 0x000AEF14 File Offset: 0x000AD114
		// (set) Token: 0x06001A36 RID: 6710 RVA: 0x000AEF1C File Offset: 0x000AD11C
		public FaceAnimator.Expression currentPrimaryExpression
		{
			get
			{
				return this._currentPrimaryExpression;
			}
			protected set
			{
				this._currentPrimaryExpression = value;
				this.expressionValues.target = new float[FaceAnimator.expressionList.Length];
				this.expressionValues.target[(int)value] = 1f;
			}
		}

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06001A37 RID: 6711 RVA: 0x000AEF4E File Offset: 0x000AD14E
		// (set) Token: 0x06001A38 RID: 6712 RVA: 0x000AEF56 File Offset: 0x000AD156
		public FaceAnimator.Expression lastPrimaryExpression { get; protected set; }

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06001A39 RID: 6713 RVA: 0x000AEF5F File Offset: 0x000AD15F
		// (set) Token: 0x06001A3A RID: 6714 RVA: 0x000AEF67 File Offset: 0x000AD167
		public bool customExpression { get; protected set; }

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x06001A3B RID: 6715 RVA: 0x000AEF70 File Offset: 0x000AD170
		// (set) Token: 0x06001A3C RID: 6716 RVA: 0x000AEF78 File Offset: 0x000AD178
		public FaceAnimator.SmoothDampTarget expressionValues { get; protected set; }

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06001A3D RID: 6717 RVA: 0x000AEF81 File Offset: 0x000AD181
		// (set) Token: 0x06001A3E RID: 6718 RVA: 0x000AEF89 File Offset: 0x000AD189
		public FaceAnimator.SmoothDampTarget varianceValues { get; protected set; }

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06001A3F RID: 6719 RVA: 0x000AEF92 File Offset: 0x000AD192
		// (set) Token: 0x06001A40 RID: 6720 RVA: 0x000AEF9A File Offset: 0x000AD19A
		public Animator animator { get; protected set; }

		// Token: 0x06001A41 RID: 6721 RVA: 0x000AEFA4 File Offset: 0x000AD1A4
		private void Start()
		{
			this.animator = base.GetComponent<Animator>();
			this.runtimeAnimatorOverrideController = new AnimatorOverrideController(this.animatorOverrideController.runtimeAnimatorController);
			this.runtimeAnimatorOverrideController.name = "FacialOverrideAnimator";
			List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
			this.animatorOverrideController.GetOverrides(overrides);
			List<KeyValuePair<AnimationClip, AnimationClip>> newOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>
			{
				new KeyValuePair<AnimationClip, AnimationClip>(this.overrideA, this.overrideA),
				new KeyValuePair<AnimationClip, AnimationClip>(this.overrideB, this.overrideB)
			};
			foreach (KeyValuePair<AnimationClip, AnimationClip> clipPair in overrides)
			{
				if (!(clipPair.Key == this.overrideA) && !(clipPair.Key == this.overrideB))
				{
					newOverrides.Add(clipPair);
				}
			}
			this.animationClipOverrides = newOverrides.ToArray();
			this.animator.runtimeAnimatorController = this.runtimeAnimatorOverrideController;
			this.runtimeAnimatorOverrideController.ApplyOverrides(overrides);
			this.expressionValues = new FaceAnimator.SmoothDampTarget(FaceAnimator.expressionList.Length);
			this.varianceValues = new FaceAnimator.SmoothDampTarget(FaceAnimator.expressionList.Length);
			if (FaceAnimator.expressionHashes == null)
			{
				FaceAnimator.expressionHashes = new int[FaceAnimator.expressionList.Length];
			}
			for (int i = 0; i < FaceAnimator.expressionList.Length; i++)
			{
				FaceAnimator.Expression expression = FaceAnimator.expressionList[i];
				FaceAnimator.expressionHashes[(int)expression] = Animator.StringToHash(expression.ToString());
				this.expressionValues.target[(int)expression] = this.animator.GetFloat(FaceAnimator.expressionHashes[(int)expression]);
				this.expressionValues.current[(int)expression] = this.animator.GetFloat(FaceAnimator.expressionHashes[(int)expression]);
			}
			this.hashDynamicExpression = Animator.StringToHash("DynamicExpression");
			this.hashLoopDynamic = Animator.StringToHash("LoopDynamic");
			this.currentPrimaryExpression = FaceAnimator.Expression.Neutral;
		}

		// Token: 0x140000B2 RID: 178
		// (add) Token: 0x06001A42 RID: 6722 RVA: 0x000AF19C File Offset: 0x000AD39C
		// (remove) Token: 0x06001A43 RID: 6723 RVA: 0x000AF1D4 File Offset: 0x000AD3D4
		public event FaceAnimator.ExpressionChange OnExpressionChanged;

		// Token: 0x06001A44 RID: 6724 RVA: 0x000AF20C File Offset: 0x000AD40C
		public void SetExpression(FaceAnimator.Expression expression)
		{
			this.SetExpression(expression, null);
		}

		// Token: 0x06001A45 RID: 6725 RVA: 0x000AF22C File Offset: 0x000AD42C
		public bool SetExpression(FaceAnimator.Expression expression, float? resetNeutralTime = null)
		{
			if (expression == this.currentPrimaryExpression)
			{
				return false;
			}
			this.lastPrimaryExpression = this.currentPrimaryExpression;
			this.currentPrimaryExpression = expression;
			this.neutralExpressionTime = ((resetNeutralTime != null) ? (Time.time + resetNeutralTime) : null);
			this.customExpression = false;
			FaceAnimator.ExpressionChange onExpressionChanged = this.OnExpressionChanged;
			if (onExpressionChanged != null)
			{
				onExpressionChanged(this.lastPrimaryExpression, expression);
			}
			return true;
		}

		// Token: 0x06001A46 RID: 6726 RVA: 0x000AF2BB File Offset: 0x000AD4BB
		public void SetCustomExpressionValues(float[] targets)
		{
			if (targets.IsNullOrEmpty() || targets.Length != this.expressionValues.target.Length)
			{
				return;
			}
			this.expressionValues.target = targets;
			this.customExpression = true;
		}

		// Token: 0x06001A47 RID: 6727 RVA: 0x000AF2EB File Offset: 0x000AD4EB
		public void PlayAnimation(AnimationClip clip)
		{
			this.PlayAnimation(clip, false);
		}

		// Token: 0x06001A48 RID: 6728 RVA: 0x000AF2F8 File Offset: 0x000AD4F8
		public void PlayAnimation(AnimationClip clip, bool loop)
		{
			this.animator.SetBool(this.hashLoopDynamic, loop);
			int target = (this.animator.GetInteger(this.hashDynamicExpression) == 1) ? 2 : 1;
			this.animationClipOverrides[target - 1] = new KeyValuePair<AnimationClip, AnimationClip>(this.animationClipOverrides[target - 1].Key, clip);
			this.runtimeAnimatorOverrideController.ApplyOverrides(this.animationClipOverrides);
			this.animator.SetInteger(this.hashDynamicExpression, target);
			this.animated = true;
			this.animationEndTime = Time.time + clip.length;
			base.StartCoroutine(this.AnimationEnd(this.animationEndTime));
		}

		// Token: 0x06001A49 RID: 6729 RVA: 0x000AF3A6 File Offset: 0x000AD5A6
		protected IEnumerator AnimationEnd(float endTime)
		{
			yield return Yielders.ForSeconds(endTime - Time.time);
			if (!endTime.IsApproximately(this.animationEndTime) && endTime < this.animationEndTime)
			{
				yield break;
			}
			this.animated = false;
			yield break;
		}

		// Token: 0x06001A4A RID: 6730 RVA: 0x000AF3BC File Offset: 0x000AD5BC
		public void StopAnimation()
		{
			this.animator.SetInteger(this.hashDynamicExpression, 0);
			this.animator.SetBool(this.hashLoopDynamic, false);
			this.animated = false;
		}

		// Token: 0x06001A4B RID: 6731 RVA: 0x000AF3EC File Offset: 0x000AD5EC
		public void SmoothDampChange(FaceAnimator.SmoothDampTarget values, float maxChangeSpeed)
		{
			this.smoothTime = Mathf.Max(0.0001f, this.smoothTime);
			float omega = 2f / this.smoothTime;
			float x = omega * Time.deltaTime;
			float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
			values.ZeroEphemerals();
			for (int i = 0; i < values.floatCount; i++)
			{
				values.modified_target[i] = values.target[i];
				values.change_values[i] = values.current[i] - values.target[i];
			}
			float maxChange = maxChangeSpeed * this.smoothTime;
			float maxChangeSq = maxChange * maxChange;
			float sqrMag = 0f;
			for (int j = 0; j < values.change_values.Length; j++)
			{
				sqrMag += values.change_values[j] * values.change_values[j];
			}
			float orig_sum_value = 0f;
			float mag = (float)Math.Sqrt((double)sqrMag);
			float deltaTime = Time.deltaTime;
			for (int k = 0; k < values.floatCount; k++)
			{
				if (sqrMag > maxChangeSq)
				{
					values.change_values[k] = values.change_values[k] / mag * maxChange;
				}
				values.modified_target[k] = values.current[k] - values.change_values[k];
				values.temp_values[k] = (values.velocity[k] + omega * values.change_values[k]) * deltaTime;
				values.velocity[k] = (values.velocity[k] - omega * values.temp_values[k]) * exp;
				values.output_values[k] = values.modified_target[k] + (values.change_values[k] + values.temp_values[k]) * exp;
				values.orig_minus_current[k] = values.target[k] - values.current[k];
				values.out_minus_orig[k] = values.output_values[k] - values.target[k];
				orig_sum_value += values.orig_minus_current[k] * values.out_minus_orig[k];
			}
			for (int l = 0; l < values.floatCount; l++)
			{
				if (orig_sum_value > 0f)
				{
					values.output_values[l] = values.target[l];
					values.velocity[l] = (values.output_values[l] - values.target[l]) / Time.deltaTime;
				}
				values.current[l] = values.output_values[l];
			}
		}

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06001A4C RID: 6732 RVA: 0x000AF65F File Offset: 0x000AD85F
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001A4D RID: 6733 RVA: 0x000AF662 File Offset: 0x000AD862
		protected internal override void ManagedUpdate()
		{
			base.ManagedUpdate();
			this.AutoUpdates();
		}

		// Token: 0x06001A4E RID: 6734 RVA: 0x000AF670 File Offset: 0x000AD870
		public void AutoUpdates()
		{
			if (!this.autoUpdate)
			{
				return;
			}
			if (this.neutralExpressionTime != null && Time.time >= this.neutralExpressionTime.Value)
			{
				this.SetExpression(FaceAnimator.Expression.Neutral);
			}
			if (this.brainDriven)
			{
				return;
			}
			this.SmoothDampChange(this.expressionValues, this.expressionChangeSpeedMax);
			this.UpdateAnimator();
		}

		// Token: 0x06001A4F RID: 6735 RVA: 0x000AF6D0 File Offset: 0x000AD8D0
		public void UpdateAnimator()
		{
			for (int i = 0; i < FaceAnimator.expressionList.Length; i++)
			{
				FaceAnimator.Expression expression = FaceAnimator.expressionList[i];
				float value = this.expressionValues.current[(int)expression] + this.varianceValues.current[(int)expression];
				this.animator.SetFloat(FaceAnimator.expressionHashes[(int)expression], value);
			}
		}

		// Token: 0x06001A50 RID: 6736 RVA: 0x000AF726 File Offset: 0x000AD926
		private void OnValidate()
		{
		}

		// Token: 0x040018E7 RID: 6375
		private static FaceAnimator.Expression[] _expressionList;

		// Token: 0x040018E8 RID: 6376
		public float expressionChangeSpeedMax = 15f;

		// Token: 0x040018E9 RID: 6377
		public float varianceChangeSpeedMax = 1f;

		// Token: 0x040018EA RID: 6378
		public float smoothTime = 0.15f;

		// Token: 0x040018EB RID: 6379
		public bool autoUpdate;

		// Token: 0x040018EC RID: 6380
		public AnimatorOverrideController animatorOverrideController;

		// Token: 0x040018ED RID: 6381
		public AnimationClip overrideA;

		// Token: 0x040018EE RID: 6382
		public AnimationClip overrideB;

		// Token: 0x040018F0 RID: 6384
		private FaceAnimator.Expression _currentPrimaryExpression;

		// Token: 0x040018F6 RID: 6390
		[NonSerialized]
		public bool brainDriven;

		// Token: 0x040018F7 RID: 6391
		protected AnimatorOverrideController runtimeAnimatorOverrideController;

		// Token: 0x040018F8 RID: 6392
		protected KeyValuePair<AnimationClip, AnimationClip>[] animationClipOverrides;

		// Token: 0x040018F9 RID: 6393
		protected static int[] expressionHashes;

		// Token: 0x040018FA RID: 6394
		protected int hashDynamicExpression;

		// Token: 0x040018FB RID: 6395
		protected int hashLoopDynamic;

		// Token: 0x040018FC RID: 6396
		protected float animationEndTime;

		// Token: 0x040018FD RID: 6397
		protected float? neutralExpressionTime;

		// Token: 0x020008A7 RID: 2215
		public enum Expression
		{
			// Token: 0x04004222 RID: 16930
			Angry,
			// Token: 0x04004223 RID: 16931
			Attack,
			// Token: 0x04004224 RID: 16932
			Confusion,
			// Token: 0x04004225 RID: 16933
			Death,
			// Token: 0x04004226 RID: 16934
			Fear,
			// Token: 0x04004227 RID: 16935
			Happy,
			// Token: 0x04004228 RID: 16936
			Intrigue,
			// Token: 0x04004229 RID: 16937
			Neutral,
			// Token: 0x0400422A RID: 16938
			Pain,
			// Token: 0x0400422B RID: 16939
			Sad,
			// Token: 0x0400422C RID: 16940
			Surprise,
			// Token: 0x0400422D RID: 16941
			Tired
		}

		// Token: 0x020008A8 RID: 2216
		public class SmoothDampTarget
		{
			// Token: 0x1700052D RID: 1325
			// (get) Token: 0x060040F9 RID: 16633 RVA: 0x0018953F File Offset: 0x0018773F
			// (set) Token: 0x060040FA RID: 16634 RVA: 0x00189547 File Offset: 0x00187747
			public int floatCount { get; protected set; }

			// Token: 0x060040FB RID: 16635 RVA: 0x00189550 File Offset: 0x00187750
			public SmoothDampTarget(int count)
			{
				this.floatCount = count;
				this.target = new float[count];
				this.current = new float[count];
				this.velocity = new float[count];
				this.change_values = new float[count];
				this.modified_target = new float[count];
				this.temp_values = new float[count];
				this.output_values = new float[count];
				this.orig_minus_current = new float[count];
				this.out_minus_orig = new float[count];
			}

			// Token: 0x060040FC RID: 16636 RVA: 0x001895D8 File Offset: 0x001877D8
			public void ZeroEphemerals()
			{
				for (int i = 0; i < this.floatCount; i++)
				{
					this.change_values[i] = 0f;
					this.modified_target[i] = 0f;
					this.temp_values[i] = 0f;
					this.output_values[i] = 0f;
					this.orig_minus_current[i] = 0f;
					this.out_minus_orig[i] = 0f;
				}
			}

			// Token: 0x0400422F RID: 16943
			public float[] target;

			// Token: 0x04004230 RID: 16944
			public float[] current;

			// Token: 0x04004231 RID: 16945
			public float[] velocity;

			// Token: 0x04004232 RID: 16946
			public float[] change_values;

			// Token: 0x04004233 RID: 16947
			public float[] modified_target;

			// Token: 0x04004234 RID: 16948
			public float[] temp_values;

			// Token: 0x04004235 RID: 16949
			public float[] output_values;

			// Token: 0x04004236 RID: 16950
			public float[] orig_minus_current;

			// Token: 0x04004237 RID: 16951
			public float[] out_minus_orig;
		}

		// Token: 0x020008A9 RID: 2217
		// (Invoke) Token: 0x060040FE RID: 16638
		public delegate void ExpressionChange(FaceAnimator.Expression previousExpression, FaceAnimator.Expression newExpression);
	}
}

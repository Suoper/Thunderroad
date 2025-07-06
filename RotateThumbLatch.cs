using System;
using System.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002C2 RID: 706
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/RotateThumbLatch")]
	public class RotateThumbLatch : MonoBehaviour
	{
		// Token: 0x06002258 RID: 8792 RVA: 0x000ED130 File Offset: 0x000EB330
		private void Awake()
		{
			this._defaultRotation = this.transformToRotate.localRotation;
			this._frontHandleRotation = this._defaultRotation * Quaternion.Euler(this.localRotationFrontHandleOffset);
			this._backHandleRotation = this._defaultRotation * Quaternion.Euler(this.localRotationBackHandleGoalOffset);
			this._defaultTranslation = this.latchToTranslate.localPosition;
			this._goalTranslation = this._defaultTranslation + this.localTranslationOffset;
		}

		// Token: 0x06002259 RID: 8793 RVA: 0x000ED1AE File Offset: 0x000EB3AE
		public void RotateToFrontHandleGoal(float timeToMatch)
		{
			if (this._currentRotationRoutine != null)
			{
				base.StopCoroutine(this._currentRotationRoutine);
			}
			this._currentRotationRoutine = base.StartCoroutine(this.RotateToRoutine(this._frontHandleRotation, timeToMatch));
		}

		// Token: 0x0600225A RID: 8794 RVA: 0x000ED1DD File Offset: 0x000EB3DD
		public void RotateToBackHandleGoal(float timeToMatch)
		{
			if (this._currentRotationRoutine != null)
			{
				base.StopCoroutine(this._currentRotationRoutine);
			}
			this._currentRotationRoutine = base.StartCoroutine(this.RotateToRoutine(this._backHandleRotation, timeToMatch));
		}

		// Token: 0x0600225B RID: 8795 RVA: 0x000ED20C File Offset: 0x000EB40C
		public void RotateToDefault(float timeToMatch)
		{
			if (this._currentRotationRoutine != null)
			{
				base.StopCoroutine(this._currentRotationRoutine);
			}
			this._currentRotationRoutine = base.StartCoroutine(this.RotateToRoutine(this._defaultRotation, timeToMatch));
		}

		// Token: 0x0600225C RID: 8796 RVA: 0x000ED23B File Offset: 0x000EB43B
		private IEnumerator RotateToRoutine(Quaternion rotation, float timeToMatch)
		{
			Quaternion currentRotation = this.transformToRotate.localRotation;
			float t = 0f;
			while (t < timeToMatch)
			{
				this.transformToRotate.localRotation = Quaternion.Lerp(currentRotation, rotation, this.rotationEasing.Evaluate(t / timeToMatch));
				t += Time.deltaTime;
				yield return null;
			}
			this.transformToRotate.localRotation = rotation;
			yield break;
		}

		// Token: 0x0600225D RID: 8797 RVA: 0x000ED258 File Offset: 0x000EB458
		public void TranslateToDefault(float timeToMatch)
		{
			if (this._currentTranslationRoutine != null)
			{
				base.StopCoroutine(this._currentTranslationRoutine);
			}
			this._currentTranslationRoutine = base.StartCoroutine(this.TranslateToRoutine(this._defaultTranslation, timeToMatch));
		}

		// Token: 0x0600225E RID: 8798 RVA: 0x000ED287 File Offset: 0x000EB487
		public void TranslateToGoal(float timeToMatch)
		{
			if (this._currentTranslationRoutine != null)
			{
				base.StopCoroutine(this._currentTranslationRoutine);
			}
			this._currentTranslationRoutine = base.StartCoroutine(this.TranslateToRoutine(this._goalTranslation, timeToMatch));
		}

		// Token: 0x0600225F RID: 8799 RVA: 0x000ED2B6 File Offset: 0x000EB4B6
		private IEnumerator TranslateToRoutine(Vector3 position, float timeToMatch)
		{
			Vector3 currentPosition = this.latchToTranslate.localPosition;
			float t = 0f;
			while (t < timeToMatch)
			{
				this.latchToTranslate.localPosition = Vector3.Lerp(currentPosition, position, this.translationEasing.Evaluate(t / timeToMatch));
				t += Time.deltaTime;
				yield return null;
			}
			this.latchToTranslate.localPosition = position;
			yield break;
		}

		// Token: 0x06002260 RID: 8800 RVA: 0x000ED2D3 File Offset: 0x000EB4D3
		private IEnumerator BlendPoseRoutine(RagdollHand hand, float timeToBlend, bool zeroToOne)
		{
			hand.poser.SetTargetWeight((float)(zeroToOne ? 0 : 1), false);
			float t = 0f;
			while (t < timeToBlend)
			{
				float weight = this.rotationEasing.Evaluate(t / timeToBlend);
				hand.poser.SetTargetWeight(zeroToOne ? weight : (1f - weight), false);
				t += Time.deltaTime;
				yield return null;
			}
			hand.poser.SetTargetWeight((float)(zeroToOne ? 1 : 0), false);
			yield break;
		}

		// Token: 0x06002261 RID: 8801 RVA: 0x000ED2F8 File Offset: 0x000EB4F8
		public void Rotate(float angle, HingeDrive.HingeDriveSpeedState speedState, Handle handle)
		{
			if (this.frontHandle == handle)
			{
				this.RotateToFrontHandleGoal(this.timeToRotate);
			}
			else if (this.backHandle == handle)
			{
				this.RotateToBackHandleGoal(this.timeToRotate);
			}
			this.TranslateToGoal(this.timeToTranslate);
			foreach (RagdollHand ragdollHand in handle.handlers)
			{
				base.StartCoroutine(this.BlendPoseRoutine(ragdollHand, this.timeToRotate, true));
			}
		}

		// Token: 0x06002262 RID: 8802 RVA: 0x000ED39C File Offset: 0x000EB59C
		public void RotateToDefault(float angle, HingeDrive.HingeDriveSpeedState speedState, Handle handle)
		{
			if (this.frontHandle == handle)
			{
				this.RotateToDefault(this.timeToRotate);
			}
			else if (this.backHandle == handle)
			{
				this.RotateToDefault(this.timeToRotate);
			}
			this.TranslateToDefault(this.timeToTranslate);
			foreach (RagdollHand ragdollHand in handle.handlers)
			{
				base.StartCoroutine(this.BlendPoseRoutine(ragdollHand, this.timeToRotate, false));
			}
		}

		// Token: 0x04002166 RID: 8550
		[Header("Thumb latch rotation")]
		public Transform transformToRotate;

		// Token: 0x04002167 RID: 8551
		public Vector3 localRotationFrontHandleOffset;

		// Token: 0x04002168 RID: 8552
		public Vector3 localRotationBackHandleGoalOffset;

		// Token: 0x04002169 RID: 8553
		public AnimationCurve rotationEasing;

		// Token: 0x0400216A RID: 8554
		public float timeToRotate = 0.1f;

		// Token: 0x0400216B RID: 8555
		[Header("Latch translation")]
		public Transform latchToTranslate;

		// Token: 0x0400216C RID: 8556
		public Vector3 localTranslationOffset;

		// Token: 0x0400216D RID: 8557
		public AnimationCurve translationEasing;

		// Token: 0x0400216E RID: 8558
		public float timeToTranslate = 0.1f;

		// Token: 0x0400216F RID: 8559
		[Header("Handles")]
		public Handle frontHandle;

		// Token: 0x04002170 RID: 8560
		public Handle backHandle;

		// Token: 0x04002171 RID: 8561
		private Quaternion _defaultRotation;

		// Token: 0x04002172 RID: 8562
		private Quaternion _frontHandleRotation;

		// Token: 0x04002173 RID: 8563
		private Quaternion _backHandleRotation;

		// Token: 0x04002174 RID: 8564
		private Vector3 _defaultTranslation;

		// Token: 0x04002175 RID: 8565
		private Vector3 _goalTranslation;

		// Token: 0x04002176 RID: 8566
		private Coroutine _currentRotationRoutine;

		// Token: 0x04002177 RID: 8567
		private Coroutine _currentTranslationRoutine;
	}
}

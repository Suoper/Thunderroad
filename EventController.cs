using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002C5 RID: 709
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Event-Linkers/EventController.html")]
	[AddComponentMenu("ThunderRoad/Levels/Event Controller")]
	public class EventController : MonoBehaviour
	{
		// Token: 0x0600226A RID: 8810 RVA: 0x000ED6AA File Offset: 0x000EB8AA
		protected void Awake()
		{
			this.invokedIndex = new bool[this.invokeMaxIndex + 1];
		}

		// Token: 0x0600226B RID: 8811 RVA: 0x000ED6BF File Offset: 0x000EB8BF
		protected void Start()
		{
			if (this.beginOnStart)
			{
				this.Invoke();
			}
		}

		// Token: 0x0600226C RID: 8812 RVA: 0x000ED6D0 File Offset: 0x000EB8D0
		public void Invoke()
		{
			if (this.timedEvent != null && this.invokeCount < this.maxInvoke)
			{
				if (this.concurentInvokeBehaviour == EventController.ConcurentInvokeBehaviour.IgnoreIfRunning)
				{
					if (this.coroutine != null)
					{
						return;
					}
					this.coroutine = base.StartCoroutine(this.InvokeCoroutine());
					return;
				}
				else
				{
					if (this.concurentInvokeBehaviour == EventController.ConcurentInvokeBehaviour.StopAndReplace)
					{
						if (this.coroutine != null)
						{
							base.StopCoroutine(this.coroutine);
						}
						this.coroutine = base.StartCoroutine(this.InvokeCoroutine());
						return;
					}
					if (this.concurentInvokeBehaviour == EventController.ConcurentInvokeBehaviour.RunParallel)
					{
						base.StartCoroutine(this.InvokeCoroutine());
					}
				}
			}
		}

		// Token: 0x0600226D RID: 8813 RVA: 0x000ED760 File Offset: 0x000EB960
		public void Invoke(int index)
		{
			this.invokedIndex[index] = true;
			for (int i = 0; i <= this.invokeMaxIndex; i++)
			{
				if (!this.invokedIndex[i])
				{
					return;
				}
			}
			this.Invoke();
		}

		// Token: 0x0600226E RID: 8814 RVA: 0x000ED798 File Offset: 0x000EB998
		public void StopInvoke(int index)
		{
			this.invokedIndex[index] = false;
			this.StopInvoke();
		}

		// Token: 0x0600226F RID: 8815 RVA: 0x000ED7A9 File Offset: 0x000EB9A9
		public void InvokeNow()
		{
			if (this.timedEvent != null && this.invokeCount < this.maxInvoke)
			{
				this.invokeCount++;
				this.timedEvent.Invoke();
				this.invokeCount++;
			}
		}

		// Token: 0x06002270 RID: 8816 RVA: 0x000ED7E8 File Offset: 0x000EB9E8
		public void StopInvoke()
		{
			if (this.coroutine != null)
			{
				base.StopCoroutine(this.coroutine);
				this.coroutine = null;
			}
		}

		// Token: 0x06002271 RID: 8817 RVA: 0x000ED805 File Offset: 0x000EBA05
		private IEnumerator InvokeCoroutine()
		{
			int i = 0;
			while ((float)i < this.loopCount)
			{
				float delay = UnityEngine.Random.Range(this.minDelay, this.maxDelay);
				yield return Yielders.ForSeconds(delay);
				this.invokeCount++;
				this.timedEvent.Invoke();
				int num = i;
				i = num + 1;
			}
			this.coroutine = null;
			yield break;
		}

		// Token: 0x04002184 RID: 8580
		[Header("General")]
		public bool beginOnStart;

		// Token: 0x04002185 RID: 8581
		public int maxInvoke = 999999999;

		// Token: 0x04002186 RID: 8582
		public EventController.ConcurentInvokeBehaviour concurentInvokeBehaviour;

		// Token: 0x04002187 RID: 8583
		[Header("Loop")]
		public float loopCount = 1f;

		// Token: 0x04002188 RID: 8584
		[Header("Delay")]
		public float minDelay;

		// Token: 0x04002189 RID: 8585
		public float maxDelay;

		// Token: 0x0400218A RID: 8586
		[Header("Multiple conditions")]
		[Tooltip("Number of different index in case of multiple conditions")]
		public int invokeMaxIndex;

		// Token: 0x0400218B RID: 8587
		public UnityEvent timedEvent = new UnityEvent();

		// Token: 0x0400218C RID: 8588
		protected int invokeCount;

		// Token: 0x0400218D RID: 8589
		protected Coroutine coroutine;

		// Token: 0x0400218E RID: 8590
		protected bool[] invokedIndex;

		// Token: 0x020009A6 RID: 2470
		public enum ConcurentInvokeBehaviour
		{
			// Token: 0x04004560 RID: 17760
			IgnoreIfRunning,
			// Token: 0x04004561 RID: 17761
			StopAndReplace,
			// Token: 0x04004562 RID: 17762
			RunParallel
		}
	}
}

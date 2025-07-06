using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x02000348 RID: 840
	public class InputHoldTimerEvents : MonoBehaviour
	{
		// Token: 0x17000256 RID: 598
		// (get) Token: 0x06002741 RID: 10049 RVA: 0x0010F403 File Offset: 0x0010D603
		private float currentTime
		{
			get
			{
				if (!this.useRealTime)
				{
					return Time.time;
				}
				return Time.realtimeSinceStartup;
			}
		}

		// Token: 0x06002742 RID: 10050 RVA: 0x0010F418 File Offset: 0x0010D618
		private void Start()
		{
			this.curveStartTime = this.inputTimeCurve.GetFirstTime();
			this.curveEndTime = this.inputTimeCurve.GetLastTime();
		}

		// Token: 0x06002743 RID: 10051 RVA: 0x0010F43C File Offset: 0x0010D63C
		public void SetInput(bool active)
		{
			if (active && !this.input)
			{
				this.inputTrueTime = this.currentTime;
				if (this.autoActivateIfPastCurveEnd)
				{
					this.waitUntilEndTime = base.StartCoroutine(this.WaitForCurveEnd());
				}
			}
			if (!active && this.input)
			{
				this.inputFalseTime = this.currentTime;
				if (this.waitUntilEndTime != null)
				{
					base.StopCoroutine(this.waitUntilEndTime);
				}
				this.ActivateEvent(this.inputFalseTime - this.inputTrueTime);
			}
			this.input = active;
		}

		// Token: 0x06002744 RID: 10052 RVA: 0x0010F4C0 File Offset: 0x0010D6C0
		private void ActivateEvent(float inputTime)
		{
			if (inputTime < this.curveStartTime)
			{
				return;
			}
			int curveOutput = Mathf.RoundToInt(this.inputTimeCurve.Evaluate(Mathf.Clamp(inputTime, this.curveStartTime, this.curveEndTime)));
			if (curveOutput >= this.events.Count)
			{
				Debug.LogError(string.Format("Curve output ({0}) is out of the index range for events! Perhaps you forgot to remove curve keys, or adjust curve output values?", curveOutput));
				return;
			}
			this.events[curveOutput].Invoke();
		}

		// Token: 0x06002745 RID: 10053 RVA: 0x0010F52F File Offset: 0x0010D72F
		private IEnumerator WaitForCurveEnd()
		{
			if (this.useRealTime)
			{
				yield return Yielders.ForRealSeconds(this.curveEndTime);
			}
			else
			{
				yield return Yielders.ForSeconds(this.curveEndTime);
			}
			this.ActivateEvent(this.curveEndTime);
			this.input = false;
			this.waitUntilEndTime = null;
			yield break;
		}

		// Token: 0x04002661 RID: 9825
		public AnimationCurve inputTimeCurve = new AnimationCurve();

		// Token: 0x04002662 RID: 9826
		public bool autoActivateIfPastCurveEnd;

		// Token: 0x04002663 RID: 9827
		public bool useRealTime;

		// Token: 0x04002664 RID: 9828
		[Tooltip("Input time, output an integer value corresponding with to the event in the list below. Lists and arrays start at 0 and go up from there! The events are always in order!")]
		public List<UnityEvent> events = new List<UnityEvent>();

		// Token: 0x04002665 RID: 9829
		[NonSerialized]
		public float inputTrueTime;

		// Token: 0x04002666 RID: 9830
		[NonSerialized]
		public bool input;

		// Token: 0x04002667 RID: 9831
		[NonSerialized]
		public float inputFalseTime;

		// Token: 0x04002668 RID: 9832
		[NonSerialized]
		public int lastEventOutput;

		// Token: 0x04002669 RID: 9833
		protected float curveStartTime;

		// Token: 0x0400266A RID: 9834
		protected float curveEndTime;

		// Token: 0x0400266B RID: 9835
		private Coroutine waitUntilEndTime;
	}
}

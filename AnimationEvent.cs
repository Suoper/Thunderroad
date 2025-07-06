using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x0200030E RID: 782
	[RequireComponent(typeof(Animator))]
	public class AnimationEvent : MonoBehaviour
	{
		// Token: 0x0600252D RID: 9517 RVA: 0x000FEFAC File Offset: 0x000FD1AC
		public void invokeStartEvent()
		{
			AnimationEvent.AnimatorStateEvent stateEvent = this.GetCurrentAnimatorStateEvent(0);
			if (stateEvent == null)
			{
				return;
			}
			stateEvent.startStateEvent.Invoke();
		}

		// Token: 0x0600252E RID: 9518 RVA: 0x000FEFD0 File Offset: 0x000FD1D0
		public void invokeEndEvent()
		{
			AnimationEvent.AnimatorStateEvent stateEvent = this.GetCurrentAnimatorStateEvent(0);
			if (stateEvent == null)
			{
				return;
			}
			stateEvent.endStateEvent.Invoke();
		}

		// Token: 0x0600252F RID: 9519 RVA: 0x000FEFF4 File Offset: 0x000FD1F4
		public void invokeCustomEvent(int customIndex)
		{
			AnimationEvent.AnimatorStateEvent stateEvent = this.GetCurrentAnimatorStateEvent(0);
			if (stateEvent == null)
			{
				return;
			}
			if (customIndex >= 0 && stateEvent.customStateEvent.Length > customIndex)
			{
				stateEvent.customStateEvent[customIndex].Invoke();
			}
		}

		// Token: 0x06002530 RID: 9520 RVA: 0x000FF02C File Offset: 0x000FD22C
		public AnimationEvent.AnimatorStateEvent GetCurrentAnimatorStateEvent(int layer = 0)
		{
			AnimatorStateInfo currentState = this.animator.GetCurrentAnimatorStateInfo(layer);
			foreach (AnimationEvent.AnimatorStateEvent stateEvent in this.statesEvent)
			{
				if (currentState.IsName(stateEvent.stateName))
				{
					return stateEvent;
				}
			}
			Debug.LogError(string.Format("Can not find current animator state name : {0} on {1}", currentState.fullPathHash, this.animator.gameObject.GetPathFromRoot()));
			return null;
		}

		// Token: 0x040024B2 RID: 9394
		public Animator animator;

		// Token: 0x040024B3 RID: 9395
		public AnimationEvent.AnimatorStateEvent[] statesEvent;

		// Token: 0x02000A0A RID: 2570
		[Serializable]
		public class AnimatorStateEvent
		{
			// Token: 0x040046D3 RID: 18131
			public string stateName;

			// Token: 0x040046D4 RID: 18132
			public UnityEvent startStateEvent;

			// Token: 0x040046D5 RID: 18133
			public UnityEvent endStateEvent;

			// Token: 0x040046D6 RID: 18134
			public UnityEvent[] customStateEvent;
		}
	}
}

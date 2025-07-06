using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002CA RID: 714
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/ExposureTransitionZone")]
	[RequireComponent(typeof(BoxCollider))]
	public class ExposureTransitionZone : ExposureSetterZone
	{
		// Token: 0x060022A2 RID: 8866 RVA: 0x000EE021 File Offset: 0x000EC221
		public void SetExposureOnExit()
		{
			base.SetTargetExposure(this.exposureOnExit);
		}

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x060022A3 RID: 8867 RVA: 0x000EE02F File Offset: 0x000EC22F
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return (ManagedLoops)0;
			}
		}

		// Token: 0x060022A4 RID: 8868 RVA: 0x000EE034 File Offset: 0x000EC234
		protected override void PlayerZoneChange(bool enter, ExposureSetterZone.Direction direction)
		{
			this.playerCollidersEntered += (enter ? 1 : -1);
			base.playerInZone = (this.playerCollidersEntered > 0);
			if (enter || base.playerInZone)
			{
				return;
			}
			this.targetExposure = ((direction == ExposureSetterZone.Direction.Enter) ? this.exposureOnEnter : this.exposureOnExit);
			if (this.transitionCoroutine != null)
			{
				base.StopCoroutine(this.transitionCoroutine);
			}
			this.transitionCoroutine = base.StartCoroutine(base.TransitionExposure());
			this.playerCollidersEntered = 0;
		}

		// Token: 0x040021BE RID: 8638
		[Tooltip("The exposure to set when the player leaves zone via red portal.")]
		public float exposureOnExit;

		// Token: 0x040021BF RID: 8639
		protected int playerCollidersEntered;
	}
}

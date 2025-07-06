using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000266 RID: 614
	[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Kick off config")]
	public class GolemKickOff : GolemAbility
	{
		// Token: 0x06001B9C RID: 7068 RVA: 0x000B6FEE File Offset: 0x000B51EE
		public override void Begin(GolemController golem)
		{
			base.Begin(golem);
			golem.PerformAttackMotion(this.motion, new Action(base.End));
			this.kickActive = true;
		}

		// Token: 0x06001B9D RID: 7069 RVA: 0x000B7018 File Offset: 0x000B5218
		public override void AbilityStep(int step)
		{
			base.AbilityStep(step);
			if (!this.kickActive)
			{
				return;
			}
			if (this.golem.isClimbed)
			{
				foreach (Creature creature in this.golem.ForceUngripClimbersEnumerable(true, false))
				{
					if (this.launchClimbers)
					{
						Vector3 force = (this.launchVertical ? (creature.transform.position - this.golem.transform.position).normalized : (creature.transform.position.ToXZ() - this.golem.transform.position.ToXZ()).normalized) * this.launchSpeed;
						creature.AddForce(force, ForceMode.VelocityChange, 1f, null);
					}
				}
			}
		}

		// Token: 0x06001B9E RID: 7070 RVA: 0x000B7114 File Offset: 0x000B5314
		public override void Interrupt()
		{
			base.Interrupt();
			this.kickActive = false;
		}

		// Token: 0x06001B9F RID: 7071 RVA: 0x000B7123 File Offset: 0x000B5323
		public override void OnEnd()
		{
			base.OnEnd();
			this.kickActive = false;
		}

		// Token: 0x04001A66 RID: 6758
		public GolemController.AttackMotion motion;

		// Token: 0x04001A67 RID: 6759
		public bool launchClimbers = true;

		// Token: 0x04001A68 RID: 6760
		public float launchSpeed = 2f;

		// Token: 0x04001A69 RID: 6761
		public bool launchVertical;

		// Token: 0x04001A6A RID: 6762
		private bool kickActive;
	}
}

using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200024B RID: 587
	public class AirHelper : ThunderBehaviour
	{
		// Token: 0x1400008E RID: 142
		// (add) Token: 0x06001899 RID: 6297 RVA: 0x000A2814 File Offset: 0x000A0A14
		// (remove) Token: 0x0600189A RID: 6298 RVA: 0x000A284C File Offset: 0x000A0A4C
		public event AirHelper.AirEvent OnAirEvent;

		// Token: 0x1400008F RID: 143
		// (add) Token: 0x0600189B RID: 6299 RVA: 0x000A2884 File Offset: 0x000A0A84
		// (remove) Token: 0x0600189C RID: 6300 RVA: 0x000A28BC File Offset: 0x000A0ABC
		public event AirHelper.AirEvent OnGroundEvent;

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x0600189D RID: 6301 RVA: 0x000A28F1 File Offset: 0x000A0AF1
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x0600189E RID: 6302 RVA: 0x000A28F4 File Offset: 0x000A0AF4
		public bool Climbing
		{
			get
			{
				RagdollHandClimb climb = Player.currentCreature.handLeft.climb;
				if (climb == null || !climb.isGripping || climb.gripItem != null)
				{
					climb = Player.currentCreature.handRight.climb;
					return climb != null && climb.isGripping && climb.gripItem == null;
				}
				return true;
			}
		}

		// Token: 0x0600189F RID: 6303 RVA: 0x000A294C File Offset: 0x000A0B4C
		protected internal override void ManagedUpdate()
		{
			base.ManagedUpdate();
			Player local = Player.local;
			this.locomotion = ((local != null) ? local.locomotion : null);
			if (this.locomotion == null)
			{
				return;
			}
			Creature currentCreature = Player.currentCreature;
			if (((currentCreature != null) ? currentCreature.handLeft : null) == null)
			{
				return;
			}
			bool climbing = this.Climbing;
			RaycastHit raycastHit;
			float num;
			if (this.inAir)
			{
				if ((this.locomotion.isGrounded && !this.locomotion.isJumping) || climbing)
				{
					this.Trigger(false);
					return;
				}
			}
			else if ((!this.locomotion.isGrounded || this.locomotion.isJumping) && !this.locomotion.SphereCastGround(this.minHeight, out raycastHit, out num) && !climbing)
			{
				this.Trigger(true);
			}
		}

		// Token: 0x060018A0 RID: 6304 RVA: 0x000A2A14 File Offset: 0x000A0C14
		public void Trigger(bool active)
		{
			this.inAir = active;
			Player player = this.locomotion.player;
			Creature creature = ((player != null) ? player.creature : null) ?? this.locomotion.creature;
			if (!creature)
			{
				return;
			}
			if (active)
			{
				AirHelper.AirEvent onAirEvent = this.OnAirEvent;
				if (onAirEvent == null)
				{
					return;
				}
				onAirEvent(creature);
				return;
			}
			else
			{
				AirHelper.AirEvent onGroundEvent = this.OnGroundEvent;
				if (onGroundEvent == null)
				{
					return;
				}
				onGroundEvent(creature);
				return;
			}
		}

		// Token: 0x040017AD RID: 6061
		public float minHeight = 1f;

		// Token: 0x040017B0 RID: 6064
		public Locomotion locomotion;

		// Token: 0x040017B1 RID: 6065
		public bool inAir;

		// Token: 0x02000868 RID: 2152
		// (Invoke) Token: 0x06004020 RID: 16416
		public delegate void AirEvent(Creature creature);
	}
}

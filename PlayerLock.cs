using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002E9 RID: 745
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/PlayerLock.html")]
	public class PlayerLock : MonoBehaviour
	{
		// Token: 0x060023D7 RID: 9175 RVA: 0x000F55EE File Offset: 0x000F37EE
		public void OnButtonPress(PlayerControl.Hand.Button button, bool pressed)
		{
			if (button == this.triggerButton && pressed)
			{
				UnityEvent unityEvent = this.onButtonPress;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}

		// Token: 0x060023D8 RID: 9176 RVA: 0x000F5610 File Offset: 0x000F3810
		public void Lock()
		{
			if (this.invincible)
			{
				Player.invincibility = true;
			}
			if (this.disablePlayerCamera)
			{
				Player.local.head.cam.enabled = false;
			}
			if (Player.local)
			{
				Player.local.handRight.controlHand.OnButtonPressEvent += this.OnButtonPress;
			}
			if (this.disableLocomotion && Player.local && Player.local.locomotion)
			{
				Player.local.locomotion.enabled = false;
			}
			if (this.disableMove)
			{
				Player.TogglePlayerMovement(false);
			}
			if (this.disableTurn)
			{
				Player.TogglePlayerTurn(false);
			}
			if (this.disableJump)
			{
				Player.TogglePlayerJump(false);
			}
			if (this.disableSlowMotion)
			{
				TimeManager.StopSlowMotion();
			}
			if (this.disableOptionMenu && UIPlayerMenu.instance != null)
			{
				UIPlayerMenu.instance.IsOpeningBlocked = true;
			}
			if (this.disableCasting && Player.local && Player.local.creature != null)
			{
				Player.local.creature.mana.casterRight.DisallowCasting(this);
				Player.local.creature.mana.casterRight.DisableSpellWheel(this);
				Player.local.creature.mana.casterLeft.DisallowCasting(this);
				Player.local.creature.mana.casterLeft.DisableSpellWheel(this);
			}
		}

		// Token: 0x060023D9 RID: 9177 RVA: 0x000F5794 File Offset: 0x000F3994
		public void Unlock()
		{
			if (this.invincible)
			{
				Player.invincibility = false;
			}
			if (this.disablePlayerCamera)
			{
				Player.local.head.cam.enabled = true;
			}
			if (Player.local)
			{
				Player.local.handRight.controlHand.OnButtonPressEvent -= this.OnButtonPress;
			}
			if (this.disableLocomotion && Player.local && Player.local.locomotion)
			{
				Player.local.locomotion.enabled = true;
			}
			if (this.disableMove)
			{
				Player.TogglePlayerMovement(true);
			}
			if (this.disableTurn)
			{
				Player.TogglePlayerTurn(true);
			}
			if (this.disableJump)
			{
				Player.TogglePlayerJump(true);
			}
			if (this.disableOptionMenu && UIPlayerMenu.instance != null)
			{
				UIPlayerMenu.instance.IsOpeningBlocked = false;
			}
			if (this.disableCasting && Player.local && Player.local.creature != null)
			{
				Player.local.creature.mana.casterRight.AllowCasting(this);
				Player.local.creature.mana.casterRight.AllowSpellWheel(this);
				Player.local.creature.mana.casterLeft.AllowCasting(this);
				Player.local.creature.mana.casterLeft.AllowSpellWheel(this);
			}
		}

		// Token: 0x040022FF RID: 8959
		[Header("Player lock")]
		[Tooltip("Disables Locomotion/Movement when locked")]
		public bool disableLocomotion = true;

		// Token: 0x04002300 RID: 8960
		[Tooltip("Disables Player-Controlled movement when locked")]
		public bool disableMove = true;

		// Token: 0x04002301 RID: 8961
		[Tooltip("Disables player rotation when locked")]
		public bool disableTurn = true;

		// Token: 0x04002302 RID: 8962
		[Tooltip("Disables jumping when locked")]
		public bool disableJump = true;

		// Token: 0x04002303 RID: 8963
		[Tooltip("Disables Slow-Motion/Hyperfocus when locked")]
		public bool disableSlowMotion = true;

		// Token: 0x04002304 RID: 8964
		[Tooltip("Disables casting when locked")]
		public bool disableCasting = true;

		// Token: 0x04002305 RID: 8965
		[Tooltip("When locked, player is invincible")]
		public bool invincible;

		// Token: 0x04002306 RID: 8966
		[Tooltip("When enabled, player camera is disabled when locked. This can be used for other camera uses, like cutscenes")]
		public bool disablePlayerCamera;

		// Token: 0x04002307 RID: 8967
		[Tooltip("Disables the Option Menu when locked")]
		public bool disableOptionMenu;

		// Token: 0x04002308 RID: 8968
		[Header("Button press event")]
		[Tooltip("Will play the On Button Press () event if the \"Trigger Button\" is pressed.")]
		public PlayerControl.Hand.Button triggerButton;

		// Token: 0x04002309 RID: 8969
		public UnityEvent onButtonPress;
	}
}

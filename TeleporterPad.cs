using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002F4 RID: 756
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/TeleporterPad.html")]
	public class TeleporterPad : MonoBehaviour
	{
		// Token: 0x06002418 RID: 9240 RVA: 0x000F6CAB File Offset: 0x000F4EAB
		private void OnEnable()
		{
			this.activateZone.playerEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.OnPlayerEnterActivationZone));
			this.activateZone.playerExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.OnPlayerExitActivationZone));
		}

		// Token: 0x06002419 RID: 9241 RVA: 0x000F6CE5 File Offset: 0x000F4EE5
		private void OnDisable()
		{
			this.activateZone.playerEnterEvent.RemoveListener(new UnityAction<UnityEngine.Object>(this.OnPlayerEnterActivationZone));
			this.activateZone.playerExitEvent.RemoveListener(new UnityAction<UnityEngine.Object>(this.OnPlayerExitActivationZone));
		}

		// Token: 0x0600241A RID: 9242 RVA: 0x000F6D1F File Offset: 0x000F4F1F
		protected void OnPlayerEnterActivationZone(UnityEngine.Object obj)
		{
			if (!this.teleporting)
			{
				this.startupFxController.Play();
				this.startupCoroutine = this.DelayedAction(this.startupDuration, delegate
				{
					this.BeginTeleport();
				});
			}
		}

		// Token: 0x0600241B RID: 9243 RVA: 0x000F6D52 File Offset: 0x000F4F52
		protected void OnPlayerExitActivationZone(UnityEngine.Object obj)
		{
			if (!this.teleporting)
			{
				base.StopAllCoroutines();
				this.startupFxController.Stop();
			}
		}

		// Token: 0x0600241C RID: 9244 RVA: 0x000F6D70 File Offset: 0x000F4F70
		public void BeginTeleport()
		{
			this.teleporting = true;
			Player.local.locomotion.enabled = false;
			Player.currentCreature.SetPhysicModifier(this, new float?(0f), 1f, -1f);
			Vector3 startPosition = Player.local.transform.position;
			this.lockFxController.Play();
			this.startupFxController.Stop(false);
			Action <>9__1;
			this.teleportingCoroutine = this.ProgressiveAction(this.teleportingDuration, delegate(float t)
			{
				Player.local.creature.currentLocomotion.physicBody.transform.position = Vector3.Lerp(startPosition, Player.local.GetPlayerPositionRelativeToHead(this.levitationTarget.position), this.levitationForceCurve.Evaluate(t));
				if (t == 1f)
				{
					this.teleportFxController.SetIntensity(1f);
					this.teleportFxController.Play();
					MonoBehaviour <>4__this = this;
					float delay = this.teleportingFlashDuration;
					Action delayedAction;
					if ((delayedAction = <>9__1) == null)
					{
						delayedAction = (<>9__1 = delegate()
						{
							this.EndTeleport();
						});
					}
					<>4__this.DelayedAction(delay, delayedAction);
				}
			});
		}

		// Token: 0x0600241D RID: 9245 RVA: 0x000F6E0C File Offset: 0x000F500C
		public void EndTeleport()
		{
			if (this.teleportingCoroutine != null)
			{
				base.StopCoroutine(this.teleportingCoroutine);
			}
			this.teleporting = false;
			Player.local.locomotion.enabled = true;
			Player.currentCreature.RemovePhysicModifier(this);
			this.lockFxController.Stop();
			if (this.fireTeleportEvent)
			{
				UnityEvent unityEvent = this.onTeleport;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}

		// Token: 0x04002366 RID: 9062
		[Header("References")]
		[Tooltip("The teleporter activation zone")]
		public Zone activateZone;

		// Token: 0x04002367 RID: 9063
		[Tooltip("Indicates the point which the player root will levitate to during teleportation.")]
		public Transform levitationTarget;

		// Token: 0x04002368 RID: 9064
		[Tooltip("The FXController that the teleporter pad starts up to")]
		public FxController startupFxController;

		// Token: 0x04002369 RID: 9065
		[Tooltip("The FXController that the teleporter pad plays when it is locked")]
		public FxController lockFxController;

		// Token: 0x0400236A RID: 9066
		[Tooltip("The FXController that the teleporter pad plays when it is about to teleport")]
		public FxController teleportFxController;

		// Token: 0x0400236B RID: 9067
		[Header("Parameters")]
		[Tooltip("Depicts the duration of the teleporter startup")]
		public float startupDuration = 4f;

		// Token: 0x0400236C RID: 9068
		[Tooltip("Depicts the duration of the teleporting period.")]
		public float teleportingDuration = 6f;

		// Token: 0x0400236D RID: 9069
		[Tooltip("Depicts the duration of the flash period of the teleporter.")]
		public float teleportingFlashDuration = 1f;

		// Token: 0x0400236E RID: 9070
		[Tooltip("Depicts the Animation curve of the levitation animation.")]
		public AnimationCurve levitationForceCurve;

		// Token: 0x0400236F RID: 9071
		[Header("Events")]
		[Tooltip("Does the teleporter play an event? Reference an \"Event Load Level\" to load a specific level.")]
		public bool fireTeleportEvent;

		// Token: 0x04002370 RID: 9072
		public UnityEvent onTeleport;

		// Token: 0x04002371 RID: 9073
		protected bool teleporting;

		// Token: 0x04002372 RID: 9074
		protected Coroutine startupCoroutine;

		// Token: 0x04002373 RID: 9075
		protected Coroutine teleportingCoroutine;
	}
}

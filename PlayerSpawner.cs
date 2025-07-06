using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002EA RID: 746
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/PlayerSpawner.html")]
	public class PlayerSpawner : MonoBehaviour
	{
		// Token: 0x060023DB RID: 9179 RVA: 0x000F593A File Offset: 0x000F3B3A
		public List<ValueDropdownItem<string>> GetAllContainerID()
		{
			return Catalog.GetDropdownAllID(Category.Container, "None");
		}

		// Token: 0x060023DC RID: 9180 RVA: 0x000F5947 File Offset: 0x000F3B47
		private void Awake()
		{
			PlayerSpawner.all.Add(this);
		}

		// Token: 0x060023DD RID: 9181 RVA: 0x000F5954 File Offset: 0x000F3B54
		private void OnDestroy()
		{
			PlayerSpawner.all.Remove(this);
		}

		// Token: 0x060023DE RID: 9182 RVA: 0x000F5962 File Offset: 0x000F3B62
		private void OnEnable()
		{
			PlayerSpawner.allActive.Add(this);
		}

		// Token: 0x060023DF RID: 9183 RVA: 0x000F596F File Offset: 0x000F3B6F
		private void OnDisable()
		{
			PlayerSpawner.allActive.Remove(this);
		}

		// Token: 0x060023E0 RID: 9184 RVA: 0x000F597D File Offset: 0x000F3B7D
		public static PlayerSpawner GetAny()
		{
			if (PlayerSpawner.allActive.Count == 0)
			{
				return null;
			}
			return PlayerSpawner.allActive[UnityEngine.Random.Range(0, PlayerSpawner.allActive.Count)];
		}

		// Token: 0x060023E1 RID: 9185 RVA: 0x000F59A8 File Offset: 0x000F3BA8
		public static PlayerSpawner Get(string id, List<PlayerSpawner> spawners = null)
		{
			if (spawners == null)
			{
				spawners = PlayerSpawner.allActive;
			}
			if (spawners.Count == 0)
			{
				return null;
			}
			List<PlayerSpawner> spawnersFound = new List<PlayerSpawner>();
			foreach (PlayerSpawner playerSpawner in spawners)
			{
				if (string.Equals(playerSpawner.id, id, StringComparison.OrdinalIgnoreCase))
				{
					spawnersFound.Add(playerSpawner);
				}
			}
			if (spawnersFound.Count == 0)
			{
				return null;
			}
			if (spawnersFound.Count == 1)
			{
				return spawnersFound[0];
			}
			PlayerSpawner defaultSpawner = spawnersFound[0];
			float val = UnityEngine.Random.value * 100f;
			for (int i = spawnersFound.Count - 1; i >= 0; i--)
			{
				if (spawnersFound[i].spawnWeight == -1)
				{
					spawnersFound[i].spawnWeight = 50;
				}
				if ((float)spawnersFound[i].spawnWeight < val)
				{
					spawnersFound.Remove(spawnersFound[i]);
				}
			}
			if (spawnersFound.Count != 0)
			{
				return spawnersFound[UnityEngine.Random.Range(0, spawnersFound.Count)];
			}
			return defaultSpawner;
		}

		// Token: 0x060023E2 RID: 9186 RVA: 0x000F5AC4 File Offset: 0x000F3CC4
		public void SetCurrent()
		{
			PlayerSpawner.current = this;
		}

		// Token: 0x060023E3 RID: 9187 RVA: 0x000F5ACC File Offset: 0x000F3CCC
		public void Spawn()
		{
			this.SpawnAsync(null);
		}

		// Token: 0x060023E4 RID: 9188 RVA: 0x000F5AD5 File Offset: 0x000F3CD5
		public void SpawnAsync(Action callback = null)
		{
			base.StartCoroutine(this.SpawnCoroutine(callback));
		}

		// Token: 0x060023E5 RID: 9189 RVA: 0x000F5AE5 File Offset: 0x000F3CE5
		public IEnumerator SpawnCoroutine(Action callback = null)
		{
			Action<PlayerSpawner, EventTime> action = PlayerSpawner.onSpawn;
			if (action != null)
			{
				action(this, EventTime.OnStart);
			}
			UnityEvent unityEvent = this.playerPreSpawnEvent;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			PlayerSpawner.current = this;
			Debug.Log("Spawning player at: " + base.gameObject.GetPathFromRoot() + ". " + this.id);
			ExposureSetterZone.ResetPlayerExposure();
			if (Player.local)
			{
				if (Player.local.lastCreature && Player.local.lastCreature.state == Creature.State.Dead)
				{
					Player.local.lastCreature.Despawn();
				}
				Player.local.Teleport(base.transform, false, true);
			}
			else
			{
				Player.Spawn(base.transform);
				if (this.spawnBody && Player.characterData == null)
				{
					PlayerSaveData.LoadCharacterSaves(null).AsSynchronous();
				}
				if (this.spawnBody && (GameManager.forceCalibration || !Player.characterData.calibration.calibrated))
				{
					LoadingCamera.SetState(LoadingCamera.State.Disabled, null);
					yield return PlayerSpawner.CalibratePlayer(GameManager.forceCalibration, false);
				}
			}
			List<string> restoreSpells = new List<string>();
			if (this.spawnBody)
			{
				if (!Player.local.creature)
				{
					PlayerSpawner.<>c__DisplayClass20_0 CS$<>8__locals1 = new PlayerSpawner.<>c__DisplayClass20_0();
					CS$<>8__locals1.playerCreature = null;
					CreatureData creatureData = Catalog.GetData<CreatureData>(Player.characterData.GetCreatureID(), true);
					yield return CreatureData.InstantiateCoroutine(creatureData.prefabLocation, base.transform.position, base.transform.rotation.eulerAngles.y, null, delegate(Creature value)
					{
						CS$<>8__locals1.playerCreature = value;
					});
					if (CS$<>8__locals1.playerCreature)
					{
						while (!CS$<>8__locals1.playerCreature.initialized)
						{
							yield return Yielders.EndOfFrame;
						}
						CS$<>8__locals1.playerCreature.ragdoll.ik.AddLocomotionDeltaPosition(base.transform.position);
						CS$<>8__locals1.playerCreature.ragdoll.ik.AddLocomotionDeltaRotation(base.transform.rotation, base.transform.position);
						string containerId;
						ContainerData playerContainer;
						if (Level.TryGetCurrentLevelOption(LevelOption.PlayerContainerId.Get(), out containerId) && Catalog.TryGetData<ContainerData>(containerId, out playerContainer, true))
						{
							List<ContainerContent> orgInventory = Player.characterData.CloneInventory();
							Player.characterData.ClearInventory(false);
							orgInventory.AddRange(playerContainer.GetClonedContents());
							Player.characterData.inventory.SetPlayerInventory(orgInventory);
							CS$<>8__locals1.playerCreature.Load(creatureData, Player.characterData, false);
							if (TutorialManager.instance != null && TutorialManager.instance.InProgress)
							{
								restoreSpells.AddRange(TutorialManager.instance.UnlockedSpells);
							}
						}
						else
						{
							CS$<>8__locals1.playerCreature.Load(creatureData, Player.characterData, false);
						}
						Player.local.SetCreature(CS$<>8__locals1.playerCreature, false);
						string playerVisibilityDistanceString;
						float playerVisibilityDistance;
						if (Level.current.options != null && Level.current.options.TryGetValue(LevelOption.PlayerVisibilityDistance.Get(), out playerVisibilityDistanceString) && float.TryParse(playerVisibilityDistanceString, out playerVisibilityDistance))
						{
							Player.local.SetVisibilityDistance(playerVisibilityDistance);
						}
						EventManager.InvokeCreatureSpawn(CS$<>8__locals1.playerCreature);
					}
					Player.local.locomotion.enabled = true;
					CS$<>8__locals1 = null;
					creatureData = null;
				}
			}
			else if (Player.local.creature)
			{
				ThunderEntity creature = Player.local.creature;
				Player.local.ReleaseCreature();
				creature.Despawn();
			}
			if (restoreSpells.Count > 0 && Player.local != null && Player.local.creature != null && Player.local.creature.container != null)
			{
				for (int i = 0; i < restoreSpells.Count; i++)
				{
					Player.local.creature.container.AddSpellContent(restoreSpells[i]);
				}
			}
			UnityEvent unityEvent2 = this.playerSpawnEvent;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke();
			}
			Action<PlayerSpawner, EventTime> action2 = PlayerSpawner.onSpawn;
			if (action2 != null)
			{
				action2(this, EventTime.OnEnd);
			}
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		// Token: 0x060023E6 RID: 9190 RVA: 0x000F5AFB File Offset: 0x000F3CFB
		public static IEnumerator CalibratePlayer(bool force = false, bool autoHeight = true)
		{
			if (Player.local != null && Player.local.head != null)
			{
				PlayerSpawner.<>c__DisplayClass21_0 CS$<>8__locals1 = new PlayerSpawner.<>c__DisplayClass21_0();
				CS$<>8__locals1.controllerButton = null;
				string address = Catalog.GetImagePlatformAddress("Bas.Input.{Controller}.Triggers");
				yield return Catalog.LoadAssetCoroutine<Texture>(address, delegate(Texture value)
				{
					CS$<>8__locals1.controllerButton = value;
				}, "Level");
				DisplayMessage.MessageData messageData = new DisplayMessage.MessageData("Default", "Calibration", null, 1, 0f, CS$<>8__locals1.controllerButton, null, false, false, false, true, MessageAnchorType.Head, null, false, 2f, null, true, null);
				DisplayMessage.instance.ShowMessage(messageData);
				GameManager.forceCalibration = force;
				while (GameManager.forceCalibration || !Player.characterData.calibration.calibrated)
				{
					if ((PlayerControl.handLeft.usePressed && PlayerControl.handRight.usePressed) || autoHeight)
					{
						DisplayMessage.instance.StopMessage();
						if (Player.local.head == null)
						{
							yield break;
						}
						float eyesHeight = Player.local.transform.InverseTransformPoint(Player.local.head.transform.position).y;
						Player.characterData.calibration.height = Morphology.GetHeight(eyesHeight);
						float dismissMessageTime = 2f;
						messageData = new DisplayMessage.MessageData("Default", "CalibrationDone", " ~" + (Player.characterData.calibration.height * 1.05f).ToString("F2") + "m", 1, 0f, null, null, false, true, false, true, MessageAnchorType.Head, null, true, dismissMessageTime, null, true, null);
						DisplayMessage.instance.ShowMessage(messageData);
						GameManager.forceCalibration = false;
						Player.characterData.calibration.calibrated = true;
						Player.characterData.SaveAsync(true);
						yield return Yielders.ForSeconds(dismissMessageTime + 0.5f);
					}
					yield return Yielders.ForSeconds(1f);
				}
				CS$<>8__locals1 = null;
			}
			yield break;
		}

		// Token: 0x0400230A RID: 8970
		public static List<PlayerSpawner> all = new List<PlayerSpawner>();

		// Token: 0x0400230B RID: 8971
		public static List<PlayerSpawner> allActive = new List<PlayerSpawner>();

		// Token: 0x0400230C RID: 8972
		public static PlayerSpawner current;

		// Token: 0x0400230D RID: 8973
		[Tooltip("You can change this in the level settings or code to have unique spawn points, however if you just make a map with no added code or paramters, leave this at \"default\".")]
		public string id = "default";

		// Token: 0x0400230E RID: 8974
		[Tooltip("-1 will use the default spawning chances, spawners with -1 will by default be 50% when used with weighted spawners.")]
		public int spawnWeight = -1;

		// Token: 0x0400230F RID: 8975
		[Tooltip("If enabled, will spawn the player body.")]
		public bool spawnBody = true;

		// Token: 0x04002310 RID: 8976
		public UnityEvent playerPreSpawnEvent;

		// Token: 0x04002311 RID: 8977
		public UnityEvent playerSpawnEvent;

		// Token: 0x04002312 RID: 8978
		public static Action<PlayerSpawner, EventTime> onSpawn;

		// Token: 0x020009D4 RID: 2516
		public enum Type
		{
			// Token: 0x04004613 RID: 17939
			DefaultStart,
			// Token: 0x04004614 RID: 17940
			AltnernateStart,
			// Token: 0x04004615 RID: 17941
			Stage
		}
	}
}

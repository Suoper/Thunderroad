using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ThunderRoad.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020001DD RID: 477
	public class LevelModuleWaveAssault : LevelModule
	{
		// Token: 0x0600156A RID: 5482 RVA: 0x00095337 File Offset: 0x00093537
		public override IEnumerator OnLoadCoroutine()
		{
			yield return Catalog.LoadAssetCoroutine<GameObject>(this.rewardPillarAddress, new Action<GameObject>(this.OnPillarSpawn), "LevelModuleWaveAssault");
			this.SetUpScene();
			if (WaveSpawner.instances.Count > 0)
			{
				this.waveSpawner = WaveSpawner.instances[0];
				for (int i = 0; i < WaveSpawner.instances.Count; i++)
				{
					WaveSpawner.instances[i].beginWaveOnStart = false;
				}
				this.waveSpawner.referenceType = WaveSpawner.ReferenceType.EnemyConfig;
				this.waveSpawner.OnWaveWinEvent.AddListener(new UnityAction(this.OnWaveEnded));
				this.SetUpWaveIds();
				EventManager.onPossess += this.OnPossessionEvent;
				yield break;
			}
			Debug.LogError("No wave spawner available for survival module!");
			yield break;
		}

		// Token: 0x0600156B RID: 5483 RVA: 0x00095348 File Offset: 0x00093548
		private void SetUpScene()
		{
			UIWaveSpawner[] array = UnityEngine.Object.FindObjectsOfType<UIWaveSpawner>();
			for (int j = 0; j < array.Length; j++)
			{
				array[j].gameObject.SetActive(false);
			}
			Transform pillarTransform = null;
			foreach (Level.CustomReference customReference in this.level.customReferences)
			{
				if (customReference.name == "WaveSelector")
				{
					foreach (Transform transform in customReference.transforms)
					{
						if (transform)
						{
							transform.gameObject.SetActive(false);
						}
					}
				}
				if (customReference.name == "Rack")
				{
					foreach (Transform transform2 in customReference.transforms)
					{
						if (transform2)
						{
							transform2.gameObject.SetActive(false);
						}
					}
				}
				if (customReference.name == "WeaponSelector")
				{
					foreach (Transform transform3 in customReference.transforms)
					{
						if (transform3)
						{
							transform3.gameObject.SetActive(false);
						}
					}
				}
				if (customReference.name == "RewardSpawnPosition")
				{
					pillarTransform = customReference.transforms[0];
				}
			}
			if (pillarTransform != null)
			{
				this.rewardPillar = UnityEngine.Object.Instantiate<ArenaPillar>(this.rewardPillarPrefab, pillarTransform.position, pillarTransform.rotation, pillarTransform);
			}
			for (int i = Item.allActive.Count - 1; i >= 0; i--)
			{
				Item activeItem = Item.allActive[i];
				if (activeItem == null)
				{
					Debug.LogWarning("There is a null item in the active item list. This should not happen, removing it.");
					Item.allActive.RemoveAt(i);
				}
				else if (activeItem.data.type == ItemData.Type.Weapon || activeItem.data.type == ItemData.Type.Shield || activeItem.data.type == ItemData.Type.Quiver || activeItem.data.type == ItemData.Type.Potion || activeItem.data.type == ItemData.Type.Food)
				{
					activeItem.Despawn();
				}
			}
		}

		// Token: 0x0600156C RID: 5484 RVA: 0x00095618 File Offset: 0x00093818
		private void OnPillarSpawn(GameObject obj)
		{
			if (obj != null)
			{
				this.rewardPillarPrefab = obj.GetComponent<ArenaPillar>();
			}
		}

		// Token: 0x0600156D RID: 5485 RVA: 0x00095630 File Offset: 0x00093830
		public override void OnUnload()
		{
			LoadingCamera.onStateChange = (Action<LoadingCamera.State>)Delegate.Remove(LoadingCamera.onStateChange, new Action<LoadingCamera.State>(this.OnLoadingCameraStateChange));
			this.waveSpawner.OnWaveWinEvent.RemoveListener(new UnityAction(this.OnWaveEnded));
			EventManager.onPossess -= this.OnPossessionEvent;
			if (this.rewardPillar != null)
			{
				UnityEngine.Object.Destroy(this.rewardPillar);
				this.rewardPillar = null;
			}
			if (this.rewardPillarPrefab != null)
			{
				Catalog.ReleaseAsset<ArenaPillar>(this.rewardPillarPrefab);
			}
			if (this.reward != null)
			{
				this.reward.OnDespawnEvent -= this.OnRewardDeSpawn;
			}
			if (Player.currentCreature != null)
			{
				Player.currentCreature.handLeft.OnGrabEvent -= this.OnItemGrabbed;
				Player.currentCreature.handRight.OnGrabEvent -= this.OnItemGrabbed;
				Player.currentCreature.handLeft.OnControlPoseChangeEvent -= this.OnControlPoseChanged;
				Player.currentCreature.handRight.OnControlPoseChangeEvent -= this.OnControlPoseChanged;
			}
		}

		// Token: 0x0600156E RID: 5486 RVA: 0x00095764 File Offset: 0x00093964
		protected virtual void OnPossessionEvent(Creature creature, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				Player local = Player.local;
				if ((local != null) ? local.creature : null)
				{
					if (LoadingCamera.state != LoadingCamera.State.Disabled)
					{
						LoadingCamera.onStateChange = (Action<LoadingCamera.State>)Delegate.Combine(LoadingCamera.onStateChange, new Action<LoadingCamera.State>(this.OnLoadingCameraStateChange));
						return;
					}
					this.WaitForStart();
				}
			}
		}

		// Token: 0x0600156F RID: 5487 RVA: 0x000957BB File Offset: 0x000939BB
		private void OnLoadingCameraStateChange(LoadingCamera.State state)
		{
			if (state == LoadingCamera.State.Disabled)
			{
				LoadingCamera.onStateChange = (Action<LoadingCamera.State>)Delegate.Remove(LoadingCamera.onStateChange, new Action<LoadingCamera.State>(this.OnLoadingCameraStateChange));
				this.WaitForStart();
			}
		}

		// Token: 0x06001570 RID: 5488 RVA: 0x000957E8 File Offset: 0x000939E8
		protected void OnWaveEnded()
		{
			Player local = Player.local;
			bool flag;
			if (local == null)
			{
				flag = false;
			}
			else
			{
				Creature creature = local.creature;
				bool? flag2 = (creature != null) ? new bool?(creature.isKilled) : null;
				bool flag3 = false;
				flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
			}
			if (flag)
			{
				if (this.waveIndex < this.length)
				{
					this.StartWave();
					return;
				}
				this.OnWin();
			}
		}

		// Token: 0x06001571 RID: 5489 RVA: 0x00095854 File Offset: 0x00093A54
		private void SetUpWaveIds()
		{
			this.length = this.defaultLength;
			string dungeonLengthString;
			if (this.level.options.TryGetValue(LevelOption.DungeonLength.Get(), out dungeonLengthString))
			{
				int.TryParse(dungeonLengthString, out this.length);
				if (this.length <= 0)
				{
					this.length = this.defaultLength;
				}
			}
			this.waveConfig = new WaveSpawner.EnemyConfigType[this.length];
			for (int i = 0; i < this.length; i++)
			{
				this.waveConfig[i] = (WaveSpawner.EnemyConfigType)UnityEngine.Random.Range(8, 12);
			}
			this.waveIndex = 0;
		}

		// Token: 0x06001572 RID: 5490 RVA: 0x000958E4 File Offset: 0x00093AE4
		private void WaitForStart()
		{
			Player.currentCreature.handLeft.OnGrabEvent += this.OnItemGrabbed;
			Player.currentCreature.handRight.OnGrabEvent += this.OnItemGrabbed;
			Player.currentCreature.handLeft.OnControlPoseChangeEvent += this.OnControlPoseChanged;
			Player.currentCreature.handRight.OnControlPoseChangeEvent += this.OnControlPoseChanged;
			DisplayMessage.MessageData messageData = new DisplayMessage.MessageData(this.textFightGroupId, this.textFightId, null, 10, 0f, null, null, false, false, false, false, MessageAnchorType.Head, null, true, 3f, null, true, null);
			DisplayMessage.instance.ShowMessage(messageData);
		}

		// Token: 0x06001573 RID: 5491 RVA: 0x00095993 File Offset: 0x00093B93
		private void OnControlPoseChanged(Side side, RagdollHand.ControlPose previousPose, RagdollHand.ControlPose newPose)
		{
			if (newPose == RagdollHand.ControlPose.Fist)
			{
				this.Start();
			}
		}

		// Token: 0x06001574 RID: 5492 RVA: 0x0009599F File Offset: 0x00093B9F
		private void OnItemGrabbed(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			if (handle.item == null)
			{
				return;
			}
			if (handle.item.data.type == ItemData.Type.Weapon)
			{
				this.Start();
			}
		}

		// Token: 0x06001575 RID: 5493 RVA: 0x000959D0 File Offset: 0x00093BD0
		private void Start()
		{
			Player.currentCreature.handLeft.OnGrabEvent -= this.OnItemGrabbed;
			Player.currentCreature.handRight.OnGrabEvent -= this.OnItemGrabbed;
			Player.currentCreature.handLeft.OnControlPoseChangeEvent -= this.OnControlPoseChanged;
			Player.currentCreature.handRight.OnControlPoseChangeEvent -= this.OnControlPoseChanged;
			this.StartWave();
		}

		// Token: 0x06001576 RID: 5494 RVA: 0x00095A4F File Offset: 0x00093C4F
		private void StartWave()
		{
			this.waveSpawner.enemyConfigType = this.waveConfig[this.waveIndex];
			Level.current.StartCoroutine(this.StartWaveCoroutine());
		}

		// Token: 0x06001577 RID: 5495 RVA: 0x00095A7A File Offset: 0x00093C7A
		private IEnumerator StartWaveCoroutine()
		{
			DisplayMessage.instance.ShowMessage(new DisplayMessage.MessageData(string.Format("{0}: {1}", LocalizationManager.Instance.GetLocalizedString(this.textFightGroupId, this.textWaveId, false), this.waveIndex + 1), 10, 0f, null, null, false, false, false, false, MessageAnchorType.Head, null, true, 2f, null, true, null));
			yield return Yielders.ForRealSeconds(1.5f);
			this.waveIndex++;
			this.waveSpawner.StartWave(null);
			yield break;
		}

		// Token: 0x06001578 RID: 5496 RVA: 0x00095A8C File Offset: 0x00093C8C
		private void OnWin()
		{
			Level.current.state = Level.State.Success;
			if (this.pillarZone != null && !this.pillarZone.Value.Contains(Player.local.creature.transform.position))
			{
				this.rewardPillar.TpPlayer(2f, new Action(this.SpawnReward));
				return;
			}
			this.SpawnReward();
		}

		// Token: 0x06001579 RID: 5497 RVA: 0x00095AFE File Offset: 0x00093CFE
		private IEnumerator ReturnHome()
		{
			if (this.returnHomeFadeInDuration > 0f)
			{
				CameraEffects.DoFadeEffect(true, this.returnHomeFadeInDuration);
				for (float timer = 0f; timer < this.returnHomeFadeInDuration; timer += Time.unscaledDeltaTime)
				{
					yield return null;
				}
			}
			LevelInstance levelInstance;
			if (LevelInstancesModule.TryGetCurrentLevelInstance(out levelInstance))
			{
				EventManager.InvokeDungeonSuccess(levelInstance);
			}
			LevelManager.LoadLevel(Player.characterData.mode.data.levelHome, Player.characterData.mode.data.levelHomeModeName, null, LoadingCamera.State.Enabled);
			yield break;
		}

		// Token: 0x0600157A RID: 5498 RVA: 0x00095B10 File Offset: 0x00093D10
		protected virtual void SpawnReward()
		{
			this.rewardPillar.ShowPillar();
			string lootConfigId;
			LootConfigData lootConfigData;
			if (Level.TryGetCurrentLevelOption(LevelOption.LootConfig.Get(), out lootConfigId) && Catalog.TryGetData<LootConfigData>(lootConfigId, out lootConfigData, true))
			{
				LootTableBase rewardLootTable = lootConfigData.rewardLootTable;
				if (rewardLootTable != null)
				{
					ItemData rewardData = rewardLootTable.PickOne(0, 0, null);
					if (rewardData == null)
					{
						Debug.LogError("No reward item found in loot table " + lootConfigId + "!");
						return;
					}
					this.rewardPillar.SpawnItem(rewardData, new Action<Item>(this.OnRewardSpawn));
				}
				else
				{
					Debug.LogError("No reward loot table found!");
				}
			}
			else
			{
				Debug.LogError("No loot config found! Cannot spawn reward!");
			}
			DisplayMessage.MessageData messageData = new DisplayMessage.MessageData(this.textReturnHomeGroupId, this.textReturnHomeId, null, 10, 0f, null, null, false, false, false, false, MessageAnchorType.Head, null, true, 5f, null, true, null);
			DisplayMessage.instance.ShowMessage(messageData);
		}

		// Token: 0x0600157B RID: 5499 RVA: 0x00095BD9 File Offset: 0x00093DD9
		private void OnRewardSpawn(Item obj)
		{
			this.reward = obj;
			this.reward.OnDespawnEvent += this.OnRewardDeSpawn;
		}

		// Token: 0x0600157C RID: 5500 RVA: 0x00095BF9 File Offset: 0x00093DF9
		private void OnRewardDeSpawn(EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				return;
			}
			this.level.StartCoroutine(this.ReturnHome());
		}

		// Token: 0x0600157D RID: 5501 RVA: 0x00095C11 File Offset: 0x00093E11
		public List<ValueDropdownItem<string>> GetAllTextGroupID()
		{
			return Catalog.GetTextData().GetDropdownAllTextGroups();
		}

		// Token: 0x0600157E RID: 5502 RVA: 0x00095C1D File Offset: 0x00093E1D
		public List<ValueDropdownItem<string>> GetAllTextFightGroupId()
		{
			return Catalog.GetTextData().GetDropdownAllTexts(this.textFightGroupId);
		}

		// Token: 0x0600157F RID: 5503 RVA: 0x00095C2F File Offset: 0x00093E2F
		public List<ValueDropdownItem<string>> GetAllTextReturnHomeGroupId()
		{
			return Catalog.GetTextData().GetDropdownAllTexts(this.textReturnHomeGroupId);
		}

		// Token: 0x0400155A RID: 5466
		public string rewardPillarAddress;

		// Token: 0x0400155B RID: 5467
		public int defaultLength = 3;

		// Token: 0x0400155C RID: 5468
		public string textFightGroupId;

		// Token: 0x0400155D RID: 5469
		public string textFightId;

		// Token: 0x0400155E RID: 5470
		public string textWaveId;

		// Token: 0x0400155F RID: 5471
		public string textReturnHomeGroupId;

		// Token: 0x04001560 RID: 5472
		public string textReturnHomeId;

		// Token: 0x04001561 RID: 5473
		public Bounds? pillarZone;

		// Token: 0x04001562 RID: 5474
		public float returnHomeFadeInDuration = 2f;

		// Token: 0x04001563 RID: 5475
		private WaveSpawner waveSpawner;

		// Token: 0x04001564 RID: 5476
		private WaveSpawner.EnemyConfigType[] waveConfig;

		// Token: 0x04001565 RID: 5477
		private int length;

		// Token: 0x04001566 RID: 5478
		private int waveIndex;

		// Token: 0x04001567 RID: 5479
		private ArenaPillar rewardPillarPrefab;

		// Token: 0x04001568 RID: 5480
		private ArenaPillar rewardPillar;

		// Token: 0x04001569 RID: 5481
		private Item reward;
	}
}

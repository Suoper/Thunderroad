using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002DB RID: 731
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/Level.html")]
	public class Level : ThunderBehaviour
	{
		// Token: 0x1700022A RID: 554
		// (get) Token: 0x06002337 RID: 9015 RVA: 0x000F12F0 File Offset: 0x000EF4F0
		public static bool IsDungeon
		{
			get
			{
				return Level.current != null && AreaManager.Instance != null;
			}
		}

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x06002338 RID: 9016 RVA: 0x000F130C File Offset: 0x000EF50C
		// (set) Token: 0x06002339 RID: 9017 RVA: 0x000F1313 File Offset: 0x000EF513
		public static int seed
		{
			get
			{
				return Level._seed;
			}
			set
			{
				Level._seed = value;
				UnityEngine.Random.InitState(Level._seed);
			}
		}

		// Token: 0x0600233A RID: 9018 RVA: 0x000F1328 File Offset: 0x000EF528
		public static void CheckLightMapMode()
		{
			Debug.Log("Lightmap mode: " + LightmapSettings.lightmapsMode.ToString());
		}

		// Token: 0x0600233B RID: 9019 RVA: 0x000F1358 File Offset: 0x000EF558
		public static void ChangeLightMapMode(LightmapsMode lightmapsMode)
		{
			LightmapSettings.lightmapsMode = lightmapsMode;
			Debug.Log("Lightmap mode set to: " + LightmapSettings.lightmapsMode.ToString());
		}

		// Token: 0x0600233C RID: 9020 RVA: 0x000F138D File Offset: 0x000EF58D
		public static void TetrahedralizeLightProbes()
		{
			LightProbes.Tetrahedralize();
		}

		// Token: 0x0600233D RID: 9021 RVA: 0x000F1394 File Offset: 0x000EF594
		public static void GenerateNewSeed()
		{
			Level.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}

		// Token: 0x0600233E RID: 9022 RVA: 0x000F13AC File Offset: 0x000EF5AC
		protected virtual void Awake()
		{
			if (base.gameObject.scene.name.ToLower() == "master")
			{
				Level.master = this;
				return;
			}
			Level.current = this;
			Level.GenerateNewSeed();
			this.originalFogColor = RenderSettings.fogColor;
			this.originalShadowColor = RenderSettings.subtractiveShadowColor;
			if (Level.master != this)
			{
				EventManager.onCreatureKill += this.OnCreatureKilled;
			}
		}

		// Token: 0x0600233F RID: 9023 RVA: 0x000F1423 File Offset: 0x000EF623
		private void OnDestroy()
		{
			if (Level.master != this)
			{
				EventManager.onCreatureKill -= this.OnCreatureKilled;
			}
		}

		// Token: 0x1400011A RID: 282
		// (add) Token: 0x06002340 RID: 9024 RVA: 0x000F1444 File Offset: 0x000EF644
		// (remove) Token: 0x06002341 RID: 9025 RVA: 0x000F147C File Offset: 0x000EF67C
		public event Level.LevelLoadedEvent OnLevelEvent;

		// Token: 0x06002342 RID: 9026 RVA: 0x000F14B1 File Offset: 0x000EF6B1
		public static bool TryGetCurrentLevelOption(LevelOption option, out string value)
		{
			return Level.TryGetCurrentLevelOption(option.ToString(), out value);
		}

		// Token: 0x06002343 RID: 9027 RVA: 0x000F14C8 File Offset: 0x000EF6C8
		public static bool TryGetCurrentLevelOption(string option, out string value)
		{
			value = null;
			if (option == null)
			{
				Debug.LogError("Level option is null. Cannot get value");
				return false;
			}
			value = null;
			return Level.current != null && Level.current.options != null && Level.current.options.TryGetValue(option, out value);
		}

		// Token: 0x06002344 RID: 9028 RVA: 0x000F1516 File Offset: 0x000EF716
		public virtual IEnumerator OnLevelLoadCoroutine(LevelData levelData, LevelData.Mode levelMode = null, Dictionary<string, string> levelOptions = null)
		{
			Level.<>c__DisplayClass35_0 CS$<>8__locals1 = new Level.<>c__DisplayClass35_0();
			float startTime = Time.realtimeSinceStartup;
			int percent = 50;
			yield return LoadingCamera.SetPercentageYield(percent);
			int percentInc = 8;
			this.data = levelData;
			CS$<>8__locals1.task = levelMode.CloneJsonAsync<LevelData.Mode>();
			yield return new WaitUntil(() => CS$<>8__locals1.task.IsCompleted);
			this.mode = CS$<>8__locals1.task.Result;
			this.mode = (CS$<>8__locals1.task.Result ?? this.data.GetMode(null));
			if (levelOptions == null)
			{
				levelOptions = new Dictionary<string, string>();
			}
			this.options = levelOptions;
			this.currentPlayerDeathAction = this.mode.playerDeathAction;
			string seedOptString;
			int seedOpt;
			if (this.options.TryGetValue(LevelOption.Seed.Get(), out seedOptString) && int.TryParse(seedOptString, out seedOpt) && seedOpt != 0)
			{
				Level.seed = seedOpt;
			}
			CS$<>8__locals1.terminate = false;
			using (List<LevelModule>.Enumerator enumerator = this.mode.modules.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Level.<>c__DisplayClass35_1 CS$<>8__locals2 = new Level.<>c__DisplayClass35_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.levelModeModule = enumerator.Current;
					CS$<>8__locals2.levelModeModule.level = this;
					yield return CS$<>8__locals2.levelModeModule.OnLoadCoroutine().WrapSafely(delegate(Exception error)
					{
						CS$<>8__locals2.CS$<>8__locals1.terminate = true;
						CS$<>8__locals2.levelModeModule.OnErrorThrown(error);
						LoadingCamera.SetState(LoadingCamera.State.Error, error.ToString());
					});
					if (CS$<>8__locals2.CS$<>8__locals1.terminate && Common.GetQualityLevel(false) == QualityLevel.Android)
					{
						yield break;
					}
					if (CS$<>8__locals2.CS$<>8__locals1.terminate)
					{
						throw new Exception("Error thrown in LevelModule");
					}
					CS$<>8__locals2 = null;
				}
			}
			List<LevelModule>.Enumerator enumerator = default(List<LevelModule>.Enumerator);
			yield return LoadingCamera.SetPercentageYield(percent += percentInc);
			if (levelData.id.ToLower() == "master")
			{
				this.loaded = true;
				if (this.OnLevelEvent != null)
				{
					this.OnLevelEvent();
				}
				yield break;
			}
			Level.TetrahedralizeLightProbes();
			yield return LoadingCamera.SetPercentageYield(percent += percentInc);
			if (this.spawnPlayer)
			{
				PlayerSpawner playerSpawner = PlayerSpawner.Get(this.playerSpawnerId, null);
				string customPlayerSpawnerId;
				if (this.options != null && this.options.TryGetValue(LevelOption.PlayerSpawnerId.Get(), out customPlayerSpawnerId))
				{
					Debug.Log("Loading level " + this.data.id + " with custom level option PlayerSpawnerId: " + customPlayerSpawnerId);
					PlayerSpawner customPlayerSpawner = PlayerSpawner.Get(customPlayerSpawnerId, null);
					if (customPlayerSpawner)
					{
						playerSpawner = customPlayerSpawner;
					}
					else
					{
						Debug.LogError("Could not find PlayerSpawner with ID: " + customPlayerSpawnerId);
					}
				}
				if (playerSpawner == null)
				{
					Debug.LogError("Cannot spawn player on level " + this.data.id + " , scene have no PlayerSpawner component with ID: " + this.playerSpawnerId);
					LoadingCamera.SetState(LoadingCamera.State.Error, "Cannot spawn player in level " + this.data.id);
					yield break;
				}
				yield return playerSpawner.SpawnCoroutine(null);
			}
			else
			{
				CameraEffects.DoFadeEffect(false, 0f);
			}
			yield return LoadingCamera.SetPercentageYield(percent += percentInc);
			using (List<LevelModule>.Enumerator enumerator = this.mode.modules.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Level.<>c__DisplayClass35_2 CS$<>8__locals3 = new Level.<>c__DisplayClass35_2();
					CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals1;
					CS$<>8__locals3.levelModeModule = enumerator.Current;
					yield return CS$<>8__locals3.levelModeModule.OnPlayerSpawnCoroutine().WrapSafely(delegate(Exception error)
					{
						CS$<>8__locals3.CS$<>8__locals2.terminate = true;
						CS$<>8__locals3.levelModeModule.OnErrorThrown(error);
						LoadingCamera.SetState(LoadingCamera.State.Error, error.ToString());
						throw error;
					});
					if (CS$<>8__locals3.CS$<>8__locals2.terminate && Common.GetQualityLevel(false) == QualityLevel.Android)
					{
						yield break;
					}
					if (CS$<>8__locals3.CS$<>8__locals2.terminate)
					{
						throw new Exception("Error thrown in LevelModule");
					}
					CS$<>8__locals3 = null;
				}
			}
			enumerator = default(List<LevelModule>.Enumerator);
			yield return LoadingCamera.SetPercentageYield(percent += percentInc);
			GC.Collect();
			yield return LoadingCamera.SetPercentageYield(percent += percentInc);
			if (Common.GetQualityLevel(false) == QualityLevel.Android)
			{
				yield return Yielders.ForSeconds(1f);
			}
			CS$<>8__locals1.waiting = new List<CreatureSpawner>();
			CreatureSpawner[] array = UnityEngine.Object.FindObjectsOfType<CreatureSpawner>();
			for (int i = 0; i < array.Length; i++)
			{
				CreatureSpawner creatureSpawner = array[i];
				if (creatureSpawner.spawnOnStart && creatureSpawner.enabled)
				{
					if (creatureSpawner.blockLoad)
					{
						CS$<>8__locals1.waiting.Add(creatureSpawner);
					}
					creatureSpawner.Spawn(delegate()
					{
						if (CS$<>8__locals1.waiting.Contains(creatureSpawner))
						{
							CS$<>8__locals1.waiting.Remove(creatureSpawner);
						}
					}, null);
					yield return null;
				}
			}
			array = null;
			while (CS$<>8__locals1.waiting.Count > 0)
			{
				yield return Yielders.EndOfFrame;
			}
			yield return LoadingCamera.SetPercentageYield(percent += percentInc);
			LoadingCamera.FinishLoading(LoadingCamera.LoadingType.LoadLevel);
			LoadingCamera.SetState(LoadingCamera.State.Disabled, null);
			PointerInputModule.SetUICameraToAllCanvas();
			if (GameModeManager.instance.currentGameMode != null && GameModeManager.instance.currentGameMode.name.Equals("CrystalHunt") && !levelData.id.Contains("Master") && !levelData.id.Contains("MainMenu") && TutorialManager.instance == null)
			{
				base.gameObject.AddComponent<TutorialManager>();
			}
			this.loaded = true;
			this.loadedEvent.Invoke();
			Level.LevelLoadedEvent onLevelEvent = this.OnLevelEvent;
			if (onLevelEvent != null)
			{
				onLevelEvent();
			}
			Debug.Log(string.Format("Level {0} OnLevelLoadCoroutine in: {1:F2} sec", levelData.id, Time.realtimeSinceStartup - startTime));
			yield break;
			yield break;
		}

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x06002345 RID: 9029 RVA: 0x000F153A File Offset: 0x000EF73A
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06002346 RID: 9030 RVA: 0x000F1540 File Offset: 0x000EF740
		protected internal override void ManagedUpdate()
		{
			if (this.loaded)
			{
				int count = this.mode.modules.Count;
				for (int index = 0; index < count; index++)
				{
					this.mode.modules[index].Update();
				}
			}
		}

		// Token: 0x06002347 RID: 9031 RVA: 0x000F1588 File Offset: 0x000EF788
		private void OnCreatureKilled(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd && player)
			{
				if (this.currentPlayerDeathAction == LevelData.Mode.PlayerDeathAction.AskReload)
				{
					base.StartCoroutine(this.DeathSequenceCoroutine("{DeathMessage}", true, 1f, 5f, 3f, null));
					return;
				}
				if (this.currentPlayerDeathAction == LevelData.Mode.PlayerDeathAction.ReloadLevel)
				{
					base.StartCoroutine(this.DeathSequenceCoroutine("{DeathMessage}", false, 1f, 5f, 3f, delegate
					{
						LevelManager.ReloadLevel();
					}));
					return;
				}
				if (this.currentPlayerDeathAction == LevelData.Mode.PlayerDeathAction.LoadHome)
				{
					base.StartCoroutine(this.DeathSequenceCoroutine("{DeathMessage}", false, 1f, 5f, 3f, delegate
					{
						LevelManager.LoadLevel(Player.characterData.mode.data.levelHome, Player.characterData.mode.data.levelHomeModeName, null, LoadingCamera.State.Enabled);
					}));
					return;
				}
				if (this.currentPlayerDeathAction == LevelData.Mode.PlayerDeathAction.PermaDeath)
				{
					base.StartCoroutine(this.DeathSequenceCoroutine("{PermaDeathMessage}", false, 1f, 10f, 5f, delegate
					{
						LevelManager.LoadLevel(ThunderRoadSettings.current.game.mainMenuLevelId, null, null, LoadingCamera.State.Enabled);
					}));
				}
			}
		}

		// Token: 0x06002348 RID: 9032 RVA: 0x000F16B5 File Offset: 0x000EF8B5
		public IEnumerator DeathSequenceCoroutine(string messageText, bool askReload, float slowTimeduration = 1f, float normalTimeduration = 5f, float endingFadeDuration = 3f, Action endCallback = null)
		{
			if (UIPlayerMenu.instance != null)
			{
				UIPlayerMenu.instance.IsOpeningBlocked = true;
				UIPlayerMenu.instance.Close();
			}
			if (Catalog.gameData.useDynamicMusic && ThunderBehaviourSingleton<MusicManager>.HasInstance)
			{
				ThunderBehaviourSingleton<MusicManager>.Instance.Volume = 1f;
			}
			WaveSpawner waveSpawner;
			if (WaveSpawner.TryGetRunningInstance(out waveSpawner))
			{
				waveSpawner.CancelWave();
			}
			Player.local.locomotion.enabled = false;
			EffectData data;
			if (Catalog.TryGetData<EffectData>(Catalog.gameData.deathEffectId, out data, true))
			{
				TimeManager.SetSlowMotion(true, Catalog.gameData.deathSlowMoRatio, Catalog.gameData.deathSlowMoEnterCurve, data, true, true);
			}
			else
			{
				TimeManager.SetSlowMotion(true, Catalog.gameData.deathSlowMoRatio, Catalog.gameData.deathSlowMoEnterCurve, null, true, true);
			}
			CameraEffects.SetSepia(Level.current, 1f);
			if (askReload)
			{
				yield return Catalog.InstantiateCoroutine<UIEndGameMenu>(Catalog.gameData.endGameMenuPrefabAddress, null, "UIEndGameMenu");
				UIEndGameMenu.instance.UpdateDefeatText(messageText);
				UIEndGameMenu.instance.Show(true);
			}
			else
			{
				string localizedMessage = LocalizationManager.Instance.TryGetLocalization("Default", messageText, null, false);
				DisplayMessage.instance.ShowMessage(new DisplayMessage.MessageData(localizedMessage, 10, 0f, null, null, false, false, false, true, MessageAnchorType.HandLeft, null, false, 2f, null, true, null));
			}
			yield return new WaitForSeconds(slowTimeduration);
			TimeManager.SetSlowMotion(false, Catalog.gameData.deathSlowMoRatio, Catalog.gameData.deathSlowMoExitCurve, null, true, true);
			yield return new WaitForSeconds(normalTimeduration);
			if (!askReload)
			{
				CameraEffects.DoFadeEffect(true, endingFadeDuration);
				yield return new WaitForSeconds(endingFadeDuration);
			}
			if (endCallback != null)
			{
				endCallback();
			}
			yield break;
		}

		// Token: 0x06002349 RID: 9033 RVA: 0x000F16EC File Offset: 0x000EF8EC
		public virtual void OnLevelUnload()
		{
			base.StopAllCoroutines();
			foreach (LevelModule levelModule in this.mode.modules)
			{
				levelModule.OnUnload();
			}
			if (this.currentLightingPreset != null)
			{
				this.currentLightingPreset.OnReleasePreset();
				UnityEngine.Object.Destroy(this.currentLightingPreset);
				this.currentLightingPreset = null;
			}
		}

		// Token: 0x04002245 RID: 8773
		public static Level current;

		// Token: 0x04002246 RID: 8774
		public static Level master;

		// Token: 0x04002247 RID: 8775
		[Tooltip("When ticked, player will spawn.")]
		public bool spawnPlayer = true;

		// Token: 0x04002248 RID: 8776
		[Tooltip("Container of the player when the player loads in to this level.")]
		public string playerSpawnerId = "default";

		// Token: 0x04002249 RID: 8777
		[NonSerialized]
		public LightingPreset currentLightingPreset;

		// Token: 0x0400224A RID: 8778
		public List<Level.CustomReference> customReferences;

		// Token: 0x0400224B RID: 8779
		[NonSerialized]
		public Level.State state;

		// Token: 0x0400224C RID: 8780
		[NonSerialized]
		public LevelData.Mode.PlayerDeathAction currentPlayerDeathAction;

		// Token: 0x0400224D RID: 8781
		[NonSerialized]
		public bool loaded;

		// Token: 0x0400224E RID: 8782
		private static int _seed;

		// Token: 0x0400224F RID: 8783
		[NonSerialized]
		public Color originalFogColor;

		// Token: 0x04002250 RID: 8784
		[NonSerialized]
		public Color originalShadowColor;

		// Token: 0x04002251 RID: 8785
		public UnityEvent loadedEvent;

		// Token: 0x04002252 RID: 8786
		[NonSerialized]
		public LevelData data;

		// Token: 0x04002253 RID: 8787
		[NonSerialized]
		public LevelData.Mode mode;

		// Token: 0x04002254 RID: 8788
		[NonSerialized]
		public Dictionary<string, string> options;

		// Token: 0x020009BA RID: 2490
		[Serializable]
		public class CustomReference
		{
			// Token: 0x040045AC RID: 17836
			public string name;

			// Token: 0x040045AD RID: 17837
			public List<Transform> transforms;
		}

		// Token: 0x020009BB RID: 2491
		public enum State
		{
			// Token: 0x040045AF RID: 17839
			None,
			// Token: 0x040045B0 RID: 17840
			Failure,
			// Token: 0x040045B1 RID: 17841
			Success
		}

		// Token: 0x020009BC RID: 2492
		// (Invoke) Token: 0x0600444D RID: 17485
		public delegate void LevelLoadedEvent();
	}
}

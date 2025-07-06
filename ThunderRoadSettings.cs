using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace ThunderRoad
{
	// Token: 0x02000374 RID: 884
	public class ThunderRoadSettings : ScriptableObject
	{
		// Token: 0x17000288 RID: 648
		// (get) Token: 0x06002A03 RID: 10755 RVA: 0x0011D39D File Offset: 0x0011B59D
		// (set) Token: 0x06002A04 RID: 10756 RVA: 0x0011D3A4 File Offset: 0x0011B5A4
		public static bool initialized { get; private set; }

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x06002A05 RID: 10757 RVA: 0x0011D3AC File Offset: 0x0011B5AC
		public static ThunderRoadSettings current
		{
			get
			{
				if (!ThunderRoadSettings.initialized)
				{
					ThunderRoadSettings thunderRoadSettings;
					if (Application.isBatchMode)
					{
						thunderRoadSettings = Catalog.EditorLoad<ThunderRoadSettings>("ThunderRoad.Settings", Catalog.PlatformSelection.Auto);
					}
					else
					{
						thunderRoadSettings = Addressables.LoadAssetAsync<ThunderRoadSettings>("ThunderRoad.Settings").WaitForCompletion();
					}
					if (thunderRoadSettings)
					{
						thunderRoadSettings.Init();
					}
					else
					{
						Debug.LogError("Could not load ThunderRoad settings from address: ThunderRoad.Settings");
						Application.Quit();
					}
				}
				return ThunderRoadSettings._current;
			}
		}

		// Token: 0x06002A06 RID: 10758 RVA: 0x0011D410 File Offset: 0x0011B610
		private void Init()
		{
			ThunderRoadSettings._current = this;
			ThunderRoadSettings.audioMixerSnapshotDefault = this.audioMixer.FindSnapshot("Default");
			ThunderRoadSettings.audioMixerSnapshotMute = this.audioMixer.FindSnapshot("Mute");
			ThunderRoadSettings.audioMixerSnapshotSlowmo = this.audioMixer.FindSnapshot("Slowmo");
			ThunderRoadSettings.audioMixerSnapshotUnderwater = this.audioMixer.FindSnapshot("Underwater");
			ThunderRoadSettings.audioMixerGroups = new AudioMixerGroup[Enum.GetValues(typeof(AudioMixerName)).Length];
			for (int i = 0; i < Enum.GetNames(typeof(AudioMixerName)).Length; i++)
			{
				string audioMixerName = Enum.GetNames(typeof(AudioMixerName))[i];
				foreach (AudioMixerGroup group in this.audioMixer.FindMatchingGroups(audioMixerName))
				{
					if (group.name == audioMixerName)
					{
						ThunderRoadSettings.audioMixerGroups[i] = group;
					}
				}
			}
			if (Application.isPlaying)
			{
				if (!ThunderRoadSettings._current.game)
				{
					Debug.LogError("No game settings set on ThunderRoad Settings");
					Application.Quit();
				}
				if (!ThunderRoadSettings._current.game.playerPrefab)
				{
					Debug.LogError("Player prefab is not set, unable to start game");
					Application.Quit();
				}
				Spectator spectator = UnityEngine.Object.Instantiate<Spectator>(ThunderRoadSettings._current.game.spectatorPrefab);
				if (spectator)
				{
					spectator.StartCoroutine((spectator != null) ? spectator.DebugConsoleCoroutine() : null);
				}
				else
				{
					Debug.LogError("Could not spawn spectator prefab");
				}
				if (this.build.betaProof)
				{
					spectator.proof.SetActive(true);
				}
			}
			ThunderRoadSettings.initialized = true;
			string str = "ThunderRoad settings loaded, game settings: ";
			GameSettings gameSettings = ThunderRoadSettings._current.game;
			string str2 = (gameSettings != null) ? gameSettings.name : null;
			string str3 = ", build settings: ";
			BuildSettings buildSettings = ThunderRoadSettings._current.build;
			Debug.Log(str + str2 + str3 + ((buildSettings != null) ? buildSettings.name : null));
		}

		// Token: 0x06002A07 RID: 10759 RVA: 0x0011D5EB File Offset: 0x0011B7EB
		public static AudioMixerGroup GetAudioMixerGroup(AudioMixerName audioMixerName)
		{
			if (ThunderRoadSettings.current)
			{
				return ThunderRoadSettings.audioMixerGroups[(int)audioMixerName];
			}
			return null;
		}

		// Token: 0x040027C7 RID: 10183
		private const string address = "ThunderRoad.Settings";

		// Token: 0x040027C9 RID: 10185
		private static ThunderRoadSettings _current;

		// Token: 0x040027CA RID: 10186
		public string addressableEditorPath = "BuildStaging/AddressableAssets";

		// Token: 0x040027CB RID: 10187
		public string catalogsEditorPath = "BuildStaging/Catalogs";

		// Token: 0x040027CC RID: 10188
		public string areaSceneAddress = "Level.Areas";

		// Token: 0x040027CD RID: 10189
		public bool overrideData = true;

		// Token: 0x040027CE RID: 10190
		public LayerMask groundLayer = 1;

		// Token: 0x040027CF RID: 10191
		public GameSettings game;

		// Token: 0x040027D0 RID: 10192
		public BuildSettings build;

		// Token: 0x040027D1 RID: 10193
		public AudioMixer audioMixer;

		// Token: 0x040027D2 RID: 10194
		public static AudioMixerSnapshot audioMixerSnapshotDefault;

		// Token: 0x040027D3 RID: 10195
		public static AudioMixerSnapshot audioMixerSnapshotMute;

		// Token: 0x040027D4 RID: 10196
		public static AudioMixerSnapshot audioMixerSnapshotSlowmo;

		// Token: 0x040027D5 RID: 10197
		public static AudioMixerSnapshot audioMixerSnapshotUnderwater;

		// Token: 0x040027D6 RID: 10198
		public static AudioMixerGroup[] audioMixerGroups;

		// Token: 0x040027D7 RID: 10199
		public Material mirrorMaterial;

		// Token: 0x040027D8 RID: 10200
		public Material mirrorOutlineMaterial;

		// Token: 0x040027D9 RID: 10201
		public bool pauseOnVRPresence = true;
	}
}

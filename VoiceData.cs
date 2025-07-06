using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ThunderRoad.Pools;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000249 RID: 585
	[Serializable]
	public class VoiceData : CatalogData
	{
		// Token: 0x0600187F RID: 6271 RVA: 0x000A2248 File Offset: 0x000A0448
		public override IEnumerator LoadAddressableAssetsCoroutine()
		{
			List<IEnumerator> coroutines = new List<IEnumerator>();
			int dialogsCount = this.dialogs.Count;
			for (int i = 0; i < dialogsCount; i++)
			{
				VoiceData.Dialog dialog = this.dialogs[i];
				coroutines.Add(dialog.LoadAddressableAssetsCoroutine(this));
			}
			yield return coroutines.YieldParallel();
			yield break;
		}

		// Token: 0x06001880 RID: 6272 RVA: 0x000A2258 File Offset: 0x000A0458
		public override void ReleaseAddressableAssets()
		{
			int dialogsCount = this.dialogs.Count;
			for (int i = 0; i < dialogsCount; i++)
			{
				this.dialogs[i].ReleaseAddressableAssets();
			}
		}

		// Token: 0x06001881 RID: 6273 RVA: 0x000A2290 File Offset: 0x000A0490
		public override void Init()
		{
			base.Init();
			int dialogsCount = this.dialogs.Count;
			for (int i = 0; i < dialogsCount; i++)
			{
				VoiceData.Dialog dialog = this.dialogs[i];
				dialog.hashId = Animator.StringToHash(dialog.id.ToLower());
			}
		}

		// Token: 0x06001882 RID: 6274 RVA: 0x000A22DC File Offset: 0x000A04DC
		public bool TryGetDialog(int dialogHashId, out VoiceData.Dialog dialog)
		{
			int dialogsCount = this.dialogs.Count;
			for (int i = 0; i < dialogsCount; i++)
			{
				VoiceData.Dialog thisDialog = this.dialogs[i];
				if (thisDialog.hashId == dialogHashId)
				{
					dialog = thisDialog;
					return true;
				}
			}
			dialog = null;
			return false;
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06001883 RID: 6275 RVA: 0x000A2320 File Offset: 0x000A0520
		// (set) Token: 0x06001884 RID: 6276 RVA: 0x000A2338 File Offset: 0x000A0538
		public static Transform PoolRoot
		{
			get
			{
				if (!VoiceData._poolRoot)
				{
					VoiceData.UpdatePoolRoot();
				}
				return VoiceData._poolRoot;
			}
			set
			{
				VoiceData._poolRoot = value;
			}
		}

		// Token: 0x06001885 RID: 6277 RVA: 0x000A2340 File Offset: 0x000A0540
		public int GetPooledCount()
		{
			if (Common.GetQualityLevel(false) == QualityLevel.Android)
			{
				if (this.androidPooledCount != 0)
				{
					return this.androidPooledCount;
				}
				return 1;
			}
			else
			{
				if (this.pooledCount != 0)
				{
					return this.pooledCount;
				}
				return 1;
			}
		}

		// Token: 0x06001886 RID: 6278 RVA: 0x000A236C File Offset: 0x000A056C
		public static IEnumerator GeneratePool()
		{
			VoiceData.UpdatePoolRoot();
			if (!VoiceData.pools.IsNullOrEmpty())
			{
				yield break;
			}
			List<CatalogData> voiceDatas = Catalog.GetDataList(Category.Voice);
			int voiceDatasCount = voiceDatas.Count;
			for (int i = 0; i < voiceDatasCount; i++)
			{
				VoiceData voiceData = voiceDatas[i] as VoiceData;
				if (voiceData != null && !voiceData.dialogs.IsNullOrEmpty())
				{
					int dialogsCount = voiceData.dialogs.Count;
					for (int j = 0; j < dialogsCount; j++)
					{
						VoiceData.Dialog dialog = voiceData.dialogs[j];
						AudioContainer audioContainer = dialog.audioContainer;
						if (audioContainer == null)
						{
							Debug.LogError("audioContainer " + dialog.audioGroupAddress + " returns a null audioContainer. Is the address correct?");
						}
						else if (audioContainer.sounds.IsNullOrEmpty())
						{
							Debug.LogError("audioContainer " + dialog.audioGroupAddress + " does not contain any sounds");
							Catalog.ReleaseAsset<AudioContainer>(audioContainer);
						}
						else
						{
							int soundsCount = audioContainer.hashes.Length;
							for (int k = 0; k < soundsCount; k++)
							{
								int hash = audioContainer.hashes[k];
								VoiceData.GetOrCreateAudioPoolManager(audioContainer, dialog.audioGroupAddress, hash);
							}
						}
					}
				}
			}
			Debug.Log(string.Format("VoiceData gameobjects created: {0}", VoiceData.PoolRoot.transform.childCount));
			yield break;
		}

		// Token: 0x06001887 RID: 6279 RVA: 0x000A2374 File Offset: 0x000A0574
		public static IEnumerator DespawnAllOutOfPool()
		{
			VoiceData.UpdatePoolRoot();
			foreach (AudioPoolManager<AudioKey> audioPoolManager in VoiceData.pools.Values)
			{
				audioPoolManager.DespawnOutOfPool();
			}
			yield return null;
			yield break;
		}

		// Token: 0x06001888 RID: 6280 RVA: 0x000A237C File Offset: 0x000A057C
		public static IEnumerator ClearPool()
		{
			VoiceData.UpdatePoolRoot();
			Debug.Log(string.Format("VoiceData Pools have: {0} objects", VoiceData._poolRoot.transform.childCount));
			foreach (AudioPoolManager<AudioKey> audioPoolManager in VoiceData.pools.Values)
			{
				audioPoolManager.Dispose();
				yield return null;
			}
			Dictionary<AudioKey, AudioPoolManager<AudioKey>>.ValueCollection.Enumerator enumerator = default(Dictionary<AudioKey, AudioPoolManager<AudioKey>>.ValueCollection.Enumerator);
			VoiceData.pools.Clear();
			yield break;
			yield break;
		}

		// Token: 0x06001889 RID: 6281 RVA: 0x000A2384 File Offset: 0x000A0584
		private static void UpdatePoolRoot()
		{
			VoiceData._poolRoot = GameManager.poolTransform.Find("VoiceData");
			if (!VoiceData._poolRoot)
			{
				VoiceData._poolRoot = new GameObject("VoiceData").transform;
				VoiceData._poolRoot.SetParent(GameManager.poolTransform, false);
			}
		}

		// Token: 0x0600188A RID: 6282 RVA: 0x000A23D5 File Offset: 0x000A05D5
		public static void Despawn(AudioSourceGameObject<VoiceData.VoiceAudioKey> pooledObject)
		{
			if (pooledObject.isPooled && VoiceData.PoolRoot)
			{
				pooledObject.poolManager.Release(pooledObject);
				return;
			}
			UnityEngine.Object.Destroy(pooledObject.gameObject);
		}

		// Token: 0x0600188B RID: 6283 RVA: 0x000A2403 File Offset: 0x000A0603
		public static AudioPoolManager<AudioKey> GetOrCreateAudioPoolManager(AudioContainer audioContainer, string audioGroupAddress, AudioClip audioClip)
		{
			return VoiceData.GetOrCreateAudioPoolManager(audioContainer, audioGroupAddress, Animator.StringToHash(audioClip.name));
		}

		/// <summary>
		/// This will get or create the AudioPoolManager for a particular VoiceData and audioClip from this VoiceDatas audioContainer
		/// </summary>
		// Token: 0x0600188C RID: 6284 RVA: 0x000A2418 File Offset: 0x000A0618
		public static AudioPoolManager<AudioKey> GetOrCreateAudioPoolManager(AudioContainer audioContainer, string audioGroupAddress, int audioClipHash)
		{
			AudioKey audioKey = new AudioKey("voicedata", audioGroupAddress, audioClipHash);
			AudioPoolManager<AudioKey> poolManager;
			AudioClip audioClip;
			if (!VoiceData.pools.TryGetValue(audioKey, out poolManager) && audioContainer.TryGetAudioClip(audioClipHash, out audioClip))
			{
				poolManager = new AudioPoolManager<AudioKey>(VoiceData.PoolRoot, audioKey, audioClip, PoolType.Stack, GameManager.local.PoolCollectionChecks, audioContainer.maxPoolCountPerClip, 1, true);
				VoiceData.pools.Add(audioKey, poolManager);
			}
			return poolManager;
		}

		// Token: 0x0400179C RID: 6044
		public int pooledCount;

		// Token: 0x0400179D RID: 6045
		public int androidPooledCount;

		// Token: 0x0400179E RID: 6046
		public List<VoiceData.Dialog> dialogs = new List<VoiceData.Dialog>();

		/// <summary>
		/// Key of [audioContainerAddress + clipHash] to a pool of audiosources
		/// </summary>
		// Token: 0x0400179F RID: 6047
		public static Dictionary<AudioKey, AudioPoolManager<AudioKey>> pools = new Dictionary<AudioKey, AudioPoolManager<AudioKey>>();

		// Token: 0x040017A0 RID: 6048
		public static Transform _poolRoot;

		// Token: 0x0200085D RID: 2141
		[Serializable]
		public class Dialog
		{
			// Token: 0x17000502 RID: 1282
			// (get) Token: 0x06003FDE RID: 16350 RVA: 0x00188220 File Offset: 0x00186420
			[JsonIgnore]
			public AudioContainer audioContainer
			{
				get
				{
					if (!(this.advancedAudioContainer != null))
					{
						return this.basicAudioContainer;
					}
					return this.advancedAudioContainer.audioContainer;
				}
			}

			// Token: 0x06003FDF RID: 16351 RVA: 0x00188242 File Offset: 0x00186442
			public virtual void ReleaseAddressableAssets()
			{
				if (this.audioContainer)
				{
					Catalog.ReleaseAsset<AudioContainer>(this.audioContainer);
				}
				this.basicAudioContainer = null;
				this.advancedAudioContainer = null;
			}

			// Token: 0x06003FE0 RID: 16352 RVA: 0x0018826A File Offset: 0x0018646A
			public virtual IEnumerator LoadAddressableAssetsCoroutine(VoiceData voiceData)
			{
				this.rootData = voiceData;
				if (this.audioContainer)
				{
					Catalog.ReleaseAsset<AudioContainer>(this.audioContainer);
				}
				yield return Catalog.LoadAssetCoroutine<ScriptableObject>(this.audioGroupAddress, delegate(ScriptableObject value)
				{
					if (value == null)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"VoiceData ",
							this.rootData.id,
							" dialog ",
							this.id,
							" audioGroupAddress: ",
							this.audioGroupAddress,
							" returns a null audioContainer. Is the address correct?"
						}));
						return;
					}
					this.OnAudioGroupAssetLoaded(value);
				}, this.id);
				yield break;
			}

			// Token: 0x06003FE1 RID: 16353 RVA: 0x00188280 File Offset: 0x00186480
			private void OnAudioGroupAssetLoaded(ScriptableObject scriptableObject)
			{
				AdvancedAudioContainer advancedAudioContainerScriptable = scriptableObject as AdvancedAudioContainer;
				if (advancedAudioContainerScriptable == null)
				{
					AudioContainer audioContainerScriptable = scriptableObject as AudioContainer;
					if (audioContainerScriptable != null)
					{
						this.basicAudioContainer = audioContainerScriptable;
					}
				}
				else
				{
					this.basicAudioContainer = advancedAudioContainerScriptable.audioContainer;
					this.advancedAudioContainer = advancedAudioContainerScriptable;
				}
				if (this.audioContainer == null)
				{
					Debug.LogError("audioContainer " + this.audioGroupAddress + " returns a null audioContainer. Is the address correct?");
					return;
				}
				if (this.audioContainer.sounds.IsNullOrEmpty())
				{
					Debug.LogError("audioContainer " + this.audioGroupAddress + " does not contain any sounds");
					return;
				}
				this.audioContainer.maxPoolCountPerClip = this.rootData.GetPooledCount();
				this.audioContainer.GenerateAudioClipHashes();
			}

			// Token: 0x04004160 RID: 16736
			public string id;

			// Token: 0x04004161 RID: 16737
			[NonSerialized]
			public int hashId;

			// Token: 0x04004162 RID: 16738
			[Tooltip("Defaults to voice. Can be set to another channel if necessary.")]
			public AudioMixerName mixerChannel = AudioMixerName.Voice;

			// Token: 0x04004163 RID: 16739
			public string audioGroupAddress;

			// Token: 0x04004164 RID: 16740
			[NonSerialized]
			public AudioContainer basicAudioContainer;

			// Token: 0x04004165 RID: 16741
			[NonSerialized]
			public AdvancedAudioContainer advancedAudioContainer;

			// Token: 0x04004166 RID: 16742
			[NonSerialized]
			public VoiceData rootData;
		}

		// Token: 0x0200085E RID: 2142
		public readonly struct VoiceAudioKey : IKey<VoiceData.VoiceAudioKey>, IEquatable<VoiceData.VoiceAudioKey>
		{
			// Token: 0x17000503 RID: 1283
			// (get) Token: 0x06003FE4 RID: 16356 RVA: 0x001883AA File Offset: 0x001865AA
			public int AudioClipNameHash { get; }

			// Token: 0x17000504 RID: 1284
			// (get) Token: 0x06003FE5 RID: 16357 RVA: 0x001883B2 File Offset: 0x001865B2
			public string VoiceDataId { get; }

			// Token: 0x17000505 RID: 1285
			// (get) Token: 0x06003FE6 RID: 16358 RVA: 0x001883BA File Offset: 0x001865BA
			public int DialogId { get; }

			// Token: 0x17000506 RID: 1286
			// (get) Token: 0x06003FE7 RID: 16359 RVA: 0x001883C2 File Offset: 0x001865C2
			public string AudioContainerAddress { get; }

			// Token: 0x06003FE8 RID: 16360 RVA: 0x001883CC File Offset: 0x001865CC
			public VoiceAudioKey(string voiceDataId, int dialogHashId, string audioContainerAddress, int audioClipNameHash)
			{
				this.VoiceDataId = voiceDataId;
				this.DialogId = dialogHashId;
				this.AudioContainerAddress = audioContainerAddress;
				this.AudioClipNameHash = audioClipNameHash;
				this.stringKey = string.Format("{0}-{1}-{2}-{3}", new object[]
				{
					this.AudioClipNameHash,
					this.VoiceDataId,
					this.DialogId,
					this.AudioContainerAddress
				});
			}

			// Token: 0x06003FE9 RID: 16361 RVA: 0x0018843C File Offset: 0x0018663C
			public bool Equals(VoiceData.VoiceAudioKey other)
			{
				return this.AudioClipNameHash == other.AudioClipNameHash && this.VoiceDataId == other.VoiceDataId && this.DialogId == other.DialogId && this.AudioContainerAddress == other.AudioContainerAddress;
			}

			// Token: 0x06003FEA RID: 16362 RVA: 0x00188490 File Offset: 0x00186690
			public override bool Equals(object obj)
			{
				if (obj is VoiceData.VoiceAudioKey)
				{
					VoiceData.VoiceAudioKey other = (VoiceData.VoiceAudioKey)obj;
					return this.Equals(other);
				}
				return false;
			}

			// Token: 0x06003FEB RID: 16363 RVA: 0x001884B8 File Offset: 0x001866B8
			public override int GetHashCode()
			{
				return ((this.AudioClipNameHash * 397 ^ this.VoiceDataId.GetHashCode()) * 397 ^ this.DialogId.GetHashCode()) * 397 ^ this.AudioContainerAddress.GetHashCode();
			}

			// Token: 0x06003FEC RID: 16364 RVA: 0x00188504 File Offset: 0x00186704
			public override string ToString()
			{
				return string.Format("AudioClipNameHash: {0}, VoiceDataId: {1}, DialogId: {2}, AudioContainerAddress: {3}", new object[]
				{
					this.AudioClipNameHash,
					this.VoiceDataId,
					this.DialogId,
					this.AudioContainerAddress
				});
			}

			// Token: 0x06003FED RID: 16365 RVA: 0x00188544 File Offset: 0x00186744
			public string GetKeyString()
			{
				return this.stringKey;
			}

			// Token: 0x0400416B RID: 16747
			private readonly string stringKey;
		}
	}
}

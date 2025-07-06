using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ThunderRoadVRKBSharedData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ThunderRoad
{
	// Token: 0x020001D3 RID: 467
	[Serializable]
	public class KeyboardData : CatalogData
	{
		// Token: 0x06001514 RID: 5396 RVA: 0x000937B3 File Offset: 0x000919B3
		public List<ValueDropdownItem<string>> GetAllTextId()
		{
			return Catalog.GetDropdownAllID(Category.Text, "None");
		}

		// Token: 0x06001515 RID: 5397 RVA: 0x000937C0 File Offset: 0x000919C0
		public override IEnumerator LoadAddressableAssetsCoroutine()
		{
			if (!string.IsNullOrEmpty(this.keyboardConfig.KeyProperties.imageAddress))
			{
				yield return Catalog.LoadAssetCoroutine<Sprite>(this.keyboardConfig.KeyProperties.imageAddress, delegate(Sprite sprite)
				{
					this.keyboardConfig.KeyProperties.image = sprite;
				}, this.id);
			}
			foreach (KeyValuePair<string, KeyboardLayerConfiguration> keyboardConfigLayer in this.keyboardConfig.layers)
			{
				using (Dictionary<string, KeyConfiguration>.Enumerator enumerator2 = keyboardConfigLayer.Value.keys.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						KeyValuePair<string, KeyConfiguration> key = enumerator2.Current;
						if (!string.IsNullOrEmpty(key.Value.overrideProperties.imageAddress))
						{
							yield return Catalog.LoadAssetCoroutine<Sprite>(key.Value.overrideProperties.imageAddress, delegate(Sprite sprite)
							{
								key.Value.overrideProperties.image = sprite;
							}, this.id);
						}
					}
				}
				Dictionary<string, KeyConfiguration>.Enumerator enumerator2 = default(Dictionary<string, KeyConfiguration>.Enumerator);
			}
			Dictionary<string, KeyboardLayerConfiguration>.Enumerator enumerator = default(Dictionary<string, KeyboardLayerConfiguration>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06001516 RID: 5398 RVA: 0x000937CF File Offset: 0x000919CF
		public override IEnumerator OnCatalogRefreshCoroutine()
		{
			yield return Catalog.LoadLocationCoroutine<GameObject>(this.prefabAddress, delegate(IResourceLocation value)
			{
				this.prefabLocation = value;
			}, this.id);
			yield break;
		}

		// Token: 0x06001517 RID: 5399 RVA: 0x000937E0 File Offset: 0x000919E0
		public void SpawnAsync(Action<Keyboard> callback, float scale = 1f, string placeHolderText = null, Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
		{
			if (!GameManager.CheckContentActive(this.sensitiveContent, this.sensitiveFilterBehaviour))
			{
				return;
			}
			Vector3 value = position.GetValueOrDefault();
			if (position == null)
			{
				value = ((parent != null) ? parent.position : new Vector3(0f, -1000f, 0f));
				position = new Vector3?(value);
			}
			Quaternion value2 = rotation.GetValueOrDefault();
			if (rotation == null)
			{
				value2 = Quaternion.identity;
				rotation = new Quaternion?(value2);
			}
			Keyboard keyboard = null;
			if (this.prefabLocation != null)
			{
				Addressables.InstantiateAsync(this.prefabLocation, position.Value, rotation.Value, parent, false).Completed += delegate(AsyncOperationHandle<GameObject> handle)
				{
					if (handle.Status == AsyncOperationStatus.Succeeded)
					{
						keyboard = handle.Result.GetComponent<Keyboard>();
						keyboard.gameObject.transform.localScale = new Vector3(scale, scale, scale);
						keyboard.addressableHandle = handle;
						keyboard.transform.SetParent(parent, true);
						if (keyboard.data == null || keyboard.data.id != this.id)
						{
							keyboard.Load(this);
						}
						keyboard.Show(placeHolderText, null);
						keyboard.InvokeOnSpawnEvent(EventTime.OnEnd);
						Action<Keyboard> callback3 = callback;
						if (callback3 == null)
						{
							return;
						}
						callback3(keyboard);
						return;
					}
					else
					{
						Debug.LogWarning("Unable to instantiate keyboard from address " + this.prefabAddress);
						Addressables.ReleaseInstance(handle);
						Action<Keyboard> callback4 = callback;
						if (callback4 == null)
						{
							return;
						}
						callback4(null);
						return;
					}
				};
				return;
			}
			Debug.LogWarning("Prefab location is null, unable to instantiate keyboardId " + this.id);
			Action<Keyboard> callback2 = callback;
			if (callback2 == null)
			{
				return;
			}
			callback2(null);
		}

		// Token: 0x04001501 RID: 5377
		public string prefabAddress;

		// Token: 0x04001502 RID: 5378
		[NonSerialized]
		public IResourceLocation prefabLocation;

		// Token: 0x04001503 RID: 5379
		public HashSet<string> localizationIds;

		// Token: 0x04001504 RID: 5380
		public int RepeatDelayMilliseconds = 500;

		// Token: 0x04001505 RID: 5381
		public int RepeatRatePerSecond = 15;

		// Token: 0x04001506 RID: 5382
		public bool AllowSimultaneousKeyPresses;

		// Token: 0x04001507 RID: 5383
		public string placeholderText = "Enter Text..";

		// Token: 0x04001508 RID: 5384
		public bool PasswordMode;

		// Token: 0x04001509 RID: 5385
		public KeyboardConfiguration keyboardConfig;
	}
}

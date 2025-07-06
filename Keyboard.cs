using System;
using ThunderRoadVRKBSharedData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using VRKB;

namespace ThunderRoad
{
	// Token: 0x020001D2 RID: 466
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Keyboard")]
	[AddComponentMenu("ThunderRoad/Keyboard")]
	public class Keyboard : ThunderBehaviour
	{
		// Token: 0x1400007B RID: 123
		// (add) Token: 0x06001500 RID: 5376 RVA: 0x000931BC File Offset: 0x000913BC
		// (remove) Token: 0x06001501 RID: 5377 RVA: 0x000931F4 File Offset: 0x000913F4
		public event Keyboard.SpawnEvent OnSpawnEvent;

		// Token: 0x1400007C RID: 124
		// (add) Token: 0x06001502 RID: 5378 RVA: 0x0009322C File Offset: 0x0009142C
		// (remove) Token: 0x06001503 RID: 5379 RVA: 0x00093264 File Offset: 0x00091464
		public event Keyboard.SpawnEvent OnDespawnEvent;

		// Token: 0x06001504 RID: 5380 RVA: 0x00093299 File Offset: 0x00091499
		protected virtual void Start()
		{
			if (this.vrkb == null && !base.gameObject.TryGetComponent<KeyboardBehaviour>(out this.vrkb))
			{
				this.vrkb = base.gameObject.AddComponent<KeyboardBehaviour>();
			}
		}

		// Token: 0x06001505 RID: 5381 RVA: 0x000932D0 File Offset: 0x000914D0
		public virtual void Load(KeyboardData keyboardData)
		{
			this.data = keyboardData;
			if (this.vrkb == null && !base.gameObject.TryGetComponent<KeyboardBehaviour>(out this.vrkb))
			{
				this.vrkb = base.gameObject.AddComponent<KeyboardBehaviour>();
			}
			this.vrkb.PasswordMode = this.data.PasswordMode;
			this.vrkb.PlaceholderText = this.data.placeholderText;
			this.vrkb.RepeatDelayInMilliseconds = (float)this.data.RepeatDelayMilliseconds;
			this.vrkb.RepeatRatePerSecond = (float)this.data.RepeatRatePerSecond;
			this.vrkb.AllowSimultaneousKeyPresses = this.data.AllowSimultaneousKeyPresses;
			this.vrkb.PressedKeyMaterial = this.PressedKeyMaterial;
			this.vrkb.HoverKeyMaterial = this.HoverKeyMaterial;
			this.vrkb.UnpressedKeyMaterial = this.UnpressedKeyMaterial;
			KeyboardBehaviour keyboardBehaviour = this.vrkb;
			if (keyboardBehaviour.OnKeyPress == null)
			{
				keyboardBehaviour.OnKeyPress = new OnKeyPressEvent();
			}
			keyboardBehaviour = this.vrkb;
			if (keyboardBehaviour.OnKeyRelease == null)
			{
				keyboardBehaviour.OnKeyRelease = new OnKeyReleaseEvent();
			}
			keyboardBehaviour = this.vrkb;
			if (keyboardBehaviour.OnCancel == null)
			{
				keyboardBehaviour.OnCancel = new VRKB.OnCancelEvent();
			}
			keyboardBehaviour = this.vrkb;
			if (keyboardBehaviour.OnConfirm == null)
			{
				keyboardBehaviour.OnConfirm = new VRKB.OnConfirmEvent();
			}
			this.vrkb.OnKeyPress.AddListener(new UnityAction<KeyBehaviour, bool>(this.VrkbOnKeyPress));
			this.vrkb.OnKeyRelease.AddListener(new UnityAction<KeyBehaviour>(this.VrkbOnKeyRelease));
			this.vrkb.OnConfirm.AddListener(new UnityAction<string>(this.VrkbOnConfirm));
			this.vrkb.OnCancel.AddListener(new UnityAction<string>(this.VrkbOnCancel));
			this.vrkb.OnCancel.AddListener(delegate(string <p0>)
			{
				this.vrkb.ClearText();
			});
			KeyboardLayerConfiguration layerConfig;
			if (this.data.keyboardConfig.layers.TryGetValue(this.data.keyboardConfig.defaultLayerName, out layerConfig))
			{
				foreach (KeyButton child in base.gameObject.GetComponentsInChildren<KeyButton>())
				{
					string id = child.keyId;
					if (layerConfig.keys.ContainsKey(id))
					{
						KeyBehaviour keyBehaviour;
						if (child.gameObject.TryGetOrAddComponent(out keyBehaviour))
						{
							keyBehaviour.Init(id, this.vrkb);
						}
						else
						{
							Debug.LogError("Key " + id + " - does not have a KeyBehaviour component.");
						}
					}
					else
					{
						Debug.LogWarning(string.Concat(new string[]
						{
							"KeyButton Id: ",
							id,
							" does not exist in default layer: ",
							this.data.keyboardConfig.defaultLayerName,
							" on keyboard ",
							this.data.id
						}));
					}
				}
			}
			else
			{
				Debug.LogError("No default layer found called: " + this.data.keyboardConfig.defaultLayerName + " in KeyboardData: " + this.data.id);
			}
			this.vrkb.LoadConfig(this.data.keyboardConfig);
		}

		// Token: 0x06001506 RID: 5382 RVA: 0x000935E4 File Offset: 0x000917E4
		private void VrkbOnKeyPress(KeyBehaviour keyBehaviour, bool autoRepeatPress)
		{
			Keyboard.OnKeyEvent onKey = this.OnKey;
			if (onKey == null)
			{
				return;
			}
			onKey.Invoke(new Keyboard.KeyEvent(keyBehaviour));
		}

		// Token: 0x06001507 RID: 5383 RVA: 0x000935FC File Offset: 0x000917FC
		private void VrkbOnKeyRelease(KeyBehaviour keyBehaviour)
		{
			Keyboard.KeyEvent keyEvent = new Keyboard.KeyEvent(keyBehaviour)
			{
				pressed = false
			};
			Keyboard.OnKeyEvent onKey = this.OnKey;
			if (onKey == null)
			{
				return;
			}
			onKey.Invoke(keyEvent);
		}

		// Token: 0x06001508 RID: 5384 RVA: 0x0009362C File Offset: 0x0009182C
		private void VrkbOnCancel(string textBeforeCancel)
		{
			Keyboard.OnCancelEvent onCancel = this.OnCancel;
			if (onCancel == null)
			{
				return;
			}
			onCancel.Invoke(textBeforeCancel);
		}

		// Token: 0x06001509 RID: 5385 RVA: 0x0009363F File Offset: 0x0009183F
		private void VrkbOnConfirm(string textSubmitted)
		{
			Keyboard.OnConfirmEvent onConfirm = this.OnConfirm;
			if (onConfirm == null)
			{
				return;
			}
			onConfirm.Invoke(textSubmitted);
		}

		// Token: 0x0600150A RID: 5386 RVA: 0x00093652 File Offset: 0x00091852
		public virtual void InvokeOnSpawnEvent(EventTime eventTime)
		{
			Keyboard.SpawnEvent onSpawnEvent = this.OnSpawnEvent;
			if (onSpawnEvent == null)
			{
				return;
			}
			onSpawnEvent(eventTime);
		}

		// Token: 0x0600150B RID: 5387 RVA: 0x00093668 File Offset: 0x00091868
		public void SetPlaceholderText(string placeholderText)
		{
			KeyboardBehaviour keyboardBehaviour = this.vrkb;
			this.data.placeholderText = placeholderText;
			keyboardBehaviour.PlaceholderText = placeholderText;
		}

		// Token: 0x0600150C RID: 5388 RVA: 0x00093690 File Offset: 0x00091890
		public void SetTypedText(string typedText)
		{
			KeyboardBehaviour keyboardBehaviour = this.vrkb;
			this.data.placeholderText = typedText;
			keyboardBehaviour.Text = typedText;
		}

		/// <summary>
		/// Show the instantiated keyboard.
		/// </summary>
		/// <param name="placeHolderText">The custom placeholder text should already be localized in the selected language</param>
		/// <param name="typedText">Text that will be displayed as already typed. Useful to to show any previously typed characters when the keyboard is re-opened</param>
		// Token: 0x0600150D RID: 5389 RVA: 0x000936B7 File Offset: 0x000918B7
		public void Show(string placeHolderText = null, string typedText = null)
		{
			if (placeHolderText != null)
			{
				this.SetPlaceholderText(placeHolderText);
			}
			if (!typedText.IsNullOrEmptyOrWhitespace())
			{
				this.SetTypedText(typedText);
			}
			base.gameObject.SetActive(true);
		}

		// Token: 0x0600150E RID: 5390 RVA: 0x000936DE File Offset: 0x000918DE
		public void Show(Vector3 position, Quaternion rotation, Transform parent, string placeholderText = null, string typedText = null)
		{
			base.transform.SetParent(parent, true);
			base.transform.SetPositionAndRotation(position, rotation);
			this.Show(placeholderText, typedText);
		}

		// Token: 0x0600150F RID: 5391 RVA: 0x00093704 File Offset: 0x00091904
		public void Hide()
		{
			base.gameObject.SetActive(false);
		}

		// Token: 0x06001510 RID: 5392 RVA: 0x00093712 File Offset: 0x00091912
		public void Despawn(float delay)
		{
			if (delay > 0f && !base.IsInvoking("Despawn"))
			{
				base.Invoke("Despawn", delay);
				return;
			}
			this.Despawn();
		}

		// Token: 0x06001511 RID: 5393 RVA: 0x0009373C File Offset: 0x0009193C
		[ContextMenu("Despawn")]
		public virtual void Despawn()
		{
			Keyboard.SpawnEvent onDespawnEvent = this.OnDespawnEvent;
			if (onDespawnEvent != null)
			{
				onDespawnEvent(EventTime.OnStart);
			}
			base.gameObject.SetActive(false);
			if (this.addressableHandle.IsValid())
			{
				Addressables.ReleaseInstance(this.addressableHandle);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			Keyboard.SpawnEvent onDespawnEvent2 = this.OnDespawnEvent;
			if (onDespawnEvent2 == null)
			{
				return;
			}
			onDespawnEvent2(EventTime.OnEnd);
		}

		// Token: 0x040014F6 RID: 5366
		public Keyboard.OnKeyEvent OnKey;

		// Token: 0x040014F7 RID: 5367
		public Keyboard.OnCancelEvent OnCancel;

		// Token: 0x040014F8 RID: 5368
		public Keyboard.OnConfirmEvent OnConfirm;

		// Token: 0x040014F9 RID: 5369
		public Material UnpressedKeyMaterial;

		// Token: 0x040014FA RID: 5370
		public Material HoverKeyMaterial;

		// Token: 0x040014FB RID: 5371
		public Material PressedKeyMaterial;

		// Token: 0x040014FC RID: 5372
		[NonSerialized]
		public KeyboardData data;

		// Token: 0x040014FD RID: 5373
		[NonSerialized]
		public AsyncOperationHandle<GameObject> addressableHandle;

		// Token: 0x04001500 RID: 5376
		public KeyboardBehaviour vrkb;

		// Token: 0x020007FC RID: 2044
		public struct KeyEvent
		{
			// Token: 0x06003E90 RID: 16016 RVA: 0x00184180 File Offset: 0x00182380
			public KeyEvent(KeyBehaviour keyBehaviour)
			{
				this.actionType = keyBehaviour.Config.Action.Type;
				this.keyId = keyBehaviour.keyId;
				this.layer = keyBehaviour.Config.layer;
				this.label = keyBehaviour.Config.Labels[0];
				this.arg = keyBehaviour.Config.Action.Arg;
				this.pressed = keyBehaviour.IsPressed;
				this.pressStartTime = keyBehaviour.PressStartTime;
				this.repeating = keyBehaviour.IsRepeating;
				this.prevRepeatTime = keyBehaviour.PrevRepeatTime;
			}

			// Token: 0x04004012 RID: 16402
			public string keyId;

			// Token: 0x04004013 RID: 16403
			public KeyActionTypes actionType;

			// Token: 0x04004014 RID: 16404
			public string layer;

			// Token: 0x04004015 RID: 16405
			public string label;

			// Token: 0x04004016 RID: 16406
			public string arg;

			// Token: 0x04004017 RID: 16407
			public bool pressed;

			// Token: 0x04004018 RID: 16408
			public float pressStartTime;

			// Token: 0x04004019 RID: 16409
			public bool repeating;

			// Token: 0x0400401A RID: 16410
			public float prevRepeatTime;
		}

		// Token: 0x020007FD RID: 2045
		[Serializable]
		public class OnKeyEvent : UnityEvent<Keyboard.KeyEvent>
		{
		}

		// Token: 0x020007FE RID: 2046
		[Serializable]
		public class OnCancelEvent : UnityEvent<string>
		{
		}

		// Token: 0x020007FF RID: 2047
		[Serializable]
		public class OnConfirmEvent : UnityEvent<string>
		{
		}

		// Token: 0x02000800 RID: 2048
		// (Invoke) Token: 0x06003E95 RID: 16021
		public delegate void SpawnEvent(EventTime eventTime);
	}
}

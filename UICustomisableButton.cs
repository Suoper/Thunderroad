using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200037A RID: 890
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/UICustomisableButton")]
	public class UICustomisableButton : ThunderBehaviour, IPointerClickHandler, IEventSystemHandler, UICustomisableButton.IPointerPhysicalClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		// Token: 0x06002A33 RID: 10803 RVA: 0x0011E276 File Offset: 0x0011C476
		public static T ValidateEventData<T>(BaseEventData data) where T : class
		{
			if (!(data is T))
			{
				throw new ArgumentException(string.Format("Invalid type: {0} passed to event expecting {1}", data.GetType(), typeof(T)));
			}
			return data as T;
		}

		// Token: 0x06002A34 RID: 10804 RVA: 0x0011E2B5 File Offset: 0x0011C4B5
		private static void Execute(UICustomisableButton.IPointerPhysicalClickHandler handler, BaseEventData eventData)
		{
			handler.OnPointerPhysicalClick(UICustomisableButton.ValidateEventData<PointerEventData>(eventData));
		}

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06002A35 RID: 10805 RVA: 0x0011E2C3 File Offset: 0x0011C4C3
		public static ExecuteEvents.EventFunction<UICustomisableButton.IPointerPhysicalClickHandler> pointerPhysicalClickHandler
		{
			get
			{
				return UICustomisableButton.s_PointerPhysicalClickHandler;
			}
		}

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06002A36 RID: 10806 RVA: 0x0011E2CA File Offset: 0x0011C4CA
		// (set) Token: 0x06002A37 RID: 10807 RVA: 0x0011E2D2 File Offset: 0x0011C4D2
		public bool IsPointerInside { get; private set; }

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06002A38 RID: 10808 RVA: 0x0011E2DB File Offset: 0x0011C4DB
		// (set) Token: 0x06002A39 RID: 10809 RVA: 0x0011E2E3 File Offset: 0x0011C4E3
		public bool IsPointerDown { get; private set; }

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06002A3A RID: 10810 RVA: 0x0011E2EC File Offset: 0x0011C4EC
		// (set) Token: 0x06002A3B RID: 10811 RVA: 0x0011E2F4 File Offset: 0x0011C4F4
		public bool IsConnectedToBool { get; set; }

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x06002A3C RID: 10812 RVA: 0x0011E2FD File Offset: 0x0011C4FD
		// (set) Token: 0x06002A3D RID: 10813 RVA: 0x0011E305 File Offset: 0x0011C505
		public bool IsConnectedBoolTrue { get; private set; }

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x06002A3E RID: 10814 RVA: 0x0011E30E File Offset: 0x0011C50E
		public bool IsToggle
		{
			get
			{
				return this.toggle != null;
			}
		}

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x06002A3F RID: 10815 RVA: 0x0011E31C File Offset: 0x0011C51C
		// (set) Token: 0x06002A40 RID: 10816 RVA: 0x0011E324 File Offset: 0x0011C524
		public bool IsInteractable
		{
			get
			{
				return this.interactable;
			}
			set
			{
				this.SetInteractable(value);
			}
		}

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x06002A41 RID: 10817 RVA: 0x0011E32D File Offset: 0x0011C52D
		private bool HasInvertedHoverSettings
		{
			get
			{
				return this.IsConnectedToBool && this.IsConnectedBoolTrue;
			}
		}

		// Token: 0x06002A42 RID: 10818 RVA: 0x0011E340 File Offset: 0x0011C540
		protected void Awake()
		{
			if (this.toggle == null)
			{
				this.toggle = base.GetComponent<Toggle>();
			}
			if (this.IsToggle)
			{
				this.toggle.onValueChanged.AddListener(delegate(bool <p0>)
				{
					this.OnToggleValueChanged(this.toggle);
				});
			}
			this.SetInitialColor();
		}

		// Token: 0x06002A43 RID: 10819 RVA: 0x0011E394 File Offset: 0x0011C594
		protected void Start()
		{
			if (this.outlineFontAsset != null && this.customOutlineFontMaterial != null)
			{
				this.outlineFontAsset.material = this.customOutlineFontMaterial;
			}
			this.onPointerEnter.AddListener(new UnityAction(this.OnPointerEnter));
			this.onPointerExit.AddListener(new UnityAction(this.OnPointerExit));
			this.onPointerClick.AddListener(new UnityAction(this.OnPointerClick));
			base.StartCoroutine(this.InitCoroutine());
		}

		// Token: 0x06002A44 RID: 10820 RVA: 0x0011E420 File Offset: 0x0011C620
		public new void OnEnable()
		{
			base.OnEnable();
			if (this.IsPointerInside)
			{
				this.OnPointerExit(null);
			}
			this.SetInteractable(this.interactable);
		}

		// Token: 0x06002A45 RID: 10821 RVA: 0x0011E443 File Offset: 0x0011C643
		public new void OnDisable()
		{
			base.OnDisable();
			this.SetInteractable(this.interactable);
		}

		// Token: 0x06002A46 RID: 10822 RVA: 0x0011E458 File Offset: 0x0011C658
		private void Update()
		{
			if (this.isPressAndHoldEnabled && this.IsPointerInside && this.IsPointerDown)
			{
				if (this.isPressAndHoldActive)
				{
					float autoClickInterval = (this.holdAutoClicksCount <= this.holdSlowClicksCount) ? this.holdSlowClicksInterval : this.holdFastClicksInterval;
					if (this.lastClick + autoClickInterval <= Time.time)
					{
						this.holdAutoClicksCount++;
						this.OnPointerClick(null);
						return;
					}
				}
				else if (this.lastClick + this.minPressAndHoldDuration <= Time.time)
				{
					this.isPressAndHoldActive = true;
				}
			}
		}

		// Token: 0x06002A47 RID: 10823 RVA: 0x0011E4E4 File Offset: 0x0011C6E4
		private void OnDestroy()
		{
			if (this.onPointerEnterAudioContainer != null)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.onPointerEnterAudioContainer);
			}
			if (this.onPointerExitAudioContainer != null)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.onPointerExitAudioContainer);
			}
			if (this.onClickAudioContainer != null)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.onClickAudioContainer);
			}
		}

		// Token: 0x06002A48 RID: 10824 RVA: 0x0011E53C File Offset: 0x0011C73C
		protected IEnumerator InitCoroutine()
		{
			if (!string.IsNullOrEmpty(this.onPointerEnterSoundAddress))
			{
				Catalog.LoadAssetAsync<AudioContainer>(this.onPointerEnterSoundAddress, delegate(AudioContainer clip)
				{
					this.onPointerEnterAudioContainer = clip;
				}, "UICustomisableButton");
			}
			if (!string.IsNullOrEmpty(this.onPointerExitSoundAddress))
			{
				Catalog.LoadAssetAsync<AudioContainer>(this.onPointerExitSoundAddress, delegate(AudioContainer clip)
				{
					this.onPointerExitAudioContainer = clip;
				}, "UICustomisableButton");
			}
			if (!string.IsNullOrEmpty(this.onPointerClickSoundAddress))
			{
				Catalog.LoadAssetAsync<AudioContainer>(this.onPointerClickSoundAddress, delegate(AudioContainer clip)
				{
					this.onClickAudioContainer = clip;
				}, "UICustomisableButton");
			}
			yield return null;
			yield break;
		}

		// Token: 0x06002A49 RID: 10825 RVA: 0x0011E54C File Offset: 0x0011C74C
		private void OnPointerEnter()
		{
			Graphic[] graphics;
			if (!this.IsConnectedToBool)
			{
				this.SetFont(this.outlineFontAsset, this.labels);
				graphics = this.labels;
				this.ColorTransition(graphics, this.labelsInitialColor, this.hoverColor, this.fadeDuration, false);
				this.ColorTransition(this.buttonGraphicsSwap, this.buttonGraphicsSwapInitialColor, this.hoverColor, this.fadeDuration, false);
				this.GraphicSwapTransition(this.buttonGraphics, this.buttonGraphicsSwap);
				return;
			}
			if (this.IsConnectedBoolTrue)
			{
				this.SetFont(this.defaultFontAsset, this.labels);
				graphics = this.labels;
				this.ColorTransition(graphics, this.labelsInitialColor, this.defaultColor, this.fadeDuration, false);
				this.ColorTransition(this.buttonGraphics, this.buttonGraphicsInitialColor, this.defaultColor, this.fadeDuration, false);
				this.GraphicSwapTransition(this.buttonGraphicsSwap, this.buttonGraphics);
				return;
			}
			this.SetFont(this.outlineFontAsset, this.labels);
			graphics = this.labels;
			this.ColorTransition(graphics, this.labelsInitialColor, this.hoverColor, this.fadeDuration, false);
			this.ColorTransition(this.buttonGraphicsSwap, this.buttonGraphicsSwapInitialColor, this.hoverColor, this.fadeDuration, false);
			this.GraphicSwapTransition(this.buttonGraphics, this.buttonGraphicsSwap);
		}

		// Token: 0x06002A4A RID: 10826 RVA: 0x0011E69C File Offset: 0x0011C89C
		private void OnPointerExit()
		{
			Graphic[] graphics;
			if (!this.IsConnectedToBool)
			{
				this.SetFont(this.defaultFontAsset, this.labels);
				graphics = this.labels;
				this.ColorTransition(graphics, this.labelsInitialColor, this.IsInteractable ? this.defaultColor : this.disabledColor, this.fadeDuration, false);
				this.ColorTransition(this.buttonGraphics, this.buttonGraphicsInitialColor, this.IsInteractable ? this.defaultColor : this.disabledColor, this.fadeDuration, false);
				this.GraphicSwapTransition(this.buttonGraphicsSwap, this.buttonGraphics);
				return;
			}
			if (this.IsConnectedBoolTrue)
			{
				this.SetFont(this.outlineFontAsset, this.labels);
				graphics = this.labels;
				this.ColorTransition(graphics, this.labelsInitialColor, this.IsInteractable ? this.defaultColor : this.hoverColor, this.fadeDuration, false);
				this.ColorTransition(this.buttonGraphicsSwap, this.buttonGraphicsSwapInitialColor, this.IsInteractable ? this.defaultColor : this.hoverColor, this.fadeDuration, false);
				this.GraphicSwapTransition(this.buttonGraphics, this.buttonGraphicsSwap);
				return;
			}
			this.SetFont(this.defaultFontAsset, this.labels);
			graphics = this.labels;
			this.ColorTransition(graphics, this.labelsInitialColor, this.IsInteractable ? this.defaultColor : this.disabledColor, this.fadeDuration, false);
			this.ColorTransition(this.buttonGraphics, this.buttonGraphicsInitialColor, this.IsInteractable ? this.defaultColor : this.disabledColor, this.fadeDuration, false);
			this.GraphicSwapTransition(this.buttonGraphicsSwap, this.buttonGraphics);
		}

		// Token: 0x06002A4B RID: 10827 RVA: 0x0011E850 File Offset: 0x0011CA50
		private void OnPointerClick()
		{
			Graphic[] graphics;
			if (!this.IsConnectedToBool)
			{
				this.ColorTransition(this.buttonGraphics, this.buttonGraphicsInitialColor, this.pressedColor, this.fadeDuration, true);
				graphics = this.labels;
				this.ColorTransition(graphics, this.labelsInitialColor, this.pressedColor, this.fadeDuration, true);
				return;
			}
			if (this.IsConnectedBoolTrue)
			{
				graphics = this.labels;
				this.ColorTransition(graphics, this.labelsInitialColor, this.defaultColor, this.fadeDuration, true);
				this.ColorTransition(this.buttonGraphics, this.buttonGraphicsInitialColor, this.defaultColor, this.fadeDuration, true);
				this.GraphicSwapTransition(this.buttonGraphicsSwap, this.buttonGraphics);
				return;
			}
			graphics = this.labels;
			this.ColorTransition(graphics, this.labelsInitialColor, this.hoverColor, this.fadeDuration, true);
			this.ColorTransition(this.buttonGraphicsSwap, this.buttonGraphicsSwapInitialColor, this.pressedColor, this.fadeDuration, true);
			this.GraphicSwapTransition(this.buttonGraphics, this.buttonGraphicsSwap);
		}

		// Token: 0x06002A4C RID: 10828 RVA: 0x0011E956 File Offset: 0x0011CB56
		private void OnToggleValueChanged(Toggle toggle)
		{
			this.SetButtonState(toggle.isOn);
		}

		// Token: 0x06002A4D RID: 10829 RVA: 0x0011E964 File Offset: 0x0011CB64
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.IsPointerInside = true;
			if (!this.interactable || (this.IsToggle && this.toggle.isOn))
			{
				return;
			}
			if (this.useHapticOnPointerEnter)
			{
				this.Haptic(this.hapticOnPointerEnterIntensity);
			}
			this.Sound(this.onPointerEnterAudioContainer, this.onPointerEnterSoundVolume);
			UnityEvent unityEvent = this.onPointerEnter;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x06002A4E RID: 10830 RVA: 0x0011E9CC File Offset: 0x0011CBCC
		public void OnPointerExit(PointerEventData eventData)
		{
			this.IsPointerInside = false;
			this.IsPointerDown = false;
			if (this.IsToggle && this.toggle.isOn)
			{
				return;
			}
			if (this.useHapticOnPointerExit)
			{
				this.Haptic(this.hapticOnPointerExitIntensity);
			}
			this.Sound(this.onPointerExitAudioContainer, this.onPointerExitSoundVolume);
			if (this.isPressAndHoldEnabled)
			{
				this.ClearPressAndHoldStatus();
			}
			UnityEvent unityEvent = this.onPointerExit;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x06002A4F RID: 10831 RVA: 0x0011EA44 File Offset: 0x0011CC44
		public void OnPointerClick(PointerEventData eventData)
		{
			if (!this.interactable || this.lastClick + this.clickCoolDown > Time.time)
			{
				return;
			}
			if (this.useHapticOnPointerClick)
			{
				this.Haptic(this.hapticOnPointerClickIntensity);
			}
			this.Sound(this.onClickAudioContainer, this.onPointerClickSoundVolume);
			UnityEvent unityEvent = this.onPointerClick;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			this.lastClick = Time.time;
		}

		// Token: 0x06002A50 RID: 10832 RVA: 0x0011EAB0 File Offset: 0x0011CCB0
		public void OnPointerDown(PointerEventData eventData)
		{
			if (!this.interactable)
			{
				return;
			}
			this.IsPointerDown = true;
			UnityEvent unityEvent = this.onPointerDown;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x06002A51 RID: 10833 RVA: 0x0011EAD2 File Offset: 0x0011CCD2
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!this.interactable)
			{
				return;
			}
			this.IsPointerDown = false;
			if (this.isPressAndHoldEnabled)
			{
				this.ClearPressAndHoldStatus();
			}
			UnityEvent unityEvent = this.onPointerUp;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x06002A52 RID: 10834 RVA: 0x0011EB02 File Offset: 0x0011CD02
		public void OnPointerPhysicalClick(PointerEventData eventData)
		{
			if (this.allowPhysicalClick)
			{
				this.OnPointerClick(eventData);
			}
		}

		/// <summary>
		/// Change the visual feedback when the button IsInteractable flag changes
		/// </summary>
		/// <param name="isInteractable"></param>
		// Token: 0x06002A53 RID: 10835 RVA: 0x0011EB14 File Offset: 0x0011CD14
		private void SetInteractable(bool isInteractable)
		{
			this.interactable = isInteractable;
			if (this.IsToggle)
			{
				this.toggle.interactable = isInteractable;
				if (this.toggle.isOn && !isInteractable)
				{
					this.toggle.isOn = false;
				}
			}
			Graphic[] graphics;
			if (!isInteractable)
			{
				this.SetFont(this.defaultFontAsset, this.labels);
				Color color = this.disabledColor;
				graphics = this.labels;
				this.SetColor(color, graphics, this.labelsInitialColor);
				this.SetColor(this.disabledColor, this.buttonGraphics, this.buttonGraphicsInitialColor);
				this.SetColor(this.disabledColor, this.buttonGraphicsSwap, this.buttonGraphicsSwapInitialColor);
				if (this.IsPointerDown)
				{
					this.IsPointerDown = false;
				}
				this.ClearPressAndHoldStatus();
				return;
			}
			if (this.HasInvertedHoverSettings)
			{
				this.SetFont((this.IsPointerInside || (this.IsToggle && this.toggle.isOn)) ? this.defaultFontAsset : this.outlineFontAsset, this.labels);
				Color color2 = this.IsPointerInside ? this.defaultColor : this.hoverColor;
				graphics = this.labels;
				this.SetColor(color2, graphics, this.labelsInitialColor);
				this.SetColor(this.IsPointerInside ? this.defaultColor : this.hoverColor, this.buttonGraphics, this.buttonGraphicsInitialColor);
				this.SetColor(this.IsPointerInside ? this.defaultColor : this.hoverColor, this.buttonGraphicsSwap, this.buttonGraphicsSwapInitialColor);
				return;
			}
			this.SetFont((this.IsPointerInside || (this.IsToggle && this.toggle.isOn)) ? this.outlineFontAsset : this.defaultFontAsset, this.labels);
			Color color3 = this.IsPointerInside ? this.hoverColor : this.defaultColor;
			graphics = this.labels;
			this.SetColor(color3, graphics, this.labelsInitialColor);
			this.SetColor(this.IsPointerInside ? this.hoverColor : this.defaultColor, this.buttonGraphics, this.buttonGraphicsInitialColor);
			this.SetColor(this.IsPointerInside ? this.hoverColor : this.defaultColor, this.buttonGraphicsSwap, this.buttonGraphicsSwapInitialColor);
		}

		// Token: 0x06002A54 RID: 10836 RVA: 0x0011ED3E File Offset: 0x0011CF3E
		private void ClearPressAndHoldStatus()
		{
			this.isPressAndHoldActive = false;
			this.holdAutoClicksCount = 0;
		}

		/// <summary>
		/// Plays an haptic vibration on the pointing hand
		/// </summary>
		/// <param name="intensity">Intensity of the haptic vibration</param>
		// Token: 0x06002A55 RID: 10837 RVA: 0x0011ED4E File Offset: 0x0011CF4E
		public void Haptic(float intensity)
		{
			PlayerControl.Hand pointerHand = this.GetPointerHand();
			if (pointerHand == null)
			{
				return;
			}
			pointerHand.HapticShort(intensity, false);
		}

		/// <summary>
		/// Plays a sound on the active pointer audio source
		/// </summary>
		/// <param name="clip">Audio clip to play</param>
		/// <param name="volumeDb">Volume of the sound (in DB)</param>
		// Token: 0x06002A56 RID: 10838 RVA: 0x0011ED64 File Offset: 0x0011CF64
		public void Sound(AudioContainer audioContainer, float volumeDb)
		{
			if (!audioContainer)
			{
				return;
			}
			Pointer pointer = Pointer.GetActive();
			if (pointer != null && pointer.audioSource != null)
			{
				pointer.audioSource.PlayOneShot(audioContainer.PickAudioClip(), EffectAudio.DecibelToLinear(volumeDb));
			}
		}

		/// <summary>
		/// Get the currently pointing hand
		/// </summary>
		// Token: 0x06002A57 RID: 10839 RVA: 0x0011EDB0 File Offset: 0x0011CFB0
		private PlayerControl.Hand GetPointerHand()
		{
			if (!Pointer.GetActive() || !Player.local)
			{
				return null;
			}
			PlayerHand hand = Player.local.GetHand(Pointer.activeSide);
			if (!hand)
			{
				return null;
			}
			return hand.controlHand;
		}

		/// <summary>
		/// Make the graphics fade from their current color to the given one
		/// </summary>
		/// <param name="graphics">Graphics to change color</param>
		/// <param name="color">Color to fade to</param>
		/// <param name="fadeTime">Time to fade from the default color to the next one</param>
		/// <param name="pingPong">Should the fading returns back to the default color once done?</param>
		// Token: 0x06002A58 RID: 10840 RVA: 0x0011EDF8 File Offset: 0x0011CFF8
		private void ColorTransition(Graphic[] graphics, Color[] initialGraphicsColor, Color color, float fadeTime, bool pingPong = false)
		{
			if (graphics == null || graphics.Length == 0)
			{
				return;
			}
			if (base.gameObject.activeInHierarchy)
			{
				if (this.graphicsFadeCoroutine != null)
				{
					base.StopCoroutine(this.graphicsFadeCoroutine);
				}
				this.graphicsFadeCoroutine = base.StartCoroutine(this.ColorTransitionRoutine(graphics, initialGraphicsColor, color, fadeTime, pingPong));
				return;
			}
			if (!pingPong)
			{
				this.SetColor(color, graphics, initialGraphicsColor);
			}
		}

		/// <summary>
		/// Make the graphics fade from their current color to the given one
		/// </summary>
		/// <param name="labels">Labels to change color</param>
		/// <param name="color">Color to fade to</param>
		/// <param name="fadeTime">Time to fade from the default color to the next one</param>
		/// <param name="pingPong">Should the fading returns back to the default color once done?</param>
		// Token: 0x06002A59 RID: 10841 RVA: 0x0011EE58 File Offset: 0x0011D058
		private void ColorTransition(TextMeshProUGUI[] labels, Color[] initialLabelsColor, Color color, float fadeTime, bool pingPong = false)
		{
			if (labels == null || labels.Length == 0)
			{
				return;
			}
			if (base.gameObject.activeInHierarchy)
			{
				if (this.labelsFadeCoroutine != null)
				{
					base.StopCoroutine(this.labelsFadeCoroutine);
				}
				this.labelsFadeCoroutine = base.StartCoroutine(this.ColorTransitionRoutine(labels, initialLabelsColor, color, fadeTime, pingPong));
				return;
			}
			if (!pingPong)
			{
				this.SetColor(color, labels, initialLabelsColor);
			}
		}

		/// <summary>
		/// Make the graphics fade from their current color to the given one
		/// </summary>
		/// <param name="graphics">Graphics to change color</param>
		/// <param name="color">Color to fade to</param>
		/// <param name="fadeTime">Time to fade from the default color to the next one</param>
		/// <param name="pingPong">Should the fading returns back to the default color once done?</param>
		/// <returns></returns>
		// Token: 0x06002A5A RID: 10842 RVA: 0x0011EEB5 File Offset: 0x0011D0B5
		private IEnumerator ColorTransitionRoutine(Graphic[] graphics, Color[] initialGraphicsColor, Color color, float fadeTime, bool pingPong = false)
		{
			float t = 0f;
			Color startColor = graphics[0].color;
			while (t < fadeTime)
			{
				this.SetColor(Color.Lerp(startColor, color, t / fadeTime), graphics, initialGraphicsColor);
				t += Time.deltaTime;
				yield return null;
			}
			this.SetColor(color, graphics, initialGraphicsColor);
			if (!pingPong)
			{
				yield break;
			}
			t = 0f;
			while (t < fadeTime)
			{
				this.SetColor(Color.Lerp(color, startColor, t / fadeTime), graphics, initialGraphicsColor);
				t += Time.deltaTime;
				yield return null;
			}
			this.SetColor(startColor, graphics, initialGraphicsColor);
			yield break;
		}

		/// <summary>
		/// Make the graphics fade from their current color to the given one
		/// </summary>
		/// <param name="graphics">Graphics to change color</param>
		/// <param name="color">Color to fade to</param>
		/// <param name="fadeTime">Time to fade from the default color to the next one</param>
		/// <param name="pingPong">Should the fading returns back to the default color once done?</param>
		/// <returns></returns>
		// Token: 0x06002A5B RID: 10843 RVA: 0x0011EEE9 File Offset: 0x0011D0E9
		private IEnumerator ColorTransitionRoutine(TextMeshProUGUI[] labels, Color[] initialLabelsColor, Color color, float fadeTime, bool pingPong = false)
		{
			float t = 0f;
			Color startColor = labels[0].color;
			while (t < fadeTime)
			{
				this.SetColor(Color.Lerp(startColor, color, t / fadeTime), labels, initialLabelsColor);
				t += Time.deltaTime;
				yield return null;
			}
			this.SetColor(color, labels, initialLabelsColor);
			if (!pingPong)
			{
				yield break;
			}
			t = 0f;
			while (t < fadeTime)
			{
				this.SetColor(Color.Lerp(color, startColor, t / fadeTime), labels, initialLabelsColor);
				t += Time.deltaTime;
				yield return null;
			}
			this.SetColor(startColor, labels, initialLabelsColor);
			yield break;
		}

		/// <summary>
		/// Make the first graphics array disable, and the second one enable.
		/// </summary>
		/// <param name="graphicsToSwap">Graphics to disable</param>
		/// <param name="graphicsToSwapFor">Graphics to enable</param>
		// Token: 0x06002A5C RID: 10844 RVA: 0x0011EF20 File Offset: 0x0011D120
		private void GraphicSwapTransition(Graphic[] graphicsToSwap, Graphic[] graphicsToSwapFor)
		{
			for (int i = 0; i < graphicsToSwap.Length; i++)
			{
				graphicsToSwap[i].gameObject.SetActive(false);
			}
			for (int j = 0; j < graphicsToSwapFor.Length; j++)
			{
				graphicsToSwapFor[j].gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Sets the color of the graphics
		/// </summary>
		/// <param name="color">Color to set the graphics to</param>
		// Token: 0x06002A5D RID: 10845 RVA: 0x0011EF68 File Offset: 0x0011D168
		private void SetColor(Color color, Graphic[] graphics, Color[] initialGraphicsColor)
		{
			if (graphics == null)
			{
				return;
			}
			if (this.multiplyColor)
			{
				for (int i = 0; i < graphics.Length; i++)
				{
					graphics[i].color = initialGraphicsColor[i] * color;
				}
				return;
			}
			for (int j = 0; j < graphics.Length; j++)
			{
				graphics[j].color = color;
			}
		}

		/// <summary>
		/// Sets the color of the graphics
		/// </summary>
		/// <param name="color">Color to set the graphics to</param>
		// Token: 0x06002A5E RID: 10846 RVA: 0x0011EFBC File Offset: 0x0011D1BC
		private void SetColor(Color color, TextMeshProUGUI[] labels, Color[] initialLabelsColor)
		{
			if (labels == null)
			{
				return;
			}
			if (this.multiplyColor)
			{
				for (int i = 0; i < labels.Length; i++)
				{
					labels[i].color = initialLabelsColor[i] * color;
				}
				return;
			}
			for (int j = 0; j < labels.Length; j++)
			{
				labels[j].color = color;
			}
		}

		// Token: 0x06002A5F RID: 10847 RVA: 0x0011F010 File Offset: 0x0011D210
		public void SetInitialColor()
		{
			this.buttonGraphicsInitialColor = new Color[this.buttonGraphics.Length];
			for (int i = 0; i < this.buttonGraphics.Length; i++)
			{
				this.buttonGraphicsInitialColor[i] = this.buttonGraphics[i].color;
			}
			this.buttonGraphicsSwapInitialColor = new Color[this.buttonGraphicsSwap.Length];
			for (int j = 0; j < this.buttonGraphicsSwap.Length; j++)
			{
				this.buttonGraphicsSwapInitialColor[j] = this.buttonGraphicsSwap[j].color;
			}
			this.labelsInitialColor = new Color[this.labels.Length];
			for (int k = 0; k < this.labels.Length; k++)
			{
				this.labelsInitialColor[k] = this.labels[k].color;
			}
			this.SetInteractable(this.interactable);
		}

		// Token: 0x06002A60 RID: 10848 RVA: 0x0011F0E8 File Offset: 0x0011D2E8
		public void ResetInitialColor()
		{
			if (this.buttonGraphicsInitialColor != null && this.buttonGraphicsInitialColor.Length == this.buttonGraphics.Length)
			{
				for (int i = 0; i < this.buttonGraphics.Length; i++)
				{
					this.buttonGraphics[i].color = this.buttonGraphicsInitialColor[i];
				}
			}
			if (this.buttonGraphicsSwapInitialColor != null && this.buttonGraphicsSwapInitialColor.Length == this.buttonGraphicsSwap.Length)
			{
				for (int j = 0; j < this.buttonGraphicsSwap.Length; j++)
				{
					this.buttonGraphicsSwap[j].color = this.buttonGraphicsSwapInitialColor[j];
				}
			}
			if (this.labelsInitialColor != null && this.labelsInitialColor.Length == this.labels.Length)
			{
				for (int k = 0; k < this.labels.Length; k++)
				{
					this.labels[k].color = this.labelsInitialColor[k];
				}
			}
		}

		/// <summary>
		/// Use this to set the default or the outline font to the text labels
		/// </summary>
		/// <param name="font">New font to set</param>
		/// <param name="labels">List of labels to update</param>
		// Token: 0x06002A61 RID: 10849 RVA: 0x0011F1C8 File Offset: 0x0011D3C8
		private void SetFont(TMP_FontAsset font, TMP_Text[] labels)
		{
			if (font == null || labels == null)
			{
				return;
			}
			for (int i = 0; i < labels.Length; i++)
			{
				labels[i].font = font;
			}
		}

		// Token: 0x06002A62 RID: 10850 RVA: 0x0011F1FC File Offset: 0x0011D3FC
		public void SetButtonState(bool isOn)
		{
			if (this.IsConnectedToBool)
			{
				this.IsConnectedBoolTrue = isOn;
			}
			Graphic[] graphics;
			if (isOn)
			{
				this.SetFont(this.outlineFontAsset, this.labels);
				Color color = this.pressedColor;
				graphics = this.labels;
				this.SetColor(color, graphics, this.labelsInitialColor);
				this.SetColor(this.pressedColor, this.buttonGraphicsSwap, this.buttonGraphicsSwapInitialColor);
				this.GraphicSwapTransition(this.buttonGraphics, this.buttonGraphicsSwap);
				return;
			}
			this.SetFont(this.defaultFontAsset, this.labels);
			Color color2 = this.defaultColor;
			graphics = this.labels;
			this.SetColor(color2, graphics, this.labelsInitialColor);
			this.SetColor(this.defaultColor, this.buttonGraphics, this.buttonGraphicsInitialColor);
			this.GraphicSwapTransition(this.buttonGraphicsSwap, this.buttonGraphics);
		}

		// Token: 0x06002A63 RID: 10851 RVA: 0x0011F2C8 File Offset: 0x0011D4C8
		public void SetupStyle(UIPlayerMenu.Style style, Color defaultColor, Color highlightColor, Dictionary<UIPlayerMenu.Style, Material> outlineFontMaterials)
		{
			this.defaultColor = defaultColor;
			this.hoverColor = highlightColor;
			this.pressedColor = highlightColor;
			for (int i = 0; i < this.buttonGraphics.Length; i++)
			{
				this.buttonGraphics[i].color = defaultColor;
			}
			for (int j = 0; j < this.buttonGraphicsSwap.Length; j++)
			{
				this.buttonGraphicsSwap[j].color = highlightColor;
			}
			for (int k = 0; k < this.labels.Length; k++)
			{
				if (this.labels[k].font == this.outlineFontAsset)
				{
					this.labels[k].fontMaterial = outlineFontMaterials[style];
				}
				else
				{
					this.labels[k].color = defaultColor;
				}
			}
			if (this.IsToggle)
			{
				this.SetButtonState(this.toggle.isOn);
			}
		}

		// Token: 0x040027F4 RID: 10228
		private static readonly ExecuteEvents.EventFunction<UICustomisableButton.IPointerPhysicalClickHandler> s_PointerPhysicalClickHandler = new ExecuteEvents.EventFunction<UICustomisableButton.IPointerPhysicalClickHandler>(UICustomisableButton.Execute);

		// Token: 0x040027F5 RID: 10229
		[Header("General")]
		[SerializeField]
		private bool interactable = true;

		// Token: 0x040027F6 RID: 10230
		public Toggle toggle;

		// Token: 0x040027F7 RID: 10231
		[Tooltip("Set of graphics that are enabled when the button is in normal state.")]
		public Graphic[] buttonGraphics;

		// Token: 0x040027F8 RID: 10232
		[Tooltip("Set of graphics that are enabled when the button is in highlighted or selected state.")]
		public Graphic[] buttonGraphicsSwap;

		// Token: 0x040027F9 RID: 10233
		[Tooltip("Set of labels that will change colors with the user interactions.")]
		public TMP_Text[] labels;

		// Token: 0x040027FA RID: 10234
		public float fadeDuration = 0.1f;

		// Token: 0x040027FB RID: 10235
		public bool allowPhysicalClick;

		// Token: 0x040027FC RID: 10236
		public float clickCoolDown;

		// Token: 0x040027FD RID: 10237
		[Header("Press and Hold")]
		public bool isPressAndHoldEnabled;

		// Token: 0x040027FE RID: 10238
		public float minPressAndHoldDuration = 0.5f;

		// Token: 0x040027FF RID: 10239
		public float holdSlowClicksInterval = 0.3f;

		// Token: 0x04002800 RID: 10240
		public float holdFastClicksInterval = 0.125f;

		// Token: 0x04002801 RID: 10241
		public int holdSlowClicksCount = 3;

		// Token: 0x04002802 RID: 10242
		[Header("Fonts")]
		public TMP_FontAsset defaultFontAsset;

		// Token: 0x04002803 RID: 10243
		public TMP_FontAsset outlineFontAsset;

		// Token: 0x04002804 RID: 10244
		public Material customOutlineFontMaterial;

		// Token: 0x04002805 RID: 10245
		[Header("Colors")]
		public bool multiplyColor;

		// Token: 0x04002806 RID: 10246
		public Color defaultColor = Color.white;

		// Token: 0x04002807 RID: 10247
		public Color hoverColor = Color.white;

		// Token: 0x04002808 RID: 10248
		public Color pressedColor = Color.white;

		// Token: 0x04002809 RID: 10249
		public Color disabledColor = Color.gray;

		// Token: 0x0400280A RID: 10250
		[Header("Haptics")]
		public bool useHapticOnPointerEnter = true;

		// Token: 0x0400280B RID: 10251
		public bool useHapticOnPointerExit;

		// Token: 0x0400280C RID: 10252
		public bool useHapticOnPointerClick = true;

		// Token: 0x0400280D RID: 10253
		public float hapticOnPointerEnterIntensity = 1f;

		// Token: 0x0400280E RID: 10254
		public float hapticOnPointerExitIntensity = 0.5f;

		// Token: 0x0400280F RID: 10255
		public float hapticOnPointerClickIntensity = 2f;

		// Token: 0x04002810 RID: 10256
		[Header("Sounds")]
		public string onPointerEnterSoundAddress = "Bas.AudioGroup.UI.LightClick";

		// Token: 0x04002811 RID: 10257
		public string onPointerExitSoundAddress;

		// Token: 0x04002812 RID: 10258
		public string onPointerClickSoundAddress = "Bas.AudioGroup.UI.PlayerMenuValidationDefault";

		// Token: 0x04002813 RID: 10259
		public float onPointerEnterSoundVolume = 1f;

		// Token: 0x04002814 RID: 10260
		public float onPointerExitSoundVolume = 1f;

		// Token: 0x04002815 RID: 10261
		public float onPointerClickSoundVolume = 1f;

		// Token: 0x04002816 RID: 10262
		[Header("Events")]
		public UnityEvent onPointerClick;

		// Token: 0x04002817 RID: 10263
		public UnityEvent onPointerEnter;

		// Token: 0x04002818 RID: 10264
		public UnityEvent onPointerExit;

		// Token: 0x04002819 RID: 10265
		public UnityEvent onPointerDown;

		// Token: 0x0400281A RID: 10266
		public UnityEvent onPointerUp;

		// Token: 0x0400281B RID: 10267
		private Coroutine graphicsFadeCoroutine;

		// Token: 0x0400281C RID: 10268
		private Coroutine labelsFadeCoroutine;

		// Token: 0x0400281D RID: 10269
		private AudioContainer onPointerEnterAudioContainer;

		// Token: 0x0400281E RID: 10270
		private AudioContainer onPointerExitAudioContainer;

		// Token: 0x0400281F RID: 10271
		private AudioContainer onClickAudioContainer;

		// Token: 0x04002820 RID: 10272
		private float lastClick;

		// Token: 0x04002821 RID: 10273
		private bool isPressAndHoldActive;

		// Token: 0x04002822 RID: 10274
		private int holdAutoClicksCount;

		// Token: 0x04002823 RID: 10275
		private Color[] buttonGraphicsInitialColor;

		// Token: 0x04002824 RID: 10276
		private Color[] buttonGraphicsSwapInitialColor;

		// Token: 0x04002825 RID: 10277
		private Color[] labelsInitialColor;

		// Token: 0x02000A8E RID: 2702
		public interface IPointerPhysicalClickHandler : IEventSystemHandler
		{
			// Token: 0x0600469A RID: 18074
			void OnPointerPhysicalClick(PointerEventData eventData);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000397 RID: 919
	public class UIWaveSpawner : MonoBehaviour
	{
		// Token: 0x06002BBB RID: 11195 RVA: 0x00126EA6 File Offset: 0x001250A6
		private void Awake()
		{
			this.leftPageTitle.SetLocalizationIds("Default", "{Waves}");
		}

		// Token: 0x06002BBC RID: 11196 RVA: 0x00126EBD File Offset: 0x001250BD
		private void Start()
		{
			this.ClearWaveData();
			base.StartCoroutine(this.SetupBook());
		}

		// Token: 0x06002BBD RID: 11197 RVA: 0x00126ED4 File Offset: 0x001250D4
		private void OnEnable()
		{
			this.waveSpawner.OnWaveBeginEvent.AddListener(new UnityAction(this.OnWaveStarted));
			this.waveSpawner.OnWaveWinEvent.AddListener(new UnityAction(this.OnWaveEnded));
			this.waveSpawner.OnWaveLossEvent.AddListener(new UnityAction(this.OnWaveEnded));
			this.waveSpawner.OnWaveCancelEvent.AddListener(new UnityAction(this.OnWaveEnded));
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
		}

		// Token: 0x06002BBE RID: 11198 RVA: 0x00126F64 File Offset: 0x00125164
		private void OnDisable()
		{
			this.waveSpawner.OnWaveBeginEvent.RemoveListener(new UnityAction(this.OnWaveStarted));
			this.waveSpawner.OnWaveWinEvent.RemoveListener(new UnityAction(this.OnWaveEnded));
			this.waveSpawner.OnWaveLossEvent.RemoveListener(new UnityAction(this.OnWaveEnded));
			this.waveSpawner.OnWaveCancelEvent.RemoveListener(new UnityAction(this.OnWaveEnded));
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002BBF RID: 11199 RVA: 0x00126FF2 File Offset: 0x001251F2
		private void OnDrawGizmos()
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.402f, 0.29f, 0f));
		}

		// Token: 0x06002BC0 RID: 11200 RVA: 0x00127022 File Offset: 0x00125222
		private void OnLanguageChanged(string language)
		{
			this.SetLocalizedFields();
		}

		// Token: 0x06002BC1 RID: 11201 RVA: 0x0012702A File Offset: 0x0012522A
		public void ToggleActivateCanvas()
		{
			base.gameObject.SetActive(this.activateCanvas);
			this.activateCanvas = !this.activateCanvas;
		}

		// Token: 0x06002BC2 RID: 11202 RVA: 0x0012704C File Offset: 0x0012524C
		public void ToggleCanvas()
		{
			CanvasGroup canvasGroup = base.GetComponentInParent<CanvasGroup>();
			if (canvasGroup)
			{
				this.showCanvas = !this.showCanvas;
				canvasGroup.alpha = (float)(this.showCanvas ? 1 : 0);
			}
		}

		// Token: 0x06002BC3 RID: 11203 RVA: 0x0012708A File Offset: 0x0012528A
		private IEnumerator SetupBook()
		{
			while (!Catalog.IsJsonLoaded() || !LocalizationManager.Instance.IsTextDataParsed)
			{
				yield return null;
			}
			using (var enumerator = (from w in Catalog.GetDataList(Category.Wave)
			where ((WaveData)w).alwaysAvailable || (((WaveData)w).waveSelectors != null && ((WaveData)w).waveSelectors.Any((string l) => l.ToLower() == this.id.ToLower()))
			group w by ((WaveData)w).category into grp
			select new
			{
				grp.Key
			}).OrderBy(delegate(c)
			{
				if (!(c.Key == ""))
				{
					return c.Key;
				}
				return "ZZZZZZZZ";
			}).ToList().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					<>f__AnonymousType1<string> category = enumerator.Current;
					UiWaveSpawnerCategoryElement uiWaveSpawnerCategoryElement = UnityEngine.Object.Instantiate<UiWaveSpawnerCategoryElement>(this.categoryElement, this.categoryElement.transform.parent);
					uiWaveSpawnerCategoryElement.gameObject.SetActive(true);
					string categoryName = (category.Key != "") ? (char.IsNumber(category.Key[0]) ? category.Key.Substring(1, category.Key.Length - 1) : category.Key) : "Misc";
					List<CatalogData> wavesData = Catalog.GetDataList(Category.Wave).Where(delegate(CatalogData w)
					{
						if (((WaveData)w).alwaysAvailable || (((WaveData)w).waveSelectors != null && ((WaveData)w).waveSelectors.Any((string l) => l.ToLower() == this.id.ToLower())))
						{
							WaveData waveData = (WaveData)w;
							return ((waveData != null) ? waveData.category : null) == category.Key;
						}
						return false;
					}).ToList<CatalogData>();
					uiWaveSpawnerCategoryElement.SetCategory(this, categoryName, wavesData);
				}
			}
			this.waveSpawner.OnWaveBeginEvent.AddListener(new UnityAction(this.OnWaveStarted));
			this.waveSpawner.OnWaveWinEvent.AddListener(new UnityAction(this.OnWaveEnded));
			this.waveSpawner.OnWaveLossEvent.AddListener(new UnityAction(this.OnWaveEnded));
			this.waveSpawner.OnWaveCancelEvent.AddListener(new UnityAction(this.OnWaveEnded));
			this.categoryElement.gameObject.SetActive(false);
			yield break;
		}

		// Token: 0x06002BC4 RID: 11204 RVA: 0x0012709C File Offset: 0x0012529C
		private void SetLocalizedFields()
		{
			if (this.waveData == null)
			{
				return;
			}
			TextData.Wave waveLocalization = LocalizationManager.Instance.GetLocalizedTextWave(this.waveData.localizationId);
			this.waveTitle.SetLocalizedText((waveLocalization != null) ? waveLocalization.title : this.waveData.title);
			this.waveDescription.SetLocalizedText((waveLocalization != null) ? waveLocalization.description : this.waveData.description);
		}

		// Token: 0x06002BC5 RID: 11205 RVA: 0x0012710C File Offset: 0x0012530C
		private void ClearWaveData()
		{
			this.waveData = null;
			this.selectWaveText.gameObject.SetActive(true);
			this.waveTitle.gameObject.SetActive(false);
			this.waveDescription.gameObject.SetActive(false);
			this.npcMaxAliveCount.gameObject.SetActive(false);
			this.npcTotalCount.gameObject.SetActive(false);
			this.startButton.IsInteractable = false;
		}

		// Token: 0x06002BC6 RID: 11206 RVA: 0x00127181 File Offset: 0x00125381
		private void OnWaveStarted()
		{
			this.leftPageTitle.SetLocalizationIds("Default", "{Fight}");
			this.wavesScroll.gameObject.SetActive(false);
			this.fightProgress.SetActive(true);
		}

		// Token: 0x06002BC7 RID: 11207 RVA: 0x001271B5 File Offset: 0x001253B5
		private void OnWaveEnded()
		{
			this.fightState.SetLocalizationIds("Default", "{FightEnded}");
			this.stopButton.labels[0].GetComponent<UIText>().SetLocalizationIds("Default", "{Return}");
		}

		// Token: 0x06002BC8 RID: 11208 RVA: 0x001271F0 File Offset: 0x001253F0
		public void OnWaveSelectedChanged(Toggle toggle, WaveData waveData)
		{
			if (toggle.isOn)
			{
				this.waveData = waveData;
				if (this.selectWaveText.gameObject.activeInHierarchy)
				{
					this.selectWaveText.gameObject.SetActive(false);
					this.waveTitle.gameObject.SetActive(true);
					this.waveDescription.gameObject.SetActive(true);
				}
				this.SetLocalizedFields();
				this.npcMaxAliveCount.gameObject.SetActive(true);
				this.npcTotalCount.gameObject.SetActive(true);
				this.npcMaxAliveCount.text = waveData.GetMaxAlive().ToString();
				this.npcTotalCount.text = ((waveData.loopBehavior != WaveData.LoopBehavior.NoLoop) ? "∞" : (((float)waveData.minTotalCount == waveData.maxTotalCount) ? waveData.minTotalCount.ToString(CultureInfo.InvariantCulture) : (waveData.minTotalCount.ToString() + "-" + waveData.maxTotalCount.ToString())));
				this.startButton.IsInteractable = true;
				return;
			}
			this.ClearWaveData();
		}

		// Token: 0x06002BC9 RID: 11209 RVA: 0x00127304 File Offset: 0x00125504
		public void OnStartClick()
		{
			if (this.waveSpawner)
			{
				this.waveSpawner.StartWave(this.waveData, this.startDelay, true);
				this.fightState.SetLocalizationIds("Default", "{FightInProgress}");
			}
			else
			{
				Debug.LogError("SpawnLocation not set!");
				this.fightState.SetLocalizationIds("Default", "{Error}");
			}
			this.startButton.IsInteractable = false;
			this.stopButton.labels[0].GetComponent<UIText>().SetLocalizationIds("Default", "{Stop}");
		}

		// Token: 0x06002BCA RID: 11210 RVA: 0x0012739C File Offset: 0x0012559C
		public void OnStopClick()
		{
			this.leftPageTitle.SetLocalizationIds("Default", "{Waves}");
			this.wavesScroll.gameObject.SetActive(true);
			this.fightProgress.SetActive(false);
			this.waveSpawner.StopWave(false);
			this.startButton.IsInteractable = true;
		}

		// Token: 0x0400293D RID: 10557
		[Header("Setup")]
		public string id;

		// Token: 0x0400293E RID: 10558
		[SerializeField]
		private float startDelay = 5f;

		// Token: 0x0400293F RID: 10559
		[Header("References")]
		public WaveSpawner waveSpawner;

		// Token: 0x04002940 RID: 10560
		[SerializeField]
		private UIText leftPageTitle;

		// Token: 0x04002941 RID: 10561
		[SerializeField]
		private ToggleGroup wavesGrid;

		// Token: 0x04002942 RID: 10562
		[SerializeField]
		private UiWaveSpawnerCategoryElement categoryElement;

		// Token: 0x04002943 RID: 10563
		[SerializeField]
		private GameObject fightProgress;

		// Token: 0x04002944 RID: 10564
		[SerializeField]
		private UIText fightState;

		// Token: 0x04002945 RID: 10565
		[SerializeField]
		private UIScrollController wavesScroll;

		// Token: 0x04002946 RID: 10566
		[SerializeField]
		private UIText selectWaveText;

		// Token: 0x04002947 RID: 10567
		[SerializeField]
		private UIText waveTitle;

		// Token: 0x04002948 RID: 10568
		[SerializeField]
		private UIText waveDescription;

		// Token: 0x04002949 RID: 10569
		[SerializeField]
		private TextMeshProUGUI npcMaxAliveCount;

		// Token: 0x0400294A RID: 10570
		[SerializeField]
		private TextMeshProUGUI npcTotalCount;

		// Token: 0x0400294B RID: 10571
		[SerializeField]
		private UICustomisableButton startButton;

		// Token: 0x0400294C RID: 10572
		[SerializeField]
		private UICustomisableButton stopButton;

		// Token: 0x0400294D RID: 10573
		protected WaveData waveData;

		// Token: 0x0400294E RID: 10574
		private bool activateCanvas = true;

		// Token: 0x0400294F RID: 10575
		private bool showCanvas;
	}
}

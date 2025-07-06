using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000294 RID: 660
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/FxController.html")]
	public class FxController : MonoBehaviour
	{
		// Token: 0x140000EA RID: 234
		// (add) Token: 0x06001F10 RID: 7952 RVA: 0x000D3F54 File Offset: 0x000D2154
		// (remove) Token: 0x06001F11 RID: 7953 RVA: 0x000D3F8C File Offset: 0x000D218C
		public event Action onLifetimeExpired;

		// Token: 0x06001F12 RID: 7954 RVA: 0x000D3FC4 File Offset: 0x000D21C4
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.modules = new List<FxModule>(base.GetComponentsInChildren<FxModule>());
			foreach (FxModule fxModule in this.modules)
			{
				fxModule.controller = this;
			}
			if (Application.isPlaying && this.initialized)
			{
				this.Refresh();
			}
		}

		// Token: 0x06001F13 RID: 7955 RVA: 0x000D404C File Offset: 0x000D224C
		private void Start()
		{
			this.source = base.GetComponentInParent<Item>();
			if (this.source == null)
			{
				this.source = base.GetComponentInParent<Creature>();
			}
			if (this.playOnStart)
			{
				this.Play();
			}
			this.initialized = true;
		}

		// Token: 0x06001F14 RID: 7956 RVA: 0x000D4084 File Offset: 0x000D2284
		public void Play()
		{
			this.Refresh();
			int modulesCount = this.modules.Count;
			for (int i = 0; i < modulesCount; i++)
			{
				FxModule fxModule = this.modules[i];
				fxModule.controller = this;
				fxModule.Play();
			}
			this.isPlaying = true;
			if (this.lifeTime > 0f)
			{
				base.Invoke("OnLifetimeExpired", this.lifeTime);
			}
		}

		/// <summary>
		/// Intensity values between 0 and 1.
		/// </summary>
		// Token: 0x06001F15 RID: 7957 RVA: 0x000D40EC File Offset: 0x000D22EC
		public void SetIntensity(float intensity)
		{
			float num;
			if (intensity >= 0f)
			{
				if (intensity <= 1f)
				{
					num = intensity;
				}
				else
				{
					num = 1f;
				}
			}
			else
			{
				num = 0f;
			}
			intensity = num;
			this.intensity = intensity;
			int modulesCount = this.modules.Count;
			for (int i = 0; i < modulesCount; i++)
			{
				FxModule fxModule = this.modules[i];
				fxModule.controller = this;
				fxModule.SetIntensity(intensity);
			}
		}

		// Token: 0x06001F16 RID: 7958 RVA: 0x000D4158 File Offset: 0x000D2358
		public void SetSpeed(float speed)
		{
			this.speed = speed;
			int modulesCount = this.modules.Count;
			for (int i = 0; i < modulesCount; i++)
			{
				this.modules[i].SetSpeed(speed);
			}
		}

		// Token: 0x06001F17 RID: 7959 RVA: 0x000D4198 File Offset: 0x000D2398
		public void Refresh()
		{
			int modulesCount = this.modules.Count;
			for (int i = 0; i < modulesCount; i++)
			{
				FxModule fxModule = this.modules[i];
				fxModule.controller = this;
				fxModule.SetIntensity(this.intensity);
				fxModule.SetSpeed(this.speed);
			}
		}

		// Token: 0x06001F18 RID: 7960 RVA: 0x000D41E7 File Offset: 0x000D23E7
		public void Stop()
		{
			this.Stop(true);
		}

		// Token: 0x06001F19 RID: 7961 RVA: 0x000D41F0 File Offset: 0x000D23F0
		public void Stop(bool playStopEffect = true)
		{
			int modulesCount = this.modules.Count;
			for (int i = 0; i < modulesCount; i++)
			{
				FxModule fxModule = this.modules[i];
				fxModule.controller = this;
				fxModule.Stop(playStopEffect);
			}
			this.isPlaying = false;
		}

		// Token: 0x06001F1A RID: 7962 RVA: 0x000D4235 File Offset: 0x000D2435
		protected void OnLifetimeExpired()
		{
			if (this.onLifetimeExpired != null)
			{
				this.onLifetimeExpired();
			}
		}

		// Token: 0x04001E12 RID: 7698
		[Header("Variables")]
		[Range(0f, 1f)]
		public float intensity;

		// Token: 0x04001E13 RID: 7699
		[Range(0f, 1f)]
		public float speed;

		// Token: 0x04001E14 RID: 7700
		public Vector3 direction;

		// Token: 0x04001E15 RID: 7701
		[Header("Options")]
		public bool playOnStart;

		// Token: 0x04001E16 RID: 7702
		public float lifeTime;

		// Token: 0x04001E17 RID: 7703
		[Header("Detected Modules")]
		public List<FxModule> modules;

		// Token: 0x04001E18 RID: 7704
		[NonSerialized]
		public object source;

		// Token: 0x04001E1A RID: 7706
		protected bool initialized;

		// Token: 0x04001E1B RID: 7707
		[NonSerialized]
		public bool isPlaying;
	}
}

using System;
using System.Collections.Generic;
using Shadowood.RaycastTexture;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002F7 RID: 759
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/Water.html")]
	[AddComponentMenu("ThunderRoad/Levels/Water")]
	public class Water : MonoBehaviour
	{
		// Token: 0x0600242F RID: 9263 RVA: 0x000F74B8 File Offset: 0x000F56B8
		public bool TryGetWaterHeight(Vector3 point, out float waterHeight)
		{
			return this.raycastTexture.TryGetWaterHeight(point, out waterHeight);
		}

		// Token: 0x06002430 RID: 9264 RVA: 0x000F74C7 File Offset: 0x000F56C7
		private void OnValidate()
		{
			this.raycastTexture = base.GetComponentInChildren<RaycastTexture>();
		}

		// Token: 0x06002431 RID: 9265 RVA: 0x000F74D8 File Offset: 0x000F56D8
		private void Awake()
		{
			if (!this.raycastTexture)
			{
				Debug.LogErrorFormat(this, "Water component have no raycastTexture, disabling as it must be obsolete", Array.Empty<object>());
				base.gameObject.SetActive(false);
				base.enabled = false;
				return;
			}
			Water.all.Add(this);
			this.ConnectAreaEvents();
		}

		// Token: 0x06002432 RID: 9266 RVA: 0x000F7527 File Offset: 0x000F5727
		private void Start()
		{
			Water.quality = Water.Quality.Low;
			Water.SetQuality(Water.quality);
		}

		// Token: 0x06002433 RID: 9267 RVA: 0x000F7539 File Offset: 0x000F5739
		private void OnDestroy()
		{
			if (!this.raycastTexture)
			{
				return;
			}
			this.DisconnectAreaEvents();
			Water.all.Remove(this);
		}

		// Token: 0x06002434 RID: 9268 RVA: 0x000F755C File Offset: 0x000F575C
		public void ConnectAreaEvents()
		{
			if (this.connectedToAreaEvents)
			{
				return;
			}
			if (this.area == null)
			{
				this.area = base.GetComponentInParent<Area>();
			}
			if (this.area)
			{
				this.area.onPlayerEnter.AddListener(new UnityAction(this.OnPlayerEnterArea));
				this.area.onPlayerExit.AddListener(new UnityAction(this.OnPlayerExitArea));
				if (this.showWhenInRoomOnly)
				{
					base.gameObject.SetActive(AreaManager.Instance.CurrentArea == this.area.spawnableArea);
				}
				else
				{
					if (this.underWaterEffect != null)
					{
						this.underWaterEffect.SetActive(AreaManager.Instance.CurrentArea == this.area.spawnableArea);
					}
					this.area.onHideChange += this.OnAreaHideChange;
				}
			}
			this.connectedToAreaEvents = true;
		}

		// Token: 0x06002435 RID: 9269 RVA: 0x000F7648 File Offset: 0x000F5848
		public void DisconnectAreaEvents()
		{
			if (!this.connectedToAreaEvents)
			{
				return;
			}
			if (this.area)
			{
				this.area.onPlayerEnter.RemoveListener(new UnityAction(this.OnPlayerEnterArea));
				this.area.onPlayerExit.RemoveListener(new UnityAction(this.OnPlayerExitArea));
				this.area.onHideChange -= this.OnAreaHideChange;
			}
		}

		// Token: 0x06002436 RID: 9270 RVA: 0x000F76BA File Offset: 0x000F58BA
		private void OnPlayerEnterArea()
		{
			if (this.showWhenInRoomOnly)
			{
				base.gameObject.SetActive(true);
				return;
			}
			if (this.underWaterEffect != null)
			{
				this.underWaterEffect.SetActive(true);
			}
			this.SetAsCurrent();
		}

		// Token: 0x06002437 RID: 9271 RVA: 0x000F76F1 File Offset: 0x000F58F1
		private void OnPlayerExitArea()
		{
			if (this.showWhenInRoomOnly)
			{
				base.gameObject.SetActive(false);
				return;
			}
			if (this.underWaterEffect != null)
			{
				this.underWaterEffect.SetActive(false);
			}
			this.ReleaseAsCurrent();
		}

		// Token: 0x06002438 RID: 9272 RVA: 0x000F7728 File Offset: 0x000F5928
		private void OnAreaHideChange(bool isHide)
		{
			base.gameObject.SetActive(!isHide);
		}

		// Token: 0x06002439 RID: 9273 RVA: 0x000F773C File Offset: 0x000F593C
		public static void SetQuality(Water.Quality quality)
		{
			Water.quality = quality;
			foreach (Water ocean in Water.all)
			{
				if (ocean.raycastTexture)
				{
					if (quality == Water.Quality.Low)
					{
						ocean.raycastTexture.SetPlanarReflectionOff();
					}
					else if (quality == Water.Quality.High)
					{
						ocean.raycastTexture.SetPlanarReflectionOn();
					}
				}
			}
		}

		// Token: 0x0600243A RID: 9274 RVA: 0x000F77BC File Offset: 0x000F59BC
		public void RefreshPlatformSettings()
		{
		}

		// Token: 0x0600243B RID: 9275 RVA: 0x000F77BE File Offset: 0x000F59BE
		private void OnEnable()
		{
			if (this.area == null)
			{
				this.SetAsCurrent();
				return;
			}
			if (this.showWhenInRoomOnly)
			{
				this.SetAsCurrent();
			}
			Water.SetQuality(Water.quality);
		}

		// Token: 0x0600243C RID: 9276 RVA: 0x000F77ED File Offset: 0x000F59ED
		private void OnDisable()
		{
			this.ReleaseAsCurrent();
		}

		// Token: 0x0600243D RID: 9277 RVA: 0x000F77F5 File Offset: 0x000F59F5
		private void SetAsCurrent()
		{
			if (Water.current == this)
			{
				return;
			}
			if (!this.raycastTexture)
			{
				return;
			}
			Water.exist = true;
			Water.current = this;
			this.isActif = true;
		}

		// Token: 0x0600243E RID: 9278 RVA: 0x000F7828 File Offset: 0x000F5A28
		private void ReleaseAsCurrent()
		{
			if (Water.current != this)
			{
				return;
			}
			if (!this.raycastTexture)
			{
				return;
			}
			Water.exist = false;
			Water.current = null;
			this.isActif = false;
			foreach (Water ocean in Water.all)
			{
				if (ocean.isActif)
				{
					Water.exist = true;
					Water.current = ocean;
				}
			}
		}

		// Token: 0x0600243F RID: 9279 RVA: 0x000F78B8 File Offset: 0x000F5AB8
		public bool IsPositionAffected(Vector3 position)
		{
			return this.area == null || this.area.spawnableArea.Bounds.Contains(position);
		}

		// Token: 0x04002383 RID: 9091
		public RaycastTexture raycastTexture;

		// Token: 0x04002384 RID: 9092
		public GameObject underWaterEffect;

		// Token: 0x04002385 RID: 9093
		[Tooltip("When enabled, the Water will only be enabled when you enter the room (Dungeons Only)")]
		public bool showWhenInRoomOnly = true;

		// Token: 0x04002386 RID: 9094
		[NonSerialized]
		public bool waterHeightCanChangeovertime;

		// Token: 0x04002387 RID: 9095
		public static List<Water> all = new List<Water>();

		// Token: 0x04002388 RID: 9096
		public static Water current;

		// Token: 0x04002389 RID: 9097
		public static bool exist;

		// Token: 0x0400238A RID: 9098
		[NonSerialized]
		public static Water.Quality quality = Water.Quality.Low;

		// Token: 0x0400238B RID: 9099
		protected Area area;

		// Token: 0x0400238C RID: 9100
		protected bool connectedToAreaEvents;

		// Token: 0x0400238D RID: 9101
		protected bool isActif;

		// Token: 0x020009E3 RID: 2531
		public enum Quality
		{
			// Token: 0x04004642 RID: 17986
			Low,
			// Token: 0x04004643 RID: 17987
			High
		}
	}
}

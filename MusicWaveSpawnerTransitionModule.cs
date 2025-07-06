using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020001FC RID: 508
	[Serializable]
	public class MusicWaveSpawnerTransitionModule : MusicDynamicModule
	{
		// Token: 0x06001612 RID: 5650 RVA: 0x000984E4 File Offset: 0x000966E4
		public override void Connect()
		{
			base.Connect();
			WaveSpawner.OnWaveSpawnerEnabledEvent.AddListener(new UnityAction<WaveSpawner>(this.OnWaveSpawnerAdd));
			WaveSpawner.OnWaveSpawnerDisabledEvent.AddListener(new UnityAction<WaveSpawner>(this.OnWaveSpawnerRemoved));
			List<WaveSpawner> waveSpawnerList = WaveSpawner.instances;
			if (waveSpawnerList != null)
			{
				this._activeWaveCount = 0;
				int count = waveSpawnerList.Count;
				for (int i = 0; i < count; i++)
				{
					WaveSpawner waveSpawner = waveSpawnerList[i];
					if (waveSpawner.isRunning)
					{
						this._activeWaveCount++;
					}
					waveSpawner.OnWaveBeginEvent.AddListener(new UnityAction(this.OnWaveStart));
					waveSpawner.OnWaveAnyEndEvent.AddListener(new UnityAction(this.OnWaveStop));
				}
			}
			this.ChangeMusic();
		}

		// Token: 0x06001613 RID: 5651 RVA: 0x00098598 File Offset: 0x00096798
		public override void Disconnect()
		{
			base.Disconnect();
			WaveSpawner.OnWaveSpawnerEnabledEvent.RemoveListener(new UnityAction<WaveSpawner>(this.OnWaveSpawnerAdd));
			WaveSpawner.OnWaveSpawnerDisabledEvent.RemoveListener(new UnityAction<WaveSpawner>(this.OnWaveSpawnerRemoved));
			List<WaveSpawner> waveSpawnerList = WaveSpawner.instances;
			if (waveSpawnerList != null)
			{
				int count = waveSpawnerList.Count;
				for (int i = 0; i < count; i++)
				{
					WaveSpawner waveSpawner = waveSpawnerList[i];
					waveSpawner.OnWaveBeginEvent.RemoveListener(new UnityAction(this.OnWaveStart));
					waveSpawner.OnWaveAnyEndEvent.RemoveListener(new UnityAction(this.OnWaveStop));
				}
			}
			this._activeWaveCount = 0;
		}

		// Token: 0x06001614 RID: 5652 RVA: 0x00098630 File Offset: 0x00096830
		private void OnWaveSpawnerAdd(WaveSpawner waveSpawner)
		{
			if (waveSpawner.isRunning)
			{
				this._activeWaveCount++;
			}
			waveSpawner.OnWaveBeginEvent.AddListener(new UnityAction(this.OnWaveStart));
			waveSpawner.OnWaveAnyEndEvent.AddListener(new UnityAction(this.OnWaveStop));
			this.ChangeMusic();
		}

		// Token: 0x06001615 RID: 5653 RVA: 0x00098688 File Offset: 0x00096888
		private void OnWaveSpawnerRemoved(WaveSpawner waveSpawner)
		{
			if (waveSpawner.isRunning)
			{
				this._activeWaveCount--;
			}
			waveSpawner.OnWaveBeginEvent.RemoveListener(new UnityAction(this.OnWaveStart));
			waveSpawner.OnWaveAnyEndEvent.RemoveListener(new UnityAction(this.OnWaveStop));
			this.ChangeMusic();
		}

		// Token: 0x06001616 RID: 5654 RVA: 0x000986DF File Offset: 0x000968DF
		private void OnWaveStart()
		{
			this._activeWaveCount++;
			this.ChangeMusic();
		}

		// Token: 0x06001617 RID: 5655 RVA: 0x000986F5 File Offset: 0x000968F5
		private void OnWaveStop()
		{
			this._activeWaveCount--;
			if (this._activeWaveCount <= 0)
			{
				this._activeWaveCount = 0;
			}
			this.ChangeMusic();
		}

		// Token: 0x06001618 RID: 5656 RVA: 0x0009871B File Offset: 0x0009691B
		private void ChangeMusic()
		{
			if (this._activeWaveCount > 0)
			{
				ThunderBehaviourSingleton<MusicManager>.Instance.ChangeMusicTypeDelayed(this.waveMusicGroupId, 1);
				return;
			}
			ThunderBehaviourSingleton<MusicManager>.Instance.ChangeMusicType(this.ambianceMusicGroupId, 0, Music.MusicTransition.TransitionType.Immediate);
		}

		// Token: 0x040015BC RID: 5564
		public string ambianceMusicGroupId;

		// Token: 0x040015BD RID: 5565
		public string waveMusicGroupId;

		// Token: 0x040015BE RID: 5566
		private int _activeWaveCount;
	}
}

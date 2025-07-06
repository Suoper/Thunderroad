using System;
using UnityEngine;
using UnityEngine.Audio;

namespace ThunderRoad
{
	// Token: 0x020002CB RID: 715
	public class Fade : MonoBehaviour
	{
		// Token: 0x060022A6 RID: 8870 RVA: 0x000EE0BD File Offset: 0x000EC2BD
		public void Begin()
		{
			this.Begin(this.target);
		}

		// Token: 0x060022A7 RID: 8871 RVA: 0x000EE0CB File Offset: 0x000EC2CB
		public void End()
		{
			this.End(this.target);
		}

		// Token: 0x060022A8 RID: 8872 RVA: 0x000EE0D9 File Offset: 0x000EC2D9
		public void BeginCamera()
		{
			this.Begin(Fade.Target.CameraOnly);
		}

		// Token: 0x060022A9 RID: 8873 RVA: 0x000EE0E2 File Offset: 0x000EC2E2
		public void EndCamera()
		{
			this.End(Fade.Target.CameraOnly);
		}

		// Token: 0x060022AA RID: 8874 RVA: 0x000EE0EB File Offset: 0x000EC2EB
		public void BeginAudio()
		{
			this.Begin(Fade.Target.AudioOnly);
		}

		// Token: 0x060022AB RID: 8875 RVA: 0x000EE0F4 File Offset: 0x000EC2F4
		public void EndAudio()
		{
			this.End(Fade.Target.AudioOnly);
		}

		// Token: 0x060022AC RID: 8876 RVA: 0x000EE100 File Offset: 0x000EC300
		protected void Begin(Fade.Target target)
		{
			if (target != Fade.Target.AudioOnly)
			{
				CameraEffects.DoFadeEffect(true, this.fadeInDuration);
			}
			if (target != Fade.Target.CameraOnly)
			{
				Debug.Log("Fade Begin " + target.ToString());
				Player local = Player.local;
				bool flag;
				if (local == null)
				{
					flag = false;
				}
				else
				{
					Creature creature = local.creature;
					bool? flag2 = (creature != null) ? new bool?(creature.eyesUnderwater) : null;
					bool flag3 = true;
					flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
				}
				SnapshotTool.DoSnapshotTransition(flag ? ThunderRoadSettings.audioMixerSnapshotUnderwater : ThunderRoadSettings.audioMixerSnapshotDefault, 0f);
				SnapshotTool.DoSnapshotTransition(ThunderRoadSettings.audioMixerSnapshotMute, this.fadeInDuration);
			}
		}

		// Token: 0x060022AD RID: 8877 RVA: 0x000EE1A8 File Offset: 0x000EC3A8
		protected void End(Fade.Target target)
		{
			if (target != Fade.Target.AudioOnly)
			{
				CameraEffects.DoFadeEffect(false, this.fadeOutDuration);
			}
			if (target != Fade.Target.CameraOnly)
			{
				Debug.Log("Fade End " + target.ToString());
				Player local = Player.local;
				bool flag;
				if (local == null)
				{
					flag = false;
				}
				else
				{
					Creature creature = local.creature;
					bool? flag2 = (creature != null) ? new bool?(creature.eyesUnderwater) : null;
					bool flag3 = true;
					flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
				}
				AudioMixerSnapshot snapshot = flag ? ThunderRoadSettings.audioMixerSnapshotUnderwater : ThunderRoadSettings.audioMixerSnapshotDefault;
				SnapshotTool.DoSnapshotTransition(ThunderRoadSettings.audioMixerSnapshotMute, 0f);
				SnapshotTool.DoSnapshotTransition(snapshot, this.fadeOutDuration);
			}
		}

		// Token: 0x040021C0 RID: 8640
		public float fadeInDuration = 2f;

		// Token: 0x040021C1 RID: 8641
		public float fadeOutDuration = 2f;

		// Token: 0x040021C2 RID: 8642
		public Fade.Target target = Fade.Target.CameraAndAudio;

		// Token: 0x020009AE RID: 2478
		public enum Target
		{
			// Token: 0x0400457A RID: 17786
			CameraOnly,
			// Token: 0x0400457B RID: 17787
			AudioOnly,
			// Token: 0x0400457C RID: 17788
			CameraAndAudio
		}
	}
}

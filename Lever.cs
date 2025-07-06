using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002DC RID: 732
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Lever.html")]
	public class Lever : HingeDrive
	{
		// Token: 0x0600234B RID: 9035 RVA: 0x000F178E File Offset: 0x000EF98E
		private void Start()
		{
			this.onHingeMove.AddListener(new UnityAction<float, HingeDrive.HingeDriveSpeedState, float, float>(this.UpdateState));
		}

		// Token: 0x0600234C RID: 9036 RVA: 0x000F17A8 File Offset: 0x000EF9A8
		private void UpdateState(float angularVelocity, HingeDrive.HingeDriveSpeedState hingeDriveSpeedState, float angle, float angle01)
		{
			float a = Mathf.InverseLerp(this.minAngle, this.maxAngle, angle);
			if (a > this.deadZone)
			{
				if (this.state != Lever.State.Down)
				{
					this.leverDownEvent.Invoke();
					this.state = Lever.State.Down;
				}
			}
			else if (a < 1f - this.deadZone)
			{
				if (this.state != Lever.State.Up)
				{
					this.leverUpEvent.Invoke();
					this.state = Lever.State.Up;
				}
			}
			else
			{
				this.state = Lever.State.InBetween;
			}
			if (Math.Abs(a - this._lastAngle) > 0.01f)
			{
				this.leverAnalogEvent.Invoke(this.invertOutput ? (1f - a) : a);
				this._lastAngle = a;
			}
		}

		// Token: 0x04002256 RID: 8790
		[Header("Lever values")]
		[Range(0f, 1f)]
		public float deadZone = 0.8f;

		// Token: 0x04002257 RID: 8791
		public bool invertOutput = true;

		// Token: 0x04002258 RID: 8792
		[NonSerialized]
		public Lever.State state = Lever.State.InBetween;

		// Token: 0x04002259 RID: 8793
		[Header("Lever Related Events")]
		public UnityEvent leverUpEvent = new UnityEvent();

		// Token: 0x0400225A RID: 8794
		public UnityEvent leverDownEvent = new UnityEvent();

		// Token: 0x0400225B RID: 8795
		public UnityEvent<float> leverAnalogEvent = new UnityEvent<float>();

		// Token: 0x0400225C RID: 8796
		private float _lastAngle;

		// Token: 0x020009C4 RID: 2500
		public enum State
		{
			// Token: 0x040045D7 RID: 17879
			Up,
			// Token: 0x040045D8 RID: 17880
			Down,
			// Token: 0x040045D9 RID: 17881
			InBetween
		}
	}
}

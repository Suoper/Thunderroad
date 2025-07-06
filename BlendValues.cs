using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002BA RID: 698
	public class BlendValues : MonoBehaviour
	{
		// Token: 0x060021D9 RID: 8665 RVA: 0x000E9360 File Offset: 0x000E7560
		public void SetValue1(float value)
		{
			this.value1 = value;
			this.changeDetected = true;
		}

		// Token: 0x060021DA RID: 8666 RVA: 0x000E9370 File Offset: 0x000E7570
		public void SetValue2(float value)
		{
			this.value2 = value;
			this.changeDetected = true;
		}

		// Token: 0x060021DB RID: 8667 RVA: 0x000E9380 File Offset: 0x000E7580
		public void SetValue3(float value)
		{
			this.value3 = value;
			this.changeDetected = true;
		}

		// Token: 0x060021DC RID: 8668 RVA: 0x000E9390 File Offset: 0x000E7590
		public void SetValue4(float value)
		{
			this.value4 = value;
			this.changeDetected = true;
		}

		// Token: 0x060021DD RID: 8669 RVA: 0x000E93A0 File Offset: 0x000E75A0
		public void SetValue5(float value)
		{
			this.value5 = value;
			this.changeDetected = true;
		}

		// Token: 0x060021DE RID: 8670 RVA: 0x000E93B0 File Offset: 0x000E75B0
		private void LateUpdate()
		{
			if (this.changeDetected)
			{
				this.Refresh();
				this.changeDetected = false;
			}
		}

		// Token: 0x060021DF RID: 8671 RVA: 0x000E93C8 File Offset: 0x000E75C8
		public void Refresh()
		{
			if (this.blendMode == BlendMode.Min)
			{
				if (this.valueCount == 2)
				{
					this.outputValue = Mathf.Min(this.value1, this.value2);
				}
				else if (this.valueCount == 3)
				{
					this.outputValue = Mathf.Min(new float[]
					{
						this.value1,
						this.value2,
						this.value3
					});
				}
				else if (this.valueCount == 4)
				{
					this.outputValue = Mathf.Min(new float[]
					{
						this.value1,
						this.value2,
						this.value3,
						this.value4
					});
				}
				else if (this.valueCount == 5)
				{
					this.outputValue = Mathf.Min(new float[]
					{
						this.value1,
						this.value2,
						this.value3,
						this.value4,
						this.value5
					});
				}
			}
			else if (this.blendMode == BlendMode.Max)
			{
				if (this.valueCount == 2)
				{
					this.outputValue = Mathf.Max(this.value1, this.value2);
				}
				else if (this.valueCount == 3)
				{
					this.outputValue = Mathf.Max(new float[]
					{
						this.value1,
						this.value2,
						this.value3
					});
				}
				else if (this.valueCount == 4)
				{
					this.outputValue = Mathf.Max(new float[]
					{
						this.value1,
						this.value2,
						this.value3,
						this.value4
					});
				}
				else if (this.valueCount == 5)
				{
					this.outputValue = Mathf.Max(new float[]
					{
						this.value1,
						this.value2,
						this.value3,
						this.value4,
						this.value5
					});
				}
			}
			else if (this.blendMode == BlendMode.Average)
			{
				if (this.valueCount == 2)
				{
					this.outputValue = (this.value1 + this.value2) / 2f;
				}
				else if (this.valueCount == 3)
				{
					this.outputValue = (this.value1 + this.value2 + this.value3) / 3f;
				}
				else if (this.valueCount == 4)
				{
					this.outputValue = (this.value1 + this.value2 + this.value3 + this.value4) / 4f;
				}
				else if (this.valueCount == 5)
				{
					this.outputValue = (this.value1 + this.value2 + this.value3 + this.value4 + this.value5) / 5f;
				}
			}
			else if (this.blendMode == BlendMode.Multiply)
			{
				if (this.valueCount == 2)
				{
					this.outputValue = this.value1 * this.value2;
				}
				else if (this.valueCount == 3)
				{
					this.outputValue = this.value1 * this.value2 * this.value3;
				}
				else if (this.valueCount == 4)
				{
					this.outputValue = this.value1 * this.value2 * this.value3 * this.value4;
				}
				else if (this.valueCount == 5)
				{
					this.outputValue = this.value1 * this.value2 * this.value3 * this.value4 * this.value5;
				}
			}
			UnityEvent<float> unityEvent = this.output;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(this.outputValue);
		}

		// Token: 0x040020C1 RID: 8385
		public BlendMode blendMode = BlendMode.Average;

		// Token: 0x040020C2 RID: 8386
		[Range(2f, 5f)]
		public int valueCount = 2;

		// Token: 0x040020C3 RID: 8387
		public UnityEvent<float> output = new UnityEvent<float>();

		// Token: 0x040020C4 RID: 8388
		protected float value1;

		// Token: 0x040020C5 RID: 8389
		protected float value2;

		// Token: 0x040020C6 RID: 8390
		protected float value3;

		// Token: 0x040020C7 RID: 8391
		protected float value4;

		// Token: 0x040020C8 RID: 8392
		protected float value5;

		// Token: 0x040020C9 RID: 8393
		protected float outputValue;

		// Token: 0x040020CA RID: 8394
		protected bool changeDetected;
	}
}

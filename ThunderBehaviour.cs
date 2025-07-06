using System;
using UnityEngine;

namespace ThunderRoad
{
	/// <summary>
	/// Scripts which use this implementation of MonoBehaviour will be subscribed to the managed update loops
	/// </summary>
	///             https://docs.unity3d.com/Manual/ExecutionOrder.html
	// Token: 0x02000372 RID: 882
	public class ThunderBehaviour : MonoBehaviour
	{
		// Token: 0x1700027E RID: 638
		// (get) Token: 0x060029E8 RID: 10728 RVA: 0x0011D0EC File Offset: 0x0011B2EC
		public string ThunderBehaviourTypeName
		{
			get
			{
				string result;
				if ((result = this._typeName) == null)
				{
					result = (this._typeName = base.GetType().Name);
				}
				return result;
			}
		}

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x060029E9 RID: 10729 RVA: 0x0011D117 File Offset: 0x0011B317
		public Type ThunderBehaviourType
		{
			get
			{
				if (this._type == null)
				{
					this._type = base.GetType();
				}
				return this._type;
			}
		}

		/// <summary>
		/// Overrides Unitys gameObject engine call and gets the cached gameObject for this ThunderBehaviour
		/// </summary>
		// Token: 0x17000280 RID: 640
		// (get) Token: 0x060029EA RID: 10730 RVA: 0x0011D133 File Offset: 0x0011B333
		public new GameObject gameObject
		{
			get
			{
				if (this._gameObject == null && this._gameObject == null)
				{
					this._gameObject = base.gameObject;
				}
				return this._gameObject;
			}
		}

		/// <summary>
		/// Overrides Unitys transform engine call and gets the cached transform for this ThunderBehaviour
		/// </summary>
		// Token: 0x17000281 RID: 641
		// (get) Token: 0x060029EB RID: 10731 RVA: 0x0011D15D File Offset: 0x0011B35D
		public new Transform transform
		{
			get
			{
				if (this._transform == null && this._transform == null)
				{
					this._transform = base.transform;
				}
				return this._transform;
			}
		}

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x060029EC RID: 10732 RVA: 0x0011D187 File Offset: 0x0011B387
		public Transform baseTransform
		{
			get
			{
				return base.transform;
			}
		}

		/// <summary>
		/// Defines which loops will be executed on this behaviour. It is checked during OnEnable and OnDisable
		/// </summary>
		// Token: 0x17000283 RID: 643
		// (get) Token: 0x060029ED RID: 10733 RVA: 0x0011D18F File Offset: 0x0011B38F
		public virtual ManagedLoops EnabledManagedLoops
		{
			get
			{
				return (ManagedLoops)0;
			}
		}

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x060029EE RID: 10734 RVA: 0x0011D192 File Offset: 0x0011B392
		protected virtual int SliceOverNumFrames
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x060029EF RID: 10735 RVA: 0x0011D195 File Offset: 0x0011B395
		protected virtual int GetNextTimeSliceId
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x060029F0 RID: 10736 RVA: 0x0011D198 File Offset: 0x0011B398
		private void SetTimeSliceId()
		{
			this.TimeSliceId = this.GetNextTimeSliceId % this.SliceOverNumFrames;
			this.TimeSliceFrames = this.SliceOverNumFrames;
		}

		// Token: 0x060029F1 RID: 10737 RVA: 0x0011D1B9 File Offset: 0x0011B3B9
		public void OnEnable()
		{
			this._transform = base.transform;
			this._gameObject = base.gameObject;
			this.SetTimeSliceId();
			UpdateManager.AddBehaviour(this);
			this.ManagedOnEnable();
		}

		// Token: 0x060029F2 RID: 10738 RVA: 0x0011D1E5 File Offset: 0x0011B3E5
		public void OnDisable()
		{
			UpdateManager.RemoveBehaviour(this);
			this.ManagedOnDisable();
		}

		// Token: 0x060029F3 RID: 10739 RVA: 0x0011D1F3 File Offset: 0x0011B3F3
		protected virtual void ManagedOnEnable()
		{
		}

		// Token: 0x060029F4 RID: 10740 RVA: 0x0011D1F5 File Offset: 0x0011B3F5
		protected virtual void ManagedOnDisable()
		{
		}

		// Token: 0x060029F5 RID: 10741 RVA: 0x0011D1F7 File Offset: 0x0011B3F7
		internal void SetIndex(ManagedLoops loops, int index)
		{
			switch (loops)
			{
			case ManagedLoops.FixedUpdate:
				this.fixedIndex = index;
				return;
			case ManagedLoops.Update:
				this.updateIndex = index;
				return;
			case ManagedLoops.FixedUpdate | ManagedLoops.Update:
				break;
			case ManagedLoops.LateUpdate:
				this.lateUpdateIndex = index;
				break;
			default:
				return;
			}
		}

		// Token: 0x060029F6 RID: 10742 RVA: 0x0011D22C File Offset: 0x0011B42C
		internal int GetIndex(ManagedLoops loops)
		{
			switch (loops)
			{
			case ManagedLoops.FixedUpdate:
				return this.fixedIndex;
			case ManagedLoops.Update:
				return this.updateIndex;
			case ManagedLoops.LateUpdate:
				return this.lateUpdateIndex;
			}
			return -1;
		}

		// Token: 0x060029F7 RID: 10743 RVA: 0x0011D271 File Offset: 0x0011B471
		protected internal virtual void ManagedFixedUpdate()
		{
		}

		// Token: 0x060029F8 RID: 10744 RVA: 0x0011D273 File Offset: 0x0011B473
		protected internal virtual void ManagedUpdate()
		{
		}

		// Token: 0x060029F9 RID: 10745 RVA: 0x0011D275 File Offset: 0x0011B475
		protected internal virtual void ManagedLateUpdate()
		{
		}

		// Token: 0x040027BC RID: 10172
		private string _typeName;

		// Token: 0x040027BD RID: 10173
		private Type _type;

		// Token: 0x040027BE RID: 10174
		protected GameObject _gameObject;

		// Token: 0x040027BF RID: 10175
		protected Transform _transform;

		// Token: 0x040027C0 RID: 10176
		private int fixedIndex = -1;

		// Token: 0x040027C1 RID: 10177
		private int updateIndex = -1;

		// Token: 0x040027C2 RID: 10178
		private int lateUpdateIndex = -1;

		// Token: 0x040027C3 RID: 10179
		[NonSerialized]
		public int TimeSliceId;

		// Token: 0x040027C4 RID: 10180
		[NonSerialized]
		public int TimeSliceFrames = 1;
	}
}

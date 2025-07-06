using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200034F RID: 847
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/LiquidReceiver.html")]
	[AddComponentMenu("ThunderRoad/Liquid receiver")]
	public class LiquidReceiver : MonoBehaviour
	{
		// Token: 0x0600278C RID: 10124 RVA: 0x00110CEC File Offset: 0x0010EEEC
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		/// <summary>
		/// Invoked when liquid is consumed.
		/// </summary>
		// Token: 0x14000130 RID: 304
		// (add) Token: 0x0600278D RID: 10125 RVA: 0x00110CFC File Offset: 0x0010EEFC
		// (remove) Token: 0x0600278E RID: 10126 RVA: 0x00110D34 File Offset: 0x0010EF34
		public event LiquidReceiver.ReceptionEvent OnReceptionEvent;

		/// <summary>
		/// This mouth relay.
		/// </summary>
		// Token: 0x17000257 RID: 599
		// (get) Token: 0x0600278F RID: 10127 RVA: 0x00110D69 File Offset: 0x0010EF69
		public CreatureMouthRelay Relay
		{
			get
			{
				return this.mouthRelay;
			}
		}

		// Token: 0x06002790 RID: 10128 RVA: 0x00110D74 File Offset: 0x0010EF74
		private void Awake()
		{
			base.gameObject.layer = GameManager.GetLayer(LayerName.LiquidFlow);
			this.mouthRelay = base.GetComponentInParent<CreatureMouthRelay>();
			if (this.mouthRelay != null)
			{
				this.mouthRelay.OnRelayUpdateEvent += this.OnRelayUpdate;
				CreatureMouthRelay creatureMouthRelay = this.mouthRelay;
				creatureMouthRelay.OnParticleCollideEvent = (CreatureMouthRelay.OnParticleCollide)Delegate.Combine(creatureMouthRelay.OnParticleCollideEvent, new CreatureMouthRelay.OnParticleCollide(this.OnLiquidReceived));
			}
			this.liquidContainer = base.GetComponentInParent<LiquidContainer>();
			if (!string.IsNullOrEmpty(this.drinkEffectId))
			{
				EffectData drinkEffectData = Catalog.GetData<EffectData>(this.drinkEffectId, true);
				if (drinkEffectData != null)
				{
					this.effectInstance = drinkEffectData.Spawn(base.transform, true, null, false);
				}
			}
		}

		// Token: 0x06002791 RID: 10129 RVA: 0x00110E2A File Offset: 0x0010F02A
		private void OnRelayUpdate()
		{
			if (this.isReceiving && Time.time - this.lastLiquidReceivedTime >= this.stopDelay)
			{
				if (this.effectInstance != null)
				{
					this.effectInstance.Stop(0);
				}
				this.isReceiving = false;
			}
		}

		// Token: 0x06002792 RID: 10130 RVA: 0x00110E64 File Offset: 0x0010F064
		public virtual void OnLiquidReceived(GameObject other)
		{
			LiquidContainer liquidContainer = other.GetComponentInParent<LiquidContainer>();
			if (liquidContainer == null || Vector3.Angle(Vector3.up, base.transform.up) > this.maxAngle)
			{
				return;
			}
			this.lastLiquidReceivedTime = Time.time;
			this.isReceiving = true;
			if (Time.time - this.lastEffectTime > this.effectRate)
			{
				float currentLevel = liquidContainer.GetLiquidLevel();
				LiquidContainer liquidContainer2 = liquidContainer;
				CreatureMouthRelay creatureMouthRelay = this.mouthRelay;
				EventManager.InvokeLiquidConsumed(liquidContainer2, (creatureMouthRelay != null) ? creatureMouthRelay.creature : null, EventTime.OnStart);
				foreach (LiquidData.Content content in liquidContainer.contents)
				{
					float dilution = content.level / currentLevel;
					dilution = (float.IsNaN(dilution) ? 0f : dilution);
					content.liquidData.OnLiquidReception(this, dilution, liquidContainer);
					LiquidReceiver.ReceptionEvent onReceptionEvent = this.OnReceptionEvent;
					if (onReceptionEvent != null)
					{
						onReceptionEvent(content.liquidData, dilution, liquidContainer);
					}
				}
				this.lastEffectTime = Time.time;
				LiquidContainer liquidContainer3 = liquidContainer;
				CreatureMouthRelay creatureMouthRelay2 = this.mouthRelay;
				EventManager.InvokeLiquidConsumed(liquidContainer3, (creatureMouthRelay2 != null) ? creatureMouthRelay2.creature : null, EventTime.OnEnd);
				if (this.effectInstance != null)
				{
					this.effectInstance.Play(0, false, false);
				}
			}
		}

		// Token: 0x06002793 RID: 10131 RVA: 0x00110FA8 File Offset: 0x0010F1A8
		private void OnParticleCollision(GameObject other)
		{
			if (!base.enabled)
			{
				return;
			}
			CreatureMouthRelay.OnParticleCollide onParticleCollideEvent = this.Relay.OnParticleCollideEvent;
			if (onParticleCollideEvent == null)
			{
				return;
			}
			onParticleCollideEvent(other);
		}

		// Token: 0x040026A6 RID: 9894
		public string drinkEffectId;

		// Token: 0x040026A7 RID: 9895
		public float maxAngle = 30f;

		// Token: 0x040026A8 RID: 9896
		public float stopDelay = 0.1f;

		// Token: 0x040026A9 RID: 9897
		public float effectRate = 1f;

		// Token: 0x040026AA RID: 9898
		protected CreatureMouthRelay mouthRelay;

		// Token: 0x040026AB RID: 9899
		protected EffectInstance effectInstance;

		// Token: 0x040026AC RID: 9900
		protected LiquidContainer liquidContainer;

		// Token: 0x040026AD RID: 9901
		protected float lastLiquidReceivedTime;

		// Token: 0x040026AE RID: 9902
		protected float lastEffectTime;

		// Token: 0x040026AF RID: 9903
		protected bool isReceiving;

		// Token: 0x02000A40 RID: 2624
		// (Invoke) Token: 0x060045AB RID: 17835
		public delegate void ReceptionEvent(LiquidData liquid, float dilution, LiquidContainer liquidContainer);
	}
}

using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200028C RID: 652
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectParticleChild.html")]
	public class EffectParticleChild : MonoBehaviour
	{
		// Token: 0x04001D75 RID: 7541
		[Header("Color Gradient")]
		public EffectTarget linkStartColor;

		// Token: 0x04001D76 RID: 7542
		public EffectTarget linkStartGradient;

		// Token: 0x04001D77 RID: 7543
		public EffectTarget linkEmissionColor;

		// Token: 0x04001D78 RID: 7544
		public EffectTarget linkBaseColor;

		// Token: 0x04001D79 RID: 7545
		public EffectTarget linkTintColor;

		// Token: 0x04001D7A RID: 7546
		public string targetPartName;

		// Token: 0x04001D7B RID: 7547
		public bool emitEffectOnCollision;

		// Token: 0x04001D7C RID: 7548
		public bool ignoreAlpha;

		// Token: 0x04001D7D RID: 7549
		[Header("Intensity to duration")]
		public bool duration;

		// Token: 0x04001D7E RID: 7550
		public AnimationCurve curveDuration;

		// Token: 0x04001D7F RID: 7551
		[Header("Intensity to lifetime")]
		public bool lifeTime;

		// Token: 0x04001D80 RID: 7552
		public AnimationCurve curveLifeTime;

		// Token: 0x04001D81 RID: 7553
		public float randomRangeLifeTime;

		// Token: 0x04001D82 RID: 7554
		[Header("Intensity to speed")]
		public bool speed;

		// Token: 0x04001D83 RID: 7555
		public AnimationCurve curveSpeed;

		// Token: 0x04001D84 RID: 7556
		public float randomRangeSpeed;

		// Token: 0x04001D85 RID: 7557
		[Header("Intensity to size")]
		public bool size;

		// Token: 0x04001D86 RID: 7558
		public AnimationCurve curveSize;

		// Token: 0x04001D87 RID: 7559
		public float randomRangeSize;

		// Token: 0x04001D88 RID: 7560
		[Header("Intensity to emission rate")]
		public bool rate;

		// Token: 0x04001D89 RID: 7561
		public AnimationCurve curveRate;

		// Token: 0x04001D8A RID: 7562
		public bool rateOverDistance;

		// Token: 0x04001D8B RID: 7563
		public AnimationCurve curveDistanceRate;

		// Token: 0x04001D8C RID: 7564
		public float randomRangeRate;

		// Token: 0x04001D8D RID: 7565
		[Header("Intensity to alpha")]
		public bool alpha;

		// Token: 0x04001D8E RID: 7566
		public AnimationCurve curveAlpha;

		// Token: 0x04001D8F RID: 7567
		[Header("Intensity to shape radius")]
		public bool shapeRadius;

		// Token: 0x04001D90 RID: 7568
		public AnimationCurve curveShapeRadius;

		// Token: 0x04001D91 RID: 7569
		[Header("Intensity to shape arc")]
		public bool shapeArc;

		// Token: 0x04001D92 RID: 7570
		public AnimationCurve curveShapeArc;

		// Token: 0x04001D93 RID: 7571
		[Header("Intensity to burst")]
		public bool burst;

		// Token: 0x04001D94 RID: 7572
		public AnimationCurve curveBurst;

		// Token: 0x04001D95 RID: 7573
		public short randomRangeBurst;

		// Token: 0x04001D96 RID: 7574
		[Header("Intensity to velocity over lifetime")]
		public bool velocityOverLifetime;

		// Token: 0x04001D97 RID: 7575
		public AnimationCurve curvevelocityOverLifetime;

		// Token: 0x04001D98 RID: 7576
		[Header("Intensity to light intensity")]
		public bool lightIntensity;

		// Token: 0x04001D99 RID: 7577
		public AnimationCurve curveLightIntensity;

		// Token: 0x04001D9A RID: 7578
		[Header("Mesh")]
		public bool mesh;

		// Token: 0x04001D9B RID: 7579
		[Header("Renderer")]
		public EffectTarget useRenderer;

		// Token: 0x04001D9C RID: 7580
		[Header("Collider")]
		public bool collider;

		// Token: 0x04001D9D RID: 7581
		[Header("Collision")]
		public bool sendCollisionEvents;

		// Token: 0x04001D9E RID: 7582
		[NonSerialized]
		public ParticleSystemRenderer particleRenderer;

		// Token: 0x04001D9F RID: 7583
		[NonSerialized]
		public MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04001DA0 RID: 7584
		[NonSerialized]
		public ParticleSystem particleSystem;

		// Token: 0x04001DA1 RID: 7585
		[NonSerialized]
		public ParticleCollisionDetector particleCollisionDetector;

		// Token: 0x04001DA2 RID: 7586
		[NonSerialized]
		public ParticleCollisionSpawner particleCollisionSpawner;
	}
}

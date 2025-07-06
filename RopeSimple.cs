using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002F0 RID: 752
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/RopeSimple.html")]
	public class RopeSimple : MonoBehaviour
	{
		// Token: 0x06002400 RID: 9216 RVA: 0x000F6260 File Offset: 0x000F4460
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x06002401 RID: 9217 RVA: 0x000F6270 File Offset: 0x000F4470
		public void Awake()
		{
			this.rb = base.GetComponentInParent<Rigidbody>();
			if (!this.rb)
			{
				Debug.LogError("RopeSimple component need to have a parent rigidbody");
				base.enabled = false;
				return;
			}
			if (!this.targetAnchor)
			{
				Debug.LogError("RopeSimple component doesn't have any target anchor set!", this);
				base.enabled = false;
				return;
			}
			this.springJoint = this.rb.gameObject.AddComponent<SpringJoint>();
			this.springJoint.spring = this.spring;
			this.springJoint.damper = this.damper;
			this.springJoint.minDistance = this.minDistance;
			this.springJoint.maxDistance = ((this.maxDistance <= 0f) ? Vector3.Distance(base.transform.position, this.targetAnchor.position) : this.maxDistance);
			this.springJoint.anchor = this.rb.transform.InverseTransformPoint(base.transform.position);
			this.springJoint.autoConfigureConnectedAnchor = false;
			this.springJoint.connectedAnchor = this.targetAnchor.position;
			this.springJoint.connectedBody = this.connectedBody;
			this.mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder).GetComponent<MeshRenderer>();
			UnityEngine.Object.DestroyImmediate(this.mesh.GetComponent<Collider>());
			this.meshTransform = this.mesh.transform;
			this.meshTransform.SetParent(this.targetAnchor);
			this.mesh.name = "RopeMesh";
			this.mesh.material = this.material;
			this.lightVolumeReceiver = this.mesh.gameObject.AddComponent<LightVolumeReceiver>();
			this.lightVolumeReceiver.volumeDetection = LightVolumeReceiver.VolumeDetection.StaticPerMesh;
			EffectData effectData = Catalog.GetData<EffectData>(this.effectId, true);
			if (effectData != null)
			{
				this.effectInstance = effectData.Spawn(base.transform.position, base.transform.rotation, base.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
				this.effectInstance.SetIntensity(0f);
			}
		}

		// Token: 0x06002402 RID: 9218 RVA: 0x000F6488 File Offset: 0x000F4688
		private void Start()
		{
			this.ropeScaling.x = 1f;
			this.ropeScaling.y = 1f;
			this.ropeScaling.z = 0f;
			this.ropeScaling.w = 0f;
		}

		// Token: 0x06002403 RID: 9219 RVA: 0x000F64D5 File Offset: 0x000F46D5
		protected void OnDisable()
		{
			if (this.effectInstance != null)
			{
				this.effectInstance.Stop(0);
			}
		}

		// Token: 0x06002404 RID: 9220 RVA: 0x000F64EC File Offset: 0x000F46EC
		protected void LateUpdate()
		{
			if (this.rb.IsSleeping())
			{
				if (this.effectInstance != null && this.effectInstance.isPlaying)
				{
					this.effectInstance.Stop(0);
				}
				return;
			}
			Vector3 position = base.transform.position;
			Vector3 targetAnchorPosition = this.targetAnchor.position;
			this.meshTransform.position = Vector3.Lerp(position, targetAnchorPosition, 0.5f);
			this.meshTransform.rotation = Quaternion.FromToRotation(this.mesh.transform.TransformDirection(Vector3.up), targetAnchorPosition - position) * this.mesh.transform.rotation;
			float distance = Vector3.Distance(position, targetAnchorPosition);
			this.mesh.transform.localScale = new Vector3(this.radius, distance * 0.5f, this.radius);
			foreach (MaterialInstance materialInstance in this.lightVolumeReceiver.materialInstances)
			{
				if (materialInstance.CachedRenderer && materialInstance.CachedRenderer.isVisible)
				{
					this.ropeScaling.y = distance * this.tilingOffset;
					materialInstance.material.SetVector(RopeSimple.BaseMapSt, this.ropeScaling);
				}
			}
			if (this.effectInstance != null)
			{
				float audioIntensity = Mathf.InverseLerp(this.audioMinForce, this.audioMaxForce, this.springJoint.currentForce.magnitude) * Mathf.InverseLerp(this.audioMinSpeed, this.audioMaxSpeed, this.rb.velocity.magnitude);
				this.effectInstance.SetIntensity(audioIntensity);
				if (!this.effectInstance.isPlaying)
				{
					this.effectInstance.Play(0, false, false);
				}
			}
		}

		// Token: 0x06002405 RID: 9221 RVA: 0x000F66D0 File Offset: 0x000F48D0
		protected void OnDrawGizmos()
		{
			if (this.targetAnchor)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(base.transform.position, this.targetAnchor.position);
			}
		}

		// Token: 0x04002332 RID: 9010
		[Tooltip("Depicts what connects to the rope. It connects from the origin point of the transform of the Rope Simple script, to the object.\n\nFor example, the RopeSimple script is attached to a chandelier, of which the target anchor is a point on a ceiling that the chandelier is connected to.")]
		public Transform targetAnchor;

		// Token: 0x04002333 RID: 9011
		[Header("Spring joint")]
		[Tooltip("The spring modifier of the rope. Depicts how springy the rope is")]
		public float spring = 10000f;

		// Token: 0x04002334 RID: 9012
		[Tooltip("The damper modifier of the rope. Depicts how much of a dampener the rope has")]
		public float damper;

		// Token: 0x04002335 RID: 9013
		[Tooltip("The minimum distance of the rope. 0 means default length between the Rope Simple transform and the target anchor.")]
		public float minDistance;

		// Token: 0x04002336 RID: 9014
		[Tooltip("The maximum distance of the rope. 0 means default length between the Rope Simple transform and the target anchor.")]
		public float maxDistance;

		// Token: 0x04002337 RID: 9015
		[Tooltip("Instead of using the transform of Rope Simple, you can instead reference a specific rigidbody/item.")]
		public Rigidbody connectedBody;

		// Token: 0x04002338 RID: 9016
		[Header("Rope mesh")]
		[Tooltip("Depicts the radius of the rope mesh.")]
		public float radius = 0.015f;

		// Token: 0x04002339 RID: 9017
		[Tooltip("Depicts the material tiling offset.")]
		public float tilingOffset = 10f;

		// Token: 0x0400233A RID: 9018
		[Tooltip("Depicts the material that the rope uses.")]
		public Material material;

		// Token: 0x0400233B RID: 9019
		[Header("Audio")]
		[Tooltip("Depicts the Effect ID from JSON to make sounds when the rope moves")]
		public string effectId;

		// Token: 0x0400233C RID: 9020
		[Tooltip("The minimum force required to play the Effect sound")]
		public float audioMinForce = 400f;

		// Token: 0x0400233D RID: 9021
		[Tooltip("The maximum sound as of which plays the maximum volume of the audio")]
		public float audioMaxForce = 1000f;

		// Token: 0x0400233E RID: 9022
		[Tooltip("Depicts the minimum speed the rope needs to go to play the audio")]
		public float audioMinSpeed = 0.25f;

		// Token: 0x0400233F RID: 9023
		[Tooltip("Depicts the maximum speed the rope needs to go to play the audio at its' max volume")]
		public float audioMaxSpeed = 2f;

		// Token: 0x04002340 RID: 9024
		private Rigidbody rb;

		// Token: 0x04002341 RID: 9025
		private MeshRenderer mesh;

		// Token: 0x04002342 RID: 9026
		private SpringJoint springJoint;

		// Token: 0x04002343 RID: 9027
		private Transform meshTransform;

		// Token: 0x04002344 RID: 9028
		private static readonly int BaseMapSt = Shader.PropertyToID("_BaseMap_ST");

		// Token: 0x04002345 RID: 9029
		private Vector4 ropeScaling = Vector4.zero;

		// Token: 0x04002346 RID: 9030
		private LightVolumeReceiver lightVolumeReceiver;

		// Token: 0x04002347 RID: 9031
		private EffectInstance effectInstance;
	}
}

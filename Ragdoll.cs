using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RainyReignGames.MeshDismemberment;
using ThunderRoad.Reveal;
using ThunderRoad.Skill.SpellPower;
using Unity.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000278 RID: 632
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Ragdoll")]
	[AddComponentMenu("ThunderRoad/Creatures/Ragdoll")]
	public class Ragdoll : ThunderBehaviour
	{
		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06001CC9 RID: 7369 RVA: 0x000C151F File Offset: 0x000BF71F
		public ConfigurableJoint StabilizationJoint
		{
			get
			{
				return this.stabilizationJoint;
			}
		}

		// Token: 0x140000D0 RID: 208
		// (add) Token: 0x06001CCA RID: 7370 RVA: 0x000C1528 File Offset: 0x000BF728
		// (remove) Token: 0x06001CCB RID: 7371 RVA: 0x000C1560 File Offset: 0x000BF760
		public event Ragdoll.StateChange OnStateChange;

		// Token: 0x140000D1 RID: 209
		// (add) Token: 0x06001CCC RID: 7372 RVA: 0x000C1598 File Offset: 0x000BF798
		// (remove) Token: 0x06001CCD RID: 7373 RVA: 0x000C15D0 File Offset: 0x000BF7D0
		public event Ragdoll.SliceEvent OnSliceEvent;

		// Token: 0x140000D2 RID: 210
		// (add) Token: 0x06001CCE RID: 7374 RVA: 0x000C1608 File Offset: 0x000BF808
		// (remove) Token: 0x06001CCF RID: 7375 RVA: 0x000C1640 File Offset: 0x000BF840
		public event Ragdoll.TouchActionDelegate OnTouchActionEvent;

		// Token: 0x140000D3 RID: 211
		// (add) Token: 0x06001CD0 RID: 7376 RVA: 0x000C1678 File Offset: 0x000BF878
		// (remove) Token: 0x06001CD1 RID: 7377 RVA: 0x000C16B0 File Offset: 0x000BF8B0
		public event Ragdoll.HeldActionDelegate OnHeldActionEvent;

		// Token: 0x140000D4 RID: 212
		// (add) Token: 0x06001CD2 RID: 7378 RVA: 0x000C16E8 File Offset: 0x000BF8E8
		// (remove) Token: 0x06001CD3 RID: 7379 RVA: 0x000C1720 File Offset: 0x000BF920
		public event Ragdoll.TelekinesisGrabEvent OnTelekinesisGrabEvent;

		// Token: 0x140000D5 RID: 213
		// (add) Token: 0x06001CD4 RID: 7380 RVA: 0x000C1758 File Offset: 0x000BF958
		// (remove) Token: 0x06001CD5 RID: 7381 RVA: 0x000C1790 File Offset: 0x000BF990
		public event Ragdoll.TelekinesisReleaseEvent OnTelekinesisReleaseEvent;

		// Token: 0x140000D6 RID: 214
		// (add) Token: 0x06001CD6 RID: 7382 RVA: 0x000C17C8 File Offset: 0x000BF9C8
		// (remove) Token: 0x06001CD7 RID: 7383 RVA: 0x000C1800 File Offset: 0x000BFA00
		public event Ragdoll.GrabEvent OnGrabEvent;

		// Token: 0x140000D7 RID: 215
		// (add) Token: 0x06001CD8 RID: 7384 RVA: 0x000C1838 File Offset: 0x000BFA38
		// (remove) Token: 0x06001CD9 RID: 7385 RVA: 0x000C1870 File Offset: 0x000BFA70
		public event Ragdoll.UngrabEvent OnUngrabEvent;

		// Token: 0x140000D8 RID: 216
		// (add) Token: 0x06001CDA RID: 7386 RVA: 0x000C18A8 File Offset: 0x000BFAA8
		// (remove) Token: 0x06001CDB RID: 7387 RVA: 0x000C18E0 File Offset: 0x000BFAE0
		public event Ragdoll.ContactEvent OnContactStartEvent;

		// Token: 0x140000D9 RID: 217
		// (add) Token: 0x06001CDC RID: 7388 RVA: 0x000C1918 File Offset: 0x000BFB18
		// (remove) Token: 0x06001CDD RID: 7389 RVA: 0x000C1950 File Offset: 0x000BFB50
		public event Ragdoll.ContactEvent OnContactStopEvent;

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06001CDE RID: 7390 RVA: 0x000C1985 File Offset: 0x000BFB85
		// (set) Token: 0x06001CDF RID: 7391 RVA: 0x000C198D File Offset: 0x000BFB8D
		public bool sliceRunning { get; private set; }

		// Token: 0x06001CE0 RID: 7392 RVA: 0x000C1998 File Offset: 0x000BFB98
		public void AutoCreateMeshColliders()
		{
			new Dictionary<Transform, List<SkinnedMeshRenderer>>();
			foreach (SkinnedMeshRenderer smr in this.meshRig.parent.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				MeshCollider smrCollider = new GameObject(smr.name + "_Collider").AddComponent<MeshCollider>();
				smrCollider.sharedMesh = smr.sharedMesh;
				smrCollider.convex = true;
				smrCollider.transform.parent = base.transform;
				if (smr.bones.Length == 0)
				{
					Debug.LogError(smr.name + " has no associated bones and won't be automatically moved to the ragdoll!");
				}
				if (smr.bones.Length == 1)
				{
					smrCollider.transform.parent = this.GetPart(smr.bones[0]).transform;
				}
				if (smr.bones.Length != 0)
				{
					Mesh mesh = smr.sharedMesh;
					if (mesh.GetBonesPerVertex().Length == 0)
					{
						return;
					}
					NativeArray<BoneWeight1> boneWeights = mesh.GetAllBoneWeights();
					int greatestInfluenceBone = -1;
					float greatestInfluence = 0f;
					float[] influences = new float[smr.bones.Length];
					for (int i = 0; i < boneWeights.Length; i++)
					{
						BoneWeight1 boneWeight = boneWeights[i];
						influences[boneWeight.boneIndex] += boneWeight.weight;
						if (influences[boneWeight.boneIndex] > greatestInfluence)
						{
							greatestInfluence = influences[boneWeight.boneIndex];
							greatestInfluenceBone = boneWeight.boneIndex;
						}
					}
					Transform associatedBone = smr.bones[greatestInfluenceBone];
					Debug.Log("Associated bone: " + associatedBone.name);
					foreach (RagdollPart part in base.GetComponentsInChildren<RagdollPart>())
					{
						if (part.meshBone == associatedBone)
						{
							smrCollider.transform.parent = part.transform;
							break;
						}
					}
				}
			}
		}

		// Token: 0x06001CE1 RID: 7393 RVA: 0x000C1B68 File Offset: 0x000BFD68
		public void AutoAssignParentPartsByBones()
		{
			Dictionary<Transform, RagdollPart> bonedParts = new Dictionary<Transform, RagdollPart>();
			foreach (RagdollPart part in base.GetComponentsInChildren<RagdollPart>())
			{
				if (part.meshBone != null)
				{
					bonedParts[part.meshBone] = part;
				}
			}
			foreach (KeyValuePair<Transform, RagdollPart> bonedPart in bonedParts)
			{
				RagdollPart part2 = bonedPart.Value;
				Transform boneParent = bonedPart.Key.parent;
				while (boneParent != null)
				{
					RagdollPart parentPart;
					if (bonedParts.TryGetValue(boneParent, out parentPart))
					{
						part2.parentPart = parentPart;
						break;
					}
					boneParent = boneParent.parent;
				}
			}
		}

		// Token: 0x06001CE2 RID: 7394 RVA: 0x000C1C30 File Offset: 0x000BFE30
		private void Awake()
		{
			this.parts = new List<RagdollPart>();
			this.SetOrderedPartList();
		}

		// Token: 0x06001CE3 RID: 7395 RVA: 0x000C1C44 File Offset: 0x000BFE44
		private RagdollPart AddPartOrdered(RagdollPart part)
		{
			this.parts.Add(part);
			foreach (RagdollPart ragdollPart in base.GetComponentsInChildren<RagdollPart>())
			{
				if (ragdollPart.parentPart == part)
				{
					RagdollPart foundPart = this.AddPartOrdered(ragdollPart);
					if (foundPart)
					{
						this.parts.Add(foundPart);
					}
				}
			}
			return null;
		}

		// Token: 0x06001CE4 RID: 7396 RVA: 0x000C1CA4 File Offset: 0x000BFEA4
		private void SetOrderedPartList()
		{
			this.parts.Clear();
			RagdollPart[] partsFound = base.GetComponentsInChildren<RagdollPart>();
			for (int i = 0; i < partsFound.Length; i++)
			{
				partsFound[i].LinkParent();
			}
			this.AddDrillRecursive(this.rootPart);
		}

		// Token: 0x06001CE5 RID: 7397 RVA: 0x000C1CE8 File Offset: 0x000BFEE8
		private void AddDrillRecursive(RagdollPart part)
		{
			this.parts.Add(part);
			for (int i = 0; i < part.childParts.Count; i++)
			{
				this.AddDrillRecursive(part.childParts[i]);
			}
		}

		// Token: 0x06001CE6 RID: 7398 RVA: 0x000C1D2C File Offset: 0x000BFF2C
		public void Init(Creature creature)
		{
			this.creature = creature;
			creature.OnFallEvent += this.OnFallEvent;
			GameObject newAnimatorGo = new GameObject("Animator");
			newAnimatorGo.transform.SetParentOrigin(base.transform);
			UnityEngine.Object.Instantiate<Transform>(this.meshRig, newAnimatorGo.transform).name = this.meshRig.name;
			SkinnedMeshRenderer skinnedMeshRenderer = new GameObject("SkinnedMesh").AddComponent<SkinnedMeshRenderer>();
			skinnedMeshRenderer.transform.SetParent(newAnimatorGo.transform);
			skinnedMeshRenderer.rootBone = this.meshRootBone;
			this.dismemberment = new Dismemberment(creature.lodGroup ? creature.lodGroup.gameObject : creature.gameObject);
			this.dismemberment.Completed += this.OnSlice;
			Animator newAnimator = newAnimatorGo.AddComponent<Animator>();
			newAnimator.keepAnimatorStateOnDisable = true;
			newAnimator.applyRootMotion = creature.animator.applyRootMotion;
			newAnimator.runtimeAnimatorController = creature.animator.runtimeAnimatorController;
			newAnimator.avatar = creature.animator.avatar;
			newAnimator.cullingMode = creature.animator.cullingMode;
			newAnimator.updateMode = creature.animator.updateMode;
			creature.animator.enabled = false;
			this.ik = creature.animator.GetComponent<IkController>();
			if (this.ik)
			{
				IkController newIk = Common.CloneComponent(this.ik, newAnimatorGo, true) as IkController;
				UnityEngine.Object.Destroy(this.ik);
				this.ik = newIk;
			}
			CreatureAnimatorEventReceiver animatorEventReceiver = creature.animator.GetComponent<CreatureAnimatorEventReceiver>();
			if (animatorEventReceiver)
			{
				Common.CloneComponent(animatorEventReceiver, newAnimatorGo, true);
				UnityEngine.Object.Destroy(animatorEventReceiver);
			}
			this.humanoidIk = creature.animator.GetComponent<HumanoidFullBodyIK>();
			if (this.humanoidIk)
			{
				HumanoidFullBodyIK newIK = Common.CloneComponent(this.humanoidIk, newAnimatorGo, true) as HumanoidFullBodyIK;
				UnityEngine.Object.Destroy(this.humanoidIk);
				this.humanoidIk = newIK;
			}
			this.animatorRig = this.FindTransformAtSamePath(newAnimator.transform, this.meshRig, creature.animator.transform);
			creature.animator = newAnimator;
			creature.animator.logWarnings = false;
			creature.animator.name = "Animator";
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.Init(this);
			}
			this.bones = new List<Ragdoll.Bone>();
			foreach (Transform boneTransform in this.meshRig.GetComponentsInChildren<Transform>())
			{
				Transform animationTransform = this.FindTransformAtSamePath(this.animatorRig, boneTransform, this.meshRig);
				if (animationTransform)
				{
					Ragdoll.Bone bone = new Ragdoll.Bone(creature, boneTransform, animationTransform, this.GetPart(boneTransform));
					this.bones.Add(bone);
				}
				else
				{
					Debug.LogErrorFormat(boneTransform, "Could not find animation transform at same path for bone " + boneTransform.name + ", did you set the correct meshRig and meshRootBone?", Array.Empty<object>());
				}
			}
			this.ResetRegions();
			if (creature.handLeft)
			{
				creature.handLeft.InitAfterBoneInit();
				WristRelaxer wristRelaxer = creature.handLeft.GetComponent<WristRelaxer>();
				if (wristRelaxer)
				{
					wristRelaxer.Init();
				}
			}
			if (creature.handRight)
			{
				creature.handRight.InitAfterBoneInit();
				WristRelaxer wristRelaxer2 = creature.handRight.GetComponent<WristRelaxer>();
				if (wristRelaxer2)
				{
					wristRelaxer2.Init();
				}
			}
			this.totalMass = 0f;
			foreach (Ragdoll.Bone bone2 in this.bones)
			{
				if (bone2.animation.parent)
				{
					bone2.parent = this.GetBone(bone2.animation.parent);
					if (bone2.parent != null)
					{
						bone2.parent.childs.Add(bone2);
					}
				}
				bone2.hasChildAnimationJoint = bone2.animation.GetComponentInChildren<ConfigurableJoint>();
				if (bone2.part)
				{
					this.totalMass += bone2.part.physicBody.mass;
				}
				this.SetMeshBone(bone2, false, false);
				bone2.mesh.name = bone2.mesh.name + "_Mesh";
			}
			this.ik = newAnimator.GetComponent<IkController>();
			if (this.ik)
			{
				this.ik.Setup();
			}
			if (creature.data != null)
			{
				RagdollMassScalar massScalar;
				if (base.TryGetComponent<RagdollMassScalar>(out massScalar))
				{
					UnityEngine.Object.Destroy(massScalar);
				}
				RagdollMassScalar.ScaleMass<RagdollPart>(this.parts, creature.data.ragdollData.standingMass / RagdollMassScalar.GetTotalMass<RagdollPart>(this.parts, new Func<RagdollPart, float>(Ragdoll.<Init>g__GetStandingMass|129_0)), new Func<RagdollPart, float>(Ragdoll.<Init>g__GetStandingMass|129_0), delegate(RagdollPart part, float mass)
				{
					part.physicBody.mass = mass;
					part.standingMass = mass;
				});
				RagdollMassScalar.ScaleMass<RagdollPart>(this.parts, creature.data.ragdollData.handledMass / RagdollMassScalar.GetTotalMass<RagdollPart>(this.parts, new Func<RagdollPart, float>(Ragdoll.<Init>g__GetHandledMass|129_2)), new Func<RagdollPart, float>(Ragdoll.<Init>g__GetHandledMass|129_2), delegate(RagdollPart part, float mass)
				{
					part.handledMass = mass;
				});
				RagdollMassScalar.ScaleMass<RagdollPart>(this.parts, creature.data.ragdollData.ragdolledMass / RagdollMassScalar.GetTotalMass<RagdollPart>(this.parts, new Func<RagdollPart, float>(Ragdoll.<Init>g__GetRagdolledMass|129_4)), new Func<RagdollPart, float>(Ragdoll.<Init>g__GetRagdolledMass|129_4), delegate(RagdollPart part, float mass)
				{
					part.ragdolledMass = mass;
				});
				foreach (CreatureData.PartData partData in creature.data.ragdollData.parts)
				{
					foreach (RagdollPart ragdollpart in this.parts)
					{
						if (partData.bodyPartTypes == (RagdollPart.Type)0 || partData.bodyPartTypes.HasFlagNoGC(ragdollpart.type))
						{
							ragdollpart.data = partData;
						}
					}
				}
			}
			PhysicsToggleManager.Local.UpdatePhysicToggle(this.creature, true, true, true);
			this.initialized = true;
		}

		// Token: 0x06001CE7 RID: 7399 RVA: 0x000C23CC File Offset: 0x000C05CC
		public void ResetRegions()
		{
			this.rootRegion = new Ragdoll.Region(this.parts, true);
			if (this.ragdollRegions == null)
			{
				this.ragdollRegions = new List<Ragdoll.Region>
				{
					this.rootRegion
				};
				return;
			}
			this.ragdollRegions.Clear();
			this.ragdollRegions.Add(this.rootRegion);
		}

		// Token: 0x06001CE8 RID: 7400 RVA: 0x000C2428 File Offset: 0x000C0628
		public void UpdateMetalArmor()
		{
			this.hasMetalArmor = false;
			using (List<RagdollPart>.Enumerator enumerator = this.parts.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.UpdateMetalArmor())
					{
						this.hasMetalArmor = true;
					}
				}
			}
		}

		// Token: 0x06001CE9 RID: 7401 RVA: 0x000C2488 File Offset: 0x000C0688
		public void Load(CreatureData data)
		{
			this.destabilizedSpringRotationMultiplier = data.ragdollData.destabilizedSpringRotationMultiplier;
			this.destabilizedDamperRotationMultiplier = data.ragdollData.destabilizedDamperRotationMultiplier;
			this.destabilizedGroundSpringRotationMultiplier = data.ragdollData.destabilizedGroundSpringRotationMultiplier;
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.Load();
			}
			this.DisableCharJointBreakForce();
		}

		// Token: 0x06001CEA RID: 7402 RVA: 0x000C2514 File Offset: 0x000C0714
		public void OnDespawn()
		{
			this.ResetRegions();
			this.DisableCharJointBreakForce();
			foreach (RagdollPart ragdollPart in this.parts)
			{
				if (ragdollPart.isSliced)
				{
					if (ragdollPart.slicedMeshRoot)
					{
						UnityEngine.Object.Destroy(ragdollPart.slicedMeshRoot.gameObject);
					}
					if (ragdollPart.bone.meshSplit)
					{
						UnityEngine.Object.Destroy(ragdollPart.bone.meshSplit.gameObject);
					}
					ragdollPart.slicedMeshRoot = null;
					ragdollPart.bone.meshSplit = null;
					if (ragdollPart.isSliced)
					{
						if (ragdollPart.sliceChildAndDisableSelf)
						{
							foreach (Collider collider in ragdollPart.colliderGroup.colliders)
							{
								collider.enabled = true;
							}
							foreach (HandleRagdoll handleRagdoll in ragdollPart.handles)
							{
								handleRagdoll.SetTouch(true);
							}
						}
						LightVolumeReceiver lightVolumeReceiver = ragdollPart.GetComponent<LightVolumeReceiver>();
						if (lightVolumeReceiver)
						{
							UnityEngine.Object.Destroy(lightVolumeReceiver);
						}
					}
					ragdollPart.characterJointLocked = false;
					ragdollPart.isSliced = false;
					ragdollPart.CreateCharJoint(true);
				}
			}
			this.sliceRunning = false;
			this.isSliced = false;
			this.ClearPhysicModifiers();
			this.forcePhysic.Clear();
		}

		// Token: 0x06001CEB RID: 7403 RVA: 0x000C26E0 File Offset: 0x000C08E0
		public void SetColliders(bool active)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				foreach (Collider collider in ragdollPart.colliderGroup.colliders)
				{
					collider.enabled = active;
				}
			}
		}

		// Token: 0x06001CEC RID: 7404 RVA: 0x000C2770 File Offset: 0x000C0970
		public void CancelVelocity()
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.physicBody.velocity = Vector3.zero;
			}
		}

		// Token: 0x06001CED RID: 7405 RVA: 0x000C27CC File Offset: 0x000C09CC
		public void MultiplyVelocity(float amount)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.physicBody.velocity *= amount;
			}
		}

		// Token: 0x06001CEE RID: 7406 RVA: 0x000C2830 File Offset: 0x000C0A30
		public void EnableJointLimit()
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.ResetCharJointLimit();
			}
		}

		// Token: 0x06001CEF RID: 7407 RVA: 0x000C2880 File Offset: 0x000C0A80
		public void DisableJointLimit()
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.DisableCharJointLimit();
			}
		}

		// Token: 0x06001CF0 RID: 7408 RVA: 0x000C28D0 File Offset: 0x000C0AD0
		protected void SetAnimationBoneToRig(Ragdoll.Bone bone)
		{
			if (bone.parent == null)
			{
				return;
			}
			if (bone.hasChildAnimationJoint)
			{
				bone.animation.SetParent(bone.parent.animation);
				bone.animation.localPosition = bone.orgLocalPosition;
				bone.animation.localRotation = bone.orgLocalRotation;
				bone.animation.localScale = Vector3.one;
				return;
			}
			this.SetAnimationBoneToPart(bone, true);
		}

		// Token: 0x06001CF1 RID: 7409 RVA: 0x000C2940 File Offset: 0x000C0B40
		protected void SetAnimationBoneToPart(Ragdoll.Bone bone, bool resetNoPartBone = false)
		{
			if (bone.parent == null)
			{
				return;
			}
			if (bone.parent.part)
			{
				bone.animation.SetParent(bone.parent.part.transform);
			}
			else if (bone.parent.animation.GetComponentInParent<Animator>() == this.creature.animator && bone.parent.parent != null && bone.parent.parent.part)
			{
				bone.animation.SetParent(bone.parent.parent.part.transform);
			}
			else
			{
				bone.animation.SetParent(bone.parent.animation);
			}
			if (resetNoPartBone && !bone.part)
			{
				bone.animation.localPosition = bone.orgLocalPosition;
				bone.animation.localRotation = bone.orgLocalRotation;
			}
			bone.animation.localScale = Vector3.one;
		}

		// Token: 0x06001CF2 RID: 7410 RVA: 0x000C2A48 File Offset: 0x000C0C48
		protected void SetAnimationBoneToRoot(Ragdoll.Bone bone, bool resetToOrgPosition)
		{
			if (bone.parent != null)
			{
				bone.animation.SetParent(bone.parent.animation, true);
			}
			else
			{
				bone.animation.SetParent(this.animatorRig, true);
			}
			if (resetToOrgPosition)
			{
				bone.animation.localPosition = bone.orgLocalPosition;
				bone.animation.localRotation = bone.orgLocalRotation;
			}
			bone.animation.localScale = Vector3.one;
		}

		// Token: 0x06001CF3 RID: 7411 RVA: 0x000C2AC0 File Offset: 0x000C0CC0
		protected void SetMeshBone(Ragdoll.Bone bone, bool forceParentMesh = false, bool parentAnimation = false)
		{
			if (bone.part)
			{
				if (bone.part.isSliced)
				{
					return;
				}
				bone.mesh.SetParent(parentAnimation ? bone.animation : bone.part.transform);
				bone.mesh.localPosition = Vector3.zero;
				bone.mesh.localRotation = Quaternion.identity;
			}
			else if (bone.parent != null && (bone.hasChildAnimationJoint || forceParentMesh))
			{
				bone.mesh.SetParent(bone.parent.mesh, true);
				bone.mesh.localPosition = bone.orgLocalPosition;
				bone.mesh.localRotation = bone.orgLocalRotation;
			}
			else
			{
				bone.mesh.SetParent(bone.animation);
				bone.mesh.localPosition = Vector3.zero;
				bone.mesh.localRotation = Quaternion.identity;
			}
			bone.mesh.localScale = Vector3.one;
		}

		// Token: 0x06001CF4 RID: 7412 RVA: 0x000C2BBC File Offset: 0x000C0DBC
		public HandleRagdoll GetHandle(string name)
		{
			foreach (RagdollPart ragdollPart in this.creature.ragdoll.parts)
			{
				foreach (HandleRagdoll handleRagdoll in ragdollPart.handles)
				{
					if (handleRagdoll.name == name)
					{
						return handleRagdoll;
					}
				}
			}
			return null;
		}

		// Token: 0x06001CF5 RID: 7413 RVA: 0x000C2C60 File Offset: 0x000C0E60
		public void OnCreatureEnable()
		{
			this.forcePhysic = new BoolHandler(false);
			this.forcePhysic.OnChangeEvent += this.OnForcePhysicChange;
			this.lastPhysicToggleTime = Time.time;
			if (this.creature && this.creature.initialized)
			{
				if (this.creature.loaded)
				{
					if (this.state == Ragdoll.State.Standing || this.state == Ragdoll.State.NoPhysic)
					{
						PhysicsToggleManager.Local.UpdatePhysicToggle(this.creature, true, true, true);
					}
					else
					{
						this.SetState(this.state, true, true);
					}
				}
				else
				{
					PhysicsToggleManager.Local.UpdatePhysicToggle(this.creature, true, true, true);
				}
			}
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.OnRagdollEnable();
			}
			if (this.standingUp)
			{
				this.CancelGetUp(true);
			}
		}

		// Token: 0x06001CF6 RID: 7414 RVA: 0x000C2D60 File Offset: 0x000C0F60
		private void OnForcePhysicChange(bool oldValue, bool newValue)
		{
			if (!newValue || this.state != Ragdoll.State.NoPhysic)
			{
				return;
			}
			this.SetState(Ragdoll.State.Standing);
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.collisionHandler.RemovePhysicModifier(this);
			}
		}

		// Token: 0x06001CF7 RID: 7415 RVA: 0x000C2DCC File Offset: 0x000C0FCC
		public void OnCreatureDisable()
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.OnRagdollDisable();
			}
			if (this.creature && this.creature.initialized)
			{
				if (this.creature.loaded)
				{
					using (List<Ragdoll.Bone>.Enumerator enumerator2 = this.bones.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Ragdoll.Bone bone = enumerator2.Current;
							this.SetAnimationBoneToRig(bone);
						}
						return;
					}
				}
				this.SetState(Ragdoll.State.Disabled, true);
			}
		}

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06001CF8 RID: 7416 RVA: 0x000C2E90 File Offset: 0x000C1090
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001CF9 RID: 7417 RVA: 0x000C2E94 File Offset: 0x000C1094
		protected internal override void ManagedUpdate()
		{
			if (this.initialized && this.creature != null && !this.creature.isPlayer && this.creature.data.destabilizeOnFall && Vector3.Angle(Vector3.up, this.creature.locomotion.transform.up) > 0.5f)
			{
				this.SetState(Ragdoll.State.Destabilized);
				this.creature.locomotion.physicBody.MoveRotation(Quaternion.FromToRotation(Vector3.forward, -Vector3.up) * Quaternion.LookRotation(Vector3.up));
			}
		}

		// Token: 0x06001CFA RID: 7418 RVA: 0x000C2F44 File Offset: 0x000C1144
		public void AnimatorMoveUpdate()
		{
			if (this.ragdollRegions.Count > 1)
			{
				for (int i = this.ragdollRegions.Count - 1; i > 0; i--)
				{
					if (this.ragdollRegions[i] != this.rootRegion)
					{
						foreach (RagdollPart ragdollPart in this.ragdollRegions[i].parts)
						{
							ragdollPart.AnimatorMoveUpdate();
						}
					}
				}
			}
		}

		// Token: 0x06001CFB RID: 7419 RVA: 0x000C2FDC File Offset: 0x000C11DC
		private void TogglePhysic()
		{
			if (this.state == Ragdoll.State.NoPhysic)
			{
				this.SetState(Ragdoll.State.Standing);
				return;
			}
			if (this.state == Ragdoll.State.Standing)
			{
				this.SetState(Ragdoll.State.NoPhysic);
			}
		}

		// Token: 0x06001CFC RID: 7420 RVA: 0x000C2FFF File Offset: 0x000C11FF
		public static bool IsPhysicalState(Ragdoll.State state, bool kinematicIsPhysic = false)
		{
			if (kinematicIsPhysic)
			{
				return state == Ragdoll.State.Standing || state == Ragdoll.State.Destabilized || state == Ragdoll.State.Inert || state == Ragdoll.State.Frozen || state == Ragdoll.State.Kinematic;
			}
			return state == Ragdoll.State.Standing || state == Ragdoll.State.Destabilized || state == Ragdoll.State.Inert || state == Ragdoll.State.Frozen;
		}

		// Token: 0x06001CFD RID: 7421 RVA: 0x000C302B File Offset: 0x000C122B
		public bool IsPhysicsEnabled(bool kinematicIsPhysic = false)
		{
			return Ragdoll.IsPhysicalState(this.state, kinematicIsPhysic);
		}

		// Token: 0x06001CFE RID: 7422 RVA: 0x000C3039 File Offset: 0x000C1239
		public static bool IsAnimatedState(Ragdoll.State state, bool destabilizedIsAnimated = false)
		{
			if (destabilizedIsAnimated)
			{
				return state == Ragdoll.State.Standing || state == Ragdoll.State.Kinematic || state == Ragdoll.State.NoPhysic || state == Ragdoll.State.Destabilized;
			}
			return state == Ragdoll.State.Standing || state == Ragdoll.State.Kinematic || state == Ragdoll.State.NoPhysic;
		}

		// Token: 0x06001CFF RID: 7423 RVA: 0x000C305F File Offset: 0x000C125F
		public bool IsAnimationEnabled(bool destabilizedIsAnimated = false)
		{
			return Ragdoll.IsAnimatedState(this.state, destabilizedIsAnimated);
		}

		// Token: 0x06001D00 RID: 7424 RVA: 0x000C306D File Offset: 0x000C126D
		public void SetState(Ragdoll.State newState)
		{
			this.SetState(newState, false, false);
		}

		// Token: 0x06001D01 RID: 7425 RVA: 0x000C3078 File Offset: 0x000C1278
		public void SetState(Ragdoll.State newState, bool force)
		{
			this.SetState(newState, force, false);
		}

		// Token: 0x06001D02 RID: 7426 RVA: 0x000C3084 File Offset: 0x000C1284
		public void SetState(Ragdoll.State newState, bool force, bool resetPartJointIfPhysic)
		{
			if (!force && this.state == newState)
			{
				return;
			}
			Ragdoll.PhysicStateChange physicStateChange = Ragdoll.PhysicStateChange.None;
			if (!Ragdoll.IsPhysicalState(this.state, false) && Ragdoll.IsPhysicalState(newState, false))
			{
				physicStateChange = Ragdoll.PhysicStateChange.ParentingToPhysic;
			}
			else if (Ragdoll.IsPhysicalState(this.state, false) && !Ragdoll.IsPhysicalState(newState, false))
			{
				physicStateChange = Ragdoll.PhysicStateChange.PhysicToParenting;
			}
			if (this.OnStateChange != null)
			{
				this.OnStateChange(this.state, newState, physicStateChange, EventTime.OnStart);
			}
			if (physicStateChange == Ragdoll.PhysicStateChange.PhysicToParenting || (force && !Ragdoll.IsPhysicalState(newState, false)))
			{
				if (this.creature.lodGroup && this.creature.gameObject.activeSelf)
				{
					this.creature.lodGroup.transform.SetParentOrigin(base.transform);
				}
				using (List<RagdollPart>.Enumerator enumerator = this.parts.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						RagdollPart part = enumerator.Current;
						part.collisionHandler.RemovePhysicModifier(this);
						part.physicBody.isKinematic = true;
						part.DisableCharJointLimit();
						part.bone.SetPinPositionForce(0f, 0f, 0f);
						part.bone.SetPinRotationForce(0f, 0f, 0f, null);
						part.bone.animationJoint.gameObject.SetActive(false);
						foreach (HandleRagdoll handleRagdoll in part.handles)
						{
							Vector3 orgHandleLocalPosition = handleRagdoll.transform.localPosition;
							Quaternion orgHandleLocalRotation = handleRagdoll.transform.localRotation;
							handleRagdoll.transform.SetParent(part.bone.animation.transform, false);
							handleRagdoll.transform.localPosition = orgHandleLocalPosition;
							handleRagdoll.transform.localRotation = orgHandleLocalRotation;
						}
					}
					goto IL_33F;
				}
			}
			if (physicStateChange == Ragdoll.PhysicStateChange.ParentingToPhysic || (force && Ragdoll.IsPhysicalState(newState, false)))
			{
				if (this.creature.lodGroup)
				{
					this.creature.lodGroup.transform.SetParentOrigin(this.rootPart.transform);
				}
				foreach (RagdollPart part2 in this.parts)
				{
					if (resetPartJointIfPhysic)
					{
						part2.gameObject.SetActive(false);
						part2.bone.animationJoint.gameObject.SetActive(false);
					}
					foreach (HandleRagdoll handleRagdoll2 in part2.handles)
					{
						Vector3 orgHandleLocalPosition2 = handleRagdoll2.transform.localPosition;
						Quaternion orgHandleLocalRotation2 = handleRagdoll2.transform.localRotation;
						handleRagdoll2.transform.SetParent(part2.transform);
						handleRagdoll2.transform.localPosition = orgHandleLocalPosition2;
						handleRagdoll2.transform.localRotation = orgHandleLocalRotation2;
					}
					foreach (Holder holder in part2.collisionHandler.holders)
					{
						holder.transform.SetParent(part2.transform, false);
					}
				}
			}
			IL_33F:
			if (newState == Ragdoll.State.Standing)
			{
				this.creature.animator.enabled = true;
				this.creature.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
				foreach (RagdollPart part3 in this.parts)
				{
					if (part3.bone.fixedJoint)
					{
						UnityEngine.Object.Destroy(part3.bone.fixedJoint);
					}
					if (!part3.isSliced)
					{
						part3.collisionHandler.SetPhysicModifier(this, new float?(0f), 1f, -1f, -1f, -1f, null);
					}
					if (part3.isSliced)
					{
						part3.collisionHandler.RemovePhysicModifier(this);
					}
					if (this.hipsAttached)
					{
						if (part3 == this.rootPart)
						{
							part3.bone.SetPinPositionForce(this.springPositionForce * this.hipsAttachedSpringPositionMultiplier, this.damperPositionForce * this.hipsAttachedDamperPositionMultiplier, this.maxPositionForce * this.hipsAttachedSpringPositionMultiplier);
							part3.bone.SetPinRotationForce(this.springRotationForce * this.hipsAttachedSpringRotationMultiplier, this.damperRotationForce * this.hipsAttachedDamperRotationMultiplier, this.maxRotationForce * this.hipsAttachedSpringRotationMultiplier, null);
						}
						else
						{
							part3.bone.SetPinPositionForce(0f, 0f, 0f);
							part3.bone.SetPinRotationForce(this.springRotationForce, this.damperRotationForce, this.maxRotationForce, null);
						}
					}
					else
					{
						part3.bone.SetPinPositionForce(this.springPositionForce, this.damperPositionForce, this.maxPositionForce);
						part3.bone.SetPinRotationForce(this.springRotationForce, this.damperRotationForce, this.maxRotationForce, null);
					}
				}
				if (physicStateChange == Ragdoll.PhysicStateChange.ParentingToPhysic || (force && Ragdoll.IsPhysicalState(newState, false)))
				{
					this.SavePartsPosition();
					this.ResetPartsToOrigin(false);
					foreach (Ragdoll.Bone bone in this.bones)
					{
						this.SetAnimationBoneToRig(bone);
						this.SetMeshBone(bone, false, false);
					}
					foreach (RagdollPart ragdollPart in this.parts)
					{
						ragdollPart.gameObject.SetActive(true);
						ragdollPart.bone.animationJoint.gameObject.SetActive(true);
					}
					this.LoadPartsPosition();
				}
				else
				{
					foreach (Ragdoll.Bone bone2 in this.bones)
					{
						this.SetAnimationBoneToRig(bone2);
					}
				}
				this.creature.locomotion.enabled = true;
				this.creature.SetAnimatorHeightRatio(1f);
			}
			else if (newState == Ragdoll.State.Inert || newState == Ragdoll.State.Destabilized)
			{
				this.CancelGetUp(false);
				this.creature.animator.enabled = (newState == Ragdoll.State.Destabilized);
				this.creature.animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
				foreach (RagdollPart part4 in this.parts)
				{
					if (part4.bone.fixedJoint)
					{
						UnityEngine.Object.Destroy(part4.bone.fixedJoint);
					}
					part4.collisionHandler.RemovePhysicModifier(this);
					if (newState == Ragdoll.State.Inert || part4 == this.rootPart)
					{
						part4.bone.SetPinPositionForce(0f, 0f, 0f);
						part4.bone.SetPinRotationForce(0f, 0f, 0f, null);
					}
					else
					{
						part4.bone.SetPinPositionForce(0f, 0f, 0f);
						part4.bone.SetPinRotationForce(this.springRotationForce * this.destabilizedSpringRotationMultiplier, this.damperRotationForce * this.destabilizedDamperRotationMultiplier, this.maxRotationForce, null);
					}
				}
				if (physicStateChange == Ragdoll.PhysicStateChange.ParentingToPhysic || (force && Ragdoll.IsPhysicalState(newState, false)))
				{
					this.SavePartsPosition();
					this.ResetPartsToOrigin(false);
					foreach (Ragdoll.Bone bone3 in this.bones)
					{
						if (bone3.parent != null)
						{
							if (bone3.hasChildAnimationJoint)
							{
								bone3.animation.SetParent(bone3.parent.animation);
								bone3.animation.localPosition = bone3.orgLocalPosition;
								bone3.animation.localRotation = bone3.orgLocalRotation;
								bone3.animation.localScale = Vector3.one;
							}
							else
							{
								this.SetAnimationBoneToPart(bone3, false);
							}
						}
						this.SetMeshBone(bone3, false, false);
					}
					foreach (RagdollPart ragdollPart2 in this.parts)
					{
						ragdollPart2.gameObject.SetActive(true);
						ragdollPart2.bone.animationJoint.gameObject.SetActive(true);
					}
					this.LoadPartsPosition();
				}
				foreach (Ragdoll.Bone bone4 in this.bones)
				{
					this.SetAnimationBoneToPart(bone4, false);
				}
				this.creature.locomotion.enabled = false;
				this.creature.SetAnimatorHeightRatio(0f);
			}
			else if (newState == Ragdoll.State.Frozen)
			{
				this.CancelGetUp(false);
				this.creature.animator.enabled = false;
				this.creature.animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
				foreach (RagdollPart ragdollPart3 in this.parts)
				{
					ragdollPart3.collisionHandler.RemovePhysicModifier(this);
					ragdollPart3.bone.SetPinPositionForce(0f, 0f, 0f);
					ragdollPart3.bone.SetPinRotationForce(0f, 0f, 0f, null);
				}
				if (physicStateChange == Ragdoll.PhysicStateChange.ParentingToPhysic || (force && Ragdoll.IsPhysicalState(newState, false)))
				{
					this.SavePartsPosition();
					foreach (Ragdoll.Bone bone5 in this.bones)
					{
						if (bone5.parent != null)
						{
							this.SetAnimationBoneToPart(bone5, false);
						}
					}
					this.ResetPartsToOrigin(false);
					foreach (Ragdoll.Bone bone6 in this.bones)
					{
						if (bone6.parent != null)
						{
							this.SetMeshBone(bone6, false, false);
						}
					}
					foreach (RagdollPart ragdollPart4 in this.parts)
					{
						ragdollPart4.gameObject.SetActive(true);
						ragdollPart4.bone.animationJoint.gameObject.SetActive(true);
					}
					this.LoadPartsPosition();
				}
				else
				{
					foreach (Ragdoll.Bone bone7 in this.bones)
					{
						if (bone7.parent != null)
						{
							this.SetAnimationBoneToPart(bone7, false);
						}
					}
				}
				foreach (RagdollPart part5 in this.parts)
				{
					if (part5.characterJoint && !part5.bone.fixedJoint)
					{
						part5.bone.fixedJoint = part5.characterJoint.gameObject.AddComponent<FixedJoint>();
						part5.bone.fixedJoint.connectedBody = part5.characterJoint.connectedBody;
					}
				}
				this.creature.locomotion.enabled = false;
				this.creature.SetAnimatorHeightRatio(1f);
			}
			else if (newState == Ragdoll.State.NoPhysic)
			{
				this.CancelGetUp(false);
				foreach (RagdollPart part6 in this.parts)
				{
					if (part6.bone.fixedJoint)
					{
						UnityEngine.Object.Destroy(part6.bone.fixedJoint);
					}
					foreach (Holder holder2 in part6.collisionHandler.holders)
					{
						holder2.transform.SetParent(part6.bone.animation.transform, false);
					}
					part6.gameObject.SetActive(false);
				}
				foreach (Ragdoll.Bone bone8 in this.bones)
				{
					this.SetAnimationBoneToRoot(bone8, true);
					this.SetMeshBone(bone8, false, true);
				}
				foreach (RagdollPart part7 in this.parts)
				{
					part7.transform.SetParent(part7.bone.animation.transform);
					part7.transform.localPosition = Vector3.zero;
					part7.transform.localRotation = Quaternion.identity;
					part7.transform.localScale = Vector3.one;
				}
				this.creature.animator.enabled = true;
				this.creature.animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
				this.creature.locomotion.enabled = true;
				this.creature.SetAnimatorHeightRatio(1f);
			}
			else if (newState == Ragdoll.State.Kinematic)
			{
				this.CancelGetUp(false);
				foreach (RagdollPart part8 in this.parts)
				{
					if (part8.bone.fixedJoint)
					{
						UnityEngine.Object.Destroy(part8.bone.fixedJoint);
					}
					foreach (Holder holder3 in part8.collisionHandler.holders)
					{
						holder3.transform.SetParent(part8.transform, false);
					}
					part8.gameObject.SetActive(true);
				}
				foreach (Ragdoll.Bone bone9 in this.bones)
				{
					this.SetAnimationBoneToRoot(bone9, true);
					this.SetMeshBone(bone9, false, false);
				}
				foreach (RagdollPart part9 in this.parts)
				{
					part9.transform.SetParent(part9.bone.animation.transform);
					part9.transform.localPosition = Vector3.zero;
					part9.transform.localRotation = Quaternion.identity;
					part9.transform.localScale = Vector3.one;
				}
				this.creature.animator.enabled = true;
				this.creature.animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
				this.creature.locomotion.enabled = true;
				this.creature.SetAnimatorHeightRatio(1f);
			}
			else if (newState == Ragdoll.State.Disabled)
			{
				this.CancelGetUp(false);
				foreach (RagdollPart part10 in this.parts)
				{
					if (part10.bone.fixedJoint)
					{
						UnityEngine.Object.Destroy(part10.bone.fixedJoint);
					}
					foreach (Holder holder4 in part10.collisionHandler.holders)
					{
						holder4.transform.SetParent(part10.transform, false);
					}
					part10.gameObject.SetActive(false);
				}
				foreach (Ragdoll.Bone bone10 in this.bones)
				{
					this.SetAnimationBoneToRoot(bone10, true);
					this.SetMeshBone(bone10, true, false);
				}
				foreach (RagdollPart part11 in this.parts)
				{
					part11.transform.SetParent(part11.root);
					part11.transform.localPosition = part11.rootOrgLocalPosition;
					part11.transform.localRotation = part11.rootOrgLocalRotation;
					part11.transform.localScale = Vector3.one;
				}
				this.creature.animator.enabled = false;
				this.creature.animator.cullingMode = AnimatorCullingMode.CullCompletely;
				this.creature.locomotion.enabled = false;
				this.creature.SetAnimatorHeightRatio(1f);
			}
			Ragdoll.State previousState = this.state;
			this.state = newState;
			if (this.creature.data != null && this.creature.data.gender != CreatureData.Gender.None)
			{
				this.creature.animator.SetFloat(Creature.hashFeminity, (float)((this.creature.data.gender == CreatureData.Gender.Female) ? 1 : 0));
			}
			Vector3 position = this.creature.transform.position;
			Quaternion rotation = this.creature.transform.rotation;
			this.creature.animator.Update(0f);
			this.creature.transform.position = position;
			this.creature.transform.rotation = rotation;
			this.RefreshPartsLayer();
			this.RefreshPartJointAndCollision();
			if (this.creature.initialized)
			{
				this.creature.RefreshRenderers();
			}
			this.creature.RefreshCollisionOfGrabbedItems();
			if (this.OnStateChange != null)
			{
				this.OnStateChange(previousState, newState, physicStateChange, EventTime.OnEnd);
			}
		}

		// Token: 0x06001D03 RID: 7427 RVA: 0x000C4548 File Offset: 0x000C2748
		public void SavePartsPosition()
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.savedPosition = ragdollPart.transform.position;
				ragdollPart.savedRotation = ragdollPart.transform.rotation;
			}
		}

		// Token: 0x06001D04 RID: 7428 RVA: 0x000C45B4 File Offset: 0x000C27B4
		public void LoadPartsPosition()
		{
			foreach (RagdollPart part in this.parts)
			{
				part.transform.position = part.savedPosition;
				part.transform.rotation = part.savedRotation;
			}
		}

		// Token: 0x06001D05 RID: 7429 RVA: 0x000C4624 File Offset: 0x000C2824
		public void ResetPartsToOrigin(bool isKinematic = false)
		{
			foreach (RagdollPart part in this.parts)
			{
				part.physicBody.isKinematic = isKinematic;
				if (part.transform.parent != part.root)
				{
					part.transform.SetParent(part.root);
				}
				part.transform.localPosition = part.rootOrgLocalPosition;
				part.transform.localRotation = part.rootOrgLocalRotation;
				part.transform.localScale = Vector3.one;
			}
		}

		// Token: 0x06001D06 RID: 7430 RVA: 0x000C46D8 File Offset: 0x000C28D8
		public void RefreshPartJointAndCollision()
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.ResetCharJointLimit();
				ragdollPart.collisionHandler.active = (this.state == Ragdoll.State.Inert || this.state == Ragdoll.State.Destabilized || this.state == Ragdoll.State.Frozen);
				foreach (HandleRagdoll handleRagdoll in ragdollPart.handles)
				{
					handleRagdoll.RefreshJointAndCollision();
				}
			}
		}

		// Token: 0x06001D07 RID: 7431 RVA: 0x000C4790 File Offset: 0x000C2990
		protected void OnFallEvent(Creature.FallState fallState)
		{
			if (this.state == Ragdoll.State.Destabilized)
			{
				if (fallState == Creature.FallState.StabilizedOnGround || fallState == Creature.FallState.Stabilizing)
				{
					using (List<RagdollPart>.Enumerator enumerator = this.parts.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							RagdollPart part = enumerator.Current;
							if (!(part == this.rootPart))
							{
								part.bone.SetPinRotationForce(this.springRotationForce * this.destabilizedGroundSpringRotationMultiplier, this.damperRotationForce * this.destabilizedDamperRotationMultiplier, this.maxRotationForce, null);
							}
						}
						return;
					}
				}
				foreach (RagdollPart part2 in this.parts)
				{
					if (!(part2 == this.rootPart))
					{
						part2.bone.SetPinRotationForce(this.springRotationForce * this.destabilizedSpringRotationMultiplier, this.damperRotationForce * this.destabilizedDamperRotationMultiplier, this.maxRotationForce, null);
					}
				}
			}
		}

		// Token: 0x06001D08 RID: 7432 RVA: 0x000C48A0 File Offset: 0x000C2AA0
		public void InvokeTelekinesisGrabEvent(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll)
		{
			if (this.OnTelekinesisGrabEvent != null)
			{
				this.OnTelekinesisGrabEvent(spellTelekinesis, handleRagdoll);
			}
		}

		// Token: 0x06001D09 RID: 7433 RVA: 0x000C48B7 File Offset: 0x000C2AB7
		public void InvokeTelekinesisReleaseEvent(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll, bool lastHandler)
		{
			if (this.OnTelekinesisReleaseEvent != null)
			{
				this.OnTelekinesisReleaseEvent(spellTelekinesis, handleRagdoll, lastHandler);
			}
		}

		// Token: 0x06001D0A RID: 7434 RVA: 0x000C48CF File Offset: 0x000C2ACF
		public void InvokeGrabEvent(RagdollHand ragdollHand, HandleRagdoll handleRagdoll)
		{
			if (this.OnGrabEvent != null)
			{
				this.OnGrabEvent(ragdollHand, handleRagdoll);
			}
		}

		// Token: 0x06001D0B RID: 7435 RVA: 0x000C48E6 File Offset: 0x000C2AE6
		public void InvokeUngrabEvent(RagdollHand ragdollHand, HandleRagdoll handleRagdoll, bool lastHandler)
		{
			if (this.OnUngrabEvent != null)
			{
				this.OnUngrabEvent(ragdollHand, handleRagdoll, lastHandler);
			}
		}

		// Token: 0x06001D0C RID: 7436 RVA: 0x000C4900 File Offset: 0x000C2B00
		public bool SphereCastGround(float sphereRadius, float castLenght, out RaycastHit raycastHit, out float groundDistance)
		{
			if (Physics.SphereCast(new Ray(new Vector3(this.creature.ragdoll.rootPart.transform.position.x, this.creature.ragdoll.rootPart.transform.position.y + sphereRadius, this.creature.ragdoll.rootPart.transform.position.z), Vector3.down), sphereRadius, out raycastHit, castLenght, ThunderRoadSettings.current.groundLayer))
			{
				groundDistance = raycastHit.distance - sphereRadius;
				return true;
			}
			groundDistance = 0f;
			return false;
		}

		// Token: 0x06001D0D RID: 7437 RVA: 0x000C49AA File Offset: 0x000C2BAA
		public virtual void StandUp()
		{
			this.CancelGetUp(true);
			BrainModuleHitReaction module = this.creature.brain.instance.GetModule<BrainModuleHitReaction>(true);
			if (module != null)
			{
				module.StopStagger();
			}
			this.getUpCoroutine = base.StartCoroutine(this.StandUpCoroutine());
		}

		// Token: 0x06001D0E RID: 7438 RVA: 0x000C49E6 File Offset: 0x000C2BE6
		protected virtual IEnumerator StandUpCoroutine()
		{
			this.standingUp = true;
			this.standStartTime = Time.time;
			if (!this.isGrabbed && !this.isTkGrabbed)
			{
				this.SetBodyPositionToHips();
				if (this.rootPart.physicBody.transform.forward.y > 0f)
				{
					this.creature.animator.SetInteger(Creature.hashGetUp, 1);
				}
				else
				{
					this.creature.animator.SetInteger(Creature.hashGetUp, 2);
				}
			}
			else
			{
				this.SetBodyPositionToHead();
			}
			this.creature.locomotion.capsuleCollider.radius = 0f;
			this.creature.locomotion.enabled = true;
			this.creature.SetAnimatorHeightRatio(1f);
			if (this.creature)
			{
				this.creature.enabled = true;
			}
			this.SetState(Ragdoll.State.Standing);
			this.RemovePhysicModifier(this);
			this.SetPinForceMultiplier(0f, 0f, 0f, 0f, true, false, (RagdollPart.Type)0, null);
			bool standUp = false;
			float elapsedTime = 0f;
			if (!this.isGrabbed && !this.isTkGrabbed)
			{
				while (elapsedTime < this.preStandUpDuration)
				{
					float standUpRatio = this.standUpCurve.Evaluate(elapsedTime / this.preStandUpDuration);
					this.SetPinForceMultiplier(standUpRatio, standUpRatio, standUpRatio, standUpRatio, true, false, (RagdollPart.Type)0, null);
					elapsedTime += Time.deltaTime;
					if (!standUp && elapsedTime / this.preStandUpDuration > this.preStandUpRatio)
					{
						if (!this.isGrabbed && !this.isTkGrabbed)
						{
							this.creature.animator.SetInteger(Creature.hashGetUp, 0);
						}
						standUp = true;
					}
					yield return null;
				}
			}
			else
			{
				while (elapsedTime < this.standUpFromGrabDuration)
				{
					float standUpRatio2 = this.standUpCurve.Evaluate(elapsedTime / this.standUpFromGrabDuration);
					this.SetPinForceMultiplier(standUpRatio2, standUpRatio2, standUpRatio2, standUpRatio2, true, false, (RagdollPart.Type)0, null);
					elapsedTime += Time.deltaTime;
					yield return null;
				}
			}
			this.ResetPinForce(true, false, (RagdollPart.Type)0);
			this.SetState(Ragdoll.State.Standing);
			if (!this.isGrabbed && !this.isTkGrabbed)
			{
				while (this.creature.animator.GetBool(Creature.hashIsBusy))
				{
					this.creature.locomotion.capsuleCollider.radius = Mathf.Lerp(0f, this.creature.locomotion.colliderRadius, this.creature.animator.GetCurrentAnimatorStateInfo(3).normalizedTime);
					yield return null;
				}
			}
			else
			{
				this.creature.locomotion.capsuleCollider.radius = this.creature.locomotion.colliderRadius;
				if (this.isGrabbed)
				{
					this.creature.locomotion.StartShrinkCollider();
				}
			}
			this.creature.RefreshRenderers();
			this.getUpCoroutine = null;
			this.standingUp = false;
			yield break;
		}

		// Token: 0x06001D0F RID: 7439 RVA: 0x000C49F8 File Offset: 0x000C2BF8
		public virtual void CancelGetUp(bool resetState = true)
		{
			if (this.getUpCoroutine != null)
			{
				base.StopCoroutine(this.getUpCoroutine);
				this.creature.animator.SetInteger(Creature.hashGetUp, 0);
				this.creature.locomotion.capsuleCollider.radius = this.creature.locomotion.colliderRadius;
				this.getUpCoroutine = null;
				if (resetState)
				{
					this.SetState(Ragdoll.State.Destabilized, true);
				}
			}
			this.standingUp = false;
		}

		// Token: 0x06001D10 RID: 7440 RVA: 0x000C4A70 File Offset: 0x000C2C70
		public virtual void SetBodyPositionToHips()
		{
			Vector3 ragdollHorizontalDir;
			if (this.rootPart.physicBody.transform.forward.y > 0f)
			{
				ragdollHorizontalDir = new Vector3(this.rootPart.transform.right.x, 0f, this.rootPart.transform.right.z);
			}
			else
			{
				ragdollHorizontalDir = new Vector3(-this.rootPart.transform.right.x, 0f, -this.rootPart.transform.right.z);
			}
			foreach (RagdollPart ragdollPart2 in this.parts)
			{
				ragdollPart2.transform.SetParent(null, true);
			}
			this.creature.locomotion.physicBody.velocity = Vector3.zero;
			this.creature.locomotion.physicBody.angularVelocity = Vector3.zero;
			this.creature.transform.position = this.rootPart.transform.position;
			RaycastHit raycastHit;
			float groundDistance;
			if (this.creature.locomotion.SphereCastGround(10f, out raycastHit, out groundDistance))
			{
				this.creature.transform.position = raycastHit.point;
			}
			this.creature.transform.rotation = Quaternion.LookRotation(ragdollHorizontalDir, Vector3.up);
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.transform.SetParent(ragdollPart.root, true);
			}
		}

		// Token: 0x06001D11 RID: 7441 RVA: 0x000C4C4C File Offset: 0x000C2E4C
		public virtual void SetBodyPositionToHead()
		{
			foreach (RagdollPart ragdollPart2 in this.parts)
			{
				ragdollPart2.transform.SetParent(null, true);
			}
			this.creature.transform.position = this.headPart.transform.position;
			RaycastHit raycastHit;
			float groundDistance;
			if (this.creature.locomotion.SphereCastGround(10f, out raycastHit, out groundDistance))
			{
				this.creature.transform.position = raycastHit.point;
			}
			this.creature.transform.rotation = Quaternion.LookRotation(this.headPart.transform.forward.ToXZ(), Vector3.up);
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.transform.SetParent(ragdollPart.root, true);
			}
		}

		/// <summary>
		/// Adds a stabilization joint connected to the root of the ragdoll
		/// If the stabilization joint already exists, it will take any further requests to enable it and add it to a queue. 
		/// When one component removes the joint it will look for the next one to take over
		/// </summary>
		/// <param name="owningObject">The object to which the joint will be applied</param>
		/// <param name="relativeObject">The object relative to which this root will be placed</param>
		// Token: 0x06001D12 RID: 7442 RVA: 0x000C4D74 File Offset: 0x000C2F74
		public virtual void AddStabilizationJoint(GameObject owningObject, Ragdoll.StabilizationJointSettings jointSettings = null)
		{
			if (this.stabilizationJoint == null)
			{
				this.stabilizationJointFollowObject = owningObject;
				GameObject stabilizationJointObject = new GameObject("stabilizationJoint");
				stabilizationJointObject.transform.position = this.stabilizationJointFollowObject.transform.position;
				stabilizationJointObject.transform.rotation = this.stabilizationJointFollowObject.transform.rotation;
				Rigidbody stabilityRB = stabilizationJointObject.AddComponent<Rigidbody>();
				this.stabilizationJoint = stabilizationJointObject.AddComponent<ConfigurableJoint>();
				this.stabilizationJoint.connectedBody = this.rootPart.physicBody.rigidBody;
				if (jointSettings != null)
				{
					this.SetSettings(jointSettings, stabilityRB);
					return;
				}
			}
			else if (this.stabilizationJointFollowObject == null)
			{
				this.stabilizationJointFollowObject = owningObject;
				this.stabilizationJoint.transform.position = this.stabilizationJointFollowObject.transform.position;
				this.stabilizationJoint.transform.rotation = this.stabilizationJointFollowObject.transform.rotation;
				Rigidbody stabilityRB2 = this.stabilizationJoint.GetComponent<Rigidbody>();
				if (jointSettings != null)
				{
					this.SetSettings(jointSettings, stabilityRB2);
					return;
				}
			}
			else
			{
				this.stabilizationJointQueue.Add(new Ragdoll.StabilizationJointQueue(owningObject, jointSettings));
			}
		}

		// Token: 0x06001D13 RID: 7443 RVA: 0x000C4E98 File Offset: 0x000C3098
		private void SetSettings(Ragdoll.StabilizationJointSettings jointSettings, Rigidbody stabilityRB)
		{
			stabilityRB.isKinematic = jointSettings.isKinematic;
			stabilityRB.useGravity = false;
			this.stabilizationJoint.configuredInWorldSpace = jointSettings.configuredInWorldSpace;
			this.stabilizationJoint.autoConfigureConnectedAnchor = jointSettings.autoConfigureConnectedAnchor;
			this.stabilizationJoint.axis = jointSettings.axis;
			this.stabilizationJoint.angularXMotion = jointSettings.angularXMotion;
			this.stabilizationJoint.angularYMotion = jointSettings.angularYMotion;
			this.stabilizationJoint.angularZMotion = jointSettings.angularZMotion;
			this.stabilizationJoint.angularYZLimitSpring = jointSettings.angularYZLimitSpring;
			this.stabilizationJoint.angularZLimit = jointSettings.angularZLimit;
			this.stabilizationJoint.angularYLimit = jointSettings.angularYLimit;
			this.stabilizationJoint.angularXLimitSpring = jointSettings.angularXLimitSpring;
			SoftJointLimit XLimitLow = default(SoftJointLimit);
			XLimitLow.limit = -jointSettings.angularXLimit.limit;
			XLimitLow.bounciness = jointSettings.angularXLimit.bounciness;
			XLimitLow.contactDistance = jointSettings.angularXLimit.contactDistance;
			this.stabilizationJoint.highAngularXLimit = jointSettings.angularXLimit;
			this.stabilizationJoint.lowAngularXLimit = XLimitLow;
			this.stabilizationJoint.angularYZDrive = jointSettings.angularYZDrive;
			this.stabilizationJoint.angularXDrive = jointSettings.angularXDrive;
		}

		/// <summary>
		/// When this ragdoll already possess a stabilization joint
		/// </summary>
		/// <returns></returns>
		// Token: 0x06001D14 RID: 7444 RVA: 0x000C4FE0 File Offset: 0x000C31E0
		public bool HasStabilizationJointEnabled()
		{
			return this.stabilizationJoint != null;
		}

		/// <summary>
		/// When this ragdoll already possess a stabilization joint
		/// </summary>
		/// <returns></returns>
		// Token: 0x06001D15 RID: 7445 RVA: 0x000C4FEE File Offset: 0x000C31EE
		public bool HasStabilizationJoint(GameObject owningObject)
		{
			return this.stabilizationJoint && this.stabilizationJointFollowObject == owningObject;
		}

		/// <summary>
		/// Removes the joint from this body and if there's a queue, adds it to the next item
		/// </summary>
		// Token: 0x06001D16 RID: 7446 RVA: 0x000C500C File Offset: 0x000C320C
		public virtual void RemoveStabilizationJoint(GameObject owningObject)
		{
			if (this.stabilizationJointQueue.Count == 0)
			{
				if (this.HasStabilizationJoint(owningObject))
				{
					this.stabilizationJointFollowObject = null;
					this.stabilizationJoint.connectedBody = null;
					UnityEngine.Object.Destroy(this.stabilizationJoint.gameObject);
					this.stabilizationJoint = null;
					return;
				}
			}
			else
			{
				if (this.HasStabilizationJoint(owningObject))
				{
					this.stabilizationJointFollowObject = null;
					this.AddStabilizationJoint(this.stabilizationJointQueue[0].owningObject, this.stabilizationJointQueue[0].settings);
					this.stabilizationJointQueue.RemoveAt(0);
					return;
				}
				this.stabilizationJointQueue.RemoveAll((Ragdoll.StabilizationJointQueue item) => item.owningObject == owningObject || item.owningObject == null);
			}
		}

		// Token: 0x06001D17 RID: 7447 RVA: 0x000C50D4 File Offset: 0x000C32D4
		public Ragdoll.Bone GetBone(Transform meshOrAnimBone)
		{
			foreach (Ragdoll.Bone bone in this.bones)
			{
				if (bone.animation == meshOrAnimBone || bone.mesh == meshOrAnimBone)
				{
					return bone;
				}
			}
			return null;
		}

		// Token: 0x06001D18 RID: 7448 RVA: 0x000C5144 File Offset: 0x000C3344
		public RagdollPart GetPart(Transform meshBone)
		{
			if (meshBone == null)
			{
				return null;
			}
			int partsCount = this.parts.Count;
			for (int i = 0; i < partsCount; i++)
			{
				RagdollPart part = this.parts[i];
				if (part.meshBone == meshBone)
				{
					return part;
				}
			}
			return null;
		}

		// Token: 0x06001D19 RID: 7449 RVA: 0x000C5192 File Offset: 0x000C3392
		public RagdollPart GetPart(RagdollPart.Type partTypes)
		{
			return this.GetPart(partTypes, RagdollPart.Section.Full);
		}

		// Token: 0x06001D1A RID: 7450 RVA: 0x000C519C File Offset: 0x000C339C
		public RagdollPart GetPart(RagdollPart.Type partTypes, RagdollPart.Section section = RagdollPart.Section.Full)
		{
			int partsCount = this.parts.Count;
			for (int i = 0; i < partsCount; i++)
			{
				RagdollPart part = this.parts[i];
				if (partTypes.HasFlagNoGC(part.type) && (section == RagdollPart.Section.Full || part.section == section))
				{
					return part;
				}
			}
			return null;
		}

		// Token: 0x06001D1B RID: 7451 RVA: 0x000C51EC File Offset: 0x000C33EC
		public RagdollPart GetPartByName(string name)
		{
			int partsCount = this.parts.Count;
			for (int i = 0; i < partsCount; i++)
			{
				RagdollPart part = this.parts[i];
				if (part.name.Equals(name))
				{
					return part;
				}
			}
			return null;
		}

		// Token: 0x06001D1C RID: 7452 RVA: 0x000C5230 File Offset: 0x000C3430
		public void GetClosestPartColliderAndMatHash(Vector3 origin, out RagdollPart closestPart, out Collider closestCollider, out int materialHash, bool includeApparel = false, Vector2? randomPos = null)
		{
			closestPart = this.parts[0];
			for (int i = 1; i < this.parts.Count; i++)
			{
				RagdollPart part = this.parts[i];
				if ((part.transform.position - origin).sqrMagnitude < (closestPart.transform.position - origin).sqrMagnitude)
				{
					closestPart = part;
				}
			}
			closestCollider = closestPart.colliderGroup.colliders[0];
			for (int j = 1; j < closestPart.colliderGroup.colliders.Count; j++)
			{
				Collider collider = closestPart.colliderGroup.colliders[j];
				if ((collider.transform.position - origin).sqrMagnitude < (closestCollider.transform.position - origin).sqrMagnitude)
				{
					closestCollider = collider;
				}
			}
			materialHash = Animator.StringToHash(closestCollider.material.name);
			if (includeApparel)
			{
				Vector2 mapPosition = (randomPos != null) ? randomPos.Value : new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
				MaterialData wardrobeMaterial = null;
				foreach (Creature.RendererData renderData in closestPart.renderers)
				{
					MeshPart meshPart = renderData.meshPart;
					if (((meshPart != null) ? meshPart.idMap : null) != null)
					{
						MaterialData colorData = MaterialData.GetMaterial(renderData.meshPart.idMap.GetPixel(Mathf.RoundToInt(mapPosition.x * (float)renderData.meshPart.idMap.width), Mathf.RoundToInt(mapPosition.y * (float)renderData.meshPart.idMap.height)));
						if (wardrobeMaterial == null || (colorData != null && colorData.apparelProtectionLevel > wardrobeMaterial.apparelProtectionLevel))
						{
							wardrobeMaterial = colorData;
						}
					}
				}
				if (wardrobeMaterial != null)
				{
					materialHash = wardrobeMaterial.physicMaterialHash;
				}
			}
		}

		// Token: 0x06001D1D RID: 7453 RVA: 0x000C5448 File Offset: 0x000C3648
		protected Transform FindTransformAtSamePath(Transform transform, Transform targetTransform, Transform targetRoot)
		{
			string path = this.GetGameObjectPath(targetTransform, targetRoot);
			if (path == null)
			{
				return transform;
			}
			return transform.Find(path);
		}

		// Token: 0x06001D1E RID: 7454 RVA: 0x000C546C File Offset: 0x000C366C
		protected string GetGameObjectPath(Transform target, Transform root)
		{
			if (target == root)
			{
				return null;
			}
			string path = "/" + target.name;
			while (target.parent != null && !(target.parent == root))
			{
				target = target.parent;
				path = "/" + target.name + path;
			}
			return path.Remove(0, 1);
		}

		// Token: 0x06001D1F RID: 7455 RVA: 0x000C54D8 File Offset: 0x000C36D8
		private void OnDrawGizmosSelected()
		{
			foreach (Ragdoll.Bone bone in this.bones)
			{
				if (bone.parent != null && bone.part)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(bone.parent.animation.position, bone.animation.position);
				}
			}
		}

		// Token: 0x06001D20 RID: 7456 RVA: 0x000C5564 File Offset: 0x000C3764
		public void SetPartsLayer(int layer)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.SetLayer(layer);
			}
		}

		// Token: 0x06001D21 RID: 7457 RVA: 0x000C55B8 File Offset: 0x000C37B8
		public void SetPartsLayer(LayerName layerName)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.SetLayer(layerName);
			}
		}

		// Token: 0x06001D22 RID: 7458 RVA: 0x000C560C File Offset: 0x000C380C
		public void RefreshPartsLayer()
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.RefreshLayer();
			}
		}

		// Token: 0x06001D23 RID: 7459 RVA: 0x000C565C File Offset: 0x000C385C
		public virtual void SetPinForceMultiplier(float springMultiplier, float damperMultiplier, float posMaxForceMult, float rotMaxForceMult, bool jointLimits = true, bool noRoot = false, RagdollPart.Type partTypes = (RagdollPart.Type)0, Ragdoll.Region affectedRegion = null)
		{
			if (affectedRegion == null)
			{
				affectedRegion = this.rootRegion;
			}
			float rotationForce = this.springRotationForce * springMultiplier;
			float positionForce = this.springPositionForce * springMultiplier;
			float positionForceDamper = this.damperPositionForce * damperMultiplier;
			float rotationForceDamper = this.damperRotationForce * damperMultiplier;
			float positionForceMax = this.maxPositionForce * posMaxForceMult;
			float rotationForceMax = this.maxRotationForce * rotMaxForceMult;
			int partsCount = this.parts.Count;
			for (int i = 0; i < partsCount; i++)
			{
				RagdollPart ragdollPart = this.parts[i];
				if ((!noRoot || !(ragdollPart == this.rootPart)) && (partTypes == (RagdollPart.Type)0 || partTypes.HasFlagNoGC(ragdollPart.type)))
				{
					ragdollPart.bone.SetPinPositionForce(positionForce, positionForceDamper, positionForceMax);
					ragdollPart.bone.SetPinRotationForce(rotationForce, rotationForceDamper, rotationForceMax, affectedRegion);
					if (jointLimits)
					{
						ragdollPart.ResetCharJointLimit();
					}
					else
					{
						ragdollPart.DisableCharJointLimit();
					}
				}
			}
		}

		// Token: 0x06001D24 RID: 7460 RVA: 0x000C5738 File Offset: 0x000C3938
		public virtual void SetPinForce(float posSpring, float posDamper, float rotSpring, float rotDamper, float posMaxForce, float rotMaxForce, bool jointLimits = true, bool noRoot = false, RagdollPart.Type partTypes = (RagdollPart.Type)0, Ragdoll.Region affectedRegion = null)
		{
			if (affectedRegion == null)
			{
				affectedRegion = this.rootRegion;
			}
			foreach (RagdollPart ragdollPart in this.parts)
			{
				if ((!noRoot || !(ragdollPart == this.rootPart)) && (partTypes == (RagdollPart.Type)0 || partTypes.HasFlagNoGC(ragdollPart.type)))
				{
					ragdollPart.bone.SetPinPositionForce(posSpring, posDamper, posMaxForce);
					ragdollPart.bone.SetPinRotationForce(rotSpring, rotDamper, rotMaxForce, affectedRegion);
					if (jointLimits)
					{
						ragdollPart.ResetCharJointLimit();
					}
					else
					{
						ragdollPart.DisableCharJointLimit();
					}
				}
			}
		}

		// Token: 0x06001D25 RID: 7461 RVA: 0x000C57E8 File Offset: 0x000C39E8
		public void ResetPinForce(bool jointLimits = true, bool noRoot = false, RagdollPart.Type partTypes = (RagdollPart.Type)0)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				if ((!noRoot || !(ragdollPart == this.rootPart)) && (partTypes == (RagdollPart.Type)0 || partTypes.HasFlagNoGC(ragdollPart.type)))
				{
					ragdollPart.bone.ResetPinForce();
					if (jointLimits)
					{
						ragdollPart.ResetCharJointLimit();
					}
					else
					{
						ragdollPart.DisableCharJointLimit();
					}
				}
			}
		}

		// Token: 0x06001D26 RID: 7462 RVA: 0x000C5874 File Offset: 0x000C3A74
		public void SetPhysicModifier(object handler, float? gravityRatio = null, float massRatio = 1f, float drag = -1f, float angularDrag = -1f, EffectData effectData = null)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.collisionHandler.SetPhysicModifier(handler, gravityRatio, massRatio, drag, angularDrag, -1f, null);
			}
			Ragdoll.PhysicModifier physicModifier = this.physicModifiers.FirstOrDefault((Ragdoll.PhysicModifier p) => p.handler == handler);
			if (physicModifier != null)
			{
				if (effectData != null && effectData != physicModifier.effectData && physicModifier.effectInstance != null)
				{
					physicModifier.effectInstance.End(false, -1f);
					physicModifier.effectInstance = effectData.Spawn(this.rootPart.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					physicModifier.effectInstance.SetRenderer(this.creature.GetRendererForVFX(), false);
					physicModifier.effectInstance.Play(0, false, false);
					return;
				}
			}
			else
			{
				physicModifier = new Ragdoll.PhysicModifier(handler, effectData);
				if (effectData != null)
				{
					physicModifier.effectInstance = effectData.Spawn(this.rootPart.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					physicModifier.effectInstance.SetRenderer(this.creature.GetRendererForVFX(), false);
					physicModifier.effectInstance.Play(0, false, false);
				}
				this.physicModifiers.Add(physicModifier);
			}
		}

		// Token: 0x06001D27 RID: 7463 RVA: 0x000C59F8 File Offset: 0x000C3BF8
		public void RemovePhysicModifier(object handler)
		{
			int partsCount = this.parts.Count;
			for (int i = 0; i < partsCount; i++)
			{
				this.parts[i].collisionHandler.RemovePhysicModifier(handler);
			}
			for (int j = 0; j < this.physicModifiers.Count; j++)
			{
				Ragdoll.PhysicModifier physicModifier = this.physicModifiers[j];
				if (physicModifier.handler == handler)
				{
					if (physicModifier.effectInstance != null)
					{
						physicModifier.effectInstance.End(false, -1f);
					}
					this.physicModifiers.RemoveAtIgnoreOrder(j);
					j--;
				}
			}
		}

		// Token: 0x06001D28 RID: 7464 RVA: 0x000C5A8C File Offset: 0x000C3C8C
		public void ClearPhysicModifiers()
		{
			int partsCount = this.parts.Count;
			for (int i = 0; i < partsCount; i++)
			{
				this.parts[i].collisionHandler.ClearPhysicModifiers();
			}
			for (int j = 0; j < this.physicModifiers.Count; j++)
			{
				Ragdoll.PhysicModifier physicModifier = this.physicModifiers[j];
				if (physicModifier.effectInstance != null)
				{
					physicModifier.effectInstance.End(false, -1f);
				}
			}
			this.physicModifiers.Clear();
		}

		// Token: 0x06001D29 RID: 7465 RVA: 0x000C5B10 File Offset: 0x000C3D10
		public void EnableCharJointBreakForce(float multiplier = 1f)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.EnableCharJointBreakForce(multiplier);
			}
			this.charJointBreakEnabled = true;
		}

		// Token: 0x06001D2A RID: 7466 RVA: 0x000C5B68 File Offset: 0x000C3D68
		public void DisableCharJointBreakForce()
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				ragdollPart.ResetCharJointBreakForce();
			}
			this.charJointBreakEnabled = false;
		}

		// Token: 0x06001D2B RID: 7467 RVA: 0x000C5BC0 File Offset: 0x000C3DC0
		public virtual void IgnoreCollision(Collider collider, bool ignore, RagdollPart.Type ignoredParts = (RagdollPart.Type)0)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				if (!ignoredParts.HasFlagNoGC(ragdollPart.type))
				{
					foreach (Collider partCollider in ragdollPart.colliderGroup.colliders)
					{
						if (!(partCollider == null) && !(collider == null))
						{
							Physics.IgnoreCollision(partCollider, collider, ignore);
						}
					}
				}
			}
		}

		// Token: 0x06001D2C RID: 7468 RVA: 0x000C5C74 File Offset: 0x000C3E74
		public virtual void IgnoreCollision(Ragdoll otherRagdoll, bool ignore)
		{
			foreach (RagdollPart ragdollPart in this.parts)
			{
				foreach (Collider partCollider in ragdollPart.colliderGroup.colliders)
				{
					foreach (RagdollPart ragdollPart2 in otherRagdoll.parts)
					{
						foreach (Collider otherPartCollider in ragdollPart2.colliderGroup.colliders)
						{
							if (!(partCollider == null) && !(otherPartCollider == null))
							{
								Physics.IgnoreCollision(partCollider, otherPartCollider, ignore);
							}
						}
					}
				}
			}
		}

		// Token: 0x06001D2D RID: 7469 RVA: 0x000C5DA4 File Offset: 0x000C3FA4
		public void ForBothHands(Action<RagdollHand> action)
		{
			action(this.creature.handLeft);
			action(this.creature.handRight);
		}

		// Token: 0x06001D2E RID: 7470 RVA: 0x000C5DC8 File Offset: 0x000C3FC8
		public bool TrySlice(RagdollPart slicedPart)
		{
			Ragdoll.SliceEvent onSliceEvent = this.OnSliceEvent;
			if (onSliceEvent != null)
			{
				onSliceEvent(slicedPart, EventTime.OnStart);
			}
			EventManager.InvokeRagdollSlice(slicedPart, EventTime.OnStart);
			if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Dismemberment, BuildSettings.ContentFlagBehaviour.Discard))
			{
				return false;
			}
			if (slicedPart.isSliced)
			{
				return false;
			}
			if (slicedPart == this.rootPart)
			{
				return false;
			}
			if (!slicedPart.sliceFillMaterial)
			{
				Debug.LogError("Slice fill material is null!");
			}
			base.StartCoroutine(this.SliceCoroutine(slicedPart));
			return true;
		}

		// Token: 0x06001D2F RID: 7471 RVA: 0x000C5E3A File Offset: 0x000C403A
		public virtual void OnHeldAction(RagdollHand ragdollHand, RagdollPart part, HandleRagdoll handle, Interactable.Action action)
		{
			Ragdoll.HeldActionDelegate onHeldActionEvent = this.OnHeldActionEvent;
			if (onHeldActionEvent == null)
			{
				return;
			}
			onHeldActionEvent(ragdollHand, part, handle, action);
		}

		// Token: 0x06001D30 RID: 7472 RVA: 0x000C5E51 File Offset: 0x000C4051
		public virtual void OnTouchAction(RagdollHand ragdollHand, RagdollPart part, Interactable interactable, Interactable.Action action)
		{
			Ragdoll.TouchActionDelegate onTouchActionEvent = this.OnTouchActionEvent;
			if (onTouchActionEvent == null)
			{
				return;
			}
			onTouchActionEvent(ragdollHand, part, interactable, action);
		}

		// Token: 0x06001D31 RID: 7473 RVA: 0x000C5E68 File Offset: 0x000C4068
		public void CollisionStartStop(CollisionInstance collisionInstance, RagdollPart part, bool active)
		{
			if (active)
			{
				Ragdoll.ContactEvent onContactStartEvent = this.OnContactStartEvent;
				if (onContactStartEvent == null)
				{
					return;
				}
				onContactStartEvent(collisionInstance, part);
				return;
			}
			else
			{
				Ragdoll.ContactEvent onContactStopEvent = this.OnContactStopEvent;
				if (onContactStopEvent == null)
				{
					return;
				}
				onContactStopEvent(collisionInstance, part);
				return;
			}
		}

		// Token: 0x06001D32 RID: 7474 RVA: 0x000C5E92 File Offset: 0x000C4092
		private IEnumerator SliceCoroutine(RagdollPart slicedPart)
		{
			while (this.sliceRunning)
			{
				yield return null;
			}
			this.sliceRunning = true;
			slicedPart.DestroyCharJoint();
			if (slicedPart.bone.fixedJoint)
			{
				UnityEngine.Object.Destroy(slicedPart.bone.fixedJoint);
			}
			foreach (Ragdoll.Bone bone in slicedPart.bone.childs)
			{
				if (bone.part)
				{
					bone.SetPinPositionForce(0f, 0f, 0f);
					bone.SetPinRotationForce(0f, 0f, 0f, null);
					bone.part.collisionHandler.RemovePhysicModifier(this);
				}
			}
			if (slicedPart.data != null)
			{
				Vector3 slicePosition;
				Vector3 sliceDirection;
				slicedPart.GetSlicePositionAndDirection(out slicePosition, out sliceDirection);
				if (slicedPart.data.sliceSeparationForce > 0f && !slicedPart.physicBody.isKinematic)
				{
					slicedPart.physicBody.velocity *= slicedPart.data.sliceVelocityMultiplier;
					slicedPart.physicBody.AddForce(sliceDirection * slicedPart.data.sliceSeparationForce, ForceMode.VelocityChange);
				}
			}
			List<Ragdoll.Bone> childBones = slicedPart.bone.GetAllChilds();
			yield return this.dismemberment.DoRip(slicedPart.bone.mesh, (from b in childBones
			select b.mesh).ToArray<Transform>(), slicedPart.sliceThreshold, slicedPart.sliceFillMaterial, 1, null, null);
			yield break;
		}

		// Token: 0x06001D33 RID: 7475 RVA: 0x000C5EA8 File Offset: 0x000C40A8
		private void OnSlice(object sender, Dismemberment.CompletedEventArgs args)
		{
			if (!args.successful)
			{
				this.sliceRunning = false;
				return;
			}
			Ragdoll.Bone slicedBone = this.GetBone(args.sourceBoneToSplit);
			slicedBone.meshSplit = args.splitBone;
			slicedBone.part.slicedMeshRoot = args.splitGameObject.transform;
			slicedBone.part.slicedMeshRoot.SetParentOrigin(slicedBone.part.transform);
			this.ragdollRegions.Add(slicedBone.part.ragdollRegion.SplitFromRegion(slicedBone.part));
			for (int i = 0; i < args.sourceBonesToSplit.Length; i++)
			{
				Ragdoll.Bone slicedChildBone = this.GetBone(args.sourceBonesToSplit[i]);
				if (slicedChildBone.part)
				{
					if (slicedChildBone.part.isSliced)
					{
						break;
					}
					slicedChildBone.part.isSliced = true;
					slicedChildBone.part.RefreshLayer();
					slicedChildBone.SetPinPositionForce(0f, 0f, 0f);
					slicedChildBone.SetPinRotationForce(0f, 0f, 0f, null);
					slicedChildBone.part.collisionHandler.RemovePhysicModifier(this);
					for (int i2 = 0; i2 < slicedChildBone.part.renderers.Count; i2++)
					{
						foreach (SkinnedMeshRenderer skinnedMeshRenderer in args.sourceRenderers)
						{
							if (i2 < slicedChildBone.part.renderers.Count && i < args.sourceRenderers.Length && slicedChildBone.part.renderers[i2].renderer == args.sourceRenderers[i])
							{
								slicedChildBone.part.skinnedMeshRenderers.Add(args.splitRenderers[i]);
								slicedChildBone.part.skinnedMeshRendererIndexes.Add(i2);
							}
						}
					}
				}
				slicedChildBone.meshSplit = args.splitBones[i];
				slicedChildBone.meshSplit.name = slicedChildBone.meshSplit.name + "_Split";
				slicedChildBone.meshSplit.SetParent((slicedChildBone.mesh.parent == slicedChildBone.parent.mesh) ? slicedChildBone.parent.meshSplit : slicedChildBone.mesh.parent);
				slicedChildBone.meshSplit.localPosition = slicedChildBone.mesh.localPosition;
				slicedChildBone.meshSplit.localRotation = slicedChildBone.mesh.localRotation;
				slicedChildBone.meshSplit.localScale = slicedChildBone.mesh.localScale;
				if (i == 0)
				{
					slicedChildBone.mesh.SetParent(slicedBone.parent.mesh, true);
					slicedChildBone.mesh.localPosition = slicedBone.orgLocalPosition;
					slicedChildBone.mesh.localRotation = slicedBone.orgLocalRotation;
					slicedChildBone.mesh.localScale = Vector3.one;
				}
				else
				{
					slicedChildBone.mesh.SetParent(slicedChildBone.parent.mesh, true);
					slicedChildBone.mesh.localPosition = slicedChildBone.orgLocalPosition;
					slicedChildBone.mesh.localRotation = slicedChildBone.orgLocalRotation;
					slicedChildBone.mesh.localScale = Vector3.one;
				}
			}
			foreach (Creature.RendererData smrInfo in this.creature.renderers)
			{
				for (int j = 0; j < args.sourceRenderers.Length; j++)
				{
					if (smrInfo.renderer == args.sourceRenderers[j])
					{
						smrInfo.splitRenderer = args.splitRenderers[j];
						if (smrInfo.revealDecal && smrInfo.revealDecal.revealMaterialController)
						{
							RevealMaterialController splitRevealMaterialController = smrInfo.splitRenderer.gameObject.AddComponent<RevealMaterialController>();
							splitRevealMaterialController.CopySettingsFrom(smrInfo.revealDecal.revealMaterialController);
							smrInfo.splitReveal = smrInfo.splitRenderer.gameObject.AddComponent<RevealDecal>();
							smrInfo.splitReveal.maskWidth = smrInfo.revealDecal.maskWidth;
							smrInfo.splitReveal.maskHeight = smrInfo.revealDecal.maskHeight;
							smrInfo.splitReveal.type = smrInfo.revealDecal.type;
							smrInfo.splitReveal.revealMaterialController = splitRevealMaterialController;
						}
						if (this.creature.manikinParts)
						{
							this.creature.manikinParts.SetPartDirty(smrInfo.manikinPart, true);
						}
					}
				}
			}
			if (slicedBone.part.data != null)
			{
				Vector3 slicePosition;
				Vector3 sliceDirection;
				slicedBone.part.GetSlicePositionAndDirection(out slicePosition, out sliceDirection);
				if (slicedBone.part.data.sliceParentEffectData != null)
				{
					EffectInstance effectInstance = slicedBone.part.data.sliceParentEffectData.Spawn(slicePosition, Quaternion.LookRotation(sliceDirection), slicedBone.parent.mesh.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					effectInstance.SetIntensity(1f);
					effectInstance.Play(0, false, false);
				}
				if (slicedBone.part.data.sliceChildEffectData != null)
				{
					Vector3 childSlicePosition = slicePosition;
					Vector3 childSliceDirection = -sliceDirection;
					RagdollPart part = slicedBone.part;
					if ((part != null) ? part.slicedMeshRoot : null)
					{
						childSlicePosition = slicedBone.part.slicedMeshRoot.position;
						childSliceDirection = slicedBone.part.slicedMeshRoot.right;
					}
					EffectInstance effectInstance2 = slicedBone.part.data.sliceChildEffectData.Spawn(childSlicePosition, Quaternion.LookRotation(childSliceDirection), slicedBone.meshSplit.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					effectInstance2.SetIntensity(1f);
					effectInstance2.Play(0, false, false);
				}
			}
			if (Level.current && AreaManager.Instance)
			{
				LightVolumeReceiver lightVolumeReceiver = slicedBone.part.gameObject.AddComponent<LightVolumeReceiver>();
				lightVolumeReceiver.addMaterialInstances = false;
				lightVolumeReceiver.initRenderersOnStart = false;
				lightVolumeReceiver.currentLightProbeVolume = this.creature.lightVolumeReceiver.currentLightProbeVolume;
				lightVolumeReceiver.SetRenderers(new List<Renderer>(args.splitRenderers), false);
				lightVolumeReceiver.UpdateRenderers();
				this.creature.lightVolumeReceiver.UpdateRenderers();
			}
			this.sliceRunning = false;
			this.forcePhysic.Add(this);
			Ragdoll.SliceEvent onSliceEvent = this.OnSliceEvent;
			if (onSliceEvent != null)
			{
				onSliceEvent(slicedBone.part, EventTime.OnEnd);
			}
			EventManager.InvokeRagdollSlice(slicedBone.part, EventTime.OnEnd);
		}

		// Token: 0x06001D34 RID: 7476 RVA: 0x000C6524 File Offset: 0x000C4724
		public bool HasPenetratedPart()
		{
			int partsCount = this.parts.Count;
			for (int i = 0; i < partsCount; i++)
			{
				if (this.parts[i].IsPenetrated())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001D36 RID: 7478 RVA: 0x000C66DD File Offset: 0x000C48DD
		[CompilerGenerated]
		internal static float <Init>g__GetStandingMass|129_0(RagdollPart part)
		{
			return part.standingMass;
		}

		// Token: 0x06001D37 RID: 7479 RVA: 0x000C66E5 File Offset: 0x000C48E5
		[CompilerGenerated]
		internal static float <Init>g__GetHandledMass|129_2(RagdollPart part)
		{
			return part.handledMass;
		}

		// Token: 0x06001D38 RID: 7480 RVA: 0x000C66ED File Offset: 0x000C48ED
		[CompilerGenerated]
		internal static float <Init>g__GetRagdolledMass|129_4(RagdollPart part)
		{
			return part.ragdolledMass;
		}

		// Token: 0x04001B93 RID: 7059
		public Transform meshRig;

		// Token: 0x04001B94 RID: 7060
		public Transform meshRootBone;

		// Token: 0x04001B95 RID: 7061
		[Header("Parts")]
		public RagdollPart headPart;

		// Token: 0x04001B96 RID: 7062
		public RagdollPart leftUpperArmPart;

		// Token: 0x04001B97 RID: 7063
		public RagdollPart rightUpperArmPart;

		// Token: 0x04001B98 RID: 7064
		public RagdollPart targetPart;

		// Token: 0x04001B99 RID: 7065
		public RagdollPart rootPart;

		// Token: 0x04001B9A RID: 7066
		[Header("Default forces")]
		public float springPositionForce = 1000f;

		// Token: 0x04001B9B RID: 7067
		public float damperPositionForce = 50f;

		// Token: 0x04001B9C RID: 7068
		public float maxPositionForce = 1000f;

		// Token: 0x04001B9D RID: 7069
		public float springRotationForce = 800f;

		// Token: 0x04001B9E RID: 7070
		public float damperRotationForce = 50f;

		// Token: 0x04001B9F RID: 7071
		public float maxRotationForce = 100f;

		// Token: 0x04001BA0 RID: 7072
		[Header("Destabilized")]
		public float destabilizedSpringRotationMultiplier = 0.5f;

		// Token: 0x04001BA1 RID: 7073
		public float destabilizedDamperRotationMultiplier = 0.1f;

		// Token: 0x04001BA2 RID: 7074
		public float destabilizedGroundSpringRotationMultiplier = 0.2f;

		// Token: 0x04001BA3 RID: 7075
		[Header("HipsAttached")]
		public float hipsAttachedSpringPositionMultiplier = 1f;

		// Token: 0x04001BA4 RID: 7076
		public float hipsAttachedDamperPositionMultiplier;

		// Token: 0x04001BA5 RID: 7077
		public float hipsAttachedSpringRotationMultiplier = 1f;

		// Token: 0x04001BA6 RID: 7078
		public float hipsAttachedDamperRotationMultiplier;

		// Token: 0x04001BA7 RID: 7079
		[Header("StandUp")]
		public AnimationCurve standUpCurve;

		// Token: 0x04001BA8 RID: 7080
		public float standUpFromGrabDuration = 1f;

		// Token: 0x04001BA9 RID: 7081
		public float preStandUpDuration = 3f;

		// Token: 0x04001BAA RID: 7082
		public float preStandUpRatio = 0.7f;

		// Token: 0x04001BAB RID: 7083
		[Header("Player arm")]
		public float playerArmPositionSpring = 5000f;

		// Token: 0x04001BAC RID: 7084
		public float playerArmPositionDamper = 40f;

		// Token: 0x04001BAD RID: 7085
		public float playerArmRotationSpring = 1000f;

		// Token: 0x04001BAE RID: 7086
		public float playerArmRotationDamper = 40f;

		// Token: 0x04001BAF RID: 7087
		public float playerArmMaxPositionForce = 3000f;

		// Token: 0x04001BB0 RID: 7088
		public float playerArmMaxRotationForce = 250f;

		// Token: 0x04001BB1 RID: 7089
		[Header("Collision")]
		public float collisionEffectMinDelay = 0.2f;

		// Token: 0x04001BB2 RID: 7090
		public float collisionMinVelocity = 2f;

		// Token: 0x04001BB3 RID: 7091
		[NonSerialized]
		public float lastCollisionEffectTime;

		// Token: 0x04001BB4 RID: 7092
		[Header("Misc")]
		public bool allowSelfDamage;

		// Token: 0x04001BB5 RID: 7093
		public bool grippable = true;

		// Token: 0x04001BB6 RID: 7094
		[NonSerialized]
		public Creature creature;

		// Token: 0x04001BB7 RID: 7095
		[Header("Physic toggle")]
		public bool physicToggle;

		// Token: 0x04001BB8 RID: 7096
		public float physicTogglePlayerRadius = 5f;

		// Token: 0x04001BB9 RID: 7097
		public float physicToggleRagdollRadius = 3f;

		// Token: 0x04001BBA RID: 7098
		public float physicEnabledDuration = 2f;

		// Token: 0x04001BBB RID: 7099
		public float lastPhysicToggleTime;

		// Token: 0x04001BBC RID: 7100
		[NonSerialized]
		public bool shouldEnablePhysic;

		// Token: 0x04001BBD RID: 7101
		public static bool playerPhysicBody;

		// Token: 0x04001BBE RID: 7102
		[NonSerialized]
		public Transform animatorRig;

		// Token: 0x04001BBF RID: 7103
		[NonSerialized]
		public float totalMass;

		// Token: 0x04001BC0 RID: 7104
		public Ragdoll.State state = Ragdoll.State.Disabled;

		// Token: 0x04001BC1 RID: 7105
		public bool hipsAttached;

		// Token: 0x04001BC2 RID: 7106
		public List<RagdollPart> parts;

		// Token: 0x04001BC3 RID: 7107
		[NonSerialized]
		public List<Ragdoll.Bone> bones = new List<Ragdoll.Bone>();

		// Token: 0x04001BC4 RID: 7108
		[NonSerialized]
		public Ragdoll.Region rootRegion;

		// Token: 0x04001BC5 RID: 7109
		[NonSerialized]
		public List<Ragdoll.Region> ragdollRegions = new List<Ragdoll.Region>();

		// Token: 0x04001BC6 RID: 7110
		[NonSerialized]
		public IkController ik;

		// Token: 0x04001BC7 RID: 7111
		[NonSerialized]
		public HumanoidFullBodyIK humanoidIk;

		// Token: 0x04001BC8 RID: 7112
		[NonSerialized]
		public List<RagdollHand> handlers = new List<RagdollHand>();

		// Token: 0x04001BC9 RID: 7113
		[NonSerialized]
		public List<SpellCaster> tkHandlers = new List<SpellCaster>();

		// Token: 0x04001BCA RID: 7114
		[NonSerialized]
		public bool isGrabbed;

		// Token: 0x04001BCB RID: 7115
		[NonSerialized]
		public bool isTkGrabbed;

		// Token: 0x04001BCC RID: 7116
		[NonSerialized]
		public bool isSliced;

		// Token: 0x04001BCD RID: 7117
		[NonSerialized]
		public bool charJointBreakEnabled;

		// Token: 0x04001BCE RID: 7118
		public BoolHandler forcePhysic;

		// Token: 0x04001BCF RID: 7119
		[NonSerialized]
		public List<Ragdoll.PhysicModifier> physicModifiers = new List<Ragdoll.PhysicModifier>();

		// Token: 0x04001BD0 RID: 7120
		[NonSerialized]
		public bool initialized;

		// Token: 0x04001BD1 RID: 7121
		[NonSerialized]
		public bool standingUp;

		// Token: 0x04001BD2 RID: 7122
		[NonSerialized]
		public float standStartTime;

		// Token: 0x04001BD3 RID: 7123
		protected Coroutine getUpCoroutine;

		// Token: 0x04001BD4 RID: 7124
		private ConfigurableJoint stabilizationJoint;

		// Token: 0x04001BD5 RID: 7125
		private GameObject stabilizationJointFollowObject;

		// Token: 0x04001BD6 RID: 7126
		private List<Ragdoll.StabilizationJointQueue> stabilizationJointQueue = new List<Ragdoll.StabilizationJointQueue>();

		// Token: 0x04001BE1 RID: 7137
		private Dismemberment dismemberment;

		// Token: 0x04001BE3 RID: 7139
		[NonSerialized]
		public bool hasMetalArmor;

		// Token: 0x04001BE4 RID: 7140
		[NonSerialized]
		public bool meshRaycast = true;

		// Token: 0x020008F2 RID: 2290
		public enum State
		{
			// Token: 0x04004326 RID: 17190
			Inert,
			// Token: 0x04004327 RID: 17191
			Destabilized,
			// Token: 0x04004328 RID: 17192
			Frozen,
			// Token: 0x04004329 RID: 17193
			Standing,
			// Token: 0x0400432A RID: 17194
			Kinematic,
			// Token: 0x0400432B RID: 17195
			NoPhysic,
			// Token: 0x0400432C RID: 17196
			Disabled
		}

		// Token: 0x020008F3 RID: 2291
		[Serializable]
		public class Region
		{
			// Token: 0x060041F2 RID: 16882 RVA: 0x0018C138 File Offset: 0x0018A338
			public Region(List<RagdollPart> partsList, bool copy)
			{
				this.parts = (copy ? partsList.ToList<RagdollPart>() : partsList);
				for (int p = 0; p < this.parts.Count; p++)
				{
					this.parts[p].ragdollRegion = this;
				}
			}

			// Token: 0x060041F3 RID: 16883 RVA: 0x0018C188 File Offset: 0x0018A388
			public Ragdoll.Region SplitFromRegion(RagdollPart part)
			{
				List<RagdollPart> newPartList = new List<RagdollPart>();
				List<RagdollPart> frontier = new List<RagdollPart>
				{
					part
				};
				while (frontier.Count > 0)
				{
					RagdollPart top = frontier[0];
					newPartList.Add(top);
					this.parts.Remove(top);
					List<RagdollPart> childParts = top.childParts;
					if (childParts != null && childParts.Count > 0)
					{
						frontier.AddRange(top.childParts);
					}
					frontier.RemoveAt(0);
				}
				return new Ragdoll.Region(newPartList, false);
			}

			// Token: 0x0400432D RID: 17197
			public List<RagdollPart> parts;
		}

		// Token: 0x020008F4 RID: 2292
		public class PhysicModifier
		{
			// Token: 0x060041F4 RID: 16884 RVA: 0x0018C200 File Offset: 0x0018A400
			public PhysicModifier(object handler, EffectData effectData = null)
			{
				this.handler = handler;
				this.effectData = effectData;
			}

			// Token: 0x0400432E RID: 17198
			public object handler;

			// Token: 0x0400432F RID: 17199
			public EffectData effectData;

			// Token: 0x04004330 RID: 17200
			[NonSerialized]
			public EffectInstance effectInstance;
		}

		/// <summary> 
		/// Helper class to keep the line of which objects would like to apply a stabilization joint.
		/// </summary>
		// Token: 0x020008F5 RID: 2293
		private class StabilizationJointQueue
		{
			// Token: 0x060041F5 RID: 16885 RVA: 0x0018C216 File Offset: 0x0018A416
			public StabilizationJointQueue(GameObject owningObject, Ragdoll.StabilizationJointSettings settings)
			{
				this.owningObject = owningObject;
				this.settings = settings;
			}

			// Token: 0x04004331 RID: 17201
			public GameObject owningObject;

			// Token: 0x04004332 RID: 17202
			public Ragdoll.StabilizationJointSettings settings;
		}

		/// <summary>
		/// Helper class to assign necessary settings to the joint,
		/// nearly identical to the stabilization joint, but additional stuff might be needed sometimes
		/// </summary>
		// Token: 0x020008F6 RID: 2294
		public class StabilizationJointSettings
		{
			// Token: 0x04004333 RID: 17203
			public Vector3 axis = Vector3.zero;

			// Token: 0x04004334 RID: 17204
			public ConfigurableJointMotion angularXMotion = ConfigurableJointMotion.Free;

			// Token: 0x04004335 RID: 17205
			public ConfigurableJointMotion angularYMotion = ConfigurableJointMotion.Free;

			// Token: 0x04004336 RID: 17206
			public ConfigurableJointMotion angularZMotion = ConfigurableJointMotion.Free;

			// Token: 0x04004337 RID: 17207
			public bool configuredInWorldSpace;

			// Token: 0x04004338 RID: 17208
			public bool autoConfigureConnectedAnchor = true;

			// Token: 0x04004339 RID: 17209
			public SoftJointLimit angularXLimit;

			// Token: 0x0400433A RID: 17210
			public SoftJointLimitSpring angularXLimitSpring;

			// Token: 0x0400433B RID: 17211
			public SoftJointLimit angularYLimit;

			// Token: 0x0400433C RID: 17212
			public SoftJointLimit angularZLimit;

			// Token: 0x0400433D RID: 17213
			public SoftJointLimitSpring angularYZLimitSpring;

			// Token: 0x0400433E RID: 17214
			public JointDrive angularYZDrive;

			// Token: 0x0400433F RID: 17215
			public JointDrive angularXDrive;

			// Token: 0x04004340 RID: 17216
			public bool isKinematic;

			// Token: 0x04004341 RID: 17217
			public GameObject relativeObject;
		}

		// Token: 0x020008F7 RID: 2295
		public enum PhysicStateChange
		{
			// Token: 0x04004343 RID: 17219
			None,
			// Token: 0x04004344 RID: 17220
			ParentingToPhysic,
			// Token: 0x04004345 RID: 17221
			PhysicToParenting
		}

		// Token: 0x020008F8 RID: 2296
		// (Invoke) Token: 0x060041F8 RID: 16888
		public delegate void StateChange(Ragdoll.State previousState, Ragdoll.State newState, Ragdoll.PhysicStateChange physicStateChange, EventTime eventTime);

		// Token: 0x020008F9 RID: 2297
		// (Invoke) Token: 0x060041FC RID: 16892
		public delegate void SliceEvent(RagdollPart ragdollPart, EventTime eventTime);

		// Token: 0x020008FA RID: 2298
		// (Invoke) Token: 0x06004200 RID: 16896
		public delegate void TouchActionDelegate(RagdollHand ragdollHand, RagdollPart part, Interactable interactable, Interactable.Action action);

		// Token: 0x020008FB RID: 2299
		// (Invoke) Token: 0x06004204 RID: 16900
		public delegate void HeldActionDelegate(RagdollHand ragdollHand, RagdollPart part, HandleRagdoll handle, Interactable.Action action);

		// Token: 0x020008FC RID: 2300
		// (Invoke) Token: 0x06004208 RID: 16904
		public delegate void TelekinesisGrabEvent(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll);

		// Token: 0x020008FD RID: 2301
		// (Invoke) Token: 0x0600420C RID: 16908
		public delegate void TelekinesisReleaseEvent(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll, bool lastHandler);

		// Token: 0x020008FE RID: 2302
		// (Invoke) Token: 0x06004210 RID: 16912
		public delegate void GrabEvent(RagdollHand ragdollHand, HandleRagdoll handleRagdoll);

		// Token: 0x020008FF RID: 2303
		// (Invoke) Token: 0x06004214 RID: 16916
		public delegate void UngrabEvent(RagdollHand ragdollHand, HandleRagdoll handleRagdoll, bool lastHandler);

		// Token: 0x02000900 RID: 2304
		// (Invoke) Token: 0x06004218 RID: 16920
		public delegate void ContactEvent(CollisionInstance collisionInstance, RagdollPart ragdollPart);

		// Token: 0x02000901 RID: 2305
		[Serializable]
		public class Bone
		{
			// Token: 0x0600421B RID: 16923 RVA: 0x0018C25C File Offset: 0x0018A45C
			public Bone(Creature creature, Transform mesh, Transform animation, RagdollPart part)
			{
				this.mesh = mesh;
				this.animation = animation;
				this.part = part;
				this.orgLocalPosition = animation.localPosition;
				this.orgLocalRotation = animation.localRotation;
				this.orgCreatureLocalPosition = creature.transform.InverseTransformPoint(animation.position);
				this.orgCreatureLocalRotation = Quaternion.Inverse(animation.rotation) * creature.transform.rotation;
				this.childs = new List<Ragdoll.Bone>();
				if (part)
				{
					this.boneHashes = new int[part.linkedMeshBones.Length + 1];
					this.boneHashes[0] = Animator.StringToHash(mesh.name);
					for (int i = 0; i < part.linkedMeshBones.Length; i++)
					{
						this.boneHashes[i + 1] = Animator.StringToHash(part.linkedMeshBones[i].name);
					}
					GameObject animationGo = new GameObject("AnimAnchor");
					animationGo.transform.SetParentOrigin(this.animation);
					Rigidbody rigidbody = animationGo.AddComponent<Rigidbody>();
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					this.animationJoint = animationGo.AddComponent<ConfigurableJoint>();
					part.transform.SetPositionAndRotation(animation.position, animation.rotation);
					this.animationJoint.autoConfigureConnectedAnchor = false;
					this.animationJoint.connectedAnchor = Vector3.zero;
					this.animationJoint.SetConnectedPhysicBody(part.physicBody);
					part.bone = this;
					part.OnGrabbed += this.PartGrabbed;
					part.OnUngrabbed += this.PartUngrabbed;
					part.OnTKGrab += this.PartTKGrab;
					part.OnTKRelease += this.PartTKRelease;
					return;
				}
				this.boneHashes = new int[1];
				this.boneHashes[0] = Animator.StringToHash(mesh.name);
			}

			// Token: 0x0600421C RID: 16924 RVA: 0x0018C43B File Offset: 0x0018A63B
			private void PartGrabbed(RagdollHand ragdollHand, HandleRagdoll handle)
			{
				this.UpdatePartMass();
			}

			// Token: 0x0600421D RID: 16925 RVA: 0x0018C443 File Offset: 0x0018A643
			private void PartUngrabbed(RagdollHand ragdollHand, HandleRagdoll handle)
			{
				this.UpdatePartMass();
			}

			// Token: 0x0600421E RID: 16926 RVA: 0x0018C44B File Offset: 0x0018A64B
			private void PartTKGrab(SpellTelekinesis spellTelekinesis, HandleRagdoll handle)
			{
				this.UpdatePartMass();
			}

			// Token: 0x0600421F RID: 16927 RVA: 0x0018C453 File Offset: 0x0018A653
			private void PartTKRelease(SpellTelekinesis spellTelekinesis, HandleRagdoll handle)
			{
				this.UpdatePartMass();
			}

			// Token: 0x06004220 RID: 16928 RVA: 0x0018C45C File Offset: 0x0018A65C
			private void UpdatePartMass()
			{
				List<RagdollHand> handlers = this.part.ragdoll.handlers;
				int num = (handlers != null) ? handlers.Count : 0;
				List<SpellCaster> tkHandlers = this.part.ragdoll.tkHandlers;
				if (num + ((tkHandlers != null) ? tkHandlers.Count : 0) > 0)
				{
					this.part.physicBody.mass = this.part.handledMass;
					return;
				}
				this.part.physicBody.mass = Mathf.Lerp(this.part.ragdolledMass, this.part.standingMass, Mathf.Clamp01(this.animationJoint.xDrive.positionSpring / this.part.ragdoll.springPositionForce));
			}

			// Token: 0x06004221 RID: 16929 RVA: 0x0018C518 File Offset: 0x0018A718
			public List<Ragdoll.Bone> GetAllChilds()
			{
				List<Ragdoll.Bone> allChilds = new List<Ragdoll.Bone>();
				allChilds.Add(this);
				foreach (Ragdoll.Bone child in this.childs)
				{
					allChilds.AddRange(child.GetAllChilds());
				}
				return allChilds;
			}

			// Token: 0x06004222 RID: 16930 RVA: 0x0018C580 File Offset: 0x0018A780
			public float GetAnimationBoneHeight()
			{
				return this.part.ragdoll.creature.animator.transform.InverseTransformPointUnscaled(this.animation.position).y;
			}

			// Token: 0x06004223 RID: 16931 RVA: 0x0018C5B4 File Offset: 0x0018A7B4
			public void SetPinPositionForce(float spring, float damper, float maxForce)
			{
				if (this.part.isSliced)
				{
					spring = 0f;
					damper = 0f;
					maxForce = 0f;
				}
				JointDrive jointDrive = default(JointDrive);
				jointDrive.positionSpring = spring;
				jointDrive.positionDamper = damper;
				jointDrive.maximumForce = maxForce;
				this.animationJoint.xDrive = jointDrive;
				this.animationJoint.yDrive = jointDrive;
				this.animationJoint.zDrive = jointDrive;
				this.UpdatePartMass();
			}

			// Token: 0x06004224 RID: 16932 RVA: 0x0018C630 File Offset: 0x0018A830
			public void SetPinRotationForce(float spring, float damper, float maxForce, Ragdoll.Region region = null)
			{
				if (region == null)
				{
					region = this.part.ragdoll.rootRegion;
				}
				if (this.part.ragdollRegion != region)
				{
					spring = 0f;
					damper = 0f;
					maxForce = 0f;
				}
				this.animationJoint.rotationDriveMode = RotationDriveMode.Slerp;
				JointDrive jointDrive = default(JointDrive);
				jointDrive.positionSpring = spring;
				jointDrive.positionDamper = damper;
				jointDrive.maximumForce = maxForce;
				this.animationJoint.slerpDrive = jointDrive;
			}

			// Token: 0x06004225 RID: 16933 RVA: 0x0018C6B0 File Offset: 0x0018A8B0
			public void ResetPinForce()
			{
				if (!this.part.isSliced)
				{
					this.SetPinPositionForce(this.part.ragdoll.springPositionForce, this.part.ragdoll.damperPositionForce, this.part.ragdoll.maxPositionForce);
					this.SetPinRotationForce(this.part.ragdoll.springRotationForce, this.part.ragdoll.damperRotationForce, this.part.ragdoll.maxRotationForce, null);
				}
			}

			// Token: 0x04004346 RID: 17222
			public int[] boneHashes;

			// Token: 0x04004347 RID: 17223
			public Transform mesh;

			// Token: 0x04004348 RID: 17224
			public Transform animation;

			// Token: 0x04004349 RID: 17225
			public Transform meshSplit;

			// Token: 0x0400434A RID: 17226
			public ConfigurableJoint animationJoint;

			// Token: 0x0400434B RID: 17227
			public FixedJoint fixedJoint;

			// Token: 0x0400434C RID: 17228
			public RagdollPart part;

			// Token: 0x0400434D RID: 17229
			public Ragdoll.Bone parent;

			// Token: 0x0400434E RID: 17230
			public List<Ragdoll.Bone> childs;

			// Token: 0x0400434F RID: 17231
			public bool hasChildAnimationJoint;

			// Token: 0x04004350 RID: 17232
			public Vector3 orgLocalPosition;

			// Token: 0x04004351 RID: 17233
			public Quaternion orgLocalRotation;

			// Token: 0x04004352 RID: 17234
			public Vector3 orgCreatureLocalPosition;

			// Token: 0x04004353 RID: 17235
			public Quaternion orgCreatureLocalRotation;
		}
	}
}

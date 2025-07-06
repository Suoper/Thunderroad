using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000315 RID: 789
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ColliderGroup.html")]
	public class ColliderGroup : ThunderBehaviour
	{
		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06002586 RID: 9606 RVA: 0x00101347 File Offset: 0x000FF547
		// (set) Token: 0x06002587 RID: 9607 RVA: 0x00101354 File Offset: 0x000FF554
		public Imbue imbue
		{
			get
			{
				return this.RootGroup._imbue;
			}
			set
			{
				this._imbue = value;
			}
		}

		// Token: 0x14000120 RID: 288
		// (add) Token: 0x06002588 RID: 9608 RVA: 0x00101360 File Offset: 0x000FF560
		// (remove) Token: 0x06002589 RID: 9609 RVA: 0x00101398 File Offset: 0x000FF598
		public event Action loadedEvent;

		// Token: 0x0600258A RID: 9610 RVA: 0x001013CD File Offset: 0x000FF5CD
		protected void Awake()
		{
			this.GroupSetup();
			this.name = base.name;
		}

		// Token: 0x0600258B RID: 9611 RVA: 0x001013E4 File Offset: 0x000FF5E4
		private void Start()
		{
			if (!this.collisionHandler && string.IsNullOrEmpty(this.colliderGroupId))
			{
				Debug.LogErrorFormat(this, "ColliderGroup have no collisionHandler, this will cause errors during collision. " + base.gameObject.GetPathFromRoot(), Array.Empty<object>());
			}
			if (this.collisionHandler == null && !string.IsNullOrEmpty(this.colliderGroupId))
			{
				ColliderGroupData groupData = Catalog.GetData<ColliderGroupData>(this.colliderGroupId, true);
				if (groupData != null)
				{
					this.Load(groupData);
				}
			}
		}

		// Token: 0x0600258C RID: 9612 RVA: 0x00101460 File Offset: 0x000FF660
		internal void GroupSetup()
		{
			this.colliders = new List<Collider>(0);
			base.GetComponentsInChildren<Collider>(true, this.colliders);
			if (this.colliders.IsNullOrEmpty())
			{
				Debug.LogWarning("ColliderGroup on : " + base.gameObject.GetPathFromRoot() + " has no colliders under it. Was it added by mistake?", this);
				return;
			}
			this.collisionHandler = base.GetComponentInParent<CollisionHandler>();
			CollisionHandler collisionHandler = this.collisionHandler;
			RagdollPart part = (collisionHandler != null) ? collisionHandler.ragdollPart : null;
			if (part != null && part.hasMetalArmor)
			{
				this.isMetal = true;
			}
			Vector3 collidersCentroid = Vector3.zero;
			int collidersCount = this.colliders.Count;
			for (int i = 0; i < collidersCount; i++)
			{
				Collider collider = this.colliders[i];
				if (!this.whooshPoint)
				{
					CapsuleCollider capsuleCollider = collider as CapsuleCollider;
					if (capsuleCollider == null)
					{
						SphereCollider sphereCollider = collider as SphereCollider;
						if (sphereCollider == null)
						{
							BoxCollider boxCollider = collider as BoxCollider;
							if (boxCollider == null)
							{
								MeshCollider meshCollider = collider as MeshCollider;
								if (meshCollider != null)
								{
									collidersCentroid += meshCollider.transform.position;
								}
							}
							else
							{
								collidersCentroid += boxCollider.transform.TransformPoint(boxCollider.center);
							}
						}
						else
						{
							collidersCentroid += sphereCollider.transform.TransformPoint(sphereCollider.center);
						}
					}
					else
					{
						collidersCentroid += capsuleCollider.transform.TransformPoint(capsuleCollider.center);
					}
					if (!this.isMetal)
					{
						MaterialData material = MaterialData.GetMaterial(collider);
						if (material != null && material.isMetal)
						{
							this.isMetal = true;
						}
					}
				}
			}
			if (!this.whooshPoint)
			{
				this.whooshPoint = new GameObject("WhooshPoint").transform;
				this.whooshPoint.SetParentOrigin(base.transform);
				this.whooshPoint.position = collidersCentroid / (float)collidersCount;
			}
			this.modifier = new ColliderGroupData.Modifier();
			this.data = new ColliderGroupData();
			this.data.id = "Default";
		}

		// Token: 0x0600258D RID: 9613 RVA: 0x00101654 File Offset: 0x000FF854
		public Bounds Bounds()
		{
			if (this.colliders.Count == 0)
			{
				return default(Bounds);
			}
			Bounds bounds = default(Bounds);
			foreach (Collider collider in this.colliders)
			{
				bounds.Encapsulate(collider.bounds);
			}
			return bounds;
		}

		// Token: 0x0600258E RID: 9614 RVA: 0x001016D0 File Offset: 0x000FF8D0
		public Vector3 ClosestPoint(Vector3 point)
		{
			if (this.colliders.Count == 0)
			{
				return default(Vector3);
			}
			Vector3 closest = default(Vector3);
			float distance = float.PositiveInfinity;
			for (int i = 0; i < this.colliders.Count; i++)
			{
				Vector3 newPoint = this.colliders[i].ClosestPoint(point);
				if ((point - newPoint).sqrMagnitude < distance)
				{
					closest = newPoint;
				}
			}
			return closest;
		}

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x0600258F RID: 9615 RVA: 0x00101743 File Offset: 0x000FF943
		public ColliderGroup RootGroup
		{
			get
			{
				ColliderGroup colliderGroup = this.parentGroup;
				return ((colliderGroup != null) ? colliderGroup.RootGroup : null) ?? this;
			}
		}

		// Token: 0x06002590 RID: 9616 RVA: 0x0010175C File Offset: 0x000FF95C
		public void Load(ColliderGroupData colliderGroupData)
		{
			this.data = (colliderGroupData.Clone() as ColliderGroupData);
			this.modifier = this.data.GetModifier(this);
			List<ColliderGroup> list = this.subImbueGroups;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i])
					{
						list[i].parentGroup = this;
					}
				}
			}
			if (this.modifier.imbueType != ColliderGroupData.ImbueType.None && this.parentGroup == null)
			{
				this.imbue = base.gameObject.AddComponent<Imbue>();
				this.imbue.OnImbueSpellChange += delegate(Imbue imbue, SpellCastCharge spellData, float amount, float change, EventTime time)
				{
					if (time == EventTime.OnEnd)
					{
						CollisionHandler collisionHandler = this.collisionHandler;
						if (collisionHandler == null)
						{
							return;
						}
						collisionHandler.item.InvokeOnImbuesChange(spellData, amount, change, time);
					}
				};
			}
			else if (this.imbue && this.parentGroup == null)
			{
				UnityEngine.Object.Destroy(this.imbue);
			}
			if (this.loadedEvent != null)
			{
				this.loadedEvent();
			}
		}

		// Token: 0x06002591 RID: 9617 RVA: 0x00101840 File Offset: 0x000FFA40
		public void GenerateImbueMesh()
		{
			this.colliders = new List<Collider>(base.GetComponentsInChildren<Collider>());
			List<CombineInstance> combines = new List<CombineInstance>();
			List<Vector3> orgScales = new List<Vector3>();
			foreach (Collider collider in this.colliders)
			{
				CombineInstance combineInstance = default(CombineInstance);
				orgScales.Add(collider.transform.localScale);
				if (collider is BoxCollider)
				{
					combineInstance.mesh = this.GenerateCubeMesh((collider as BoxCollider).size);
					combineInstance.transform = base.transform.worldToLocalMatrix * (collider.transform.localToWorldMatrix * Matrix4x4.Translate((collider as BoxCollider).center));
				}
				else if (collider is CapsuleCollider)
				{
					float height = (collider as CapsuleCollider).height;
					float radius = (collider as CapsuleCollider).radius;
					Vector3 scale = Vector3.one;
					if ((collider as CapsuleCollider).direction == 0)
					{
						scale = new Vector3(height, radius, radius);
						collider.transform.localScale = new Vector3(collider.transform.localScale.x, Mathf.Max(collider.transform.localScale.y, collider.transform.localScale.z), Mathf.Max(collider.transform.localScale.y, collider.transform.localScale.z));
					}
					if ((collider as CapsuleCollider).direction == 1)
					{
						scale = new Vector3(radius, height, radius);
						collider.transform.localScale = new Vector3(Mathf.Max(collider.transform.localScale.x, collider.transform.localScale.z), collider.transform.localScale.y, Mathf.Max(collider.transform.localScale.x, collider.transform.localScale.z));
					}
					if ((collider as CapsuleCollider).direction == 2)
					{
						scale = new Vector3(radius, radius, height);
						collider.transform.localScale = new Vector3(Mathf.Max(collider.transform.localScale.x, collider.transform.localScale.y), Mathf.Max(collider.transform.localScale.x, collider.transform.localScale.y), collider.transform.localScale.z);
					}
					combineInstance.mesh = this.GenerateCubeMesh(scale);
					combineInstance.transform = base.transform.worldToLocalMatrix * (collider.transform.localToWorldMatrix * Matrix4x4.Translate((collider as CapsuleCollider).center));
				}
				else if (collider is SphereCollider)
				{
					float maxSize = Mathf.Max(new float[]
					{
						collider.transform.localScale.x,
						collider.transform.localScale.y,
						collider.transform.localScale.z
					});
					collider.transform.localScale = new Vector3(maxSize, maxSize, maxSize);
					combineInstance.mesh = ColliderGroup.GenerateIcoSphereMesh(4, (collider as SphereCollider).radius);
					Matrix4x4 transformMatrix = Matrix4x4.Translate((collider as SphereCollider).center) * Matrix4x4.Scale(new Vector3(1f / collider.transform.lossyScale.x, 1f / collider.transform.lossyScale.y, 1f / collider.transform.lossyScale.z));
					combineInstance.transform = base.transform.worldToLocalMatrix * (collider.transform.localToWorldMatrix * transformMatrix);
				}
				else if (collider is MeshCollider)
				{
					combineInstance.mesh = (collider as MeshCollider).sharedMesh;
					combineInstance.transform = base.transform.worldToLocalMatrix * collider.transform.localToWorldMatrix;
				}
				combines.Add(combineInstance);
			}
			Mesh imbueMesh = new Mesh();
			imbueMesh.name = "GeneratedMesh";
			imbueMesh.CombineMeshes(combines.ToArray());
			int i = 0;
			foreach (Collider collider2 in this.colliders)
			{
				collider2.transform.localScale = orgScales[i];
				i++;
			}
			MeshFilter meshFilter = new GameObject("ImbueGeneratedMesh").AddComponent<MeshFilter>();
			meshFilter.transform.SetParent(base.transform);
			meshFilter.transform.localPosition = Vector3.zero;
			meshFilter.transform.localRotation = Quaternion.identity;
			meshFilter.transform.localScale = Vector3.one;
			meshFilter.sharedMesh = imbueMesh;
			this.imbueEffectRenderer = meshFilter.gameObject.AddComponent<MeshRenderer>();
		}

		// Token: 0x06002592 RID: 9618 RVA: 0x00101DA0 File Offset: 0x000FFFA0
		private Mesh GenerateCubeMesh(Vector3 size)
		{
			Vector3 p0 = new Vector3(-size.x * 0.5f, -size.y * 0.5f, size.z * 0.5f);
			Vector3 p = new Vector3(size.x * 0.5f, -size.y * 0.5f, size.z * 0.5f);
			Vector3 p2 = new Vector3(size.x * 0.5f, -size.y * 0.5f, -size.z * 0.5f);
			Vector3 p3 = new Vector3(-size.x * 0.5f, -size.y * 0.5f, -size.z * 0.5f);
			Vector3 p4 = new Vector3(-size.x * 0.5f, size.y * 0.5f, size.z * 0.5f);
			Vector3 p5 = new Vector3(size.x * 0.5f, size.y * 0.5f, size.z * 0.5f);
			Vector3 p6 = new Vector3(size.x * 0.5f, size.y * 0.5f, -size.z * 0.5f);
			Vector3 p7 = new Vector3(-size.x * 0.5f, size.y * 0.5f, -size.z * 0.5f);
			Vector3[] vertices = new Vector3[]
			{
				p0,
				p,
				p2,
				p3,
				p7,
				p4,
				p0,
				p3,
				p4,
				p5,
				p,
				p0,
				p6,
				p7,
				p3,
				p2,
				p5,
				p6,
				p2,
				p,
				p7,
				p6,
				p5,
				p4
			};
			int[] triangles = new int[]
			{
				3,
				1,
				0,
				3,
				2,
				1,
				7,
				5,
				4,
				7,
				6,
				5,
				11,
				9,
				8,
				11,
				10,
				9,
				15,
				13,
				12,
				15,
				14,
				13,
				19,
				17,
				16,
				19,
				18,
				17,
				23,
				21,
				20,
				23,
				22,
				21
			};
			Mesh mesh = new Mesh();
			mesh.name = "GeneratedCubeMesh";
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.Optimize();
			mesh.RecalculateNormals();
			return mesh;
		}

		// Token: 0x06002593 RID: 9619 RVA: 0x00102038 File Offset: 0x00100238
		public static Mesh GenerateIcoSphereMesh(int n, float radius)
		{
			int num = n * 4;
			int vertexNum = num * num / 16 * 24;
			Vector3[] vertices = new Vector3[vertexNum];
			int[] triangles = new int[vertexNum];
			Vector2[] uv = new Vector2[vertexNum];
			Quaternion[] init_vectors = new Quaternion[]
			{
				new Quaternion(0f, 1f, 0f, 0f),
				new Quaternion(0f, 0f, 1f, 0f),
				new Quaternion(1f, 0f, 0f, 0f),
				new Quaternion(0f, -1f, 0f, 0f),
				new Quaternion(1f, 0f, 0f, 0f),
				new Quaternion(0f, 0f, 1f, 0f),
				new Quaternion(0f, 1f, 0f, 0f),
				new Quaternion(-1f, 0f, 0f, 0f),
				new Quaternion(0f, 0f, 1f, 0f),
				new Quaternion(0f, -1f, 0f, 0f),
				new Quaternion(0f, 0f, 1f, 0f),
				new Quaternion(-1f, 0f, 0f, 0f),
				new Quaternion(0f, 1f, 0f, 0f),
				new Quaternion(1f, 0f, 0f, 0f),
				new Quaternion(0f, 0f, -1f, 0f),
				new Quaternion(0f, 1f, 0f, 0f),
				new Quaternion(0f, 0f, -1f, 0f),
				new Quaternion(-1f, 0f, 0f, 0f),
				new Quaternion(0f, -1f, 0f, 0f),
				new Quaternion(-1f, 0f, 0f, 0f),
				new Quaternion(0f, 0f, -1f, 0f),
				new Quaternion(0f, -1f, 0f, 0f),
				new Quaternion(0f, 0f, -1f, 0f),
				new Quaternion(1f, 0f, 0f, 0f)
			};
			int i = 0;
			for (int j = 0; j < 24; j += 3)
			{
				for (int p = 0; p < n; p++)
				{
					Quaternion edge_p = Quaternion.Lerp(init_vectors[j], init_vectors[j + 2], (float)p / (float)n);
					Quaternion edge_p2 = Quaternion.Lerp(init_vectors[j + 1], init_vectors[j + 2], (float)p / (float)n);
					Quaternion edge_p3 = Quaternion.Lerp(init_vectors[j], init_vectors[j + 2], (float)(p + 1) / (float)n);
					Quaternion edge_p4 = Quaternion.Lerp(init_vectors[j + 1], init_vectors[j + 2], (float)(p + 1) / (float)n);
					for (int q = 0; q < n - p; q++)
					{
						Quaternion a = Quaternion.Lerp(edge_p, edge_p2, (float)q / (float)(n - p));
						Quaternion b = Quaternion.Lerp(edge_p, edge_p2, (float)(q + 1) / (float)(n - p));
						Quaternion c;
						Quaternion d;
						if (edge_p3 == edge_p4)
						{
							c = edge_p3;
							d = edge_p3;
						}
						else
						{
							c = Quaternion.Lerp(edge_p3, edge_p4, (float)q / (float)(n - p - 1));
							d = Quaternion.Lerp(edge_p3, edge_p4, (float)(q + 1) / (float)(n - p - 1));
						}
						triangles[i] = i;
						vertices[i++] = new Vector3(a.x, a.y, a.z);
						triangles[i] = i;
						vertices[i++] = new Vector3(b.x, b.y, b.z);
						triangles[i] = i;
						vertices[i++] = new Vector3(c.x, c.y, c.z);
						if (q < n - p - 1)
						{
							triangles[i] = i;
							vertices[i++] = new Vector3(c.x, c.y, c.z);
							triangles[i] = i;
							vertices[i++] = new Vector3(b.x, b.y, b.z);
							triangles[i] = i;
							vertices[i++] = new Vector3(d.x, d.y, d.z);
						}
					}
				}
			}
			Mesh mesh = new Mesh();
			mesh.name = "IcoSphere";
			for (int k = 0; k < vertexNum; k++)
			{
				vertices[k] *= radius;
			}
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = uv;
			mesh.RecalculateNormals();
			return mesh;
		}

		// Token: 0x04002500 RID: 9472
		[Tooltip("(Optional) Use a mesh instead of collider(s) to apply imbue vfx and particles effects")]
		public Renderer imbueEffectRenderer;

		// Token: 0x04002501 RID: 9473
		[Tooltip("(Optional) Set a renderer to apply emission to the object of which it is imbued to. Note that emission must be white in the texture to ensure it gets the correct colours.")]
		public Renderer imbueEmissionRenderer;

		// Token: 0x04002502 RID: 9474
		[Tooltip("Set the spawn position/direction of spell-based projectiles (e.g. Flamethrower).\n\nBlue Arrow/Z Axis points forward.")]
		public Transform imbueShoot;

		// Token: 0x04002503 RID: 9475
		[Tooltip("This point is used to determine the velocity that the whoosh uses. This does not create the whoosh sound, that is used in the Whoosh component.\n\nIf left empty, it will create an object at runtime, located at the position of the colll")]
		public Transform whooshPoint;

		// Token: 0x04002504 RID: 9476
		[Tooltip("Custom imbue effect")]
		public FxController imbueCustomFxController;

		// Token: 0x04002505 RID: 9477
		[Tooltip("Allow a unique spell ID for custom imbue effect")]
		public string imbueCustomSpellID;

		// Token: 0x04002506 RID: 9478
		[Tooltip("List other collider groups here that you want this main collidergroup to share its imbue with.\n\nFor example, if a weapon is a double-sided axe, of which each blade is one different collider group, you can use this to make it so both colliders benefit from Imbue effects.")]
		public List<ColliderGroup> subImbueGroups;

		// Token: 0x04002507 RID: 9479
		[Tooltip("Allow the spell to play its imbue effect when imbued")]
		public bool allowImbueEffect = true;

		// Token: 0x04002508 RID: 9480
		[Tooltip("(Optional) Load this collider group data. Useful when you have a collidergroup with no item.")]
		public string colliderGroupId;

		// Token: 0x04002509 RID: 9481
		[NonSerialized]
		public ColliderGroup parentGroup;

		// Token: 0x0400250A RID: 9482
		[NonSerialized]
		public List<Collider> colliders;

		// Token: 0x0400250B RID: 9483
		[HideInInspector]
		[NonSerialized]
		public ColliderGroupData data;

		// Token: 0x0400250C RID: 9484
		[NonSerialized]
		public CollisionHandler collisionHandler;

		// Token: 0x0400250D RID: 9485
		[NonSerialized]
		public ColliderGroupData.Modifier modifier;

		// Token: 0x0400250E RID: 9486
		[NonSerialized]
		protected Imbue _imbue;

		// Token: 0x0400250F RID: 9487
		[NonSerialized]
		public bool isMetal;

		// Token: 0x04002510 RID: 9488
		public new string name;

		// Token: 0x04002511 RID: 9489
		public Texture cookie;
	}
}

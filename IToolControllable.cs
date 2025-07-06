using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200034C RID: 844
	public interface IToolControllable
	{
		// Token: 0x0600277A RID: 10106
		bool IsCopyable();

		// Token: 0x0600277B RID: 10107 RVA: 0x001109E8 File Offset: 0x0010EBE8
		void CopyControllableTo(UnityEngine.Object other)
		{
			GameObject gameObject2;
			if ((gameObject2 = (other as GameObject)) == null)
			{
				Component component = other as Component;
				gameObject2 = ((component != null) ? component.gameObject : null);
			}
			GameObject gameObject = gameObject2;
			if (gameObject == null)
			{
				Debug.LogError("Tried to copy a component to an object which isn't in the scene!");
				return;
			}
			if (!this.IsCopyable())
			{
				Debug.LogWarning("Tried to copy a component type that doesn't support copying!");
				return;
			}
			(gameObject.AddComponent(base.GetType()) as IToolControllable).CopyFrom(this);
		}

		// Token: 0x0600277C RID: 10108
		void CopyTo(UnityEngine.Object other);

		// Token: 0x0600277D RID: 10109
		void CopyFrom(IToolControllable original);

		// Token: 0x0600277E RID: 10110
		void Remove();

		// Token: 0x0600277F RID: 10111
		Transform GetTransform();

		// Token: 0x06002780 RID: 10112
		void ReparentAlign(Component other);

		// Token: 0x06002781 RID: 10113 RVA: 0x00110A51 File Offset: 0x0010EC51
		void ReparentAlignTransform(Component other)
		{
			Transform transform = this.GetTransform();
			transform.parent = other.transform;
			transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		}
	}
}

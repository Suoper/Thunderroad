using System;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000312 RID: 786
	public class BoundsConverter : PartialConverter<Bounds>
	{
		// Token: 0x06002547 RID: 9543 RVA: 0x000FF80C File Offset: 0x000FDA0C
		protected override void ReadValue(ref Bounds value, string name, JsonReader reader, JsonSerializer serializer)
		{
			if (name == "center")
			{
				reader.Read();
				value.center = serializer.Deserialize<Vector3>(reader);
				return;
			}
			if (name == "size")
			{
				reader.Read();
				value.size = serializer.Deserialize<Vector3>(reader);
				return;
			}
			if (name == "extents")
			{
				reader.Read();
				value.extents = serializer.Deserialize<Vector3>(reader);
				return;
			}
			if (name == "min")
			{
				reader.Read();
				value.min = serializer.Deserialize<Vector3>(reader);
				return;
			}
			if (!(name == "max"))
			{
				return;
			}
			reader.Read();
			value.max = serializer.Deserialize<Vector3>(reader);
		}

		// Token: 0x06002548 RID: 9544 RVA: 0x000FF8C8 File Offset: 0x000FDAC8
		protected override void WriteJsonProperties(JsonWriter writer, Bounds value, JsonSerializer serializer)
		{
			writer.WritePropertyName("center");
			serializer.Serialize(writer, value.center, typeof(Vector3));
			writer.WritePropertyName("size");
			serializer.Serialize(writer, value.size, typeof(Vector3));
		}
	}
}

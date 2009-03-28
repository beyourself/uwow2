using System;
using System.IO;
using Hazzik.Net;

namespace Hazzik.Objects {
	public abstract partial class WorldObject  {
		protected WorldObject(int maxValues) {
			Guid = ObjectGuid.NewGuid();
			Type = ObjectTypes.Object;
			ScaleX = 1f;
		}

		public abstract ObjectTypeId TypeId { get; }

		public virtual UpdateFlags UpdateFlag {
			get { return UpdateFlags.HighGuid | UpdateFlags.LowGuid; }
		}

		public virtual void WriteCreateBlock(BinaryWriter writer) {
		}

		public IPacket GetDestroyObjectPkt() {
			var result = new WorldPacket(WMSG.SMSG_DESTROY_OBJECT);
			var writer = result.CreateWriter();
			writer.Write(Guid);
			return result;
		}

		public void WriteCreateBlock(bool self, BinaryWriter writer) {
			writer.Write((byte)TypeId);
			writer.Write((byte)(!self ? UpdateFlag : UpdateFlag | UpdateFlags.Self));
			WriteCreateBlock(writer);
			if(UpdateFlag.Has(UpdateFlags.HighGuid)) {
				writer.Write((uint)0x00);
			}
			if(UpdateFlag.Has(UpdateFlags.LowGuid)) {
				writer.Write((uint)0x00);
			}
			if(UpdateFlag.Has(UpdateFlags.TargetGuid)) {
				writer.WritePackGuid(0x00);
			}
			if(UpdateFlag.Has(UpdateFlags.Transport)) {
				writer.Write((uint)0x00);
			}
		}
	}
}
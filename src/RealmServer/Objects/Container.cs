using System;

namespace Hazzik.Objects {
	public partial class Container : Item {
		public Container()
			: this((int)UpdateFields.CONTAINER_END) {
		}

		protected Container(int updateMaskLength)
			: base(updateMaskLength) {
			Type |= ObjectTypes.Container;
		}

		public override ObjectTypeId TypeId {
			get { return ObjectTypeId.Container; }
		}
	}
}
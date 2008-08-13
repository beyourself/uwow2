﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Hazzik.Net {
	public class AuthPacket : PacketBase, IPacket {

		internal AuthPacket(int code, byte[] data)
			: base(code, data) {
		}

		internal AuthPacket(int code)
			: base(code) {
		}
	}
}
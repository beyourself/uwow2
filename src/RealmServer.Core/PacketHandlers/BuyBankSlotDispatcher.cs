using System;
using Hazzik.Attributes;
using Hazzik.Net;

namespace Hazzik.PacketHandlers {
	[PacketHandlerClass(WMSG.CMSG_BUY_BANK_SLOT)]
	internal class BuyBankSlotDispatcher : IPacketDispatcher {
		#region IPacketDispatcher Members

		public void Dispatch(ISession session, IPacket packet) {
			ulong guid = packet.CreateReader().ReadUInt64();
			session.Player.BankBags.BuySlot();
		}

		#endregion
	}
}
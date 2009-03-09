using System;
using System.Collections.Generic;
using System.Reflection;
using Hazzik.Attributes;
using Hazzik.Net;

namespace Hazzik {
	public class PacketHandler<C, H>
		where C : PacketHandlerClassAttribute
		where H : PacketHandlerAttribute {
		private readonly IList<Assembly> _assemblies = new List<Assembly>();
		private readonly IDictionary<int, MethodInfo> _handlers = new Dictionary<int, MethodInfo>();

		public void AddAssembly(Assembly assembly) {
			_assemblies.Add(assembly);
		}

		public void Load() {
			foreach(var assembly in _assemblies) {
				foreach(var type in assembly.GetTypes()) {
					LoadType(type);
				}
			}
		}

		protected void LoadType(Type type) {
			var attribs = (C[])type.GetCustomAttributes(typeof(C), true);
			if(attribs.Length != 0) {
				foreach(var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
					var handlers = (H[])method.GetCustomAttributes(typeof(H), true);
					foreach(var handler in handlers) {
						RegisterPacketHandler(handler.Code, method);
					}
				}
			}
		}

		private void RegisterPacketHandler(int msg, MethodInfo method) {
			_handlers[msg] = method;
		}

		public void Unload() {
			foreach(var assembly in _assemblies) {
				foreach(var type in assembly.GetTypes()) {
					UnloadType(type);
				}
			}
		}

		protected void UnloadType(Type type) {
			var attribs = (C[])type.GetCustomAttributes(typeof(C), true);
			if(attribs.Length != 0) {
				foreach(var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
					var handlers = (H[])method.GetCustomAttributes(typeof(H), true);
					foreach(var handler in handlers) {
						UnregisterPacketHandler(handler.Code);
					}
				}
			}
		}

		public void UnregisterPacketHandler(int msg) {
			_handlers.Remove(msg);
		}

		public void Handle(ISession client, IPacket packet) {
			MethodInfo method;
			if(_handlers.TryGetValue(packet.Code, out method)) {
				method.Invoke(null, new object[] { client, packet });
			}
		}
	}
}
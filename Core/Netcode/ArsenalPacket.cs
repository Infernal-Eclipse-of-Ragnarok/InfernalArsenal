using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.Netcode
{
    internal abstract class ArsenalPacket : ILoadable
    {
        public abstract void HandlePacket(BinaryReader packet, int sender);

        private ushort _NetID;
        private PropertyInfo _Prop_Static_Instance;

        public void Load(Mod mod)
        {
            _NetID = ArsenalNetcode.RegisterHandler(this);

            var type = GetType();
            var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

            if (instanceProperty == null)
                return;

            if (!instanceProperty.PropertyType.IsAssignableFrom(type))
                InfernalEclipseWeaponsDLC.Instance.Logger.Error($"Packet instance's 'Instance' property is not asssignable with given type! [Failed On: '{type.FullName}']");

            instanceProperty.SetValue(null, this);
            _Prop_Static_Instance = instanceProperty; // We saving this for Unload Steps
        }

        public virtual void Unload()
        {
            _Prop_Static_Instance?.SetValue(null, null);
            _Prop_Static_Instance = null;
        }

        public void CloneAndBroadcast(BinaryReader packet, long startIndex, int length, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            if (startIndex < 0)
                return;

            packet.BaseStream.Position = startIndex;

            // Limit stackalloc size to 256 bytes
            Span<byte> buffer = length <= 256 ? stackalloc byte[length] : new byte[length];
            packet.BaseStream.Read(buffer);

            var newPacket = CreateBasePacket();
            newPacket.Write(buffer);
            newPacket.Send(ignoreClient);
        }

        public ModPacket CreateBasePacket()
        {
            var packet = InfernalEclipseWeaponsDLC.Instance.GetPacket();
            ArsenalNetcode.WriteHandlerNetID(packet, _NetID);
            return packet;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.Netcode
{
    public class ArsenalNetcode : ModSystem
    {
        private static List<ArsenalPacket> _PacketHandlers = [];

        internal static ushort RegisterHandler(ArsenalPacket handler)
        {
            var id = (ushort)_PacketHandlers.Count;
            _PacketHandlers.Add(handler);
            return id;
        }

        internal static void WriteHandlerNetID(BinaryWriter packet, ushort netID)
        {
            if (_PacketHandlers.Count > 256)
                packet.Write(netID);
            else
                packet.Write((byte)netID);
        }

        internal static ushort ReadHandlerNetID(BinaryReader packet)
        {
            if (_PacketHandlers.Count > 256)
                return packet.ReadUInt16();
            else
                return packet.ReadByte();
        }

        public override void OnModUnload()
        {
            _PacketHandlers = null;
        }

        public static void HandlePacket(Mod mod, BinaryReader reader, int whoAmI)
        {
            try
            {
                var netID = ReadHandlerNetID(reader);
                var packetHandler = _PacketHandlers[netID];
                if (packetHandler is not null)
                {
                    packetHandler.HandlePacket(reader, whoAmI);
                }
                else
                {
                    //
                    // Default case: with no idea how long the packet is, we can't safely read data.
                    // Throw an exception now instead of allowing the network stream to corrupt.
                    //

                    InfernalEclipseWeaponsDLC.Instance.Logger.Error($"Failed to parse WeaponsDLC packet: No WeaponsDLC packet exists with ID {netID}.");
                    throw new Exception("Failed to parse WeaponsDLC packet: Invalid WeaponsDLC packet ID.");
                }
            }
            catch (Exception e)
            {
                if (e is EndOfStreamException eose)
                    InfernalEclipseWeaponsDLC.Instance.Logger.Error("Failed to parse WeaponsDLC packet: Packet was too short, missing data, or otherwise corrupt.", eose);
                else if (e is ObjectDisposedException ode)
                    InfernalEclipseWeaponsDLC.Instance.Logger.Error("Failed to parse WeaponsDLC packet: Packet reader disposed or destroyed.", ode);
                else if (e is IOException ioe)
                    InfernalEclipseWeaponsDLC.Instance.Logger.Error("Failed to parse WeaponsDLC packet: An unknown I/O error occurred.", ioe);
                else
                    throw; // this either will crash the game or be caught by TML's packet policing
            }
        }
    }
}

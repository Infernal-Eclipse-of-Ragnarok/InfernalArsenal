using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;

namespace InfernalEclipseWeaponsDLC.Core.Netcode
{
    internal sealed class MousePositionSyncPacket : ArsenalPacket
    {
        public static MousePositionSyncPacket Instance { get; private set; }

        public static void Send(InfernalWeaponsPlayer player, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var packet = Instance.CreateBasePacket();

            byte whoAmI = (byte)(player?.Player.whoAmI ?? Main.maxPlayers);
            packet.Write(whoAmI);

            packet.Write((short)player.mouseWorldDeltaFromPlayer.X);
            packet.Write((short)player.mouseWorldDeltaFromPlayer.Y);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            Player readPlayer = null;

            int index = packet.ReadByte();

            if (index <= Main.maxPlayers)
            {
                readPlayer = Main.player[index];

                if (IsNullOrInactive(readPlayer))
                    readPlayer = null;
            }

            var player = readPlayer?.GetModPlayer<InfernalWeaponsPlayer>();
            var deltaX = packet.ReadInt16();
            var deltaY = packet.ReadInt16();

            if (player is null)
                return;

            var delta = new Vector2(deltaX, deltaY);
            player.mouseWorldDeltaFromPlayer = delta;
            player.mouseRotationFromPlayer = delta.ToRotation();

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }

        private static bool IsNullOrInactive(Entity entity)
        {
            if (entity is null) return true;
            if (!entity.active) return true;

            return false;
        }
    }

    internal sealed class MouseRotationSyncPacket : ArsenalPacket
    {
        public static MouseRotationSyncPacket Instance { get; private set; }

        public static void Send(InfernalWeaponsPlayer player, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var packet = Instance.CreateBasePacket();

            byte whoAmI = (byte)(player?.Player.whoAmI ?? Main.maxPlayers);
            packet.Write(whoAmI);

            packet.Write((Half)player.mouseRotationFromPlayer);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            Player readPlayer = null;

            int index = packet.ReadByte();

            if (index <= Main.maxPlayers)
            {
                readPlayer = Main.player[index];

                if (IsNullOrInactive(readPlayer))
                    readPlayer = null;
            }

            var player = readPlayer?.GetModPlayer<InfernalWeaponsPlayer>();
            var rotation = (float)packet.ReadHalf();

            if (player is null)
                return;

            player.mouseRotationFromPlayer = rotation;
            player.mouseWorldDeltaFromPlayer = rotation.ToRotationVector2();

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }

        private static bool IsNullOrInactive(Entity entity)
        {
            if (entity is null) return true;
            if (!entity.active) return true;

            return false;
        }
    }
}

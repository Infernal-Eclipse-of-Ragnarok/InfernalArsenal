using Microsoft.Xna.Framework;
using SOTS.Helpers;
using System;
using Terraria;
using Terraria.ID;

namespace InfernalEclipseWeaponsDLC.Utilities
{
    public static class PlayerExtensions
    {
        public static bool HasAltFunctionUse(this Player player)
        {
            return player.altFunctionUse == ItemAlternativeFunctionID.ActivatedAndUsed;
        }

        public static bool HasMaxSentries(this Player player)
        {
            int sentryCount = 0;

            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.owner == player.whoAmI && projectile.sentry)
                    sentryCount++;
            }

            return sentryCount >= player.maxTurrets;
        }

        public static bool IsOnStandableGround(this Entity entity, float yOffset = 0.0f, bool onlySolid = false)
        {
            return IsOnStandableGround(entity.BottomLeft.X, entity.BottomLeft.Y + yOffset, entity.width, onlySolid);
        }

        public static bool IsOnStandableGround(in float startX, float y, int width, bool onlySolid = false)
        {
            if (width <= 0)
                throw new ArgumentException("width cannot be negative");
            float num = startX;
            do
            {
                Point tileCoordinates = Utils.ToTileCoordinates(new Vector2(num, y + 0.01f));
                if (onlySolid && SolidTile(tileCoordinates.X, tileCoordinates.Y) || SolidOrSolidTopTile(tileCoordinates.X, tileCoordinates.Y))
                    return true;
                num += 16f;
            }
            while ((double)num < (double)startX + (double)width);
            return false;
        }

        public static bool SolidTile(int i, int j) => WorldGen.InWorld(i, j, 0) && SolidTile((Main.tile)[i, j]);

        public static bool SolidTile(Tile t) => t.HasTile && !t.IsActuated && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType];

        public static bool SolidOrSolidTopTile(int i, int j) => WorldGen.InWorld(i, j, 0) && SolidOrSolidTopTile(Main.tile[i, j]);


        public static bool SolidOrSolidTopTile(Tile t)
        {
            if (!t.HasTile || t.IsActuated)
                return false;
            return Main.tileSolid[t.TileType] || Main.tileSolidTop[t.TileType];
        }
    }
}

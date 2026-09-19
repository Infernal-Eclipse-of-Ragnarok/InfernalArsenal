using CalamityMod;
using CalamityMod.DataStructures;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Utilities
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    internal class CalamityHelper
    {
        public static Color DebuffTooltipColor(int buffId) => CalamityUtils.GetDebuffTooltipNameColor(buffId);

        public static void DrawAfterimagesCentered(Projectile proj, int mode, Color lightColor, int typeOneIncrement = 1, Texture2D texture = null, bool drawCentered = true, bool shrink = false, int armorShaderToUse = 0) 
            => CalamityUtils.DrawAfterimagesCentered(proj, mode, lightColor, typeOneIncrement, texture, drawCentered, shrink, armorShaderToUse);

        public static void SetDebuffData(int buffId, DebuffData debuffData) => CalamityBuffSets.DebuffDataset[buffId] = debuffData;

        public static bool IsMaceFlail(Projectile proj) => proj.ModProjectile is BaseMaceFlailProjectile;
    }
}

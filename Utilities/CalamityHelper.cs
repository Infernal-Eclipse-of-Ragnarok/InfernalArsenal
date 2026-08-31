using CalamityMod;
using CalamityMod.DataStructures;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Utilities
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    internal class CalamityHelper
    {
        public static Color DebuffTooltipColor(int buffId) => CalamityUtils.GetDebuffTooltipNameColor(buffId);

        public static void SetDebuffData(int buffId, DebuffData debuffData) => CalamityBuffSets.DebuffDataset[buffId] = debuffData;

        public static bool IsMaceFlail(Projectile proj) => proj.ModProjectile is BaseMaceFlailProjectile;
    }
}

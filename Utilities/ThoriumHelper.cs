using Terraria.ModLoader;
using ThoriumMod;

namespace InfernalEclipseWeaponsDLC.Utilities
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    internal class ThoriumHelper
    {
        public static bool IsStrangeSkullToggled() => ThoriumConfigServer.Instance.donatorWeapons.toggleStrangeSkull;
    }
}

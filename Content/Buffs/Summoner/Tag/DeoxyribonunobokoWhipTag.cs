using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Tag
{
    public class DeoxyribonunobokoWhipTagDefault : ModBuff
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Assets/Textures/Empty";
        public const int TagDamage = 12;
        public const int CritChance = 15;

        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }

    public class DeoxyribonunobokoWhipTagStrong : ModBuff
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Assets/Textures/Empty";
        public const int TagDamage = 20;
        public const int CritChance = 15;

        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}

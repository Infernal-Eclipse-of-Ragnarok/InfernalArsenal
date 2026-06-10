using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Buffs
{
    public class PerfectFlailBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.15f;
            //player.GetCritChance(DamageClass.Generic) += 15;
            //player.statDefense += 20;
            player.endurance += 0.15f;
            player.moveSpeed += 0.15f;
        }
    }
}
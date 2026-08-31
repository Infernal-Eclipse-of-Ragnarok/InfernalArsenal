using SOTS.Common.GlobalNPCs;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.Players
{
    [JITWhenModsEnabled("SOTS")]
    [ExtendsFromMod("SOTS")]
    public class SOTSCurseVoidEffects : ModPlayer
    {
        public bool ancientShieldEquipped;

        public override void ResetEffects()
        {
            ancientShieldEquipped = false;
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (ancientShieldEquipped)
            {
                DebuffNPC debuffNPC = npc.GetGlobalNPC<DebuffNPC>();

                float critChance = Player.GetTotalCritChance(DamageClass.Generic);
                float baseChance = 0.05f + critChance * 0.01f;
                int baseStacks = 1;
                if (Main.rand.NextFloat(1) < baseChance / (baseStacks + debuffNPC.BleedingCurse * 0.8f))
                {
                    debuffNPC.StackDebuff(npc, Player, ref debuffNPC.BleedingCurse, 1, 0);
                }
            }
        }
    }
}

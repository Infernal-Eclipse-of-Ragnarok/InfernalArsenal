using CalamityMod.Systems.Collections;
using InfernalEclipseWeaponsDLC.Content.Buffs;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.GlobalNPCs
{
    public class WeaponsGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool limbBurn;

        public override void ResetEffects(NPC npc)
        {
            limbBurn = false;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (item.type != ModContent.ItemType<Stick>())
                return;

            if (!npc.boss)
                return;

            if (BossHasNoContactDamage(npc))
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }

        public override void PostAI(NPC npc)
        {
            if (!(CalamityNPCSets.ImmuneToSlowsAndOtherSpecialEffects[npc.type] || npc.boss))
            {
                float velocitySlownessFactor = 1f;

                if (limbBurn)
                    velocitySlownessFactor += 0.075f;

                velocitySlownessFactor = 1f / velocitySlownessFactor;
                npc.velocity *= velocitySlownessFactor;
            }
        }

        private static bool BossHasNoContactDamage(NPC npc)
        {
            if (npc.damage <= 0)
                return true;

            return false;
        }
    }
}

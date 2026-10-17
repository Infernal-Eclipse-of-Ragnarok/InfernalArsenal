using InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Tag;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using InfernalEclipseWeaponsDLC.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
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

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.boss)
            {
                foreach (Player player in Main.ActivePlayers)
                {
                    player.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored = 0;
                }
            }
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

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (ModLoader.HasMod("CalamityMod"))
                return;

            if (projectile.npcProj || projectile.trap || !projectile.IsMinionOrSentryRelated)
                return;

            // SummonTagDamageMultiplier scales down tag damage for some specific minion and sentry projectiles for balance purposes.
            var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];

            if (npc.HasBuff<GrandThunderWhipTag>())
            {
                modifiers.FlatBonusDamage += 3 * projTagMultiplier;
            }

            if (npc.HasBuff<DeoxyribonunobokoWhipTagDefault>())
            {
                modifiers.FlatBonusDamage += DeoxyribonunobokoWhipTagDefault.TagDamage * projTagMultiplier;

                if (Main.rand.Next(0, 101) < DeoxyribonunobokoWhipTagDefault.CritChance)
                    modifiers.SetCrit();
            }

            if (npc.HasBuff<DeoxyribonunobokoWhipTagStrong>())
            {
                modifiers.FlatBonusDamage += DeoxyribonunobokoWhipTagStrong.TagDamage * projTagMultiplier;

                if (Main.rand.Next(0, 101) < DeoxyribonunobokoWhipTagStrong.CritChance)
                    modifiers.SetCrit();
            }
        }

        public override void PostAI(NPC npc)
        {
            bool calImmuneToSlowsAndOtherSpecialEffects = false;

            if (ModLoader.HasMod("CalamityMod"))
                calImmuneToSlowsAndOtherSpecialEffects = CalamityHelper.IsImmuneToSlowsAndOtherSpecialEffects(npc);

            if (!(calImmuneToSlowsAndOtherSpecialEffects || npc.boss))
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

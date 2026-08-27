using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using CalamityMod.Projectiles.Typeless;
using InfernalEclipseWeaponsDLC.Content.Items.Accessories.Donor;
using InfernalEclipseWeaponsDLC.Content.Items.Accessories.LifeShields;
using InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee;
using InfernalEclipseWeaponsDLC.Core.Cooldowns;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using InfernalEclipseWeaponsDLC.Core.Players.Dashes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.Players
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class WeaponsPlayerCalamity : ModPlayer
    {
        public override void ResetEffects()
        {
            InfernalWeaponsPlayer mp = Player.GetModPlayer<InfernalWeaponsPlayer>();

            if (!mp.imagiknightHeraldry && mp.heraldyBuffFromOther <= 0f)
                Player.Calamity().cooldowns.Remove(ImagiknightHeraldryBuff.ID);

            if (!mp.hasWarbanner)
                Player.Calamity().cooldowns.Remove(WarbanneroftheRighteousBuff.ID);

            mp.imagiknightHeraldry = false;
            mp.hasWarbanner = false;

            Player.Calamity().warbannerDamageMult = 0f;
            mp.heraldryDamageMult = 0f;
            mp.heraldyBuffFromOther = 0f;
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            InfernalWeaponsPlayer mp = Player.GetModPlayer<InfernalWeaponsPlayer>();

            if (mp.blightedBadge && !npc.dontTakeDamage)
            {
                int onHitDamage = (int)Player.GetBestClassDamage().ApplyTo(BlightedBadge.ThornsDamage);

                Projectile bolt = Projectile.NewProjectileDirect(Player.GetSource_OnHurt(hurtInfo.DamageSource), npc.Center, Vector2.Zero, ModContent.ProjectileType<FlashBolt>(), onHitDamage, 0f, Player.whoAmI, npc.whoAmI);
                bolt.DamageType = Player.GetBestClass();
            }
        }

        public override void UpdateEquips()
        {
            InfernalWeaponsPlayer mp = Player.GetModPlayer<InfernalWeaponsPlayer>();
            CalamityPlayer modPlayer = Player.Calamity();

            if (mp.imagiknightHeraldry)
            {
                modPlayer.WarbanneroftheRighteous = true;

                int maxValue = (int)(ImagiknightHeraldry.MaxBonus * 100);
                float bonus = ImagiknightHeraldry.CalculateBonus(Player);
                float displayBonus = bonus * 100f; // Should range from 0 to the maxValue

                if (modPlayer.cooldowns.TryGetValue(ImagiknightHeraldryBuff.ID, out var cooldown))
                    cooldown.timeLeft = maxValue - (int)displayBonus;
                else
                    Player.AddCooldown(ImagiknightHeraldryBuff.ID, maxValue);

                mp.heraldryDamageMult = bonus;

                modPlayer.warbannerDamageMult = Math.Max(modPlayer.warbannerDamageMult, mp.heraldryDamageMult);
            }
            else
            {
                float bestBonus = 0f;
                Player bestHeraldryPlayer = null;

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player other = Main.player[i];

                    if (!other.active || other.dead || other.whoAmI == Player.whoAmI)
                        continue;

                    if (other.team == 0 || other.team != Player.team)
                        continue;

                    InfernalWeaponsPlayer otherWeaponsPlayer = other.GetModPlayer<InfernalWeaponsPlayer>();

                    if (!otherWeaponsPlayer.imagiknightHeraldry)
                        continue;

                    float bonus = ImagiknightHeraldry.CalculateBonusFromDistance(other, Player);

                    if (bonus > bestBonus)
                    {
                        bestBonus = bonus;
                        bestHeraldryPlayer = other;
                    }
                }

                if (bestBonus > 0f)
                {
                    modPlayer.WarbanneroftheRighteous = true;
                    mp.heraldyBuffFromOther = bestBonus;

                    if (bestHeraldryPlayer != null && !bestHeraldryPlayer.GetModPlayer<InfernalWeaponsPlayer>().hideHeraldryVisual)
                        modPlayer.warbannerGlow = true;

                    if (Player.ownedProjectileCounts[ModContent.ProjectileType<WarbannerLight>()] < 1 && !bestHeraldryPlayer.GetModPlayer<InfernalWeaponsPlayer>().hideHeraldryVisual && !Player.dead)
                    {
                        Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<WarbannerLight>(), 0, 0f, Player.whoAmI);
                    }

                    int maxValue = (int)(ImagiknightHeraldry.MaxBonus * 100);
                    int displayBonus = (int)(bestBonus * 100f);

                    if (modPlayer.cooldowns.TryGetValue(ImagiknightHeraldryBuff.ID, out var cooldown))
                        cooldown.timeLeft = maxValue - displayBonus;
                    else
                        Player.AddCooldown(ImagiknightHeraldryBuff.ID, maxValue);

                    modPlayer.warbannerDamageMult = Math.Max(modPlayer.warbannerDamageMult, bestBonus);
                }
            }
        }

        public override void PostUpdateEquips()
        {
            InfernalWeaponsPlayer mp = Player.GetModPlayer<InfernalWeaponsPlayer>();
            CalamityPlayer calamityPlayer = Player.Calamity();

            if (mp.perennialShield)
            {
                if (calamityPlayer.reaverSpeed)
                {
                    Player.moveSpeed += 0.1f;
                    calamityPlayer.DashID = PerennialShieldDash.ID;
                    Player.dashType = 0;
                }
                else if (calamityPlayer.reaverDefense)
                {
                    Player.endurance += 0.1f;
                    Player.statLifeMax2 += 15;
                }
                else if (calamityPlayer.reaverExplore)
                {
                    Player.jumpSpeedBoost += 1f;
                    Player.noFallDmg = true;
                }
            }
        }
    }
}

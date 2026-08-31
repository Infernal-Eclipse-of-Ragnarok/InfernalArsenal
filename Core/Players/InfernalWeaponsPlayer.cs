using InfernalEclipseWeaponsDLC.Content.Buffs;
using InfernalEclipseWeaponsDLC.Content.Items.Materials;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee.Void;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using InfernalEclipseWeaponsDLC.Content.Projectiles.FlailPro;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.Void;
using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs;
using ThoriumMod.Projectiles;
using ThoriumMod.Utilities;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace InfernalEclipseWeaponsDLC.Core.NewFolder
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class InfernalWeaponsPlayer : ModPlayer
    {
        private Mod calamity => ModIntegrationsSystem.Calamity.Loaded? ModIntegrationsSystem.Calamity.Mod : null;

        const int shard2chance = 20;

        public bool spearSearing;
        public bool spearArctic;
        public bool minionCrits;
        public bool godsPitch;
        public bool blightedBadge;
        public bool imagiknightHeraldry;
        public bool doubleFlailAcc;
        public bool perfectPurple;
        public bool blackholeFlail;
        public bool holyFlail;
        public bool perennialShield;
        public bool scourgeBag;
        public bool scourgeBag2;

        public bool hideHeraldryVisual;
        public bool hasWarbanner;

        public int missileIndex = 10;
        public int CataclysmFistShotCount = 0;
        public int annihilationBonusShotTimeLeft = 0;
        public int annihilationBonusShotCooldown = 0;

        public float heraldryDamageMult = 0f;
        public float heraldyBuffFromOther = 0f;

        private static readonly HashSet<int> ManualFlails = new();

        public override void ResetEffects()
        {
            spearSearing = false;
            spearArctic = false;
            minionCrits = false;
            godsPitch = false;
            blightedBadge = false;
            doubleFlailAcc = false;
            perfectPurple = false;
            blackholeFlail = false;
            holyFlail = false;
            perennialShield = false;

            if (annihilationBonusShotTimeLeft > 0)
                annihilationBonusShotTimeLeft--;

            if (annihilationBonusShotCooldown > 0)
                annihilationBonusShotCooldown--;
        }

        public override void Load()
        {
            ManualFlails.Clear();

            AddManualFlailProjectile("CalamityMod", "ClamCrusherFlail");
            AddManualFlailProjectile("CalamityMod", "CrescentMoonFlail");
            AddManualFlailProjectile("CalamityMod", "DragonPowFlail");
            AddManualFlailProjectile("CalamityMod", "PulseDragonProjectile");

            AddManualFlailProjectile("Clamity", "ClamitasCrusherProjectile");

            AddManualFlailProjectile("SOTS", "Shattershine");
            AddManualFlailProjectile("SOTS", "AtenProj");
            AddManualFlailProjectile("SOTS", "NorthStar");

            ManualFlails.Add(ProjectileID.Flairon);
        }

        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            bool isSulfurCatch = calamity != null ? Player.InModBiome(calamity.Find<ModBiome>("SulphurousSeaBiome")) : Player.ZoneBeach;
            bool inWater = !attempt.inLava && !attempt.inHoney;

            if (!isSulfurCatch || !inWater) return;

            bool goodEnoughLevel = attempt.fishingLevel >= 45;
            bool randomChanceSuccess = Main.rand.NextBool(shard2chance);

            if (!randomChanceSuccess || !goodEnoughLevel) return;

            itemDrop = ModContent.ItemType<DeepSeaDrawlShard2>();
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            object result = ModLoader.GetMod("ThoriumMod").Call("IsBardProjectile", proj);

            bool concus = false;

            if (ModLoader.TryGetMod("ThoriumRework", out Mod helheim))
            {
                if (proj.ModProjectile != null && proj.ModProjectile.Mod.Name == "ThoriumRework" && proj.ModProjectile.Name == "ConcussiveInstrument")
                {
                    concus = true;
                }
            }
            else
            {
                concus = false;
            }

            if (result is ValueTuple<bool, byte> valueTuple && valueTuple.Item1)
            {
                if (godsPitch && !concus)
                {
                    int metalPipe = ModContent.ProjectileType<MetalPipe>();
                    if (metalPipe != proj.type && Main.myPlayer == Player.whoAmI)
                    {
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.NewProjectile(proj.GetSource_OnHit(target), target.Center, new Vector2(target.position.X - target.oldPosition.X, -16f), metalPipe, (hit.Damage + damageDone) / 3, proj.knockBack, proj.owner, target.whoAmI, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 0, 0, 0);
                    }
                }
            }

            //Flail chance procs
            bool isFlail = proj.ModProjectile is FlailProBase || proj.aiStyle == ProjAIStyleID.Flail || ManualFlails.Contains(proj.type);

            bool isCalMace = false;
            if (ModIntegrationsSystem.Calamity.Loaded)
            {
                isCalMace = CalamityHelper.IsMaceFlail(proj);
            }

            if (isFlail || isCalMace)
            {
                Vector2 vector = proj.velocity * 0.5f;

                if (vector == Vector2.Zero)
                {
                    vector = Main.MouseWorld - Player.Center;
                    vector.Normalize();
                    vector *= 6f;
                }

                if (doubleFlailAcc && Utils.NextBool(Main.rand, 8))
                {
                    SoundEngine.PlaySound(SoundID.Item1, proj.Center);
                    Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, vector, ModContent.ProjectileType<HotFlailCorePro>(), (int)(proj.damage * 0.75f), proj.knockBack, proj.owner);
                }
                if (doubleFlailAcc && Utils.NextBool(Main.rand, 8))
                {
                    SoundEngine.PlaySound(SoundID.Item1, proj.Center);
                    Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, vector, ModContent.ProjectileType<ColdFlailCorePro>(), (int)(proj.damage * 0.75f), proj.knockBack, proj.owner);
                }
                if (perfectPurple && Utils.NextBool(Main.rand, 4))
                {
                    SoundEngine.PlaySound(SoundID.Item1, proj.Center);
                    Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, vector, ModContent.ProjectileType<PerfectFlailCorePro>(), (int)(proj.damage * 1f), proj.knockBack, proj.owner);
                }
                if (blackholeFlail && Utils.NextBool(Main.rand, 4))
                {
                    SoundEngine.PlaySound(SoundID.Item1, proj.Center);
                    Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, vector, ModContent.ProjectileType<BlackHoleFlailCorePro>(), (int)(proj.damage * 3f), proj.knockBack, proj.owner);
                }
                if (holyFlail && Utils.NextBool(Main.rand, 4))
                {
                    SoundEngine.PlaySound(SoundID.Item1, proj.Center);
                    Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, vector, ModContent.ProjectileType<HolyFlailCorePro>(), (int)(proj.damage * 1), proj.knockBack, proj.owner);
                }

                //Guaranteed procs
                if (doubleFlailAcc)
                {
                    SoundEngine.PlaySound(SoundID.Item1, proj.Center);

                    int projectileType = Main.rand.NextBool() ? ModContent.ProjectileType<HotFlailCorePro>() : ModContent.ProjectileType<ColdFlailCorePro>();

                    Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, vector, projectileType, (int)(proj.damage * 0.75f), proj.knockBack, proj.owner);
                }
                if (perfectPurple)
                {
                    SoundEngine.PlaySound(SoundID.Item1, proj.Center);
                    Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, vector, ModContent.ProjectileType<PerfectFlailCorePro>(), (int)(proj.damage * 1f), proj.knockBack, proj.owner);
                }
                if (holyFlail)
                {
                    SoundEngine.PlaySound(SoundID.Item1, proj.Center);
                    Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, vector, ModContent.ProjectileType<HolyFlailCorePro>(), (int)(proj.damage * 1), proj.knockBack, proj.owner);
                }
            }

            if (proj.aiStyle == ProjAIStyleID.Yoyo)
            {
                if (scourgeBag && ModIntegrationsSystem.Calamity.Loaded)
                {
                    target.AddBuff(ModIntegrationsSystem.Calamity.Mod.Find<ModBuff>("HolyFlames").Type, 180);
                    if (Utils.NextBool(Main.rand, 5))
                    {
                        target.AddBuff(ModContent.BuffType<LimbBurn>(), 120);
                        for (int m = 0; m < 8; m++)
                        {
                            int num5 = Dust.NewDust(target.position, target.width, target.height, DustID.Firework_Yellow, Main.rand.Next(-3, 3), Main.rand.Next(-3, 3), 255, new Color(255, 165, 255), 1.5f);
                            Main.dust[num5].noGravity = true;
                        }
                    }
                    if (!scourgeBag2 && !Player.HasBuff(ModContent.BuffType<YoyoProtectionDebuff>()))
                    {
                        scourgeBag2 = true;
                        for (int n = 0; n < 15; n++)
                        {
                            int num6 = Dust.NewDust(Player.position, 20, 20, DustID.Firework_Yellow, 0f, 0f, 255, new Color(255, 255, 0), 1.35f);
                            Main.dust[num6].noGravity = true;
                            Main.dust[num6].velocity = new Vector2(0.75f, 0f);
                            int num7 = Main.rand.Next(-50, 51);
                            int num8 = Main.rand.Next(-50, 51);
                            Dust dust3 = Main.dust[num6];
                            dust3.position.X = dust3.position.X + num7;
                            Dust dust4 = Main.dust[num6];
                            dust4.position.Y = dust4.position.Y + num8;
                            Main.dust[num6].velocity.X = -(float)num7 * 0.075f;
                            Main.dust[num6].velocity.Y = -(float)num8 * 0.075f;
                        }
                    }
                }
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.hostile)
                return;

            if (minionCrits && IsSummonDamage(proj))
            {
                if (Main.rand.Next(100) < ActualClassCrit(Player, DamageClass.Summon))
                    modifiers.SetCrit();
            }
        }

        public override void PostUpdateEquips()
        {
            ThoriumPlayer thoriumPlayer = Player.GetThoriumPlayer();

            if (scourgeBag2)
            {
               Player.AddBuff(ModContent.BuffType<YoyoProtectionBuff>(), 2);
               thoriumPlayer.thoriumEndurance += 0.1f;
            }
        }

        public override void PostUpdateMiscEffects()
        {
            if (ModLoader.HasMod("SOTS"))
            {
                if (Player.controlUseItem && Player.HeldItem.type == Mod.Find<ModItem>("GauntletofAnnihilationVoid").Type)
                {
                    if (annihilationBonusShotTimeLeft > 0 && annihilationBonusShotCooldown == 0)
                    {
                        CombatText.NewText(Player.Hitbox, Color.Lerp(Color.Cyan, Color.Magenta, 0.5f), Main.rand.NextBool() ? "It's not over yet!" : "Did that hurt?", true);
                        SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/DoGFireball"), new Vector2?(Player.position));
                        Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Player.velocity, ModContent.ProjectileType<CosmicPunch>(), Player.HeldItem.damage * 20, Player.HeldItem.knockBack, Player.whoAmI, ai1: 6, ai2: 10);
                        annihilationBonusShotTimeLeft = 0;
                        annihilationBonusShotCooldown = 120;
                    }
                }
            }
            else
            {
                if (Player.controlUseItem && Player.HeldItem.type == ModContent.ItemType<GauntletofAnnihilation>())
                {
                    if (annihilationBonusShotTimeLeft > 0 && annihilationBonusShotCooldown == 0)
                    {
                        CombatText.NewText(Player.Hitbox, Color.Lerp(Color.Cyan, Color.Magenta, 0.5f), Main.rand.NextBool() ? "It's not over yet!" : "Did that hurt?", true);
                        SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/DoGFireball"), new Vector2?(Player.position));
                        Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Player.velocity, ModContent.ProjectileType<CosmicPunch>(), Player.HeldItem.damage * 15, Player.HeldItem.knockBack, Player.whoAmI, ai1: 6, ai2: 10);
                        annihilationBonusShotTimeLeft = 0;
                        annihilationBonusShotCooldown = 120;
                    }
                }
            }

            MiscEffects();
        }

        private void MiscEffects()
        {
            if (ModLoader.HasMod("SOTS"))
            {
                
                if (Player.HeldItem.type == Mod.Find<ModItem>("CataclysmicGauntletVoid").Type) //we have to do it this way since the item doesn't load without SOTS.
                    SupremeCataclysmFist.GenerateDustOnOwnerHand(Player);

                if (Player.HeldItem.type == Mod.Find<ModItem>("GauntletofAnnihilationVoid").Type)
                    GauntletofAnnihilationPunches.GenerateDustOnOwnerHand(Player);
            }
            else
            {
                if (Player.HeldItem.type == ModContent.ItemType<CataclysmicGauntlet>())
                {
                    SupremeCataclysmFist.GenerateDustOnOwnerHand(Player);
                }
            }
        }

        // thank you fargos
        public static bool IsSummonDamage(Projectile projectile, bool includeMinionShot = true, bool includeWhips = true)
        {
            if (!includeWhips && ProjectileID.Sets.IsAWhip[projectile.type])
                return false;

            if (!includeMinionShot && (ProjectileID.Sets.MinionShot[projectile.type] || ProjectileID.Sets.SentryShot[projectile.type]))
                return false;

            return projectile.CountsAsClass(DamageClass.Summon) || projectile.minion || projectile.sentry || projectile.minionSlots > 0 || ProjectileID.Sets.MinionSacrificable[projectile.type]
                || (includeMinionShot && (ProjectileID.Sets.MinionShot[projectile.type] || ProjectileID.Sets.SentryShot[projectile.type]))
                || (includeWhips && ProjectileID.Sets.IsAWhip[projectile.type]);
        }

        public float ActualClassCrit(Player player, DamageClass damageClass)
            => (damageClass == DamageClass.Summon || damageClass == DamageClass.SummonMeleeSpeed) && !(minionCrits)
            ? 0
            : player.GetTotalCritChance(damageClass);

        private static void AddManualFlailProjectile(string modName, string projectileName)
        {
            if (!ModLoader.TryGetMod(modName, out Mod mod))
                return;

            if (mod.TryFind(projectileName, out ModProjectile projectile))
            {
                ManualFlails.Add(projectile.Type);
            }
        }
    }
}

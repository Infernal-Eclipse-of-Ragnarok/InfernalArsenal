using InfernalEclipseWeaponsDLC.Content.Buffs;
using InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Tag;
using InfernalEclipseWeaponsDLC.Content.Items.Materials;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee.Void;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using InfernalEclipseWeaponsDLC.Content.Projectiles.FlailPro;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.Void;
using InfernalEclipseWeaponsDLC.Core.Netcode;
using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs;
using ThoriumMod.Items.BardItems;
using ThoriumMod.Items.Donate;
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
        public bool sentryCrits;
        public bool seersGlass;
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
        public bool gorgonIncarnon = false;
        public bool marniteHoverboots;
        public bool marniteHoverbootsCanHover;
        public bool marniteHoverbootsJumped;
        public bool hideHeraldryVisual;
        public bool hasWarbanner;

        public float heraldryDamageMult = 0f;
        public float heraldyBuffFromOther = 0f;
        public int marniteHoverbootsHoverTimer;
        public int marniteHoverbootsJumpTimer;

        public int missileIndex = 0;
        public int CataclysmFistShotCount = 0;
        public int annihilationBonusShotTimeLeft = 0;
        public int annihilationBonusShotCooldown = 0;
        public int murasamaHitCooldown = 0;
        public int gorgonShotsHit;
        public int gorgonIncarnonShotsStored;
        public int hollowbrookTier;
        public Vector2? hollowbrookLocation;
        public int hollowbrookCrits;
        public int hollowbrookSpawnCooldown = 0;
        public int nukeWhistleMissileTimer;
        public int nukeWhistleMissileSetTimer;
        public Vector2 nukeWhistleVelocity;
        public int nukeWhistleMissilesRemaining;
        public int nukeWhistleDamage;
        public float nukeWhistleKnockback;
        public int nukeWhistleProjectileType;
        public int nukeWhistleBarrageIndex;

        #region Mouse Listeners
        public Vector2 mouseWorld => Player.MountedCenter + mouseWorldDeltaFromPlayer;
        public Vector2 mouseWorldDeltaFromPlayer;
        public float mouseRotationFromPlayer;
        private Vector2 oldMouseWorldDeltaFromPlayer;
        private int mouseWorldPacketTimer;
        private const int MouseWorldPacketInterval = 2;
        public bool mouseWorldListener;
        public bool mouseRotationListener;
        public bool syncMousePosition;
        public bool syncMouseRotation;
        #endregion

        private static readonly HashSet<int> ManualFlails = new();

        public override void ResetEffects()
        {
            spearSearing = false;
            spearArctic = false;
            sentryCrits = false;
            seersGlass = false;
            godsPitch = false;
            blightedBadge = false;
            doubleFlailAcc = false;
            perfectPurple = false;
            blackholeFlail = false;
            holyFlail = false;
            perennialShield = false;
            marniteHoverboots = false;

            if (annihilationBonusShotTimeLeft > 0)
                annihilationBonusShotTimeLeft--;

            if (annihilationBonusShotCooldown > 0)
                annihilationBonusShotCooldown--;

            if (gorgonShotsHit < 0)
                gorgonShotsHit = 0;
            else if (gorgonShotsHit > 20)
                gorgonShotsHit = 20;

            if (gorgonIncarnonShotsStored < 0)
                gorgonIncarnonShotsStored = 0;
            else if (gorgonIncarnonShotsStored > 20)
                gorgonIncarnonShotsStored = 20;

            if (!hollowbrookLocation.HasValue)
            {
                hollowbrookTier = 0;
            }
            hollowbrookLocation = null;

            if (hollowbrookCrits < 0)
                hollowbrookCrits = 0;
            else if (hollowbrookCrits > 10)
                hollowbrookCrits = 10;

            if (hollowbrookSpawnCooldown > 0)
                hollowbrookSpawnCooldown--;
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

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.HasBuff<HollowbrookTag>() && hollowbrookTier >= 2)
            {
                if (hit.Crit)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 spawnPosition = target.Center;

                        bool TrySpawnPickup(int itemType)
                        {
                            if (!CanSpawnPickup(itemType, spawnPosition))
                                return false;

                            int itemIndex = Item.NewItem(target.GetSource_OnHit(target), target.getRect(), itemType);

                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);

                            return true;
                        }

                        bool heart = TrySpawnPickup(ItemID.Heart);
                        bool star = TrySpawnPickup(ItemID.Star);
                        bool note = false;
                        if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium) && thorium.TryFind("InspirationNote", out ModItem inspNote))
                        {
                            note = TrySpawnPickup(inspNote.Type);
                        }

                        if (heart || star || note)
                        {
                            hollowbrookSpawnCooldown = 5 * 60;
                        }
                    }

                    hollowbrookCrits++;
                }
            }
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

            if (target.HasBuff<HollowbrookTag>() && hollowbrookTier >= 2)
            {
                if (hit.Crit)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 spawnPosition = target.Center;

                        bool TrySpawnPickup(int itemType)
                        {
                            if (!CanSpawnPickup(itemType, spawnPosition))
                                return false;

                            int itemIndex = Item.NewItem(target.GetSource_OnHit(target), target.getRect(), itemType);

                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);

                            return true;
                        }

                        bool heart = TrySpawnPickup(ItemID.Heart);
                        bool star = TrySpawnPickup(ItemID.Star);
                        bool note = false;
                        if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium) && thorium.TryFind("InspirationNote", out ModItem inspNote))
                        {
                            note = TrySpawnPickup(inspNote.Type);
                        }

                        if (heart || star || note)
                        {
                            hollowbrookSpawnCooldown = 5 * 60;
                        }
                    }

                    hollowbrookCrits++;
                }

                if (proj.IsMinionOrSentryRelated)
                {
                    if (ModLoader.TryGetMod("CalamityMod", out Mod calamity) && calamity.TryFind("ArmorCrunch", out ModBuff armorCrunch))
                    {
                        target.AddBuff(armorCrunch.Type, 180);
                    }
                    else
                    {
                        target.AddBuff(BuffID.BrokenArmor, 180);
                    }
                }
            }
        }

        private bool CanSpawnPickup(int itemType, Vector2 position)
        {
            if (hollowbrookSpawnCooldown != 0)
                return false;

            int total = 0;
            foreach (Item item in Main.ActiveItems)
            {
                if (item.type != itemType)
                    continue;

                total++;
            }

            return total < 6;
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.hostile)
                return;

            if (sentryCrits && IsSummonDamage(proj) && (proj.sentry || ProjectileID.Sets.SentryShot[proj.type]))
            {
                if (Main.rand.Next(100) < ActualClassCrit(Player, DamageClass.Summon))
                    modifiers.SetCrit();
            }
        }

        public static void InflictRandomDebuff(NPC target)
        {
            int debuffRarity = Main.rand.Next(21);

            ModLoader.TryGetMod("ThoriumMod", out Mod thoriumMod);
            ModLoader.TryGetMod("CalamityMod", out Mod cal);

            if (debuffRarity < 5)
            {
                int debuff = Main.rand.Next(8);

                switch (debuff)
                {
                    case 0:
                        target.AddBuff(BuffID.Poisoned, 180, false);
                        break;
                    case 2:
                        target.AddBuff(BuffID.Frostburn, 180, false);
                        break;
                    case 3:
                        goto default;
                    case 4:
                        target.AddBuff(BuffID.Confused, 90, false);
                        break;
                    case 5:
                        target.AddBuff(BuffID.Ichor, 120, false);
                        break;
                    case 6:
                        if (thoriumMod != null)
                        {
                            target.AddBuff(thoriumMod.Find<ModBuff>("Charmed").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 7:
                        if (thoriumMod != null)
                        {
                            if (!target.boss)
                            {
                                target.AddBuff(thoriumMod.Find<ModBuff>("Stunned").Type, 30, false);
                                break;
                            }
                            else goto default;
                        }
                        else goto default;
                    default:
                        target.AddBuff(BuffID.OnFire3, 180, false);
                        break;
                }
            }
            else if (debuffRarity >= 5 && debuffRarity < 20)
            {
                int debuff = Main.rand.Next(8);

                switch (debuff)
                {
                    case 0:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("BrimstoneFlames").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 2:
                        target.AddBuff(BuffID.ShadowFlame, 180, false);
                        break;
                    case 3:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("Plague").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 4:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("AstralInfectionDebuff").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 5:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("CrushDepth").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 6:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("Crumbling").Type, 120, false);
                            break;
                        }
                        else goto default;
                    default:
                        target.AddBuff(BuffID.CursedInferno, 180, false);
                        break;
                }
            }
            else
            {
                int debuff = Main.rand.Next(11);

                switch (debuff)
                {
                    case 0:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("MiracleBlight").Type, 120, false);
                            break;
                        }
                        else goto default;
                    case 2:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("VulnerabilityHex").Type, 120, false);
                            break;
                        }
                        else goto default;
                    case 3:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("WhisperingDeath").Type, 120, false);
                            break;
                        }
                        else goto default;
                    case 4:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("MarkedforDeath").Type, 120, false);
                            break;
                        }
                        else goto default;
                    case 5:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("Laceration").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 6:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("ElementalMix").Type, 60, false);
                            break;
                        }
                        else goto default;
                    case 7:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("HolyFlames").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 8:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("GodSlayerInferno").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 9:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("Dragonfire").Type, 180, false);
                            break;
                        }
                        else goto default;
                    case 10:
                        if (cal != null)
                        {
                            target.AddBuff(cal.Find<ModBuff>("Voidfrost").Type, 180, false);
                            break;
                        }
                        else goto default;
                    default:
                        if (Main.rand.NextBool())
                        {
                            target.AddBuff(BuffID.CursedInferno, 180, false);
                        }
                        else
                        {
                            target.AddBuff(BuffID.ShadowFlame, 180, false);
                        }
                        break;
                }
            }
        }

        public static void GrantRandomBuff(Player player)
        {
            player.AddBuff(Utils.NextFromList<int>(Main.rand, new int[]
            {
                2, 3, 5, 6, 7, 24, 26, 26, 48, 59, 87, 89, 105, 108, 110, 112, 113, 114, 115, 116, 117, 146, 150, 158, 170, 173, 176, 179, 206, 207, 257,
            }), 600, false);
        }

        public override void PreUpdate()
        {
            if (Main.myPlayer == base.Player.whoAmI)
            {
                Vector2 vector = (LockOnHelper.Enabled ? LockOnHelper.PredictedPosition : Main.MouseWorld);
                mouseWorldDeltaFromPlayer = vector - base.Player.MountedCenter;
                mouseRotationFromPlayer = mouseWorldDeltaFromPlayer.ToRotation();

                if (mouseWorldListener && Vector2.Distance(mouseWorldDeltaFromPlayer, oldMouseWorldDeltaFromPlayer) > 5f)
                {
                    oldMouseWorldDeltaFromPlayer = mouseWorldDeltaFromPlayer;
                    syncMousePosition = true;
                    mouseWorldListener = false;
                }

                if (mouseRotationListener && Math.Abs(mouseWorldDeltaFromPlayer.ToRotation() - oldMouseWorldDeltaFromPlayer.ToRotation()) > 0.15f)
                {
                    oldMouseWorldDeltaFromPlayer = mouseWorldDeltaFromPlayer;
                    syncMouseRotation = true;
                    mouseRotationListener = false;
                }
            }
        }

        public override void UpdateDead()
        {
            murasamaHitCooldown = 0;
        }

        public override void PostUpdate()
        {
            if (nukeWhistleMissilesRemaining <= 0)
                return;

            nukeWhistleMissileTimer--;

            if (nukeWhistleMissileTimer > 0)
                return;

            Player player = Player;

            if (player.dead || !player.active)
            {
                nukeWhistleMissilesRemaining = 0;
                return;
            }

            Vector2 target = Main.MouseWorld;
            Vector2 direction = target - player.Center;

            if (direction.LengthSquared() > 0f)
                direction.Normalize();

            int sign = Main.rand.NextBool() ? 1 : -1;

            int xOffset = (int)((Main.MouseWorld.X - player.Center.X) / 3f)
                + Main.rand.Next(-4, 8) * 65 * sign;

            Projectile.NewProjectile(
                player.GetSource_ItemUse(player.HeldItem),
                player.Center + new Vector2(xOffset, -Main.screenHeight / 2f),
                nukeWhistleVelocity,
                nukeWhistleProjectileType,
                nukeWhistleDamage,
                nukeWhistleKnockback,
                player.whoAmI
            );

            nukeWhistleMissilesRemaining--;

            // 10 ticks between each missile.
            if (nukeWhistleMissilesRemaining > 0)
                nukeWhistleMissileTimer = nukeWhistleMissileSetTimer;
        }

        public override void PostUpdateEquips()
        {
            ThoriumPlayer thoriumPlayer = Player.GetThoriumPlayer();

            if (scourgeBag2)
            {
               Player.AddBuff(ModContent.BuffType<YoyoProtectionBuff>(), 2);
               Player.endurance += 0.1f;
            }

            float hoverSpeed = 0.4f;
            if (marniteHoverboots && !Player.mount.Active)
            {
                marniteHoverbootsJumped = Player.controlDown;

                bool airborne = !Player.IsOnStandableGround();
                if (airborne)
                {
                    if (marniteHoverbootsHoverTimer == 0 && Player.velocity.Y >= hoverSpeed)
                        Player.position.Y -= hoverSpeed;

                    marniteHoverbootsHoverTimer++;
                }
                else
                {
                    marniteHoverbootsHoverTimer = 0;
                }

                marniteHoverbootsCanHover = marniteHoverbootsHoverTimer < 240;

                if (marniteHoverbootsCanHover && !marniteHoverbootsJumped && airborne)
                {
                    Player.maxFallSpeed = 0f;
                    Player.fallStart = (int)(Player.position.Y / 16f);

                    Vector2 dustPosition = Player.BottomLeft - new Vector2(2f);
                    int dustWidth = Player.width + 4;

                    int dustIndex = Dust.NewDust(dustPosition, dustWidth, 4, DustID.Vortex, 0f, 0f, 100, new Color(), 1.25f);
                    Dust dust = Main.dust[dustIndex];
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.velocity *= 0f;
                }
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            ThoriumPlayer thoriumPlayer = Player.GetThoriumPlayer();

            if (scourgeBag2)
            {
                ((ModPlayer)this).Player.AddBuff(ModContent.BuffType<YoyoProtectionDebuff>(), 60 * SandshroudPouch.CooldownSeconds, true, false);
                scourgeBag2 = false;

                for (int n = 0; n < 15; n++)
                {
                    int num6 = Dust.NewDust(
                        Player.position,
                        0,
                        0,
                        DustID.Firework_Yellow,
                        0f,
                        0f,
                        255,
                        new Color(255, 255, 0),
                        1.35f
                    );

                    Main.dust[num6].noGravity = true;

                    int num7 = Main.rand.Next(-50, 51);
                    int num8 = Main.rand.Next(-50, 51);

                    Dust dust = Main.dust[num6];

                    dust.position.X += num7;
                    dust.position.Y += num8;

                    dust.velocity.X = num7 * 0.075f;
                    dust.velocity.Y = num8 * 0.075f;
                }
            }
        }

        public override void PostUpdateMiscEffects()
        {
            if (murasamaHitCooldown > 0)
                murasamaHitCooldown--;

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

            if (Player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
            {
                mouseWorldPacketTimer = Math.Min(mouseWorldPacketTimer + 1, MouseWorldPacketInterval);
                if (mouseWorldPacketTimer >= MouseWorldPacketInterval)
                {
                    if (syncMousePosition)
                    {
                        mouseWorldPacketTimer = 0;
                        syncMousePosition = false;
                        syncMouseRotation = false;
                        MousePositionSyncPacket.Send(this);
                    }

                    if (syncMouseRotation)
                    {
                        mouseWorldPacketTimer = 0;
                        syncMouseRotation = false;
                        MouseRotationSyncPacket.Send(this);
                    }
                }
            }
        }

        public override void ModifyZoom(ref float zoom)
        {
            ThoriumPlayer mp = Player.GetThoriumPlayer();

            if (mp.accScryingGlassActive && mp.accScryingGlass && seersGlass)
            {
                zoom *= 2f;
            }
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

        public float ActualClassCrit(Player player, DamageClass damageClass) => (damageClass == DamageClass.Summon || damageClass == DamageClass.SummonMeleeSpeed) && !(sentryCrits) ? 0 : player.GetTotalCritChance(damageClass);

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

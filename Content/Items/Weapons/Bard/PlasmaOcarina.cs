using System;
using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class PlasmaOcarina : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<ResourceConsumptionChance>(4);
            Empowerments.AddInfo<ResourceRegen>(4);
            Empowerments.AddInfo<ResourceMaximum>(4);
            Empowerments.AddInfo<CriticalStrikeChance>(3);
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetBardDefaults()
        {
            Item.Size = new Vector2(38, 31);
            Item.value = Item.sellPrice(gold: 3);

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.damage = 550;
            Item.knockBack = 4f;
            Item.noMelee = true;

            Item.UseSound = SoundID.Item42;

            Item.shoot = ModContent.ProjectileType<BardPulseRiffleShot>();
            Item.shootSpeed = 10f;

            InspirationCost = 1;

            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();

            ((ModItem)this).Item.useStyle = ItemUseStyleID.Shoot;
            if (!ModLoader.HasMod("Look"))
            {
                ((ModItem)this).Item.holdStyle = 3;
            }
        }

        int channelValue = 0;
        int notUsedTimer = 0;

        public override bool AltFunctionUse(Player player) => true;

        public override void BardHoldItem(Player player)
        {
            if (notUsedTimer > 0) notUsedTimer--;
            if (channelValue > 0 && notUsedTimer <= 0) channelValue = 0;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(6, -10);
        }

        public override void UseItemFrame(Player player)
        {
            ((ModItem)this).HoldItemFrame(player);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += Utils.RotatedBy(new Vector2((float)(ModLoader.HasMod("Look") ? (-4) : (-6)), (float)(ModLoader.HasMod("Look") ? 6 : 8)) * player.Directions, (double)player.itemRotation, default(Vector2));
        }

        public override bool? BardUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                notUsedTimer = 10;
                channelValue++;
            }

            return player.altFunctionUse == 2 ? channelValue > 180 : null;
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (channelValue > 180)
            {
                Vector2 center = player.Center;

                for (int i = 0; i < 40; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 direction = angle.ToRotationVector2();

                    Vector2 particleVelocity = direction * Main.rand.NextFloat(8f, 30f);

                    float scale = Main.rand.NextFloat(0.4f, 1.2f);

                    Color color = Main.rand.NextBool(3) ? Color.DarkBlue : Color.Aquamarine;

                    GeneralParticleHandler.SpawnParticle(
                        new GlowOrbParticle(
                            center,
                            particleVelocity,
                            affectedByGravity: false,
                            75, // lifetime
                            scale,
                            color
                        )
                    );
                }

                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/TeslaCannonFire")
                {
                    Volume = 0.8f,
                    PitchVariance = 0.2f
                }, player.Center);

                Projectile projectile2 = Projectile.NewProjectileDirect(source, player.Center + (HoldoutOffset() ?? Vector2.Zero), Vector2.Zero, ModContent.ProjectileType<BardPulseRiffleBoom>(), (int)(damage * 3), knockback);
                float j = 10;
                projectile2.ai[1] = Main.rand.NextFloat(320f, 870f) + j * 45f;
                projectile2.localAI[1] = Main.rand.NextFloat(0.08f, 0.25f);
                projectile2.Opacity = MathHelper.Lerp(0.18f, 0.6f, j / 7f) + Main.rand.NextFloat(-0.08f, 0.08f);
                projectile2.Calamity().stealthStrike = true;
                projectile2.DamageType = Item.DamageType;
                projectile2.netUpdate = true;
                channelValue = 0;
                return false;
            }
            else if (channelValue > 0)
            {
                Vector2 center = player.Center;

                // spawn a few per tick while charging
                for (int i = 0; i < 1; i++)
                {
                    float radius = Main.rand.NextFloat(30f, 60f); // how far out they spawn
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);

                    Vector2 spawnPos = center + angle.ToRotationVector2() * radius;

                    // direction INTO the player
                    Vector2 velocityToPlayer = center - spawnPos;
                    velocityToPlayer.Normalize();
                    velocityToPlayer *= Main.rand.NextFloat(1f, 3f);

                    // scale starts big, shrinks over time
                    float scale = Main.rand.NextFloat(0.1f, 1.0f);

                    Color color = Main.rand.NextBool(3) ? Color.DarkBlue : Color.Aquamarine;

                    GeneralParticleHandler.SpawnParticle(
                        new GlowOrbParticle(
                            spawnPos,
                            velocityToPlayer,
                            affectedByGravity: false,
                            20, // lifetime
                            scale,
                            color
                        )
                    );
                }
            }

            if (player.altFunctionUse != 2)
            {
                Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(-.2f), type, damage, knockback);
                Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(0f), type, damage, knockback);
                Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(.2f), type, damage, knockback);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MysteriousCircuitry>(20)
                .AddIngredient<DubiousPlating>(20)
                .AddIngredient<CosmiliteBar>(8)
                .AddIngredient<AscendantSpiritEssence>(2)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }

    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class BardPulseRiffleShot : BardProjectile, ILocalizedModType, IModType
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        private Color mainColor = Color.Aquamarine;

        private bool notSplit;

        private bool doDamage;

        private NPC closestTarget;

        private NPC lastTarget;

        private float distance;

        private int timesItCanHit = 3;

        public new string LocalizationCategory => "Projectiles.Misc";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[base.Type] = true;
        }

        public override void SetBardDefaults()
        {
            base.Projectile.width = 16;
            base.Projectile.height = 16;
            base.Projectile.friendly = true;
            base.Projectile.DamageType = ThoriumDamageBase<BardDamage>.Instance;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 900;
            base.Projectile.tileCollide = false;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (timesItCanHit <= 0)
            {
                base.Projectile.Kill();
            }

            notSplit = base.Projectile.ai[1] == 0f;
            Lighting.AddLight(base.Projectile.Center, 0.3f, 0f, 0.5f);
            float num = Vector2.Distance(Main.player[base.Projectile.owner].Center, base.Projectile.Center);
            base.Projectile.rotation = base.Projectile.velocity.ToRotation();
            float num2 = 10f;
            if (base.Projectile.localAI[0] == 0f && !notSplit)
            {
                base.Projectile.extraUpdates = 1;
                base.Projectile.timeLeft = 1240;
                if (base.Projectile.ai[1] == 1f)
                {
                    SoundStyle style = SoundID.DD2_DarkMageCastHeal with
                    {
                        Volume = 1.7f,
                        Pitch = 0.3f
                    };
                    SoundEngine.PlaySound(in style, base.Projectile.Center);
                }
            }

            if (notSplit)
            {
                doDamage = true;
                base.Projectile.extraUpdates = 100;
            }

            base.Projectile.localAI[0] += 1f;
            if (base.Projectile.localAI[0] > num2 && notSplit)
            {
                float num3 = MathHelper.Clamp(base.Projectile.localAI[0] * 0.009f, 0f, 3.5f);
                if (num < 1400f)
                {
                    GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(base.Projectile.Center, -base.Projectile.velocity * Main.rand.NextFloat(-0.01f, 0.01f), affectedByGravity: false, 30, 1.1f - base.Projectile.ai[1] + num3, Main.rand.NextBool(3) ? Color.DarkBlue : mainColor));
                }

                if (num < 1400f && Main.rand.NextBool())
                {
                    GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(base.Projectile.Center + base.Projectile.velocity * Main.rand.NextFloat(-2f, 2f), -base.Projectile.velocity * Main.rand.NextFloat(-0.01f, 0.01f), affectedByGravity: false, 30, 1.1f - base.Projectile.ai[1] + num3, Main.rand.NextBool(3) ? Color.DarkBlue : mainColor));
                }

                if (base.Projectile.localAI[0] > 20f && Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(base.Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.RainbowTorch);
                    dust.scale = Main.rand.NextFloat(0.7f, 1.5f);
                    dust.velocity = base.Projectile.velocity * Main.rand.NextFloat(-3f, 3f);
                    dust.noGravity = true;
                    dust.color = (Main.rand.NextBool(3) ? Color.DarkBlue : mainColor);
                    dust.noLight = true;
                }
            }

            if (!notSplit)
            {
                if (num < 1400f)
                {
                    GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(base.Projectile.Center, -base.Projectile.velocity * Main.rand.NextFloat(-0.01f, 0.01f), affectedByGravity: false, 5, 1.7f - base.Projectile.ai[1] * 0.18f, Main.rand.NextBool(3) ? Color.DarkBlue : mainColor));
                }

                if (num < 1400f && Main.rand.NextBool())
                {
                    GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(base.Projectile.Center + base.Projectile.velocity * Main.rand.NextFloat(-2f, 2f), -base.Projectile.velocity * Main.rand.NextFloat(-0.01f, 0.01f), affectedByGravity: false, 5, 1.7f - base.Projectile.ai[1] * 0.18f, Main.rand.NextBool(3) ? Color.DarkBlue : mainColor));
                }

                if (base.Projectile.localAI[0] > 90f)
                {
                    if (base.Projectile.localAI[0] == 91f)
                    {
                        base.Projectile.velocity *= 0.001f;
                    }

                    if (base.Projectile.localAI[0] == 120f)
                    {
                        distance = 3000f;
                        SoundStyle style = new SoundStyle("CalamityMod/Sounds/Item/PulseSound");
                        style.Volume = 0.35f;
                        style.Pitch = 0.3f;
                        style.MaxInstances = -1;
                        SoundEngine.PlaySound(in style, base.Projectile.Center);
                        GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(base.Projectile.Center, Vector2.Zero, mainColor, new Vector2(1f, 1f), Main.rand.NextFloat(12f, 25f), 0f, 0.5f, 15));
                        for (int i = 0; i < 6; i++)
                        {
                            Dust dust2 = Dust.NewDustPerfect(base.Projectile.Center, DustID.RainbowTorch);
                            dust2.scale = Main.rand.NextFloat(0.6f, 1.1f);
                            dust2.velocity = new Vector2(6f, 6f).RotatedByRandom(100.0) * Main.rand.NextFloat(0.05f, 0.8f);
                            dust2.noGravity = true;
                            dust2.color = (Main.rand.NextBool(3) ? Color.DarkBlue : mainColor);
                            dust2.noLight = true;
                        }
                    }

                    if (base.Projectile.localAI[0] > 120f)
                    {
                        if (closestTarget != null)
                        {
                            doDamage = Vector2.Distance(base.Projectile.Center, closestTarget.Center) < 10f;
                        }
                        else
                        {
                            doDamage = false;
                        }

                        float num4 = 9.5f;
                        if (closestTarget != null && closestTarget.active)
                        {
                            float targetAngle = base.Projectile.SafeDirectionTo(closestTarget.Center).ToRotation();
                            float maxChange = 10f + base.Projectile.localAI[0] * 8E-05f;
                            base.Projectile.velocity = base.Projectile.velocity.ToRotation().AngleTowards(targetAngle, maxChange).ToRotationVector2() * num4;
                        }
                        else
                        {
                            base.Projectile.velocity = base.Projectile.rotation.ToRotationVector2() * num4;
                            base.Projectile.velocity *= 0.999f;
                        }

                        if (closestTarget != null && Vector2.Distance(base.Projectile.Center, closestTarget.Center) < 10f)
                        {
                            closestTarget = null;
                            distance = 3000f;
                        }

                        base.Projectile.extraUpdates = 5 + (int)((float)base.Projectile.numHits * 0.3f);
                        for (int j = 0; j < Main.npc.Length; j++)
                        {
                            if (Main.npc[j].CanBeChasedBy() || Main.npc[j] == lastTarget)
                            {
                                float num5 = Main.npc[j].width / 2 + Main.npc[j].height / 2;
                                if (Vector2.Distance(base.Projectile.Center, Main.npc[j].Center) < distance + num5)
                                {
                                    closestTarget = Main.npc[j];
                                    distance = Vector2.Distance(base.Projectile.Center, Main.npc[j].Center);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Main.rand.NextBool(5))
                    {
                        Dust dust3 = Dust.NewDustPerfect(base.Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.RainbowTorch);
                        dust3.scale = Main.rand.NextFloat(0.8f, 1.4f);
                        dust3.velocity = -base.Projectile.velocity * Main.rand.NextFloat(0.2f, 0.5f);
                        dust3.noGravity = true;
                        dust3.color = (Main.rand.NextBool(3) ? Color.DarkBlue : mainColor);
                        dust3.noLight = true;
                    }

                    base.Projectile.velocity *= 0.98f;
                }
            }

            if (base.Projectile.localAI[0] == num2 && notSplit)
            {
                PulseBurst(4f, 5f);
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (!doDamage)
            {
                return false;
            }

            return null;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bool flag = target.life <= 0;
            if (notSplit)
            {
                base.Projectile.Kill();
                for (int i = 0; i <= 9; i++)
                {
                    Dust dust = Dust.NewDustPerfect(base.Projectile.Center, DustID.RainbowTorch);
                    dust.scale = Main.rand.NextFloat(0.4f, 1.1f);
                    dust.velocity = (base.Projectile.velocity * 4f).RotateRandom(0.60000002384185791) * Main.rand.NextFloat(0.2f, 0.9f);
                    dust.noGravity = true;
                    dust.color = (Main.rand.NextBool(3) ? Color.DarkBlue : mainColor);
                    dust.noLight = true;
                }

                float num = MathHelper.Clamp(base.Projectile.localAI[0] * 0.01f, 0f, 3.5f);
                for (int j = 0; j < 9; j++)
                {
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(base.Projectile.Center + Main.rand.NextVector2Circular(8f, 8f) - base.Projectile.velocity * 7f, base.Projectile.velocity * Main.rand.NextFloat(0.7f, 3.1f), affectedByGravity: false, 30, 0.9f * Main.rand.NextFloat(0.9f, 1.1f) + num, mainColor));
                }

                //int num2 = 4;
                //int damage = (int)((float)base.Projectile.damage * 0.5f);
                //for (int k = 0; k < num2; k++)
                //{
                //    Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), base.Projectile.Center, base.Projectile.velocity.SafeNormalize(Vector2.Zero) * (14f - (float)k * 0.7f) * ((float)(k + 1) * 0.25f), ModContent.ProjectileType<PulseRifleShot>(), damage, base.Projectile.knockBack, base.Projectile.owner, 0f, 1f + (float)k);
                //}
            }
            else
            {
                lastTarget = target;
                distance = 3000f;
                base.Projectile.localAI[0] = 60f;
                base.Projectile.velocity *= MathHelper.Clamp(1.5f - (float)base.Projectile.numHits * 0.5f, 1f, 1.5f);
                for (int l = 0; l <= 2; l++)
                {
                    GeneralParticleHandler.SpawnParticle(new SquishyLightParticle(base.Projectile.Center, (base.Projectile.velocity * 2f).RotatedByRandom(0.699999988079071) * Main.rand.NextFloat(0.1f, 0.4f), Main.rand.NextFloat(0.1f, 0.25f), Main.rand.NextBool(3) ? Color.DarkBlue : mainColor, Main.rand.Next(20, 31), 0.25f, 2f));
                }

                if (hit.Damage > 2 && target == closestTarget)
                {
                    timesItCanHit--;
                }

                if (flag)
                {
                    timesItCanHit++;
                    base.Projectile.timeLeft += 90;
                }
            }
        }

        private void PulseBurst(float speed1, float speed2)
        {
            for (int i = 0; i <= 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(base.Projectile.Center, DustID.RainbowTorch);
                dust.scale = Main.rand.NextFloat(0.4f, 1.4f);
                dust.velocity = (base.Projectile.velocity * 4f).RotateRandom(0.60000002384185791) * Main.rand.NextFloat(0.2f, 0.9f);
                dust.noGravity = true;
                dust.color = (Main.rand.NextBool(3) ? Color.DarkBlue : mainColor);
                dust.noLight = true;
            }

            for (int j = 0; j <= 15; j++)
            {
                GeneralParticleHandler.SpawnParticle(new SquishyLightParticle(base.Projectile.Center, (base.Projectile.velocity * 4f).RotatedByRandom(0.60000002384185791) * Main.rand.NextFloat(0.1f, 0.4f), Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextBool(3) ? Color.DarkBlue : mainColor, Main.rand.Next(30, 41), 0.25f, 2f));
            }
        }
    }

    public class BardPulseRiffleBoom : BardProjectile, ILocalizedModType, IModType
    {
        public override string Texture => "CalamityMod/Particles/SmallBloomRingLayered";

        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public float PlasmaOcarinaLifetimeCompletion => 1f - Projectile.timeLeft / 60f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetBardDefaults()
        {
            Projectile.DamageType = (DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance;
            Projectile.width = 96;
            Projectile.height = 96;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.timeLeft = 60;
            Projectile.scale = 0.001f;
        }

        public override void AI()
        {
            if (WeaponConfig.Instance.EnableScreenEffects)
            {
                float lifetime = 60f;
                float t = Projectile.timeLeft / lifetime;

                float screenShakePower =
                    (1f - t) *
                    Utils.GetLerpValue(1300f, 0f,
                        Projectile.Distance(Main.LocalPlayer.Center), true);

                Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakePower * 6f;
            }

            Projectile.ai[0]++;

            // Initial random rotation setup
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.rotation = Utils.NextFloat(Main.rand, MathHelper.TwoPi);
                Projectile.localAI[0] = Utils.ToDirectionInt(Utils.NextBool(Main.rand));
                Projectile.netUpdate = true;
            }

            Projectile.Opacity = (1f - (float)Math.Pow(PlasmaOcarinaLifetimeCompletion, 1.56)) * 0.4f;
            Projectile.scale = MathHelper.Lerp(0.1f, 30f, PlasmaOcarinaLifetimeCompletion);
            Projectile.rotation += Projectile.localAI[0] * 0.012f;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            Color start = Color.Aquamarine;
            Color end = Color.DarkBlue;
            Color blended = Color.Lerp(start, end, PlasmaOcarinaLifetimeCompletion);

            return blended * Projectile.Opacity;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<GraniteSurge>(), 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Color drawColor = Projectile.GetAlpha(lightColor) * 0.33f;

            for (int i = 0; i < 8; i++)
            {
                float rotation = Projectile.rotation;
                Vector2 drawOffset = Utils.ToRotationVector2((float)Math.PI * 2f * i / 8f) * Projectile.scale;
                Vector2 drawPosition = Projectile.Center - Main.screenPosition + drawOffset;

                if (i % 2 == 1) rotation *= -1f;

                Main.EntitySpriteDraw(
                    texture, drawPosition, null, drawColor,
                    rotation, Utils.Size(texture) * 0.5f,
                    Projectile.scale, SpriteEffects.None, 0f
                );
            }
            return false;
        }

        public override bool? CanHitNPC(NPC target) => !target.CountsAsACritter && !target.friendly && target.chaseable;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.scale * 48f, targetHitbox);
        }
    }
}
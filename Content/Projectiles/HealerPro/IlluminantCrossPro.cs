using InfernalEclipseWeaponsDLC.Content.Projectiles.RoguePro;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs.Healer;
using ThoriumMod.Sounds;
using InfernalEclipseWeaponsDLC.Utilities;
using System.Collections.Generic;


namespace InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro
{
    public class IlluminantCrossPro : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Assets/Textures/Empty";

        public int empower;

        public const int empowerMax = 5;

        public Player Player => Main.player[Projectile.owner];

        public ref float MaxScale => ref Projectile.ai[0];
        public ref float AI_Timer => ref Projectile.ai[1];

        private HashSet<int> healedPlayers = new HashSet<int>(); // track who got healed

        private bool hasHealedThisBurst = false;

        public override void SetDefaults()
        {
            ((ModProjectile)this).Projectile.DamageType = (DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance;
            ((Entity)((ModProjectile)this).Projectile).width = 10;
            ((Entity)((ModProjectile)this).Projectile).height = 10;
            ((ModProjectile)this).Projectile.aiStyle = -1;
            ((ModProjectile)this).Projectile.penetrate = -1;
            ((ModProjectile)this).Projectile.alpha = 255;
            ((ModProjectile)this).Projectile.tileCollide = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];

            modifiers.SourceDamage += empower * 0.2f;
            modifiers.HitDirectionOverride = Utils.ToDirectionInt(target.Center.X >= player.Center.X);

            if (!Collision.CanHitLine(player.position, player.width, player.height,
                                      target.position, target.width, target.height))
            {
                modifiers.SourceDamage *= 0.5f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.immune[Projectile.owner] = 20;
        }


        public override void AI()
        {
            Player player = Main.player[((ModProjectile)this).Projectile.owner];

            //Charging code
            int dustType = DustID.BlueCrystalShard;
            float scale = 3.5f + (float)empower * 0.5f;
            int num6 = Dust.NewDust(((Entity)((ModProjectile)this).Projectile).position, ((Entity)((ModProjectile)this).Projectile).width, ((Entity)((ModProjectile)this).Projectile).height, dustType, 0f, 0f, 100, default(Color), 0.5f + (float)empower * 0.1f);
            Dust obj = Main.dust[num6];
            obj.color = new Color(80, 200, 255);
            obj.noGravity = true;
            obj.velocity *= 0.75f;


            if (Main.rand.NextBool(2)) // 50% chance to spawn the mushroom dust
            {
                Color[] mushroomColors = new Color[]
                {
        Color.LightSeaGreen,
        Color.Blue
                };

                int mushroomDustCount = 1; // small scaling with empower
                float spawnRadius = 25f; // start further out with more empower

                for (int i = 0; i < mushroomDustCount; i++)
                {
                    Color chosenColor = mushroomColors[Main.rand.Next(mushroomColors.Length)];
                    Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitY);

                    // spawn around the projectile, not at the center
                    Vector2 spawnPos = Projectile.Center + direction * spawnRadius;

                    int dustIndex = Dust.NewDust(
                        spawnPos,
                        0, 0,
                        DustID.Cloud,
                        0f, 0f, 100,
                        chosenColor,
                        0.7f + empower * 0.1f
                    );

                    Dust d = Main.dust[dustIndex];
                    d.noGravity = true;

                    // velocity inward toward the projectile center
                    d.velocity = -direction * Main.rand.NextFloat(2f, 2.5f);

                    d.fadeIn = 0.6f;
                    d.velocity *= Main.rand.NextFloat(0.9f, 1.3f);
                }
            }

            int num7 = Main.rand.Next(-20, 21);
            int num8 = Main.rand.Next(-20, 21);
            obj.position.X += num7;
            obj.position.Y += num8;
            obj.velocity.X = (float)(-num7) * (0.035f + (float)empower * 0.0065f);
            obj.velocity.Y = (float)(-num8) * (0.035f + (float)empower * 0.0065f);


            int interval = (int)(12f / player.GetTotalAttackSpeed((DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance));
            SoundStyle val;
            if (((ModProjectile)this).Projectile.ai[1]++ >= (float)interval)
            {
                if (empower < 5)
                {
                    empower++;
                    if (empower == 5)
                    {
                        val = SoundID.MaxMana.WithVolumeScale(0.75f);
                        SoundEngine.PlaySound(val, player.Center);
                        if (dustType > 0)
                        {
                            float count = 30f;
                            for (int i = 0; (float)i < count; i++)
                            {
                                Vector2 vector = Vector2.Zero;
                                vector += -Utils.RotatedBy(Vector2.UnitY, (double)((float)i * ((float)Math.PI * 2f) / count), default(Vector2)) * new Vector2(16f, 16f);
                                Dust obj2 = Dust.NewDustDirect(((Entity)((ModProjectile)this).Projectile).Center, 0, 0, dustType, 0f, 0f, 255, default(Color), 1f);
                                obj2.color = new Color(80, 200, 255);
                                obj2.noGravity = true;
                                obj2.position = ((Entity)((ModProjectile)this).Projectile).Center + vector;
                                obj2.velocity = Utils.SafeNormalize(vector, Vector2.UnitY) * 1f;
                            }
                        }
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.Item24, player.Center);
                    }
                }
                ((ModProjectile)this).Projectile.ai[1] = 0f;
            }
            player.SetDummyItemTime(2);
            if (player.channel && !player.noItems && !player.CCed)
            {
                Projectile.timeLeft = 10;

                if (Main.myPlayer == Projectile.owner)
                {
                    // Instantly lock the projectile to the mouse position
                    Projectile.Center = Main.MouseWorld;

                    // Ensure it's perfectly synced in multiplayer
                    Projectile.netUpdate = true;

                    // Make the player face the mouse
                    player.ChangeDir(Math.Sign(Main.MouseWorld.X - player.Center.X));
                }

                Projectile.velocity = Vector2.Zero; // no momentum needed
                Projectile.gfxOffY = player.gfxOffY;
            }
            else
            {
                if (((ModProjectile)this).Projectile.timeLeft <= 6)
                {
                    return;
                }
                if (((ModProjectile)this).Projectile.localAI[0] == 0f)
                {
                    ((ModProjectile)this).Projectile.localAI[0] = 1f;
                    ((ModProjectile)this).Projectile.friendly = true;
                    ((Entity)((ModProjectile)this).Projectile).velocity.X = 0f;
                    ((Entity)((ModProjectile)this).Projectile).velocity.Y = 0f;
                    ((ModProjectile)this).Projectile.tileCollide = false;
                    ((ModProjectile)this).Projectile.alpha = 255;
                    Vector2 oldCenter = ((Entity)((ModProjectile)this).Projectile).Center;
                    ((Entity)((ModProjectile)this).Projectile).Size = new Vector2((float)(45 + empower * 14));
                    ((Entity)((ModProjectile)this).Projectile).Center = oldCenter;
                }
                ((ModProjectile)this).Projectile.timeLeft = 6;
                val = ThoriumSounds.BasicSpell.WithVolumeScale(1.07f);
                SoundEngine.PlaySound(val, Projectile.Center);
                float num102 = 50f;
                for (int j = 0; (float)j < num102; j++)
                {
                    Vector2 vector12 = Vector2.UnitX * 0f;
                    vector12 += -Utils.RotatedBy(Vector2.UnitY, (double)((float)j * ((float)Math.PI * 2f / num102)), default(Vector2)) * new Vector2(2f, 2f);
                    vector12 = Utils.RotatedBy(vector12, (double)Utils.ToRotation(((Entity)((ModProjectile)this).Projectile).velocity), default(Vector2));
                    int num104 = Dust.NewDust(((Entity)((ModProjectile)this).Projectile).Center, 0, 0, dustType, 0f, 0f, 0, default(Color), 1.35f);
                    Main.dust[num104].color = new Color(80, 200, 255);
                    Main.dust[num104].noGravity = true;
                    Main.dust[num104].position = ((Entity)((ModProjectile)this).Projectile).Center + vector12;
                    Main.dust[num104].velocity = ((Entity)((ModProjectile)this).Projectile).velocity * 0f + Utils.SafeNormalize(vector12, Vector2.UnitY) * scale;
                }
                if (empower >= 5)
                {
                    float num202 = 50f;
                    for (int k = 0; (float)k < num202; k++)
                    {
                        Vector2 vector13 = Vector2.UnitX * 0f;
                        vector13 += -Utils.RotatedBy(Vector2.UnitY, (double)((float)k * ((float)Math.PI * 2f / num202)), default(Vector2)) * new Vector2(2f, 2f);
                        vector13 = Utils.RotatedBy(vector13, (double)Utils.ToRotation(((Entity)((ModProjectile)this).Projectile).velocity), default(Vector2));
                        int num204 = Dust.NewDust(((Entity)((ModProjectile)this).Projectile).Center, 0, 0, dustType, 0f, 0f, 0, default(Color), 1.35f);
                        Main.dust[num204].color = new Color(80, 200, 255);
                        Main.dust[num204].noGravity = true;
                        Main.dust[num204].position = ((Entity)((ModProjectile)this).Projectile).Center + vector13;
                        Main.dust[num204].velocity = ((Entity)((ModProjectile)this).Projectile).velocity * 0f + Utils.SafeNormalize(vector13, Vector2.UnitY) * (scale / 3f);
                    }
                    float num302 = 50f;
                    for (int l = 0; (float)l < num302; l++)
                    {
                        Vector2 vector14 = Vector2.UnitX * 0f;
                        vector14 += -Utils.RotatedBy(Vector2.UnitY, (double)((float)l * ((float)Math.PI * 2f / num302)), default(Vector2)) * new Vector2(2f, 2f);
                        vector14 = Utils.RotatedBy(vector14, (double)Utils.ToRotation(((Entity)((ModProjectile)this).Projectile).velocity), default(Vector2));
                        int num304 = Dust.NewDust(((Entity)((ModProjectile)this).Projectile).Center, 0, 0, dustType, 0f, 0f, 0, default(Color), 1.35f);
                        Main.dust[num304].color = new Color(80, 200, 255);
                        Main.dust[num304].noGravity = true;
                        Main.dust[num304].position = ((Entity)((ModProjectile)this).Projectile).Center + vector14;
                        Main.dust[num304].velocity = ((Entity)((ModProjectile)this).Projectile).velocity * 0f + Utils.SafeNormalize(vector14, Vector2.UnitY) * (scale / 3f);
                    }

                    // === FIRE 3 ILLUMINANT BOLTS ON FULL CHARGE ===
                    if (Main.myPlayer == Projectile.owner)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            // Randomized direction for each bolt
                            Vector2 shootDirection = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitY);

                            // Randomized speed
                            float speed = Main.rand.NextFloat(1f, 2f);
                            Vector2 velocity = shootDirection * speed;

                            // Spawn the projectile
                            int proj = Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                Projectile.Center,
                                velocity,
                                ModContent.ProjectileType<IlluminantBolt>(),
                                Projectile.damage,
                                Projectile.knockBack,
                                Projectile.owner
                            );

                            // Make sure it’s Radiant (Healer) damage
                            if (proj >= 0 && proj < Main.maxProjectiles)
                            {
                                Main.projectile[proj].DamageType = ThoriumDamageBase<HealerDamage>.Instance;
                            }
                        }
                    }

                    if (empower >= 5)
                    {
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            Player target = Main.player[i];
                            if (target.active && !target.dead && CanHitPlayer(target))
                            {
                                float distance = Vector2.Distance(Projectile.Center, target.Center);
                                if (distance < 45 + empower * 14) // same as your burst radius
                                {
                                    HealTeammateThorium(player, target, baseHeal: 5);
                                }
                            }
                        }
                    }

                }

                // 🔹 Empower-based burst ring effect
                int burstDustCount = 10 + empower * 8; // more dust with higher empower
                float burstSpeed = 1.2f + empower * 1.2f; // faster dusts with more empower
                Color[] colors = new Color[] { Color.LightSeaGreen, Color.Blue }; // mix of colors

                Color chosenColor = colors[Main.rand.Next(colors.Length)];

                for (int i = 0; i < burstDustCount; i++)
                {
                    // Random dust type for variety
                    int dustTypeChoice = DustID.Cloud;
                    Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitY);

                    // Position slightly out from center
                    Vector2 spawnPos = Projectile.Center + direction * 5f;

                    // Create dust
                    int dustIndex = Dust.NewDust(
                        spawnPos,
                        0,
                        0,
                        dustTypeChoice,
                        direction.X * burstSpeed,
                        direction.Y * burstSpeed,
                        100,
                        chosenColor,
                        1.2f
                    );

                    Dust d = Main.dust[dustIndex];
                    d.noGravity = true;
                    d.velocity *= Main.rand.NextFloat(0.9f, 1.3f);
                }

            }
        }

        private void HealTeammateThorium(Player healer, Player target, int baseHeal)
        {
            if (healer.whoAmI != Main.myPlayer) return;
            if (healer == target) return;
            if (healer.team == 0 || healer.team != target.team) return;

            if (healedPlayers.Contains(target.whoAmI))
                return;

            if (baseHeal <= 0 && healer.GetModPlayer<ThoriumPlayer>().healBonus <= 0)
                return; // Nothing to heal

            HealerHelper.HealPlayer(
                healer,
                target,
                healAmount: baseHeal, // <-- don't add healBonus here
                recoveryTime: 60,
                healEffects: true,
                extraEffects: p => p.AddBuff(ModContent.BuffType<Cured>(), 30, true, false)
            );

            healedPlayers.Add(target.whoAmI);

            // Optional dust visuals
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(target.width / 2f, target.height / 2f);
                Dust.NewDustPerfect(target.Center + offset, DustID.Enchanted_Gold, Vector2.Zero, 150, Color.White, 1.2f).noGravity = true;
            }
        }

        public override bool CanHitPlayer(Player target)
        {
            Player owner = Main.player[Projectile.owner];
            // Only hit teammates (but not self)
            return owner.team != 0 && target.team == owner.team && target.whoAmI != owner.whoAmI;
        }


        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Player player = Main.player[Projectile.owner];

            // Only heal teammates, not self
            if (player.team != 0 && player.team == target.team && target.whoAmI != player.whoAmI && empower >= 5)
            {
                HealTeammateThorium(player, target, baseHeal: 5); // base 0 + Thorium bonus
            }

            // Only apply debuff if this is a PvP hit
            bool isPvPHit = player.whoAmI != target.whoAmI    // Not self
                            && player.team != 0             // Teams are enabled
                            && player.team != target.team;  // Not same team

            if (isPvPHit)
            {
                if (!hasHealedThisBurst && empower >= 5)
                {
                    int healAmount = 5;
                    player.statLife += healAmount;
                    if (player.statLife > player.statLifeMax2)
                        player.statLife = player.statLifeMax2;

                    player.HealEffect(healAmount);

                    hasHealedThisBurst = true; // mark that we've healed for this swing
                }

                for (int i = 0; i < 5; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(target.width / 2f, target.height / 2f);
                    Dust dust = Dust.NewDustPerfect(target.Center + offset, DustID.Enchanted_Gold, Vector2.Zero, 150, Color.White, 1.2f);
                    dust.noGravity = true;
                }
            }
        }
    }
}

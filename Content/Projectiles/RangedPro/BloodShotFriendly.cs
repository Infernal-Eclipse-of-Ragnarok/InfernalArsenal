using InfernalEclipseWeaponsDLC.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.RangedPro
{
    public class BloodShotFriendly : ModProjectile
    {
        // Use vanilla Blood Shot sprite
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BloodShot;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;

            Projectile.Center = Projectile.position + new Vector2(Projectile.width / 2, Projectile.height / 2);
        }

        // Prevent player damage
        public override bool CanHitPlayer(Player target) => false;

        private bool spawned = false;
        private bool exploded = false;

        public override void AI()
        {
            // Spawn burst only once
            if (!spawned)
            {
                SoundEngine.PlaySound(SoundID.Item17, Projectile.position);

                spawned = true;
                int dustCount = 12; // doubled for dramatic effect

                Vector2 baseVelocity = Projectile.velocity;
                float speed = baseVelocity.Length(); // use projectile speed for dust speed
                for (int i = 0; i < dustCount; i++)
                {
                    // Random rotation within ±15 degrees (30 degree arc)
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));
                    Vector2 dustVel = baseVelocity.RotatedBy(angleOffset).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(speed * 0.5f, speed);

                    int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood);
                    Main.dust[dustIndex].velocity = dustVel;
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].scale = Main.rand.NextFloat(1.5f, 2.2f);
                }

            }

            // Rotation
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Light homing, red glow, trailing dust
            float maxDetectRadius = 800f;
            float projSpeed = 16f;
            NPC target = FindClosestNPC(maxDetectRadius);
            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                direction *= projSpeed;
                Projectile.velocity = (Projectile.velocity * 30f + direction) / 31f;
            }

            Lighting.AddLight(Projectile.Center, 0.7f, 0f, 0f);

            // trailing dust
            if (Main.rand.NextBool(2)) // more frequent
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = Main.rand.NextFloat(1.2f, 1.6f);
            }
        }



        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closest = null;
            float sqrMaxDistance = maxDetectDistance * maxDetectDistance;

            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC target = Main.npc[k];
                if (target.CanBeChasedBy())
                {
                    float sqrDistance = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    if (sqrDistance < sqrMaxDistance)
                    {
                        sqrMaxDistance = sqrDistance;
                        closest = target;
                    }
                }
            }
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Get the texture
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // Draw at projectile center
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

            return false; // we've handled drawing
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.player[Projectile.owner].GetModPlayer<BloodRagePlayer>().BloodRageActive)
            {
                if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                {
                    if (calamity.TryFind("BurningBlood", out ModBuff burningBlood))
                    {
                        target.AddBuff(burningBlood.Type, 180);
                    }
                }
            }

            if (!exploded)
            {
                exploded = true;
                Explode();
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!exploded)
            {
                exploded = true;
                Explode();
            }
        }

        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            float aoeRadius = 100f;

            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f));
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood);
                Main.dust[dustIndex].velocity = dustVel;
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].scale = Main.rand.NextFloat(1.5f, 2.5f);
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                int explosionDamage = Projectile.damage;
                if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) <= aoeRadius)
                {
                    NPC.HitInfo hitInfo = new NPC.HitInfo()
                    {
                        Damage = explosionDamage,
                        Knockback = 0f,
                        HitDirection = 0,
                        Crit = false,
                        DamageType = DamageClass.Ranged
                    };
                    npc.StrikeNPC(hitInfo);

                    if (Main.player[Projectile.owner].GetModPlayer<BloodRagePlayer>().BloodRageActive)
                    {
                        if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                        {
                            if (calamity.TryFind("BurningBlood", out ModBuff burningBlood))
                            {
                                npc.AddBuff(burningBlood.Type, 180);
                            }
                        }
                    }
                }
            }
        }

    }
}

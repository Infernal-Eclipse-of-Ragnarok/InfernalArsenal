using CalamityMod.Buffs.Potions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items.Donate;
using ThoriumMod.Projectiles.Bard;
using ThoriumMod.Items;
using ThoriumMod.Sounds;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.AirPodShawty
{
    public class AirPod : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;

        public override void SetBardDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.penetrate = 1;
            Projectile.alpha = 0;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 60;
        }

        public override void AI()
        {
            // Spin really fast while flying
            Projectile.rotation += 0.8f * Projectile.direction; // tweak speed as needed
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CalamityMod.Buffs.DamageOverTime.ElementalMix>(), 60 * 3);

            SoundEngine.PlaySound(ThoriumSounds.DubstepWub, Projectile.position);

            // Spawn burst of projectiles on death (6–8 projectiles)
            int numProjectiles = Main.rand.Next(6, 9);

            for (int i = 0; i < numProjectiles; i++)
            {
                // Random arc around projectile's velocity
                Vector2 baseVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 perturbed = baseVelocity.RotatedByRandom(MathHelper.ToRadians(45)); // 45° arc, tweak if you want wider/narrower
                float speed = Main.rand.NextFloat(2f, 12f);

                perturbed *= speed;

                Projectile.NewProjectile(
                    Projectile.GetSource_Death(),
                    Projectile.Center,
                    perturbed,
                    ModContent.ProjectileType<AirPodNote>(), // <-- replace with your desired projectile type
                    Projectile.damage / 2,     // maybe weaker than the main shot
                    Projectile.knockBack,
                    Projectile.owner
                );

                Color[] dustColors = { Color.Yellow, Color.CornflowerBlue, Color.LimeGreen };

                int dustType;
                Color lightColor;

                switch (Main.rand.Next(0, 3))
                {
                    case 0:
                        dustType = DustID.YellowTorch;
                        lightColor = Color.Yellow;
                        break;
                    case 1:
                        dustType = DustID.BlueTorch;
                        lightColor = Color.CornflowerBlue;
                        break;
                    case 2:
                        dustType = DustID.GreenTorch;
                        lightColor = Color.LimeGreen;
                        break;
                    default:
                        dustType = DustID.YellowTorch;
                        lightColor = Color.Yellow;
                        break;
                }

                // Random number of dust particles
                int dustCount = Main.rand.Next(3, 7);
                for (int j = 0; j < dustCount; j++)
                {
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center,
                        dustType,
                        perturbed * Main.rand.NextFloat(1f, 10f),
                        0,
                        Color.White, // keep dust white so the dust sprite shows its natural color
                        Main.rand.NextFloat(1.0f, 2.0f)
                    );

                    dust.noGravity = true;
                    dust.fadeIn = 0.1f;
                    dust.velocity *= 0.7f;
                    dust.scale *= 1.2f;

                    // Add light using the actual color we want
                    Lighting.AddLight(dust.position,
                        lightColor.R / 255f * 1.2f,
                        lightColor.G / 255f * 1.2f,
                        lightColor.B / 255f * 1.2f
                    );
                }
            }
        }
    }
}

using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    public class AcidBelcherPro : BardProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Environment/AcidDrop";
        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;

        private const int TrailLength = 5; // number of afterimages
        private Vector2[] oldPos = new Vector2[TrailLength];
        private float[] oldRot = new float[TrailLength];

        public override void SetBardDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 20;
            Projectile.timeLeft = 840;
            Projectile.penetrate = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.hostile = false;

            Projectile.ignoreWater = false;
            Projectile.localNPCHitCooldown = 10;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            // Shift trail arrays
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }
            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;

            // Gravity and rotation
            Projectile.velocity.Y += 0.15f;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Optional dust effect
            if (Utils.NextBool(Main.rand))
            {
                Dust obj = Dust.NewDustPerfect(((Entity)((ModProjectile)this).Projectile).Center, DustID.JungleTorch, (Vector2?)null, 0, default(Color), 1f);
                obj.noGravity = true;
                obj.scale = Utils.NextFloat(Main.rand, 0.65f, 0.8f);
                obj.velocity = -((Entity)((ModProjectile)this).Projectile).velocity * 0.4f;
            }
            if (((ModProjectile)this).Projectile.ai[0] < 3f)
            {
                if (((ModProjectile)this).Projectile.timeLeft == 839)
                {
                    Projectile projectile = ((ModProjectile)this).Projectile;
                    ((Entity)projectile).velocity = ((Entity)projectile).velocity * 0.5f;
                }
                ((ModProjectile)this).Projectile.rotation = Utils.ToRotation(((Entity)((ModProjectile)this).Projectile).velocity) - (float)Math.PI / 2f;
                if (((Entity)((ModProjectile)this).Projectile).velocity.Y <= 12f)
                {
                    ((Entity)((ModProjectile)this).Projectile).velocity.Y += 0.15f;
                }
                ((ModProjectile)this).Projectile.tileCollide = ((ModProjectile)this).Projectile.timeLeft <= 300;
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            for (int i = TrailLength - 1; i >= 0; i--)
            {
                if (oldPos[i] == Vector2.Zero) continue;

                float alpha = ((float)(TrailLength - i) / TrailLength) * 0.5f; // fade
                Color drawColor = Color.White * alpha;
                Main.spriteBatch.Draw(tex, oldPos[i] - Main.screenPosition, null, drawColor, oldRot[i], tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            }

            // Draw the main projectile last
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
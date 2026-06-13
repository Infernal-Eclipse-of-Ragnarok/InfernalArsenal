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
    public class FroststeelPulse : BardProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Summon/RustyBeaconPulse";
        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;

        public float LifetimeCompletion => 1f - (float)((ModProjectile)this).Projectile.timeLeft / 30f;

        public override void SetBardDefaults()
        {
            ((Entity)((ModProjectile)this).Projectile).width = 96;
            ((Entity)((ModProjectile)this).Projectile).height = 96;
            ((ModProjectile)this).Projectile.friendly = true;
            ((ModProjectile)this).Projectile.tileCollide = false;
            ((ModProjectile)this).Projectile.ignoreWater = true;
            ((ModProjectile)this).Projectile.penetrate = -1;
            ((ModProjectile)this).Projectile.usesIDStaticNPCImmunity = true;
            ((ModProjectile)this).Projectile.idStaticNPCHitCooldown = 30;
            ((ModProjectile)this).Projectile.timeLeft = 30;
            ((ModProjectile)this).Projectile.scale = 0.001f;
        }

        public override void AI()
        {
            if (((ModProjectile)this).Projectile.localAI[0] == 0f)
            {
                ((ModProjectile)this).Projectile.rotation = Utils.NextFloat(Main.rand, (float)Math.PI * 2f);
                ((ModProjectile)this).Projectile.localAI[0] = Utils.ToDirectionInt(Utils.NextBool(Main.rand));
                ((ModProjectile)this).Projectile.netUpdate = true;
            }
            Projectile.Opacity = (1f - (float)Math.Pow(LifetimeCompletion, 1.56)) * 0.4f;
            ((ModProjectile)this).Projectile.scale = MathHelper.Lerp(0.3f, 16f, LifetimeCompletion);
            Projectile projectile = ((ModProjectile)this).Projectile;
            projectile.rotation += ((ModProjectile)this).Projectile.localAI[0] * 0.012f;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Icy white-blue gradient
            Color start = new Color(180, 230, 255, 0);  // soft light blue
            Color end = new Color(255, 255, 255, 32);  // fades to white glow
            Color blended = Color.Lerp(start, end, LifetimeCompletion);

            return blended * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value;
            Color drawColor = ((ModProjectile)this).Projectile.GetAlpha(lightColor) * 0.33f;
            for (int i = 0; i < 8; i++)
            {
                float rotation = ((ModProjectile)this).Projectile.rotation;
                Vector2 drawOffset = Utils.ToRotationVector2((float)Math.PI * 2f * (float)i / 8f) * ((ModProjectile)this).Projectile.scale;
                Vector2 drawPosition = ((Entity)((ModProjectile)this).Projectile).Center - Main.screenPosition + drawOffset;
                if (i % 2 == 1)
                {
                    rotation *= -1f;
                }
                Main.EntitySpriteDraw(texture, drawPosition, (Rectangle?)null, drawColor, rotation, Utils.Size(texture) * 0.5f, ((ModProjectile)this).Projectile.scale, (SpriteEffects)0, 0f);
            }
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return !target.CountsAsACritter && !target.friendly && target.chaseable;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            target.AddBuff(BuffID.Frozen, 60);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(((Entity)((ModProjectile)this).Projectile).Center, ((ModProjectile)this).Projectile.scale * 48f, targetHitbox);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
    }
}
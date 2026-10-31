using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.SentryPro.Void
{
    public class VeryVeryStrangeSkullShotPro : ModProjectile
    {
        public bool forwardRotation;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.SentryShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = ModLoader.TryGetMod("SOTS", out Mod sots) ? sots.Find<DamageClass>("VoidSummon") : DamageClass.Summon;
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            forwardRotation = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft < 170)
            {
                Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

                Vector2 origin = new(texture.Width * 0.5f, Projectile.height * 0.5f);

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    Vector2 drawPosition = Projectile.oldPos[i] - Main.screenPosition + origin +  new Vector2(0f, Projectile.gfxOffY);

                    Color color = Projectile.GetAlpha(new Color(255, 255, 255, 0) * 0.1f) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length);

                    Main.EntitySpriteDraw(texture, drawPosition, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }

            return true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] != 0f)
                return;

            Projectile.localAI[0] = 1f;

            SoundEngine.PlaySound(new SoundStyle("InfernalEclipseWeaponsDLC/Assets/Sounds/GasterBlast") { PitchVariance = 0.5f }, Projectile.Center);
        }

        public override void PostAI()
        {
            if (forwardRotation)
                Projectile.rotation = Utils.ToRotation(Projectile.velocity) + 1.57079637f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int buffType = BuffID.ShadowFlame;

            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                buffType = cal.Find<ModBuff>("Nightwither").Type;
            }
            else if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
            {
                buffType = thorium.Find<ModBuff>("StrangeSkullDebuff").Type;
            }

            target.AddBuff(buffType, 300);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 0, default, 1f);
                Main.dust[dust].noGravity = true;
            }
        }
    }
}

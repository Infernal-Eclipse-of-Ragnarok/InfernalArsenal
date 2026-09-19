using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.DeusFlute
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class DeusFlutePro : BardProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.None}";

        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;
        public int ColorType
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }

        public override void SetBardDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }

        private bool spawned;
        public override void AI()
        {
            if (!spawned)
            {
                spawned = true;
            }

            if (Main.player[Projectile.owner].GetModPlayer<ThoriumPlayer>().accWindHoming)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy(Projectile.owner) && Vector2.DistanceSquared(npc.Center, Projectile.Center) < 600 * 600)
                    {
                        Vector2 vector = npc.Center - Projectile.Center;
                        float num4 = Projectile.velocity.Length();
                        vector.Normalize();
                        vector *= num4;
                        Projectile.velocity = (Projectile.velocity * 2 + vector) / 10f;
                        Projectile.velocity.Normalize();
                        Projectile.velocity *= num4;
                        break;
                    }
                }

            }

            int dustType = ColorType == 1 ? ModContent.DustType<CalamityMod.Dusts.AstralBlue>() : ModContent.DustType<CalamityMod.Dusts.AstralOrange>();
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.oldPos[i], Projectile.width, Projectile.height, dustType, Alpha: 0, Scale: Main.rand.NextFloat(0.4f, 1.2f));
                dust.noGravity = true;
                dust.noLight = false;
                dust.rotation += Main.rand.NextFloat(MathHelper.TwoPi);
                dust.velocity = default;
            }
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 spawnOffset = new(Main.rand.NextFloat(-100, 100), Main.rand.NextFloat(-400, -200));
            Vector2 position = target.Center + spawnOffset;
            Vector2 velocity = new Vector2(20, 0).RotatedBy(new Vector2(-spawnOffset.X, -spawnOffset.Y).ToRotation());
            Projectile.NewProjectileDirect(Projectile.GetSource_OnHit(target), position, velocity, ModContent.ProjectileType<DeusFluteStar>(), Projectile.damage, Projectile.knockBack, ai0: ColorType, ai1: target.whoAmI);
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
        }
    }
}
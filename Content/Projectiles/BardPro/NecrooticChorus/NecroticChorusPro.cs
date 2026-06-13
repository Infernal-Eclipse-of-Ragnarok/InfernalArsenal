using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.NecrooticChorus
{
    public class NecroticChorusPro : BardProjectile
    {

        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;
        public override string Texture => $"Terraria/Images/Item_{ItemID.None}";

        public override void SetStaticDefaults()
        {
        }

        public override void SetBardDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        private bool spawned;
        public override void AI()
        {
            if (!spawned)
            {
                spawned = true;
                for (int num161 = 0; num161 < 8; num161++)
                {
                    Dust obj11 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 100);
                    obj11.velocity = (Main.rand.NextFloatDirection() * (float)Math.PI).ToRotationVector2() * 2f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
                    obj11.scale = 0.65f;
                    obj11.fadeIn = 1.3f;
                    obj11.position = Projectile.Center;
                }
            }

            Projectile.alpha -= 20;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            for (int i = 1; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100);
                dust.velocity = dust.velocity * 0.5f + Projectile.velocity * 0.5f;
                dust.velocity *= 0.5f;
                dust.scale = (6 - i) * 0.3f;
                dust.noGravity = true;
                dust.position = Projectile.Center - Projectile.velocity * 2f * i;
            }

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 3f, DustID.RainbowMk2, Projectile.velocity * 0.5f, 0, Color.Red * (Main.rand.NextFloat() * 0.3f + 0.1f));
                dust.noGravity = true;
                dust.scale = 0.5f;
            }

            Lighting.AddLight(Projectile.Center, 0.22f, 0.035f, 0.035f);

            if (Main.player[Projectile.owner].GetModPlayer<ThoriumPlayer>().accWindHoming)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy(Projectile.owner) && Vector2.DistanceSquared(npc.Center, Projectile.Center) < 200 * 200)
                    {
                        Vector2 vector = npc.Center - Projectile.Center;
                        float num4 = Projectile.velocity.Length();
                        vector.Normalize();
                        vector *= num4;
                        Projectile.velocity = (Projectile.velocity * 19f + vector) / 20f;
                        Projectile.velocity.Normalize();
                        Projectile.velocity *= num4;
                        break;
                    }
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int type = ModContent.ProjectileType<NecroticChorusWisp>();
            foreach(Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == type && proj.owner == Main.myPlayer)
                {
                    proj.ai[0] = target.whoAmI;
                    proj.netUpdate = true;
                }
            }

            target.AddBuff(ModContent.BuffType<BurningBlood>(), 180);
            target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 180);
        }
    }
}
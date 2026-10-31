using InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Summons;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.MinionPro
{
    public class SusLookingEggPro : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Summoner/SuspiciousLookingEgg";

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 28;
            Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);

            // Egg shell / dirt-like particles
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width,  Projectile.height, DustID.Dirt,  Main.rand.NextFloat(0.8f, 1.8f), Main.rand.NextFloat(-2.5f, -0.5f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = Main.rand.NextBool(3);
                dust.velocity *= 1.5f;
            }

            // Smoke burst
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 0f), 100, default, Main.rand.NextFloat(0.8f, 1.3f));
                dust.noGravity = true;
            }

            // Broken shell gore
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 3; i++)
                {
                    int goreType = Main.rand.Next(61, 64);

                    Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), goreType, Main.rand.NextFloat(0.8f, 1.1f));
                    gore.velocity.Y -= 1.5f;
                }
            }

            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<CecropiaMothBuff>(), 2);
            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<CercropiaMoth>(), Projectile.damage, 6.5f, Main.myPlayer).originalDamage = Projectile.damage;
        }
    }
}

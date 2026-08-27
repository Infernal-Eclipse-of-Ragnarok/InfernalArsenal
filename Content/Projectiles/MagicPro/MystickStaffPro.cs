using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro
{
    public class MystickStaffPro : ModProjectile
    {
        private static readonly int[] dusts = new int[5]
        {
            59,
            60,
            61,
            62,
            6
        };

        private const float RotationOffset = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 52;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 350;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = Projectile.width / 2;
            height = Projectile.height / 2;
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Mystick Staff Debuffs
            target.AddBuff(ModContent.BuffType<Voidfrost>(), 300, false);
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300, false);
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 300, false);
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300, false);
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300, false);
            target.AddBuff(ModContent.BuffType<Laceration>(), 180, false);
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 120, false);
            target.AddBuff(ModContent.BuffType<WhisperingDeath>(), 120, false);
            target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 120, false);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 120, false);

            //Arckane Staff Debuffs
            target.AddBuff(BuffID.CursedInferno, 300, false);
            target.AddBuff(BuffID.Ichor, 120, false);
            target.AddBuff(ModContent.BuffType<Crumbling>(), 120, false);
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300, false);
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 300, false);
            target.AddBuff(ModContent.BuffType<Plague>(), 300, false);
            target.AddBuff(BuffID.ShadowFlame, 300, false);
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300, false);

            //Magick Staff Debuffs
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thor))
            {
                if (!target.boss) target.AddBuff(thor.Find<ModBuff>("Stunned").Type, 30, false);
                //target.AddBuff(thor.Find<ModBuff>("MagickStaffDebuff").Type, 300, false);
                target.AddBuff(thor.Find<ModBuff>("Charmed").Type, 180, false);
            }

            target.AddBuff(BuffID.Ichor, 120, false);
            target.AddBuff(BuffID.Confused, 90, false);
            target.AddBuff(BuffID.OnFire, 300, false);
            target.AddBuff(BuffID.Frostburn, 300, false);
            target.AddBuff(BuffID.Poisoned, 300, false);

            if (target.IsHostile())
            {
                Player player = Main.player[Projectile.owner];
                player.statLife += 10;
                if (player.statLife > player.statLifeMax2)
                    player.statLife = player.statLifeMax2;
                player.HealEffect(10, true);
            }
        }

        public override void AI()
        {
            int directionInt = Utils.ToDirectionInt(Projectile.velocity.X > 0.0);
            Projectile.rotation += directionInt * 0.35f;
            Projectile.spriteDirection = Projectile.direction = directionInt;

            if (!Utils.NextBool(Main.rand, 2)) return;
            for (int i = 0; i < dusts.Length; i++)
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dusts[i],
                    Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 50, default, 1.35f).noGravity = true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center, null);
            for (int index = 0; index < dusts.Length; ++index)
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dusts[index], oldVelocity.X * 0.2f, oldVelocity.Y * 0.2f, 100, Color.White, 1.25f);
            return true;
        }
    }
}

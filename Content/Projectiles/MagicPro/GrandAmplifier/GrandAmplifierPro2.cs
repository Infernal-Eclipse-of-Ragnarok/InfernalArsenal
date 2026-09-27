using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro.GrandAmplifier;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro.GrandAmplifier
{
    public class GrandAmplifierPro2 : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Assets/Textures/Empty";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 2;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            const float range = 400f;

            // Optional: Dust ring for visuals
            int points = 40;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.ToRadians(360f * i / points);
                Vector2 offset = angle.ToRotationVector2() * range;
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Electric);
                d.noGravity = true;
                d.velocity = Vector2.Zero;
                d.scale = 1.2f;
                d.alpha = 150;
            }

            bool spawnedAny = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active || !npc.CanBeChasedBy())
                    continue;

                Vector2 closestPoint = new Vector2(
                    MathHelper.Clamp(player.Center.X, npc.Hitbox.Left, npc.Hitbox.Right),
                    MathHelper.Clamp(player.Center.Y, npc.Hitbox.Top, npc.Hitbox.Bottom)
                );
                float distance = Vector2.Distance(player.Center, closestPoint);

                if (distance > range)
                    continue;

                var calNPC = npc.Calamity();

                bool hasSoulBurn = false;

                // Optional InfernalEclipseAPI check
                if (ModLoader.TryGetMod("InfernalEclipseAPI", out Mod infernalEclipseAPI))
                {
                    ModBuff soulBurn = infernalEclipseAPI.Find<ModBuff>("SoulBurn7");

                    if (soulBurn != null)
                        hasSoulBurn = npc.FindBuffIndex(soulBurn.Type) != -1;
                }

                if (calNPC.electrified == false && 
                    calNPC.staticDischarge == false && 
                    calNPC.vermillionFlux == false &&
                    calNPC.auricRebuke == false &&
                    calNPC.galvanicCorrosion == false &&
                    !hasSoulBurn)
                    continue;

                Vector2 spawnPos = npc.Center + new Vector2(0f, -150f);
                float debuffMultiplier = 1f;

                if (calNPC.auricRebuke)
                {
                    debuffMultiplier = 200;
                }
                else if (hasSoulBurn)
                {
                    debuffMultiplier = 50f;
                }
                else if (calNPC.vermillionFlux)
                {
                    debuffMultiplier = 40;
                }
                else if (calNPC.galvanicCorrosion)
                {
                    debuffMultiplier = 3;
                }
                else if (calNPC.electrified)
                {
                    debuffMultiplier = 1.5f;
                }
                else
                {
                    debuffMultiplier = 1f;
                }

                int proj2 = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<GrandAmplifierLightning>(),
                    (int)(player.HeldItem.damage * debuffMultiplier),
                    0f,
                    player.whoAmI,
                    npc.whoAmI
                );
            }

            if (spawnedAny)
            {
                SoundEngine.PlaySound(SoundID.Item92, player.Center);
            }

            Projectile.Kill();
        }
    }
}

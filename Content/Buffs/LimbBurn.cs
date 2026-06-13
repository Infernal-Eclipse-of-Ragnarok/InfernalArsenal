using CalamityMod.DataStructures;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Systems.Collections;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using InfernalEclipseWeaponsDLC.Core.GlobalNPCs;

namespace InfernalEclipseWeaponsDLC.Content.Buffs
{
    public class LimbBurn : ModBuff
    {
        public static DebuffData debuffData = new DebuffData()
        {
            EnemyLostRegen = 100f,
            HeatDebuffScaling = 1,
            MinimumDamageTickSize = 6,
            MultiplierDamageTickSize = 0,
            DrawAboveNPC = true
        };
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
            CalamityBuffSets.DebuffDataset[Type] = debuffData;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.runAcceleration *= 0.975f;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<WeaponsGlobalNPC>().limbBurn = true;

            if (Utils.NextBool(Main.rand, 4))
            {
                Vector2 position = npc.Center +
                    new Vector2(
                        Utils.NextFloat(Main.rand, -npc.width / 2f, npc.width / 2f),
                        Utils.NextFloat(Main.rand, -npc.height / 2f, npc.height / 2f)
                    );

                Vector2 Vect = Utils.RotatedByRandom(
                    new Vector2(0f, Utils.NextBool(Main.rand, 4) ? -5f : -9f),
                    MathHelper.ToRadians(25f)
                ) * Utils.NextFloat(Main.rand, 0.1f, 1.9f);

                GeneralParticleHandler.SpawnParticle(
                    new CritSpark(position, Vect,
                        Utils.NextBool(Main.rand) ? Color.Yellow : Color.Orange,
                        Color.Goldenrod,
                        0.8f, 15, 2f, 1.9f)
                );
            }

            if (Utils.NextBool(Main.rand, 4))
            {
                Vector2 val = npc.position - Vector2.One * 2f;
                Vector2 dustVel = npc.velocity + new Vector2(0f, Utils.NextFloat(Main.rand, -5f, -1f));

                Dust obj = Dust.NewDustDirect(val, npc.width + 4, npc.height + 4, DustID.GemTopaz,
                    dustVel.X, dustVel.Y, 0, default(Color), 1f);

                obj.noGravity = true;
                obj.scale = Utils.NextFloat(Main.rand, 0.7f, 1.2f);
                obj.alpha = 235;
            }

            Lighting.AddLight(npc.position, 0.25f, 0.25f, 0.1f);
        }
    }
}
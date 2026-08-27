using System;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.ID;

namespace InfernalEclipseWeaponsDLC.Content.Buffs
{
    public class TVRemotePaused : ModBuff
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Projectiles/OtherPro/AbsoluteTVRemotePauseIcon";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;

            for (int i = 0; i < NPCID.Count; i++)
            {
                NPCID.Sets.ImmuneToAllBuffs[i] = false;
            }
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<PausedGlobalNPC>().TimeFrozen = true;
        }
    }

    public class PausedGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool TimeFrozen;

        private float storedRotation;
private bool rotationStored;

        public override void ResetEffects(NPC npc)
        {
            TimeFrozen = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (TimeFrozen && npc.life <= 1)
            {
                npc.lifeRegen = 0;
            }
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (TimeFrozen && npc.life <= 1) return false;
            return true;
        }

        public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
        {
            if (TimeFrozen && npc.life <= 1) return false;
            return null;
        }

        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (TimeFrozen && npc.life <= 1) return false;
            return null;
        }

        public override bool CheckDead(NPC npc)
        {
            if (TimeFrozen && npc.life <= 1)
            {
                npc.life = 1;
                return false;
            }
            return true;
        }

        public override bool PreAI(NPC npc)
        {
            bool retval = base.PreAI(npc);

            if (TimeFrozen)
            {
                if (!rotationStored)
                {
                    storedRotation = npc.rotation;
                    rotationStored = true;
                }

                npc.position = npc.oldPosition;
                npc.velocity = Vector2.Zero;
                npc.frameCounter = 0;

                retval = false;
            }
            else
            {
                rotationStored = false;
            }

            return retval;
        }

        public override void PostAI(NPC npc)
        {
            if (TimeFrozen)
            {
                npc.position = npc.oldPosition;
                npc.velocity = Vector2.Zero;

                npc.frameCounter = 0;
                npc.rotation = storedRotation;
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!TimeFrozen)
                return;

            // =========================
            // PAUSE ICON ABOVE HEAD
            // =========================

            Texture2D pauseIcon = ModContent.Request<Texture2D>(
                "InfernalEclipseWeaponsDLC/Content/Projectiles/OtherPro/AbsoluteTVRemotePauseIcon"
            ).Value;

            Vector2 iconOrigin = pauseIcon.Size() / 2f;

            float time = Main.GlobalTimeWrappedHourly;

            // smooth floating motion
            float hover = (float)Math.Sin(time * 3f) * 3f;

            // subtle rotation (TV pause vibe)
            float rotation =
                (float)Math.Sin(time * 2f) * 0.06f +
                Main.rand.NextFloat(-0.01f, 0.01f);

            // position above NPC head
            Vector2 iconPos = npc.Top - screenPos + new Vector2(0f, -30f + hover);

            float alpha = 0.9f + Main.rand.NextFloat(0.1f);

            spriteBatch.Draw(
                pauseIcon,
                iconPos,
                null,
                Color.White * alpha,
                rotation,
                iconOrigin,
                0.8f,
                SpriteEffects.None,
                0f
            );
        }
    }

    public class TVRemotePlayerPaused : ModBuff
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Projectiles/OtherPro/AbsoluteTVRemotePauseIcon";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PausedPlayer>().TimeFrozen = true;
        }
    }

    public class PausedPlayer : ModPlayer
    {
        public bool TimeFrozen;

        public override void ResetEffects()
        {
            TimeFrozen = false;
        }

        public override void PreUpdateMovement()
        {
            if (TimeFrozen)
            {
                Player.velocity = Vector2.Zero;
                Player.position = Player.oldPosition;
            }
        }

        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {
            if (TimeFrozen)
            {
                Player.controlLeft = false;
                Player.controlRight = false;
                Player.controlJump = false;
                Player.controlUseItem = false;
            }
        }
    }
}

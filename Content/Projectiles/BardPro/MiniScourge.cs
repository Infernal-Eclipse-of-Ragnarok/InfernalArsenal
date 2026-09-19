using System;
using System.Collections.Generic;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    internal class MiniScourge : BardProjectile
    {
        List<Vector2> _segPositions = [];
        List<Vector2> _segVels = [];

        const int BOUNCE_MAX = 3;
        const int SEG_MAX = 5;
        const float SEG_DIST = 12f; // distance between segments
        const float SEG_DRAG = 0.92f; // how much segments resist movement
        const float SEG_GRAV_STRENGTH = 0.15f; // gravity effect when attached
        const float SEG_STIFFNESS_STRENGTH = 0.25f; // how much the worm resists bending when attached
        const float DETACH_VEL_MUL = 5f;
        const int ATTACK_TIME = 10;

        NPC previousVictim;
        NPC attachVictim;
        Vector2 attachOffset;

        float attachTime;
        float maxAttachTime;
        float attachCooldown;
        float maxAttachCooldown;

        int bounces;

        bool startKill;

        public override BardInstrumentType InstrumentType => BardInstrumentType.String;
        public override void SetBardDefaults()
        {
            Projectile.damage = 30;
            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.CritChance = 10;
            Projectile.friendly = true;
            Projectile.maxPenetrate = -1;

            // not sure if -1 works here
            Projectile.timeLeft = 99999;

            Projectile.alpha = 0;

            // exists until it bounces twice
            Projectile.penetrate = -1;

            maxAttachTime = 120;
            maxAttachCooldown = 30;

            for (int i = 0; i < SEG_MAX; i++)
            {
                _segPositions.Add(Projectile.Center);
                _segVels.Add(Vector2.Zero);
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.X -= 10;
            hitbox.Y -= 10;
        }
        public override void AI()
        {
            SegPhysUpdate();

            if (attachVictim != null)
            {
                Projectile.position = attachVictim.position - attachOffset;

                if (Projectile.timeLeft % ATTACK_TIME == 0)
                {
                    attachVictim.SimpleStrikeNPC(Projectile.damage, 0, Main.rand.NextBool(10), damageType: DamageClass.Ranged, damageVariation: true);
                }

                if (attachTime >= maxAttachTime || !attachVictim.active)
                {
                    attachTime = 0;
                    attachVictim = null;

                    Projectile.friendly = true;

                    // negative so it flies away from whence it came
                    Projectile.velocity = -Vector2.Normalize(attachOffset) * DETACH_VEL_MUL;
                }

                attachCooldown = maxAttachCooldown;
                attachTime++;
                return;
            }

            if (startKill)
            {
                Projectile.velocity *= 0.98f;

                Projectile.alpha += 5;

                if (Projectile.Opacity <= 0)
                    Projectile.Kill();
            }
            else
            {
                Projectile.velocity.Y += 0.1f;
            }

            if (attachCooldown > 0)
                attachCooldown--;
        }
        private void SegPhysUpdate()
        {
            _segPositions[0] = Projectile.Center;
            _segVels[0] = Projectile.velocity;

            // Update each segment to follow the one ahead
            for (int i = 1; i < _segPositions.Count; i++)
            {
                var currentPos = _segPositions[i];

                if (attachVictim != null)
                {
                    float gravityMultiplier = (float)i / (_segPositions.Count - 1); // 0 to 1 based on segment index
                    var gravity = new Vector2(0, SEG_GRAV_STRENGTH * gravityMultiplier);
                    _segVels[i] += gravity;

                    // fun fact: i asked an ai what this is called and it said "flexural stiffness," which is cool i guess
                    if (i >= 2)
                    {
                        var segA = _segPositions[i - 2];
                        var segB = _segPositions[i - 1];
                        var segC = _segPositions[i];

                        var diffA = segB - segA;
                        var diffB = segC - segB;

                        if (diffA.Length() > 0.1f && diffB.Length() > 0.1f)
                        {
                            diffA.Normalize();
                            diffB.Normalize();

                            var idealDirection = diffA;

                            // apply bending force with respect to stiffness
                            var bendForce = (idealDirection - diffB) * SEG_STIFFNESS_STRENGTH;

                            // try to bend
                            _segVels[i] += bendForce;
                        }
                    }
                }

                // simulate drag
                _segVels[i] *= SEG_DRAG;

                _segPositions[i] += _segVels[i];

                // force constraints
                var previousPos = _segPositions[i - 1];
                var connectionVector = _segPositions[i] - previousPos;
                float currentDistance = connectionVector.Length();

                // enact movement with respect to the constraints
                if (currentDistance > 0.1f)
                {
                    connectionVector.Normalize();
                    _segPositions[i] = previousPos + connectionVector * SEG_DIST;

                    var correctedVelocity = _segPositions[i] - currentPos;
                    _segVels[i] = correctedVelocity * 0.5f + _segVels[i] * 0.5f;
                }
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (attachVictim != null) return;
            if (attachCooldown > 0) return;
            if (target == previousVictim) return;

            //LaunchSkulls(Main.rand.Next(3, 5), -Projectile.velocity / 2);
            Projectile.friendly = false;
            attachVictim = target;
            previousVictim = attachVictim;
            attachOffset = target.position - Projectile.position;
        }
        public override void OnKill(int timeLeft)
        {
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounces++;

            if (bounces >= BOUNCE_MAX)
            {
                Projectile.tileCollide = false;
                startKill = true;

                SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.position);

                for (int i = 0; i < _segPositions.Count; i++)
                {
                    var curSegPos = _segPositions[i];

                    for (int j = 0; j < 10; j++)
                    {
                        Dust.NewDustDirect(curSegPos, 10, 10, DustID.Smoke, _segVels[i].X, _segVels[i].Y);
                    }
                }

                return false;
            }

            // spawn the "poisonous skull"
            LaunchSkulls(4 - bounces, oldVelocity / 2);

            // "normals"
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.5f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
            }

            return false;
        }

        private void LaunchSkulls(int count, Vector2 velocity)
        {
            // If velocity is zero, replace it with a normalized vector * MinimumVelocity
            if (velocity.LengthSquared() <= 0.0001f)
                velocity = Vector2.UnitY * 1f;

            for (int i = 0; i < count; i++)
            {
                int scourgeVenom = ModContent.ProjectileType<ScourgeVenomCloud>();

                Vector2 adjPos = Projectile.position - velocity;
                Vector2 adjVel = velocity.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.75f, 1.25f);

                var skull = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    adjPos,
                    -adjVel,
                    scourgeVenom,
                    26,
                    5,
                    Projectile.owner
                );

                skull.friendly = true;
                skull.hostile = false;
                skull.DamageType = ModContent.GetInstance<BardDamage>();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var projTex = TextureAssets.Projectile[Type].Value;

            Rectangle[] splits = [
                // head
                new(0, 0, projTex.Width, 29),
            // segs 1, 2, 3  
            new(0, 30, projTex.Width, 16),
            new(0, 47, projTex.Width, 16),
            new(0, 63, projTex.Width, 16),
            // tail
            new(0, 80, projTex.Width, 19)
            ];

            for (int i = 0; i < Math.Min(_segPositions.Count, splits.Length); i++)
            {
                Vector2 segmentPos = _segPositions[i];

                float rotation = 0f;

                // draw to face next segments
                if (i == 0)
                {
                    if (_segPositions.Count > 1)
                    {
                        Vector2 direction = _segPositions[1] - segmentPos;
                        if (direction.Length() > 0.1f)
                            // magical subtraction to have proper facing
                            rotation = direction.ToRotation() - MathHelper.PiOver2;
                    }
                    else if (_segVels[i].Length() > 0.1f)
                    {
                        rotation = _segVels[i].ToRotation() + MathHelper.PiOver2;
                    }
                }
                else
                {
                    Vector2 direction = _segPositions[i - 1] - segmentPos;
                    if (direction.Length() > 0.1f)
                        rotation = direction.ToRotation() + MathHelper.PiOver2;
                }

                Vector2 scale = Vector2.One;

                Vector2 screenPos = segmentPos - Main.screenPosition;

                Main.spriteBatch.Draw(
                    projTex,
                    screenPos,
                    splits[i],
                    lightColor * Projectile.Opacity,
                    rotation,
                    new Vector2(splits[i].Width / 2f, splits[i].Height / 2f),
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }

            return false;
        }
    }
}

using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Rogue
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class D20 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 23;
            Item.height = 23;
            Item.damage = 50;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 1);
            Item.shoot = ModContent.ProjectileType<D20Roller>();
            Item.shootSpeed = 12f;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
        }

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit = 5f;
        }

        // We override the Shoot method to manually hijack the Stealth Strike logic
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Check if Calamity says our stealth meter is full and ready
            bool isStealthStrike = player.Calamity().StealthStrikeAvailable();

            // Spawn the Roller projectile manually
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // If we have a stealth strike, FORCE the Roller to know about it
            if (isStealthStrike)
            {
                Main.projectile[proj].Calamity().stealthStrike = true; // Calamity's visual/damage flag
            }

            return false;
        }
    }

    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class D20Projectile : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Rogue/D20";

        public override void SetDefaults()
        {
            Projectile.width = 23;
            Projectile.height = 23;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Projectile.DamageType = calamity.Find<DamageClass>("RogueDamageClass");
            }
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f * Projectile.direction;
            int roll = (int)Projectile.ai[0];

            if (Projectile.localAI[1] == 0)
            {
                Projectile.localAI[1] = 1;

                // Dynamic Damage Scaling: 1 = 25% damage, 20 = 200% damage
                float damageMultiplier = 0.25f + (roll - 1) * (1.75f / 19f);
                Projectile.damage = (int)(Projectile.damage * damageMultiplier);

                if (roll == 1)
                {
                    SoundEngine.PlaySound(new SoundStyle("InfernalEclipseWeaponsDLC/Assets/Sounds/fart"), Projectile.position);
                }
                else if (roll >= 9 && roll <= 12)
                {
                    SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
                    NPC target = FindClosestNPC(1000f);
                    Vector2 targetCenter = target != null ? target.Center : Main.MouseWorld;

                    Vector2 direction = targetCenter - Projectile.Center;
                    direction.Normalize();

                    // Applies the new straight-line trajectory with 1.5x speed
                    float originalSpeed = Projectile.velocity.Length();
                    Projectile.velocity = direction * (originalSpeed * 1.5f);
                }
                else if (roll >= 13 && roll <= 16)
                {
                    Projectile.velocity *= 1.5f;
                    SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
                }
                else
                {
                    Projectile.velocity *= 2.2f;
                    SoundEngine.PlaySound(SoundID.Item117, Projectile.position);
                }
            }

            // --- CONSTANT VISUALS AND PHYSICS ---

            // 1: Complete dud. Instantly drops straight down with no forward momentum.
            if (roll == 1)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
                Projectile.velocity.X = 0f;
                Projectile.velocity.Y = 8f; // Plummets fast
            }

            /*
            // 2-4: Smoke screen & slow drop
            else if (roll >= 2 && roll <= 4)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
                Projectile.velocity.X *= 0.95f;
                Projectile.velocity.Y += 0.6f;
            }
            */

            // 5-8: Fire gravity
            else if (roll >= 2 && roll <= 8)
            {
                Projectile.velocity.Y += 0.2f;
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
            }

            // 9-12: Fast straight line with silver particle spray trail (Zero homing after frame 1)
            else if (roll >= 9 && roll <= 12)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SilverCoin, -Projectile.velocity.X * 0.5f, -Projectile.velocity.Y * 0.5f);
            }

            // 13-16: Fast, Subtle Gold Glow, Medium Homing
            else if (roll >= 13 && roll <= 16)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame);
                PerformHoming(600f, 12f);
            }

            // 17-19: Unfair Homing, Standard Fire Trail (NO gold glow)
            else if (roll >= 17 && roll != 20)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
                PerformHoming(1000f, 3f);
            }

            // 20: BLINDINGLY OBVIOUS GOLD, Unfair Homing
            else if (roll == 20)
            {
                Lighting.AddLight(Projectile.Center, 1f, 0.8f, 0.2f);

                Dust bigGold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame);
                bigGold.scale = 2.5f;
                bigGold.noGravity = true;

                PerformHoming(1000f, 3f);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int roll = (int)Projectile.ai[0];

            if (roll == 20)
            {
                modifiers.SetCrit();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int roll = (int)Projectile.ai[0];

            if ((roll >= 5 && roll <= 8) || (roll >= 17 && roll <= 20))
            {
                target.AddBuff(BuffID.OnFire, 300);
            }
        }

        public override void OnKill(int timeLeft)
        {
            int roll = (int)Projectile.ai[0];

            // 17-19: Standard, smaller explosion
            if (roll >= 17 && roll != 20)
            {
                SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, default, 1.5f);
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, default, 1.5f);
                }
            }

            // 20: MASSIVE apocalyptic explosion
            else if (roll == 20)
            {
                SoundEngine.PlaySound(SoundID.Item62, Projectile.position);

                for (int i = 0; i < 40; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 0, default, 2.5f);
                }
                for (int i = 0; i < 60; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.InfernoFork, Main.rand.NextFloat(-12f, 12f), Main.rand.NextFloat(-12f, 12f), 0, default, 3.5f);
                }
            }
        }

        private void PerformHoming(float maxDetectDistance, float turnResistance)
        {
            NPC closestNPC = FindClosestNPC(maxDetectDistance);

            if (closestNPC != null)
            {
                Vector2 direction = closestNPC.Center - Projectile.Center;
                direction.Normalize();

                float speed = Projectile.velocity.Length();
                if (speed < 10f) speed = 10f;

                Projectile.velocity = (Projectile.velocity * turnResistance + direction * speed) / (turnResistance + 1f);
            }
        }

        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                if (target.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }
            return closestNPC;
        }
    }

    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class D20Roller : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Rogue/D20";

        public override void SetDefaults()
        {
            Projectile.width = 23;
            Projectile.height = 23;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Projectile.DamageType = calamity.Find<DamageClass>("RogueDamageClass");
            }
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.ai[1] = Projectile.velocity.X;
            Projectile.localAI[0] = Projectile.velocity.Y;
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0] = Main.rand.Next(1, 21);
            Projectile.ai[2] = Main.rand.Next(1, 21);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center + new Vector2(0, -60f);
            Projectile.rotation += 0.4f;

            // Read the custom flag we hard-coded in D20.cs!
            bool isStealthStrike = Projectile.Calamity().stealthStrike;

            if (Projectile.timeLeft % 5 == 0)
            {
                Projectile.ai[0] = Main.rand.Next(1, 21);
                Projectile.ai[2] = Main.rand.Next(1, 21);
            }

            if (Projectile.timeLeft == 1)
            {
                int finalRoll;
                Vector2 savedVelocity = new(Projectile.ai[1], Projectile.localAI[0]);

                if (isStealthStrike)
                {
                    int roll1 = (int)Projectile.ai[0];
                    int roll2 = (int)Projectile.ai[2];
                    finalRoll = System.Math.Max(roll1, roll2);

                    if (finalRoll != 1 && finalRoll != 20)
                        finalRoll += (int)(player.luck * 10);

                    Color textColor = finalRoll switch
                    {
                        1 => Color.Gray,
                        20 => Color.Red,
                        _ => Color.Purple
                    };

                    CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y - 30, player.width, player.height), textColor, finalRoll.ToString() + " (Advantage!)", true);

                    Vector2 vel1 = savedVelocity.RotatedBy(MathHelper.ToRadians(8));

                    int proj1 = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel1, ModContent.ProjectileType<D20Projectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner, finalRoll);

                    Main.projectile[proj1].Calamity().stealthStrike = true;
                }
                else
                {
                    finalRoll = (int)Projectile.ai[0];

                    if (finalRoll != 1 && finalRoll != 20)
                        finalRoll += (int)(player.luck * 10);

                    Color textColor = finalRoll switch
                    {
                        1 => Color.Gray,
                        20 => Color.Red,
                        _ => Color.Gold
                    };

                    CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y - 30, player.width, player.height), textColor, finalRoll.ToString(), true);

                    Vector2 direction = Main.MouseWorld - Projectile.Center;
                    direction.Normalize();

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * savedVelocity.Length(), ModContent.ProjectileType<D20Projectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner, finalRoll);
                }
            }
        }

        public override void PostDraw(Color lightColor)
        {
            string text = Projectile.ai[0].ToString();
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(-15, -15);

            Color textColor = Projectile.Calamity().stealthStrike ? Color.MediumPurple : Color.White;
            Utils.DrawBorderString(Main.spriteBatch, text, drawPos, textColor, 1.3f);

            if (Projectile.Calamity().stealthStrike)
            {
                string advtext = Projectile.ai[2].ToString();
                Vector2 advdrawPos = Projectile.Center - Main.screenPosition + new Vector2(-15, -35);

                Utils.DrawBorderString(Main.spriteBatch, advtext, advdrawPos, textColor, 1.3f);
            }
        }
    }
}

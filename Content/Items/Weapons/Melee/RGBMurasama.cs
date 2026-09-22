using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro;
using InfernalEclipseWeaponsDLC.Core;
using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee
{
    public class RGBMurasama : ModItem
    {
        private static Mod Calamity => ModIntegrationsSystem.Calamity.Loaded ? ModIntegrationsSystem.Calamity.Mod : null;
        public int frameCounter = 0;
        public int frame = 0;
        public bool IDUnlocked(Player player) => Calamity != null ? CalamityHelper.DownedDoG : NPC.downedMoonlord;

        public static readonly SoundStyle OrganicHit = new("InfernalEclipseWeaponsDLC/Assets/Sounds/MurasamaHitOrganic") { Volume = 0.45f };
        public static readonly SoundStyle InorganicHit = new("InfernalEclipseWeaponsDLC/Assets/Sounds/MurasamaHitInorganic") { Volume = 0.55f };
        public static readonly SoundStyle Swing = new("InfernalEclipseWeaponsDLC/Assets/Sounds/MurasamaSwing") { Volume = 0.2f };
        public static readonly SoundStyle BigSwing = new("InfernalEclipseWeaponsDLC/Assets/Sounds/MurasamaBigSwing") { Volume = 0.25f };

        private string GetHashCodeNew(Color color)
        {
            string[] list = new string[16]
            {
                "0", "1", "2", "3",
                "4", "5", "6", "7",
                "8", "9", "A", "B",
                "C", "D", "E", "F"
            };
            int[] list2 = new int[3] { color.R, color.G, color.B };
            string hash = "";
            foreach (int i in list2)
            {
                hash += list[i / 16] + list[i % 16];
            }
            //Main.NewText(hash);
            return hash;
        }

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(2, 13));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 90;
            Item.height = 134;
            Item.damage = 2322; // keep the last 2 numbers "22"
            Item.DamageType = Calamity != null ? Calamity.Find<DamageClass>("TrueMeleeNoSpeedDamageClass") : DamageClass.Melee;
            Item.crit = 61;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 5;
            Item.knockBack = 7.22f;
            Item.autoReuse = false;
            Item.value = Item.buyPrice(2);
            Item.shoot = ModContent.ProjectileType<RGBMurasamaSlash>();
            Item.shootSpeed = 24f;
            Item.rare = Calamity != null ? Calamity.Find<ModRarity>("CosmicPurple").Type : ItemRarityID.Purple;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture;

            if (IDUnlocked(Main.LocalPlayer))
            {
                //0 = 6 frames, 8 = 3 frames]
                texture = ModContent.Request<Texture2D>(Texture).Value;
                spriteBatch.Draw(texture, position, Item.GetCurrentFrame(ref frame, ref frameCounter, 2, 13), Color.White, 0f, origin, scale, SpriteEffects.None, 0);
                texture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/RGBMurasama_Glow").Value;
                spriteBatch.Draw(texture, position, Item.GetCurrentFrame(ref frame, ref frameCounter, 2, 13), Color.Lerp(Main.DiscoColor, Color.White, 0.25f), 0f, origin, scale, SpriteEffects.None, 0);
            }
            else
            {
                texture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/RGBMurasamaSheathed").Value;
                spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0);
            }

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture;

            if (IDUnlocked(Main.LocalPlayer))
            {
                texture = ModContent.Request<Texture2D>(Texture).Value;
                spriteBatch.Draw(texture, Item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 2, 13), lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            }
            else
            {
                texture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/RGBMurasamaSheathed").Value;
                spriteBatch.Draw(texture, Item.position - Main.screenPosition, null, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            }
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (!IDUnlocked(Main.LocalPlayer))
                return;
            Texture2D texture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/RGBMurasama_Glow").Value;
            spriteBatch.Draw(texture, Item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 2, 13, false), Color.Lerp(Main.DiscoColor, Color.White, 0.25f), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] > 0)
                return false;
            return IDUnlocked(player);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int index = tooltips.FindLastIndex(tt => tt.Mod.Equals("Terraria") && tt.Name.Equals("Tooltip3"));
            if (index != -1)
            {
                tooltips[index].Text = "[c/" + GetHashCodeNew(Main.DiscoColor) + ":" + tooltips[index].Text + "]";
            }
        }

        public override void AddRecipes()
        {
            if (Calamity != null)
            {
                CreateRecipe()
                    .AddIngredient(Calamity.Find<ModItem>("Murasama"))
                    .AddIngredient(Calamity.Find<ModItem>("MiracleMatter"))
                    .AddIngredient(ItemID.RainbowBrick, 50)
                    .AddTile(Calamity.Find<ModTile>("DraedonsForge"))
                    .Register();
            }
            else
            {
                CreateRecipe()
                    .AddIngredient(ItemID.Muramasa)
                    .AddIngredient(ItemID.LunarBar, 10)
                    .AddIngredient(ItemID.MartianConduitPlating, 8) //memories
                    .AddIngredient(ItemID.BrokenHeroSword) //broken
                    .AddIngredient(ItemID.PaintingTheTruthIsUpThere) //the truth goes unspoken
                    //.AddIngredient(ItemID.) //i've even forgotten my name
                    .AddIngredient(ItemID.PaintingTheSeason) //i don't know the season or what is the reason
                    .AddIngredient(ItemID.BladeofGrass) //i'm standing here holding my blade
                    //.AddIngredient(ItemID.) //a desolate
                    .AddIngredient(ItemID.PlacePainting) //place, without any trace
                    .AddIngredient(ItemID.PaintingColdSnap) //it's only the cold
                    .AddIngredient(ItemID.AnkletoftheWind) //wind I feel
                    //.AddIngredient(ItemID.Spite) //it's me that i spite
                    .AddIngredient(ItemID.DD2ElderCrystalStand) //as i stand up and fight
                    .AddIngredient(ItemID.BloodButcherer) //the only thing i know for real, there will be blood
                    .AddIngredient(ItemID.RainbowBrick, 50)
                    .AddTile(TileID.BloodMoonMonolith)
                    .Register();
            }
        }
    }
}

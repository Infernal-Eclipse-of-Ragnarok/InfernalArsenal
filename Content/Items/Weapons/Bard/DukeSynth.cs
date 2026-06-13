using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.DukeSynth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    public class DukeSynth : BardItem
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Bard/DukeSynthShoot";
        public override bool IsLoadingEnabled(Mod mod) => true;
        public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<EmpowermentProlongation>(2);
            Empowerments.AddInfo<Damage>(3);
            Empowerments.AddInfo<FlatDamage>(2);
            //"Viral Wisdom"
        }

        public override void SetBardDefaults()
        {
            Item.damage = 220;
            InspirationCost = 1;
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 20f;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.UseSound = SoundID.Zombie53;
            Item.shoot = ModContent.ProjectileType<SulfurSpirit>();
            Item.shootSpeed = 10f;

            if (!ModLoader.HasMod("Look"))
            {
                ((ModItem)this).Item.holdStyle = 3;
            }
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float spread = MathHelper.ToRadians(10);
            for (int i = 0; i < 3; i++)
            {
                Vector2 newVelocity = velocity.RotatedBy(spread * (i - 1));
                Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        //TODO: IDK why this isn't working but I want too make the item appear more in the players hands and use the other sprite in the inventory and on the ground in the world.
        public override Vector2? HoldoutOrigin()
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
            return new Vector2(texture.Width / 2f, texture.Height / 2f);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(5, 0);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += new Vector2(0, 0f) * player.Directions;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D uprightTex = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Bard/DukeSynth").Value;
            spriteBatch.Draw(uprightTex, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false; // Prevent default draw
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D uprightTex = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Bard/DukeSynth").Value;
            Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height - uprightTex.Height / 2);
            spriteBatch.Draw(uprightTex, position, null, lightColor, rotation, uprightTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            return false; // Prevent default draw
        }
    }
}

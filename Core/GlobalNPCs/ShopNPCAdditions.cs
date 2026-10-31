using InfernalEclipseWeaponsDLC.Content.Items.Materials;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Ranged;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Rogue;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.GlobalNPCs
{
    public class ShopNPCAdditions : GlobalNPC
    {
        private static readonly string[] TravelingMerchantNames =
        [
            "Baro Ki'Teer",
        ];

        private void AddNewNames(List<string> nameList, string[] patreonNames)
        {
            if (patreonNames is null || patreonNames.Length == 0)
            {
                return;
            }
            for (int i = 0; i < patreonNames.Length; i++)
            {
                nameList.Add(patreonNames[i]);
            }
        }

        public override void ModifyNPCNameList(NPC npc, List<string> nameList)
        {
            switch (npc.type)
            {
                case NPCID.TravellingMerchant:
                    AddNewNames(nameList, TravelingMerchantNames);
                    break;
                default:
                    break;
            }
        }

        public override void ModifyShop(NPCShop shop)
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                if (shop.NpcType == cal.Find<ModNPC>("Archmage").Type)
                {
                    shop.Add(new NPCShop.Entry(ModContent.ItemType<DeepSeaDrawlShard3>(), Condition.DownedGolem));
                }
            }
        }

        public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
        {
            if (npc.type == NPCID.TravellingMerchant)
            {
                if (NPC.downedHalloweenKing)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] == null || items[i].IsAir)
                        {
                            items[i] = new Item(ModContent.ItemType<PrismaGorgon>());
                            break;
                        }
                    }
                }

                if (NPC.downedGolemBoss && !ModLoader.HasMod("CalamityMod"))
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] == null || items[i].IsAir)
                        {
                            items[i] = new Item(ModContent.ItemType<DeepSeaDrawlShard3>());
                            break;
                        }
                    }
                }

                if (Main.rand.Next(100) < 10)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] == null || items[i].IsAir)
                        {
                            items[i] = new Item(ModContent.ItemType<D20>());
                            break;
                        }
                    }
                }
            }
        }
    }
}
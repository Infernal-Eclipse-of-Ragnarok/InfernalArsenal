using Terraria.GameContent.ItemDropRules;
using Terraria;
using Terraria.ModLoader;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard;
using CalamityMod.Items.TreasureBags;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Rogue;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Magic;
using CalamityMod.Items.Fishing.SulphurCatches;
using CalamityMod;
using ThoriumMod.Items.Depths;
using InfernalEclipseWeaponsDLC.Content.Items.Materials;
using Terraria.ID;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Hybrid;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Magic;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Melee.Scythes;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Melee;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner.Whips;

namespace InfernalEclipseWeaponsDLC.Core.GlobalItems
{
    public class TreasureBagDropChanges : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (item.type == ItemID.WallOfFleshBossBag)
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ForeverHungry>(), 3));
            }

            if (item.type == ItemID.WallOfFleshBossBag)
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<BottleOfSouls>(), 3));
            }

            if (ModLoader.TryGetMod("CalamityMod", out _))
            {
                if (item.type == ModContent.ItemType<DesertScourgeBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SandSlasher>(), 3));
                }

                if (item.type == ModContent.ItemType<AquaticScourgeBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SulphuricShanty>(), 7));
                }

                if (item.type == ModContent.ItemType<BrimstoneElementalBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<BrimstoneHarp>(), 3));
                }

                if (item.type == ModContent.ItemType<AstrumDeusBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeusFlute>(), 3));
                }

                if (item.type == ModContent.ItemType<RavagerBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<NecroticChorus>(), 3));
                }

                if (item.type == ModContent.ItemType<SignusBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<TheParallel>(), 3));
                }

                if (item.type == ModContent.ItemType<DragonfollyBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<BirbSaxophone>(), 3));
                }

                if (item.type == ModContent.ItemType<DevourerofGodsBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeathsWhisper>(), 3));
                }

                if (item.type == ModContent.ItemType<OldDukeBag>())
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorrodedCane>(), 4));
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DukeSynth>(), 4));
                }


                if (item.type == ModContent.ItemType<SulphurousCrate>() || item.type == ModContent.ItemType<HydrothermalCrate>())
                {
                    LeadingConditionRule mainRule = itemLoot.DefineConditionalDropSet(() => DownedBossSystem.downedSlimeGod || Main.hardMode);
                    mainRule.Add(new OneFromOptionsDropRule(6, 1, ModContent.ItemType<DeepSeaDrawl>(), ModContent.ItemType<DeepseaTrident>()));
                }

                if (item.type == ModContent.ItemType<AquaticDepthsCrate>() || item.type == ModContent.ItemType<AbyssalCrate>())
                {
                    LeadingConditionRule mainRule = itemLoot.DefineConditionalDropSet(() => DownedBossSystem.downedCryogen && NPC.downedGolemBoss);
                    mainRule.Add(new OneFromOptionsDropRule(20, 1, ModContent.ItemType<DeepSeaDrawlShard1>()));
                }
            }

            if (ModLoader.TryGetMod("Consolaria", out Mod console))
            {
                if (item.type == console.Find<ModItem>("OcramBag").Type)
                {
                    //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<OcramKnife>(), 4));
                    //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<TheBlight>(), 4));
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Legacy>(), 4));
                }
            }

            if (ModLoader.TryGetMod("SOTS", out Mod sots))
            {
                if (item.type == sots.Find<ModItem>("GlowmothBag").Type)
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<MothwingDagger>(), 5));
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<IlluminantCross>(), 5));
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<GlowstringBiwa>(), 5));
                }
            }

            if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
            {
                if (item.type == thorium.Find<ModItem>("TheGrandThunderBirdTreasureBag").Type)
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<GrandThunderWhip>(), 4));
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StormCarver>(), 4));
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<GrandAmplifier>(), 4));
                }

                if (item.type == thorium.Find<ModItem>("LichTreasureBag").Type)
                {
                    var rule = new CommonDropNotScalingWithLuck(ModContent.ItemType<ListoftheDamned>(), 5, 1, 1)
                    {
                        chanceNumerator = 2 // 2/5 = 40%
                    };
                    itemLoot.Add(rule);
                }
            }

            if (ModLoader.TryGetMod("FargowiltasSouls", out Mod souls) && !ModLoader.TryGetMod("YharimEX", out _))
            {
                if (item.type == souls.Find<ModItem>("MutantBag").Type)
                {
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<AuricBrimfireCrosier>(), 1));
                }
            }
        }
    }
}

using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace SaneInventory
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [HarmonyPatch]
    public class SaneInventory : BaseUnityPlugin
    {
        public const string GUID = "org.remmiz.plugins.saneinventory";
        public const string NAME = "SaneInventory";
        public const string VERSION = "1.0.0";

        public static ManualLogSource logger;

        private static ConfigEntry<bool> itemUnlimitedStackEnabledConfig;

        private static ConfigEntry<bool> shipKarveCargoIncreaseEnabledConfig;
        private static ConfigEntry<int> shipKarveCargoIncreaseColumnsConfig;
        private static ConfigEntry<int> shipKarveCargoIncreaseRowsConfig;

        private static ConfigEntry<bool> containerWeightLimitEnabledConfig;
        private static ConfigEntry<int> shipKarveCargoWeightLimitConfig;
        private static ConfigEntry<int> shipLongboatCargoWeightLimitConfig;
        private static ConfigEntry<int> woodChestWeightLimitConfig;
        private static ConfigEntry<int> personalChestWeightLimitConfig;
        private static ConfigEntry<int> reinforcedChestWeightLimitConfig;
        private static ConfigEntry<int> blackMetalChestWeightLimitConfig;

        void Awake()
        {
            logger = Logger;

            bindConfig();

            logger.LogInfo(NAME + " - Patching in");
            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        static void UpdateMaxItemStack(ref ObjectDB __instance)
        {
            if (itemUnlimitedStackEnabledConfig.Value)
            {
                foreach (ItemDrop.ItemData.ItemType type in (ItemDrop.ItemData.ItemType[])Enum.GetValues(typeof(ItemDrop.ItemData.ItemType)))
                {
                    foreach (ItemDrop item in __instance.GetAllItems(type, ""))
                    {
                        if (item.m_itemData.m_shared.m_name.StartsWith("$item_") && item.m_itemData.m_shared.m_maxStackSize > 1)
                            item.m_itemData.m_shared.m_maxStackSize = int.MaxValue;
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ship), "Awake")]
        static void UpdateShipCargoSize(ref Ship __instance)
        {
            if (shipKarveCargoIncreaseEnabledConfig.Value)
            {
                if (__instance.name.ToLower().Contains("karve"))
                {
                    Container container = __instance.gameObject.transform.GetComponentInChildren<Container>();
                    if (container != null)
                    {
                        logger.LogInfo(NAME + " - Updating Karve cargo size");
                        container.m_width = Math.Min(shipKarveCargoIncreaseColumnsConfig.Value, 6);
                        container.m_height = Math.Min(shipKarveCargoIncreaseRowsConfig.Value, 3);
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
        static void HideMaxStackSizeInventoryGrid(ref InventoryGrid __instance, ref Inventory ___m_inventory)
        {
            if (itemUnlimitedStackEnabledConfig.Value)
            {
                int width = ___m_inventory.GetWidth();

                foreach (ItemDrop.ItemData allItem in ___m_inventory.GetAllItems())
                {
                    if (allItem.m_shared.m_maxStackSize > 1)
                    {
                        InventoryGrid.Element e = __instance.GetElement(allItem.m_gridPos.x, allItem.m_gridPos.y, width);
                        e.m_amount.text = allItem.m_stack.ToString();
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
        static void HideMaxStackSizeHotkeyBar(ref HotkeyBar __instance, ref List<ItemDrop.ItemData> ___m_items, ref List<HotkeyBar.ElementData> ___m_elements)
        {
            if (itemUnlimitedStackEnabledConfig.Value)
            {
                for (int j = 0; j < ___m_items.Count; j++)
                {
                    ItemDrop.ItemData itemData = ___m_items[j];
                    HotkeyBar.ElementData elementData2 = ___m_elements[itemData.m_gridPos.x];

                    if (itemData.m_shared.m_maxStackSize > 1)
                    {
                        elementData2.m_amount.text = itemData.m_stack.ToString();
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryGui), "UpdateContainerWeight")]
        static void DisplayContainerMaxWeight(ref InventoryGui __instance, ref Container ___m_currentContainer)
        {
            if (containerWeightLimitEnabledConfig.Value)
            {
                if (___m_currentContainer == null || !__instance.m_currentContainer.transform.parent)
                    return;

                float maxWeight;

                //if (!__instance.m_currentContainer.transform.parent)
                //    maxWeight = getContainerMaxWeight(___m_currentContainer.m_inventory.m_name);
                //else
                    maxWeight = getShipMaxWeight(___m_currentContainer.transform.parent.name);

                if (maxWeight > 0)
                {
                    int totalWeight = Mathf.CeilToInt(___m_currentContainer.GetInventory().GetTotalWeight());

                    if (totalWeight > maxWeight && (double)Mathf.Sin(Time.time * 10f) > 0.0)
                        __instance.m_containerWeight.text = "<color=red>" + totalWeight.ToString() + "</color>/" + maxWeight.ToString();
                    else
                        __instance.m_containerWeight.text = totalWeight.ToString() + "/" + maxWeight.ToString();
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ship), "FixedUpdate")]
        static void ApplyShipWeightForce(ref Ship __instance, ref Rigidbody ___m_body)
        {
            // TODO: Add drag to ship if overweight
            if (containerWeightLimitEnabledConfig.Value)
            {
                Container container = __instance.gameObject.transform.GetComponentInChildren<Container>();
                if (container != null)
                {
                    float maxWeight = getShipMaxWeight(__instance.name);

                    float containerWeight = container.GetInventory().GetTotalWeight();

                    if (containerWeight > maxWeight)
                    {
                        float weightForce = (containerWeight - maxWeight) / maxWeight;
                        ___m_body.AddForceAtPosition(Vector3.down * weightForce * 5, ___m_body.worldCenterOfMass, (ForceMode)2);
                    }
                }
            }
        }

        static int getContainerMaxWeight(string name)
        {
            switch (name)
            {
                case "$piece_chestwood":
                    return woodChestWeightLimitConfig.Value;
                case "$piece_chest":
                    return reinforcedChestWeightLimitConfig.Value;
                case "$piece_chestprivate":
                    return personalChestWeightLimitConfig.Value;
                case "$piece_chestblackmetal":
                    return blackMetalChestWeightLimitConfig.Value;
            }

            return 0;
        }

        static float getShipMaxWeight(string name)
        {
            if (name.ToLower().Contains("karve"))
                return shipKarveCargoWeightLimitConfig.Value;
            if (name.ToLower().Contains("vikingship"))
                return shipLongboatCargoWeightLimitConfig.Value;

            return 0;
        }

        private void bindConfig()
        {
            itemUnlimitedStackEnabledConfig = Config.Bind(NAME + ".ItemUnlimitedStack", "enabled", true,
                new ConfigDescription(
                    "Should item stack size limit be removed?",
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }
                )
            );

            shipKarveCargoIncreaseEnabledConfig = Config.Bind(NAME + ".ShipKarveCargoIncrease", "enabled", true,
                new ConfigDescription(
                    "Should Karve cargo hold size be increased?",
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }
                )
            );
            shipKarveCargoIncreaseColumnsConfig = Config.Bind(NAME + ".ShipKarveCargoIncrease", "karve_cargo_width", 3,
                "Number of columns for the Karve cargo hold. Max of 6."
            );
            shipKarveCargoIncreaseRowsConfig = Config.Bind(NAME + ".ShipKarveCargoIncrease", "karve_cargo_height", 2,
                "Number of rows for the Karve cargo hold. Max of 3."
            );

            containerWeightLimitEnabledConfig = Config.Bind(NAME + ".ContainerWeightLimit", "enabled", true,
                new ConfigDescription(
                    "Should containers have weight limits?",
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }
                )
            );
            shipKarveCargoWeightLimitConfig = Config.Bind(NAME + ".ContainerWeightLimit", "karve_weight_limit", 1000,
                "Weight limit for the Karve"
            );
            shipLongboatCargoWeightLimitConfig = Config.Bind(NAME + ".ContainerWeightLimit", "longship_weight_limit", 4000,
                "Weight limit for the Longship"
            );
            woodChestWeightLimitConfig = Config.Bind(NAME + ".ContainerWeightLimit", "woodchest_weight_limit", 1000,
                "Weight limit for the Wood Chest"
            );
            personalChestWeightLimitConfig = Config.Bind(NAME + ".ContainerWeightLimit", "personalchest_weight_limit", 1000,
                "Weight limit for the Personal Chest"
            );
            reinforcedChestWeightLimitConfig = Config.Bind(NAME + ".ContainerWeightLimit", "reinforcedchest_weight_limit", 2000,
                "Weight limit for the Reinforced Chest"
            );
            blackMetalChestWeightLimitConfig = Config.Bind(NAME + ".ContainerWeightLimit", "blackmetalchest_weight_limit", 4000,
                "Weight limit for the Black Metal Chest"
            );
        }
    }

}
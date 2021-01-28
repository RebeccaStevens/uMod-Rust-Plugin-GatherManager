using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{

    [Info("Gathering Manager", "Mughisi", "2.2.75", ResourceId = 675)]
    class GatherManager : RustPlugin
    {
        #region Configuration Data
        // Do not modify these values because this will not change anything, the values listed below are only used to create
        // the initial configuration file. If you wish changes to the configuration file you should edit 'GatherManager.json'
        // which is located in your server's config folder: <drive>:\...\server\<your_server_identity>\oxide\config\

        private bool configChanged;

        // Plugin settings
        private const string DefaultChatPrefix = "Gather Manager";
        private const string DefaultChatPrefixColor = "#008000ff";

        public string ChatPrefix { get; private set; }
        public string ChatPrefixColor { get; private set; }

        // Plugin options
        private static readonly Dictionary<string, object> DefaultGatherResourceModifiers = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> DefaultGatherDispenserModifiers = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> DefaultQuarryResourceModifiers = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> DefaultPickupResourceModifiers = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> DefaultSurveyResourceModifiers = new Dictionary<string, object>();

        // Defaults
        private const float DefaultMiningQuarryResourceTickRate = 5f;
        private const int DefaultQuarryInventoryOutputSlots = 18;
        private const float DefaultExcavatorResourceTickRate = 3f;
        private const float DefaultExcavatorTimeForFullResources = 120f;
        private const int DefaultExcavatorInventoryOutputSlots = 36;
        private const float DefaultExcavatorRunningTimePerFuelUnit = 120f;

        public Dictionary<string, float> GatherResourceModifiers { get; private set; }
        public Dictionary<string, float> GatherDispenserModifiers { get; private set; }
        public Dictionary<string, float> MiningQuarryStaticResourceModifiers { get; private set; }
        public Dictionary<string, float> MiningQuarryDynamicResourceModifiers { get; private set; }
        public Dictionary<string, float> PumpJackStaticResourceModifiers { get; private set; }
        public Dictionary<string, float> PumpJackDynamicResourceModifiers { get; private set; }
        public Dictionary<string, float> ExcavatorResourceModifiers { get; private set; }
        public Dictionary<string, float> PickupResourceModifiers { get; private set; }
        public Dictionary<string, float> SurveyResourceModifiers { get; private set; }
        public float MiningQuarryStaticResourceTickRate { get; private set; }
        public float MiningQuarryDynamicResourceTickRate { get; private set; }
        public float PumpJackStaticResourceTickRate { get; private set; }
        public float PumpJackDynamicResourceTickRate { get; private set; }
        public int MiningQuarryStaticInventoryOutputSlots { get; private set; }
        public int MiningQuarryDynamicInventoryOutputSlots { get; private set; }
        public int PumpJackStaticInventoryOutputSlots { get; private set; }
        public int PumpJackDynamicInventoryOutputSlots { get; private set; }
        public float ExcavatorResourceTickRate { get; private set; }
        public float ExcavatorTimeForFullResources { get; private set; }
        public int ExcavatorInventoryOutputSlots { get; private set; }
        public float ExcavatorRunningTimePerFuelUnit { get; private set; }

        // Plugin messages
        private const string DefaultNotAllowed = "You don't have permission to use this command.";
        private const string DefaultInvalidArgumentsGather =
            "Invalid arguments supplied! Use gather.rate <type:dispenser|pickup|quarry|pumpjack|excavator|survey> <resource> <multiplier>";
        private const string DefaultInvalidArgumentsDispenser =
            "Invalid arguments supplied! Use dispenser.scale <dispenser:tree|ore|corpse> <multiplier>";
        private const string DefaultInvalidArgumentsSpeed =
            "Invalid arguments supplied! Use quarry.rate <time between gathers in seconds>";
        private const string DefaultInvalidModifier =
            "Invalid modifier supplied! The new modifier always needs to be bigger than 0!";
        private const string DefaultInvalidSpeed = "You can't set the speed lower than 1 second!";
        private const string DefaultModifyResource = "You have set the gather rate for {0} to x{1} from {2}.";
        private const string DefaultModifyResource2 = "You have set the gather rate for {0} to x{1} from {2} & {3}.";
        private const string DefaultModifyResourceRemove = "You have reset the gather rate for {0} from {1}.";
        private const string DefaultModifyResourceRemove2 = "You have reset the gather rate for {0} from {1} & {2}.";
        private const string DefaultModifySpeed = "The Mining Quarry will now provide resources every {0} seconds.";
        private const string DefaultInvalidResource =
            "{0} is not a valid resource. Check gather.resources for a list of available options.";
        private const string DefaultModifyDispenser = "You have set the resource amount for {0} dispensers to x{1}";
        private const string DefaultInvalidDispenser =
            "{0} is not a valid dispenser. Check gather.dispensers for a list of available options.";

        private const string DefaultDispensers = "Resource Dispensers";
        private const string DefaultCharges = "Survey Charges";
        private const string DefaultStaticQuarries = "Monument Mining Quarries";
        private const string DefaultDynamicQuarries = "Player Mining Quarries";
        private const string DefaultStaticPumpJacks = "Monument Pump Jacks";
        private const string DefaultDynamicPumpJacks = "Player Pump Jacks";
        private const string DefaultExcavators = "Excavators";
        private const string DefaultPickups = "pickups";

        private const string DefaultHelpText = "/gather - Shows you detailed gather information.";
        private const string DefaultHelpTextPlayer = "Resources gained from gathering have been scaled to the following:";
        private const string DefaultHelpTextAdmin = "To change the resources gained by gathering use the command:\r\ngather.rate <type:dispenser|pickup|quarry|pumpjack|excavator|survey> <resource> <multiplier>\r\nTo change the amount of resources in a dispenser type use the command:\r\ndispenser.scale <dispenser:tree|ore|corpse> <multiplier>\r\nTo change the time between Mining Quarry gathers:\r\nquarry.tickrate <seconds>";
        private const string DefaultHelpTextPlayerGains = "Resources gained from {0}:";
        private const string DefaultHelpTextPlayerStaticMiningQuarrySpeed = "Time between Monument Mining Quarry gathers: {0} second(s).";
        private const string DefaultHelpTextPlayerDynamicMiningQuarrySpeed = "Time between Player Mining Quarry gathers: {0} second(s).";
        private const string DefaultHelpTextPlayerStaticPumpJackSpeed = "Time between Monument Pump Jack gathers: {0} second(s).";
        private const string DefaultHelpTextPlayerDynamicPumpJackSpeed = "Time between Player Pump Jack gathers: {0} second(s).";
        private const string DefaultHelpTextPlayerExcavatorSpeed = "Excavator gathers speed factor: {0}.";
        private const string DefaultHelpTextPlayerDefault = "Default values.";

        public string NotAllowed { get; private set; }
        public string InvalidArgumentsGather { get; private set; }
        public string InvalidArgumentsDispenser { get; private set; }
        public string InvalidArgumentsSpeed { get; private set; }
        public string InvalidModifier { get; private set; }
        public string InvalidSpeed { get; private set; }
        public string ModifyResource { get; private set; }
        public string ModifyResource2 { get; private set; }
        public string ModifyResourceRemove { get; private set; }
        public string ModifyResourceRemove2 { get; private set; }
        public string ModifySpeed { get; private set; }
        public string InvalidResource { get; private set; }
        public string ModifyDispenser { get; private set; }
        public string InvalidDispenser { get; private set; }
        public string HelpText { get; private set; }
        public string HelpTextPlayer { get; private set; }
        public string HelpTextAdmin { get; private set; }
        public string HelpTextPlayerGains { get; private set; }
        public string HelpTextPlayerDefault { get; private set; }
        public string HelpTextPlayerStaticMiningQuarrySpeed { get; private set; }
        public string HelpTextPlayerDynamicMiningQuarrySpeed { get; private set; }
        public string HelpTextPlayerStaticPumpJackSpeed { get; private set; }
        public string HelpTextPlayerDynamicPumpJackSpeed { get; private set; }
        public string HelpTextPlayerExcavatorSpeed { get; private set; }
        public string Dispensers { get; private set; }
        public string Charges { get; private set; }
        public string MiningQuarriesStatic { get; private set; }
        public string MiningQuarriesDynamic { get; private set; }
        public string PumpJacksStatic { get; private set; }
        public string PumpJacksDynamic { get; private set; }
        public string Excavators { get; private set; }
        public string Pickups { get; private set; }

        #endregion

        private readonly List<string> subcommands = new List<string>() { "dispenser", "pickup", "quarry", "survey" };

        private readonly Hash<string, ItemDefinition> validResources = new Hash<string, ItemDefinition>();

        private readonly Hash<string, ResourceDispenser.GatherType> validDispensers = new Hash<string, ResourceDispenser.GatherType>();

        private void Init() => LoadConfigValues();

        private void OnServerInitialized()
        {
            var resourceDefinitions = ItemManager.itemList;
            foreach (var def in resourceDefinitions.Where(def => def.category == ItemCategory.Food || def.category == ItemCategory.Resources))
                validResources.Add(def.displayName.english.ToLower(), def);

            validDispensers.Add("tree", ResourceDispenser.GatherType.Tree);
            validDispensers.Add("ore", ResourceDispenser.GatherType.Ore);
            validDispensers.Add("corpse", ResourceDispenser.GatherType.Flesh);
            validDispensers.Add("flesh", ResourceDispenser.GatherType.Flesh);

            updateQuarries();
            updateExcavators();
        }

        private void Unload()
        {
            restoreQuarries();
            restoreExcavators();
        }

        protected override void LoadDefaultConfig() => PrintWarning("New configuration file created.");

        [ChatCommand("gather")]
        private void Gather(BasePlayer player, string command, string[] args)
        {
            var help = HelpTextPlayer;
            if (GatherResourceModifiers.Count == 0 && PickupResourceModifiers.Count == 0 && MiningQuarryStaticResourceModifiers.Count == 0 && MiningQuarryDynamicResourceModifiers.Count == 0 && PumpJackStaticResourceModifiers.Count == 0 && PumpJackDynamicResourceModifiers.Count == 0 && ExcavatorResourceModifiers.Count == 0 && SurveyResourceModifiers.Count == 0)
                help += HelpTextPlayerDefault;
            else
            {
                if (GatherResourceModifiers.Count > 0)
                {
                    var dispensers = string.Format(HelpTextPlayerGains, Dispensers);
                    dispensers = GatherResourceModifiers.Aggregate(dispensers, (current, entry) => current + ("\r\n    " + entry.Key + ": x" + entry.Value));
                    help += "\r\n" + dispensers;
                }
                if (PickupResourceModifiers.Count > 0)
                {
                    var pickups = string.Format(HelpTextPlayerGains, Pickups);
                    pickups = PickupResourceModifiers.Aggregate(pickups, (current, entry) => current + ("\r\n    " + entry.Key + ": x" + entry.Value));
                    help += "\r\n" + pickups;
                }
                if (MiningQuarryStaticResourceModifiers.Count > 0)
                {
                    var quarries = string.Format(HelpTextPlayerGains, MiningQuarriesStatic);
                    quarries = MiningQuarryStaticResourceModifiers.Aggregate(quarries, (current, entry) => current + ("\r\n    " + entry.Key + ": x" + entry.Value));
                    help += "\r\n" + quarries;
                }
                if (MiningQuarryDynamicResourceModifiers.Count > 0)
                {
                    var quarries = string.Format(HelpTextPlayerGains, MiningQuarriesDynamic);
                    quarries = MiningQuarryDynamicResourceModifiers.Aggregate(quarries, (current, entry) => current + ("\r\n    " + entry.Key + ": x" + entry.Value));
                    help += "\r\n" + quarries;
                }
                if (PumpJackStaticResourceModifiers.Count > 0)
                {
                    var quarries = string.Format(HelpTextPlayerGains, PumpJacksStatic);
                    quarries = PumpJackStaticResourceModifiers.Aggregate(quarries, (current, entry) => current + ("\r\n    " + entry.Key + ": x" + entry.Value));
                    help += "\r\n" + quarries;
                }
                if (PumpJackDynamicResourceModifiers.Count > 0)
                {
                    var quarries = string.Format(HelpTextPlayerGains, PumpJacksDynamic);
                    quarries = PumpJackDynamicResourceModifiers.Aggregate(quarries, (current, entry) => current + ("\r\n    " + entry.Key + ": x" + entry.Value));
                    help += "\r\n" + quarries;
                }
                if (ExcavatorResourceModifiers.Count > 0)
                {
                    var excavators = string.Format(HelpTextPlayerGains, Excavators);
                    excavators = ExcavatorResourceModifiers.Aggregate(excavators, (current, entry) => current + ("\r\n    " + entry.Key + ": x" + entry.Value));
                    help += "\r\n" + excavators;
                }
                if (SurveyResourceModifiers.Count > 0)
                {
                    var charges = string.Format(HelpTextPlayerGains, Charges);
                    charges = SurveyResourceModifiers.Aggregate(charges, (current, entry) => current + ("\r\n    " + entry.Key + ": x" + entry.Value));
                    help += "\r\n" + charges;
                }
            }

            if (MiningQuarryStaticResourceTickRate != DefaultMiningQuarryResourceTickRate)
                help += "\r\n" + string.Format(HelpTextPlayerStaticMiningQuarrySpeed, MiningQuarryStaticResourceTickRate);
            if (MiningQuarryDynamicResourceTickRate != DefaultMiningQuarryResourceTickRate)
                help += "\r\n" + string.Format(HelpTextPlayerDynamicMiningQuarrySpeed, MiningQuarryDynamicResourceTickRate);
            if (PumpJackStaticResourceTickRate != DefaultMiningQuarryResourceTickRate)
                help += "\r\n" + string.Format(HelpTextPlayerStaticPumpJackSpeed, MiningQuarryStaticResourceTickRate);
            if (PumpJackDynamicResourceTickRate != DefaultMiningQuarryResourceTickRate)
                help += "\r\n" + string.Format(HelpTextPlayerDynamicPumpJackSpeed, MiningQuarryDynamicResourceTickRate);
            if (ExcavatorResourceTickRate != DefaultExcavatorResourceTickRate)
                help += "\r\n" + string.Format(HelpTextPlayerExcavatorSpeed, ExcavatorResourceTickRate / ExcavatorTimeForFullResources * DefaultExcavatorTimeForFullResources / DefaultExcavatorResourceTickRate);

            SendMessage(player, help);
            if (!player.IsAdmin) return;
            SendMessage(player, HelpTextAdmin);
        }

        private void SendHelpText(BasePlayer player) => SendMessage(player, HelpText);

        [ConsoleCommand("gather.rate")]
        private void GatherRate(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NotAllowed);
                return;
            }

            var subcommand = arg.GetString(0).ToLower();
            if (!arg.HasArgs(3) || !subcommands.Contains(subcommand))
            {
                arg.ReplyWith(InvalidArgumentsGather);
                return;
            }

            if (!validResources[arg.GetString(1).ToLower()] && arg.GetString(1) != "*")
            {
                arg.ReplyWith(string.Format(InvalidResource, arg.GetString(1)));
                return;
            }

            var resource = validResources[arg.GetString(1).ToLower()]?.displayName.english ?? "*";
            var modifier = arg.GetFloat(2, -1);
            var remove = false;
            if (modifier < 0)
            {
                if (arg.GetString(2).ToLower() == "remove")
                    remove = true;
                else
                {
                    arg.ReplyWith(InvalidModifier);
                    return;
                }
            }

            switch (subcommand)
            {
                case "dispenser":
                    if (remove)
                    {
                        if (GatherResourceModifiers.ContainsKey(resource))
                            GatherResourceModifiers.Remove(resource);
                        arg.ReplyWith(string.Format(ModifyResourceRemove, resource, Dispensers));
                    }
                    else
                    {
                        if (GatherResourceModifiers.ContainsKey(resource))
                            GatherResourceModifiers[resource] = modifier;
                        else
                            GatherResourceModifiers.Add(resource, modifier);
                        arg.ReplyWith(string.Format(ModifyResource, resource, modifier, Dispensers));
                    }
                    SetConfigValue(new string[] { "Options", "GatherResourceModifiers" }, GatherResourceModifiers);
                    break;
                case "pickup":
                    if (remove)
                    {
                        if (PickupResourceModifiers.ContainsKey(resource))
                            PickupResourceModifiers.Remove(resource);
                        arg.ReplyWith(string.Format(ModifyResourceRemove, resource, Pickups));
                    }
                    else
                    {
                        if (PickupResourceModifiers.ContainsKey(resource))
                            PickupResourceModifiers[resource] = modifier;
                        else
                            PickupResourceModifiers.Add(resource, modifier);
                        arg.ReplyWith(string.Format(ModifyResource, resource, modifier, Pickups));
                    }
                    SetConfigValue(new string[] { "Options", "PickupResourceModifiers" }, PickupResourceModifiers);
                    break;
                case "quarry":
                    if (remove)
                    {
                        if (MiningQuarryStaticResourceModifiers.ContainsKey(resource))
                            MiningQuarryStaticResourceModifiers.Remove(resource);
                        if (MiningQuarryDynamicResourceModifiers.ContainsKey(resource))
                            MiningQuarryDynamicResourceModifiers.Remove(resource);
                        arg.ReplyWith(string.Format(ModifyResourceRemove2, resource, MiningQuarriesStatic, MiningQuarriesDynamic));
                    }
                    else
                    {
                        if (MiningQuarryStaticResourceModifiers.ContainsKey(resource))
                            MiningQuarryStaticResourceModifiers[resource] = modifier;
                        else
                            MiningQuarryStaticResourceModifiers.Add(resource, modifier);
                        if (MiningQuarryDynamicResourceModifiers.ContainsKey(resource))
                            MiningQuarryDynamicResourceModifiers[resource] = modifier;
                        else
                            MiningQuarryDynamicResourceModifiers.Add(resource, modifier);
                        arg.ReplyWith(string.Format(ModifyResource2, resource, modifier, MiningQuarriesStatic, MiningQuarriesDynamic));
                    }
                    SetConfigValue(new string[] { "Options", "MiningQuarry", "Static", "ResourceModifiers" }, MiningQuarryStaticResourceModifiers);
                    SetConfigValue(new string[] { "Options", "MiningQuarry", "Dynamic", "ResourceModifiers" }, MiningQuarryDynamicResourceModifiers);
                    break;
                case "pumpjack":
                    if (remove)
                    {
                        if (PumpJackStaticResourceModifiers.ContainsKey(resource))
                            PumpJackStaticResourceModifiers.Remove(resource);
                        if (PumpJackDynamicResourceModifiers.ContainsKey(resource))
                            PumpJackDynamicResourceModifiers.Remove(resource);
                        arg.ReplyWith(string.Format(ModifyResourceRemove2, resource, PumpJacksStatic, PumpJacksDynamic));
                    }
                    else
                    {
                        if (PumpJackStaticResourceModifiers.ContainsKey(resource))
                            PumpJackStaticResourceModifiers[resource] = modifier;
                        else
                            PumpJackStaticResourceModifiers.Add(resource, modifier);
                        if (PumpJackDynamicResourceModifiers.ContainsKey(resource))
                            PumpJackDynamicResourceModifiers[resource] = modifier;
                        else
                            PumpJackDynamicResourceModifiers.Add(resource, modifier);
                        arg.ReplyWith(string.Format(ModifyResource2, resource, modifier, PumpJacksStatic, PumpJacksDynamic));
                    }
                    SetConfigValue(new string[] { "Options", "PumpJack", "Static", "ResourceModifiers" }, PumpJackStaticResourceModifiers);
                    SetConfigValue(new string[] { "Options", "PumpJack", "Dynamic", "ResourceModifiers" }, PumpJackDynamicResourceModifiers);
                    break;
                case "excavator":
                    if (remove)
                    {
                        if (ExcavatorResourceModifiers.ContainsKey(resource))
                            ExcavatorResourceModifiers.Remove(resource);
                        arg.ReplyWith(string.Format(ModifyResourceRemove, resource, Excavators));
                    }
                    else
                    {
                        if (ExcavatorResourceModifiers.ContainsKey(resource))
                            ExcavatorResourceModifiers[resource] = modifier;
                        else
                            ExcavatorResourceModifiers.Add(resource, modifier);
                        arg.ReplyWith(string.Format(ModifyResource, resource, modifier, Excavators));
                    }
                    SetConfigValue(new string[] { "Options", "ExcavatorResourceModifiers" }, ExcavatorResourceModifiers);
                    break;
                case "survey":
                    if (remove)
                    {
                        if (SurveyResourceModifiers.ContainsKey(resource))
                            SurveyResourceModifiers.Remove(resource);
                        arg.ReplyWith(string.Format(ModifyResourceRemove, resource, Charges));
                    }
                    else
                    {
                        if (SurveyResourceModifiers.ContainsKey(resource))
                            SurveyResourceModifiers[resource] = modifier;
                        else
                            SurveyResourceModifiers.Add(resource, modifier);
                        arg.ReplyWith(string.Format(ModifyResource, resource, modifier, Charges));
                    }
                    SetConfigValue(new string[] { "Options", "SurveyResourceModifiers" }, SurveyResourceModifiers);
                    break;
            }
        }

        [ConsoleCommand("gather.resources")]
        private void GatherResources(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NotAllowed);
                return;
            }

            arg.ReplyWith(validResources.Aggregate("Available resources:\r\n", (current, resource) => current + (resource.Value.displayName.english + "\r\n")) + "* (For all resources that are not setup separately)");
        }

        [ConsoleCommand("gather.dispensers")]
        private void GatherDispensers(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NotAllowed);
                return;
            }

            arg.ReplyWith(validDispensers.Aggregate("Available dispensers:\r\n", (current, dispenser) => current + (dispenser.Value.ToString("G") + "\r\n")));
        }


        [ConsoleCommand("dispenser.scale")]
        private void DispenserRate(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NotAllowed);
                return;
            }

            if (!arg.HasArgs(2))
            {
                arg.ReplyWith(InvalidArgumentsDispenser);
                return;
            }

            if (!validDispensers.ContainsKey(arg.GetString(0).ToLower()))
            {
                arg.ReplyWith(string.Format(InvalidDispenser, arg.GetString(0)));
                return;
            }

            var dispenser = validDispensers[arg.GetString(0).ToLower()].ToString("G");
            var modifier = arg.GetFloat(1, -1);
            if (modifier < 0)
            {
                arg.ReplyWith(InvalidModifier);
                return;
            }

            if (GatherDispenserModifiers.ContainsKey(dispenser))
                GatherDispenserModifiers[dispenser] = modifier;
            else
                GatherDispenserModifiers.Add(dispenser, modifier);
            SetConfigValue(new string[] { "Options", "GatherDispenserModifiers" }, GatherDispenserModifiers);
            arg.ReplyWith(string.Format(ModifyDispenser, dispenser, modifier));
        }

        [ConsoleCommand("quarry.tickrate")]
        private void MiningQuarryTickRate(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NotAllowed);
                return;
            }

            if (!arg.HasArgs())
            {
                arg.ReplyWith(InvalidArgumentsSpeed);
                return;
            }

            var modifier = arg.GetFloat(0, -1);
            if (modifier < 1)
            {
                arg.ReplyWith(InvalidSpeed);
                return;
            }

            MiningQuarryStaticResourceTickRate = modifier;
            MiningQuarryDynamicResourceTickRate = modifier;
            SetConfigValue(new string[] { "Options", "MiningQuarry", "Static", "ResourceTickRate" }, MiningQuarryStaticResourceTickRate);
            SetConfigValue(new string[] { "Options", "MiningQuarry", "Dynamic", "ResourceTickRate" }, MiningQuarryDynamicResourceTickRate);
            arg.ReplyWith(string.Format(ModifySpeed, modifier));
            updateQuarries();
        }

        [ConsoleCommand("pumpjack.tickrate")]
        private void PumpJackTickRate(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NotAllowed);
                return;
            }

            if (!arg.HasArgs())
            {
                arg.ReplyWith(InvalidArgumentsSpeed);
                return;
            }

            var modifier = arg.GetFloat(0, -1);
            if (modifier < 1)
            {
                arg.ReplyWith(InvalidSpeed);
                return;
            }

            PumpJackStaticResourceTickRate = modifier;
            PumpJackDynamicResourceTickRate = modifier;
            SetConfigValue(new string[] { "Options", "PumpJack", "Static", "ResourceTickRate" }, PumpJackStaticResourceTickRate);
            SetConfigValue(new string[] { "Options", "PumpJack", "Dynamic", "ResourceTickRate" }, PumpJackDynamicResourceTickRate);
            arg.ReplyWith(string.Format(ModifySpeed, modifier));
            updateQuarries();
        }

        private float getQuarryResourceTickRate(MiningQuarry quarry)
        {
            return quarry.isStatic
                ? quarry.canExtractLiquid
                    ? PumpJackStaticResourceTickRate
                    : MiningQuarryStaticResourceTickRate
                : quarry.canExtractLiquid
                    ? PumpJackDynamicResourceTickRate
                    : MiningQuarryDynamicResourceTickRate;
        }

        private int getQuarryInventoryOutputSlots(MiningQuarry quarry)
        {
            return quarry.isStatic
                ? quarry.canExtractLiquid
                    ? PumpJackStaticInventoryOutputSlots
                    : MiningQuarryStaticInventoryOutputSlots
                : quarry.canExtractLiquid
                    ? PumpJackDynamicInventoryOutputSlots
                    : MiningQuarryDynamicInventoryOutputSlots;
        }

        private Dictionary<string, float> getQuarryResourceModifiers(MiningQuarry quarry)
        {
            return quarry.isStatic
                ? quarry.canExtractLiquid
                    ? PumpJackStaticResourceModifiers
                    : MiningQuarryStaticResourceModifiers
                : quarry.canExtractLiquid
                    ? PumpJackDynamicResourceModifiers
                    : MiningQuarryDynamicResourceModifiers;
        }

        private void updateQuarry(MiningQuarry quarry)
        {
            float resourceTickRate = getQuarryResourceTickRate(quarry);
            if (quarry.IsOn() && quarry.processRate != resourceTickRate)
            {
                quarry.CancelInvoke(quarry.ProcessResources);
                quarry.InvokeRepeating(quarry.ProcessResources, resourceTickRate, resourceTickRate);
            }

            quarry.processRate = resourceTickRate;

            if (quarry.hopperPrefab.instance != null)
            {
                int outputInventorySlots = getQuarryInventoryOutputSlots(quarry);
                quarry.hopperPrefab.instance.GetComponent<StorageContainer>().inventory.capacity = outputInventorySlots;
            }
        }
        
        private void updateQuarries()
        {
            var quarries = UnityEngine.Object.FindObjectsOfType<MiningQuarry>();
            foreach (var quarry in quarries)
            {
                updateQuarry(quarry);
            }
        }
        
        private void restoreQuarries()
        {
            var quarries = UnityEngine.Object.FindObjectsOfType<MiningQuarry>();
            foreach (var quarry in quarries)
            {
                if (quarry.IsOn() && quarry.processRate != DefaultMiningQuarryResourceTickRate)
                {
                    quarry.CancelInvoke(quarry.ProcessResources);
                    quarry.InvokeRepeating(quarry.ProcessResources, DefaultMiningQuarryResourceTickRate, DefaultMiningQuarryResourceTickRate);
                }
                quarry.processRate = DefaultMiningQuarryResourceTickRate;

                quarry.hopperPrefab.instance.GetComponent<StorageContainer>().inventory.capacity = DefaultQuarryInventoryOutputSlots;
            }
        }

        [ConsoleCommand("excavator.tickrate")]
        private void ExcavatorTickRate(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NotAllowed);
                return;
            }

            if (!arg.HasArgs())
            {
                arg.ReplyWith(InvalidArgumentsSpeed);
                return;
            }

            var modifier = arg.GetFloat(0, -1);
            if (modifier < 1)
            {
                arg.ReplyWith(InvalidSpeed);
                return;
            }

            ExcavatorResourceTickRate = modifier;
            SetConfigValue(new string[] { "Options", "ExcavatorResourceTickRate" }, ExcavatorResourceTickRate);
            arg.ReplyWith(string.Format(ModifySpeed, modifier));
            updateExcavators();
        }
        
        private void updateExcavators()
        {
            var excavatorArms = UnityEngine.Object.FindObjectsOfType<ExcavatorArm>();
            foreach (var excavatorArm in excavatorArms)
            {
                if (excavatorArm.IsOn() && excavatorArm.resourceProductionTickRate != ExcavatorResourceTickRate)
                {
                    excavatorArm.CancelInvoke(excavatorArm.ProduceResources);
                    excavatorArm.InvokeRepeating(excavatorArm.ProduceResources, ExcavatorResourceTickRate, ExcavatorResourceTickRate);
                }
                excavatorArm.resourceProductionTickRate = ExcavatorResourceTickRate;

                excavatorArm.timeForFullResources = ExcavatorTimeForFullResources;
            }

            var excavatorOutputPiles = UnityEngine.Object.FindObjectsOfType<ExcavatorOutputPile>();
            foreach (var excavatorOutputPile in excavatorOutputPiles)
            {
                excavatorOutputPile.inventory.capacity = ExcavatorInventoryOutputSlots;
            }

            var dieselEngines = UnityEngine.Object.FindObjectsOfType<DieselEngine>();
            foreach (var dieselEngine in dieselEngines)
            {
                dieselEngine.runningTimePerFuelUnit = ExcavatorRunningTimePerFuelUnit;
            }
        }
        
        private void restoreExcavators()
        {
            var excavatorArms = UnityEngine.Object.FindObjectsOfType<ExcavatorArm>();
            foreach (var excavatorArm in excavatorArms)
            {
                if (excavatorArm.IsOn() && excavatorArm.resourceProductionTickRate != ExcavatorResourceTickRate)
                {
                    excavatorArm.CancelInvoke(excavatorArm.ProduceResources);
                    excavatorArm.InvokeRepeating(excavatorArm.ProduceResources, DefaultExcavatorResourceTickRate, DefaultExcavatorResourceTickRate);
                }
                excavatorArm.resourceProductionTickRate = ExcavatorResourceTickRate;

                excavatorArm.timeForFullResources = DefaultExcavatorTimeForFullResources;
            }

            var excavatorOutputPiles = UnityEngine.Object.FindObjectsOfType<ExcavatorOutputPile>();
            foreach (var excavatorOutputPile in excavatorOutputPiles)
            {
                excavatorOutputPile.inventory.capacity = DefaultExcavatorInventoryOutputSlots;
            }

            var dieselEngines = UnityEngine.Object.FindObjectsOfType<DieselEngine>();
            foreach (var dieselEngine in dieselEngines)
            {
                dieselEngine.runningTimePerFuelUnit = DefaultExcavatorRunningTimePerFuelUnit;
            }
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (!entity.ToPlayer())
            {
                return;
            }

            var gatherType = dispenser.gatherType.ToString("G");
            var amount = item.amount;

            float modifier;
            if (GatherResourceModifiers.TryGetValue(item.info.displayName.english, out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
            else if (GatherResourceModifiers.TryGetValue("*", out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }

            if (!GatherResourceModifiers.ContainsKey(gatherType))
            {
                return;
            }

            var dispenserModifier = GatherDispenserModifiers[gatherType];

            try
            {
                dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount += amount - item.amount / dispenserModifier;

                if (dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount < 0)
                {
                    item.amount += (int)dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount;
                }
            }
            catch { }
        }

        private void OnDispenserBonus(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            OnDispenserGather(dispenser, entity, item);
        }

        private void OnGrowableGathered(GrowableEntity growable, Item item, BasePlayer player)
        {
            float modifier;
            if ( GatherResourceModifiers.TryGetValue(item.info.displayName.english, out modifier) )
            {
                item.amount = (int)(item.amount * modifier);
            }
            else if ( GatherResourceModifiers.TryGetValue("*", out modifier) )
            {
                item.amount = (int)(item.amount * modifier);
            }
        }

        private void OnQuarryGather(MiningQuarry quarry, Item item)
        {
            Dictionary<string, float> resourceModifiers = getQuarryResourceModifiers(quarry);
            float modifier;
            if (resourceModifiers.TryGetValue(item.info.displayName.english, out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
            else if (resourceModifiers.TryGetValue("*", out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
        }

        private void OnExcavatorGather(ExcavatorArm excavator, Item item)
        {
            float modifier;
            if (ExcavatorResourceModifiers.TryGetValue(item.info.displayName.english, out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
            else if (ExcavatorResourceModifiers.TryGetValue("*", out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
        }

        private void OnCollectiblePickup(Item item, BasePlayer player)
        {
            float modifier;
            if (PickupResourceModifiers.TryGetValue(item.info.displayName.english, out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
            else if (PickupResourceModifiers.TryGetValue("*", out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
        }

        private void OnSurveyGather(SurveyCharge surveyCharge, Item item)
        {
            float modifier;
            if (SurveyResourceModifiers.TryGetValue(item.info.displayName.english, out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
            else if (SurveyResourceModifiers.TryGetValue("*", out modifier))
            {
                item.amount = (int)(item.amount * modifier);
            }
        }

        private object OnConstructionPlace(BaseEntity entity, Construction component, Construction.Target constructionTarget, BasePlayer player)
        {
            if (entity is MiningQuarry)
            {
                // Update the quarry but not until the quarry is ready.
                entity.Invoke(() => {
                    updateQuarry(entity as MiningQuarry);
                }, 0f);
            }

            return null;
        }

        private void LoadConfigValues()
        {
            // Plugin settings
            ChatPrefix = GetConfigValue(new string[] { "Settings", "ChatPrefix" }, DefaultChatPrefix);
            ChatPrefixColor = GetConfigValue(new string[] { "Settings", "ChatPrefixColor" }, DefaultChatPrefixColor);

            // Plugin options
            var gatherResourceModifiers = GetConfigValue(new string[] { "Options", "GatherResourceModifiers" }, DefaultGatherResourceModifiers);
            var gatherDispenserModifiers = GetConfigValue(new string[] { "Options", "GatherDispenserModifiers" }, DefaultGatherDispenserModifiers);
            var pickupResourceModifiers = GetConfigValue(new string[] { "Options", "PickupResourceModifiers" }, DefaultPickupResourceModifiers);
            var surveyResourceModifiers = GetConfigValue(new string[] { "Options", "SurveyResourceModifiers" }, DefaultSurveyResourceModifiers);

            // Old config settings.
            var quarryResourceModifiers = GetConfigValue(new string[] { "Options", "QuarryResourceModifiers" }, DefaultQuarryResourceModifiers);
            var excavatorResourceModifiers = GetConfigValue(new string[] { "Options", "ExcavatorResourceModifiers" }, quarryResourceModifiers);
            RemoveConfigValue(new string[] { "Options", "QuarryResourceModifiers" });
            RemoveConfigValue(new string[] { "Options", "ExcavatorResourceModifiers" });

            var miningQuarryStaticResourceModifiers = GetConfigValue(new string[] { "Options", "MiningQuarry", "Static", "ResourceModifiers" }, quarryResourceModifiers);
            var miningQuarryDynamicResourceModifiers = GetConfigValue(new string[] { "Options", "MiningQuarry", "Dynamic", "ResourceModifiers" }, quarryResourceModifiers);
            var pumpJackStaticResourceModifiers = GetConfigValue(new string[] { "Options", "PumpJack", "Static", "ResourceModifiers" }, quarryResourceModifiers);
            var pumpJackDynamicResourceModifiers = GetConfigValue(new string[] { "Options", "PumpJack", "Dynamic", "ResourceModifiers" }, quarryResourceModifiers);
            excavatorResourceModifiers = GetConfigValue(new string[] { "Options", "Excavator", "ResourceModifiers" }, excavatorResourceModifiers);

            // Old config setting.
            var miningQuarryResourceTickRate = GetConfigValue(new string[] { "Options", "MiningQuarryResourceTickRate" }, DefaultMiningQuarryResourceTickRate, true);
            RemoveConfigValue(new string[] { "Options", "MiningQuarryResourceTickRate" });

            MiningQuarryStaticResourceTickRate = GetConfigValue(new string[] { "Options", "MiningQuarry", "Static", "ResourceTickRate" }, miningQuarryResourceTickRate);
            MiningQuarryDynamicResourceTickRate = GetConfigValue(new string[] { "Options", "MiningQuarry", "Dynamic", "ResourceTickRate" }, miningQuarryResourceTickRate);
            MiningQuarryStaticInventoryOutputSlots = GetConfigValue(new string[] { "Options", "MiningQuarry", "Static", "InventoryOutputSlots" }, DefaultQuarryInventoryOutputSlots);
            MiningQuarryDynamicInventoryOutputSlots = GetConfigValue(new string[] { "Options", "MiningQuarry", "Dynamic", "InventoryOutputSlots" }, DefaultQuarryInventoryOutputSlots);

            PumpJackStaticResourceTickRate = GetConfigValue(new string[] { "Options", "PumpJack", "Static", "ResourceTickRate" }, miningQuarryResourceTickRate);
            PumpJackDynamicResourceTickRate = GetConfigValue(new string[] { "Options", "PumpJack", "Dynamic", "ResourceTickRate" }, miningQuarryResourceTickRate);
            PumpJackStaticInventoryOutputSlots = GetConfigValue(new string[] { "Options", "PumpJack", "Static", "InventoryOutputSlots" }, DefaultQuarryInventoryOutputSlots);
            PumpJackDynamicInventoryOutputSlots = GetConfigValue(new string[] { "Options", "PumpJack", "Dynamic", "InventoryOutputSlots" }, DefaultQuarryInventoryOutputSlots);

            // Old config settings.
            var excavatorResourceTickRate = GetConfigValue(new string[] { "Options", "ExcavatorResourceTickRate" }, DefaultExcavatorResourceTickRate);
            var excavatorTimeForFullResources = GetConfigValue(new string[] { "Options", "ExcavatorTimeForFullResources" }, DefaultExcavatorTimeForFullResources);
            RemoveConfigValue(new string[] { "Options", "ExcavatorResourceTickRate" });
            RemoveConfigValue(new string[] { "Options", "ExcavatorBeltSpeedMax" });
            RemoveConfigValue(new string[] { "Options", "ExcavatorTimeForFullResources" });

            ExcavatorResourceTickRate = GetConfigValue(new string[] { "Options", "Excavator", "ResourceTickRate" }, excavatorResourceTickRate);
            ExcavatorTimeForFullResources = GetConfigValue(new string[] { "Options", "Excavator", "TimeForFullResources" }, excavatorTimeForFullResources);
            ExcavatorInventoryOutputSlots = GetConfigValue(new string[] { "Options", "Excavator", "InventoryOutputSlots" }, DefaultExcavatorInventoryOutputSlots);
            ExcavatorRunningTimePerFuelUnit = GetConfigValue(new string[] { "Options", "Excavator", "RunningTimePerFuelUnit" }, DefaultExcavatorRunningTimePerFuelUnit);

            if (MiningQuarryStaticInventoryOutputSlots > 36 || MiningQuarryDynamicInventoryOutputSlots > 36 || PumpJackStaticInventoryOutputSlots > 36 || PumpJackDynamicInventoryOutputSlots > 36 || ExcavatorInventoryOutputSlots > 36)
            {
                Debug.LogWarning("Don't set the number of inventory slots to more than the number visually display to the user.");
            }

            GatherResourceModifiers = new Dictionary<string, float>();
            foreach (var entry in gatherResourceModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                GatherResourceModifiers.Add(entry.Key, rate);
            }

            GatherDispenserModifiers = new Dictionary<string, float>();
            foreach (var entry in gatherDispenserModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                GatherDispenserModifiers.Add(entry.Key, rate);
            }

            MiningQuarryStaticResourceModifiers = new Dictionary<string, float>();
            foreach (var entry in miningQuarryStaticResourceModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                MiningQuarryStaticResourceModifiers.Add(entry.Key, rate);
            }

            MiningQuarryDynamicResourceModifiers = new Dictionary<string, float>();
            foreach (var entry in miningQuarryDynamicResourceModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                MiningQuarryDynamicResourceModifiers.Add(entry.Key, rate);
            }
            
            PumpJackStaticResourceModifiers = new Dictionary<string, float>();
            foreach (var entry in pumpJackStaticResourceModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                PumpJackStaticResourceModifiers.Add(entry.Key, rate);
            }
            
            PumpJackDynamicResourceModifiers = new Dictionary<string, float>();
            foreach (var entry in pumpJackDynamicResourceModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                PumpJackDynamicResourceModifiers.Add(entry.Key, rate);
            }

            ExcavatorResourceModifiers = new Dictionary<string, float>();
            foreach (var entry in excavatorResourceModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                ExcavatorResourceModifiers.Add(entry.Key, rate);
            }

            PickupResourceModifiers = new Dictionary<string, float>();
            foreach (var entry in pickupResourceModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                PickupResourceModifiers.Add(entry.Key, rate);
            }

            SurveyResourceModifiers = new Dictionary<string, float>();
            foreach (var entry in surveyResourceModifiers)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                SurveyResourceModifiers.Add(entry.Key, rate);
            }

            // Plugin messages
            NotAllowed = GetConfigValue(new string[] { "Messages", "NotAllowed" }, DefaultNotAllowed);
            InvalidArgumentsGather = GetConfigValue(new string[] { "Messages", "InvalidArgumentsGather" }, DefaultInvalidArgumentsGather);
            InvalidArgumentsDispenser = GetConfigValue(new string[] { "Messages", "InvalidArgumentsDispenserType" }, DefaultInvalidArgumentsDispenser);
            InvalidArgumentsSpeed = GetConfigValue(new string[] { "Messages", "InvalidArgumentsMiningQuarrySpeed" }, DefaultInvalidArgumentsSpeed);
            InvalidModifier = GetConfigValue(new string[] { "Messages", "InvalidModifier" }, DefaultInvalidModifier);
            InvalidSpeed = GetConfigValue(new string[] { "Messages", "InvalidMiningQuarrySpeed" }, DefaultInvalidSpeed);
            ModifyResource = GetConfigValue(new string[] { "Messages", "ModifyResource" }, DefaultModifyResource);
            ModifyResource2 = GetConfigValue(new string[] { "Messages", "ModifyResource2" }, DefaultModifyResource2);
            ModifyResourceRemove = GetConfigValue(new string[] { "Messages", "ModifyResourceRemove" }, DefaultModifyResourceRemove);
            ModifyResourceRemove2 = GetConfigValue(new string[] { "Messages", "ModifyResourceRemove2" }, DefaultModifyResourceRemove2);
            ModifySpeed = GetConfigValue(new string[] { "Messages", "ModifyMiningQuarrySpeed" }, DefaultModifySpeed);
            InvalidResource = GetConfigValue(new string[] { "Messages", "InvalidResource" }, DefaultInvalidResource);
            ModifyDispenser = GetConfigValue(new string[] { "Messages", "ModifyDispenser" }, DefaultModifyDispenser);
            InvalidDispenser = GetConfigValue(new string[] { "Messages", "InvalidDispenser" }, DefaultInvalidDispenser);
            HelpText = GetConfigValue(new string[] { "Messages", "HelpText" }, DefaultHelpText);
            HelpTextAdmin = GetConfigValue(new string[] { "Messages", "HelpTextAdmin" }, DefaultHelpTextAdmin);
            HelpTextPlayer = GetConfigValue(new string[] { "Messages", "HelpTextPlayer" }, DefaultHelpTextPlayer);
            HelpTextPlayerGains = GetConfigValue(new string[] { "Messages", "HelpTextPlayerGains" }, DefaultHelpTextPlayerGains);
            HelpTextPlayerDefault = GetConfigValue(new string[] { "Messages", "HelpTextPlayerDefault" }, DefaultHelpTextPlayerDefault);
            HelpTextPlayerStaticMiningQuarrySpeed = GetConfigValue(new string[] { "Messages", "HelpTextPlayerStaticMiningQuarrySpeed" }, DefaultHelpTextPlayerStaticMiningQuarrySpeed);
            HelpTextPlayerDynamicMiningQuarrySpeed = GetConfigValue(new string[] { "Messages", "HelpTextPlayerDynamicMiningQuarrySpeed" }, DefaultHelpTextPlayerDynamicMiningQuarrySpeed);
            HelpTextPlayerStaticPumpJackSpeed = GetConfigValue(new string[] { "Messages", "HelpTextPlayerStaticPumpJackSpeed" }, DefaultHelpTextPlayerStaticPumpJackSpeed);
            HelpTextPlayerDynamicPumpJackSpeed = GetConfigValue(new string[] { "Messages", "HelpTextPlayerDynamicPumpJackSpeed" }, DefaultHelpTextPlayerDynamicPumpJackSpeed);
            HelpTextPlayerExcavatorSpeed = GetConfigValue(new string[] { "Messages", "HelpTextPlayerExcavatorSpeed" }, DefaultHelpTextPlayerExcavatorSpeed);
            Dispensers = GetConfigValue(new string[] { "Messages", "Dispensers" }, DefaultDispensers);
            MiningQuarriesStatic = GetConfigValue(new string[] { "Messages", "MiningQuarriesStatic" }, DefaultStaticQuarries);
            MiningQuarriesDynamic = GetConfigValue(new string[] { "Messages", "MiningQuarriesDynamic" }, DefaultDynamicQuarries);
            PumpJacksStatic = GetConfigValue(new string[] { "Messages", "PumpJacksStatic" }, DefaultStaticPumpJacks);
            PumpJacksDynamic = GetConfigValue(new string[] { "Messages", "PumpJacksDynamic" }, DefaultDynamicPumpJacks);
            Excavators = GetConfigValue(new string[] { "Messages", "Excavators" }, DefaultExcavators);
            Charges = GetConfigValue(new string[] { "Messages", "SurveyCharges" }, DefaultCharges);
            Pickups = GetConfigValue(new string[] { "Messages", "Pickups" }, DefaultPickups);

            RemoveConfigValue(new string[] { "Messages", "HelpTextPlayerMiningQuarrySpeed" });
            RemoveConfigValue(new string[] { "Messages", "MiningQuarries" });

            if (!configChanged) return;
            PrintWarning("Configuration file updated.");
            SaveConfig();
        }

        private T GetConfigValue<T>(string[] settingPath, T defaultValue, bool deprecated = false)
        {
            object value = Config.Get(settingPath);
            if (value == null)
            {
                if (!deprecated) {
                    SetConfigValue(settingPath, defaultValue);
                    configChanged = true;
                }
                return defaultValue;
            }
            return Config.ConvertValue<T>(value);
        }

        private void SetConfigValue<T>(string[] settingPath, T newValue)
        {
            List<object> pathAndTrailingValue = new List<object>();
            foreach (var segment in settingPath)
            {
                pathAndTrailingValue.Add(segment);
            }
            pathAndTrailingValue.Add(newValue);

            Config.Set(pathAndTrailingValue.ToArray());
            SaveConfig();
        }

        private void RemoveConfigValue(string[] settingPath)
        {
            if (settingPath.Length == 1)
            {
                Config.Remove(settingPath[0]);
            }

            List<string> parentPath = new List<string>();
            for (int i = 0; i < settingPath.Length - 1; i++)
            {
                parentPath.Add(settingPath[i]);
            }

            Dictionary<string, object> parent = Config.Get(parentPath.ToArray()) as Dictionary<string, object>;
            parent.Remove(settingPath[settingPath.Length - 1]);
            SaveConfig();
        }

        private void SendMessage(BasePlayer player, string message, params object[] args) => player?.SendConsoleCommand("chat.add", 0, -1, string.Format($"<color={ChatPrefixColor}>{ChatPrefix}</color>: {message}", args), 1.0);
    }
}

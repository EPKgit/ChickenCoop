using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Targeting;
using static UnityEngine.Timeline.TimelineAsset;
using static UnityEngine.InputSystem.Controls.AxisControl;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.SocialPlatforms;

public class AbilityDataXMLParser : Singleton<AbilityDataXMLParser>
{
    private class AbilityXMLDataEntry
    {
        public uint ID;
        public string name;
        public string defaultLibrary;
        public string iconPath;

        public bool? isPassive = null;
        public float? cooldown = null;
        public float? duration = null;
        public bool? doesTicking = null;
        public int? recasts = null;
        public float? recastWindow = null;

        public string[] tooltips;
        public List<AbilityXMLTargetingData> targetingData;
        public List<AbilityXMLVariable> vars;
        public List<string> ability_tags;
        public List<string> ability_blocked_tags;
        public List<string> ability_applied_tags;
        public AbilityXMLDataEntry(uint i, string n, string defaultLib, string icon)
        {
            ID = i;
            name = n;
            defaultLibrary = defaultLib;
            iconPath = icon;
            targetingData = new List<AbilityXMLTargetingData>();
            tooltips = new string[(int)Ability.AbilityUpgradeSlot.MAX + 1];
            vars = new List<AbilityXMLVariable>();
            ability_tags = new List<string>();
            ability_blocked_tags = new List<string>();
            ability_applied_tags = new List<string>();
        }
    }

    private class AbilityXMLTargetingData
    {
        public AbilityTargetingData targetingData;
        private AbilityXMLTargetingData() { }
        public AbilityXMLTargetingData(XmlElement node, string defaultLibrary)
        {
            targetingData = new AbilityTargetingData();
            switch(node.Name)
            {
                case "noneTargetingData":
                {
                    targetingData.targetType = TargetType.NONE;
                } break;

                case "lineTargetingData":
                {
                    targetingData.targetType = TargetType.LINE_TARGETED;
                    targetingData.range = float.Parse(node["range"].InnerText);
                    ParseOORHandling(node);

                    targetingData.previewScale = new Vector3(float.Parse(node["width"].InnerText), 0, 0);
                } break;

                case "groundTargetingData":
                {
                    targetingData.targetType = TargetType.GROUND_TARGETED;
                    targetingData.range = float.Parse(node["range"].InnerText);
                    ParseOORHandling(node);

                    var size = node["size"];
                    float x = 0, y = 0;
                    if (size["xy"] != null)
                    {
                        x = y = float.Parse(size["xy"].InnerText);
                    }
                    else
                    {
                        x = float.Parse(size["x"].InnerText);
                        y = float.Parse(size["y"].InnerText);
                    }
                    targetingData.previewScale = new Vector3(x, y, 0);
                } break;

                case "entityTargetingData":
                {
                    targetingData.targetType = TargetType.ENTITY_TARGETED;
                    targetingData.range = float.Parse(node["range"].InnerText);
                    ParseOORHandling(node);

                    targetingData.affiliation = (Targeting.Affiliation)Enum.Parse(typeof(Targeting.Affiliation), node["affiliation"].InnerText);
                } break;
            }

            var rangePreviewPrefabNode = node["rangePreviewPrefab"];
            if (rangePreviewPrefabNode != null)
            {
                Tuple<string, string> path = AssetLibraryManager.instance.ParseStringToCategoryAndName(rangePreviewPrefabNode.InnerText, defaultLibrary);
                targetingData.rangePreviewPrefab = AssetLibraryManager.instance.GetPrefab(path.Item1, path.Item2);
            }

            var secondaryPreviewPrefabNode = node["secondaryPreviewPrefab"];
            if (secondaryPreviewPrefabNode != null)
            {
                Tuple<string, string> path = AssetLibraryManager.instance.ParseStringToCategoryAndName(secondaryPreviewPrefabNode.InnerText, defaultLibrary);
                targetingData.secondaryPreviewPrefab = AssetLibraryManager.instance.GetPrefab(path.Item1, path.Item2);
            }
        }

        void ParseOORHandling(XmlElement node)
        {
            var oorNode = node.GetAttribute("oorHandling");
            if(oorNode != null && oorNode != "")
            {
                targetingData.outOfRangeHandlingType = (OutOfRangeHandlingType)Enum.Parse(typeof(OutOfRangeHandlingType), oorNode);
            } 
        }
    }
    

    private class AbilityXMLVariable
    {
        public string name;
        public string value;
        public string type;

        private AbilityXMLVariable() { }
        public AbilityXMLVariable(XmlElement node)
        {
            type = node.Name;
            name = node.GetAttribute("name");
            value = node.InnerText;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ParseXMLData();
    }

    Dictionary<uint, AbilityXMLDataEntry> table;
    const string XMLFolderPath = "AbilityData";
    bool loadedTable = false;

    public void ForceReimport()
    {
        // TODO make reimports from hot reloading only reload specific files
        loadedTable = false;
        ParseXMLData();
        foreach(PlayerInitialization player in PlayerInitialization.all)
        {
            foreach(Ability a in player.GetComponent<PlayerAbilities>().Abilities)
            {
                UpdateAbilityData(a);
            }
        }
    }

    //TODO MAKE THREAD SAFE
    void ParseXMLData()
    {
        if (loadedTable)
        {
            return;
        }
        loadedTable = false;

        var files = Resources.LoadAll(XMLFolderPath, typeof(TextAsset));
        table = new Dictionary<uint, AbilityXMLDataEntry>();
        foreach (var file in files)
        {
            if (file == null)
            {
                Debug.LogAssertion("ERROR: COULD NOT OPEN ABILITY XML FOR UNKNOWN FILE");
                return;
            }
            if(file.GetType() != typeof(TextAsset))
            {
                Debug.LogAssertion("ERROR: ABILITY XML IS NOT TEXT ASSET:" + file.name);
                return;
            }

            if(!LoadFile((TextAsset)file))
            {
                Debug.LogAssertion("ERROR IN FILE " + file.name);
                return;
            }
        }
        
        loadedTable = true;
    }

    bool LoadFile(TextAsset file)
    {
        DebugFlags.Log(DebugFlags.Flags.ABILITYXML, "starting to load " + file.name);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(file.text);

        XmlNodeList list = doc.SelectNodes("//comment()");
        foreach (XmlNode node in list)
        {
            node.ParentNode.RemoveChild(node);
        }
        var ability_list_node = doc["ability_list"];
        string default_library = ability_list_node.GetAttribute("default_library");
        foreach (XmlElement ability in ability_list_node.ChildNodes)
        {
            DebugFlags.Log(DebugFlags.Flags.ABILITYXML, "\tloading ability:" + ability["ability_name"].InnerText);
            uint ability_ID = Convert.ToUInt32(ability["ability_ID"].InnerText);
            if (table.ContainsKey(ability_ID))
            {
                Debug.LogError("ERROR: duplicate ability id found for '" + ability["ability_name"].InnerText + "' ID:" + ability_ID);
                return false;
            }
            string ability_name = ability["ability_name"].InnerText;
            string ability_icon = "";
            var icon_text = ability["ability_icon"];
            if (icon_text != null)
            {
                ability_icon = icon_text.InnerText;
            }
            AbilityXMLDataEntry data = new AbilityXMLDataEntry(ability_ID, ability_name, default_library, ability_icon);

            DoAbilityData(ability["ability_data"], data);

            var tooltip_list_node = ability["tooltip_list"];
            if (tooltip_list_node != null)
            {
                var tooltip = tooltip_list_node["default"];
                if (tooltip != null)
                {
                    data.tooltips[(int)Ability.AbilityUpgradeSlot.DEFAULT] = tooltip.InnerText;
                }
                tooltip = tooltip_list_node["red"];
                if (tooltip != null)
                {
                    data.tooltips[(int)Ability.AbilityUpgradeSlot.RED] = tooltip.InnerText;
                }
                tooltip = tooltip_list_node["yellow"];
                if (tooltip != null)
                {
                    data.tooltips[(int)Ability.AbilityUpgradeSlot.YELLOW] = tooltip.InnerText;
                }
                tooltip = tooltip_list_node["blue"];
                if (tooltip != null)
                {
                    data.tooltips[(int)Ability.AbilityUpgradeSlot.BLUE] = tooltip.InnerText;
                }

            }

            foreach (XmlElement targetData in ability["targeting_list"].ChildNodes)
            {
                data.targetingData.Add(new AbilityXMLTargetingData(targetData, default_library));
            }

            var var_list_node = ability["var_list"];
            if (var_list_node != null)
            {
                foreach (XmlElement variable in var_list_node.ChildNodes)
                {
                    data.vars.Add(new AbilityXMLVariable(variable));
                }
            }

            var ability_tag_node = ability["ability_tags"];
            if (ability_tag_node != null)
            {
                foreach (XmlElement tag in ability_tag_node.ChildNodes)
                {
                    data.ability_tags.Add(tag.InnerText);
                }
            }

            var ability_blocked_tag_node = ability["ability_tags_to_block"];
            if (ability_blocked_tag_node != null)
            {
                foreach (XmlElement tag in ability_blocked_tag_node.ChildNodes)
                {
                    data.ability_blocked_tags.Add(tag.InnerText);
                }
            }

            var ability_applied_tag_node = ability["ability_tags_to_apply"];
            if (ability_applied_tag_node != null)
            {
                foreach (XmlElement tag in ability_applied_tag_node.ChildNodes)
                {
                    data.ability_applied_tags.Add(tag.InnerText);
                }
            }

            table.Add(ability_ID, data);
        }
        return true;
    }
    private void DoAbilityData(XmlElement ability_data_node, AbilityXMLDataEntry data)
    {
        var isPassive_node = ability_data_node["isPassive"];
        if (isPassive_node != null) 
        {
            data.isPassive = bool.Parse(isPassive_node.InnerText);
        }

        var cooldown_node = ability_data_node["cd"];
        if(cooldown_node == null)
        {
            cooldown_node = ability_data_node["cooldown"];
        }
        if (cooldown_node != null)
        {
            data.cooldown = float.Parse(cooldown_node.InnerText);
        }

        var duration_node = ability_data_node["duration"];
        if (duration_node == null)
        {
            duration_node = ability_data_node["maxDuration"];
        }
        if (duration_node != null)
        {
            data.duration = float.Parse(duration_node.InnerText);
        }

        var doesTicking_node = ability_data_node["doesTicking"];
        if (doesTicking_node != null)
        {
            data.doesTicking = bool.Parse(doesTicking_node.InnerText);
        }

        var recasts_node = ability_data_node["recasts"];
        if (recasts_node == null)
        {
            recasts_node = ability_data_node["recastableCount"];
        }
        if (recasts_node != null)
        {
            data.recasts = int.Parse(recasts_node.InnerText);
        }

        var recast_window_node = ability_data_node["recastWindow"];
        if (recast_window_node != null)
        {
            data.recastWindow = float.Parse(recast_window_node.InnerText);
        }
    }

    private Dictionary<string, Func<Ability, string, object>> evaluationMethods = new Dictionary<string, Func<Ability, string, object>>()
    {
        {
            "int", (a, s) =>
            {
                return int.Parse(s);
            }
        },

        {
            "float", (a, s) =>
            {
                return float.Parse(s);
            }
        },

        {
            "bool", (a, s) =>
            {
                return bool.Parse(s);
            }
        },

        {
            "kb", (a, s) =>
            {
                s = s.ToLower();
                if(s == "tiny")
                {
                    return KnockbackPreset.TINY;
                }
                else if(s == "little")
                {
                    return KnockbackPreset.LITTLE;
                }
                else if(s == "medium")
                {
                    return KnockbackPreset.MEDIUM;
                }
                else if(s == "big")
                {
                    return KnockbackPreset.BIG;
                }
                return KnockbackPreset.MAX;
            }
        },
        {
            "asset", (a, s) =>
            {
                Tuple<string, string> path = AssetLibraryManager.instance.ParseStringToCategoryAndName(s, a.defaultAssetLibraryName);
                return AssetLibraryManager.instance.GetPrefab(path.Item1, path.Item2);
            }
        },
        {
            "scriptableobject", (a, s) =>
            {
                Tuple<string, string> path = AssetLibraryManager.instance.ParseStringToCategoryAndName(s, a.defaultAssetLibraryName);
                return AssetLibraryManager.instance.GetScriptableObject(path.Item1, path.Item2);
            }
        },
    };

    private static Dictionary<string, string> commonVariableRenames = new Dictionary<string, string>()
    {
        { "cooldown", "maxCooldown" },
        { "cd", "maxCooldown" },
        { "dmg", "damage" },
        { "duration", "maxDuration:currentDuration" },
        { "dur", "maxDuration:currentDuration" },
        { "recasts", "numberTimesRecastable" },
        { "recastNum", "numberTimesRecastable" },
    };

    private static Dictionary<string, Ability.AbilityUpgradeSlot> abilityUpgradeLookup = new Dictionary<string, Ability.AbilityUpgradeSlot>()
    {
        { "default", Ability.AbilityUpgradeSlot.DEFAULT },
        { "red", Ability.AbilityUpgradeSlot.RED },
        { "blue", Ability.AbilityUpgradeSlot.BLUE },
        { "yellow", Ability.AbilityUpgradeSlot.YELLOW },
    };

    public static string[] CommonRenames(string startingName)
    {
        if (commonVariableRenames.ContainsKey(startingName))
        {
            return commonVariableRenames[startingName].Split(':');
        }
        else
        {
            return new string[] { startingName };
        }
    }

    /// <summary>
    /// Helper method to check if we have already setup our table
    /// </summary>
    /// <returns>True if the table is loaded, false otherwise</returns>
    public bool XMLLoaded()
    {
        return loadedTable;
    }

    /// <summary>
    /// Helper method to determine if we have this ability in the XML at all
    /// </summary>
    /// <param name="ID">The ID of the method (0 is never a valid ID)</param>
    /// <returns>True if the XML is loaded and we have a valid ID</returns>
    public bool HasEntryFor(uint ID)
    {
        return loadedTable ? table.ContainsKey(ID) : false;
    }


    /// <summary>
    /// Helper method to determine if we have something in the xml that will be overriding the given field
    /// </summary>
    /// <param name="ID">The ID of the ability to check</param>
    /// <param name="fieldName">The string name of the field</param>
    /// <returns>True if we found a valid ability with that field override, false otherwise</returns>
    public string HasFieldInEntry(uint ID, string fieldName)
    {
        AbilityXMLDataEntry entry = null;
        if (ID == 0 || !loadedTable || !table.TryGetValue(ID, out entry))
        {
            return "";
        }
        if(fieldName.StartsWith("_"))
        {
            fieldName = fieldName.Substring(1);
        }
        if(fieldName.EndsWith("k__BackingField"))
        {
            fieldName = fieldName.Replace("k__BackingField", "");
            fieldName = fieldName.Substring(1, fieldName.Length - 2); //cuts of the starting and trailing angle brackets
        }
        foreach(var e in entry.vars)
        {
            var nameArray = CommonRenames(e.name);
            foreach (var s in nameArray)
            {
                if(s == fieldName)
                {
                    return e.value;
                }
            }
        }
        return "";
    }

    public bool UpdateAbilityData(Ability a)
    {
        AbilityXMLDataEntry entry;
        if (a.ID == 0 || !table.TryGetValue(a.ID, out entry))
        {
            Debug.LogError("ERROR: FAILED TO FIND XML DATA FOR ABILITY:\"" + a.GetType().Name + "\"");
            return false;
        }
        a.abilityName = entry.name;
        a.defaultAssetLibraryName = entry.defaultLibrary;
        if (entry.iconPath == "null")
        {
            a.icon = null;
        }
        else
        {
            a.icon = AssetLibraryManager.instance.GetIcon(entry.iconPath, entry.defaultLibrary);
        }

        if(entry.isPassive.HasValue)
        {
            a.isPassive = entry.isPassive.Value;
        }

        if(entry.cooldown.HasValue) 
        {
            a.maxCooldown = entry.cooldown.Value;
        }

        if (entry.duration.HasValue)
        {
            a.maxDuration = entry.duration.Value;
            a.ResetDuration();
        }

        if(entry.doesTicking.HasValue)
        {
            a.isTickingAbilityOverride = entry.doesTicking.Value;
        }

        if(entry.recasts.HasValue)
        {
            a.numberTimesRecastable = entry.recasts.Value;
            a.recastWindow = entry.recastWindow.Value;
        }

        for (int x = 0; x < entry.tooltips.Length; ++x)
        {
            a.SetTooltip((Ability.AbilityUpgradeSlot)x, entry.tooltips[x]);
        }
        
        int n = entry.targetingData.Count;
        var targetingData = new AbilityTargetingData[n];
        for (int x = 0; x < n; x++)
        {
            targetingData[x] = entry.targetingData[x].targetingData;
        }
        a.SetupTargetingData(targetingData);
        
        foreach (AbilityXMLVariable variable in entry.vars)
        {
            var nameArray = CommonRenames(variable.name);
            foreach (var s in nameArray)
            {
                DoField(a, s, variable, entry);
            }
        }

        foreach(string tag in entry.ability_tags)
        {
            a.abilityTags.AddTagPermanent(GameplayTagFlags.TagLookup[tag]);
        }
        foreach (string tag in entry.ability_blocked_tags)
        {
            a.tagsToBlock.AddTagPermanent(GameplayTagFlags.TagLookup[tag]);
        }
        foreach (string tag in entry.ability_applied_tags)
        {
            a.tagsToApply.AddTagPermanent(GameplayTagFlags.TagLookup[tag]);
        }

        a.OnAbilityDataUpdated();
        return true;
    }

    private void DoField(Ability a, string name, AbilityXMLVariable variable, AbilityXMLDataEntry entry)
    {
        Func<Ability, string, object> evaluator = evaluationMethods[variable.type.ToLower()];
        if (evaluator == null)
        {
            Debug.LogError(string.Format("ERROR: FAILED TO EVALUATE PROPERTY ORIG:\"{0}\" RENAMED:\"{1}\" WITH TYPE \"{2}\" ON ABILITY \"{3}\" FROM DATA NAMED \"{4}\"", variable.name, name, variable.type, a.GetType().Name, entry.name));
            return;
        }
        if(!SetValueInHierarchy(a, a.GetType(), name, evaluator(a, variable.value)))
        {
            Debug.LogError(string.Format("ERROR: FAILED TO FIND PROPERTY/FIELD ORIG:\"{0}\" RENAMED:\"{1}\" WITH TYPE \"{2}\" ON ABILITY \"{3}\" FROM DATA NAMED \"{4}\"", variable.name, name, variable.type, a.GetType().Name, entry.name));
            return;
        }
    }

    private bool SetValueInType(object target, Type t, string name, object value)
    {
        var prop = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null)
        {
            prop.SetValue(target, value);
            return true;
        }
        var field = t.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
            return true;
        }
        return false;
    }

    private bool SetValueInHierarchy(object target, Type t, string name, object value)
    {
        if(SetValueInType(target, t, name, value))
        {
            return true;
        }
        foreach (var intf in t.GetInterfaces())
        {
            if(SetValueInType(target, intf, name, value))
            {
                return true;
            }
        }
        if (t.BaseType != null)
        {
            return SetValueInHierarchy(target, t.BaseType, name, value);
        }
        return false;
    }
}

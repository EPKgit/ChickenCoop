using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Targeting;

public class AbilityDataXMLParser : Singleton<AbilityDataXMLParser>
{
    private class AbilityXMLDataEntry
    {
        public uint ID;
        public string name;
        public List<AbilityXMLTooltip> tooltips;
        public List<AbilityXMLTargetingData> targetingData;
        public List<AbilityXMLVariable> vars;
        public AbilityXMLDataEntry(uint i, string n)
        {
            ID = i;
            name = n;
            targetingData = new List<AbilityXMLTargetingData>();
            tooltips = new List<AbilityXMLTooltip>();
            vars = new List<AbilityXMLVariable>();
        }
    }

    private class AbilityXMLTooltip
    {
        public string type;
        public string text;

        private AbilityXMLTooltip() { }
        public AbilityXMLTooltip(XmlElement node)
        {
            type = node.GetAttribute("type");
            text = node.GetAttribute("text");
        }
    }

    private class AbilityXMLTargetingData
    {
        public AbilityTargetingData targetingData;
        private AbilityXMLTargetingData() { }
        public AbilityXMLTargetingData(XmlElement node)
        {
            targetingData = new AbilityTargetingData();
            switch(node.Name)
            {
                case "noneTargetingDataType":
                {
                    targetingData.targetType = TargetType.NONE;
                    targetingData.range = float.Parse(node["range"].InnerText);
                    targetingData.previewScale = new Vector3(float.Parse(node["width"].InnerText), 0, 0);
                } break;

                case "lineTargetingData":
                {
                    targetingData.targetType = TargetType.LINE_TARGETED;
                    targetingData.range = float.Parse(node["range"].InnerText);
                    targetingData.previewScale = new Vector3(float.Parse(node["width"].InnerText), 0, 0);
                } break;

                case "groundTargetingData":
                {
                    targetingData.targetType = TargetType.GROUND_TARGETED;
                    targetingData.range = float.Parse(node["range"].InnerText);
                    var size = node["size"];
                    targetingData.previewScale = new Vector3(float.Parse(size["x"].InnerText), float.Parse(size["y"].InnerText), 0);
                } break;

                case "entityTargetingData":
                {
                    targetingData.targetType = TargetType.ENTITY_TARGETED;
                    targetingData.range = float.Parse(node["range"].InnerText);
                    targetingData.affiliation = (Targeting.Affiliation)Enum.Parse(typeof(Targeting.Affiliation), node["affiliation"].InnerText);
                } break;
            }

            //TODO do custom preview parsing
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
            name = node.GetAttribute("name");
            value = node.GetAttribute("value");
            type = node.GetAttribute("type");
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
            foreach(Ability a in player.GetComponent<PlayerAbilities>().abilities)
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
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(file.text);

        XmlNodeList list = doc.SelectNodes("//comment()");
        foreach (XmlNode node in list)
        {
            node.ParentNode.RemoveChild(node);
        }
        var root = doc["ability_list"];
        foreach (XmlElement ability in root.ChildNodes)
        {
            uint ability_ID = Convert.ToUInt32(ability["ability_ID"].InnerText);
            if (table.ContainsKey(ability_ID))
            {
                Debug.LogError("ERROR: duplicate ability id found for '" + ability["ability_name"].InnerText + "' ID:" + ability_ID);
                return false;
            }
            string ability_name = ability["ability_name"].InnerText;
            AbilityXMLDataEntry data = new AbilityXMLDataEntry(ability_ID, ability_name);
            foreach (XmlElement tooltip in ability["tooltip_list"].ChildNodes)
            {
                data.tooltips.Add(new AbilityXMLTooltip(tooltip));
            };
            foreach (XmlElement targetData in ability["targeting_list"].ChildNodes)
            {
                data.targetingData.Add(new AbilityXMLTargetingData(targetData));
            };
            foreach (XmlElement variable in ability["var_list"].ChildNodes)
            {
                data.vars.Add(new AbilityXMLVariable(variable));
            }
            table.Add(ability_ID, data);
        }
        return true;
    }


    private Dictionary<string, Func<string, object>> evaluationMethods = new Dictionary<string, Func<string, object>>()
    {
        {
            "int", (s) =>
            {
                return int.Parse(s);
            }
        },

        {
            "float", (s) =>
            {
                return float.Parse(s);
            }
        },
        {
            "kb", (s) =>
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
            "asset", (s) =>
            {
                int index = s.IndexOf('.');
                string category = s.Substring(0, index);
                string name = s.Substring(index + 1);
                return AssetLibraryManager.instance.RequestPrefab(name, category);
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
        AbilityXMLDataEntry entry = null;
        if (a.ID == 0 || !table.TryGetValue(a.ID, out entry))
        {
            Debug.LogError("ERROR: FAILED TO FIND XML DATA FOR ABILITY:\"" + a.GetType().Name + "\"");
            return false;
        }
        a.abilityName = entry.name;
        foreach(AbilityXMLTooltip tooltip in entry.tooltips)
        {
            a.SetTooltip(abilityUpgradeLookup[tooltip.type], tooltip.text);
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

        a.OnAbilityDataUpdated();
        return true;
    }

    private void DoField(Ability a, string name, AbilityXMLVariable variable, AbilityXMLDataEntry entry)
    {
        Func<string, object> evaluator = evaluationMethods[variable.type.ToLower()];
        if (evaluator == null)
        {
            Debug.LogError(string.Format("ERROR: FAILED TO EVALUATE PROPERTY ORIG:\"{0}\" RENAMED:\"{1}\" WITH TYPE \"{2}\" ON ABILITY \"{3}\" FROM DATA NAMED \"{4}\"", variable.name, name, variable.type, a.GetType().Name, entry.name));
            return;
        }
        if(!SetValueInHierarchy(a, a.GetType(), name, evaluator(variable.value)))
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

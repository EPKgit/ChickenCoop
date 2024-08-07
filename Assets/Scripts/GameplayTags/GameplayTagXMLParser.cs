using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR_WIN
using UnityEngine.Windows;
using System.Text;
#endif

namespace GameplayTagInternals
{

public class GameplayTagXMLParser : Singleton<GameplayTagXMLParser>
{
    private class GameplayTagTreeNode
    {
        public bool isRoot = false;
        public string fullName;
        public string relativeName;
        public GameplayTagTreeNode parent;
        public List<GameplayTagTreeNode> children;
        public GameplayTagTreeNode(GameplayTagTreeNode parent, string relativeName)
        {
            this.parent = parent;
            children = new List<GameplayTagTreeNode>();
            this.relativeName = relativeName;
            if(parent == null || parent.isRoot)
            {
                fullName = relativeName;
                return;
            }
            fullName = parent.fullName + "." + relativeName;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ParseXMLData();
    }

    const string XMLPath = "GameplayTags";
    bool loadedTable = false;
    private GameplayTagTreeNode treeRoot;

    public void ForceReimport()
    {
        loadedTable = false;
        ParseXMLData();
    }

    TextAsset GetFile()
    {
        if (loadedTable)
        {
            return null;
        }
        loadedTable = false;
        var file = Resources.Load<TextAsset>(XMLPath);
        if (file == null)
        {
            Debug.LogAssertion("ERROR: COULD NOT OPEN ABILITY XML");
        }
        return file;
    }
    
    void ParseXMLData()
    {
        TextAsset file = GetFile();
        if(file == null)
        {
            return;
        }
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(file.text);

        XmlNodeList list = doc.SelectNodes("//comment()");
        foreach (XmlNode node in list)
        {
            node.ParentNode.RemoveChild(node);
        }
        treeRoot = new GameplayTagTreeNode(null, "NONE");
        treeRoot.isRoot = true;
        ParseNode(doc["root"], treeRoot);
        loadedTable = true;
    }

    void ParseNode(XmlElement node, GameplayTagTreeNode parent)
    {
        foreach (XmlElement tagDef in node.ChildNodes)
        {
            GameplayTagTreeNode child = new GameplayTagTreeNode(parent, tagDef.LocalName);
            parent.children.Add(child);
            ParseNode(tagDef, child);
        }
    }

#if UNITY_EDITOR_WIN
    const string TagEnumFilePath = "Assets/Scripts/GameplayTags/Generated_GameplayFlags.cs";
    const string TagEnumXSDFilePath = "Assets/Resources/AbilityData/Generated_GameplayFlags.xsd";
    public void GenerateFiles()
    {
        StringBuilder enumBuilder = new StringBuilder();
        StringBuilder dictBuilder = new StringBuilder();
        dictBuilder.AppendLine("\tpublic static readonly Dictionary<string, string> TagLookup = new Dictionary<string, string>()");
        dictBuilder.AppendLine("\t{");
        TraverseForEnumGeneration(dictBuilder, enumBuilder, treeRoot, 1);
        dictBuilder.AppendLine("\t};");
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("//this file is generated by GameplayTagXMLParser");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("public static class GameplayTagFlags");
        builder.AppendLine("{");
        builder.Append(dictBuilder.ToString());
        builder.Append(enumBuilder.ToString());
        builder.AppendLine("}");

        File.WriteAllBytes(TagEnumFilePath, Encoding.ASCII.GetBytes(builder.ToString().ToCharArray()));

        builder.Clear();
        builder.AppendLine("<?xml version=\"1.0\"?>");
        builder.AppendLine("<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\">");
        builder.AppendLine("\t<!--this file is generated by GameplayTagXMLParser -->");
        builder.AppendLine("\t<xs:simpleType name=\"gameplayTagType\">");
        builder.AppendLine("\t\t<xs:restriction base=\"xs:string\">");
        TraverseForXSDGeneration(builder, treeRoot, 3);
        builder.AppendLine("\t\t</xs:restriction>");
        builder.AppendLine("\t</xs:simpleType>");
        builder.AppendLine("</xs:schema>");
        
        File.WriteAllBytes(TagEnumXSDFilePath, Encoding.ASCII.GetBytes(builder.ToString().ToCharArray()));

        EditorUtility.RequestScriptReload();
    }

    void TraverseForEnumGeneration(StringBuilder dictBuilder, StringBuilder builder, GameplayTagTreeNode node, int tabCount)
    {
        dictBuilder.AppendLine(string.Format("\t\t{{ \"{0}\", \"{1}\" }},", node.relativeName.ToUpper(), node.fullName));
        builder.AppendLine(string.Format("{0}public static string {1} {{ get => \"{2}\"; }}", new string('\t', tabCount), node.relativeName.ToUpper(), node.fullName));
        foreach(var child in node.children)
        {
            TraverseForEnumGeneration(dictBuilder, builder, child, tabCount + 1);
        }
    }

    void TraverseForXSDGeneration(StringBuilder builder, GameplayTagTreeNode node, int tabCount)
    {
        builder.AppendLine(string.Format("{0}<xs:enumeration value=\"{1}\"/>", new string('\t', tabCount), node.relativeName.ToUpper(), node.fullName));
        foreach (var child in node.children)
        {
            TraverseForXSDGeneration(builder, child, tabCount + 1);
        }
    }
#endif

    /// <summary>
    /// Helper method to check if we have already setup our table
    /// </summary>
    /// <returns>True if the table is loaded, false otherwise</returns>
    public bool XMLLoaded()
    {
        return loadedTable;
    }

    public List<string> GetRawTagList()
    {
        List<string> ret = new List<string>();
        TraverseTreeForTagList(treeRoot, ret);
        return ret;
    } 
    private void TraverseTreeForTagList(GameplayTagTreeNode node, List<string> ret)
    {
        ret.Add(node.fullName);
        foreach(var child in node.children)
        {
            TraverseTreeForTagList(child, ret);
        }
    }
}
}
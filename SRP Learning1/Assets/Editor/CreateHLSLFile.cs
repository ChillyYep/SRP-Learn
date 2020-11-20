using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.ProjectWindowCallback;
using System.Text;

public class CreateHLSLFile
{

    [MenuItem("Assets/Create/Script/HLSLFile")]
    public static void CreateFile()
    {
        string locationPath = GetSelectedPathOrFallback();
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<AfterEditHLSLFileName>(), locationPath + "/Default.hlsl", null, null);
    }
    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
}
class AfterEditHLSLFileName : EndNameEditAction
{
    private const string HLSLTemplate = @"#ifndef {0}_INCLUDED
#define {0}_INCLUDED

#endif";
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        var obj = CreateSCriptAssetFromTemplate(pathName);
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }
    internal UnityEngine.Object CreateSCriptAssetFromTemplate(string pathName)
    {
        string fullPath = Path.GetFullPath(pathName);
        UTF8Encoding encoding = new UTF8Encoding(true, false);
        using (StreamWriter streamWriter = new StreamWriter(fullPath, false, encoding))
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
            streamWriter.Write(string.Format(HLSLTemplate, fileNameWithoutExtension.ToUpper()));
        }
        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
    }
}
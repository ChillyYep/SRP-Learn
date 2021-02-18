using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 扩展Material Inspector
/// </summary>
public class CustomShaderGUI : ShaderGUI
{
    MaterialEditor editor;
    Object[] materials;
    MaterialProperty[] properties;
    bool Clipping
    {
        set => SetProperty("_Clipping", "_CLIPPING", value);
    }
    bool PremultiplyAlpha
    {
        set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
    }
    BlendMode srcBlend
    {
        set => SetProperty("_SrcBlend", (float)value);
    }
    BlendMode dstBlend
    {
        set => SetProperty("_DstBlend", (float)value);
    }
    bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }
    RenderQueue renderQueue
    {
        set
        {
            foreach (Material m in materials)
            {
                m.renderQueue = (int)value;
            }
        }
    }
    bool HasProperty(string name) => FindProperty(name, properties, false) != null;
    bool HasPremultiplyAlpga => HasProperty("_PremulAlpha");
    bool showPresets;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        editor = materialEditor;
        this.properties = properties;
        materials = materialEditor.targets;
        EditorGUILayout.Space();
        showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);
        if (showPresets)
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }
    }
    bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }
        return false;
    }
    void OpaquePreset()
    {
        if (PresetButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            srcBlend = BlendMode.One;
            dstBlend = BlendMode.Zero;
            ZWrite = true;
            renderQueue = RenderQueue.Geometry;
        }
    }
    void ClipPreset()
    {
        if (PresetButton("Clip"))
        {
            Clipping = true;
            PremultiplyAlpha = false;
            srcBlend = BlendMode.One;
            dstBlend = BlendMode.Zero;
            ZWrite = true;
            renderQueue = RenderQueue.AlphaTest;
        }
    }
    void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            srcBlend = BlendMode.SrcAlpha;
            dstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            renderQueue = RenderQueue.Transparent;
        }
    }
    void TransparentPreset()
    {
        if (HasPremultiplyAlpga && PresetButton("Transparent"))
        {
            Clipping = false;
            PremultiplyAlpha = true;
            srcBlend = BlendMode.One;
            dstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            renderQueue = RenderQueue.Transparent;
        }
    }
    bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, properties, false);
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }
        return false;
    }
    void SetProperty(string name, string keyword, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }
    }
    void SetKeyword(string keyword, bool enabled)
    {
        if (enabled)
        {
            foreach (Material m in materials)
            {
                m.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material m in materials)
            {
                m.DisableKeyword(keyword);
            }
        }
    }
}

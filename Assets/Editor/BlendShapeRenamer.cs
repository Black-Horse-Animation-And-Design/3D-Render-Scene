using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BlendshapeRenamer : EditorWindow
{
    AnimationClip clip;
    Vector2 scroll;

    List<string> originalNames = new List<string>();
    List<string> newNames = new List<string>();
    List<EditorCurveBinding> bindings = new List<EditorCurveBinding>();
    List<AnimationCurve> curves = new List<AnimationCurve>();

    [MenuItem("Tools/Blendshape Renamer")]
    static void Open()
    {
        GetWindow<BlendshapeRenamer>("Blendshape Renamer");
    }

    void OnGUI()
    {
        clip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", clip, typeof(AnimationClip), false);

        if (clip != null && GUILayout.Button("Load Blendshapes"))
        {
            LoadBlendshapes();
        }

        if (originalNames.Count > 0)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            for (int i = 0; i < originalNames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(originalNames[i], GUILayout.Width(200));
                newNames[i] = EditorGUILayout.TextField(newNames[i]);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Apply Rename"))
            {
                ApplyRename();
            }
        }
    }

    void LoadBlendshapes()
    {
        originalNames.Clear();
        newNames.Clear();
        bindings.Clear();
        curves.Clear();

        var curveBindings = AnimationUtility.GetCurveBindings(clip);

        foreach (var b in curveBindings)
        {
            if (b.propertyName.StartsWith("blendShape."))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, b);

                bindings.Add(b);
                curves.Add(curve);

                string shapeName = b.propertyName.Replace("blendShape.", "");
                originalNames.Add(shapeName);
                newNames.Add(shapeName);
            }
        }
    }

    void ApplyRename()
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            AnimationUtility.SetEditorCurve(clip, bindings[i], null);
        }

        for (int i = 0; i < bindings.Count; i++)
        {
            var b = bindings[i];
            b.propertyName = "blendShape." + newNames[i];
            AnimationUtility.SetEditorCurve(clip, b, curves[i]);
        }

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
    }
}
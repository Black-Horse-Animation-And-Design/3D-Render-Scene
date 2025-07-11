/*
Bake AO - Easy Ambient Occlusion Baking - A plugin for baking ambient occlusion (AO) textures in the Unity Editor.
by Procedural Pixels - Jan Mróz

Documentation: https://proceduralpixels.com/BakeAO/Documentation
Asset Store: https://assetstore.unity.com/packages/slug/263743 

Help: If the plugin is not working correctly, if there’s a bug, or if you need assistance and the documentation does not help, please contact me via Discord (https://discord.gg/NT2pyQ28Jx) or email (dev@proceduralpixels.com).
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEngine.Serialization;

namespace ProceduralPixels.BakeAO.Editor
{
    public class BakeAOSettings : SingletonScriptableObject<BakeAOSettings>
    {
        [System.Serializable]
        public struct RemapData
        {
            public Shader nonBakeAOShader;
            public Shader bakeAOShader;

            public RemapData(Shader nonBakeAOShader, Shader bakeAOShader)
            {
                this.nonBakeAOShader = nonBakeAOShader;
                this.bakeAOShader = bakeAOShader;
            }
        }

        static readonly LayerMask defaultMask = new LayerMask() { value = -1 };

        [SerializeField, FormerlySerializedAs("shadersThatSupportsBakeAO")]
        internal List<Shader> shadersThatSupportBakeAO = new List<Shader>();

        [SerializeField]
        internal List<RemapData> shaderRemap = new List<RemapData>();

        [SerializeField]
        internal List<LayerMask> layersInteraction = new List<LayerMask>() { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, };

        [SerializeField]
        internal bool bakeMaterialsForStaticGameObjects = false;

        public bool DoesObjectsInteract(int bakedObjectIndex, int otherObjectIndex)
        {
            while (layersInteraction.Count < 32)
            {
                layersInteraction.Add(-1);
                EditorUtility.SetDirty(this);
            }

            while (layersInteraction.Count > 32)
            {
                layersInteraction.RemoveAt(layersInteraction.Count - 1);
                EditorUtility.SetDirty(this);
            }

            if (bakedObjectIndex < 0 || bakedObjectIndex > 31 || otherObjectIndex < 0 || otherObjectIndex > 31)
                throw new InvalidOperationException("Layer index should be in range [0;31]");

            var interactLayers = layersInteraction[bakedObjectIndex];
            return (interactLayers & (1 << otherObjectIndex)) != 0;
        }

        public bool IsShaderSupported(Shader shader)
        {
            Validate();
            return shadersThatSupportBakeAO.Contains(shader);
        }

        public bool CanShaderBeRemapped(Shader shader)
        {
            Validate();
            return shaderRemap.Any(p => p.nonBakeAOShader == shader && p.bakeAOShader != null);
        }

        public void MarkShaderAsSupported(Shader shader)
        {
            Validate();
            if (shadersThatSupportBakeAO.Contains(shader))
                return;

            shadersThatSupportBakeAO.Add(shader);
        }

        public void MarkShaderAsUnsupported(Shader shader)
        {
            Validate();
            if (!shadersThatSupportBakeAO.Contains(shader))
                return;

            shadersThatSupportBakeAO.Remove(shader);
        }

        public void RemoveShaderFromSupported(Shader shader)
        {
            shadersThatSupportBakeAO.Remove(shader);
        }

        public void AddShaderRemap(Shader nonBakeAOShader, Shader bakeAOShader)
        {
            var remap = new RemapData(nonBakeAOShader, bakeAOShader);
            if (!shaderRemap.Contains(remap))
                shaderRemap.Add(new RemapData(nonBakeAOShader, bakeAOShader));
        }

        public void RemoveShaderRemap(Shader nonBakeAOShader, Shader bakeAOShader)
        {
            shaderRemap.Remove(new RemapData(nonBakeAOShader, bakeAOShader));
        }

        public bool CanUpdateMaterial(Material material)
        {
            return (AssetDatabase.IsMainAsset(material)) && shaderRemap.Any(entry => entry.nonBakeAOShader == material.shader);
        }

        public bool IsMaterialSupported(Material material)
        {
            return IsShaderSupported(material.shader);
        }

        public void UpdateMaterial(Material material)
        {
            Validate();

            if (!CanUpdateMaterial(material))
                throw new System.Exception("Cant update this material");

            if (!CanShaderBeRemapped(material.shader))
            {
                Debug.LogError($"Material shader can't be remapped", material);
                return;
            }

            Undo.RecordObject(material, "Change material shader");
            material.shader = shaderRemap.First(p => p.nonBakeAOShader == material.shader).bakeAOShader;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
        }

        private int lastValidationFrame = 0;

        private void Validate()
        {
            if (Time.renderedFrameCount == lastValidationFrame)
                return;

            lastValidationFrame = Time.renderedFrameCount;
            shaderRemap.RemoveAll(p => p.bakeAOShader == null || p.nonBakeAOShader == null);
            shadersThatSupportBakeAO.RemoveAll(s => s == null);
        }

        [MenuItem("Edit/Procedural Pixels/Bake AO/Mark selected shaders as supported by bake AO")]
        public static void MarkSelectedShadersAsSupportedByBakeAO()
        {
            // Filter shaders
            var shaders = Selection.objects.Where(o => o != null).Where(o => o is Shader).Select(o => (Shader)o);
            foreach (var shader in shaders)
                BakeAOSettings.Instance.MarkShaderAsSupported(shader);
        }

        [MenuItem("Edit/Procedural Pixels/Bake AO/Mark selected shaders as supported by bake AO", validate = true)]
        public static bool MarkSelectedShadersAsSupportedByBakeAO_Validate()
        {
            // Filter shaders
            return Selection.objects.Where(o => o != null).Count(o => o is Shader) > 0;
        }

        [MenuItem("Edit/Procedural Pixels/Bake AO/Mark selected shaders as unsupported by bake AO")]
        public static void MarkSelectedShadersAsUnsupportedByBakeAO()
        {
            // Filter shaders
            var shaders = Selection.objects.Where(o => o != null).Where(o => o is Shader).Select(o => (Shader)o);
            foreach (var shader in shaders)
                BakeAOSettings.Instance.MarkShaderAsUnsupported(shader);
        }

        [MenuItem("Edit/Procedural Pixels/Bake AO/Mark selected shaders as unsupported by bake AO", validate = true)]
        public static bool MarkSelectedShadersAsUnsupportedByBakeAO_Validate()
        {
            // Filter shaders
            return Selection.objects.Where(o => o != null).Count(o => o is Shader) > 0;
        }
    }
}
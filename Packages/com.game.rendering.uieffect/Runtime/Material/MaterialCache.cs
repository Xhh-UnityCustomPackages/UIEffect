using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    public class MaterialCache
    {
        static Dictionary<int, MaterialEntry> materialMap = new Dictionary<int, MaterialEntry>();

        private class MaterialEntry
        {
            public Material material;
            // public int referenceCount;

            public void Release()
            {
                if (material)
                {
                    Object.DestroyImmediate(material, false);
                }

                material = null;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ClearCache()
        {
            foreach (var entry in materialMap.Values)
            {
                entry.Release();
            }

            materialMap.Clear();
        }
#endif


        public static Material GetMaterial(int instanceID, Material baseMaterial, Graphic graphic)
        {
            MaterialEntry entry;
            if (!materialMap.TryGetValue(instanceID, out entry))
            {
                entry = new MaterialEntry()
                {
                    material = new Material(baseMaterial)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    },
                };
                entry.material.shader = Shader.Find("Hidden/UI/UI-Effect");
                entry.material.shaderKeywords = null;
                materialMap.Add(instanceID, entry);
            }
            // onModifyMaterial(material, graphic);
            return entry.material;
        }

    }
}

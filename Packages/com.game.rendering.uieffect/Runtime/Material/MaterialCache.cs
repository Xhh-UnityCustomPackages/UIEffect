using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;

namespace Game.Core.UIEffect
{
    public class MaterialCache
    {
        private static Dictionary<Hash128, MaterialEntry> materialMap = new();
        private static readonly StringBuilder s_StringBuilder = new();

        private class MaterialEntry
        {
            public Material material;
            public Hash128 hashCode;
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


        public static Material GetMaterial(Hash128 hashCode, Material baseMaterial, Graphic graphic, List<string> keywords)
        {
            Debug.LogError(hashCode.ToString());
            MaterialEntry entry;
            if (!materialMap.TryGetValue(hashCode, out entry))
            {
                entry = new MaterialEntry()
                {
                    material = new Material(baseMaterial)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    },
                    hashCode = hashCode
                };
                entry.material.shader = Shader.Find("Hidden/UI/UI-Effect");
                entry.material.shaderKeywords = null;
                SetShaderVariants(entry.material, keywords);
                materialMap.Add(hashCode, entry);
            }

            return entry.material;
        }

        private static void SetShaderVariants(Material newMaterial, List<string> variants)
        {
            var keywords = variants
                .Select(x => x.ToString().ToUpper())
                .Concat(newMaterial.shaderKeywords) //加上原来的
                .Distinct()
                .ToArray();

            newMaterial.shaderKeywords = keywords;

            // Add variant name
            s_StringBuilder.Clear();
            s_StringBuilder.Length = 0;
            s_StringBuilder.Append(newMaterial.shader.name);
            foreach (var keyword in keywords)
            {
                s_StringBuilder.Append("|");
                s_StringBuilder.Append(keyword);
            }

            newMaterial.name = s_StringBuilder.ToString();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace Game.Core.UIEffect.Editor
{
    public static class UIEffect_FieldInfo
    {
        public static Dictionary<int, List<FieldInfo>> FieldInfoList = new Dictionary<int, List<FieldInfo>>();


        public static int GetFieldInfo(BaseUIEffect target, out List<FieldInfo> fieldInfoList)
        {
            Type targetType = target.GetType();
            int targetTypeHashCode = targetType.GetHashCode();

            if (!FieldInfoList.TryGetValue(targetTypeHashCode, out fieldInfoList))
            {
                IList<Type> typeTree = targetType.GetBaseTypes();
                fieldInfoList = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic)
                    .OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
                    .ToList();

                for (int i = fieldInfoList.Count - 1; i >= 0; i--)
                {
                    if (!fieldInfoList[i].IsDefined(typeof(SerializeField)))
                        fieldInfoList.RemoveAt(i);
                }


                FieldInfoList.Add(targetTypeHashCode, fieldInfoList);
            }

            return fieldInfoList.Count;
        }

        public static int GetFieldInfo(UnityEngine.Object target, out List<FieldInfo> fieldInfoList)
        {
            Type targetType = target.GetType();
            int targetTypeHashCode = targetType.GetHashCode();

            if (!FieldInfoList.TryGetValue(targetTypeHashCode, out fieldInfoList))
            {
                IList<Type> typeTree = targetType.GetBaseTypes();
                fieldInfoList = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic)
                    .OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
                    .ToList();
                FieldInfoList.Add(targetTypeHashCode, fieldInfoList);
            }

            return fieldInfoList.Count;
        }

        public static IList<Type> GetBaseTypes(this Type t)
        {
            var types = new List<Type>();
            while (t.BaseType != null)
            {
                types.Add(t);
                t = t.BaseType;
            }

            return types;
        }
    }
}

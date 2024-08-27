using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Game.Core.UIEffect
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIEffectPathAttribute : System.Attribute
    {
        public string Path;
        public string Name;

        public UIEffectPathAttribute(string path)
        {
            Path = path;
            Name = path.Split('/').Last();
        }

        static public string GetUIEffectDefaultName(System.Type type)
        {
            UIEffectPathAttribute attribute = type.GetCustomAttributes(false).OfType<UIEffectPathAttribute>().FirstOrDefault();
            return attribute != null ? attribute.Name : type.Name;
        }

        static public string GetUIEffectDefaultPath(System.Type type)
        {
            UIEffectPathAttribute attribute = type.GetCustomAttributes(false).OfType<UIEffectPathAttribute>().FirstOrDefault();
            return attribute != null ? attribute.Path : null;
        }
    }
}

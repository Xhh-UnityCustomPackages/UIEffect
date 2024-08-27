using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    public abstract class BaseMaterialEffect : BaseUIEffect
    {
        public abstract string MaterialKeyWord { get; }
        public void SetMaterialDirty()
        {
        }

        public virtual void ModifyMaterial(Material newMaterial)
        {

        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIFill")]
    public class UIFill : BaseMaterialEffect
    {
        public override string MaterialKeyWord => "FILL";
    }
}
using UnityEngine;
using System.Collections;

public class TweenPosition : TweenBehaviour
{   
    public Vector3 to;

    protected override LTDescr OnPlay()
    {
        return LeanTween.move(this.target, this.to, this.time);
    }
}

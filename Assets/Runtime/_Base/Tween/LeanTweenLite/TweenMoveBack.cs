using UnityEngine;
using System.Collections;

public class TweenMoveBack : TweenBehaviour
{
    private Vector3 mOrigin = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        this.mOrigin = this.transform.position;
    }

    protected override LTDescr OnPlay()
    {
        return LeanTween.move(this.target, this.mOrigin, this.time);
    }
}

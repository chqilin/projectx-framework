using UnityEngine;

public class TweenScale : TweenBehaviour
{
    public Vector3 to = Vector3.one;

    private Vector3 mBaseScale = Vector3.one;

    protected override void Awake()
    {
        base.Awake();
        this.mBaseScale = this.target.transform.localScale;
    }

    protected override LTDescr OnPlay()
    {
        Vector3 scaleTo = Vector3.Scale(this.mBaseScale, this.to);
		LeanTween.cancel(this.target.gameObject);
		return LeanTween.scale(this.target, scaleTo, this.time);  
	}
}
                                                  
using System.Collections.Generic;
using UnityEngine;

public class TweenBehaviour : MonoBehaviour
{
    public GameObject target = null;
    public bool playOnStart = true;
    public LeanTweenType easeType = LeanTweenType.linear;
    public LeanTweenType loopType = LeanTweenType.once;
    public float time = 1.0f;

    private LTDescr mTween = null;
    private System.Action<object> onComplete = null;
    private object onCompleteParam = null;

    public LTDescr tween
    {
        get { return this.mTween; }
    }

    public void Play()
    {
        this.mTween = this.OnPlay();
        this.mTween.setEase(this.easeType);
        this.mTween.setLoopType(this.loopType);
        this.mTween.setOnComplete(this.OnComplete);
    }

    public void Stop()
    {
        this.OnStop();
    }

    public void Complete(System.Action<object> onComplete, object param = null)
    {
        this.onComplete = onComplete;
        this.onCompleteParam = param;
    }

    protected virtual void Awake()
    {
        if (this.target == null)
        {
            this.target = this.gameObject;
        }
    }

    protected virtual void Start()
    {       
        if(this.playOnStart)
        {
            this.Play();
        }
    }

    protected virtual LTDescr OnPlay()
    {
        return null;
    }

    protected virtual void OnStop()
    {
        LeanTween.cancel(this.target, this.mTween.uniqueId);
    }

    protected virtual void OnComplete()
    {
        if(this.onComplete != null)
        {
            this.onComplete(this.onCompleteParam);
        }
    }
}

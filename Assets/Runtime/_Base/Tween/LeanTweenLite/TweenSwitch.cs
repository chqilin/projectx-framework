using UnityEngine;
using System.Collections;
using ProjectX;

public class TweenSwitch : TweenBehaviour
{
    public Transform openPoint = null;
    public Transform closePoint = null;

    public bool isOpen { get; private set; }

    protected override void Start()
    {
        base.Start();
        this.Close();
    }

    protected override LTDescr OnPlay()
    {
        return this.Switch();
    }

    public LTDescr Switch()
    {
        if (this.isOpen) 
            return this.Close();
        else 
            return this.Open();
    }

    public LTDescr Open()
    {
        this.isOpen = true;
        LeanTween.cancel(this.target);
        return LeanTween.move(this.target, this.openPoint.position, this.time);
    }

    public LTDescr Close()
    {
        this.isOpen = false;
        LeanTween.cancel(this.target);
        return LeanTween.move(this.target, this.closePoint.position, this.time);
    }
}

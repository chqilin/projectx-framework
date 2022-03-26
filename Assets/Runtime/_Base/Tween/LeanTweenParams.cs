using UnityEngine;
using System.Collections;
/// <summary>
/// Class to configure LeanTween more easily
/// </summary>
public class LeanTweenParams
{
    /// <summary>
    /// 
    /// </summary>
	public LeanTweenType easeType = LeanTweenType.linear;

	public LeanTweenType loopType = LeanTweenType.once;

    /// <summary>
    /// 
    /// </summary>
    public AnimationCurve animationCurve;

    /// <summary>
    /// 
    /// </summary>
    public string onComplete;

    /// <summary>
    /// 
    /// </summary>
    public GameObject onCompleteTarget;

    /// <summary>
    /// 
    /// </summary>
    public object onCompleteParams;

    /// <summary>
    /// 
    /// </summary>
    public string onUpdate;

    /// <summary>
    /// 
    /// </summary>
    public GameObject onUpdateTarget;

    /// <summary>
    /// 
    /// </summary>
    public Hashtable onUpdateParams;

    /// <summary>
    /// 
    /// </summary>
    public float delay = 0;

    /// <summary>
    /// 
    /// </summary>
    public bool orientToPath = false;
	
	public LeanTweenParams ()
	{
		
	}
	/// <summary>
	/// Convert to HashTable for use with LeanTween
	/// </summary>
	/// <returns></returns>
	public Hashtable ToHash()
	{
		Hashtable hash = new Hashtable();
		
		hash.Add("delay",delay);

		hash.Add("loopType", loopType);
		
		if(animationCurve != null)
		{
			hash.Add("ease", animationCurve);
		}
		else
		{
			hash.Add("ease", easeType);
		}
		
		hash.Add("orientToPath",orientToPath);	
		
		if(onUpdate != null)
		{
            if (onUpdate != null)
            {
                hash.Add("onUpdate", onUpdate);
            }
			hash.Add("onUpdateTarget",onUpdateTarget);
		}

        if (onUpdateParams != null)
        {
            hash.Add("onUpdateParam", onUpdateParams);
        }
		
		if(onComplete != null)
		{
			hash.Add("onComplete",onComplete);
			hash.Add("onCompleteTarget",onCompleteTarget);
            hash.Add("onCompleteParam",onCompleteParams);
		}

        
		return hash;
	}
}
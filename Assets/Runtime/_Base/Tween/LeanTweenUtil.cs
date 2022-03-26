
using UnityEngine;
using ProjectX;

public class LeanTweenUtil
{
    public static LTDescr Throw(GameObject obj, Vector3 to, float time)
    {
        Vector3 p0 = obj.transform.position;
        Vector3 p2 = to;
        Vector3 p1 = (p0 + p2) * 0.5f + Vector3.up * (p2 - p0).magnitude * 0.5f;
        return Biezer(obj, p0, p1, p2, time);
    }

    public static LTDescr Biezer(GameObject obj, Vector3 p0, Vector3 p1, Vector3 p2, float time)
    {
        LTDescr tween = LeanTween.value(obj, 0, 1, time);
        tween.setOnUpdate(t =>
        {
            Vector3 p = XMath.SampleBezierCurve(p0, p1, p2, t);
            obj.transform.position = p;
        });
        return tween;
    }
}

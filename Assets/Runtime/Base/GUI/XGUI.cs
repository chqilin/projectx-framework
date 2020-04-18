using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region RectTransform Extensions
/*
-----------------------------Parent------------------------------
|                                                               |
|                                                               |
|   LTAnchor--------------------------------------RTAnchor      |
|      |                                             |          |
|      |                                             |          |
|      |      LTCorner-----------------RTCorner      |          |
|      |          |                       |          |          |
|      |          |                       |          |          |
|      |          |         Pivot         |          |          |
|      |          |                       |          |          |
|      |          |                       |          |          |
|      |      LBCorner-----------------RBCorner      |          |
|      |                                             |          |
|      |                                             |          |
|   LBAnchor--------------------------------------RBAnchor      |
|                                                               |
|                                                               |
-----------------------------------------------------------------

Rect: corners-rectange
Rect.xy = LBCorner
Rect.width = RBCorner - LBCorner
Rect.height = LTCorner - LBCorner

Anchor： some special point in parent such as Parent.LTCorner, Parent.RBCorner etc.
AnchorMin = LBAnchor / Parent.Size
AnchorMax = RTAnchor / Parent.Size

Offset: the offset from anchor to corner
OffsetMin = LBCorner - LBAnchor
OffsetMax = RTCorner - RTAnchor

AnchoredPosition = Pivot - (LBAnchor + RTAnchor) * 0.5f
*/
public static class RectTransformExtensions
{
    public static RectTransform GetParentRectTransform(this RectTransform _this)
    {
        if (_this.parent == null)
            return null;
        return _this.parent.GetComponentInParent<RectTransform>();
    }

    public static Vector2 GetSize(this RectTransform _this)
    {
        return _this.rect.size;
    }

    public static float GetWidth(this RectTransform _this)
    {
        return _this.rect.width;
    }

    public static float GetHeight(this RectTransform _this)
    {
        return _this.rect.height;
    }

    public static void SetWidth(this RectTransform _this, float newSizeX)
    {
        _this.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSizeX);
    }

    public static void SetHeight(this RectTransform _this, float newSizeY)
    {
        _this.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeY);
    }

    public static void SetSize(this RectTransform _this, Vector2 newSize)
    {
        SetSize(_this, newSize.x, newSize.y);
    }

    public static void SetSize(this RectTransform _this, float newSizeX, float newSizeY)
    {
        _this.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSizeX);
        _this.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeY);
    }

    public static void SetScreenPosition(this RectTransform _this, Vector3 screenpos)
    {
        Canvas canvas = _this.GetComponentInParent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 destpos = screenpos /= canvas.scaleFactor;
            RectTransform parent = _this.GetParentRectTransform();
            Vector2 anchorCenter = (_this.anchorMin + _this.anchorMax) * 0.5f;
            Vector2 centerPos = Vector2.Scale(parent.rect.size, anchorCenter);
            _this.anchoredPosition = destpos - centerPos;
        }
        else if (canvas.worldCamera != null)
        {
            screenpos.z = canvas.planeDistance;
            Vector3 worldpos = canvas.worldCamera.ScreenToWorldPoint(screenpos);
            _this.transform.position = worldpos;
        }
    }

    public static void SetBottomLeftPosition(this RectTransform _this, Vector2 newPos)
    {
        Vector3 lp = default(Vector3);
        lp.Set(
            newPos.x + (_this.pivot.x * _this.rect.width),
            newPos.y + (_this.pivot.y * _this.rect.height),
            _this.localPosition.z);
        _this.localPosition = lp;
    }

    public static void SetTopLeftPosition(this RectTransform _this, Vector2 newPos)
    {
        Vector3 lp = default(Vector3);
        lp.Set(
            newPos.x + (_this.pivot.x * _this.rect.width),
            newPos.y - ((1f - _this.pivot.y) * _this.rect.height),
            _this.localPosition.z);
        _this.localPosition = lp;
    }

    public static void SetBottomRightPosition(this RectTransform _this, Vector2 newPos)
    {
        Vector3 lp = default(Vector3);
        lp.Set(
            newPos.x - ((1f - _this.pivot.x) * _this.rect.width),
            newPos.y + (_this.pivot.y * _this.rect.height),
            _this.localPosition.z);
        _this.localPosition = lp;
    }

    public static void SetRightTopPosition(this RectTransform _this, Vector2 newPos)
    {
        Vector3 lp = default(Vector3);
        lp.Set(
            newPos.x - ((1f - _this.pivot.x) * _this.rect.width),
            newPos.y - ((1f - _this.pivot.y) * _this.rect.height),
            _this.localPosition.z);
        _this.localPosition = lp;
    }
}
#endregion

#region VerticalLayoutGroup Extensions
public static class VerticalLayoutGroupExtensions
{
    public static void ResizeForContent(this VerticalLayoutGroup _this)
    {
        RectTransform t = _this.GetComponent<RectTransform>();
        float h = ComputeContentHeight(_this);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }

    private static float ComputeContentHeight(VerticalLayoutGroup layout)
    {
        float height = 0;

        height += layout.spacing;
        for (int i = 0; i < layout.transform.childCount; i++)
        {
            Transform t = layout.transform.GetChild(i);
            if (!t.gameObject.activeSelf)
                continue;
            RectTransform rt = t.GetComponent<RectTransform>();
            if (rt == null)
                continue;
            height += layout.spacing;
            height += rt.rect.height;
        }
        height += layout.spacing;

        return height;
    }
}
#endregion

#region HorizontalLayoutGroup Extensions
public static class HorizontalLayoutGroupExtensions
{
    public static void ResizeForContent(this HorizontalLayoutGroup _this)
    {
        RectTransform t = _this.GetComponent<RectTransform>();
        float h = ComputeContentWidth(_this);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }

    private static float ComputeContentWidth(HorizontalLayoutGroup layout)
    {
        float width = 0;

        width += layout.spacing;
        for (int i = 0; i < layout.transform.childCount; i++)
        {
            Transform t = layout.transform.GetChild(i);
            if (!t.gameObject.activeSelf)
                continue;
            RectTransform rt = t.GetComponent<RectTransform>();
            if (rt == null)
                continue;
            width += layout.spacing;
            width += rt.rect.width;
        }
        width += layout.spacing;

        return width;
    }
}
#endregion

#region GridLayoutGroup Extensions
public static class GridLayoutGroupExtensions
{
    public static void ResizeForContent(GridLayoutGroup _this)
    {
        RectTransform t = _this.GetComponent<RectTransform>();
        float h = ComputeContentHeight(_this);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }

    private static float ComputeContentHeight(GridLayoutGroup layout)
    {
        float cellH = layout.cellSize.y;
        float spaceH = layout.spacing.y;

        int cells = GetActiveChildCount(layout.transform);
        int xCount = layout.constraintCount;
        int yCount = cells % xCount != 0 ? cells / xCount + 1 : cells / xCount;

        float height = (cellH + spaceH) * yCount + spaceH;
        return height;
    }

    private static int GetActiveChildCount(Transform t)
    {
        int count = 0;
        for (int i = 0; i < t.childCount; i++)
        {
            if (t.GetChild(i).gameObject.activeSelf)
                count++;
        }
        return count;
    }
} 
#endregion

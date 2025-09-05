using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalSnapScrollView : SnapScrollView
{

    protected override Vector2 GetSnappedContentPos()
        => new (-itemList[SnapIndex].anchoredPosition.x, scrollRect.content.anchoredPosition.y);

    protected override float GetMaxScrollDistance()
        => scrollRect.viewport.rect.width;

    protected override float GetItemDisToViewPort(Vector2 itemCenter, Vector2 viewPortCenter)
        => itemCenter.x - viewPortCenter.x;

}
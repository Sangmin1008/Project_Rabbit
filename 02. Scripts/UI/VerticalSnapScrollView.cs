using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalSnapScrollView : SnapScrollView
{
    protected override Vector2 GetSnappedContentPos() 
        => new (scrollRect.content.anchoredPosition.x, -itemList[SnapIndex].anchoredPosition.y);
    protected override float GetMaxScrollDistance() 
        => scrollRect.viewport.rect.height;

    protected override float GetItemDisToViewPort(Vector2 itemCenter, Vector2 viewPortCenter)
        => itemCenter.y - viewPortCenter.y;
}
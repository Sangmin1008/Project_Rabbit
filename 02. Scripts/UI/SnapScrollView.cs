using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(ScrollRect))]
public abstract class SnapScrollView : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    [Serializable]
    public struct SmoothOption
    {
        public bool enabled;
        public float moveSpeed;
    }
    
    [Serializable]
    public struct ScaleOption
    {
        public bool enabled;
        public Vector3 min;
        public Vector3 max;
    }
    
    [Serializable]
    public struct RotationOption
    {
        public bool enabled;
        public bool isMirror;
        public Vector3 min;
        public Vector3 max;
    }
    
    public event UnityAction<PointerEventData> OnBeginDragEvent;
    public event UnityAction<PointerEventData> OnEndDragEvent;
    
    public int SnapIndex;
    
    [Header("Component")]
    [SerializeField] protected ScrollRect scrollRect;
    [SerializeField] public List<RectTransform> itemList;

    [Header("Option")]
    [SerializeField] private SmoothOption smoothOption;
    [SerializeField] private ScaleOption scaleOption;
    [SerializeField] private RotationOption rotationOption;
    
    private Coroutine _lerpCoroutine;
    
    protected abstract Vector2 GetSnappedContentPos();
    protected abstract float GetMaxScrollDistance();
    protected abstract float GetItemDisToViewPort(Vector2 itemCenter, Vector2 viewPortCenter);

  
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_lerpCoroutine != null)
        {
            StopCoroutine(_lerpCoroutine);
        }
        
        OnBeginDragEvent?.Invoke(eventData);
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        SetSnapIndex();

        var targetPos = GetSnappedContentPos(); 
        _lerpCoroutine = StartCoroutine(SnapScrollRect(targetPos));
        
        OnEndDragEvent?.Invoke(eventData);
    }
    
    
    public void DirectUpdateItemList(int index)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        SnapIndex = index;
        
        for (int i = 0; i < itemList.Count; i++)
        {
            if (scaleOption.enabled)
            {
                Vector3 scale = i == SnapIndex ? scaleOption.max : scaleOption.min;
                itemList[i].localScale = scale;
            }

            if (rotationOption.enabled)
            {
                float dir = !rotationOption.isMirror ? 1 : i < SnapIndex ? -1 : 1;
                Vector3 rotation = i == SnapIndex ? rotationOption.max : rotationOption.min;
                itemList[i].localRotation = Quaternion.Euler(rotation * dir);
            }
        }
        
        scrollRect.content.anchoredPosition = GetSnappedContentPos();
    }


    void SetSnapIndex()
    {
        Vector2 viewportCenter = scrollRect.viewport.TransformPoint(scrollRect.viewport.rect.center);
        float maxDis = GetMaxScrollDistance()/ 2;
        float maxItemDisRatio = 0;
        int tempSnapIndex = SnapIndex;
        
        for (int i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            Vector2 itemCenter = item.TransformPoint(item.rect.center);

            float itemDis = GetItemDisToViewPort(itemCenter, viewportCenter);
            float itemDisNormalizedRatio = Mathf.Clamp01(1f -  Mathf.Abs(itemDis) / maxDis);

            if (itemDisNormalizedRatio > maxItemDisRatio)
            {
                maxItemDisRatio = itemDisNormalizedRatio;
                tempSnapIndex = i;
            }
        }

        SnapIndex = tempSnapIndex;
    }
    

    
    public void UpdateItemList()
    {
        //TransformPoint = 로컬좌표를 기준 글로벌좌표로 (마치 pivot 0.5, 0.5)
        
        Vector2 viewportCenter = scrollRect.viewport.TransformPoint(scrollRect.viewport.rect.center);
        float maxDis = GetMaxScrollDistance()/ 2;

        for (int i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            Vector2 itemCenter = item.TransformPoint(item.rect.center);
            float itemDis = GetItemDisToViewPort(itemCenter, viewportCenter);
            float itemDisNormalizedRatio = Mathf.Clamp01(1f -  Mathf.Abs(itemDis) / maxDis);

            if (Mathf.Abs(itemDis) < maxDis)
            {
                if (scaleOption.enabled)
                {
                    Vector3 scale = Vector3.Lerp(scaleOption.min, scaleOption.max, itemDisNormalizedRatio);
                    item.localScale = scale;
                }

                if (rotationOption.enabled)
                {
                    float dir = !rotationOption.isMirror ? 1 : itemDis > 0 ? 1 : -1;
                    Vector3 rotation = Vector3.Lerp(rotationOption.min, rotationOption.max, itemDisNormalizedRatio);
                    item.localRotation = Quaternion.Euler(rotation * dir);
                }
            }
        }
    }


    IEnumerator SnapScrollRect(Vector2 targetPos)
    {
        if (smoothOption.enabled)
        {
            while ((scrollRect.content.anchoredPosition - targetPos).sqrMagnitude > 0.01f)
            {
                scrollRect.content.anchoredPosition = Vector2.Lerp(scrollRect.content.anchoredPosition, targetPos, Time.unscaledDeltaTime * smoothOption.moveSpeed);
                yield return null;
            }
        }
        
        scrollRect.content.anchoredPosition = targetPos;
        _lerpCoroutine = null;
    }
}

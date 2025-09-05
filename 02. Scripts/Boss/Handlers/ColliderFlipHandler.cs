using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class ColliderFlipHandler : MonoBehaviour
{
    private PolygonCollider2D _col;
    private Vector2[][] _originalPaths;

    void Awake()
    {
        _col = GetComponent<PolygonCollider2D>();

        // 패스 개수만큼 배열 할당
        _originalPaths = new Vector2[_col.pathCount][];
        for (int i = 0; i < _col.pathCount; i++)
        {
            _originalPaths[i] = _col.GetPath(i);
        }
    }

    // X축 기준으로 콜라이더 포인트를 좌우 반전
    public void MirrorX(bool flip)
    {
        for (int i = 0; i < _originalPaths.Length; i++)
        {
            var src = _originalPaths[i];
            var dst = new Vector2[src.Length];
            for (int j = 0; j < src.Length; j++)
            {
                float x = flip ? -src[j].x : src[j].x;
                dst[j] = new Vector2(x, src[j].y);
            }
            _col.SetPath(i, dst);
        }
    }
}
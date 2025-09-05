using UnityEngine;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour
{
    [Tooltip("테두리 색상")]
    public Color color = Color.red;
    [Range(0, 16), Tooltip("테두리 두께")]
    public int outlineSize = 1;

    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _materialPropertyBlock;

    private void Awake()
    {
        //  부모에 SpriteRenderer가 있으면 그걸 쓰기
        _spriteRenderer = GetComponent<SpriteRenderer>();

        //  없으면 자식들 중에 찾기
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (_spriteRenderer == null)
        {
            Debug.LogError($"[{nameof(SpriteOutline)}] SpriteRenderer를 찾을 수 없습니다! " + "이 컴포넌트는 SpriteRenderer가 있는 오브젝트나 그 자식에 붙여야 합니다.");
        }

        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    //  테두리를 켜거나 끄는 메서드
    public void UpdateOutline(bool on)
    {
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat("_Outline", on ? 1f : 0f);
        _materialPropertyBlock.SetColor("_OutlineColor", color);
        _materialPropertyBlock.SetFloat("_OutlineSize", outlineSize);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}

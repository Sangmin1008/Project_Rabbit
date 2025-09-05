using UnityEngine;

public class SavePointInteractor : OutlineInteractable
{
    public override string GetInteractPrompt() => "Save Point";

    public override void Interact()
    {
        UIManager.Instance.Open<SaveLoadMenuUI>(true);
    }
}

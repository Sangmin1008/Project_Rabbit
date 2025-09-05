using UnityEngine;

public class BoardInteractor : OutlineInteractable
{
    [SerializeField] DialogSystem dialogSystem;
    public override string GetInteractPrompt() => "상호 작용";

    public override void Interact()
    {
        dialogSystem.AdvanceDialog();
    }
}
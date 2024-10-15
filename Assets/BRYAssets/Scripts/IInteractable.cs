using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public string InteractionPrompt { get; }

    public void InteractWithObjects(PlayerInteract interactor);
}

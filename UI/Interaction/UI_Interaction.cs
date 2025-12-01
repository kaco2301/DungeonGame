using UnityEngine;

public abstract class UI_Interaction : UI_Panel
{
    public Define.InteractionUIType type;
    public abstract void SetData(InteractableData data);
}

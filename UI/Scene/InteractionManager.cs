using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public interface IInteractable
{
    InteractableData GetData(); 
    float HoldDuration { get; }
    bool Interact(PlayerInteractor interactor);
    bool IsAvailable();
    event System.Action<IInteractable> OnRemoved;
}

public struct InteractableData
{
    public InteractionType InteractType;
    public InteractionUIType UIType;
    public Sprite Icon;
    public string InteractionText;
    public string ItemName;
}

public class InteractionManager
{
    private Dictionary<InteractionUIType, UI_Interaction> _lookup;
    private UI_Interaction _current;

    public InteractionManager(List<InteractionPanelEntry> entries)
    {
        _lookup = entries.Where(e => e.panel != null)
                         .ToDictionary(e => e.type, e => e.panel);
    }
    
    public void Show(InteractableData data)
    {
        Hide();

        if (!_lookup.TryGetValue(data.UIType, out var panel))
        {
            Debug.LogWarning($"No interaction panel for {data.UIType}");
            return;
        }

        _current = panel;
        _current.SetData(data);
        _current.Show();
    }

    public void Hide()
    {
        if (_current == null) return;
        _current.Hide();
        _current = null;
    }

    public void HideAll()
    {
        foreach(var entries in _lookup.Values)
        {
            entries.Hide();
        }
    }

    public void UpdateFill(float amount)
    {
        if (_current is UI_Prompt prompt)
            prompt.UpdateFill(amount);
    }

    public void CancelFill()
    {
        if (_current is UI_Prompt prompt)        
            prompt.CancelFill();
    }
}

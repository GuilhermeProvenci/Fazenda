using UnityEngine;

/// <summary>
/// Item de recurso natural (madeira, pedra, minérios, etc.)
/// </summary>
[CreateAssetMenu(fileName = "New Resource", menuName = "Inventory/Items/Resource Item")]
public class ResourceItem : ItemData
{
    [Header("Resource Properties")]
    [Tooltip("Este recurso é renovável?")]
    public bool isRenewable = false;
    
    [Tooltip("Tempo para respawn do recurso (se renovável)")]
    [Min(0)]
    public float respawnTime = 60f;
    
    [Tooltip("Qualidade do recurso (1-5 estrelas)")]
    [Range(1, 5)]
    public int quality = 1;
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        tooltip += $"\n\n<color=green>Resource</color>\n";
        tooltip += $"Quality: {new string('★', quality)}\n";
        
        if (isRenewable)
            tooltip += $"Renewable (Respawn: {respawnTime}s)";
        else
            tooltip += "Non-renewable";
        
        return tooltip;
    }
}

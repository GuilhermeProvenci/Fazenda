using UnityEngine;

/// <summary>
/// Item de ferramenta (machado, pá, regador, etc.)
/// </summary>
[CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Items/Tool Item")]
public class ToolItem : ItemData
{
    [Header("Tool Properties")]
    [Tooltip("Tipo de ferramenta")]
    public ToolType toolType = ToolType.Axe;
    
    [Tooltip("Ferramenta tem durabilidade?")]
    public bool hasDurability = true;
    
    [Tooltip("Durabilidade máxima")]
    [Min(1)]
    public int maxDurability = 100;
    
    [Tooltip("Eficiência da ferramenta (multiplicador)")]
    [Range(0.5f, 5f)]
    public float efficiency = 1f;
    
    [Tooltip("Nível mínimo para usar")]
    [Min(1)]
    public int requiredLevel = 1;
    
    [Header("Audio")]
    [Tooltip("Som ao usar a ferramenta")]
    public AudioClip useSound;
    
    public override bool Use(GameObject user)
    {
        Debug.Log($"[ToolItem] {user.name} is using {itemName} ({toolType})");
        
        // Toca som
        if (useSound != null)
        {
            AudioSource.PlayClipAtPoint(useSound, user.transform.position);
        }
        
        // TODO: Aplicar lógica de uso da ferramenta
        // Exemplo: reduzir durabilidade
        
        return false; // Ferramentas não são consumidas no uso
    }
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        tooltip += $"\n\n<color=cyan>Tool</color>\n";
        tooltip += $"Type: {toolType}\n";
        tooltip += $"Efficiency: {efficiency}x\n";
        
        if (hasDurability)
            tooltip += $"Durability: {maxDurability}\n";
        
        if (requiredLevel > 1)
            tooltip += $"Required Level: {requiredLevel}";
        
        return tooltip;
    }
}

/// <summary>
/// Tipos de ferramentas (compatível com Player.ToolType)
/// </summary>
public enum ToolType
{
    Axe,           // Machado
    Shovel,        // Pá
    WateringCan,   // Regador
    Hoe,           // Enxada
    Pickaxe,       // Picareta
    FishingRod,    // Vara de pesca
    Hammer,        // Martelo
    Sickle         // Foice
}

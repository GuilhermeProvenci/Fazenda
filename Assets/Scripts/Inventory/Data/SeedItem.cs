using UnityEngine;

/// <summary>
/// Item de semente para plantar
/// </summary>
[CreateAssetMenu(fileName = "New Seed", menuName = "Inventory/Items/Seed Item")]
public class SeedItem : ItemData
{
    [Header("Seed Properties")]
    [Tooltip("Prefab da planta que cresce")]
    public GameObject plantPrefab;
    
    [Tooltip("Tempo de crescimento em segundos")]
    [Min(1)]
    public float growthTime = 60f;
    
    [Tooltip("Estações em que pode ser plantada")]
    public Season[] validSeasons;
    
    [Tooltip("Quantidade de colheita")]
    [Range(1, 10)]
    public int harvestYield = 1;
    
    [Tooltip("Item resultante da colheita")]
    public ItemData harvestItem;
    
    [Tooltip("Requer água diariamente?")]
    public bool requiresWatering = true;
    
    [Tooltip("Pode ser replantada após colheita?")]
    public bool isReplantable = false;
    
    public override bool Use(GameObject user)
    {
        Debug.Log($"[SeedItem] Attempting to plant {itemName}");
        
        // TODO: Implementar lógica de plantio
        // 1. Verificar se está em solo arável
        // 2. Verificar estação
        // 3. Plantar semente
        
        return true; // Semente foi consumida ao plantar
    }
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        tooltip += $"\n\n<color=green>Seed</color>\n";
        tooltip += $"Growth Time: {growthTime}s\n";
        tooltip += $"Harvest: {harvestYield}x ";
        
        if (harvestItem != null)
            tooltip += harvestItem.itemName;
        
        tooltip += "\n";
        
        if (validSeasons != null && validSeasons.Length > 0)
        {
            tooltip += "Seasons: ";
            foreach (var season in validSeasons)
                tooltip += $"{season} ";
            tooltip += "\n";
        }
        
        if (requiresWatering)
            tooltip += "Requires watering\n";
        
        if (isReplantable)
            tooltip += "Replantable";
        
        return tooltip;
    }
}

/// <summary>
/// Estações do ano
/// </summary>
public enum Season
{
    Spring,  // Primavera
    Summer,  // Verão
    Fall,    // Outono
    Winter   // Inverno
}

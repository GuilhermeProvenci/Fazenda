using UnityEngine;

/// <summary>
/// Classe base abstrata para todos os itens do jogo.
/// Todos os itens devem herdar desta classe.
/// </summary>
public abstract class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Nome do item exibido no jogo")]
    public string itemName = "New Item";
    
    [Tooltip("ID único para serialização e save/load")]
    public string itemID = "";
    
    [TextArea(2, 4)]
    [Tooltip("Descrição do item")]
    public string description = "";
    
    [Header("Visual")]
    [Tooltip("Ícone do item no inventário")]
    public Sprite icon;
    
    [Tooltip("Prefab do item quando dropado no mundo")]
    public GameObject worldPrefab;
    
    [Header("Properties")]
    [Tooltip("Categoria do item")]
    public ItemCategory category = ItemCategory.Misc;
    
    [Tooltip("Item pode ser empilhado?")]
    public bool isStackable = true;
    
    [Tooltip("Quantidade máxima por pilha")]
    [Range(1, 999)]
    public int maxStackSize = 99;
    
    [Tooltip("Peso do item (para sistema de carga)")]
    [Min(0)]
    public float weight = 1f;
    
    [Header("Economy")]
    [Tooltip("Preço de venda")]
    [Min(0)]
    public int sellPrice = 10;
    
    [Tooltip("Preço de compra")]
    [Min(0)]
    public int buyPrice = 20;
    
    [Header("Legacy Compatibility")]
    [Tooltip("Tipo do item no sistema antigo (para compatibilidade)")]
    public ItemType legacyType;
    
    // ---------------------------
    // MÉTODOS VIRTUAIS
    // ---------------------------
    
    /// <summary>
    /// Usa o item. Retorna true se o item foi consumido.
    /// </summary>
    public virtual bool Use(GameObject user)
    {
        Debug.Log($"[ItemData] Using {itemName}");
        return false; // Por padrão, item não é consumido
    }
    
    /// <summary>
    /// Chamado quando o item é adicionado ao inventário
    /// </summary>
    public virtual void OnPickup(GameObject picker)
    {
        Debug.Log($"[ItemData] {itemName} picked up by {picker.name}");
    }
    
    /// <summary>
    /// Chamado quando o item é removido do inventário
    /// </summary>
    public virtual void OnDrop(GameObject dropper)
    {
        Debug.Log($"[ItemData] {itemName} dropped by {dropper.name}");
    }
    
    /// <summary>
    /// Retorna informações detalhadas do item (para tooltips)
    /// </summary>
    public virtual string GetTooltip()
    {
        string tooltip = $"<b>{itemName}</b>\n";
        tooltip += $"{description}\n\n";
        tooltip += $"<i>Category: {category}</i>\n";
        
        if (isStackable)
            tooltip += $"Max Stack: {maxStackSize}\n";
        
        tooltip += $"Weight: {weight}kg\n";
        tooltip += $"Value: {sellPrice}g";
        
        return tooltip;
    }
    
    // ---------------------------
    // VALIDAÇÃO
    // ---------------------------
    
    private void OnValidate()
    {
        // Gera ID automaticamente se vazio
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = name.Replace(" ", "");
        }
        
        // Garante que maxStackSize seja pelo menos 1
        if (maxStackSize < 1)
            maxStackSize = 1;
        
        // Se não é stackable, max stack = 1
        if (!isStackable)
            maxStackSize = 1;
    }
}

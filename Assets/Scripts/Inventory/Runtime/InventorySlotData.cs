using UnityEngine;

/// <summary>
/// Representa os dados de um slot do inventário
/// </summary>
[System.Serializable]
public class InventorySlotData
{
    [SerializeField] private ItemData item;
    [SerializeField] private float quantity;
    [SerializeField] private int slotIndex;
    
    // Propriedades públicas
    public ItemData Item => item;
    public float Quantity => quantity;
    public int SlotIndex => slotIndex;
    
    public bool IsEmpty => item == null || quantity <= 0.0001f;
    public bool IsFull => item != null && quantity >= item.maxStackSize - 0.0001f;
    
    // ---------------------------
    // CONSTRUTOR
    // ---------------------------
    
    public InventorySlotData(int index)
    {
        slotIndex = index;
        item = null;
        quantity = 0;
    }
    
    // ---------------------------
    // MÉTODOS
    // ---------------------------
    
    /// <summary>
    /// Verifica se pode adicionar um item neste slot
    /// </summary>
    public bool CanAddItem(ItemData newItem, float amount = 1f)
    {
        if (newItem == null) return false;
        
        // Slot vazio aceita qualquer item
        if (IsEmpty) return true;
        
        // Só pode adicionar se for o mesmo item
        if (item != newItem) return false;
        
        // Só pode adicionar se o item for stackable
        if (!item.isStackable) return false;
        
        // Verifica se não excede o máximo
        return quantity + amount <= item.maxStackSize + 0.0001f;
    }
    
    /// <summary>
    /// Adiciona quantidade ao slot
    /// </summary>
    public float AddQuantity(float amount)
    {
        if (item == null) return 0;
        
        float spaceLeft = item.maxStackSize - quantity;
        float toAdd = Mathf.Min(amount, spaceLeft);
        
        quantity += toAdd;
        return toAdd; // Retorna quanto foi realmente adicionado
    }
    
    /// <summary>
    /// Remove quantidade do slot
    /// </summary>
    public float RemoveQuantity(float amount)
    {
        float toRemove = Mathf.Min(amount, quantity);
        quantity -= toRemove;
        
        if (quantity <= 0.0001f)
        {
            Clear();
        }
        
        return toRemove; // Retorna quanto foi realmente removido
    }
    
    /// <summary>
    /// Define o item e quantidade do slot
    /// </summary>
    public void SetItem(ItemData newItem, float newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
        
        if (item != null && quantity > item.maxStackSize)
        {
            quantity = item.maxStackSize;
        }
    }
    
    /// <summary>
    /// Limpa o slot
    /// </summary>
    public void Clear()
    {
        item = null;
        quantity = 0;
    }
    
    /// <summary>
    /// Clona os dados do slot
    /// </summary>
    public InventorySlotData Clone()
    {
        var clone = new InventorySlotData(slotIndex);
        clone.item = item;
        clone.quantity = quantity;
        return clone;
    }
    
    // ---------------------------
    // DEBUG
    // ---------------------------
    
    public override string ToString()
    {
        if (IsEmpty)
            return $"Slot {slotIndex}: Empty";
        
        return $"Slot {slotIndex}: {item.itemName} x{quantity:F2}";
    }
}

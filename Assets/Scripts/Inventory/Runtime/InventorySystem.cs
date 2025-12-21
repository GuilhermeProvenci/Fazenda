using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema principal de inventário com slots dinâmicos
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int maxSlots = 30;
    [SerializeField] private bool autoExpand = false;
    
    [Header("Runtime Data")]
    [SerializeField] private List<InventorySlotData> slots = new List<InventorySlotData>();
    
    // ---------------------------
    // EVENTOS
    // ---------------------------
    
    public event Action<InventorySlotData> OnItemAdded;
    public event Action<InventorySlotData> OnItemRemoved;
    public event Action<int, int> OnItemMoved; // fromIndex, toIndex
    public event Action OnInventoryChanged;
    
    // ---------------------------
    // SINGLETON (OPCIONAL)
    // ---------------------------
    
    public static InventorySystem Instance { get; private set; }
    
    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------
    
    private void Awake()
    {
        // Singleton pattern (opcional)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[InventorySystem] Multiple instances detected!");
        }
        
        InitializeSlots();
    }
    
    private void InitializeSlots()
    {
        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlotData(i));
        }
        
        Debug.Log($"[InventorySystem] Initialized with {maxSlots} slots");
    }
    
    // ==========================================
    // ADICIONAR ITENS
    // ==========================================
    
    /// <summary>
    /// Adiciona um item ao inventário
    /// </summary>
    public bool AddItem(ItemData item, float quantity = 1f)
    {
        if (item == null || quantity <= 0.0001f)
        {
            Debug.LogWarning("[InventorySystem] Cannot add null item or invalid quantity");
            return false;
        }
        
        float remaining = quantity;
        
        // Tenta empilhar em slots existentes (se stackable)
        if (item.isStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.Item == item && !slot.IsFull)
                {
                    float added = slot.AddQuantity(remaining);
                    remaining -= added;
                    
                    OnItemAdded?.Invoke(slot);
                    
                    if (remaining <= 0.0001f)
                    {
                        OnInventoryChanged?.Invoke();
                        Debug.Log($"[InventorySystem] Added {quantity:F2}x {item.itemName}");
                        return true;
                    }
                }
            }
        }
        
        // Adiciona em slots vazios
        while (remaining > 0.0001f)
        {
            var emptySlot = FindEmptySlot();
            
            if (emptySlot == null)
            {
                if (autoExpand && slots.Count < 100) // Limite de segurança
                {
                    ExpandInventory(1);
                    emptySlot = FindEmptySlot();
                }
                else
                {
                    Debug.LogWarning($"[InventorySystem] Inventory full! Could not add {remaining:F2}x {item.itemName}");
                    OnInventoryChanged?.Invoke();
                    return false; // Inventário cheio
                }
            }
            
            float toAdd = item.isStackable ? Mathf.Min(remaining, item.maxStackSize) : 1f;
            
            emptySlot.SetItem(item, toAdd);
            remaining -= toAdd;
            
            OnItemAdded?.Invoke(emptySlot);
        }
        
        OnInventoryChanged?.Invoke();
        Debug.Log($"[InventorySystem] Added {quantity:F2}x {item.itemName}");
        return true;
    }
    
    // ==========================================
    // REMOVER ITENS
    // ==========================================
    
    /// <summary>
    /// Remove um item do inventário
    /// </summary>
    public bool RemoveItem(ItemData item, float quantity = 1f)
    {
        if (!HasItem(item, quantity))
        {
            Debug.LogWarning($"[InventorySystem] Not enough {item.itemName} to remove");
            return false;
        }
        
        float remaining = quantity;
        
        // Remove de trás para frente para evitar problemas
        for (int i = slots.Count - 1; i >= 0 && remaining > 0.0001f; i--)
        {
            var slot = slots[i];
            if (slot.Item != item) continue;
            
            float removed = slot.RemoveQuantity(remaining);
            remaining -= removed;
            
            OnItemRemoved?.Invoke(slot);
        }
        
        OnInventoryChanged?.Invoke();
        Debug.Log($"[InventorySystem] Removed {quantity:F2}x {item.itemName}");
        return true;
    }
    
    /// <summary>
    /// Remove item de um slot específico
    /// </summary>
    public bool RemoveFromSlot(int slotIndex, float quantity = 1f)
    {
        var slot = GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return false;
        
        slot.RemoveQuantity(quantity);
        OnItemRemoved?.Invoke(slot);
        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    // ==========================================
    // MOVER ITENS (DRAG & DROP)
    // ==========================================
    
    /// <summary>
    /// Move item de um slot para outro
    /// </summary>
    public bool MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= slots.Count) return false;
        if (toIndex < 0 || toIndex >= slots.Count) return false;
        if (fromIndex == toIndex) return false;
        
        var fromSlot = slots[fromIndex];
        var toSlot = slots[toIndex];
        
        if (fromSlot.IsEmpty) return false;
        
        // Caso 1: Slot destino vazio - move tudo
        if (toSlot.IsEmpty)
        {
            toSlot.SetItem(fromSlot.Item, fromSlot.Quantity);
            fromSlot.Clear();
        }
        // Caso 2: Mesmo item e stackable - tenta empilhar
        else if (fromSlot.Item == toSlot.Item && toSlot.Item.isStackable)
        {
            float spaceLeft = toSlot.Item.maxStackSize - toSlot.Quantity;
            float toMove = Mathf.Min(fromSlot.Quantity, spaceLeft);
            
            toSlot.AddQuantity(toMove);
            fromSlot.RemoveQuantity(toMove);
        }
        // Caso 3: Itens diferentes - swap
        else
        {
            var tempItem = toSlot.Item;
            float tempQuantity = toSlot.Quantity;
            
            toSlot.SetItem(fromSlot.Item, fromSlot.Quantity);
            fromSlot.SetItem(tempItem, tempQuantity);
        }
        
        OnItemMoved?.Invoke(fromIndex, toIndex);
        OnInventoryChanged?.Invoke();
        
        Debug.Log($"[InventorySystem] Moved item from slot {fromIndex} to {toIndex}");
        return true;
    }
    
    /// <summary>
    /// Divide stack ao meio
    /// </summary>
    public bool SplitStack(int fromIndex, int toIndex)
    {
        var fromSlot = GetSlot(fromIndex);
        var toSlot = GetSlot(toIndex);
        
        if (fromSlot == null || toSlot == null) return false;
        if (fromSlot.IsEmpty || !toSlot.IsEmpty) return false;
        if (fromSlot.Quantity <= 0.0001f) return false;
        
        float half = fromSlot.Quantity / 2f;
        
        toSlot.SetItem(fromSlot.Item, half);
        fromSlot.RemoveQuantity(half);
        
        OnItemMoved?.Invoke(fromIndex, toIndex);
        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    // ==========================================
    // QUERIES
    // ==========================================
    
    /// <summary>
    /// Verifica se tem um item
    /// </summary>
    public bool HasItem(ItemData item, float quantity = 1f)
    {
        return GetItemCount(item) >= quantity - 0.0001f;
    }
    
    /// <summary>
    /// Retorna quantidade total de um item
    /// </summary>
    public float GetItemCount(ItemData item)
    {
        float count = 0;
        foreach (var slot in slots)
        {
            if (slot.Item == item)
                count += slot.Quantity;
        }
        return count;
    }
    
    /// <summary>
    /// Retorna um slot específico
    /// </summary>
    public InventorySlotData GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index];
    }
    
    /// <summary>
    /// Encontra primeiro slot vazio
    /// </summary>
    public InventorySlotData FindEmptySlot()
    {
        return slots.Find(s => s.IsEmpty);
    }
    
    /// <summary>
    /// Retorna todos os slots
    /// </summary>
    public List<InventorySlotData> GetAllSlots()
    {
        return new List<InventorySlotData>(slots);
    }
    
    /// <summary>
    /// Retorna slots por categoria
    /// </summary>
    public List<InventorySlotData> GetItemsByCategory(ItemCategory category)
    {
        return slots.FindAll(s => !s.IsEmpty && s.Item.category == category);
    }
    
    /// <summary>
    /// Retorna número de slots vazios
    /// </summary>
    public int GetEmptySlotCount()
    {
        return slots.FindAll(s => s.IsEmpty).Count;
    }
    
    // ==========================================
    // UTILIDADES
    // ==========================================
    
    /// <summary>
    /// Expande o inventário
    /// </summary>
    public void ExpandInventory(int additionalSlots)
    {
        int currentCount = slots.Count;
        for (int i = 0; i < additionalSlots; i++)
        {
            slots.Add(new InventorySlotData(currentCount + i));
        }
        
        maxSlots = slots.Count;
        OnInventoryChanged?.Invoke();
        
        Debug.Log($"[InventorySystem] Expanded inventory to {maxSlots} slots");
    }
    
    /// <summary>
    /// Limpa todo o inventário
    /// </summary>
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
        
        OnInventoryChanged?.Invoke();
        Debug.Log("[InventorySystem] Inventory cleared");
    }
    
    /// <summary>
    /// Organiza inventário (agrupa itens iguais)
    /// </summary>
    public void SortInventory()
    {
        // TODO: Implementar lógica de organização
        Debug.Log("[InventorySystem] Sorting inventory...");
    }
    
    // ==========================================
    // DEBUG
    // ==========================================
    
    [ContextMenu("Debug: Print Inventory")]
    private void DebugPrintInventory()
    {
        Debug.Log("=== INVENTORY ===");
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
                Debug.Log(slot.ToString());
        }
        Debug.Log($"Empty slots: {GetEmptySlotCount()}/{maxSlots}");
    }
    
    [ContextMenu("Debug: Fill Inventory")]
    private void DebugFillInventory()
    {
        // Carrega um item de teste
        var testItems = Resources.LoadAll<ItemData>("Items");
        if (testItems.Length > 0)
        {
            AddItem(testItems[0], 10);
        }
    }
}

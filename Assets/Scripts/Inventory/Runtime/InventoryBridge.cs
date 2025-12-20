using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ponte de compatibilidade entre sistema antigo (ItemType enum) e novo (ItemData ScriptableObjects)
/// Permite que código legado continue funcionando enquanto migração é feita
/// </summary>
public class InventoryBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventorySystem newInventory;
    
    [Header("Item Mapping")]
    [Tooltip("Mapeamento manual de ItemType para ItemData")]
    [SerializeField] private List<ItemTypeMapping> itemMappings = new List<ItemTypeMapping>();
    
    // Cache de mapeamento
    private Dictionary<ItemType, ItemData> typeToDataMap = new Dictionary<ItemType, ItemData>();
    private Dictionary<ItemData, ItemType> dataToTypeMap = new Dictionary<ItemData, ItemType>();
    
    // Eventos compatíveis com sistema antigo
    public event Action OnInventoryChanged;
    
    // Singleton para acesso fácil
    public static InventoryBridge Instance { get; private set; }
    
    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[InventoryBridge] Multiple instances detected!");
        }
        
        if (newInventory == null)
            newInventory = GetComponent<InventorySystem>();
        
        if (newInventory == null)
            newInventory = FindObjectOfType<InventorySystem>();
        
        if (newInventory == null)
        {
            Debug.LogError("[InventoryBridge] InventorySystem not found!");
            enabled = false;
            return;
        }
        
        BuildMapping();
    }
    
    private void Start()
    {
        // Inscreve no evento do novo sistema
        newInventory.OnInventoryChanged += () => OnInventoryChanged?.Invoke();
    }
    
    private void OnDestroy()
    {
        if (newInventory != null)
        {
            newInventory.OnInventoryChanged -= () => OnInventoryChanged?.Invoke();
        }
    }
    
    // ---------------------------
    // MAPEAMENTO
    // ---------------------------
    
    private void BuildMapping()
    {
        typeToDataMap.Clear();
        dataToTypeMap.Clear();
        
        // Adiciona mapeamentos manuais
        foreach (var mapping in itemMappings)
        {
            if (mapping.itemData != null)
            {
                typeToDataMap[mapping.itemType] = mapping.itemData;
                dataToTypeMap[mapping.itemData] = mapping.itemType;
            }
        }
        
        // Tenta carregar itens de Resources e mapear automaticamente
        AutoMapItemsFromResources();
        
        Debug.Log($"[InventoryBridge] Mapped {typeToDataMap.Count} item types");
    }
    
    private void AutoMapItemsFromResources()
    {
        var allItems = Resources.LoadAll<ItemData>("Items");
        
        foreach (var item in allItems)
        {
            // Usa o campo legacyType do ItemData
            if (!typeToDataMap.ContainsKey(item.legacyType))
            {
                typeToDataMap[item.legacyType] = item;
                dataToTypeMap[item] = item.legacyType;
            }
        }
    }
    
    /// <summary>
    /// Adiciona mapeamento manualmente
    /// </summary>
    public void AddMapping(ItemType type, ItemData data)
    {
        typeToDataMap[type] = data;
        dataToTypeMap[data] = type;
    }
    
    // ---------------------------
    // API COMPATÍVEL COM SISTEMA ANTIGO
    // ---------------------------
    
    /// <summary>
    /// Adiciona item usando ItemType (compatibilidade)
    /// </summary>
    public bool Add(ItemType type, float amount)
    {
        if (!typeToDataMap.ContainsKey(type))
        {
            Debug.LogWarning($"[InventoryBridge] No mapping found for {type}");
            return false;
        }
        
        return newInventory.AddItem(typeToDataMap[type], (int)amount);
    }
    
    /// <summary>
    /// Remove item usando ItemType (compatibilidade)
    /// </summary>
    public bool Remove(ItemType type, float amount)
    {
        if (!typeToDataMap.ContainsKey(type))
        {
            Debug.LogWarning($"[InventoryBridge] No mapping found for {type}");
            return false;
        }
        
        return newInventory.RemoveItem(typeToDataMap[type], (int)amount);
    }
    
    /// <summary>
    /// Retorna quantidade de um item (compatibilidade)
    /// </summary>
    public float Get(ItemType type)
    {
        if (!typeToDataMap.ContainsKey(type))
            return 0;
        
        return newInventory.GetItemCount(typeToDataMap[type]);
    }
    
    /// <summary>
    /// Verifica se tem item (compatibilidade)
    /// </summary>
    public bool Has(ItemType type, float amount)
    {
        if (!typeToDataMap.ContainsKey(type))
            return false;
        
        return newInventory.HasItem(typeToDataMap[type], (int)amount);
    }
    
    /// <summary>
    /// Retorna limite de um item (compatibilidade)
    /// </summary>
    public float GetLimit(ItemType type)
    {
        if (!typeToDataMap.ContainsKey(type))
            return 0;
        
        var itemData = typeToDataMap[type];
        return itemData.maxStackSize * 10; // Aproximação: assume 10 slots
    }
    
    /// <summary>
    /// Retorna porcentagem (compatibilidade)
    /// </summary>
    public float GetPercentage(ItemType type)
    {
        float current = Get(type);
        float limit = GetLimit(type);
        
        if (limit == 0) return 0;
        return current / limit;
    }
    
    /// <summary>
    /// Verifica se está cheio (compatibilidade)
    /// </summary>
    public bool IsFull(ItemType type)
    {
        return Get(type) >= GetLimit(type);
    }
    
    // ---------------------------
    // CONVERSÃO
    // ---------------------------
    
    /// <summary>
    /// Converte ItemType para ItemData
    /// </summary>
    public ItemData GetItemData(ItemType type)
    {
        return typeToDataMap.ContainsKey(type) ? typeToDataMap[type] : null;
    }
    
    /// <summary>
    /// Converte ItemData para ItemType
    /// </summary>
    public ItemType GetItemType(ItemData data)
    {
        return dataToTypeMap.ContainsKey(data) ? dataToTypeMap[data] : ItemType.Wood;
    }
    
    // ---------------------------
    // DEBUG
    // ---------------------------
    
    [ContextMenu("Print Mappings")]
    private void DebugPrintMappings()
    {
        Debug.Log("=== ITEM TYPE MAPPINGS ===");
        foreach (var kvp in typeToDataMap)
        {
            Debug.Log($"{kvp.Key} → {kvp.Value.itemName}");
        }
    }
    
    [ContextMenu("Rebuild Mapping")]
    private void DebugRebuildMapping()
    {
        BuildMapping();
    }
}

/// <summary>
/// Mapeamento de ItemType para ItemData
/// </summary>
[System.Serializable]
public class ItemTypeMapping
{
    public ItemType itemType;
    public ItemData itemData;
}

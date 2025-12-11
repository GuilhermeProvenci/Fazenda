using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    // Quantidades atuais
    private Dictionary<ItemType, float> amounts =
        new Dictionary<ItemType, float>();

    // Limites por item
    [Header("Limites por Item")]
    public Dictionary<ItemType, float> limits =
        new Dictionary<ItemType, float>()
        {
            { ItemType.Wood, 64 },
            { ItemType.Carrot, 64 },
            { ItemType.Fish, 64 },
            { ItemType.Water, 64 },

            { ItemType.WoodenPlank, 128 },
            { ItemType.Stone, 999 },
            { ItemType.IronOre, 999 },

            { ItemType.Fence, 20 },
            { ItemType.Chest, 10 },
            { ItemType.Meal, 30 }
        };

    public event Action OnInventoryChanged;

    private void Awake()
    {
        // Inicializa tudo com zero
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            if (!amounts.ContainsKey(type))
                amounts[type] = 0;
        }
    }

    // ==========================================
    // GETTERS
    // ==========================================

    public float Get(ItemType type) => amounts[type];
    public float GetLimit(ItemType type) => limits.ContainsKey(type) ? limits[type] : 0;

    public float GetPercentage(ItemType type)
    {
        float limit = GetLimit(type);
        if (limit == 0) return 0;
        return amounts[type] / limit;
    }

    public bool Has(ItemType type, float amount)
    {
        return amounts[type] >= amount;
    }

    public bool IsFull(ItemType type)
    {
        return amounts[type] >= GetLimit(type);
    }

    // ==========================================
    // ADIÇÃO
    // ==========================================

    public bool Add(ItemType type, float amount)
    {
        if (amount <= 0) return false;

        float current = amounts[type];
        float limit = GetLimit(type);

        float newValue = Mathf.Clamp(current + amount, 0, limit);
        float added = newValue - current;

        if (added <= 0)
            return false;

        amounts[type] = newValue;
        OnInventoryChanged?.Invoke();

        Debug.Log($"[Inventory] +{added} {type}");
        return true;
    }

    // ==========================================
    // REMOÇÃO
    // ==========================================

    public bool Remove(ItemType type, float amount)
    {
        if (!Has(type, amount))
            return false;

        amounts[type] -= amount;
        OnInventoryChanged?.Invoke();
        return true;
    }

    // ==========================================
    // DEBUG
    // ==========================================

    [ContextMenu("Fill All")]
    private void DebugFillAll()
    {
        foreach (var type in amounts.Keys)
            amounts[type] = GetLimit(type);

        OnInventoryChanged?.Invoke();
        Debug.Log("Inventory filled.");
    }

    [ContextMenu("Clear All")]
    private void DebugClearAll()
    {
        foreach (var type in amounts.Keys)
            amounts[type] = 0;

        OnInventoryChanged?.Invoke();
        Debug.Log("Inventory cleared.");
    }
}

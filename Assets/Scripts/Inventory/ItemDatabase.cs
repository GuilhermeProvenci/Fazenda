using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Database centralizado de informações de itens
/// Crie um asset: Assets → Create → Inventory → Item Database
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public ItemType type;
        public string displayName;
        public Sprite icon;
        [TextArea(2, 4)]
        public string description;
        public Color iconTint = Color.white;
    }

    [Header("Item Database")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    // Cache para busca rápida
    private Dictionary<ItemType, ItemData> itemCache;

    // ---------------------------
    // INICIALIZAÇÃO
    // ---------------------------

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        itemCache = new Dictionary<ItemType, ItemData>();

        foreach (var item in items)
        {
            if (!itemCache.ContainsKey(item.type))
                itemCache[item.type] = item;
        }
    }

    // ---------------------------
    // GETTERS
    // ---------------------------

    public ItemData GetItemData(ItemType type)
    {
        if (itemCache == null)
            BuildCache();

        return itemCache.ContainsKey(type) ? itemCache[type] : null;
    }

    public Sprite GetIcon(ItemType type)
    {
        var data = GetItemData(type);
        return data?.icon;
    }

    public string GetDisplayName(ItemType type)
    {
        var data = GetItemData(type);
        return data?.displayName ?? type.ToString();
    }

    public string GetDescription(ItemType type)
    {
        var data = GetItemData(type);
        return data?.description ?? "";
    }

    public Color GetIconTint(ItemType type)
    {
        var data = GetItemData(type);
        return data?.iconTint ?? Color.white;
    }

    // ---------------------------
    // EDITOR HELPER
    // ---------------------------

    [ContextMenu("Auto-Create Missing Items")]
    private void AutoCreateMissingItems()
    {
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            bool exists = items.Exists(i => i.type == type);

            if (!exists)
            {
                items.Add(new ItemData
                {
                    type = type,
                    displayName = FormatName(type.ToString()),
                    description = $"Item do tipo {type}"
                });
            }
        }

        BuildCache();
        Debug.Log($"[ItemDatabase] Items criados/verificados: {items.Count}");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private string FormatName(string enumName)
    {
        // Converte "WoodenPlank" → "Wooden Plank"
        return System.Text.RegularExpressions.Regex.Replace(
            enumName,
            "([a-z])([A-Z])",
            "$1 $2"
        );
    }

    // ---------------------------
    // SINGLETON PATTERN
    // ---------------------------

    private static ItemDatabase instance;

    public static ItemDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ItemDatabase>("ItemDatabase");

                if (instance == null)
                {
                    Debug.LogError("[ItemDatabase] Asset não encontrado em Resources! Crie um em Assets/Resources/ItemDatabase.asset");
                }
            }

            return instance;
        }
    }
}
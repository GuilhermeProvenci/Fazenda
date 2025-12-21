using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Registrador central que carrega e fornece acesso a todos os ItemData do jogo.
/// Evita a necessidade de arrastar referências manuais no Inspector para cada script.
/// </summary>
public static class ItemRegistry
{
    private static Dictionary<string, ItemData> itemDatabase;
    private static bool isInitialized = false;

    public static bool IsInitialized => isInitialized;

    /// <summary>
    /// Inicializa a base de dados carregando todos os ScriptableObjects de Items/
    /// </summary>
    public static void Initialize()
    {
        if (isInitialized) return;

        itemDatabase = new Dictionary<string, ItemData>();
        ItemData[] items = Resources.LoadAll<ItemData>("Items");

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning($"[ItemRegistry] Item '{item.name}' não possui ItemID definido. Usando nome do asset.");
                if (!itemDatabase.ContainsKey(item.name))
                    itemDatabase.Add(item.name, item);
            }
            else if (!itemDatabase.ContainsKey(item.itemID))
            {
                itemDatabase.Add(item.itemID, item);
            }
            else
            {
                Debug.LogError($"[ItemRegistry] ItemID duplicado detectado: {item.itemID}");
            }
        }

        isInitialized = true;
        Debug.Log($"[ItemRegistry] {itemDatabase.Count} itens carregados com sucesso.");
    }

    /// <summary>
    /// Busca um item pelo seu ItemID. 
    /// Se não encontrar, retorna um item genérico "Missing" para evitar crashes.
    /// </summary>
    public static ItemData GetItem(string id)
    {
        if (!isInitialized) Initialize();

        if (itemDatabase == null) itemDatabase = new Dictionary<string, ItemData>();

        if (itemDatabase.TryGetValue(id, out ItemData item))
        {
            return item;
        }

        Debug.LogError($"[ItemRegistry] Item '{id}' não encontrado em Resources/Items/! Criando item temporário...");
        
        // Cria um item de fallback para não quebrar o jogo
        // Usando CreateInstance de ResourceItem que é concreto
        ResourceItem fallback = ScriptableObject.CreateInstance<ResourceItem>();
        fallback.itemID = id;
        fallback.itemName = $"MISSING: {id}";
        fallback.description = "ERRO: O asset para este item ainda não foi criado na pasta Resources/Items/.";
        
        // Adiciona ao database para não criar múltiplos fallbacks para o mesmo ID
        itemDatabase[id] = fallback;
        
        return fallback;
    }
}

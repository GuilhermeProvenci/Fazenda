using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject base para todas as receitas de crafting.
/// Pode ser usado diretamente através do menu 'Create' ou estendido via script.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Info da Receita")]
    [SerializeField] protected string recipeName = "Nova Receita";
    [TextArea(2, 4)]
    [SerializeField] protected string description = "Descrição da receita";
    [SerializeField] protected Sprite icon;

    [Header("Ingredientes Necessários")]
    [SerializeField] protected CraftingIngredient[] ingredients;

    [Header("Resultado")]
    [SerializeField] protected ItemData resultItem;
    [SerializeField] protected int resultAmount = 1;
    [SerializeField] protected GameObject resultPrefab; 

    [Header("Requisitos")]
    [SerializeField] protected bool requiresWorkbench = false;
    [SerializeField] protected int unlockLevel = 0;

    // Propriedades públicas
    public string RecipeName => recipeName;
    public string Description => description;
    public Sprite Icon => icon;
    public CraftingIngredient[] Ingredients => ingredients;
    public ItemData ResultItem => GetResultItem();
    public int ResultAmount => resultAmount;
    public GameObject ResultPrefab => resultPrefab;
    public bool RequiresWorkbench => requiresWorkbench;
    public int UnlockLevel => unlockLevel;

    /// <summary>
    /// Resolve o item de resultado (tenta buscar pelo nome se estiver nulo)
    /// </summary>
    public virtual ItemData GetResultItem()
    {
        if (resultItem != null) return resultItem;
        
        // Se estiver nulo, tenta buscar pelo nome do asset ou nome da receita
        string searchName = name.Replace("Recipe", "");
        return ItemRegistry.GetItem(searchName);
    }

    // ======================================================
    // LOGIC METHODS (VIRTUAL)
    // ======================================================

    /// <summary>
    /// Verifica se o jogador tem os ingredientes necessários.
    /// </summary>
    public virtual bool CanCraft(InventorySystem inv)
    {
        if (inv == null) return false;
        
        if (ingredients == null || ingredients.Length == 0)
            return false;

        foreach (var ingredient in ingredients)
        {
            ItemData item = ingredient.GetItem();
            if (item == null) return false;

            if (!inv.HasItem(item, ingredient.amount))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Consome os ingredientes do inventário.
    /// </summary>
    public virtual bool ConsumeIngredients(InventorySystem inv)
    {
        if (!CanCraft(inv)) return false;

        foreach (var ingredient in ingredients)
        {
            ItemData item = ingredient.GetItem();
            if (item != null)
                inv.RemoveItem(item, ingredient.amount);
        }

        return true;
    }

    /// <summary>
    /// Retorna o texto formatado com os ingredientes que faltam.
    /// </summary>
    public virtual string GetMissingIngredientsText(InventorySystem inv)
    {
        string missing = "";

        if (ingredients == null || ingredients.Length == 0)
            return "Receita sem ingredientes configurados.";

        foreach (var ingredient in ingredients)
        {
            ItemData item = ingredient.GetItem();
            
            if (item == null)
            {
                missing += $"ERRO: Item '{ingredient.itemID}' não encontrado!\n";
                continue;
            }

            float has = inv.GetItemCount(item);
            float needs = ingredient.amount;

            if (has < needs - 0.001f)
            {
                missing += $"{item.itemName}: {has:F0}/{needs:F0}\n";
            }
        }

        return missing;
    }
}

/// <summary>
/// Ingrediente da receita - Suporta tanto ID por texto quanto referência direta
/// </summary>
[System.Serializable]
public class CraftingIngredient
{
    public string itemID;      // Recomendado: Use o ID para automação
    public ItemData itemData;  // Opcional: Referência direta no Inspector
    public float amount = 1;

    /// <summary>
    /// Resolve o ItemData usando a referência direta ou o ItemRegistry
    /// </summary>
    public static ItemData ResolveItem(CraftingIngredient ingredient)
    {
        if (ingredient.itemData != null) return ingredient.itemData;
        
        if (!string.IsNullOrEmpty(ingredient.itemID))
            return ItemRegistry.GetItem(ingredient.itemID);
            
        return null;
    }

    public ItemData GetItem()
    {
        ItemData item = ResolveItem(this);
        
        // Fallback para quando o ID está vazio mas o ScriptableObject está ali (para receitas antigas)
        if (item == null && itemData != null)
            return itemData;
            
        return item;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que define uma receita de crafting
/// Crie receitas: Assets → Create → Crafting → Recipe
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Info da Receita")]
    [SerializeField] private string recipeName = "Nova Receita";
    [TextArea(2, 4)]
    [SerializeField] private string description = "Descrição da receita";
    [SerializeField] private Sprite icon;

    [Header("Ingredientes Necessários")]
    [SerializeField] private CraftingIngredient[] ingredients;

    [Header("Resultado")]
    [SerializeField] private ItemType resultItemType;
    [SerializeField] private int resultAmount = 1;
    [SerializeField] private GameObject resultPrefab; // Para spawnar objetos

    [Header("Requisitos")]
    [SerializeField] private bool requiresWorkbench = false;
    [SerializeField] private int unlockLevel = 0;

    // Propriedades públicas
    public string RecipeName => recipeName;
    public string Description => description;
    public Sprite Icon => icon;
    public CraftingIngredient[] Ingredients => ingredients;
    public ItemType ResultItemType => resultItemType;
    public int ResultAmount => resultAmount;
    public GameObject ResultPrefab => resultPrefab;
    public bool RequiresWorkbench => requiresWorkbench;
    public int UnlockLevel => unlockLevel;

    // ======================================================
    // CRAFT CHECK
    // ======================================================

    public bool CanCraft(InventoryController inv)
    {
        if (inv == null) return false;

        foreach (var ingredient in ingredients)
        {
            if (!inv.Has(ingredient.itemType, ingredient.amount))
                return false;
        }

        return true;
    }

    // ======================================================
    // CONSUME INGREDIENTS
    // ======================================================

    public bool ConsumeIngredients(InventoryController inv)
    {
        if (!CanCraft(inv)) return false;

        foreach (var ingredient in ingredients)
        {
            inv.Remove(ingredient.itemType, ingredient.amount);
        }

        return true;
    }

    // ======================================================
    // MISSING INGREDIENTS
    // ======================================================

    public string GetMissingIngredientsText(InventoryController inv)
    {
        string missing = "";

        foreach (var ingredient in ingredients)
        {
            float has = inv.Get(ingredient.itemType);
            float needs = ingredient.amount;

            if (has < needs)
            {
                missing += $"{ingredient.itemName}: {has}/{needs}\n";
            }
        }

        return missing;
    }
}

/// <summary>
/// Ingrediente da receita
/// </summary>
[System.Serializable]
public class CraftingIngredient
{
    public string itemName = "Item";
    public ItemType itemType;
    public float amount = 1;
    public Sprite icon;
}

/// <summary>
/// Tipos de itens suportados
/// </summary>
public enum ItemType
{
    Wood,
    Carrot,
    Fish,
    Water,

    Fence,
    Chest,
    Meal,

    WoodenPlank,
    Stone,
    IronOre
}

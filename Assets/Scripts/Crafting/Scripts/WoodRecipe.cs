using UnityEngine;

/// <summary>
/// Categoria de receitas que usam madeira como base.
/// Demonstra como usar herança para especializar receitas.
/// </summary>
public abstract class WoodRecipe : CraftingRecipe
{
    protected void SetupWoodRequirement(float woodAmount)
    {
        ingredients = new CraftingIngredient[] 
        { 
            new CraftingIngredient { itemID = "Wood", amount = woodAmount } 
        };
    }
}

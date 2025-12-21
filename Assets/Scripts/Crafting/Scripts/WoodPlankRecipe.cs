using UnityEngine;

[CreateAssetMenu(fileName = "WoodPlank", menuName = "Crafting/Recipes/Wood Plank")]
public class WoodPlankRecipe : WoodRecipe
{
    private void OnEnable()
    {
        recipeName = "Tábua de Madeira";
        description = "Uma tábua refinada para construções.";
        SetupWoodRequirement(2f);
        resultAmount = 4;
        
        // O resultItem será buscado pelo Registry se estiver configurado na Unity
        // ou podemos forçar via código se tivermos o ID:
        // resultItem = ItemRegistry.GetItem("WoodPlank");
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "Chest", menuName = "Crafting/Recipes/Chest")]
public class ChestRecipe : WoodRecipe
{
    private void OnEnable()
    {
        recipeName = "Baú de Madeira";
        description = "Um baú resistente para guardar seus itens.";
        SetupWoodRequirement(10f); // Baús custam mais madeira
        resultAmount = 1;

        // Nota: O resultItem pode ser configurado no Inspector ou buscado pelo ID
        // if (resultItem == null) resultItem = ItemRegistry.GetItem("Chest");
    }
}

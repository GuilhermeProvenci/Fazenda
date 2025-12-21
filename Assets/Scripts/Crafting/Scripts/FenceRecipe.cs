using UnityEngine;

[CreateAssetMenu(fileName = "Fence", menuName = "Crafting/Recipes/Fence")]
public class FenceRecipe : WoodRecipe
{
    private void OnEnable()
    {
        recipeName = "Cerca de Madeira";
        description = "Ideal para cercar seus animais.";
        SetupWoodRequirement(3f);
        resultAmount = 4; // Um pouco de madeira gera várias cercas
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Botão individual de receita no menu de crafting
/// </summary>
public class RecipeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI ingredientsText;
    [SerializeField] private Image backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color canCraftColor = Color.white;
    [SerializeField] private Color cannotCraftColor = new Color(0.5f, 0.5f, 0.5f);

    private CraftingRecipe recipe;
    private CraftingManager craftingManager;
    private InventoryController inventory;

    // ---------------------------
    // SETUP
    // ---------------------------

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void Start()
    {
        inventory = FindObjectOfType<InventoryController>();
    }

    private void Update()
    {
        UpdateVisual();
    }

    /// <summary>
    /// Configura o botão com uma receita
    /// </summary>
    public void Setup(CraftingRecipe craftingRecipe, CraftingManager manager)
    {
        recipe = craftingRecipe;
        craftingManager = manager;

        if (recipe == null)
        {
            Debug.LogError("RecipeButton: Receita é null!");
            return;
        }

        // Nome
        if (nameText != null)
            nameText.text = recipe.RecipeName;

        // Ícone
        if (iconImage != null && recipe.Icon != null)
            iconImage.sprite = recipe.Icon;

        // Ingredientes
        UpdateIngredientsText();

        // Visual inicial
        UpdateVisual();
    }

    // ---------------------------
    // UI UPDATE
    // ---------------------------

    private void UpdateIngredientsText()
    {
        if (ingredientsText == null || recipe == null || inventory == null)
            return;

        string text = "";

        foreach (var ingredient in recipe.Ingredients)
        {
            float current = inventory.Get(ingredient.itemType);
            float needed = ingredient.amount;

            bool hasEnough = current >= needed;
            string color = hasEnough ? "green" : "red";

            text += $"<color={color}>{ingredient.itemName}: {current}/{needed}</color>\n";
        }

        ingredientsText.text = text;
    }

    private void UpdateVisual()
    {
        if (recipe == null || inventory == null)
            return;

        bool canCraft = recipe.CanCraft(inventory);

        // Background
        if (backgroundImage != null)
            backgroundImage.color = canCraft ? canCraftColor : cannotCraftColor;

        // Botão interativo
        if (button != null)
            button.interactable = canCraft;

        // Ingredientes
        UpdateIngredientsText();
    }

    // ---------------------------
    // CLICK
    // ---------------------------

    private void OnClick()
    {
        if (recipe == null || craftingManager == null)
            return;

        Debug.Log($"[RecipeButton] Clicou em: {recipe.RecipeName}");

        craftingManager.TryCraft(recipe);

        // Atualiza visual após craftar
        UpdateVisual();
    }
}

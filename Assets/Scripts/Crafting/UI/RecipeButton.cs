using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Botão de receita melhorado com evento de seleção
/// </summary>
public class RecipeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject lockIcon;

    [Header("Colors")]
    [SerializeField] private Color canCraftColor = Color.white;
    [SerializeField] private Color cannotCraftColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color selectedColor = new Color(0.3f, 0.8f, 0.3f);

    private CraftingRecipe recipe;
    private CraftingManager craftingManager;
    private InventorySystem inventory;
    private bool isSelected;

    // Evento de seleção
    public event Action<CraftingRecipe> OnSelected;

    // ---------------------------
    // SETUP
    // ---------------------------

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClick);

        inventory = FindObjectOfType<InventorySystem>();
    }

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
        {
            iconImage.sprite = recipe.Icon;
            iconImage.enabled = true;
        }

        // Visual inicial
        UpdateVisual();
    }

    // ---------------------------
    // UPDATE
    // ---------------------------

    public void UpdateVisual()
    {
        if (recipe == null || inventory == null)
            return;

        bool canCraft = recipe.CanCraft(inventory);

        // Background
        if (backgroundImage != null)
        {
            if (isSelected)
                backgroundImage.color = selectedColor;
            else
                backgroundImage.color = canCraft ? canCraftColor : cannotCraftColor;
        }

        // Lock icon
        if (lockIcon != null)
            lockIcon.SetActive(!canCraft);

        // Botão sempre clicável (para ver detalhes)
        if (button != null)
            button.interactable = true;
    }

    // ---------------------------
    // CLICK
    // ---------------------------

    private void OnClick()
    {
        if (recipe == null)
            return;

        isSelected = true;
        UpdateVisual();

        // Notifica que foi selecionado
        OnSelected?.Invoke(recipe);

        Debug.Log($"[RecipeButton] Selecionada: {recipe.RecipeName}");
    }

    public void Deselect()
    {
        isSelected = false;
        UpdateVisual();
    }
}
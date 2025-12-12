using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UI de Crafting melhorada com filtros, scroll e busca
/// </summary>
public class CraftingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject craftingPanel;
    [SerializeField] private Transform recipeGrid;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject recipeButtonPrefab;

    [Header("Filter Buttons")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button craftableButton;
    [SerializeField] private Button lockedButton;

    [Header("Search")]
    [SerializeField] private TMP_InputField searchField;

    [Header("Recipe Details")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private Image recipeIcon;
    [SerializeField] private TextMeshProUGUI recipeName;
    [SerializeField] private TextMeshProUGUI recipeDescription;
    [SerializeField] private TextMeshProUGUI ingredientsText;
    [SerializeField] private Button craftButton;

    [Header("Colors")]
    [SerializeField] private Color activeFilterColor = Color.green;
    [SerializeField] private Color inactiveFilterColor = Color.gray;

    // Estado
    private CraftingManager craftingManager;
    private InventoryController inventory;
    private List<RecipeButton> recipeButtons = new List<RecipeButton>();
    private CraftingRecipe selectedRecipe;
    private RecipeFilter currentFilter = RecipeFilter.All;

    private enum RecipeFilter { All, Craftable, Locked }

    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------

    private void Awake()
    {
        craftingManager = FindObjectOfType<CraftingManager>();
        inventory = FindObjectOfType<InventoryController>();

        if (craftingManager == null)
        {
            Debug.LogError("[CraftingUI] CraftingManager não encontrado!");
            enabled = false;
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("[CraftingUI] InventoryController não encontrado!");
            enabled = false;
            return;
        }

        SetupButtons();
    }

    private void Start()
    {
        if (craftingPanel != null)
            craftingPanel.SetActive(false);

        if (detailsPanel != null)
            detailsPanel.SetActive(false);

        // Inscreve-se em mudanças do inventário para atualizar botões
        inventory.OnInventoryChanged += RefreshRecipeButtons;
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshRecipeButtons;
    }

    // ---------------------------
    // SETUP
    // ---------------------------

    private void SetupButtons()
    {
        if (allButton != null)
            allButton.onClick.AddListener(() => SetFilter(RecipeFilter.All));

        if (craftableButton != null)
            craftableButton.onClick.AddListener(() => SetFilter(RecipeFilter.Craftable));

        if (lockedButton != null)
            lockedButton.onClick.AddListener(() => SetFilter(RecipeFilter.Locked));

        if (craftButton != null)
            craftButton.onClick.AddListener(CraftSelectedRecipe);

        if (searchField != null)
            searchField.onValueChanged.AddListener(OnSearchChanged);
    }

    // ---------------------------
    // OPEN/CLOSE
    // ---------------------------

    public void Open()
    {
        if (craftingPanel != null)
            craftingPanel.SetActive(true);

        RefreshRecipeList();
        UpdateFilterButtons();
    }

    public void Close()
    {
        if (craftingPanel != null)
            craftingPanel.SetActive(false);

        if (detailsPanel != null)
            detailsPanel.SetActive(false);

        selectedRecipe = null;
    }

    public void Toggle()
    {
        //if (craftingPanel != null && craftingPanel.activeSelf)
        if (craftingPanel != null && craftingPanel.gameObject.activeSelf)
            Close();
        else
            Open();
    }

    // ---------------------------
    // FILTROS
    // ---------------------------

    private void SetFilter(RecipeFilter filter)
    {
        currentFilter = filter;
        RefreshRecipeList();
        UpdateFilterButtons();
    }

    private void UpdateFilterButtons()
    {
        if (allButton != null)
            allButton.GetComponent<Image>().color =
                currentFilter == RecipeFilter.All ? activeFilterColor : inactiveFilterColor;

        if (craftableButton != null)
            craftableButton.GetComponent<Image>().color =
                currentFilter == RecipeFilter.Craftable ? activeFilterColor : inactiveFilterColor;

        if (lockedButton != null)
            lockedButton.GetComponent<Image>().color =
                currentFilter == RecipeFilter.Locked ? activeFilterColor : inactiveFilterColor;
    }

    // ---------------------------
    // BUSCA
    // ---------------------------

    private void OnSearchChanged(string searchText)
    {
        RefreshRecipeList();
    }

    // ---------------------------
    // LISTA DE RECEITAS
    // ---------------------------

    public void RefreshRecipeList()
    {
        ClearRecipeButtons();
        CreateRecipeButtons();

        // Reseta scroll para o topo
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    private void RefreshRecipeButtons()
    {
        // Atualiza apenas o visual dos botões existentes (mais performático)
        foreach (var button in recipeButtons)
        {
            if (button != null)
                button.UpdateVisual();
        }
    }

    private void ClearRecipeButtons()
    {
        foreach (var button in recipeButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        recipeButtons.Clear();
    }

    private void CreateRecipeButtons()
    {
        var recipes = GetFilteredRecipes();

        foreach (var recipe in recipes)
        {
            GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeGrid);
            RecipeButton button = buttonObj.GetComponent<RecipeButton>();

            if (button != null)
            {
                button.Setup(recipe, craftingManager);
                button.OnSelected += OnRecipeSelected;
                recipeButtons.Add(button);
            }
        }

        Debug.Log($"[CraftingUI] {recipes.Count} receitas exibidas");
    }

    private List<CraftingRecipe> GetFilteredRecipes()
    {
        var allRecipes = craftingManager.GetAvailableRecipes();
        var filtered = new List<CraftingRecipe>();

        string searchText = searchField != null ? searchField.text.ToLower() : "";

        foreach (var recipe in allRecipes)
        {
            // Filtro de busca
            if (!string.IsNullOrEmpty(searchText))
            {
                if (!recipe.RecipeName.ToLower().Contains(searchText))
                    continue;
            }

            // Filtro de categoria
            bool canCraft = recipe.CanCraft(inventory);

            switch (currentFilter)
            {
                case RecipeFilter.Craftable:
                    if (!canCraft) continue;
                    break;

                case RecipeFilter.Locked:
                    if (canCraft) continue;
                    break;
            }

            filtered.Add(recipe);
        }

        // Ordena: craftáveis primeiro, depois por nome
        return filtered.OrderByDescending(r => r.CanCraft(inventory))
                      .ThenBy(r => r.RecipeName)
                      .ToList();
    }

    // ---------------------------
    // DETALHES DA RECEITA
    // ---------------------------

    private void OnRecipeSelected(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        ShowRecipeDetails(recipe);
    }

    private void ShowRecipeDetails(CraftingRecipe recipe)
    {
        if (detailsPanel != null)
            detailsPanel.SetActive(true);

        if (recipeIcon != null)
            recipeIcon.sprite = recipe.Icon;

        if (recipeName != null)
            recipeName.text = recipe.RecipeName;

        if (recipeDescription != null)
            recipeDescription.text = recipe.Description;

        UpdateIngredientsText(recipe);
        UpdateCraftButton(recipe);
    }

    private void UpdateIngredientsText(CraftingRecipe recipe)
    {
        if (ingredientsText == null) return;

        string text = "<b>Ingredientes:</b>\n";

        foreach (var ingredient in recipe.Ingredients)
        {
            float current = inventory.Get(ingredient.itemType);
            float needed = ingredient.amount;
            bool hasEnough = current >= needed;

            string color = hasEnough ? "green" : "red";
            text += $"• <color={color}>{ingredient.itemName}: {current}/{needed}</color>\n";
        }

        ingredientsText.text = text;
    }

    private void UpdateCraftButton(CraftingRecipe recipe)
    {
        if (craftButton == null) return;

        bool canCraft = recipe.CanCraft(inventory);
        craftButton.interactable = canCraft;

        var buttonText = craftButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
            buttonText.text = canCraft ? "Craft" : "Locked";
    }

    // ---------------------------
    // CRAFTING
    // ---------------------------

    private void CraftSelectedRecipe()
    {
        if (selectedRecipe == null) return;

        bool success = craftingManager.TryCraft(selectedRecipe);

        if (success)
        {
            // Atualiza detalhes
            ShowRecipeDetails(selectedRecipe);

            // Atualiza lista se necessário
            if (currentFilter == RecipeFilter.Craftable)
                RefreshRecipeList();
        }
    }
}
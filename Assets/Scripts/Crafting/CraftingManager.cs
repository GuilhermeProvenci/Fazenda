using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerenciador central do sistema de crafting
/// </summary>
public class CraftingManager : MonoBehaviour
{
    [Header("Receitas Disponíveis")]
    [SerializeField] private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

    [Header("Settings")]
    [SerializeField] private KeyCode craftingMenuKey = KeyCode.C;
    [SerializeField] private bool requireWorkbenchForAll = false;

    [Header("References")]
    [SerializeField] private GameObject craftingUI;
    [SerializeField] private Transform craftingGrid;
    [SerializeField] private GameObject recipeButtonPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip craftSound;
    [SerializeField] private AudioClip failSound;

    private InventoryController inventory;
    private AudioSource audioSource;
    private bool isNearWorkbench = false;
    private bool menuOpen = false;

    // Eventos
    public event Action<CraftingRecipe> OnItemCrafted;
    public event Action<string> OnCraftFailed;

    // Singleton
    public static CraftingManager Instance { get; private set; }

    // ---------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        inventory = FindObjectOfType<InventoryController>();

        if (inventory == null)
        {
            Debug.LogError("CraftingManager: InventoryController não encontrado!");
            enabled = false;
            return;
        }

        if (craftingUI != null)
            craftingUI.SetActive(false);

        Debug.Log($"[CraftingManager] Carregadas {allRecipes.Count} receitas");
    }

    private void Update()
    {
        HandleInput();
    }

    // ---------------------------
    // INPUT
    // ---------------------------

    private void HandleInput()
    {
        if (Input.GetKeyDown(craftingMenuKey))
            ToggleCraftingMenu();
    }

    public void ToggleCraftingMenu()
    {
        menuOpen = !menuOpen;

        if (craftingUI != null)
        {
            craftingUI.SetActive(menuOpen);

            if (menuOpen)
                RefreshRecipeList();
        }

        Debug.Log($"[CraftingManager] Menu {(menuOpen ? "aberto" : "fechado")}");
    }

    // ---------------------------
    // CRAFTING LOGIC
    // ---------------------------

    public bool TryCraft(CraftingRecipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogError("[CraftingManager] Receita é null!");
            return false;
        }

        Debug.Log($"[CraftingManager] Tentando craftar: {recipe.RecipeName}");

        // Workbench
        if (recipe.RequiresWorkbench && !isNearWorkbench && requireWorkbenchForAll)
        {
            FailCraft("Precisa estar perto de uma bancada!");
            return false;
        }

        // Ingredientes
        if (!recipe.CanCraft(inventory))
        {
            string missing = recipe.GetMissingIngredientsText(inventory);
            FailCraft($"Ingredientes insuficientes:\n{missing}");
            return false;
        }

        // Consumir ingredientes
        if (!recipe.ConsumeIngredients(inventory))
        {
            FailCraft("Erro ao consumir ingredientes!");
            return false;
        }

        // Adiciona item craftado ao inventário
        inventory.Add(recipe.ResultItemType, recipe.ResultAmount);

        // Spawn no mundo
        if (recipe.ResultPrefab != null)
            SpawnCraftedItem(recipe);

        PlaySound(craftSound);
        OnItemCrafted?.Invoke(recipe);

        Debug.Log($"[CraftingManager] ✓ Craftado: {recipe.RecipeName} x{recipe.ResultAmount}");
        return true;
    }

    private void FailCraft(string message)
    {
        OnCraftFailed?.Invoke(message);
        PlaySound(failSound);
        Debug.Log("[CraftingManager] Falha: " + message);
    }

    // ---------------------------
    // SPAWN RESULT
    // ---------------------------

    private void SpawnCraftedItem(CraftingRecipe recipe)
    {
        // pega posição do player (não exige PlayerItens)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        Vector3 spawnPos = player.transform.position + Vector3.right * 1.5f;

        for (int i = 0; i < recipe.ResultAmount; i++)
        {
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                UnityEngine.Random.Range(-0.5f, 0.5f),
                0f
            );

            Instantiate(recipe.ResultPrefab, spawnPos + offset, Quaternion.identity);
        }

        Debug.Log($"[CraftingManager] Spawnado {recipe.ResultAmount}x {recipe.RecipeName}");
    }

    // ---------------------------
    // RECEITAS
    // ---------------------------

    public List<CraftingRecipe> GetAvailableRecipes() =>
        new List<CraftingRecipe>(allRecipes);

    public List<CraftingRecipe> GetCraftableRecipes()
    {
        List<CraftingRecipe> craftable = new List<CraftingRecipe>();

        foreach (var r in allRecipes)
            if (r.CanCraft(inventory))
                craftable.Add(r);

        return craftable;
    }

    public void AddRecipe(CraftingRecipe recipe)
    {
        if (recipe != null && !allRecipes.Contains(recipe))
        {
            allRecipes.Add(recipe);
            Debug.Log($"[CraftingManager] Receita adicionada: {recipe.RecipeName}");
        }
    }

    public void RemoveRecipe(CraftingRecipe recipe)
    {
        if (allRecipes.Contains(recipe))
        {
            allRecipes.Remove(recipe);
            Debug.Log($"[CraftingManager] Receita removida: {recipe.RecipeName}");
        }
    }

    // ---------------------------
    // UI
    // ---------------------------

    private void RefreshRecipeList()
    {
        if (craftingGrid == null || recipeButtonPrefab == null)
            return;

        foreach (Transform child in craftingGrid)
            Destroy(child.gameObject);

        foreach (var recipe in allRecipes)
        {
            GameObject button = Instantiate(recipeButtonPrefab, craftingGrid);
            RecipeButton rb = button.GetComponent<RecipeButton>();
            if (rb != null) rb.Setup(recipe, this);
        }

        Debug.Log($"[CraftingManager] UI atualizada com {allRecipes.Count} receitas");
    }

    // ---------------------------
    // WORKBENCH
    // ---------------------------

    public void SetNearWorkbench(bool near)
    {
        isNearWorkbench = near;
    }

    // ---------------------------
    // HELPERS
    // ---------------------------

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (isNearWorkbench)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(player.transform.position, 2f);
            }
        }
    }
}

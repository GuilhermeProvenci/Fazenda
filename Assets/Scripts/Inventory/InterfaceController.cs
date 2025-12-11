using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla a interface do inventário
/// 
/// </summary>
public class InterfaceController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private Transform itemsGrid;
    [SerializeField] private GameObject slotPrefab;

    [Header("Database")]
    [SerializeField] private ItemDatabase itemDatabase; // Opcional - usa Resources se null

    [Header("Settings")]
    [SerializeField] private bool showEmptySlots = true;
    [SerializeField] private bool useDatabase = true;

    // Estado
    private bool isInventoryOpen = false;
    private List<InventorySlot> slots = new List<InventorySlot>();

    // Componentes
    private InventoryController inventory;

    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------

    private void Awake()
    {
        inventory = FindObjectOfType<InventoryController>();

        if (inventory == null)
        {
            Debug.LogError("[InterfaceController] InventoryController não encontrado!");
            enabled = false;
            return;
        }

        if (itemsGrid == null)
        {
            Debug.LogError("[InterfaceController] ItemsGrid não configurado!");
            enabled = false;
            return;
        }

        if (slotPrefab == null)
        {
            Debug.LogError("[InterfaceController] SlotPrefab não configurado!");
            enabled = false;
            return;
        }

        // Carrega database se não foi setado manualmente
        if (itemDatabase == null && useDatabase)
        {
            itemDatabase = ItemDatabase.Instance;
        }
    }

    private void Start()
    {
        // Fecha inventário no início
        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        // Inscreve-se nas mudanças de inventário
        inventory.OnInventoryChanged += RefreshInventory;

        // Cria slots iniciais
        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshInventory;
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
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryUI != null)
            inventoryUI.SetActive(isInventoryOpen);

        if (isInventoryOpen)
            RefreshInventory();
    }

    // ---------------------------
    // REFRESH
    // ---------------------------

    private void RefreshInventory()
    {
        ClearSlots();
        CreateSlots();
    }

    private void ClearSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        slots.Clear();
    }

    private void CreateSlots()
    {
        // Percorre todos os tipos de item
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            float amount = inventory.Get(type);

            // Se não mostrar slots vazios e não tem item, pula
            if (!showEmptySlots && amount <= 0)
                continue;

            // Cria o slot
            GameObject slotObj = Instantiate(slotPrefab, itemsGrid);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();

            if (slot != null)
            {
                Sprite icon = GetIconForItem(type);
                string name = GetNameForItem(type);

                slot.Setup(type, amount, icon, name);
                slots.Add(slot);
            }
        }

        Debug.Log($"[InterfaceController] {slots.Count} slots criados");
    }

    // ---------------------------
    // ITEM INFO
    // ---------------------------

    private Sprite GetIconForItem(ItemType type)
    {
        // Tenta pegar do database primeiro
        if (useDatabase && itemDatabase != null)
        {
            return itemDatabase.GetIcon(type);
        }

        // Fallback: retorna null (slot vai mostrar só texto)
        return null;
    }

    private string GetNameForItem(ItemType type)
    {
        // Tenta pegar do database primeiro
        if (useDatabase && itemDatabase != null)
        {
            return itemDatabase.GetDisplayName(type);
        }

        // Fallback: nome formatado do enum
        return FormatEnumName(type.ToString());
    }

    private string FormatEnumName(string enumName)
    {
        // "WoodenPlank" → "Wooden Plank"
        return System.Text.RegularExpressions.Regex.Replace(
            enumName,
            "([a-z])([A-Z])",
            "$1 $2"
        );
    }

    // ---------------------------
    // PUBLIC API
    // ---------------------------

    public void OpenInventory()
    {
        isInventoryOpen = true;
        if (inventoryUI != null)
            inventoryUI.SetActive(true);
        RefreshInventory();
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        if (inventoryUI != null)
            inventoryUI.SetActive(false);
    }

    // ---------------------------
    // DEBUG
    // ---------------------------

    [ContextMenu("Force Refresh")]
    private void DebugRefresh()
    {
        if (Application.isPlaying)
            RefreshInventory();
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador da interface do inventário
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool pauseGameWhenOpen = false;
    
    [Header("Layout")]
    [SerializeField] private int slotsPerRow = 6;
    [SerializeField] private float slotSpacing = 10f;
    
    // Estado
    private bool isOpen = false;
    private List<UISlot> uiSlots = new List<UISlot>();
    
    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------
    
    private void Awake()
    {
        // Encontra inventory se não setado
        if (inventory == null)
            inventory = FindObjectOfType<InventorySystem>();
        
        if (inventory == null)
        {
            Debug.LogError("[InventoryUI] InventorySystem not found!");
            enabled = false;
            return;
        }
        
        // Valida referências
        if (inventoryPanel == null)
        {
            Debug.LogError("[InventoryUI] Inventory panel not assigned!");
            enabled = false;
            return;
        }
        
        if (slotsContainer == null)
        {
            Debug.LogError("[InventoryUI] Slots container not assigned!");
            enabled = false;
            return;
        }
        
        if (slotPrefab == null)
        {
            Debug.LogError("[InventoryUI] Slot prefab not assigned!");
            enabled = false;
            return;
        }
    }
    
    private void Start()
    {
        // Fecha inventário no início
        inventoryPanel.SetActive(false);
        isOpen = false;
        
        // Inscreve nos eventos do inventário
        inventory.OnInventoryChanged += RefreshUI;
        
        // Cria slots iniciais
        CreateSlots();
        RefreshUI();
    }
    
    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshUI;
        }
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
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
        
        // ESC para fechar
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
    
    // ---------------------------
    // CONTROLE DE VISIBILIDADE
    // ---------------------------
    
    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }
    
    public void Open()
    {
        inventoryPanel.SetActive(true);
        isOpen = true;
        
        if (pauseGameWhenOpen)
            Time.timeScale = 0f;
        
        RefreshUI();
        
        Debug.Log("[InventoryUI] Inventory opened");
    }
    
    public void Close()
    {
        inventoryPanel.SetActive(false);
        isOpen = false;
        
        if (pauseGameWhenOpen)
            Time.timeScale = 1f;
        
        Debug.Log("[InventoryUI] Inventory closed");
    }
    
    // ---------------------------
    // CRIAÇÃO DE SLOTS
    // ---------------------------
    
    private void CreateSlots()
    {
        // Limpa slots existentes
        ClearSlots();
        
        var allSlots = inventory.GetAllSlots();
        
        // Configura grid layout
        var gridLayout = slotsContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraintCount = slotsPerRow;
            gridLayout.spacing = new Vector2(slotSpacing, slotSpacing);
        }
        
        // Cria um UISlot para cada slot do inventário
        for (int i = 0; i < allSlots.Count; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            slotObj.name = $"Slot_{i}";
            
            UISlot uiSlot = slotObj.GetComponent<UISlot>();
            if (uiSlot == null)
            {
                uiSlot = slotObj.AddComponent<UISlot>();
            }
            
            // Adiciona DragHandler se não tiver
            if (slotObj.GetComponent<DragHandler>() == null)
            {
                slotObj.AddComponent<DragHandler>();
            }
            
            uiSlot.Initialize(inventory, i);
            uiSlot.Setup(allSlots[i]);
            
            uiSlots.Add(uiSlot);
        }
        
        Debug.Log($"[InventoryUI] Created {uiSlots.Count} UI slots");
    }
    
    private void ClearSlots()
    {
        foreach (var slot in uiSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        
        uiSlots.Clear();
    }
    
    // ---------------------------
    // REFRESH
    // ---------------------------
    
    private void RefreshUI()
    {
        var allSlots = inventory.GetAllSlots();
        
        // Se número de slots mudou, recria tudo
        if (uiSlots.Count != allSlots.Count)
        {
            CreateSlots();
            return;
        }
        
        // Atualiza cada slot
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < allSlots.Count)
            {
                uiSlots[i].Setup(allSlots[i]);
            }
        }
    }
    
    // ---------------------------
    // PUBLIC API
    // ---------------------------
    
    public bool IsOpen => isOpen;
    
    public void RefreshSlot(int index)
    {
        if (index >= 0 && index < uiSlots.Count)
        {
            var slotData = inventory.GetSlot(index);
            if (slotData != null)
            {
                uiSlots[index].Setup(slotData);
            }
        }
    }
    
    // ---------------------------
    // DEBUG
    // ---------------------------
    
    [ContextMenu("Force Refresh")]
    private void DebugForceRefresh()
    {
        if (Application.isPlaying)
        {
            RefreshUI();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Representa um slot visual do inventário com suporte a drag & drop
/// </summary>
public class UISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectionHighlight;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color highlightColor = new Color(0.4f, 0.4f, 0.4f, 0.9f);
    [SerializeField] private Color emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    [SerializeField] private Color selectedColor = new Color(0.5f, 0.5f, 0f, 1f);
    
    // Estado
    private InventorySlotData slotData;
    private int slotIndex;
    private bool isHovered;
    private bool isSelected;
    
    // Referências
    private InventorySystem inventory;
    
    // ---------------------------
    // SETUP
    // ---------------------------
    
    public void Initialize(InventorySystem inventorySystem, int index)
    {
        inventory = inventorySystem;
        slotIndex = index;
        
        if (selectionHighlight != null)
            selectionHighlight.SetActive(false);
    }
    
    public void Setup(InventorySlotData data)
    {
        slotData = data;
        Refresh();
    }
    
    public void Refresh()
    {
        if (slotData == null || slotData.IsEmpty)
        {
            // Slot vazio
            if (iconImage != null)
                iconImage.enabled = false;
            
            if (quantityText != null)
                quantityText.text = "";
            
            if (backgroundImage != null)
                backgroundImage.color = emptyColor;
            
            return;
        }
        
        // Slot com item
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = slotData.Item.icon;
        }
        
        if (quantityText != null)
        {
            quantityText.text = slotData.Quantity > 1 ? slotData.Quantity.ToString() : "";
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : 
                                    isHovered ? highlightColor : 
                                    normalColor;
        }
    }
    
    // ---------------------------
    // EVENTOS DE MOUSE
    // ---------------------------
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        
        if (!slotData.IsEmpty && backgroundImage != null)
        {
            backgroundImage.color = highlightColor;
        }
        
        // TODO: Mostrar tooltip
        ShowTooltip();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor :
                                    slotData.IsEmpty ? emptyColor : 
                                    normalColor;
        }
        
        // TODO: Esconder tooltip
        HideTooltip();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }
    
    private void OnLeftClick()
    {
        // Seleciona o slot
        SetSelected(!isSelected);
        
        Debug.Log($"[UISlot] Clicked slot {slotIndex}: {slotData}");
    }
    
    private void OnRightClick()
    {
        // Usa o item
        if (!slotData.IsEmpty)
        {
            UseItem();
        }
    }
    
    // ---------------------------
    // AÇÕES
    // ---------------------------
    
    private void UseItem()
    {
        if (slotData == null || slotData.IsEmpty) return;
        
        // Encontra o jogador
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[UISlot] Player not found!");
            return;
        }
        
        // Usa o item
        bool consumed = slotData.Item.Use(player);
        
        // Se foi consumido, remove do inventário
        if (consumed)
        {
            inventory.RemoveFromSlot(slotIndex, 1);
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selectionHighlight != null)
            selectionHighlight.SetActive(selected);
        
        Refresh();
    }
    
    // ---------------------------
    // TOOLTIP
    // ---------------------------
    
    private void ShowTooltip()
    {
        if (slotData == null || slotData.IsEmpty) return;
        
        // TODO: Implementar sistema de tooltip
        // Por enquanto, apenas log
        Debug.Log($"[Tooltip] {slotData.Item.GetTooltip()}");
    }
    
    private void HideTooltip()
    {
        // TODO: Esconder tooltip
    }
    
    // ---------------------------
    // GETTERS
    // ---------------------------
    
    public InventorySlotData SlotData => slotData;
    public int SlotIndex => slotIndex;
    public bool IsEmpty => slotData == null || slotData.IsEmpty;
}

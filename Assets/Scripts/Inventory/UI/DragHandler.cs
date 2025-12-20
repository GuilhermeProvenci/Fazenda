using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Gerencia o arrasto de itens do inventário
/// </summary>
[RequireComponent(typeof(UISlot))]
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster raycaster;
    
    [Header("Settings")]
    [SerializeField] private float dragAlpha = 0.6f;
    
    private UISlot uiSlot;
    private GameObject dragIcon;
    private CanvasGroup dragCanvasGroup;
    private RectTransform dragRectTransform;
    private Vector2 originalPosition;
    
    private void Awake()
    {
        uiSlot = GetComponent<UISlot>();
        
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        
        if (raycaster == null)
            raycaster = canvas.GetComponent<GraphicRaycaster>();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Não arrasta se slot vazio
        if (uiSlot.IsEmpty) return;
        
        // Cria ícone de arrasto
        CreateDragIcon();
        
        Debug.Log($"[DragHandler] Started dragging from slot {uiSlot.SlotIndex}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon == null) return;
        
        // Atualiza posição do ícone
        dragRectTransform.position = eventData.position;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon == null) return;
        
        // Encontra o slot de destino
        var targetSlot = GetSlotUnderPointer(eventData);
        
        if (targetSlot != null && targetSlot != uiSlot)
        {
            // Move o item
            var inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null)
            {
                // Shift pressionado = split stack
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    inventory.SplitStack(uiSlot.SlotIndex, targetSlot.SlotIndex);
                }
                else
                {
                    inventory.MoveItem(uiSlot.SlotIndex, targetSlot.SlotIndex);
                }
            }
        }
        
        // Destroi ícone de arrasto
        DestroyDragIcon();
        
        Debug.Log($"[DragHandler] Ended dragging");
    }
    
    // ---------------------------
    // DRAG ICON
    // ---------------------------
    
    private void CreateDragIcon()
    {
        // Cria GameObject para o ícone
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(canvas.transform);
        dragIcon.transform.SetAsLastSibling(); // Fica por cima de tudo
        
        // Adiciona RectTransform
        dragRectTransform = dragIcon.AddComponent<RectTransform>();
        dragRectTransform.sizeDelta = new Vector2(64, 64); // Tamanho do ícone
        
        // Adiciona Image
        var image = dragIcon.AddComponent<Image>();
        image.sprite = uiSlot.SlotData.Item.icon;
        image.raycastTarget = false; // Não bloqueia raycasts
        
        // Adiciona CanvasGroup para controlar alpha
        dragCanvasGroup = dragIcon.AddComponent<CanvasGroup>();
        dragCanvasGroup.alpha = dragAlpha;
        dragCanvasGroup.blocksRaycasts = false; // Não bloqueia raycasts
        
        // Adiciona texto de quantidade (se > 1)
        if (uiSlot.SlotData.Quantity > 1)
        {
            var textObj = new GameObject("Quantity");
            textObj.transform.SetParent(dragIcon.transform);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(1, 0);
            textRect.anchorMax = new Vector2(1, 0);
            textRect.pivot = new Vector2(1, 0);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(30, 20);
            
            var text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = uiSlot.SlotData.Quantity.ToString();
            text.fontSize = 14;
            text.alignment = TMPro.TextAlignmentOptions.BottomRight;
            text.color = Color.white;
            text.raycastTarget = false;
        }
    }
    
    private void DestroyDragIcon()
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
            dragRectTransform = null;
            dragCanvasGroup = null;
        }
    }
    
    // ---------------------------
    // HELPERS
    // ---------------------------
    
    private UISlot GetSlotUnderPointer(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        raycaster.Raycast(eventData, results);
        
        foreach (var result in results)
        {
            var slot = result.gameObject.GetComponent<UISlot>();
            if (slot != null)
                return slot;
            
            // Tenta no pai também
            slot = result.gameObject.GetComponentInParent<UISlot>();
            if (slot != null)
                return slot;
        }
        
        return null;
    }
}

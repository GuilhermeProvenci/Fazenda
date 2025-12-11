using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    [SerializeField] private Color filledColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    [SerializeField] private Color highlightColor = Color.yellow;

    // Estado
    private ItemType itemType;
    private float quantity;
    private bool isEmpty = true;

    // ---------------------------
    // SETUP
    // ---------------------------

    /// <summary>
    /// Configura o slot com um item
    /// </summary>
    public void Setup(ItemType type, float amount, Sprite icon, string itemName)
    {
        itemType = type;
        quantity = amount;
        isEmpty = amount <= 0;

        // Atualiza visual
        if (backgroundImage != null)
            backgroundImage.color = isEmpty ? emptyColor : filledColor;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = !isEmpty && icon != null;
        }

        if (quantityText != null)
        {
            quantityText.text = isEmpty ? "" : $"{amount:F0}";
            quantityText.enabled = !isEmpty;
        }

        if (nameText != null)
        {
            nameText.text = isEmpty ? "" : itemName;
            nameText.enabled = !isEmpty;
        }
    }

    /// <summary>
    /// Limpa o slot (sem item)
    /// </summary>
    public void Clear()
    {
        Setup(ItemType.Wood, 0, null, "");
    }

    /// <summary>
    /// Destaca o slot visualmente
    /// </summary>
    public void Highlight(bool active)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = active ? highlightColor :
                (isEmpty ? emptyColor : filledColor);
        }
    }

    // ---------------------------
    // GETTERS
    // ---------------------------

    public ItemType ItemType => itemType;
    public float Quantity => quantity;
    public bool IsEmpty => isEmpty;
}
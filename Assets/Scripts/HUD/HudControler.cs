using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Novo HUD totalmente baseado em InventoryController
/// </summary>
public class HudControler : MonoBehaviour
{
    [Header("Item Bars")]
    [SerializeField] private Image waterUIBar;
    [SerializeField] private Image woodUIBar;
    [SerializeField] private Image carrotUIBar;
    [SerializeField] private Image fishUIBar;

    [Header("Item Text (Opcional)")]
    [SerializeField] private TextMeshProUGUI waterText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI carrotText;
    [SerializeField] private TextMeshProUGUI fishText;

    [Header("Tool Selection")]
    [SerializeField] private List<Image> toolsUI = new();
    [SerializeField] private Color selectColor = Color.white;
    [SerializeField] private Color alphaColor = new(1, 1, 1, 0.3f);
    [SerializeField] private float selectedScale = 0.5f;
    [SerializeField] private bool animateSelection = true;

    [Header("Visual Feedback")]
    [SerializeField] private bool showChangeAnimation = true;
    [SerializeField] private float animationSpeed = 5f;

    [Header("Low Resource Warning")]
    [SerializeField] private bool showLowWarning = true;
    [SerializeField] private float lowThreshold = 0.2f;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningBlinkSpeed = 2f;

    // Componentes
    private InventoryController inventory;
    private Player player;

    // Cache para animações
    private float targetWater;
    private float targetWood;
    private float targetCarrot;
    private float targetFish;

    private int lastSelectedTool = -1;
    private float warningTimer;

    private void Awake()
    {
        inventory = FindObjectOfType<InventoryController>();
        player = FindObjectOfType<Player>();

        if (inventory == null)
        {
            Debug.LogError("HUD: InventoryController não encontrado!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        InitializeBars();

        inventory.OnInventoryChanged += UpdateItemBars;

        UpdateItemBars();
        UpdateToolSelection();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateItemBars;
    }

    private void Update()
    {
        AnimateBars();
        UpdateToolSelection();
        UpdateWarnings();
    }

    // --------------------------
    // BARRAS
    // --------------------------

    private void InitializeBars()
    {
        SetBarFill(waterUIBar, 0);
        SetBarFill(woodUIBar, 0);
        SetBarFill(carrotUIBar, 0);
        SetBarFill(fishUIBar, 0);
    }

    private void UpdateItemBars()
    {
        targetWater = inventory.GetPercentage(ItemType.Water);
        targetWood = inventory.GetPercentage(ItemType.Wood);
        targetCarrot = inventory.GetPercentage(ItemType.Carrot);
        targetFish = inventory.GetPercentage(ItemType.Fish);

        UpdateItemTexts();
    }

    private void AnimateBars()
    {
        float spd = animationSpeed * Time.deltaTime;

        AnimateBar(waterUIBar, targetWater, spd);
        AnimateBar(woodUIBar, targetWood, spd);
        AnimateBar(carrotUIBar, targetCarrot, spd);
        AnimateBar(fishUIBar, targetFish, spd);
    }

    private void AnimateBar(Image bar, float target, float speed)
    {
        if (bar == null) return;

        bar.fillAmount = Mathf.Lerp(bar.fillAmount, target, speed);

        if (Mathf.Abs(bar.fillAmount - target) < 0.001f)
            bar.fillAmount = target;
    }

    private void UpdateItemTexts()
    {
        if (waterText != null)
            waterText.text = $"{inventory.Get(ItemType.Water):F0}/{inventory.GetLimit(ItemType.Water)}";

        if (woodText != null)
            woodText.text = $"{inventory.Get(ItemType.Wood)}/{inventory.GetLimit(ItemType.Wood)}";

        if (carrotText != null)
            carrotText.text = $"{inventory.Get(ItemType.Carrot)}/{inventory.GetLimit(ItemType.Carrot)}";

        if (fishText != null)
            fishText.text = $"{inventory.Get(ItemType.Fish)}/{inventory.GetLimit(ItemType.Fish)}";
    }

    // --------------------------
    // TOOLS
    // --------------------------

    private void UpdateToolSelection()
    {
        if (player == null || toolsUI.Count == 0)
            return;

        int current = (int)player.equippedTool;

        if (current == lastSelectedTool)
            return;

        for (int i = 0; i < toolsUI.Count; i++)
        {
            if (toolsUI[i] == null) continue;

            bool selected = (i == current);

            toolsUI[i].color = selected ? selectColor : alphaColor;

            if (animateSelection)
            {
                float scale = selected ? selectedScale : 1f;
                toolsUI[i].transform.localScale = Vector3.one * scale;
            }
        }

        lastSelectedTool = current;
    }

    // --------------------------
    // LOW RESOURCE
    // --------------------------

    private void UpdateWarnings()
    {
        if (!showLowWarning) return;

        warningTimer += Time.deltaTime;

        Warn(waterUIBar, targetWater);
        Warn(woodUIBar, targetWood);
        Warn(carrotUIBar, targetCarrot);
        Warn(fishUIBar, targetFish);
    }

    private void Warn(Image bar, float pct)
    {
        if (bar == null) return;

        if (pct < lowThreshold && pct > 0)
        {
            float t = Mathf.PingPong(warningTimer * warningBlinkSpeed, 1f);
            bar.color = Color.Lerp(Color.white, warningColor, t);
        }
        else
        {
            bar.color = Color.white;
        }
    }

    private void SetBarFill(Image bar, float v)
    {
        if (bar != null)
            bar.fillAmount = v;
    }
}

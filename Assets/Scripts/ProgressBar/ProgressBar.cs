using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sistema reutilizável de barra de progresso para feedback visual
/// </summary>
public class ProgressBar : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Canvas canvas;

    [Header("Colors")]
    [SerializeField] private Color progressColor = Color.yellow;
    [SerializeField] private Color completeColor = Color.green;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.5f);

    [Header("Positioning")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Vector2 barSize = new Vector2(1f, 0.15f);

    private Transform target;
    private float currentProgress;
    private bool isVisible;

    private void Awake()
    {
        SetupCanvas();
        SetupImages();
        Hide();
    }

    private void SetupCanvas()
    {
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ProgressCanvas");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = Vector3.zero;

            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            RectTransform rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 20);
            rect.localScale = Vector3.one * 0.01f;

            // CRÍTICO: Garante que canvas está na layer correta
            canvasObj.layer = gameObject.layer;

            Debug.Log($"[ProgressBar] Canvas criado em {transform.name}");
        }
    }

    private void SetupImages()
    {
        // Background
        if (backgroundImage == null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.color = backgroundColor;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
        }

        // Fill
        if (fillImage == null)
        {
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(canvas.transform, false);
            fillImage = fillObj.AddComponent<Image>();
            fillImage.color = progressColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0f;

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = new Vector2(-4, -4); // Padding
        }
    }

    private void LateUpdate()
    {
        if (target != null && isVisible)
        {
            transform.position = target.position + offset;

            // Faz a barra sempre olhar para a câmera
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }
    }

    /// <summary>
    /// Atualiza o progresso da barra (0 a 1)
    /// </summary>
    public void UpdateProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);

        if (fillImage != null)
        {
            fillImage.fillAmount = currentProgress;

            // Muda cor quando completo
            fillImage.color = currentProgress >= 1f ? completeColor : progressColor;
        }

        if (!isVisible)
            Show();
    }

    /// <summary>
    /// Define o alvo que a barra deve seguir
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Define cores personalizadas
    /// </summary>
    public void SetColors(Color progress, Color complete, Color background)
    {
        progressColor = progress;
        completeColor = complete;
        backgroundColor = background;

        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;

        if (fillImage != null)
            fillImage.color = currentProgress >= 1f ? completeColor : progressColor;
    }

    /// <summary>
    /// Define o offset da barra em relação ao alvo
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void Show()
    {
        isVisible = true;
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
            Debug.Log($"[ProgressBar] Mostrando barra em {transform.name}");
        }
        else
        {
            Debug.LogError($"[ProgressBar] Canvas é NULL em {transform.name}!");
        }
    }

    public void Hide()
    {
        isVisible = false;
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    public void Reset()
    {
        currentProgress = 0f;
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = progressColor;
        }
        Hide();
    }

    /// <summary>
    /// Cria uma barra de progresso programaticamente
    /// </summary>
    public static ProgressBar Create(Transform parent, Vector3 offset = default)
    {
        GameObject barObj = new GameObject("ProgressBar");
        barObj.transform.SetParent(parent);
        barObj.transform.localPosition = Vector3.zero;

        ProgressBar bar = barObj.AddComponent<ProgressBar>();
        bar.SetTarget(parent);

        if (offset != default)
            bar.SetOffset(offset);

        return bar;
    }
}
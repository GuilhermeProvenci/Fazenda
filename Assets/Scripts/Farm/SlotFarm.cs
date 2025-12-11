using UnityEngine;
using System.Collections;

/// <summary>
/// Gerencia um slot de fazenda com sistema de progresso visual integrado ao InventoryController
/// </summary>
public class SlotFarm : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite groundSprite;
    [SerializeField] private Sprite holeSprite;
    [SerializeField] private Sprite carrotSprite;

    [Header("Sprite Auto-Setup")]
    [SerializeField] private bool useCurrentSpriteAsGround = true;

    [Header("Configurações de Cultivo")]
    [SerializeField] private int digRequired = 4;
    [SerializeField] private float waterRequired = 2f;
    [SerializeField] private float wateringSpeed = 0.5f;

    [Header("Visual Feedback")]
    [SerializeField] private bool showProgressBar = true;
    [SerializeField] private Vector3 progressBarOffset = new Vector3(0, 0.8f, 0);
    [SerializeField] private Color digColor = new Color(0.6f, 0.4f, 0.2f);
    [SerializeField] private Color waterColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color growthColor = Color.green;

    [Header("Audio")]
    [SerializeField] private AudioClip digSound;
    [SerializeField] private AudioClip harvestSound;

    // Estado interno
    private int digCounter;
    private float waterCounter;
    private bool holeDug;
    private bool isWatering;
    private bool isGrown;

    // Componentes
    private InventoryController inventory;
    private ProgressBar progressBar;
    private AudioSource audioSource;

    // Estados
    private enum FarmState { Empty, Digging, ReadyToPlant, Watering, Grown }
    private FarmState currentState = FarmState.Empty;

    // ------------------------------------------------------
    // UNITY
    // ------------------------------------------------------

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("[SlotFarm] SpriteRenderer não encontrado!");
                enabled = false;
                return;
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        inventory = FindObjectOfType<InventoryController>();

        if (inventory == null)
        {
            Debug.LogError("SlotFarm: InventoryController não encontrado!");
            enabled = false;
            return;
        }

        InitializeSlot();
        CreateProgressBar();
    }

    private void Update()
    {
        if (currentState == FarmState.Empty || currentState == FarmState.Digging)
            return;

        UpdateWatering();
        UpdateGrowth();
        UpdateVisualFeedback();
    }

    // ------------------------------------------------------
    // INICIALIZAÇÃO
    // ------------------------------------------------------

    private void InitializeSlot()
    {
        digCounter = digRequired;
        waterCounter = 0f;
        holeDug = false;
        isGrown = false;
        currentState = FarmState.Empty;

        if (groundSprite == null && useCurrentSpriteAsGround)
            groundSprite = spriteRenderer.sprite;

        spriteRenderer.sprite = groundSprite;
    }

    private void CreateProgressBar()
    {
        if (!showProgressBar)
            return;

        progressBar = ProgressBar.Create(transform, progressBarOffset);
        progressBar.SetColors(digColor, growthColor, new Color(0, 0, 0, 0.7f));
        progressBar.Hide();
    }

    // ------------------------------------------------------
    // ÁUDIO
    // ------------------------------------------------------

    private void PlaySFX(AudioClip clip, bool interruptible = false)
    {
        if (clip == null) return;

        if (interruptible)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // ------------------------------------------------------
    // CAVAÇÃO
    // ------------------------------------------------------

    private void OnHit()
    {
        if (holeDug)
            return;

        digCounter--;
        currentState = FarmState.Digging;

        UpdateDigProgress();
        PlaySFX(digSound, interruptible: true);

        if (digCounter <= 0)
            CompleteDigging();
    }

    private void CompleteDigging()
    {
        holeDug = true;
        currentState = FarmState.ReadyToPlant;

        if (holeSprite != null)
            spriteRenderer.sprite = holeSprite;

        if (progressBar != null)
            StartCoroutine(HideProgressBarDelayed(0.5f));
    }

    private void UpdateDigProgress()
    {
        if (progressBar == null || holeDug)
            return;

        float progress = 1f - ((float)digCounter / digRequired);
        progressBar.UpdateProgress(progress);
    }

    // ------------------------------------------------------
    // IRRIGAÇÃO
    // ------------------------------------------------------

    private void UpdateWatering()
    {
        if (!isWatering || !holeDug || isGrown)
            return;

        currentState = FarmState.Watering;

        if (!inventory.Has(ItemType.Water, wateringSpeed * Time.deltaTime))
            return;

        if (inventory.Remove(ItemType.Water, wateringSpeed * Time.deltaTime))
        {
            waterCounter += wateringSpeed * Time.deltaTime;
            waterCounter = Mathf.Min(waterCounter, waterRequired);
        }
    }

    private void UpdateWaterProgress()
    {
        if (progressBar == null || !holeDug)
            return;

        float progress = waterCounter / waterRequired;
        progressBar.UpdateProgress(progress);
    }

    // ------------------------------------------------------
    // CRESCIMENTO
    // ------------------------------------------------------

    private void UpdateGrowth()
    {
        if (waterCounter < waterRequired || isGrown)
            return;

        GrowPlant();
    }

    private void GrowPlant()
    {
        isGrown = true;
        currentState = FarmState.Grown;

        if (carrotSprite != null)
            spriteRenderer.sprite = carrotSprite;

        if (progressBar != null)
        {
            progressBar.UpdateProgress(1f);
            progressBar.SetColors(growthColor, growthColor, new Color(0, 0, 0, 0.7f));
        }
    }

    // ------------------------------------------------------
    // COLHEITA
    // ------------------------------------------------------

    private void Harvest()
    {
        if (!isGrown)
            return;

        if (inventory.Add(ItemType.Carrot, 1))
        {
            PlaySFX(harvestSound);
            ResetSlot();
        }
    }

    private void ResetSlot()
    {
        waterCounter = 0f;
        isGrown = false;
        currentState = FarmState.ReadyToPlant;

        if (holeSprite != null)
            spriteRenderer.sprite = holeSprite;

        progressBar?.Hide();
    }

    // ------------------------------------------------------
    // VISUAL
    // ------------------------------------------------------

    private void UpdateVisualFeedback()
    {
        if (progressBar == null)
            return;

        if (currentState == FarmState.Digging)
            UpdateDigProgress();

        if (currentState == FarmState.Watering)
            UpdateWaterProgress();
    }

    // ------------------------------------------------------
    // TRIGGERS
    // ------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Dig"))
            OnHit();

        if (col.CompareTag("Water"))
            isWatering = true;
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player") && isGrown)
        {
            if (Input.GetKeyDown(KeyCode.E))
                Harvest();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Water"))
            isWatering = false;

        if (col.CompareTag("Dig"))
        {
            if (audioSource.clip == digSound)
                audioSource.Stop();
        }
    }

    // ------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------

    private IEnumerator HideProgressBarDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        progressBar?.Hide();
    }
}

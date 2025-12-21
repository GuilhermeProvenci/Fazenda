using UnityEngine;
using System.Collections;

/// <summary>
/// Gerencia fonte de água com feedback visual e sistema de coleta
/// </summary>
public class Water : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float waterPerSecond = 2f;
    [SerializeField] private float maxCollectionTime = 3f;
    [SerializeField] private KeyCode collectKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private bool showProgressBar = true;
    [SerializeField] private Vector3 progressBarOffset = new Vector3(0, 1.2f, 0);
    [SerializeField] private Color waterColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color completeColor = Color.cyan;

    [Header("UI Feedback")]
    [SerializeField] private GameObject promptUI;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip completeSound;

    [Header("Efeitos Visuais")]
    [SerializeField] private ParticleSystem waterParticles;
    [SerializeField] private SpriteRenderer waterSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color collectingColor = Color.cyan;

    // Estado
    private bool detectingPlayer;
    private bool isCollecting;
    private float collectionProgress;

    [Header("Inventory")]
    private ItemData waterItem;

    // Componentes
    private InventorySystem inventory;
    private ProgressBar progressBar;
    private AudioSource audioSource;

    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (collectSound != null || completeSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();

        if (waterSprite == null)
            waterSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        inventory = FindObjectOfType<InventorySystem>();

        if (inventory == null)
        {
            Debug.LogError("Water: InventorySystem não encontrado!");
            enabled = false;
            return;
        }

        // Inicializa item via Registry
        waterItem = ItemRegistry.GetItem("Water");

        CreateProgressBar();

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        HandleInput();
        UpdateVisualState();
    }

    // ---------------------------
    // INICIALIZAÇÃO
    // ---------------------------

    private void CreateProgressBar()
    {
        if (!showProgressBar)
            return;

        progressBar = ProgressBar.Create(transform, progressBarOffset);
        progressBar.SetColors(waterColor, completeColor, new Color(0, 0, 0, 0.7f));
        progressBar.Hide();
    }

    // ---------------------------
    // INPUT E COLETA
    // ---------------------------

    private void HandleInput()
    {
        if (!detectingPlayer || waterItem == null)
            return;

        // Inicia coleta
        if (Input.GetKeyDown(collectKey) && !isCollecting)
        {
            if (inventory.GetItemCount(waterItem) >= waterItem.maxStackSize - 0.001f)
            {
                Debug.Log("Inventário de água está cheio!");
                return;
            }

            StartCollection();
        }

        // Cancela coleta
        if (Input.GetKeyUp(collectKey) && isCollecting)
        {
            StopCollection();
        }

        // Enquanto segurando a tecla
        if (Input.GetKey(collectKey) && isCollecting)
        {
            ContinueCollection();
        }
    }

    private void StartCollection()
    {
        isCollecting = true;
        collectionProgress = 0f;

        if (progressBar != null)
        {
            progressBar.Show();
            progressBar.UpdateProgress(0f);
        }

        if (waterParticles != null)
            waterParticles.Play();

        PlaySound(collectSound, true);

        Debug.Log("[Water] Coletando água...");
    }

    private void ContinueCollection()
    {
        if (!isCollecting || waterItem == null)
            return;

        // Progresso visual
        float deltaProgress = Time.deltaTime / maxCollectionTime;
        collectionProgress += deltaProgress;

        // Adicionar água
        float amount = waterPerSecond * Time.deltaTime;
        inventory.AddItem(waterItem, amount);

        // Atualiza barra
        if (progressBar != null)
        {
            progressBar.UpdateProgress(Mathf.Min(collectionProgress, 1f));
        }

        // Finaliza coleta
        if (collectionProgress >= 1f || inventory.GetItemCount(waterItem) >= waterItem.maxStackSize - 0.001f)
        {
            CompleteCollection();
        }
    }

    private void CompleteCollection()
    {
        isCollecting = false;
        collectionProgress = 0f;

        if (progressBar != null)
        {
            progressBar.UpdateProgress(1f);
            StartCoroutine(HideProgressBarDelayed(0.5f));
        }

        if (waterParticles != null)
            waterParticles.Stop();

        PlaySound(completeSound);

        Debug.Log("[Water] Água coletada!");
    }

    private void StopCollection()
    {
        if (!isCollecting)
            return;

        isCollecting = false;
        collectionProgress = 0f;

        if (progressBar != null)
            progressBar.Hide();

        if (waterParticles != null)
            waterParticles.Stop();

        if (audioSource != null)
            audioSource.Stop();

        Debug.Log("[Water] Coleta cancelada.");
    }

    // ---------------------------
    // VISUAL FEEDBACK
    // ---------------------------

    private void UpdateVisualState()
    {
        // Cor do sprite
        if (waterSprite != null)
            waterSprite.color = isCollecting ? collectingColor : normalColor;

        // Prompt
        if (promptUI != null && waterItem != null)
        {
            bool isFull = inventory.GetItemCount(waterItem) >= waterItem.maxStackSize - 0.001f;
            bool show = detectingPlayer && !isCollecting && !isFull;
            promptUI.SetActive(show);
        }
    }

    // ---------------------------
    // TRIGGERS
    // ---------------------------

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            detectingPlayer = true;

            if (waterItem != null && inventory.GetItemCount(waterItem) >= waterItem.maxStackSize - 0.001f)
                Debug.Log("Inventário de água cheio!");
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            detectingPlayer = false;

            if (isCollecting)
                StopCollection();
        }
    }

    // ---------------------------
    // HELPERS
    // ---------------------------

    private void PlaySound(AudioClip clip, bool loop = false)
    {
        if (audioSource == null || clip == null)
            return;

        if (loop)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator HideProgressBarDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (progressBar != null)
            progressBar.Hide();
    }

    // ---------------------------
    // DEBUG
    // ---------------------------

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
    }

    [ContextMenu("Test Collection")]
    private void TestCollection()
    {
        if (Application.isPlaying)
        {
            StartCollection();
            Invoke(nameof(CompleteCollection), 1f);
        }
    }
}

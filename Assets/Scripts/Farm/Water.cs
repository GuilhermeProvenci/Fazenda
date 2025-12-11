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

    // Componentes
    private InventoryController inventory;
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
        inventory = FindObjectOfType<InventoryController>();

        if (inventory == null)
        {
            Debug.LogError("Water: InventoryController não encontrado!");
            enabled = false;
            return;
        }

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
        if (!detectingPlayer)
            return;

        // Inicia coleta
        if (Input.GetKeyDown(collectKey) && !isCollecting)
        {
            if (inventory.IsFull(ItemType.Water))
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
        if (!isCollecting)
            return;

        // Progresso visual
        float deltaProgress = Time.deltaTime / maxCollectionTime;
        collectionProgress += deltaProgress;

        // Adicionar água
        float amount = waterPerSecond * Time.deltaTime;
        inventory.Add(ItemType.Water, amount);

        // Atualiza barra
        if (progressBar != null)
        {
            progressBar.UpdateProgress(Mathf.Min(collectionProgress, 1f));
        }

        // Finaliza coleta
        if (collectionProgress >= 1f || inventory.IsFull(ItemType.Water))
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
        if (promptUI != null)
        {
            bool show = detectingPlayer && !isCollecting && !inventory.IsFull(ItemType.Water);
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

            if (inventory.IsFull(ItemType.Water))
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

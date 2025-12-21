using UnityEngine;
using System.Collections;

/// <summary>
/// Construção de casa com animação, martelo, recursos e progresso
/// Versão atualizada para InventoryController
/// </summary>
public class House : MonoBehaviour
{
    [Header("Build Requirements")]
    [SerializeField] private int woodAmount = 10;
    [SerializeField] private float timeAmount = 5f;

    [Header("Colors")]
    [SerializeField] private Color startColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color endColor = Color.white;
    [SerializeField] private Color canBuildColor = Color.green;
    [SerializeField] private Color cannotBuildColor = Color.red;

    [Header("Components")]
    [SerializeField] private GameObject houseColl;
    [SerializeField] private SpriteRenderer houseSprite;
    [SerializeField] private Transform point;
    [SerializeField] private KeyCode buildKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private bool showProgressBar = true;
    [SerializeField] private Vector3 progressBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] private bool showColorFeedback = true;

    [Header("Audio")]
    [SerializeField] private AudioClip hammerSound;
    [SerializeField] private AudioClip completeSound;

    [Header("Requirements Info")]
    [SerializeField] private ItemData woodItem;

    // ... (existing serialized fields) ...

    // Estado
    private bool detectingPlayer;
    private bool isBuilding;
    private bool isBuilt;
    private float timeCount;

    // Componentes
    private Player player;
    private PlayerAnim playerAnim;
    private InventorySystem inventory;
    private AudioSource audioSource;
    private ProgressBar progressBar;

    private Vector3 originalPlayerPosition;

    // ------------------------------------------------------
    // UNITY
    // ------------------------------------------------------

    private void Awake()
    {
        if (houseSprite == null)
            houseSprite = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hammerSound != null || completeSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();

        if (houseColl != null)
            houseColl.SetActive(false);
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        inventory = FindObjectOfType<InventorySystem>();

        if (player == null || inventory == null)
        {
            Debug.LogError("[House] Player ou InventorySystem não encontrado!");
            enabled = false;
            return;
        }

        playerAnim = player.GetComponent<PlayerAnim>();

        CreateProgressBar();
        InitializeVisual();
    }

    private void Update()
    {
        if (isBuilt)
            return;

        if (!isBuilding)
        {
            HandleInput();
            UpdateVisualFeedback();
        }
        else
        {
            UpdateBuildProgress();
        }
    }

    // ------------------------------------------------------
    // INICIALIZAÇÃO
    // ------------------------------------------------------

    private void InitializeVisual()
    {
        if (houseSprite != null)
            houseSprite.color = startColor;
    }

    private void CreateProgressBar()
    {
        if (!showProgressBar)
            return;

        progressBar = ProgressBar.Create(transform, progressBarOffset);
        progressBar.SetColors(Color.yellow, Color.green, new Color(0, 0, 0, 0.7f));
        progressBar.Hide();
    }

    // ------------------------------------------------------
    // INPUT
    // ------------------------------------------------------

    private void HandleInput()
    {
        if (!detectingPlayer)
            return;

        if (Input.GetKeyDown(buildKey))
            TryStartBuild();
    }

    private void TryStartBuild()
    {
        if (isBuilding || isBuilt || woodItem == null)
            return;

        float currentWood = inventory.GetItemCount(woodItem);
        if (currentWood < woodAmount)
        {
            Debug.Log($"[House] Madeira insuficiente {currentWood:F0}/{woodAmount}");
            return;
        }

        StartBuild();
    }

    private void StartBuild()
    {
        isBuilding = true;
        timeCount = 0f;

        if (!inventory.RemoveItem(woodItem, (float)woodAmount))
        {
            Debug.LogError("[House] Remove falhou, cancelando construção!");
            isBuilding = false;
            return;
        }

        Debug.Log("[House] Construção iniciada!");

        originalPlayerPosition = player.transform.position;

        if (point != null)
            player.transform.position = point.position;

        if (playerAnim != null)
            playerAnim.OnHammeringStarted();

        player.isPaused = true;

        if (houseSprite != null)
            houseSprite.color = startColor;

        if (progressBar != null)
        {
            progressBar.Show();
            progressBar.UpdateProgress(0f);
        }

        PlaySound(hammerSound, true);

        //promptUI?.SetActive(false);
    }

    // ------------------------------------------------------
    // PROGRESSO
    // ------------------------------------------------------

    private void UpdateBuildProgress()
    {
        timeCount += Time.deltaTime;
        float progress = Mathf.Clamp01(timeCount / timeAmount);

        progressBar?.UpdateProgress(progress);

        if (houseSprite != null && showColorFeedback)
            houseSprite.color = Color.Lerp(startColor, endColor, progress);

        if (timeCount >= timeAmount)
            CompleteBuild();
    }

    private void CompleteBuild()
    {
        isBuilding = false;
        isBuilt = true;

        Debug.Log("[House] Construção concluída!");

        playerAnim?.OnHammeringEnded();
        player.isPaused = false;

        if (houseSprite != null)
            houseSprite.color = endColor;

        houseColl?.SetActive(true);

        if (progressBar != null)
            StartCoroutine(HideProgressBarDelayed(1f));

        PlaySound(completeSound);
    }

    // ------------------------------------------------------
    // VISUAL
    // ------------------------------------------------------

    private void UpdateVisualFeedback()
    {
        if (!detectingPlayer || !showColorFeedback || woodItem == null)
            return;

        if (houseSprite != null)
        {
            bool canBuild = inventory.GetItemCount(woodItem) >= woodAmount;

            Color target = canBuild ? canBuildColor : cannotBuildColor;
            houseSprite.color = Color.Lerp(houseSprite.color, target, Time.deltaTime * 5f);
        }

        //promptUI?.SetActive(true);
    }

    // ------------------------------------------------------
    // TRIGGERS
    // ------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            detectingPlayer = true;

            if (woodItem != null)
            {
                bool canBuild = inventory.GetItemCount(woodItem) >= woodAmount;
                float currentWood = inventory.GetItemCount(woodItem);

                string msg = canBuild
                    ? $"Pressione {buildKey} para construir ({woodAmount} madeira)"
                    : $"Madeira insuficiente ({currentWood:F0}/{woodAmount})";

                Debug.Log("[House] " + msg);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            detectingPlayer = false;

            if (!isBuilding && !isBuilt && houseSprite != null)
                houseSprite.color = startColor;

            //promptUI?.SetActive(false);
        }
    }

    // ------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------

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

        progressBar?.Hide();
    }

    // ------------------------------------------------------
    // DEBUG
    // ------------------------------------------------------

    [ContextMenu("Force Build")]
    private void DebugForceBuild()
    {
        if (Application.isPlaying && !isBuilt && woodItem != null)
        {
            inventory.AddItem(woodItem, (float)woodAmount);
            timeCount = timeAmount;
            TryStartBuild();
        }
    }

    [ContextMenu("Complete Build Instantly")]
    private void DebugInstant()
    {
        if (Application.isPlaying && isBuilding)
            timeCount = timeAmount;
    }

    [ContextMenu("Reset House")]
    private void DebugReset()
    {
        if (Application.isPlaying)
        {
            isBuilt = false;
            isBuilding = false;

            timeCount = 0f;

            houseSprite.color = startColor;
            houseColl?.SetActive(false);

            playerAnim?.OnHammeringEnded();
            player.isPaused = false;

            Debug.Log("[House] Resetada!");
        }
    }
}

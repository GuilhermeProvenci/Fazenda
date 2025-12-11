using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de pesca com minigame sincronizado com animação
/// </summary>
public class Casting : MonoBehaviour
{
    [Header("Fishing Settings")]
    [Range(0, 100)]
    [SerializeField] private int baseFishChance = 50;
    [SerializeField] private KeyCode castKey = KeyCode.Q;
    [SerializeField] private KeyCode catchKey = KeyCode.Q;

    [Header("Timing (sincronizado com animação)")]
    [SerializeField] private float castAnimationDuration = 1f;    // Tempo da animação de lançar
    [SerializeField] private float waitMinTime = 2f;              // Mín tempo até peixe morder
    [SerializeField] private float waitMaxTime = 5f;              // Máx tempo até peixe morder
    [SerializeField] private float catchWindowDuration = 2f;      // Tempo pra apertar Q
    [SerializeField] private float pullAnimationDuration = 1f;    // Tempo da animação de puxar

    [Header("Fish Settings")]
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private Vector2 spawnOffsetMin = new Vector2(-3f, 0.5f);
    [SerializeField] private Vector2 spawnOffsetMax = new Vector2(-1f, 1.5f);
    [SerializeField] private int fishValue = 1;

    [Header("Visual Feedback")]
    [SerializeField] private bool showProgressBar = true;
    [SerializeField] private Vector3 progressBarOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Color waitingColor = Color.yellow;
    [SerializeField] private Color biteColor = Color.red;
    [SerializeField] private Color successColor = Color.green;

    [Header("UI")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private GameObject bitePromptUI;  // "APERTE Q!"

    [Header("Audio")]
    [SerializeField] private AudioClip castSound;
    [SerializeField] private AudioClip biteSound;
    [SerializeField] private AudioClip catchSound;
    [SerializeField] private AudioClip missSound;

    [Header("Effects")]
    [SerializeField] private ParticleSystem waterSplash;
    [SerializeField] private GameObject bobberPrefab;

    // Estado
    private bool detectingPlayer;
    private bool isFishing;
    private bool isBiting;
    private bool canCatch;

    // Componentes
    private PlayerItens playerItens;
    private PlayerAnim playerAnim;
    private ProgressBar progressBar;
    private AudioSource audioSource;
    private GameObject currentBobber;

    // Enum para estados claros
    private enum FishingState { Idle, Casting, Waiting, Biting, Catching, Pulling, Complete }
    private FishingState currentState = FishingState.Idle;

    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && HasAnySounds())
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        playerItens = FindObjectOfType<PlayerItens>();
        if (playerItens == null)
        {
            Debug.LogError("Casting: PlayerItens não encontrado!");
            enabled = false;
            return;
        }

        playerAnim = playerItens.GetComponent<PlayerAnim>();
        if (playerAnim == null)
        {
            Debug.LogWarning("Casting: PlayerAnim não encontrado.");
        }

        CreateProgressBar();

        if (promptUI != null)
            promptUI.SetActive(false);

        if (bitePromptUI != null)
            bitePromptUI.SetActive(false);
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

        progressBar = ProgressBar.Create(playerItens.transform, progressBarOffset);
        progressBar.SetColors(waitingColor, successColor, new Color(0, 0, 0, 0.7f));
        progressBar.Hide();
    }

    private bool HasAnySounds()
    {
        return castSound != null || biteSound != null ||
               catchSound != null || missSound != null;
    }

    // ---------------------------
    // INPUT
    // ---------------------------

    private void HandleInput()
    {
        // Input para iniciar pesca
        if (!isFishing && detectingPlayer && Input.GetKeyDown(castKey))
        {
            StartFishing();
        }

        // Input para pegar o peixe (durante bite)
        if (canCatch && Input.GetKeyDown(catchKey))
        {
            CatchFish();
        }
    }

    // ---------------------------
    // SISTEMA DE PESCA
    // ---------------------------

    private void StartFishing()
    {
        if (isFishing)
            return;

        isFishing = true;
        isBiting = false;
        canCatch = false;
        currentState = FishingState.Casting;

        // Notifica animação
        if (playerAnim != null)
            playerAnim.OnCastingStarted();

        // Inicia sequência
        StartCoroutine(FishingSequence());
    }

    private IEnumerator FishingSequence()
    {
        // FASE 1: LANÇAMENTO (animação de lançar vara)
        yield return StartCoroutine(CastingPhase());

        // FASE 2: ESPERA (vara na água, aguardando peixe)
        yield return StartCoroutine(WaitingPhase());

        // FASE 3: MINIGAME (peixe mordeu, jogador precisa reagir)
        if (isBiting)
        {
            yield return StartCoroutine(BitingPhase());
        }

        // FASE 4: FINALIZAÇÃO
        EndFishing();
    }

    // ---------------------------
    // FASE 1: LANÇAMENTO
    // ---------------------------
    private IEnumerator CastingPhase()
    {
        currentState = FishingState.Casting;
        Debug.Log("[Casting] FASE 1: Lançando vara...");

        PlaySound(castSound);

        // Spawna bobber
        if (bobberPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(-2f, 0, 0);
            currentBobber = Instantiate(bobberPrefab, spawnPos, Quaternion.identity);
        }

        // Aguarda tempo da animação de lançar
        yield return new WaitForSeconds(castAnimationDuration);

        Debug.Log("[Casting] FASE 1 completa: Vara lançada!");
    }

    // ---------------------------
    // FASE 2: ESPERANDO PEIXE
    // ---------------------------
    private IEnumerator WaitingPhase()
    {
        currentState = FishingState.Waiting;
        Debug.Log("[Casting] FASE 2: Aguardando peixe morder...");

        // Mostra barra de espera
        if (progressBar != null)
        {
            progressBar.Show();
            progressBar.SetColors(waitingColor, waitingColor, new Color(0, 0, 0, 0.7f));
            progressBar.UpdateProgress(0.5f); // Meia barra = esperando
        }

        // Tempo aleatório até peixe morder
        float waitTime = Random.Range(waitMinTime, waitMaxTime);
        yield return new WaitForSeconds(waitTime);

        // Rola chance de peixe morder
        int roll = Random.Range(0, 100);
        if (roll < baseFishChance)
        {
            isBiting = true;
            Debug.Log("[Casting] FASE 2 completa: Peixe mordeu!");
        }
        else
        {
            isBiting = false;
            Debug.Log("[Casting] FASE 2 completa: Nenhum peixe mordeu.");
        }
    }

    // ---------------------------
    // FASE 3: MINIGAME (MORDIDA)
    // ---------------------------
    private IEnumerator BitingPhase()
    {
        currentState = FishingState.Biting;
        canCatch = true;

        Debug.Log($"[Casting] FASE 3: PEIXE MORDEU! Você tem {catchWindowDuration}s para apertar {catchKey}!");

        // Feedback visual/sonoro
        OnFishBite();

        // Barra vermelha de urgência
        if (progressBar != null)
        {
            progressBar.SetColors(biteColor, successColor, new Color(0, 0, 0, 0.7f));
        }

        // Mostra UI "APERTE Q!"
        if (bitePromptUI != null)
            bitePromptUI.SetActive(true);

        // Countdown visual
        float elapsed = 0f;
        bool caught = false;

        while (elapsed < catchWindowDuration && !caught)
        {
            elapsed += Time.deltaTime;
            float progress = 1f - (elapsed / catchWindowDuration);

            if (progressBar != null)
                progressBar.UpdateProgress(progress);

            // Verifica se pegou (o input é tratado em HandleInput)
            if (currentState == FishingState.Catching)
            {
                caught = true;
            }

            yield return null;
        }

        canCatch = false;

        // Esconde prompt
        if (bitePromptUI != null)
            bitePromptUI.SetActive(false);

        // Se não pegou a tempo
        if (!caught)
        {
            MissFish();
        }

        Debug.Log($"[Casting] FASE 3 completa: {(caught ? "PEGOU!" : "PERDEU!")}");
    }

    // ---------------------------
    // EVENTOS DE PESCA
    // ---------------------------

    private void OnFishBite()
    {
        PlaySound(biteSound);

        // Efeito visual
        if (waterSplash != null)
            waterSplash.Play();

        // Bobber pula
        if (currentBobber != null)
        {
            StartCoroutine(BobberJump());
        }
    }

    private void CatchFish()
    {
        if (!canCatch || currentState != FishingState.Biting)
            return;

        currentState = FishingState.Catching;

        Debug.Log("[Casting] ✓ PEIXE FISGADO!");

        PlaySound(catchSound);

        // Barra verde de sucesso
        if (progressBar != null)
        {
            progressBar.UpdateProgress(1f);
            progressBar.SetColors(successColor, successColor, new Color(0, 0, 0, 0.7f));
        }

        // Spawna o peixe
        SpawnFish();

        // Aguarda animação de puxar
        StartCoroutine(PullingPhase());
    }

    private void MissFish()
    {
        Debug.Log("[Casting] ✗ Peixe escapou!");
        PlaySound(missSound);
    }

    private IEnumerator PullingPhase()
    {
        currentState = FishingState.Pulling;

        // Aguarda tempo da animação de puxar vara
        yield return new WaitForSeconds(pullAnimationDuration);

        currentState = FishingState.Complete;
    }

    private void SpawnFish()
    {
        if (fishPrefab == null || playerItens == null)
            return;

        Vector3 offset = new Vector3(
            Random.Range(spawnOffsetMin.x, spawnOffsetMax.x),
            Random.Range(spawnOffsetMin.y, spawnOffsetMax.y),
            0f
        );

        GameObject fish = Instantiate(fishPrefab, playerItens.transform.position + offset, Quaternion.identity);

        Fish fishComponent = fish.GetComponent<Fish>();
        if (fishComponent != null)
        {
            fishComponent.SetValue(fishValue);
        }
    }

    private void EndFishing()
    {
        Debug.Log("[Casting] === PESCA FINALIZADA ===");

        isFishing = false;
        isBiting = false;
        canCatch = false;
        currentState = FishingState.Idle;

        // Destrói bobber
        if (currentBobber != null)
        {
            Destroy(currentBobber);
            currentBobber = null;
        }

        // Esconde barra
        if (progressBar != null)
        {
            StartCoroutine(HideProgressBarDelayed(0.5f));
        }

        // Notifica animação
        if (playerAnim != null)
            playerAnim.OnCastingEnded();
    }

    // ---------------------------
    // VISUAL FEEDBACK
    // ---------------------------

    private void UpdateVisualState()
    {
        if (promptUI != null)
        {
            bool shouldShow = detectingPlayer && !isFishing;
            promptUI.SetActive(shouldShow);
        }
    }

    private IEnumerator BobberJump()
    {
        if (currentBobber == null)
            yield break;

        Vector3 startPos = currentBobber.transform.position;
        Vector3 jumpPos = startPos + Vector3.up * 0.5f;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            currentBobber.transform.position = Vector3.Lerp(startPos, jumpPos, Mathf.Sin(t * Mathf.PI));

            yield return null;
        }

        currentBobber.transform.position = startPos;
    }

    // ---------------------------
    // TRIGGERS
    // ---------------------------

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            detectingPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            detectingPlayer = false;
        }
    }

    // ---------------------------
    // HELPERS
    // ---------------------------

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator HideProgressBarDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (progressBar != null)
            progressBar.Hide();
    }

    // Método legado
    public void OnCasting()
    {
        // Mantido para compatibilidade
    }

    // ---------------------------
    // DEBUG
    // ---------------------------

    [ContextMenu("Force Success")]
    private void DebugForceSuccess()
    {
        if (Application.isPlaying && currentState == FishingState.Biting)
        {
            CatchFish();
        }
    }

    [ContextMenu("Test Fishing")]
    private void DebugTestFishing()
    {
        if (Application.isPlaying && !isFishing)
        {
            detectingPlayer = true;
            StartFishing();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualiza área de spawn dos peixes
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + new Vector3(
            (spawnOffsetMin.x + spawnOffsetMax.x) / 2f,
            (spawnOffsetMin.y + spawnOffsetMax.y) / 2f,
            0f
        );
        Vector3 size = new Vector3(
            spawnOffsetMax.x - spawnOffsetMin.x,
            spawnOffsetMax.y - spawnOffsetMin.y,
            0.1f
        );
        Gizmos.DrawWireCube(center, size);
    }
}
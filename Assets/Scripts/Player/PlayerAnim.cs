using UnityEngine;

/// <summary>
/// Gerencia todas as animações do jogador de forma otimizada
/// </summary>
public class PlayerAnim : MonoBehaviour
{
    [Header("References")]
    private Player player;
    private Animator anim;
    private Casting casting;

    [Header("Animation Settings")]
    [SerializeField] private float flipThreshold = 0.01f;

    // Cache de IDs dos parâmetros do Animator (mais eficiente)
    private int transitionHash;
    private int isRollHash;
    private int isCastingHash;
    private int hammeringHash;

    // Estado anterior para evitar chamadas desnecessárias
    private int lastTransition = -1;
    private Vector2 lastDirection;

    private void Awake()
    {
        player = GetComponent<Player>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            Debug.LogError("PlayerAnim: Componente Player não encontrado!");
            enabled = false;
            return;
        }

        if (anim == null)
        {
            Debug.LogError("PlayerAnim: Componente Animator não encontrado!");
            enabled = false;
            return;
        }

        // Cache dos hashes (muito mais eficiente que strings)
        transitionHash = Animator.StringToHash("Transition");
        isRollHash = Animator.StringToHash("isRoll");
        isCastingHash = Animator.StringToHash("isCasting");
        hammeringHash = Animator.StringToHash("Hammering");
    }

    private void Start()
    {
        casting = FindObjectOfType<Casting>();

        if (casting == null)
            Debug.LogWarning("PlayerAnim: Casting não encontrado na cena.");
    }

    private void Update()
    {
        UpdateAnimations();
    }

    /// <summary>
    /// Atualiza todas as animações baseadas no estado do jogador
    /// </summary>
    private void UpdateAnimations()
    {
        // IMPORTANTE: Flip sempre acontece, independente da animação
        UpdateCharacterFlip();

        // Prioridade: Ações > Corrida > Movimento > Idle
        if (TryPlayActionAnimation())
            return;

        if (TryPlayRollAnimation())
            return;

        if (TryPlayRunAnimation())
            return;

        PlayMovementAnimation();
    }

    // ---------------------------------------
    // MOVIMENTO E FLIP
    // ---------------------------------------

    /// <summary>
    /// Controla animação de movimento básico
    /// </summary>
    private void PlayMovementAnimation()
    {
        bool isMoving = player.Direction.sqrMagnitude > flipThreshold * flipThreshold;
        int transition = isMoving ? 1 : 0;

        SetTransition(transition);
    }

    /// <summary>
    /// Controla o flip horizontal do personagem
    /// </summary>
    private void UpdateCharacterFlip()
    {
        // Ignora se movimento muito pequeno
        if (Mathf.Abs(player.Direction.x) < flipThreshold)
            return;

        // Flip baseado na direção ATUAL (não na anterior)
        if (player.Direction.x > 0)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (player.Direction.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }

        lastDirection = player.Direction;
    }

    // ---------------------------------------
    // CORRIDA E ROLL
    // ---------------------------------------

    /// <summary>
    /// Tenta tocar animação de corrida
    /// </summary>
    private bool TryPlayRunAnimation()
    {
        if (!player.isRunning)
            return false;

        SetTransition(2);
        return true;
    }

    /// <summary>
    /// Tenta tocar animação de rolamento
    /// </summary>
    private bool TryPlayRollAnimation()
    {
        if (!player.IsRolling)
            return false;

        anim.SetTrigger(isRollHash);
        return true;
    }

    // ---------------------------------------
    // AÇÕES (cortar, cavar, regar)
    // ---------------------------------------

    /// <summary>
    /// Tenta tocar animações de ação
    /// </summary>
    private bool TryPlayActionAnimation()
    {
        if (player.IsCutting)
        {
            SetTransition(3);
            return true;
        }

        if (player.IsDigging)
        {
            SetTransition(4);
            return true;
        }

        if (player.IsWatering)
        {
            SetTransition(5);
            return true;
        }

        return false;
    }

    // ---------------------------------------
    // AÇÕES EXTERNAS (pesca, construção)
    // ---------------------------------------

    /// <summary>
    /// Inicia animação de pesca
    /// </summary>
    public void OnCastingStarted()
    {
        if (player == null)
            return;

        player.isPaused = true;
        anim.SetTrigger(isCastingHash);
    }

    /// <summary>
    /// Finaliza animação de pesca
    /// </summary>
    public void OnCastingEnded()
    {
        if (player == null)
            return;

        player.isPaused = false;

        if (casting != null)
            casting.OnCasting();
    }

    /// <summary>
    /// Inicia animação de martelada
    /// </summary>
    public void OnHammeringStarted()
    {
        anim.SetBool(hammeringHash, true);
    }

    /// <summary>
    /// Finaliza animação de martelada
    /// </summary>
    public void OnHammeringEnded()
    {
        anim.SetBool(hammeringHash, false);
    }

    // ---------------------------------------
    // HELPERS
    // ---------------------------------------

    /// <summary>
    /// Define transição apenas se mudou (otimização)
    /// </summary>
    private void SetTransition(int value)
    {
        if (lastTransition == value)
            return;

        anim.SetInteger(transitionHash, value);
        lastTransition = value;
    }

    /// <summary>
    /// Reseta todas as animações
    /// </summary>
    public void ResetAnimations()
    {
        SetTransition(0);
        anim.SetBool(hammeringHash, false);
    }
}
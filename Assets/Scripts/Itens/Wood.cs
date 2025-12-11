using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Madeira coletável com movimento inicial e coleta automática ou manual
/// </summary>
public class Wood : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float timeMove = 1f;
    [SerializeField] private Vector2 moveDirection = Vector2.right;

    [Header("Collection")]
    [SerializeField] private int woodValue = 1;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private bool autoCollect = true;
    [SerializeField] private KeyCode collectKey = KeyCode.E;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(0.6f, 0.4f, 0.2f);

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;

    // Estado
    private float timeCount;
    private bool isMoving = true;
    private bool playerNearby;

    // Componentes
    private InventoryController inventory;
    private AudioSource audioSource;

    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && collectSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        inventory = FindObjectOfType<InventoryController>();

        if (inventory == null)
            Debug.LogWarning("Wood: InventoryController não encontrado!");

        Destroy(gameObject, lifetime); // autodestruição
    }

    private void Update()
    {
        HandleMovement();

        if (!autoCollect)
            CheckManualCollection();

        UpdateVisual();
    }

    // ---------------------------
    // MOVIMENTO
    // ---------------------------

    private void HandleMovement()
    {
        if (!isMoving)
            return;

        timeCount += Time.deltaTime;

        if (timeCount < timeMove)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            isMoving = false;
        }
    }

    // ---------------------------
    // COLETA
    // ---------------------------

    private void CheckManualCollection()
    {
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(collectKey))
            CollectWood();
    }

    private void CollectWood()
    {
        if (inventory == null)
            return;

        if (inventory.Add(ItemType.Wood, woodValue))
        {
            PlayCollectSound();

            Debug.Log($"[Wood] Coletado! +{woodValue} madeira");

            if (spriteRenderer != null)
                spriteRenderer.color = Color.green;

            Destroy(gameObject, 0.1f);
        }
        else
        {
            Debug.Log("[Wood] Inventário cheio!");
        }
    }

    // ---------------------------
    // VISUAL FEEDBACK
    // ---------------------------

    private void UpdateVisual()
    {
        if (autoCollect || spriteRenderer == null)
            return;

        spriteRenderer.color = playerNearby ? highlightColor : normalColor;
    }

    // ---------------------------
    // TRIGGERS
    // ---------------------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (autoCollect)
        {
            CollectWood();
        }
        else
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerNearby = false;
    }

    // ---------------------------
    // HELPERS
    // ---------------------------

    private void PlayCollectSound()
    {
        if (audioSource != null && collectSound != null)
            audioSource.PlayOneShot(collectSound);
    }

    public void SetValue(int value)
    {
        woodValue = value;
    }

    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    // ---------------------------
    // DEBUG
    // ---------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.4f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, moveDirection * 2f);
    }

    [ContextMenu("Collect Now")]
    private void DebugCollect()
    {
        if (Application.isPlaying)
            CollectWood();
    }

    [ContextMenu("Stop Movement")]
    private void DebugStopMovement()
    {
        isMoving = false;
    }
}

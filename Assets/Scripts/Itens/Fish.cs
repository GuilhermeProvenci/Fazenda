using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Peixe coletável com animação de flutuação e coleta manual
/// </summary>
public class Fish : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int fishValue = 1;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float collectRadius = 1.5f;
    [SerializeField] private KeyCode collectKey = KeyCode.E;

    [Header("Movement")]
    [SerializeField] private bool floatAnimation = true;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatHeight = 0.3f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;

    // Estado
    private bool playerNearby;
    private Vector3 startPosition;
    private float spawnTime;

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
        {
            Debug.LogError("Fish: InventoryController não encontrado!");
            Destroy(gameObject);
            return;
        }

        startPosition = transform.position;
        spawnTime = Time.time;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (floatAnimation)
            AnimateFloat();

        CheckPlayerInput();
        UpdateVisual();
    }

    // ---------------------------
    // ANIMAÇÃO
    // ---------------------------

    private void AnimateFloat()
    {
        float offset = Mathf.Sin((Time.time - spawnTime) * floatSpeed) * floatHeight;
        transform.position = startPosition + Vector3.up * offset;
    }

    // ---------------------------
    // COLETA
    // ---------------------------

    private void CheckPlayerInput()
    {
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(collectKey))
            CollectFish();
    }

    private void CollectFish()
    {
        if (inventory.Add(ItemType.Fish, fishValue))
        {
            PlayCollectSound();
            Debug.Log($"[Fish] Coletado! +{fishValue} peixe(s)");

            if (spriteRenderer != null)
                spriteRenderer.color = Color.green;

            Destroy(gameObject, 0.1f);
        }
        else
        {
            Debug.Log("[Fish] Inventário de peixes cheio!");
        }
    }

    // ---------------------------
    // VISUAL FEEDBACK
    // ---------------------------

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = playerNearby ? highlightColor : normalColor;
    }

    // ---------------------------
    // TRIGGERS
    // ---------------------------

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
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
        fishValue = value;
    }

    // ---------------------------
    // DEBUG
    // ---------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }

    [ContextMenu("Collect Now")]
    private void DebugCollect()
    {
        if (Application.isPlaying)
            CollectFish();
    }
}

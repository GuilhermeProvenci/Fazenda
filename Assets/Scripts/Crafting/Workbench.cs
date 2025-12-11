using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bancada de trabalho que permite crafting avançado
/// </summary>
public class Workbench : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRadius = 2f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = Color.cyan;

    [Header("UI")]
    [SerializeField] private GameObject promptUI;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;

    // Estado
    private bool playerNearby = false;
    private CraftingManager craftingManager;
    private AudioSource audioSource;

    // ---------------------------
    // UNITY LIFECYCLE
    // ---------------------------

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && openSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        craftingManager = FindObjectOfType<CraftingManager>();

        if (craftingManager == null)
        {
            Debug.LogWarning("Workbench: CraftingManager não encontrado!");
        }

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        HandleInput();
        UpdateVisual();
    }

    // ---------------------------
    // INPUT
    // ---------------------------

    private void HandleInput()
    {
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            OpenCraftingMenu();
        }
    }

    private void OpenCraftingMenu()
    {
        if (craftingManager != null)
        {
            craftingManager.ToggleCraftingMenu();

            if (audioSource != null && openSound != null)
                audioSource.PlayOneShot(openSound);

            Debug.Log("[Workbench] Menu de crafting aberto");
        }
    }

    // ---------------------------
    // VISUAL FEEDBACK
    // ---------------------------

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = playerNearby ? activeColor : normalColor;

        if (promptUI != null)
        {
            promptUI.SetActive(playerNearby);
        }
    }

    // ---------------------------
    // TRIGGERS
    // ---------------------------

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerNearby = true;

            if (craftingManager != null)
            {
                craftingManager.SetNearWorkbench(true);
            }

            Debug.Log("[Workbench] Jogador próximo - Pressione E para craftar");
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerNearby = false;

            if (craftingManager != null)
            {
                craftingManager.SetNearWorkbench(false);
            }

            Debug.Log("[Workbench] Jogador saiu");
        }
    }

    // ---------------------------
    // DEBUG
    // ---------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
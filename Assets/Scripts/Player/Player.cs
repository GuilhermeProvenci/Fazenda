using UnityEngine;

public class Player : MonoBehaviour
{
    // ------------------------------
    // ENUMS
    // ------------------------------
    public enum ToolType { Axe, Shovel, WateringCan }
    public enum PlayerAction { None, Cutting, Digging, Watering, Rolling }

    // ------------------------------
    // INSPECTOR
    // ------------------------------
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;

    [Header("Tools")]
    public ToolType equippedTool;

    [Header("Watering")]
    [SerializeField] private float waterConsumptionRate = 0.5f;

    [Header("Inventory")]
    private ItemData waterItem;

    // ------------------------------
    // COMPONENTS
    // ------------------------------
    private HudController hud;
    private Rigidbody2D rig;
    private InventorySystem inventory;

    // ------------------------------
    // ESTADOS
    // ------------------------------
    public bool isPaused;
    public bool isRunning;
    public PlayerAction currentAction = PlayerAction.None;

    private Vector2 direction;
    private float currentSpeed;

    // ------------------------------
    // UNITY
    // ------------------------------
    private void Awake()
    {
        hud = FindObjectOfType<HudController>();

        if (hud == null)
            Debug.LogWarning("Player: HudController não encontrado na cena!");
    }

    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        inventory = FindObjectOfType<InventorySystem>();

        if (rig == null)
            Debug.LogError("Player: Rigidbody2D não encontrado!");

        if (inventory == null)
            Debug.LogError("Player: InventorySystem não encontrado!");

        // Inicializa itens automaticamente via Registry
        waterItem = ItemRegistry.GetItem("Water");
        if (waterItem == null)
            Debug.LogWarning("Player: Item 'Water' não encontrado no Registro!");

        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (!isPaused)
        {
            ReadToolInput();
            ReadDirectionalInput();
            ReadRunInput();
            ReadActionInput();
        }
    }

    private void FixedUpdate()
    {
        if (!isPaused)
            Move();
    }

    // ------------------------------
    // INPUT
    // ------------------------------
    void ReadDirectionalInput()
    {
        direction.x = Input.GetAxisRaw("Horizontal");
        direction.y = Input.GetAxisRaw("Vertical");
    }

    void ReadRunInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            isRunning = true;

        if (Input.GetKeyUp(KeyCode.LeftShift))
            isRunning = false;
    }

    void ReadToolInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            equippedTool = ToolType.Axe;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            equippedTool = ToolType.Shovel;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            equippedTool = ToolType.WateringCan;
        }
    }

    void ReadActionInput()
    {
        if (Input.GetMouseButtonDown(0))
            TryStartAction();

        if (Input.GetMouseButtonUp(0))
            StopAction();
    }

    // ------------------------------
    // ACTION LOGIC
    // ------------------------------
    void TryStartAction()
    {
        switch (equippedTool)
        {
            case ToolType.Axe:
                StartCutting();
                break;
 
            case ToolType.Shovel:
                StartDigging();
                break;

            case ToolType.WateringCan:
                StartWatering();
                break;
        }
    }

    void StopAction()
    {
        if (currentAction != PlayerAction.None)
        {
            currentAction = PlayerAction.None;
            currentSpeed = walkSpeed;
        }
    }

    void StartCutting()
    {
        currentAction = PlayerAction.Cutting;
        currentSpeed = 0;
    }

    void StartDigging()
    {
        currentAction = PlayerAction.Digging;
        currentSpeed = 0;
    }

    void StartWatering()
    {
        if (waterItem == null)
        {
            Debug.LogWarning("Player: waterItem não atribuído no Inspector!");
            return;
        }

        if (inventory.GetItemCount(waterItem) > 0)
        {
            currentAction = PlayerAction.Watering;
            currentSpeed = 0;
        }
        else
        {
            Debug.Log("Sem água no inventário!");
        }
    }

    // ------------------------------
    // MOVIMENTO
    // ------------------------------
    void Move()
    {
        float finalSpeed = CalculateFinalSpeed();

        if (rig != null)
        {
            rig.MovePosition(rig.position + direction * finalSpeed * Time.fixedDeltaTime);
        }

        // Agora usando InventorySystem e ItemData
        if (currentAction == PlayerAction.Watering && waterItem != null)
        {
            float waterToConsume = waterConsumptionRate * Time.fixedDeltaTime;

            if (!inventory.RemoveItem(waterItem, waterToConsume))
            {
                StopAction();
                Debug.Log("Água acabou!");
            }
        }
    }

    float CalculateFinalSpeed()
    {
        if (currentAction != PlayerAction.None)
            return 0;

        return isRunning ? runSpeed : walkSpeed;
    }

    // ------------------------------
    // GETTERS PARA ANIMAÇÃO
    // ------------------------------
    public Vector2 Direction => direction;

    public bool IsCutting => currentAction == PlayerAction.Cutting;
    public bool IsDigging => currentAction == PlayerAction.Digging;
    public bool IsWatering => currentAction == PlayerAction.Watering;
    public bool IsRolling => currentAction == PlayerAction.Rolling;

    // ------------------------------
    // CONTROLES
    // ------------------------------
    public void Pause()
    {
        isPaused = true;
        StopAction();
    }

    public void Unpause()
    {
        isPaused = false;
    }

    public void ForceStopAction()
    {
        StopAction();
    }
}

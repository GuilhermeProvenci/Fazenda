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

    // ------------------------------
    // COMPONENTS
    // ------------------------------
    private HudControler hud;
    private Rigidbody2D rig;
    private InventoryController inventory;

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
        hud = FindObjectOfType<HudControler>();

        if (hud == null)
            Debug.LogWarning("Player: HudControler não encontrado na cena!");
    }

    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        inventory = FindObjectOfType<InventoryController>();

        if (rig == null)
            Debug.LogError("Player: Rigidbody2D não encontrado!");

        if (inventory == null)
            Debug.LogError("Player: InventoryController não encontrado!");

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
        if (inventory.Get(ItemType.Water) > 0)
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

        // Agora usando InventoryController
        if (currentAction == PlayerAction.Watering)
        {
            float waterToConsume = waterConsumptionRate * Time.fixedDeltaTime;

            if (!inventory.Remove(ItemType.Water, waterToConsume))
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

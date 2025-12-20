using UnityEngine;

/// <summary>
/// Item consumível (comida, poções, etc.)
/// </summary>
[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Items/Consumable Item")]
public class ConsumableItem : ItemData
{
    [Header("Consumable Properties")]
    [Tooltip("Restaura vida")]
    [Min(0)]
    public float healthRestore = 0f;
    
    [Tooltip("Restaura stamina/energia")]
    [Min(0)]
    public float staminaRestore = 0f;
    
    [Tooltip("Restaura fome")]
    [Min(0)]
    public float hungerRestore = 0f;
    
    [Tooltip("Efeitos de buff (opcional)")]
    public string buffEffect = "";
    
    [Tooltip("Duração do buff em segundos")]
    [Min(0)]
    public float buffDuration = 0f;
    
    [Header("Audio")]
    [Tooltip("Som ao consumir")]
    public AudioClip consumeSound;
    
    public override bool Use(GameObject user)
    {
        Debug.Log($"[ConsumableItem] {user.name} consumed {itemName}");
        
        // TODO: Aplicar efeitos ao jogador
        // Exemplo:
        // var playerHealth = user.GetComponent<PlayerHealth>();
        // if (playerHealth != null)
        //     playerHealth.Heal(healthRestore);
        
        // Toca som
        if (consumeSound != null)
        {
            AudioSource.PlayClipAtPoint(consumeSound, user.transform.position);
        }
        
        return true; // Item foi consumido
    }
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        tooltip += $"\n\n<color=yellow>Consumable</color>\n";
        
        if (healthRestore > 0)
            tooltip += $"❤ Health: +{healthRestore}\n";
        
        if (staminaRestore > 0)
            tooltip += $"⚡ Stamina: +{staminaRestore}\n";
        
        if (hungerRestore > 0)
            tooltip += $"🍖 Hunger: +{hungerRestore}\n";
        
        if (!string.IsNullOrEmpty(buffEffect))
            tooltip += $"✨ Buff: {buffEffect} ({buffDuration}s)";
        
        return tooltip;
    }
}

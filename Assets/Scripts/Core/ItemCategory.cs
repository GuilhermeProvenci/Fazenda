using UnityEngine;

/// <summary>
/// Categorias de itens do jogo
/// </summary>
public enum ItemCategory
{
    Resource,      // Recursos naturais (madeira, pedra, minérios)
    Consumable,    // Itens consumíveis (comida, poções)
    Tool,          // Ferramentas (machado, pá, regador)
    Seed,          // Sementes para plantar
    Placeable,     // Objetos colocáveis (cercas, baús, construções)
    Craftable,     // Itens craftados
    Quest,         // Itens de quest/missão
    Misc           // Diversos
}

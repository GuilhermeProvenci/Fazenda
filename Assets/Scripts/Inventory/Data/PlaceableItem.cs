using UnityEngine;

/// <summary>
/// Item colocável no mundo (cercas, baús, construções, etc.)
/// </summary>
[CreateAssetMenu(fileName = "New Placeable", menuName = "Inventory/Items/Placeable Item")]
public class PlaceableItem : ItemData
{
    [Header("Placement Properties")]
    [Tooltip("Prefab a ser instanciado quando colocado")]
    public GameObject placeablePrefab;
    
    [Tooltip("Pode ser colocado em qualquer lugar?")]
    public bool canPlaceAnywhere = false;
    
    [Tooltip("Requer superfície plana?")]
    public bool requiresFlatSurface = true;
    
    [Tooltip("Distância máxima de colocação")]
    [Min(0)]
    public float maxPlacementDistance = 3f;
    
    [Tooltip("Pode ser rotacionado ao colocar?")]
    public bool canRotate = true;
    
    [Tooltip("Snap to grid?")]
    public bool snapToGrid = false;
    
    [Tooltip("Tamanho do grid (se snap ativo)")]
    [Min(0.1f)]
    public float gridSize = 1f;
    
    [Header("Audio")]
    [Tooltip("Som ao colocar")]
    public AudioClip placeSound;
    
    public override bool Use(GameObject user)
    {
        Debug.Log($"[PlaceableItem] Attempting to place {itemName}");
        
        // TODO: Implementar lógica de colocação
        // 1. Raycast para encontrar posição
        // 2. Verificar se pode colocar
        // 3. Instanciar prefab
        // 4. Tocar som
        
        if (placeSound != null)
        {
            AudioSource.PlayClipAtPoint(placeSound, user.transform.position);
        }
        
        return true; // Item foi consumido ao colocar
    }
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        tooltip += $"\n\n<color=orange>Placeable</color>\n";
        tooltip += $"Range: {maxPlacementDistance}m\n";
        
        if (snapToGrid)
            tooltip += $"Grid: {gridSize}m\n";
        
        if (canRotate)
            tooltip += "Can be rotated\n";
        
        if (requiresFlatSurface)
            tooltip += "Requires flat surface";
        
        return tooltip;
    }
}

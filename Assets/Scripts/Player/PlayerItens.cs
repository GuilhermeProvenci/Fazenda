using UnityEngine;

public class PlayerItens : MonoBehaviour
{
    [SerializeField] private float currentWater;
    [SerializeField] private float waterLimit = 64f;

    public float CurrentWater => currentWater;
    public float WaterLimit => waterLimit;

    public bool AddWater(float amount)
    {
        if (amount <= 0) return false;
        currentWater = Mathf.Clamp(currentWater + amount, 0, waterLimit);
        return true;
    }

    public bool RemoveWater(float amount)
    {
        if (amount <= 0 || currentWater < amount)
            return false;

        currentWater -= amount;
        return true;
    }
}

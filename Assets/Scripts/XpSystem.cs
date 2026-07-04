using UnityEngine;

public class XpSystem : MonoBehaviour
{
    public int maxXp = 50;
    [HideInInspector] public int currentXp;
    [HideInInspector] public int levelPlayer = 0;
    public event System.Action OnXpChanged;
    public event System.Action OnLevelUp;

    public void AddXP(int xpAdded)
    {
        currentXp += xpAdded;
        if (currentXp > maxXp)
        {
            currentXp -= maxXp;
            levelPlayer++;
            OnLevelUp?.Invoke();
        }
        OnXpChanged?.Invoke();
    }
}
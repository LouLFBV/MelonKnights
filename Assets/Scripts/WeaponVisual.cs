using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    public WeaponType weaponType;
    [SerializeField] private GameObject visualForward;
    [SerializeField] private GameObject visualBack;
    [SerializeField] private GameObject visualLeft;
    [SerializeField] private GameObject visualRight;

    public void SetDirection(Vector2 direction)
    {
        visualForward.SetActive(false);
        visualBack.SetActive(false);
        visualLeft.SetActive(false);
        visualRight.SetActive(false);

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                visualRight.SetActive(true);
            else
                visualLeft.SetActive(true);
        }
        else
        {
            if (direction.y > 0)
                visualBack.SetActive(true);
            else
                visualForward.SetActive(true);
        }
    }
}
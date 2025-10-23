using UnityEngine;

public class TownHall : MonoBehaviour
{
    void OnMouseDown()
    {
        UIManager.Instance.OpenShop();
    }
}

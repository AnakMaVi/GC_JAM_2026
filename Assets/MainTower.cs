using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MainTower : MonoBehaviour
{
    private void Reset()
    {
        // MainTower must be a solid collider so incoming objects can collide.
        Collider towerCollider = GetComponent<Collider>();
        if (towerCollider != null)
        {
            towerCollider.isTrigger = false;
        }
    }
}

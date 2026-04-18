using UnityEngine;

public class movment_skills : MonoBehaviour
{
    [SerializeField] private Transform mainTower;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stopDistance = 0.05f;

    void Start()
    {
        TryFindMainTower();
    }

    void Update()
    {
        if (mainTower == null)
        {
            TryFindMainTower();
            return;
        }

        float distance = Vector3.Distance(transform.position, mainTower.position);
        if (distance <= stopDistance)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            mainTower.position,
            speed * Time.deltaTime
        );
    }

    private void TryFindMainTower()
    {
        GameObject towerObject = GameObject.Find("MainTower");
        if (towerObject != null)
        {
            mainTower = towerObject.transform;
        }
    }
}

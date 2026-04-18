using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]

public class movment_skills : MonoBehaviour
{
    [SerializeField] private Transform mainTower;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stopDistance = 0.05f;

    private Rigidbody rb;
    private bool hasCollidedWithTower;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        TryFindMainTower();
    }

    private void FixedUpdate()
    {
        if (hasCollidedWithTower)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        if (mainTower == null)
        {
            TryFindMainTower();
            return;
        }

        Vector3 currentPosition = rb.position;
        float distance = Vector3.Distance(currentPosition, mainTower.position);
        if (distance <= stopDistance)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 nextPosition = Vector3.MoveTowards(
            currentPosition,
            mainTower.position,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
    }

    private void TryFindMainTower()
    {
        GameObject towerObject = GameObject.Find("MainTower");
        if (towerObject != null)
        {
            mainTower = towerObject.transform;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == mainTower || collision.gameObject.GetComponent<MainTower>() != null)
        {
            hasCollidedWithTower = true;
            rb.linearVelocity = Vector3.zero;
        }
    }
}

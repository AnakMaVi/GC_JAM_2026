using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]

public class movment_skills : MonoBehaviour
{
    [SerializeField] private Transform mainTower;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stopDistance = 0.05f;
    [SerializeField] private float damagePercentOnHit = 0.05f;
    [SerializeField] private float damageCooldown = 1f;

    private Rigidbody rb;
    private bool hasCollidedWithTower;
    private float damageTimer;

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
            return;
        }

        MainTower tower = FindObjectOfType<MainTower>();
        if (tower != null)
        {
            mainTower = tower.transform;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (TryResolveTower(collision.collider, collision.transform, out MainTower tower))
        {
            hasCollidedWithTower = true;
            rb.linearVelocity = Vector3.zero;
            tower.TakeDamagePercent(damagePercentOnHit);
            damageTimer = damageCooldown;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!TryResolveTower(collision.collider, collision.transform, out MainTower tower))
        {
            return;
        }

        damageTimer -= Time.fixedDeltaTime;
        if (damageTimer <= 0f)
        {
            tower.TakeDamagePercent(damagePercentOnHit);
            damageTimer = damageCooldown;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TryResolveTower(other, other.transform, out MainTower tower))
        {
            hasCollidedWithTower = true;
            rb.linearVelocity = Vector3.zero;
            tower.TakeDamagePercent(damagePercentOnHit);
            damageTimer = damageCooldown;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!TryResolveTower(other, other.transform, out MainTower tower))
        {
            return;
        }

        damageTimer -= Time.fixedDeltaTime;
        if (damageTimer <= 0f)
        {
            tower.TakeDamagePercent(damagePercentOnHit);
            damageTimer = damageCooldown;
        }
    }

    private bool TryResolveTower(Collider contactCollider, Transform contactTransform, out MainTower tower)
    {
        tower = null;

        if (contactCollider != null)
        {
            tower = contactCollider.GetComponent<MainTower>();
            if (tower == null)
            {
                tower = contactCollider.GetComponentInParent<MainTower>();
            }

            if (tower == null)
            {
                tower = contactCollider.GetComponentInChildren<MainTower>();
            }
        }

        if (tower == null && contactTransform != null)
        {
            tower = contactTransform.GetComponent<MainTower>();
            if (tower == null)
            {
                tower = contactTransform.GetComponentInParent<MainTower>();
            }
        }

        if (tower == null && mainTower != null)
        {
            if (contactTransform == mainTower || contactTransform.IsChildOf(mainTower) || mainTower.IsChildOf(contactTransform))
            {
                tower = mainTower.GetComponent<MainTower>();
                if (tower == null)
                {
                    tower = mainTower.GetComponentInParent<MainTower>();
                }
            }
        }

        return tower != null;
    }
}

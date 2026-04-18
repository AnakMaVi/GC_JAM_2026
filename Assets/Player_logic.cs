using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Player_logic : MonoBehaviour
{
    private enum MovementPlane
    {
        XY,
        XZ
    }

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private MovementPlane movementPlane = MovementPlane.XY;

    [Header("Ataque automatico")]
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask attackLayers = ~0;

    private Rigidbody rb;
    private Vector3 moveInput;
    private float attackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void Update()
    {
        Vector2 keyboardMove = ReadKeyboardMovement();
        float horizontal = keyboardMove.x;
        float vertical = keyboardMove.y;
        moveInput = movementPlane == MovementPlane.XY
            ? new Vector3(horizontal, vertical, 0f).normalized
            : new Vector3(horizontal, 0f, vertical).normalized;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            AutoAttack();
        }
    }

    private Vector2 ReadKeyboardMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard.sKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed)
        {
            vertical += 1f;
        }

        return new Vector2(horizontal, vertical);
    }

    private void FixedUpdate()
    {
        Vector3 delta = moveInput * moveSpeed * Time.fixedDeltaTime;
        if (rb != null && !rb.isKinematic)
        {
            rb.MovePosition(rb.position + delta);
            return;
        }

        transform.position += delta;
    }

    private void AutoAttack()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange,
            attackLayers,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hits.Length; i++)
        {
            GameObject target = hits[i].attachedRigidbody != null
                ? hits[i].attachedRigidbody.gameObject
                : hits[i].gameObject;

            if (!target.CompareTag("enemigo"))
            {
                continue;
            }

            // The enemy can later implement TakeDamage(int damage).
            target.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

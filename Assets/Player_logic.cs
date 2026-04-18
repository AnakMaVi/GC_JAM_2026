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

    private enum AttackType
    {
        A,
        B,
        C,
        D
    }

    private const string EnemyTagA = "enemigo_a";
    private const string EnemyTagB = "enemigo_b";
    private const string EnemyTagC = "enemigo_c";
    private const string EnemyTagD = "enemigo_d";

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private MovementPlane movementPlane = MovementPlane.XY;

    [Header("Ataque automatico")]
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float focusedDamagePercent = 0.25f;
    [SerializeField] private float splashDamagePercent = 0.01f;
    [SerializeField] private LayerMask attackLayers = ~0;

    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private Renderer meshRenderer;
    private Vector3 moveInput;
    private float attackTimer;
    private AttackType currentAttackType = AttackType.A;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        meshRenderer = GetComponentInChildren<Renderer>();

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        ApplyAttackVisualColor();
    }

    private void Update()
    {
        HandleAttackTypeInput();

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

    private void HandleAttackTypeInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            SetAttackType(AttackType.A);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            SetAttackType(AttackType.B);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            SetAttackType(AttackType.C);
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            SetAttackType(AttackType.D);
        }
    }

    private void SetAttackType(AttackType nextType)
    {
        if (currentAttackType == nextType)
        {
            return;
        }

        currentAttackType = nextType;
        ApplyAttackVisualColor();
    }

    private void ApplyAttackVisualColor()
    {
        if (!CompareTag("player"))
        {
            return;
        }

        Color color = currentAttackType switch
        {
            AttackType.A => Color.red,
            AttackType.B => Color.green,
            AttackType.C => Color.blue,
            AttackType.D => Color.yellow,
            _ => Color.red
        };

        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
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

            string targetTag = target.tag;
            if (!IsEnemyTag(targetTag))
            {
                continue;
            }

            float damagePercent = IsFocusedTarget(targetTag) ? focusedDamagePercent : splashDamagePercent;
            target.SendMessage("TakeDamagePercent", damagePercent, SendMessageOptions.DontRequireReceiver);
        }
    }

    private bool IsEnemyTag(string targetTag)
    {
        return targetTag == EnemyTagA
            || targetTag == EnemyTagB
            || targetTag == EnemyTagC
            || targetTag == EnemyTagD;
    }

    private bool IsFocusedTarget(string targetTag)
    {
        return currentAttackType switch
        {
            AttackType.A => targetTag == EnemyTagA,
            AttackType.B => targetTag == EnemyTagB,
            AttackType.C => targetTag == EnemyTagC,
            AttackType.D => targetTag == EnemyTagD,
            _ => false
        };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

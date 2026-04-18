using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    private const string PlayerTag = "Player";

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private MovementPlane movementPlane = MovementPlane.XY;

    [Header("Ataque automatico")]
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float focusedDamagePercent = 0.25f;
    [SerializeField] private float splashDamagePercent = 0.01f;
    [SerializeField] private LayerMask attackLayers = ~0;

    [Header("Visual")]
    [SerializeField] private Transform playerVisualTarget;
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Sprite[] attackTypeSprites = new Sprite[4];

    private Rigidbody rb;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    

    private Renderer[] meshRenderers;
    private Image[] uiImages;
    private MaterialPropertyBlock colorBlock;
    private Transform visualRoot;
    private bool warnedNoVisual;
    private Vector3 moveInput;
    private float attackTimer;
    private AttackType currentAttackType = AttackType.A;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colorBlock = new MaterialPropertyBlock();
        ResolveVisualTargets();

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        ApplyAttackSprite();
        ApplyAttackVisualColor();
    }

    private void Update()
    {
        HandleAttackTypeInput();
        if (visualRoot == null)
        {
            ResolveVisualTargets();
        }

        ApplyAttackVisualColor();

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
        else if (keyboard.digit1Key.wasPressedThisFrame)
        {
            SetAttackType(AttackType.A);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            SetAttackType(AttackType.B);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            SetAttackType(AttackType.C);
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
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
        ApplyAttackSprite();
        ApplyAttackVisualColor();
    }

    private void ApplyAttackSprite()
    {
        int index = (int)currentAttackType;
        bool hasSprite = attackTypeSprites != null
            && index < attackTypeSprites.Length
            && attackTypeSprites[index] != null;

        // Try the manually assigned renderer first
        SpriteRenderer sr = playerRenderer as SpriteRenderer;

        // Fall back to the first auto-found SpriteRenderer
        if (sr == null && spriteRenderers != null && spriteRenderers.Length > 0)
        {
            sr = spriteRenderers[0];
        }

        if (sr != null && hasSprite)
        {
            sr.sprite = attackTypeSprites[index];
        }
    }

    private void ApplyAttackVisualColor()
    {
        if (visualRoot == null)
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

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = color;
            }
        }

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            Renderer currentRenderer = meshRenderers[i];
            if (currentRenderer == null)
            {
                continue;
            }

            currentRenderer.GetPropertyBlock(colorBlock);

            if (currentRenderer.sharedMaterial != null && currentRenderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                colorBlock.SetColor("_BaseColor", color);
            }

            if (currentRenderer.sharedMaterial != null && currentRenderer.sharedMaterial.HasProperty("_Color"))
            {
                colorBlock.SetColor("_Color", color);
            }

            currentRenderer.SetPropertyBlock(colorBlock);

            if (currentRenderer.sharedMaterial != null)
            {
                if (currentRenderer.sharedMaterial.HasProperty("_BaseColor"))
                {
                    currentRenderer.material.SetColor("_BaseColor", color);
                }

                if (currentRenderer.sharedMaterial.HasProperty("_Color"))
                {
                    currentRenderer.material.SetColor("_Color", color);
                }
            }
        }

        for (int i = 0; i < uiImages.Length; i++)
        {
            if (uiImages[i] != null)
            {
                uiImages[i].color = color;
            }
        }
    }

    private void ResolveVisualTargets()
    {
        if (playerVisualTarget != null)
        {
            visualRoot = playerVisualTarget;
        }
        else
        {
            GameObject playerByTag = null;
            try
            {
                playerByTag = GameObject.FindGameObjectWithTag(PlayerTag);
            }
            catch
            {
                playerByTag = null;
            }

            if (playerByTag != null)
            {
                visualRoot = playerByTag.transform;
            }
            else
            {
                GameObject playerByName = GameObject.Find("Player");
                visualRoot = playerByName != null ? playerByName.transform : transform;
            }
        }

        spriteRenderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        meshRenderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        uiImages = visualRoot.GetComponentsInChildren<Image>(true);

        if (!warnedNoVisual && spriteRenderers.Length == 0 && meshRenderers.Length == 0 && uiImages.Length == 0)
        {
            warnedNoVisual = true;
            Debug.LogWarning("Player_logic: No renderers found to color. Assign Player Visual Target in inspector.", this);
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
            QueryTriggerInteraction.UseGlobal
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

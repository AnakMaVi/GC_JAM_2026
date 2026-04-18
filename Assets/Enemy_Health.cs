using UnityEngine;
using UnityEngine.UI;

public class Enemy_Health : MonoBehaviour
{
    [Header("Vida base por tag (se asigna automaticamente)")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Barra de vida (se crea automaticamente si no se asigna)")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, 1.5f, 0f);

    private float currentHealth;

    private void Awake()
    {
        maxHealth = GetMaxHealthByTag(tag);
        currentHealth = maxHealth;

        if (healthBarFill == null)
        {
            BuildHealthBarCanvas();
        }

        RefreshBar();
    }

    // Called by Player_logic via SendMessage
    public void TakeDamagePercent(float percent)
    {
        currentHealth -= maxHealth * percent;
        currentHealth = Mathf.Max(currentHealth, 0f);
        RefreshBar();

        if (currentHealth <= 0f)
        {
            OnDeath();
        }
    }

    // Called by Player_logic legacy path via SendMessage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        RefreshBar();

        if (currentHealth <= 0f)
        {
            OnDeath();
        }
    }

    private float GetMaxHealthByTag(string enemyTag)
    {
        return enemyTag switch
        {
            "enemigo_a" => 80f,
            "enemigo_b" => 120f,
            "enemigo_c" => 200f,
            "enemigo_d" => 300f,
            _ => 100f
        };
    }

    private void RefreshBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    private void BuildHealthBarCanvas()
    {
        GameObject canvasGO = new GameObject("HealthBarCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = healthBarOffset;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one * 0.01f;

        Canvas worldCanvas = canvasGO.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100f, 10f);

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = Color.black;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill - color by tag
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(canvasGO.transform, false);
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = GetColorByTag(tag);
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillAmount = 1f;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
    }

    private Color GetColorByTag(string enemyTag)
    {
        return enemyTag switch
        {
            "enemigo_a" => Color.red,
            "enemigo_b" => Color.green,
            "enemigo_c" => Color.blue,
            "enemigo_d" => Color.yellow,
            _ => Color.white
        };
    }
}

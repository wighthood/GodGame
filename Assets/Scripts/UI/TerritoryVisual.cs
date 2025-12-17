using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class TerritoryVisual : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private LineRenderer borderLine;
    [SerializeField] private SpriteRenderer fillSprite;
    [SerializeField] private TextMeshProUGUI labelText; 

    [Header("Geometry Settings")]
    [SerializeField] private int segments = 50;
    [Tooltip("Augmente cette valeur (ex: 1.15) pour que le remplissage touche la bordure.")]
    [SerializeField] private float sizeMultiplier = 1.15f; 

    [Header("Opacity Settings (Background)")]
    [Range(0f, 1f)] [SerializeField] private float minFillOpacity = 0.0f;
    [Range(0f, 1f)] [SerializeField] private float maxFillOpacity = 0.4f;

    [Header("Opacity Settings (Text)")]
    [Tooltip("Si tu veux toujours voir le texte, mets ça à 1.")]
    [Range(0f, 1f)] [SerializeField] private float minTextOpacity = 1.0f; 
    [Range(0f, 1f)] [SerializeField] private float maxTextOpacity = 1.0f;

    [Header("Scale Settings (Text)")] // --- NOUVEAU ---
    [Tooltip("Échelle du texte quand on est zoomé TRES PRÈS (ex: 0.2 pour le rendre petit).")]
    [SerializeField] private float minTextScale = 0.2f; 

    [Tooltip("Échelle du texte quand on est zoomé TRES LOIN (ex: 1.0 pour taille normale).")]
    [SerializeField] private float maxTextScale = 1.0f;

    private float currentRadius;
    private Color baseColor;

    public void Initialize(float _radius, Color _color)
    {
        currentRadius = _radius;
        baseColor = _color;

        SetupBorder();
        SetupFill();
        SetupLabel();
    }

    public void UpdateRadius(float _radius)
    {
        if (Mathf.Abs(currentRadius - _radius) > 0.1f)
        {
            currentRadius = _radius;
            SetupBorder();
            SetupFill();
        }
    }

    public void UpdateLabel(string _species, int _population)
    {
        if (labelText != null)
        {
            labelText.text = $"{_species}\n<size=80%>Pop: {_population}</size>";
        }
    }

    private void SetupBorder()
    {
        if (borderLine == null) borderLine = GetComponent<LineRenderer>();

        borderLine.positionCount = segments + 1;
        borderLine.useWorldSpace = false;
        borderLine.startWidth = 0.2f;
        borderLine.endWidth = 0.2f;
        borderLine.loop = true;
        
        borderLine.startColor = baseColor;
        borderLine.endColor = baseColor;

        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * currentRadius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * currentRadius;
            borderLine.SetPosition(i, new Vector3(x, y, 0f));
            angle += (360f / segments);
        }
    }

    private void SetupFill()
    {
        if (fillSprite == null)
        {
            Transform child = transform.Find("Fill");
            if (child != null) fillSprite = child.GetComponent<SpriteRenderer>();
        }

        if (fillSprite != null)
        {
            fillSprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f); 
            float finalScale = currentRadius * 2f * sizeMultiplier;
            fillSprite.transform.localScale = Vector3.one * finalScale; 
        }
    }

    private void SetupLabel()
    {
        if (labelText != null)
        {
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
        }
    }

    public void SetFillOpacity(float _zoomFactor)
    {
        if (fillSprite != null)
        {
            float targetAlpha = Mathf.Lerp(minFillOpacity, maxFillOpacity, _zoomFactor);
            Color c = fillSprite.color;
            fillSprite.color = new Color(c.r, c.g, c.b, targetAlpha);
        }
        
        if (labelText != null)
        {
            float textAlpha = Mathf.Lerp(minTextOpacity, maxTextOpacity, _zoomFactor);
            labelText.alpha = textAlpha; 
            
            float currentScale = Mathf.Lerp(minTextScale, maxTextScale, _zoomFactor);
            labelText.transform.localScale = Vector3.one * currentScale;
        }
    }
}
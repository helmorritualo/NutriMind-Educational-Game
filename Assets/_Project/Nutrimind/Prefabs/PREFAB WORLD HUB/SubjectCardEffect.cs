using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SubjectCardEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Glow")]
    public Outline outline;
    public Color glowColor = Color.green;

    [Header("Scale")]
    public float hoverScale = 1.08f;
    public float clickScale = 0.95f;
    public float scaleSpeed = 8f;

    [Header("Breathing")]
    public float minGlow = 2f;
    public float maxGlow = 8f;
    public float breathingSpeed = 2f;

    [Header("Hover")]
    public float hoverGlow = 15f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool hovering;
    private bool pressing;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (outline != null)
            outline.effectColor = glowColor;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );

        if (outline != null)
        {
            float glowSize;

            if (hovering)
            {
                glowSize = pressing ? hoverGlow * 1.2f : hoverGlow;
            }
            else
            {
                glowSize = Mathf.Lerp(
                    minGlow,
                    maxGlow,
                    (Mathf.Sin(Time.time * breathingSpeed) + 1f) * 0.5f
                );
            }

            outline.effectDistance =
                new Vector2(glowSize, glowSize);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        pressing = false;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressing = true;
        targetScale = originalScale * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressing = false;
        targetScale = hovering ? originalScale * hoverScale : originalScale;
    }
}

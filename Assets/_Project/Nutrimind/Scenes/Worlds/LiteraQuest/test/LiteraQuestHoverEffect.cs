using UnityEngine;
using UnityEngine.EventSystems;

public class LiteraQuestHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    public float normalScale = 1f;
    public float hoverScale = 0.96f;   // Zoom out when hover
    public float clickScale = 0.90f;   // Zoom out more when clicked
    public float speed = 10f;

    private Vector3 targetScale;

    void Start()
    {
        targetScale = Vector3.one * normalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one * normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = Vector3.one * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
    }
}

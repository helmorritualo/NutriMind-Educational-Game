using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BreathingGlowImage : MonoBehaviour
{
    [Header("Glow Colors")]
    [SerializeField] private Color dimColor = new Color(1f, 1f, 1f, 0.55f);
    [SerializeField] private Color glowColor = new Color(1f, 0.9f, 0.35f, 0.95f);

    [Header("Breathing")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private bool useUnscaledTime = true;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float pulse = (Mathf.Sin(time * speed) + 1f) * 0.5f;

        image.color = Color.Lerp(dimColor, glowColor, pulse);
    }
}
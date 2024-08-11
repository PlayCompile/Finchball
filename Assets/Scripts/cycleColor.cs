using UnityEngine;
using UnityEngine.UI;

public class cycleColor : MonoBehaviour
{
    private RawImage rawImage;
    public RawImage opposite;

    // Colors cycle on this component. Opposite colors cycle if opposite is also assigned.
    public float cycleSpeed = 1f;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        // Get the current time in the cycle
        float t = Mathf.PingPong(Time.time * cycleSpeed, 1f);

        // Calculate the color based on the time
        Color color = Color.HSVToRGB(t, 1f, 1f);

        // Apply the color to the main RawImage
        rawImage.color = color;

        if (opposite != null)
        {
            // Calculate the opposite color by adding 0.5 to the hue (180-degree shift)
            Color oppositeColor = Color.HSVToRGB((t + 0.5f) % 1f, 1f, 1f);

            // Apply the opposite color to the opposite RawImage
            opposite.color = oppositeColor;
        }
    }
}
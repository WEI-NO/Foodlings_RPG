using UnityEngine;
using UnityEngine.UI;

public class UIScrollingBackground : MonoBehaviour
{
    public float scrollSpeedX = 0.1f;
    public float scrollSpeedY = 0f;

    private Material material;
    public Vector2 offset;

    void Start()
    {
        Image image = GetComponent<Image>();

        if (image == null)
        {
            this.enabled = false;
            return;
        }

        // Create a unique material instance for this Image
        material = Instantiate(image.material);
        image.material = material;
    }

    void Update()
    {
        offset.x += scrollSpeedX * Time.deltaTime;
        offset.y += scrollSpeedY * Time.deltaTime;

        material.mainTextureOffset = offset;
    }
}

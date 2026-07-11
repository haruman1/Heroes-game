using UnityEngine;

public class PlayerLandingIndicator : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask groundLayer;
    public float maxDistance = 25f;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.65f);
    public float minScale = 0.4f;
    public float maxScale = 1.4f;
    public Vector3 shadowOffset = new Vector3(0, 0.05f, 0);

    [Header("Line Settings")]
    public bool showGuideLine = true;
    public Color lineStartColor = new Color(1f, 1f, 1f, 0.3f);
    public Color lineEndColor = new Color(1f, 1f, 1f, 0.05f);
    public float lineWidth = 0.04f;

    [Header("Visual Elements")]
    public Sprite customShadowSprite;

    private GameObject shadowObj;
    private SpriteRenderer shadowRenderer;
    private GameObject lineObj;
    private LineRenderer lineRenderer;
    private playerJ playerScript;

    void Start()
    {
        playerScript = GetComponent<playerJ>();
        
        // Auto-assign ground layer from player if not set
        if (playerScript != null && groundLayer == 0)
        {
            groundLayer = playerScript.groundLayer;
        }

        // 1. Set up Shadow Object
        shadowObj = new GameObject("LandingIndicatorShadow");
        shadowObj.transform.SetParent(transform);
        shadowObj.transform.localRotation = Quaternion.identity;

        shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
        shadowRenderer.sortingOrder = 5; // Render on top of backgrounds/tiles

        if (customShadowSprite != null)
        {
            shadowRenderer.sprite = customShadowSprite;
        }
        else
        {
            shadowRenderer.sprite = CreateProceduralCircleSprite();
        }
        shadowRenderer.color = Color.clear;

        // 2. Set up Line Object (Vertical guide line)
        if (showGuideLine)
        {
            lineObj = new GameObject("LandingIndicatorLine");
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;
            lineObj.transform.localRotation = Quaternion.identity;

            lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 2;

            // Use the standard Sprites/Default shader to render colors without needing materials in assets
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                lineRenderer.material = new Material(spriteShader);
            }

            lineRenderer.startColor = Color.clear;
            lineRenderer.endColor = Color.clear;
        }
    }

    private Sprite CreateProceduralCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float center = size / 2.0f;
        float radius = size / 2.0f - 1.0f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                if (distance <= radius)
                {
                    // Cosine falloff for a soft, realistic drop shadow
                    float normalizedDist = distance / radius;
                    float alpha = Mathf.Clamp01(Mathf.Cos(normalizedDist * Mathf.PI * 0.5f));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();

        // Create Sprite from Texture
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    void LateUpdate()
    {
        if (playerScript == null || shadowObj == null) return;

        // Start raycast from the player's ground check position (feet) if available
        Vector2 origin = playerScript.groundCheck != null ? (Vector2)playerScript.groundCheck.position : (Vector2)transform.position;

        // Perform raycast downwards to find ground height
        RaycastHit2D hit = Physics2D.Raycast(origin + Vector2.up * 0.1f, Vector2.down, maxDistance, groundLayer);

        bool showVisuals = false;
        float distance = 0f;
        Vector2 hitPoint = Vector2.zero;

        if (hit.collider != null)
        {
            hitPoint = hit.point;
            distance = hit.distance;

            // Check if player is grounded using playerScript's state
            bool isGrounded = playerScript.IsGrounded;

            // Only show shadow and line if the player is in the air and not right on the ground
            if (!isGrounded && distance > 0.4f)
            {
                showVisuals = true;
            }
        }

        if (showVisuals)
        {
            // Calculate height ratio (0 near ground, 1 at max distance)
            float ratio = Mathf.Clamp01(distance / maxDistance);

            // Update Shadow position, scale, and color
            shadowObj.transform.position = (Vector3)hitPoint + shadowOffset;
            shadowObj.transform.rotation = Quaternion.identity; // Freeze shadow rotation

            // Soft shadow becomes larger and fainter the higher the player is
            float scale = Mathf.Lerp(minScale, maxScale, ratio);
            shadowObj.transform.localScale = new Vector3(scale, scale * 0.4f, 1f); // Elliptical 2D shadow shape

            float alpha = Mathf.Lerp(shadowColor.a, 0.05f, ratio);
            shadowRenderer.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, alpha);

            // Update Guide Line position and color
            if (showGuideLine && lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, origin);
                lineRenderer.SetPosition(1, hitPoint);

                // Fade line opacity out as player goes higher
                float lineAlphaStart = Mathf.Lerp(lineStartColor.a, 0.05f, ratio);
                float lineAlphaEnd = Mathf.Lerp(lineEndColor.a, 0.01f, ratio);

                lineRenderer.startColor = new Color(lineStartColor.r, lineStartColor.g, lineStartColor.b, lineAlphaStart);
                lineRenderer.endColor = new Color(lineEndColor.r, lineEndColor.g, lineEndColor.b, lineAlphaEnd);
            }
        }
        else
        {
            // Smoothly fade out shadow and disable line
            shadowRenderer.color = Color.Lerp(shadowRenderer.color, Color.clear, Time.deltaTime * 10f);
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }
    }

    void OnDestroy()
    {
        // Cleanup dynamically created GameObjects
        if (shadowObj != null)
        {
            Destroy(shadowObj);
        }
        if (lineObj != null)
        {
            Destroy(lineObj);
        }
    }
}

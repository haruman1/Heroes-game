using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float speed = 2f;
    public Transform[] points;

    private int currentPointIndex = 0;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(transform.position, points[currentPointIndex].position) < 0.25f)
        {
            currentPointIndex++;
            if (currentPointIndex == points.Length)
            {
                currentPointIndex = 0;
            }
        }
        transform.position = Vector2.MoveTowards(
            transform.position,
            points[currentPointIndex].position,
            speed * Time.deltaTime
        );
        spriteRenderer.flipX = (transform.position.x - points[currentPointIndex].position.x) < 0f;
    }
}

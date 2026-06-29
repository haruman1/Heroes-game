using UnityEngine;

public class Coins_duit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerJ player = collision.GetComponent<playerJ>();
            Debug.Log("Player collided with coin. Coin count: " + player.coinCount);
            if (player != null)
            {
                player.coinCount++;
                Destroy(gameObject);
                Debug.Log("Player collected coin. Coin count: " + player.coinCount);
            }
        }
    }
}

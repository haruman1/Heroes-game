using UnityEngine;

public class Coins_duit : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerJ player = collision.GetComponent<playerJ>();
            if (player != null)
            {
                player.AddCoin(1);
                player.PlaySFX(player.coinSound, 0.5f); // Play coin sound effect at half volume
                Destroy(gameObject);
                Debug.Log("Player collected coin. Coin count: " + player.coinCount);
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour
{
    [Header("Door Visuals")]
    public SpriteRenderer doorSpriteRenderer;
    public Sprite openDoorSprite;
    public Animator doorAnimator;
    public string openAnimationTrigger = "Open";

    [Header("Door Audio")]
    public AudioSource doorAudioSource;
    public AudioClip openSound;

    [Header("Events")]
    public UnityEvent onDoorOpened;

    [Header("Dialogue Interaction")]
    public InitialDialogueManager doorDialogue;
    private bool dialogueTriggered = false;

    private bool isOpened = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !dialogueTriggered && doorDialogue != null)
        {
            dialogueTriggered = true;
            Debug.Log("[DoorController] Pemain mendekati pintu. Memunculkan dialog!");
            doorDialogue.TriggerDialogue();
        }
    }

    public void OpenDoor()
    {
        if (isOpened) return;

        isOpened = true;
        Debug.Log("[DoorController] Membuka pintu!");

        // 1. Ganti Sprite (jika menggunakan sprite statis)
        if (doorSpriteRenderer != null && openDoorSprite != null)
        {
            doorSpriteRenderer.sprite = openDoorSprite;
        }

        // 2. Mainkan Animasi (jika menggunakan Animator)
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }

        // 3. Mainkan Suara
        if (doorAudioSource != null && openSound != null)
        {
            doorAudioSource.PlayOneShot(openSound);
        }

        // 4. Hilangkan Collider yang memblokir jalan (opsional)
        Collider2D doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        onDoorOpened?.Invoke();
    }
}

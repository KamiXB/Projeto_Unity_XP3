using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Key : MonoBehaviour
{
    [Tooltip("Tag to detect player")]
    public string playerTag = "Player";

    [Tooltip("If true, key will be destroyed on collect")]
    public bool destroyOnCollect = true;

    void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If the collider belongs to the player (by tag) OR it has a Moviment component, process collect
        if (!other.CompareTag(playerTag) && other.GetComponent<Moviment>() == null) return;

        var mov = other.GetComponent<Moviment>();
        if (mov != null)
        {
            mov.CollectKey();
            Debug.Log($"Key: player '{other.gameObject.name}' collected a key.");
        }
        else
        {
            other.SendMessage("CollectKey", SendMessageOptions.DontRequireReceiver);
            Debug.Log($"Key: sent CollectKey to '{other.gameObject.name}' via SendMessage.");
        }

        // Notify any ChaveUI in scene so the HUD updates immediately (no need to wait for Update polling)
        var chaveUi = FindObjectOfType<ChaveUI>();
        if (chaveUi != null)
        {
            chaveUi.ShowKey();
        }

        if (destroyOnCollect) Destroy(gameObject);
    }
}

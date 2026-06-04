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
        if (!other.CompareTag(playerTag)) return;

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

        if (destroyOnCollect) Destroy(gameObject);
    }
}

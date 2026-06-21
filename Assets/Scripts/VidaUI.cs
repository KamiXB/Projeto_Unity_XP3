using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class VidaUI : MonoBehaviour
{
    [Tooltip("Sprites representing 0,1,2,3 hearts (index matches current health).")]
    [SerializeField] private Sprite[] heartSprites = new Sprite[4];

    private Image img;
    [Tooltip("Assign the player's Moviment in the Inspector to avoid automatic search issues.")]
    [SerializeField] private Moviment playerMov;
    private int lastHp = -1;

    void Awake()
    {
        img = GetComponent<Image>();
        if (img == null) Debug.LogWarning("VidaUI: Image component missing.");
    }

    void Start()
    {
        // Try to locate the player's Moviment robustly.
        if (playerMov == null)
        {
            // 1) Try finding by tag (recommended to tag the player GameObject as "Player")
            var go = GameObject.FindWithTag("Player");
            if (go != null)
            {
                playerMov = go.GetComponent<Moviment>();
            }
        }

        if (playerMov == null)
        {
            // 2) Try FindObjectOfType (active objects)
            playerMov = FindObjectOfType<Moviment>();
        }

        if (playerMov == null)
        {
            // 3) Fallback: include inactive/assets (expensive). Use only if nothing else found.
            var all = Resources.FindObjectsOfTypeAll<Moviment>();
            if (all != null && all.Length > 0)
                playerMov = all[0];
        }

        if (playerMov == null)
            Debug.LogWarning("VidaUI: no Moviment found. Assign playerMov in the Inspector to avoid search issues.");

        // initialize UI to current value (force update)
        UpdateUI(true);
    }

    void Update()
    {
        if (playerMov != null)
        {
            // update only when health changed
            int hp = playerMov.CurrentHealth;
            if (hp != lastHp) UpdateUI(false);
        }
    }

    private void UpdateUI(bool force)
    {
        int hp = playerMov != null ? playerMov.CurrentHealth : 0;
        hp = Mathf.Clamp(hp, 0, heartSprites.Length - 1);

        // If not forced and HP hasn't changed, skip update
        if (!force && hp == lastHp) return;

        if (img != null && heartSprites != null && heartSprites.Length > hp && heartSprites[hp] != null)
        {
            img.sprite = heartSprites[hp];
            lastHp = hp;
        }
    }
}

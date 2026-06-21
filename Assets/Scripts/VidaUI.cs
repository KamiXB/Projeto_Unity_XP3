using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class VidaUI : MonoBehaviour
{
    [Tooltip("Sprites representing 0,1,2,3 hearts (index matches current health).")]
    [SerializeField] private Sprite[] heartSprites = new Sprite[4];

    private Image img;
    private Moviment playerMov;

    void Awake()
    {
        img = GetComponent<Image>();
        if (img == null) Debug.LogWarning("VidaUI: Image component missing.");
    }

    void Start()
    {
        // try auto-find player Moviment
        var mov = FindObjectOfType<Moviment>();
        if (mov != null) playerMov = mov;
        else Debug.LogWarning("VidaUI: no Moviment found in scene. Assign playerMov if necessary.");

        UpdateUI();
    }

    void Update()
    {
        if (playerMov != null)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        int hp = playerMov != null ? playerMov.CurrentHealth : 0;
        hp = Mathf.Clamp(hp, 0, heartSprites.Length - 1);
        if (img != null && heartSprites != null && heartSprites.Length > hp && heartSprites[hp] != null)
        {
            img.sprite = heartSprites[hp];
        }
    }
}

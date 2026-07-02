using UnityEngine;
using UnityEngine.UI;

// Simple UI helper: mostra uma imagem quando o jogador coleta a chave
public class ChaveUI : MonoBehaviour
{
    [Tooltip("Image que representa a chave na HUD. Se vazio, tentará obter Image no mesmo GameObject.")]
    [SerializeField] private Image chaveImage;

    [Tooltip("Referência opcional ao Moviment do jogador. Se vazio, será encontrada em cena.")]
    [SerializeField] private Moviment playerMov;

    private bool shown = false;

    void Start()
    {
        if (chaveImage == null)
            chaveImage = GetComponent<Image>();

        // If not assigned on the inspector or as a sibling Image, try to locate a reasonable candidate in scene
        if (chaveImage == null)
        {
            // prefer by name containing 'chave' or 'key'
            var all = Resources.FindObjectsOfTypeAll<Image>();
            foreach (var img in all)
            {
                if (img == null) continue;
                var n = img.gameObject.name.ToLowerInvariant();
                if (n.Contains("chave") || n.Contains("key") || n.Contains("keyimage") || n.Contains("chaveimage"))
                {
                    chaveImage = img;
                    break;
                }
            }

            // fallback: first Image under an active Canvas in scene
            if (chaveImage == null)
            {
                foreach (var img in all)
                {
                    if (img == null) continue;
                    if (img.canvas != null)
                    {
                        chaveImage = img;
                        break;
                    }
                }
            }
        }

        if (chaveImage != null)
        {
            // ensure hidden initially
            chaveImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ChaveUI: no Image assigned or found in scene. Assign in Inspector or name the UI Image 'Chave'/'Key'.");
        }

        if (playerMov == null)
            playerMov = FindObjectOfType<Moviment>();
    }

    void Update()
    {
        if (shown) return;
        if (playerMov == null)
        {
            playerMov = FindObjectOfType<Moviment>();
            if (playerMov == null) return;
        }

        if (playerMov.HasKey)
        {
            ShowKey();
            shown = true;
        }
    }

    public void ShowKey()
    {
        if (chaveImage == null)
        {
            Debug.LogWarning("ChaveUI.ShowKey called but chaveImage is null.");
            return;
        }

        // Ensure parent chain (Canvas / parents) is active so the Image can be visible.
        ActivateParentChain(chaveImage.gameObject);

        chaveImage.gameObject.SetActive(true);
    }

    // Ensure all parents of the target GameObject are active. Useful when UI elements are nested under
    // disabled containers or Canvas objects so SetActive on the leaf has no visual effect.
    private void ActivateParentChain(GameObject go)
    {
        if (go == null) return;
        Transform t = go.transform.parent;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);
            }
            t = t.parent;
        }
    }

    public void HideKey()
    {
        if (chaveImage != null)
        {
            chaveImage.gameObject.SetActive(false);
            shown = false;
        }
    }
}

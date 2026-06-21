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

        if (chaveImage != null)
            chaveImage.gameObject.SetActive(false);

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
        if (chaveImage != null)
            chaveImage.gameObject.SetActive(true);
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

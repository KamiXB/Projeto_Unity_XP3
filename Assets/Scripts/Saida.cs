using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Saida : MonoBehaviour
{
    [Header("Detecção")]
    [Tooltip("Tag do jogador que ativa a saída.")]
    public string playerTag = "Player";

    [Header("Ação")]
    [Tooltip("Se true, carrega outra cena após a ativação.")]
    public bool loadScene = false;
    [Tooltip("Nome da cena a carregar (deve estar em Build Settings).")]
    public string sceneName = "";
    [Tooltip("Atraso antes de carregar a cena (segundos).")]
    public float sceneDelay = 1f;

    [Header("Mostrar texto")]
    [Tooltip("Se true, mostra um texto na tela ao ativar.")]
    public bool showText = true;
    [Tooltip("Componente TextMeshPro (UI) que exibirá a mensagem. Opcional: se não atribuído, a mensagem vai para o console.")]
    public TMP_Text uiText;
    [Tooltip("Texto a ser exibido.")]
    public string textToShow = "Você venceu!";
    [Tooltip("Duração que o texto ficará visível (segundos).")]
    public float textDuration = 3f;

    bool activated = false;

    void Start()
    {
        // garante que o trigger seja um trigger
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        if (showText && uiText != null)
            uiText.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag(playerTag)) return;

        activated = true;
        StartCoroutine(HandleExit());
    }

    IEnumerator HandleExit()
    {
        // Mostrar texto, se configurado
        if (showText)
        {
            if (uiText != null)
            {
                uiText.text = textToShow;
                uiText.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log(textToShow);
            }
        }

        // espera o tempo do texto antes de carregar (ou apenas espera se não for carregar cena)
        float wait = showText ? textDuration : 0f;
        if (loadScene && wait < sceneDelay)
            wait = sceneDelay; // garante que espere pelo menos o delay da cena

        yield return new WaitForSeconds(wait);

        if (loadScene)
        {
            if (!string.IsNullOrEmpty(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                Debug.LogWarning("Saida: 'sceneName' não definido. Nenhuma cena será carregada.");
        }
    }
}

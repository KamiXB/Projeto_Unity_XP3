using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager :MonoBehaviour
{
    [SerializeField] private string nomedoLevel;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelCreditos;
    public void Jogar()
    {
        SceneManager.LoadScene(nomedoLevel);

    }

    public void AbrirCreditos()
    {
        painelMenuInicial.SetActive(false);        
        painelCreditos.SetActive(true);

    }

    public void FecharCreditos()
    {
        painelCreditos.SetActive(false);
        painelMenuInicial.SetActive(true); 
    }

    public void Sair()
    {
        Debug.Log("Sair do jogo");
        Application.Quit();
    }
}

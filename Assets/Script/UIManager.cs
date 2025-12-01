using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject homePanel;
    public GameObject loginPanel;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Logout() {
        homePanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void Login() {
        homePanel.SetActive(true);
        loginPanel.SetActive(false);
    }
}

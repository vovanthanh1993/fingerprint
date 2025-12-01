using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject homePanel;
    public GameObject loginPanel;

    public GameObject androidLogin;
    public GameObject iosLogin;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Setup login buttons based on platform
            SetupLoginButtons();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void SetupLoginButtons()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Android platform - show Android login button only
        if (androidLogin != null) androidLogin.SetActive(true);
        if (iosLogin != null) iosLogin.SetActive(false);
        Debug.Log("Platform: Android - Showing Android login button");
        #elif UNITY_IOS && !UNITY_EDITOR
        // iOS platform - show iOS login button only
        if (androidLogin != null) androidLogin.SetActive(false);
        if (iosLogin != null) iosLogin.SetActive(true);
        Debug.Log("Platform: iOS - Showing iOS login button");
        #else
        // Editor or other platforms - show both for testing
        if (androidLogin != null) androidLogin.SetActive(true);
        if (iosLogin != null) iosLogin.SetActive(true);
        Debug.Log("Platform: Editor/Other - Showing both login buttons for testing");
        #endif
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

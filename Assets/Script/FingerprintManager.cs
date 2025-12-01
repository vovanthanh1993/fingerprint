using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.InteropServices;

public class FingerprintManager : MonoBehaviour
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass biometricPluginClass;
    #endif
    
    #if UNITY_IOS && !UNITY_EDITOR
    // iOS plugin imports
    [DllImport("__Internal")]
    private static extern bool IsBiometricAvailable();
    
    [DllImport("__Internal")]
    private static extern void AuthenticateBiometric(string reason);
    #endif
    
    private static FingerprintManager instance;

    void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            // Ensure GameObject is not destroyed when loading new scenes
            DontDestroyOnLoad(gameObject);
            // Ensure GameObject name is correct
            gameObject.name = "FingerprintManager";
            Debug.Log("FingerprintManager GameObject initialized: " + gameObject.name);
        }
        else
        {
            // If instance already exists, destroy duplicate
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize plugin class
        InitializePlugin();
        
        Debug.Log("FingerprintManager Start() - GameObject: " + gameObject.name + ", Active: " + gameObject.activeSelf);
    }

    /// <summary>
    /// Initialize plugin class
    /// </summary>
    public static void InitializePlugin()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            Debug.Log("Initializing Android BiometricPlugin class...");
            // Get plugin class
            biometricPluginClass = new AndroidJavaClass("com.example.biometricplugin.BiometricPlugin");
            Debug.Log("Android BiometricPlugin class loaded successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load BiometricPlugin class: " + e.Message);
            Debug.LogError("Please check:");
            Debug.LogError("1. File biometricplugin.aar exists in Assets/Plugins/Android/");
            Debug.LogError("2. Package name is correct: com.example.biometricplugin.BiometricPlugin");
            Debug.LogError("3. Plugin has been built and exported correctly");
            biometricPluginClass = null;
        }
        #elif UNITY_IOS && !UNITY_EDITOR
        Debug.Log("Initializing iOS BiometricPlugin...");
        bool isAvailable = IsBiometricAvailable();
        if (isAvailable)
        {
            Debug.Log("iOS Biometric authentication is available");
        }
        else
        {
            Debug.LogWarning("iOS Biometric authentication is not available on this device");
        }
        #else
        Debug.Log("InitializePlugin() - Running in Editor, skipping plugin initialization");
        #endif
    }

    /// <summary>
    /// Call authenticate method from plugin (Android or iOS)
    /// </summary>
    public static void Authenticate()
    {
        Debug.Log("=== Authenticate() called ===");
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Android implementation
        try
        {
            if (biometricPluginClass == null)
            {
                Debug.LogWarning("Android plugin class not loaded. Initializing now...");
                InitializePlugin();
                
                if (biometricPluginClass == null)
                {
                    Debug.LogError("Failed to initialize Android BiometricPlugin class!");
                    Debug.LogError("Please check:");
                    Debug.LogError("1. File biometricplugin.aar exists in Assets/Plugins/Android/");
                    Debug.LogError("2. Plugin package name is correct: com.example.biometricplugin.BiometricPlugin");
                    return;
                }
            }

            Debug.Log("Android plugin class is ready. Calling authenticate()...");
            
            // Call authenticate method from Android plugin
            // Plugin will automatically get Unity Activity from UnityPlayer.currentActivity
            biometricPluginClass.CallStatic("authenticate");
            
            Debug.Log("Android authenticate() called successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error calling Android authenticate: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
        }
        
        #elif UNITY_IOS && !UNITY_EDITOR
        // iOS implementation
        try
        {
            Debug.Log("Calling iOS biometric authentication...");
            
            // Check if biometric is available
            if (!IsBiometricAvailable())
            {
                Debug.LogError("Biometric authentication is not available on this iOS device");
                if (instance != null)
                {
                    instance.OnError("Biometric authentication is not available");
                }
                return;
            }
            
            // Call iOS authenticate with reason message
            string reason = "Authenticate to continue";
            AuthenticateBiometric(reason);
            
            Debug.Log("iOS authenticate() called successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error calling iOS authenticate: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
        }
        
        #else
        // Editor or unsupported platform
        Debug.LogWarning("Biometric authentication only works on actual Android/iOS devices!");
        Debug.LogWarning("Please build and run on a device to test.");
        #endif
    }

    /// <summary>
    /// Instance method to call from Unity Button OnClick
    /// Unity Button OnClick can call both static and instance methods
    /// </summary>
    public void AuthenticateButton()
    {
        Debug.Log("Button clicked - calling Authenticate()...");
        Authenticate();
    }

    /// <summary>
    /// Callback called from Android/iOS plugin when authentication succeeds
    /// Android: UnitySendMessage("FingerprintManager", "OnSuccess", "")
    /// iOS: UnitySendMessage("FingerprintManager", "OnSuccess", "")
    /// </summary>
    public void OnSuccess(string message)
    {
        Debug.Log("========================================");
        Debug.Log("=== OnSuccess CALLBACK RECEIVED ===");
        Debug.Log("========================================");
        Debug.Log("Biometric authentication succeeded! Message: " + message);
        Debug.Log("GameObject: " + gameObject.name + ", Active: " + gameObject.activeSelf);
        Debug.Log("Loading GamePlayScene...");
        
        // Load GamePlayScene when authentication succeeds
        // Delay slightly to ensure callback has completed
        StartCoroutine(LoadGamePlaySceneDelayed());
    }

    private System.Collections.IEnumerator LoadGamePlaySceneDelayed()
    {
        // Wait 0.1 seconds to ensure callback has completed
        yield return new WaitForSeconds(0.1f);
        
        try
        {
            Debug.Log("Loading scene: GamePlayScene");
            UIManager.instance.Login();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load GamePlayScene: " + e.Message);
            Debug.LogError("Make sure 'GamePlayScene' is added to Build Settings.");
        }
    }

    /// <summary>
    /// Callback called from Android/iOS plugin when authentication fails
    /// Android: UnitySendMessage("FingerprintManager", "OnFailed", "")
    /// iOS: UnitySendMessage("FingerprintManager", "OnFailed", "")
    /// </summary>
    public void OnFailed(string message)
    {
        Debug.Log("========================================");
        Debug.Log("=== OnFailed CALLBACK RECEIVED ===");
        Debug.Log("========================================");
        Debug.Log("Biometric authentication failed! Message: " + message);
        Debug.Log("GameObject: " + gameObject.name + ", Active: " + gameObject.activeSelf);
        
        // Add logic to handle authentication failure here
        // Example: show error message, allow retry, etc.
    }

    /// <summary>
    /// Callback called from Android/iOS plugin when an error occurs
    /// Android: UnitySendMessage("FingerprintManager", "OnError", errorMessage)
    /// iOS: UnitySendMessage("FingerprintManager", "OnError", errorMessage)
    /// </summary>
    public void OnError(string errorMessage)
    {
        Debug.LogError("========================================");
        Debug.LogError("=== OnError CALLBACK RECEIVED ===");
        Debug.LogError("========================================");
        Debug.LogError("Biometric authentication error: " + errorMessage);
        Debug.LogError("GameObject: " + gameObject.name + ", Active: " + gameObject.activeSelf);
    }
}

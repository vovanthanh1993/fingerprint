using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class FingerprintManager : MonoBehaviour
{
    private static AndroidJavaClass biometricPluginClass;
    private static FingerprintManager instance;

    void Awake()
    {
        // Singleton pattern để đảm bảo chỉ có một instance
        if (instance == null)
        {
            instance = this;
            // Đảm bảo GameObject không bị destroy khi load scene mới
            DontDestroyOnLoad(gameObject);
            // Đảm bảo tên GameObject đúng
            gameObject.name = "FingerprintManager";
            Debug.Log("FingerprintManager GameObject initialized: " + gameObject.name);
        }
        else
        {
            // Nếu đã có instance, destroy duplicate
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Khởi tạo plugin class
        InitializePlugin();
        
        Debug.Log("FingerprintManager Start() - GameObject: " + gameObject.name + ", Active: " + gameObject.activeSelf);
    }

    /// <summary>
    /// Khởi tạo plugin class
    /// </summary>
    public static void InitializePlugin()
    {
        #if !UNITY_ANDROID || UNITY_EDITOR
        Debug.Log("InitializePlugin() - Running in Editor, skipping Android plugin initialization");
        return;
        #endif

        try
        {
            Debug.Log("Initializing BiometricPlugin class...");
            // Get plugin class
            biometricPluginClass = new AndroidJavaClass("com.example.biometricplugin.BiometricPlugin");
            Debug.Log("✓ BiometricPlugin class loaded successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Failed to load BiometricPlugin class: " + e.Message);
            Debug.LogError("Please check:");
            Debug.LogError("1. File biometricplugin.aar exists in Assets/Plugins/Android/");
            Debug.LogError("2. Package name is correct: com.example.biometricplugin.BiometricPlugin");
            Debug.LogError("3. Plugin has been built and exported correctly");
            biometricPluginClass = null;
        }
    }

    /// <summary>
    /// Gọi phương thức authenticate từ plugin Android
    /// Plugin sẽ tự động lấy Unity Activity từ UnityPlayer.currentActivity
    /// </summary>
    public static void Authenticate()
    {
        Debug.Log("=== Authenticate() called ===");
        
        #if !UNITY_ANDROID || UNITY_EDITOR
        Debug.LogWarning("Biometric authentication only works on actual Android devices!");
        Debug.LogWarning("Please build and run on an Android device to test.");
        return;
        #endif

        try
        {
            if (biometricPluginClass == null)
            {
                Debug.LogWarning("Plugin class not loaded. Initializing now...");
                InitializePlugin();
                
                if (biometricPluginClass == null)
                {
                    Debug.LogError("Failed to initialize BiometricPlugin class!");
                    Debug.LogError("Please check:");
                    Debug.LogError("1. File biometricplugin.aar exists in Assets/Plugins/Android/");
                    Debug.LogError("2. Plugin package name is correct: com.example.biometricplugin.BiometricPlugin");
                    return;
                }
            }

            Debug.Log("Plugin class is ready. Calling authenticate()...");
            
            // Call authenticate method from plugin
            // Plugin will automatically get Unity Activity from UnityPlayer.currentActivity
            biometricPluginClass.CallStatic("authenticate");
            
            Debug.Log("✓ authenticate() called successfully!");
            Debug.Log("If dialog doesn't appear, check Logcat for errors from Android plugin.");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error calling authenticate: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
            Debug.LogError("Check Logcat for detailed errors from Android plugin.");
        }
    }

    /// <summary>
    /// Instance method để gọi từ Unity Button OnClick
    /// Unity Button OnClick có thể gọi cả static và instance methods
    /// </summary>
    public void AuthenticateButton()
    {
        Debug.Log("Button clicked - calling Authenticate()...");
        Authenticate();
    }

    /// <summary>
    /// Callback được gọi từ Android plugin khi xác thực thành công
    /// Tên phương thức phải khớp với UnitySendMessage("FingerprintManager", "OnSuccess", "")
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
        // Delay một chút để đảm bảo callback đã hoàn thành
        StartCoroutine(LoadGamePlaySceneDelayed());
    }

    private System.Collections.IEnumerator LoadGamePlaySceneDelayed()
    {
        // Đợi 0.1 giây để đảm bảo callback đã hoàn thành
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
    /// Callback được gọi từ Android plugin khi xác thực thất bại
    /// Tên phương thức phải khớp với UnitySendMessage("FingerprintManager", "OnFailed", "")
    /// </summary>
    public void OnFailed(string message)
    {
        Debug.Log("========================================");
        Debug.Log("=== OnFailed CALLBACK RECEIVED ===");
        Debug.Log("========================================");
        Debug.Log("Biometric authentication failed! Message: " + message);
        Debug.Log("GameObject: " + gameObject.name + ", Active: " + gameObject.activeSelf);
        
        // Thêm logic xử lý khi xác thực thất bại ở đây
        // Ví dụ: hiển thị thông báo lỗi, cho phép thử lại, etc.
    }

    /// <summary>
    /// Callback được gọi từ Android plugin khi có lỗi xảy ra
    /// </summary>
    public void OnError(string errorMessage)
    {
        Debug.LogError("========================================");
        Debug.LogError("=== OnError CALLBACK RECEIVED ===");
        Debug.LogError("========================================");
        Debug.LogError("Biometric authentication error: " + errorMessage);
        Debug.LogError("GameObject: " + gameObject.name + ", Active: " + gameObject.activeSelf);
        
        // Thêm logic xử lý khi có lỗi ở đây
    }
}

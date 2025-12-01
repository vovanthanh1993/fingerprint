#ifndef BIOMETRIC_PLUGIN_H
#define BIOMETRIC_PLUGIN_H

#ifdef __cplusplus
extern "C" {
#endif

    // Check if biometric authentication is available
    bool IsBiometricAvailable();
    
    // Authenticate using biometric (Face ID or Touch ID)
    void AuthenticateBiometric(const char* reason);

#ifdef __cplusplus
}
#endif

#endif // BIOMETRIC_PLUGIN_H


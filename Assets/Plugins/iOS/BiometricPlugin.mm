#import <LocalAuthentication/LocalAuthentication.h>
#import <Foundation/Foundation.h>

// Unity interface - declare UnitySendMessage
extern "C" {
    void UnitySendMessage(const char* obj, const char* method, const char* msg);
}

extern "C" {
    
    // Function to check if biometric authentication is available
    bool IsBiometricAvailable() {
        LAContext *context = [[LAContext alloc] init];
        NSError *error = nil;
        bool canEvaluate = [context canEvaluatePolicy:LAPolicyDeviceOwnerAuthenticationWithBiometrics error:&error];
        return canEvaluate && error == nil;
    }
    
    // Function to authenticate using biometric (Face ID or Touch ID)
    void AuthenticateBiometric(const char* reason) {
        NSLog(@"[BiometricPlugin] AuthenticateBiometric called with reason: %s", reason ? reason : "nil");
        
        LAContext *context = [[LAContext alloc] init];
        NSString *nsReason = [NSString stringWithUTF8String:reason ? reason : "Authenticate to continue"];
        
        NSLog(@"[BiometricPlugin] Starting biometric authentication...");
        
        [context evaluatePolicy:LAPolicyDeviceOwnerAuthenticationWithBiometrics
                localizedReason:nsReason
                          reply:^(BOOL success, NSError *error) {
            NSLog(@"[BiometricPlugin] Biometric authentication result: success=%d, error=%@", success, error);
            
            // Must call UnitySendMessage on main thread
            dispatch_async(dispatch_get_main_queue(), ^{
                if (success) {
                    NSLog(@"[BiometricPlugin] Calling UnitySendMessage: FingerprintManager.OnSuccess");
                    UnitySendMessage("FingerprintManager", "OnSuccess", "");
                } else {
                    if (error) {
                        // Check error code to determine if it's a failure or error
                        NSInteger errorCode = [error code];
                        NSLog(@"[BiometricPlugin] Error code: %ld", (long)errorCode);
                        
                        if (errorCode == LAErrorUserCancel || errorCode == LAErrorAppCancel) {
                            // User cancelled - treat as failure
                            NSLog(@"[BiometricPlugin] User cancelled - calling OnFailed");
                            UnitySendMessage("FingerprintManager", "OnFailed", "");
                        } else {
                            // Other errors
                            NSString *errorMessage = [error localizedDescription];
                            const char* errorMsg = [errorMessage UTF8String];
                            NSLog(@"[BiometricPlugin] Error occurred - calling OnError: %s", errorMsg ? errorMsg : "unknown");
                            UnitySendMessage("FingerprintManager", "OnError", errorMsg ? errorMsg : "");
                        }
                    } else {
                        NSLog(@"[BiometricPlugin] Unknown error - calling OnFailed");
                        UnitySendMessage("FingerprintManager", "OnFailed", "");
                    }
                }
            });
        }];
    }
}


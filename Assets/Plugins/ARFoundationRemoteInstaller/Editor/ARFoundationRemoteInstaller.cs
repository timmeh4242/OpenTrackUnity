using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Assertions;


namespace ARFoundationRemote.Editor {
    public class ARFoundationRemoteInstaller : ScriptableObject {
        ARFoundationVersion arFoundationVersion = ARFoundationVersion.Version3_0_1_or_newer;

        const string pluginName = "AR Foundation Remote";
        [CanBeNull] static ARFoundationRemoteInstaller Instance;
        static Request currentRequest;

        static readonly Dictionary<string, string> minDependencies = new Dictionary<string, string> {
            {"com.unity.xr.arfoundation", "3.0.1"},
            {"com.unity.xr.arsubsystems", "3.0.0"},
            //{"com.unity.xr.management", "3.2.10"} // no need to check this dependency. Unity 2019.2 will not allow to install a version newer than 3.0.3 and Unity 2019.3 will not allow to install the version lower than 3.2.10
        };


        void OnEnable() {
            Instance = this;
        }

        public static void TryInstallOnImport() {
            InstallOrUpdate();
        }

        [ShowInInspector]
        static void InstallOrUpdate() {
            CheckDependencies(success => {
                if (success) {
                    var installRequest = Client.Add(getUrl());
                    runRequest(installRequest, () => {
                        if (installRequest.Status == StatusCode.Success) {
                            Debug.Log(pluginName + " installed successfully.\n" +
                                      "To enable iOS face tracking, install ARKit Face Tracking 3.0.1 or newer via Package Manager.\n" +
                                      "To enable ARKit Face Blendshapes, add this define to Project Settings -> Player -> Scripting Define Symbols: ARFOUNDATION_REMOTE_ENABLE_IOS_BLENDSHAPES\n" +
                                      "To enable the plugin to work with AR Foundation 4.0.0 or newer, add this define to Scripting Define Symbols: ARFOUNDATION_4_0_OR_NEWER\n");
                        } else {
                            Debug.LogError(pluginName + " installation failed: " + installRequest.Error.message);
                        }
                    });
                } else {
                    Debug.LogError(pluginName + " installation failed. Please update packages to the required versions");
                }
            });
        }

        [ShowInInspector]
        void UnInstall() {
            var removalRequest = Client.Remove("com.kyrylokuzyk.arfoundationremote");
            runRequest(removalRequest, () => {
                if (removalRequest.Status == StatusCode.Success) {
                    Debug.Log(pluginName + " removed successfully" + ". If you want to delete the plugin completely, please delete the folder: Assets/Plugins/ARFoundationRemoteInstaller");
                } else {
                    Debug.LogError(pluginName + " removal failed: " + removalRequest.Error.message);
                }
            });
        }

        static void CheckDependencies(Action<bool> callback) {
            var listRequest = Client.List();
            runRequest(listRequest, () => {
                callback(checkDependencies(listRequest));
            });
        }

        static bool checkDependencies(ListRequest listRequest) {
            var result = true;
            foreach (var package in listRequest.Result) {
                var packageName = package.name;
                if (minDependencies.TryGetValue(packageName, out string dependency)) {
                    //Debug.Log(packageName);
                    var minRequiredVersion = new Version(dependency);
                    var currentVersion = parseUnityPackageManagerVersion(package.version);
                    if (currentVersion < minRequiredVersion) {
                        result = false;
                        Debug.LogError("Please update this package to the required version via Window -> Package Manager: " + packageName + ":" + minRequiredVersion);
                    }
                }
            }

            return result;
        }

        static Version parseUnityPackageManagerVersion(string version) {
            var versionNumbersStrings = version.Split('.', '-');
            const int numOfVersionComponents = 3;
            Assert.IsTrue(versionNumbersStrings.Length >= numOfVersionComponents);
            var numbers = new List<int>();
            for (int i = 0; i < numOfVersionComponents; i++) {
                var str = versionNumbersStrings[i];
                if (int.TryParse(str, out int num)) {
                    numbers.Add(num);
                } else {
                    throw new Exception("cant parse " + str + " in " + version);
                }
            }

            return new Version(numbers[0], numbers[1], numbers[2]);
        }

        static Action requestCompletedCallback;

        static void runRequest(Request request, Action callback) {
            if (currentRequest != null) {
                Debug.Log(currentRequest.GetType().Name + " is already running, skipping new " + request.GetType().Name);
                return;
            }
        
            Assert.IsNull(requestCompletedCallback);
            Assert.IsNull(currentRequest);
            currentRequest = request;
            requestCompletedCallback = callback;
            EditorApplication.update += editorUpdate;
        }

        static void editorUpdate() {
            Assert.IsNotNull(currentRequest);
            if (currentRequest.IsCompleted) {
                EditorApplication.update -= editorUpdate;
                currentRequest = null;
                var cachedCallback = requestCompletedCallback;
                requestCompletedCallback = null;
                cachedCallback();
            }
        }

        static string getUrl() {
            var ver3_1_3URL = "https://kuzykkirill:gXfNFPSZ1sfx3PsiMxPz@gitlab.com/kuzykkirill/arfoundationremote.git#f6561ed6ebce2bc7b1688f8b224dc59dd23fbb82";
            if (Instance == null) {
                return ver3_1_3URL;
            }

            switch (Instance.arFoundationVersion) {
                case ARFoundationVersion.Version3_0_1_or_newer:
                    return ver3_1_3URL;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum ARFoundationVersion {
        Version3_0_1_or_newer,
//        Version4_0_0_preview_3
    }
}

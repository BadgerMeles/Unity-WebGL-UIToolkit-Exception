using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestWebGL {
    /// <summary>
    /// When should the test be ran.
    /// Delayed: The test will be delayed by 1 second (or whatever is set in AUIMB) after the scene loads.
    /// </summary>
    public enum When {
        Awake,
        Enable,
        Start,
        Update,
        LateUpdate,
        Delayed, // TODO: The DelayActive that was added, could probably be used to simplify the Delayed logic used.
    }
    public enum How {
        Singleton_ClearAdd, // A DontDestroyOnLoad UIDocument is created that has a VisualElement container. The UI Controller in the test scene will .Clear() and then .Add() a VisualElementTree.cloneTree() to the container. UI removal/destruction is handled by the .Clear().
        Singleton_Replace, // A DontDestroyOnLoad UIDocument is created. The UI Controller in the test scene will set (replace) the UIDocument.visualTreeAsset with that scene's specific VisualTreeAsset. UI removal/destruction is handled by the overwriting of UIDocument.visualTreeAsset.
        Individual_Create, // The UI Controller in each scene will create a new UIDocument (via AddComponent), and assign that scene's specific values to UIDocument.visualTreeAsset and .panelSettings. UI removal/destruction is handled by the unload of the scene.
        Individual_Exists, // The UI Controller in each scene already has an attached UIDocument component, preconfigured and setup. UI removal/destruction is handled by the unloading of the scene.
        Individual_Exists_Delayed, // The UI Controller in each scene already has an attached UIDocument component, preconfigured and setup; however, the GameObject start inactivated, and will activate after X number of seconds (defined as delayedSceneAmount). UI removal/destruction is handled by the unloading of the scene.
    }

    [Flags]
    public enum LogOptions {
        // Log message noting a particular method call
        LOG_ENABLE /*    */ = 1<<0,
        LOG_DISABLE /*   */ = 1<<1,
        LOG_DESTROY /*   */ = 1<<2,
        LOG_AWAKE /*     */ = 1<<3,
        LOG_START /*     */ = 1<<4,
        LOG_UPDATE /*    */ = 1<<5,
        LOG_LATEUPDATE /**/ = 1<<6,
        LOG_DELAYED /*   */ = 1<<7,
        // Log message level for the test itself.
        LOG_TEST_L1 /*   */ = 1<<8, // L1: Name of test that is being ran.
        LOG_TEST_L2 /*   */ = 1<<9  | LOG_TEST_L1, // L2: Immediatel before/after the expected failure point. ie: When successfull, two messages will be reported; when failed, one message will be reported. This acts as a 'proof' that the error occurs during that specific event.
        LOG_TEST_L3 /*   */ = 1<<10 | LOG_TEST_L2, // L3: Any other messages.
        LOG_NONE /**/ = 0,
        LOG_SUGGESTED = LOG_TEST_L2,
        LOG_ALL = ~0,
    }

    public class SplashScreen : MonoBehaviour {
        static public LogOptions debugLog { get; private set; }
        static public When when { get; private set; }
        static public How how { get; private set; }

        static public string sceneOne { get; private set; }
        static public string sceneTwo { get; private set; }
        static public bool useAsyncLoadScene { get; private set; }

        [SerializeField] protected LogOptions _debugLog = LogOptions.LOG_ALL;
        [SerializeField] protected When _when = When.Awake;
        [SerializeField] protected How _how = How.Singleton_ClearAdd;
        [SerializeField] protected bool _useAsyncLoadScene = true;
        [SerializeField] internal static float delayedSceneAmount = 3; // Only applicable with Individual_Exists_Delayed. Determines how long the scene should wait before activating the UIDocument object.

        void Start() {
            Debug.Log($"SplashScreen.Start():{SceneManager.loadedSceneCount} loaded scenes.");
            debugLog = _debugLog;
            when = _when;
            how = _how;
            useAsyncLoadScene = _useAsyncLoadScene;

            switch (this._how) {
                case How.Singleton_ClearAdd:
                case How.Singleton_Replace:
                    sceneOne = "SceneOne_NoDoc";
                    sceneTwo = "SceneTwo_NoDoc";
                    DontDestroyOnLoad(GameObject.Find("PersistUIDocument"));
                    break;
                case How.Individual_Create:
                    sceneOne = "SceneOne_NoDoc";
                    sceneTwo = "SceneTwo_NoDoc";
                    Destroy(GameObject.Find("PersistUIDocument").gameObject);
                    break;
                case How.Individual_Exists:
                    sceneOne = "SceneOne_WithDoc";
                    sceneTwo = "SceneTwo_WithDoc";
                    Destroy(GameObject.Find("PersistUIDocument").gameObject);
                    break;
                case How.Individual_Exists_Delayed:
                    sceneOne = "SceneOne_WithDocDelayed";
                    sceneTwo = "SceneTwo_WithDocDelayed";
                    Destroy(GameObject.Find("PersistUIDocument").gameObject);
                    break;
            }
            StartCoroutine(LoadScene());
        }
        private IEnumerator LoadScene() {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneOne, LoadSceneMode.Additive);
            while (!asyncLoad.isDone) yield return null;
            SceneManager.UnloadSceneAsync("SplashScreen");
        }
    }
}

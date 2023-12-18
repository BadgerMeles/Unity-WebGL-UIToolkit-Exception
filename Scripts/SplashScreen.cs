using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


/*

I also encountered this issue when building WebGL [Unity 2023.1.20f1], playing in Firefox 120.0.1 (64-bit).

However, my example I while switching between scenes (ie: Back and forth between MainMenu <-> InGame). Each scene only needs its own UI.

I tried multiple approaches:
A) Singleton UIDocument, that each scene will set the rootVisualElement to a value specific to that scene. In this scenario, I tried having a standard panelSetting and replacing it each time.
B) Singleton UIDocument, that each scene would .Clear() and .Add() a CloneTree() of its own specific VisualTreeAsset. Similarly, in this scenario, tried keeping and replacing panelSettings. 
C) Individual UIDocument, that each scene created via AddComponent, and set up with its own VisualTreeAsset and PanelSettings.
D) Individual UIDocument already existing on a GameObject in each scene, already had setup for with scene's VisualTreeAsset and PanelSettings.
E) Exactly as (D); however, the GameObject is inactive for X seconds. (This was to test if an alotted time needed to pass before loading a UIDocument)

I never saw the IndexOutOfRangeException.
I am able to consistently reproduce-  InvalidOperationException: Stack Empty.
I also am able to consistently reproduce- InvalidOperationException: Failed to Free handle with Index=0 Version=0
Which was mentioned in this thread- forum.unity.com/threads/1391191

It seems that one or the other InvalidOperationException intermitently (may change between page reloads) will be thrown; however, regardless of which is thrown, it always will be at the same point for a given test.
I also have observed that the "Stack Empty" exception seems to occure more frequently with higher quantity of element in at least one of the uxml's; subsiquently, the other Exception seems to occur more frequently when there is only 1 element in both uxml files.

Some additional observations:
Scenario (A) will always fail during the assignment of UIDocument.visualTreeAsset (ie: uidoc.visualTreeAsset = vta;)
Scenario (B) will always fail during CloneTree()
Scenario (C) will always fail during AddComponent<UIDocument>()
Scenario (D) will always fail before Awake()
Scenario (E) will always fail before Awake()
 
 */

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
        Singleton_ClearAdd, // One DontDestroyOnLoad UIDocument is created, that each scene's UI Controller will .Clear() and then .Add() a VisualElementTree.cloneTree() to. UI removal/destruction is handled by the .Clear().
        Singleton_Replace, // One DontDestroyOnLoad UIDocument is created, that each scene's UI Controller will set the UIDocument's panelSettings and visualTreeAsset. UI removal/destruction is handled by the replacement/unsetting of the visualTreeAsset on the UIDocument.
        Individual_Create, // Each UI Controller has its own UIDocument already attached and setup. UI removal/destruction is handled by the scene unloading.
        Individual_Exists, // Each UI Controller will add a new UIDocument and set it up. UI removal/destruction is handled by the scene unloading.
        Individual_Exists_Delayed, // Each UI Controller will add a new UIDocument and set it up. UI removal/destruction is handled by the scene unloading.
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
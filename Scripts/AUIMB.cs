using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UIElements;

namespace TestWebGL {
    /// <summary>
    /// Abstract MonoBehaviour for UIToolkit Controllers
    /// Originally this class had additional functionality, but was stripped for testing.
    /// This abstract class now also facilitates all of the test functionality for this TestWebGL example.
    /// </summary>
    public abstract class AUIMB : MonoBehaviour {

        [SerializeField] PanelSettings panelSettings;

        [SerializeField] protected SpriteRenderer foo;
        [SerializeField] protected UIDocument uiDocument;
        [SerializeField] protected VisualTreeAsset visualTreeAsset;
        [SerializeField] protected VisualElement root;
        protected bool loading = false;
        private bool doneTest = false;
        private float delayed = 1; // Only relevant for When.Delayed. Delays X number seconds after loading to execute the test. - This was to check if the scene needed "some time to settle" before doing anything with the UI.

        private LogOptions debugLog => SplashScreen.debugLog;
        protected abstract string thisScene { get; }

        protected IEnumerator LoadSceneOne() => LoadScene(SplashScreen.sceneTwo, SplashScreen.sceneOne);
        protected IEnumerator LoadSceneTwo() => LoadScene(SplashScreen.sceneOne, SplashScreen.sceneTwo);
        private IEnumerator LoadScene(string from, string to) {
            Debug.Log($"AUIMB[{this.GetType().Name}].LoadScene():{from} -> {to}");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);
            while (!asyncLoad.isDone) yield return null;

            Debug.Log($"AUIMB[{this.GetType().Name}].LoadScene(): Finished Loading {to}. loadedSceneCount:{SceneManager.loadedSceneCount}. Unloading {from}.");
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(from);
            while (!asyncUnload.isDone) yield return null;
            // Technically, this should never happen - as long as the tests are setup correctly. Since this should get stopped when the scene is unloaded. However, this message will get shown if the wrong scene was unloaded - so, it's basically just here as a sanity check.
            Debug.LogWarning($"AUIMB[{this.GetType().Name}].LoadScene(): Finished Unloading {from}. loadedSceneCount:{SceneManager.loadedSceneCount}. [You probably should never see this message. See comments.]");
        }

        private void OnEnable() {
            if (debugLog.HasFlag(LogOptions.LOG_ENABLE)) Debug.Log($"AUIMB[{this.GetType().Name}].OnEnable()");
            if (SplashScreen.when == When.Enable) DoTest();
        }
        private void Awake() {
            if (debugLog.HasFlag(LogOptions.LOG_AWAKE)) Debug.Log($"AUIMB[{this.GetType().Name}].Awake()");
            if (SplashScreen.when == When.Awake) DoTest();
        }
        private void Start() {
            if (debugLog.HasFlag(LogOptions.LOG_START)) Debug.Log($"AUIMB[{this.GetType().Name}].Start()");
            if (SplashScreen.when == When.Start) DoTest();
        }

        private void Update() {
            if (loading) { } // Do nothing, just wait.
            else if (Input.GetKeyDown(KeyCode.PageDown) && SplashScreen.sceneOne.Equals(thisScene)) {
                loading = true;
                if (SplashScreen.useAsyncLoadScene) StartCoroutine(LoadSceneTwo());
                else {
                    SceneManager.LoadScene(SplashScreen.sceneTwo);
                    SceneManager.UnloadSceneAsync(SplashScreen.sceneOne);
                }
            } else if (Input.GetKeyDown(KeyCode.PageUp) && SplashScreen.sceneTwo.Equals(thisScene)) {
                loading = true;
                if (SplashScreen.useAsyncLoadScene) StartCoroutine(LoadSceneOne());
                else {
                    SceneManager.LoadScene(SplashScreen.sceneOne);
                    SceneManager.UnloadSceneAsync(SplashScreen.sceneTwo);
                }
            } else if (SplashScreen.when == When.Update && !doneTest) {
                if (debugLog.HasFlag(LogOptions.LOG_UPDATE)) Debug.Log($"AUIMB[{this.GetType().Name}].Update()");
                DoTest();
            } else if (SplashScreen.when == When.Delayed && !doneTest) {
                if (delayed > 0) {
                    if (debugLog.HasFlag(LogOptions.LOG_UPDATE)) Debug.Log($"AUIMB[{this.GetType().Name}].Update(): Delaying Test: {delayed} frame remaining.");
                    delayed -= Time.deltaTime;
                } else {
                    if (debugLog.HasFlag(LogOptions.LOG_DELAYED)) Debug.Log($"AUIMB[{this.GetType().Name}].Update(): Starting Delayed Test");
                    DoTest();
                }
            }
        }
        private void LateUpdate() {
            if (SplashScreen.when == When.LateUpdate && !doneTest) { if (debugLog.HasFlag(LogOptions.LOG_LATEUPDATE)) Debug.Log($"AUIMB[{this.GetType().Name}].LateUpdate()"); DoTest(); }
        }
        void DoTest() {
            if (doneTest) Debug.LogWarning($"AUIMB[{this.GetType().Name}].DoTest():Tests should only be ran once.");
            doneTest = true;
            switch (SplashScreen.how) {
                case How.Singleton_ClearAdd: DoTest_Singleton_ClearAdd(); break;
                case How.Singleton_Replace: DoTest_Singleton_Replace(); break;
                case How.Individual_Create: DoTest_Individual_Create(); break;
                case How.Individual_Exists: DoTest_Individual_Exists(); break;
                case How.Individual_Exists_Delayed: DoTest_Individual_Exists_Delayed(); break;
            }
        }

        /// <summary>
        /// This test assumes the scene has a global/singleton UIDocument available, with an existing VisualElement to use as a containter. This will Clear() the container, then attempt to add a CloneTree() VisualElement to the container via .Add().
        /// The failure is expected to occur during the assignment of the visualTreeAsset to the UIDocument.
        /// L1: Name of test that is being ran.
        /// L2: Immediatel before/after the expected failure point. ie: When successfull, two messages will be reported; when failed, one message will be reported. This acts as a 'proof' that the error occurs during that specific event.
        /// L3: Any other messages.
        /// Note that the local-variable assignment was done intentionally to separate reading/writing data on this component.
        /// </summary>
        void DoTest_Singleton_ClearAdd() {
            string logPrefix = $"AUIMB[{this.GetType().Name}].DoTest_Singleton_ClearAdd()";
            if (debugLog.HasFlag(LogOptions.LOG_TEST_L1)) Debug.Log($"{logPrefix}");

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Get this.visualTreeAsset");
            VisualTreeAsset vta = this.visualTreeAsset;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L2)) Debug.Log($"{logPrefix}:CloneTree");
            VisualElement ver = vta.CloneTree(); // This is what typically will fail, when it fails.

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L2)) Debug.Log($"{logPrefix}:GetComponent<UIDocument>");
            UIDocument gsDoc = GameObject.Find("PersistUIDocument").GetComponent<UIDocument>();

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Get rootVisualElement");
            VisualElement gsRoot = gsDoc.rootVisualElement;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Assign uiDocument");
            this.uiDocument = gsDoc;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Find testing container");
            VisualElement wui = gsRoot.Q<VisualElement>("TestContainer");

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Clear Container");
            wui.Clear();

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Assign this.root");
            this.root = ver;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Add this.root to testing container");
            wui.Add(root);

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Done");
        }
        /// <summary>
        /// This test assumes the scene has a global/singleton UIDocument available, and attempts to overwrite/set the VisualTreeAsset on that existing UIDocument.
        /// The failure is expected to occur during the assignment of the visualTreeAsset to the UIDocument.
        /// L1: Name of test that is being ran.
        /// L2: Immediatel before/after the expected failure point. ie: When successfull, two messages will be reported; when failed, one message will be reported. This acts as a 'proof' that the error occurs during that specific event.
        /// L3: Any other messages.
        /// Note that the local-variable assignment was done intentionally to separate reading/writing data on this component.
        /// </summary>
        void DoTest_Singleton_Replace() {
            string logPrefix = $"AUIMB[{this.GetType().Name}].DoTest_Singleton_Replace()";
            if (debugLog.HasFlag(LogOptions.LOG_TEST_L1)) Debug.Log($"{logPrefix}");

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Get this.visualTreeAsset");
            VisualTreeAsset vta = this.visualTreeAsset;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:GetComponent<UIDocument>");
            UIDocument gsDoc = GameObject.Find("PersistUIDocument").GetComponent<UIDocument>();

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L2)) Debug.Log($"{logPrefix}:Assign uiDocument.visualTreeAsset");
            gsDoc.visualTreeAsset = vta;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L2)) Debug.Log($"{logPrefix}:Get rootVisualElement");
            VisualElement rootVE = gsDoc.rootVisualElement;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Assign uiDocument");
            this.uiDocument = gsDoc;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Assign root");
            this.root = rootVE;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Done");
        }
        /// <summary>
        /// This test assumes the scenes do not already have a UIDocument available, and creates one for itself via AddComponent().
        /// The failure is expected to occur during the AddComponent call.
        /// L1: Name of test that is being ran.
        /// L2: Immediatel before/after the expected failure point. ie: When successfull, two messages will be reported; when failed, one message will be reported. This acts as a 'proof' that the error occurs during that specific event.
        /// L3: Any other messages.
        /// Note that the local-variable assignment was done intentionally to separate reading/writing data on this component.
        /// </summary>
        void DoTest_Individual_Create() {
            string logPrefix = $"AUIMB[{this.GetType().Name}].DoTest_Individual_Create()";
            if (debugLog.HasFlag(LogOptions.LOG_TEST_L1)) Debug.Log($"{logPrefix}");

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Get this.panelSettings");
            PanelSettings ps = this.panelSettings;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Get this.visualTreeAsset");
            VisualTreeAsset vta = this.visualTreeAsset;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L2)) Debug.Log($"{logPrefix}:Set thisGO");
            GameObject thisGO = this.gameObject;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L2)) Debug.Log($"{logPrefix}:AddComponent");
            UIDocument uid = thisGO.AddComponent<UIDocument>();

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L2)) Debug.Log($"{logPrefix}:Set PanelSettings");
            uid.panelSettings = ps;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Set visualTreeAsset");
            uid.visualTreeAsset = vta;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Get rootVisualElement");
            VisualElement rve = uid.rootVisualElement;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Assign UIDocument");
            this.uiDocument = uid;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Assign root");
            this.root = rve;

            if (debugLog.HasFlag(LogOptions.LOG_TEST_L3)) Debug.Log($"{logPrefix}:Done");
        }
        /// <summary>
        /// This test does nothing, because the scenes already have the UIDocument complete setup and configured already.
        /// The failure is expected to occur before Awake()
        /// L1: Name of test that is being ran.
        /// L2: N/A
        /// L3: N/A
        /// </summary>
        void DoTest_Individual_Exists() {
            if (debugLog.HasFlag(LogOptions.LOG_TEST_L1)) Debug.Log($"AUIMB[{this.GetType().Name}].DoTest_Individual_Exists():Nothing to do, UI is setup and configured in the scene.");
        }
        /// <summary>
        /// This test does nothing, because the scenes already have the UIDocument complete setup and configured already.
        /// The failure is expected to occur before Awake()
        /// L1: Name of test that is being ran.
        /// L2: N/A
        /// L3: N/A
        /// </summary>
        void DoTest_Individual_Exists_Delayed() {
            if (debugLog.HasFlag(LogOptions.LOG_TEST_L1)) Debug.Log($"AUIMB[{this.GetType().Name}].DoTest_Individual_ExistsDelayed():Nothing to do, UI is setup and configured in the scene.");
        }

        private void OnDisable() { if (debugLog.HasFlag(LogOptions.LOG_DISABLE)) Debug.Log($"AUIMB[{this.GetType().Name}].OnDisable()"); }
        private void OnDestroy() { if (debugLog.HasFlag(LogOptions.LOG_DESTROY)) Debug.Log($"AUIMB[{this.GetType().Name}].OnDestroy()"); }
    }
}
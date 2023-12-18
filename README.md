# Description
A sample project exampling scenarios that cause Exceptions to be thrown in WebGL builds with UIToolkit.

I encountered this issue while working on a project, and spent a considerable amount of time trying to figure out what was happening - I was not able to find any reference to the original the error message I was getting.

Eventually, another error was produced, that I was able to get more information on. This lead me to the following reported issues.

I strongly suspect that this examples the same issue reported in the following:
* https://issuetracker.unity3d.com/issues/indexoutofrangeexception-in-uielements-dot-layout-dot-layoutmanager-dot-destroynode-when-changing-uxmls
* https://forum.unity.com/threads/1391191/
* https://forum.unity.com/threads/1465172/
* https://forum.unity.com/threads/1465181/

# Example Scene Info
SplashScreen is the starting scene to load.
The TestSetup object has several options available.

When: Determines at which point during the Execution Order the test should run.
How: Determines the 'type' of test to run.

* Singleton_ClearAdd : A DontDestroyOnLoad UIDocument is created that has a VisualElement container. The UI Controller in the test scene will .Clear() and then .Add() a VisualElementTree.cloneTree() to the container. UI removal/destruction is handled by the .Clear().
* Singleton_Replace : A DontDestroyOnLoad UIDocument is created. The UI Controller in the test scene will set (replace) the UIDocument.visualTreeAsset with that scene's specific VisualTreeAsset. UI removal/destruction is handled by the overwriting of UIDocument.visualTreeAsset.
* Individual_Create : The UI Controller in each scene will create a new UIDocument (via AddComponent), and assign that scene's specific values to UIDocument.visualTreeAsset and .panelSettings. UI removal/destruction is handled by the unload of the scene.
* Individual_Exists : The UI Controller in each scene already has an attached UIDocument component, preconfigured and setup. UI removal/destruction is handled by the unloading of the scene.
* Individual_Exists_Delayed : The UI Controller in each scene already has an attached UIDocument component, preconfigured and setup; however, the GameObject start inactivated, and will activate after X number of seconds (defined as delayedSceneAmount). UI removal/destruction is handled by the unloading of the scene.

# Workaround ?
* SceneManager.LoadSceneAsync() will result in the Exception being thrown.
* SceneManager.LoadScene(), so far, has not thrown the Exception.

# Usage
The Exceptions only occur when playing a WebGL build. I have not observed them in the Editor, or in any other build platform.

1. Set the TestSetup game object with the options you want to test for this build.
1. Build & Run the scene for WebGL.
1. Once built and playing, press Page-Up/Down to transition from scene to scene. [It is recommended to wait a couple seconds between scene loads, as I did not go out of my way to build any preventative measures for fast-switching, that could potentially leave a scene loaded when it shouldn't be.]
1. Eventually it will break, an Exception will be thrown, and the UI will no longer update when the scene changes.

# Observations
I never saw the IndexOutOfRangeException, mentioned in some of the posts noted above.

This example can consistently reproduce-  InvalidOperationException: Stack Empty.

This example can also consistently reproduce- InvalidOperationException: Failed to Free handle with Index=0 Version=0

It seems that one or the other InvalidOperationException intermitently (may change between page reloads) will be thrown; however, regardless of which is thrown, it always will be at the same point for a given test.

I also have observed that the "Stack Empty" exception seems to occure more frequently with higher quantity of element in at least one of the uxml's; subsiquently, the other Exception seems to occur more frequently when there is only 1 element in both uxml files.

# Expectations
* Singleton_Replace : Should fail during the assignment of UIDocument.visualTreeAsset (ie: uidoc.visualTreeAsset = vta;)
* Singleton_ClearAdd : Should fail during CloneTree()
* Individual_Create : Should fail during AddComponent<UIDocument>()
* Individual_Exists : Should fail before Awake()
* Individual_Exists_Delayed : Should fail before Awake()

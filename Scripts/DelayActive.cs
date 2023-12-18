using UnityEngine;

namespace TestWebGL {
    public class DelayActive : MonoBehaviour {
        [SerializeField] GameObject toActivate;
        private float delay;

        private void Start() { delay = SplashScreen.delayedSceneAmount; }

        void Update() {
            if ((delay -= Time.deltaTime) < 0) {
                toActivate.SetActive(true);
                Destroy(this.gameObject);
            }
        }
    }
}
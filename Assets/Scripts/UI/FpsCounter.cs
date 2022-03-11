using System;
using UnityEngine;

namespace UI {
    public class FpsCounter : MonoBehaviour {

        public int Samples = 100;
        public Action<float> OnFpsUpdate;
        
        private float totalTime;
        private int count;
        private float fps;

        private void Start() {
            count = Samples;
        }

        private void Update() {
            count -= 1;
            totalTime += Time.deltaTime;

            if (count <= 0) {
                fps = Samples / totalTime;
                OnFpsUpdate?.Invoke(fps);
                totalTime = 0f;
                count = Samples;
            }
        }
    }
}
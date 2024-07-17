using UnityEngine;

namespace Abecombe.FPSUtil
{
    public class FPSSetter : MonoBehaviour
    {
        [SerializeField]
        public int TargetFPS = 60;

        private int _previousTargetFPS;

        private void OnEnable()
        {
            QualitySettings.vSyncCount = 0;

            Application.targetFrameRate = TargetFPS;
            _previousTargetFPS = TargetFPS;
        }

        private void Update()
        {
            TargetFPS = Mathf.Max(15, TargetFPS);
            if (TargetFPS == _previousTargetFPS) return;

            Application.targetFrameRate = TargetFPS;
            _previousTargetFPS = TargetFPS;
        }
    }
}
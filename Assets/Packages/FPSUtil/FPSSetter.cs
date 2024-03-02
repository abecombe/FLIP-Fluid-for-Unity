using UnityEngine;

namespace Abecombe.FPSUtil
{
    public class FPSSetter : MonoBehaviour
    {
        [SerializeField]
        private int _targetFPS = 60;

        private void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _targetFPS;
        }
    }
}
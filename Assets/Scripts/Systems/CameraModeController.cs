using UnityEngine;

namespace SheriffAmericanLife
{
    public class CameraModeController : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform thirdPersonAnchor,firstPersonAnchor;
        [SerializeField] private bool firstPerson;
        public void ToggleCameraMode() { SetFirstPerson(!firstPerson); }
        public void SetFirstPerson(bool value) { firstPerson=value; if(viewCamera) viewCamera.fieldOfView=firstPerson?78:68; }
        private void LateUpdate()
        {
            if(!viewCamera) return;
            Transform target=firstPerson?firstPersonAnchor:thirdPersonAnchor;
            if(target) viewCamera.transform.SetPositionAndRotation(target.position,target.rotation);
        }
    }
}
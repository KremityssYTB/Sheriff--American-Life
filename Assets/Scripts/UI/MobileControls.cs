using UnityEngine;

namespace SheriffAmericanLife
{
    public class MobileControls : MonoBehaviour
    {
        [SerializeField] private ThirdPersonWalker walker;
        [SerializeField] private PatrolVehicleController vehicle;
        [SerializeField] private CameraModeController cameraMode;
        [SerializeField] private DispatchCallSystem dispatch;
        public void Move(Vector2 value) { if(walker) walker.SetMoveInput(value); }
        public void SprintDown() { if(walker) walker.SetSprint(true); }
        public void SprintUp() { if(walker) walker.SetSprint(false); }
        public void Drive(float throttle,float steering) { if(vehicle){vehicle.SetThrottle(throttle);vehicle.SetSteering(steering);} }
        public void ToggleCamera() { if(cameraMode) cameraMode.ToggleCameraMode(); }
        public void RequestCall() { if(dispatch) dispatch.GenerateTestCall(); }
        public void ClearCall() { if(dispatch) dispatch.ClearCall(); }
    }
}
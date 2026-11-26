using UnityEngine;

namespace SheriffAmericanLife
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonWalker : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float walkSpeed=4.8f, sprintMultiplier=1.55f, gravity=-22f;
        private CharacterController controller;
        private Vector2 input;
        private bool sprint;
        private float verticalSpeed;

        private void Awake() { controller=GetComponent<CharacterController>(); }
        public void SetCameraTransform(Transform value) { cameraTransform=value; }
        public void SetMovementEnabled(bool value) { enabled=value; input=Vector2.zero; sprint=false; }
        public void SetMoveInput(Vector2 value) { input=Vector2.ClampMagnitude(value,1f); }
        public void SetSprint(bool value) { sprint=value; }

        private void Update()
        {
            Vector3 f=cameraTransform?cameraTransform.forward:transform.forward;
            Vector3 r=cameraTransform?cameraTransform.right:transform.right;
            f.y=0; r.y=0; f.Normalize(); r.Normalize();
            Vector3 move=f*input.y+r*input.x;
            if(move.sqrMagnitude>.01f) transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),12f*Time.deltaTime);
            if(controller.isGrounded&&verticalSpeed<0) verticalSpeed=-2f;
            verticalSpeed+=gravity*Time.deltaTime;
            controller.Move((move*walkSpeed*(sprint?sprintMultiplier:1f)+Vector3.up*verticalSpeed)*Time.deltaTime);
        }
    }
}
using UnityEngine;

namespace SheriffAmericanLife.Legacy
{
    // Retained for the original greybox bootstrap. New gameplay should use Systems.CameraModeController.
    public class CameraModeController : MonoBehaviour
    {
        [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0, 3.2f, -6f);
        [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 1.65f, 0.15f);
        [SerializeField] private float smoothSpeed = 10f;
        private Transform target;
        private bool firstPerson;

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 offset = firstPerson ? firstPersonOffset : thirdPersonOffset;
            Vector3 desired = target.position + target.rotation * offset;
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                firstPerson ? target.rotation : Quaternion.LookRotation(target.position + Vector3.up - transform.position),
                smoothSpeed * Time.deltaTime);
            if (Input.GetKeyDown(KeyCode.V)) firstPerson = !firstPerson;
        }
    }
}
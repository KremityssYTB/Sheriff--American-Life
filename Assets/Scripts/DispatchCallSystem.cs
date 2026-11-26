using UnityEngine;

namespace SheriffAmericanLife
{
    public class DispatchCallSystem : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 callLocation = new Vector3(45, 0, 70);
        [SerializeField] private KeyCode acceptKey = KeyCode.J;
        private bool activeCall;

        private void Update()
        {
            if (Input.GetKeyDown(acceptKey))
            {
                activeCall = !activeCall;
                Debug.Log(activeCall ? "DISPATCH: Call accepted — respond to the marked location." :
                    "DISPATCH: Call cleared.");
            }
            if (activeCall && player != null && Vector3.Distance(player.position, callLocation) < 5f)
            {
                activeCall = false;
                Debug.Log("DISPATCH: On scene. Call marked arrived.");
            }
        }

        private void OnDrawGizmos()
        {
            if (!activeCall) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(callLocation, 3f);
        }
    }
}
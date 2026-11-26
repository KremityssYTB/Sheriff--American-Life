using UnityEngine;

namespace SheriffAmericanLife
{
    public class PatrolVehicleController : MonoBehaviour
    {
        [SerializeField] private float acceleration=18f,maxSpeed=28f,reverseSpeed=9f,steering=85f,drag=5f;
        private float throttle,steer,speed;
        public void SetThrottle(float value) { throttle=Mathf.Clamp(value,-1,1); }
        public void SetSteering(float value) { steer=Mathf.Clamp(value,-1,1); }
        private void Update()
        {
            float limit=throttle>=0?maxSpeed:reverseSpeed;
            speed=Mathf.MoveTowards(speed,throttle*limit,acceleration*Time.deltaTime);
            if(Mathf.Abs(throttle)<.05f) speed=Mathf.MoveTowards(speed,0,drag*Time.deltaTime);
            transform.Rotate(0,steer*steering*Mathf.Clamp01(Mathf.Abs(speed)/4f)*Mathf.Sign(speed)*Time.deltaTime,0);
            transform.position+=transform.forward*(speed*Time.deltaTime);
        }
    }
}
using System;
using UnityEngine;

namespace SheriffAmericanLife
{
    public class DispatchCallSystem : MonoBehaviour
    {
        [Serializable] public class DispatchCall { public string title; public string details; public Vector3 location; }
        [SerializeField] private GameObject waypointPrefab;
        public event Action<DispatchCall> CallReceived;
        public DispatchCall ActiveCall { get; private set; }
        private GameObject waypoint;
        private readonly string[] titles={"Suspicious vehicle","Noise complaint","Traffic collision","Shoplifting report","Welfare check"};

        public void GenerateTestCall()
        {
            Vector2 p=UnityEngine.Random.insideUnitCircle.normalized*UnityEngine.Random.Range(70,220);
            ActiveCall=new DispatchCall { title=titles[UnityEngine.Random.Range(0,titles.Length)],details="Respond and assess the situation.",location=new Vector3(p.x,0,p.y) };
            if(waypoint) Destroy(waypoint);
            waypoint=waypointPrefab?Instantiate(waypointPrefab,ActiveCall.location+Vector3.up*.25f,Quaternion.identity):GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            waypoint.name="Dispatch Waypoint"; waypoint.transform.position=ActiveCall.location+Vector3.up*.25f;
            CallReceived?.Invoke(ActiveCall);
        }
        public void ClearCall() { ActiveCall=null; if(waypoint) Destroy(waypoint); }
    }
}
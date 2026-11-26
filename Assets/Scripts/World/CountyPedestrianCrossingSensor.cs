using System.Collections.Generic;
using UnityEngine;

namespace SheriffAmericanLife
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CountyPedestrianCrossingSensor : MonoBehaviour
    {
        [SerializeField] private CountyFourWayStopController controller;
        private readonly HashSet<int> occupants = new HashSet<int>();
        private BoxCollider trigger;

        public void Configure(CountyFourWayStopController owner, Vector3 size)
        {
            controller = owner;
            trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = size;
        }

        private void Awake()
        {
            trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            CountyPedestrianCrossingAgent pedestrian = other.GetComponentInParent<CountyPedestrianCrossingAgent>();
            if (pedestrian == null) return;
            int id = pedestrian.GetInstanceID();
            if (occupants.Add(id) && controller != null) controller.SetPedestrianOccupancy(id, true);
        }

        private void OnTriggerExit(Collider other)
        {
            CountyPedestrianCrossingAgent pedestrian = other.GetComponentInParent<CountyPedestrianCrossingAgent>();
            if (pedestrian == null) return;
            int id = pedestrian.GetInstanceID();
            if (occupants.Remove(id) && controller != null) controller.SetPedestrianOccupancy(id, false);
        }

        private void OnDisable()
        {
            if (controller != null)
                foreach (int id in occupants) controller.SetPedestrianOccupancy(id, false);
            occupants.Clear();
        }
    }
}

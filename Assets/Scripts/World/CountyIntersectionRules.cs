using System;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Pure Florida-style intersection priority rules used by the four-way-stop controller.
    /// </summary>
    public static class CountyIntersectionRules
    {
        public enum Approach { North, East, South, West }
        public enum Movement { Straight, Left, Right }

        /// <summary>
        /// At a four-way stop, earlier arrival has priority. For effectively simultaneous
        /// arrivals, the vehicle approaching from the right has priority.
        /// Approach describes the side of the intersection from which the vehicle arrives.
        /// </summary>
        public static bool MustYieldAtFourWayStop(
            Approach myApproach, float myArrivalTime,
            Approach otherApproach, float otherArrivalTime,
            float simultaneousTolerance = 0.15f)
        {
            if (myApproach == otherApproach) return false;

            float difference = myArrivalTime - otherArrivalTime;
            if (Math.Abs(difference) > Math.Max(0f, simultaneousTolerance))
                return difference > 0f;

            return otherApproach == ApproachOnMyRight(myApproach);
        }

        /// <summary>
        /// A left-turning vehicle yields to oncoming vehicles proceeding straight
        /// or turning right. This does not replace checking pedestrians or occupancy.
        /// </summary>
        public static bool MustYieldForOncomingTraffic(
            Approach myApproach, Movement myMovement,
            Approach otherApproach, Movement otherMovement)
        {
            if (myMovement != Movement.Left) return false;
            if (otherApproach != Opposite(myApproach)) return false;

            return otherMovement == Movement.Straight ||
                   otherMovement == Movement.Right;
        }

        /// <summary>Vehicles must not enter while a pedestrian occupies the crossing path.</summary>
        public static bool MustYieldForPedestrian(bool pedestrianInConflictCrosswalk)
        {
            return pedestrianInConflictCrosswalk;
        }

        public static Approach ApproachOnMyRight(Approach approach)
        {
            switch (approach)
            {
                case Approach.North: return Approach.West;
                case Approach.East: return Approach.North;
                case Approach.South: return Approach.East;
                default: return Approach.South;
            }
        }

        public static Approach Opposite(Approach approach)
        {
            switch (approach)
            {
                case Approach.North: return Approach.South;
                case Approach.East: return Approach.West;
                case Approach.South: return Approach.North;
                default: return Approach.East;
            }
        }
    }
}

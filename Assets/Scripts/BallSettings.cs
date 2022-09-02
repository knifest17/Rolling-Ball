using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "BallSettings", menuName = "BallSettings", order = 0)]
    public class BallSettings : ScriptableObject
    {
        [SerializeField]
        float
            speedMultiplier,
            friction,
            squashSpeed,
            squashSpeedMult,
            squashRollingMult,
            minSquashSpeed,
            maxSquash;

        public float SpeedMultiplier => speedMultiplier;
        public float Friction => friction;
        public float SquashSpeed => squashSpeed;
        public float SquashSpeedMult => squashSpeedMult;
        public float SquashRollingMult => squashRollingMult;
        public float MinSquashSpeed => minSquashSpeed;
        public float MaxSquash => maxSquash;
    }
}
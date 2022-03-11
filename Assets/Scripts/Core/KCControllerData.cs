using UnityEngine;

namespace EC.Core {
    [CreateAssetMenu(fileName = "newKCControllerData", menuName = "Data/KCControllerData")]
    public class KCControllerData : ScriptableObject {
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;

        [Header("Jumping")] 
        public int jumpCount = 1;
        public bool AllowJumpingWhenSliding = false;
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;

        [Header("Rolling")] 
        public AnimationCurve RollSpeedCurve;
        public float RollDuration = 0.3f;
        public float RollSpeedPeak = 10f;

        [Header("Misc")]
        public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public float CrouchedCapsuleHeight = 1f;
    }
}
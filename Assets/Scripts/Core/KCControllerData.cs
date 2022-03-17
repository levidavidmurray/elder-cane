using Animancer;
using UnityEngine;

namespace EC.Core {
    [CreateAssetMenu(fileName = "newKCControllerData", menuName = "Data/KCControllerData")]
    public class KCControllerData : ScriptableObject {
        [Foldout("Stable Movement", styled = true)]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public float RotationSpeed = 10f;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;
        public AnimationCurve VelocityMagnitudeSpeedCurve;

        [Foldout("Air Movement", styled = true)]
        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;

        [Foldout("Jumping", styled = true)]
        public int jumpCount = 1;
        public bool AllowJumpingWhenSliding = false;
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float MaxJumpHeight = 1f;
        public float MaxJumpTime = 0.5f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;
        public float JumpCooldown = 0.2f;

        [Foldout("Rolling", styled = true)]
        public AnimationCurve RollSpeedCurve;
        public AnimationCurve RollAnimSpeedCurve;
        public float RollDuration = 0.3f;
        public float RollAnimSpeed = 1f;
        public float MaxRollSpeed = 10f;
        public float RollCooldown = 0.2f;
        
        [Foldout("Backflip", styled = true)]
        public float MaxFlipSpeed = 10f;
        public float FlipAnimSpeed = 2f;

        [Foldout("Misc", styled = true)]
        public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public float CrouchedCapsuleHeight = 1f;

        [Foldout("Animations", styled = true)] 
        public AnimationClip IdleAnimation;
        public LinearMixerTransition MoveAnimation;
        public AnimationClip JumpAnimation;
        public AnimationClip FallAnimation;
        public AnimationClip LandAnimation;
    }
}
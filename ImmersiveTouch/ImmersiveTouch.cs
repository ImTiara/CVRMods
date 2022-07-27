using HarmonyLib;
using MelonLoader;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;
using ABI_RC.Core.Player;
using ABI.CCK.Components;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(ImmersiveTouch.ImmersiveTouch), "ImmersiveTouch", "1.0.0", "ImTiara", "https://github.com/ImTiara/CVRMods")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace ImmersiveTouch
{
    public class ImmersiveTouch : MelonMod
    {
        public const float ORTHOGRAPHIC_SIZE_MOD = 4.0f;
        public const float CLIP_PLANE_MOD = 40.0f;

        public static MelonPreferences_Entry<bool> ENABLE;
        public static MelonPreferences_Entry<float> HAPTIC_STRENGTH;
        public static MelonPreferences_Entry<float> HAPTIC_SENSITIVITY;
        public static MelonPreferences_Entry<bool> DOUBLE_SIDED;
        public static MelonPreferences_Entry<bool> COLLIDE_PLAYERS;
        public static MelonPreferences_Entry<bool> COLLIDE_WORLD;
        public static MelonPreferences_Entry<float> RENDER_INTERVAL;

        public static float m_HapticDistance = 0.015f;

        public static CameraHaptic m_LeftCameraHaptic;
        public static CameraHaptic m_RightCameraHaptic;
        
        public static CameraHaptic m_LeftCameraHapticDouble;
        public static CameraHaptic m_RightCameraHapticDouble;

        private static FieldInfo _avatarDescriptor;
        public static CVRAvatar CurrentAvatar
        {
            get
            {
                return (CVRAvatar)_avatarDescriptor.GetValue(PlayerSetup.Instance);
            }
        }

        public override void OnApplicationStart()
        {
            var category = MelonPreferences.CreateCategory("ImmersiveTouch", "Immersive Touch");
            ENABLE = category.CreateEntry("Enabled", true, "Enable Immersive Touch");
            HAPTIC_STRENGTH = category.CreateEntry("VibrationStrength", 750.0f, "Vibration Strength");
            HAPTIC_SENSITIVITY = category.CreateEntry("StrokeSensitivity", 250.0f, "Stroke Sensitivity");
            DOUBLE_SIDED = category.CreateEntry("DoubleSidedCollision", false, "Double Sided Collision");
            COLLIDE_PLAYERS = category.CreateEntry("PlayerCollision", true, "Player Collision");
            COLLIDE_WORLD = category.CreateEntry("WorldCollision", true, "World Collision");
            RENDER_INTERVAL = category.CreateEntry("RenderInterval", 0.09f, "Render Interval (Lower value = accurate collisions but more GPU usage)");

            ENABLE.OnValueChanged += (editedValue, defaultValue) =>
            {
                SetupAvatar(true);
            };

            HAPTIC_SENSITIVITY.OnValueChanged += (editedValue, defaultValue)
                => SetupAvatar(false);

            DOUBLE_SIDED.OnValueChanged += (editedValue, defaultValue)
                => SetupAvatar(false);

            COLLIDE_PLAYERS.OnValueChanged += (editedValue, defaultValue)
                => UpdateCameraCullingMasks();

            COLLIDE_WORLD.OnValueChanged += (editedValue, defaultValue)
                => UpdateCameraCullingMasks();

            _avatarDescriptor = typeof(PlayerSetup).GetField("_avatarDescriptor", BindingFlags.NonPublic | BindingFlags.Instance);

            HarmonyInstance.Patch(typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)), null,
                new HarmonyMethod(typeof(ImmersiveTouch).GetMethod("OnSetupAvatar", BindingFlags.Public | BindingFlags.Static)));
        }

        public static void OnSetupAvatar()
        {
            try
            {
                SetupAvatar(true);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error checking when avatar changed:\n{e}");
            }
        }

        public static void SetupAvatar(bool showMessages)
        {
            try
            {
                DestroyImmersiveTouch();

                if (!ENABLE.Value) return;

                XRHaptics.Register();

                if (XRHaptics.RightController == null || XRHaptics.LeftController == null)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot send haptics because no VR controllers was found.");
                    return;
                }

                Animator animator = PlayerSetup.Instance._animator;
                if (animator == null || !animator.isHuman)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot use this avatar because no valid animator was found.");
                    return;
                }
                
                float viewHeight = CurrentAvatar.viewPosition.y;
                m_HapticDistance = viewHeight / HAPTIC_SENSITIVITY.Value;

                Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

                if (leftHand == null || rightHand == null)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot use this avatar because the Left/Right hand bone are missing.");
                    return;
                }

                Transform leftMiddleProximal = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                Transform rightMiddleProximal = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

                if (leftMiddleProximal == null || rightMiddleProximal == null)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot use this avatar because the Left/Right Middle Proximal finger bone are missing.");
                    return;
                }

                Transform leftMiddleDistal = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
                Transform rightMiddleDistal = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

                if (leftMiddleDistal == null || rightMiddleDistal == null)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot use this avatar because the Left/Right Middle Distal finger bone are missing.");
                    return;
                }

                m_LeftCameraHaptic = ConfigureCameraHaptic(XRHaptics.LeftController, leftHand, leftMiddleProximal, leftMiddleDistal, viewHeight);
                m_RightCameraHaptic = ConfigureCameraHaptic(XRHaptics.RightController, rightHand, rightMiddleProximal, rightMiddleDistal, viewHeight);

                if (DOUBLE_SIDED.Value)
                {
                    m_LeftCameraHapticDouble = ConfigureCameraHaptic(XRHaptics.LeftController, leftHand, leftMiddleProximal, leftMiddleDistal, viewHeight);
                    m_RightCameraHapticDouble = ConfigureCameraHaptic(XRHaptics.RightController, rightHand, rightMiddleProximal, rightMiddleDistal, viewHeight);

                    m_LeftCameraHapticDouble.transform.localEulerAngles = new Vector3(0, 0, 0);
                    m_RightCameraHapticDouble.transform.localEulerAngles = new Vector3(0, 0, 0);
                }

                UpdateCameraCullingMasks();

                if (showMessages) MelonLogger.Msg("Immersive Touch is now active on this avatar!");
            }
            catch (Exception e)
            {
                if (showMessages) MelonLogger.Error($"Error when setting up avatar: {e}");
            }
        }

        public static CameraHaptic ConfigureCameraHaptic(InputDevice inputDevice, Transform hand, Transform middleProximal, Transform middleDistal, float viewHeight)
        {
            if (inputDevice == null) return null;
            
            Transform cameraHapticObject = new GameObject().transform;
            cameraHapticObject.parent = middleProximal;
            cameraHapticObject.localPosition = Vector3.zero;
            cameraHapticObject.parent = hand;
            cameraHapticObject.localEulerAngles = new Vector3(0, 180, 0);

            CameraHaptic cameraHaptic = cameraHapticObject.gameObject.AddComponent<CameraHaptic>();
            cameraHaptic.inputDevice = inputDevice;
            cameraHaptic.camera.orthographicSize = Vector3.Distance(hand.position, middleDistal.position) / ORTHOGRAPHIC_SIZE_MOD;
            cameraHaptic.camera.nearClipPlane = -(viewHeight / CLIP_PLANE_MOD);
            cameraHaptic.camera.farClipPlane = viewHeight / CLIP_PLANE_MOD;

            return cameraHaptic;
        }

        public static void DestroyImmersiveTouch()
        {
            if (m_LeftCameraHaptic != null)
            {
                Object.Destroy(m_LeftCameraHaptic.gameObject);
            }

            if (m_RightCameraHaptic != null)
            {
                Object.Destroy(m_RightCameraHaptic.gameObject);
            }

            if (!DOUBLE_SIDED.Value)
            {
                if (m_LeftCameraHapticDouble != null)
                {
                    Object.Destroy(m_LeftCameraHapticDouble.gameObject);
                }

                if (m_RightCameraHapticDouble != null)
                {
                    Object.Destroy(m_RightCameraHapticDouble.gameObject);
                }
            }
        }

        public static void UpdateCameraCullingMasks()
        {
            try
            {
                int result = CalculateLayerMask(COLLIDE_WORLD.Value, COLLIDE_PLAYERS.Value);

                if (m_LeftCameraHaptic != null) m_LeftCameraHaptic.camera.cullingMask = result;
                if (m_RightCameraHaptic != null) m_RightCameraHaptic.camera.cullingMask = result;

                if (DOUBLE_SIDED.Value)
                {
                    if (m_LeftCameraHapticDouble != null) m_LeftCameraHapticDouble.camera.cullingMask = result;
                    if (m_RightCameraHapticDouble != null) m_RightCameraHapticDouble.camera.cullingMask = result;
                }
            }
            catch(Exception e)
            {
                MelonLogger.Error($"Error updating culling masks: {e}");
            }
        }

        public static int CalculateLayerMask(bool allowWorld, bool allowPlayers)
            => (allowWorld ? (1 << 0) : 0) | (allowPlayers ? (1 << 10) : 0);
    }
}
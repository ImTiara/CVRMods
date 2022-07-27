using System.Collections.Generic;
using UnityEngine.XR;

namespace ImmersiveTouch
{
    internal class XRHaptics
    {
        public static List<InputDevice> m_InputDevices = new();

        public static InputDevice LeftController;
        public static InputDevice RightController;
        
        public static void Register()
        {
            m_InputDevices.Clear();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand, m_InputDevices);

            foreach (var inputDevice in m_InputDevices)
            {
                switch (inputDevice.characteristics)
                {
                    case InputDeviceCharacteristics.HeldInHand |
                    InputDeviceCharacteristics.TrackedDevice |
                    InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left: LeftController = inputDevice; break;
                        
                    case InputDeviceCharacteristics.HeldInHand |
                    InputDeviceCharacteristics.TrackedDevice |
                    InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right: RightController = inputDevice; break;
                }
            }
        }

        public static void SendHaptic(InputDevice inputDevice, float duration)
        {
            inputDevice.SendHapticImpulse(0u, 1, duration);
        }
    }
}

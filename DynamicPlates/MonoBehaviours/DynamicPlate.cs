using ABI_RC.Core.Player;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DynamicPlates
{
    public class DynamicPlate : MonoBehaviour
    {
        public static Dictionary<PuppetMaster, DynamicPlate> dynamicPlates = new();
        public static FieldInfo _animatorField = typeof(PuppetMaster).GetField("_animator", BindingFlags.NonPublic | BindingFlags.Instance);

        public PuppetMaster puppetMaster;
        public Transform target;

        public void Update()
        {
            float scale = Mathf.Clamp(Vector3.Distance(Camera.main.transform.position, transform.position) / 4, 0.1f, 2.0f);

            transform.localScale = new Vector3(scale, scale, scale);

            transform.position = target != null ? new Vector3(target.position.x, target.position.y + DynamicPlates.HEIGHT.Value, target.position.z) :
                new Vector3(puppetMaster.transform.position.x, puppetMaster.transform.position.y + 1.5f, puppetMaster.transform.position.z);
        }

        public void OnAvatarChanged()
        {
            Animator animator = (Animator)_animatorField.GetValue(puppetMaster);

            if (animator != null && animator.isHuman)
            {
                target = animator.GetBoneTransform(HumanBodyBones.Head);
            }
            else
            {
                target = puppetMaster.voicePosition?.transform;
            }
        }

        public void OnDestroy()
        {
            try
            {
                dynamicPlates.Remove(puppetMaster);
            }
            catch (Exception e)
            {
                MelonLogger.Msg(e);
            }
        }
    }
}

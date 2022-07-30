using ABI_RC.Core.Savior;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(GestureIndicator.GestureIndicator), "GestureIndicator", "1.0.1", "ImTiara", "https://github.com/ImTiara/CVRMods")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace GestureIndicator
{
    public class GestureIndicator : MelonMod
    {
        public static MelonPreferences_Entry<bool> ENABLE;
        public static MelonPreferences_Entry<float> X_POS;
        public static MelonPreferences_Entry<float> Y_POS;
        public static MelonPreferences_Entry<float> DISTANCE;
        public static MelonPreferences_Entry<float> SIZE;
        public static MelonPreferences_Entry<float> OPACITY;
        public static MelonPreferences_Entry<string> LEFT_COLOR;
        public static MelonPreferences_Entry<string> RIGHT_COLOR;

        public static GameObject m_Root;

        public static RectTransform m_LeftRootRect;
        public static RectTransform m_RightRootRect;

        public static RectTransform m_LeftImageRect;
        public static RectTransform m_RightImageRect;

        public static Image m_LeftImage;
        public static Image m_RightImage;

        public static readonly Dictionary<float, Sprite> elements = new()
        {
            { -1, null },
            { 0, null },
            { 1, null },
            { 2, null },
            { 3, null },
            { 4, null },
            { 5, null },
            { 6, null },
        };

        public override void OnApplicationStart()
        {
            AssetLoader.Load();

            elements[-1] = AssetLoader.openHand;
            elements[0] = AssetLoader._null;
            elements[1] = AssetLoader.fist;
            elements[2] = AssetLoader.thumbsUp;
            elements[3] = AssetLoader.fingerGun;
            elements[4] = AssetLoader.point;
            elements[5] = AssetLoader.victory;
            elements[6] = AssetLoader.rockAndRoll;

            var category = MelonPreferences.CreateCategory("GestureIndicator", "Gesture Indicator");
            ENABLE = category.CreateEntry("Enabled", true, "Enable Gesture Indicator");
            X_POS = category.CreateEntry("XPos", 17.0f, "X Position");
            Y_POS = category.CreateEntry("YPos", -22.0f, "Y Position");
            DISTANCE = category.CreateEntry("Distance", 1750.0f, "Distance");
            SIZE = category.CreateEntry("Size", 175.0f, "Size");
            OPACITY = category.CreateEntry("Opacity", 1.0f, "Opacity");
            LEFT_COLOR = category.CreateEntry("LeftColor", "#00FFFF", "Left Color");
            RIGHT_COLOR = category.CreateEntry("RightColor", "#00FFFF", "Right Color");

            ENABLE.OnValueChanged += (editedValue, defaultValue) => ToggleIndicators(ENABLE.Value);

            X_POS.OnValueChanged += (editedValue, defaultValue) => SetPosition(new Vector2(X_POS.Value, Y_POS.Value));
            Y_POS.OnValueChanged += (editedValue, defaultValue) => SetPosition(new Vector2(X_POS.Value, Y_POS.Value));
            
            DISTANCE.OnValueChanged += (editedValue, defaultValue) => SetDistance(DISTANCE.Value);
            SIZE.OnValueChanged += (editedValue, defaultValue) => SetSize(SIZE.Value);
            
            OPACITY.OnValueChanged += (editedValue, defaultValue) => RefreshColors();
            LEFT_COLOR.OnValueChanged += (editedValue, defaultValue) => RefreshColors();
            RIGHT_COLOR.OnValueChanged += (editedValue, defaultValue) => RefreshColors();
            
            MelonCoroutines.Start(WaitForRecognizer());
        }

        public static IEnumerator CheckGesture()
        {
            while (ENABLE.Value)
            {
                try
                {
                    if (CVRInputManager.Instance.gestureLeft > 0.05f && CVRInputManager.Instance.gestureLeft < 1.95f)
                    {
                        m_LeftImage.sprite = elements[1];
                    }
                    else
                    {
                        m_LeftImage.sprite = elements[CVRInputManager.Instance.gestureLeft];
                    }

                    if (CVRInputManager.Instance.gestureRight > 0.05f && CVRInputManager.Instance.gestureRight < 1.95f)
                    {
                        m_RightImage.sprite = elements[1];
                    }
                    else
                    {
                        m_RightImage.sprite = elements[CVRInputManager.Instance.gestureRight];
                    }
                }
                catch (Exception e) { MelonLogger.Error("Error checking gestures: " + e); }

                yield return new WaitForSeconds(.1f);
            }
        }

        public static IEnumerator WaitForRecognizer()
        {
            while (CVRGestureRecognizer.Instance == null) yield return null;

            ToggleIndicators(ENABLE.Value);
        }

        public static void ToggleIndicators(bool enable)
        {
            if (m_Root == null)
            {
                m_Root = GameObject.Instantiate(AssetLoader.template, Camera.main.transform);
                m_Root.transform.localPosition = Vector3.zero;
                m_Root.transform.localRotation = Quaternion.identity;
                
                m_LeftRootRect = m_Root.transform.Find("LeftRoot").GetComponent<RectTransform>();
                m_RightRootRect = m_Root.transform.Find("RightRoot").GetComponent<RectTransform>();

                m_LeftImage = m_LeftRootRect.GetComponentInChildren<Image>();
                m_RightImage = m_RightRootRect.GetComponentInChildren<Image>();

                m_LeftImageRect = m_LeftImage.GetComponent<RectTransform>();
                m_RightImageRect = m_RightImage.GetComponent<RectTransform>();

                m_LeftImage.sprite = AssetLoader._null;
                m_RightImage.sprite = AssetLoader._null;
            }
            
            m_Root.SetActive(enable);

            if (enable)
            {
                SetPosition(new Vector2(X_POS.Value, Y_POS.Value));
                SetDistance(DISTANCE.Value);
                SetSize(SIZE.Value);
                RefreshColors();

                MelonCoroutines.Start(CheckGesture());
            }
        }

        public static void SetPosition(Vector2 vec)
        {
            m_LeftRootRect.localEulerAngles = new Vector3(-vec.y, -vec.x, 0);
            m_RightRootRect.localEulerAngles = new Vector3(-vec.y, vec.x, 0);
        }

        public static void SetDistance(float dist)
        {
            m_LeftImageRect.localPosition = new Vector3(m_LeftImageRect.localPosition.x, m_LeftImageRect.localPosition.y, dist);
            m_RightImageRect.localPosition = new Vector3(m_RightImageRect.localPosition.x, m_RightImageRect.localPosition.y, dist);
        }

        public static void SetSize(float size)
        {
            m_LeftImageRect.sizeDelta = new Vector2(size, size);
            m_RightImageRect.sizeDelta = new Vector2(size, size);
        }

        public static Color HexToColor(string hex)
        {
            hex = !hex.StartsWith("#") ? "#" + hex : hex;
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        public static void RefreshColors()
        {
            Color color = HexToColor(LEFT_COLOR.Value);
            color.a = OPACITY.Value;
            m_LeftImage.color = color;

            color = HexToColor(RIGHT_COLOR.Value);
            color.a = OPACITY.Value;
            m_RightImage.color = color;
        }
    }
}
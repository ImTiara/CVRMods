using ABI_RC.Core.Savior;
using ABI_RC.Systems.InputManagement;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(GestureIndicator.GestureIndicator), "GestureIndicator", "1.0.3", "ImTiara", "https://github.com/ImTiara/CVRMods")]
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
        public static MelonPreferences_Entry<bool> ICON_FADE;
        public static MelonPreferences_Entry<int> FADE_TIME;
        public static MelonPreferences_Entry<float> FADE_END_OPACITY;

        public static GameObject m_Root;

        public static RectTransform m_LeftRootRect;
        public static RectTransform m_RightRootRect;

        public static RectTransform m_LeftImageRect;
        public static RectTransform m_RightImageRect;

        public static Image m_LeftImage;
        public static Image m_RightImage;

        public static Color m_LeftColor;
        public static Color m_RightColor;

        public static float m_RightGesture_time = 0f;
        public static float m_LeftGesture_time = 0f;

        public static int m_RightGesture_last = 0;
        public static int m_LeftGesture_last = 0;

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
            LEFT_COLOR = category.CreateEntry("LeftColor", "#00FFFF", "Left Color");
            RIGHT_COLOR = category.CreateEntry("RightColor", "#00FFFF", "Right Color");
            OPACITY = category.CreateEntry("Opacity", 1.0f, "Opacity");
            ICON_FADE = category.CreateEntry("Icon_Fade", true, "Fade Gesture Icons after change");
            FADE_TIME = category.CreateEntry("Fade_Time", 5, "Fade time");
            FADE_END_OPACITY = category.CreateEntry("Fade_End_Opacity", 0.1f, "Faded Opacity");

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
                    if (CVRInputManager.Instance.gestureLeft < 0f || CVRInputManager.Instance.gestureLeft > 0.05f)
                    {
                        if (CVRInputManager.Instance.gestureLeft > 0.05f && CVRInputManager.Instance.gestureLeft < 1.95f)
                        {
                            m_LeftImage.sprite = elements[1];

                            if (m_LeftGesture_last != 1)
                            {
                                m_LeftGesture_last = 1;
                                m_LeftGesture_time = Time.time + FADE_TIME.Value;
                            }
                        }
                        else
                        {
                            m_LeftImage.sprite = elements[CVRInputManager.Instance.gestureLeft];

                            if (m_LeftGesture_last != (int)CVRInputManager.Instance.gestureLeft)
                            {
                                m_LeftGesture_last = (int)CVRInputManager.Instance.gestureLeft;
                                m_LeftGesture_time = Time.time + FADE_TIME.Value;
                            }
                        }
                        SetColors();
                    }
                    else
                    {
                        m_LeftImage.sprite = elements[0];
                        m_LeftGesture_last = 0;
                    }


                    if (CVRInputManager.Instance.gestureRight < 0f || CVRInputManager.Instance.gestureRight > 0.05f)
                    {
                        if (CVRInputManager.Instance.gestureRight > 0.05f && CVRInputManager.Instance.gestureRight < 1.95f)
                        {
                            m_RightImage.sprite = elements[1];

                            if (m_RightGesture_last != 1)
                            {
                                m_RightGesture_last = 1;
                                m_RightGesture_time = Time.time + FADE_TIME.Value;
                            }
                        }
                        else
                        {
                            m_RightImage.sprite = elements[CVRInputManager.Instance.gestureRight];

                            if (m_RightGesture_last != (int)CVRInputManager.Instance.gestureRight)
                            {
                                m_RightGesture_last = (int)CVRInputManager.Instance.gestureRight;
                                m_RightGesture_time = Time.time + FADE_TIME.Value;
                            }
                        }
                        SetColors();
                    }
                    else
                    { 
                        m_RightImage.sprite = elements[0];
                        m_RightGesture_last = 0;
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
                m_Root.layer = 15;

                m_LeftRootRect = m_Root.transform.Find("LeftRoot").GetComponent<RectTransform>();
                m_RightRootRect = m_Root.transform.Find("RightRoot").GetComponent<RectTransform>();

                m_LeftImage = m_LeftRootRect.GetComponentInChildren<Image>();
                m_RightImage = m_RightRootRect.GetComponentInChildren<Image>();

                m_LeftImage.material = new Material(AssetLoader.gestureShader);
                m_RightImage.material = new Material(AssetLoader.gestureShader);

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
            m_LeftColor = HexToColor(LEFT_COLOR.Value);
            m_RightColor = HexToColor(RIGHT_COLOR.Value);
            SetColors();
        }

        public static void SetColors()
        {
            var opacity_L = ICON_FADE.Value ? Mathf.Max( ((m_LeftGesture_time - Time.time) / FADE_TIME.Value * OPACITY.Value), FADE_END_OPACITY.Value)  : OPACITY.Value;
            var opacity_R = ICON_FADE.Value ? Mathf.Max( ((m_RightGesture_time - Time.time) / FADE_TIME.Value * OPACITY.Value), FADE_END_OPACITY.Value) : OPACITY.Value;

            m_LeftColor.a = opacity_L;
            m_LeftImage.color = m_LeftColor;

            m_RightColor.a = opacity_R;
            m_RightImage.color = m_RightColor;
            //MelonLogger.Msg($"opacity_L:{opacity_L}, opacity_R:{opacity_R}");
        }
    }
}
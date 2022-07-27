using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace ImmersiveTouch
{
    public class CameraHaptic : MonoBehaviour
    {
        public RenderTexture renderTexture;
        public Camera camera;
        public InputDevice inputDevice;

        public bool isColliding;
        public Vector3 lastCollisionVector;
        
        private Action<AsyncGPUReadbackRequest> asyncGPUReadbackRequest;
        private bool _render = true;

        public void Awake()
        {
            renderTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };
            renderTexture.Create();
            
            camera = gameObject.AddComponent<Camera>();
            camera.depth = -5;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.nearClipPlane = -0.1f;
            camera.farClipPlane = 0.1f;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.targetTexture = renderTexture;
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.enabled = false;

            StartCoroutine(Renderer());

            asyncGPUReadbackRequest = new Action<AsyncGPUReadbackRequest>((readback) =>
            {
                isColliding = readback.GetData<Color32>(0)[0] != Color.black;
            });
        }

        public IEnumerator Renderer()
        {
            while (_render)
            {
                camera.enabled = true;
                yield return new WaitForSeconds(ImmersiveTouch.RENDER_INTERVAL.Value);
            }
        }

        public void OnRenderImage(RenderTexture source, RenderTexture _)
        {
            camera.enabled = false;
            AsyncGPUReadback.Request(source, 0, asyncGPUReadbackRequest);
        }

        public void Update()
        {
            if (isColliding && Vector3.Distance(transform.position, lastCollisionVector) > ImmersiveTouch.m_HapticDistance)
            {
                lastCollisionVector = transform.position;

                XRHaptics.SendHaptic(inputDevice, (ushort)ImmersiveTouch.HAPTIC_STRENGTH.Value);
            }
        }

        public void OnDestroy()
        {
            _render = false;

            renderTexture.Release();
            renderTexture.DiscardContents();
        }
    }
}

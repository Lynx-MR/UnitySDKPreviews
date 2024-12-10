using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ProjectCameraFeed_Mono : ImageProcessBase
    {
        [SerializeField] private Material m_material = null;

        private static Action m_action = null; // Action for UI thread
        private Texture2D m_textureLeft = null; // Texture to handle camera texture
        private Texture2D m_textureRight = null; // Texture to handle camera texture

        private GameObject DisplayPlane;


        public override void StartVideoCapture()
        {
            LynxCaptureAPI.onRGBFrames += OnCallbackFrame;

            // Start video capture at given FPS
            if (!LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, 90))
                Debug.LogError("Failed to start camera");
            else
                DisplayPlane = LynxCaptureProjection.SetupMonoUndistortMesh(m_material, true);

            DisplayPlane.transform.localPosition = Vector3.zero;
            DisplayPlane.transform.localScale = Vector3.one * 100;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                DisplayPlane.SetActive(false);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                DisplayPlane.SetActive(true);
        }




        private void LateUpdate()
        {
            // Fire event from callback in main thread
            if (m_action != null)
            {
                m_action.Invoke();
                m_action = null;
            }
        }

        private void OnCallbackFrame(LynxFrameInfo frameInfo)
        {
            if (m_action == null)
            {

                if (m_textureLeft == null)
                {
                    m_action = () =>
                    {
                        m_textureLeft = new Texture2D((int)frameInfo.width, (int)(frameInfo.height* 1.5), TextureFormat.R8, false, false);
                        
                        m_textureLeft.filterMode = FilterMode.Point;

                        m_material.SetTexture("_CameraTexture", m_textureLeft);

                    };
                }
                else
                {
                    m_action = () =>
                    {
                        m_textureLeft.LoadRawTextureData(frameInfo.leftEyeBuffer, (int)frameInfo.bufferSize);
                        m_textureLeft.Apply();
                    };
                }
            }
        }
    }
}
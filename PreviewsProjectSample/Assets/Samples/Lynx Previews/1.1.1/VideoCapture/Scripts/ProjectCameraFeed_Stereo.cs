using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ProjectCameraFeed_Stereo : ImageProcessBase
    {
        [SerializeField] private Material m_materialLeft = null;
        [SerializeField] private Material m_materialRight = null;

        private static Action m_action = null; // Action for UI thread
        private Texture2D m_textureLeft = null; // Texture to handle camera texture
        private Texture2D m_textureRight = null; // Texture to handle camera texture

        private GameObject[] DisplayPlanes;


        public override void StartVideoCapture()
        {
            LynxCaptureAPI.onRGBFrames += OnCallbackFrame;

            // Start video capture at given FPS
            if (!LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, 90))
                Debug.LogError("Failed to start camera");
            else
                DisplayPlanes = LynxCaptureProjection.SetupStereoUndistortMesh(m_materialLeft, m_materialRight);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                DisplayPlanes[0].SetActive(!DisplayPlanes[0].activeSelf);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                DisplayPlanes[1].SetActive(!DisplayPlanes[1].activeSelf);
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
                        m_textureRight = new Texture2D((int)frameInfo.width, (int)(frameInfo.height* 1.5), TextureFormat.R8, false, false);

                        m_textureLeft.filterMode = FilterMode.Point;
                        m_textureRight.filterMode = FilterMode.Point;

                        m_materialLeft.SetTexture("_CameraTexture", m_textureLeft);
                        m_materialLeft.SetFloat("_IsRightEye", 0);
                        m_materialRight.SetTexture("_CameraTexture", m_textureRight);
                        m_materialRight.SetFloat("_IsRightEye", 1);
                        Debug.Log("RGB textures set on material");

                    };
                }
                else
                {
                    m_action = () =>
                    {
                        m_textureLeft.LoadRawTextureData(frameInfo.leftEyeBuffer, (int)frameInfo.bufferSize);
                        m_textureLeft.Apply();

                        m_textureRight.LoadRawTextureData(frameInfo.rightEyeBuffer, (int)frameInfo.bufferSize);
                        m_textureRight.Apply();
                    };
                }
            }
        }
    }
}
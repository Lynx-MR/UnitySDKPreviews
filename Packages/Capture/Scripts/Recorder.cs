/**
 * @file Recorder.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Record AR + VR and save it on the headset under /sdcard/DCIM/Lynx/ScreenAndVideoShots/video_YYYY-MM-D_HH-MM-SS.mp4.
 */

using System;
using UnityEngine;
using UnityEngine.Rendering;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx.Capture
{
    public class Recorder : ImageProcessBase
    {
        #region INSPECTOR
        [Tooltip("Material used to display the capture")]
        [SerializeField] Material m_material = null;
        #endregion


        private const int RECORD_FPS = 90;
        private const int RECORD_WIDTH = 768;
        private const int RECORD_HEIGHT = 702;

        private VideoEncoderNatifMng videoEncoderNatifMng = new VideoEncoderNatifMng();

        private static Action<byte[]> m_action = null; // Action for UI thread
        private Texture2D m_textureAR = null; // Texture to handle camera texture
        private Texture2D m_textureVR = null; // Texture to handle camera texture
        private Texture2D m_output = null; // Blended output texture
        private byte[] yBytes; // Bytes from converted capture

        #region UNITY API
        public override void StartVideoCapture()
        {
            // Associate callback for AR frame capture
            LynxCaptureAPI.onRGBFrames += OnCallbackFrame;
            LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, RECORD_FPS);
        }

        private void LateUpdate()
        {
            // Retrieve VR part from Unity
            if (SystemInfo.supportsAsyncGPUReadback)
            {
                RenderTexture rt = RenderTexture.GetTemporary(RECORD_WIDTH, RECORD_HEIGHT, 32);
                Camera.main.targetTexture = rt;
                Camera.main.Render();
                AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32, (AsyncGPUReadbackRequest request) =>
                {
                    // Fire event from callback in main thread
                    if (m_action != null)
                    {
                        m_action.Invoke(request.GetData<byte>().ToArray());
                        m_action = null;
                    }
                });

                Camera.main.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            StopRecord();
        }
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Call this method to start recording.
        /// </summary>
        public void StartRecord()
        {
            if (SystemInfo.supportsAsyncGPUReadback)
            {
                videoEncoderNatifMng.RecordVideo("",
                        RECORD_WIDTH,
                        RECORD_HEIGHT,
                        25,
                        2000000,
                        false);
            }
            else
                Debug.LogError("Cannot start recording. Does not support GPU readback.");
        }

        /// <summary>
        /// Call this method to stop recording.
        /// </summary>
        public void StopRecord()
        {
            videoEncoderNatifMng.StopRecord();
        }
        #endregion



        /// <summary>
        /// Create texturexs at first frame.
        /// Then, update textures with frame data.
        /// </summary>
        /// <param name="frameInfo">Struct holding info and buffers for each eye</param>
        private void OnCallbackFrame(LynxFrameInfo frameInfo)
        {
            if (m_action == null)
            {

                if (m_textureAR == null)
                {
                    m_action = (rgbaBytes) =>
                    {
                        m_output = new Texture2D(RECORD_WIDTH, (int)(RECORD_HEIGHT * 1.5), TextureFormat.R8, false, false);
                        yBytes = new byte[(int)(RECORD_WIDTH * RECORD_HEIGHT * 1.5)];

                        m_textureAR = new Texture2D(RECORD_WIDTH, (int)(RECORD_HEIGHT * 1.5), TextureFormat.R8, false, false);
                        m_textureAR.filterMode = FilterMode.Point;
                        m_material.mainTexture = m_textureAR;

                        m_textureVR = new Texture2D(RECORD_WIDTH, RECORD_HEIGHT, TextureFormat.RGBA32, false, false);
                        m_material.SetTexture("_TexVR", m_textureVR);
                    };
                }
                else
                {
                    m_action = (rgbaBytes) =>
                    {
                        // Blend AR + VR in same image
                        try
                        {
                            // Convert YUV pointer to RGB buffer
                            LynxOpenCV.ResizeYUV(frameInfo.width, frameInfo.height, RECORD_WIDTH, RECORD_HEIGHT, frameInfo.leftEyeBuffer, yBytes);

                            // Update shader textures
                            m_textureAR.LoadRawTextureData(yBytes); // AR
                            m_textureAR.Apply();

                            m_textureVR.LoadRawTextureData(rgbaBytes); // VR
                            m_textureVR.Apply();

                            if (videoEncoderNatifMng.m_processStarted)
                            {
                                // Create Render Texture for Blit
                                RenderTexture rt = RenderTexture.GetTemporary(RECORD_WIDTH, (int)(RECORD_HEIGHT * 1.5), 8);
                                Graphics.Blit(m_textureAR, rt, m_material);


                                // Get data from Render texture
                                m_output.ReadPixels(new Rect(0, 0, RECORD_WIDTH, RECORD_HEIGHT * 1.5f), 0, 0);
                                RenderTexture.ReleaseTemporary(rt);

                                //LynxOpenCV.SetClock();
                                byte[] bytes = m_output.GetRawTextureData();

                                // MP4 video Encoding.
                                videoEncoderNatifMng.SetFrameAndEncode(bytes);
                                //if (videoEncoderNatifMng.m_processStarted)
                                //{
                                //    videoEncoderNatifMng.SetFrameAndEncode(bytes);
                                //}
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex.ToString());
                        }

                        GC.Collect();
                    };
                }
            }
        }
    }
}
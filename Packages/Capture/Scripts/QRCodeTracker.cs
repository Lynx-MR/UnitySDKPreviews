/**
 * @file QRCodeTracker.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Simple script to track a QR code via Lynx Video Capture.
 */
using System;
using System.Linq;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class QRCodeTracker : ImageProcessBase
    {
        #region INSPECTOR
        [Tooltip("QR code border size (meters)")]
        public float QRCodeSize = 0.10f;
        #endregion

        #region VARIABLES
        private static Action m_action = null; // Action for UI thread
        #endregion

        #region UNITY
        public override void StartVideoCapture()
        {
            LynxOpenCV.SetQRCodeSize(QRCodeSize);

            LynxCaptureAPI.onRGBFrames += OnCallbackProcessFrame;

            // Start video capture if it does not run
            if (!LynxCaptureAPI.IsCaptureRunning[LynxCaptureAPI.ESensorType.RGB])
            {
                LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, 30);

            }

        }

        private void LateUpdate()
        {
            // Fire events in UI thread
            if (m_action != null)
            {
                m_action.Invoke();
                m_action = null;
            }
        }
        #endregion

        /// <summary>
        /// Callback when frame is received from LynxCapture.
        /// </summary>
        /// <param name="width">Width of the captured frame</param>
        /// <param name="height">Height of the captured frame</param>
        /// <param name="data">Frame buffer</param>
        void OnCallbackProcessFrame(LynxFrameInfo frameInfo)
        {

            if (m_action == null)
            {
                // Buffer size (only Y data)
                int size = (int)(frameInfo.width * frameInfo.height);

                LynxOpenCV.Vec3d eulers;
                LynxOpenCV.Vec3d translation;
                char[] chars = new char[1024];
                if (LynxOpenCV.ProcessFrame(frameInfo.leftEyeBuffer, (int)frameInfo.width, (int)frameInfo.height, out eulers, out translation, chars)) // OpenCV process
                {

                    m_action = () =>
                    {
                        //string qrStr = Marshal.PtrToStringAnsi(qrPtr);
                        //Debug.Log($"Found QR code : {s.ToString()}");
                        for (int i = 0, count = 10; i < count; ++i)
                            Debug.Log(chars[i]);

                        Vector3 newPos = Camera.main.transform.position + Camera.main.transform.rotation * new Vector3((float)translation.x, -(float)translation.y, (float)translation.z);
                        this.transform.position = newPos;
                        this.transform.rotation = Quaternion.Euler(((float)eulers.x) * Mathf.Rad2Deg, ((float)eulers.y) * Mathf.Rad2Deg, ((float)eulers.z) * Mathf.Rad2Deg) * Quaternion.LookRotation(newPos - Camera.main.transform.position);

                    };
                }
            }
        }
    }
}
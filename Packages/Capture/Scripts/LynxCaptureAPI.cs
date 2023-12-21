/**
 * @file LynxCaptureAPI.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief API for video capture features.
 */

using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public static class LynxCaptureAPI
    {
        public enum ESensorType : byte
        {
            RGB,
            TRACKING,
            HANDTRACKING
        }

        public enum ESensorEye : UInt32
        {
            LEFT = 0,
            RIGHT
        }

        // Current status of the capture
        public static Dictionary<ESensorType, bool> IsCaptureRunning { get; private set; } = new Dictionary<ESensorType, bool>()
        {
            { ESensorType.RGB, false },
            { ESensorType.TRACKING, false },
            { ESensorType.HANDTRACKING, false }

        };

        public delegate void OnFrameDelegate(LynxFrameInfo frameInfo);

        // To subscribe on Video capture callback
        public static OnFrameDelegate onRGBFrames = null;
        public static OnFrameDelegate onTrackingFrames = null;
        public static OnFrameDelegate onHandtrackingFrames = null;

        // Be sure QXR is initialiazed for video capture and head pose
        public static bool IsQXRInitialized { get; private set; } = false;


        public static HeadPoseQXR m_head_QXR;


        static LynxCaptureAPI()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (LynxCaptureLibraryInterface.InitializeQXR())
            {
                IsQXRInitialized = true;
            }
            else
#endif
            {
                Debug.LogWarning("Cannot initialize QXR in Editor mode");
                IsQXRInitialized = false;
            }
        }

        /// <summary>
        /// Initialize and start camera capture.
        /// </summary>
        /// <param name="maxFPS">Number of FPS to target.</param>
        /// <returns>False and an error log if it fails.</returns>
        public static bool StartCapture(ESensorType sensorType, int maxFPS = 30)
        {
            if (IsCaptureRunning[sensorType])
                return false;
            
            if (!IsQXRInitialized)
            {
                Debug.LogError("QXR not initalized. Video capture cannot start");
                IsCaptureRunning[sensorType] = false;
                return false;
            }

            LynxCaptureLibraryInterface.SetMaxFPS((byte)sensorType, maxFPS);
            if(sensorType == ESensorType.TRACKING)
                LynxCaptureLibraryInterface.SetCallback((byte)sensorType, MonochromeFramesCallback);

            else if (sensorType == ESensorType.HANDTRACKING)
                LynxCaptureLibraryInterface.SetCallback((byte)sensorType, HandtrackingFramesCallback);

            else
                LynxCaptureLibraryInterface.SetCallback((byte)sensorType, RGBFramesCallback);


            

            if(!LynxCaptureLibraryInterface.StartCamera((byte)sensorType))
            {
                Debug.LogError("Cannot start camera");
                IsCaptureRunning[sensorType] = false;
                return false;
            }

            IsCaptureRunning[sensorType] = true;

            IntrinsicData intrinsic;
            ExtrinsicData extrinsic;
            if(!ReadCameraParameters(sensorType, ESensorEye.LEFT, out intrinsic, out extrinsic))
                Debug.LogError("FAILED to read intrinsic data");

            LynxOpenCV.LynxCameraInitConfiguration(ref intrinsic);

            return true;
        }

        /// <summary>
        /// Stop opened camera.
        /// </summary>
        public static void StopCapture(ESensorType sensorType)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            LynxCaptureLibraryInterface.StopCamera((byte)sensorType);
#endif
            IsCaptureRunning[sensorType] = false;
        }

        /// <summary>
        /// Read first camera parameters and return the matching structure.
        /// </summary>
        /// <returns>Intrinsic parameters or null if it failed.</returns>
        public static bool ReadCameraParameters(ESensorType sensorType, ESensorEye sensorChirality, out LynxCaptureLibraryInterface.IntrinsicData intrinsic, out LynxCaptureLibraryInterface.ExtrinsicData extrinsic)
        {

#if !UNITY_EDITOR
            if (!LynxCaptureLibraryInterface.ReadCameraParameters((byte)sensorType, (int)sensorChirality, out intrinsic, out extrinsic))
            {
                Debug.LogError("Cannot read camera parameters (ensure camera is running).");
                return false;
            }
#else
            intrinsic = new LynxCaptureLibraryInterface.IntrinsicData();
            extrinsic = new LynxCaptureLibraryInterface.ExtrinsicData();
#endif

            return true;
        }

        /// <summary>
        /// Call back event when a frame is captured from the camera.
        /// </summary>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="data">Frame data buffer</param>
        [MonoPInvokeCallback(typeof(LynxCaptureLibraryInterface.frame_callback_function))]
        private static void RGBFramesCallback(LynxFrameInfo frameInfo)
        {
            onRGBFrames?.Invoke(frameInfo);
        }

        /// <summary>
        /// Call back event when a frame is captured from the camera.
        /// </summary>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="dataLeft">Frame data buffer for left eye</param>
        /// <param name="dataRight">Frame data buffer for right eye</param>
        [MonoPInvokeCallback(typeof(LynxCaptureLibraryInterface.frame_callback_function))]
        private static void MonochromeFramesCallback(LynxFrameInfo frameInfo)
        {
            onTrackingFrames?.Invoke(frameInfo);
        }

        /// <summary>
        /// Call back event when a frame is captured from the camera.
        /// </summary>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="dataLeft">Frame data buffer for left eye</param>
        /// <param name="dataRight">Frame data buffer for right eye</param>
        [MonoPInvokeCallback(typeof(LynxCaptureLibraryInterface.frame_callback_function))]
        private static void HandtrackingFramesCallback(LynxFrameInfo frameInfo)
        {
            onHandtrackingFrames?.Invoke(frameInfo);
        }

        public static void StopAllCameras()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            LynxCaptureLibraryInterface.StopAllCameras();
#endif
        }

        /// <summary>
        /// Get head position in Unity coordinate space.
        /// </summary>
        /// <param name="outHeadPose">Head strut holding output data.</param>
        public static void GetHeadPosition(ref HeadPose outHeadPose)
        {
            if (!IsQXRInitialized)
            {
                Debug.LogError("QXR not initalized. Video capture cannot start");
                return;
            }

            LynxCaptureLibraryInterface.GetHeadTracking(ref m_head_QXR);
            outHeadPose.position = new Vector3(-m_head_QXR.position.y, m_head_QXR.position.x, -m_head_QXR.position.z);
            outHeadPose.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f) * new Quaternion(m_head_QXR.rotation.x, m_head_QXR.rotation.y, -m_head_QXR.rotation.z, -m_head_QXR.rotation.w);
            outHeadPose.reserved = m_head_QXR.reserved;
            outHeadPose.timestamp = m_head_QXR.timestamp;
        }


        #region STRUCTS
        /// <summary>
        /// Head pose data for Unity coordinate space.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HeadPose
        {
            public Quaternion rotation;
            public Vector3 position;
            public UInt32 reserved;
            public UInt64 timestamp;
        };
        #endregion
    }
}
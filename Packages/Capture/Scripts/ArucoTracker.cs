/**
 * @file ArucoTracker.cs
 * 
 * @author Lynx
 * 
 * @brief Simple script to track a Aruco code via Lynx Video Capture.
 */
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ArucoTracker : ImageProcessBase
    {
        #region INSPECTOR
        [Tooltip("Aruco code border size (meters)")]
        public float markerLength = 0.10f;
        #endregion

        #region VARIABLES
        private static Action m_action = null; // Action for UI thread
        private ExtrinsicData m_extrinsics_of_left_RGB;
        private Quaternion rotationBetweenRGBLeftAndIMU;
        private Vector3 translationBetweenRGBLeftAndIMU;
        private Vector3 IMUOffsetToCenterEye = new(0.001f, 0.010f, -0.011f);
        #endregion

        #region UNITY
        public override void StartVideoCapture()
        {

            LynxCaptureAPI.onRGBFrames += OnCallbackProcessFrame;

            // Start video capture if it does not run
            if (!LynxCaptureAPI.IsCaptureRunning[LynxCaptureAPI.ESensorType.RGB])
            {
                LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, 30);

            }


            IntrinsicData i_tmp;

            LynxCaptureLibraryInterface.ReadCameraParameters((int)LynxCaptureAPI.ESensorType.RGB, 0, out i_tmp, out m_extrinsics_of_left_RGB);


            Quaternion rotationQuaternion = (new Quaternion((float)m_extrinsics_of_left_RGB.orientation[0],
                                                           (float)m_extrinsics_of_left_RGB.orientation[1],
                                                           (float)m_extrinsics_of_left_RGB.orientation[2],
                                                           (float)m_extrinsics_of_left_RGB.orientation[3])).normalized;
            Quaternion translation_as_quaternion = new Quaternion((float)m_extrinsics_of_left_RGB.position[0],
                                                           (float)m_extrinsics_of_left_RGB.position[1],
                                                           (float)m_extrinsics_of_left_RGB.position[2],
                                                           0f);

            Quaternion rotationQuaternion_conjugate = rotationQuaternion;
            rotationQuaternion_conjugate.x = -rotationQuaternion_conjugate.x;
            rotationQuaternion_conjugate.y = -rotationQuaternion_conjugate.y;
            rotationQuaternion_conjugate.z = -rotationQuaternion_conjugate.z;
            // conjugate is Real part - Imaginary part
            rotationQuaternion_conjugate.w = rotationQuaternion_conjugate.w;

            Quaternion inverseTranslation = rotationQuaternion * translation_as_quaternion * rotationQuaternion_conjugate;

            translationBetweenRGBLeftAndIMU = new Vector3(-inverseTranslation.x, -inverseTranslation.y, -inverseTranslation.z);
            rotationBetweenRGBLeftAndIMU = rotationQuaternion_conjugate;

            LynxOpenCV.initArucoDetector(markerLength);

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


                ObjectTransformation outTransformation;


                // OpenCV process
                if (LynxOpenCV.ProcessFrameAruco(frameInfo.leftEyeBuffer, (int)frameInfo.width, (int)frameInfo.height, out outTransformation))
                {
                    m_action = () =>
                    {
                        Matrix4x4 rotationMatrix = new Matrix4x4();

                        rotationMatrix.SetRow(0, new Vector4(outTransformation.rotationMatrix[0],
                                                             outTransformation.rotationMatrix[1],
                                                             outTransformation.rotationMatrix[2],
                                                             0));
                        rotationMatrix.SetRow(1, new Vector4(outTransformation.rotationMatrix[3],
                                                             outTransformation.rotationMatrix[4],
                                                             outTransformation.rotationMatrix[5],
                                                             0));
                        rotationMatrix.SetRow(2, new Vector4(outTransformation.rotationMatrix[6],
                                                             outTransformation.rotationMatrix[7],
                                                             outTransformation.rotationMatrix[8],
                                                             0));
                        rotationMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

                        Quaternion rotationQuaternion = rotationMatrix.rotation;

                        // We use the extrinsics of the left rgb camera to get the rotation and translation from the RGB to the main camera in the
                        // middle of the two RGB cameras This is under the hypothesis that translationBetweenRGBLeftAndMain is in opencv coordinate system 
                        Vector3 translationArucoAndMain = new Vector3(outTransformation.translationVector[0],
                                                                                                   outTransformation.translationVector[1],
                                                                                                   outTransformation.translationVector[2])
                                                                                        + translationBetweenRGBLeftAndIMU
                                                                                        + IMUOffsetToCenterEye;
                        rotationQuaternion = rotationQuaternion * rotationBetweenRGBLeftAndIMU;

                        // Case 1 the extrinsics represente the position of the main camera in the RGB coordinate system.
                        //rotationMatrix = rotationMatrix.t();
                        //cv::Mat translationMat = -rotationMatrix * cv::Mat(translationVec);
                        //translationVec = translationMat.reshape(3).at<cv::Vec3d>();

                        // We need to inverse the y axis, we also need to remove 0.032 m or 32mm as we have a transformation relative 
                        // to the left rgb camera and the main camera is between the two rgb cameras which are separated by 64mm
                        this.transform.position = Camera.main.transform.TransformPoint(new Vector3(translationArucoAndMain[0],
                                                                                                   -translationArucoAndMain[1],
                                                                                                   translationArucoAndMain[2]));
                        // We want to transform the rotation from opencv to unity so we put a minus sign in front of the y value.
                        // As this changes the right handedness of the coordinate system we add minus signs to x, y, and z.
                        // As we want the rotation of the marker in world space we need to rotate the marker relative to the camera
                        // and then the camera relative to the workd
                        this.transform.rotation = Camera.main.transform.rotation * new Quaternion(-(float)rotationQuaternion.x,
                                                                                                  (float)rotationQuaternion.y,
                                                                                                  -(float)rotationQuaternion.z,
                                                                                                  (float)rotationQuaternion.w);

                    };
                }

            }
        }
    }
}
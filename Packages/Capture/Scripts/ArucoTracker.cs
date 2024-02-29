/**
 * @file ArucoTracker.cs
 * 
 * @author Lynx
 * 
 * @brief Simple script to track a Aruco code via Lynx Video Capture.
 */
using System;
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
        private Quaternion rotationBetweenRGBLeftAndIMU = Quaternion.identity;
        private Vector3 translationBetweenRGBLeftAndIMU = Vector3.zero;
        private static Vector3 IMUOffsetToCenterEye = new(0.001f, 0.010f, -0.011f);
        #endregion

        private static float[] coeffs = new float[4] { 1.0f, 1.0f, -1.0f, -1.0f };

        #region UNITY
        public override void StartVideoCapture()
        {

            LynxCaptureAPI.onRGBFrames += OnCallbackProcessFrame;

            // Start video capture if it does not run
            if (!LynxCaptureAPI.IsCaptureRunning[LynxCaptureAPI.ESensorType.RGB])
            {
                LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, 30);

            }

            InitArucoTracking(markerLength, ref translationBetweenRGBLeftAndIMU, ref rotationBetweenRGBLeftAndIMU);
        }

        private void LateUpdate()
        {

            if(Input.GetKeyUp(KeyCode.Alpha1))
            {
                coeffs[0] *= -1.0f;
                Debug.Log($"{coeffs[0]}\t{coeffs[1]}\t{coeffs[2]}\t{coeffs[3]}");
            }
            if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                coeffs[1] *= -1.0f;
                Debug.Log($"{coeffs[0]}\t{coeffs[1]}\t{coeffs[2]}\t{coeffs[3]}");
            }
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                coeffs[2] *= -1.0f;
                Debug.Log($"{coeffs[0]}\t{coeffs[1]}\t{coeffs[2]}\t{coeffs[3]}");
            }
            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                coeffs[3] *= -1.0f;
                Debug.Log($"{coeffs[0]}\t{coeffs[1]}\t{coeffs[2]}\t{coeffs[3]}");
            }


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
                        // Get Unity position and rotation based on Aruco tracking and extrinsics parameters
                        Vector3 newTranslation;
                        Quaternion newRotation;
                        ConvertArucoPosition(outTransformation, rotationBetweenRGBLeftAndIMU, translationBetweenRGBLeftAndIMU, out newTranslation, out newRotation);

                        // Apply new pose
                        this.transform.position = newTranslation;
                        this.transform.rotation = newRotation;
                    };
                }

            }
        }

        /// <summary>
        /// Initialialize Aruco tracking by retrieving translation and extrinsics data and using the given marker size.
        /// Warning: require camera to be started
        /// </summary>
        /// <param name="markerLength"></param>
        /// <param name="ouTranslationIntrinsics"></param>
        /// <param name="outRotationIntrinsics"></param>
        /// <param name="idCamera">Given camera ID (0: left, 1: right)</param>
        public static void InitArucoTracking(float markerLength, ref Vector3 ouTranslationIntrinsics, ref Quaternion outRotationIntrinsics, int idCamera = 0)
        {
            ExtrinsicData e_tmp;

            LynxCaptureLibraryInterface.ReadCameraParameters((int)LynxCaptureAPI.ESensorType.RGB, idCamera, out _, out e_tmp);


            Quaternion rotationQuaternion = (new Quaternion((float)e_tmp.orientation[0],
                                                           (float)e_tmp.orientation[1],
                                                           (float)e_tmp.orientation[2],
                                                           (float)e_tmp.orientation[3])).normalized;
            Quaternion translation_as_quaternion = new Quaternion((float)e_tmp.position[0],
                                                           (float)e_tmp.position[1],
                                                           (float)e_tmp.position[2],
                                                           0f); 

            Quaternion rotationQuaternion_conjugate = rotationQuaternion;
            rotationQuaternion_conjugate.x = -rotationQuaternion_conjugate.x;
            rotationQuaternion_conjugate.y = -rotationQuaternion_conjugate.y;
            rotationQuaternion_conjugate.z = -rotationQuaternion_conjugate.z;

            Quaternion inverseTranslation = rotationQuaternion * translation_as_quaternion * rotationQuaternion_conjugate;

            ouTranslationIntrinsics = new Vector3(-inverseTranslation.x, -inverseTranslation.y, -inverseTranslation.z);
            outRotationIntrinsics = rotationQuaternion_conjugate;

            LynxOpenCV.initArucoDetector(markerLength);
        }

        /// <summary>
        /// Convert object transform from Aruco tracking to Unity coordinate space with given extrinsics parameters from IMU.
        /// </summary>
        /// <param name="objTransformation">Object transform from Aruco tracking</param>
        /// <param name="rotationBetweenCameraAndIMU">Extrinsics parameters of the camera rotation.</param>
        /// <param name="translationBetweenCameraAndIMU">Extrinsics parameters of the camera translation.</param>
        /// <param name="outPosition">New computed position for Unity space</param>
        /// <param name="outRotation">New computed rotation for Unity space</param>
        public static void ConvertArucoPosition(ObjectTransformation objTransformation, Quaternion rotationBetweenCameraAndIMU, Vector3 translationBetweenCameraAndIMU, out Vector3 outPosition, out Quaternion outRotation)
        {
            Matrix4x4 rotationMatrix = new Matrix4x4();

            rotationMatrix.SetRow(0, new Vector4(objTransformation.rotationMatrix[0],
                                                 objTransformation.rotationMatrix[1],
                                                 objTransformation.rotationMatrix[2],
                                                 0));
            rotationMatrix.SetRow(1, new Vector4(objTransformation.rotationMatrix[3],
                                                 objTransformation.rotationMatrix[4],
                                                 objTransformation.rotationMatrix[5],
                                                 0));
            rotationMatrix.SetRow(2, new Vector4(objTransformation.rotationMatrix[6],
                                                 objTransformation.rotationMatrix[7],
                                                 objTransformation.rotationMatrix[8],
                                                 0));
            rotationMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

            Quaternion rotationQuaternion = rotationMatrix.rotation;


            // We need to inverse the y axis, we also need to remove 0.032 m or 32mm as we have a transformation relative 
            // to the left rgb camera and the main camera is between the two rgb cameras which are separated by 64mm
            outPosition = Camera.main.transform.TransformPoint(new Vector3(objTransformation.translationVector[0] - 0.032f,
                                        -objTransformation.translationVector[1],
                                        objTransformation.translationVector[2]));

            // We want to transform the rotation from opencv to unity so we put a minus sign in front of the y value.
            // As this changes the right handedness of the coordinate system we add minus signs to x, y, and z.
            // As we want the rotation of the marker in world space we need to rotate the marker relative to the camera
            // and then the camera relative to the workd
            outRotation = Camera.main.transform.rotation * new Quaternion((float)-rotationQuaternion.x,
                                                                            (float)rotationQuaternion.y,
                                                                            (float)-rotationQuaternion.z,
                                                                            (float)rotationQuaternion.w);
        }

    }
}
/**
 * @file LynxOpenCV.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief API to use the Lynx OpenCV wrapper with video capture.
 */

using System;
using System.Runtime.InteropServices;

namespace Lynx
{
    public class LynxOpenCV
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Vec3d
        {
            public double x;
            public double y;
            public double z;
        }
        #region LIBRARY ENTRY POINTS
        private const string LIB_NAME = "LynxOpenCV";

        [DllImport(LIB_NAME)]
        public static extern void LynxCameraInitConfiguration(ref LynxCaptureLibraryInterface.IntrinsicData intrinsic);

        /// <summary>
        /// Convert default YUV NV12 buffer to RGB buffer.
        /// </summary>
        /// <param name="YUVBuffer">[in] NV12 buffer to convert.</param>
        /// <param name="width">[in] Frame width</param>
        /// <param name="height">[in] Frame height</param>
        /// <param name="outRGBBuffer">[out] RGB output buffer converted from the YUV</param>
        [DllImport(LIB_NAME)]
        public static extern void YUV2RGB(IntPtr YUVBuffer, uint width, uint height, byte[] outRGBBuffer);

        /// <summary>
        /// Convert default YUV NV12 buffer to RGB buffer.
        /// </summary>
        /// <param name="YUVBuffer">[in] NV12 buffer to convert.</param>
        /// <param name="width">[in] Frame width</param>
        /// <param name="height">[in] Frame height</param>
        /// <param name="outRGBBuffer">[out] RGB output buffer converted from the YUV</param>
        [DllImport(LIB_NAME)]
        public static extern void YUV2RGB(byte[] YUVBuffer, uint width, uint height, byte[] outRGBBuffer);

        [DllImport(LIB_NAME)]
        public static extern void YUVFrame2ResizedRGB(int width, int height, int new_width, int new_height, IntPtr inBuffer, byte[] outRGBBuffer);


        /// <summary>
        /// Convert default YUV NV12 buffer to RGBA buffer.
        /// </summary>
        /// <param name="YUVBuffer">[in] NV12 buffer to convert.</param>
        /// <param name="width">[in] Frame width</param>
        /// <param name="height">[in] Frame height</param>
        /// <param name="outRGBABuffer">[out] RGBA output buffer converted from the YUV</param>
        [DllImport(LIB_NAME)]
        public static extern void YUV2RGBA(IntPtr YUVBuffer, uint width, uint height, byte[] outRGBABuffer);

        /// <summary>
        /// Convert default YUV NV12 buffer to RGBA buffer.
        /// </summary>
        /// <param name="YUVBuffer">[in] NV12 buffer to convert.</param>
        /// <param name="width">[in] Frame width</param>
        /// <param name="height">[in] Frame height</param>
        /// <param name="outRGBABuffer">[out] RGBA output buffer converted from the YUV</param>
        [DllImport(LIB_NAME)]
        public static extern void YUV2RGBA(byte[] YUVBuffer, uint width, uint height, byte[] outRGBABuffer);

        /// <summary>
        /// Convert RGB buffer to YUV I420 buffer.
        /// </summary>
        /// <param name="rgbBuffer">[in] Input RGB buffer</param>
        /// <param name="width">[in] Frame width</param>
        /// <param name="height">[in] Frame height</param>
        /// <param name="outYUVBuffer">[out] YUV buffer</param>
        [DllImport(LIB_NAME)]
        public static extern void RGB2YUV(IntPtr rgbBuffer, uint width, uint height, byte[] outYUVBuffer);

        /// <summary>
        /// Convert RGBA buffer to YUV I420 buffer.
        /// </summary>
        /// <param name="rgbBuffer">[in] RGB buffer</param>
        /// <param name="width">[in] Frame width</param>
        /// <param name="height">[in] Frame height</param>
        /// <param name="outYUVBuffer">[out] Output YUV buffer</param>
        [DllImport(LIB_NAME)]
        public static extern void RGBA2YUV(IntPtr rgbaBuffer, uint width, uint height, byte[] outYUVBuffer);

        /// <summary>
        /// Mix AR and VR part into RGBA buffer.
        /// The VR part should be RGBA format and will be modified as the result of the process.
        /// </summary>
        /// <param name="inYUVBufferAR">NV12 buffer to convert</param>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="inOutRGBABufferVR">[in/out] VR RGBA buffer as input, mixed with the AR part from the YUV as the output.</param>
        [DllImport(LIB_NAME)]
        public static extern void Compose_ARVR_2_RGBA_From_YUV_AR(IntPtr inYUVBufferAR, uint width, uint height, byte[] inOutRGBABufferVR);

        /// <summary>
        /// Mix AR and VR part into RGBA buffer.
        /// The VR part should be RGBA format and will be modified as the result of the process.
        /// </summary>
        /// <param name="inYUVBufferAR">NV12 buffer to convert</param>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="inOutRGBABufferVR">[in/out] VR RGBA buffer as input, mixed with the AR part from the YUV as the output.</param>
        [DllImport(LIB_NAME)]
        public static extern void Compose_ARVR_2_RGBA_From_YUV_AR(byte[] inYUVBufferAR, uint width, uint height, byte[] inOutRGBABufferVR);


        [DllImport(LIB_NAME)]
        public static extern void Compose_ARVR_2_RGBA_from_RGBA_AR(byte[] inRGBABufferAR, uint width, uint height, byte[] inOutRGBABufferVR);


        /// <summary>
        /// Resize the buffer to match new width and height
        /// </summary>
        /// <param name="width">Width of the buffer source</param>
        /// <param name="height">Height of the buffer source</param>
        /// <param name="depth">Bytes number (1, 2, 3 or 4)</param>
        /// <param name="new_width">Target width for the new buffer</param>
        /// <param name="new_height">Target height for the new buffer</param>
        /// <param name="inBuffer">Buffer source</param>
        /// <param name="outBuffer">Target buffer (should be allocated with the good size first (new byte[new_width * new_height * depth])</param>
        [DllImport(LIB_NAME)]
        public static extern void ResizeFrame(uint width, uint height, uint depth, uint new_width, uint new_height, byte[] inBuffer, byte[] outBuffer);

        /// <summary>
        /// Resize the buffer to match new width and height
        /// </summary>
        /// <param name="width">Width of the buffer source</param>
        /// <param name="height">Height of the buffer source</param>
        /// <param name="depth">Bytes number (1, 2, 3 or 4)</param>
        /// <param name="new_width">Target width for the new buffer</param>
        /// <param name="new_height">Target height for the new buffer</param>
        /// <param name="inBuffer">Buffer source</param>
        /// <param name="outBuffer">Target buffer (should be allocated with the good size first (new byte[new_width * new_height * depth])</param>
        [DllImport(LIB_NAME)]
        public static extern void ResizeFrame(uint width, uint height, uint depth, uint new_width, uint new_height, IntPtr inBuffer, byte[] outBuffer);


        [DllImport(LIB_NAME)]
        public static extern void ResizeYUV(uint width, uint height, uint new_width, uint new_height, IntPtr inBuffer, byte[] outBuffer);

        /// <summary>
        /// Filp given RGBA buffer vertically and/or horizontally.
        /// </summary>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="verticalFlip">True to flip vertically the frame</param>
        /// <param name="HorizontalFlip">True to flip horizontally the frame</param>
        /// <param name="inOutBuffer">Buffer to flip</param>
        [DllImport(LIB_NAME)]
        public static extern void FlipRGBAFrame(uint width, uint height, bool verticalFlip, bool HorizontalFlip, byte[] inOutBuffer);

        // <summary>
        /// Filp given RGB buffer vertically and/or horizontally.
        /// </summary>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="verticalFlip">True to flip vertically the frame</param>
        /// <param name="HorizontalFlip">True to flip horizontally the frame</param>
        /// <param name="inOutBuffer">Buffer to flip</param>
        [DllImport(LIB_NAME)]
        public static extern void FlipRGBFrame(uint width, uint height, bool verticalFlip, bool HorizontalFlip, byte[] inOutBuffer);

        /// <summary>
        /// Create fisheye undistortion mapping for given extrinsics and intrinsics parameters.
        /// </summary>
        /// <param name="cameraIdx">Define an index to store maps (to use when undistord buffer).</param>
        /// <param name="intrinsics">Given intrinsics parameters</param>
        /// <param name="extrinsics">Given extrinsics parameters</param>
        [DllImport(LIB_NAME)]
        public static extern void InitUndistortRectifyMap(byte idx, ref LynxCaptureLibraryInterface.IntrinsicData intrinsics, ref LynxCaptureLibraryInterface.ExtrinsicData extrinsics);

        /// <summary>
        /// Apply undistortion maps on given buffer (convert YUV to RGB).
        /// </summary>
        /// <param name="cameraIdx">Index where maps are stored (defined by InitUndistortRectifyMap)</param>
        /// <param name="inYUVBuffer">Given YUV buffer.</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="outRGBBuffer">Output undistorded buffer (RGB)</param>
        [DllImport(LIB_NAME)]
        public static extern void UndistordRGBFromYUV(byte idx, IntPtr YUVBuffer, uint width, uint height, byte[] outRGBBuffer);

        /// <summary>
        /// Apply undistortion maps on given buffer.
        /// </summary>
        /// <param name="cameraIdx">Index where maps are stored (defined by InitUndistortRectifyMap)</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="outRGBBuffer">Given RGB buffer to modify</param>
        [DllImport(LIB_NAME)]
        public static extern void UndistordRGB(byte idx, uint width, uint height, byte[] outRGBBuffer);

        /// <summary>
        /// Apply undistortion maps on given buffer.
        /// </summary>
        /// <param name="cameraIdx">Index where maps are stored (defined by InitUndistortRectifyMap)</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="outRGBBuffer">Given RGB buffer to modify</param>
        [DllImport(LIB_NAME)]
        public static extern void UndistortGreyScale(byte idx, uint width, uint height, IntPtr outRGBBuffer);

        /// <summary>
        /// Set a C++ clock at current time.
        /// </summary>
        [DllImport(LIB_NAME)]
        public static extern void SetClock();

        /// <summary>
        /// Return delta time between previous SetClock instruction and current time.
        /// </summary>
        /// <returns></returns>
        [DllImport(LIB_NAME)]
        public static extern double Unclock();

        #region QR CODE

        [DllImport(LIB_NAME)]
        public static extern bool ProcessFrame(IntPtr buffer, int width, int height, out Vec3d eulers, out Vec3d translate, char[] qrText);

        [DllImport(LIB_NAME)]
        public static extern bool ProcessFrame(byte[] buffer, int width, int height, out Vec3d eulers, out Vec3d translate, char[] qrText);

        [DllImport(LIB_NAME)]
        public static extern float GetQRCodeSize();

        [DllImport(LIB_NAME)]
        public static extern void SetQRCodeSize(float size);
        #endregion

        #region Aruco

        [DllImport(LIB_NAME)]
        public static extern bool ProcessFrameAruco(IntPtr buffer, int width, int height, out LynxCaptureLibraryInterface.ObjectTransformation outTransformation);

        [DllImport(LIB_NAME)]
        public static extern void initArucoDetector(float markerLength);
        #endregion

        #endregion

    }
}
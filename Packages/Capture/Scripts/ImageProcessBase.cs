/**
 * @file ImageProcessBase.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Helper to ease manage lifecycle in samples.
 */
using System;
using UnityEngine;

namespace Lynx
{
    public abstract class ImageProcessBase : MonoBehaviour
    {
        /// <summary>
        /// Method to start the video capture (called at start and when app is resumed.)
        /// </summary>
        public abstract void StartVideoCapture();

        public virtual void Start()
        {
            StartVideoCapture();
        }


        protected virtual void OnApplicationQuit()
        {
            LynxCaptureAPI.StopAllCameras();
        }

        protected virtual void OnApplicationPause(bool pause)
        {
            if (pause)
                LynxCaptureAPI.StopAllCameras();
            else
                StartVideoCapture();
        }

        protected virtual void OnApplicationFocus(bool focus)
        {
            if (!focus)
                LynxCaptureAPI.StopAllCameras();
            else
                StartVideoCapture();
        }

        /// <summary>
        /// Apply array bytes to the texture
        /// </summary>
        /// <param name="targetTexture">Texture on which to apply buffer</param>
        /// <param name="managedArray">Byte buffer holding frame data</param>
        protected virtual void DisplayBuffer(ref Texture2D targetTexture, byte[] managedArray)
        {
            try
            {
                targetTexture.LoadRawTextureData(managedArray);
                targetTexture.Apply();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

            GC.Collect(); // Frame callback can be so fast than Unity has not the time to use Garbage Collector to remove previous one from memory
        }
    }
}
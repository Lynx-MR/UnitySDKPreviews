/**
 * @file PointCloudManager.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Retrieve and display points from QXR point cloud. Points are updated by block of NB_POINTS_FOR_ROLLING points to avoid latency on main thread when displayed.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class PointCloudManager : MonoBehaviour
    {
        public GameObject m_cloudPointPrefab = null;

        [Range(0.001f, 1.0f)]
        [SerializeField] private float pointSize = 0.001f;

        public XrPointCloudQTI PointCloud = new XrPointCloudQTI(); // Struct holding point cloud data

        private Thread m_GetPointThread = null; // Thread to parallelize point cloud retrievement from main thread.
        private const int NB_POINTS_FOR_ROLLING = 100; // Number of points to display at each frame update
        private List<GameObject> m_pointsObj = new List<GameObject>(); // List of 3D representation for each point
        private bool m_isRunning = false;

        public List<Vector3> PointsList
        {
            get
            {
                List<Vector3> res = new List<Vector3>();
                for (int i = 0; i < PointCloud.numPoints; ++i)
                    res.Add(Convert3DPointFromQXR(PointCloud.points[i]));

                return res;
            }
        }


        void Start()
        {
            if (!LynxCaptureAPI.IsQXRInitialized)
            {
                Debug.LogError("Cannot initialize QXR");
                return;
            }

            // Thread to get points continuously without blocking main thread (UI)
            m_GetPointThread = new Thread(() =>
            {

                while (m_isRunning)
                {
                    GetPointCloud(ref PointCloud);

                    // [HERE] Add your point cloud process

                    Thread.Sleep(500); // Wait 500 milliseconds between each cloud point copy
                }
            });

            m_isRunning = true;
            m_GetPointThread.Start();

            // Start rolling display
            StartCoroutine(RollingPointDrawCoroutine());
        }

        private void OnApplicationQuit()
        {
            m_GetPointThread?.Join();
        }

        public IEnumerator RollingPointDrawCoroutine()
        {
            while (m_isRunning)
            {
                int counter = 0; // Counter to avoid overprocessing at each frame

                // Loop over all points in point cloud structure
                for (int it = 0, count = (int)PointCloud.numPoints; it < count; ++it)
                {
                    // Get the next point in the list, or create it if it doesn't exist in the list
                    GameObject go;
                    if (it < m_pointsObj.Count)
                    {
                        go = m_pointsObj[it];
                    }
                    else
                    {
                        go = GameObject.Instantiate(m_cloudPointPrefab, null);
                        m_pointsObj.Add(go);
                    }

                    // Place point at given coordinates from point cloud (with coordinate space conversion from QXR and Unity)
                    go.transform.position = Convert3DPointFromQXR(PointCloud.points[it]);
                    go.transform.localScale = Vector3.one * pointSize;

                    // Update only NB_POINTS_FOR_ROLLING points by frame to avoid blocking the main thread
                    ++counter;
                    if(counter == NB_POINTS_FOR_ROLLING)
                    {
                        counter = 0;
                        yield return new WaitForEndOfFrame();
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Convert 3D point from QXR coordinate space to Unity coordinate space.
        /// </summary>
        /// <param name="qxrPoint">3D position to convert in Unity coordinate space.</param>
        /// <returns>Position of the point in Unity coordinate space.</returns>
        public Vector3 Convert3DPointFromQXR(Vector3 qxrPoint)
        {
            return new Vector3(-qxrPoint.y, qxrPoint.x, -qxrPoint.z);
        }

        /// <summary>
        /// Convert 3D point from QXR coordinate space to Unity coordinate space from QXR structure.
        /// </summary>
        /// <param name="qxrPoint">3D position to convert in Unity coordinate space.</param>
        /// <returns>Position of the point in Unity coordinate space.</returns>
        public Vector3 Convert3DPointFromQXR(XrMapPointQTI qxrPoint)
        {
            return Convert3DPointFromQXR(new Vector3(qxrPoint.x, qxrPoint.y, qxrPoint.z));
        }
    }
}
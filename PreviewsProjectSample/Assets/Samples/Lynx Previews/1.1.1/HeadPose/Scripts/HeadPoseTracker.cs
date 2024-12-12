/**
 * @file HeadPoseTracker.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Sample script to simulate head position and rotation on attached object.
 */
using UnityEngine;
using static Lynx.LynxCaptureAPI;

namespace Lynx
{
    public class HeadPoseTracker : MonoBehaviour
    {
        HeadPose head;

        void Update()
        {
            GetHeadPosition(ref head);

            this.transform.position = head.position;
            this.transform.rotation = head.rotation;
        }
    }
}
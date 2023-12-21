/**
 * @file QuitBehaviour.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Simple script easily callable from Unity Inspector to quit the application.
 */
using UnityEngine;

namespace Lynx.Capture
{
    public class QuitBehaviour : MonoBehaviour
    {
        public void QuitApplication()
        {
            Application.Quit();
        }
    }
}
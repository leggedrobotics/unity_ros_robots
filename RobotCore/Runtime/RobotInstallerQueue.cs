#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;

namespace RSL.Robots
{
    public class RobotInstallerQueue
    {
        public static int TotalPackages { get; private set; }
        public static int RemainingPackages => _installQueue.Count + (_currentRequest != null ? 1 : 0);
        public static string CurrentPackageName { get; private set; }
        public static bool IsRunning => _currentRequest != null || _installQueue.Count > 0;
        public delegate void OnQueueCompletedHandler();
        public static OnQueueCompletedHandler OnQueueCompleted;
        private static Queue<string> _installQueue = new Queue<string>();
        private static AddRequest _currentRequest;

        public static void AddToQueue(List<string> gitUrls)
        {
            TotalPackages = gitUrls.Count;
            foreach (var url in gitUrls) _installQueue.Enqueue(url);
            
            if (_currentRequest == null) EditorApplication.update += ProcessQueue;
        }

        private static void ProcessQueue()
        {
            if (_currentRequest == null && _installQueue.Count > 0)
            {
                string nextUrl = _installQueue.Dequeue();
                // Simple string parsing to get the folder name from the URL for the UI
                CurrentPackageName = nextUrl.Split('=').LastOrDefault() ?? "Package"; 
                _currentRequest = Client.Add(nextUrl);
                return;
            }

            if (_currentRequest != null && _currentRequest.IsCompleted)
            {
                _currentRequest = null;
                if (_installQueue.Count == 0)
                {
                    EditorApplication.update -= ProcessQueue;
                    TotalPackages = 0;
                    OnQueueCompleted?.Invoke();
                }
            }
        }
    }
}
#endif
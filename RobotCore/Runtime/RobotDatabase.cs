using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace RSL.Robots
{

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    

    [CustomEditor(typeof(RobotDatabase))]
    public class RobotDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw the list of robots

            RobotDatabase db = (RobotDatabase)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Scan for Robots (ros.robots.*)", GUILayout.Height(30)))
            {
                db.RefreshDatabase();
            }

            GUILayout.Space(10);
            GUILayout.Label("Robot Package Catalog", EditorStyles.boldLabel);
            GUILayout.Space(5);
            
            if (db.robotPackages == null )
            {
                EditorGUILayout.HelpBox("Catalog is empty. Check your JSON path.", MessageType.Warning);
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Install All", GUILayout.Height(25)))
            {
                List<string> allToInstall = new List<string>();
                foreach (var robot in db.robotPackages.Values)
                {
                    if (!RobotDatabase.IsInstalled(robot.id))
                    {
                        allToInstall.Add(robot.url);
                    }
                }
                if (allToInstall.Count > 0)
                {
                    RobotInstallerQueue.AddToQueue(allToInstall);
                }
            }

            if (GUILayout.Button("Remove All", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Confirm Remove All", "Are you sure you want to uninstall all robots?", "Yes", "No"))
                {
                    foreach (var robot in db.robotPackages.Values)
                    {
                        if (RobotDatabase.IsInstalled(robot.id))
                        {
                            db.UninstallRobot(robot.id);
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);


            foreach (RobotPackage robot in db.robotPackages.Values)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                // Display Robot Name
                EditorGUILayout.LabelField(robot.name, GUILayout.Width(150));

                bool isInstalled = RobotDatabase.IsInstalled(robot.id);

                if (isInstalled)
                {
                    GUI.color = Color.green;
                    GUILayout.Label("✓ Installed", GUILayout.Width(80));
                    GUI.color = Color.white;
                    
                    if (GUILayout.Button("Uninstall", GUILayout.Width(80)))
                    {
                        if (EditorUtility.DisplayDialog("Confirm Uninstall", $"Are you sure you want to uninstall {robot.name}?", "Yes", "No"))
                        {
                            db.UninstallRobot(robot.id);
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Install", GUILayout.Width(80)))
                    {
                        db.InstallRobot(robot.id);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (RobotInstallerQueue.IsRunning)
            {
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Installation Progress", EditorStyles.boldLabel);

                // Calculate progress (0.0 to 1.0)
                int completed = RobotInstallerQueue.TotalPackages - RobotInstallerQueue.RemainingPackages;
                float progress = (float)completed / RobotInstallerQueue.TotalPackages;

                // Draw the Progress Bar
                Rect rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, progress, $"Installing: {RobotInstallerQueue.CurrentPackageName} ({completed}/{RobotInstallerQueue.TotalPackages})");

                // Force the inspector to update even if the mouse isn't moving
                Repaint(); 
            }

        }
    }

#endif

    [CreateAssetMenu(fileName = "RobotDatabase", menuName = "Robots/RobotDatabase")]
    public class RobotDatabase : ScriptableObject
    {
        [System.Serializable]
        public struct RobotEntry
        {
            public string name;
            public string rootFrame;
            public Sprite icon;
            public GameObject prefab;
        }


        public List<RobotEntry> allRobots = new List<RobotEntry>();
        public Dictionary<string, RobotPackage> robotPackages = new Dictionary<string, RobotPackage>();

        #if UNITY_EDITOR
        public void OnEnable()
        {// Inside your ScriptableObject or Editor script
            string packageId = "com.leggedrobotics.ros.robots.core";
            string relativePath = "Runtime/robots.json";
            string unityPath = $"Packages/{packageId}/{relativePath}";

            if (File.Exists(unityPath))
            {
                string jsonContent = File.ReadAllText(unityPath);
                var robotPackagesData = JsonUtility.FromJson<RobotPackageList>(jsonContent);
                robotPackages = new Dictionary<string, RobotPackage>();
                foreach (var robot in robotPackagesData.robots)
                {
                    robotPackages[robot.id] = robot;
                    
                }
            }
            RobotInstallerQueue.OnQueueCompleted += RefreshDatabase;
        }

        public void InstallRobot(string robotPackageId)
        {
            List<string> installOrder = GetInstallationOrder(robotPackageId);
            if (installOrder.Count == 0)
            {
                Debug.Log($"[UPM] Robot '{robotPackageId}' is already installed or has no installable dependencies.");
                return;
            }

            RobotInstallerQueue.AddToQueue(installOrder);
        }

        public List<string> GetInstallationOrder(string targetId)
        {
            List<string> orderedUrls = new List<string>();
            HashSet<string> visited = new HashSet<string>();

            void Resolve(string id)
            {
                if (visited.Contains(id) || !robotPackages.ContainsKey(id)) return;

                visited.Add(id);
                RobotPackage pkg = robotPackages[id];

                if (pkg.dependencies != null)
                {
                    foreach (string depId in pkg.dependencies)
                        Resolve(depId);
                }

                if (!IsInstalled(id))
                {
                    orderedUrls.Add(pkg.url);
                }
            }

            Resolve(targetId);
            return orderedUrls;
        }

        public void UninstallRobot(string robotPackageId)
        {
            if (!IsInstalled(robotPackageId))
            {
            Debug.Log($"[UPM] Robot '{robotPackageId}' is not installed.");
            return;
            }

            List<string> dependenciesToRemove = new List<string>();
            GetDependencies(robotPackageId, dependenciesToRemove);
            dependenciesToRemove.Reverse(); // Uninstall dependencies in reverse order
            foreach (string depId in dependenciesToRemove)
            {
                if (IsInstalled(depId))
                {
                    UnityEditor.PackageManager.Client.Remove(depId);
                }
            }

            RefreshDatabase();
        }

        private void GetDependencies(string packageId, List<string> dependencies)
        {
            foreach (var robot in robotPackages.Values)
            {
                if (robot.dependencies != null && robot.dependencies.Contains(packageId))
                {
                    dependencies.Add(robot.id);
                    GetDependencies(robot.id, dependencies);
                }
            }
            dependencies.Add(packageId);
        }
        

        public static bool IsInstalled(string packageId)
        {
            var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
            return packages != null && System.Array.Exists(packages, p => p.name == packageId);
        }

        public void RefreshDatabase()
        {
            allRobots.Clear();

            List<string> searchFolders = new List<string> { "Assets" };

            var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
            foreach (var package in packages)
            {
                if (package.name.Contains("ros.robots"))
                {
                    searchFolders.Add(package.assetPath);
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", searchFolders.ToArray());
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null && prefab.TryGetComponent<RobotInfo>(out var robotData))
                {
                    allRobots.Add(new RobotEntry
                    {
                        name = robotData.robotName,
                        rootFrame = robotData.rootFrame,
                        icon = robotData.robotSprite,
                        prefab = prefab
                    });
                }
            }
                

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log($"Database Refreshed: Found {allRobots.Count} robots.");
        }
        #endif
    }
}
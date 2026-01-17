using System;
using System.Collections.Generic;
using UnityEngine;

namespace RSL.Robots
{
    [System.Serializable]
    public class RobotPackage
    {
        [SerializeField]
        public string id;
        
        [SerializeField]
        public string name;
        
        [SerializeField]
        public string url;
        
        [SerializeField]
        public List<string> dependencies;

        public static RobotPackage FromJson(string json)
        {
            return JsonUtility.FromJson<RobotPackage>(json);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    public class RobotPackageList
    {
        [SerializeField]
        public List<RobotPackage> robots;
    }
}
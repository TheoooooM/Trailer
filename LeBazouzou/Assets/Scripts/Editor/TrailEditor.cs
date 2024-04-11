using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CameraTrail))]
    public class TrailEditor : UnityEditor.Editor
    {
        private CameraTrail trail;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Set At Index"))
            {
                trail.Init();
            }
        }

        private void OnEnable()
        {
            trail = (CameraTrail)target;
            trail.Init();
        }
    }
}

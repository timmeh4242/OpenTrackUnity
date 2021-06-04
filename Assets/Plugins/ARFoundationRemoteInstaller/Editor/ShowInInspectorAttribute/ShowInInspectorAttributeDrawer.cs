#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace ARFoundationRemote.Editor {
    [CustomEditor(typeof(ARFoundationRemoteInstaller), true), CanEditMultipleObjects]
    public class ShowInInspectorAttributeDrawer : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            showMethodsInInspector(targets);
        }

        static void showMethodsInInspector(params Object[] targets) {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static;
            var methods = targets.First().GetType().GetMethods(flags).Where(m => m.GetCustomAttributes(typeof(ShowInInspectorAttribute)).Any() && m.GetParameters().Length == 0);
            foreach (var methodInfo in methods) {
                if (GUILayout.Button(methodInfo.Name, new GUIStyle(GUI.skin.button))) {
                    foreach (var target in targets) {
                        methodInfo.Invoke(target, null);
                    }
                }
            }
        }
    }
}
#endif // UNITY_EDITOR

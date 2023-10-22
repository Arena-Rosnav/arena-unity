using UnityEditor;
using UnityEngine;

namespace RestServer.AutoEndpoints {
    [CustomEditor(typeof(AttributeAutoEndpoint))]
    public class AttributeAEInspector : Editor {
        private const float LABEL_WIDTH = 150f;
        private SerializedProperty _restServer;
        private SerializedProperty _rootPath;
        private SerializedProperty _inspectCompleteScene;
        private SerializedProperty _inspectGameObjects;
        private bool _foldoutAttributes;

        private void OnEnable() {
            _restServer = serializedObject.FindProperty("restServer");
            _rootPath = serializedObject.FindProperty("endpointPath");
            _inspectCompleteScene = serializedObject.FindProperty("inspectCompleteScene");
            _inspectGameObjects = serializedObject.FindProperty("inspectGameObjects");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            var aae = (AttributeAutoEndpoint)target;

            EditorGUILayout.PropertyField(_restServer);
            EditorGUILayout.PropertyField(_rootPath, new GUIContent("Root Path"));
            EditorGUILayout.PropertyField(_inspectCompleteScene, new GUIContent("Inspect complete scene"));

            if (!aae.inspectCompleteScene) {
                EditorGUILayout.PropertyField(_inspectGameObjects);
            }

            if (aae.FoundAttributes != null && Application.isPlaying && aae.IsInitDone) {
                _foldoutAttributes = EditorGUILayout.Foldout(_foldoutAttributes, "Found Endpoints", true);
                if (_foldoutAttributes) {
                    DrawFoundAttributes(aae);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFoundAttributes(AttributeAutoEndpoint aae) {
            var foldoutStyle0 = new GUIStyle(EditorStyles.foldout) {
                fontStyle = FontStyle.Bold,
            };
            var labelStyleInner = new GUIStyle(EditorStyles.label) {
                margin = {
                    left = 40
                }
            };
            var linkLabel = new GUIStyle(EditorStyles.linkLabel) {
                fontStyle = FontStyle.Bold
            };

            foreach (var foundAttribute in aae.FoundAttributes) {
                GUILayout.BeginHorizontal(labelStyleInner);
                GUILayout.Label(foundAttribute.EndpointAttribute.Method.ToString(), GUILayout.Width(LABEL_WIDTH));
                GUILayout.Label(foundAttribute.EndpointAttribute.SubPath);
                GUILayout.EndHorizontal();
                
                // this is a "lousy" hack, but the properties do not exist in reality and we fake a nice display
                // in the inspector. Hopefully UIToolkit will be better in the future...
                
                var gameObjectLabelRect = EditorGUILayout.GetControlRect(true, 0);
                gameObjectLabelRect = new Rect(gameObjectLabelRect.position,new Vector2(gameObjectLabelRect.size.x, 18.0f));
                GUILayout.BeginHorizontal(labelStyleInner);
                GUILayout.Label("GameObject", GUILayout.Width(LABEL_WIDTH));

                GUILayout.Label(foundAttribute.GameObject.name, linkLabel);
                GUILayout.EndHorizontal();
                
                if (UnityEngine.Event.current.type == EventType.MouseDown) {
                    if (gameObjectLabelRect.Contains(UnityEngine.Event.current.mousePosition)) {
                        AssetDatabase.OpenAsset(foundAttribute.GameObject);
                    }
                }

                var rect2 = EditorGUILayout.GetControlRect(false, 1);
                rect2.height = 1;
                EditorGUI.DrawRect(rect2, new Color(0.5f, 0.5f, 0.5f, 1));
            }
        }
    }
}
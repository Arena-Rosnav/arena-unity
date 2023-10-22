using RestServer.Helper;
using UnityEditor;
using UnityEngine;

namespace RestServer.AutoEndpoints {
    [CustomEditor(typeof(StaticContentAutoEndpoint))]
    public class StaticContentAEInspector : Editor {
        private SerializedProperty _restServer;
        private SerializedProperty _rootPath;
        private SerializedProperty _mapIndexHtml;
        private SerializedProperty _useCoroutineInit;
        private SerializedProperty _files;
        private SerializedProperty _debugLog;

        private void OnEnable() {
            _restServer = serializedObject.FindProperty("restServer");
            _rootPath = serializedObject.FindProperty("rootPath");
            _mapIndexHtml = serializedObject.FindProperty("mapIndexHtml");
            _useCoroutineInit = serializedObject.FindProperty("useCoroutineInit");
            _files = serializedObject.FindProperty("files");
            _debugLog = serializedObject.FindProperty("debugLog");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            var sc = (StaticContentAutoEndpoint)target;

            if (Application.isPlaying && !sc.IsBuildDone) {
                EditorGUILayout.HelpBox("Content is generating...", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(_restServer);
            EditorGUILayout.PropertyField(_rootPath);
            EditorGUILayout.PropertyField(_mapIndexHtml);
            EditorGUILayout.PropertyField(_useCoroutineInit);
            EditorGUILayout.PropertyField(_debugLog);
            Show(_files);

            serializedObject.ApplyModifiedProperties();
        }

        public static void Show(SerializedProperty list) {
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Files");
            if (GUILayout.Button("Add asset")) {
                list.arraySize++;
            }

            bool? doExpand = null;
            if (GUILayout.Button("Expand all")) {
                doExpand = true;
            }

            if (GUILayout.Button("Collapse all")) {
                doExpand = false;
            }

            GUILayout.EndHorizontal();

            EditorGUI.indentLevel += 1;
            for (var i = 0; i < list.arraySize; i++) {
                var listElement = list.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(listElement);
                if (doExpand.HasValue) {
                    listElement.isExpanded = doExpand.Value;
                }

                // EditorGUI.indentLevel += 1;
                if (list.GetArrayElementAtIndex(i).isExpanded) {
                    if (GUILayout.Button("Remove asset")) {
                        list.DeleteArrayElementAtIndex(i);
                    }
                }

                // EditorGUI.indentLevel -= 1;
            }

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Separator();
        }
    }

    [CustomPropertyDrawer(typeof(StaticContentAEEntry))]
    public class StaticContentAEEntryInspector : PropertyDrawer {
        public static string[] ALL_MIME_TYPES {
            get {
                var mimeTypes = MimeTypeDict.MIME_TYPE_MAPPINGS;
                var allMimeTypes = new string[mimeTypes.Count + 1];
                mimeTypes.Values.CopyTo(allMimeTypes, 0);
                allMimeTypes[allMimeTypes.Length - 1] = "[Custom]";
                return allMimeTypes;
            }
        }

        private readonly Color _redColor = new Color(1.0f, 0.4f, 0.4f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var sc = (StaticContentAutoEndpoint)property.serializedObject.targetObject;

            var pSubPath = property.FindPropertyRelative("subPath");
            var pAsset = property.FindPropertyRelative("asset");
            var pIsZip = property.FindPropertyRelative("isZip");
            var pIsBinary = property.FindPropertyRelative("isBinary");
            var pContentType = property.FindPropertyRelative("contentType");

            var displayOverviewLabelStr = GetHeader(pSubPath, pIsZip, pIsBinary, pContentType);

            var hasError = HasError(pAsset, pSubPath, pContentType, pIsZip);
            var beforeContentColor = GUI.contentColor;
            if (hasError) {
                GUI.contentColor = _redColor;
            }

            label.text = displayOverviewLabelStr;
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, property.isExpanded, label);
            if (hasError) {
                GUI.contentColor = beforeContentColor;
            }

            if (property.isExpanded) {
                DrawProperty(pSubPath, pIsZip, pAsset, pIsBinary, pContentType);
            }

            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();
        }

        private void DrawProperty(SerializedProperty pSubPath, SerializedProperty pIsZip, SerializedProperty pAsset, SerializedProperty pIsBinary,
            SerializedProperty pContentType) {
            var beforeContentColor = GUI.contentColor;

            var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField);
            EditorGUI.PropertyField(rect, pSubPath);

            pIsZip.boolValue = EditorGUILayout.Toggle(
                new GUIContent("Extract Zip", "Handle the given asset as zip file and serve the contents instead of the zip file itself."),
                pIsZip.boolValue
            );

            if (pAsset.objectReferenceValue == null) {
                GUI.contentColor = _redColor;
            }

            rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField);
            EditorGUI.PropertyField(rect, pAsset);
            if (pAsset.objectReferenceValue == null) {
                GUI.contentColor = beforeContentColor;
            }

            if (pAsset.objectReferenceValue == null) {
                EditorGUILayout.HelpBox("In Unity Asset extensions should end with '.bytes' for binary/zip files.", MessageType.Info);
            }

            if (!pIsZip.boolValue) {
                pIsBinary.boolValue = EditorGUILayout.Toggle("Handle as binary?", pIsBinary.boolValue);


                if (string.IsNullOrEmpty(pContentType.stringValue)) {
                    GUI.contentColor = _redColor;
                }

                //rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField);
                var oldContentType = GetMimeTypeSelectedIdx(pContentType.stringValue);
                var newContentType = EditorGUILayout.Popup("Content Type", oldContentType, ALL_MIME_TYPES);
                if (newContentType != ALL_MIME_TYPES.Length - 1) { // NOT CUSTOM
                    if (oldContentType != newContentType) {
                        pContentType.stringValue = ALL_MIME_TYPES[newContentType];
                    }
                } else {
                    if (oldContentType != newContentType) { // switch to custom
                        pContentType.stringValue = "content/type";
                    }

                    rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField);
                    EditorGUI.PropertyField(rect, pContentType);
                }

                if (string.IsNullOrEmpty(pContentType.stringValue)) {
                    GUI.contentColor = beforeContentColor;
                }
            }
        }

        private int GetMimeTypeSelectedIdx(string mimeType) {
            for (int i = 0; i < ALL_MIME_TYPES.Length; i++) {
                if (mimeType == ALL_MIME_TYPES[i]) {
                    return i;
                }
            }

            return ALL_MIME_TYPES.Length - 1; // unknown mime type - return custom
        }

        private bool HasError(SerializedProperty pAsset, SerializedProperty pSubPath, SerializedProperty pContentType, SerializedProperty pIsZip) {
            if (pAsset.objectReferenceValue == null) {
                return true;
            }

            if (string.IsNullOrEmpty(pContentType.stringValue) && !pIsZip.boolValue) {
                return true;
            }

            return false;
        }

        private string GetHeader(SerializedProperty pSubPath, SerializedProperty pIsZip, SerializedProperty pIsBinary, SerializedProperty pContentType) {
            if (pIsZip.boolValue) {
                return $"{PathHelper.EnsureSlashPrefix(pSubPath.stringValue)} [Zip]";
            }

            var type = pIsBinary.boolValue ? "Binary" : "Text";
            if (pIsBinary.boolValue) { }

            return $"{PathHelper.EnsureSlashPrefix(pSubPath.stringValue)} [{type} ContentType: {pContentType.stringValue}]";
        }
    }
}
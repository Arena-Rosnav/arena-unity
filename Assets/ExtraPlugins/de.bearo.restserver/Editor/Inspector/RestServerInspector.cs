using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using RestServer.Helper;
using UnityEditor;
using UnityEngine;

namespace RestServer.Inspector {
    [CustomEditor(typeof(RestServer))]
    public class RestServerInspector : Editor {
        private SerializedProperty _spPort;
        private SerializedProperty _spAutostart;
        private SerializedProperty _spDebugLog;
        private SerializedProperty _spListenAddress;
        private SerializedProperty _spRespectUnityDisable;
        private SerializedProperty _spMaxThreadingHelperWorkCount;
        private SerializedProperty _spForceAccessLog;

        private bool _foldoutConnection;
        private bool _foldoutEndpoints;
        private bool _foldoutIPs;
        private bool _foldoutAdvanced;

        private bool _foldoutEndpointsGet;
        private bool _foldoutEndpointsPost;
        private bool _foldoutEndpointsPut;
        private bool _foldoutEndpointsDelete;
        private bool _foldoutEndpointsHead;
        private bool _foldoutEndpointsOptions;
        private bool _foldoutEndpointsPatch;

        private const float LABEL_WIDTH = 150f;

        private void OnEnable() {
            _spPort = serializedObject.FindProperty("port");
            _spAutostart = serializedObject.FindProperty("autostart");
            _spDebugLog = serializedObject.FindProperty("_debugLog");
            _spListenAddress = serializedObject.FindProperty("_listenAddressUnity");
            _spRespectUnityDisable = serializedObject.FindProperty("respectUnityDisable");
            _spMaxThreadingHelperWorkCount = serializedObject.FindProperty("maxThreadingHelperWorkCount");
            _spForceAccessLog = serializedObject.FindProperty("forceAccessLog");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            var rs = (RestServer)target;

            var redText = new GUIStyle(EditorStyles.label) {
                normal = {
                    textColor = new Color(1.0f, 0.4f, 0.4f)
                },
                fontStyle = FontStyle.Bold
            };
            var greenText = new GUIStyle(EditorStyles.label) {
                normal = {
                    textColor = Color.green
                },
                fontStyle = FontStyle.Bold
            };
            var oldInterfaceIdx = GetSelectedInterface(rs);

            if (oldInterfaceIdx != 0 /* ALL */) {
                EditorGUILayout.HelpBox("Server might be accessible from the outside. Please ensure that is secured accordingly.", MessageType.Warning);
            }

            // 
            // Connection Properties
            // 
            _foldoutConnection =
                EditorGUILayout.BeginFoldoutHeaderGroup(_foldoutConnection, $"Connection Properties [{rs.ListenAddress}:{GetRestServerPort(rs)}]");
            if (_foldoutConnection) {
                EditorGUI.indentLevel += 1;
                EditorGUI.BeginDisabledGroup(rs.IsStarted);

                var newInterfaceIdx = EditorGUILayout.Popup(
                    "Interface", oldInterfaceIdx,
                    oldInterfaceIdx < 3
                        ? new[] { "Only localhost", "All (IPv4)", "All (IPv6)" }
                        : new[] { "Only localhost", "All (IPv4)", "All (IPv6)", "Custom" }
                );
                SetSelectedInterface(rs, oldInterfaceIdx, newInterfaceIdx);

                EditorGUI.BeginDisabledGroup(rs.port == 0);
                if (rs.port != 0) {
                    EditorGUILayout.PropertyField(_spPort);
                } else {
                    EditorGUILayout.TextField("Port", rs.IsStarted ? $"{rs.EffectivePort}" : "<random>");
                }

                EditorGUI.EndDisabledGroup();

                var oldPort = rs.port;
                if (EditorGUILayout.Toggle("Use random port", rs.port == 0)) {
                    rs.port = 0;
                } else if (oldPort > 0) {
                    rs.port = oldPort;
                } else {
                    rs.port = 8080;
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            //
            // Autostart
            //
            EditorGUILayout.PropertyField(_spAutostart);

            //
            // Debug Log
            //
            EditorGUILayout.PropertyField(_spDebugLog);

            // 
            // Access Log
            //
            EditorGUILayout.PropertyField(_spForceAccessLog);

            //
            // Advanced
            //
            _foldoutAdvanced = EditorGUILayout.Foldout(_foldoutAdvanced, "Advanced");
            if (_foldoutAdvanced) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spRespectUnityDisable);
                EditorGUILayout.PropertyField(_spMaxThreadingHelperWorkCount);
                EditorGUI.indentLevel--;
            }

            //
            // Server status
            //
            if (rs.IsStarted) {
                EditorGUILayout.LabelField("Status", $"Running and listening on {((IPEndPoint)rs.Server.Endpoint).Address}:{rs.EffectivePort}", greenText);
                if (!rs.isActiveAndEnabled) {
                    EditorGUILayout.HelpBox("Port is kept open, but no workloads are processed because this component is disabled.", MessageType.Warning);
                }
            } else if (Application.isPlaying) {
                EditorGUILayout.LabelField("Status", "Not running (Enable via Autostart/Scripting)", redText);
            } else {
                EditorGUILayout.LabelField("Status", "Not running (enter PlayMode to start)", redText);
            }

            //
            // Start / Stop Button
            //
            GUILayout.BeginHorizontal();
            if (Application.isPlaying) {
                if (rs.IsStarted) {
                    if (GUILayout.Button("Stop Server")) {
                        rs.StopServer();
                    }
                } else {
                    if (GUILayout.Button($"Start Server on port {GetRestServerPort(rs)}")) {
                        rs.StartServer();
                    }
                }
            }

            GUILayout.EndHorizontal();

            //
            // IPs
            //
            if (rs.IsStarted && _spListenAddress.enumValueIndex > 0 && _spListenAddress.enumValueIndex < 3) { // only for Any and AnyIPv6
                _foldoutIPs = EditorGUILayout.BeginFoldoutHeaderGroup(_foldoutIPs, new GUIContent("Interface IPs"));
                if (_foldoutIPs) {
                    EditorGUI.indentLevel++;
                    var netIps = GetPossibleListenIPs(rs.Server.Endpoint.AddressFamily);
                    foreach (var netIp in netIps) {
                        var net = (NetworkInterface)netIp[0];
                        var ip = (IPAddress)netIp[1];
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", $"{ip} ({net.Name})");
                        if (GUILayout.Button("Copy")) {
                            var ipStr = $"http://{ip}:{rs.EffectivePort}/";
                            if (ip.AddressFamily == AddressFamily.InterNetworkV6) {
                                ipStr = $"http://[{ip}]:{rs.EffectivePort}/";
                            }

                            GUIUtility.systemCopyBuffer = ipStr;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            //
            // Endpoints
            //  
            DisplayAllRegisteredEndpoints();

            //
            // Review Hint
            // 
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Reviews are important to make it easier for others to find good assets in the asset store" +
                                    ". If you like the rest server, please leave a review!", MessageType.Warning);
            if (GUILayout.Button("Leave a review")) {
                Application.OpenURL("https://u3d.as/2LRv");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private string GetRestServerPort(RestServer rs) {
            return rs.port == 0 ? "<random>" : $"{rs.port}";
        }

        private List<object[]> GetPossibleListenIPs(AddressFamily addressFamily) {
            var ret = new List<object[]>();
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                var ipProps = netInterface.GetIPProperties();
                foreach (var address in ipProps.UnicastAddresses) {
                    if (address.Address.AddressFamily != addressFamily) {
                        continue;
                    }

                    ret.Add(new object[] { netInterface, address.Address });
                }
            }

            return ret;
        }

        private void SetSelectedInterface(RestServer rs, int oldInterfaceIdx, int newInterfaceIdx) {
            _spListenAddress.enumValueIndex = newInterfaceIdx;
        }

        private int GetSelectedInterface(RestServer rs) {
            return _spListenAddress.enumValueIndex;
        }

        private void DisplayAllRegisteredEndpoints() {
            var foldoutStyle0 = new GUIStyle(EditorStyles.foldout) {
                fontStyle = FontStyle.Bold,
            };
            var foldoutStyle1 = new GUIStyle(EditorStyles.foldout) {
                fontStyle = FontStyle.Bold,
                margin = {
                    left = 15
                }
            };
            var labelStyleInner = new GUIStyle(EditorStyles.label) {
                margin = {
                    left = 40
                }
            };
            var buttonStyleInner = new GUIStyle(GUI.skin.button) {
                margin = {
                    left = 40
                }
            };


            if (Application.isPlaying) {
                _foldoutEndpoints = EditorGUILayout.Foldout(_foldoutEndpoints, "Endpoints", true, foldoutStyle0);
                if (_foldoutEndpoints) {
                    // HttpMethod.GET
                    DrawGetEndpoints(foldoutStyle1, labelStyleInner, buttonStyleInner);

                    // HttpMethod.PATCH
                    _foldoutEndpointsPatch = DisplaySpecificRegisteredEndpoints(HttpMethod.PATCH, "PATCH", foldoutStyle1, labelStyleInner, buttonStyleInner,
                        _foldoutEndpointsPatch);

                    // HttpMethod.POST
                    _foldoutEndpointsPost = DisplaySpecificRegisteredEndpoints(HttpMethod.POST, "POST", foldoutStyle1, labelStyleInner, buttonStyleInner,
                        _foldoutEndpointsPost);

                    // HttpMethod.PUT
                    _foldoutEndpointsPut = DisplaySpecificRegisteredEndpoints(HttpMethod.PUT, "PUT", foldoutStyle1, labelStyleInner, buttonStyleInner,
                        _foldoutEndpointsPut);

                    // HttpMethod.DELETE
                    _foldoutEndpointsDelete =
                        DisplaySpecificRegisteredEndpoints(HttpMethod.DELETE, "DELETE", foldoutStyle1, labelStyleInner, buttonStyleInner,
                            _foldoutEndpointsDelete);

                    // HttpMethod.HEAD
                    _foldoutEndpointsHead = DisplaySpecificRegisteredEndpoints(HttpMethod.HEAD, "HEAD", foldoutStyle1, labelStyleInner, buttonStyleInner,
                        _foldoutEndpointsHead);

                    // HttpMethod.OPTIONS
                    _foldoutEndpointsOptions =
                        DisplaySpecificRegisteredEndpoints(HttpMethod.OPTIONS, "OPTIONS", foldoutStyle1, labelStyleInner, buttonStyleInner,
                            _foldoutEndpointsOptions);
                }
            }
        }

        private bool DisplaySpecificRegisteredEndpoints(HttpMethod httpMethod,
            string endpointFoldoutLabel,
            GUIStyle foldoutStyle0,
            GUIStyle labelStyleInner,
            GUIStyle buttonStyleInner,
            bool foldout) {
            var rs = (RestServer)target;

            var endpoints = rs.EndpointCollection.GetAllEndpoints(httpMethod);
            if (endpoints == null || endpoints.Count <= 0) {
                return foldout;
            }

            foldout = EditorGUILayout.Foldout(foldout, endpointFoldoutLabel, true, foldoutStyle0);
            if (!foldout) {
                return false;
            }

            DrawHorizontalLine();
            foreach (var endpoint in endpoints) {
                if (endpoint.EndpointRegex != null) {
                    GUILayout.BeginHorizontal(labelStyleInner);
                    GUILayout.Label("Pattern", GUILayout.Width(LABEL_WIDTH));
                    GUILayout.Label(endpoint.EndpointRegex.ToString());
                    GUILayout.EndHorizontal();
                } else {
                    GUILayout.BeginHorizontal(labelStyleInner);
                    GUILayout.Label("Url", GUILayout.Width(LABEL_WIDTH));
                    GUILayout.Label(endpoint.EndpointString);
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal(labelStyleInner);
                GUILayout.Label("Registered from", GUILayout.Width(LABEL_WIDTH));
                GUILayout.Label(endpoint.CodeLocation);
                GUILayout.EndHorizontal();

                DrawHorizontalLine();
            }

            return true;
        }

        private void DrawGetEndpoints(GUIStyle foldoutStyle0, GUIStyle labelStyleInner, GUIStyle buttonStyleInner) {
            var rs = (RestServer)target;

            var getEndpoints = rs.EndpointCollection.GetAllEndpoints(HttpMethod.GET);
            if (getEndpoints == null || getEndpoints.Count <= 0) {
                return;
            }

            _foldoutEndpointsGet = EditorGUILayout.Foldout(_foldoutEndpointsGet, "GET", true, foldoutStyle0);
            if (!_foldoutEndpointsGet) {
                return;
            }

            DrawHorizontalLine();
            foreach (var endpoint in getEndpoints) {
                if (endpoint.EndpointRegex != null) {
                    GUILayout.BeginHorizontal(labelStyleInner);
                    GUILayout.Label("Pattern", GUILayout.Width(LABEL_WIDTH));
                    GUILayout.Label(endpoint.EndpointRegex.ToString());
                    GUILayout.EndHorizontal();
                } else {
                    GUILayout.BeginHorizontal(labelStyleInner);
                    GUILayout.Label("Url", GUILayout.Width(LABEL_WIDTH));
                    GUILayout.Label(endpoint.EndpointString);
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal(labelStyleInner);
                GUILayout.Label("WebSocket", GUILayout.Width(LABEL_WIDTH));
                GUILayout.Label(endpoint.WebSocketUpgradeAllowed ? "Yes" : "No");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(labelStyleInner);
                GUILayout.Label("Registered from", GUILayout.Width(LABEL_WIDTH));
                GUILayout.Label(endpoint.CodeLocation);
                GUILayout.EndHorizontal();

                if (endpoint.EndpointRegex == null) {
                    if (GUILayout.Button("Open in Browser", buttonStyleInner)) {
                        Application.OpenURL($"http://localhost:{rs.EffectivePort}{endpoint.EndpointString}");
                    }
                }

                DrawHorizontalLine();
            }
        }

        private static void DrawHorizontalLine() {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RestServer;
using RestServer.AutoEndpoints;
using RestServer.Helper;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestStaticContentHandler {
#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator TestZipContent() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            var ae = th.GoServer.AddComponent<StaticContentAutoEndpoint>();
            ae.rootPath = "/scae";
            ae.useCoroutineInit = false;


            var zipAssetGuid = AssetDatabase.FindAssets("TestSCHZipAsset.zip")[0];
            var zipAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(zipAssetGuid));

            ae.files = new[] {
                new StaticContentAEEntry() {
                    asset = zipAsset,
                    isZip = true,
                    subPath = "/zip"
                },
            };

            ae.enabled = false;
            ae.enabled = true; // re-register endpoints

            yield return th.HttpAsyncGet("/scae/zip/ZipAsset.txt");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("ZipAsset", responseStr);

            // check which endpoints have been registered
            var endpoints = th.RestServer.EndpointCollection.GetAllEndpoints(HttpMethod.GET);
            Assert.AreEqual(3, endpoints.Count);

            var endpointUris = new List<string>();
            foreach (var endpoint in endpoints) {
                endpointUris.Add(endpoint.EndpointString);
            }

            Assert.Contains("/scae/zip/", endpointUris);
            Assert.Contains("/scae/zip/binary.bin", endpointUris);
            Assert.Contains("/scae/zip/ZipAsset.txt", endpointUris);
        }

        [UnityTest]
        public IEnumerator TestValidation() {
            LogAssert.Expect(LogType.Error, new Regex("No asset provided for path <missing>.*"));
            LogAssert.Expect(LogType.Error, new Regex("No content type provided for path </no-content-type>.*"));

            using var th = new TestHelper();
            yield return th.DoStartup();

            var ae = th.GoServer.AddComponent<StaticContentAutoEndpoint>();
            ae.rootPath = "/scae";
            ae.useCoroutineInit = false;

            var zipAssetGuid = AssetDatabase.FindAssets("TestSCHZipAsset.zip")[0];
            var zipAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(zipAssetGuid));

            ae.files = new[] {
                new StaticContentAEEntry() {
                    asset = zipAsset,
                    isZip = true,
                    subPath = "/zip"
                },
                new StaticContentAEEntry() {
                    isZip = true,
                    subPath = "missing"
                },
                new StaticContentAEEntry() {
                    asset = zipAsset,
                    subPath = "/no-content-type"
                }
            };

            ae.enabled = false;
            ae.enabled = true; // re-register endpoints
        }

        [UnityTest]
        public IEnumerator TestZipContent_Coroutine() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae

            using var th = new TestHelper();
            yield return th.DoStartup();

            var ae = th.GoServer.AddComponent<StaticContentAutoEndpoint>();
            ae.rootPath = "/scae";

            var zipAssetGuid = AssetDatabase.FindAssets("TestSCHZipAsset.zip")[0];
            var zipAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(zipAssetGuid));

            ae.files = new[] {
                new StaticContentAEEntry() {
                    asset = zipAsset,
                    isZip = true,
                    subPath = "/zip"
                }
            };

            ae.enabled = false;
            ae.enabled = true; // re-register endpoints

            // wait for zip to be extracted
            while (!ae.IsBuildDone) {
                yield return CICD.SafeWaitForEndOfFrame();
            }

            yield return th.HttpAsyncGet("/scae/zip/ZipAsset.txt");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("ZipAsset", responseStr);

            // map index.html
            yield return th.HttpAsyncGet("/scae/zip/");

            r = th.LastAsyncResponse;
            responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("index.html", responseStr);
        }

        [UnityTest]
        public IEnumerator TestZipContent_Coroutine_NoMap() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae

            using var th = new TestHelper();
            yield return th.DoStartup();

            var ae = th.GoServer.AddComponent<StaticContentAutoEndpoint>();
            ae.rootPath = "/scae";
            ae.mapIndexHtml = false;

            var zipAssetGuid = AssetDatabase.FindAssets("TestSCHZipAsset.zip")[0];
            var zipAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(zipAssetGuid));

            ae.files = new[] {
                new StaticContentAEEntry() {
                    asset = zipAsset,
                    isZip = true,
                    subPath = "/zip",
                }
            };

            ae.enabled = false;
            ae.enabled = true; // re-register endpoints

            // wait for zip to be extracted
            while (!ae.IsBuildDone) {
                yield return CICD.SafeWaitForEndOfFrame();
            }

            // DO NOT map index.html
            yield return th.HttpAsyncGet("/scae/zip/index.html");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("index.html", responseStr);

            yield return th.HttpAsyncGet("/scae/zip/");
            r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.NotFound, r.StatusCode);
        }

        [UnityTest]
        public IEnumerator TestTextContent() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae

            using var th = new TestHelper();
            yield return th.DoStartup();

            var ae = th.GoServer.AddComponent<StaticContentAutoEndpoint>();
            ae.rootPath = "/scae";

            var zipAssetGuid = AssetDatabase.FindAssets("TestSCHTextAsset")[0];
            var zipAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(zipAssetGuid));

            ae.files = new[] {
                new StaticContentAEEntry() {
                    asset = zipAsset,
                    isZip = false,
                    isBinary = false,
                    subPath = "/text",
                    contentType = "custom-type"
                }
            };

            ae.enabled = false;
            ae.enabled = true; // re-register endpoints

            // wait for zip to be extracted
            while (!ae.IsBuildDone) {
                yield return CICD.SafeWaitForEndOfFrame();
            }

            yield return th.HttpAsyncGet("/scae/text");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("TextAsset", responseStr);
        }

        [UnityTest]
        public IEnumerator TestTextContent_Binary() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae

            using var th = new TestHelper();
            yield return th.DoStartup();

            var ae = th.GoServer.AddComponent<StaticContentAutoEndpoint>();
            ae.rootPath = "/scae";

            var zipAssetGuid = AssetDatabase.FindAssets("TestSCHTextAsset")[0];
            var zipAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(zipAssetGuid));

            ae.files = new[] {
                new StaticContentAEEntry() {
                    asset = zipAsset,
                    isZip = false,
                    isBinary = true,
                    subPath = "/text-binary",
                    contentType = "custom-type"
                }
            };

            ae.enabled = false;
            ae.enabled = true; // re-register endpoints

            // wait for zip to be extracted
            while (!ae.IsBuildDone) {
                yield return CICD.SafeWaitForEndOfFrame();
            }

            yield return th.HttpAsyncGet("/scae/text-binary");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("TextAsset", responseStr);
        }

        [UnityTest]
        public IEnumerator TestTextContent_Manual() {
            using var th = new TestHelper();

            var sch = new StaticContentHandler();

            var assetGuid = AssetDatabase.FindAssets("TestSCHTextAsset")[0];
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(assetGuid));

            sch.RegisterContent(th.RestServer, "/text-content", "text-content", MimeType.TEXT_PLAIN);
            sch.RegisterContent(th.RestServer, "/binary-content", new byte[] { 0xAF, 0xFE }, MimeType.APPLICATION_OCTET_STREAM);

            yield return th.DoStartup();

            yield return th.HttpAsyncGet("/text-content");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("text-content", responseStr);

            yield return th.HttpAsyncGet("/binary-content");

            r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
        }

        [UnityTest]
        public IEnumerator TestTextContent_ClearContent() {
            using var th = new TestHelper();

            var sch = new StaticContentHandler();

            var assetGuid = AssetDatabase.FindAssets("TestSCHTextAsset")[0];
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(assetGuid));

            sch.RegisterContent(th.RestServer, "/text-content", "text-content", MimeType.TEXT_PLAIN);
            sch.RegisterContent(th.RestServer, "/binary-content", new byte[] { 0xAF, 0xFE }, MimeType.APPLICATION_OCTET_STREAM);
            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/stay", r => r.CreateResponse().Body("still-available").SendAsync());

            yield return th.DoStartup();

            sch.ClearContent(th.RestServer);

            yield return th.HttpAsyncGet("/text-content");

            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.NotFound, r.StatusCode);

            yield return th.HttpAsyncGet("/stay");

            r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("still-available", responseStr);
        }
#endif
    }
}
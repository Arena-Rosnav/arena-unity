#if ENABLE_PROFILER && RESTSERVER_PROFILING_CORE
using Unity.Profiling;
#if UNITY_2021_3_OR_NEWER && UNITY_EDITOR
using Unity.Profiling.Editor;
#endif

namespace RestServer.Helper {
    #region Counters

    /// <summary>
    /// Unity compatible profiler counters which can be used to measure the performance of the rest server in Unity 2021 or newer.
    /// </summary>
    public static class RestServerProfilerCounters {
        public static readonly ProfilerCategory RestServerProfilerCategory = ProfilerCategory.Scripts;

        public const string IncomingBytesCountName = "Incoming Bytes";

        /// <summary>
        /// Amount of bytes that the server receives "per frame". Only body bytes are counted, no header bytes.
        /// </summary>
        public static readonly ProfilerCounterValue<long> IncomingBytesCount =
            new ProfilerCounterValue<long>(RestServerProfilerCategory, IncomingBytesCountName, ProfilerMarkerDataUnit.Bytes,
                ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

        public const string IncomingRequestsCountName = "Incoming Requests";

        /// <summary>
        /// Count of incoming requests "per frame"
        /// </summary>
        public static readonly ProfilerCounterValue<int> IncomingRequestsCount =
            new ProfilerCounterValue<int>(RestServerProfilerCategory, IncomingRequestsCountName, ProfilerMarkerDataUnit.Count,
                ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

        public const string OutgoingBytesCountName = "Outgoing Bytes";

        /// <summary>
        /// Outgoing body bytes "per frame"
        /// </summary>
        public static readonly ProfilerCounterValue<long> OutgoingBytesCount =
            new ProfilerCounterValue<long>(RestServerProfilerCategory, OutgoingBytesCountName, ProfilerMarkerDataUnit.Bytes,
                ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

        public const string ThreadingHelperCallsCountName = "Threading Helper Calls";

        /// <summary>
        /// Count of calls that the ThreadingHelper has scheduled on the main rendering thread in this frame.
        /// </summary>
        public static readonly ProfilerCounterValue<int> ThreadingHelperCallsCount =
            new ProfilerCounterValue<int>(RestServerProfilerCategory, ThreadingHelperCallsCountName, ProfilerMarkerDataUnit.Count,
                ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

        public const string ThreadingHelperMainThreadBlockTimeName = "Threading Helper Main Thread Blocking Time (ms)";

        /// <summary>
        /// Milliseconds the ThreadingHelper workload has blocked the rendering thread.
        /// </summary>
        public static readonly ProfilerCounterValue<long> ThreadingHelperMainThreadBlockTime =
            new ProfilerCounterValue<long>(RestServerProfilerCategory, ThreadingHelperMainThreadBlockTimeName, ProfilerMarkerDataUnit.Count,
                ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);
    }

    #endregion

    #region Category

#if UNITY_2021_3_OR_NEWER && UNITY_EDITOR
    [System.Serializable]
    [ProfilerModuleMetadata("Rest Server Statistics")]
    public class RestServerProfileModule : ProfilerModule {
        private static readonly ProfilerCounterDescriptor[] _counters = {
            new(RestServerProfilerCounters.IncomingBytesCountName, RestServerProfilerCounters.RestServerProfilerCategory),
            new(RestServerProfilerCounters.IncomingRequestsCountName, RestServerProfilerCounters.RestServerProfilerCategory),
            new(RestServerProfilerCounters.OutgoingBytesCountName, RestServerProfilerCounters.RestServerProfilerCategory),
            new(RestServerProfilerCounters.ThreadingHelperCallsCountName, RestServerProfilerCounters.RestServerProfilerCategory),
            new(RestServerProfilerCounters.ThreadingHelperMainThreadBlockTimeName, RestServerProfilerCounters.RestServerProfilerCategory)
        };

        private static readonly string[] _autoEnabledCategoryNames = {
            ProfilerCategory.Scripts.Name, ProfilerCategory.Memory.Name
        };

        public RestServerProfileModule() : base(_counters, autoEnabledCategoryNames: _autoEnabledCategoryNames) { }
    }
#endif
    
    #endregion
    
}
#endif
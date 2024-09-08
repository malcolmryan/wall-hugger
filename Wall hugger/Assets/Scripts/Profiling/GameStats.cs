//
// Storage for profiling data
//
// See: https://docs.unity3d.com/Manual/Profiler-customizing-details-view.html

using Unity.Profiling;

class GameStats
{
    public static readonly ProfilerCategory MyProfilerCategory = ProfilerCategory.Scripts;

    public const string Name = "Player";
 
    // Note that these name strings need to match the names used in the ProfilerModule
    public static readonly ProfilerCounterValue<float> PlayerSpeedX =
        new ProfilerCounterValue<float>(MyProfilerCategory, "SpeedX", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounterValue<float> PlayerSpeedY =
        new ProfilerCounterValue<float>(MyProfilerCategory, "SpeedY", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounterValue<float> PlayerPosX =
        new ProfilerCounterValue<float>(MyProfilerCategory, "PosX", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounterValue<float> PlayerPosY =
        new ProfilerCounterValue<float>(MyProfilerCategory, "PosY", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounterValue<float> NContacts =
        new ProfilerCounterValue<float>(MyProfilerCategory, "Contacts", ProfilerMarkerDataUnit.Count);

}



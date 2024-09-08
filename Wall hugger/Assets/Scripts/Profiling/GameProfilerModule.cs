 //
 // Adds a custom module to the Profiler
 // 
 // See: https://docs.unity3d.com/Manual/Profiler-customizing-details-view.html
 //
 
 using Unity.Profiling;
 using Unity.Profiling.Editor;

 [System.Serializable]
 [ProfilerModuleMetadata("Player")] 
 public class GameProfilerModule : ProfilerModule
 {
    static readonly ProfilerCounterDescriptor[] k_Counters = new ProfilerCounterDescriptor[]
    {
        // Note that these name strings have to match those in GameStats
        new ProfilerCounterDescriptor("SpeedX", GameStats.MyProfilerCategory),
        new ProfilerCounterDescriptor("SpeedY", GameStats.MyProfilerCategory),
        new ProfilerCounterDescriptor("PosX", GameStats.MyProfilerCategory),
        new ProfilerCounterDescriptor("PosY", GameStats.MyProfilerCategory),
        new ProfilerCounterDescriptor("Contacts", GameStats.MyProfilerCategory),
    };

    public GameProfilerModule() : base(k_Counters) { }
}
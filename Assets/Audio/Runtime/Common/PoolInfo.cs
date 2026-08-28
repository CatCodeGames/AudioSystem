using UnityEngine.Pool;

#if CATCODE_AUDIO_UNITASK_SUPPORT
#endif

namespace CatCode.Audio
{
    public readonly struct PoolInfo
    {
        public readonly int CountAll;
        public readonly int CountActive;
        public readonly int CountInactive;

        public PoolInfo(int countAll, int countActive, int countInactive)
        {
            CountAll = countAll;
            CountActive = countActive;
            CountInactive = countInactive;
        }

        public static PoolInfo Create<T>(ObjectPool<T> pool) where T : class
            => new(pool.CountAll, pool.CountActive, pool.CountInactive);

    }
}
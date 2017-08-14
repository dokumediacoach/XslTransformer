using System;

namespace XslTransformer.Core
{
    public interface IReadSettings
    {
        /// <summary>
        /// Retrieves the generic value of a Setting by setting-key
        /// </summary>
        /// <typeparam name="T">The return type to convert the setting value to</typeparam>
        /// <param name="setting">The setting to retrieve</param>
        /// <returns>The setting value of the specified generic type</returns>
        T GetValue<T>(Setting setting) where T : struct, IComparable;

        /// <summary>
        /// Loads default settings
        /// </summary>
        void LoadDefaults();

        /// <summary>
        /// Loads persistent configuration and syncs settings from it.
        /// </summary>
        void SyncFromConfiguration();
    }
}

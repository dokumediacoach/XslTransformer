namespace XslTransformer.Core
{
    public interface IReadWriteSettings : IReadSettings
    {
        /// <summary>
        /// Sets the value of a Setting by setting-key
        /// </summary>
        /// <param name="setting">The setting to set</param>
        /// <param name="settingValue">The setting value of non-specific type</param>
        void Set(Setting setting, object settingValue);

        /// <summary>
        /// Persists the settings in configuration
        /// </summary>
        void SaveToConfiguration();
    }
}

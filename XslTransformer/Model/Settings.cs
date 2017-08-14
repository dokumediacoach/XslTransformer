using System;
using System.Collections.Generic;
using System.Configuration;
using XslTransformer.Core;

namespace XslTransformer
{
    /// <summary>
    /// The settings for XslTransformer
    /// </summary>
    public class Settings : IReadSettings, IReadWriteSettings
    {
        /// <summary>
        /// Stores Settings with Setting (key), Type and Value
        /// </summary>
        private Dictionary<Setting, SettingTypeValue> mSettings = new Dictionary<Setting, SettingTypeValue>();

        /// <summary>
        /// Stores reference to exe configuration file
        /// </summary>
        private Configuration mConfiguration;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <remarks>
        /// Loading settings default into mSettings, then syncing exe configuration settings
        /// </remarks>
        public Settings()
        {
            LoadDefaults();
            SyncFromConfiguration();
        }

        /// <summary>
        /// Loads default settings
        /// </summary>
        public void LoadDefaults()
        {
            mSettings.Remove(Setting.DtdProcessing);
            mSettings.Add(Setting.DtdProcessing, new SettingTypeValue()
            {
                Type = typeof(XmlReaderDtdProcessing),
                Value = XmlReaderDtdProcessing.Prohibit
            });
            mSettings.Remove(Setting.ValidationType);
            mSettings.Add(Setting.ValidationType, new SettingTypeValue()
            {
                Type = typeof(XmlReaderValidationType),
                Value = XmlReaderValidationType.None
            });
            mSettings.Remove(Setting.CheckCharacters);
            mSettings.Add(Setting.CheckCharacters, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = true
            });
            mSettings.Remove(Setting.XsdValidationFlag_AllowXmlAttributes);
            mSettings.Add(Setting.XsdValidationFlag_AllowXmlAttributes, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = false
            });
            mSettings.Remove(Setting.XsdValidationFlag_ProcessIdentityConstraints);
            mSettings.Add(Setting.XsdValidationFlag_ProcessIdentityConstraints, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = true
            });
            mSettings.Remove(Setting.XsdValidationFlag_ProcessSchemaLocation);
            mSettings.Add(Setting.XsdValidationFlag_ProcessSchemaLocation, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = true
            });
            mSettings.Remove(Setting.XsdValidationFlag_ProcessInlineSchema);
            mSettings.Add(Setting.XsdValidationFlag_ProcessInlineSchema, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = false
            });
            mSettings.Remove(Setting.XsdValidationFlag_ReportValidationWarnings);
            mSettings.Add(Setting.XsdValidationFlag_ReportValidationWarnings, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = false
            });
            mSettings.Remove(Setting.EnableDocumentFunction);
            mSettings.Add(Setting.EnableDocumentFunction, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = true
            });
            mSettings.Remove(Setting.EnableScript);
            mSettings.Add(Setting.EnableScript, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = true
            });
            mSettings.Remove(Setting.WriteUtf8Bom);
            mSettings.Add(Setting.WriteUtf8Bom, new SettingTypeValue()
            {
                Type = typeof(bool),
                Value = true
            });
        }

        /// <summary>
        /// Syncs setting from configuration, overwriting setting values
        /// </summary>
        public void SyncFromConfiguration()
        {
            mConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (KeyValuePair<Setting, SettingTypeValue> s in mSettings)
            {
                string configKey = s.Key.ToString();
                string configValue = mConfiguration.AppSettings.Settings[configKey]?.Value;
                if (configValue == null) 
                    continue;
                if (s.Value.Type.IsEnum)
                {
                    var enumValue = Activator.CreateInstance(s.Value.Type);
                    try
                    {
                        enumValue = Enum.Parse(s.Value.Type, configValue);
                    }
                    catch (Exception)
                    {
                        enumValue = Enum.ToObject(s.Value.Type, 0);
                    }
                    s.Value.Value = enumValue;
                    continue;
                }
                if (s.Value.Type.Equals(typeof(bool)))
                {
                    bool boolValue;
                    Boolean.TryParse(configValue, out boolValue);
                    s.Value.Value = boolValue;
                }
            }
        }

        /// <summary>
        /// Retrieves the generic value of a setting by Setting-key
        /// </summary>
        /// <typeparam name="T">The return type to convert the setting value to</typeparam>
        /// <param name="key">The Setting to retrieve</param>
        /// <returns>The setting value of the specified generic type</returns>
        public T GetValue<T>(Setting key)
            where T : struct, IComparable
        {
            Type type = mSettings[key]?.Type;
            if (type == null || !type.Equals(typeof(T)))
                return default(T);
            if (type.IsEnum)
            {
                T parsedValue;
                Enum.TryParse(Convert.ToString(mSettings[key].Value), out parsedValue);
                return parsedValue;
            }
            return (T)mSettings[key].Value;
        }

        /// <summary>
        /// Sets the value of a setting by Setting-key
        /// </summary>
        /// <param name="key">The Setting to set</param>
        /// <param name="settingValue">The setting value of non-specific type</param>
        public void Set(Setting key, object settingValue)
        {
            mSettings[key].Value = settingValue;
        }

        /// <summary>
        /// Saves the settings to exe configuration file
        /// </summary>
        public void SaveToConfiguration()
        {
            foreach (KeyValuePair<Setting, SettingTypeValue> s in mSettings)
            {
                string configKey = s.Key.ToString();
                var settingValue = Convert.ChangeType(s.Value.Value, s.Value.Type);
                string configValue = Convert.ToString(settingValue);
                mConfiguration.AppSettings.Settings.Remove(configKey);
                mConfiguration.AppSettings.Settings.Add(configKey, configValue);
            }
            mConfiguration.Save();
        }
    }

    /// <summary>
    /// Object for the Settings Dictionary value
    /// </summary>
    /// <remarks>
    /// Storing Type and object value for flexible type conversion
    /// </remarks>
    public class SettingTypeValue
    {
        public Type Type { get; set; }
        public object Value { get; set; }
    }
}

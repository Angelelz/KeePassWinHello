﻿using System;
using KeePass.App.Configuration;

namespace KeePassWinHello
{
    class Settings
    {
        private Settings()
        {
        }

        private const string CFG_VALID_PERIOD = "WindowsHello.QuickUnlock.ValidPeriod";
        private const string CFG_ENABLED = "WindowsHello.QuickUnlock.Enabled";
        private const string CFG_WINSTORAGE_ENABLED = "WindowsHello.QuickUnlock.WindowsStorage.Enabled";
        private const string CFG_REVOKE_ON_CANCEL = "WindowsHello.QuickUnlock.RevokeOnCancel";

        private static Lazy<Settings> _instance = new Lazy<Settings>(() => new Settings(), true);
        private AceCustomConfig _customConfig;

        private void UpgradeConfig()
        {
            const string deprecatedCfgAutoPrompt = "WindowsHello_AutoPrompt";
            const string deprecatedCfgValidPeriod = "WindowsHello_ValidPeriod";

            _customConfig.SetString(deprecatedCfgAutoPrompt, null);

            long validPeriod = _customConfig.GetLong(deprecatedCfgValidPeriod, -1);
            if (validPeriod != -1)
            {
                _customConfig.SetString(deprecatedCfgValidPeriod, null);

                validPeriod *= 1000;
                _customConfig.SetLong(CFG_VALID_PERIOD, validPeriod);
            }
        }

        public void Initialize(AceCustomConfig customConfig)
        {
            if (customConfig == null)
                throw new ArgumentNullException("customConfig");
            if (_customConfig != null)
                throw new InvalidOperationException("Settings have initialized already");

            _customConfig = customConfig;

            UpgradeConfig();
        }

        public static readonly Settings Instance = _instance.Value;

        public TimeSpan InvalidatingTime
        {
            get
            {
                var ms = _customConfig.GetLong(CFG_VALID_PERIOD, VALID_PERIOD_DEFAULT);
                if (ms <= 0)
                    ms = VALID_UNLIMITED_PERIOD;
                return TimeSpan.FromMilliseconds(ms);
            }
            set
            {
                var ms = (long)value.TotalMilliseconds;
                if (ms == VALID_UNLIMITED_PERIOD)
                    ms = 0;
                _customConfig.SetLong(CFG_VALID_PERIOD, ms);
            }
        }

        public bool Enabled
        {
            get
            {
                return _customConfig.GetBool(CFG_ENABLED, true);
            }
            set
            {
                _customConfig.SetBool(CFG_ENABLED, value);
            }
        }

        public bool WinStorageEnabled
        {
            get
            {
                return _customConfig.GetBool(CFG_WINSTORAGE_ENABLED, false);
            }
            set
            {
                _customConfig.SetBool(CFG_WINSTORAGE_ENABLED, value);
            }
        }

        public bool RevokeOnCancel
        {
            get
            {
                return _customConfig.GetBool(CFG_REVOKE_ON_CANCEL, true);
            }
            set
            {
                _customConfig.SetBool(CFG_REVOKE_ON_CANCEL, value);
            }
        }

        public const long VALID_PERIOD_DEFAULT = 1000 * 60 * 60 * 24; // one day in ms
        public const long VALID_UNLIMITED_PERIOD = 922337203685476; // TimeSpan.MaxValue.TotalMilliseconds - 1

        public const string DecryptConfirmationMessage = "Authentication to access KeePass database";
        public const string KeyCreationConfirmationMessage = "KeePassWinHello requires for a signed persistent key";
        public const string OptionsTabName = "WindowsHello";
        public const string ProductName = "KeePassWinHello";
    }

    internal static class SettingsExtension
    {
        public static AuthCacheType GetAuthCacheType(this Settings settings)
        {
            return settings.WinStorageEnabled ? AuthCacheType.Persistent : AuthCacheType.Local;
        }
    }
}

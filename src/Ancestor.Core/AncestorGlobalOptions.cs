﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    public static class AncestorGlobalOptions
    {
        private static bool _enabledTimeout = false;
        private static int _timeoutInterval = 10 * 1000; // 10sec
        private static string _lzPwSecretPref = "LZPW_";
        private static string _lzPwSecretNode = "";
        private static string _lzPwSecretNodePref = "LZPWN_";
        private static bool _lzPw = false;
        private static string _lzPwDataSource;
        private static string _lzPwConnectionString;
        private static bool _enabledDebug= false;
        private static bool _lzPwEnabledClearPool = true;
        private static bool _autoClose = true;
        private static bool _validateConn = true;

        static AncestorGlobalOptions()
        {
            bool enabled;
            if (bool.TryParse(ConfigurationManager.AppSettings["ancestor.option.timeout.enable"], out enabled))
                _enabledTimeout = enabled;
            if (int.TryParse(ConfigurationManager.AppSettings["ancestor.option.timeout.interval"], out int interval))
                _timeoutInterval = interval;
            var lazyPrefix = ConfigurationManager.AppSettings["ancestor.option.lzpw.prefix"];
            if (lazyPrefix != null)
                _lzPwSecretPref = lazyPrefix;
            var lazyNodePrefix = ConfigurationManager.AppSettings["ancestor.option.lzpw.node.prefix"];
            if (lazyNodePrefix != null)
                _lzPwSecretNodePref = lazyNodePrefix;
            var lazyNode = ConfigurationManager.AppSettings["ancestor.option.lzpw.node"];
            if (lazyNode != null)
                _lzPwSecretNode = lazyNode;
            if (bool.TryParse(ConfigurationManager.AppSettings["ancestor.option.debug"], out bool debug))
                _enabledDebug = debug;
            if (bool.TryParse(ConfigurationManager.AppSettings["ancestor.option.lzpw.enable"], out bool lzpw))
                _lzPw = lzpw;
            var lzPwDsn = ConfigurationManager.AppSettings["ancestor.option.lzpw.node.dsn"];
            if (lzPwDsn != null)
                _lzPwDataSource = lzPwDsn;
            var lzPwConnStr = ConfigurationManager.AppSettings["ancestor.option.lzpw.node.connstr"];
            if (lzPwConnStr != null)
                _lzPwConnectionString = lzPwConnStr;
            if (bool.TryParse(ConfigurationManager.AppSettings["ancestor.option.lzpw.clearpool"], out enabled))
                _lzPwEnabledClearPool = enabled;
            var closeType = ConfigurationManager.AppSettings["ancestor.option.close"];
            if ("manual".Equals(closeType, StringComparison.OrdinalIgnoreCase))
                _autoClose = false;
            if (bool.TryParse(ConfigurationManager.AppSettings["ancestor.option.validatable"], out enabled))
                _validateConn = enabled;
        }
        public static bool Debug
        {
            get { return _enabledDebug; }
            set { _enabledDebug = value; }
        }
        public static bool EnableTimeout
        {
            get { return _enabledTimeout; }
            set { _enabledTimeout = value; }
        }
        public static int TimeoutInterval
        {
            get { return _timeoutInterval; }
            set { _timeoutInterval = value; }
        }
        public static string LazyPasswordSecretKeyPrefix
        {
            get { return _lzPwSecretPref; }
            set { _lzPwSecretPref = value; }
        }
        public static string LazyPasswordSecretKeyNode
        {
            get { return _lzPwSecretNode; }
            set { _lzPwSecretNode = value; }
        }
        public static string LazyPasswordSecretKeyNodePrefix
        {
            get { return _lzPwSecretNodePref; }
            set { _lzPwSecretNodePref = value; }
        }
        public static bool GlobalLazyPassword
        {
            get { return _lzPw; }
            set { _lzPw = value; }
        }

        public static string GlobalLazyPasswordDataSource
        {
            get { return _lzPwDataSource; }
            set { _lzPwDataSource = value; }
        }
        public static string GlobalLazyPasswordConnectionString
        {
            get { return _lzPwConnectionString; }
            set { _lzPwConnectionString = value; }
        }

        public static bool LazyPasswordEnableClearPool
        {
            get { return _lzPwEnabledClearPool; }
            set { _lzPwEnabledClearPool = value; }
        }
        public static bool AutoClose
        {
            get { return _autoClose; }
            set { _autoClose = value; }
        }
        public static bool ValidateConnection
        {
            get { return _validateConn; }
            set { _validateConn = value; }
        }
    }
}

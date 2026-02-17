using System.Configuration;

namespace OfficeManagerWPF.Properties
{
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.0.0")]
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public string SmsApiKey
        {
            get
            {
                return ((string)(this["SmsApiKey"]));
            }
            set
            {
                this["SmsApiKey"] = value;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public string SmsApiSecret
        {
            get
            {
                return ((string)(this["SmsApiSecret"]));
            }
            set
            {
                this["SmsApiSecret"] = value;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public string SmsSenderNumber
        {
            get
            {
                return ((string)(this["SmsSenderNumber"]));
            }
            set
            {
                this["SmsSenderNumber"] = value;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public string EmailAddress
        {
            get
            {
                return ((string)(this["EmailAddress"]));
            }
            set
            {
                this["EmailAddress"] = value;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public string EmailPassword
        {
            get
            {
                return ((string)(this["EmailPassword"]));
            }
            set
            {
                this["EmailPassword"] = value;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("smtp.gmail.com")]
        public string SmtpServer
        {
            get
            {
                return ((string)(this["SmtpServer"]));
            }
            set
            {
                this["SmtpServer"] = value;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("587")]
        public int SmtpPort
        {
            get
            {
                return ((int)(this["SmtpPort"]));
            }
            set
            {
                this["SmtpPort"] = value;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("True")]
        public bool AutoNotificationEnabled
        {
            get
            {
                return ((bool)(this["AutoNotificationEnabled"]));
            }
            set
            {
                this["AutoNotificationEnabled"] = value;
            }
        }
    }
}

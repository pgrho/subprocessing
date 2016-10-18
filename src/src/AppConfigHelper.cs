using Shipwreck.Subprocessing.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Shipwreck.Subprocessing
{
    /// <summary>
    /// サブプロセスのアプリケーション構成ファイルの操作するメソッドを提供します。
    /// </summary>
    public sealed class AppConfigHelper
    {
        private readonly string _FileName;
        private XmlDocument _Document;


        private bool _HasEdited;

        /// <summary>
        /// アプリケーション構成ファイルを指定して<see cref="AppConfigHelper" />クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="fileName">アプリケーション構成ファイル。</param>
        public AppConfigHelper(string fileName)
        {
            _FileName = fileName;

            _Document = new XmlDocument();
            using (var xr = XmlReader.Create(fileName, new XmlReaderSettings() { IgnoreComments = true }))
            {
                _Document.Load(xr);
            }
        }

        public XmlDocument Document
        {
            get
            {
                _HasEdited = true;
                return _Document;
            }
        }

        public void SetConnectionString(Expression<Func<string>> memberSelector, string connectionString, string providerName = null)
        {
            var prop = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            var se = prop.GetCustomAttribute<ApplicationScopedSettingAttribute>();
            if (se == null)
            {
                throw new ArgumentException(string.Format(Resources.Arg0MustBeAPropertyExpressionWithArg1, "memberSelector", typeof(ApplicationScopedSettingAttribute).FullName));
            }
            SetConnectionString(prop.DeclaringType.FullName + "." + prop.Name, connectionString, providerName);
        }
        public void SetConnectionString(string name, string connectionString, string providerName = null)
        {
            var css = _Document.DocumentElement.GetOrAppend("connectionStrings");
            css.RemoveByName("add", name);
            var add = _Document.CreateElement("add");
            add.SetAttribute("name", name);
            add.SetAttribute("connectionString", connectionString);
            if (!string.IsNullOrEmpty(providerName))
            {
                add.SetAttribute("providerName", providerName);
            }
            css.AppendChild(add);
            _HasEdited = true;
        }

        #region SetClientSetting メソッド

        public void SetClientSetting<T>(Expression<Func<T>> memberSelector, string value)
        {
            SetClientSetting(memberSelector, value, SettingsSerializeAs.String);
        }

        public void SetClientSettingAsXml<T>(Expression<Func<T>> memberSelector, string value)
        {
            SetClientSetting(memberSelector, value, SettingsSerializeAs.Xml);
        }

        public void SetClientSetting<T>(Expression<Func<T>> memberSelector, byte[] value)
        {
            SetClientSetting(memberSelector, value == null ? null : Convert.ToBase64String(value), SettingsSerializeAs.Binary);
        }

        private void SetClientSetting<T>(Expression<Func<T>> memberSelector, string value, SettingsSerializeAs serializeAs)
        {
            var prop = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            var se = prop.GetCustomAttribute<SettingAttribute>();
            if (se == null)
            {
                throw new ArgumentException(string.Format(Resources.Arg0MustBeAPropertyExpressionWithArg1, "memberSelector", typeof(SettingAttribute).FullName));
            }
            var isUser = se is UserScopedSettingAttribute;
            if (!isUser)
            {
                var sp = prop.GetCustomAttribute<SpecialSettingAttribute>();
                if (sp != null)
                {
                    switch (sp.SpecialSetting)
                    {
                        case SpecialSetting.ConnectionString:
                            SetConnectionString(prop.DeclaringType.FullName + "." + prop.Name, value);
                            return;
                    }
                }
            }
            SetClientSetting(isUser, prop.DeclaringType.FullName, prop.Name, value, serializeAs);
        }

        private void SetClientSetting(bool isUserSettings, string section, string name, string value, SettingsSerializeAs serializeAs)
        {
            string sectionGroupName;
            Type sectionGroupType;
            if (isUserSettings)
            {
                sectionGroupName = "userSettings";
                sectionGroupType = typeof(UserSettingsGroup);
            }
            else
            {
                sectionGroupName = "applicationSettings";
                sectionGroupType = typeof(ApplicationSettingsGroup);
            }

            var cs = _Document.DocumentElement.GetOrPrepend("configSections");
            var configSectionsSectionGroup = cs.GetByNameOrAppend("sectionGroup", sectionGroupName, "name", sectionGroupName, "type", sectionGroupType.AssemblyQualifiedName);

            string[] sectionAttrs;
            if (isUserSettings)
            {
                sectionAttrs = new string[8];
                sectionAttrs[6] = "allowExeDefinition";
                sectionAttrs[7] = "MachineToLocalUser";
            }
            else
            {
                sectionAttrs = new string[6];
            }
            sectionAttrs[0] = "name";
            sectionAttrs[1] = section;
            sectionAttrs[2] = "type";
            sectionAttrs[3] = typeof(ClientSettingsSection).AssemblyQualifiedName;
            sectionAttrs[4] = "requirePermission";
            sectionAttrs[5] = "false";
            var configSectionsSection = configSectionsSectionGroup.GetByNameOrAppend("section", section, sectionAttrs);

            var sectionGroupElement = _Document.DocumentElement.GetOrAppend(sectionGroupName);

            var sectionElement = sectionGroupElement.GetOrAppend(section);

            sectionElement.RemoveByName("setting", name);

            var setting = _Document.CreateElement("setting");
            setting.SetAttribute("name", name);
            setting.SetAttribute("serializeAs", serializeAs.ToString());
            sectionElement.AppendChild(setting);

            if (serializeAs == SettingsSerializeAs.Xml)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    setting.InnerXml = value;
                }
            }
            else
            {
                var v = _Document.CreateElement("value");
                v.InnerText = value;

                setting.AppendChild(v);
            }
            _HasEdited = true;
        }

        #endregion SetClientSetting メソッド



        public void Save()
        {
            if (_Document != null && _HasEdited)
            {
                using (var sw = new StreamWriter(_FileName, false, Encoding.UTF8))
                {
                    _Document.Save(sw);
                }
            }
        }
    }
}

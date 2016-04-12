﻿using UnityEngine;
using System.IO;
using System.Text;
using CosmosTable;

namespace KEngine.Modules
{
    /// <summary>
    /// Unity SettingModule, with Resources.Load
    /// </summary>
    public class SettingModule : SettingModuleBase
    {
        /// <summary>
        /// Load KEngineConfig.txt 's `SettingPath`
        /// </summary>
        protected string SettingFolderName
        {
            get
            {
                return Path.GetFileName(AppEngine.GetConfig("SettingPath"));
            }
        }

        /// <summary>
        /// Singleton
        /// </summary>
        private static SettingModule _instance;

        /// <summary>
        /// Quick method to get TableFile from instance
        /// </summary>
        /// <param name="path"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public static TableFile Get(string path, bool useCache = false)
        {
            if (_instance == null)
                _instance = new SettingModule();
            return _instance.GetTableFile(path, useCache);
        }

        /// <summary>
        /// Unity Resources.Load setting file in Resources folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected override string LoadSetting(string path)
        {
            var ext = Path.GetExtension(path);
            var resPath = SettingFolderName + "/" + Path.ChangeExtension(path, null);
            var fileContentAsset = Resources.Load(resPath) as TextAsset;
            var fileContent = Encoding.UTF8.GetString(fileContentAsset.bytes);

            return fileContent;
        }
    }
}
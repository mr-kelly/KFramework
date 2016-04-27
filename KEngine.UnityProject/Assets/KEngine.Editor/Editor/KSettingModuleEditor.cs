﻿#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KSettingModuleEditor.cs
// Date:     2015/12/03
// Author:  Kelly
// Email: 23110388@qq.com
// Github: https://github.com/mr-kelly/KEngine
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CosmosTable;
using DotLiquid;
using KUnityEditorTools;
using UnityEditor;
using UnityEngine;

namespace KEngine.Editor
{
    /// <summary>
    /// For SettingModule
    /// </summary>
    [InitializeOnLoad]
    public class KSettingModuleEditor
    {
        /// <summary>
        /// 是否自动在编译配置表时生成静态代码，如果不需要，外部设置false
        /// </summary>
        public static bool AutoGenerateCode = true;

        /// <summary>
        /// 当生成的类名，包含数组中字符时，不生成代码
        /// </summary>
        /// <example>
        /// GenerateCodeFilesFilter = new []
        /// {
        ///     "SubdirSubSubDirExample3",
        /// };
        /// </example>
        public static string[] GenerateCodeFilesFilter = null;

        public delegate string CustomExtraStringDelegate(TableCompileResult tableCompileResult);

        /// <summary>
        /// 可以为模板提供额外生成代码块！返回string即可！
        /// 自定义[InitializeOnLoad]的类并设置这个委托
        /// </summary>
        public static CustomExtraStringDelegate CustomExtraString;

        /// <summary>
        /// 编译出的后缀名
        /// </summary>
        public static string SettingExtension = ".bytes";

        /// <summary>
        /// 生成代码吗？它的路径配置
        /// </summary>
        public static string SettingCodePath = "Assets/AppSettings.cs";

        public static string GenCodeTemplate = @"
#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Asset Bundle framework for Unity3D
// ===================================
// 
// Author:  Kelly
// Email: 23110388@qq.com
// Github: https://github.com/mr-kelly/KEngine
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.

#endregion

// This file is auto generated by KSettingModuleEditor.cs!
// Don't manipulate me!

using System.Collections;
using System.Collections.Generic;
using CosmosTable;
using KEngine;
using KEngine.Modules;
namespace {{ NameSpace }}
{
	/// <summary>
    /// All settings list here, so you can reload all settings manully from the list.
	/// </summary>
    public partial class SettingsDefine
    {
        private static IReloadableSettings[] _settingsList;
        public static IReloadableSettings[] SettingsList
        {
            get
            {
                if (_settingsList == null)
                {
                    _settingsList = new IReloadableSettings[]
                    { {% for file in Files %}
                        {{ file.ClassName }}Settings.GetInstance(),{% endfor %}
                    };
                }
                return _settingsList;
            }
        }
#if UNITY_EDITOR
        static bool HasAllReload = false;
        [UnityEditor.MenuItem(""KEngine/Settings/Try Reload All Settings Code"")]
	    public static void AllSettingsReload()
	    {
	        for (var i = 0; i < SettingsList.Length; i++)
	        {
	            var settings = SettingsList[i];
                if (HasAllReload) settings.ReloadAll();
                HasAllReload = true;

	            KLogger.Log(""Reload settings: {0}, Row Count: {1}"", settings.GetType(), settings.Count);

	        }
	    }

#endif
    }

{% for file in Files %}
	/// <summary>
	/// Auto Generate for Tab File: {{ file.TabFilePaths }}
    /// No use of generic and reflection, for better performance,  less IL code generating
	/// </summary>>
    public partial class {{file.ClassName}}Settings : IReloadableSettings
    {
		public static readonly string[] TabFilePaths = 
        {
            {{ file.TabFilePaths }}
        };
        static {{file.ClassName}}Settings _instance;
        Dictionary<{{ file.PrimaryKeyField.FormatType }}, {{file.ClassName}}Setting> _dict = new Dictionary<{{ file.PrimaryKeyField.FormatType }}, {{file.ClassName}}Setting>();

        /// <summary>
        /// Trigger delegate when reload the Settings
        /// </summary>>
	    public static System.Action OnReload;

        /// <summary>
        /// Constructor, just reload(init)
        /// When Unity Editor mode, will watch the file modification and auto reload
        /// </summary>
	    private {{file.ClassName}}Settings()
	    {
        }

        /// <summary>
        /// Get the singleton
        /// </summary>
        /// <returns></returns>
	    public static {{file.ClassName}}Settings GetInstance()
	    {
            if (_instance == null) 
            {
                _instance = new {{file.ClassName}}Settings();

                _instance.ReloadAll();
    #if UNITY_EDITOR
                if (SettingModule.IsFileSystemMode)
                {
                    for (var j = 0; j < TabFilePaths.Length; j++)
                    {
                        var tabFilePath = TabFilePaths[j];
                        SettingModule.WatchSetting(tabFilePath, (path) =>
                        {
                            if (path.Replace(""\\"", ""/"").EndsWith(path))
                            {
                                _instance.ReloadAll();
                                KLogger.LogConsole_MultiThread(""Reload success! -> "" + path);
                            }
                        });
                    }

                }
    #endif
            }
	        return _instance;
	    }
        
        public int Count
        {
            get
            {
                return _dict.Count;
            }
        }

        /// <summary>
        /// Do reload the setting file: {{ file.ClassName }}
        /// </summary>
	    public void ReloadAll()
        {
            for (var j = 0; j < TabFilePaths.Length; j++)
            {
                var tabFilePath = TabFilePaths[j];
                using (var tableFile = SettingModule.Get(tabFilePath, false))
                {
                    foreach (var row in tableFile)
                    {
                        var pk = {{ file.ClassName }}Setting.ParsePrimaryKey(row);
                        {{file.ClassName}}Setting setting;
                        if (!_dict.TryGetValue(pk, out setting))
                        {
                            setting = new {{file.ClassName}}Setting(row);
                            _dict[setting.{{ file.PrimaryKeyField.Name }}] = setting;
                        }
                        else setting.Reload(row);
                    }
                }
            }

	        if (OnReload != null)
	        {
	            OnReload();
	        }
        }

	    /// <summary>
        /// foreachable enumerable: {{ file.ClassName }}
        /// </summary>
        public static IEnumerable GetAll()
        {
            foreach (var row in GetInstance()._dict.Values)
            {
                yield return row;
            }
        }

        /// <summary>
        /// GetEnumerator for `MoveNext`: {{ file.ClassName }}
        /// </summary> 
	    public static IEnumerator GetEnumerator()
	    {
	        return GetInstance()._dict.Values.GetEnumerator();
	    }
         
	    /// <summary>
        /// Get class by primary key: {{ file.ClassName }}
        /// </summary>
        public static {{file.ClassName}}Setting Get({{ file.PrimaryKeyField.FormatType }} primaryKey)
        {
            {{file.ClassName}}Setting setting;
            if (GetInstance()._dict.TryGetValue(primaryKey, out setting)) return setting;
            return null;
        }

        // ========= CustomExtraString begin ===========
        {% if file.Extra %}{{ file.Extra }}{% endif %}
        // ========= CustomExtraString end ===========
    }

	/// <summary>
	/// Auto Generate for Tab File: {{ file.TabFilePaths }}
    /// Singleton class for less memory use
	/// </summary>
	public partial class {{file.ClassName}}Setting : TableRowParser
	{
		{% for field in file.Fields %}
        /// <summary>
        /// {{ field.Comment }}
        /// </summary>
        public {{ field.FormatType }} {{ field.Name}} { get; private set;}
        {% endfor %}

        internal {{file.ClassName}}Setting(TableRow row)
        {
            Reload(row);
        }

        internal void Reload(TableRow row)
        { {% for field in file.Fields %}
            {{ field.Name}} = row.Get_{{ field.TypeMethod }}(row.Values[{{ field.Index }}], ""{{ field.DefaultValue }}""); {% endfor %}
        }

        /// <summary>
        /// Get PrimaryKey from a table row
        /// </summary>
        /// <param name=""row""></param>
        /// <returns></returns>
        public static {{ file.PrimaryKeyField.FormatType }} ParsePrimaryKey(TableRow row)
        {
            var primaryKey = row.Get_{{ file.PrimaryKeyField.TypeMethod }}(row.Values[{{ file.PrimaryKeyField.Index }}], ""{{ file.PrimaryKeyField.DefaultValue }}"");
            return primaryKey;
        }
	}
{% endfor %} 
}
";
        /// <summary>
        /// 标记，是否正在打开提示配置变更对话框
        /// </summary>
        private static bool _isPopUpConfirm = false;

        static KSettingModuleEditor()
        {
            var path = SettingSourcePath;
            if (Directory.Exists(path))
            {
                new KDirectoryWatcher(path, (o, args) =>
                {
                    if (_isPopUpConfirm) return;

                    _isPopUpConfirm = true;
                    KEditorUtils.CallMainThread(() =>
                    {
                        EditorUtility.DisplayDialog("Excel Setting Changed!", "Ready to Recompile All!", "OK");
                        DoCompileSettings(false);
                        _isPopUpConfirm = false;
                    });
                });
                Debug.Log("[KSettingModuleEditor]Watching directory: " + SettingSourcePath);
            }
        }

        /// <summary>
        /// Generate static code from settings
        /// </summary>
        /// <param name="templateVars"></param>
        public static void GenerateCode(string genCodeFilePath, string nameSpace, List<Hash> files)
        {

            var codeTemplates = new Dictionary<string, string>()
            {
                {GenCodeTemplate, genCodeFilePath},
            };

            foreach (var kv in codeTemplates)
            {
                var templateStr = kv.Key;
                var exportPath = kv.Value;

                // 生成代码
                var template = Template.Parse(templateStr);
                var topHash = new Hash();
                topHash["NameSpace"] = nameSpace;
                topHash["Files"] = files;

                if (!string.IsNullOrEmpty(exportPath))
                {
                    var genCode = template.Render(topHash);
                    if (File.Exists(exportPath)) // 存在，比较是否相同
                    {
                        if (File.ReadAllText(exportPath) != genCode)
                        {
                            EditorUtility.ClearProgressBar();
                            // 不同，会触发编译，强制停止Unity后再继续写入
                            if (EditorApplication.isPlaying)
                            {
                                KLogger.LogError("[CAUTION]AppSettings code modified! Force stop Unity playing");
                                EditorApplication.isPlaying = false;
                            }
                            File.WriteAllText(exportPath, genCode);
                            return; // 防止Unity出现红色提示错误
                        }
                    }
                    else
                        File.WriteAllText(exportPath, genCode);

                }
            }
        }
        public static void CompileTabConfigs(string sourcePath, string compilePath, string genCodeFilePath, string changeExtension = ".bytes", bool force = false)
        {
            var compileBaseDir = compilePath;
            // excel compiler
            var compiler = new Compiler(new CompilerConfig());

            var excelExt = new HashSet<string>() { ".xls", ".xlsx" };
            var findDir = sourcePath;
            try
            {
                var allFiles = Directory.GetFiles(findDir, "*.*", SearchOption.AllDirectories);
                var allFilesCount = allFiles.Length;
                var nowFileIndex = -1; // 开头+1， 起始为0
                var results = new List<TableCompileResult>();
                foreach (var excelPath in allFiles)
                {
                    nowFileIndex++;
                    var ext = Path.GetExtension(excelPath);
                    var fileName = Path.GetFileNameWithoutExtension(excelPath);
                    if (excelExt.Contains(ext) && !fileName.StartsWith("~")) // ~开头为excel临时文件，不要读
                    {
                        // it's an excel file
                        var relativePath = excelPath.Replace(findDir, "").Replace("\\", "/");
                        if (relativePath.StartsWith("/"))
                            relativePath = relativePath.Substring(1);


                        var compileToPath = string.Format("{0}/{1}", compileBaseDir,
                            Path.ChangeExtension(relativePath, changeExtension));
                        var srcFileInfo = new FileInfo(excelPath);

                        EditorUtility.DisplayProgressBar("Compiling Excel to Tab...",
                            string.Format("{0} -> {1}", excelPath, compileToPath), nowFileIndex / (float)allFilesCount);

                        // 如果已经存在，判断修改时间是否一致，用此来判断是否无需compile，节省时间
                        bool doCompile = true;
                        if (File.Exists(compileToPath))
                        {
                            var toFileInfo = new FileInfo(compileToPath);

                            if (!force && srcFileInfo.LastWriteTime == toFileInfo.LastWriteTime)
                            {
                                //KLogger.Log("Pass!SameTime! From {0} to {1}", excelPath, compileToPath);
                                doCompile = false;
                            }
                        }
                        if (doCompile)
                        {
                            KLogger.LogWarning("[SettingModule]Compile from {0} to {1}", excelPath, compileToPath);

                            var compileResult = compiler.Compile(excelPath, compileToPath, compileBaseDir, doCompile);

                            // 添加模板值
                            results.Add(compileResult);

                            var compiledFileInfo = new FileInfo(compileToPath);
                            compiledFileInfo.LastWriteTime = srcFileInfo.LastWriteTime;

                        }
                    }
                }

                // 根据模板生成所有代码,  如果不是强制重建，无需进行代码编译
                if (!AutoGenerateCode)
                {
                    KLogger.LogWarning("Ignore Gen Settings code");
                }
                else if (!force)
                {
                    KLogger.LogWarning("Ignore Gen Settings Code, not a forcing compiling");
                }
                else
                {

                    // 根据编译结果，构建vars，同class名字的，进行合并
                    var templateVars = new Dictionary<string, TableTemplateVars>();
                    foreach (var compileResult in results)
                    {
                        var customExtraStr = CustomExtraString != null ? CustomExtraString(compileResult) : null;

                        var templateVar = new TableTemplateVars(compileResult, customExtraStr);

                        // 尝试类过滤
                        var ignoreThisClassName = false;
                        if (GenerateCodeFilesFilter != null)
                        {
                            for (var i = 0; i < GenerateCodeFilesFilter.Length; i++)
                            {
                                var filterClass = GenerateCodeFilesFilter[i];
                                if (templateVar.ClassName.Contains(filterClass))
                                {
                                    ignoreThisClassName = true;
                                    break;
                                }

                            }
                        }
                        if (!ignoreThisClassName)
                        {
                            if (!templateVars.ContainsKey(templateVar.ClassName))
                                templateVars.Add(templateVar.ClassName, templateVar);
                            else
                            {
                                templateVars[templateVar.ClassName].Paths.Add(compileResult.TabFilePath);
                            }
                        }

                    }

                    // 整合成字符串模版使用的List
                    var templateHashes = new List<Hash>();
                    foreach (var kv in templateVars)
                    {
                        var templateVar = kv.Value;
                        var renderTemplateHash = Hash.FromAnonymousObject(templateVar);
                        templateHashes.Add(renderTemplateHash);
                    }


                    var nameSpace = "AppSettings";
                    GenerateCode(genCodeFilePath, nameSpace, templateHashes);
                }

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static string SettingSourcePath
        {
            get
            {
                var sourcePath = AppEngine.GetConfig("SettingSourcePath");
                return sourcePath;
            }
        }

        [MenuItem("KEngine/Settings/Force Compile Settings + Code")]
        public static void CompileSettings()
        {
            DoCompileSettings(true);
        }
        [MenuItem("KEngine/Settings/Quick Compile Settings")]
        public static void QuickCompileSettings()
        {
            DoCompileSettings(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force">Whether or not,check diff.  false will be faster!</param>
        /// <param name="genCode">Generate static code?</param>
        public static void DoCompileSettings(bool force = true)
        {
            var sourcePath = SettingSourcePath;//AppEngine.GetConfig("SettingSourcePath");
            if (string.IsNullOrEmpty(sourcePath))
            {
                KLogger.LogError("Need to KEngineConfig: SettingSourcePath");
                return;
            }
            var compilePath = AppEngine.GetConfig("SettingPath");
            if (string.IsNullOrEmpty(compilePath))
            {
                KLogger.LogError("Need to KEngineConfig: SettingPath");
                return;
            }
            CompileTabConfigs(sourcePath, compilePath, SettingCodePath, SettingExtension, force);
        }
    }

    /// <summary>
    /// 用于liquid模板
    /// </summary>
    public class TableTemplateVars
    {
        public delegate string CustomClassNameDelegate(string originClassName, string filePath);

        /// <summary>
        /// You can custom class name
        /// </summary>
        public static CustomClassNameDelegate CustomClassNameFunc;

        public List<string> Paths = new List<string>();

        /// <summary>
        ///  构建成一个数组["aaa", "bbb"]
        /// </summary>
        public string TabFilePaths
        {
            get
            {
                var paths = "\"" + string.Join("\", \"", Paths.ToArray()) + "\"";
                return paths;
            }
        }

        public string ClassName { get; set; }
        public List<TableColumnVars> FieldsInternal { get; set; } // column + type

        public string PrimaryKey { get; set; }

        public List<Hash> Fields
        {
            get { return (from f in FieldsInternal select Hash.FromAnonymousObject(f)).ToList(); }
        }

        /// <summary>
        /// Get primary key, the first column field
        /// </summary>
        public Hash PrimaryKeyField
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// Custom extra strings
        /// </summary>
        public string Extra { get; private set; }

        public List<Hash> Columns2DefaultValus { get; set; } // column + Default Values

        public TableTemplateVars(TableCompileResult compileResult, string extraString)
            : base()
        {
            var tabFilePath = compileResult.TabFilePath;
            Paths.Add(compileResult.TabFilePath);

            ClassName = DefaultClassNameParse(tabFilePath);
            // 可自定义Class Name
            if (CustomClassNameFunc != null)
                ClassName = CustomClassNameFunc(ClassName, tabFilePath);

            FieldsInternal = compileResult.FieldsInternal;
            PrimaryKey = compileResult.PrimaryKey;
            Columns2DefaultValus = new List<Hash>();

            Extra = extraString;
        }

        /// <summary>
        /// get a class name from tab file path, default strategy
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        string DefaultClassNameParse(string tabFilePath)
        {
            // 未处理路径的类名, 去掉后缀扩展名
            var classNameOrigin = Path.ChangeExtension(tabFilePath, null);

            // 子目录合并，首字母大写, 组成class name
            var className = classNameOrigin.Replace("/", "_").Replace("\\", "_");
            className = className.Replace(" ", "");
            className = string.Join("", (from name
                in className.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                                         select (name[0].ToString().ToUpper() + name.Substring(1, name.Length - 1)))
                .ToArray());

            // 去掉+号后面的字符
            var plusSignIndex = className.IndexOf("+");
            className = className.Substring(0, plusSignIndex == -1 ? className.Length : plusSignIndex);
            return className;

        }
    }
}
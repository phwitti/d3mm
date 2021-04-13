using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace d3mm
{
    public class DirectoryConfig
    {
        public string DesperadosDirectory
        {
            get; set;
        }

        public string DataDirectory
        {
            get; set;
        }
    }

    public class Config
    {
        public string URL
        {
            get; set;
        }

        //

        public bool Maximized
        {
            get; set;
        }

        public int Width
        {
            get; set;
        }

        public int Height
        {
            get; set;
        }

        //

        public int InstalledColumnWidth
        {
            get; set;
        }

        public bool InstalledColumnVisible
        {
            get; set;
        }

        public int PreviewColumnWidth
        {
            get; set;
        }

        public bool PreviewColumnVisible
        {
            get; set;
        }

        public int NameColumnWidth
        {
            get; set;
        }

        public bool NameColumnVisible
        {
            get; set;
        }

        public int RatingColumnWidth
        {
            get; set;
        }

        public bool RatingColumnVisible
        {
            get; set;
        }
        
        public int SubmittedByColumnWidth
        {
            get; set;
        }

        public bool SubmittedByColumnVisible
        {
            get; set;
        }

        public int SummaryColumnWidth
        {
            get; set;
        }

        public bool SummaryColumnVisible
        {
            get; set;
        }

        public int TagsColumnWidth
        {
            get; set;
        }

        public bool TagsColumnVisible
        {
            get; set;
        }
        
        public int VersionColumnWidth
        {
            get; set;
        }

        public bool VersionColumnVisible
        {
            get; set;
        }

        //

        public int PrimarySortColumn
        {
            get; set;
        }

        public int PrimarySortOrder
        {
            get; set;
        }

        public int SecondarySortColumn
        {
            get; set;
        }

        public int SecondarySortOrder
        {
            get; set;
        }

        public int SplitterDistanceRight
        {
            get; set;
        }

        public bool WebBrowserCollapsed
        {
            get; set;
        }

        //

        public string UserDirectory
        {
            get; set;
        }

        public string Executable
        {
            get; set;
        }
    }

    public static class ApplicationProperties
    {
        private const string c_sConfigFile = "config.json";
        private const string c_sDirectoryConfigFile = "d3mm.config.json";
        private const string c_sDirectoryConfigDefaultDesperadosDirectory ="%appdata%/../Local/Desperados III/";
        private const string c_sDirectoryConfigDefaultDataDirectory = ".d3mm";
        private const string c_sInstallationLocationRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 610370";
        private const string c_sInstallationLocationRegistryName = "InstallLocation";
        private const string c_sInstallationLocationExecutable = "Desperados III.exe";
        private const string c_sOnlineDatabase = "https://www.phwitti.com/projects/d3mm/?apiKey=femoHwE49hJ5YwhazWMGG3hxJElACofQ";

        //

        private static Config s_config;
        private static DirectoryConfig s_directoryConfig;

        //

        public static string DesperadosDirectory
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(
                    s_directoryConfig.DesperadosDirectory);
            }
        }

        public static string DataDirectory
        {
            get 
            {
                 return Path.Combine(
                        ApplicationProperties.DesperadosDirectory,
                        s_directoryConfig.DataDirectory);
            }
        }

        public static Config Config
        {
            get { return s_config; }
        }

        //

        public static void Init()
        {
            try
            {
                s_directoryConfig = JsonConvert.DeserializeObject<DirectoryConfig>(
                    File.ReadAllText(c_sDirectoryConfigFile));
            }
            catch
            {
                s_directoryConfig = new DirectoryConfig() {
                    DesperadosDirectory = c_sDirectoryConfigDefaultDesperadosDirectory,
                    DataDirectory = c_sDirectoryConfigDefaultDataDirectory
                };
            }

            try
            {
                string configPath = System.IO.Path.Combine(
                    ApplicationProperties.DataDirectory,
                    c_sConfigFile);
                
                s_config = JsonConvert.DeserializeObject<Config>(
                    File.ReadAllText(configPath));
                
            }
            catch
            {
                string sExecutable = string.Empty;
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(c_sInstallationLocationRegistryKey))
                    {
                        if (key != null)
                        {
                            if (key.GetValue(c_sInstallationLocationRegistryName) is string sValue)
                            {
                                sExecutable = Path.Combine(sValue, c_sInstallationLocationExecutable);
                            }
                        }
                    }
                }
                catch
                {
                }

                s_config = new Config(){
                    URL = c_sOnlineDatabase,
                    Maximized = true,
                    Width = 800,
                    Height = 450,
                    InstalledColumnWidth = 85,
                    InstalledColumnVisible = true,
                    NameColumnWidth = 160,
                    NameColumnVisible = true,
                    PreviewColumnWidth = 80,
                    PreviewColumnVisible = true,
                    RatingColumnWidth = 50,
                    RatingColumnVisible = true,
                    SubmittedByColumnWidth = 95,
                    SubmittedByColumnVisible = true,
                    SummaryColumnWidth = 240,
                    SummaryColumnVisible = true,
                    TagsColumnWidth = 135,
                    TagsColumnVisible = true,
                    VersionColumnWidth = 85,
                    VersionColumnVisible = true,
                    PrimarySortColumn = -1,
                    PrimarySortOrder = 0,
                    SecondarySortColumn = -1,
                    SecondarySortOrder = 0,
                    SplitterDistanceRight = 450,
                    WebBrowserCollapsed = true,
                    UserDirectory = System.IO.Path.GetFileName(
                        System.IO.Directory.EnumerateDirectories(
                            ApplicationProperties.DesperadosDirectory, "user*").Last()),
                    Executable = sExecutable,
                };
            }
        }

        public static void Exit()
        {
            string path = Path.Combine(
                ApplicationProperties.DataDirectory,
                c_sConfigFile);

            Directory.CreateDirectory(ApplicationProperties.DataDirectory);

            File.WriteAllText(path, JsonConvert.SerializeObject(s_config, Formatting.Indented));
        }
    }
}

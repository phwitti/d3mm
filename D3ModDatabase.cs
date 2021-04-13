using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace d3mm
{
    public class D3ModDatabase
    {
        private const string c_sDatabaseFile = "database.json";
        private const string c_sExtensionMod = ".zip";
        private const string c_sExtensionSave = ".save";
        private const string c_sFolderState = "gameState_00";
        private const string c_sFolderSave = "manualSave";
        private const string c_sThumbnail = "thumb_320x180";

        //

        private bool m_bInitialized;
        private Dictionary<int, D3ModInfo> m_dMods = new Dictionary<int, D3ModInfo>();

        //

        public IEnumerable<D3ModInfo> Mods
        {
            get { return m_dMods.Values; }
        }

        public bool Initialized
        {
            get { return m_bInitialized; }
        }

        public event System.Action<D3ModInfo> onModStateChange;

        //

        public D3ModDatabase()
        {
            s_database = this;
            this.LoadDatabase();
            this.SaveDatabase();
            m_bInitialized = true;
            Task task = this.DownloadImages();
        }

        private D3ModInfo[] GetLocalDatabase()
        {
            try
            {
                return JsonConvert.DeserializeObject<D3ModInfo[]>(
                    File.ReadAllText(Path.Combine(ApplicationProperties.DataDirectory, c_sDatabaseFile)));
            }
            catch
            {
                return new D3ModInfo[0];
            }
        }

        private D3ModInfo[] GetOnlineDatabase()
        {
            try
            {
                using (var streamReader = new StreamReader(WebRequest.Create(ApplicationProperties.Config.URL).GetResponse().GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<D3ModInfo[]>(
                        streamReader.ReadToEnd());
                }
            }
            catch
            {
                return new D3ModInfo[0];
            }
        }

        private D3ModInfo[] MergeDatabase(D3ModInfo[] _local, D3ModInfo[] _online)
        {
            Dictionary<int, D3ModInfo> dicModInfoMerged = new Dictionary<int, D3ModInfo>();

            foreach (D3ModInfo modInfo in _online)
            {
                dicModInfoMerged.Add(modInfo.ID, modInfo);
            }

            foreach (D3ModInfo modInfo in _local)
            {
                if (dicModInfoMerged.TryGetValue(modInfo.ID, out D3ModInfo modInfoMerge))
                {
                    modInfoMerge.ImageURLLocal = modInfo.ImageURLLocal;
                    modInfoMerge.Installed = modInfo.Installed == null
                        ? false
                        : modInfo.Installed;
                    modInfoMerge.InstalledName = modInfo.InstalledName;
                    modInfoMerge.Status = modInfo.Status;
                    modInfoMerge.VersionInstalled = modInfo.VersionInstalled;
                }
                else if (modInfo.Installed == true)
                {
                    dicModInfoMerged.Add(modInfo.ID, modInfo);
                }
                else
                {
                    try
                    {
                        System.IO.Directory.Delete(
                            Path.Combine(ApplicationProperties.DataDirectory, modInfo.ID.ToString()),
                            recursive: true);
                    }
                    catch
                    {
                    }
                }
            }

            return dicModInfoMerged.Values.ToArray();
        }

        //

        private void LoadDatabase()
        {
            D3ModInfo[] arModInfoLocal = this.GetLocalDatabase();
            D3ModInfo[] arModInfoOnline = this.GetOnlineDatabase();
            D3ModInfo[] arModInfoMerged = this.MergeDatabase(arModInfoLocal, arModInfoOnline);
            
            foreach (D3ModInfo modInfo in arModInfoMerged)
            {
                m_dMods.Add(modInfo.ID, modInfo);
            }
        }

        private void SaveDatabase()
        {
            Directory.CreateDirectory(ApplicationProperties.DataDirectory);

            File.WriteAllText(
                Path.Combine(ApplicationProperties.DataDirectory, c_sDatabaseFile), 
                JsonConvert.SerializeObject(m_dMods.Values.ToArray(), Formatting.Indented));
        }

        //

        public async Task Install(int _iID)
        {
            if (!m_dMods.TryGetValue(_iID, out D3ModInfo modInfo))
                throw new System.Collections.Generic.KeyNotFoundException();

            modInfo.setInstalled(null);
            modInfo.Status = 0f;
            this.onModStateChange.Invoke(modInfo);
            await this.Download(_iID);

            string sDirectoryPath = Path.Combine(
                ApplicationProperties.DataDirectory,
                _iID.ToString());

            string sFilePath = Path.Combine(
                sDirectoryPath,
                Path.GetFileName(modInfo.DownloadURL) + c_sExtensionMod);

            string sExtractPath = System.IO.Path.Combine(
                ApplicationProperties.DesperadosDirectory,
                ApplicationProperties.Config.UserDirectory,
                c_sFolderState,
                c_sFolderSave);

            using (ZipArchive archive = ZipFile.OpenRead(sFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.EndsWith(c_sExtensionSave, StringComparison.OrdinalIgnoreCase))
                    {
                        int i = 0;
                        string sDestinationName = entry.Name;

                        // we omit path information
                        string sDestinationPath = Path.GetFullPath(
                            Path.Combine(sExtractPath, sDestinationName));

                        while (File.Exists(sDestinationPath))
                        {
                            sDestinationName = $"{ Path.GetFileNameWithoutExtension(entry.Name) }_{ i.ToString() }{ c_sExtensionSave }";
                            sDestinationPath = Path.GetFullPath(
                                Path.Combine(sExtractPath, sDestinationName));
                            i += 1;
                        }

                        modInfo.InstalledName = sDestinationName;
                        entry.ExtractToFile(sDestinationPath);
                        break;
                    }
                }
            }

            File.Delete(sFilePath);
            modInfo.setInstalled(true);
            modInfo.Status = 1f;
            modInfo.VersionInstalled = modInfo.Version;
            this.onModStateChange.Invoke(modInfo);
            this.SaveDatabase();
        }

        public async Task Uninstall(int _iID)
        {
            if (m_dMods.TryGetValue(_iID, out D3ModInfo modInfo))
            {
                modInfo.VersionInstalled = string.Empty;
                modInfo.Status = 1f;
                modInfo.setInstalled(null);
                this.onModStateChange.Invoke(modInfo);

                await Task.Delay(100);

                modInfo.Status = 0.5f;
                this.onModStateChange.Invoke(modInfo);
                
                if (!string.IsNullOrEmpty(modInfo.InstalledName))
                {
                    string sExtractPath = System.IO.Path.Combine(
                        ApplicationProperties.DesperadosDirectory,
                        ApplicationProperties.Config.UserDirectory,
                        c_sFolderState,
                        c_sFolderSave);

                    string sDestinationPath = Path.GetFullPath(
                        Path.Combine(sExtractPath, modInfo.InstalledName));

                    System.IO.File.Delete(sDestinationPath);
                }
                await Task.Delay(100);

                modInfo.Status = 0f;
                modInfo.setInstalled(false);
                this.onModStateChange.Invoke(modInfo);
                this.SaveDatabase();
            }
        }

        public async Task Download(int _iID)
        {
            if (!m_dMods.TryGetValue(_iID, out D3ModInfo modInfo))
                throw new System.Collections.Generic.KeyNotFoundException();

            string sDirectoryPath = System.IO.Path.Combine(
                ApplicationProperties.DataDirectory,
                _iID.ToString());

            string sFilePath = System.IO.Path.Combine(
                sDirectoryPath,
                System.IO.Path.GetFileName(modInfo.DownloadURL) + c_sExtensionMod);

            System.IO.Directory.CreateDirectory(sDirectoryPath);
            
            if (!System.IO.File.Exists(sFilePath))
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (_sender, _args) => 
                    {
                        modInfo.Status = _args.ProgressPercentage;
                        this.onModStateChange.Invoke(modInfo);
                    };

                    await webClient.DownloadFileTaskAsync(modInfo.DownloadURL, sFilePath);
                }
            }
            else
            {
                modInfo.Status = 1f;
                this.onModStateChange.Invoke(modInfo);
            }
        }

        public async Task DownloadImages()
        {
            foreach (D3ModInfo modInfo in m_dMods.Values)
            {
                if (string.IsNullOrEmpty(modInfo.ImageURLLocal)
                    || !System.IO.File.Exists(modInfo.ImageURLLocalFullPath))
                {
                    string sDirectoryPath = System.IO.Path.Combine(
                        ApplicationProperties.DataDirectory,
                        modInfo.ID.ToString());

                    string sFileName = c_sThumbnail + Path.GetExtension(modInfo.ImageURL);

                    string sFilePath = Path.Combine(
                        sDirectoryPath,
                        sFileName);

                    System.IO.Directory.CreateDirectory(sDirectoryPath);
                    
                    if (!System.IO.File.Exists(sFilePath))
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            await webClient.DownloadFileTaskAsync(modInfo.ImageURL, sFilePath);
                            modInfo.ImageURLLocal = sFileName;
                            this.onModStateChange.Invoke(modInfo);
                        }
                    }
                    else
                    {
                        modInfo.ImageURLLocal = sFileName;
                    }
                }
            }

            this.SaveDatabase();
        }

        //

        private static D3ModDatabase s_database;

        //

        public static D3ModDatabase Instance
        {
            get { return s_database; }
        }
    }
}

using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace d3mm
{
    public class D3ModInfo
    {
        private bool? m_bInstalled = false;
        private string m_sVersionInstalled = string.Empty;

        //

        public int ID
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string DownloadURL
        {
            get; set;
        }

        public string ImageURL
        {
            get; set;
        }

        public string ImageURLLocal
        {
            get; set;
        }

        [JsonIgnore]
        public string ImageURLLocalFullPath
        {
            get { return Path.Combine(ApplicationProperties.DataDirectory, this.ID.ToString(), this.ImageURLLocal); }
        }

        public bool? Installed
        {
            get
            {
                return m_bInstalled;
            }
            set
            {
                if (D3ModDatabase.Instance.Initialized)
                {
                    if (value == true)
                    {
                        Task task = D3ModDatabase.Instance.Install(this.ID);
                    }
                    else
                    {
                        Task task = D3ModDatabase.Instance.Uninstall(this.ID);
                    }
                }
                else
                {
                    m_bInstalled = value;
                }
            }
        }

        public string InstalledName
        {
            get; set;
        }

        public float Rating
        {
            get; set;
        }

        public float RatingWeighted
        {
            get; set;
        }

        public float Status
        {
            get; set;
        }

        public string SubmittedBy
        {
            get; set;
        }

        public string Summary
        {
            get; set;
        }

        public string[] Tags
        {
            get; set;
        }

        public string URL
        {
            get; set;
        }

        public string Version
        {
            get; set;
        }

        public string VersionInstalled
        {
            get
            {
                if (m_bInstalled == null)
                {
                    return this.Status.ToString();
                }
                else
                {
                    return m_sVersionInstalled ?? "";
                }
            }
            set
            {
                m_sVersionInstalled = value;
            }
        }

        //

        public void setInstalled(bool? _bInstalled)
        {
            m_bInstalled = _bInstalled;
        }
    }
}

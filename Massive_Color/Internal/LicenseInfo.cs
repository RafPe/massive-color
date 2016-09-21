using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliLock.Licensing;

namespace Massive_Color.Internal
{
    /// <summary>
    /// Klasa do zarzadzania licencja
    /// </summary>
    public class LicenseManager
    {
        public List<LicenseInfo>    LicenseInfos { get; set; }
        public readonly Boolean     isLicenseFile;                  // Zmienna uzywana do informacji czy mamy valid licencje

        /// <summary>
        /// Domyslny konstruktor
        /// </summary>
        public LicenseManager()
        {
            LicenseInfos = new List<LicenseInfo>();

            this.isLicenseFile = EvaluationMonitor.CurrentLicense.LicenseStatus == LicenseStatus.Licensed;  // Przypisujemy wartosc
        }

        public string GetByKeyName(string kname)
        {
            try
            {
                return LicenseInfos.Single(l => l.KeyName == kname).KeyValue;
            }
            catch (Exception)
            {

                return null;
                
            }

            return null;
        }

        /// <summary>
        /// Metoda do wczytania ustawien licencji - musi sie oczywiscie zgadzac z licnecja
        /// </summary>
        public void LoadLicenseDetails()
        {
            /* Check first if a valid license file is found */
            if (isLicenseFile)
            {
                /* Read additional license information */
                for (int i = 0; i < EvaluationMonitor.CurrentLicense.LicenseInformation.Count; i++)
                {
                    LicenseInfo tmp = new LicenseInfo()
                    {
                        KeyName = EvaluationMonitor.CurrentLicense.LicenseInformation.GetKey(i).ToString(),
                        KeyValue = EvaluationMonitor.CurrentLicense.LicenseInformation.GetByIndex(i).ToString()
                    };

                    this.LicenseInfos.Add(tmp);

                }

            }
        }

    }

    /// <summary>
    /// Class used for holding license information that we can easily access
    /// </summary>
    public class LicenseInfo
    {
        public string KeyName   { get; set; }
        public string KeyValue  { get; set; }
    }
}

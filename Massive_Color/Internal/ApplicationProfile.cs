using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Massive_Color.Internal
{
    /// <summary>
    /// Klasa przechowujaca informacje o profilu aplikacji
    /// Czyli polaczone ustawienia grafiki z ustawieniami specyficznymi dla aplikacji
    /// </summary>
    public class ApplicationProfile
    {
        public Boolean              Autostart           { get; set; }
        public Boolean              AutoRun             { get; set; } // Ta zmienna definiuje czy system automatycznie powienen wyszukac urzadzenia
        public int                  BaudRate            { get; set; }
        public String               LastCOMconnected    { get; set; }
        public ChannelSettings      ChannelSettings     { get; set; }

        public ApplicationProfile()
        {
            // Domyslne ustawienia
            ChannelSettings     = new ChannelSettings();
            Autostart           = false;
            LastCOMconnected    = "";
            BaudRate            = 230400;
        }
    }

    /// <summary>
    /// Klasa ktorej uzywamy do zapisania naszych ustawien 
    /// ktore pozniej wykorzystujemy
    /// </summary>
    public static class AppProfileManager
    {
        public static void Save_ApplicationProfile(ApplicationProfile ust)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(ApplicationProfile));
                TextWriter tw = new StreamWriter(String.Format(@"{0}\{1}", System.AppDomain.CurrentDomain.BaseDirectory, "Profile.set"));
                xs.Serialize(tw, ust);
                tw.Dispose();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static ApplicationProfile Load_ApplicationProfile()
        {
            try
            {
                XmlDocument myXmlDocument = new XmlDocument();
                myXmlDocument.Load(String.Format(@"{0}\{1}", System.AppDomain.CurrentDomain.BaseDirectory, "Profile.set"));

                XmlNodeReader reader = new XmlNodeReader(myXmlDocument.DocumentElement);
                XmlSerializer ser = new XmlSerializer(typeof(ApplicationProfile));
                ApplicationProfile obj = (ApplicationProfile)ser.Deserialize(reader);

                reader.Dispose();

                return obj;
            }
            catch (Exception)
            {
                Save_ApplicationProfile(new ApplicationProfile());    // Tworzymy nowy domyslny profil
                return new ApplicationProfile();                   // Zwracamy calkiem domyslne ustawienia
            }


        }
        
    }
}

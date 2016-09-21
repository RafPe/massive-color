using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using IntelliLock.Licensing;
using Massive_Color.Internal;
using Massive_Color.Windows;
using RafTech.Communications.Serial;
using RafTech.Graphics.RGCapture.x86;
using SlimDX;
using SlimDX.Direct3D9;
using LicenseManager = Massive_Color.Internal.LicenseManager;

//TODO: Wylaczony guzik do pluginow 

namespace Massive_Color
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private ApplicationProfile  ApplicationProfile;
        private ChannelsManagement  channelsManagement;             // Zarzadzanie ustawieniami do Ambi
        private RGBdata             rgbManagement = new RGBdata();     // Umozliwia zlapanie i obrobke obrazu
        private DeviceController    serialDeviceController;         // Kontrola nad urzadzeniem

        private Task    tsk_RGBdata;                                // BackgorundTask do wykonywania zadan lapania probek ekranu
        private Boolean Stop_Data_Aqquire;                          // Zmienna informujaca nas czy ma sie wykonywac lapanie probek ekranu   
        private Boolean isRGBdataAqquireRunning;                    // Zmienna przechowujaca informacje czy nasz data aqquire smiga
        private Boolean isLicenseFilePresent;                       // Zmienna ktora uzywamy zeby wiedziec czy w programie jest plik licencji       

        private LicenseManager lcm = new LicenseManager();          // License manager dla licencjonowania
        

        /// <summary>
        /// Konstruktor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();            
            
            // Wlaczamy globalna zmienna zeby wiedziec czy mamy licencje
            if (EvaluationMonitor.CurrentLicense.LicenseStatus == LicenseStatus.Licensed)
                isLicenseFilePresent = true;
                
        }

        #region Main Window Events

        /// <summary>
        /// Glowne okno zaladowane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            
            
            // [1] - Check License 
            if (!isLicenseFilePresent) TextBox_StatusBar_Bottom.Text = "No license present";
            else
            {
                try
                {
                    lcm.LoadLicenseDetails();
                    TextBox_StatusBar_Bottom.Text = String.Format("Registered to {0} {1}", lcm.GetByKeyName("FirstName"),
                        lcm.GetByKeyName("LastName"));
                }
                catch
                {
                    // ignored
                }
               
            }

           
            // [3] - Load profile             
            ApplicationProfile = AppProfileManager.Load_ApplicationProfile();      // Try to load the profile if does not exists
            // [3a] - See if we should be minimized
            if (ApplicationProfile.Autostart) this.WindowState = WindowState.Minimized; // This starts minimized app if we choosed to AutoStart application
            // [3b] - Assign proper value to brightness slider
            Slider_Brightness.Value = ApplicationProfile.ChannelSettings.Brightness;
            // [4] - Configure COM communications
            serialDeviceController = ApplicationProfile.LastCOMconnected == "" ? new DeviceController(ApplicationProfile.BaudRate) : new DeviceController(ApplicationProfile.BaudRate, ApplicationProfile.LastCOMconnected);
            // [4a] - Subscribe to event
            serialDeviceController.OnCOMdisconnected += (o, args) =>
            {
                

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ToggleButton_Start.Content = "Start";                      // Zmieniamy napis na guziku
                    isRGBdataAqquireRunning = false;                           // Zmieniamy status naszej flagi
                    Stop_Data_Aqquire = true;                                  // Zmieniamy flage aby Ambi nie wysylalo danych

                    TextBlock_StatusBar_1.Text  = " Disconnected";
                }));


            };

            // [5] - Ustaw podstawowe ustawienia
            channelsManagement = new ChannelsManagement(ApplicationProfile.ChannelSettings, (int)SystemParameters.PrimaryScreenHeight, (int)SystemParameters.PrimaryScreenWidth);    // Ustawienia
            // [6] - Konfiguracja TASKa dla chwytania danych z ekranu
            #region Task - lapanie danych
            tsk_RGBdata = new Task(() =>
            {
                while (true)
                {

                    if (!(Stop_Data_Aqquire))
                    {
                        if (serialDeviceController.DeviceConnected)
                        {

                            // Funkcje sluzace do zlapania ekranu
                            Surface s = rgbManagement.CaptureScreen(
                                            (int)SystemParameters.PrimaryScreenWidth,
                                            (int)SystemParameters.PrimaryScreenHeight
                                            );
                            DataRectangle dr = s.LockRectangle(LockFlags.None);
                            DataStream gs = dr.Data;

                            foreach (LightChannel channel in channelsManagement.LightChannels)
                            {

                                //TODO: Sprawdzamy brightness
                                //rgbManagement.AverageScreenRGBfromChannel(gs, channel, channelsManagement.Pixel_Min_Treshold);

                                try
                                {
                                    rgbManagement.AverageScreen(gs, channel, channelsManagement.Pixel_Min_Treshold,
                                        ApplicationProfile.ChannelSettings.Brightness);
                                }
                                catch
                                {

                                }
                                //TODO : Ta funkcja zostanie "fade out" 

                                //if (channelsManagement.Average_Strong_Colors)
                                //{

                                //    //RgBcapture.AdjustAverageRGB(channel, channelsManagement.Average_Strong_Colors_Adjustment);
                                //}
                            }

                            serialDeviceController.Send_Data_To_Serial(new byte[1] { 0x01 }); // wypychamy dane na pasek

                            serialDeviceController.Send_Data_To_Serial(rgbManagement.CalculateRGBarray(channelsManagement.LightChannels)); // wypychamy dane na pasek

                            serialDeviceController.Send_Data_To_Serial(new byte[1] { 0x02 }); // wypychamy dane na pasek

                            s.UnlockRectangle(); // <---------------------  UNLOCK !!!
                            s.Dispose();
                        }
                    }


                }

            });
            #endregion            
            // [4b] - See if we must AutoRun our process :D
            if (ApplicationProfile.AutoRun)
            {
                ToggleButton_Start.IsEnabled = !ToggleButton_Start.IsEnabled;   // Toggle button state
                Action_Start();                                                 // Start Ambi
            }
            // [7] Tray configs 
            

        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            AMLC_Frame_STOP();
        }

        #endregion

        #region Buttons

        /// <summary>
        /// Metoda wywolywana w odpowiedzi na naciesniecie guzika start/stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Start_OnClick(object sender, RoutedEventArgs e)
        {

            ToggleButton_Start.IsEnabled = !ToggleButton_Start.IsEnabled;   // Toggle button state

            #region Action - STOP
            if (isRGBdataAqquireRunning)                                    // Jesli smigamy to chcemy zrobic stop
            {

                Action_Stop();

            }
            #endregion

            #region Action - START
            else                                                            // Jesli nie smigamy to chcemy wlaczyc nasze Ambi
            {

                Action_Start();

            }
            #endregion

        }

        /// <summary>
        /// Metoda wywolywana aby uruchomic Ambi
        /// </summary>
        private void Action_Start()
        {
            if (serialDeviceController.DeviceConnected)                 // Jesli juz jestesmy podlaczeniu to chcemy uruchomic Ambi wczesniej skonfigurowane
            {
                ToggleButton_Start.Content = "Stop";                      // Zmieniamy napis na guziku
                isRGBdataAqquireRunning = true;                        // Zmieniamy status naszej flagi
                Stop_Data_Aqquire = false;                         // Zmieniamy flage aby Ambi nie wysylalo danych
                TextBlock_StatusBar_1.Text = " Connected | Running";
                ToggleButton_Start.IsEnabled = !ToggleButton_Start.IsEnabled;   // Toggle button state
            }
            else
            {
                #region Jesli musimy wyszukac nasze urzadzenie
                if (String.IsNullOrEmpty(ApplicationProfile.LastCOMconnected))  // Czy juz kiedys sie podlaczalismy do urzadzenia ?
                {

                    TextBlock_StatusBar_1.Text = "Searching for device...";     // Wyswietl informacje ze szukamy urzadzenia

                    if (serialDeviceController.SearchForConnectedDevice("A1"))  // Czy znalazles urzadzenie posrod wszystkich COM
                    {
                        TextBlock_StatusBar_1.Text = "Found! Connecting...";    // Wyswietl informacje ze sie podlaczamy

                        ApplicationProfile.LastCOMconnected = serialDeviceController.ComPortName;   // Zapisz pod jakim portem znalezlismy urzadzenie

                        AppProfileManager.Save_ApplicationProfile(ApplicationProfile);              // Zapisz profil

                    }
                    else
                    {
                        TextBlock_StatusBar_1.Text = "Device not found!";       // Wyswietl informacje ze nie znalezlismy urzadzenia
                        ToggleButton_Start.Content = "Start";
                        ToggleButton_Start.IsEnabled = !ToggleButton_Start.IsEnabled;               // Toggle button state
                    }


                }
                #endregion

                #region Jesli znamy juz port COM pod ktorym bylo nasze urzadzenie
                else
                {
                    TextBlock_StatusBar_1.Text = "Searching for device...";     // Wyswietl informacje ze szukamy urzadzenia

                    // Jesli nie to musimy je wyszukac ....
                    // ... jesli znalezlismy nasze urzadzenie.....
                    if (serialDeviceController.CheckForConnectedDevice("A1"))   // Jesli udalo Ci sie polaczyc
                    {
                        TextBlock_StatusBar_1.Text = "Found! Connecting...";


                    }
                    else
                    {
                        ApplicationProfile.LastCOMconnected = "";               // Wyczysc informacje o ostatnim porcie COM z profilu

                        AppProfileManager.Save_ApplicationProfile(ApplicationProfile);  // Zapisz profil 

                        TextBlock_StatusBar_1.Text = "Device not found!";       // Wyswietl informacje ze urzadzenie nie zostalo znalezione

                        ToggleButton_Start.Content = "Start";

                        ToggleButton_Start.IsEnabled = !ToggleButton_Start.IsEnabled;               // Toggle button state
                    }
                }
                #endregion

                #region Proces wyszukiwania zakonczony ... Podlaczamy sie

                if (serialDeviceController.DeviceFound)
                {
                    if (serialDeviceController.Initialize_Serial_Connection())
                    {
                        TextBlock_StatusBar_1.Text = "Connected";                                   // Wyswietl info ze jestesmy polaczeni
                        ToggleButton_Start.Content = "Stop";                                        // Zmien text na guziku tak zeby mozna bylo zatrzymac Ambi

                        if (tsk_RGBdata.Status == TaskStatus.Running)                               // Na wypadek disconnecta
                        {
                            Stop_Data_Aqquire = false;                                                  // Wznow transmisje
                        }
                        else
                        {
                            tsk_RGBdata.Start();                                                        // Uruchom zadanie
                        }

                        isRGBdataAqquireRunning = true;                                             // Zmien wartosc zmiennej informujacej czy smigamy z Ambi

                        TextBlock_StatusBar_1.Text = " Connected | Running";

                        ToggleButton_Start.IsEnabled = !ToggleButton_Start.IsEnabled;               // Toggle button state
                    }
                    else
                    {
                        TextBlock_StatusBar_1.Text = "Failed to connect";                           // Wyswietl info ze jestesmy polaczeni
                        ToggleButton_Start.Content = "Start";                                       // Zmien text na guziku tak zeby mozna bylo zatrzymac Ambi

                        ToggleButton_Start.IsEnabled = !ToggleButton_Start.IsEnabled;               // Toggle button state
                    }
                }


                #endregion
            }
        }

        /// <summary>
        /// Metoda odpowiedzialna za zatrzymanie Ambi
        /// </summary>
        private void Action_Stop()
        {
            ToggleButton_Start.Content      = "Start";                      // Zmieniamy napis na guziku
            isRGBdataAqquireRunning         = false;                        // Zmieniamy status naszej flagi
            Stop_Data_Aqquire               = true;                         // Zmieniamy flage aby Ambi nie wysylalo danych
            TextBlock_StatusBar_1.Text      = " Connected | Stopped";
            ToggleButton_Start.IsEnabled    = !ToggleButton_Start.IsEnabled;// Toggle button state
        }

        /// <summary>
        /// Metoda wyolywana w odpowiedzi na klikniecie "ustawien"
        /// Otwiera nowe okno
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Settings_OnClick(object sender, RoutedEventArgs e)
        {
            Window_Settings wdswSettings = new Window_Settings(ApplicationProfile, lcm);
            wdswSettings.ShowDialog();

            if (wdswSettings.needsSettingsUpdate)                               // Jesli zaszly zmiany
            {
                this.ApplicationProfile = wdswSettings.Get_New_Settings();      // Zapisujemy je do zmiennej
                Stop_Data_Aqquire = true;                                       // Zmieniamy flage aby Ambi nie wysylalo danych
                channelsManagement = new ChannelsManagement(ApplicationProfile.ChannelSettings, (int)SystemParameters.PrimaryScreenHeight, (int)SystemParameters.PrimaryScreenWidth);    // Ustawienia
                Stop_Data_Aqquire = false;                                      // Zmieniamy flage na nowo
            }

            if (this.ApplicationProfile.BaudRate != serialDeviceController.Baud_rate)
            {
                Stop_Data_Aqquire = true;                                       // Upewniamy sie ze nie bedzie wysylania danych
                serialDeviceController = ApplicationProfile.LastCOMconnected == "" ? new DeviceController(ApplicationProfile.BaudRate) : new DeviceController(ApplicationProfile.BaudRate, ApplicationProfile.LastCOMconnected);
                Stop_Data_Aqquire = false;                                      // Flaga zmieniona - kontroler COM zajmie sie sprawa nie wysylania
            }

        }

        #endregion


        private void AMLC_Frame_STOP()
        {
            if (serialDeviceController.DeviceConnected)
            {
                Stop_Data_Aqquire = true;                                           // Zmieniamy flage aby Ambi nie wysylalo danych
                Thread.Sleep(250);
               // serialDeviceController.Send_Data_To_Serial(new byte[1] { 0x02 });   // wypychamy dane na pasek
                serialDeviceController.Send_Data_To_Serial("");             // wylaczamy kolory :) - nie wymaga psh
                serialDeviceController.Send_Data_To_Serial("AT");             // wylaczamy kolory :) - nie wymaga psh
                serialDeviceController.Close_Serial_Connection();
            }
        }

        /// <summary>
        /// Metoda wywolywana w odpowiedzi na klikniecie opcji w tray
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Tray_Exit_OnClick(object sender, RoutedEventArgs e)
        {
            AMLC_Frame_STOP();
            Application.Current.Shutdown();
        }

        private void Slider_Brightness_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                ApplicationProfile.ChannelSettings.Brightness = Slider_Brightness.Value;

                AppProfileManager.Save_ApplicationProfile(ApplicationProfile);
            }
            catch (Exception)
            {
                
                
            }
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Massive_Color.Internal;
using Microsoft.Win32;


//TODO:  (xaml) Wylaczone Tabmenu Plugins i Firmware  
//TODO:  (xaml) Wylaczone numeric up downs dla GAMMA correction

namespace Massive_Color.Windows
{
    /// <summary>
    /// Interaction logic for Window_Settings.xaml
    /// </summary>
    public partial class Window_Settings
    {
        private ApplicationProfile  AppSettings;                        // Zmienna przechowujaca ustawienia
        public Boolean              needsSettingsUpdate { get; set; }   // Zmienna definiujaca czy musimy zrobic zmiane ustawien
        private List<LicenseInfo>   licInfo;
        
        /// <summary>
        /// Domyslny konstruktor przyjmujacy settings jako parametr
        /// </summary>
        /// <param name="appProfile"></param>
        public Window_Settings(ApplicationProfile appProfile,LicenseManager lcManager)
        {

            InitializeComponent();

            if (lcManager.isLicenseFile)
            {
                try
                {
                    TextBlock_License_Name_Value.Text       = lcManager.GetByKeyName("FirstName") + " " + lcManager.GetByKeyName("LastName");
                    TextBlock_License_Email_Value.Text      = lcManager.GetByKeyName("EMail");
                    TextBlock_License_Address_Value.Text    = lcManager.GetByKeyName("Address");
      
                }
                catch (Exception)
                {
                    
                    
                }
            }

            needsSettingsUpdate = false;

            this.AppSettings = appProfile;



            this.NumericUpDown_Channels_Top.Value       = AppSettings.ChannelSettings.Channels.Top;
            this.NumericUpDown_Channels_Right.Value     = AppSettings.ChannelSettings.Channels.Right;
            this.NumericUpDown_Channels_Bottom.Value    = AppSettings.ChannelSettings.Channels.Bottom;
            this.NumericUpDown_Channels_Left.Value      = AppSettings.ChannelSettings.Channels.Left;

            NumericUpDown_Pixel_Precision.Value = AppSettings.ChannelSettings.Pixel_Precision;
            NumericUpDown_Pixel_Treshold.Value = AppSettings.ChannelSettings.Pixel_Treshold;

            Slider_Borders_X.Value = AppSettings.ChannelSettings.Borders_X;
            Slider_Borders_Y.Value = AppSettings.ChannelSettings.Borders_Y;

            Slider_Padding_X.Value = AppSettings.ChannelSettings.Padding_X;
            Slider_Padding_Y.Value = AppSettings.ChannelSettings.Padding_Y;

            Slider_Deepth_X.Value = AppSettings.ChannelSettings.Channels_Depth_X;
            Slider_Deepth_Y.Value = AppSettings.ChannelSettings.Channels_Depth_Y;

            TextBox_SerialBaudRate.Text = AppSettings.BaudRate.ToString();

            NumericUpDown_Brightness.Value = AppSettings.ChannelSettings.Brightness;

            StackPanel_Graphics.Visibility      = Visibility.Visible;
            Button_apply_settings.Visibility    = Visibility.Visible;

            CheckBox_appAutoStart.IsChecked = AppSettings.Autostart;

            CheckBox_appAutoRun.IsChecked = AppSettings.AutoRun;

        }

        /// <summary>
        /// Metoda zwracajaca nasze ustawienia
        /// </summary>
        /// <returns></returns>
        public ApplicationProfile Get_New_Settings()
        {
            AppSettings.ChannelSettings.Channels.Top    = (int) this.NumericUpDown_Channels_Top.Value;
            AppSettings.ChannelSettings.Channels.Right  = (int) this.NumericUpDown_Channels_Right.Value;
            AppSettings.ChannelSettings.Channels.Bottom = (int) this.NumericUpDown_Channels_Bottom.Value;
            AppSettings.ChannelSettings.Channels.Left   = (int) this.NumericUpDown_Channels_Left.Value;

            AppSettings.ChannelSettings.Pixel_Precision = (int) NumericUpDown_Pixel_Precision.Value;
            AppSettings.ChannelSettings.Pixel_Treshold  = (int) NumericUpDown_Pixel_Treshold.Value;

            AppSettings.ChannelSettings.Borders_X = (int) Slider_Borders_X.Value;
            AppSettings.ChannelSettings.Borders_Y= (int) Slider_Borders_Y.Value;

            AppSettings.ChannelSettings.Padding_X = (int) Slider_Padding_X.Value;
            AppSettings.ChannelSettings.Padding_Y = (int) Slider_Padding_Y.Value;

            AppSettings.ChannelSettings.Channels_Depth_X = (int) Slider_Deepth_X.Value;
            AppSettings.ChannelSettings.Channels_Depth_Y = (int) Slider_Deepth_Y.Value;

            AppSettings.ChannelSettings.Brightness = (double) NumericUpDown_Brightness.Value;

            if (TextBox_SerialBaudRate.Text != AppSettings.BaudRate.ToString())
            {
                AppSettings.BaudRate = Convert.ToInt32(TextBox_SerialBaudRate.Text);
            }

            return this.AppSettings;
        }

        /// <summary>
        /// Metoda wywolywana w odpowiedzi na "apply"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_apply_settings_OnClick(object sender, RoutedEventArgs e)
        {

            AppSettings.BaudRate = Convert.ToInt32(TextBox_SerialBaudRate.Text);        // Jako , ze ta wartosc nie jest zapisywana przez OnChangedEvent

            needsSettingsUpdate = true;
            AppProfileManager.Save_ApplicationProfile(this.AppSettings);
            Show_Msg();
            
            
        }

        /// <summary>
        /// Metoda wyswietlajaca wiadomosc dla uzytkownika o poprawnym zapisaniu ustawien
        /// </summary>
        private async void  Show_Msg()
        {
            MessageDialogResult result = await this.ShowMessageAsync("Info", "Settings saved",
                MessageDialogStyle.Affirmative);            
        }

        #region Methods onValueChanged

        private void NumericUpDown_Brightness_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (NumericUpDown_Brightness.Value != null) AppSettings.ChannelSettings.Brightness = (double)this.NumericUpDown_Brightness.Value;
        }

        private void NumericUpDown_Channels_Top_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (NumericUpDown_Channels_Top.Value != null) AppSettings.ChannelSettings.Channels.Top = (int)this.NumericUpDown_Channels_Top.Value;
        }

        private void NumericUpDown_Channels_Right_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (NumericUpDown_Channels_Right.Value != null) AppSettings.ChannelSettings.Channels.Right = (int)this.NumericUpDown_Channels_Right.Value;
        }

        private void NumericUpDown_Channels_Bottom_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (NumericUpDown_Channels_Bottom.Value != null) AppSettings.ChannelSettings.Channels.Bottom = (int)this.NumericUpDown_Channels_Bottom.Value;
        }

        private void NumericUpDown_Channels_Left_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (NumericUpDown_Channels_Left.Value != null) AppSettings.ChannelSettings.Channels.Left = (int)this.NumericUpDown_Channels_Left.Value;
        }

        private void Slider_Borders_X_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.ChannelSettings.Borders_X = (int)Slider_Borders_X.Value;
        }

        private void Slider_Deepth_X_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.ChannelSettings.Channels_Depth_X = (int)Slider_Deepth_X.Value;            
        }

        private void Slider_Padding_X_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.ChannelSettings.Padding_X = (int)Slider_Padding_X.Value;
        }

        private void NumericUpDown_Pixel_Treshold_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            AppSettings.ChannelSettings.Pixel_Treshold = (int)NumericUpDown_Pixel_Treshold.Value;
        }

        private void NumericUpDown_Pixel_Precision_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if ( NumericUpDown_Pixel_Precision.Value != null)  AppSettings.ChannelSettings.Pixel_Precision = (int)NumericUpDown_Pixel_Precision.Value;         
        }

        private void Slider_Borders_Y_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.ChannelSettings.Borders_Y = (int)Slider_Borders_Y.Value;
        }

        private void Slider_Deepth_Y_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.ChannelSettings.Channels_Depth_Y = (int)Slider_Deepth_Y.Value;
        }

        private void Slider_Padding_Y_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.ChannelSettings.Padding_Y = (int)Slider_Padding_Y.Value;
        }

        #endregion

        /// <summary>
        /// Event zapewniajacy ze dostaniemy tylko liczby w naszym textboxie 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_SerialBaudRate_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Char.IsNumber(Convert.ToChar(e.Text));

            base.OnPreviewTextInput(e);
        }


        #region TAB menu control

        private void TabItem_Graphics_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel_Graphics.Visibility = Visibility.Visible;
            Button_apply_settings.Visibility = Visibility.Visible;

            StackPanel_AppSettings.Visibility = Visibility.Collapsed;
            StackPanel_Firmware.Visibility = Visibility.Collapsed;

            StackPanel_License.Visibility = Visibility.Collapsed;
            

        }

        private void TabItem_AppSettings_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel_Graphics.Visibility = Visibility.Collapsed;
            Button_apply_settings.Visibility = Visibility.Visible;

            StackPanel_AppSettings.Visibility = Visibility.Visible;
            StackPanel_Firmware.Visibility = Visibility.Collapsed;

            StackPanel_License.Visibility = Visibility.Collapsed;
        }

        private void TabItem_Firmware_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel_Graphics.Visibility = Visibility.Collapsed;
            Button_apply_settings.Visibility = Visibility.Collapsed;

            StackPanel_AppSettings.Visibility = Visibility.Collapsed;
            StackPanel_Firmware.Visibility = Visibility.Visible;

            StackPanel_License.Visibility = Visibility.Collapsed;
        }

        private void TabItem_Plugins_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void TabItem_Licensing_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel_License.Visibility = Visibility.Visible;

            StackPanel_Graphics.Visibility = Visibility.Collapsed;
            Button_apply_settings.Visibility = Visibility.Collapsed;

            StackPanel_AppSettings.Visibility = Visibility.Collapsed;
            StackPanel_Firmware.Visibility = Visibility.Collapsed;
        }

        #endregion


        //private void Button_Choose_Firmware_File_OnClick(object sender, RoutedEventArgs e)
        //{
        //    OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



        //    // Set filter for file extension and default file extension 
        //    dlg.DefaultExt = ".frm";
        //    dlg.Filter = "Firmware v1 Files (*.frm)|*.frm|Firmware v2 Files (*.fr2)|*.fr2";


        //    // Display OpenFileDialog by calling ShowDialog method 
        //    var result = dlg.ShowDialog();


        //    // Get the selected file name and display in a TextBox 
        //    if (result == true)
        //    {
        //        // Open document 
        //        string filename = dlg.FileName;
        //        //textBox1.Text = filename;
        //    }
        //}

        //private void Button_Firmware_Upload_OnClick(object sender, RoutedEventArgs e)
        //{
        //    //progressBar1.Maximum = updater.arrOfBytes.Length;
        //    //progressBar1.Step = 1;
        //    Updater upd = new Updater(128, @"C:\1_3.hex");
        //    upd.GetByteArrayFromHex();
        //    upd.UploadHexViaSerialPort("COM3", true, null, 230400);

        //}

        #region Application Autostart

        private void CheckBox_appAutoStart_OnChecked(object sender, RoutedEventArgs e)
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (!IsStartupItem())
            {
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue("MassiveColor", System.Reflection.Assembly.GetExecutingAssembly().Location);

                needsSettingsUpdate = true;
                AppSettings.Autostart = (bool)CheckBox_appAutoStart.IsChecked;

            }
        }

        private void CheckBox_appAutoStart_OnUnchecked(object sender, RoutedEventArgs e)
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (IsStartupItem())
            {
                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue("MassiveColor", false);

                needsSettingsUpdate = true;
                AppSettings.Autostart = (bool)CheckBox_appAutoStart.IsChecked;
            }
        }

        private bool IsStartupItem()
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rkApp.GetValue("MassiveColor") != null && rkApp.GetValue("MassiveColor").ToString() != System.Reflection.Assembly.GetExecutingAssembly().Location)
            {
                // Remove the value from the registry so that the application doesn't start with incorrect path
                rkApp.DeleteValue("MassiveColor", false);

                return false;
            }

            if (rkApp.GetValue("MassiveColor") == null)
                // The value doesn't exist, the application is not set to run at startup
                return false;
            else
            {
                // The value exists, the application is set to run at startup
                return true;
            }
        }

        #endregion

        #region Application AutoRun

        private void CheckBox_appAutoRun_OnUnchecked(object sender, RoutedEventArgs e)
        {

            needsSettingsUpdate = true;
            AppSettings.AutoRun = (bool)CheckBox_appAutoRun.IsChecked;

        }

        private void CheckBox_appAutoRun_OnChecked(object sender, RoutedEventArgs e)
        {

            needsSettingsUpdate = true;
            AppSettings.AutoRun = (bool)CheckBox_appAutoRun.IsChecked;

        }

        #endregion
    }
}

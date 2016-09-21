using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RafTech.Graphics.RGCapture.x64
{
    public class ChannelsManagement
    {
        // Properties holding information regarding screen size
        private int Screen_Max_X_Original_Size { get; set; }     // Screen X-axis - original size
        private int Screen_Max_Y_Original_Size { get; set; }     // Screen Y-axis - original size

        private int Screen_Max_X                 { get; set; }     // Max screen X-axis after borders
        private int Screen_Max_Y                 { get; set; }     // Max screen Y-axis after borders
        private int Screen_Min_X                 { get; set; }     // Min screen X-axis after borders
        private int Screen_Min_Y                 { get; set; }     // Min screen Y-axis after borders

        private int Screen_ErrorMargin = 5;                        // Value used to avoid maxing out beyond screen resolution

        // Values used for determination of channels positions / precisions / tresholds
        // ----------------------------------------------------------------------------

        // Dystanse od krawedzi ekranu podane do obliczen w pixelach lub w procentach
        private int Distance_From_Top                 { get; set; }       // Dystans od gory do krawedzi kanalow (TOP_Y_MIN)
        private int Distance_From_Bottom              { get; set; }       // Dystans od dolu do krawedzi kanalow (BOTTOM_Y_MAX)
        private int Distance_From_Left                { get; set; }       // Dystans od lewego boku do krawedzi kanalow (LEFT_MIN_X )
        private int Distance_From_Right               { get; set; }       // Dystans od prawego boku do krawedzi kanalow (RIGHT_MAX_X)

        public bool DistanceCalculations_Percentage = true;              // Dystans jest podawany w procentach - w przeciwnym wypadku w pixelach

        // Obliczone wartosci ekranu jako obramowanie naszego AmbiLight
        private int min_left        { get; set; }
        private int max_right       { get; set; }
        private int min_top         { get; set; }
        private int max_bottom      { get; set; }

        // Percentage setting for Channel Depth (defining how big our blocks will be)

        // Define Channels percentage width/height
        private int Channel_Depth_Size_Columns { get; set; }             // Procent do jakiego kanaly z obu stron (lewej i prawej) beda dochodzic 
        private int Channel_Depth_Size_Rows    { get; set; }             // Procent do jakiego kanaly z gory i dolu beda dochodzic

        // Define Channels borders margin - THIS CAN HAVE NEGATIVE IMPACT ON COLOR DETECTION!
        private int Channel_Border_Size_LeftRight { get; set; }           // Procent na jaki odsuniete sa nasze kanaly od brzegu ekranu
        private int Channel_Border_Size_TopBottom { get; set; }           // Procent na jaki odsuniete sa nasze kanaly od gory/dolu ekranu

        // Define Channels corners padding
        private int Channel_Corner_Padding_Columns  { get; set; }       // PErcentage of corner padding in columns 
        private int Channel_Corner_Padding_Rows     { get; set; }       // Percentage of corner padding in rows

        // Define number of channels
        private int Channels_top                    { get; set; }
        private int Channels_right                  { get; set; }
        private int Channels_bottom                 { get; set; }
        private int Channels_left                   { get; set; }

        // Define channel precision - so how many samples will be taken into account when averaging colors 
        // Higher Number == better results / slowe | Lower number == acceptable results / faster 
        private int Channel_precision    { get; set; }
        
        // Define pixel treshold - so minimal color we want to have to consider pixel
        public int Pixel_Min_Treshold { get; set; }

        // Define if we would like to do extra color averaging
        public bool Average_Strong_Colors               { get; set; }
        public int  Average_Strong_Colors_Adjustment    { get; set; }

        // Calculated channels
        public List<LightChannel> LightChannels { get; set; }           // Nasze kanaly


        /// <summary>
        /// Glowny konstruktor odpowiedzialny za przyjecie wszystkich informacji i ich dalsze przetworzenie
        /// </summary>
        /// <param name="Screen_Y"></param>
        /// <param name="Screen_X"></param>
        /// <param name="BordersX"></param>
        /// <param name="BordersY"></param>
        /// <param name="BlockDepthSizeX"></param>
        /// <param name="BlockDepthSizeY"></param>
        /// <param name="Padding_Columns"></param>
        /// <param name="Padding_Rows"></param>
        /// <param name="Precision"></param>
        /// <param name="PixelTreshold"></param>
        /// <param name="Top"></param>
        /// <param name="Right"></param>
        /// <param name="Bottom"></param>
        /// <param name="Left"></param>
        public ChannelsManagement(int Screen_Y, int Screen_X, int BordersX, int BordersY, int BlockDepthSizeX, int BlockDepthSizeY,
            int Padding_Columns,int Padding_Rows , int Precision , int PixelTreshold, int Top,int Right,int Bottom,int Left)
        {
            Screen_Max_X_Original_Size = Screen_X;                      // Zapisujemy rozmiar oryginalnego X
            Screen_Max_Y_Original_Size = Screen_Y;                      // Zapisujemy rozmiar oryginalnego Y

            Channel_Depth_Size_Columns  = BlockDepthSizeY;              // Jak gleboko (w procentach % ) powinny wejsc kanaly ( boki )
            Channel_Depth_Size_Rows     = BlockDepthSizeX;              // Jak gleboko (w procentach % ) powinny wejsc kanaly ( gora / dol )

            Channel_Border_Size_TopBottom = BordersX;                   // Jak bardzo odsuniete sa kanaly od gory / dolu
            Channel_Border_Size_LeftRight = BordersY;                   // Jak bardzo odsuniete sa kanaly od lewej / prawej

            Channel_Corner_Padding_Columns  = Padding_Columns;          // Corner Padding - kolumny
            Channel_Corner_Padding_Rows     = Padding_Rows;             // Corner Padding - wiersze

            Channel_precision = Precision;                              // Z jaka dokladnoscia bedziemy wyznaczac pixele

            Pixel_Min_Treshold = PixelTreshold;                         // Jakiej wartosci pixele (minimum) wezmiemy pod uwage w kalkulacjach

            Channels_top        = Top;                                  // Ile mamy kanalow na topie 
            Channels_bottom     = Bottom;                               // Ile mamy kanalow na dole
            Channels_right      = Right;                                // Ile mamy kanalow na prawej
            Channels_left       = Left;                                 // Ile mamy kanalow z lewej

            Calculate_Borders();                                        // Oblicz granice
            Calculate_Corner_Padding();                                 // Oblicz Corner Padding
            CalculateChannels(true);                                    // Oblicz kanaly

        }

        /// <summary>
        /// Metoda sluzaca do rekonfiguracji kanalow 
        /// </summary>
        /// <param name="ust"></param>
        public List<LightChannel> ReconfigureChannels(ChannelSettings ust)
        {
            Channel_Depth_Size_Columns = ust.Channels_Depth_Y;              // Jak gleboko (w procentach % ) powinny wejsc kanaly ( boki )
            Channel_Depth_Size_Rows = ust.Channels_Depth_X;              // Jak gleboko (w procentach % ) powinny wejsc kanaly ( gora / dol )

            Channel_Border_Size_TopBottom = ust.Borders_X;                   // Jak bardzo odsuniete sa kanaly od gory / dolu
            Channel_Border_Size_LeftRight = ust.Borders_Y;                   // Jak bardzo odsuniete sa kanaly od lewej / prawej

            Channel_Corner_Padding_Columns  = ust.Padding_Y;          // Corner Padding - kolumny
            Channel_Corner_Padding_Rows     = ust.Padding_X;             // Corner Padding - wiersze

            Channel_precision = ust.Pixel_Precision;                              // Z jaka dokladnoscia bedziemy wyznaczac pixele

            Pixel_Min_Treshold = ust.Pixel_Treshold;                         // Jakiej wartosci pixele (minimum) wezmiemy pod uwage w kalkulacjach

            Channels_top = ust.Channels.Top;                                  // Ile mamy kanalow na topie 
            Channels_bottom = ust.Channels.Bottom;                               // Ile mamy kanalow na dole
            Channels_right = ust.Channels.Right;                                // Ile mamy kanalow na prawej
            Channels_left = ust.Channels.Left;                                 // Ile mamy kanalow z lewej

            Calculate_Borders();                                        // Oblicz granice
            Calculate_Corner_Padding();                                 // Oblicz Corner Padding
            return CalculateChannels();                                         // Oblicz kanaly i zwroc obliczona liste
        }

        public List<LightChannel> CalculateLightChannels()
        {
            Calculate_Borders();                                                // Oblicz granice
            Calculate_Corner_Padding();                                         // Oblicz Corner Padding
            return CalculateChannels();                                         // Oblicz kanaly i zwroc obliczona liste
        }

        /// <summary>
        /// Funkcja odpowiedzialna za obliczenie ograniczenia ekranu
        /// </summary>
        private void Calculate_Borders()
        {
            Screen_Min_X = (Screen_Max_X_Original_Size * Channel_Border_Size_LeftRight) / 100;                        // Obliczmy minialna lewa pozycje
            Screen_Max_X = Screen_Max_X_Original_Size - ((Screen_Max_X_Original_Size * Channel_Border_Size_LeftRight) / 100);  // Obliczamy maxymalnie prawa pozycje

            Screen_Min_Y = (Screen_Max_Y_Original_Size * Channel_Border_Size_TopBottom) / 100;                       // Obliczamy minimalna gorna pozycje
            Screen_Max_Y = Screen_Max_Y_Original_Size - ((Screen_Max_Y_Original_Size * Channel_Border_Size_TopBottom) / 100);    // Obliczamy maxymalnie dolna pozycje

        }

        /// <summary>
        /// Metoda odpowiedzialna za stworzenie odpowiednich bokow - tzw pustch przestrzeni miedzy kolumnami a wierszami
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        /// <param name="use_percentage"></param>
        private void Calculate_Corner_Padding()
        {

            // Zapisujemy wlasnosci poszczegolnych miejsc
            Distance_From_Top       = Channel_Corner_Padding_Columns;
            Distance_From_Bottom    = Channel_Corner_Padding_Columns;

            Distance_From_Left      = Channel_Corner_Padding_Rows;
            Distance_From_Right     = Channel_Corner_Padding_Rows;

            min_left = ( ( (Screen_Max_X - Screen_Min_X) * Distance_From_Left) / 100 ) + Screen_Min_X; // Obliczmy minialna lewa pozycje
            max_right = Screen_Max_X - ((Screen_Max_X * Distance_From_Right) / 100);  // Obliczamy maxymalnie prawa pozycje

            min_top = ( ((Screen_Max_Y - Screen_Min_Y) * Distance_From_Top) / 100) + Screen_Min_Y;                       // Obliczamy minimalna gorna pozycje
            max_bottom = Screen_Max_Y - ((Screen_Max_Y * Distance_From_Bottom) / 100);    // Obliczamy maxymalnie dolna pozycje

        }

        /// <summary>
        /// Metoda odpowiedzialna za stworzenie kanalow
        /// </summary>
        /// <returns></returns>
        private List<LightChannel> CalculateChannels()
        {
            Create_Light_Channels();        // Tworzymy nasze kanaly

            foreach (LightChannel lightChannel in LightChannels)
            {
                SetUpChannelPositions(lightChannel, 20, Screen_Max_X - 50);
            }


            //Parallel.ForEach(LightChannels, lc => SetUpChannelPositions(lc, Channel_precision < 5 ? 5 : Channel_precision, Screen_Max_X)); // Obliczamy pozycje w naszych kanalach

            return this.LightChannels;

        }

        /// <summary>
        /// Metoda odpowiedzialna za stworzenie kanalow
        /// </summary>
        /// <returns></returns>
        public void CalculateChannels(bool noreturn)
        {
            Create_Light_Channels();        // Tworzymy nasze kanaly

            Parallel.ForEach(LightChannels, lc => SetUpChannelPositions(lc, Channel_precision < 5 ? 5 : Channel_precision, Screen_Max_X)); // Obliczamy pozycje w naszych kanalach

        }

        /// <summary>
        /// Metoda odpowiedzialna za stworzenie kanalow
        /// </summary>
        /// <param name="LightChannels"></param>
        /// <returns></returns>
        private void Create_Light_Channels()
        {


            List<LightChannel> temp_list = new List<LightChannel>();            // Lista uzywana do przechowania wynikow zwracanych do aplikacji


            // Zmiennie definiujace maxymalne wartosci X/Y dla tworzenia kanalow
            int Channels_Begin_X=0, 
                Channels_Begin_Y=0, 
                Channels_Stop_X=0, 
                Channels_Stop_Y=0, 
                Channel_X=0, 
                Channel_Y=0, 
                Channel_X_Width=0, 
                Channel_Y_Height=0;     



            #region Channels - TOP

            LightChannelBase channelBase_rows = new LightChannelBase();

            /* 
             * Zaczynamy od ustalenia wartosci wyznaczajacych nasze kanaly. Jako , ze obliczylismy juz granice / corner padding mozemy po prostu wyznaczyc 
             * sobie punkty skrajne i na podstawie ilosci kanalow wyznaczyc obramowania
             */

            channelBase_rows.Stop_X     = max_right;                                                                // 
            channelBase_rows.Start_X    = min_left;                                                                 // 
            channelBase_rows.Start_Y    = Screen_Min_Y;
            channelBase_rows.Height     = ( Screen_Max_Y * Channel_Depth_Size_Rows) / 100;                          // 
            channelBase_rows.Width      = (channelBase_rows.Stop_X - channelBase_rows.Start_X) / Channels_top;      // 
            channelBase_rows.Stop_Y     = channelBase_rows.Start_Y + channelBase_rows.Height;

            /*
             * Ustalamy pozycje startowa X 
             */
            Channel_X = channelBase_rows.Start_X;

            /*
             * Obliczamy kanaly na podstawie danych , ktore juz mamy 
             */

            for (int r = 0; r < Channels_top; r++)
            {
                LightChannel testAmbilightChannel = new LightChannel
                {
                    MinX = Channel_X,                                // Ta wartosc sie zmienia i jest inkrementowana ---
                    MinY = channelBase_rows.Start_Y,
                    MaxX = Channel_X + channelBase_rows.Width - 1,          // Ta wartosc sie zmienia i jest inkrementowana ---
                    MaxY = channelBase_rows.Stop_Y,      // Na podstawie bordero'w ustalamy maxymalny Y                    
                    positions = new Collection<long>(),
                    Location = ChannelPosition.Top,
                    Height = channelBase_rows.Height,
                    Width = channelBase_rows.Width
                };

                temp_list.Add(testAmbilightChannel);                 // Dodajemy do naszej listy kanalow

                Channel_X += channelBase_rows.Width;                        // Dodajemy odpowiednio dlugosc kanalu aby nastepny sie poprawnie zaczynal

            }

            var debug = "";

            #endregion

            #region Channels - RIGHT

            LightChannelBase channelBase_columns = new LightChannelBase();

            /* 
             * Zaczynamy od ustalenia wartosci wyznaczajacych nasze kanaly. Jako , ze obliczylismy juz granice / corner padding mozemy po prostu wyznaczyc 
             * sobie punkty skrajne i na podstawie ilosci kanalow wyznaczyc obramowania
             */

            channelBase_columns.Stop_X      = Screen_Max_X;                                                                 // Ok 
            channelBase_columns.Start_X     = Screen_Max_X - ((Screen_Max_X * Channel_Depth_Size_Columns) / 100);           // Ok                                                    // 
            channelBase_columns.Start_Y     = min_top;                                                                      // Ok
            channelBase_columns.Stop_Y      = max_bottom;                                                                   // Ok
            channelBase_columns.Height      = (channelBase_columns.Stop_Y - channelBase_columns.Start_Y) / Channels_right;  // Ok
            channelBase_columns.Width       = channelBase_columns.Stop_X - channelBase_columns.Start_X;                     // Ok

            /*
             * Ustalamy pozycje startowe X oraz Y dla pierwszego kanalu 
             */
            Channel_Y = channelBase_columns.Start_Y;

            /*
             * Obliczamy kanaly na podstawie danych , ktore juz mamy 
             */

            for (int r = 0; r < Channels_right; r++)
            {
                LightChannel testAmbilightChannel = new LightChannel
                {
                    MinX = channelBase_columns.Start_X,
                    MinY = Channel_Y,                                // Ta wartosc sie zmienia i jest inkrementowana ---
                    MaxX = channelBase_columns.Stop_X,
                    MaxY = Channel_Y + channelBase_columns.Height - 1,          // Ta wartosc sie zmienia i jest inkrementowana ---
                    positions = new Collection<long>(),
                    Location = ChannelPosition.Right,
                    Height = channelBase_columns.Height,
                    Width = channelBase_columns.Width
                };

                temp_list.Add(testAmbilightChannel);                 // Dodajemy do naszej listy kanalow

                Channel_Y += channelBase_columns.Height;                        // Dodajemy odpowiednio dlugosc kanalu aby nastepny sie poprawnie zaczynal

            }

            #endregion

            #region Channels - BOTTOM

            /* 
             * Zaczynamy od ustalenia wartosci wyznaczajacych nasze kanaly. Jako , ze obliczylismy juz granice / corner padding mozemy po prostu wyznaczyc 
             * sobie punkty skrajne i na podstawie ilosci kanalow wyznaczyc obramowania
             */
                                                              
            channelBase_rows.Start_Y = Screen_Max_Y - channelBase_rows.Height;            
            channelBase_rows.Width = (channelBase_rows.Stop_X - channelBase_rows.Start_X) / Channels_bottom;       
            channelBase_rows.Stop_Y = Screen_Max_Y;

            /*
             * Ustalamy pozycje startowa X 
             */
            Channel_X = channelBase_rows.Stop_X;

            /*
             * Obliczamy kanaly na podstawie danych , ktore juz mamy 
             */

            for (int r = 0; r < Channels_bottom; r++)
            {
                LightChannel testAmbilightChannel = new LightChannel
                {
                    MaxX = Channel_X,                                // Ta wartosc sie zmienia i jest inkrementowana ---
                    MinY = channelBase_rows.Start_Y,
                    MinX = Channel_X - channelBase_rows.Width + 1,          // Ta wartosc sie zmienia i jest inkrementowana ---
                    MaxY = channelBase_rows.Stop_Y,      // Na podstawie bordero'w ustalamy maxymalny Y                    
                    positions = new Collection<long>(),
                    Location = ChannelPosition.Bottom,
                    Height = channelBase_rows.Height,
                    Width = channelBase_rows.Width
                };

                temp_list.Add(testAmbilightChannel);                 // Dodajemy do naszej listy kanalow

                Channel_X -= channelBase_rows.Width;                        // Dodajemy odpowiednio dlugosc kanalu aby nastepny sie poprawnie zaczynal

            }

            #endregion

            #region Channels - LEFT            

            /* 
             * Zaczynamy od ustalenia wartosci wyznaczajacych nasze kanaly. Jako , ze obliczylismy juz granice / corner padding mozemy po prostu wyznaczyc 
             * sobie punkty skrajne i na podstawie ilosci kanalow wyznaczyc obramowania
             */

            channelBase_columns.Stop_X = ((Screen_Max_X*Channel_Depth_Size_Columns)/100);                              // OK
            channelBase_columns.Start_X = Screen_Min_X;                                                                // Ok 
            channelBase_columns.Start_Y = min_top;                                                                     // Ok
            channelBase_columns.Stop_Y = max_bottom;                                                                   // Ok
            channelBase_columns.Height = (channelBase_columns.Stop_Y - channelBase_columns.Start_Y) / Channels_left;   // Ok
            channelBase_columns.Width  = channelBase_columns.Stop_X - channelBase_columns.Start_X;                     // Ok

            /*
             * Ustalamy pozycje startowe X oraz Y dla pierwszego kanalu 
             */
            Channel_Y = channelBase_columns.Stop_Y;

            /*
             * Obliczamy kanaly na podstawie danych , ktore juz mamy 
             */

            for (int r = 0; r < Channels_left; r++)
            {
                LightChannel testAmbilightChannel = new LightChannel
                {
                    MinX = channelBase_columns.Start_X,
                    MaxY = Channel_Y,                                // Ta wartosc sie zmienia i jest inkrementowana ---
                    MaxX = channelBase_columns.Stop_X,
                    MinY = Channel_Y - channelBase_columns.Height + 1,          // Ta wartosc sie zmienia i jest inkrementowana ---
                    positions = new Collection<long>(),
                    Location = ChannelPosition.Left,
                    Height = channelBase_columns.Height,
                    Width = channelBase_columns.Width
                };

                temp_list.Add(testAmbilightChannel);                 // Dodajemy do naszej listy kanalow

                Channel_Y -= channelBase_columns.Height;                        // Dodajemy odpowiednio dlugosc kanalu aby nastepny sie poprawnie zaczynal

            }

            #endregion

            this.LightChannels = temp_list;



        }

        /// <summary>
        /// calculates channels with defined precision
        /// </summary>
        /// <param name="channel">Ambichannel object containing parameters for calculations</param>
        /// <param name="precision">How many positions we will take to get average RGB color of perimeter</param>
        /// <param name="ScreenXsize">Screen width</param>
        public void SetUpChannelPositions(LightChannel channel, int precision, int ScreenXsize)
        {

            //Forumla na obliczenie pozycji piksela 
            //przy pomocy "graphics stream"
            //
            // Index = (x + y * screenwidth) * SizeOf(Pixel)
            // 
            // pos = (x+y*screenwidth) * 4
            // 
            // 4 - poniewaz kazdy pixel ma 4 kanaly ARGB kazdy z 1 byte
            // 

            // Current position
            int current_pos = 0;

            // REMOVE ALL PREVIOUS POSTIONS
            channel.positions.Clear();

            // This is where all the magic happens: calculate the average RGB
            for (int j = channel.MinY; j <= channel.MaxY; j += precision)
            {

                for (int i = channel.MinX; i <= channel.MaxX; i += precision)
                {

                    //current_pos = (4 * j * ScreenXsize) + (4 * i);
                    current_pos = (i + j * ScreenXsize) * 4;

                    channel.positions.Add(current_pos);


                    // [CHECK] if we do not exceed max
                    if ((i + precision) > channel.MaxX)
                    {
                        i = channel.MaxX;
                    }

                }

                // [CHECK] if we do not exceed max
                if ((j + precision) > channel.MaxY)
                {
                    j = channel.MaxY;
                }
            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RafTech.Graphics.RGCapture.x64
{
    /// <summary>
    /// Class that defines single AmbiLight channel - we can 
    /// have more than one which then are hold in array :)
    /// </summary>
    public class LightChannel
    {
        public int MinY                         { get; set; }
        public int MinX                         { get; set; }
        public int MaxY                         { get; set; }
        public int MaxX                         { get; set; }
        public int Width                        { get; set; }
        public int Height                       { get; set; }
        public int R                            { get; set; }
        public int G                            { get; set; }
        public int B                            { get; set; }
        public Collection<long>     positions   { get; set; }
        public ChannelPosition      Location    { get; set; }
        public Bitmap screenShot                { get; set; }
    }

    public class LightChannelBase
    {
        public int Start_X      { get; set; }
        public int Start_Y      { get; set; }
        public int Stop_X       { get; set; }
        public int Stop_Y       { get; set; }
        public int Width        { get; set; }
        public int Height       { get; set; }

    }

    public enum ChannelPosition
    {
        Top,
        Right,
        Bottom,
        Left
    }

    /// <summary>
    /// Klasa odpowiedzialna za ustawienia
    /// </summary>
    public class ChannelSettings
    {
        public int Borders_X { get; set; }
        public int Borders_Y { get; set; }

        public int Channels_Depth_X { get; set; }
        public int Channels_Depth_Y { get; set; }

        public int Padding_X { get; set; }
        public int Padding_Y { get; set; }

        public int Pixel_Precision      { get; set; }
        public int Pixel_Treshold       { get; set; }

        public ChannelsNo Channels      { get; set; }

        public Boolean Adjust_RGB       { get; set; }

        public int Serial_baud          { get; set; }


        /// <summary>
        /// Domyslny konstruktor - bedzie zawierac nasze domyslne ustawienia
        /// </summary>
        public ChannelSettings()
        {
            this.Borders_X = 0;
            this.Borders_Y = 0;

            this.Channels_Depth_X = 10;
            this.Channels_Depth_Y = 10;

            this.Padding_X = 10;
            this.Padding_Y = 10;

            this.Pixel_Precision = 10;
            this.Pixel_Treshold = 1;

            this.Adjust_RGB = false;

            this.Channels = new ChannelsNo();

            this.Serial_baud = 230400;
        }


    }

    /// <summary>
    /// Klasa do przetrzymywania informacji o ilosci kanalow
    /// Domyslnie - nieskonfigurowany system ustawia po 1 :)
    /// </summary>
    public class ChannelsNo
    {
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public int Left { get; set; }

        public ChannelsNo()
        {
            this.Top        = 1;
            this.Right      = 1;
            this.Bottom     = 1;
            this.Left       = 1;
        }
    }
}

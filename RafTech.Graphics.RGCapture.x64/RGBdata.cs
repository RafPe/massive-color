using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;

namespace RafTech.Graphics.RGCapture.x64
{
    /// <summary>
    /// Class used for graphical operations on screen
    /// </summary>
    public class RGBdata
    {

        Device d;

        /// <summary>
        /// Constructor class
        /// </summary>
        public RGBdata()
        {
            PresentParameters present_params = new PresentParameters();
            present_params.Windowed = true;
            present_params.SwapEffect = SwapEffect.Discard;
            d = new Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, present_params);
        }

        /// <summary>
        /// Class responsible for capturing screen
        /// </summary>
        /// <param name="ScreenWidth">X-axis</param>
        /// <param name="ScreenHeight">Y-axis</param>
        /// <returns>Surface object</returns>
        public Surface CaptureScreen(int ScreenWidth, int ScreenHeight)
        {
            Surface s = Surface.CreateOffscreenPlain(d, ScreenWidth, ScreenHeight, Format.A8R8G8B8, Pool.Scratch);
            d.GetFrontBufferData(0, s);
            return s;
        }

        /// <summary>
        /// Function responsible for averaging color within the given positions
        /// </summary>
        /// <param name="gs">DataStream object - comes from screen capture</param>
        /// <param name="channel"></param>
        /// <param name="pixelColorTreshold"></param>
        /// <returns></returns>
        public Color AverageScreenRGBfromChannel(DataStream gs, LightChannel channel, int pixelColorTreshold)
        {
            // Byte definition for pixel
            byte[] bu = new byte[4];

            // End values for RGB
            int r = 0;
            int g = 0;
            int b = 0;

            int i = 1;

            // Temporary values for RGB
            int temp_r = 0;
            int temp_g = 0;
            int temp_b = 0;

            // For every position we have within our channel
            // loop through it
            foreach (long pos in channel.positions)
            {
                // Get details for current screen position
                gs.Position = pos;
                gs.Read(bu, 0, 4);

                // Assign result to temporary variables
                temp_r = bu[2];
                temp_g = bu[1];
                temp_b = bu[0];

                // Check if our color is within minial required treshold.
                // If black we dont need it :)
                if ((temp_r + temp_g + temp_b) > pixelColorTreshold)
                {
                    r += temp_r;
                    g += temp_g;
                    b += temp_b;

                    // Increment the count of accounted colors
                    i++;
                }


            }

            r = r / i != 0 ? r / i : 1;
            g = g / i != 0 ? g / i : 1;
            b = b / i != 0 ? b / i : 1;

            channel.R = r;
            channel.G = g;
            channel.B = b;

            return Color.FromArgb(1, 1, 1);
        }

        /// <summary>
        /// Function to adjust colors that are dominant within specified area!
        /// </summary>
        /// <param name="channel">Ambi Channel</param>
        /// <param name="adjustment">what should be the default adjustment</param>
        public void AdjustAverageRGB(LightChannel channel, int adjustment)
        {

            if (channel.R >= channel.G && channel.R >= channel.B)
            {
                // Red is the most dominant
                if (channel.G >= channel.B)
                {
                    // Red > Green > Blue
                    channel.R += adjustment * (channel.R - channel.G);
                    channel.B -= adjustment * (channel.G - channel.B);
                }
                else
                {
                    // Red > Blue > Green
                    channel.R += adjustment * (channel.R - channel.B);
                    channel.G -= adjustment * (channel.B - channel.G);
                }
            }
            else if (channel.G >= channel.B && channel.G >= channel.R)
            {
                // Green is the most dominant
                if (channel.R >= channel.B)
                {
                    // Green > Red > Blue
                    channel.G += adjustment * (channel.G - channel.R);
                    channel.B -= adjustment * (channel.R - channel.B);
                }
                else
                {
                    // Green > Blue > Red
                    channel.G += adjustment * (channel.G - channel.B);
                    channel.R -= adjustment * (channel.B - channel.R);
                }
            }
            else
            {
                // Blue is the most dominant
                if (channel.R >= channel.G)
                {
                    // Blue > Red > Green
                    channel.B += adjustment * (channel.B - channel.R);
                    channel.G -= adjustment * (channel.R - channel.G);
                }
                else
                {
                    // Blue > Green > Red
                    channel.B += adjustment * (channel.B - channel.G);
                    channel.R -= adjustment * (channel.G - channel.R);
                }
            }


            // Check red bounds
            if (channel.R > 255) channel.R = 255;
            //else if (process && r < MinRed) r = MinRed;
            else if (channel.R < 0) channel.R = 0;

            // Check green bounds
            if (channel.G > 255) channel.G = 255;
            //else if (process && g < MinGreen) g = MinGreen;
            else if (channel.G < 0) channel.G = 0;

            // Check blue bounds
            if (channel.B > 255) channel.B = 255;
            //else if (process && b < MinBlue) b = MinBlue;
            else if (channel.B < 0) channel.B = 0;

        }

        /// <summary>
        /// Funkcja obliczajaca array do wyslania
        /// </summary>
        /// <param name="kanaly"></param>
        /// <returns></returns>
        public byte[] CalculateRGBarray(List<LightChannel> kanaly)
        {
            byte[] naszArrBytes = new byte[kanaly.Count * 3];
            int cnt = 0;

            // Przygotuj array do wyslania
            for (int i = 0; i < (kanaly.Count); i++)
            {
                cnt = i * 3;

                #region G adjust
                if (kanaly[i].G == 13)
                {
                    kanaly[i].G = 12;
                }
                else if (kanaly[i].G == 1 || kanaly[i].G == 2)
                {
                    kanaly[i].G = 0;
                }
                #endregion

                #region R
                if (kanaly[i].R == 13)
                {
                    kanaly[i].R = 12;
                }
                else if (kanaly[i].R == 1 || kanaly[i].R == 2)
                {
                    kanaly[i].R = 0;
                }
                #endregion

                #region B
                if (kanaly[i].B == 13)
                {
                    kanaly[i].B = 12;
                }
                else if (kanaly[i].B == 1 || kanaly[i].B == 2)
                {
                    kanaly[i].B = 0;
                }
                #endregion





                naszArrBytes[cnt] = Convert.ToByte(kanaly[i].G);

                naszArrBytes[cnt + 1] = Convert.ToByte(kanaly[i].R);

                naszArrBytes[cnt + 2] = Convert.ToByte(kanaly[i].B);


            }

            return naszArrBytes;
        }


    }


}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RafTech.Communications.Serial
{
    /// <summary>
    /// Klasa uzywana do indentyfikacji urzadzenia
    /// Wspolpracuje z :
    /// [ 1 ] : Podstawowa wersja identyfikacyjna ver.YY.MM.PID.HV.SVMajor.SVMinor
    /// </summary>
    public class DeviceIdentity
    {
        public readonly DateTime            ProductionDate;         // Zmienna w ktorej trzymamy rok i miesiac produkcji
        public readonly int                 ATIver;                 // Zmienna dzieki ktorej wiemy z jakim typem urzadzenia mamy do czynienia (ktora wersja)
        public readonly String              ProductIdentifier;      // Zmienna identyfikujaca nasz produkt
        public readonly int                 HardwareVersion;        // Wersja Hardware naszego sprzetu
        public readonly FirmwareVersion     Firmware;               // Wersja Firmware naszego softu 


        private readonly Boolean IsSuccessfullyID;
        private static readonly Regex regX = new Regex(@"v\d[.]\d{2}[.]\d{2}[.][A-Z]\d{1}[.]\d{1}[.]\d{1}[.]\d{1}");
        private static readonly string dateFormat = "yy-MM";
        private static readonly CultureInfo us = new CultureInfo("en-US");
        private Match match; 

        /// <summary>
        /// Konstruktor ktory przetwadza dane otrzymane z UART'a
        /// </summary>
        /// <param name="deviceInfo"></param>
        public DeviceIdentity(string deviceInfo)
        {
            match = regX.Match(deviceInfo);

            if (match.Success)           // Jesli informacja dotyczaca urzadzenia jest zgodna ze standardem regeX
            {

                string[] tempInfo = match.Groups[0].Value.Split(Convert.ToChar("."));  // Robimy split 

                this.ATIver = Convert.ToInt32(tempInfo[0][1]);              // Jesli mamy 'v2' jako wartosc bierzemy tylko '2'
                this.ProductionDate = DateTime.ParseExact(String.Format("{0}-{1}", tempInfo[1], tempInfo[2]),dateFormat,us);    // Formatujemy sobie date
                this.ProductIdentifier = tempInfo[3];                       // Bierzemy nasz produkt identefier
                this.HardwareVersion = Convert.ToInt32(tempInfo[4]);        // Sprzawdzamy hardware version
                this.Firmware = new FirmwareVersion()                       // Bierzemy firmware version
                {
                    Major = Convert.ToInt32(tempInfo[5]),
                    Minor = Convert.ToInt32(tempInfo[6])
                };

                this.IsSuccessfullyID = true;
            }
            else
            {
                this.IsSuccessfullyID = false;
            }


            

        }

        /// <summary>
        /// Metoda potwierdzajaca nam czy udalo nam sie przeprowadzic identyfikacje
        /// </summary>
        /// <returns></returns>
        public bool IsIdentiefied()
        {
            return this.IsSuccessfullyID;
        }

    }

    /// <summary>
    /// Klasa uzywana do okreslenia firmware
    /// </summary>
    public class FirmwareVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
    }
}

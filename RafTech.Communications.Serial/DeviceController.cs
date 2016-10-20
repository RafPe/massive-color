using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RafTech.Communications.Serial
{
    public class DeviceController
    {
        public  SerialPort  CommunicationsPort; // Serial Port
        public  String      ComPortName;        // Nazwa Portu COM
        public  int         Baud_rate; 

        public Boolean DeviceFound;             // Flaga czy znalezlismy nasze urzadzenie
        public Boolean DeviceConnected;         // Flaga czy jestesmy polaczeni z urzadzeniem
        public DeviceIdentity DeviceIdentity;

        public event EventHandler OnCOMdisconnected; // Event handler do obslugi zdarzenia odlaczenia urzadzenia z pliku COM

        

        /// <summary>
        /// Domyslny konstruktor
        /// </summary>
        public DeviceController(int Serial_Port_Speed)
        {
            Baud_rate = Serial_Port_Speed;
        }

        /// <summary>
        /// Domyslny konstruktor
        /// </summary>
        public DeviceController(int Serial_Port_Speed,String PortName)
        {
            Baud_rate = Serial_Port_Speed;
            ComPortName = PortName;
        }

        /// <summary>
        /// Metoda do wyszukania urzadzenia podlaczonego do USB
        /// </summary>
        /// <param name="DeviceIdentyfier"> Urzadzenie identyfikowane jest przez ten parametr</param>
        public bool SearchForConnectedDevice(string DeviceIdentyfier)
        {
            string debugos = ";";

            var _Serial_Ports = SerialPort.GetPortNames();

            foreach (var PortName in _Serial_Ports)
            {
                if (DeviceFound == false) // Jesli nie znalezlismy urzadzenia
                {
                    // Tworzymy obiekt nowego portu do komunikacji
                    CommunicationsPort = new SerialPort()
                    {
                        PortName = PortName,
                        BaudRate = this.Baud_rate,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        DataBits = 8,
                        Handshake = Handshake.None
                    };

                    try
                    {
                        Console.WriteLine("Opened " + PortName);
                        //CommunicationsPort.ReadTimeout = 2500;
                        CommunicationsPort.Open();
                        CommunicationsPort.Write("ATI" + Environment.NewLine);

                        Thread.Sleep(1000);
                        debugos = CommunicationsPort.ReadExisting();

                        this.DeviceIdentity = new DeviceIdentity(debugos);

                        if (this.DeviceIdentity.IsIdentiefied() && this.DeviceIdentity.ProductIdentifier == DeviceIdentyfier)
                        {
                            this.DeviceFound = true;
                            // Zmieniamy flage - znalezlismy urzadzenie
                            this.ComPortName = PortName;
                            // Przypisujemy nazwe portu do naszego urzadzenia
                            CommunicationsPort.Close(); // Zamykamy port COM


                        }
                        else
                        {
                            CommunicationsPort.Close(); // Zamykamy port COM    
                        }

                        



                    }
                    catch (Exception)
                    {

                        CommunicationsPort.Close(); // Zamykamy port
                        //return DeviceFound;
                        // Zwracamy nic - bo nie znalezlismy naszego urzadzenia


                    }

                } // END  if (DeviceFound == false) 

                else
                {
                    break;
                }

            }

            return DeviceFound;        // Zwracamy nic - bo nie znalezlismy naszego urzadzenia
        }

        /// <summary>
        /// MEtoda do sprawdzenia czy urzadzenie pod portem COM jest tym ktore szukamy
        /// </summary>
        /// <param name="DeviceIdentyfier"></param>
        /// <returns></returns>
        public bool CheckForConnectedDevice(string DeviceIdentyfier)
        {
            string debugos = "";

            // Tworzymy obiekt nowego portu do komunikacji
            CommunicationsPort = new SerialPort()
            {
                PortName = this.ComPortName,
                BaudRate = this.Baud_rate,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None
            };

            try
            {
                Console.WriteLine("Opened " + ComPortName);
                //CommunicationsPort.ReadTimeout = 2500;
                CommunicationsPort.Open();
                CommunicationsPort.Write("ATI" + Environment.NewLine);

                Thread.Sleep(1000);
                debugos = CommunicationsPort.ReadExisting();

                this.DeviceIdentity = new DeviceIdentity(debugos);

                if (this.DeviceIdentity.IsIdentiefied() && this.DeviceIdentity.ProductIdentifier == DeviceIdentyfier)
                {
                    this.DeviceFound = true;    // Zmieniamy flage , ze znalezlismy urzadzenie
                    CommunicationsPort.Close(); // Zamykamy port COM
                    return DeviceFound;


                }
                else
                {
                    CommunicationsPort.Close(); // Zamykamy port COM    
                    return DeviceFound;
                }

            }
            catch (Exception)
            {

                CommunicationsPort.Close(); // Zamykamy port
                return DeviceFound;
            }
        }

        /// <summary>
        /// Asynchroniczna funkcja do wyszukiwania urzadzenia
        /// </summary>
        /// <param name="DeviceIdentyfier"></param>
        public async Task<bool> AsyncSearchForConnectedDevice(string DeviceIdentyfier)
        {
            bool result = await EnumerateConnectedDeviceAsync(DeviceIdentyfier);        // Wyszukiwanie po ID

            this.DeviceFound = result;                                                  // Przypisanie wyniku

            return DeviceFound;
        }

        public async Task<bool> EnumerateConnectedDeviceAsync(string DeviceIdentyfier)
        {
            bool result;

            result = await Task.Run(() =>
            {
                string debugos = ";";

                foreach (var PortName in SerialPort.GetPortNames())
                {
                    if (DeviceFound == false) // Jesli nie znalezlismy urzadzenia
                    {
                        // Tworzymy obiekt nowego portu do komunikacji
                        CommunicationsPort = new SerialPort()
                        {
                            PortName = PortName,
                            BaudRate = Baud_rate, //TODO zmienna!
                            Parity = Parity.None,
                            StopBits = StopBits.One,
                            DataBits = 8,
                            Handshake = Handshake.None
                        };

                        try
                        {
                            Console.WriteLine("Opened " + PortName);
                            //CommunicationsPort.ReadTimeout = 2500;
                            CommunicationsPort.Open();
                            CommunicationsPort.Write("ATI" + Environment.NewLine);

                            Thread.Sleep(1000);
                            debugos = CommunicationsPort.ReadExisting();

                            this.DeviceIdentity = new DeviceIdentity(debugos);

                            if (this.DeviceIdentity.IsIdentiefied() && this.DeviceIdentity.ProductIdentifier == DeviceIdentyfier)
                            {
                                this.DeviceFound = true;
                                // Zmieniamy flage - znalezlismy urzadzenie
                                this.ComPortName = PortName;
                                // Przypisujemy nazwe portu do naszego urzadzenia
                                CommunicationsPort.Close(); // Zamykamy port COM


                            }

                            CommunicationsPort.Close(); // Zamykamy port COM



                        }
                        catch (Exception)
                        {

                            CommunicationsPort.Close(); // Zamykamy port
                            return DeviceFound;
                            // Zwracamy nic - bo nie znalezlismy naszego urzadzenia


                        }

                    } // END  if (DeviceFound == false) 

                    else

                    {
                        break;
                    }
                }

                return DeviceFound;
            });

            return result;
        }

        /// <summary>
        /// Metoda odpowiedzialna za inicjalizacje polaczenia z naszym urzadzeniem
        /// </summary>
        /// <returns></returns>
        public bool Initialize_Serial_Connection()
        {

            if (DeviceFound)        // Tylko jesli znalezlismy nasze urzadzenie
            {
                CommunicationsPort = new SerialPort()
                {
                    PortName = ComPortName,
                    BaudRate = this.Baud_rate,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    DataBits = 8,
                    Handshake = Handshake.None
                };

                try
                {
                    CommunicationsPort.Open();
                    DeviceConnected = CommunicationsPort.IsOpen;
                    return DeviceConnected;
                }
                catch (Exception)
                {

                    DeviceConnected = CommunicationsPort.IsOpen;
                    return DeviceConnected;
                }
            }

            return DeviceConnected;
        }

        /// <summary>
        /// Metoda odpowiedzialna za zamykanie polaczenia w apliukacji
        /// </summary>
        /// <returns>Czy port jest otwarty</returns>
        public bool Close_Serial_Connection()
        {
            if (CommunicationsPort != null)
            {
                if (CommunicationsPort.IsOpen)
                {
                    CommunicationsPort.Close();
                    CommunicationsPort.Dispose();
                }


                return CommunicationsPort.IsOpen;
            }
            else
            {
                return false;
            }

            
        }

        /// <summary>
        /// Metoda do wyslania danych
        /// </summary>
        /// <param name="strToSend">Wartosc string do wyslania</param>
        /// <param name="appendCR">Czy mamy automatycznie dodac CR</param>
        /// <returns> Zwraca true jesli sie udalo wyslac / false jesli mamy error</returns>
        public bool Send_Data_To_Serial(string strToSend,bool appendCR=true)
        {
            if (DeviceConnected)
            {
                if (appendCR) strToSend += Environment.NewLine; // dodajemy CR na koncu stringa            

                try
                {
                    CommunicationsPort.Write(strToSend);        // Probujemy wyslac nowa linie tekstu

                    return true;                                // zwracamy true;
                }
                catch (Exception)                               // Jesli zlapalismy wyjatek
                {
                    DeviceConnected = false;                    // Zmieniamy flage device connected
                    if (OnCOMdisconnected != null) OnCOMdisconnected(this, null);   // Raise event disconnected COM
                    return DeviceConnected;                     // Zwracamy wartosc deviceconnected (czyli bedzie false)
                }
                
            }

            return false;                                       // domyslnie zwrocimy false
        }

        /// <summary>
        /// Metoda do wyslania danych
        /// </summary>
        /// <param name="arrToSend">array byte do wyslania</param>        
        /// <returns>Zwraca true jesli sie udalo wyslac</returns>
        public bool Send_Data_To_Serial(byte[] arrToSend)
        {
            if (DeviceConnected)
            {                        
                try
                {
                    CommunicationsPort.Write(arrToSend, 0, arrToSend.Length);        // Probujemy wyslac nowa linie tekstu

                    return true;                                // zwracamy true;
                }
                catch (Exception)                               // Jesli zlapalismy wyjatek
                {
                    DeviceConnected = false;                    // Zmieniamy flage device connected
                    if (OnCOMdisconnected != null) OnCOMdisconnected(this, null);   // Raise event disconnected COM
                    return DeviceConnected;                     // Zwracamy wartosc deviceconnected (czyli bedzie false)
                }
            }

            return false;                                       // domyslnie zwrocimy false
        }


    }
}

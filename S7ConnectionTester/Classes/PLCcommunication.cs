//S7.NET Adnotation: Copyright(c) 2009 Jürgen Schildmann

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.




using Advantech.Adam;
using S7.Net;
using System;
using System.Windows;

namespace S7ConnectionTester
{
    public class ModuleCommunication
    {
        public Plc Pelecek;
        public bool Connected;
        public AdamSocket Adamek;
        public string ModuleName = "";
        public string CommunicationBit = "M100.7";
        public string ModuleIP;

        ///////////////////     Nawiązywanie Połączenia z PLC   //////////////////////////////////////
        public string ConnectToPLC(string IP, string TypCPU, short rack = 0, short slot = 0, string CommunicationBit = "M100.7")
        {
            ModuleIP = IP;
            CpuType TypeofCPU = new CpuType();
            this.CommunicationBit = CommunicationBit;

            switch (TypCPU)
            {
                case "S7200":
                    TypeofCPU = CpuType.S7200;
                    ModuleName = "S7200";
                    Pelecek = new Plc(TypeofCPU, IP, rack, slot);
                    break;
                case "S7300":
                    TypeofCPU = CpuType.S7300;
                    ModuleName = "S7300";
                    Pelecek = new Plc(TypeofCPU, IP, rack, slot);
                    break;
                case "S7400":
                    TypeofCPU = CpuType.S7400;
                    ModuleName = "S7400";
                    Pelecek = new Plc(TypeofCPU, IP, rack, slot);
                    break;
                case "S71200":
                    TypeofCPU = CpuType.S71200;
                    ModuleName = "S71200";
                    Pelecek = new Plc(TypeofCPU, IP, rack, slot);
                    break;
                case "S71500":
                    TypeofCPU = CpuType.S71500;
                    ModuleName = "S71500";
                    Pelecek = new Plc(TypeofCPU, IP, rack, slot);
                    break;

                case "Adam":
                    Adamek = new AdamSocket();
                    ModuleName = "Advantech Adam";
                    break;

                default:

                    return "Wybrano nieobsługiwany typ procesora";
            }


            #region Connection Attempt
            try
            {

                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();        // pingowanie wskazanego adresu IP

                System.Net.NetworkInformation.PingReply Reply = ping.Send(IP, 20);                             // sprawdzanie odpowiedzi pingu


                if (Reply != null)
                {
                    switch (Reply.Status)
                    {
                        case System.Net.NetworkInformation.IPStatus.Success:                               // Jeżeli odpowiedź jest pozytywna przechodzimy do próby połączenia z PLC


                            if (Pelecek != null && Adamek == null)
                            {
                                #region Łączenie z PLC
                                Pelecek = new Plc(TypeofCPU, IP, rack, slot);
                                ErrorCode Error = Pelecek.Open();                                            // Łączenie się z adresem IP. Jeżeli pod wskazanym adresem nie ma PLC, wówczas błąd jest zapisywany w zmiennej Error
                                bool temp = Convert.ToBoolean(Pelecek.Read(CommunicationBit));
                                temp = !temp;
                                int tempint = Convert.ToInt16(temp);                                     // Próba odczytania zmiennej z Memory 100.7, zmiana jej na przeciwną, zapisanie jej w tym samym miejscu
                                Pelecek.Write(CommunicationBit, tempint);                                            // Potem następuje jej ponowny odczyt i sprawdzenie poprawności danych odczytanych z wcześniej wysłanymi
                                temp = Convert.ToBoolean(Pelecek.Read(CommunicationBit));
                                if (Convert.ToInt16(temp) == tempint)                                    // Jeżeli próba została zakończona pomyślnie, wówczas program gratuluje nam połączenia i wpisuje pierwotny stan Memory 100.7 do PLC
                                {

                                    tempint = Convert.ToInt16(!temp);
                                    Pelecek.Write(CommunicationBit, tempint);
                                    Connected = true;
                                    ModuleName = TypCPU;
                                    return "Gratulacje! Udało połączyć się ze sterownikiem PLC.";

                                }
                                else
                                {

                                    MessageBox.Show("Uwaga! Nie udało połączyć się ze sterownikiem! \n Wystąpił błąd:" + Error.ToString());
                                    return "Uwaga! Nie udało połączyć się ze sterownikiem! \n Wystąpił błąd:" + Error.ToString();

                                }

                                #endregion
                            }
                            else
                            {
                                if (Pelecek == null && Adamek != null)
                                {
                                    #region Łączenie z Adamem
                                    Adamek.Connect(IP, System.Net.Sockets.ProtocolType.Tcp, 502);

                                    if (Adamek.Connected)
                                    {
                                        ModuleName = "Advantech Adam";
                                        Connected = true;

                                        return "Udało się nawiązać połączenie z Advantech Adam";
                                    }
                                    return "Nie udało się połączyć z Adamem";

                                    #endregion
                                }
                                else
                                    return "Nie udało się połączyć z niedostępnym typem urządzenia";
                            }

                        case System.Net.NetworkInformation.IPStatus.TimedOut:                          // Błędy występujące przy pingowaniu : przekroczono limit czasu oraz nieodnaleziono IP
                            return "Przekroczono czas połączenia.";

                        default:
                            return "Błąd połączenia";
                    }
                }
                else
                    return "Adres IP nie odpowiada";
            }
            catch
            {
                return "Adres IP nie odpowiada";
            }

            #endregion

        }            /// Zwraca komunikat błędu i łączy się ze sterownikiem

        /*      public void ConnectToPLC(CpuType TypeofCPU, string IP)
              {
                  try
                  {

                      System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();        // pingowanie wskazanego adresu IP

                      System.Net.NetworkInformation.PingReply Reply = ping.Send(IP);                             // sprawdzanie odpowiedzi pingu


                      if (Reply != null)
                      {
                          //switch (Reply.Status)
                          //{
                          //    case System.Net.NetworkInformation.IPStatus.Success:                               // Jeżeli odpowiedź jest pozytywna przechodzimy do próby połączenia z PLC

                                  Pelecek = new Plc(TypeofCPU, IP, 0, 0);
                                  ErrorCode Error = Pelecek.Open();                                            // Łączenie się z adresem IP. Jeżeli pod wskazanym adresem nie ma PLC, wówczas błąd jest zapisywany w zmiennej Error
                                  bool temp = Convert.ToBoolean(Pelecek.Read("M100.7"));
                                  temp = !temp;
                                  int tempint = Convert.ToInt16(temp);                                     // Próba odczytania zmiennej z Memory 100.7, zmiana jej na przeciwną, zapisanie jej w tym samym miejscu
                                  Pelecek.Write("M100.7", tempint);                                            // Potem następuje jej ponowny odczyt i sprawdzenie poprawności danych odczytanych z wcześniej wysłanymi
                                  temp = Convert.ToBoolean(Pelecek.Read("M100.7"));
                                  if (Convert.ToInt16(temp) == tempint)                                    // Jeżeli próba została zakończona pomyślnie, wówczas program gratuluje nam połączenia i wpisuje pierwotny stan Memory 100.7 do PLC
                                  {

                                      tempint = Convert.ToInt16(!temp);
                                      Pelecek.Write("M100.7", tempint);
                                      Connected = true;
                                    //  return "Gratulacje! Udało połączyć się ze sterownikiem PLC.";

                                  }
                             //     else
                                  //    return "Uwaga! Nie udało połączyć się ze sterownikiem! \n Wystąpił błąd:" + Error.ToString();



                         //     case System.Net.NetworkInformation.IPStatus.TimedOut:                          // Błędy występujące przy pingowaniu : przekroczono limit czasu oraz nieodnaleziono IP
                                //  return "Przekroczono czas połączenia.";

                        //      default:
                               //   return "Błąd połączenia";
                       //   }
                      }
               //       else
                       //   return "Adres IP nie odpowiada";
                  }
                  catch
                  {
                   //   return "Adres IP nie odpowiada";
                  }



              }
             */

        public void ClosePLC()
        {
            if (Pelecek != null)
                Pelecek.Close();

            if (Adamek != null)
                Adamek.Disconnect();

        }

        public object ReadModule(string Variable)
        {
            object Value = "ReadError";

            if (string.IsNullOrEmpty(Variable))
            {
                MessageBox.Show("Wysłano do odczytu pustą zmienną! (Workbit, bity do odczytu, interpreter)", "Błąd odczytu!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }

            if (Pelecek != null)
            {
                Variable = Variable.ToUpper();
                Variable = Variable.Replace("Q", "A");
                Variable = Variable.Replace("I", "E");
                Value = Pelecek.Read(Variable);
            }
            if (Adamek != null)
            {
                Variable = Variable.ToUpper();
                if (Variable.Contains("I") || Variable.Contains("E"))
                {
                    Variable = Variable.Remove(0, 1);
                    string[] invocation = Variable.Split(new char[] { '.' });

                    int index;
                    bool[] data;
                    if (int.TryParse(invocation[0], out index) && Adamek.Modbus().ReadCoilStatus(index + 1, 1, out data))
                    {
                        Value = data[0];
                    }
                }
            }

            return Value;
        }

        public bool WriteToPLC(string Variable, object Value)
        {
            bool SendCorrect = false;
            Variable = Variable.ToUpper();
            Variable = Variable.Replace("Q", "A");
            Variable = Variable.Replace("I", "E");


            if (Value.ToString().ToUpper() == "TRUE")
                Value = 1;

            if (Value.ToString().ToUpper() == "FALSE")
                Value = 0;

            int temp;
            if (int.TryParse(Value.ToString(), out temp) && Pelecek != null)
            {
                Value = temp;

                Pelecek.Write(Variable, Value);


                if (Convert.ToInt32(Pelecek.Read(Variable)) == Convert.ToInt32(Value))
                    SendCorrect = true;
            }
            return SendCorrect;
        }

        public bool WriteToPLC(string Variable, string Value)
        {
            int temp = new int();
            bool SendedCorrect = false;

            Variable = Variable.ToUpper();
            Variable = Variable.Replace("Q", "A");
            Variable = Variable.Replace("I", "E");

            Value = Value.ToUpper();



            switch (Value)
            {
                case "TRUE":
                    Value = "1";
                    break;
                case "FALSE":
                    Value = "0";
                    break;
            }

            if (int.TryParse(Value, out temp))
            {
                Pelecek.Write(Variable, temp);

                if (Convert.ToInt16(Pelecek.Read(Variable)) == temp)
                    SendedCorrect = true;
            }

            return SendedCorrect;

        }

    }
}

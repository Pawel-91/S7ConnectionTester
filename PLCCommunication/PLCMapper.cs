using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S7.Net;
using System.Net;
using PerfSoftCommunication.Enums;
using System.Timers;

namespace PerfSoftCommunication.Classes
{


    /// <summary>
    /// Klasa dokonująca mapowania zmiennych w sterowniku PLC - pobiera dane ze zmiennych zawartych w alarmach dla danej maszyny.
    /// </summary>
    abstract class PLCMapper : Module
    {
        /// <summary>
        /// Obiekt komunikujący się ze sterownikiem PLC, używa biblioteki S7.NET  -- UWAGA! Nagłe wyłączenie aplikacji w trakce komunikacji może zawiesić komputer (bluescreen)
        /// </summary>
        private Plc plc;
        /// <summary>
        /// timer taktujący odświeżanie danych w słowniku, który jest potem dostępny dla klas potomnych
        /// </summary>
        private Timer timer = new Timer();
        /// <summary>
        /// Lista zmiennych do odczytania przy każdym odświeżaniu rejestru stanów pamięci
        /// </summary>
        private List<string> variables;
        /// <summary>
        /// Zmienne zmapowane do postaci słownika. Stąd są pobierane wartości stanów pamięci sterownika przez klasy potomne. 
        /// </summary>
        protected Dictionary<string, bool> mappedVariables;

        private bool establishingConnection;

        /// <summary>
        /// Konstruktor klasy PLCMapper z ręcznym przekazaniem wartości parametrów. Pobiera zmienne potrzebne do rozpoczęcia komunikacji, inicjuje mapę zmiennych, rozpoczyna działanie timera.
        /// </summary>
        /// <param name="moduleID">ID modułu</param>
        /// <param name="moduleName">nazwa modułu</param>
        /// <param name="ipAddress">adres IP sterownika</param>
        /// <param name="communicationBit">bit komunikacyjny</param>
        /// <param name="moduleType">typ modułu</param>
        /// <param name="plcType">typ plc</param>
        /// <param name="slot">gniazdo</param>
        /// <param name="rack">szyna</param>
        public PLCMapper(int moduleID, string moduleName, IPAddress ipAddress, string communicationBit, ModuleTypeEnum moduleType, CpuType plcType, int slot, int rack) : base(moduleID, moduleName, ipAddress, communicationBit, moduleType)
        {
            CpuType cpuType = plcType;
            unchecked                                                                          //żeby nie wyrzucało błędu przepełnienia przy rzutowaniu int na short
            {
                short sl = (short)slot;
                short rk = (short)rack;

                this.plc = new Plc(cpuType, this.IPAddress.ToString(), rk, sl);                //inicjowanie obiektu typu S7.Net.PLC  - potrzebny do komunikacji
            }
            this.initializeMap();                                                              //inicjowanie mapy zmiennych (musi być wykonana po utworzeniu obiektu dla zmiennej plc
            this.startTimer();                                                                 //uruchamianie timera odświeżającego zmapowane zmienne
        }

        /// <summary>
        /// Konstruktor klasy PLCMapper z pobraniem danych z obiektu typu ComponentsTable (pobieranego przez entity framework z SQLa. Pobiera zmienne potrzebne do rozpoczęcia komunikacji, inicjuje mapę zmiennych, rozpoczyna działanie timera.
        /// </summary>
        /// <param name="module">Obiekt typu ComponentsTable przechowujący dane na temat modułu ze sterownika PLC</param>
        public PLCMapper(SQLConnection.ComponentsTable module) : base(module)
        {
            establishingConnection = false;
            CpuType cpuType = ModuleType.ToS7CpuType();
            unchecked                                                                          //żeby nie wyrzucało błędu przepełnienia przy rzutowaniu int na short
            {
                short sl = Convert.ToInt16(module.Slot);
                short rk = Convert.ToInt16(module.Rack);

                plc = new Plc(cpuType, this.IPAddress.ToString(), rk, sl);                     //inicjowanie obiektu typu S7.Net.PLC  - potrzebny do komunikacji
            }
            this.initializeMap();                                                              //inicjowanie mapy zmiennych (musi być wykonana po utworzeniu obiektu dla zmiennej plc
            this.startTimer();                                                                 //uruchamianie timera odświeżającego zmapowane zmienne

        }

        /// <summary>
        /// Dodanie nowej zmiennej do mapowania
        /// </summary>
        /// <param name="variable">Nazwa zmiennej</param>
        /// <param name="mapVariables">Definiuje, czy po dodaniu nowej zmiennej ma odbyć się mapowanie (odświeżenie danych w słowniku)</param>
        protected void AddVariable(string variable, bool mapVariables = false)
        {
            if (string.IsNullOrWhiteSpace(variable) || string.IsNullOrEmpty(variable))
            {
                return;
            }

            string[] STLLines = variable.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < STLLines.Length; i++)
            {
                try
                {
                    if (!STLLines[i].Contains(' '))
                        continue;

                    GlobalClass.WriteCommentary($"{STLLines.Length} : {i}   linijka STL: {STLLines[i]}");


                    string[] line = STLLines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int indeks;
                    if (line.Length < 2 && (line[0][0] == 'M' || line[0][0] == 'm' || line[0][0] == 'I' || line[0][0] == 'i' || line[0][0] == 'Q' || line[0][0] == 'q'))
                    {
                        indeks = 0;
                    }
                    else if (line.Length >= 2)
                    {
                        indeks = 1;
                    }
                    else
                    {
                        continue;
                    }

                    line[indeks] = line[indeks].Replace('(', '\0');
                    line[indeks] = line[indeks].Replace(')', '\0');
                    if (mappedVariables.ContainsKey(line[indeks]))
                        continue;

                    string[] variables = line[indeks].getVariables();


                    this.variables.Add(variables[0]);
                    for (int j = 1; j < variables.Length; j++)
                    {
                        if (!this.mappedVariables.ContainsKey(variables[j]))
                            this.mappedVariables.Add(variables[j], false);
                        GlobalClass.WriteCommentary($"Dodawanie zmiennej: {variables[j]}");

                    }
                    GlobalClass.WriteCommentary($"Uzupełniono słownik dla zmiennej{variables[0]}", true);

                }
                catch (IndexOutOfRangeException iOORExc)
                {
                    GlobalClass.WriteCommentary($"WYJĄTEK: Indeks spoza zakresu: {iOORExc.Message}");
                }
                catch (InvalidCastException iCExc)
                {
                    GlobalClass.WriteCommentary($"WYJĄTEK: nieprawidłowe rzutowanie: {iCExc.Message}");
                }
                catch (InvalidOperationException iOExc)
                {
                    GlobalClass.WriteCommentary($"WYJĄTEK: nieprawidłowa operacja: {iOExc.Message}");
                }
                catch (Exception Exc)
                {
                    GlobalClass.WriteCommentary($"WYJĄTEK: ogólny: {Exc.Message}");
                }

            }


            if (mapVariables)
            {
                this.mapVariables();
            }
        }
        /// <summary>
        /// Otwiera połączenie ze sterownikiem PLC.
        /// </summary>
        /// <returns>Zwraca prawdę, jeśli udaje się nawiązać połączenie bez problemu.</returns>
        public override bool OpenConnection()
        {
            if (plc == null)
            {
                throw new InvalidOperationException("PLC nie został zainicjalizowany!");
            }
            ErrorCode error = plc.LastErrorCode;
            int i = 0;
            establishingConnection = true;
            timer.Stop();
            lock (plc)
            {
                do
                {
                    plc.ClearLastError();                                                           //Czyszczenie bufora z ostatniego błędu połączenia
                    if (plc.IsConnected)                                                            //Jeśli połączenie jest już otwarte, wówczas następuje jego zamknięcie i ponowne otwarcie
                    {
                        plc.Close();
                    }

                    error = plc.Open();
                    if (error != ErrorCode.NoError)
                    {
                        System.Threading.Thread.Sleep(20);
                        GlobalClass.WriteCommentary($"Próba połączenia numer: {i}");
                    }
                    i++;
                } while (error != ErrorCode.NoError && i < 50);
            }

            establishingConnection = false;
            timer.Start();

            return error == ErrorCode.NoError;                                              //Jeśli kod błędu wynosi o (NoError), wówczas zwracana jest PRAWDA
        }

        /// <summary>
        /// Sprawdza, czy urządzenie jest podłączone. Otwiera połączenie, jeśli jest zamknięte.
        /// </summary>
        /// <returns>Zwraca prawdę, jeśli jest połączenie ze sterownikiem</returns>
        public override bool CheckConnection()
        {
            bool result;
            if (!(base.CheckConnection() && plc.IsConnected))
            {
                result = this.OpenConnection();
            }
            else
            {
                result = true;
            }

            return result;
        }
        /// <summary>
        /// Wpis danych do sterownika. 
        /// </summary>
        /// <param name="variable">Nazwa zmiennej która zostanie wpisana</param>
        /// <param name="value">Wartość wpisywana do sterownika</param>
        /// <returns>Zawraca prawdę, jeśli sterownik potwierdzi wpisanie wartości</returns>
        public bool WriteToModule(string variable, object value)
        {
            if (plc.IsConnected == false)
            {
                throw new InvalidOperationException("Próba odczytu z niepodłączonego sterownika PLC!");
            }

            ErrorCode error = plc.Write(variable, value);
            return error == ErrorCode.NoError;

        }
        /// <summary>
        /// Inicjalizacja mapy zmiennych. Tworzy słownik zmiennych na podstawie listy alarmów maszyny dla której instancja klasy jest tworzona
        /// </summary>
        private void initializeMap()
        {

            GlobalClass.WriteCommentary("Inicjalizacja mapy zmiennych..", true);

            if (mappedVariables == null)                                                                // leniwe tworzenie słownika, a jeśli w innej części kodu słownik został usunięty - ponowne utworzenie
            {
                mappedVariables = new Dictionary<string, bool>();
            }
            if (variables == null)                                                                      // leniwe tworzenie listy zmiennych do odpytywania
            {
                variables = new List<string>();
            }

            using (SQLConnection.PLCDiagnosticsEntities entity = new SQLConnection.PLCDiagnosticsEntities())   // pobranie listy alarmów dla danej maszyny
            {
                var alarms = entity.AlarmTable.Where(a => a.STLtext != null && a.Enabled == true && a.MachinesTable.MachineComponentsTable.FirstOrDefault(b => b.IDComponent == ID).IDMachine == a.IDMachine).Select(c => c.STLtext).ToArray<string>();

                foreach (var alarm in alarms.Where(a => a != string.Empty))
                {
                    this.AddVariable(alarm);                                                                // dodanie zmiennej dla każdej pozycji w liście alarmów
                }

                GlobalClass.WriteCommentary($"Dodawanie zmiennych zakończono");
            }
            this.mapVariables();                                                                        // mapowanie zmiennych
            GlobalClass.WriteCommentary("Inicjalizacja zakończona.");
        }

        /// <summary>
        /// Mapowanie zmiennych. Zostają one odczytane ze sterownika przez odpytywanie Double Wordów, przetransponowane na kolejne wartości bitów w pamięci i wpisane w słowniku w postaci: Nazwa zmiennej -> wartość
        /// </summary>
        private void mapVariables()
        {
            GlobalClass.WriteCommentary("Mapowanie zmiennych..", true);

            lock (this.variables)
            {
                string[] varis = new string[this.variables.Count];
                this.variables.CopyTo(varis);

                foreach (string variable in varis)                                                                  // przeglądanie zmiennych zawartych w liście
                {
                    uint doubleWord = this.readVariable(variable);                                                  // odczyt ze sterownika zmiennej typu DoubleWord
                    string[] v = variable.getVariables();                                                           // translacja nazwy pojedynczej zmiennej na zestaw przechowujący nazwy poszczególnych bitów

                    GlobalClass.WriteCommentary($"Zmienna: {variable}, wartość: {doubleWord}");

                    var data = BitConverter.GetBytes(doubleWord);                                                   // Przetworzenie odczytanego DoubleWorda na poszczególne bajty

                    bool[][] bits = new bool[data.Length][];                                                        // tablic tablic bitów w poszczególnych bajtach

                    for (int i = 0; i < data.Length; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            this.mappedVariables[v[v.Length - (i * 8 + j) - 1]] = data[i].SelectBit(j);             // wpisywanie wartości poszczególnych bitów do odpowiednich miejsc w słowniku
                        }
                    }
                    data = null;
                    bits = null;
                    v = null;
                }
                varis = null;
            }
            GlobalClass.WriteCommentary("Mapowanie zakończone");
        }
        /// <summary>
        /// Metoda odczytująca zmienną bezpośrednio ze sterownika i zwracająca w postaci unsinged int
        /// </summary>
        /// <param name="variable">Nazwa zmiennej do odczytania</param>
        /// <returns>Zwraca wartość odczytaną ze sterownika</returns>
        protected uint readVariable(string variable)
        {
            object value;
            lock (plc)
            {
                if (plc.IsConnected == false)
                {
                    this.OpenConnection(); // throw new InvalidOperationException("Próba odczytu z niepodłączonego sterownika PLC!");
                }
                variable = variable.ToUpper().Replace('Q', 'A').Replace('I', 'E');                              // W bibliotece S7.Net jest używana notacja niemiecka dla nazw wyjść i wejść
                value = plc.Read(variable);


                int i = 0;
                while (value.ToString() == "WrongVarFormat" && i < 10)
                {
                    plc.Close();
                    plc.ClearLastError();
                    plc.Open();
                    value = plc.Read(variable);
                    i++;
                }

                if(value.ToString() == "WrongVarFormat")
                {
                    value = 0;
                }

            }

            return Convert.ToUInt32(value);
        }

        /// <summary>
        /// Rozpoczęcie działania timera taktującego odświeżanie zmiennych w słowniku
        /// </summary>
        public void startTimer()
        {
            var interval = GlobalClass.ThisComputer.RefreshTime * 5;
            timer.Interval = interval;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (establishingConnection)
            {
                timer.Stop();

                return;
            }

            timer.Stop();
            this.mapVariables();
            lock (plc)
            {
                short rack = plc.Rack, slot = plc.Slot;
                string ip = plc.IP;
                CpuType cpu = plc.CPU;
                plc.Dispose();
                plc = new Plc(cpu, ip, rack, slot);
                this.OpenConnection();
            }
            timer.Start();
        }

    }




    static class PLCExtensions
    {
        /// <summary>
        /// Generuje tablicę zmiennych na miejscu [0] - nazwa zmiennej typu Double Word (ta powinna być bezpośrednio odczytana), a na pozostałych nazwy wszystkich bitów wchodzących w skład zmiennej typu dw
        /// </summary>
        /// <param name="variable">Nazwa zmiennej, która zostanie sparsowana do odpowiedniej zmiennej dw oraz jej bitów</param>
        /// <returns>Tablica nazw zmiennych</returns>
        public static string[] getVariables(this string variable)
        {
            GlobalClass.WriteCommentary($"Variable: {variable}");
            variable = variable.ToUpper();
            //            string address = cutAddress(ref variable);
            string address = "";
            cutAddress(ref variable);
            GlobalClass.WriteCommentary($"Variable parsed: {variable}");


            string[] returnVariables = new string[33];
            if (variable[0] == 'D')
            {
                string[] s = variable.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                s[1] = s[1].Replace("DBD", "");
                s[1] = s[1].Replace("DBB", "");
                s[1] = s[1].Replace("DBW", "");
                s[1] = s[1].Replace("DBX", "");

                // tutaj liczymy najbliższą wartość zmiennej, która jest podzielna przez 4 (jako, że w 4 bajtach jest double word)
                int memoryInt = int.Parse(s[1]);
                memoryInt = memoryInt - memoryInt % 4;

                returnVariables[0] = $"{address}{s[0]}.DBD{memoryInt}";

                for (int i = 1; i < returnVariables.Length; i++)
                {
                    // iteracja będzie po 32 pozostałych elementach tablicy - dlatego najpierw liczymy numer bajtu pamięci - memoryInt + (i-1)/8
                    // i-1   - korekcja liczenia po pominięciu zerowego miejsca w tabeli - zarezerwowanej dla nazwy z double word
                    // dzielimy przez 8, bo zapisany zostanie każdy bit, więc służy to zwiększania numeru bajtu po 8 bitach (jako, że int, to część dzisiętna ucinana) 
                    // 7 - (i-1) % 8   -  selekcja konkretnych bitów, zapis odwrotny (od bitu 7 do 0) ze względu na budowę ramki w PLCku
                    returnVariables[i] = $"{address}{s[0]}.DBX{memoryInt + (i - 1) / 8}.{7 - (i - 1) % 8}";
                }
            }
            else
            {
                if (variable.Contains('.'))
                {
                    variable = variable.Remove(variable.IndexOf('.'));
                }
                char type = variable[0];
                variable = variable.Remove(0, 1);

                variable = variable.Replace("B", "");
                variable = variable.Replace("W", "");
                variable = variable.Replace("D", "");

                // tutaj liczymy najbliższą wartość zmiennej, która jest podzielna przez 4 (jako, że w 4 bajtach jest double word)
                int memoryInt = int.Parse(variable);
                memoryInt = memoryInt - memoryInt % 4;

                returnVariables[0] = $"{address}{type}D{memoryInt}";

                for (int i = 1; i < returnVariables.Length; i++)
                {
                    // iteracja będzie po 32 pozostałych elementach tablicy - dlatego najpierw liczymy numer bajtu pamięci - memoryInt + (i-1)/8
                    // i-1   - korekcja liczenia po pominięciu zerowego miejsca w tabeli - zarezerwowanej dla nazwy z double word
                    // dzielimy przez 8, bo zapisany zostanie każdy bit, więc służy to zwiększania numeru bajtu po 8 bitach (jako, że int, to część dzisiętna ucinana) 
                    // 7 - (i-1) % 8   -  selekcja konkretnych bitów, zapis odwrotny (od bitu 7 do 0) ze względu na budowę ramki w PLCku
                    // - potem wystarczy tylko w forze pointerować po rozłożonym na bity double wordzie pobranym ze sterownika
                    returnVariables[i] = $"{address}{type}{memoryInt + (i - 1) / 8}.{7 - (i - 1) % 8}";
                }

            }

            return returnVariables;
        }


        private static string cutAddress(ref string input)
        {
            if (!input.Contains('&'))
            {
                return "";
            }

            string address = "&";
          //  input = input.Replace("&", "");
            int i = 1;

            while(char.IsDigit(input[i]))
            {
                address += input[i];
                i++;
            }

            input = input.Remove(0, i);

            return address;
        }


    }
}

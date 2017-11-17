using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfSoftCommunication.Classes
{
    class Interpreter
    {

        private object RLO;

        public bool error = false;                                                         // Ustawiany na true, gdy pojawi się operacja spoza listy dostępnych
        TimeData Timedata = new TimeData(true);
        BracesData Brackets = new BracesData(true);
        private bool SetExist = false;
        Dictionary<string, object> varValues = new Dictionary<string, object>();
        private List<int> SetValues = new List<int>();
  //      string[] memoryContent;

        string command;
        public string Command
        {
            get
            {
                return command;
            }

            set
            {
                command = value;
            }
        }

        struct BracesData
        {
            public List<Int32> temp;                                     // Lista przechowująca wartości po wywołaniu nawiasu
            public List<string> tempstring;                              // Lista przechowująca nazwy operacji po wywołaniu nawiasu
            public int SocketCounter;                                    // Licznik zagnieżdżednia

            public BracesData(bool nothing)                               // konstruktor (bool nothing służy jako wymagany argument konstruktora
            {
                temp = new List<Int32>();
                tempstring = new List<string>();
                SocketCounter = 0;
            }

        }                                                              // Struktura przechowująca listy z zagnieżdżonymi danymi

        struct TimeData
        {
            public List<Int32> counter;                                                  // Licznik taktów zegara
            public List<object> OldRLO;                                                  // Przechowanie poprzedniej wartości RLO
            public List<Int32> NumberState;                                              // Numer porządkowy przypisany do funkcji
            public List<string> Timer;                                                   // Nazwa Timera (dla wykrywania zboczy to Pslope i Nslope)
            public int Number;                                                           // Numer porządkowy

            public TimeData(bool nothing)
            {
                counter = new List<int>();
                OldRLO = new List<object>();
                NumberState = new List<int>();
                Number = new int();
                Timer = new List<string>();
            }

        }                                                                // Strukutra przechoująca listy z danymi funkcji czasowych (timerów i wykrywania zboczy)

        public void Interpret(string command, List<Module> PLCList)
        {
            error = false;

            if (string.IsNullOrEmpty(command) || command.TrimEnd(' ') == string.Empty)
                return;

            command = command.Replace("\r\n", "\n");                                        // usuwanie znaków "enter"


            string[] commands = command.Split(new char[] { '\n' });                         // dzielenie kodu na poszczególne linie

            bool FirstLine = true;                                                          // pierwsza linia
            SetExist = false;

            //if (System.IO.File.Exists(@"C:\Users\Public\Documents\InternalMemory.txt"))     // Pobieranie wewnętrznej pamięci
            //    memoryContent = System.IO.File.ReadAllLines(@"C:\Users\Public\Documents\InternalMemory.txt");
            //else
            //    memoryContent = new string[] { "" };

            //string[] memoryContentCopy = new string[memoryContent.Count()];
            //memoryContent.CopyTo(memoryContentCopy, 0);

            foreach (string s in commands)
            {
                if (SetExist)
                    break;
               
                string[] line = s.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);                               // dzielenie linii na poszczególne części zwierajace operację lub zmienną 

                if (line.Length == 0)
                    return;

                line[0] = line[0].ToUpper();
                object State = new object();                                               // zmienna przechowująca stan wejściowy dla funkcji (odczytywany z PLC lub listy)


                if (FirstLine)                                                             // Ustawienie początkowego stanu pamięci RLO, aby razem ze stanem pierwszej zmiennej tworzyło właściwą STLowi tablicę przejść
                {
                    if (line[0].Contains("A"))
                        RLO = 1;
                    if (line[0].Contains("O"))
                        RLO = 0;
                    if (line[0].Contains("X"))
                        RLO = 0;
                }

                switch (line.Length)
                {
                    case 1:                                                                  // jeżeli wiersz ma 1 kolumnę
                        line[0] = line[0].ToUpper();

                        State = bracketHandler(ref line[0], PLCList);

                        //     function(line[0], true);
                        FirstLine = false;
                        break;

                    case 2:                                                                  // jeżeli wiersz ma 2 kolumny

                        //if (line[0] == "=")
                        //{
                        //    Writer(line[1], PLCList, RLO);
                        //    FirstLine = true;
                        //    break;
                        //}
                        // obsługa nawiasów
                        #region Brackets  \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
                        State = bracketHandler(ref line[1], ref line[0], PLCList);

                        #endregion

                        // Timer   \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

                        if (line[1].Length > 2 && line[1].Remove(2) == "TM")
                            State = TIMER(line[1]);

                        //  Comparator \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

                        if (line[1].Contains("==") || line[1].Contains(">=") || line[1].Contains("<=") || line[1].Contains(">") || line[1].Contains("<") || line[1].Contains("><"))    // komparator
                            State = Comparator(line[1], PLCList);

                        //#region InterpreterMemory \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
                        //if (PLCList.Count > 0 && (line[0] == "S" || line[0] == "R") && line[1].Contains("INTM"))
                        //{
                        //    if (Convert.ToInt16(RLO) > 0)
                        //    {
                        //        object value = line[0] == "S" ? RLO : 0;

                        //        var query = this.memoryContent.Select((t, i) => new { t, i }).Where(a => a.t.Contains(PLCList[0].ModuleName + "\t" + PLCList[0].IPAddress + "\t" + PLCList[0].ID + "\t" + line[1] + "\t"));
                        //        if (query.Count() == 0)
                        //            System.IO.File.AppendAllText(@"C:\Users\Public\Documents\InternalMemory.txt", PLCList[0].ModuleName + "\t" + PLCList[0].IPAddress + "\t" + PLCList[0].ID + "\t" + line[1] + "\t" + value.ToString() + "\r\t");
                        //        else
                        //        {
                        //            memoryContentCopy[query.Select(a => a.i).FirstOrDefault()] = PLCList[0].ModuleName + "\t" + PLCList[0].IPAddress + "\t" + PLCList[0].ID + "\t" + line[1] + "\t" + value.ToString();
                        //            System.IO.File.WriteAllLines(@"C:\Users\Public\Documents\InternalMemory.txt", memoryContentCopy);
                        //        }
                        //    }
                        //    FirstLine = true;
                        //    break;
                        //}
                        //#endregion

                        function(line[0], State, true);
                        FirstLine = false;
                        break;


                    case 3:

                        line[2] = line[2].ToUpper();

                        #region Brackets  \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
                        State = bracketHandler(ref line[1], ref line[0], PLCList, line[2]);

                        #endregion


                        if (line[1].Contains("==") || line[1].Contains(">=") || line[1].Contains("<=") || line[1].Contains(">") || line[1].Contains("<") || line[1].Contains("><"))    // komparator
                            State = Comparator(line[1], PLCList);



                        function(line[0], State, line[2], true);
                        FirstLine = false;
                        break;
                }                                                             // do switcha

            }                                                               // do foreacha

            Brackets.temp.Clear();
            Brackets.tempstring.Clear();
            Timedata.Number = 0;

        }

        public void Interpret(string command, Module PLCList)
        {
            error = false;

            command = command.Replace("\r\n", "\n");                                        // usuwanie znaków "enter"

            string[] commands = command.Split(new char[] { '\n' });                         // dzielenie kodu na poszczególne linie

            bool FirstLine = true;                                                          // pierwsza linia
            SetExist = false;

            //if (System.IO.File.Exists(@"C:\Users\Public\Documents\InternalMemory.txt"))
            //    memoryContent = System.IO.File.ReadAllLines(@"C:\Users\Public\Documents\InternalMemory.txt");
            //else
            //    memoryContent = new string[] { "" };
            //string[] memoryContentCopy = new string[memoryContent.Count()];
            //memoryContent.CopyTo(memoryContentCopy, 0);

            foreach (string s in commands)
            {
                if (SetExist)
                    break;

                string[] line = s.Split(new char[] { ' ' });                               // dzielenie linii na poszczególne części zwierajace operację lub zmienną 

                line[0] = line[0].ToUpper();
                object State = new object();                                               // zmienna przechowująca stan wejściowy dla funkcji (odczytywany z PLC lub listy)

                if (FirstLine)                                                             // Ustawienie początkowego stanu pamięci RLO, aby razem ze stanem pierwszej zmiennej tworzyło właściwą STLowi tablicę przejść
                {
                    if (line[0].Contains("A"))
                        RLO = 1;
                    if (line[0].Contains("O"))
                        RLO = 0;
                    if (line[0].Contains("X"))
                        RLO = 0;
                }

                switch (line.Length)
                {
                    case 1:                                                                  // jeżeli wiersz ma 1 kolumnę
                        line[0] = line[0].ToUpper();

                        State = bracketHandler(ref line[0], new List<Module> { PLCList });
                        //                    function(line[0], true);
                        FirstLine = false;
                        break;

                    case 2:                                                                  // jeżeli wiersz ma 2 kolumny


                        //if (line[0] == "=")
                        //{
                        //    Writer(line[1], PLCList, RLO);
                        //    FirstLine = true;
                        //    break;
                        //}
                        // obsługa nawiasów
                        #region Brackets  \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
                        State = bracketHandler(ref line[1], ref line[0], new List<Module>() { PLCList });

                        #endregion

                        // Timer   \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

                        if (line[1].Length > 2 && line[1].Remove(2) == "TM")
                            State = TIMER(line[1]);

                        //  Comparator \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

                        if (line[1].Contains("==") || line[1].Contains(">=") || line[1].Contains("<=") || line[1].Contains(">") || line[1].Contains("<") || line[1].Contains("><"))    // komparator
                            State = Comparator(line[1], PLCList);

                        //#region Interpreter Memory  \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
                        //if ((line[0] == "S" && line[1].Contains("INTM")) || (line[0] == "R" && line[1].Contains("INTM")))
                        //{
                        //    object value = line[0] == "S" ? RLO : 0;

                        //    if (Convert.ToInt16(RLO) > 0)
                        //    {
                        //        var query = memoryContent.Select((t, i) => new { t, i }).Where(a => a.t.Contains(PLCList.ModuleName + "\t" + PLCList.IPAddress + "\t" + PLCList.ID + "\t" + line[1] + "\t"));
                        //        if (query.Count() == 0)
                        //            System.IO.File.AppendAllText(@"C:\Users\Public\Documents\InternalMemory.txt", PLCList.ModuleName + "\t" + PLCList.IPAddress + "\t" + PLCList.ID + "\t" + line[1] + "\t" + value.ToString() + "\r\t");
                        //        else
                        //        {
                        //            memoryContentCopy[query.Select(a => a.i).FirstOrDefault()] = PLCList.ModuleName + "\t" + PLCList.IPAddress + "\t" + PLCList.ID + "\t" + line[1] + "\t" + value.ToString();
                        //            System.IO.File.WriteAllLines(@"C:\Users\Public\Documents\InternalMemory.txt", memoryContentCopy);
                        //        }
                        //    }
                        //    FirstLine = true;
                        //    break;
                        //}
                        //#endregion

                        function(line[0], State, true);
                        FirstLine = false;
                        break;

                    case 3:

                        line[2] = line[2].ToUpper();

                        #region Brackets  \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
                        State = bracketHandler(ref line[1], ref line[0], new List<Module> { PLCList }, line[2]);

                        #endregion


                        if (line[1].Contains("==") || line[1].Contains(">=") || line[1].Contains("<=") || line[1].Contains(">") || line[1].Contains("<") || line[1].Contains("><"))    // komparator
                            State = Comparator(line[1], PLCList);

                        function(line[0], State, line[2], true);
                        FirstLine = false;
                        break;
                }                                                             // do switcha

            }                                                               // do foreacha

            Brackets.temp.Clear();
            Brackets.tempstring.Clear();
            Timedata.Number = 0;
        }

        protected bool function(string functionName, bool check)
        {
            object TmpRLO = RLO;

            switch (functionName)
            {

                case "NOT":
                    if (Convert.ToBoolean(RLO))
                        RLO = 0;
                    else
                        RLO = 1;
                    break;

                case "SET":
                    SetExist = true;

                    break;


                case "CNT":
                    SetExist = true;


                    if (Timedata.Timer.Count != 0 && Timedata.Timer[Timedata.Timer.Count - 1] == "CNT")
                    {

                        if (!Convert.ToBoolean(Timedata.OldRLO[Timedata.OldRLO.Count - 1]) && Convert.ToBoolean(RLO))
                        {
                            Timedata.counter[Timedata.counter.Count - 1]++;
                        }
                        Timedata.OldRLO[Timedata.OldRLO.Count - 1] = RLO;
                        RLO = Timedata.counter[Timedata.counter.Count - 1];
                    }
                    else
                    {
                        Timedata.Timer.Add("CNT");
                        Timedata.counter.Add(0);
                        Timedata.OldRLO.Add(Convert.ToInt32(RLO));
                        Timedata.NumberState.Add(Timedata.Number);
                        Timedata.Number++;
                        RLO = 0;
                    }
                    break;


                default:
                    error = true;
                    return true;

            }
            if (!check)
                RLO = TmpRLO;

            return false;
        }

        protected bool function(string FunctionName, object state, bool check)
        {
            bool a, b;
            object TmpRLO = RLO;
            try
            {
                state = state.ToString() == "WrongVarFormat" ? 0 : state;
                a = Convert.ToBoolean(RLO);
                b = Convert.ToInt32(state) == 0 ? false : true;

                #region<Basic function>

                switch (FunctionName)
                {
                    case "A":
                        if (a && b)
                            RLO = 1;
                        else
                            RLO = 0;
                        break;
                    case "O":
                        if (a || b)
                            RLO = 1;
                        else
                            RLO = 0;
                        break;
                    case "AN":
                        b = !b;
                        if (a && b)
                            RLO = 1;
                        else
                            RLO = 0;
                        break;
                    case "ON":
                        b = !b;
                        if (a || b)
                            RLO = 1;
                        else
                            RLO = 0;
                        break;
                    case "X":
                        if ((a && !b) || (!a && b))
                            RLO = state;
                        else

                            RLO = 0;
                        break;
                    case "XN":
                        b = !b;
                        if ((a && !b) || (!a && b))
                            RLO = b;
                        else
                            RLO = 0;
                        break;
                    default:
                        error = true;
                        break;
                }
            }
            catch (Exception exc)
            {
                string s = exc.Message;
                error = true;

            }

            if (!check)
                RLO = TmpRLO;

            #endregion

            return false;

        }

        protected bool function(string FunctionName, object state, string AdvancedFunction, bool check)
        {

            //   int OldRLO = Convert.ToInt32(RLO);

            bool a, b;
            object TmpRLO = RLO;

            a = RLO.GetType() == typeof(string) && RLO.ToString().ToUpper() != "TRUE" && RLO.ToString().ToUpper() != "FALSE" ? Convert.ToBoolean(int.Parse(RLO.ToString())) : Convert.ToBoolean(RLO);
            b = Convert.ToBoolean(Convert.ToInt32(state));

            #region<Basic Function>

            switch (FunctionName)
            {
                case "A":
                    if (a && b)
                        RLO = 1;
                    else
                        RLO = 0;
                    break;
                case "O":
                    if (a || b)
                        RLO = 1;
                    else
                        RLO = 0;
                    break;
                case "AN":
                    b = !b;
                    if (a && b)
                        RLO = 1;
                    else
                        RLO = 0;
                    break;
                case "ON":
                    b = !b;
                    if (a || b)
                        RLO = 1;
                    else
                        RLO = 0;
                    break;
                case "X":
                    if ((a && !b) || (!a && b))
                        RLO = 1;
                    else

                        RLO = 0;
                    break;
                case "XN":
                    b = !b;
                    if ((a && !b) || (!a && b))
                        RLO = 1;
                    else
                        RLO = 0;
                    break;

                default:
                    error = true;
                    break;
            }

            #endregion

            if (AdvancedFunction == "P")
            {
                #region Case: P    \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

                bool existP = false;
                int tempoP = Convert.ToInt32(RLO);


                for (int i = 0; i < Timedata.NumberState.Count; i++)
                {
                    if (Timedata.Number == Timedata.NumberState[i])
                    {
                        existP = true;
                        break;
                    }
                }

                if (existP)
                {
                    if (Convert.ToBoolean(tempoP) && Convert.ToInt32(Timedata.OldRLO[Timedata.Number]) == 0)
                        RLO = 1;
                    else
                        RLO = 0;
                    Timedata.counter[Timedata.Number]++;
                }
                else
                {
                    Timedata.counter.Add(0);
                    Timedata.OldRLO.Add(Convert.ToInt32(RLO));
                    Timedata.NumberState.Add(Timedata.Number);
                    Timedata.Timer.Add("Pslope");

                }

                Timedata.OldRLO[Timedata.Number] = tempoP;
                Timedata.Number++;
                #endregion
            }
            else if (AdvancedFunction == "N")
            {
                #region  Case: N   \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

                bool existN = false;
                int tempoN = Convert.ToInt16(RLO);

                for (int i = 0; i < Timedata.NumberState.Count; i++)
                {
                    if (Timedata.Number == Timedata.NumberState[i])
                        existN = true;
                }

                if (existN)
                {
                    if (!Convert.ToBoolean(tempoN) && Convert.ToInt32(Timedata.OldRLO[Timedata.Number]) == 1)
                        RLO = true;
                    else
                        RLO = false;
                    Timedata.counter[Timedata.Number]++;
                }
                else
                {
                    Timedata.counter.Add(0);
                    Timedata.OldRLO.Add(Convert.ToInt32(RLO));
                    Timedata.NumberState.Add(Timedata.Number);
                    Timedata.Timer.Add("Nslope");
                }

                Timedata.OldRLO[Timedata.Number] = tempoN;
                Timedata.Number++;

                #endregion
            }
            else if (AdvancedFunction.Contains("TM"))
            {
                #region Case: TM  \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

                bool existT = false;

                for (int i = 0; i < Timedata.counter.Count; i++)
                {
                    if (Timedata.Number == Timedata.NumberState[i])
                        existT = true;
                }

                string[] s1 = FunctionName.Split(new char[] { '#' });

                if (existT)
                {
                    if (Convert.ToInt32(Timedata.OldRLO[Timedata.Number]) == Convert.ToInt32(RLO))
                        Timedata.counter[Timedata.Number]++;
                    else
                        Timedata.counter[Timedata.Number] = 0;

                    Timedata.OldRLO[Timedata.Number] = Convert.ToInt32(RLO);
                }
                else
                {
                    Timedata.counter.Add(0);
                    Timedata.NumberState.Add(Timedata.Number);
                    Timedata.OldRLO.Add(Convert.ToInt32(RLO));
                    Timedata.Timer.Add(AdvancedFunction);
                }

                Timedata.Number++;

                #endregion
            }

            if (!check)
                RLO = TmpRLO;
            return false;

        }

        public void ClearLists()
        {
            Timedata.counter.Clear();
            Timedata.Number = 0;
            Timedata.NumberState.Clear();
            Timedata.OldRLO.Clear();
            Timedata.Timer.Clear();

            Brackets.SocketCounter = 0;
            Brackets.temp.Clear();
            Brackets.tempstring.Clear();

        }

        private object bracketHandler(ref string parameter, ref string command, List<Module> PLCList, string advancedCommand = "")
        {
            object state = null;
            bool resetRLO = parameter.Contains("(");

            while (parameter.Contains('('))
            {
                Brackets.temp.Add(Convert.ToInt32(RLO));                            // dodanie do listy poprzednich stanów aktualnego stanu pamięci

                parameter = parameter.Remove(parameter.IndexOf("("), 1);                   // usuwamy z linii znaki rozpoczęcia nawiasu
                Brackets.tempstring.Add(command);                                 // dodajemy do listy poprzednich funkcji aktualną funkcję 

                Brackets.SocketCounter++;                                           // zwiększamy licznik zagnieżdżeń
                                                                                    //              FirstLine = true;                                                 // pozorujemy pierwszą linię programu (odcinamy się od tego, co dzieje się przed nawiasem, ale mając w listach zachowane stany pamięci i komendy)

            }

            if (resetRLO)
            {
                if (command.Contains("A"))
                    RLO = 1;
                if (command.Contains("O"))
                    RLO = 0;
                if (command.Contains("X"))
                    RLO = 0;
            }


            if (parameter.Contains(')'))                                            // Jeżeli linia zawiera jakiekolwiek znaki końca nawiasu, wówczas:
            {
                string n = parameter.Replace(")", "");
                // 1. wykonujemy funkcje do końca, jakby nawiasów nie było, aby otrzymać RLO, które potem będzie wykorzystane
                if (parameter.Length > 2 && parameter.Remove(2) == "TM")
                    state = TIMER(parameter);                                       // funkcja timera
                else
                    if (parameter.Contains("==") || parameter.Contains(">=") || parameter.Contains("<=") || parameter.Contains(">") || parameter.Contains("<") || parameter.Contains("><"))    // komparator
                    state = Comparator(n, PLCList);                      // funkcja komparatora
                else
                    state = Reader(n, PLCList);

                if (advancedCommand == "")
                {
                    function(command, state, true);
                }
                else
                {
                    function(command, state, advancedCommand, true);
                }
            }
            else if (command != "S" && command != "R")
                state = Reader(parameter, PLCList);


            while (parameter.Contains(')'))                                       // 2. działania oparte na sumarycznym wyniku nawiasu oraz funkcji występującej przed otwarciem nawiasu (zapisanej na listach zagnieżdżeń)
            {
                Brackets.SocketCounter--;                                        // dekrementacja licznika zagnieżdżeń

                parameter = parameter.Remove(parameter.IndexOf(")"), 1);               // usunięcie pojedyńczego znaku końca nawiasu

                state = RLO;                                                    // wpisanie aktualnego RLO jako odczytanego stanu ("odczytujemy" łączny wynik logiczny całego nawiasu)
                RLO = Brackets.temp[Brackets.SocketCounter];                    // wczytanie do pamięci stanu RLO z listy 
                command = Brackets.tempstring[Brackets.SocketCounter];          // wczytanie nazwy funkcji z listy
                Brackets.temp.RemoveRange(Brackets.SocketCounter, 1);           // usunięcie wykorzystanych elementów listy
                Brackets.tempstring.RemoveRange(Brackets.SocketCounter, 1);

            }

            return state;
        }

        private object bracketHandler(ref string command, List<Module> PLCList)
        {
            if (command.Contains("("))
                throw new InvalidOperationException("You cannot input openning brackets next to one-word command!");

            object state = null;

            if (command.Contains(')'))                                            // Jeżeli linia zawiera jakiekolwiek znaki końca nawiasu, wówczas:
            {
                string n = command.Replace(")", "");
                // 1. wykonujemy funkcje do końca, jakby nawiasów nie było, aby otrzymać RLO, które potem będzie wykorzystane

                function(n, true);
            }
            else
            {
                function(command, true);
            }

            while (command.Contains(")"))
            {
                Brackets.SocketCounter--;                                        // dekrementacja licznika zagnieżdżeń

                command = command.Remove(command.IndexOf(")"), 1);               // usunięcie pojedyńczego znaku końca nawiasu

                state = RLO;                                                    // wpisanie aktualnego RLO jako odczytanego stanu ("odczytujemy" łączny wynik logiczny całego nawiasu)
                RLO = Brackets.temp[Brackets.SocketCounter];                    // wczytanie do pamięci stanu RLO z listy 
                command = Brackets.tempstring[Brackets.SocketCounter];          // wczytanie nazwy funkcji z listy
                Brackets.temp.RemoveRange(Brackets.SocketCounter, 1);           // usunięcie wykorzystanych elementów listy
                Brackets.tempstring.RemoveRange(Brackets.SocketCounter, 1);
            }

            return state;
        }

        private object Comparator(string content, List<Module> PLCList)
        {

            decimal Value1, Value2;

            string[] compares = { "==", ">=", "<=", ">", "<", "><" };

            for (int i = 0; i < compares.Length; i++)
            {
                if (content.Contains(compares[i]))
                {
                    content = content.Replace(compares[i], "=");
                    string[] s = content.Split('=');

                    if (!decimal.TryParse(s[0], out Value1))
                        Value1 = Convert.ToDecimal(Reader(s[0], PLCList));
                    if (!decimal.TryParse(s[1], out Value2))
                        Value2 = Convert.ToDecimal(Reader(s[1], PLCList));

                    switch (compares[i])
                    {
                        case "==":
                            if (Value1 == Value2)
                                return true;
                            else
                                return false;
                        case ">=":
                            if (Value1 >= Value2)
                                return true;
                            else
                                return false;
                        case "<=":
                            if (Value1 <= Value2)
                                return true;
                            else
                                return false;
                        case ">":
                            if (Value1 > Value2)
                                return true;
                            else
                                return false;
                        case "<":
                            if (Value1 >= Value2)
                                return true;
                            else
                                return false;
                        case "><":
                            if (Value1 != Value2)
                                return true;
                            else
                                return false;
                    }
                }
            }
            return "Wrong Data to Compare";
        }

        private object Comparator(string content, Module PLC)
        {

            decimal Value1, Value2;

            string[] compares = { "==", ">=", "<=", ">", "<", "><" };

            for (int i = 0; i < compares.Length; i++)
            {
                if (content.Contains(compares[i]))
                {
                    content = content.Replace(compares[i], "=");
                    string[] s = content.Split('=');

                    if (!decimal.TryParse(s[0], out Value1))
                        Value1 = Convert.ToDecimal(Reader(s[0], PLC));
                    if (!decimal.TryParse(s[1], out Value2))
                        Value2 = Convert.ToDecimal(Reader(s[1], PLC));

                    switch (compares[i])
                    {
                        case "==":
                            if (Value1 == Value2)
                                return true;
                            else
                                return false;
                        case ">=":
                            if (Value1 >= Value2)
                                return true;
                            else
                                return false;
                        case "<=":
                            if (Value1 <= Value2)
                                return true;
                            else
                                return false;
                        case ">":
                            if (Value1 > Value2)
                                return true;
                            else
                                return false;
                        case "<":
                            if (Value1 >= Value2)
                                return true;
                            else
                                return false;
                        case "><":
                            if (Value1 != Value2)
                                return true;
                            else
                                return false;
                    }
                }
            }
            return "Wrong Data to Compare";
        }

        private object TIMER(string content)
        {
            object State = new object();

            if (content.Length > 2 && content.Remove(2) == "TM")
            {

                int Seconds, DeciSeconds, Time = 0;

                for (int i = 0; i < Timedata.Timer.Count; i++)
                {
                    string[] s1 = Timedata.Timer[i].Split(new char[] { '#' });

                    if (s1[0] == content.Replace("N", ""))
                    {
                        string[] s2 = s1[1].Split(',');
                        if (s2.Length > 1)
                        {
                            if (s2[1].Length > 1)
                                s2[1] = s2[1].Remove(1);


                            if (int.TryParse(s2[0], out Seconds) && int.TryParse(s2[1], out DeciSeconds))
                                Time = Seconds * 10 + DeciSeconds;
                            else
                                error = true;
                        }
                        else
                        {
                            if (int.TryParse(s2[0], out Seconds))
                                Time = Seconds * 10;
                            else
                                error = true;
                        }

                        if (content.Length > 3 && content.Remove(3) == "TMN")
                        {
                            if (Timedata.counter[i] >= Time)
                                State = 0;
                            else
                                State = 1;
                        }
                        else
                        {
                            if (Timedata.counter[i] >= Time)
                                State = 1;
                            else
                                State = 0;
                        }
                    }
                }
            }

            return State;
        }

        private object Reader(string content, List<Module> PLCList)
        {
            object state = new object();

            //#region Read from Interpreters Memory
            //if (content.Contains("INTM"))
            //{
            //    int readMemoryState;

            //    var query = memoryContent.Select((t, i) => new { t, i }).Where(a => a.t.Contains(PLCList[0].ModuleName + "\t" + PLCList[0].IPAddress + "\t" + PLCList[0].ID + "\t" + content + "\t"));
            //    if (query.Count() == 0)
            //        state = 0;
            //    else
            //        if (int.TryParse(query.FirstOrDefault().t.Split()[4], out readMemoryState))
            //        state = readMemoryState;
            //    else
            //        state = 0;

            //    return state;
            //}
            //#endregion

            if (PLCList.Count == 0)
                return null;

            if (!varValues.ContainsKey(content))
                varValues.Add(content, 0);

            #region Read from PLC
            if (content.Contains("&"))
            {

                string s = "";
                int tempID, NumberCount = 0;

                for (int i = 1; i < content.Length; i++)
                {
                    s = content.Remove(i + 1);
                    s = s.Remove(0, 1);

                    if (int.TryParse(s, out tempID))
                        NumberCount++;
                    else
                    {
                        s = content.Remove(i);
                        s = s.Remove(0, 1);
                        break;
                    }
                }


                    if (int.TryParse(s, out tempID) && tempID + 1 <= PLCList.Count)
                    {
                        state = PLCList[tempID].ReadModule(content.Remove(0, NumberCount + 1));
                        varValues[content] = state;
                    }
            }
            else
            {
                state = PLCList[0].ReadModule(content);
                varValues[content] = state;
            }
            #endregion

            if (state.ToString() == "WrongVarFormat" || state.ToString() == "ReadError")
            {
                try
                {
                    state = varValues[content];
                }
                catch (ArgumentOutOfRangeException)
                {
                    state = 0;
                }
            }

            return state;

        }

        private object Reader(string content, Module PLC)
        {
            object state = new object();

            //#region Read from Interpreters Memory
            //if (content.Contains("INTM"))
            //{
            //    int readMemoryState;

            //    var query = memoryContent.Select((t, i) => new { t, i }).Where(a => a.t.Contains(PLC.ModuleName + "\t" + PLC.IPAddress + "\t" + PLC.ID + "\t" + content + "\t"));
            //    if (query.Count() == 0)
            //        state = 0;
            //    else
            //        if (int.TryParse(query.FirstOrDefault().t.Split('\t')[4], out readMemoryState))
            //        state = readMemoryState;

            //    return state;
            //}
            //#endregion
            content = content.TrimEnd(' ');
            if (!varValues.ContainsKey(content))
                varValues.Add(content, 0);

            #region Read from PLC
            if (content.Contains("&"))
            {
                string s = "";
                int tempID, NumberCount = 0;

                for (int i = 1; i < content.Length; i++)
                {
                    s = content.Remove(i + 1);
                    s = s.Remove(0, 1);

                    if (int.TryParse(s, out tempID))
                        NumberCount++;
                    else
                    {
                        s = content.Remove(i);
                        s = s.Remove(0, 1);
                        break;
                    }
                }

                if (int.TryParse(s, out tempID))
                {
                    state = PLC.ReadModule(content.Remove(0, NumberCount + 1));
                    varValues[content] = state;
                }
            }
            else
            {
                state = PLC.ReadModule(content);
                varValues[content] = state;
            }
            #endregion

            if (state.ToString() == "WrongVarFormat")
            {
                try
                {
                    state = varValues[content];
                }
                catch (ArgumentOutOfRangeException)
                {
                    state = 0;
                }
            }

            return state;
        }

        //private void Writer(string content, List<Module> PLCList, object Item)
        //{

        //    if (content.Contains("&"))
        //    {
        //        string s = "";
        //        int tempID, NumberCount = 0;

        //        for (int i = 1; i < content.Length; i++)
        //        {
        //            s = content.Remove(i + 1);
        //            s = s.Remove(0, 1);

        //            if (int.TryParse(s, out tempID))
        //                NumberCount++;
        //            else
        //            {
        //                s = content.Remove(i);
        //                s = s.Remove(0, 1);
        //                break;
        //            }
        //        }

        //        for (int i = 0; i < PLCList.Count; i++)
        //        {
        //            if (int.TryParse(s, out tempID) && tempID == PLCList[i].ID)
        //                PLCList[i].Module.WriteToPLC(content.Remove(0, NumberCount + 1), Item);
        //        }
        //    }
        //    else
        //        PLCList[0].w.WriteToPLC(content, Item);

        //}

        //private void Writer(string content, Module PLC, object Item)
        //{

        //    if (content.Contains("&"))
        //    {
        //        string s = "";
        //        int tempID, NumberCount = 0;

        //        for (int i = 1; i < content.Length; i++)
        //        {
        //            s = content.Remove(i + 1);
        //            s = s.Remove(0, 1);

        //            if (int.TryParse(s, out tempID))
        //                NumberCount++;
        //            else
        //            {
        //                s = content.Remove(i);
        //                s = s.Remove(0, 1);
        //                break;
        //            }
        //        }
        //        if (int.TryParse(s, out tempID) && tempID == PLC.ID)
        //            PLC.Module.WriteToPLC(content.Remove(0, NumberCount + 1), Item);

        //    }
        //    else
        //        PLC.Module.WriteToPLC(content, Item);

        //}

        public object RLOobtain()
        {
            if (SetExist)
                if (RLO == null)
                    return "No commands";
                else
                    return RLO;

            else
                return "NO SET";
        }

        public bool BoolRLOobtain()
        {
            int tempInt;
            bool tempBool;
            string s = RLOobtain().ToString();

            if(s == "No commands" || s == "NO SET")
            {
                return false;
            }

            if(int.TryParse(s,out tempInt))
            {
                return tempInt >= 1;
            }

            bool.TryParse(s, out tempBool);

            return tempBool;
        }

    }
}

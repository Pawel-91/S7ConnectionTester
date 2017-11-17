using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PerfSoftCommunication.Enums;
using PerfSoftCommunication.Interfaces;
using S7.Net;

namespace PerfSoftCommunication.Classes
{
    class PLC : PLCMapper, IDataRead<uint>, IDataRead<bool>
    {

        public PLC(int moduleID, string moduleName, IPAddress ipAddress, string communicationBit, ModuleTypeEnum moduleType, CpuType plcType, int slot, int rack) : base(moduleID, moduleName, ipAddress, communicationBit, moduleType,plcType, slot, rack)
        {

        }
        public PLC(SQLConnection.ComponentsTable module) : base(module)
        {

        }
        public override bool ReadModule(string variable)
        {
            if (!base.mappedVariables.ContainsKey(variable))
            {
                base.AddVariable(variable, true);
            }

            if (base.mappedVariables.ContainsKey(variable))             //Sprawdzenie drugi raz - jeśli wskazana zmienna nie jest boolem, wówczas w słowniku i tak się ona nie pokaże
            {
                return base.mappedVariables[variable];
            }
            else
            {
                throw new IndexOutOfRangeException("Wskazana zmienna nie jest bitem!!");
            }
            
        }
        bool IDataRead<bool>.ReadModule(string variable)
        { 
            return this.ReadModule(variable);
        }
        uint IDataRead<uint>.ReadModule(string variable)
        {
            var value = readVariable(variable);
         //   var value = (this as IDataRead<object>).ReadModule(variable);
            //int tempint;
            //bool tempbool;
            //int.TryParse(value.ToString(), out tempint);

            //if(bool.TryParse(value.ToString(), out tempbool))
            //{
            //    return tempbool ? 1 : 0;
            //}

            return value;
        }


    }
}

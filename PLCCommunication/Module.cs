using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerfSoftCommunication.Interfaces;
using PerfSoftCommunication.Enums;
using System.Net;
using System.Net.NetworkInformation;
using PerfSoftCommunication.SQLConnection;

namespace PerfSoftCommunication.Classes
{
    abstract class Module : IConnectionChecker, IDataRead<bool>
    {
        private ModuleTypeEnum moduleType;
        private IPAddress iPAddress;
        private string moduleName;
        private int moduleID;
        private string communicationBit;

        public Module(int moduleID, string moduleName, IPAddress ipAddress, string communicationBit, ModuleTypeEnum moduleType)
        {
            this.ID = moduleID;
            this.ModuleName = moduleName;
            this.IPAddress = ipAddress;
            this.CommunicationBit = communicationBit;
            this.ModuleType = moduleType;
        }

        public Module(ComponentsTable module)
        {
            this.moduleID = module.IDComponent;
            this.iPAddress = IPAddress.Parse(module.IPAddress);
            this.moduleName = module.Name;
            this.communicationBit = module.CommunicationBit;
            this.moduleType = module.Type.ToModuleTypeEnum();
        }

        internal ModuleTypeEnum ModuleType
        {
            get
            {
                return moduleType;
            }
            set { this.moduleType = value; }
        }
        public IPAddress IPAddress
        {
            get
            {
                return iPAddress;
            }

            protected set { iPAddress = value; }
        } 
        public string ModuleName
        {
            get
            {
                return moduleName;
            }

            set
            {
                this.moduleName = value;
            }
        }
        public int ID
        {
            get
            {
                return moduleID;
            }

           protected set
            {
                this.moduleID = value;
            }
        }
        public string CommunicationBit
        {
            get
            {
                return communicationBit;
            }

            protected set { this.communicationBit = value; }
        }

        public abstract bool ReadModule(string variable);
        public abstract bool OpenConnection();

        /// <summary>
        /// Pinguje moduł i na podstawie odpowiedzi urządzenia zwraca, czy jest podłączone
        /// </summary>
        /// <returns>Zwraca PRAWDA, jeśli pingowanie zakończy się sukcesem</returns>
        public virtual bool CheckConnection()
        {
            using (Ping ping = new Ping())
            {
                if (ping.Send(IPAddress).Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        bool IConnectionChecker.CheckConnection()
        {
            return this.CheckConnection();
        }


    }
}

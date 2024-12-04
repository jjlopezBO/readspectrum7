using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;


namespace ReadSpectrum7
{
    public sealed class OracleI
    {
        private  OracleConnection connection = (OracleConnection)null;
        private  OracleTransaction transaction = (OracleTransaction)null;
        private  string connectionString = (string)null;
     
        private OracleI()
        {
            Console.Write("ORACLE. ACCESO CONTROLADO MODULO N. 1.00.00");
        }

      

       
        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                connectionString = value;
                StartConnection();
            }
        }

        public void DisposeCommand(OracleCommand command)
        {
            if (command == null)
                return;
            command.Dispose();
            command = (OracleCommand)null;
        }

        public void EndConnection()
        {
            if (transaction != null)
            {
                transaction.Dispose();
              //  transaction = ()null;
            }
            if (connection == null || connection.State != ConnectionState.Open)
                return;
           connection.Close();
            connection.Dispose();
            connection = (OracleConnection)null;
        }

        public void StartConnection()
        {
            if (connection != null && connection.State == ConnectionState.Open)
                return;
            if (connectionString == "")
            {

                
                throw new Exception("No se ha definido cadena de conexión");
            }
            try
            {
                Program.LogErrorOnDisk("Iniciando Conexión");
                connection = new OracleConnection(connectionString);
                connection.Open();                
                Program.LogErrorOnDisk("Conexión abierta");
            }
            catch (Exception ex)
            {
                Program.LogErrorOnDisk(ex);
            }
        }

        public void StartTransaction()
        {
            try
            {
                Program.LogErrorOnDisk("Iniciando Transacción");

                transaction = connection.BeginTransaction();
                Program.LogErrorOnDisk("Transacción Iniciada"); 
                
            }
            catch (Exception ex)
            {

                Program.LogErrorOnDisk(ex);
            }
        }

        public void RollBackTransaction()
        {
            if (transaction == null)
                return;
            transaction.Rollback();
        }

        public void CommitTransaction()
        {
            if (transaction == null)
                return;
            transaction.Commit();
        }
    }
}

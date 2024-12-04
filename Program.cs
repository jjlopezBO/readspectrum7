using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;


namespace ReadSpectrum7
{
    class Program
    {
        private static List<FilesDown> lista;
        private static DataRawObjectId rawObj;

        private static void ParseArgs(string[] args, out DateTime fecha, out DateTime fecha2)
        {
            fecha = DateTime.Now.Date;
            fecha2 = DateTime.Now.Date;
            if (args == null)
            {
                Console.WriteLine("Se considera la fecha actual");
                fecha = DateTime.Now;
                fecha2 = DateTime.Now;
            }
            else if (args.Length != 2)
            {
                Console.WriteLine("Se considera la fecha actual");
                ref DateTime local1 = ref fecha;
                DateTime now = DateTime.Now;
                DateTime date1 = now.Date;
                local1 = date1;
                ref DateTime local2 = ref fecha2;
                now = DateTime.Now;
                DateTime date2 = now.Date;
                local2 = date2;
            }
            else
            {
                string s1 = args[0];
                string s2 = args[1];
                try
                {
                    IFormatProvider provider = (IFormatProvider)new CultureInfo("es-BO", true);
                    fecha = DateTime.Parse(s1, provider);
                    fecha2 = DateTime.Parse(s2, provider);
                }
                catch
                {
                    Console.WriteLine("Parametros con formato incorrecto dd/MM/yy: {0} - {1}", (object)s1, (object)s2);
                    Console.ReadLine();
                }
            }
        }

        public static void LogErrorOnDisk(string s )
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BBB_LOG" + DateTime.Now.ToString("ddMMyyyy") + ".txt");
                StreamWriter streamWriter = !File.Exists(path) ? File.CreateText(path) : File.AppendText(path);
                streamWriter.WriteLine();
                streamWriter.WriteLine("------" + (object)DateTime.Now + "--------");
                streamWriter.WriteLine();
                streamWriter.WriteLine(s);
                
                streamWriter.WriteLine();
                streamWriter.WriteLine("--------------------------------");
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Error in writing errorlog:" + ex.Message);
            }
        }
        public static void LogErrorOnDisk(Exception e)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOG" + DateTime.Now.ToString("ddMMyyyy") + ".txt");
                StreamWriter streamWriter = !File.Exists(path) ? File.CreateText(path) : File.AppendText(path);
                streamWriter.WriteLine();
                streamWriter.WriteLine("------" + (object)DateTime.Now + "--------");
                streamWriter.WriteLine();
                streamWriter.WriteLine(e.Message);
                streamWriter.WriteLine(e.ToString());
                streamWriter.WriteLine();
                streamWriter.WriteLine("--------------------------------");
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Error in writing errorlog:" + ex.Message);
            }
        }

        static void Main(string[] args)
        {
            string logName = string.Format("\\log_{0}.txt", DateTime.Now.ToString("ddMMyyyy"));
            StreamWriter streamWriter = new StreamWriter((Stream)new FileStream(AppDomain.CurrentDomain.BaseDirectory + logName, FileMode.Create))
            {
                AutoFlush = true
            };
            Console.BufferWidth = 500;
            Console.SetOut((TextWriter)streamWriter);
            DateTime fecha;
            DateTime fecha2;
            Program.ParseArgs(args, out fecha, out fecha2);
            fecha = new DateTime(2024, 11, 30);
            fecha2 = new DateTime(2024, 11, 30);
            Console.WriteLine("Se ejecuta la carga de los dias {0} al {1}", (object)fecha, (object)fecha2);
            Oracle.ManagedDataAccess.Client.OracleConnection cn = new Oracle.ManagedDataAccess.Client.OracleConnection("USER ID=spectrum;DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.2.13)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=orcl.cndc.bo)));PASSWORD=spectrum;PERSIST SECURITY INFO=true;");
            cn.Open();
            try
            {
                Program.rawObj = new DataRawObjectId(cn);
                Program.lista = new FilesDown("", "", "").ReadFilesDown(cn);
                
                while (fecha <= fecha2)
                {

                    foreach (FilesDown fd in Program.lista)
                    {
                        try
                        {
                           
                                              Program.ProcessDay(fd, fecha,cn);
                           
                        }
                        catch (Exception e)
                        {

                            LogErrorOnDisk(e);
                        }

                    }
                    Console.WriteLine(fecha);
                    fecha= fecha.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                //   int num = (int)MessageBox.Show("se ha presentado un error por favor contactese con el adminsitrador");
                LogErrorOnDisk(ex);
            }
            streamWriter.Flush();
            streamWriter.Close();
        }

        private static void ProcessDay(FilesDown fd, DateTime fecha, OracleConnection cn)
        {
            double num1 = fd.ReadFile(fecha.Date);
            fd.LoadToDb(fecha.Date,cn);
            double num2 = fd.SumDay(fecha,cn);
            StringBuilder stringBuilder = new StringBuilder();
            string str1 = num2.ToString().Replace(",", ".");
            string str2 = num1.ToString().Replace(",", ".");
            if (Math.Round(num2, 2) != Math.Round(num1, 2))
                stringBuilder.AppendFormat("{0},{1},{2},{3},{4},{5} ", (object)"oracle@cndc.bo", (object)"jjlopez@cndc.bo ", (object)"PROCESO: CARGA SPECTRUM", (object)string.Format("{3}:- Valores DIFERENTES fecha : {0} DB: {1} -File: {2} ", (object)fecha, (object)str1, (object)str2, (object)DateTime.Now), (object)"192.168.2.14", (object)"juanjoseru@hotmail.com");

            //SendEmail.email.Send(stringBuilder.ToString(), true);
        }
    }
 
}

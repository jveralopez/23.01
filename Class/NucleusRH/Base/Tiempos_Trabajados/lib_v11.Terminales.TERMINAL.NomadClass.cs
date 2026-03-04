using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Tiempos_Trabajados.Terminales
{
    public class IO_VALIDATOR
    {
        private string _EventsIN;
        public string EventsIN
        {
            get { return _EventsIN; }
        }

        private string _EventsOUT;
        public string EventsOUT
        {
            get { return _EventsOUT; }
        }
        
        private TERMINAL ObjTerminal;
        private Hashtable InputList;
        private Hashtable OutputList;

        char[] separator = { ';' };

        public IO_VALIDATOR(int oi_terminal, string pInputList, string pOutputList)
        {
            ObjTerminal = TERMINAL.Get(oi_terminal);
            InputList = new Hashtable();
            OutputList = new Hashtable();
            
            //LISTA DE EVENTOS DE ENTRADA
            string[] InputArr;
            if (ObjTerminal.d_evento_entradaNull)
            {
                _EventsIN = pInputList.ToUpper();
                InputArr = _EventsIN.Split(separator);
            }
            else
            {
                _EventsIN = ObjTerminal.d_evento_entrada.ToUpper();
                InputArr = _EventsIN.Split(separator);
            }
            for (int t = 0; t < InputArr.Length; t++)
                InputList.Add(InputArr[t], InputArr[t]);
            
            //LISTA DE EVENTOS DE SALIDA
            string[] OutputArr;
            if (ObjTerminal.d_evento_salidaNull)
            {
                _EventsOUT = pOutputList.ToUpper();
                OutputArr = _EventsOUT.Split(separator);
            }
            else
            {
                _EventsOUT = ObjTerminal.d_evento_salida.ToUpper();
                OutputArr = _EventsOUT.Split(separator);
            }
            for (int t = 0; t < OutputArr.Length; t++)
                OutputList.Add(OutputArr[t], OutputArr[t]);
        }

        public bool IsInput(string pEvent)
        {
            if (InputList.ContainsKey(pEvent)) return true;
            else return false;
        }

        public bool IsOutput(string pEvent)
        {
            if (OutputList.ContainsKey(pEvent)) return true;
            else return false;
        }

    }

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Terminales
    public partial class TERMINAL : Nomad.NSystem.Base.NomadObject
    {

        public static void LeerFichadas(int oi_terminal)
        {
            NomadBatch b = NomadBatch.GetBatch("Leer Fichadas", "Leer Fichadas");

            NomadProxy Proxy = NomadProxy.GetProxy();
            NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL MyTerm = NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.Get(oi_terminal);

            string filename = MyTerm.d_ubicacion_soft + "\\" + MyTerm.d_soft_terminal;
            string fileparam = MyTerm.d_param_soft_reg;

            //Mensaje Inicial de Presentacion
            b.Log("Leyendo fichadas desde " + MyTerm.d_terminal + "...");

            //Verifico que este activa la terminal
            if (!MyTerm.l_activa)
                throw new Exception("La terminal no esta activa.");

            //Verifico si esta definido el software de terminal
            if (MyTerm.d_soft_terminal == "")
                throw new Exception("El terminal no tiene especificado ningun software de terminal.");

            //Verifico que Existe el Archivo
            b.SetMess("Buscando " + filename + "...");
            if (!System.IO.File.Exists(filename))
                throw new Exception("El programa para descargar las fichadas no fue encontrado en la ubicacion especificada.");
            NomadBatch.Trace("OK.");

            //Bloqueo la Instancia.
            if (!NomadProxy.GetProxy().Lock().LockOBJ("IncorporarFichadas"))
                throw new Exception("Se estan incorporando Fichadas en este Momento.");

            //Ejecuto el EXE
            b.SetPro(5);
            b.SetMess("Descargando las Fichadas...");
            b.Log("Ejecutando " + filename + " " + fileparam + "...");

            //Genero el Process Info
            System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo();
            ps.FileName = filename;
            ps.Arguments = fileparam;
            ps.WorkingDirectory = MyTerm.d_ubicacion_soft;
            ps.CreateNoWindow = true;
            ps.UseShellExecute = false;
            ps.RedirectStandardOutput = true;
            ps.RedirectStandardError = false;

            string output;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = ps;
            p.Start();

            do
            {
                output = p.StandardOutput.ReadLine();
                if ((output != null) && (output != "")) b.Log(output);
            } while (output != null);
            p.WaitForExit();
            if (p.ExitCode != 0)
                throw new Exception("La lectura del Terminal finalizo con Errores.");

            b.Log("La lectura del Terminal finalizo Correctamente.");

            //Verifico que Existe el Archivo
            b.SetPro(45);
            b.SetMess("Buscando archivo de Registraciones....");
            b.Log("Buscando el archivo " + MyTerm.d_archivo_reg + "...");
            if (!System.IO.File.Exists(MyTerm.d_archivo_reg))
                throw new Exception("El archivo de fichadas no fue encontrado en la ubicacion especificada.");
            NomadBatch.Trace("OK.");

            //////////////////////////////////////////////////////////////////////////
            //Mover el archivo generado a la carpeta de interfaces
            b.SetPro(50);
            b.SetMess("Realizando el BackUp....");
            b.Log("Copiando archivo generado, a la carpeta de interfaces....");
            Proxy.FileServiceIO().SaveBinFile("INTERFACES", "regist.dat", MyTerm.d_archivo_reg);
            Proxy.FileServiceIO().SaveBinFile("BACKUP", DateTime.Now.ToString("yyyyMMddHHmmss") + "_regist.dat", MyTerm.d_archivo_reg);
            System.IO.File.Delete(MyTerm.d_archivo_reg);
            NomadBatch.Trace("OK.");

            //////////////////////////////////////////////////////////////////////////
            //Importando Fichadas
            b.SetPro(60);
            b.SetMess("Incorporando Registraciones....");
            b.SetSubBatch(60, 100);
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.ImportarFichadas(oi_terminal);
        }

        public static void ExportarNomina(int oi_terminal, Nomad.NSystem.Proxy.NomadXML filtro)
        {
            NomadBatch b = NomadBatch.GetBatch("Exportar Nomina", "Exportar Nomina");

            NomadProxy Proxy = NomadProxy.GetProxy();
            NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL MyTerm = NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.Get(oi_terminal);

            string filename = MyTerm.d_ubicacion_soft + "\\" + MyTerm.d_soft_terminal;
            string fileparam = MyTerm.d_param_soft_nom;

            //Mensaje Inicial de Presentacion
            b.Log("Exportar Nomina a " + MyTerm.d_terminal + "...");

            //Verifico que este activa la terminal
            if (!MyTerm.l_activa)
                throw new Exception("La terminal no esta activa.");

            //Verifico si esta definido el software de terminal
            if (MyTerm.d_soft_terminal == "")
                throw new Exception("El terminal no tiene especificado ningun software de terminal.");

            //Verifico que Existe el Archivo
            b.SetMess("Buscando " + filename + "...");
            if (!System.IO.File.Exists(filename))
                throw new Exception("El programa para descargar las fichadas no fue encontrado en la ubicacion especificada.");
            NomadBatch.Trace("OK.");

            //////////////////////////////////////////////////////////////////////////
            //Exportando Nomina
            b.SetMess("Generando archivo de Nomina...");
            b.SetSubBatch(5, 60);
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.ExportarNomina(oi_terminal, filtro);
            NomadBatch.Trace("OK.");

            //////////////////////////////////////////////////////////////////////////
            //Mover el archivo generado de la carpeta de interfaces
            b.SetPro(60);
            b.SetMess("Copiando archivo generado....");
            b.Log("Copiando archivo generado, a la carpeta de interfaces....");
            Proxy.FileServiceIO().LoadBinFile("INTERFACES", "personal.nom", MyTerm.d_archivo_nom);
            System.IO.File.Delete(MyTerm.d_archivo_reg);
            NomadBatch.Trace("OK.");

            //////////////////////////////////////////////////////////////////////////
            //Ejecuto el EXE
            b.SetPro(70);
            b.SetMess("Enviando la Nomina al Terminal...");
            b.Log("Ejecutando " + filename + " " + fileparam + "...");

            //Genero el Process Info
            System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo();
            ps.FileName = filename;
            ps.Arguments = fileparam;
            ps.WorkingDirectory = MyTerm.d_ubicacion_soft;
            ps.CreateNoWindow = true;
            ps.UseShellExecute = false;
            ps.RedirectStandardOutput = true;
            ps.RedirectStandardError = false;

            string output;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = ps;
            p.Start();

            //Mesajes debueltos por el EJECUTABLE
            do
            {
                output = p.StandardOutput.ReadLine();
                if ((output != null) && (output != "")) b.Log(output);
            } while (output != null);
            p.WaitForExit();

            //Finalizado con ERROR
            if (p.ExitCode != 0)
                throw new Exception("El envio de la Nomina al Terminal finalizo con Errores.");

            //Finalizado OK
            b.SetPro(100);
            b.Log("El envio de la Nomina al Terminal finalizo Correctamente.");
        }

        public static void IngresoFichadasAuto()
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Incorporacion Automatica de Fichadas");
            NomadXML xmlTer = NomadEnvironment.QueryNomadXML(Resources.QRY_TERMINALES, "");

            for (NomadXML xmlCT = xmlTer.FirstChild().FirstChild(); xmlCT != null; xmlCT = xmlCT.Next())
            {
                try
                {
                    objBatch.Log("Recuperando fichadas del reloj " + xmlCT.GetAttr("code"));
                    NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.LeerFichadas(xmlCT.GetAttrInt("id"));
                    objBatch.Log("Reloj " + xmlCT.GetAttr("code") + ": OK");
                }
                catch (Exception e)
                {
                    objBatch.Err("Error en reloj " + xmlCT.GetAttr("code") + ": " + e.Message.ToString());
                }
            }
        }
    }
}



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

namespace NucleusRH.Base.Tiempos_Trabajados.Conceptos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Conceptos - Tiempos Trabajados
    public partial class CONCEPTO : Nomad.NSystem.Base.NomadObject
    {
        public static void GenerarArchivo(Nomad.NSystem.Proxy.NomadXML Param, string Mode)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Archivo de Horas Trabajadas");

            objBatch.SetPro(0);
            objBatch.SetMess("Generando Archivo...");

            NomadXML queryResult = NomadEnvironment.QueryNomadXML(CONCEPTO.Resources.QRY_CONCEPTOS, Param.ToString());

            switch (Mode)
            {
                case "N":
                    NucleusRH.Base.Tiempos_Trabajados.InterfaceHoras.HORAS.GenerarArchivo(queryResult);
                    break;
                case "OPUS":
                    NucleusRH.Base.Tiempos_Trabajados.InterfaceHorasOPUS.HORAS.GenerarArchivo(queryResult);
                    break;
                case "P":
                    NucleusRH.Base.Tiempos_Trabajados.InterfaceHorasPayroll.HORAS.GenerarArchivo(queryResult);
                    break;
            }
        }
    }
}



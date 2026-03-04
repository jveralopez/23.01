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

namespace NucleusRH.Base.Tiempos_Trabajados.NASPersonal
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Nomina de Personal
    public partial class NOMINA : Nomad.NSystem.Base.NomadObject
    {

        public static void ExportarNomina(int oi_terminal, string queryResult)
        {
            NomadBatch b = NomadBatch.GetBatch("Exportar Nomina NAS", "Exportar Nomina NAS");

            //Codigo en .NET
            string cvtSrcchar;
            string lasssecc, legajoString, ape_y_nomString, tarjetaString, tipo_docString, nro_docString;
            char[] auxChar = { (char)225, (char)233, (char)237, (char)243, (char)250, (char)193, (char)201, (char)205, (char)211, (char)218, (char)241, (char)209, (char)44 };
            int ficadd, ficerr;
            NucleusRH.Base.Tiempos_Trabajados.NASPersonal.NOMINA ddoNOMINA;

            cvtSrcchar = new string(auxChar);            

            b.Log("Eliminando el archivo anterior.");
            NomadProxy.GetProxy().DDOService().Set("NucleusRH.Base.Tiempos_Trabajados.NASPersonal.NOMINA", "<object class=\"NucleusRH.Base.Tiempos_Trabajados.NASPersonal.NOMINA\" ><NOMINA nmd-status=\"~d,\" /></object>");

            b.Log("Generando Archivo de Nomina...");
            b.SetMess("Generando Archivo de Nomina...");
            ficadd = 0; ficerr = 0; lasssecc = "";

            NomadXML xmlResult = new NomadXML(queryResult);
            xmlResult = xmlResult.FirstChild();

            int i = 0;
            int tot = xmlResult.ChildLength;
            for (NomadXML cur = xmlResult.FirstChild(); cur != null; cur = cur.Next(), i++)
            {
                b.SetPro(0, 100, tot, i);
                b.SetMess("Analizando linea " + (i + 1).ToString() + " de " + tot.ToString());

                ape_y_nomString = "";
                tarjetaString = "";
                legajoString = "";
                tipo_docString = "";
                nro_docString = "";

                try
                {
                    //Informacion para el ERROR
                    lasssecc = "informacion general";
                    ape_y_nomString = cur.GetAttr("ape_y_nom");
                    tarjetaString = cur.GetAttr("tarjeta");
                    legajoString = cur.GetAttr("legajo");
                    tipo_docString = cur.GetAttr("tipo_doc");
                    nro_docString = cur.GetAttr("nro_doc");

                    lasssecc = "creando registro";
                    ddoNOMINA = new NucleusRH.Base.Tiempos_Trabajados.NASPersonal.NOMINA();

                    //VALORES
                    lasssecc = "asignando valores";
                    ddoNOMINA.ape_y_nom = ape_y_nomString;
                    ddoNOMINA.tarjeta = tarjetaString;
                    ddoNOMINA.legajo = legajoString;
                    ddoNOMINA.tipo_doc = tipo_docString;
                    ddoNOMINA.nro_doc = nro_docString;

                    //Grabo
                    lasssecc = "Guardando Nomina";
                    try { NomadEnvironment.GetCurrentTransaction().Save(ddoNOMINA); ficadd++; }
                    catch { b.Err("No se pudo guardar el Registro. Personal: " + ape_y_nomString + "..."); ficerr++; continue; }

                    if (ficadd % 50 == 0) b.Log("Se exportaron " + ficadd.ToString() + " Legajos.");
                }
                catch (Exception e)
                {
                    ficerr++;
                    b.Err("Error desconocido. " + lasssecc + ".- " + e.Message + ". Personal: " + ape_y_nomString + "...");
                }
            }

            //Finalizo.
            b.Log("Se exportaron " + ficadd.ToString() + " Legajos.");
            if (ficerr > 0) b.Log("Se encontraron " + ficerr.ToString() + " Legajos con ERROR.");
            b.SetPro(100);
        }
    }
}



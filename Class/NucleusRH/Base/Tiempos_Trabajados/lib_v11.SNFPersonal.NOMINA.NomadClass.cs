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

namespace NucleusRH.Base.Tiempos_Trabajados.SNFPersonal
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Nomina de Personal
    public partial class NOMINA : Nomad.NSystem.Base.NomadObject
    {

        public static void ExportarNomina(int oi_terminal, string queryResult)
        {
            NomadBatch b = NomadBatch.GetBatch("Exportar Nomina SNF", "Exportar Nomina SNF");

            //Codigo en .NET
            string validchar, cvtSrcchar, cvtTrgchar;
            string lasssecc, legajoString, empresaString, nombreString;
            char[] auxChar = { (char)225, (char)233, (char)237, (char)243, (char)250, (char)193, (char)201, (char)205, (char)211, (char)218, (char)241, (char)209, (char)44 };
            int ficadd, ficerr, t;
            NucleusRH.Base.Tiempos_Trabajados.SNFPersonal.NOMINA ddoNOMINA;

            validchar = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz,. :";
            cvtSrcchar = new string(auxChar);
            cvtTrgchar = "aeiouAEIOUnN,";

            b.Log("Eliminando el archivo anterior.");
            NomadProxy.GetProxy().DDOService().Set("NucleusRH.Base.Tiempos_Trabajados.SNFPersonal.NOMINA", "<object class=\"NucleusRH.Base.Tiempos_Trabajados.SNFPersonal.NOMINA\" ><NOMINA nmd-status=\"~d,\" /></object>");

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

                empresaString = "";
                legajoString = "";
                try
                {
                    //Informacion para el ERROR
                    lasssecc = "informacion general";
                    empresaString = cur.GetAttr("c_empresa");
                    legajoString = cur.GetAttr("e_numero_legajo");

                    lasssecc = "creando registro";
                    ddoNOMINA = new NucleusRH.Base.Tiempos_Trabajados.SNFPersonal.NOMINA();

                    //VALORES
                    lasssecc = "asignando valores";
                    ddoNOMINA.estado = "N";
                    ddoNOMINA.mensaje = cur.GetAttr("c_mensaje");
                    try { ddoNOMINA.legajo = cur.GetAttr("e_nro_legajo_reloj").PadLeft(5, '0'); }
                    catch { b.Err("No se pudo leer el Legajo Reloj. " + empresaString + "-" + legajoString + "..."); ficerr++; continue; }
                    if (cur.GetAttr("e_nro_legajo_reloj").Length > 5)
                        b.Wrn("El nro de legajo tiene mas de 5 de largo. Personal: " + empresaString + "-" + legajoString + "...");

                    try
                    {
                        //Obtengo la Nombre
                        nombreString = cur.GetAttr("d_ape_y_nom");

                        //Verifico el Largo
                        if (nombreString.Length > 20) nombreString = nombreString.Substring(0, 20);

                        //Valido los Caracteres
                        auxChar = nombreString.ToCharArray(); nombreString = "";
                        for (t = 0; t < auxChar.Length; t++)
                        {
                            if (validchar.IndexOf(auxChar[t]) >= 0)
                                nombreString += auxChar[t].ToString();
                            else
                                if (cvtSrcchar.IndexOf(auxChar[t]) >= 0)
                                    nombreString += cvtTrgchar.Substring(cvtSrcchar.IndexOf(auxChar[t]), 1);
                                else
                                    nombreString += ".";
                        }

                        //Actualizo el Nombre
                        ddoNOMINA.nombre = nombreString;
                    }
                    catch { b.Err("No se pudo leer el Apellido y Nombre. Personal: " + empresaString + "-" + legajoString + "..."); ficerr++; continue; }

                    try { ddoNOMINA.tarjeta = cur.GetAttr("d_nro_tarjeta").PadLeft(8, '0'); }
                    catch { b.Err("No se pudo leer el Numero de Tarjeta. Personal: " + empresaString + "-" + legajoString + "..."); ficerr++; continue; }

                    if (cur.GetAttr("d_nro_tarjeta").Length != 8)
                        b.Wrn("El nro de tarjeta no tiene largo 8. Personal: " + empresaString + "-" + legajoString + "...");

                    //Grabo
                    lasssecc = "Guardando Nomina";
                    try { NomadEnvironment.GetCurrentTransaction().Save(ddoNOMINA); ficadd++; }
                    catch { b.Err("No se pudo guardar el Registro. Personal: " + empresaString + "-" + legajoString + "..."); ficerr++; continue; }

                    if (ficadd % 50 == 0) b.Log("Se exportaron " + ficadd.ToString() + " Legajos.");
                }
                catch (Exception e)
                {
                    ficerr++;
                    b.Err("Error desconocido. " + lasssecc + ".- " + e.Message + ". Personal: " + empresaString + "-" + legajoString + "...");                    
                }
            }

            //Finalizo.
            b.Log("Se exportaron " + ficadd.ToString() + " Legajos.");
            if (ficerr > 0) b.Log("Se encontraron " + ficerr.ToString() + " Legajos con ERROR.");
            b.SetPro(100);
        }

    }
}



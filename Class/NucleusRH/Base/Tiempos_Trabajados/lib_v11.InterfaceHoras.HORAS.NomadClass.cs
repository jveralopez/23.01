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

namespace NucleusRH.Base.Tiempos_Trabajados.InterfaceHoras
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Conceptos - Tiempos Trabajados
    public partial class HORAS : Nomad.NSystem.Base.NomadObject
    {
        public static void GenerarArchivo(Nomad.NSystem.Proxy.NomadXML queryResult)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Horas Trabajadas a NucleusRH", "Horas Trabajadas a NucleusRH");

            objBatch.SetPro(0);
            objBatch.SetMess("Generando Archivo para NucleusRH...");
            HORAS ddoINT;

            //Limpiando el Archivo
            NomadProxy.GetProxy().DDOService().Set("NucleusRH.Base.Tiempos_Trabajados.InterfaceHoras.HORAS", "<object class=\"NucleusRH.Base.Tiempos_Trabajados.InterfaceHoras.HORAS\" ><HORAS nmd-status=\"~d,\" /></object>");
            NomadXML QRY = queryResult.FirstChild();

            //Recorro la Lista
            int x = 0;
            for (NomadXML myCUR = QRY.FirstChild(); myCUR != null; myCUR = myCUR.Next(), x++)
            {
                objBatch.SetPro(0, 100, QRY.ChildLength, x);
                objBatch.SetMess("Generando " + x.ToString() + " de " + QRY.ChildLength.ToString() + " ...");

                ddoINT = new HORAS();

                //VALORES		     		
                ddoINT.legajo = myCUR.GetAttr("e_numero_legajo");
                ddoINT.enlace = myCUR.GetAttr("c_externo");
                ddoINT.cantidad = myCUR.GetAttr("cantidad");

                NomadEnvironment.GetCurrentTransaction().Save(ddoINT);
            }
        }
    }
}
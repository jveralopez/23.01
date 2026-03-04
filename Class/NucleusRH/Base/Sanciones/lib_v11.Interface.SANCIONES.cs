using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Sanciones.Interface
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interface de Sanciones
    public partial class SANCIONES 
    {
        public static void GenerarArchivo(Nomad.NSystem.Proxy.NomadXML queryResult)
        {

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Novedades de Sanciones a NucleusRH", "Novedades de Sanciones a NucleusRH");

            objBatch.SetPro(0);
            objBatch.SetMess("Generando archivo para NucleusRH...");
            SANCIONES ddoINT;

            //Limpiando el Archivo
            NomadProxy.GetProxy().DDOService().Set("NucleusRH.Base.Sanciones.Interface.SANCIONES", "<object class=\"NucleusRH.Base.Sanciones.Interface.SANCIONES\" ><SANCIONES nmd-status=\"~d,\" /></object>");
            NomadXML QRY = queryResult.FirstChild();

            //Recorro la Lista
            int x = 0;
            for (NomadXML myCUR = QRY.FirstChild(); myCUR != null; myCUR = myCUR.Next(), x++)
            {
                objBatch.SetPro(0, 100, QRY.ChildLength, x);
                objBatch.SetMess("Generando " + x.ToString() + " de " + QRY.ChildLength.ToString() + " ...");

                ddoINT = new SANCIONES();

                //VALORES		     		
                ddoINT.legajo = myCUR.GetAttr("e_numero_legajo");
                ddoINT.enlace = myCUR.GetAttr("c_enlace");
                ddoINT.dias = myCUR.GetAttr("dias_susp_real");

                //Grabo
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(ddoINT);
                }
                catch
                {

                }
            }

        }
    }
}

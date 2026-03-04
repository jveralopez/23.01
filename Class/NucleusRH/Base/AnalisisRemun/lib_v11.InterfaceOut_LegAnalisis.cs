using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.AnalisisRemun.InterfaceOut_LegAnalisis
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase interface Salida periodos
    public partial class SALIDA 
    {
        public static void ejec_outRemun(string par_oi_Empresa, string par_Periodo, Nomad.NSystem.Proxy.NomadXML Doc_Liq)
        {


            Nomad.NSystem.Base.NomadBatch objBatch;

            Nomad.NSystem.Base.NomadBatch.Trace("*********  Comienza la ejecucion de Interface Analisis Remuneraciones **************");

            NomadProxy.GetProxy().DDOService().Set("NucleusRH.Base.AnalisisRemun.InterfaceOut_LegAnalisis.SALIDA", "<object class=\"NucleusRH.Base.AnalisisRemun.InterfaceOut_LegAnalisis.SALIDA\" > <SALIDA nmd-status=\"~d,\" /></object>");


            NucleusRH.Base.AnalisisRemun.InterfacesOut.clsImportarPeriodos nmdOUT;
            nmdOUT = new NucleusRH.Base.AnalisisRemun.InterfacesOut.clsImportarPeriodos();
            objBatch = Nomad.NSystem.Base.NomadBatch.GetBatch("Iniciando...", "Ejecutando");

            objBatch.SetSubBatch(0, 100);

            nmdOUT.fncEjecutarIntOut(par_oi_Empresa, par_Periodo, Doc_Liq);
            objBatch.SetMess(" Fin actualizacion");

            Nomad.NSystem.Base.NomadBatch.Trace("********* Fin del proceso **************");

        }

    }
}

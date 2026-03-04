using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.AnalisisRemun.Interface_LegAnalisis
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase interface entrada periodos
    public partial class ENTRADA 
    {
        public static void ejec_intRemun(string par_oi_Empresa, string par_Reemplaza)
        {


            Nomad.NSystem.Base.NomadBatch objBatch;

            Nomad.NSystem.Base.NomadBatch.Trace("*********  Comienza la ejecucion de Interface Analisis Remuneraciones **************");

            NucleusRH.Base.AnalisisRemun.Interfaces.clsImportarPeriodos nmdINT;
            nmdINT = new NucleusRH.Base.AnalisisRemun.Interfaces.clsImportarPeriodos();
            objBatch = Nomad.NSystem.Base.NomadBatch.GetBatch("Iniciando...", "Ejecutando");

            objBatch.SetSubBatch(0, 100);

            nmdINT.ImportarRemuneracion(par_oi_Empresa, par_Reemplaza);
            objBatch.SetMess(" Fin actualizacion");

            Nomad.NSystem.Base.NomadBatch.Trace("********* Fin del proceso **************");

        }

    }
}

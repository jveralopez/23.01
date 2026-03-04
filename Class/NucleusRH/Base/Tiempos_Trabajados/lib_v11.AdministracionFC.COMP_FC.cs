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

namespace NucleusRH.Base.Tiempos_Trabajados.AdministracionFC 
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Compensaciones de Francos Compensatorios
    public partial class COMP_FC : Nomad.NSystem.Base.NomadObject
    {
        public static void EliminarComp(Nomad.NSystem.Proxy.NomadXML param)
        {
            //Instancio el Objeto Batch
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Eliminar registro de Compensación", "Eliminar registro de Compensación");

            if(param.isDocument)
                param = param.FirstChild();

            COMP_FC objCOMP_FC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC.Get(param.GetAttr("id"));
            REGISTRO_FC objREG_FC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(objCOMP_FC.oi_registro_fc);

            objREG_FC.n_saldo += objCOMP_FC.n_cant_comp;
            objREG_FC.n_consumidos -= objCOMP_FC.n_cant_comp;

            //Elimino el COMP_FC y guardo el REGISTRO_FC
            try
            {
                NomadEnvironment.GetCurrentTransaction().Begin();
                objREG_FC.COMP_FC.RemoveById(objCOMP_FC.Id);
                NomadEnvironment.GetCurrentTransaction().Save(objREG_FC);
                NomadEnvironment.GetCurrentTransaction().Commit();
                objBatch.Log("El registro se eliminó correctamente");
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                objBatch.Err("No se pudo eliminar la compensación del día: " + objCOMP_FC.f_comp.ToString("dd/MM/yyyy") + "\n Error generado: " + e.Message);
            }

        }
    }
}

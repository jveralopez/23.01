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

namespace NucleusRH.Base.Vacaciones.WFSolicitud
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase WF Solicitud Vacaciones
    public partial class SOLICITUD : Nomad.NSystem.Base.NomadObject
    {
        public static void Aprobar(Nomad.NSystem.Proxy.NomadXML XMLData)
        {
            NomadXML MySOLICITUD = XMLData.FindElement("WFI").FindElement("DATOS").FindElement("SOLICITUD");

            //Obtengo el PERSONAL EMP
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP objPER = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(MySOLICITUD.GetAttrInt("oi_personal_emp"));

            //Agrego la Solicitud
            string SOLId = objPER.AgregarSolicitud(
                MySOLICITUD.GetAttrDateTime("f_desde_solicitud"),
                MySOLICITUD.GetAttrDateTime("f_hasta_solicitud").AddDays(-1),
                MySOLICITUD.GetAttrInt("e_dias"),
                0,
                "",
                "",
                MySOLICITUD.GetAttr("l_automatica"),
                "",
                ""
                );

            //Apruebo la Solicitud.
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.AprobarSolicitud(objPER, SOLId, false);
            NomadEnvironment.GetCurrentTransaction().Save(objPER);

            //Cambio el Estado del Workflow.
            MySOLICITUD.SetAttr("c_estado", "Aprobado");
            Nomad.Base.Workflow.WorkflowInstancias.WFI.PassTo(ref XMLData, "FINALIZADA");
            return;
        }
        
        public static Nomad.NSystem.Proxy.NomadXML RecalcularFechas(string oi_personal_emp, string fecha_desde, string fecha_hasta, string cant_dias)
        {
            string dias_bonif = "0";
            //Le resto 1 a la fecha HASTA
            if (fecha_hasta != "") fecha_hasta = Nomad.NSystem.Functions.StringUtil.date2str(Nomad.NSystem.Functions.StringUtil.str2date(fecha_hasta).AddDays(-1));

            //Calculo la Cantidad de DIAS
            NomadXML retval = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.RecalcularFechas(oi_personal_emp, fecha_desde, fecha_hasta, cant_dias);

            //Le sumo 1 a la fecha HASTA
            fecha_hasta = retval.GetAttr("fecha_hasta");
            if (fecha_hasta != "") fecha_hasta = Nomad.NSystem.Functions.StringUtil.date2str(Nomad.NSystem.Functions.StringUtil.str2date(fecha_hasta).AddDays(+1));
            retval.SetAttr("fecha_hasta", fecha_hasta);

            //Obtengo dias bonificados	

            DateTime desde = new DateTime(int.Parse(fecha_desde.Substring(0, 4)), int.Parse(fecha_desde.Substring(4, 2)), int.Parse(fecha_desde.Substring(6, 2)));
            DateTime hasta = new DateTime(int.Parse(fecha_hasta.Substring(0, 4)), int.Parse(fecha_hasta.Substring(4, 2)), int.Parse(fecha_hasta.Substring(6, 2)));

            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.GetDiasBonif(oi_personal_emp, desde, hasta, ref dias_bonif);
            retval.SetAttr("dias_bonif", dias_bonif);

            //Fin
            return retval;
        }
    }
}

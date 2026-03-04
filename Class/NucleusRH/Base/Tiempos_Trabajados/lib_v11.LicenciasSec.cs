using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Personal.LegajoEmpresa;
using NucleusRH.Base.Personal.Licencias;
using NucleusRH.Base.Organizacion.Convenios;
using NucleusRH.Base.Tiempos_Trabajados.LicenciasRol;

namespace NucleusRH.Base.Tiempos_Trabajados.LicenciasSec
{
  //////////////////
  //Clase Licencias
  public partial class LICENCIA_SEC : Nomad.NSystem.Base.NomadObject
  {
  ///////////// VALIDACIONES VIEJAS ////////////////////////////////////////////////////////////////////////////////////////////////////////
    public static NomadXML ValidarLicenciaSec(int oi_personal_emp, int oi_licencia_rol, DateTime f_inicio, DateTime f_fin, int dias, int anio)
    {
        //Recuperar (Tipo de) Licencia
        LICENCIA_ROL ddoLicRol = LICENCIA_ROL.Get(oi_licencia_rol);

        return NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.ValidarLicencia(oi_personal_emp, int.Parse(ddoLicRol.oi_licencia), f_inicio, f_fin, dias, anio);
    }

  /////////////////////////////////////////////////////////////////
  public static void AltaLicenciaSec(LICENCIA_SEC DDO, bool forzar)
  {
    //Recuperar Licencia por Rol
        LICENCIA_ROL ddoLicRol = LICENCIA_ROL.Get(DDO.oi_licencia_rol);

        //Crear la Licencia
        LICENCIA_PER ddoLicPer = new LICENCIA_PER();
        ddoLicPer.oi_licencia = ddoLicRol.oi_licencia;
        ddoLicPer.f_inicio = DDO.f_inicio;
        ddoLicPer.f_fin = DDO.f_fin;
        ddoLicPer.e_cant_dias = DDO.e_cant_dias;
        ddoLicPer.e_anio_corresp = DDO.e_anio_corresp;
        ddoLicPer.o_licencia_per = DDO.o_licencia;
        ddoLicPer.l_bloqueada = false;
        ddoLicPer.l_interfaz = false;
        ddoLicPer.l_habiles = false;

    //Validar la Licencia
    NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicenciaSec(DDO.oi_personal_emp, ddoLicPer, forzar);

        //Recuperar el Legajo
        PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(DDO.oi_personal_emp);

        //Agregar Licencia a la coleccion
        ddoPerEmp.LICEN_PER.Add(ddoLicPer);

        //Guardar el Legajo
    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPerEmp);

        //Completar la Licencia con Seguridad
        DDO.oi_licencia_per = StringUtil.str2int(ddoPerEmp.LICEN_PER[ddoPerEmp.LICEN_PER.Count - 1].Id);
        DDO.f_crea = DateTime.Now;
        DDO.d_user_crea = NomadProxy.GetProxy().UserEtty;

        //Guardar la Licencia con Seguridad
    NomadEnvironment.GetCurrentTransaction().Save(DDO);
  }

  ///////////// VALIDACIONES VIEJAS /////////////
    public static NomadXML ValidarDelete(string ID)
    {
        NomadXML xmlResult = new NomadXML("RESULT");
        xmlResult.SetAttr("valid", "0");

        //Recuperar Licencia con Seguridad
        LICENCIA_SEC DDO = LICENCIA_SEC.Get(ID);

        try
        {
            //Recuperar Licencia en Legajo
            LICENCIA_PER ddoLicPer = LICENCIA_PER.Get(DDO.oi_licencia_per);

            //Existe Licencia en Legajo
            //No puede eliminar Licencia con Seguridad
            xmlResult.SetAttr("alert", "La Licencia se encuentra asociada a un Legajo.");
        }
        catch (Exception)
        {
            //No existe Licencia en Legajo
            //Puede eliminar Licencia con Seguridad
            xmlResult.SetAttr("valid", "1");
        }

        return xmlResult;
    }

  /////////////////////////////////////////////
    public static void BajaLicenciaSec(string ID)
    {
        //Recuperar Licencia con Seguridad
        LICENCIA_SEC ddoLicSec = LICENCIA_SEC.Get(ID);

        //Recuperar Legajo
        PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(ddoLicSec.oi_personal_emp);

        //Borrar Licencia en Legajo
        ddoPerEmp.LICEN_PER.RemoveById(ddoLicSec.oi_licencia_per.ToString());

        //Guardar Legajo
        NomadEnvironment.GetCurrentTransaction().Save(ddoPerEmp);

        //Borrar Licencia con Seguridad
        NomadEnvironment.GetCurrentTransaction().Delete(ddoLicSec);
    }

    public static void AnularLicenciaSec(string ID)
    {
        NucleusRH.Base.Tiempos_Trabajados.LicenciasSec.LICENCIA_SEC ddoLicSec = NucleusRH.Base.Tiempos_Trabajados.LicenciasSec.LICENCIA_SEC.Get(ID);

        PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(ddoLicSec.oi_personal_emp);

        //Borrar Licencia en Legajo
        ddoPerEmp.LICEN_PER.RemoveById(ddoLicSec.oi_licencia_per.ToString());

        //Guardar Legajo
        NomadEnvironment.GetCurrentTransaction().Save(ddoPerEmp);

        NomadEnvironment.GetCurrentTransaction().Save(ddoLicSec);

    }

      //guarda un registro de tta23_licencias_per: oi de per02_licen_per
    public static bool GuardarLicenciaPorRol(string id)
    {

        NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPer = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.Get(id,true);

        NomadXML XMLResult = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.LicenciasRol.LICENCIA_ROL.Resources.GetPorLicencia,"<DATOS oi_licencia='"+ddoLicPer.oi_licencia+"' />").FirstChild();

        NucleusRH.Base.Tiempos_Trabajados.LicenciasSec.LICENCIA_SEC ddolicenciaSec = new LICENCIA_SEC();

        ddolicenciaSec.f_inicio = ddoLicPer.f_inicio;
        ddolicenciaSec.f_fin = ddoLicPer.f_fin;
        ddolicenciaSec.e_cant_dias = ddoLicPer.e_cant_dias;
        ddolicenciaSec.e_anio_corresp = ddoLicPer.e_anio_corresp;
        ddolicenciaSec.d_user_crea = "Workflow";
        ddolicenciaSec.f_crea = DateTime.Now;
        ddolicenciaSec.oi_licencia_per = ddoLicPer.id;
        ddolicenciaSec.oi_personal_emp = ddoLicPer.oi_personal_emp.ToString();
        ddolicenciaSec.oi_licencia_rol = XMLResult.GetAttr("ID");

        //Guardar licencia per
        NomadEnvironment.GetCurrentTransaction().Save(ddolicenciaSec);

        return true;
    }
  }
}


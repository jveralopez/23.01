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

namespace NucleusRH.Base.Tiempos_Trabajados.LicenciasRol 
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Licencias por Rol
  public partial class LICENCIA_ROL : Nomad.NSystem.Base.NomadObject
  {
    //////////////////////////////////////////////////////////////////////////////////
    //Metodos
    //

    public static void SaveLicRol(NucleusRH.Base.Tiempos_Trabajados.LicenciasRol.LICENCIA_ROL DDO)
    {
        NomadTransaction objTran = new NomadTransaction();

        //Definir la politica de seguridad        
        DDO.Security.Policy = "./ROLE/" + Nomad.Base.Security.Roles.ROL.Get(DDO.d_rol).COD;

        //Guardar la Licencia por Rol
        try
        {
            objTran.Begin();
            objTran.Save(DDO);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }
    }


    public static NomadXML DeleteLicRol(string ID)
    {
        NomadTransaction objTran = new NomadTransaction();

        NomadXML xmlResult = new NomadXML("RESULT");
        xmlResult.SetAttr("valid", 1);
        
        //Recuperar la Licencia por Rol
        LICENCIA_ROL DDO = LICENCIA_ROL.Get(ID);
        
        //Borrar la Licencia por Rol
        try
        {
            objTran.Begin();
            objTran.Delete(DDO);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            xmlResult.SetAttr("valid", 0);
            xmlResult.SetAttr("alert", "Existe una dependencia de registros.");
        }

        return xmlResult;
    }

  }

}
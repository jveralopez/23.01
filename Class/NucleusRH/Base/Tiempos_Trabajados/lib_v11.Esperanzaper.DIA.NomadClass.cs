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

namespace NucleusRH.Base.Tiempos_Trabajados.Esperanzaper
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Esperanza Horaria
  public partial class DIA : Nomad.NSystem.Base.NomadObject
  {

    public string Horario
    {
      get
      {
        string strResult = "";

        if (!oi_licenciaNull)
          strResult = NomadEnvironment.QueryValue("TTA02_HORARIOS", "c_horario", "oi_horario", oi_horario.ToString(), "", true);

        return strResult;
      }
    }

    public string Turno
    {
      get
      {
        string strResult = "";

        if (!oi_turnoNull)
          strResult = NomadEnvironment.QueryValue("TTA14_TURNOS", "c_turno", "oi_turno", oi_turno.ToString(), "", true);

        return strResult;
      }
    }

    public string Licencia
    {
      get
      {
        string strResult = "";

        if (!oi_licenciaNull)
          strResult = NomadEnvironment.QueryValue("PER16_LICENCIAS", "c_licencia", "oi_licencia", oi_licencia.ToString(), "", true);

        return strResult;
      }
    }

  }
}



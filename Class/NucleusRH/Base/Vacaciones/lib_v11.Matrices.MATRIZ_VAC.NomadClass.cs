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

namespace NucleusRH.Base.Vacaciones.Matrices
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Solicitud Vacaciones
    public partial class MATRIZ_VAC : Nomad.NSystem.Base.NomadObject
    {
        public void validarMatriz()
        {
            if (this.CELDAS_MAT.Count <= 0)
                throw new NomadAppException("Debe completar al menos una celda de detalle para poder guardar la matriz.");

            int maximo = 1200;
            int t, minMAT, maxMAT, lastDIAS;
            int[] Meses = new int[maximo];

            //Inicializo el Arrays
            for (t = 0; t < maximo; t++)
                Meses[t] = -1;

            //Analizo las CELDAS
            minMAT = -1; maxMAT = -1;
            foreach (NucleusRH.Base.Vacaciones.Matrices.CELDA_MAT x in this.CELDAS_MAT)
            {
                if (x.e_dias_vac < 0)
                    throw new NomadAppException("Celdas de la Matriz tiene cantidad de dias menor a 0.");

                if (x.e_dias_vac >= maximo)
                    throw new NomadAppException("Celdas de la Matriz tiene cantidad de dias mayor a " + maximo.ToString() + ".");

                if (x.e_antig_desde < 0)
                    throw new NomadAppException("Celdas de la Matriz tiene cantidad de meses menor a 0.");

                if (x.e_antig_hasta < 0)
                    throw new NomadAppException("Celdas de la Matriz tiene cantidad de meses mayor a " + maximo.ToString() + ".");

                if (x.e_antig_desde > x.e_antig_hasta)
                    throw new NomadAppException("Celdas de la Matriz tiene el mes desde mayor al mes hasta.");

                if (minMAT == -1 || minMAT > x.e_antig_desde) minMAT = x.e_antig_desde;
                if (maxMAT == -1 || maxMAT < x.e_antig_hasta) maxMAT = x.e_antig_hasta;

                //Verifico Solapamientos
                for (t = x.e_antig_desde; t <= x.e_antig_hasta; t++)
                {
                    if (Meses[t] != -1)
                        throw new NomadAppException("Se encontraron Celdas de la Matriz con solapamiento de meses de antiguedad.");

                    Meses[t] = x.e_dias_vac;
                }
            }

            //Verifico Bujeros
            lastDIAS = 0;

            for (t = minMAT; t <= maxMAT; t++)
            {
                if (Meses[t] == -1)
                    throw new NomadAppException("Existen huecos en la Matriz.");

                if (lastDIAS > Meses[t])
                    throw new NomadAppException("La cantidad de dias de Vacaciones para el MES " + t.ToString() + " es menor al del MES " + (t - 1).ToString() + ".");

                lastDIAS = Meses[t];
            }
        }

    ////////////////////////////////////////////////////////////////////////////////////////////
    //Los métodos siguientes NO deberían estar en esta librería, pero por cuestiones de definiciones
    //previas se situo aquí. El lugar correcto sería LegajoVacaciones.PERSONAL_EMP.
    //Sin embargo, no se puede generar un controlador en una clase extendida
    ////////////////////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////////////////////
    //Este método recupera las solicitudes de vacaciones de un legajo en un periodo determinado definido
    //por la cantidad de dias previo y dias posteriores a la fecha actual
    ////////////////////////////////////////////////////////////////////////////////////////////

    public static List<SortedList<string, object>> GetVacaciones(int PAR, int dias_prev, int dias_post)
    {
      NomadLog.Debug("-----------------------------------------------------");
      NomadLog.Debug("-----------GET SOLICITUDES DE VACACIONES-------------");
      NomadLog.Debug("-----------------------------------------------------");

      NomadLog.Debug("Get_SOL.PersonalEMP: " + PAR);
      NomadLog.Debug("Get_SOL.dias_prev: " + dias_prev);
      NomadLog.Debug("Get_SOL.dias_post: " + dias_post);

      List<SortedList<string, object>> retorno = new List<SortedList<string, object>>();
      SortedList<string, object> row;
      SortedList<string, string> types = new SortedList<string, string>();
      string type = "";

      int linea;

      NomadXML MyROW;

      NomadXML param = new NomadXML("PARAM");

      //Agrego los parametros
      param.SetAttr("oi_personal_emp", PAR);
      param.SetAttr("dias_prev", dias_prev);
      param.SetAttr("dias_post", dias_post);

      //Ejecuto el recurso
      NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Vacaciones.Matrices.MATRIZ_VAC.GET_SOL", param.ToString());
      NomadLog.Debug((resultado.FirstChild() == null ? "No se encontraron Solicitudes de vacaciones para el legajo con ID: " + PAR + "." : "Solicitudes de Vacaciones encontradas " + resultado.ChildLength + "."));

      if (resultado.FirstChild() != null)
      {
        //Armo una sorted list con los atributos del resultado y sus tipos
        for (int x = 0; x < resultado.Attrs.Count; x++)
        {
          types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
        }

        //resultado = resultado.FirstChild();
        for (linea = 1, MyROW = resultado.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
        {
          row = new SortedList<string, object>();

          for (int r = 0; r < MyROW.Attrs.Count; r++)
          {
            //Busco de que tipo es el atributo
            foreach (KeyValuePair<string, string> kvp in types)
            {
              if (kvp.Key == MyROW.Attrs[r].ToString())
              {
                type = kvp.Value;
                break;
              }
            }

            //Agrego el atributo en base a su tipo
            switch (type)
            {
              case "string":
                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                break;
              case "int":
                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrInt(MyROW.Attrs[r].ToString()));
                break;
              case "datetime":
                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDateTime(MyROW.Attrs[r].ToString()));
                break;
              case "double":
                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDouble(MyROW.Attrs[r].ToString()));
                break;
              case "bool":
                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrBool(MyROW.Attrs[r].ToString()));
                break;
              default:
                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                break;
            }
            type = "";
          }

          //Agrego la solicitud a la lista de resultados
          retorno.Add(row);
        }
      }
      NomadLog.Debug("Retorno: " + retorno.ToString());
      return retorno;
    }

    public static System.Collections.Generic.SortedList<string, object> GetVacacionesYConvenio(int PAR, int dias_prev, int dias_post)
    {
        NomadLog.Debug("----------------------------------------------------------------");
        NomadLog.Debug("-----------GET SOLICITUDES DE VACACIONES Y CONVENIO-------------");
        NomadLog.Debug("----------------------------------------------------------------");

        NomadLog.Debug("Get_SOL.PersonalEMP: " + PAR);
        NomadLog.Debug("Get_SOL.dias_prev: " + dias_prev);
        NomadLog.Debug("Get_SOL.dias_post: " + dias_post);

        int saldo;

        List<SortedList<string, object>> solicitudes = new List<SortedList<string, object>>(); 
        SortedList<string, object> legajo = new SortedList<string, object>(); 

        SortedList<string, object> retorno = new SortedList<string, object>();

        retorno["LEGAJO"] = legajo;

        //Obtengo las solicitudes
        solicitudes = MATRIZ_VAC.GetVacaciones(PAR, dias_prev, dias_post);
        retorno["SOLICITUDES"] = solicitudes;

        //Obtengo el saldo
        saldo = MATRIZ_VAC.GetSaldo(PAR);

        //Obtengo el convenio del query
        SortedList<string, string> types = new SortedList<string, string>();
        string type = "";


        NomadXML param = new NomadXML("PARAM");

        //Agrego los parametros
        param.SetAttr("oi_personal_emp", PAR);

        //Ejecuto el recurso
        NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.QRY_GetConvenioLeg", param.ToString());
        NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró un convenio para el legajo con ID: " + PAR + "." : "Convenios encontrados " + resultado.ChildLength + "."));
     

        if (resultado.FirstChild() != null)
        {
            //Armo una sorted list con los atributos del resultado y sus tipos
            for (int x = 0; x < resultado.Attrs.Count; x++)
            {
                types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
            }

            resultado = resultado.FirstChild();

            for (int r = 0; r < resultado.Attrs.Count; r++)
            {
                //Busco de que tipo es el atributo
                foreach (KeyValuePair<string, string> kvp in types)
                {
                    if (kvp.Key == resultado.Attrs[r].ToString())
                    {
                        type = kvp.Value;
                        break;
                    }
                }

                //Agrego el atributo en base a su tipo
                switch (type)
                {
                    case "string":
                        legajo.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                        break;
                    case "int":
                        legajo.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                        break;
                    case "datetime":
                        legajo.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                        break;
                    case "double":
                        legajo.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                        break;
                    case "bool":
                        legajo.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                        break;
                    default:
                        legajo.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                        break;
                }

                type = "";
            }
        }

        //Agrego al legajo el saldo
        legajo.Add("SALDO", saldo);
  
        NomadLog.Debug("Retorno: " + retorno.ToString());
        return retorno;
    }

////////////////////////////////////////////////////////////////////////////////////////////
//Este método calcula el saldo de días de vacaciones de un legajo.
//Es utilizado dentro del WF solicitud de vacaciones.
////////////////////////////////////////////////////////////////////////////////////////////
public static int GetSaldo(int PAR)
{
  NomadLog.Debug("-----------------------------------------------");
  NomadLog.Debug("-----------GET SALDO DE VACACIONES-------------");
  NomadLog.Debug("-----------------------------------------------");

  NomadLog.Debug("Get_SOL.PersonalEMP: " + PAR);

  NomadXML param = new NomadXML("PARAM");

  //Agrego los parametros
  param.SetAttr("oi_personal_emp", PAR);

  //Ejecuto el recurso
  NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Vacaciones.Matrices.MATRIZ_VAC.GET_SALDO", param.ToString());
  NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró saldo de vacaciones para el legajo con ID: " + PAR + "." : "Saldo de Vacaciones encontradas " + resultado.ChildLength + "."));

  return (resultado.FirstChild() != null ? resultado.FirstChild().GetAttrInt("SALDO"):0);
}

////////////////////////////////////////////////////////////////////////////////////////////
//Este método guarda una solictud de vacaciones generada desde el WF solicitud de vacaciones.
// Recibe los datos desde el formulario del EF.
////////////////////////////////////////////////////////////////////////////////////////////
public static string GuardarSolicitud(string PER, SortedList<string, object> SOLICITUD)
{
   NomadLog.Debug("-----------------------------------------");
   NomadLog.Debug("-----------AGREGAR SOLICITUD-------------");
   NomadLog.Debug("-----------------------------------------");

   NomadLog.Debug("GuardarSolictud.oi_personal: " + PER);

   //Get PERSONAL
   NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD SOL;
   NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP PERSONAL_EMP;
   SOL = new NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD();
   string l_habiles = "0";

   PERSONAL_EMP = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(PER);
   if (PERSONAL_EMP == null) return "0";

   foreach (KeyValuePair<string, object> kvp in SOLICITUD)
  {
     switch (kvp.Key)
      {
          case "F_INICIO":
             if (kvp.Value != null)
              SOL.f_desde_solicitud = (DateTime)kvp.Value;
                break;
          case "F_FIN":
            if (kvp.Value != null)
                 SOL.f_hasta_solicitud = (DateTime)kvp.Value;
                break;
          case "MOTIVOS":
              if (kvp.Value != null)
                SOL.d_motivo_solic = (string)kvp.Value;
                break;
          case "OBSERVACIONES":
            if (kvp.Value != null)
                SOL.o_solicitud = (string)kvp.Value;
                break;
          case "DIAS":
            if (kvp.Value != null)
                SOL.e_dias_solicitud = Convert.ToInt32(kvp.Value);
                break;
          case "BONIFICADOS":
            if (kvp.Value != null)
                SOL.e_dias_bonif = Convert.ToInt32(kvp.Value);
                break;
          case "L_HABILES":
                 l_habiles = (string)kvp.Value;
                break;
          case "D_HABILES":
            if (kvp.Value != null)
                SOL.d_habiles = (string)kvp.Value;
                break;
       }
  }

    try
    {
      NomadLog.Debug("L_HABILES: " + SOL.l_habiles);
        string solID = PERSONAL_EMP.AgregarSolicitud(SOL.f_desde_solicitud,SOL.f_hasta_solicitud,SOL.e_dias_solicitud,0,SOL.d_motivo_solic,SOL.o_solicitud,"false",l_habiles.ToString(),SOL.d_habiles);
       PERSONAL_EMP.Aprobar_Solicitud(solID,true);
        return "1";
    }
    catch (Exception ex)
    {
      NomadLog.Debug("Error guardando SOLICITUD: " + ex);
       return "0";
    }
  }

////////////////////////////////////////////////////////////////////////////////////////////
//Este método recalcula la cantidad de días de acuerdo a que el legajo tenga días habiles o
// días corridos de vacaciones. Es utilizado dentro del WF solicitud de vacaciones.
////////////////////////////////////////////////////////////////////////////////////////////
public static SortedList<string, object> RecalcularFecha(string oi_personal_emp, string fecha_desde, string fecha_hasta)
{
    NomadLog.Debug("-----------------PARAMETROS-----------------------------");
    NomadLog.Debug("OI_PERSONAL_EMP: " + oi_personal_emp);
    NomadLog.Debug("FECHA DESDE: " + fecha_desde);
    NomadLog.Debug("FECHA HASTA: " + fecha_hasta);
    NomadLog.Debug("--------------------------------------------------------");
 
    SortedList<string, object> data = new SortedList<string, object>();
    SortedList<string, string> rta = new SortedList<string, string>();
    SortedList<string, object> retorno = new SortedList<string, object>();
    DateTime desde = new DateTime();
    DateTime hasta = new DateTime();
    bool solapamiento = false;
    string msgError = "";
    rta.Add("VAL", "ERR");
    rta.Add("DES", "El método no fue procesado");

    desde = Nomad.NSystem.Functions.StringUtil.str2date(fecha_desde);
    hasta = Nomad.NSystem.Functions.StringUtil.str2date(fecha_hasta);

    LegajoVacaciones.PERSONAL_EMP objPerEmp = LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp, true);

    foreach (Personal.LegajoEmpresa.LICENCIA_PER lic in objPerEmp.LICEN_PER)
    {
        if ((desde >= lic.f_inicio && desde <= lic.f_fin) || (hasta >= lic.f_inicio && hasta <= lic.f_fin) || (desde <= lic.f_inicio && hasta >= lic.f_fin))
        {
            solapamiento = true;
            msgError = "La solicitud de vacaciones se solapa con una licencia ya generada \n";
            break;
        }     
    }

    Nomad.NSystem.Proxy.NomadXML rtaXML;
    rtaXML = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.RecalcularFechas(oi_personal_emp, fecha_desde, fecha_hasta, "");

    NomadLog.Debug("Retorno1: " + rtaXML.ToString());

    string fdesde = rtaXML.GetAttr("fecha_desde");
    string fhasta = rtaXML.GetAttr("fecha_hasta");
    string dias = rtaXML.GetAttr("cant_dias");
    string bonif = rtaXML.GetAttr("dias_bonif");
    string l_habiles = rtaXML.GetAttr("l_habiles");
    string d_habiles = rtaXML.GetAttr("d_habiles");

    data.Add("fecha_desde", fdesde);
    data.Add("fecha_hasta", fhasta);
    data.Add("cant_dias", dias);
    data.Add("dias_bonif", bonif);
    data.Add("l_habiles", l_habiles);
    data.Add("d_habiles", d_habiles);

    retorno["DATA"] = data;
    retorno["RTA"] = rta;

    NomadLog.Debug("Retorno: " + retorno.ToString());

    if (rtaXML.GetAttr("ERR") != "" || solapamiento)
    {
        msgError += rtaXML.GetAttr("ERR"); 
        rta = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", msgError);
        retorno["RTA"] = rta;
        return retorno;
    }
    else
    {
        rta = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "Validaciones correctas");
        retorno["RTA"] = rta;
        return retorno;
    }
}

public static System.Collections.Generic.SortedList<string, string> ValidarYRegistrarVacacion(string PER, System.Collections.Generic.SortedList<string, object> SOLICITUD) 
        {
            #region DEBUG
            NomadLog.Debug("----------------------------------------------------");
            NomadLog.Debug("-----------Validar y Registrar Vacacion-------------");
            NomadLog.Debug("----------------------------------------------------");

            foreach (KeyValuePair<string, object> kvp in SOLICITUD)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            NomadLog.Debug("ValidarYRegistrarVacacion.oi_personal_emp: " + PER);
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");
            try
            {
                //Metodos para validar nuevamente antes de guardar.

                #region SOLAPAMIENTO
                //Solapamiento con otras licencias
                DateTime desde = new DateTime();
                DateTime hasta = new DateTime();

                desde = Nomad.NSystem.Functions.StringUtil.str2date(SOLICITUD["F_DESDE"].ToString());
                hasta = Nomad.NSystem.Functions.StringUtil.str2date(SOLICITUD["F_FIN"].ToString());

                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP objPerEmp = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(PER);
                if (objPerEmp == null)
                    throw new Exception("El legajo con oi_personal_emp " + PER + " no se pudo encontrar.");

                foreach (Personal.LegajoEmpresa.LICENCIA_PER lic in objPerEmp.LICEN_PER)
                {
                    if ((desde >= lic.f_inicio && desde <= lic.f_fin) || (hasta >= lic.f_inicio && hasta <= lic.f_fin) || (desde <= lic.f_inicio && hasta >= lic.f_fin))
                        throw new Exception("La solicitud de vacaciones se solapa con una licencia ya generada.");
                }
                #endregion

                #region RECALCULAR FECHAS
                //Recalcular Fechas
                Nomad.NSystem.Proxy.NomadXML rtaXML;
                rtaXML = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.RecalcularFechas(PER, SOLICITUD["F_DESDE"].ToString(), SOLICITUD["F_FIN"].ToString(), "");

                if (rtaXML.GetAttr("ERR") != "")
                    throw new Exception(rtaXML.GetAttr("ERR"));
                #endregion

                #region GENERAR SOLICITUD
                //Generar Solicitud
                NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP personal = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(PER);

                string motivos;
                if(SOLICITUD["MOTIVOS"] == null) { motivos = null; } 
                else{ motivos = SOLICITUD["MOTIVOS"].ToString(); }

                string SolId = personal.AgregarSolicitud(
                    rtaXML.GetAttrDateTime("fecha_desde"),
                    rtaXML.GetAttrDateTime("fecha_hasta"),
                    rtaXML.GetAttrInt("cant_dias"),
                    0,
                    motivos,
                    "",
                    "1",
                    rtaXML.GetAttr("l_habiles"),
                    rtaXML.GetAttr("d_habiles")
                );

                NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.AprobarSolicitud(personal, SolId, false);

                //Busco entre las solicitudes del personal la que se agrego recientemente por las fechas del periodo
                //Esto se hace prq el metodo AgregarSolicitud() no devuelve un id como entero sino que parece un id hasheado.
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(personal);
                bool findSol = false;
                foreach (LegajoVacaciones.SOLICITUD sol in personal.SOLICITUDES)
                {
                    if(sol.f_desde_solicitud == rtaXML.GetAttrDateTime("fecha_desde") && sol.f_hasta_solicitud == rtaXML.GetAttrDateTime("fecha_hasta"))
                    {
                        SolId = sol.id.ToString();
                        findSol = true;
                    }
                }

                if(!findSol)
                    throw new Exception("No se pudo encontrar la solicutud que se generó recientemente.");
                #endregion

                #region LIQUIDAR SOLICITUD
                //Liquidar solicitud vacación

                //Armado XML parametro liquidacion vacacion
                NomadXML xmlElemento = new NomadXML("RAIZ");
                NomadXML xmlLiquidar = new NomadXML("DATOS");
                NomadXML xmlROWS = new NomadXML("ROWS");
                xmlROWS.AddHeadElement("ROW");
                
                NomadXML xmlSELECT = new NomadXML("SELECT");
                xmlSELECT .AddHeadElement("ROW");

                xmlROWS.FirstChild().SetAttr("id",SolId);
                xmlROWS.FirstChild().SetAttr("dias", rtaXML.GetAttr("cant_dias"));
                xmlROWS.FirstChild().SetAttr("desde", rtaXML.GetAttr("fecha_desde"));
                xmlROWS.FirstChild().SetAttr("hasta", rtaXML.GetAttr("fecha_hasta"));
                xmlROWS.FirstChild().SetAttr("oi_personal_emp", PER);
                xmlROWS.FirstChild().SetAttr("label", "");

                xmlSELECT.FirstChild().SetAttr("id", SolId);

                xmlLiquidar.AddHeadElement(xmlSELECT);
                xmlLiquidar.AddHeadElement(xmlROWS);
                xmlElemento.AddHeadElement(xmlLiquidar);

                NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD.Liquidar_Selec(xmlElemento);
                #endregion

                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "La licencia por vacaciones se registró exitosamente");
            }
            catch(Exception e)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR",e.Message);
            }
            return retorno;
        }

    }
}



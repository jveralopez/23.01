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

namespace NucleusRH.Base.Legales.PedidosLiqFinal
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Pedidos de liquidación final
  public partial class PEDIDO_LF : Nomad.NSystem.Base.NomadObject
  {

    //////////////////////////////////////////////////////////////////////////////////
    //Metodos
    //

    public static NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF NEW_PEDIDO_LF(string oi_carpeta)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("-----------NEW_PEDIDO_LF-------------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

        //Obtener cantidad de pedidos de LF para la carpeta
        NomadXML xmlParamPedidoLF = new NomadXML("DATA");
        xmlParamPedidoLF.SetAttr("COUNT", "1");
        xmlParamPedidoLF.SetAttr("WHERE", "LEG61_PEDIDOS_LF.oi_carpeta=" + oi_carpeta.ToString());
        NomadXML xmlCountPedidosLF = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Resources.INFO, xmlParamPedidoLF.ToString());

        //Obtener los montos de la carpeta
        NomadXML xmlParamCarpeta = new NomadXML("DATA");
        xmlParamCarpeta.SetAttr("oi_carpeta", oi_carpeta);
        NomadXML xmlMontosCarpeta = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Resources.getMontosCarpeta, xmlParamCarpeta.ToString());

        //Obtener los montos total (acuerdo) e inicial (reclamo)
        string montoTotal = xmlMontosCarpeta.FirstChild().GetAttr("n_monto_acuerdo");
    string montoInicial = xmlMontosCarpeta.FirstChild().GetAttr("n_monto_reclamo");

        //Crear un nuevo pedido de LF con los datos obtenidos
        NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF objPedidoLF = new NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF();
        objPedidoLF.oi_carpeta = oi_carpeta;
        //objPedidoLF.c_pedido_lf = (int.Parse(xmlCountPedidosLF.FirstChild().GetAttr("COUNT")) + 1).ToString("0000");
		objPedidoLF.c_pedido_lf = (StringUtil.str2int(xmlCountPedidosLF.FirstChild().GetAttr("COUNT")) + 1).ToString("0000");
        objPedidoLF.f_crea = DateTime.Now;
    objPedidoLF.c_usr_crea = NomadProxy.GetProxy().UserEtty;
        objPedidoLF.c_estado = "P";
        if (montoTotal!="") objPedidoLF.n_monto_total = StringUtil.str2dbl(montoTotal);
    if (montoInicial!="") objPedidoLF.n_monto_inicial = StringUtil.str2dbl(montoInicial);
    if (objPedidoLF.n_monto_total>0 && objPedidoLF.n_monto_inicial>0 && objPedidoLF.n_monto_total<=objPedidoLF.n_monto_inicial)
      objPedidoLF.n_porcentaje = (objPedidoLF.n_monto_total/objPedidoLF.n_monto_inicial)*100;

    //Sugerir la forma de pago
        string actores = "";
        NomadXML xmlActoresCarpeta = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Resources.getActoresCarpeta, xmlParamCarpeta.ToString());
        foreach (NomadXML row in xmlActoresCarpeta.FirstChild().GetChilds())
        {
            actores += row.GetAttr("d_sujeto").ToUpper() + ", ";
        }
        if (actores != "") objPedidoLF.d_pago = "Cheque no a la orden, sin cruzar, a favor de " + actores.Remove(actores.Length - 2);

        return objPedidoLF;
    }

    public static void SAVE_PEDIDO_LF(NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF DDO_PEDIDO_LF)
    {
        NomadEnvironment.GetTrace().Info("--------------------------------------");
        NomadEnvironment.GetTrace().Info("-----------SAVE_PEDIDO_LF-------------");
        NomadEnvironment.GetTrace().Info("--------------------------------------");

        NomadTransaction objTran = new NomadTransaction();

        //Guardar el pedido de LF
        try
        {
            objTran.Begin();
            objTran.Save(DDO_PEDIDO_LF);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }
    }

    public static void ANULAR_PEDIDO_LF(string oi_pedido_lf, string observaciones)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("----------ANULAR_PEDIDO_LF-----------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

        //Recuperar el pedido de LF
    NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF objPedidoLF = NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Get(oi_pedido_lf);

    //Cambiar estado del pedido de LF a Anulado y agregar fecha, usuario y observaciones
        objPedidoLF.c_estado = "A";
        objPedidoLF.f_anula = DateTime.Now;
    objPedidoLF.c_usr_anula = NomadProxy.GetProxy().UserEtty;
    objPedidoLF.o_anula = observaciones;

        //Guardar el pedido de LF
        NomadTransaction objTran = new NomadTransaction();
        try
        {
            objTran.Begin();
            objTran.Save(objPedidoLF);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }
    }

  //APROBAR PEDIDO DE LIQUIDACION FINAL MASIVO
  public static NomadXML APROBAR_PEDIDO_LF(NomadXML pedidos, string observaciones)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("----------APROBAR_PEDIDO_LF----------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

    NomadTransaction objTran = new NomadTransaction();
    NomadXML xmlError = new NomadXML("ERROR");
        NomadXML xmlParam;

    //Validar que exista el tipo de tarea 12
        xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_tipo_tarea", "12");
        NomadXML xmlTipoTarea12 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Tipos_Tarea.TIPO_TAREA.Resources.QRY_TIPO_TAREA, xmlParam.ToString());
        if (xmlTipoTarea12.FirstChild().GetAttr("oi_tipo_tarea") == null || xmlTipoTarea12.FirstChild().GetAttr("oi_tipo_tarea") == "")
        {
            xmlError.SetAttr("error", "No se puede crear la tarea asociada porque no existe el tipo de tarea 12-Pedido de Liquidación Final.");
            return xmlError;
        }

        //Validar que exista el sujeto GRU_LIQ con clasificacion PIN
        xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_sujeto", "GRU_LIQ");
        xmlParam.SetAttr("c_clasif_suj", "PIN");
        NomadXML xmlClasifSujeto12 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Sujetos.CLASIF_SUJETO.Resources.QRY_CLASIF_SUJETO, xmlParam.ToString());
        if (xmlClasifSujeto12.FirstChild().GetAttr("oi_clasif_sujeto") == null || xmlClasifSujeto12.FirstChild().GetAttr("oi_clasif_sujeto") == "")
        {
            xmlError.SetAttr("error", "No se puede crear la tarea asociada porque no existe el sujeto GRU_LIQ o la clasificación PIN para el mismo.");
            return xmlError;
        }

    //Recuperar el OI del estado de tarea 1-A Completar
    xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_estado_tarea", "1");
    NomadXML xmlEstadoTarea1 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Estados_Tarea.ESTADO_TAREA.Resources.QRY_EST_TAREA, xmlParam.ToString());

    foreach (NomadXML pedido in pedidos.FirstChild().GetChilds())
        {
      //Recuperar el pedido de LF
      string oi_pedido_lf = pedido.GetAttr("oi_pedido_lf");
      NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF objPedidoLF = NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Get(oi_pedido_lf);

      //Cambiar estado del pedido a Aprobado y agregar fecha, usuario y observaciones
      objPedidoLF.c_estado = "AP";
      objPedidoLF.f_aprueba = DateTime.Now;
      objPedidoLF.c_usr_aprueba = NomadProxy.GetProxy().UserEtty;
      objPedidoLF.o_aprueba = observaciones;

      //Validar si ya existe una tarea de tipo 13 para la carpeta
      xmlParam = new NomadXML("DATA");
      xmlParam.SetAttr("c_tipo_tarea", "13");
      xmlParam.SetAttr("oi_carpeta", objPedidoLF.oi_carpeta);
      NomadXML xmlTarea13 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Tareas.TAREA.Resources.QRY_TAREA, xmlParam.ToString());

      if (xmlTarea13.FirstChild().GetAttr("oi_tarea") == null || xmlTarea13.FirstChild().GetAttr("oi_tarea") == "")
      {
        //Validar que exista el tipo de tarea 13
        xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_tipo_tarea", "13");
        NomadXML xmlTipoTarea13 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Tipos_Tarea.TIPO_TAREA.Resources.QRY_TIPO_TAREA, xmlParam.ToString());
        if (xmlTipoTarea13.FirstChild().GetAttr("oi_tipo_tarea") == null || xmlTipoTarea13.FirstChild().GetAttr("oi_tipo_tarea") == "")
        {
          xmlError.SetAttr("error", "No se puede crear la tarea adicional porque no existe el tipo de tarea 13-Adjuntar Telegrama.");
          return xmlError;
        }

        //Validar que exista la sucursal de la carpeta como sujeto con clasificacion SUC
        NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(objPedidoLF.oi_carpeta);
        NucleusRH.Base.Organizacion.Empresas.UBICACION objUbicacion = NucleusRH.Base.Organizacion.Empresas.UBICACION.Get(objCarpeta.oi_ubicacion);
        xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_sujeto", objUbicacion.c_ubicacion);
        xmlParam.SetAttr("c_clasif_suj", "SUC");
        NomadXML xmlClasifSujeto13 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Sujetos.CLASIF_SUJETO.Resources.QRY_CLASIF_SUJETO, xmlParam.ToString());
        if (xmlClasifSujeto13.FirstChild().GetAttr("oi_clasif_sujeto") == null || xmlClasifSujeto13.FirstChild().GetAttr("oi_clasif_sujeto") == "")
        {
          xmlError.SetAttr("error", "No se puede crear la tarea adicional porque no existe el sujeto " + objUbicacion.c_ubicacion + " (" + objUbicacion.d_ubicacion + ") o la clasificación SUC para el mismo.");
          return xmlError;
        }

        //Crear la tarea adicional de tipo 13
        NucleusRH.Base.Legales.Tareas.TAREA objTarea13 = new NucleusRH.Base.Legales.Tareas.TAREA();
        objTarea13.oi_carpeta = objPedidoLF.oi_carpeta;
        objTarea13.oi_estado_tarea = xmlEstadoTarea1.FirstChild().GetAttr("oi_estado_tarea");
        objTarea13.oi_tipo_tarea = xmlTipoTarea13.FirstChild().GetAttr("oi_tipo_tarea");
        objTarea13.f_alta = DateTime.Now;
        objTarea13.f_tarea = DateTime.Now;
        objTarea13.f_vto_tarea = DateTime.Now.AddDays(1);
        objTarea13.d_tarea = "Adjuntar telegrama por acuerdo con renuncia";
        objTarea13.oi_clasif_sujeto = xmlClasifSujeto13.FirstChild().GetAttr("oi_clasif_sujeto");

        //Guardar la tarea adicional de tipo 13
        try
        {
          objTran.Begin();
          objTran.Save(objTarea13);
          objTran.Commit();
        }
        catch (Exception)
        {
          objTran.Rollback();
          throw;
        }
      }

      //Crear la tarea asociada de tipo 12
      NucleusRH.Base.Legales.Tareas.TAREA objTarea12 = new NucleusRH.Base.Legales.Tareas.TAREA();
      objTarea12.oi_carpeta = objPedidoLF.oi_carpeta;
      objTarea12.oi_estado_tarea = xmlEstadoTarea1.FirstChild().GetAttr("oi_estado_tarea");
      objTarea12.oi_tipo_tarea = xmlTipoTarea12.FirstChild().GetAttr("oi_tipo_tarea");
      objTarea12.f_alta = DateTime.Now;
      objTarea12.f_tarea = DateTime.Now;
      objTarea12.f_vto_tarea = DateTime.Now.AddDays(5);
      string monto = Convert.ToDouble(objPedidoLF.n_monto_total).ToString("F2", System.Globalization.CultureInfo.CreateSpecificCulture("es-ES"));
      objTarea12.d_tarea = "Pedido de Liquidación Final " + objPedidoLF.c_pedido_lf + " Monto: $" + monto;
      objTarea12.oi_clasif_sujeto = xmlClasifSujeto12.FirstChild().GetAttr("oi_clasif_sujeto");

      //Guardar la tarea asociada de tipo 12
      try
      {
        objTran.Begin();
        objTran.SaveRefresh(objTarea12);
        objTran.Commit();
      }
      catch (Exception)
      {
        objTran.Rollback();
        throw;
      }

      objPedidoLF.oi_tarea = objTarea12.id.ToString();

      //Guardar el pedido de LF
      try
      {
        objTran.Begin();
        objTran.Save(objPedidoLF);
        objTran.Commit();
      }
      catch (Exception)
      {
        objTran.Rollback();
        throw;
      }

      //Guardar el documento de la tarea asociada de tipo 12
      SAVE_DOCUMENTO(objPedidoLF.id.ToString());
    }

    return xmlError;
    }

  /*
  public static NomadXML APROBAR_PEDIDO_LF(string oi_pedido_lf, string observaciones)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("----------APROBAR_PEDIDO_LF----------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

    NomadTransaction objTran = new NomadTransaction();
    NomadXML xmlError = new NomadXML("ERROR");
        NomadXML xmlParam;

        //Recuperar el pedido de LF
    NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF objPedidoLF = NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Get(oi_pedido_lf);

    //Cambiar estado del pedido a Aprobado y agregar fecha, usuario y observaciones
        objPedidoLF.c_estado = "AP";
        objPedidoLF.f_aprueba = DateTime.Now;
    objPedidoLF.c_usr_aprueba = NomadProxy.GetProxy().UserEtty;
    objPedidoLF.o_aprueba = observaciones;

        //Validar si ya existe una tarea de tipo 13 para la carpeta
        xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_tipo_tarea", "13");
        xmlParam.SetAttr("oi_carpeta", objPedidoLF.oi_carpeta);
        NomadXML xmlTarea13 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Tareas.TAREA.Resources.QRY_TAREA, xmlParam.ToString());

    //Recuperar el OI del estado de tarea 1-A Completar
    xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_estado_tarea", "1");
    NomadXML xmlEstadoTarea1 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Estados_Tarea.ESTADO_TAREA.Resources.QRY_EST_TAREA, xmlParam.ToString());

        if (xmlTarea13.FirstChild().GetAttr("oi_tarea") == null || xmlTarea13.FirstChild().GetAttr("oi_tarea") == "")
        {
            //Validar que exista el tipo de tarea 13
            xmlParam = new NomadXML("DATA");
            xmlParam.SetAttr("c_tipo_tarea", "13");
            NomadXML xmlTipoTarea13 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Tipos_Tarea.TIPO_TAREA.Resources.QRY_TIPO_TAREA, xmlParam.ToString());
            if (xmlTipoTarea13.FirstChild().GetAttr("oi_tipo_tarea") == null || xmlTipoTarea13.FirstChild().GetAttr("oi_tipo_tarea") == "")
            {
                xmlError.SetAttr("error", "No se puede crear la tarea adicional porque no existe el tipo de tarea 13-Adjuntar Telegrama.");
                return xmlError;
            }

            //Validar que exista la sucursal de la carpeta como sujeto con clasificacion SUC
            NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(objPedidoLF.oi_carpeta);
            NucleusRH.Base.Organizacion.Empresas.UBICACION objUbicacion = NucleusRH.Base.Organizacion.Empresas.UBICACION.Get(objCarpeta.oi_ubicacion);
            xmlParam = new NomadXML("DATA");
            xmlParam.SetAttr("c_sujeto", objUbicacion.c_ubicacion);
            xmlParam.SetAttr("c_clasif_suj", "SUC");
            NomadXML xmlClasifSujeto13 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Sujetos.CLASIF_SUJETO.Resources.QRY_CLASIF_SUJETO, xmlParam.ToString());
            if (xmlClasifSujeto13.FirstChild().GetAttr("oi_clasif_sujeto") == null || xmlClasifSujeto13.FirstChild().GetAttr("oi_clasif_sujeto") == "")
            {
                xmlError.SetAttr("error", "No se puede crear la tarea adicional porque no existe el sujeto " + objUbicacion.c_ubicacion + " (" + objUbicacion.d_ubicacion + ") o la clasificación SUC para el mismo.");
                return xmlError;
            }

            //Crear la tarea adicional de tipo 13
            NucleusRH.Base.Legales.Tareas.TAREA objTarea13 = new NucleusRH.Base.Legales.Tareas.TAREA();
            objTarea13.oi_carpeta = objPedidoLF.oi_carpeta;
            objTarea13.oi_estado_tarea = xmlEstadoTarea1.FirstChild().GetAttr("oi_estado_tarea");
            objTarea13.oi_tipo_tarea = xmlTipoTarea13.FirstChild().GetAttr("oi_tipo_tarea");
            objTarea13.f_alta = DateTime.Now;
            objTarea13.f_tarea = DateTime.Now;
            objTarea13.f_vto_tarea = DateTime.Now.AddDays(1);
            objTarea13.d_tarea = "Adjuntar telegrama por acuerdo con renuncia";
            objTarea13.oi_clasif_sujeto = xmlClasifSujeto13.FirstChild().GetAttr("oi_clasif_sujeto");

            //Guardar la tarea adicional de tipo 13
            try
            {
                objTran.Begin();
                objTran.Save(objTarea13);
                objTran.Commit();
            }
            catch (Exception)
            {
                objTran.Rollback();
                throw;
            }
        }

        //Validar que exista el tipo de tarea 12
        xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_tipo_tarea", "12");
        NomadXML xmlTipoTarea12 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Tipos_Tarea.TIPO_TAREA.Resources.QRY_TIPO_TAREA, xmlParam.ToString());
        if (xmlTipoTarea12.FirstChild().GetAttr("oi_tipo_tarea") == null || xmlTipoTarea12.FirstChild().GetAttr("oi_tipo_tarea") == "")
        {
            xmlError.SetAttr("error", "No se puede crear la tarea asociada porque no existe el tipo de tarea 12-Pedido de Liquidación Final.");
            return xmlError;
        }

        //Validar que exista el sujeto GRU_LIQ con clasificacion PIN
        xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_sujeto", "GRU_LIQ");
        xmlParam.SetAttr("c_clasif_suj", "PIN");
        NomadXML xmlClasifSujeto12 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Sujetos.CLASIF_SUJETO.Resources.QRY_CLASIF_SUJETO, xmlParam.ToString());
        if (xmlClasifSujeto12.FirstChild().GetAttr("oi_clasif_sujeto") == null || xmlClasifSujeto12.FirstChild().GetAttr("oi_clasif_sujeto") == "")
        {
            xmlError.SetAttr("error", "No se puede crear la tarea asociada porque no existe el sujeto GRU_LIQ o la clasificación PIN para el mismo.");
            return xmlError;
        }

        //Crear la tarea asociada de tipo 12
        NucleusRH.Base.Legales.Tareas.TAREA objTarea12 = new NucleusRH.Base.Legales.Tareas.TAREA();
        objTarea12.oi_carpeta = objPedidoLF.oi_carpeta;
        objTarea12.oi_estado_tarea = xmlEstadoTarea1.FirstChild().GetAttr("oi_estado_tarea");
        objTarea12.oi_tipo_tarea = xmlTipoTarea12.FirstChild().GetAttr("oi_tipo_tarea");
        objTarea12.f_alta = DateTime.Now;
        objTarea12.f_tarea = DateTime.Now;
        objTarea12.f_vto_tarea = DateTime.Now.AddDays(5);
        string monto = Convert.ToDouble(objPedidoLF.n_monto_total).ToString("F2", System.Globalization.CultureInfo.CreateSpecificCulture("es-ES"));
        objTarea12.d_tarea = "Pedido de Liquidación Final " + objPedidoLF.c_pedido_lf + " Monto: $" + monto;
        objTarea12.oi_clasif_sujeto = xmlClasifSujeto12.FirstChild().GetAttr("oi_clasif_sujeto");

        //Guardar la tarea asociada de tipo 12
        try
        {
            objTran.Begin();
            objTran.SaveRefresh(objTarea12);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }

    objPedidoLF.oi_tarea = objTarea12.id.ToString();

        //Guardar el pedido de LF
        try
        {
            objTran.Begin();
            objTran.Save(objPedidoLF);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }

    //Guardar el documento de la tarea asociada de tipo 12
        SAVE_DOCUMENTO(objPedidoLF.id.ToString());

    return xmlError;
    }
  */

  //RECHAZAR PEDIDO DE LIQUIDACION FINAL MASIVO
  public static void RECHAZAR_PEDIDO_LF(NomadXML pedidos, string observaciones)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("---------RECHAZAR_PEDIDO_LF----------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

    NomadTransaction objTran = new NomadTransaction();

    foreach (NomadXML pedido in pedidos.FirstChild().GetChilds())
        {
      //Recuperar el pedido de LF
      string oi_pedido_lf = pedido.GetAttr("oi_pedido_lf");
      NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF objPedidoLF = NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Get(oi_pedido_lf);

      //Si el pedido de LF esta Aprobado, cambiar el estado de la tarea asociada a Cancelada
      if (objPedidoLF.c_estado == "AP")
      {
        //Recuperar el OI del estado de tarea 4-Cancelada
        NomadXML xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_estado_tarea", "4");
        NomadXML xmlEstadoTarea4 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Estados_Tarea.ESTADO_TAREA.Resources.QRY_EST_TAREA, xmlParam.ToString());

        NucleusRH.Base.Legales.Tareas.TAREA objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(objPedidoLF.oi_tarea);
        objTarea.oi_estado_tarea = xmlEstadoTarea4.FirstChild().GetAttr("oi_estado_tarea");

        //Guardar la tarea asociada
        try
        {
          objTran.Begin();
          objTran.Save(objTarea);
          objTran.Commit();
        }
        catch (Exception)
        {
          objTran.Rollback();
          throw;
        }
      }

      //Cambiar estado del pedido de LF a Rechazado y agregar fecha, usuario y observaciones
      objPedidoLF.c_estado = "R";
      objPedidoLF.f_rechaza = DateTime.Now;
      objPedidoLF.c_usr_rechaza = NomadProxy.GetProxy().UserEtty;
      objPedidoLF.o_rechaza = observaciones;

      //Guardar el pedido de LF
      try
      {
        objTran.Begin();
        objTran.Save(objPedidoLF);
        objTran.Commit();
      }
      catch (Exception)
      {
        objTran.Rollback();
        throw;
      }
    }
    }

  /*
   public static void RECHAZAR_PEDIDO_LF(string oi_pedido_lf, string observaciones)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("---------RECHAZAR_PEDIDO_LF----------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

    NomadTransaction objTran = new NomadTransaction();

    //Recuperar el pedido de LF
        NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF objPedidoLF = NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Get(oi_pedido_lf);

    //Si el pedido de LF esta Aprobado, cambiar el estado de la tarea asociada a Cancelada
    if (objPedidoLF.c_estado == "AP")
    {
      //Recuperar el OI del estado de tarea 4-Cancelada
      NomadXML xmlParam = new NomadXML("DATA");
      xmlParam.SetAttr("c_estado_tarea", "4");
      NomadXML xmlEstadoTarea4 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Estados_Tarea.ESTADO_TAREA.Resources.QRY_EST_TAREA, xmlParam.ToString());

      NucleusRH.Base.Legales.Tareas.TAREA objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(objPedidoLF.oi_tarea);
      objTarea.oi_estado_tarea = xmlEstadoTarea4.FirstChild().GetAttr("oi_estado_tarea");

      //Guardar la tarea asociada
      try
      {
        objTran.Begin();
        objTran.Save(objTarea);
        objTran.Commit();
      }
      catch (Exception)
      {
        objTran.Rollback();
        throw;
      }
    }

    //Cambiar estado del pedido de LF a Rechazado y agregar fecha, usuario y observaciones
        objPedidoLF.c_estado = "R";
        objPedidoLF.f_rechaza = DateTime.Now;
    objPedidoLF.c_usr_rechaza = NomadProxy.GetProxy().UserEtty;
    objPedidoLF.o_rechaza = observaciones;

        //Guardar el pedido de LF
        try
        {
            objTran.Begin();
            objTran.Save(objPedidoLF);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }
    }
  */

    public static void CUMPLIR_PEDIDO_LF(string oi_tarea, string n_recibo)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("----------CUMPLIR_PEDIDO_LF----------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

    NomadTransaction objTran = new NomadTransaction();

        //Recuperar el pedido de LF
        NomadXML xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("oi_tarea", oi_tarea);
        NomadXML xmlPedidoLF = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Resources.getPedidoLFxTarea, xmlParam.ToString());
        string oi_pedido_lf = xmlPedidoLF.FirstChild().GetAttr("oi_pedido_lf");
        NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF objPedidoLF = NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Get(oi_pedido_lf);

        //Cambiar el estado a Cumplido y agregar fecha, usuario y numero de recibo
        objPedidoLF.c_estado = "C";
        objPedidoLF.f_cumple = DateTime.Now;
    objPedidoLF.c_usr_cumple = NomadProxy.GetProxy().UserEtty;
    objPedidoLF.n_recibo = n_recibo;

        //Guardar el pedido de LF
        try
        {
            objTran.Begin();
            objTran.Save(objPedidoLF);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }
    }

  public static void CUMPLIR_TAREA(string oi_tarea, string n_recibo, string observaciones)
    {
        NomadTransaction objTran = new NomadTransaction();

    //Recuperar la tarea
        NucleusRH.Base.Legales.Tareas.TAREA objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(oi_tarea);

    //Recuperar el OI del estado de tarea 3-Cumplida
    NomadXML xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_estado_tarea", "3");
    NomadXML xmlEstadoTarea3 = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Estados_Tarea.ESTADO_TAREA.Resources.QRY_EST_TAREA, xmlParam.ToString());

    //Cambiar el estado de la tarea a Cumplida y agregar observaciones
        objTarea.oi_estado_tarea = xmlEstadoTarea3.FirstChild().GetAttr("oi_estado_tarea");
        objTarea.o_tarea = observaciones;

    //Guardar la tarea
    try
    {
      objTran.Begin();
      objTran.Save(objTarea);
      objTran.Commit();
    }
    catch (Exception)
    {
      objTran.Rollback();
      throw;
    }

    //Cumplir el pedido de LF
    CUMPLIR_PEDIDO_LF(oi_tarea, n_recibo);
    }

  public static Nomad.NSystem.Proxy.NomadXML GET_RECIBOS(string oi_tarea)
    {
    //Recuperar tarea
    NucleusRH.Base.Legales.Tareas.TAREA objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(oi_tarea);

    //Recuperar carpeta
    NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(objTarea.oi_carpeta);

    //Obtener recibos
    NomadXML xmlParam = new NomadXML("DATA");
    xmlParam.SetAttr("oi_carpeta", objTarea.oi_carpeta);
	DateTime fecha_tope = objTarea.f_tarea.AddDays(-30);
    xmlParam.SetAttr("fecha_tope", fecha_tope.ToString("yyyyMMdd"));
    if (objCarpeta.d_custom_1 == "COL") xmlParam.SetAttr("colaborador", "1");
    NomadXML xmlRecibos = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Resources.getRecibos, xmlParam.ToString());

    return xmlRecibos;
    }

    public static Nomad.NSystem.Proxy.NomadXML GENERATE_XML_REPORT(string oi_pedido_lf)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("--------GENERATE_XML_REPORT----------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

        NomadXML xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("oi_pedido_lf", oi_pedido_lf);

        NomadXML xmlImagen = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Resources.getImagenEmpresa, xmlParam.ToString());
        string oi_imagen = xmlImagen.FirstChild().GetAttr("oi_imagen");

        string strFileName = "";
        NomadProxy objProxy = NomadProxy.GetProxy();

        if (!System.IO.File.Exists(objProxy.RunPath + "Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + oi_imagen + ".jpg"))
        {
            //Si no tiene imagen, devuelvo el XML como estaba
            if (oi_imagen == "")
            {
                return xmlParam;
            }

            BINFile objFile;
            const string strClase = "NucleusRH.Base.Personal.Imagenes.HEAD";

            objFile = objProxy.BINService().GetFile(strClase, oi_imagen);

            strFileName = objFile.SaveFile(objProxy.RunPath + "Nomad\\TEMP");
        }
        else
        {
            strFileName = objProxy.RunPath + "Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + oi_imagen + ".jpg";
        }
        xmlParam.SetAttr("ruta", strFileName);

        return xmlParam;
    }

    public static void SAVE_DOCUMENTO(string oi_pedido_lf)
    {
        NomadEnvironment.GetTrace().Info("-------------------------------------");
        NomadEnvironment.GetTrace().Info("-----------SAVE_DOCUMENTO------------");
        NomadEnvironment.GetTrace().Info("-------------------------------------");

        //Ejecutar el reporte
        NomadXML xmlParam = GENERATE_XML_REPORT(oi_pedido_lf);
        Nomad.NmdFoRender objout = new Nomad.NmdFoRender("NucleusRH.Base.Legales.PedidoLiqFinal.rpt", xmlParam);

        //Crear el Archivo de Salida
        string outFileName = "DocTareaPedidoLF_" + oi_pedido_lf + ".pdf";
        string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";

        //Generar el Documento PDF
        objout.Generate(outFilePath + "\\" + outFileName, Nomad.NmdFoRenderFormat.RENDER_PDF);

        //Nuevo documento digital a partir de PDF generado
        NucleusRH.Base.Legales.DocumentosDigitales.HEAD objDocDig = new NucleusRH.Base.Legales.DocumentosDigitales.HEAD();
        objDocDig.FILE = outFileName;

        //Leer el archivo y convertir a string base 64
        byte[] objFileByte = File.ReadAllBytes(outFilePath + "\\" + outFileName);
        string objFile = System.Convert.ToBase64String(objFileByte);
        int index = 0;

        //Agregar BINS
        for (int i = 0; i < objFile.Length; i = i + 4000)
        {
            NucleusRH.Base.Legales.DocumentosDigitales.BIN objBin = new NucleusRH.Base.Legales.DocumentosDigitales.BIN();

            if ((objFile.Length - i ) > 4000)
                objBin.DATA = objFile.Substring(i, 4000);
            else
                objBin.DATA = objFile.Substring(i);

            objBin.POS = index;
            index++;

            objDocDig.BINS.Add(objBin);
        }

        objDocDig.CREATE = DateTime.Now;
        objDocDig.SIZE = (objFile.Length / 4) * 3;

        NomadTransaction objTran = new NomadTransaction();

        //Guardar documento digital
        try
        {
            objTran.Begin();
            objTran.SaveRefresh(objDocDig);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }

        //Recuperar el pedido de LF
        NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF objPedidoLF = NucleusRH.Base.Legales.PedidosLiqFinal.PEDIDO_LF.Get(oi_pedido_lf);

    //Nuevo documento
        NucleusRH.Base.Legales.Documentos.DOCUMENTO objDoc = new NucleusRH.Base.Legales.Documentos.DOCUMENTO();
        objDoc.oi_carpeta = objPedidoLF.oi_carpeta;
        objDoc.oi_tarea = objPedidoLF.oi_tarea;
        objDoc.oi_doc_digital = objDocDig.id.ToString();
        objDoc.d_doc_dig = "Reporte de Pedido de Liquidación Final";
        objDoc.f_doc_dig = objDocDig.CREATE;

        //Guardar documento
        try
        {
            objTran.Begin();
            objTran.Save(objDoc);
            objTran.Commit();
        }
        catch (Exception)
        {
            objTran.Rollback();
            throw;
        }
    }

  }

}



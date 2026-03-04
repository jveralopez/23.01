using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Liquidacion.InterfaceDistribucionCentroCosto;
using NucleusRH.Base.Liquidacion.Legajo_Liquidacion;
using NucleusRH.Base.Organizacion.Centros_de_Costos;
using System.Text.RegularExpressions;

namespace NucleusRH.Base.Liquidacion_DCC
{

  class clsImportarDCC
  {

    private Hashtable htaOIs = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public clsImportarDCC()
    {
    }

    /// <summary>
    /// Importa la distribucion de centros de costos a los legajos liquidación
    /// </summary>
    /// <param name="pstrOIEmpresa">OI de la Empresa</param>
    public void Importar(string pstrOIEmpresa)
    {
      Hashtable htaCentosCostos = new Hashtable(); //Contendrá los objetos de Centos de Costos
      Hashtable htaPersonalLiqs = new Hashtable(); //Contendrá los objetos PersonalLiq
      Hashtable htaPersonalDelete = new Hashtable(); //Contendrá los objetos PersonalLiq a eliminar todos los registros de Dist de CC
      int intItems;
      int intCurrentItem, intCurrentItem2;
      string strOIPersonalLiq;
      int intInserts = 0;
      int intDeletes = 0;

      Regex rgx1 = new Regex(@"^[0-9]+$"); //Valida que sea un numero entero
      Regex rgx2 = new Regex(@"[0-9]+\.[0-9]+"); //Valida que sea un numero demcimal con . decimal

      NomadXML xmlOIItems;
      NomadBatch objBatch = NomadBatch.GetBatch("Importar DCC", "Importar DCC");

      ENTRADA objInterfazDCC;
      PERSONAL_EMP objPersonalEmp = null;
      DISTRIB_COS objDCCPersona;

      NomadBatch.Trace("--------------------------------------------------------------------------");
      NomadBatch.Trace(" Comienza Importar Distribucion de CC ------------------------------------");
      NomadBatch.Trace("--------------------------------------------------------------------------");

      objBatch.SetPro(0);
      objBatch.SetMess("Importando la Distribucion de Centros de Costos.");
      objBatch.Log("Comienza la importación");

      //Obtiene todos los Centos de Costos posibles a importar
      htaCentosCostos = NomadEnvironment.QueryHashtable(CENTRO_COSTO.Resources.QRY_CC, "", "cod");

      //Obtiene la lista de OIs de la interface de Distribucion de CC para LegajosLiquidación
      xmlOIItems = NomadEnvironment.QueryNomadXML(ENTRADA.Resources.QRY_OIArchivo, "");
      xmlOIItems = xmlOIItems.FirstChild();

      //Recorre el archivo y pide los DDO de interface
      objBatch.SetPro(10);
      intItems = xmlOIItems.ChildLength;
      intCurrentItem = 0;
      intCurrentItem2 = 0;

      //Recorre los registros de entrada para marcar cuales tiene que borrarle valores de la dependencia
      for (NomadXML xmlRowOI = xmlOIItems.FirstChild(); xmlRowOI != null; xmlRowOI = xmlRowOI.Next())
      {
          intCurrentItem2++;

          objInterfazDCC = ENTRADA.Get(xmlRowOI.GetAttr("id"));

          //Valida el contenido del registro
          NomadBatch.Trace("Validando valores a Eliminar");

          //Si solo tiene valor el campo del numero del legajo - entonces tengo que eliminar la dependencia de distribucion de CC del mismo
          if (objInterfazDCC.e_numero_legajo.ToString() != "0" && string.IsNullOrEmpty(objInterfazDCC.c_ccosto.ToString()) && string.IsNullOrEmpty(objInterfazDCC.n_p_distrib.ToString()) && string.IsNullOrEmpty(objInterfazDCC.o_distrib_cos.ToString()))
          {
                //Obtiene el PersonalLiq desde la hash o desde un Get ----------------------------------------
                NomadBatch.Trace("Obteniendo el OI Personal_Emp");
                if (!htaPersonalDelete.ContainsKey(objInterfazDCC.e_numero_legajo.ToString()))
                {
                    //El personal Liq todavia no existe en la hash - se agrega solo una vez

                    //Obtiene el PersonalLiq desde la DB y lo guarda en la hash
                    strOIPersonalLiq = this.GetOIPersonal(pstrOIEmpresa, objInterfazDCC.e_numero_legajo.ToString());

                    //Valida que el legajo exista en la empresa
                    if (strOIPersonalLiq == "")
                    {
                        objBatch.Err("El Legajo al que se quiere eliminar los registros de distribucion de CC, no existe en la Empresa, se rechaza el registro con numero de Legajo: '" + objInterfazDCC.e_numero_legajo + " - Línea: '" + intCurrentItem2.ToString() + "'");
                        continue;
                    }
                    else
                    {
                        //Recupera una unica vez de la BD - Luego lo va a ir recuperando de la hashtable
                        objPersonalEmp = PERSONAL_EMP.Get(strOIPersonalLiq);
                        ArrayList dd = new ArrayList();

                        //Recorro el child de Distribuciones de CC del legajo para limpiar la coleccion
                        foreach (DISTRIB_COS DISTRIB in objPersonalEmp.DISTRIB_COS)
                        {
                            dd.Add(DISTRIB);
                        }
                        foreach (DISTRIB_COS DISTRIB in dd)
                            objPersonalEmp.DISTRIB_COS.Remove(DISTRIB);

                        //Se agrega el legajo a la lista de legajos a realizar el save - del delete de la dependencia
                        htaPersonalDelete.Add(objInterfazDCC.e_numero_legajo.ToString(), objPersonalEmp);
                    }
                }
              else
                {
                    continue;
                }
          }
          else
          {
              continue;
          }
      }

      //Actualiza los que no tiene que borrar
      for (NomadXML xmlRowOI = xmlOIItems.FirstChild(); xmlRowOI != null; xmlRowOI = xmlRowOI.Next())
      {
        objInterfazDCC = ENTRADA.Get(xmlRowOI.GetAttr("id"));
        intCurrentItem++;
        int Errores = 0; //para la linea en particular - individual
        double porc_valido; //para la linea en particular - individual

        objBatch.SetMess("Importando el registro '" + intCurrentItem.ToString() + "' de '" + intItems.ToString() + "'.");

        //Valida el contenido del registro
        NomadBatch.Trace("Validando valores");

        //Valido atributos obligatorios
        if (objInterfazDCC.e_numero_legajo.ToString() == "0")
        {
            objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro - Línea: '" + intCurrentItem.ToString() + "'");
            Errores++;
            continue;
        }

      //Si el legajo no existe en la hash a eliminar - sigo con el proceso
      if (!htaPersonalDelete.ContainsKey(objInterfazDCC.e_numero_legajo.ToString()))
      {
        if (objInterfazDCC.c_ccosto.ToString() == "")
        {
            objBatch.Err("No se especificó el Código del Centro de Costo, se rechaza el registro - Línea: '" + intCurrentItem.ToString() + "' - Legajo: '" + objInterfazDCC.e_numero_legajo + "'");
            Errores++;
            continue;
        }

        //Valida que sea un "numero entero ó un numero con punto (.) decimal" - sin separador coma (,) - sin caracteres especiales - ni nulo/vacio
        if (rgx1.IsMatch(objInterfazDCC.n_p_distrib.ToString()) || rgx2.IsMatch(objInterfazDCC.n_p_distrib.ToString()))
        {
            porc_valido = Nomad.NSystem.Functions.StringUtil.str2dbl(objInterfazDCC.n_p_distrib.ToString());
            //Validar por Cero
            if (porc_valido == 0)
            {
                objBatch.Err("El valor del porcentaje no puede ser igual a cero - Se rechaza el registro - Línea: '" + intCurrentItem.ToString() + "' - Legajo: '" + objInterfazDCC.e_numero_legajo + "'");
                Errores++;
                continue;
            }
        }
        else
        {
            objBatch.Err("El porcentaje no tiene el formato correcto - Se rechaza el registro - Línea: '" + intCurrentItem.ToString() + "' - Legajo: '" + objInterfazDCC.e_numero_legajo + "'");
            Errores++;
            continue;
        }

        //Recupero el OI del codigo de CC ingresado si existe
        string oiCC = "";
        if (htaCentosCostos.ContainsKey(objInterfazDCC.c_ccosto))
        {
          oiCC = ((NomadXML)htaCentosCostos[objInterfazDCC.c_ccosto]).GetAttr("oi");
        }
        else
        {
            objBatch.Err("El código de Centro de Costo ingresado no existe, se rechaza el registro - Línea: '" + intCurrentItem.ToString() + "' Código Centro de Costo: '" + objInterfazDCC.c_ccosto + "' - Legajo: '" + objInterfazDCC.e_numero_legajo + "'");
            Errores++;
            continue;
        }

        //Obtiene el PersonalLiq desde la hash o desde un Get ----------------------------------------
        NomadBatch.Trace("Obteniendo el OI Personal_Emp");
        if (htaPersonalLiqs.ContainsKey(objInterfazDCC.e_numero_legajo.ToString()))
        {
          //Obtiene el PersonalLiq desde la hash
          objPersonalEmp = (PERSONAL_EMP)htaPersonalLiqs[objInterfazDCC.e_numero_legajo.ToString()];
        }
        else
        {
          //Obtiene el PersonalLiq desde la DB y lo guarda en la hash
          strOIPersonalLiq = this.GetOIPersonal(pstrOIEmpresa, objInterfazDCC.e_numero_legajo.ToString());

          //Valida que el legajo exista en la liquidación
          if (strOIPersonalLiq == "")
          {
            objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro con numero de Legajo: '" + objInterfazDCC.e_numero_legajo + "' y Centro de Costo: '" + objInterfazDCC.c_ccosto + "' - Linea: '" + intCurrentItem.ToString() + "'");
            Errores++;
            continue;
          }
          else
          {
            //Recupera una unica vez de la BD - Luego lo va a ir recuperando de la hashtable
            objPersonalEmp = PERSONAL_EMP.Get(strOIPersonalLiq);
            ArrayList dd = new ArrayList();

            //Recorro el child de Distribuciones de CC del legajo para limpiar la coleccion
            foreach (DISTRIB_COS DISTRIB in objPersonalEmp.DISTRIB_COS)
            {
                dd.Add(DISTRIB);
            }
            foreach (DISTRIB_COS DISTRIB in dd)
                objPersonalEmp.DISTRIB_COS.Remove(DISTRIB);
          }
        }

        //Verifica que el registro no tenga errores
        if (Errores == 0)
        {
          objDCCPersona = new DISTRIB_COS();
          objDCCPersona.n_p_distrib = porc_valido;
          objDCCPersona.oi_centro_costo = oiCC;
          objDCCPersona.o_distrib_cos = objInterfazDCC.o_distrib_cos;
          objPersonalEmp.DISTRIB_COS.Add(objDCCPersona);

          //Se agrega el legajo a la lista de legajos a realizar el save
          if (!htaPersonalLiqs.ContainsKey(objInterfazDCC.e_numero_legajo.ToString()))
            htaPersonalLiqs.Add(objInterfazDCC.e_numero_legajo.ToString(), objPersonalEmp);
        }

      }
      else
      {
          //Si el legajo ya existe en la hash a eliminar - entonces ignoro el registro
          continue;
      }

      }

      objBatch.SetPro(80);

      //Recorrer los legajos guardados en la htaPersonalDelete
      objBatch.Log("Eliminando los datos en la Base de Datos. (" + htaPersonalDelete.Count.ToString() + " legajos)");
      int y = 1;
      foreach (string strLegajo in htaPersonalDelete.Keys)
      {
          objBatch.SetPro(80, 100, htaPersonalDelete.Count, y);
          PERSONAL_EMP objPersona = (PERSONAL_EMP)htaPersonalDelete[strLegajo];
          try
          {
              NomadEnvironment.GetCurrentTransaction().Save(objPersona);
              intDeletes++;
              //continue;
          }
          catch (Exception ex)
          {
              objBatch.Err("No se pudo actualizar el legajo '" + strLegajo + "'. " + ex.Message);
              continue;
          }
          y++;
      }

      //Recorrer los legajos guardados en la htaPersonalLiqs - Si el rdo de la suma del porcentaje es igual a 1 o 0 - guarda el registro
      objBatch.Log("Validando y/o Grabando los datos en la Base de Datos. (" + htaPersonalLiqs.Count.ToString() + " legajos)");
      int x = 1;
      foreach (string strLegajo in htaPersonalLiqs.Keys)
      {
        objBatch.SetPro(80, 100, htaPersonalLiqs.Count, x);

        PERSONAL_EMP objPersona;
        double porcentaje = 0;

        objPersona = (PERSONAL_EMP)htaPersonalLiqs[strLegajo];

        foreach (DISTRIB_COS DISTRIB in objPersona.DISTRIB_COS)
        {
            porcentaje = porcentaje + DISTRIB.n_p_distrib;
        }

        if (porcentaje == 0 || porcentaje == 1)
        {
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(objPersona);
                intInserts++;
            }
            catch (Exception ex)
            {
                if (ex.Message == "DB.SQLSERVER.2627")
                {
                    //Violation of primary key. Handle Exception
                    objBatch.Err("No se pudo actualizar el legajo '" + strLegajo + "' - Existen Codigos de Centros de Costo repetidos en mas de un registro del legajo.");
                }
                else
                {
                    objBatch.Err("No se pudo actualizar el legajo '" + strLegajo + "'. " + ex.Message);
                }
                continue;

            }
            x++;
        }
        else
        {
            objBatch.Err("La distribución de Centros de Costo debe sumar 1, recuerde que 0.5 equivale al 50 por ciento - No se pudo actualizar el legajo: '" + strLegajo + "'");
            continue;
        }
      }

      objBatch.SetPro(100);
      objBatch.Log("La importación terminó correctamente.");
      objBatch.Log("Se actualizaron '" + intInserts.ToString() + "' legajos liquidación.");
      objBatch.Log("Se actualizaron eliminando todos los registros de la distribucion de CC de '" + intDeletes.ToString() + "' legajos liquidación.");
    }

    /// <summary>
    /// Obtiene un hashtable accesible por codigo de varible y retorna el OI.
    /// </summary>
    /// <param name="pstrOiEmpresa">Oi de la empresa del legajo.</param>
    /// <param name="pstrLegajo">Código del legajo.</param>
    /// <returns></returns>
    private string GetOIPersonal(string pstrOiEmpresa, string pstrLegajo)
    {
      if (this.htaOIs == null)
      {
        string strParametros = "<PARAMS oi_empresa=\"" + pstrOiEmpresa + "\" />";
        this.htaOIs = NomadEnvironment.QueryHashtable(ENTRADA.Resources.QRY_IDLegajos, strParametros, "cod");
      }

      //devuelve una coleccion, con todos los legajos de la empresa seleccionada - esto es htaOIs

      return this.htaOIs.ContainsKey(pstrLegajo) ? ((NomadXML)this.htaOIs[pstrLegajo]).GetAttr("oi") : "";
    }

  }
}



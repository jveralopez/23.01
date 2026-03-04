using System;
using System.Xml;
using System.Collections;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Tiempos_Trabajados.FichadasIng;
using NucleusRH.Base.Tiempos_Trabajados.Esperanzaper;
using NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas;
using NucleusRH.Base.Tiempos_Trabajados.Personal;
using NucleusRH.Base.Tiempos_Trabajados.Liquidacion;
using NucleusRH.Base.Tiempos_Trabajados.Esperanza;

/*
Histórico
22/08/2008 - Se elimina la generación de la esperanza desde aqui. Se adaptan los procesos al sistema de "SEMANAS".
05/03/2009 - Se agrega el proceso para el reporte de "Tarjeta del personal".
22/04/2009 - Se agrega el control del parámetro EveES que indica si existen relojes que no distinguen eventos de entrada o salida.
06/05/2009 - Se elimina la validación previa al procesamiento y se agrega una validación sencilla dentro del método de procesamiento.
03/02/2010 - Si el procesamiento es PARCIAL no se toma en cuenta la última fichada en caso de ser entrada (Solo para el procesamiento, para la validación si se toma).
04/02/2010 - Se agrega la bandera "Procesamiento parcial" dentro del hash (La liquidación intenta cerrarla como no parcial).
07/07/2010 - Se modifica el método GetPresence: Cuando se solapan dos horarios se prioriza los horarios "Verdaderos" a los tomados por defecto (por ejemplo cuando no existen detalles para un dia se toma como inicio y fin las 0 horas).
07/07/2010 - Se modifica el método GetPresence: Se agranda en dos días (uno antes y otro después) el query de fichadas a utilizar.
07/09/2010 - Se modifica el método GetHASHProLegajo. Se suma en día a la fecha fin antes de llamar al método CalcularFechasFichadas.
07/09/2010 - Se modifica el método RangeDate se agrega parámetro para indicar si toma todos los tipos de hora o solo los de presencia obligatoria.
29/09/2010 - Se modifica el método CargarRangosFichadas(). Ahora los rangos de tipo rangoindefinido los carga aún si no se controla fichada. Es para que el proceso de clasificación pueda cambiarles es tipo (de I a E/S).
29/09/2010 - Se agrega nuevo parámetro al procesamiento (reprocesar) para que fuerce la generación sin importar si los hash son iguales.
29/09/2010 - Se agrega el array de EsperanzaPer.DIA como parámetro al método ApplyConceptos.
20/10/2010 - Se corrige el método RangeDate. Se calculaba mal la fecha HASTA del rango cuando la fecha de inicio del día posterior se solapa con el día en cuestión.
09/11/2010 - Se compatibiliza el procesamiento y la validación de fichadas con la nueva lista rápida del formulario de filtro.
02/06/2011 - Se Eliminan las jornadas de procesos anteriores cuando a un legajo le falla la validacion ligth
02/06/2011 - Se agrega funcion AddInOuts() para agregar los eventos de entrada y salida utilizados en reportes.
07/06/2011 - Se agrega el Preload de los datos para la generacion de la esperanza
20/09/2011 - Se blanquean los mensajes de error en las personas liquidadas
29/06/2012 - Se agrega un metodo PreValidarLegajo que se llama antes de validar las fichadas.
30/08/2012 - Se agregan nuevas tolerancias de ingreso y egreso para los informes de asistencias
17/09/2012 - Edicion manual del procesamiento
13/12/2012 - La validacion light no omitia la ultima fichada si era parcial
12/05/2014 - En el método LoadLegajos() no se validaba que existan legajos seleccionados en el XMl de parametros que llega desde la pantalla.
*/

namespace NucleusRH.Base.Tiempos_Trabajados {

  public enum Sumarizaciones {NoSumariza=0, Sumariza=1, EnAusenteExedente=2};
  public enum TiposHito      {Fichada=1, Esperanza=2, InicioDia=3};
  public enum TiposHoras     {Normal=1000, Ausencia=1001, Descanso=1002, Excedente=1003};

  /// <summary>
  /// Clase que administra las generaciones de la Esperanza Personal, las validaciones de fichadas y las liquidaciones de horas.
  /// </summary>
  public class Procesos {

    //Variables de uso interno
    private NomadProxy m_Proxy;
    private int intContTotal = 0;
    private int intContErr = 0;

    private NomadBatch objBatch;

    private ArrayList arrIngresos = null;
    private ArrayList arrEgresos = null;
    private ArrayList arrRangos = null;

    private Hashtable m_colEstructs = null;
    private RHLiq.LiqConceptosBase m_ejecConc=null;
    private LIQUIDACION myliq=null;

    public Procesos() {
      m_Proxy = Nomad.NSystem.Proxy.NomadProxy.GetProxy();
    }

    //----------------------------------------------------------------------------------------
    // PROCESOS PRINCIPALES
    //----------------------------------------------------------------------------------------

    /// <summary>
    /// Valida las fichadas ingresadas al sistema
    /// </summary>
    /// <param name="pstrParams">Parámetros de entrada</param>
    /// <param name="pbolPrePro">Indica si la validación la está ejecutndo el Procesamiento</param>
    /// <returns></returns>
    public string ValidarFichadas(string pstrParams, bool pbolPrePro)  {
      clsParams objParams;

      this.objBatch = NomadBatch.GetBatch("ValidacionFichadas", "");

      NomadBatch.Trace("--------------------------------------------------------------------------");
      NomadBatch.Trace(" Comienza Validar Fichadas -----------------------------------------------");
      NomadBatch.Trace("--------------------------------------------------------------------------");

      this.objBatch.SetPro(0);
      this.objBatch.SetMess("Validando las fichadas.");
      this.objBatch.Log("Comienza la validación de fichadas");

      //Crea el objeto de parámetros
      try {
        objParams = new clsParams(pstrParams);
      } catch (Exception ex) {
        throw new Exception("Ocurrió un error cargando los parámetros. " + ex.Message);
      }

      //Recorre los legajos y por cada uno le ejecuta los TRES procesos de validación
      //0 - Pasar las fichadas Indefinidas a Entrada o Salida
      //1 - Validar las fichadas correlativas
      //2 - Validar las fichadas contra la esperanza
      XmlElement xelLegajo;
      objParams.CantLegajos = objParams.Legajos.ChildNodes.Count;

      this.objBatch.Log("Total de legajos a validar: " + objParams.CantLegajos.ToString());
      for (int x = 0; x < objParams.CantLegajos; x++) {
        xelLegajo = (XmlElement) objParams.Legajos.ChildNodes.Item(x);
        this.objBatch.SetPro(10, 90, objParams.CantLegajos, x);
        this.objBatch.SetMess("Validando el legajo " + xelLegajo.GetAttribute("e_numero_legajo") + " (" + ((int)(x + 1)).ToString() + " de " + objParams.CantLegajos.ToString() + " legajos).");
        NomadBatch.Trace("Validando el legajo " + xelLegajo.GetAttribute("e_numero_legajo") + " (OI:" + xelLegajo.GetAttribute("id") + ")");

        try {
          //Ejecuta los procesos de validación para la persona
          ValidarLegajo(objParams, xelLegajo);
        } catch (Exception ex) {
          xelLegajo.SetAttribute("status", "E");
          xelLegajo.SetAttribute("err_desc", ex.Message);

          this.objBatch.Err("Error en el legajo " + xelLegajo.GetAttribute("e_numero_legajo") + ". " + ex.Message);
        }
      }

      //Genera el elemento RESULTADO
      string strResult;
      if (intContTotal == 0) {
        strResult = "<RESULTADO validos=\"0\" error=\"0\"";
      } else {
        double dblPerVal = ((this.intContTotal - this.intContErr) * 100) / this.intContTotal;
        int intPerVal;

        if (dblPerVal > 0 && dblPerVal < 1)
          intPerVal = 1;
        else
          intPerVal = Convert.ToInt32(dblPerVal);

        int intPerErr = 100 - intPerVal;
        strResult = "<RESULTADO validos=\"" + intPerVal.ToString() + "\" error=\"" + intPerErr.ToString() + "\"";
      }
      strResult = strResult + " f_ini=\"" + objParams.FechaInicioORIG.ToString("yyyyMMdd") + "\" f_fin=\"" + objParams.FechaFinORIG.ToString("yyyyMMdd") + "\" />";

      if (pbolPrePro)
        if (this.intContErr != 0) {
          this.objBatch.Wrn("Recuerde que los legajos con error no serán procesadas.");
        }

      strResult =
        "<DATOS>" +
          strResult +
          objParams.Legajos.OuterXml +
        "</DATOS>";

      objParams.FinalizaProceso = DateTime.Now;
      TimeSpan TotalProc = objParams.FinalizaProceso.Subtract(objParams.ComienzaProceso);

      this.objBatch.Log("El proceso de validación de fichadas ha finalizado.");
      this.objBatch.Log("Legajos validados: " + objParams.CantLegajos.ToString());
      this.objBatch.Log("Fichadas validadas: " + objParams.CantFichadas.ToString());
      this.objBatch.Log("Tiempo total: " + TotalProc.Hours.ToString() + ":" + TotalProc.Minutes.ToString() + " (total minutos: " + TotalProc.TotalMinutes.ToString() + ")");

      this.objBatch.SetMess("Validación de fichadas concluida.");
      this.objBatch.SetPro(100);

      NomadBatch.Trace("--------------------------------------------------------------------------");
      NomadBatch.Trace(" El proceso de Validar Fichadas ha finalizado ----------------------------");
      NomadBatch.Trace("--------------------------------------------------------------------------");

      return strResult;
    }

    /// <summary>
    /// Retorna un XML con los datos necesarios para el reporte de Asistnecias.
    /// </summary>
    /// <returns></returns>
    public NomadXML GetPresence(int pOi_personal_emp, DateTime pDesde, DateTime pHasta,
          bool pAddEvents, bool pAddHope, bool pAddRegis, bool pRegisDet,
          bool pAddNews, bool pAddLicences, bool pAddAuthorizedHours) {

      NomadXML xmlResult = new NomadXML("data");
      NomadXML xmlHorario;
      NomadXML xmlHope;
      NomadXML xmlDay = new NomadXML();
      NomadXML xmlEvents;
      NomadXML xmlNotPaired = new NomadXML();
      NomadXML xmlNotPaireds = new NomadXML();
      NomadXML xmlRegis;

      clsParamsA objParams;
      ArrayList arrDays;
      NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA objDay;
      DateTime dteFromRegis = new DateTime();
      DateTime dteToRegis = new DateTime();
      SortedList slsDays = new SortedList();
      clsPuntoA objIn;
      clsPuntoA objOut;
      clsPuntoA objLastOut;
      clsTipoHora objTipoHora;
        Tipos_Horas.TIPOHORA TipoHora = null;

      this.objBatch = NomadBatch.GetBatch("Análisis de Fichadas y Esperanzas", "");

      NomadBatch.Trace("--------------------------------------------------------------------------");
      NomadBatch.Trace(" Comienza el 'Análisis de Fichadas y Esperanzas' -------------------------");
      NomadBatch.Trace("--------------------------------------------------------------------------");

      this.objBatch.SetPro(0);
      this.objBatch.Log("Comienza el 'Análisis de Fichadas y Esperanzas'.");

      this.arrRangos = new ArrayList();

      //Crea el objeto de parámetros
      try {
        objParams = new clsParamsA(pOi_personal_emp, pDesde, pHasta);
      } catch (Exception ex) {
        throw new Exception("Ocurrió un error cargando los parámetros. " + ex.Message);
      }

      //Agrega los datos al elemento DATA
      xmlResult.SetAttr("oi_per", pOi_personal_emp.ToString());
      xmlResult.SetAttr("fd", pDesde.ToString("yyyyMMdd"));
      xmlResult.SetAttr("fh", pHasta.ToString("yyyyMMdd"));

      //Prepara la lista de nodos días
      for (DateTime dteDay = objParams.FechaInicio; dteDay <= objParams.FechaFin; dteDay = dteDay.AddDays(1)) {
        xmlDay = new NomadXML("<dia f=\"" + dteDay.ToString("yyyyMMdd") + "\" />");
        xmlDay = xmlDay.FirstChild();
        xmlResult.AddTailElement(xmlDay);

        if (pRegisDet)
          xmlDay.AddTailElement("total-fichadas");

        if (pAddRegis)
          xmlDay.AddTailElement("fichadas-no-aparejadas");

        slsDays.Add(dteDay.ToString("yyyyMMdd"), xmlDay);
      }

      try {
        //Pide la esperanza del legajo
        NomadProxy.GetProxy().CacheDel("GetHOPEPer");
        GC.Collect();

        arrDays = ESPERANZAPER.GetDaysHope(objParams.FechaInicio, objParams.FechaFin, int.Parse(objParams.Legajo.GetAttr("id")));
      } catch (Exception ex) {
        this.objBatch.Err("No se pudo generar la esperanza para el legajo " + objParams.Legajo.GetAttr("e_numero_legajo") + ".");
        NomadBatch.Trace("Error: " + ex.Message);
        return xmlResult;
      }

      if (arrDays.Count == 0) {
        this.objBatch.Wrn("No se ha encontrado esperanza para el legajo " + objParams.Legajo.GetAttr("e_numero_legajo") + ".");
      }

      //Recorre los DIAS -------------------------------------------------------------------------
      for (int x = 0; x < arrDays.Count; x++) {
        objDay = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA) arrDays[x];

        //Recupera el nodo día y completa los valores
        xmlDay = (NomadXML) slsDays[objDay.f_dia.ToString("yyyyMMdd")];
        xmlDay.SetAttr("tipo", objDay.c_tipo);
        xmlDay.SetAttr("posicion", objDay.e_posicion);
        xmlDay.SetAttr("oi_calendario", objDay.oi_calendario.ToString());
        xmlDay.SetAttr("oi_turno", objDay.oi_turno.ToString());
        xmlDay.SetAttr("oi_escuadra", objDay.oi_escuadra.ToString());
        xmlDay.SetAttr("oi_licencia", objDay.oi_licencia.ToString());
        xmlDay.SetAttr("afectado_nov", objDay.l_afectado_nov ? "1" : "0");

        //Obtiene los datos del horário para el día
        xmlHorario = objParams.GetHorario(objDay.oi_horario);
        xmlDay.SetAttr("oi_horario", objDay.oi_horario.ToString());
        xmlDay.SetAttr("tipo_horario", xmlHorario.GetAttr("d_tipohorario"));

        dteFromRegis = objDay.f_dia;
        dteToRegis = objDay.f_dia.AddDays(1d);
        if (xmlHorario.GetAttr("d_tipohorario") != "L") {
          //Los rangos solo se calculan cuando son horarios R o T. En caso de ser libre quedan de 0 a 0 horas.
          int PreIng = objParams.PreIng;
          int PostEgr = objParams.PostEgr;

          if(objParams.PreIng2 > PreIng)
            PreIng = objParams.PreIng2;
          if(objParams.PostEgr2 > PostEgr)
            PostEgr = objParams.PostEgr2;

      Procesos.RangeDate(objParams, objDay.f_dia, pOi_personal_emp.ToString(), ref dteFromRegis, ref dteToRegis, PostEgr, PreIng);
        }
        xmlDay.SetAttr("ffrom", dteFromRegis.ToString("yyyyMMddHHmmss"));
        xmlDay.SetAttr("fto", dteToRegis.ToString("yyyyMMddHHmmss"));

        //Pregunta si se quiere agregar la esperanza
        if (pAddHope) {
          xmlHope = xmlDay.AddTailElement("esp");
            foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDet in objDay.DETALLE) 
            {
                NomadXML objDetXML = new NomadXML(objDet.ToString()).FirstChild();

                TipoHora = GetTipoHora(objDet.oi_tipohora);

                if(TipoHora.l_obligatorio)
                    objDetXML.SetAttr("obligatorio","1");
                else
                    objDetXML.SetAttr("obligatorio", "0");
                xmlHope.AddTailElement(objDetXML);
            }
        }

        //Pregunta si se quiere agregar los eventos
        if (pAddEvents) {

          //Crea el elemento "eventos"
          xmlEvents = xmlDay.AddTailElement("eventos");

          //Determina si el horario es de tipo LIBRE
          xmlHorario = objParams.GetHorario(objDay.oi_horario);

          objIn  = null;
          objOut  = null;
          objLastOut = null;

          // - Si el horario es L se crea un Punto que abarque todo el día y se lo agrega en Entrada y Salida
          // - Si el horario es R o T se analizan los detalles para agregar los puntos a las Entradas y las Salidas
          if (xmlHorario.GetAttr("d_tipohorario") == "L") {

            //Crea los eventos y los agrega pero solo los agregará si es obligatoria que vaya en el día
            objIn = new clsPuntoA(objDay.f_dia, 0, "l", 0, ((24 * 60) - 1));
            objOut  = new clsPuntoA(objDay.f_dia, ((24 * 60) - 1), "l", (24 * 60) - 1, 0);

            //Recorre el DETALLE de cada uno de los DIAS
            foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDetalle in objDay.DETALLE) {

              if (!objDetalle.l_autorizada) continue;

              //Verifica si el tipo de hora es de presencia obligatoria.
              objTipoHora = objParams.GetTipoHora(objDetalle.oi_tipohora);

              if ( objTipoHora.EnAusencia == Sumarizaciones.EnAusenteExedente ) {
                //Es de presencia obligatoria
                //Indica en el elemento "dia" que es obligatória la presencia
                ((NomadXML) slsDays[objDay.f_dia.ToString("yyyyMMdd")]).SetAttr("esp", "1");

                this.arrRangos.Add(objIn);
                this.arrRangos.Add(objOut);

                break;
              }
            }

          } else {

            int eveOb = 0;
      //Recorre el DETALLE para saber cuantos rangos obligatorios tiene el dia
      foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDetalle in objDay.DETALLE) {
              if (!objDetalle.l_autorizada) continue;
              //Verifica si el tipo de hora es de presencia obligatoria.
              objTipoHora = objParams.GetTipoHora(objDetalle.oi_tipohora);
              if (objTipoHora.EnAusencia == Sumarizaciones.EnAusenteExedente)
        eveOb++;
      }

      //Recorre el DETALLE de cada uno de los DIAS
            int eveC = 0;
      foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDetalle in objDay.DETALLE) {
              if (!objDetalle.l_autorizada) continue;

              //Verifica si el tipo de hora es de presencia obligatoria.
              objTipoHora = objParams.GetTipoHora(objDetalle.oi_tipohora);

              if (objTipoHora.EnAusencia == Sumarizaciones.EnAusenteExedente) {
                //Es de presencia obligatoria
                ((NomadXML) slsDays[objDay.f_dia.ToString("yyyyMMdd")]).SetAttr("esp", "1");

                //Crea la entrada
        if(eveOb == 1)
        {
          objIn = new clsPuntoA(objDay.f_dia, objDetalle.e_horainicio, "e", objParams.PreIng, objParams.PostIng);
          objOut  = new clsPuntoA(objDay.f_dia, objDetalle.e_horafin, "s", objParams.PreEgr2, objParams.PostEgr2);
        }
        else if(eveC == 0)
                {
                  eveC++;
                  objIn = new clsPuntoA(objDay.f_dia, objDetalle.e_horainicio, "e", objParams.PreIng, objParams.PostIng);
                  objOut  = new clsPuntoA(objDay.f_dia, objDetalle.e_horafin, "s", objParams.PreEgr, objParams.PostEgr);
                }
                else
                {
                  objIn = new clsPuntoA(objDay.f_dia, objDetalle.e_horainicio, "e", objParams.PreIng2, objParams.PostIng2);
                  objOut  = new clsPuntoA(objDay.f_dia, objDetalle.e_horafin, "s", objParams.PreEgr2, objParams.PostEgr2);
                }

        if (objLastOut == null) {
                  //Es la primer entrada del día. Se agrega seguro
                  this.arrRangos.Add(objIn);

                  //Relaciona los elementos del reporte
                  objIn.RelatedXML = Procesos.GetNewDataItem(objIn.Day, objIn.Minutes, objIn.Type);
                  xmlEvents.AddTailElement(objIn.RelatedXML);

                }
        else
        {

                  //Ya existe una salida
                  if (objIn.DateTime != objLastOut.DateTime)
          {
                    //La salida anterior y la entrada nueva son distintas
                    //Se agregan la salida anterior, y la nueva entrada. Tambien se setea la nueva salida.

                    this.arrRangos.Add(objLastOut);
                    this.arrRangos.Add(objIn);

                    //Relaciona los elementos del reporte
                    objLastOut.RelatedXML = Procesos.GetNewDataItem(objLastOut.Day, objLastOut.Minutes, objLastOut.Type);
                    objIn.RelatedXML = Procesos.GetNewDataItem(objIn.Day, objIn.Minutes, objIn.Type);

                    xmlEvents.AddTailElement(objLastOut.RelatedXML);
                    xmlEvents.AddTailElement(objIn.RelatedXML);

                  }
                }

                objLastOut = objOut;

              }

            }

            //Agrega a la lista la de salidas
            if (objLastOut != null) {
              this.arrRangos.Add(objLastOut);

              objLastOut.RelatedXML = Procesos.GetNewDataItem(objLastOut.Day, objLastOut.Minutes, objLastOut.Type);
              xmlEvents.AddTailElement(objLastOut.RelatedXML);

            }

          }

        }

      }

      //pAddRegis pRegisDet

      if (pAddRegis || pRegisDet) {

        //Pide las fichadas del legajo
        FICHADASING objFichada;

        //Obtiene las fichadas
        NomadObjectList nolFichadas;
        objParams.CalcularFechasFichadas(objParams.FechaInicio, objParams.FechaFin, objParams.Legajo.GetAttr("id"));
        nolFichadas = this.GetFichadasIngDDO(objParams.FechaInicioFICHADAS.AddDays(-1), objParams.FechaFinFICHADAS.AddDays(1), objParams.Legajo.GetAttr("id"));
    this.objBatch.Log("Tomando las fichadas mayores e igual a " + objParams.FechaInicioFICHADAS.AddDays(-1).ToString("dd/MM/yyyy HH:mm") + " y menores a "  + objParams.FechaInicioFICHADAS.AddDays(1).ToString("dd/MM/yyyy HH:mm"));
        if (nolFichadas.Count == 0) {
          this.objBatch.Wrn("No se han encontrado fichadas para el legajo " + objParams.Legajo.GetAttr("e_numero_legajo") + ".");
        }

        //Recorre las FICHADAS
        clsPuntoA objP = new clsPuntoA();
        int intPos = 0;
        bool IsOK = false;

        for (int x = 0; x < nolFichadas.Count; x++) {
          objFichada = (FICHADASING) nolFichadas[x];

          IsOK = false;

          //Las ANULADAS o las INDEFINIDAS no las utiliza.
          if (objFichada.c_estado == "A" || objFichada.c_tipo == "I")
            continue;

          if (pRegisDet || pAddRegis) {

            //Busca el día al cual se le incluirá la fichada

      //Prueba con el dia en particular
            xmlDay = (NomadXML) slsDays[objFichada.f_fechahora.ToString("yyyyMMdd")];
            if ( xmlDay == null || (!(objFichada.f_fechahora >= StringUtil.str2date(xmlDay.GetAttr("ffrom")) && objFichada.f_fechahora < StringUtil.str2date(xmlDay.GetAttr("fto")))) ) {
              //Prueba con el dia anterior
              xmlDay = (NomadXML) slsDays[objFichada.f_fechahora.AddDays(-1d).ToString("yyyyMMdd")];
              if (xmlDay == null || (!(objFichada.f_fechahora >= StringUtil.str2date(xmlDay.GetAttr("ffrom")) && objFichada.f_fechahora < StringUtil.str2date(xmlDay.GetAttr("fto")))) ) {
                //Prueba con el día posterior
                xmlDay = (NomadXML) slsDays[objFichada.f_fechahora.AddDays(1d).ToString("yyyyMMdd")];
          if (xmlDay == null || (!(objFichada.f_fechahora >= StringUtil.str2date(xmlDay.GetAttr("ffrom")) && objFichada.f_fechahora < StringUtil.str2date(xmlDay.GetAttr("fto")))) ) {
          //No es ningún día
          xmlDay = null;
          }

              }
            }

          }

          if (xmlDay == null) continue;

          //Pregunta si se quiere agregar el detalle de las fichadas
          if (pRegisDet) {

            xmlRegis = xmlDay.FindElement("total-fichadas").AddTailElement("fic");
            xmlRegis.SetAttr("oi",             objFichada.Id);
            xmlRegis.SetAttr("fh",             objFichada.f_fechahora.ToString("yyyyMMddHHmmss"));
            xmlRegis.SetAttr("es",             objFichada.c_estado);
            xmlRegis.SetAttr("o",              objFichada.c_origen);
            xmlRegis.SetAttr("t",              objFichada.c_tipo);
            xmlRegis.SetAttr("en",             objFichada.l_entrada ? "1" : "0");
            xmlRegis.SetAttr("obs",            objFichada.o_observaciones);
            xmlRegis.SetAttr("oi_liquidacion", objFichada.oi_liquidacion);
            xmlRegis.SetAttr("oi_terminal",    objFichada.oi_terminal);
          }

          if (!pAddRegis) break;

          //Obtiene el elemento de "Fichadas no apareadas"
          xmlNotPaireds = xmlDay.FindElement("fichadas-no-aparejadas");

          //Setea el primer punto (que puede ser de entrada o de salida)
          if (intPos < this.arrRangos.Count) objP = (clsPuntoA) this.arrRangos[intPos];

          while (intPos < this.arrRangos.Count) {
            if (objFichada.f_fechahora < objP.FechaInicio) {
              break;
            } else {
              if (objFichada.f_fechahora >= objP.FechaInicio && objFichada.f_fechahora <= objP.FechaFin && objP.Day == StringUtil.str2date(xmlDay.GetAttr("f"))) {
                //Pregunta si es un evento LIBRE
                if (objP.Type == "l") {
                  IsOK = false;

                } else {
                  //Es un evento de tipo FIJO o ROTATIVO

                  //Valida que el evento concuerde con el tipo de fichada
                  if ( (objFichada.c_tipo == "E" && objP.Type == "s") || (objFichada.c_tipo == "S" && objP.Type == "e") )
                    break;

                  //Pregunta si el evento ya tiene asociado una fichada
                  if (objP.RelatedXML.GetAttr("oif") == "") {
                    objP.RelatedXML.SetAttr("oif", objFichada.Id);
                    objP.FichadaDT = objFichada.f_fechahora;
                    IsOK = true;
                  } else {
                    //Pregunta si la fichada actual está mas cerca del evento
                    TimeSpan tspActualDiff = objP.DateTime - objFichada.f_fechahora;
                    TimeSpan tspLastDiff = objP.DateTime - objP.FichadaDT;

                    if (Math.Abs(tspActualDiff.TotalMilliseconds) < Math.Abs(tspLastDiff.TotalMilliseconds)) {
                      //La fichada actual está cerca que la anterior

                      //Agrega la fichada anterior en la lista de fichadas no asociadas
                      xmlNotPaired = xmlNotPaireds.AddTailElement("f");
                      xmlNotPaired.SetAttr("oif", objP.RelatedXML.GetAttr("oif"));

                      //Asocia la fichada actual con el evento
                      objP.RelatedXML.SetAttr("oif", objFichada.Id);
                      objP.FichadaDT = objFichada.f_fechahora;
                      IsOK = true;

                    }
                  }
                }

                break;

              } else {
                intPos++;
                try {objP = (clsPuntoA) this.arrRangos[intPos];} catch {}
              }
            }

          }

          //Si la fichada no está aparejada con ningín rango se la agrega a la lista de no aparejadas
          if (!IsOK) {
            xmlNotPaired = xmlNotPaireds.AddTailElement("f");
            xmlNotPaired.SetAttr("oif", objFichada.Id);
          }

        }
      }

      //Proceso finalizado
      this.objBatch.SetPro(100);
      NomadBatch.Trace("Proceso finalizado.");

      return xmlResult;
    }

    Hashtable TipoHoras = new Hashtable();
    private Tipos_Horas.TIPOHORA GetTipoHora(string oiTipoHora)
    {
        Tipos_Horas.TIPOHORA th;
        if(TipoHoras.ContainsKey(oiTipoHora))
            th = (Tipos_Horas.TIPOHORA)TipoHoras[oiTipoHora];
        else
        {
            th = Tipos_Horas.TIPOHORA.Get(oiTipoHora);
            TipoHoras.Add(oiTipoHora, th);
        }

        return th;
    }

    /// <summary>
    /// Genera la liquidación para el personal pasado
    /// </summary>
    /// <param name="pstrOiPersonal">Oi del personalEmp a procesar.</param>
    /// <param name="pobjParams">Objeto de parámetros.</param>
    /// <param name="pstrLiquidacion"></param>
    /// <param name="pxelLegajo"></param>
    /// <param name="phtaTotalPersonalList"></param>
    /// <returns></returns>
    private LIQUIDACIONPERS ProcesarLegajo(string pstrOiPersonal, clsParams pobjParams, string pstrLiquidacion, XmlElement pxelLegajo, Hashtable phtaTotalPersonalList) {

      LIQUIDACIONPERS objLiqPers;
      LIQUIDACJOR objDayLiq = null;
      LIQUIDACIONPROC objHourLiq;

      ArrayList arrHope;
      NomadXML nxmFichada;
      clsHito objHito;

      //Limpia el caché para el legajo
      pobjParams.ClearLegCache();
      NomadProxy.GetProxy().CacheDel("GetHOPEPer");

      //Obtiene las fichadas

      pobjParams.CalcularFechasFichadas(pobjParams.FechaInicio, pobjParams.FechaFin, pstrOiPersonal);
  
      nxmFichada = pobjParams.GetFichadasIng(pobjParams.FechaInicioFICHADAS, pobjParams.FechaFinFICHADAS, pstrOiPersonal);
      NomadBatch.Trace("Tomando las fichadas mayores e igual a " + pobjParams.FechaInicioFICHADAS.ToString("dd/MM/yyyy HH:mm") + " y menores a "  + pobjParams.FechaFinFICHADAS.ToString("dd/MM/yyyy HH:mm"));

      //Sumariza las fichadas procesadas
      if (nxmFichada != null)
        if (nxmFichada.FirstChild() != null)
          pobjParams.CantFichadas = pobjParams.CantFichadas + nxmFichada.FirstChild().ChildLength;

    //Obtiene el objeto de Liquidacion Personal (LIQUIDACIONPERS) para eliminarle las jornadas
    objLiqPers = GetLiqPer(phtaTotalPersonalList, pstrOiPersonal, pstrLiquidacion, false);

    //VALIDA SI REPORCESA EN CASO DE QUE EL LEGAJO SEA MANUAL
    if(pobjParams.ReProcesar == false && objLiqPers.l_manual)
    {
    this.objBatch.Err("El legajo '" + pxelLegajo.GetAttribute("e_numero_legajo") + "' (OI:" + pxelLegajo.GetAttribute("id") + ") no se va a reprocesar porque ha sido modificado de forma MANUAL.");
        return objLiqPers;
    }

    //Realiza una validación no exaustiva de las fichas para determinar si son consistentes
      //Se valida primero y luego se hace el hash porque la validación es más rápida
      NomadBatch.Trace("Realiza validacion Ligth");
      if (!LigthValidation(pobjParams, nxmFichada)) {
        this.objBatch.Err("Fichadas inconsistentes. Por favor valide las fichadas para el legajo " + pxelLegajo.Attributes["e_numero_legajo"].Value + ".");
        pxelLegajo.SetAttribute("status", "N");
        pxelLegajo.SetAttribute("err_desc", "Fichadas inconsistentes.");

        //Obtiene el objeto de Liquidacion Personal (LIQUIDACIONPERS) para eliminarle las jornadas
        //objLiqPers = GetLiqPer(phtaTotalPersonalList, pstrOiPersonal, pstrLiquidacion, false);
        if (objLiqPers != null) {
          //El objeto ya existia. Se graba con las jornadas eliminadas.
          try {
            objLiqPers.d_tipo_mensaje = "E";
            objLiqPers.o_mensaje = "Fichadas inconsistentes. Por favor valide las fichadas.";
            objLiqPers.HashNull = true;
            NomadEnvironment.GetCurrentTransaction().Save(objLiqPers);

          } catch (Exception ex) {
            throw new Exception("Se ha producido un error al intentar eliminar los datos de las jornadas procesadas al legajo " + pxelLegajo.Attributes["e_numero_legajo"].Value + "." + ex.Message, ex);
          }

        }

        return null;
      }

    //Obtiene el objeto Liquidacion Personal (LIQUIDACIONPERS)

    if (objLiqPers == null)
      objLiqPers = GetLiqPer(phtaTotalPersonalList, pstrOiPersonal, pstrLiquidacion);

    //Obtiene el objeto de LiquidacionPer y verifica si el HASH ha cambiado
      NomadBatch.Trace("Calcula HASH.");
      string strLPHash = GetHASHProcesamientoLegajo(pstrLiquidacion, pstrOiPersonal, pobjParams);
      if (pobjParams.ReProcesar == false && strLPHash == objLiqPers.Hash) {
        //El HASH no se ha modificado. Significa que no se han modificado datos y el resultado del procesamiento
        //para esta persona será el mismo.
        NomadBatch.Trace("El legajo '" + pxelLegajo.GetAttribute("e_numero_legajo") + "' (OI:" + pxelLegajo.GetAttribute("id") + ") no se va a reprocesar porque no cambio el HASH.");
        return objLiqPers;
      }

      //Suma en uno la cantidad de legajos realmente procesados
      pobjParams.CantLegReproc = pobjParams.CantLegReproc + 1;

      //Se obtiene la ESPERANZA para el legajo.
      NomadBatch.Trace("Obtiene la esperanza.");
      arrHope = ESPERANZAPER.GetDaysHope(pobjParams.FechaInicio, pobjParams.FechaFin.AddDays(-1d), int.Parse(pxelLegajo.GetAttribute("id")));
      if (arrHope.Count == 0) {
        this.objBatch.Wrn("No se ha encontrado esperanza para el legajo " + pxelLegajo.GetAttribute("e_numero_legajo") + ".");
        pxelLegajo.SetAttribute("status", "N");
        pxelLegajo.SetAttribute("err_desc", "No se ha encontrado esperanza.");
        return null;
      }

      //Genera la lista de hitos de esperanza
      NomadBatch.Trace("Genera la cadena de Hitos y agrega las fichadas. (pre)");
      objHito = GenerateTimeChain(pobjParams, arrHope, int.Parse(pxelLegajo.GetAttribute("id")));
	  
      NomadBatch.Trace("Genera la cadena de Hitos y agrega las fichadas. (post)");	  

      //Si el procesamiento es PARCIAL se elimina la última fichada en caso de ser de entrada (de las no anuladas)
      if (pobjParams.Parcial) {

        if (nxmFichada.FirstChild().ChildLength > 0) {
          NomadXML xmlFichada = nxmFichada.FirstChild().LastChild();
          if (xmlFichada.GetAttr("c_tipo") == "E") {
            nxmFichada.FirstChild().DeleteChild(xmlFichada);
          }
        }
      }

      //Agrega las fichadas a la lista de HITOS
      objHito = AgregarFichadas(objHito, nxmFichada);

      LogHito(objHito);

      NomadBatch.Trace("Comienza la recorrida.");

      //Recorre los hitos y empieza a sumarizar --------------------------------------------------------
      clsHito objWorkHito = objHito;
      clsHito objLastHope = null;
      clsHito objLastFichada = null;
      DateTime dteLastEvent = new DateTime(1900, 1, 1);
      TimeSpan tspDiff;
      string strTypeDay = "";

    string strOIHorario = "";
    string strOITurno = "";

      bool bolPresent = false;
      bool bolMustBePresent = false;
      bool bolLicenceDay = false; //Indica si el dia está bajo una licencia
      bool bolIsWorking = false; //Indica si la persona está trabajando o no
      bool bolLastHitWasDay = false;
      string strWorkPlace = "0";

      DateTime dteLastRegis = new DateTime(1900, 1, 1);
      ArrayList arrWorkedRegis = new ArrayList();
      ArrayList arrHopes = new ArrayList();
      clsPunto objPunto;
      double dblWorkedHours;

      //Busca la primer fichada
      clsHito objSearch;
      for (objSearch = objWorkHito; objSearch != null && (objSearch.TipoHito != TiposHito.Fichada);
        objSearch = objSearch.Siguiente);

      //Si la primer fichada es de salida significa que el empleado empezo a trabajar el mes anterior
      //Pone, entonces, como que estubiera trabajando
      //Se contabiliza como que se trabajó desde el INICIO DE DIA
      if (objSearch != null && !objSearch.Entrada) {
        dteLastEvent = pobjParams.FechaInicioFICHADAS;
        bolIsWorking = true;
        strWorkPlace = objSearch.OiEstructura;
        bolPresent = true;

        //Determina si el rango el inicio de día y la fichada no supera el tiempo máximo dentro del lugar
        tspDiff = objSearch.FechaHora.Subtract(dteLastEvent);
        if (tspDiff > pobjParams.MaxDifIngEgr)
          throw new Exception("El legajo '" + pxelLegajo.GetAttribute("e_numero_legajo") + "' sobrepasó el tiempo máximo dentro del lugar el día " + objWorkHito.FechaHora.ToString("dd/MM/yyyy") + ".");
      }

      while (objWorkHito != null) {

    if (objWorkHito.TipoHito == TiposHito.InicioDia) {
      //Inicio de DIA -----------------------------------------------------------------------------------------

          if (objLastHope != null) {
            throw new Exception("Se encontró una esperanza durante el cambio de dia " + objWorkHito.FechaHora.ToString("dd/MM/yyyy") + ".");
          }

          //Cambio de dia, y estaba adentro
          if (bolIsWorking && objDayLiq != null) {
            if (strTypeDay == "L") {
              //Simulo que se fue a media noche
              objPunto = new clsPunto(dteLastRegis, objWorkHito.FechaHora, strWorkPlace);
              arrWorkedRegis.Add(objPunto);
            } else {
              //Calculo las horas como si se fuera a media noche
              dblWorkedHours = WorkedHours(dteLastEvent, objWorkHito.FechaHora, pobjParams);
              if (dblWorkedHours > (double) 0) {

                objHourLiq = new LIQUIDACIONPROC();

                objHourLiq.n_cantidadhs = dblWorkedHours;
                objHourLiq.oi_tipohora = objLastHope == null ? Convert.ToInt32(TiposHoras.Excedente).ToString() : objLastHope.TipoHora.ToString();
                if (objLastHope != null) objHourLiq.oi_tipohora_esp = objLastHope.TipoHora.ToString();
                objHourLiq.f_fechoraentrada = dteLastEvent;
                objHourLiq.f_fechorasalida = objWorkHito.FechaHora;
                objHourLiq.oi_estructura = objLastHope == null ? strWorkPlace.ToString() : objLastHope.OiEstructura.ToString();
        objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;

                objDayLiq.LIQUIDACPROC.Add(objHourLiq);
              }
            }
          }

          //Actualiza el inicio de día
          if (bolLastHitWasDay) {
            //Si llegan dos hitos seguidos de InicioDia elimina el último
            objLiqPers.LIQUIDACJOR.Remove(objDayLiq);
            objDayLiq = null;
          }

          //Comprueba la variable de presencia del día anterior
          if (objDayLiq != null) {
            objDayLiq.l_presente = bolPresent;
            objDayLiq.l_esperado = bolMustBePresent;
          }

          CloseDay(objDayLiq, pstrOiPersonal, strTypeDay, bolPresent, bolMustBePresent, arrHopes, arrWorkedRegis, pobjParams, pxelLegajo, bolLicenceDay, strOITurno, strOIHorario);

          //Prepara los datos para el NUEVO DÍA ----------------------
          strTypeDay = objWorkHito.TipoHorario;
      strOIHorario = objWorkHito.OIHorario;
      strOITurno = objWorkHito.OITurno;

          //Para los LIBRES
          if (strTypeDay == "L") {
            arrHopes = new ArrayList();
            arrWorkedRegis = new ArrayList();
          }

          //Crea el objeto Día de la liquidacion
          objLastHope = null;
          objDayLiq = new LIQUIDACJOR();
          objLiqPers.LIQUIDACJOR.Add(objDayLiq);
          objDayLiq.f_fecjornada = objWorkHito.Dia;
          objDayLiq.c_tipo       = objWorkHito.TipoDia;
          objDayLiq.e_horainicio = objWorkHito.MinutosIniDia;
          objDayLiq.e_horafin    = objWorkHito.MinutosFinDia;

          bolMustBePresent = false;
          bolPresent       = false;
          bolLastHitWasDay = true;
          bolLicenceDay    = objWorkHito.OILicencia != "";

          //Cambio de dia, y estaba adentro, Simulo que entro a media noche
          if (bolIsWorking) {
            dteLastRegis = objWorkHito.FechaHora;
            dteLastEvent = objWorkHito.FechaHora;
            bolPresent = true;
          }

        } else {

      //Es ESPERANZA o FICHADA -----------------------------------------------------------------------------------

          bolLastHitWasDay = false;

      this.AddInOuts(objDayLiq, strTypeDay, objWorkHito, objLastHope);

          if (strTypeDay == "L") {
            //Es de tipo LIBRE

            //Con los horarios LIBRES se actua diferente que con los FIJOS o ROTATIVOS
            //Los libres tienen un array de "puntos" que representan desde que hora trabajaron hasta que hora
            //Estos puntos se crean con la fichada de ENTRADA y se guarda con la fichada de SALIDA

            //Para la esperanza se genera un array con objetos conteniendo LA CANTIDAD DE HORAS A TRABAJAR.

            if (objWorkHito.TipoHito == TiposHito.Fichada) {

              objLastFichada = objWorkHito;

              if (bolIsWorking) {
                //Se realizó una fichada de EGRESO

                if (objWorkHito.Entrada) {
                  throw new Exception("Se encontró una fichada de entrada cuando se esperaba una de egreso " + objWorkHito.FechaHora.ToString("dd/MM/yyyy HH:mm") + ".");
                }

                objPunto = new clsPunto(dteLastRegis, objWorkHito.FechaHora, objWorkHito.OiEstructura.ToString());
                arrWorkedRegis.Add(objPunto);

                bolIsWorking = false;

              } else {
                //Se realizó una fichada de INGRESO

                if (objWorkHito.Entrada == false) {
                  throw new Exception("Se encontró una fichada de salida cuando se esperaba una de ingreso " + objWorkHito.FechaHora.ToString("dd/MM/yyyy HH:mm") + ".");
                }

                dteLastRegis = objWorkHito.FechaHora;
                strWorkPlace = objWorkHito.OiEstructura;
                bolPresent = true;
                bolIsWorking = true;

              }

            } else {
              //Es una esperanza
              if (objLastHope != null) {
                tspDiff = objWorkHito.FechaHora.Subtract(objLastHope.FechaHora);
                CalculoHoras choHora = (CalculoHoras) arrHopes[arrHopes.Count - 1];
                choHora.CantHoras = tspDiff.TotalHours;
                arrHopes[arrHopes.Count - 1] = choHora;
              }

              if (objWorkHito.TipoHora.OI != clsTipoHora.Excedent) {

                CalculoHoras choHora = new CalculoHoras();
                choHora.OiHora = objWorkHito.TipoHora.OI;
                choHora.CantHoras = 0;
                choHora.OiEstructura = objWorkHito.OiEstructura;
                choHora.EnPresencia = objWorkHito.TipoHora.EnPresencia;
                choHora.EnAusencia = objWorkHito.TipoHora.EnAusencia;
                arrHopes.Add(choHora);

                objLastHope = objWorkHito;
                dteLastEvent = objWorkHito.FechaHora;
              } else
                objLastHope = null;

              bolMustBePresent = bolMustBePresent || (objWorkHito.TipoHora.EnAusencia == Sumarizaciones.EnAusenteExedente);

            }

          } else {
            //Es de tipo FIJO o ROTATIVO

            if (bolIsWorking) {
              //EL EMPLEADO ESTÁ TRABAJANDO

              //Determina si es de tipo FICHADA o ESPERANZA
              if (objWorkHito.TipoHito == TiposHito.Fichada) {
                //El empleado marca una FICHADA de egreso [2]
                if (objWorkHito.Entrada) {
                  throw new Exception("Se encontró una fichada de entrada cuando se esperaba una de egreso " + objWorkHito.FechaHora.ToString("dd/MM/yyyy HH:mm") + ".");
                }

                //Guarda la última fichada para luego controlar si la última fichada es de entrada
                objLastFichada = objWorkHito;

                //Solo creará un registro si el hito de esperanza anterior sumariza o sumariza en excedente
                if (objLastHope == null ||
                    (objLastHope != null && objLastHope.TipoHora.EnPresencia != Sumarizaciones.NoSumariza) ) {

                  //La última esperanza iniciada controlaba ausencia => el tipo se está llendose antes
                  dblWorkedHours = WorkedHours(dteLastEvent, objWorkHito.FechaHora, pobjParams);
                  if (dblWorkedHours > (double) 0) {
                    objHourLiq = new LIQUIDACIONPROC();
                    objDayLiq.LIQUIDACPROC.Add(objHourLiq);
                    objHourLiq.n_cantidadhs = dblWorkedHours;

                    if (objLastHope == null) {
                      //El empleado entró y salió mientras no habia esperanzas. Se marca como extra.
                      objHourLiq.oi_tipohora = Convert.ToInt32(TiposHoras.Excedente).ToString();
                    } else {
                      //El empleado salió mientras está en vigencia una esperanza. Se marca según las sumarizaciones del EnPresente de la esperanza anterior.
                      objHourLiq.oi_tipohora = objLastHope.TipoHora.EnPresencia == Sumarizaciones.Sumariza ? objLastHope.TipoHora.OI : objLastHope.TipoHora.Oi_TH_Exc;
                      objHourLiq.oi_tipohora_esp = objLastHope.TipoHora.OI;
                    }

                    objHourLiq.f_fechoraentrada = dteLastEvent;
                    objHourLiq.f_fechorasalida = objWorkHito.FechaHora;
                    objHourLiq.oi_estructura = objLastHope == null ? strWorkPlace : objLastHope.OiEstructura;
          objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
                  }
                }

                dteLastEvent = objWorkHito.FechaHora;
                bolIsWorking = false;

              } else {
                //Se cambió el tipo de hora mientras el empleado estaba trabajndo [4]

                //Solo creará un registro ei el hito de esperanza anterior sumariza o sumariza en excedente
                if (objLastHope == null ||
                    (objLastHope != null && objLastHope.TipoHora.EnPresencia != Sumarizaciones.NoSumariza) ) {

                  dblWorkedHours = WorkedHours(dteLastEvent, objWorkHito.FechaHora, pobjParams);
                  if (dblWorkedHours > (double) 0) {
                    objHourLiq = new LIQUIDACIONPROC();
                    objDayLiq.LIQUIDACPROC.Add(objHourLiq);
                    objHourLiq.n_cantidadhs = dblWorkedHours;
                    //objHourLiq.n_cantidadhs = WorkedHours(dteLastEvent, objWorkHito.FechaHora, pobjParams);

                    if (objLastHope == null) {
                      //El empleado trabajaba en extras y empezó una esperanza. Se marca como extra.
                      objHourLiq.oi_tipohora = Convert.ToInt32(TiposHoras.Excedente).ToString();
                    } else {
                      //El empleado trabajaba con una esperanza y empezó una nueva esperanza. Se marca según las sumarizaciones del EnPresente de la esperanza anterior.
                      objHourLiq.oi_tipohora = objLastHope.TipoHora.EnPresencia == Sumarizaciones.Sumariza ? objLastHope.TipoHora.OI : objLastHope.TipoHora.Oi_TH_Exc;
                      objHourLiq.oi_tipohora_esp = objLastHope.TipoHora.OI;
                    }

                    objHourLiq.f_fechoraentrada = dteLastEvent;
                    objHourLiq.f_fechorasalida = objWorkHito.FechaHora;
                    objHourLiq.oi_estructura = objLastHope == null ? strWorkPlace : objWorkHito.OiEstructura;
          objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
                  }

                }

                bolMustBePresent = bolMustBePresent || (objWorkHito.TipoHora.EnAusencia == Sumarizaciones.EnAusenteExedente);

                dteLastEvent = objWorkHito.FechaHora;
                bolIsWorking = true;
                objLastHope = objWorkHito.TipoHora.OI == clsTipoHora.Excedent ? null : objWorkHito;

              }
            } else {
              //EL EMPLEADO NO ESTÁ TRABAJANDO

              //Calcula la diferencia
              tspDiff = objWorkHito.FechaHora.Subtract(dteLastEvent);

              //El empleado no está trabajando
              if (objWorkHito.TipoHito == TiposHito.Fichada) {
                //El empleado marca una FICHADA de ingreso [1]
                if (objWorkHito.Entrada == false) {
                  throw new Exception("Se encontró una fichada de salida cuando se esperaba una de ingreso " + objWorkHito.FechaHora.ToString("dd/MM/yyyy HH:mm") + ".");
                }

                //Guarda la última fichada para luego controlar si la última fichada es de entrada
                objLastFichada = objWorkHito;

                //Sumariza si EnAusencia de la hora anterior sumariza en ausente o en si mismo
                if (objLastHope != null && (objLastHope.TipoHora.EnAusencia != Sumarizaciones.NoSumariza) ) {

                  //La última esperanza iniciada sumarizaba.
                  if (tspDiff.TotalSeconds > (double) 0) {
                    objHourLiq = new LIQUIDACIONPROC();
                    objDayLiq.LIQUIDACPROC.Add(objHourLiq);
                    objHourLiq.n_cantidadhs = tspDiff.TotalHours;

                    objHourLiq.oi_tipohora = objLastHope.TipoHora.EnAusencia == Sumarizaciones.Sumariza ? objLastHope.TipoHora.OI : objLastHope.TipoHora.Oi_TH_Aus;
                    objHourLiq.oi_tipohora_esp = objLastHope.TipoHora.OI;

                    objHourLiq.f_fechoraentrada = dteLastEvent;
                    objHourLiq.f_fechorasalida = objWorkHito.FechaHora;
                    objHourLiq.oi_estructura = objLastHope.OiEstructura;
          objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
                  }
                  bolMustBePresent = true;
                }

                dteLastEvent = objWorkHito.FechaHora;
                strWorkPlace = objWorkHito.OiEstructura;
                bolIsWorking = true;
                bolPresent = true;

              } else {
                //Se ha cambiado el tipo de hora con el empleado afuera [3]

                //Sumariza si EnAusencia de la hora anterior sumariza en ausente o en si mismo
                if (objLastHope != null && (objLastHope.TipoHora.EnAusencia != Sumarizaciones.NoSumariza) ) {

                  //La última esperanza iniciada sumarizaba.
                  if (tspDiff.TotalSeconds > (double) 0) {
                    objHourLiq = new LIQUIDACIONPROC();
                    objDayLiq.LIQUIDACPROC.Add(objHourLiq);

                    objHourLiq.n_cantidadhs = tspDiff.TotalHours;

                    objHourLiq.oi_tipohora = objLastHope.TipoHora.EnAusencia == Sumarizaciones.Sumariza ? objLastHope.TipoHora.OI : objLastHope.TipoHora.Oi_TH_Aus;
                    objHourLiq.oi_tipohora_esp = objLastHope.TipoHora.OI;

                    objHourLiq.f_fechoraentrada = dteLastEvent;
                    objHourLiq.f_fechorasalida = objWorkHito.FechaHora;
                    objHourLiq.oi_estructura = objLastHope.OiEstructura;
          objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
                  }
                  bolMustBePresent = true;
                }

                dteLastEvent = objWorkHito.FechaHora;
                bolIsWorking = false;
                objLastHope = objWorkHito.TipoHora.OI == clsTipoHora.Excedent ? null : objWorkHito;

              }

            }
          }

        }

        //Lee el proximo
        objWorkHito = objWorkHito.Siguiente;
      }

      //-----------------------------------------------------------------------------------------------
      // Terminó de recorrer todos los HITOS
      //-----------------------------------------------------------------------------------------------
/*
      if (bolLastHitWasDay) {
        objLiqPers.LIQUIDACJOR.Remove(objDayLiq);
        objDayLiq = null;
      }
*/
      //Determina si la última fichada fué de ENTRADA !!
      if (objLastFichada != null && objLastFichada.Entrada) {
        //Si la última fichada fué ENTRADA se contabiliza como que se trabajó hasta el FIN DE DIA
          if (strTypeDay == "L")
          {
              tspDiff = pobjParams.FechaFin.AddDays(-1).Subtract(dteLastEvent);

          }
          else
          {
              tspDiff = pobjParams.FechaFinFICHADAS.Subtract(dteLastEvent);
          }
       
        //Determina si el rango de la fichada y el fin de día no supera el tiempo máximo dentro del lugar
        if (tspDiff > pobjParams.MaxDifIngEgr && !pobjParams.Parcial)
          throw new Exception("El legajo '" + pxelLegajo.GetAttribute("e_numero_legajo") + "' sobrepasó el tiempo máximo dentro del lugar el día " + dteLastEvent.ToString("dd/MM/yyyy") + ".");

        if (strTypeDay == "L") {
            //objPunto = new clsPunto(dteLastRegis, pobjParams.FechaFin.AddDays(-1), objWorkHito == null ? null : objWorkHito.OiEstructura.ToString());
            objPunto = new clsPunto(dteLastRegis, pobjParams.FechaFin, strWorkPlace);
          arrWorkedRegis.Add(objPunto);
        } else {

          //El último día fue FIJO o ROTATIVO
          if (tspDiff.TotalSeconds > (double) 0) {

            objHourLiq = new LIQUIDACIONPROC();
            objDayLiq.LIQUIDACPROC.Add(objHourLiq);
            objHourLiq.n_cantidadhs = tspDiff.TotalHours;

            if (objLastHope == null) {
              //El empleado trabajó extras.
              objHourLiq.oi_tipohora = Convert.ToInt32(TiposHoras.Excedente).ToString();
            } else {
              //El empleado trabajó con una esperanza. Se marca según las sumarizaciones del EnPresente.
              objHourLiq.oi_tipohora = objLastHope.TipoHora.EnPresencia == Sumarizaciones.Sumariza ? objLastHope.TipoHora.OI : objLastHope.TipoHora.Oi_TH_Exc;
              objHourLiq.oi_tipohora_esp = objLastHope.TipoHora.OI;
            }

            objHourLiq.f_fechoraentrada = dteLastEvent;
            objHourLiq.f_fechorasalida = pobjParams.FechaFinFICHADAS;
            objHourLiq.oi_estructura = objLastFichada.OiEstructura;
      objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
          }
        }
      }

      if (bolLastHitWasDay) {
        if (objDayLiq.LIQUIDACPROC.Count == 0) {
          objLiqPers.LIQUIDACJOR.Remove(objDayLiq);
          objDayLiq = null;
        }
      }

      CloseDay(objDayLiq, pstrOiPersonal, strTypeDay, bolPresent, bolMustBePresent, arrHopes, arrWorkedRegis, pobjParams, pxelLegajo, bolLicenceDay, strOITurno, strOIHorario);

      //Guarda el Hash de la liquidación per
      objLiqPers.Hash = strLPHash;
    if (pxelLegajo.GetAttribute("oi_grupo_hor") != "")
      objLiqPers.oi_grupo_hor = pxelLegajo.GetAttribute("oi_grupo_hor");
    else
      objLiqPers.oi_grupo_horNull = true;

      //Crea la transacción y ejecuta el Begin
      NomadEnvironment.GetCurrentTransaction().Begin();

      string strStep = "";
      try {

        //Adjunta las fichadas utilizadas a la liquidación
        //AdjuntarLiquidacion

        //Calcula las horas Franco
        strStep = "CalcFC";
        //NucleusRH.Base.Tiempos_Trabajados.CalculoFrancos objCalcFC = new NucleusRH.Base.Tiempos_Trabajados.CalculoFrancos();
        CalculoFrancos.CalcularLiqPer(objLiqPers);
        NomadBatch.Trace("Terminó de calcular las horas FC.");

        strStep = "EjecutarConceptos";
        NomadBatch.Trace("Ejecutar Conceptos.");
    NomadBatch.Trace("objLiqPers -- " + objLiqPers.SerializeAll());

    if (this.m_ejecConc!=null)
        {
      NomadBatch.Trace("this.m_ejecConc!=null");
      if (this.myliq==null) {
        NomadBatch.Trace("this.myliq==null");
        this.myliq=objLiqPers.Getoi_liquidacion();
      }

      NomadBatch.Trace("this.m_ejecConc.ApplyConceptos()");
      this.m_ejecConc.ApplyConceptos(this.myliq, objLiqPers.Getoi_personal_emp(false), objLiqPers, arrHope);
    }

        strStep = "SaveLiq";
        objLiqPers.l_manual = false;
    NomadEnvironment.GetCurrentTransaction().Save(objLiqPers);

        strStep = "SaveFC";
        NomadBatch.Trace("Llama al metodo SaveBancosFC");
    this.m_ejecConc.SaveBancosFC();
        NomadBatch.Trace("Llamada del metodo finalizada.");

        strStep = "EjecEvePreSave";
        NomadBatch.Trace("Llama el Evento evePreSave");
        LIQUIDACIONPERS.evePreSave(objLiqPers);
        NomadBatch.Trace("Llamada del evento finalizada.");

        strStep = "MakeCommit";
        NomadBatch.Trace("Se realizará el commit.");
        NomadEnvironment.GetCurrentTransaction().Commit();
        NomadBatch.Trace("Commit realido correctamente.");

        strStep = "EjecEvePosLI";
        NomadBatch.Trace("Ejecuta el Evento evePostLiquidarPersona.");
        LIQUIDACIONPERS.evePostLiquidarPersona(objLiqPers);
        NomadBatch.Trace("Ejecucion del evento finalizada.");

      } catch (NomadAppException nex) {

        try {
          NomadEnvironment.GetCurrentTransaction().Rollback();
        } catch {}

        throw new Exception(nex.Message, nex);

      } catch (Exception ex) {

        string strMess = "";
        switch (strStep) {
          case "CalcFC":
            strMess = "Se ha producido un error al calcular las Horas Franco Compensatorio. ";
            break;
          case "EjecEvePreSave":
            strMess = "Se ha producido un error al realizando el proceso Custom 'evePreSave'. ";
            break;
          case "SaveLiq":
            strMess = "Se ha producido un error al grabar el procesamiento. ";
            break;
          case "MakeCommit":
            strMess = "Se ha producido un error al hacer el commit general del procesamiento. ";
            break;
        }

        if (strStep != "EjecEvePosLI")
          NomadEnvironment.GetCurrentTransaction().Rollback();

        throw new Exception(strMess + ex.Message, ex);
      }

      return objLiqPers;
    }

    private void AddInOuts(LIQUIDACJOR pobjDayLiq, string pstrTypeDay, clsHito pobjWorkHito, clsHito pobjLastHope) {

      //Carga los datos para el reporte
      if (pobjWorkHito.TipoHito == TiposHito.Esperanza) {

        if (pstrTypeDay != "L") {
          if (pobjWorkHito.TipoHora.EnAusencia == Sumarizaciones.EnAusenteExedente) {
            if (pobjLastHope == null || pobjLastHope.TipoHora.EnAusencia != Sumarizaciones.EnAusenteExedente) {

              if (pobjDayLiq.e_entrada_1Null)
                pobjDayLiq.e_entrada_1 = pobjWorkHito.Minutos;
              else
                if (pobjDayLiq.e_entrada_2Null)
                  pobjDayLiq.e_entrada_2 = pobjWorkHito.Minutos;
                else
                  if (pobjDayLiq.e_entrada_3Null)
                    pobjDayLiq.e_entrada_3 = pobjWorkHito.Minutos;

            }
          } else {
            if (pobjLastHope != null && pobjLastHope.TipoHora.EnAusencia == Sumarizaciones.EnAusenteExedente) {

              if (pobjDayLiq.e_salida_1Null)
                pobjDayLiq.e_salida_1 = pobjWorkHito.Minutos;
              else
                if (pobjDayLiq.e_salida_2Null)
                  pobjDayLiq.e_salida_2 = pobjWorkHito.Minutos;
                else
                  if (pobjDayLiq.e_salida_3Null)
                    pobjDayLiq.e_salida_3 = pobjWorkHito.Minutos;

            }

          }
        }
      }
    }

        //----------------------------------------------------------------------------------------
        // PROPIEDADES PUBLICAS
        //----------------------------------------------------------------------------------------

        /// <summary>
        /// Retorna la unidad organizativa asociada a la estructura pasada.
        /// </summary>
        /// <param name="pstrOIEstruct"></param>
        /// <returns></returns>
        private string GetUniOrg(string pstrOIEstruct) {
            string strCod;

            //No se recuperaron las unidades. Las recupera.
            if (this.m_colEstructs == null) {
                this.m_colEstructs = new Hashtable();

                string strQryResult = NomadEnvironment.QueryString(Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_ESTRUC_UNI, "");

                XmlDocument xmlUnis = new XmlDocument();
                xmlUnis.LoadXml(strQryResult);

                //Recorre las unidades organizativas
                foreach (XmlNode xnoItem in xmlUnis.DocumentElement.ChildNodes) {
                    strCod = ((XmlElement)xnoItem).GetAttribute("oi_estructura");

                    if (!this.m_colEstructs.ContainsKey(strCod)) {
                        this.m_colEstructs.Add(strCod, ((XmlElement)xnoItem).GetAttribute("c_unidad_org"));
                    }
                }
            }

            return this.m_colEstructs.ContainsKey(pstrOIEstruct) ? this.m_colEstructs[pstrOIEstruct].ToString() : "No se encontró código";

        }

        private void CloseDay(LIQUIDACJOR pobjDayLiq, string pstrOiPersonal, string pstrTypeDay, bool pbolPresent, bool pbolMustBePresent, ArrayList parrHopes, ArrayList parrWorkedRegis, clsParams pobjParams, XmlElement pxelPer, bool pbolLicenceDay, string pstrOITurno, string pstrOIHorario) {
            TimeSpan tspDiff;
            LIQUIDACIONPROC objHourLiq;
            clsPunto objPunto;
            CalculoHoras choHope;
            DateTime dteTemp;
            double  dlbWorkedHours;

            try {

                if (pobjDayLiq == null)
                    return;

                pobjDayLiq.l_presente = pbolPresent;
                pobjDayLiq.l_esperado = pbolLicenceDay ? false : pbolMustBePresent; //Si es dia de licencia no tiene que venir

        //¿puede ser que el if deba ser == en vez de != ?

        //if (pstrOITurno != "") pobjDayLiq.oi_turno = 2;
        //if (pstrOIHorario != "") pobjDayLiq.oi_horario = 2;

        pobjDayLiq.oi_turno = (pstrOITurno == "") ? 2 : int.Parse(pstrOITurno);
        pobjDayLiq.oi_horario = (pstrOIHorario == "") ? 2 : int.Parse(pstrOIHorario);

                if (pstrTypeDay == "L") {
                    //El día anterior fué de tipo LIBRE

                    DateTime dteLastDate = pobjDayLiq.f_fecjornada;

                    //Recorre las horas definidas
                    for(int x = 0; x < parrHopes.Count; x++) {
                        choHope = (CalculoHoras) parrHopes[x];

                        while (true) {

                            if (parrWorkedRegis.Count == 0) {

                                //No quedan más fichadas, recorre las esperanzas para agregarlas a parcialmente ausente
                                if (choHope.CantHoras > 0) {
                                    dteTemp = dteLastDate.AddHours(choHope.CantHoras);
                                    tspDiff = dteTemp.Subtract(dteLastDate);

                                    if (choHope.EnAusencia != Sumarizaciones.NoSumariza) {
                                        //La esperanza es menor al tiempo que se trabajó en la fichada
                                        objHourLiq = new LIQUIDACIONPROC();
                                        pobjDayLiq.LIQUIDACPROC.Add(objHourLiq);
                                        objHourLiq.n_cantidadhs = tspDiff.TotalHours;

                                        objHourLiq.oi_tipohora = choHope.EnAusencia == Sumarizaciones.Sumariza ? choHope.OiHora : Convert.ToInt32(TiposHoras.Ausencia).ToString();
                                        objHourLiq.oi_tipohora_esp = choHope.OiHora;

                                        objHourLiq.f_fechoraentrada = dteLastDate;
                                        objHourLiq.f_fechorasalida = dteTemp;
                                        objHourLiq.oi_estructura = choHope.OiEstructura;
                    objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
                                    }

                                    dteLastDate = dteTemp;
                                }
                                break;

                            } else {
                                objPunto = (clsPunto) parrWorkedRegis[0];
                                tspDiff = objPunto.FechaFin.Subtract(objPunto.FechaInicio);

                                if (choHope.CantHoras >= tspDiff.TotalHours) {

                                  //El empleado trabajo menos de la cantidad que tiene la esperanza

                                  dlbWorkedHours = WorkedHours(objPunto.FechaInicio, objPunto.FechaFin, pobjParams);

                                  //Solo se guarda si sumariza en presencia
                                  if (choHope.EnPresencia != Sumarizaciones.NoSumariza) {

                                      if (dlbWorkedHours > (double)0 ) {
                                            //Se espera mas tiempo del trabajado en la fichada actual
                                            objHourLiq = new LIQUIDACIONPROC();
                                            pobjDayLiq.LIQUIDACPROC.Add(objHourLiq);
                                            objHourLiq.n_cantidadhs = dlbWorkedHours;

                                            objHourLiq.oi_tipohora = choHope.EnPresencia == Sumarizaciones.Sumariza ? choHope.OiHora : Convert.ToInt32(TiposHoras.Excedente).ToString();
                                            objHourLiq.oi_tipohora_esp = choHope.OiHora;

                                            objHourLiq.f_fechoraentrada = objPunto.FechaInicio;
                                            objHourLiq.f_fechorasalida = objPunto.FechaFin;
                                            objHourLiq.oi_estructura = choHope.OiEstructura;
                      objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
                                        }
                                    }

                                    //Se actualiza la esperanza
                                    choHope.CantHoras = choHope.CantHoras - dlbWorkedHours;
                                    if (choHope.CantHoras < 0) choHope.CantHoras = 0;

                                    //Elimina el punto del array
                                    parrWorkedRegis.Remove(objPunto);

                                    dteLastDate = objPunto.FechaFin;
                                } else {

                                    //La esperanza es menor al tiempo que se trabajó en la fichada
                                    if (choHope.CantHoras > 0) {

                                        dteTemp = objPunto.FechaInicio.AddHours(choHope.CantHoras);
                                        dlbWorkedHours=WorkedHours(objPunto.FechaInicio, dteTemp, pobjParams);

                                        if (choHope.EnPresencia != Sumarizaciones.NoSumariza) {

                                          if ( dlbWorkedHours > (double)0 ) {
                                                //La esperanza es menor al tiempo que se trabajó en la fichada
                                                objHourLiq = new LIQUIDACIONPROC();
                                                pobjDayLiq.LIQUIDACPROC.Add(objHourLiq);
                                                objHourLiq.n_cantidadhs = dlbWorkedHours;
                                                objHourLiq.oi_tipohora = choHope.OiHora;
                                                objHourLiq.f_fechoraentrada = objPunto.FechaInicio;
                                                objHourLiq.f_fechorasalida = dteTemp;
                                                objHourLiq.oi_estructura = choHope.OiEstructura;
                        objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
                                            }
                                        }

                                      //Se actualiza la esperanza
                                      choHope.CantHoras = choHope.CantHoras - dlbWorkedHours;
                                      if (choHope.CantHoras < 0) choHope.CantHoras = 0;

                                        //Se actualiza la fichada
                                        objPunto.FechaInicio = dteTemp;

                                        dteLastDate = dteTemp;
                                    }

                                    break;
                                }
                            }

                        }

                    }

                    if (parrWorkedRegis.Count > 0) {
                        //No quedan más esperanzas, recorre las fichadas para agregarlas a excedente
                        for(int y = 0; y < parrWorkedRegis.Count; y++) {
                            objPunto = (clsPunto) parrWorkedRegis[y];

                          dlbWorkedHours = WorkedHours(objPunto.FechaInicio, objPunto.FechaFin, pobjParams);
                          if (dlbWorkedHours > (double)0 ) {
                                objHourLiq = new LIQUIDACIONPROC();
                                pobjDayLiq.LIQUIDACPROC.Add(objHourLiq);
                                objHourLiq.n_cantidadhs = dlbWorkedHours;
                                objHourLiq.oi_tipohora = Convert.ToInt32(TiposHoras.Excedente).ToString();
                                objHourLiq.f_fechoraentrada = objPunto.FechaInicio;
                                objHourLiq.f_fechorasalida = objPunto.FechaFin;
                                objHourLiq.oi_estructura = objPunto.OiEstructura;
                objHourLiq.n_horas_fc = CalculoFrancos.CalcularPorHora(objHourLiq) / 60;
                            }
                        }

                    }

                }

        NomadBatch.Trace("Llama el Evento evePostLiquidarPersona");
        pobjDayLiq.OIPersonalEmp = pstrOiPersonal;
        LIQUIDACJOR.evePostCloseDay(pobjDayLiq);
        NomadBatch.Trace("Llamada del evento finalizada.");

          } catch (Exception ex) {
            throw new Exception (ex.Message);
          }
        }

    private double WorkedHours(DateTime pdteStart, DateTime pdteEnd, clsParams pobjParams) {
            TimeSpan tspWorked;
            double dblWorkedMins = 0; //Cantidad de minutos trabajados para el tipo de hora corriente
            double dblTemp;

            //Calcula los minutos trabajados para el tipo de hora en particular
            tspWorked = pdteEnd.Subtract(pdteStart);
            dblWorkedMins = tspWorked.TotalMinutes;

            //Pregunta si tiene la mínima cantidad para ser contabilizado
            if (pobjParams.MinMinutosLiq > 0) {
                if (dblWorkedMins < pobjParams.MinMinutosLiq)
                    dblWorkedMins = 0;
            }

            //Pregunta si existe una cantidad mínima de trabajo por hora
            if (pobjParams.MinMinutosHora > 0) {
                dblTemp = dblWorkedMins % 60;
                if (dblTemp < pobjParams.MinMinutosHora)
                    dblWorkedMins = dblWorkedMins - dblTemp;

            } else {
                //Pregunta si existe el redondeo por hora
                if (pobjParams.RedondeoA > 0) {
                    dblTemp = dblWorkedMins % pobjParams.RedondeoA;
                    dblWorkedMins = dblWorkedMins - dblTemp;
                }
            }

            tspWorked = new TimeSpan(0, Convert.ToInt32(dblWorkedMins), 0);
            return tspWorked.TotalHours;
        }

        /// <summary>
        /// Genera el procesamiento de horas.
        /// </summary>
        /// <param name="pstrLiquidacion">Oi de la liquidación.</param>
        /// <param name="pstrParams">String con formato XML conteniendo los parámetros cargados en pantalla.</param>
        /// <returns></returns>
        public string GenerarProcesamiento(string pstrLiquidacion, string pstrPersonalList) {
            XmlElement xelLegajo;

            this.objBatch = NomadBatch.GetBatch("Procesamiento", "");

            NomadBatch.Trace(".");
            NomadBatch.Trace(".");
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace(" Comienza el Procesamiento de Horas. -------------------------------------");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            this.objBatch.SetPro(0);
            this.objBatch.SetMess("Generando los Conceptos.");
            this.objBatch.Log("Generando los Conceptos.");

            this.m_ejecConc = RHLiq.LiqConceptosBase.GetObject();
            if (this.m_ejecConc == null) return null;

            this.objBatch.SetPro(0);
            this.objBatch.SetMess("Generación del Procesamiento.");
            this.objBatch.Log("Comienza la generación del Procesamiento de horas.");

            //Crea el objeto de parámetros
            clsParams objParams = new clsParams(pstrLiquidacion, pstrPersonalList);
            if (objParams.Legajos.ChildNodes.Count <= 0 ) {
        this.objBatch.Err("No se encontraron legajos para procesar.");
        return null;
      }

            //Obtiene la lista de personas totales para la liquidación
            Hashtable htaTotalPersonalList = GetLiqPersList(pstrLiquidacion);

            objParams.CantLegajos = objParams.Legajos.ChildNodes.Count;

            NomadBatch.Trace("-----------------------------------------------");
            NomadBatch.Trace("-- Datos Procesamiento ------------------------");
            NomadBatch.Trace("Total personas liquidacion: " + htaTotalPersonalList.Count.ToString());
            NomadBatch.Trace("objParams.CantLegajos     : " + objParams.CantLegajos.ToString());
            NomadBatch.Trace("objParams.FechaInicio     : " + objParams.FechaInicio.ToString("dd/MM/yyyy HH:mm:ss"));
            NomadBatch.Trace("objParams.FechaFin        : " + objParams.FechaFin.AddDays(-1.0).ToString("dd/MM/yyyy HH:mm:ss"));
            NomadBatch.Trace("-----------------------------------------------");

      //Prepara los datos para la generacion de la esperanza
      this.objBatch.SetMess("Obteniendo datos para la generación de la esperanza.");
            NomadBatch.Trace("-----------------------------------------------");
      NomadBatch.Trace("-- Datos PreLoad ------------------------------");
            NomadBatch.Trace("FechaIni : " + objParams.FechaInicio.AddMonths(-1).ToString("dd/MM/yyyy HH:mm:ss"));
            NomadBatch.Trace("FechaFin : " + objParams.FechaFin.AddMonths(1).ToString("dd/MM/yyyy HH:mm:ss"));
            NomadBatch.Trace("-----------------------------------------------");
      this.objBatch.SetPro(10);
      clsPreLoader objPreLoad = new clsPreLoader(objParams.FechaInicio.AddMonths(-1), objParams.FechaFin.AddMonths(1));
      objPreLoad.SetLegajos(objParams.PersonalList);
      objPreLoad.LoadData();
      NomadProxy.GetProxy().CacheAdd("HopePreLoader", objPreLoad);

            //Recorre los legajos
            this.objBatch.Log("Total de legajos a procesar: " + objParams.CantLegajos.ToString());
            for (int x = 0; x < objParams.CantLegajos; x++) {
                xelLegajo = (XmlElement) objParams.Legajos.ChildNodes.Item(x);

                this.objBatch.SetPro(20, 90, objParams.CantLegajos, x);
        NomadBatch.Trace(" ");
        NomadBatch.Trace(" ");
        NomadBatch.Trace("----------------------------------------------------------------------------------------------------------------------------------");
                this.objBatch.SetMess("Procesando el legajo " + xelLegajo.GetAttribute("e_numero_legajo") + " (" + ((int)(x + 1)).ToString() + " de " + objParams.CantLegajos.ToString() + " legajos).");

                try {

                    //Genera la liqudación para un personal
                    ProcesarLegajo (xelLegajo.Attributes["id"].Value, objParams, pstrLiquidacion, xelLegajo, htaTotalPersonalList);

                    //Fuerza la llamada al Garvage Collector
                    GC.Collect();

                } catch (Exception ex) {
                    this.objBatch.Err("Error en el legajo '" + xelLegajo.GetAttribute("e_numero_legajo") + "'. " + ex.Message);

                    xelLegajo.SetAttribute("status", "E");
                    xelLegajo.SetAttribute("err_desc", ex.Message);
                }

            }

            //Crea la transacción y Graba los cambios
            this.objBatch.Log("Marcando el procesamiento como 'PROCESADO'.");

            //Una vez que se ejecuta el procesamiento, no importa el resultado, se lo marca con estado 'pro'
            LIQUIDACION objLiq = LIQUIDACION.Get(pstrLiquidacion);
            objLiq.c_estado = "pro";
            //objLiq.Hash = GetLiqHash(pstrLiquidacion);

            NomadTransaction objTransaction = Nomad.NSystem.Base.NomadEnvironment.GetCurrentTransaction();
      objTransaction.Save(objLiq);

      objParams.FinalizaProceso = DateTime.Now;

      TimeSpan TotalProc = objParams.FinalizaProceso.Subtract(objParams.ComienzaProceso);

            this.objBatch.SetPro(100);
            this.objBatch.Log("Procesamiento finalizado.");
            this.objBatch.Log("Legajos procesados: " + objParams.CantLegajos.ToString());
            this.objBatch.Log("Legajos previamente procesados (sin cambios): " + ((int) (objParams.CantLegajos - objParams.CantLegReproc)).ToString());
            this.objBatch.Log("Fichadas procesadas: " + objParams.CantFichadas.ToString());
            this.objBatch.Log("Tiempo total: " + TotalProc.Hours.ToString() + ":" + TotalProc.Minutes.ToString() + " (total minutos: " + TotalProc.TotalMinutes.ToString() + ")");
            this.objBatch.SetMess("Procesamiento finalizado.");

            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace(" El procesamiento de Horas ha finalizado ---------------------------------");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            return null;
        }

        /// <summary>
        /// Devuelve el DDO de LiquidacionPers, si no existe lo crea
        /// </summary>
        /// <param name="pstrOiPersonal"></param>
        /// <param name="hasPersonalList"></param>
    private LIQUIDACIONPERS GetLiqPer(Hashtable phasPersonalList, string pstrOiPersonal, string pstrLiquidacion) {
      return this.GetLiqPer(phasPersonalList, pstrOiPersonal, pstrLiquidacion, true);
    }

    /// <summary>
        /// Devuelve el DDO de LiquidacionPers, si no existe lo crea
        /// </summary>
        /// <param name="pstrOiPersonal"></param>
        /// <param name="hasPersonalList"></param>
    /// <param name="pbolCreate"></param>
    private LIQUIDACIONPERS GetLiqPer(Hashtable phasPersonalList, string pstrOiPersonal, string pstrLiquidacion, bool pbolCreate) {

            LIQUIDACIONPERS objLiqPers;

      //Determina si la LiquidaciopnPers existe
            if (phasPersonalList.ContainsKey(pstrOiPersonal)) {
                //El ddo existe, lo pide
                objLiqPers = LIQUIDACIONPERS.Get((string) phasPersonalList[pstrOiPersonal]);

                //Se blanquea el mensaje de error en el proceso.
                objLiqPers.d_tipo_mensajeNull = true;
                objLiqPers.o_mensajeNull = true;

                //Recorre los jornales y los elimina uno a uno
                while (objLiqPers.LIQUIDACJOR.Count > 0)
                     objLiqPers.LIQUIDACJOR.Remove(objLiqPers.LIQUIDACJOR[0]);

                //Recorre los conceptos calculados y los elimina uno a uno
                while (objLiqPers.CONC_PER.Count > 0)
                    objLiqPers.CONC_PER.Remove(objLiqPers.CONC_PER[0]);

            } else {
        if (pbolCreate) {
          //El ddo no existe, lo crea
          objLiqPers = new LIQUIDACIONPERS();
          objLiqPers.oi_personal_emp = pstrOiPersonal;
          objLiqPers.oi_liquidacion = pstrLiquidacion;
        } else
          objLiqPers = null;
            }

            return objLiqPers;

        }

    /// <summary>
    /// Realiza una validación no exaustiva de las fichadas.
    /// </summary>
    /// <param name="pobjParams">Objeto de parámetros.</param>
    /// <param name="pnxmFichadas">XML con las fichadas.</param>
    /// <returns></returns>
    private bool LigthValidation(clsParams pobjParams, NomadXML pnxmFichadas) {
      bool     bolEnter;
      bool     bolEnterAnt = true;
      bool     bolFirstTime = true;
      DateTime dteFechaFichada;
      DateTime dteFechaFichadaAnt = new DateTime();
      TimeSpan tsDiff;

      //Si el procesamiento es PARCIAL se elimina la última fichada en caso de ser de entrada (de las no anuladas)
      if (pobjParams.Parcial)
      {
        if (pnxmFichadas.FirstChild().ChildLength > 0)
        {
          NomadXML xmlFichada = pnxmFichadas.FirstChild().LastChild();
          if (xmlFichada.GetAttr("c_tipo") == "E")
          {
            pnxmFichadas.FirstChild().DeleteChild(xmlFichada);
          }
        }
      }

    //Recorre los registros de las fichadas
      for (NomadXML nxmFichada = pnxmFichadas.FirstChild().FirstChild(); nxmFichada != null; nxmFichada = nxmFichada.Next()) {

        //Valida que las fichadas tengan el estado correcto
        if (nxmFichada.GetAttr("c_estado") == "E" || nxmFichada.GetAttr("c_tipo") == "I")
          return false;

        //Lee algunos parametros de la fichada
        dteFechaFichada = StringUtil.str2date(nxmFichada.GetAttr("f_fechahora"));
        bolEnter = nxmFichada.GetAttr("c_tipo") == "E";

        if (bolFirstTime == false) {

          //Determina si son fichadas consecutivas del mismo tipo
          if (bolEnter == bolEnterAnt)
            return false;

          tsDiff = dteFechaFichada.Subtract(dteFechaFichadaAnt);

          if (bolEnter) {
            //Es una fichada de ENTRADA
            //Compara contra la anterior y determina si salió y volvio a entrar "rápidamente"
            if (tsDiff <= pobjParams.DifEgrIng)
              return false; //Es una salida "corta", se deberian anular ambas fichadas

          } else {
            //Es una fichada de SALIDA

            //Determina si el rango entre fichadas no supera el tiempo máximo dentro del lugar
            if (tsDiff > pobjParams.MaxDifIngEgr)
              return false;
          }

        } else {

          bolFirstTime = false;
        }

        dteFechaFichadaAnt = dteFechaFichada;
        bolEnterAnt = bolEnter;
      }

      return true;
    }

    /// <summary>
    /// Valida las fichadas de un personal en particular
    /// </summary>
    /// <param name="pobjParams"></param>
    /// <param name="pxelLegajo"></param>
    private void ValidarLegajo(clsParams pobjParams, XmlElement pxelLegajo) {
      ArrayList arrHope;
      NomadObjectList nolFichadas;
      clsHito objHito = null;

      //Limpia los arrays de rangos disponibles para fichadas
      this.arrIngresos = null;
      this.arrEgresos = null;
      this.arrRangos = null;
      NomadProxy.GetProxy().CacheDel("GetHOPEPer");

      pxelLegajo.SetAttribute("status", "P");

      //Obtiene las fichadas
      nolFichadas = GetFichadasIngDDO(pobjParams.FechaInicio, pobjParams.FechaFin, pxelLegajo.GetAttribute("id"));
      if (nolFichadas.Count == 0) {
        pxelLegajo.SetAttribute("err_desc", "No se han encontrado fichadas.");
        return;
      }

      //Utiliza la clasificacion Agil si así se indico en el parametro
      if(pobjParams.TipoClasif == "A")
        PreValidarLegajo(nolFichadas, pobjParams);

      //Sumariza las fichadas procesadas
      pobjParams.CantFichadas = pobjParams.CantFichadas + nolFichadas.Count;

      //Puede que existan relojes que no indican si los eventos son de Entrada o Salida
      if (pobjParams.RelojIndicaEventos == false) {

        //Se obtiene la ESPERANZA para el legajo. Se pide la esperanza para un dia antes y otro despues.
        arrHope = ESPERANZAPER.GetDaysHope(pobjParams.FechaInicio.AddDays(-1), pobjParams.FechaFin.AddDays(1), int.Parse(pxelLegajo.GetAttribute("id")));
        if (arrHope.Count == 0) {
          this.objBatch.Wrn("No se ha encontrado esperanza para el legajo " + pxelLegajo.Attributes["e_numero_legajo"].Value + ".");
          pxelLegajo.SetAttribute("status", "N");
          pxelLegajo.SetAttribute("err_desc", "No se ha encontrado esperanza.");
          return;
        }

        //Genera la lista de hitos de esperanza
        objHito = GenerateTimeChain(pobjParams, arrHope, int.Parse(pxelLegajo.GetAttribute("id")));

        //Ejecuta el paso 0
        VFStep0(pobjParams, pxelLegajo, objHito, nolFichadas);
      }

      //Ejecuta el paso 1
      if (VFStep1 (pobjParams, nolFichadas)) {

        if (pobjParams.NeedsStep2) {
          if (objHito == null) {

            //Se obtiene la ESPERANZA para el legajo.
            arrHope = ESPERANZAPER.GetDaysHope(pobjParams.FechaInicio.AddDays(-1), pobjParams.FechaFin.AddDays(1), int.Parse(pxelLegajo.GetAttribute("id")));
            if (arrHope.Count == 0) {
              this.objBatch.Wrn("No se ha encontrado esperanza para el legajo " + pxelLegajo.Attributes["e_numero_legajo"].Value + ".");
              pxelLegajo.SetAttribute("status", "N");
              pxelLegajo.SetAttribute("err_desc", "No se ha encontrado esperanza.");
              return;
            }

            //Si no está generada la cadena de hitos la genera
            objHito = GenerateTimeChain(pobjParams, arrHope, int.Parse(pxelLegajo.GetAttribute("id")));
          }

          //Ejecuta el paso 2
          VFStep2(pobjParams, objHito, nolFichadas, pxelLegajo);
        }

      } else {
        throw new Exception("Error en el primer paso de la verificación.");
      }

      LogHito(objHito);

    }

    /// <summary>
    /// Valida las fichadas de forma agil (sin analizar las fichadas)
    /// </summary>
    /// <param name="nolFichadas">Lista con las fichadas a procesar</param>
    /// <param name="clsParams">Objeto de parametros del proceso</param>
    private void PreValidarLegajo(NomadObjectList pnolFichadas, clsParams pobjParams)
    {
      FICHADASING objFichada;
      FICHADASING objFichadaAnt = null;
      FICHADASING objFichadaAntAnt = null;

      bool bolNeedSave = false;
      string strTempTipo;

      TimeSpan tsDif;

      int intMilsRange = int.Parse(((double)(pobjParams.MinRange * 60000d)).ToString()); //60 *1000

      //--------------------------------------------------------------------------------------
      // Recorre las fichadas buscando alguna "Indefinida (c_tipo = 'I')"
      // En caso de existir indefinidas las guarda en un Array.
      //--------------------------------------------------------------------------------------

      NomadBatch.Trace("Clasificando fichadas...");

      //Crea la transacción
      NomadTransaction objTransaction = Nomad.NSystem.Base.NomadEnvironment.GetCurrentTransaction();
      objTransaction.Begin();

      //Recorre las FICHADAS
      for (int x = 0; x < pnolFichadas.Count; x++)
      {
      objFichada = (FICHADASING)pnolFichadas[x];
      NomadLog.Debug("objFichada -- " + objFichada.SerializeAll());
      if(objFichada.c_estado == "A")
        continue;
      tsDif = pobjParams.FechaInicioORIG.Subtract(objFichada.f_fechahora);

      //Pregunta si la fichada es anterior al rango
      if (objFichada.f_fechahora < pobjParams.FechaInicioORIG)
        objFichadaAnt = objFichada;
      else
      {
        if (objFichadaAnt == null)
        {
        //No se encontró una fichada anterior
        objFichada.c_tipo = "E";
        objFichada.l_entrada = true;
        }
        else
        {
        if (objFichadaAnt.c_tipo == "I" )
        {
          //Tira un error porque la última fichada antes de comenzar el rango de fechas es INDEFINIDA
          throw new Exception("La fichada previa al rango de validación es de tipo INDEFINIDA. Debe tener un tipo válido.");
        }

        strTempTipo = objFichada.c_tipo;
        if(objFichadaAnt.c_tipo == "S")
        {
          objFichada.c_tipo = "E";
          objFichada.l_entrada = true;
        }
        else
        {
          objFichada.c_tipo = "S";
          objFichada.l_entrada = false;
        }
        if (objFichada.c_tipo != strTempTipo)
          bolNeedSave = true;

        //Calcula las fechas de la fichada actual y la de la anterior
        tsDif = objFichada.f_fechahora.Subtract(objFichadaAnt.f_fechahora);

        //Determina si son fichadas seguidas.
        if (tsDif.TotalMilliseconds < intMilsRange)
        {
          objFichada.c_estado = "A";
          objFichada.o_observaciones = "Existen dos fichadas marcadas seguidas. Pertenece al rango del parámtero 'MinRange'.";
          bolNeedSave = true;
        }
        else
        {
          if (objFichadaAnt.c_tipo == "S")
          {
          if (tsDif <= pobjParams.DifEgrIng)
          {
            //Es una salida "corta", se anulan las dos fichadas
            objFichada.c_estado = "A";
            objFichada.o_observaciones = "Es una salida 'corta'. Pertenece al rango del parámtero DifEgrIng.";
            objFichadaAnt.c_estado = "A";
            objFichadaAnt.o_observaciones = "Es una salida 'corta'. Pertenece al rango del parámtero DifEgrIng.";
            bolNeedSave = true;
          }
          }
        }
        }

        //Si la última fichada no fué anulada se utilizan los datos
        if (objFichada.c_estado != "A")
        {
        objFichadaAntAnt = objFichadaAnt;
        objFichadaAnt = objFichada;
        }
        else
        objFichadaAnt = objFichadaAntAnt;
      }
      }
      if (bolNeedSave)
      objTransaction.Commit();
      else
      objTransaction.Rollback();
    }

        private void VFStep0(clsParams pobjParams, XmlElement pxelLegajo, clsHito pobjHito, NomadObjectList pnolFichadas)  {

            FICHADASING objFichada;
            ArrayList arrFichadasI = new ArrayList();

            //--------------------------------------------------------------------------------------
            // Recorre las fichadas buscando alguna "Indefinida (c_tipo = 'I')"
            // En caso de existir indefinidas las guarda en un Array.
            //--------------------------------------------------------------------------------------

            NomadBatch.Trace("Ejecuta el paso 0.");

            //Recorre las FICHADAS
            for (int x = 0; x < pnolFichadas.Count; x++) {
                objFichada = (FICHADASING) pnolFichadas[x];

                //Solo utiliza las INDEFINIDAS NO ANULADAS
                if (objFichada.c_estado != "A" && objFichada.c_tipo == "I")
                    arrFichadasI.Add(objFichada);

            }

            //--------------------------------------------------------------------------------------
            // Recorre el array de fichadas indefinidas
            //--------------------------------------------------------------------------------------
            if (arrFichadasI.Count > 0) {

                //Carga los arrays con los rangos
                CargarRangosFichadas(pobjHito);

                clsPunto objP = new clsPunto();
                int intPos = 0;
                bool bolNeedSave = false;

                //Crea la transacción
                NomadTransaction objTransaction = Nomad.NSystem.Base.NomadEnvironment.GetCurrentTransaction();
                objTransaction.Begin();

                if (this.arrRangos.Count != 0) {

                    //Recorre las fichadas y las compara contra las entradas y salidas posibles
                    for (int x = 0; x < arrFichadasI.Count; x++) {

                        objFichada = (FICHADASING) arrFichadasI[x];

                        if (objFichada.c_estado == "A") continue; //NO utiliza las ANULADAS

                        //Setea los primeros puntos de entrada y salida para los FIJOS y ROTATIVOS
                        if (intPos < this.arrRangos.Count) objP = (clsPunto) this.arrRangos[intPos];

                        //Recorre las fichadas y determina si es una entrada o una salida
                        while (intPos < arrRangos.Count) {
                            if (objFichada.f_fechahora < objP.FechaInicio) {
                                break;
                            } else {
                                if (objFichada.f_fechahora >= objP.FechaInicio && objFichada.f_fechahora <= objP.FechaFin) {
                                    //La fichada se encuentra dentro de algún rango
                                    if(objP.Entrada)
                                    {
                                      objFichada.c_tipo = "E";
                                      objFichada.l_entrada = true;
                                    }
                                    else
                                    {
                                      objFichada.c_tipo = "S";
                                      objFichada.l_entrada = false;
                                    }

                                    break;
                                } else {
                                    intPos++;
                                    try {objP = (clsPunto) arrRangos[intPos];} catch {}
                                }
                            }
                        }

                        if (objFichada.c_tipo != "I") {
                            //Se modificó el tipo a la fichada. Se Guarda
                            objTransaction.Save(objFichada);
                            bolNeedSave = true;
                        }

                    }
                }

                //Persiste las modificaciones
                if (bolNeedSave)
                    objTransaction.Commit();
                else
                    objTransaction.Rollback();

            }

            return;
        }

        /// <summary>
        /// Ejecuta el paso 1 de la validación de FICHADAS
        /// Solamente anula los registros con problemas sencillos.
        /// </summary>
        /// <param name="pobjParams"></param>
        /// <param name="pnolFichadas"></param>
        /// <returns></returns>
        private bool VFStep1(clsParams pobjParams, NomadObjectList pnolFichadas) {
            FICHADASING objFichada;
            FICHADASING objPrevFichada = null;
            FICHADASING objLastFichada = null;

      NomadBatch.Trace("Ejecuta el paso 1.");

      //Parámetros
      int intMilsRange;

            //Diferencia entre una salida y la entrada anterior.
            System.TimeSpan tsDiffSE = new TimeSpan(0, 15, 0);

            intMilsRange = int.Parse (((double)(pobjParams.MinRange * 60000d)).ToString()); //60 *1000

            //Variables para el proceso
            int intStepErrs = this.intContErr;
            System.TimeSpan tsDiff;
            bool blnToCommit = false;
            bool blnNextCorrect = true; //Indica que la segiguiente fichada se toma como correcta
            DateTime dteFechaFichadaAnt = new DateTime(1900, 1, 1, 0, 0, 0); //Fecha de la fichada anterior
            DateTime dteFechaFichada;  //Fecha de la fichada actual
            bool bolKeepPrev;
            bool bolPrimerFichada = true;

            //Recorre las fichadas
            for (int x = 0; x < pnolFichadas.Count; x++) {
                bolKeepPrev = false;
                this.intContTotal++;

                objFichada = (FICHADASING) pnolFichadas[x];

                //Solo utiliza las no ANULADAS
                if (objFichada.c_estado == "A")
                    continue;

                //Verifica que no esté indefinida
                if (objFichada.c_tipo == "I") {
                    objFichada.c_estado = "E";
                    objFichada.o_observaciones = "No se pude validar una fichada de tipo 'INDEFINIDA'.";
                    this.objBatch.Err("Se encontró una fichada tipo 'INDEFINIDA'.");
                    this.intContErr++;
                    blnToCommit = true;
                    blnNextCorrect = true;
                    continue;
                }

                //Limpia las observaciones
                if (pobjParams.NeedsStep2 == false)
          blnToCommit = blnToCommit || (objFichada.o_observaciones != "") || (objFichada.c_estado != "C");

                objFichada.o_observaciones = "";
                objFichada.c_estado = "C";

                //Almaceno la Ultima Fichada BUENA
                objLastFichada = objFichada;

                if (bolPrimerFichada) {
                    //Si la primer fichada es de SALIDA y NO corresponde al día INICIO de pedido de validación => Tira error
                    if (objFichada.c_tipo == "S" && objFichada.f_fechahora.Day != pobjParams.FechaInicio.Day) {
                        objFichada.c_estado = "E";
                        objFichada.o_observaciones = "Se encontró una fichada de SALIDA cuando se esperaba una de ENTRADA.";
            this.objBatch.Err("Se encontró una fichada de SALIDA cuando se esperaba una de ENTRADA.");
            this.intContErr++;
            blnToCommit = true;
            blnNextCorrect = true;
                      continue;
                    }
                    bolPrimerFichada = false;
                }

                //Pregunta si se toma la fichada como correcta
                if (blnNextCorrect) {
                    objPrevFichada = objFichada;
                    dteFechaFichadaAnt = objPrevFichada.f_fechahora;
                    blnNextCorrect = false;
                    continue;
                }

                //Calcula las fechas de la fichada actual y la de la anterior
                dteFechaFichada = objFichada.f_fechahora;
                dteFechaFichadaAnt = objPrevFichada.f_fechahora;
                tsDiff = dteFechaFichada.Subtract(dteFechaFichadaAnt);

                //Determina si son dos fichadas del mismo tipo
                if (objFichada.c_tipo == objPrevFichada.c_tipo) {

                    //Determina si son dos fichadas marcadas seguidas
                    if (tsDiff.TotalMilliseconds < intMilsRange) {
                        objFichada.c_estado = "A";
                        objFichada.o_observaciones = "Existen dos fichadas marcadas seguidas. Pertenece al rango del parámtero 'MinRange'.";
                    } else {
                        //Error: Dos fichadas consecutivas del mismo tipo
                        objFichada.c_estado = "E";
                        objFichada.o_observaciones = "Se encontraron dos fichadas consecutivas del mismo tipo (Segunda del par).";

                        objPrevFichada.c_estado = "E";
                        objPrevFichada.o_observaciones = "Se encontraron dos fichadas consecutivas del mismo tipo (Primera del par).";

                        this.objBatch.Err("Se encontraron dos fichadas consecutivas del mismo tipo.");

                        this.intContErr = this.intContErr + 2;
                        blnToCommit = true;
                        blnNextCorrect = true;
                    }
                } else {
                    //La fichada actual es diferente que la anterior
                    if (objFichada.c_tipo == "S") {
                        //La fichada es de SALIDA

                        //Pregunta si se sobrepasa el tiempo máximo dentro del lugar
                        if (tsDiff > pobjParams.MaxDifIngEgr) {
                            //Sobrepasa el tiempo máximo dentro del lugar
                            objFichada.c_estado = "E";
                            objFichada.o_observaciones = "Se sobrepasó el tiempo máximo dentro del lugar.";
                            this.objBatch.Err("Se sobrepasó el tiempo máximo dentro del lugar.");
                            this.intContErr++;
                            blnToCommit = true;
                            blnNextCorrect = true;
                        }

                    } else {
                        //La fichada es de ENTRADA

                        //Compara contra la anterior y determina si salió y volvio a entrar "rápidamente"
                        if (tsDiff <= pobjParams.DifEgrIng) {
                            //Es una salida "corta", se anulan las dos fichadas
                            objFichada.c_estado = "A";
                            objFichada.o_observaciones = "Es una salida 'corta'. Pertenece al rango del parámtero DifEgrIng.";
                            objPrevFichada.c_estado = "A";
                            objPrevFichada.o_observaciones = "Es una salida 'corta'. Pertenece al rango del parámtero DifEgrIng.";

                            if (x > 2) {
                                objPrevFichada = (FICHADASING) pnolFichadas[x - 2];
                                bolKeepPrev = true;
                            } else
                                blnNextCorrect = true;
                        }
                    }

                    if (!bolKeepPrev)
                        objPrevFichada = objFichada;
                }
            }

            if ((objLastFichada!=null) && (objLastFichada.c_tipo == "E") && (!pobjParams.Parcial))
            {
              if (objLastFichada.f_fechahora<pobjParams.FechaFin)
              {
                //No se encontro la Fichada de Salida.
                objLastFichada.c_estado = "E";
                objLastFichada.o_observaciones = "No se encontro la Fichada de Salida.";
                this.objBatch.Err("No se encontro la Fichada de Salida.");
                this.intContErr++;
                blnToCommit = true;
              }
            }

            //Se produjeron cambios, es necesario grabar.
            if (blnToCommit) {

                //Crea la transacción
                NomadTransaction objTransaction = Nomad.NSystem.Base.NomadEnvironment.GetCurrentTransaction();
                objTransaction.Begin();

                //Recorre las fichadas y las comitea en caso de existir error
                for (int x = 0; x < pnolFichadas.Count; x++) {
                    objFichada = (FICHADASING) pnolFichadas[x];
                    objTransaction.Save(objFichada);
                }

                objTransaction.Commit();
            }

            return this.intContErr == intStepErrs;
        }

        /// <summary>
        /// Ejecuta el paso 2 de la validación
        /// </summary>
        /// <param name="pobjParams"></param>
        /// <param name="pobjHito"></param>
        /// <param name="pnolFichadas"></param>
        /// <param name="pxelLegajo"></param>
        /// <returns></returns>
        private bool VFStep2(clsParams pobjParams, clsHito pobjHito, NomadObjectList pnolFichadas, XmlElement pxelLegajo)  {
            Hashtable htaFreeHope = new Hashtable();
            string strErrDesc = "";
            bool bolPrimerFichada = true; //Indica si es la primer fichada VÁLIDA

          NomadBatch.Trace("Ejecuta el paso 2.");

            FICHADASING objFichada;
            FICHADASING objUltimaFichada = null;
            clsPunto objPI = new clsPunto();
            clsPunto objPE = new clsPunto();
            int intPosI = 0;
            int intPosE = 0;
            string strOiStruct;

            bool Errors = false;

            //Crea la transacción
            NomadTransaction objTransaction = NomadEnvironment.GetCurrentTransaction();
            objTransaction.Begin();

            //Carga los arrays con los rangos
            CargarRangosFichadas(pobjHito);

            if (this.arrIngresos.Count != 0 && this.arrEgresos.Count != 0) {

                bool IsOK;
                //Recorre las fichadas y las compara contra las entradas y salidas posibles
                for (int x = 0; x < pnolFichadas.Count; x++) {

                    objFichada = (FICHADASING) pnolFichadas[x];
                    IsOK = true;

                    if (objFichada.c_estado == "A") continue; //Solo utiliza las NO ANULADAS

                    //Verifica las fichadas de los EXTREMOS (VERIFICA LA PRIMER FICHADA)
                    if (bolPrimerFichada) {
                        //Si la primer fichada es de SALIDA y NO corresponde al día INICIO de pedido de validación => Tira error
                        if (objFichada.c_tipo == "S" && objFichada.f_fechahora.Day != pobjParams.FechaInicio.Day) {
                            IsOK = false;
                            strErrDesc = "Se encontró una fichada de SALIDA cuando se esperaba una de ENTRADA.";
                        }
                        bolPrimerFichada = false;
                    }

                    if (IsOK) {
                        //Obtiene la estructura desde donde se realizó la fichada
                        strOiStruct = pobjParams.GetStruct(objFichada.oi_terminal.ToString());
                        if (strOiStruct == null) {
                            IsOK = false;
                            strErrDesc = "La fichada no corresponde con una Estructura válida.";

                        } else {

                            IsOK = false;
                            strErrDesc = "La fichada " + objFichada.f_fechahora.ToString("dd/MM HH:mm") + " no se encuentra dentro de los rangos esperados.{" + objFichada.Id + "}";

                            //Setea los primeros puntos de entrada y salida
                            if (intPosI < this.arrIngresos.Count) objPI = (clsPunto) this.arrIngresos[intPosI];
                            if (intPosE < this.arrEgresos.Count) objPE = (clsPunto) this.arrEgresos[intPosE];

                            if (objFichada.c_tipo == "E") {
                                //Fichada de tipo Ingreso (ENTRADA)
                                while (intPosI < this.arrIngresos.Count) {
                                    if (objFichada.f_fechahora < objPI.FechaInicio) {
                                        break;
                                    } else {
                                        if (objFichada.f_fechahora >= objPI.FechaInicio && objFichada.f_fechahora <= objPI.FechaFin) {

                                            if (objPI.OiEstructura == "" || objFichada.oi_terminalNull) {
                                                //El rango no tiene estructura. No se controla.
                                                IsOK = true;
                                            } else {
                                                //Valida si la fichada corresponde con un oi estructura esperado
                                                if (objPI.OiEstructura != strOiStruct) {
                                                    IsOK = false;
                                                    strErrDesc = "La fichada no corresponde con una Estructura esperada. Se esperaba en la '" + GetUniOrg(objPI.OiEstructura) + "' y se marcó en la '" + this.GetUniOrg(strOiStruct) + "'";
                                                } else {
                                                    IsOK = true;
                                                }
                                            }
                                            break;
                                        } else {
                                            intPosI++;
                                            try {objPI = (clsPunto) this.arrIngresos[intPosI];} catch {}
                                        }
                                    }
                                }

                            } else {
                                //Fichada de tipo Egreso
                                while (intPosE < this.arrEgresos.Count) {
                                    if (objFichada.f_fechahora < objPE.FechaInicio) {
                                        break;
                                    } else {
                                        if (objFichada.f_fechahora >= objPE.FechaInicio && objFichada.f_fechahora <= objPE.FechaFin) {
                                            if (objPI.OiEstructura == "" || objFichada.oi_terminalNull) {
                                                //El rango no tiene estructura. No se controla.
                                                IsOK = true;
                                            } else {
                                                if (objPI.OiEstructura != strOiStruct) {
                                                    IsOK = false;
                                                    strErrDesc = "La fichada no corresponde con una Estructura esperada. Se esperaba en la '" + this.GetUniOrg(objPI.OiEstructura) + "' y se marcó en la '" + this.GetUniOrg(strOiStruct) + "'";
                                                } else {
                                                    IsOK = true;
                                                }
                                            }
                                            break;
                                        } else {
                                            intPosE++;
                                            try {objPE = (clsPunto) this.arrEgresos[intPosE];} catch {}
                                        }
                                    }
                                }
                            }

                        }
                    }

                    if (IsOK == false) {
                        //La fichada no es correcta. Debe marcarla como error
                        objFichada.c_estado = "E";
                        objFichada.o_observaciones = strErrDesc.Substring(0, strErrDesc.IndexOf('{'));

                        objTransaction.Save(objFichada);
                        this.objBatch.Err("Se encontró una fichada que no corresponde con la esperanza.");
                        Errors = true;

                        //Actualiza el xml de retorno
                        pxelLegajo.SetAttribute("status", "E");
            if(pxelLegajo.GetAttribute("err_desc") != "")
              pxelLegajo.SetAttribute("err_desc", pxelLegajo.GetAttribute("err_desc") + "," + strErrDesc);
            else
              pxelLegajo.SetAttribute("err_desc", strErrDesc);

                        this.intContErr++;

                    } else {
                        //Si no tiene errores lo agrega a la transaccion para limpiarle el estado y las observaciones
                        objFichada.c_estado = "C";
                        objFichada.o_observaciones = "";
                        objTransaction.Save(objFichada);
                    }

                    objUltimaFichada = objFichada;
                }

                //Verifica las fichadas de los EXTREMOS (VERIFICA LA ÚLTIMA FICHADA)
                if (objUltimaFichada != null) {
                    //Si la úaltima fichada es de ENTRADA y NO corresponde al día FIN de pedido de validación => Tira error
                    if (objUltimaFichada.c_tipo == "E" && objUltimaFichada.f_fechahora.Day != pobjParams.FechaInicio.Day && !pobjParams.Parcial) {
                        IsOK = false;
                        strErrDesc = "Se encontró una fichada de ENTRADA cuando se esperaba una de SALIDA.";
                    }
                }

            }

            //Persiste las modificaciones
            //En caso de que no exista ningún error se setea la propiedad l_verificado_ok
            if (Errors == false) {
                //No existen errores. Se marca a todos los DDO como verificados ok

                objTransaction.Rollback();
                objTransaction.Begin();

                //Recorre las fichadas y las compara contra las entradas y salidas posibles
                for (int x = 0; x < pnolFichadas.Count; x++) {
                    objFichada = (FICHADASING) pnolFichadas[x];

                    if (objFichada.c_estado != "A") {
                        objFichada.c_estado = "C";
                        objFichada.o_observaciones = "";
                    }

                    objTransaction.Save(objFichada);
                }

                pxelLegajo.SetAttribute("status", "P");
            }

            objTransaction.Commit();

            return true;
        }

        /// <summary>
        /// Retorna un ArrayList con las fichadas (Objetos)
        /// </summary>
        /// <param name="pdteFechaInicio"></param>
        /// <param name="pdteFechaFin"></param>
        /// <param name="pstrID"></param>
        /// <returns></returns>
        private NomadObjectList GetFichadasIngDDO(DateTime pdteFechaInicio, DateTime pdteFechaFin, string pstrID) {
      return this.GetFichadasIngDDO(pdteFechaInicio, pdteFechaFin, pstrID, true);
        }

        /// <summary>
        /// Retorna un ArrayList con las fichadas (Objetos)
        /// </summary>
        /// <param name="pdteFechaInicio"></param>
        /// <param name="pdteFechaFin"></param>
        /// <param name="pstrID"></param>
        /// <param name="pbolAddDay"></param>
        /// <returns></returns>
        private NomadObjectList GetFichadasIngDDO(DateTime pdteFechaInicio, DateTime pdteFechaFin, string pstrID, bool pbolAddDay) {
      //Crea el objeto de Parametros
      NomadXML objQParams = new NomadXML("DATOS");
      objQParams.SetAttr("oi_personal_emp", pstrID);
      objQParams.SetAttr("f_Inicio", StringUtil.date2str(pdteFechaInicio));
      objQParams.SetAttr("f_Fin", StringUtil.date2str( (pbolAddDay ? pdteFechaFin.AddDays(1d) : pdteFechaFin) ) );

      //Recupera las fichadas
      return NomadEnvironment.GetObjects(FICHADASING.Resources.QRY_FichadasIngDDO, objQParams.ToString(), typeof(FICHADASING));
        }

        /// <summary>
        /// Calcula los Array de entradas y salidas para compararlo con las fichadas
        /// </summary>
        /// <param name="pobjHito">Primer Hito de la cadena.</param>
        private void CargarRangosFichadas (clsHito pobjHito) {
            clsHito objWorkHito;
            clsHito objHitoDia;
            clsPunto objPunto;
            clsPunto objUltimoPuntoSinControl = null;

            //int status = 0; //0 = inicio // 1 = está dentro // 2 = está fuera
            int status = 2; //0 = inicio // 1 = está dentro // 2 = está fuera

            //Valida si es necesario cargar los Arrays
            if (this.arrIngresos != null || this.arrEgresos != null || this.arrRangos != null)
                return;

            objWorkHito = pobjHito;
            objHitoDia = pobjHito;
            this.arrIngresos = new ArrayList();
            this.arrEgresos = new ArrayList();
            this.arrRangos = new ArrayList();

            //Recorre TODOS LOS HITOS
            while (objWorkHito != null) {

                if (objWorkHito.TipoHito == TiposHito.InicioDia) {
                        objHitoDia = objWorkHito;
                        objWorkHito = objWorkHito.Siguiente;

                        if (objHitoDia.ControlaFichada) {
                            if (objUltimoPuntoSinControl != null) {
                                //Es el primer día que valida después de otro que no validaba.
                                objUltimoPuntoSinControl.FechaFin = objHitoDia.FechaHora.AddSeconds(-1d);
                                objUltimoPuntoSinControl = null;
                            }
                        } else {
                            //El día no controla las fichadas.
                            //Se crea un rango gigante que abarque todos los días enteros que no validan
                            if (objUltimoPuntoSinControl == null) {
                                objUltimoPuntoSinControl = new clsPunto(objHitoDia.FechaHora, objHitoDia.FechaHora.AddYears(10), "");
                                this.arrIngresos.Add(objUltimoPuntoSinControl);
                                this.arrEgresos.Add(objUltimoPuntoSinControl);
                                //this.arrRangos.Add(objUltimoPuntoSinControl);
                            }
                        }
                        continue;
                }

                //Se determina si se controlaran las fichadas para el día
                //if (objHitoDia.ControlaFichada) {

                if ( objWorkHito.TipoHora.EnAusencia == Sumarizaciones.EnAusenteExedente ) {
                    //La hora es de presencia obligatoria
                    if (status != 1) {
                        objPunto = new clsPunto(objWorkHito.FechaHora.AddMinutes(objHitoDia.TolPreIng * (-1)), objWorkHito.FechaHora.AddMinutes(objHitoDia.TolPosIng), objWorkHito.OiEstructura.ToString() );
                        objPunto.Entrada = true;

                        if (objHitoDia.ControlaFichada) this.arrIngresos.Add(objPunto);
                        this.arrRangos.Add(objPunto);

                        status = 1;
                    }
                } else {
                    if ( objWorkHito.TipoHoraAnterior.EnAusencia == Sumarizaciones.EnAusenteExedente ) {
                        if (status != 2) {
                            objPunto = new clsPunto(objWorkHito.FechaHora.AddMinutes(objHitoDia.TolPreEgr * (-1)), objWorkHito.FechaHora.AddMinutes(objHitoDia.TolPosEgr), objWorkHito.OiEstructura.ToString() );
                            objPunto.Entrada = false;

                            if (objHitoDia.ControlaFichada) this.arrEgresos.Add(objPunto);
                            this.arrRangos.Add(objPunto);

                            status = 2;
                        }
                    }
                }

                //}

                objWorkHito = objWorkHito.Siguiente;

            }

        }

        public string GetHASHProcesamientoLegajo(string pstrOiProcesamiento, string pstrOiLegajo, ref NomadXML pnxmFichadas) {
            clsParams objParams = new clsParams();
            return GetHASHProcesamientoLegajo(pstrOiProcesamiento, pstrOiLegajo, ref pnxmFichadas, objParams);
        }

        public string GetHASHProcesamientoLegajo(string pstrOiProcesamiento, string pstrOiLegajo, clsParams pobjParams) {
            NomadXML r = new NomadXML();
            return GetHASHProcesamientoLegajo(pstrOiProcesamiento, pstrOiLegajo, ref r, pobjParams);
        }

        public string GetHASHProcesamientoLegajo(string pstrOiProcesamiento, string pstrOiLegajo, ref NomadXML pnxmFichadas, clsParams pobjParams) {
            if (this.m_ejecConc == null)
              this.m_ejecConc = RHLiq.LiqConceptosBase.GetObject();
            string strAddInfo = this.m_ejecConc.MD5HASH + (pobjParams.Parcial ? "0" : "1");
            return pobjParams.GetHASHProLegajo(pstrOiProcesamiento, pstrOiLegajo, strAddInfo, ref pnxmFichadas);
        }

/*

        /// <summary>
        /// Retorna el hash para la liquidación.
        /// </summary>
        /// <param name="pstrLiquidacion">OI de liquidación.</param>
        /// <returns></returns>
        private string GetLiqHash(string pstrLiquidacion) {
            string strResult = "";

      //Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATOS");

      objQParams.SetAttr("oi_liquidacion", pstrLiquidacion);

            //Ejecuta el query
            strResult = m_Proxy.SQLService().Get(Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION.Resources.QRY_DatosHash, objQParams.ToString());

      strResult = StringUtil.GetMD5(strResult);

            return strResult;

        }

*/
        /// <summary>
        /// Devuelve la lista TOTAL de legajos ya involucrados en el procesamiento
        /// </summary>
        /// <param name="pstrOI_Liq"></param>
        /// <returns></returns>
        private Hashtable GetLiqPersList(string pstrOI_Liq) {
            string strResult = "";
            Hashtable htaResultado = new Hashtable();

       //Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATO");
      objQParams.SetAttr("oi_liquidacion", pstrOI_Liq);

            //Ejecuta el query
            strResult = NomadEnvironment.QueryString(LIQUIDACION.Resources.QRY_PERSONAL_EN_LIQ, objQParams.ToString());

            //Recorre los tipos de horas y los agrega en la colección
            XmlTextReader xtrLP = new XmlTextReader(strResult, System.Xml.XmlNodeType.Document, null);
            xtrLP.XmlResolver = null; // ignore the DTD
            xtrLP.WhitespaceHandling = WhitespaceHandling.None;
            xtrLP.Read();

            while (xtrLP.Read()) {
        if (!xtrLP.IsStartElement())
          continue;

                htaResultado.Add(xtrLP.GetAttribute("oi_personal_emp"), xtrLP.GetAttribute("oi_LiquidacionPers"));
            }

            return htaResultado;
        }

        /// <summary>
        /// Indica si las fichadas pasadas existen en otras liquidaciones ya cerradas.
        /// </summary>
        /// <param name="pstrOisRegis">String conteniedo los OI de las fichadas separadas por coma.</param>
        /// <param name="pstrOiLiquidacion">OI de el procesamiento.</param>
        /// <returns></returns>
        private string ValidateLiqRegisters(string pstrOisRegis, string pstrOiLiquidacion) {
            string strResult = "";

      //Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATOS");
      objQParams.SetAttr("oi_fichadasing", pstrOisRegis);
      objQParams.SetAttr("oi_liquidacion", pstrOiLiquidacion);

            //Ejecuta el query
            strResult = m_Proxy.SQLService().Get(LIQUIDACION.Resources.QRY_Contador_Fichadas_Usadas, objQParams.ToString());

            XmlDocument xmlLiqs = new XmlDocument();
            xmlLiqs.LoadXml(strResult);

            //Recorre las liquidaciones, en caso de existir, y las enumera.
      strResult = "";
            foreach (XmlNode xnoItem in xmlLiqs.DocumentElement.ChildNodes) {
               strResult = strResult + ((XmlElement)xnoItem).GetAttribute("c_liquidacion") + ", ";
            }

       strResult = strResult == "" ? "" : strResult.Substring(0, strResult.Length - 2);
            return strResult;

        }

        /// <summary>
        /// Indica si las fichadas pasadas existen en otras liquidaciones ya cerradas.
        /// </summary>
        /// <param name="pstrOisRegis">String conteniedo los oi de las fichadas separadas por coma.</param>
        /// <param name="pstrOiLiquidacion">Oi de liquidación.</param>
        /// <returns></returns>
        private ArrayList GetOiFichadas(string pstrOiLiquidacion, string pstrOiPersonalEmp) {
            string strResult = "";
            ArrayList arrResult = new ArrayList();

      //Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATOS");
      objQParams.SetAttr("oi_liquidacion", pstrOiLiquidacion);
      objQParams.SetAttr("oi_personal_emp", pstrOiPersonalEmp);

            //Ejecuta el query
            strResult = m_Proxy.SQLService().Get(LIQUIDACIONPERS.Resources.QRY_Fichadas08, objQParams.ToString());

            XmlDocument xmlLiqs = new XmlDocument();
            xmlLiqs.LoadXml(strResult);

            //Recorre las liquidaciones, en caso de existir, y las enumera.
            foreach (XmlNode xnoItem in xmlLiqs.DocumentElement.ChildNodes)
               arrResult.Add( ((XmlElement)xnoItem).GetAttribute("oi_fichada") );

            return arrResult;

        }

/*
        /// <summary>
        /// Obtiene el detalles de unas fichadas en particular.
        /// </summary>
        /// <param name="strOisRegis">String conteniedo los oi de las fichadas separadas por coma.</param>
        /// <returns></returns>
        private XmlDocument GetRegistersDetails(string strOisRegis) {
            string strResult = "";
            XmlDocument xmlResult;

      //Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATOS");
      objQParams.SetAttr("oi_fichadasing", strOisRegis);

            //Ejecuta el query
            strResult = m_Proxy.SQLService().Get(FICHADASING.Resources.QRY_FichadasDetalles, objQParams.ToString());

      xmlResult = new XmlDocument();
      xmlResult.LoadXml(strResult);

            return xmlResult;

        }
*/

        //Ejecuta el query para obtener las personas involucradas a la validación
        private string GetPersonalList(clsParams pobjParams) {
            return "";
        }

        /// <summary>
        /// Ejecuta el query para obtener una lista de personas
        /// </summary>
        /// <param name="pobjParams"></param>
        /// <returns></returns>
        private string GetPersonalListParam(string poi_Empresa) {
            string strResult = "";

      //Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATOS");
            if (poi_Empresa != null && poi_Empresa != "")
                objQParams.SetAttr("oi_empresa", poi_Empresa);

            //Ejecuta el query
            strResult = m_Proxy.SQLService().Get(Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.QRY_ListaDePersonal, objQParams.ToString());

            return strResult;

        }

    /// <summary>
    /// Genera la lista enlazada de hitos a partir de una esperanza dada
    /// </summary>
    /// <param name="pstrHope">String con formato XML de las esperanzas</param>
    /// <returns></returns>
    private clsHito GenerateTimeChain(clsParams pobjParams, ArrayList parrHope,int oi_personal_emp) {
      clsHito objHitoI = null;                  //Primer HITO. Se retornará como resultado del método.
      clsHito objLastHopeInDay = new clsHito(); //Ultimo HITO de esperanza dentro del día
      clsHito objLastHope = null;               //Ultimo HITO de esperanza (puede ser del mismo día o del anterior)
      clsHito objLastDay = null;                //Ultimo HITO de tipo DIA. Sirve para actualizarle la fecha fin

      NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA objDia;

      bool bolFirstOfDay;
      bool CreateFirstHito;

      clsHito objHitoDay = null;
      clsHito objFirstHito = null;
      clsHito objSecondHito;

      pobjParams.NeedsStep2 = false;
      NomadBatch.Trace("GenerateTimeChain inicio ...");

		
   	  NomadBatch.Trace("Cantidad de Dias: " + parrHope.Count);		  
	  
      //Recorre los DIAS
      for (int x = 0; x < parrHope.Count; x++) {
		NomadBatch.Trace("Recorre los DIAS: " + x);		  
		
        objDia = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA) parrHope[x];
        bolFirstOfDay = true;

        //Crea el HITO de DIA
        objHitoDay = new clsHito(pobjParams, objDia.f_dia, objDia.oi_horario, objDia.oi_turno, objDia.oi_licencia, objDia.c_tipo);
        //if (objDia.oi_licencia != "")
        //  NomadBatch.Trace("GenerateTimeChain() - oi_licencia del día " + objDia.f_dia.ToString("dd/MM/yyyy") + " - " + objDia.oi_licencia);

        if (objHitoI == null)
          objHitoI = objHitoDay;
        else
          objLastHopeInDay.Siguiente = objHitoDay;

        objLastHopeInDay = null;



        //Actualiza el parámetro de Requiere paso 2
        pobjParams.NeedsStep2 = pobjParams.NeedsStep2 || objHitoDay.ControlaFichada;

        //Recorre el DETALLE de cada uno de los DIAS
		
		int q = 0;
		
		NomadBatch.Trace("Recorre el DETALLE de cada uno de los DIAS");
        foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDetalle in objDia.DETALLE) {

			q++;
			NomadBatch.Trace("Dia: " + q);		  
			CreateFirstHito = true;

          if (!objDetalle.l_autorizada) continue;

          if (objLastHopeInDay != null) {
            if (objDetalle.e_horainicio == objLastHopeInDay.Minutos) {
              if (objDetalle.oi_tipohora == objLastHopeInDay.TipoHora.OI) {

                //Cuando dos o mas hitos seguidos son del mismo tipo de hora los une
                //El ejemplo muestra hitos de tipo de hora normal que son consecutivos
                // Hn1 Hn2 Hn3 Hn4 => Hn1 Hn4
                objLastHopeInDay.Minutos = objDetalle.e_horafin;
                continue;
              } else {

                //Cuando dos hitos seguidos tienen la misma hora pero son de distintos tipos de hora
                //se elimina el último y se agrega el nuevo
                // H1 H2 Ex3 Ex4 => H1 Ex3 Ex4

                //Le cambio el TipoHora que inicia al último HITO
                objLastHopeInDay.SetTipoHora(objDetalle.oi_tipohora);
              }

              CreateFirstHito = false;
            }
          }

          //Crea el SEGUNDO hito con datos para un tipo de hora "Excedente"
          objSecondHito = new clsHito(pobjParams,
                    objHitoDay.Dia, objDetalle.e_horafin,
                    clsTipoHora.Excedent, objDetalle.oi_tipohora,
                    objDetalle.oi_estructura);

          if (CreateFirstHito) {
            //Crea el PRIMER hito
            objFirstHito = new clsHito(pobjParams,
                      objHitoDay.Dia, objDetalle.e_horainicio,
                      objDetalle.oi_tipohora, clsTipoHora.Excedent,
                      objDetalle.oi_estructura);

            //Engancha el último con el primero de recién
            if (objLastHopeInDay != null)
              objLastHopeInDay.Siguiente = objFirstHito;

            //Engancha el primer hito con el segundo
            objFirstHito.Siguiente = objSecondHito;

            if (bolFirstOfDay)
              objHitoDay.Siguiente = objFirstHito;

          } else {
            //Engancha el último hito con el segundo
            objLastHopeInDay.Siguiente = objSecondHito;
          }

          //Setea el segundo hito como el último
          objLastHopeInDay = objSecondHito;

          //Calcula el inicio del DÍA
          //Se hace aqui porque es necesario saber el primer detalle
          if (bolFirstOfDay) {
            if (objFirstHito.FechaHora < objHitoDay.FechaHora) {
              //El inicio de la esperanza es menor al inicio del día. Se actualiza el del día.
              objHitoDay.FechaHora = objFirstHito.FechaHora;
            } else {
              if (objLastHope != null)
                if (objLastHope.FechaHora > objHitoDay.FechaHora) {
                  //El fin de la última esperanza del día anterior es mayor al inicio del día. Se actualiza el del día.
                  objHitoDay.FechaHora = objLastHope.FechaHora;
                }
            }
          }

          objLastHope = objSecondHito;
          bolFirstOfDay = false;
        }

        if (objLastHopeInDay == null) {

          if (objLastHope != null)
            if (objLastHope.FechaHora > objHitoDay.FechaHora) {
              //El fin de la última esperanza del día anterior es mayor al inicio del día. Se actualiza el del día.
              objHitoDay.FechaHora = objLastHope.FechaHora;
            }

            //No hubo detalles dentro de un día. Setea como objLastHopeInDay el hito día.
            objLastHopeInDay = objHitoDay;
        }

        //Calcula la fecha/hora fin del hito de tipo dia anterior
        if (objLastDay != null)
          objLastDay.SetRangeDay(objHitoDay.FechaHora);

        objLastDay = objHitoDay;
      }

      objLastDay = null;

      if (objHitoI.FechaHora >= pobjParams.FechaInicio)
      {
          Esperanzaper.DIA DiaAnterior = ESPERANZAPER.GetDayHope(pobjParams.FechaInicio.AddDays(-1), oi_personal_emp);

          if(DiaAnterior!=null)
          {
			  NomadBatch.Trace("DiaAnterior.DETALLE.Count = " + DiaAnterior.DETALLE.Count);		  
			  
			  if (DiaAnterior.DETALLE.Count > 0)
			  {
				  NomadBatch.Trace("(Esperanzaper.DETALLE)DiaAnterior.DETALLE[" + (DiaAnterior.DETALLE.Count - 1).ToString() +  "]");		  				  
				  
				  Esperanzaper.DETALLE det = (Esperanzaper.DETALLE)DiaAnterior.DETALLE[DiaAnterior.DETALLE.Count - 1];
				  DateTime fechaHoraUltimoDetalleAnterior = DiaAnterior.f_dia.AddMinutes(det.e_horafin);
				  if (fechaHoraUltimoDetalleAnterior > objHitoI.FechaHora)
				  {
					  //El fin de la última esperanza del día anterior es mayor al inicio del día. Se actualiza el del día.
					  objHitoI.FechaHora = fechaHoraUltimoDetalleAnterior;
				  }   
				  NomadBatch.Trace("acá legó ...");		  				  				  
			  }
          }

      }
     
      return objHitoI;
    }

      /// <summary>
      /// Aplica las fichadas sobre la lista enlazada de hitos
      /// </summary>
      /// <param name="pobjFirstHito">Primer hito de la seguidilla de hitos de la esperanza por persona</param>
      /// <param name="pnxmFichadas">Fichadas de la persona en particular</param>
      /// <returns></returns>
      private clsHito AgregarFichadas(clsHito pobjFirstHito, NomadXML pnxmFichadas) {

		NomadBatch.Trace("Comienza AgregarFichadas()");
		
        clsHito objFirstHito      = pobjFirstHito;
        clsHito objWorkHito       = pobjFirstHito;
        clsHito objTolAnterior    = null;
        clsHito objTolSiguiente   = null;
        clsHito objHitoRegistPrev = null;

        clsHito objHitoPrev       = null;
        clsHito objRegis;

        string  oiEstructura;
        string  strHora;
        string  oiFichada;
        bool    bolEnter;
        bool    bolDayToProc = false;

        objTolSiguiente = objFirstHito.ObtenerSiguienteEsperanza();
		int r = 0;
		
		NomadBatch.Trace("Recorre los registros de las fichadas");

        //Recorre los registros de las fichadas
        for (NomadXML nxmFichada = pnxmFichadas.FirstChild().FirstChild(); nxmFichada != null; nxmFichada = nxmFichada.Next()) {
			r++;
			
			NomadBatch.Trace("Registro: " + r);					

          //Lee algunos parametros de la fichada
		  
		  NomadBatch.Trace("Lee algunos parametros de la fichada");		  		  
		  
          strHora      = nxmFichada.GetAttr("f_fechahora");
          bolEnter     = nxmFichada.GetAttr("c_tipo") == "E";
          oiEstructura = nxmFichada.GetAttr("oi_estructura");
          oiFichada    = nxmFichada.GetAttr("oi_fichadasing");

          //Crea el hito de la fichada
		  NomadBatch.Trace("Crea el hito de la fichada");		  
          objRegis = new clsHito(strHora, bolEnter, oiEstructura, oiFichada);

          //Obtiene las esperanza ANTERIOR y POSTERIOR a la fecha de la fichada
		  NomadBatch.Trace("Obtiene las esperanza ANTERIOR y POSTERIOR a la fecha de la fichada - pre while");
		  
		  int f = 0;
          while (objTolSiguiente != null && objRegis.FechaHora > objTolSiguiente.FechaHora) {
			f++;
			 
            objTolAnterior  = objTolSiguiente;
			NomadBatch.Trace("while fila: " + f + " pre ObtenerSiguienteEsperanza");			
            objTolSiguiente = objTolSiguiente.ObtenerSiguienteEsperanza();
			NomadBatch.Trace("while fila: " + f + " post ObtenerSiguienteEsperanza");			
          }
		  NomadBatch.Trace("Obtiene las esperanza ANTERIOR y POSTERIOR a la fecha de la fichada - post while");


          //Obtiene el hito anterior a la fichada (Puede ser una ESPERANZA o un DIA)
		  NomadBatch.Trace("Obtiene el hito anterior a la fichada (Puede ser una ESPERANZA o un DIA)");
          objHitoPrev = objWorkHito.ObtenerSiguiente(objRegis.FechaHora);

          //Solo se aplican las tolerancias a las fichadas de los horarios FIJOS y ROTATIVOS
          if (objWorkHito.TipoHito == TiposHito.InicioDia)
            bolDayToProc = objWorkHito.TipoHorario != "L";

          //Aplica las tolerancias a las fichadas correspondientes a dias Fijos o Rotativos
		  NomadBatch.Trace("Aplica las tolerancias a las fichadas correspondientes a dias Fijos o Rotativos");
          if (bolDayToProc)
            objHitoRegistPrev = ApplyTol(objTolAnterior, objTolSiguiente, objRegis);
          else
            objHitoRegistPrev = null;

          //Agrego la registracion a la lista enlazada
		  NomadBatch.Trace("Agrego la registracion a la lista enlazada");		  
		  
          if (objHitoRegistPrev == null) objHitoRegistPrev = objHitoPrev;
          if (objHitoRegistPrev == null) objHitoRegistPrev = objWorkHito;
		  
          clsHito objToInsert = objHitoRegistPrev;
		  
		  NomadBatch.Trace("pre while objToInsert.Siguiente");		  		  
		  
          while(objToInsert.Siguiente != null && objToInsert.Siguiente.TipoHito == TiposHito.Fichada)
            objToInsert = objToInsert.Siguiente;
		
		  NomadBatch.Trace("post while objToInsert.Siguiente");		  		

          if (objToInsert.FechaHora > objRegis.FechaHora)
            objRegis.FechaHora = objToInsert.FechaHora;

          objRegis.Siguiente    = objToInsert.Siguiente;
          objToInsert.Siguiente = objRegis;

          objWorkHito = objRegis;

        }
		NomadBatch.Trace("Fin AgregarFichadas()");

        return objFirstHito;
      }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objPrevHope"></param>
        /// <param name="objNextHope"></param>
        /// <param name="objRegis"></param>
        /// <returns></returns>
        private clsHito ApplyTol(clsHito objPrevHope, clsHito objNextHope, clsHito objRegis) {
            bool InRangePre = false;
            bool InRangePos = false;
            clsTipoHora objTH;

            DateTime dtePre = new DateTime(1900, 1, 1, 0, 0, 0);
            DateTime dtePos = objRegis.FechaHora.AddYears(1);

            TimeSpan tspPre = new TimeSpan(1);
            TimeSpan tspPos = new TimeSpan(1);

            //Verifica si está en rango de ambos hitos (Previo y Siguiente)
            if (objPrevHope != null) {

                for (int x = 0; x < 2 && !InRangePos; x++) {

                  objTH = x == 0 ? objPrevHope.TipoHora: objPrevHope.TipoHoraAnterior;

                    dtePre = objPrevHope.FechaHora;
                    tspPre = objRegis.FechaHora.Subtract(dtePre);

                    if (objRegis.Entrada)
                        InRangePos = tspPre.TotalMinutes <= (double) objTH.TolPosIng;
                    else
                        InRangePos = tspPre.TotalMinutes <= (double) objTH.TolPosEgr;
                }

            }

            if (objNextHope != null) {
                for (int x = 0; x < 2 && !InRangePre; x++) {

                  objTH = x == 0 ? objNextHope.TipoHora : objNextHope.TipoHoraAnterior;

                      dtePos = objNextHope.FechaHora;
                      tspPos = dtePos.Subtract(objRegis.FechaHora);

                      if (objRegis.Entrada)
                          InRangePre = tspPos.TotalMinutes <= (double) objTH.TolPreIng;
                      else
                          InRangePre = tspPos.TotalMinutes <= (double) objTH.TolPreEgr;
                }
            }

            if (InRangePos == true && InRangePre == true) {
                if (tspPos.TotalMinutes > tspPre.TotalMinutes) {
                    InRangePos = true;
                    InRangePre = false;
                } else {
                    InRangePre = true;
                    InRangePos = false;
                }
            }

            if (InRangePos) {
                objRegis.FechaHora = dtePre;
                return objPrevHope;
            } else
            if (InRangePre) {
                objRegis.FechaHora = dtePos;
                return objNextHope;
            }

      return null;
        }

        /// <summary>
        /// Devuelve un Hashtable con las estrucuras accesibles por oi_terminal
        /// </summary>
        /// <returns></returns>
        private Hashtable GetEstructuras() {
            string strResult = "";
            Hashtable htaResultado = new Hashtable();

            //Ejecuta el query
            strResult = NomadEnvironment.QueryString(FICHADASING.Resources.QRY_Estructuras, "");

            //Recorre los tipos de horas y los agrega en la colección
            XmlTextReader xtrES = new XmlTextReader(strResult, System.Xml.XmlNodeType.Document, null);
            xtrES.XmlResolver = null; // ignore the DTD
            xtrES.WhitespaceHandling = WhitespaceHandling.None;
            xtrES.Read();

            while (xtrES.Read()) {

        if (!xtrES.IsStartElement())
          continue;

                htaResultado.Add(xtrES.GetAttribute("oi_terminal"), xtrES.GetAttribute("oi_estructura"));
            }

            return htaResultado;

        }

        private string ValidDate(string pstrDate) {
            string strResult = "19000101000000";
            if (pstrDate != null)
                if (pstrDate.Length == 8)
                    strResult = pstrDate + "000000";
                else
                    if (pstrDate.Length == 14)
                        strResult = pstrDate;

            return strResult;
        }

    private static NomadXML GetNewDataItem(DateTime pdteDate, int pintMinutes, string pstrType) {
      NomadXML xmlResult = new NomadXML("eve");

      xmlResult.SetAttr("f", StringUtil.date2str(pdteDate));
      xmlResult.SetAttr("m", pintMinutes);
      xmlResult.SetAttr("t", pstrType);

      return xmlResult;
    }

    private static NomadXML GetNewDataItem(DateTime pdteDate, string pstrOiFichada, string pstrType) {
      NomadXML xmlResult = new NomadXML("eve");

      xmlResult.SetAttr("f", pdteDate.ToString("yyyyMMdd"));
      xmlResult.SetAttr("oif", pstrOiFichada);
      if (pstrType == "l")
        xmlResult.SetAttr("t", "l");

      return xmlResult;
    }

    /// <summary>
    /// Calcula los rangos de fechas para un día en particular
    /// </summary>
    /// <param name="pobjParams">Objeto parámetros</param>
    /// <param name="pdteDate">Fecha en cuestion</param>
    /// <param name="pstrID">Oi personal</param>
    /// <param name="pdteFrom">Parametro de salida. Fecha desde para el día calculado.</param>
    /// <param name="pdteTo">Parametro de salida. Fecha fin para el día calculado.</param>
    /// <param name="pPostEgr">Tolerancia Post Egreso</param>
    /// <param name="pPreIng">Tolerancia Pre Ingreso</param>
    public static void RangeDate(clsBaseParams pobjParams, DateTime pdteDate, string pstrID, ref DateTime pdteFrom, ref DateTime pdteTo, int pPostEgr, int pPreIng) {
      Procesos.RangeDate(pobjParams, pdteDate, pstrID, ref pdteFrom, ref pdteTo, false, pPostEgr, pPreIng, false);
    }

    /// <summary>
    /// Calcula los rangos de fechas para un día en particular
    /// </summary>
    /// <param name="pobjParams">Objeto parámetros</param>
    /// <param name="pdteDate">Fecha en cuestion</param>
    /// <param name="pstrID">Oi personal</param>
    /// <param name="pdteFrom">Parametro de salida. Fecha desde para el día calculado.</param>
    /// <param name="pdteTo">Parametro de salida. Fecha fin para el día calculado.</param>
    public static void RangeDate(clsBaseParams pobjParams, DateTime pdteDate, string pstrID, ref DateTime pdteFrom, ref DateTime pdteTo) {
      Procesos.RangeDate(pobjParams, pdteDate, pstrID, ref pdteFrom, ref pdteTo, true, 0, 0, true);
    }

    /// <summary>
    /// Calcula las fechas inicio y fin deacuerdo a la esperanza
    /// </summary>
    /// <param name="pobjParams">Objeto parámetros</param>
    /// <param name="pdteDate">Fecha del día a calcular</param>
    /// <param name="pstrID">Oi personal</param>
    /// <param name="pdteFrom">Parametro de salida. Fecha desde para el rango calculado.</param>
    /// <param name="pdteTo">Parametro de salida. Fecha hasta para el rango calculado.</param>
    /// <param name="pbolUseHourTol">Indica si utiliza las tolerancias del tipo de hora. En caso contrario utilizará los parámetros pPostEgr y pPreIng.</param>
    /// <param name="pPostEgr">Tolerancia post egreso.</param>
    /// <param name="pPreIng">Tolerancia post ingreso.</param>
    /// <param name="pbolTHObligatoria">Indica si utiliza solo las horas obligatorias para el cálculo del rango.</param>
    public static void RangeDate(clsBaseParams pobjParams, DateTime pdteDate, string pstrID, ref DateTime pdteFrom, ref DateTime pdteTo, bool pbolUseHourTol, int pPostEgr, int pPreIng, bool pbolTHObligatoria) {
      ArrayList arrHope;

      DateTime dteY = pdteDate.AddDays(-1d); //Yesterday
      DateTime dteT = pdteDate.AddDays(1d);  //Tomorrow

      DateTime dteYTo = new DateTime(1900, 1, 1);
      DateTime dteFrom = new DateTime(1900, 1, 1);
      DateTime dteTo = new DateTime(1900, 1, 1);
      DateTime dteTFrom = new DateTime(1900, 1, 1);

      clsTipoHora objTipoHoraPost = null;
      clsTipoHora objTipoHoraPrev = null;
      int intTolTHPost;
      int intTolTHPrev;
      bool bolHasDetails = false; //Indica si el día en particular analizado tuvo detalles

      NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA objTEMP = null;

      NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objFirstDetail;
      NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objLastDetail;

      //------------------------------------------------------------------
      //Calcula la ESPERANZA desde un día antes hasta un día después
      arrHope = ESPERANZAPER.GetDaysHope(dteY, dteT, int.Parse(pstrID));
      for (int x = 0; x < arrHope.Count; x++) {
        objTEMP = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA) arrHope[x];

        objFirstDetail = null;
        objLastDetail = null;
        intTolTHPost = 0;
        intTolTHPrev = 0;

        for(int d = 0; d < objTEMP.DETALLE.Count; d++) {

          objFirstDetail  = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE) objTEMP.DETALLE[d];
          objTipoHoraPrev = pobjParams.GetTipoHora(objFirstDetail.oi_tipohora);

          //Es de presencia obligatoria
          if (objFirstDetail.l_autorizada && (pbolTHObligatoria || objTipoHoraPrev.EnAusencia == Sumarizaciones.EnAusenteExedente))
            break;
          else
            objFirstDetail = null;
        }

        for(int d = (objTEMP.DETALLE.Count - 1); d >= 0; d--) {

          objLastDetail   = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE) objTEMP.DETALLE[d];
          objTipoHoraPost = pobjParams.GetTipoHora(objLastDetail.oi_tipohora);

          //Es de presencia obligatoria
          if (objLastDetail.l_autorizada && (pbolTHObligatoria || objTipoHoraPost.EnAusencia == Sumarizaciones.EnAusenteExedente))
            break;
          else
            objLastDetail = null;
        }

        //Calcula las tolerancias a usar
        if (pbolUseHourTol) {
          //Utilizará las del tipo de hora
            if (objFirstDetail != null && objTipoHoraPost != null) intTolTHPost = objTipoHoraPost.TolPosEgr;
            if (objLastDetail  != null && objTipoHoraPrev != null) intTolTHPrev = objTipoHoraPrev.TolPreIng;
        } else {
          //Utilizará las de los parámetros
          intTolTHPost = pPostEgr;
          intTolTHPrev = pPreIng;
        }

        if (objTEMP.f_dia == dteY) {
          //Es un día antes ----------------------------------------
          if (objLastDetail != null)
            dteYTo = objTEMP.f_dia.AddMinutes(objLastDetail.e_horafin + intTolTHPost);

        } else {
          if (objTEMP.f_dia == pdteDate) {
            //Es el día en particular ----------------------------------------
            bolHasDetails = objFirstDetail != null && objLastDetail != null;

            if (objFirstDetail != null)
              dteFrom = objTEMP.f_dia.AddMinutes(objFirstDetail.e_horainicio - intTolTHPrev);
            else
              dteFrom = pdteDate;

            if (objLastDetail != null)
              dteTo = objTEMP.f_dia.AddMinutes(objLastDetail.e_horafin + intTolTHPost);
            else
              dteTo = pdteDate.AddDays(1d);

          } else {
            //Es un día después ----------------------------------------
            if (objFirstDetail != null)
              dteTFrom = objTEMP.f_dia.AddMinutes(objFirstDetail.e_horainicio - intTolTHPrev);

          }
        }

      }

      DateTime dteCompare = new DateTime(1900, 1, 1);
      //Resuelve la fecha DESDE
      if (dteYTo != dteCompare && dteFrom != dteCompare && dteFrom < dteYTo) {
        if (bolHasDetails)
          pdteFrom = dteFrom.AddMinutes(((TimeSpan)(dteYTo.Subtract(dteFrom))).TotalMinutes / 2);
        else
          pdteFrom = dteYTo;
      } else {
        if (dteYTo > pdteFrom && (dteYTo != dteCompare)) pdteFrom = dteYTo;
        if (dteFrom < pdteFrom && (dteFrom != dteCompare)) pdteFrom = dteFrom;
      }

      //Resuelve la fecha HASTA
      if (dteTo != dteCompare && dteTFrom != dteCompare && dteTFrom < dteTo) {
        if (bolHasDetails)
          pdteTo = dteTFrom.AddMinutes( ((TimeSpan)(dteTo.Subtract(dteTFrom))).TotalMinutes / 2);
        else
          pdteTo = dteTFrom;
      } else {
        if (dteTo > pdteTo && (dteTo != dteCompare)) pdteTo = dteTo;
        if (dteTFrom < pdteTo && (dteTFrom != dteCompare)) pdteTo = dteTFrom;
      }

    }

    //Métodos ESTÁTICOS ---------------------------------------------------------------------------------
    /// <summary>
    /// Retorna una colección de objetos clsTipoHora
    /// </summary>
    /// <returns></returns>
    public static Hashtable GetTiposHoras() {
      NomadProxy  objProxy = NomadProxy.GetProxy();
      NomadXML    xmlTHs;
      Hashtable   htaTH;
      clsTipoHora objTH;
      string      strCacheKey = "TTA.Procesos.GetTiposHoras";

      //Verifica si está en CAHCE
      htaTH = (Hashtable) objProxy.CacheGetObj(strCacheKey);
      if (htaTH != null) return htaTH;

      htaTH = new Hashtable();

      //Ejecuta el query
      xmlTHs = NomadEnvironment.QueryNomadXML(Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA.Resources.QRY_TiposHoras, "");

      if (xmlTHs != null) xmlTHs = xmlTHs.FirstChild();

      //Recorre los tipos de horas y los agrega en la colección
      for (NomadXML xmlTH = xmlTHs.FirstChild(); xmlTH != null; xmlTH = xmlTH.Next()) {

        objTH = new clsTipoHora(xmlTH.GetAttr("oi"),
          xmlTH.GetAttr("cod"),
          int.Parse( xmlTH.GetAttr("tolPreIng") == "" ? "0" : xmlTH.GetAttr("tolPreIng") ),
          int.Parse( xmlTH.GetAttr("tolPreEgr") == "" ? "0" : xmlTH.GetAttr("tolPreEgr") ),
          int.Parse( xmlTH.GetAttr("tolPosIng") == "" ? "0" : xmlTH.GetAttr("tolPosIng") ),
          int.Parse( xmlTH.GetAttr("tolPosEgr") == "" ? "0" : xmlTH.GetAttr("tolPosEgr") ),

          int.Parse( xmlTH.GetAttr("e_sumariza_aus") == "" ? "0" : xmlTH.GetAttr("e_sumariza_aus") ),
          int.Parse( xmlTH.GetAttr("e_sumariza_pre") == "" ? "0" : xmlTH.GetAttr("e_sumariza_pre") ),

          xmlTH.GetAttr("oi_tipohora_exc") == "" ? Convert.ToInt32(TiposHoras.Excedente).ToString() : xmlTH.GetAttr("oi_tipohora_exc"),
          xmlTH.GetAttr("oi_tipohora_aus") == "" ? Convert.ToInt32(TiposHoras.Ausencia).ToString()  : xmlTH.GetAttr("oi_tipohora_aus") );

          htaTH.Add(objTH.OI, objTH);
      }

      //Agrega el tipo de hora excedente
      objTH = new clsTipoHora(clsTipoHora.Excedent, "EXCEDENTE",
        0, 0, 0, 0,
        Convert.ToInt32(Sumarizaciones.NoSumariza),
        Convert.ToInt32(Sumarizaciones.EnAusenteExedente),
        Convert.ToInt32(TiposHoras.Excedente).ToString(),
        Convert.ToInt32(TiposHoras.Excedente).ToString());

      htaTH.Add(objTH.OI, objTH);

      //Lo agrega al CACHE
      objProxy.CacheAdd(strCacheKey, htaTH);

      return htaTH;
    }

    /// <summary>
    /// Tracea los datos de los hitos
    /// </summary>
    /// <param name="pobjH"></param>
    private void LogHito(clsHito pobjHito) {
      //Muestra los hitos

      NomadBatch.Trace("Traceo de HITOS -------------------------------------------------------------------");
      if (pobjHito != null) {

        for (clsHito objH = pobjHito; objH != null; objH = objH.Siguiente) {
          switch (objH.TipoHito) {
            case TiposHito.InicioDia:
              NomadBatch.Trace(" ");
              NomadBatch.Trace("Inicio DÍA: " + objH.Dia.ToString("dd/MM/yyyy") +
                  " (" + objH.FechaHora.ToString("dd/MM/yyyy HH:mm:ss") + ")" +
                  " (Horario: " + objH.DescripcionHorario + ")" +
                  (objH.ControlaFichada ? "" : " (No controla FICHADAS)"));

              break;

            case TiposHito.Esperanza:
              NomadBatch.Trace("  Esperanza: " + objH.FechaHora.ToString("dd/MM/yyyy HH:mm:ss") + " TH:" + objH.TipoHora.Cod + "(oi:" + objH.TipoHora.OI + ") (st:" + objH.OiEstructura + ")");
              break;

            case TiposHito.Fichada:
              NomadBatch.Trace("    " + (objH.Entrada ? "-->" : "<--") + objH.FechaHora.ToString("dd/MM/yyyy HH:mm:ss"));
              break;

          }
        }

      } else {
        NomadBatch.Trace("El objeto objHito es null");
      }

      NomadBatch.Trace("Fin Traceo de HITOS ---------------------------------------------------------------");
    }

  }

    /// <summary>
    /// Punto o hito en un lista enlazada representando la sucesión de hechos
    /// </summary>
    public class clsHito {
        private DateTime m_FechaDia; //Indica el dia al cual pertenece
        private DateTime m_FechaHora; //Indica la fecha/hora a la cual pertenece (El dia + los minutos)
        private int m_MinutosIniDia; //Indica los minutos en que inicia el día
        private int m_MinutosFinDia; //Indica los minutos en que finaliza el día

        private string m_TH_Inicio; //El hito inicia este tipo de hora
        private string m_TH_Fin; //El hito finaliza este tipo de hora
        private int m_intMinutos;
        private string m_Oi_Estructura;

        private clsParams m_objParams;

    private string m_Oi_Fichada;

        //private Sumarizaciones m_EnAusencia; //Indica que hacer en caso de ausencia (no sumariza, sumariza o sumariza en AUSENTE)
        //private Sumarizaciones m_EnPresencia; //Indica que hacer en caso de presencia (no sumariza, sumariza o sumariza en Excedente)

        //private bool m_Autorizada;

        private TiposHito m_TipoHito; //Indica si es de tipo fichada, Esperanza o InicioDía
        private bool m_Entrada; //Indica si la fichada es de entrada o salida

        private string m_Oi_Horario;
    private string m_Oi_Turno;

        //private bool m_RequiereAutorizacion = false; //Indica si es una hora de la que se requiere la autorizacion

        private clsHito m_objSiguiente;

        private string m_OILicencia; //Utilizado en los hitos de días. Indica si en ese día hay una licencia.
        private string m_strTipoDia;

        /// <summary>
        /// Costructor genérico.
        /// </summary>
        public clsHito() {
            m_TH_Inicio = "";
            m_TH_Fin = "";
        }

        /// <summary>
        /// Contructor del objeto para el tipo ESPERANZA
        /// </summary>
        /// <param name="pobjParams">Objeto de parametros</param>
        /// <param name="pstrFechaHora">Fecha/hora del suceso</param>
        /// <param name="pintTPInicio">Código del tipo de hora que inicia el Hito</param>
        /// <param name="pstrTPFin">Código del tipo de hora que finaliza el Hito</param>
        /// <param name="pintOIHorario">OI del horario del hito</param>
        /// <param name="pintOiEstructura">OI de la estructura</param>
        public clsHito(clsParams pobjParams, DateTime pdteFechaDia, int pintMinutos, string pstrTPInicio, string pstrTPFin, string pstrOiEstructura) : this() {
            m_TipoHito = TiposHito.Esperanza;

            m_objParams = pobjParams;
            m_intMinutos = pintMinutos;
            m_TH_Inicio = pstrTPInicio;
            m_TH_Fin = pstrTPFin;
            m_Oi_Estructura = pstrOiEstructura;
            m_FechaDia = pdteFechaDia;
            m_FechaHora = pdteFechaDia.AddMinutes(Convert.ToDouble(pintMinutos));

            //this.m_EnAusencia = pSumAus; ver de sacarlos del
            //this.m_EnPresencia = pSumPre;
        }

        /// <summary>
        /// Contructor del objeto para el tipo FICHADA
        /// </summary>
        /// <param name="pstrFechaHora">Fecha/hora del suceso</param>
        /// <param name="pbolEntrada">Indica si la fichada es de entrada o salida</param>
        /// <param name="pstrOiEstructura">Oi de la estructura</param>
        /// <param name="pstroiFichada">Oi de la fichada</param>
        public clsHito(string pstrFechaHora, bool pbolEntrada, string pstrOiEstructura, string pstroiFichada) : this() {
            m_TipoHito = TiposHito.Fichada;

            m_FechaHora = StringUtil.str2date(pstrFechaHora);
            m_Entrada = pbolEntrada;
            m_Oi_Estructura = pstrOiEstructura;
            m_Oi_Fichada = pstroiFichada;
        }

    /// <summary>
    /// Contructor del objeto para el tipo INICIO DE DIA
    /// </summary>
    /// <param name="pobjParams">Objeto de parametros</param>
    /// <param name="pdteFechaDia">Fecha del dia</param>
    /// <param name="pstrOiHorario">Oi del horario</param>
    /// <param name="pstrOITurno">Oi del turno</param>
    /// <param name="pstrOILicencia">Oi de la licencia</param>
    /// <param name="pstrTipo">Tipo de dia (D, DF, N F)</param>

        public clsHito(clsParams pobjParams, DateTime pdteFechaDia, string pstrOiHorario, string pstrOITurno, string pstrOILicencia, string pstrTipo) : this() {
            m_TipoHito = TiposHito.InicioDia;

            m_objParams  = pobjParams;
            m_FechaDia   = pdteFechaDia;
            m_Oi_Horario = pstrOiHorario;
            m_FechaHora  = pdteFechaDia;
      m_Oi_Turno   = pstrOITurno;

          m_OILicencia  = pstrOILicencia;

          this.m_strTipoDia = pstrTipo;
          this.m_MinutosIniDia = 0;
          this.m_MinutosFinDia = 1440;

        }

        // Propiedades ---------------------------------------------------------------------
        public TiposHito TipoHito { get {return m_TipoHito;} }
        public clsHito Siguiente { get {return m_objSiguiente;} set {m_objSiguiente = value;} }

        //Para hitos GENERICOS --------------------------------
        public int Minutos { get {return m_intMinutos;} set {m_intMinutos = value;} }
        public string OiEstructura { get {return m_Oi_Estructura;} set {m_Oi_Estructura = value;} }
        public clsTipoHora TipoHora {
            get {
                    return this.m_objParams.GetTipoHora(this.m_TH_Inicio);
                }
        }
        public clsTipoHora TipoHoraAnterior {
            get {
                    return this.m_objParams.GetTipoHora(this.m_TH_Fin);
            }
        }

        /// <summary>
        /// Indica el dia "REAL".
        /// Para el HITO INICIO DE DIA es el inicio del dia
        /// Para el HITO ESPERANZA la fecha del dia mas los minutos
        /// </summary>
        public DateTime FechaHora { get {return m_FechaHora; } set {m_FechaHora = value;} }

        //Para hitos tipo FICHADA --------------------------------
        public bool Entrada { get {return m_Entrada;} set {m_Entrada = value;} }
        public string OiFichada { get {return m_Oi_Fichada;} }

        //Para hitos tipo INICIO DE DIA --------------------------------
    public string OILicencia { get {return m_OILicencia;} set {m_OILicencia = value;} }
        public string OITurno { get {return this.m_Oi_Turno;} set {m_Oi_Turno = value;} }
        public string TipoDia { get { return this.m_strTipoDia; } }
        public int MinutosIniDia { get { return this.m_MinutosIniDia; } }
        public int MinutosFinDia { get { return this.m_MinutosFinDia; } }

        /// <summary>
        /// Indica el dia al cual pertenece un hito de tipo dia.
        /// </summary>
        public DateTime Dia {
            get { return m_FechaDia; }
        }

        /// <summary>
        /// Retorna el tipo de horario asociado al hito (F, R, L)
        /// </summary>
        public string TipoHorario {
            get {
                    NomadXML nxmHorario = this.m_objParams.GetHorario(this.m_Oi_Horario);
                    return nxmHorario.GetAttr("d_tipohorario");
              }
        }

        /// <summary>
        /// Retorna el OI de horario asociado al hito
        /// </summary>
        public string OIHorario { get { return this.m_Oi_Horario; } }

        /// <summary>
        /// Retorna la descripcion del horario
        /// </summary>
        public string DescripcionHorario {
            get {
                    NomadXML nxmHorario = this.m_objParams.GetHorario(this.m_Oi_Horario);
                    return nxmHorario.GetAttr("c_horario") + " - " + nxmHorario.GetAttr("d_horario");
              }
        }

        /// <summary>
        /// Indica si se controlan las fichadas para el día
        /// </summary>
        public bool ControlaFichada {
            get {
                    NomadXML nxmHorario = this.m_objParams.GetHorario(this.m_Oi_Horario);
                    return nxmHorario.GetAttr("l_controla_fic") == "1";
                }
         }

        public double TolPreIng {
            get {
                    string strTolerancia;
                    NomadXML nxmHorario = this.m_objParams.GetHorario(this.m_Oi_Horario);
                    strTolerancia = nxmHorario.GetAttr("e_tolpreing");
                    return strTolerancia == "" ? 0d : double.Parse(strTolerancia) ;
                }
         }

        public double TolPreEgr {
            get {
                    string strTolerancia;
                    NomadXML nxmHorario = this.m_objParams.GetHorario(this.m_Oi_Horario);
                    strTolerancia = nxmHorario.GetAttr("e_tolpreegr");
                    return strTolerancia == "" ? 0d : double.Parse(strTolerancia) ;
                }
         }
        public double TolPosIng {
            get {
                    string strTolerancia;
                    NomadXML nxmHorario = this.m_objParams.GetHorario(this.m_Oi_Horario);
                    strTolerancia = nxmHorario.GetAttr("e_tolposing");
                    return strTolerancia == "" ? 0d : double.Parse(strTolerancia) ;
                }
         }
        public double TolPosEgr {
            get {
                    string strTolerancia;
                    NomadXML nxmHorario = this.m_objParams.GetHorario(this.m_Oi_Horario);
                    strTolerancia = nxmHorario.GetAttr("e_tolposegr");
                    return strTolerancia == "" ? 0d : double.Parse(strTolerancia) ;
                }
         }

/*

        /// <summary>
        /// Indica si el hito fué "cambiado" o "utilizado" por una novedad.
        /// </summary>
        public bool UtilizadoNovedad {
            get { return m_UtilizadoNovedad; }
            set { m_UtilizadoNovedad = value; }
        }

        /// <summary>
        /// Indica que hacer en caso de ausencia (no sumariza, sumariza o sumariza en AUSENTE).
        /// </summary>
        public Sumarizaciones EnAusencia {
            get { return m_EnAusencia; }
            set { m_EnAusencia = value; }
        }

        /// <summary>
        /// Indica que hacer en caso de presencia (no sumariza, sumariza o sumariza en Excedente).
        /// </summary>
        public Sumarizaciones EnPresencia {
            get { return m_EnPresencia; }
            set { m_EnPresencia = value; }
        }

        public string OiHorario { get {return m_Oi_Horario;} set {m_Oi_Horario = value;} }

        public bool Autorizada { get {return m_Autorizada;} set {m_Autorizada = value;} }

        public bool RequiereAutorizacion { get {return m_RequiereAutorizacion;} set {m_RequiereAutorizacion = value;} }
*/

        // Métodos -------------------------------------------------------------------------

    public void SetRangeDay(DateTime pdteFechaFin) {
            TimeSpan tspDif;

      tspDif = this.m_FechaHora.Subtract(this.m_FechaDia);
      this.m_MinutosIniDia = Convert.ToInt32(tspDif.TotalMinutes);

      tspDif = pdteFechaFin.Subtract(this.m_FechaDia);
          this.m_MinutosFinDia = Convert.ToInt32(tspDif.TotalMinutes); //Indica los minutos en que finaliza el día
    }

        /// <summary>
        /// Setea el tipo de hora al hito
        /// </summary>
        /// <param name="pstrOiTipoHora">Nuevo OI_Tipo_Hora</param>
        public void SetTipoHora(string pstrOiTipoHora) {
            this.m_TH_Inicio = pstrOiTipoHora;
        }

        //Devuelve el último hito menor a la fecha pasada. Siempre empieza preguntando por si mismo.
        public clsHito ObtenerSiguiente(DateTime pdteFecha) {
            clsHito objResult = null;
            clsHito objIter;

            if (this.FechaHora < pdteFecha) {
                objResult = this;

                objIter = this.Siguiente;

                while (objIter != null) {

                    if (objIter.FechaHora < pdteFecha)
                        objResult = objIter;
                    else
                        break;

                    objIter = objIter.Siguiente;
                }
            }

            return objResult;
        }

        /// <summary>
        /// Devuelve el último hito del tipo ESPERANZA menor a la fecha pasada.
        /// Empieza preguntando por el siguiente de si mismo.
        /// </summary>
        /// <returns></returns>
        public clsHito ObtenerSiguienteEsperanza() {
            return ObtenerSiguiente(TiposHito.Esperanza);
        }

        /// <summary>
        /// Devuelve el último hito del tipo INICIODIA menor a la fecha pasada.
        /// Empieza preguntando por el siguiente de si mismo.
        /// </summary>
        /// <returns></returns>
        public clsHito ObtenerSiguienteDia() {
            return ObtenerSiguiente(TiposHito.InicioDia);
        }
/*
        /// <summary>
        /// Devuelve el último hito del tipo INICIODIA menor a la fecha pasada.
        /// Empieza preguntando por si mismo.
        /// </summary>
        /// <returns></returns>
        public clsHito ObtenerDia(DateTime pdteFecha) {
            clsHito objResult = null;

            for (clsHito objSearch = this; objSearch != null; objSearch = objSearch.Siguiente)
              if (objSearch.TipoHito == TiposHito.InicioDia)
                  if (objSearch.dteFechaDia == pdteFecha) {
            objResult = objSearch;
            break;
                  }

            return objResult;
        }
*/
        /// <summary>
        /// Devuelve el último hito del tipo especificado menor a la fecha pasada.
        /// Empieza preguntando por el siguiente de si mismo.
        /// </summary>
        /// <returns></returns>
        public clsHito ObtenerSiguiente(TiposHito pTipoHito) {
            clsHito objResult;

            for (objResult = this.Siguiente; objResult != null; objResult = objResult.Siguiente)
              if (objResult.TipoHito == pTipoHito)
                  break;

            return objResult;
        }

    }

    /// <summary>
    /// Objeto que contiene los valores de los parámetros pasados al método y los valores de los parámetros de la tabla ORG26_PARAMETROS
    /// </summary>
    public class clsBaseParams {

        protected Hashtable m_htaTiposHoras = null;
        protected string    m_strTiposHoras = "";

        protected DateTime  m_dteFechaInicioFICHADAS;
        protected DateTime  m_dteFechaFinFICHADAS;

        /// <summary>
        /// Retorna un objeto TipoHora.
        /// </summary>
        /// <returns></returns>
        public clsTipoHora GetTipoHora(string pstrOiTipoHora) {
            if (this.m_htaTiposHoras == null)
                this.LoadTiposHoras();

            if (this.m_htaTiposHoras.ContainsKey(pstrOiTipoHora))
                return (clsTipoHora) this.m_htaTiposHoras[pstrOiTipoHora];
            else
                throw new Exception ("Se está pidiendo un Tipo de Hora que no existe. OI_TIPOHORA='" + pstrOiTipoHora + "'");

        }

    /// <summary>
    /// Calcula las fechas inicio y fin para de obtención de fichadas para el PROCESAMIENTO
    /// </summary>
    /// <param name="pdteFechaInicio">Fecha inicio del Procesamiento</param>
    /// <param name="pdteFechaFin">Fecha Fin del Procesamiento + 1 día</param>
    /// <param name="pstrID"></param>
    /// <returns></returns>
    public void CalcularFechasFichadas(DateTime pdteFechaInicio, DateTime pdteFechaFin, string pstrID) {

      DateTime dteFechaInicioFrom = pdteFechaInicio;
      DateTime dteFechaInicioTo = pdteFechaInicio.AddDays(1d);
      DateTime dteFechaFinFrom = pdteFechaFin.AddDays(-1d);
      DateTime dteFechaFinTo = pdteFechaFin;

      //------------------------------------------------------------------
      //Calcula la FECHA INICIO
      Procesos.RangeDate(this, dteFechaInicioFrom, pstrID, ref dteFechaInicioFrom, ref dteFechaInicioTo);

      //------------------------------------------------------------------
      //Calcula la FECHA FIN
      Procesos.RangeDate(this, dteFechaFinFrom, pstrID, ref dteFechaFinFrom, ref dteFechaFinTo);

      this.m_dteFechaInicioFICHADAS = dteFechaInicioFrom;
      this.m_dteFechaFinFICHADAS    = dteFechaFinTo;
    }

    /// <summary>
    /// Carga una Hashtable con los tipos de hora. La clave es el oi_tipohora.
    /// </summary>
    /// <returns></returns>
    protected void LoadTiposHoras() {
      if (this.m_strTiposHoras != "") return;

      this.m_htaTiposHoras = new Hashtable();

      //Ejecuta el query
      this.m_strTiposHoras = NomadEnvironment.QueryString(Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA.Resources.QRY_TiposHoras, "");

      //Recorre los tipos de horas y los agrega en la colección
      XmlTextReader xtrTH = new XmlTextReader(this.m_strTiposHoras, System.Xml.XmlNodeType.Document, null);
      xtrTH.XmlResolver = null; // ignore the DTD
      xtrTH.WhitespaceHandling = WhitespaceHandling.None;
      xtrTH.Read();

      clsTipoHora objTH;

      while (xtrTH.Read()) {

        if (!xtrTH.IsStartElement()) continue;

        objTH = new clsTipoHora(xtrTH.GetAttribute("oi"),
        xtrTH.GetAttribute("cod"),
        int.Parse( xtrTH.GetAttribute("tolPreIng") == null ? "0" : xtrTH.GetAttribute("tolPreIng") ),
        int.Parse( xtrTH.GetAttribute("tolPreEgr") == null ? "0" : xtrTH.GetAttribute("tolPreEgr") ),
        int.Parse( xtrTH.GetAttribute("tolPosIng") == null ? "0" : xtrTH.GetAttribute("tolPosIng") ),
        int.Parse( xtrTH.GetAttribute("tolPosEgr") == null ? "0" : xtrTH.GetAttribute("tolPosEgr") ),

        int.Parse( xtrTH.GetAttribute("e_sumariza_aus") == null ? "0" : xtrTH.GetAttribute("e_sumariza_aus") ),
        int.Parse( xtrTH.GetAttribute("e_sumariza_pre") == null ? "0" : xtrTH.GetAttribute("e_sumariza_pre") ),

        xtrTH.GetAttribute("oi_tipohora_exc") == null ? Convert.ToInt32(TiposHoras.Excedente).ToString() : xtrTH.GetAttribute("oi_tipohora_exc"),
        xtrTH.GetAttribute("oi_tipohora_aus") == null ? Convert.ToInt32(TiposHoras.Ausencia).ToString() : xtrTH.GetAttribute("oi_tipohora_aus") );

        m_htaTiposHoras.Add(objTH.OI, objTH);

      }

      //Agrega el tipo de hora excedente
      objTH = new clsTipoHora(clsTipoHora.Excedent, "EXCEDENTE",
      0, 0, 0, 0,
      Convert.ToInt32(Sumarizaciones.NoSumariza),
      Convert.ToInt32(Sumarizaciones.EnAusenteExedente),
      Convert.ToInt32(TiposHoras.Excedente).ToString(),
      Convert.ToInt32(TiposHoras.Excedente).ToString());
      m_htaTiposHoras.Add(objTH.OI, objTH);

    }

        public DateTime FechaInicioFICHADAS { get {return m_dteFechaInicioFICHADAS;} set {m_dteFechaInicioFICHADAS = value;} }
        public DateTime FechaFinFICHADAS { get {return m_dteFechaFinFICHADAS;} set {m_dteFechaFinFICHADAS = value;} }

  }

    /// <summary>
    /// Objeto que contiene los valores de los parámetros pasados al método y los valores de los parámetros de la tabla ORG26_PARAMETROS
    /// </summary>
    public class clsParams : clsBaseParams {
    private string m_strListLegajos;
        private XmlElement m_xelLegajos;
        private bool m_bolParcial = false;
    private bool m_bolReProcesar = false;

        //Campos Fecha
        private DateTime m_dteFechaInicioORIG;
        private DateTime m_dteFechaInicio;
        private DateTime m_dteFechaFinORIG;
        private DateTime m_dteFechaFin;

        private Hashtable m_htaHorarios = null;
    private Hashtable m_htaStructs = null;

        //Variables de la tabla parámetros
        private double m_dblMinRange;
        private TimeSpan m_tspMaxDifIngEgr;
        private TimeSpan m_tspDifEgrIng;
        private double m_dblMinMinutosLiq;
        private double m_dblMinMinutosHora;
        private double m_dblRedondeoA;
        private bool m_bolEveES = true;
        private string m_strTipoClasif;

        //Variables utilizadas para el cálculo del HASH
        private string m_strDBParams = "";
        private NomadXML m_nxmFichadasIng = null;

        private string m_strOIEstructura;

        private string m_strPersonalList = "";

    //Variables de información adicional administrada y suministrada por los métodos de la clase 'Procesos'
    private int m_intCantFichadas;
    private int m_intCantLegajos;
    private int m_intCantLegReproc;
    private DateTime m_dteComienzo;
    private DateTime m_dteFin;
    private string m_strMensaje1;
    private string m_strMensaje2;
    private bool m_bolNeedsStep2 = false;

        public clsParams() {
        }

        /// <summary>
        /// Constructor para los parámetros utilizados en la VALIDACION
        /// </summary>
        /// <param name="pstrParams">String con formato XML conteniendo los parámetros cargados en pantalla.</param>
        public clsParams(string pstrParams) {
            XmlDocument xmlParams;
            XmlElement xelElement;
            string strTemp;

            if (pstrParams.Length == 0) throw new Exception("Parámetros de entrada inválidos.");

            xmlParams = new XmlDocument();
            xmlParams.LoadXml(pstrParams);

            xelElement = (XmlElement) xmlParams.DocumentElement;

            //Analiza el elemento FILTRO
            if (xelElement.Name != "FILTRO")
                throw new Exception("No se encontró el elemento 'FILTRO'.");

            //Obtiene las fechas
            if (xelElement.HasAttribute("f_ini")) {
                strTemp = xelElement.GetAttribute("f_ini");

                if (strTemp.Length != 8)
                  throw new Exception("El atributo 'Fecha Inicio (f_ini)' no tiene un valor válido.");

                this.m_dteFechaInicioORIG = StringUtil.str2date(strTemp);

            } else
              throw new Exception("No se encontró el atributo 'Fecha Inicio (f_ini)'.");

            if (xelElement.HasAttribute("f_fin")) {
                strTemp = xelElement.GetAttribute("f_fin");

                if (strTemp.Length != 8)
                  throw new Exception("El atributo 'Fecha Fin (f_fin)' no tiene un valor válido.");

                this.m_dteFechaFinORIG = StringUtil.str2date(strTemp);

            } else
              throw new Exception("No se encontró el atributo 'Fecha Fin (f_fin)'.");

            this.m_bolParcial = xelElement.GetAttribute("parcial") == "1";

            //Actualiza las fechas
            this.m_dteFechaInicio = this.m_dteFechaInicioORIG.AddDays(-1d);
            this.m_dteFechaFin = this.m_dteFechaFinORIG.AddDays(1d);

            //Obtiene la lista de personas
            LoadLegajos((XmlElement) xelElement.GetElementsByTagName("PERSONAS").Item(0));

            //Carga los parametros de la Base de Datos
            this.GetDBParams();

            //Setea el tipo de clasificación proveniente de la pantalla
            NomadXML XMLPantalla = new NomadXML();
            XMLPantalla.SetText(pstrParams);
            if(XMLPantalla.FirstChild().GetAttr("TipoClasif") != "") this.m_strTipoClasif = XMLPantalla.FirstChild().GetAttr("TipoClasif");
            NomadEnvironment.GetTrace().Info("TIPO CLASIF PANTALLA: -- " + this.m_strTipoClasif );

            this.ClearVariables();
        }

        /// <summary>
        /// Constructor para los parámetros utilizados en el PROCESAMIENTO de horas
        /// </summary>
        /// <param name="pstrProcesamiento">Oi del Prosesamiento</param>
        /// <param name="pstrParams">String con formato XML conteniendo los parámetros cargados en pantalla.</param>
        public clsParams(string pstrProcesamiento, string pstrParams) {
            XmlDocument xmlParams;
            XmlElement xelElement;
            NomadXML xmlPro;
            string strTemp;

            //Obtiene los datos del procesamiento
            if (pstrProcesamiento.Trim() == "") throw new Exception("No se pudo obtener el OI del Procesamiento a Generar.");
            xmlPro = GetDatosProcesamiento(pstrProcesamiento);

            //Obtiene las fechas desde y hasta de procesamiento.
            if (xmlPro.GetAttr("f_fechaInicio") != "") {
                strTemp = xmlPro.GetAttr("f_fechaInicio");
                if (strTemp.Length != 8)
                  throw new Exception("El atributo 'Fecha inicio de procesamiento (f_fechaInicio)' no tiene un valor válido.");

                this.m_dteFechaInicio = StringUtil.str2date(strTemp);

            } else
                throw new Exception("El procesamiento no tiene Fecha inicio.");

            if (xmlPro.GetAttr("f_fechaFin") != "") {
                strTemp = xmlPro.GetAttr("f_fechaFin");
                if (strTemp.Length != 8)
                  throw new Exception("El atributo 'Fecha fin de procesamiento (f_fechaFin)' no tiene un valor válido.");

                this.m_dteFechaFin = StringUtil.str2date(strTemp);
            } else
                throw new Exception("El procesamiento no tiene Fecha fin.");

            //Le agrega un dia a la fecha fin de la liquidación (PROCESAMIENTO)
            this.m_dteFechaFin = m_dteFechaFin.AddDays(1.0);

      //Interpreta el XML de parámetros
            if (pstrParams.Length == 0) throw new Exception("Parámetros de entrada inválidos.");

            xmlParams = new XmlDocument();
            xmlParams.LoadXml(pstrParams);
            xelElement = (XmlElement) xmlParams.DocumentElement;

            //Obtiene la lista de personas
            LoadLegajos((XmlElement) xelElement.GetElementsByTagName("PERSONAS").Item(0));

            //Determina si es un procesamiento parcial
            this.m_bolParcial = xelElement.GetAttribute("parcial") == "1";

      this.m_bolReProcesar = xelElement.GetAttribute("reprocesar") == "1";

            //Carga los parametros de la Base de Datos
            this.GetDBParams();

            m_strOIEstructura = null;

            this.ClearVariables();
        }
        //Métodos --------------------------------------------------------------------------

    public void ClearVariables() {
      this.m_intCantFichadas  = 0;
      this.m_intCantLegajos   = 0;
      this.m_intCantLegReproc = 0;
      this.m_dteComienzo      = DateTime.Now;
      this.m_dteFin           = DateTime.Now;
      this.m_strMensaje1      = "";
      this.m_strMensaje2      = "";
    }

    public string PersonalList { get { return this.m_strListLegajos; } }

    public string GetPersonalIN() {
            if (m_strPersonalList == "")
                m_strPersonalList = this.GetPersonalList();

            return m_strPersonalList;
        }

        public ArrayList GetPersonals() {
            ArrayList arrResultado = new ArrayList();

            foreach (XmlElement xelPersona in m_xelLegajos.ChildNodes) {
                if (xelPersona.Name == "ROW") {
                    arrResultado.Add(xelPersona.Attributes["id"].Value );
                }
            }

            return arrResultado;
        }

        /// <summary>
        /// Limpia el caché generado por cada LEGAJO
        /// </summary>
        public void ClearLegCache() {
            this.m_nxmFichadasIng = null;
            return;
        }

        //Propiedades ----------------------------------------------------------------------
        public DateTime FechaInicio { get {return m_dteFechaInicio;} set {m_dteFechaInicio = value;} }
        public DateTime FechaFin { get {return m_dteFechaFin;} set {m_dteFechaFin = value;} }
        public DateTime FechaInicioORIG { get {return m_dteFechaInicioORIG;} set {m_dteFechaInicioORIG = value;} }
        public DateTime FechaFinORIG { get {return m_dteFechaFinORIG;} set {m_dteFechaFinORIG = value;} }

        public XmlElement Legajos { get {return this.m_xelLegajos;} }

        public double MinRange { get {return m_dblMinRange;} }
        public TimeSpan MaxDifIngEgr { get {return m_tspMaxDifIngEgr;} }
        public TimeSpan DifEgrIng { get {return m_tspDifEgrIng;} }

        public double MinMinutosLiq { get {return m_dblMinMinutosLiq;} }
        public double MinMinutosHora { get {return m_dblMinMinutosHora;} }
        public double RedondeoA { get {return m_dblRedondeoA;} }

        public bool RelojIndicaEventos { get {return this.m_bolEveES;} }

        public string TipoClasif { get {return this.m_strTipoClasif;} }

        public bool Parcial { get {return m_bolParcial;} }
    public bool ReProcesar { get {return m_bolReProcesar;} }

        public string OIEstructura { get {return m_strOIEstructura;} }

        public int      CantFichadas    { get {return m_intCantFichadas;}  set {this.m_intCantFichadas = value;} }
        public int      CantLegajos     { get {return m_intCantLegajos;}   set {this.m_intCantLegajos = value;} }
        public int      CantLegReproc   { get {return m_intCantLegReproc;} set {this.m_intCantLegReproc = value;} }
        public DateTime ComienzaProceso { get {return m_dteComienzo;}      set {this.m_dteComienzo = value;} }
        public DateTime FinalizaProceso { get {return m_dteFin;}           set {this.m_dteFin = value;} }
        public string   Mensaje1        { get {return m_strMensaje1;}      set {this.m_strMensaje1 = value;} }
        public string   Mensaje2        { get {return m_strMensaje2;}      set {this.m_strMensaje2 = value;} }

    public bool NeedsStep2 { get {return this.m_bolNeedsStep2;} set {this.m_bolNeedsStep2 = value;} }

        //Metodos privados ------------------------------------------------------------------
/*
        /// <summary>
        /// Carga los datos adicionales de los legajos a utililzar.
        /// </summary>
        /// <param name="pxelLegajos">Elemento LEGAJOS.</param>
        /// <returns></returns>
        private void LoadLegajos(XmlElement pxelLegajos) {
      string strLegajosIN = "";
      string strResult;
      string strTotal = "";
      int intMaxRound;

            NomadBatch.Trace("Bloque de Oi_personal_Emp a analizar: -----------------------------");

            XmlNode xnoLegajo;
            XmlDocument xmlLegajos = new XmlDocument();
            NomadXML nxmParametros = new NomadXML("DATOS");

            //Ejecuta el query cada 100 personas
            for(int c = 0; c < pxelLegajos.ChildNodes.Count; c = c + 100) {

        intMaxRound = c + 100;

        strLegajosIN = "";
        for(int l = c; (l < intMaxRound) && (l < pxelLegajos.ChildNodes.Count); l++) {
          xnoLegajo = pxelLegajos.ChildNodes[l];
          strLegajosIN = strLegajosIN + "-" + xnoLegajo.Attributes["id"].Value + "-";
        }

        strLegajosIN = strLegajosIN.Replace("--", ",");
        strLegajosIN = strLegajosIN.Replace("-", "");

              //Carga los parametros necesarios para la ejecución del query
              nxmParametros.SetAttr("oi_personal_emps", strLegajosIN);

        NomadBatch.Trace("      " + strLegajosIN);
        //Ejecuta el query
        strResult = NomadEnvironment.QueryString(Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.QRY_DatosPersonal, nxmParametros.ToString());
        xmlLegajos.LoadXml(strResult);

        if (strTotal == "") {
          strTotal = "<" + xmlLegajos.DocumentElement.Name + ">";
        }

        strTotal = strTotal + xmlLegajos.DocumentElement.InnerXml;
            }

            NomadBatch.Trace("Fin Bloque de Oi_personal_Emp a analizar --------------------------");

      strTotal = strTotal + "</" + xmlLegajos.DocumentElement.Name + ">";

      xmlLegajos.LoadXml(strTotal);
            this.m_xelLegajos = (XmlElement) xmlLegajos.DocumentElement;

        }
*/
        /// <summary>
        /// Carga los datos adicionales de los legajos a utililzar.
        /// </summary>
        /// <param name="pxelLegajos">Elemento LEGAJOS.</param>
        /// <returns></returns>
        private void LoadLegajos(XmlElement pxelLegajos) {
      string strLegajosIN = "";
      string strResult;
      string strTotal = "";

      NomadBatch.Trace("Bloque de Oi_personal_Emp a analizar: -----------------------------");

      XmlNode xnoLegajos;
      XmlDocument xmlLegajos = new XmlDocument();
      NomadXML nxmParametros = new NomadXML("DATOS");
      this.m_strListLegajos = "0";

            if (pxelLegajos.ChildNodes.Count == 0) {
                throw new Exception("No se encontraron legajos seleccionados.");
            }

      //Ejecuta el query cada 100 personas
      for(int c = 0; c < pxelLegajos.ChildNodes.Count; c++) {

        xnoLegajos = pxelLegajos.ChildNodes[c];

        strLegajosIN = xnoLegajos.Attributes["VALUES"].Value;

        //Carga los parametros necesarios para la ejecución del query
        nxmParametros.SetAttr("oi_personal_emps", strLegajosIN);
        this.m_strListLegajos = this.m_strListLegajos + "," + strLegajosIN;

        //Ejecuta el query
        strResult = NomadEnvironment.QueryString(Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.QRY_DatosPersonal, nxmParametros.ToString());
        xmlLegajos.LoadXml(strResult);

        if (strTotal == "") {
          strTotal = "<" + xmlLegajos.DocumentElement.Name + ">";
        }

        strTotal = strTotal + xmlLegajos.DocumentElement.InnerXml;
      }

      NomadBatch.Trace("Fin Bloque de Oi_personal_Emp a analizar --------------------------");

      strTotal = strTotal + "</" + xmlLegajos.DocumentElement.Name + ">";

      xmlLegajos.LoadXml(strTotal);
      this.m_xelLegajos = (XmlElement) xmlLegajos.DocumentElement;

        }

        /// <summary>
        /// //Trae los parámetros de la base de datos (tabla ORG26_PARAMETROS)
        /// </summary>
        private string GetDBParams() {
            if (this.m_strDBParams == "") {

          //Crea el objeto de Parametros
                NomadXML objQParams = new NomadXML("DATOS");
          objQParams.SetAttr("c_modulo", "TTA");
          objQParams.SetAttr("d_clase", "\\'Terminal\\',\\'Fichadas\\', \\'Esperanza\\'");

                //Ejecuta el query
                this.m_strDBParams = NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Resources.QRY_Parametros, objQParams.ToString());

                if (this.m_strDBParams.EndsWith("/>"))
                    return "";

                XmlTextReader xtrResult = new XmlTextReader(this.m_strDBParams, System.Xml.XmlNodeType.Document, null);
                xtrResult.XmlResolver = null; xtrResult.WhitespaceHandling = WhitespaceHandling.None;

                xtrResult.Read();
                xtrResult.Read();

                //Recorre los registros
                string strTemporalValue;
                while (xtrResult.Name != "params") {
                    strTemporalValue = xtrResult.GetAttribute("d_valor") == null ? "0" : xtrResult.GetAttribute("d_valor");

                    switch (xtrResult.GetAttribute("c_parametro")) {
                        case "MinRange": m_dblMinRange = StringUtil.str2dbl(strTemporalValue); break;
                        case "MaxDifIngEgr": m_tspMaxDifIngEgr = new TimeSpan(0, int.Parse(strTemporalValue), 0); break; //Máxima diferencia entre una entrada y su salida
                        case "DifEgrIng": m_tspDifEgrIng = new TimeSpan(0, int.Parse(strTemporalValue), 0); break; //Diferencia entre una salida y una entrada para que se tomen como anuladas

                        case "MinMinutosLiq": m_dblMinMinutosLiq = StringUtil.str2dbl(strTemporalValue); break;
                        case "MinMinutosHora": m_dblMinMinutosHora = StringUtil.str2dbl(strTemporalValue); break;

                        case "RedondeoA": m_dblRedondeoA = StringUtil.str2dbl(strTemporalValue); break;

                        case "EveES": m_bolEveES = (strTemporalValue == "1"); break;

                        case "TipoClasif": m_strTipoClasif = strTemporalValue; break;

                    }

                    xtrResult.Read();
                }

                xtrResult.Close();

        NomadBatch.Trace("Resumen de lectura de los parámetros:");
        NomadBatch.Trace("    MinRange: " + m_dblMinRange.ToString());
        NomadBatch.Trace("    MaxDifIngEgr: " + m_tspMaxDifIngEgr.ToString());
        NomadBatch.Trace("    DifEgrIng: " + m_tspDifEgrIng.ToString());
        NomadBatch.Trace("    MinMinutosLiq: " + m_dblMinMinutosLiq.ToString());
        NomadBatch.Trace("    MinMinutosHora: " + m_dblMinMinutosHora.ToString());
        NomadBatch.Trace("    RedondeoA: " + m_dblRedondeoA.ToString());
        NomadBatch.Trace("    EveES: " + m_bolEveES.ToString());
        NomadBatch.Trace("    TipoClasif: " + m_strTipoClasif);

            }

            return this.m_strDBParams;
        }

    /// <summary>
    /// Obtiene los datos del procesamiento que se quiere procesar
    /// </summary>
    /// <param name="pstrOiPro">Oi del procesamiento.</param>
    /// <returns></returns>
    private NomadXML GetDatosProcesamiento(string pstrOiPro) {
      NomadXML xmlResult;

      //Crea el objeto de Parametros
      NomadXML objQParams = new NomadXML("DATOS");
      objQParams.SetAttr("oi_Liquidacion", pstrOiPro);

      //Ejecuta el query CACHEABLE
      xmlResult = NomadEnvironment.QueryNomadXML(Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION.Resources.QRY_DatosLiquidacion, objQParams.ToString(), true);

      return xmlResult.FirstChild();
    }

        private string GetPersonalList() {
            /*
            System.Text.StringBuilder strPersonalList = new System.Text.StringBuilder();
            System.Text.StringBuilder strPersonalListSplit = new System.Text.StringBuilder();
            string strFieldName;
            string strNexo = "";
            string strNexoSplit = "";

            this.m_arrPersonalListSplited = new ArrayList();

            strFieldName = "id";

            foreach (XmlElement xelPersona in m_xelLegajos.ChildNodes) {
                strPersonalList.Append(strNexo + xelPersona.Attributes[strFieldName].Value);
                strPersonalListSplit.Append(strNexoSplit + xelPersona.Attributes[strFieldName].Value);

              strNexo = ",";
        strNexoSplit = ",";

        if (strPersonalListSplit.Length > 100) {
                  this.m_arrPersonalListSplited.Add(strPersonalListSplit.ToString());
                  strPersonalListSplit.Length = 0;
                  strNexoSplit = "";
                }

            }

            //Completa el array de personas spliteadas
            if (strPersonalListSplit.Length > 0)
              this.m_arrPersonalListSplited.Add(strPersonalListSplit.ToString());

            return strPersonalList.ToString();
             */ return "";
        }

        //Metodos publicos ------------------------------------------------------------------
    public string GetHASHProLegajo(string pstrOiProcesamiento, string pstrOiLegajo, string pstrAdditInfo, ref NomadXML pnxmFichadas) {
      DateTime dteFechaInicio;
      DateTime dteFechaFin;
      string   strResult = "";
      NomadXML nxmFichadas;
      string   strParams;

      NomadBatch.Trace("HASH: GetDatosProcesamiento");
      NomadXML xmlPro = GetDatosProcesamiento(pstrOiProcesamiento);

      //Agrega el oi_personal_emp
      strResult = strResult + pstrOiLegajo;

      //Agrega los datos de la liquidación
      strResult = strResult + xmlPro.ToString();

      //Obtiene las fechas del Procesamiento
      dteFechaInicio = StringUtil.str2date(xmlPro.GetAttr("f_fechaInicio"));
      dteFechaFin = StringUtil.str2date(xmlPro.GetAttr("f_fechaFin"));

      //Adjunta la ESPERANZA
      NomadBatch.Trace("HASH: GetDaysSerialize");
      strResult = strResult + this.GetDaysSerialize(ESPERANZAPER.GetDaysHope(dteFechaInicio, dteFechaFin, int.Parse(pstrOiLegajo)));

      //Adjunta las FICHADAS
      NomadBatch.Trace("HASH: CalcularFechasFichadas");
    dteFechaFin = dteFechaFin.AddDays(1d); //Incrementa en un día la fecha fin ya que el método CalcularFechasFichadas así lo requiere
      CalcularFechasFichadas(dteFechaInicio, dteFechaFin, pstrOiLegajo);
      NomadBatch.Trace("HASH: GetFichadasIng");
      nxmFichadas = GetFichadasIng(this.FechaInicioFICHADAS, this.FechaFinFICHADAS, pstrOiLegajo);
      strResult = strResult + nxmFichadas.ToString();

      if (pnxmFichadas != null)
        pnxmFichadas = nxmFichadas;

      //Adjunta los TIPOS DE HORAS
      NomadBatch.Trace("HASH: LoadTiposHoras");
      this.LoadTiposHoras();
      strResult = strResult + this.m_strTiposHoras;

      //Adjunta los valores de los PARÁMETROS DE LA DB
      NomadBatch.Trace("HASH: GetDBParams");
      strResult = strResult + GetDBParams();

      //Adjunta las variables utilizada en CONCEPTOS
      NomadBatch.Trace("HASH: Variables de conceptos");
      strParams = "<DATOS oi_personal_emp=\"" + pstrOiLegajo + "\" oi_liquidacion=\"" + pstrOiProcesamiento + "\" />";
      strResult = strResult + NomadEnvironment.QueryString(LIQUIDACIONPERS.Resources.QRY_DatosHash, strParams);

      //Calcula el MD5
      NomadBatch.Trace("HASH: GetMD5");
      NomadBatch.Trace("HASH: Source GetMD5 : " + pstrAdditInfo + ">-<" + strResult);
      strResult = StringUtil.GetMD5(pstrAdditInfo + ">-<" + strResult);

      NomadBatch.Trace("HASH: FIN!!");
      return strResult;

        }

        /// <summary>
        /// Genera un string con la suma de los valores de los Días
        /// </summary>
        /// <param name="parrDays">Colección de días.</param>
        /// <returns></returns>
        private string GetDaysSerialize(ArrayList parrDays) {
            System.Text.StringBuilder sbResult;
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA objDia;

            sbResult = new System.Text.StringBuilder();

            sbResult.Append("<DIAS>");
            for (int x = 0; x < parrDays.Count; x++) {
                objDia = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA) parrDays[x];

                //Agrega los valores del DIA
                sbResult.Append("<DIA " +
                     "c_tipo=\"" + objDia.c_tipo + "\" " +
                     "e_posicion=\"" + objDia.e_posicion + "\" " +
                     "f_dia=\"" + objDia.f_dia.ToString("yyyyMMddHHmmss") + "\" " +
                     "l_afectado_nov=\"" + (objDia.l_afectado_nov ? "1" : "2") + "\" " +
                     "oi_calendario=\"" + objDia.oi_calendario + "\" " +
                     "oi_escuadra=\"" + objDia.oi_escuadra + "\" " +
                     "oi_horario=\"" + objDia.oi_horario + "\" " +
                     "oi_licencia=\"" + objDia.oi_licencia + "\" " +
                     "oi_turno=\"" + objDia.oi_turno + "\" >");

                sbResult.Append("<DETALLES>");
                //Recorre los detalles
                foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDetalle in objDia.DETALLE) {

                    //Agrega los valores del DIA
                    sbResult.Append("<DETALLE " +
                         "e_horainicio=\"" + objDetalle.e_horainicio.ToString() + "\" " +
                         "e_horafin=\"" + objDetalle.e_horafin.ToString() + "\" " +
                         "l_autorizada=\"" + (objDetalle.l_autorizada ? "1" : "2") + "\" " +
                         "oi_estructura=\"" + objDetalle.oi_estructura + "\" " +
                         "oi_tipohora=\"" + objDetalle.oi_tipohora + "\" />");
                }
                sbResult.Append("</DETALLES>");
                sbResult.Append("</DIA>");
            }
            sbResult.Append("</DIAS>");

            return sbResult.ToString();
        }

      /// <summary>
      /// Obtiene las fichadas de un legajo en particular
      /// </summary>
      /// <param name="pdteFechaInicio"></param>
      /// <param name="pdteFechaFin"></param>
      /// <param name="pstrID"></param>
      /// <returns></returns>
      public NomadXML GetFichadasIng(DateTime pdteFechaInicio, DateTime pdteFechaFin, string pstrID) {

        if (this.m_nxmFichadasIng == null) {
          //Crea el objeto de Parametros
          NomadXML objQParams = new NomadXML("DATOS");
          objQParams.SetAttr("oi_personal_emp", pstrID);
          objQParams.SetAttr("f_Inicio", StringUtil.date2str(pdteFechaInicio));
          //objQParams.SetAttr("f_Fin", StringUtil.date2str(pdteFechaFin.AddDays(1d)));
          objQParams.SetAttr("f_Fin", StringUtil.date2str(pdteFechaFin));

          //Ejecuta el query
          this.m_nxmFichadasIng = NomadEnvironment.QueryNomadXML(FICHADASING.Resources.QRY_FichadasIng, objQParams.ToString());

          NomadBatch.Trace("GetFichadasIng() PARAMS " + objQParams.ToString());

        }

        return this.m_nxmFichadasIng;
      }

        /// <summary>
        /// Retorna un NomadXML con los datos del horario solicitado.
        /// </summary>
        /// <param name="pstrOiHorario"></param>
        /// <returns></returns>
        public NomadXML GetHorario(string pstrOiHorario) {
            if (this.m_htaHorarios == null)
                this.m_htaHorarios = NomadEnvironment.QueryHashtable(NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.Resources.QRY_Horarios, "", "oi_horario", true);

            if (this.m_htaHorarios.ContainsKey(pstrOiHorario))
                return (NomadXML) this.m_htaHorarios[pstrOiHorario];
            else
                throw new Exception ("Es está pidiendo un Horario que no existe. OI_HORARIO='" + pstrOiHorario + "'");
        }

        /// <summary>
        /// Retorna la estructura desde una terminal pasada.
        /// </summary>
        /// <param name="pstrOITerminal"></param>
        /// <returns></returns>
        public string GetStruct(string pstrOITerminal) {
      string strResult = "";

      if (this.m_htaStructs == null) {
        this.m_htaStructs = new Hashtable();

        //Ejecuta el query
        strResult = NomadEnvironment.QueryString(FICHADASING.Resources.QRY_Estructuras, "");

        //Recorre los tipos de horas y los agrega en la colección
        XmlTextReader xtrES = new XmlTextReader(strResult, System.Xml.XmlNodeType.Document, null);
        xtrES.XmlResolver = null; // ignore the DTD
        xtrES.WhitespaceHandling = WhitespaceHandling.None;
        xtrES.Read();

        while (xtrES.Read()) {
          if (!xtrES.IsStartElement())
            continue;
          this.m_htaStructs.Add(xtrES.GetAttribute("oi_terminal"), xtrES.GetAttribute("oi_estructura"));
        }
      }

      if (this.m_htaStructs.ContainsKey(pstrOITerminal))
        return (string) this.m_htaStructs[pstrOITerminal];
      else
        return "";

        }

        /// <summary>
        /// Retorna si la liquidación se corrió para todas las personas y para el rango completo.
        /// </summary>
        /// <param name="pintCantidad">Cantidad de personas procesadas</param>
        /// <returns></returns>
        public bool LiqCompleta(int pintCantidad) {
            /*
            bool bolResult = true;
            bolResult = bolResult && (m_strFechaInicio == m_strFechaInicioOrig);

            bolResult = bolResult && (StringUtil.str2date(m_strFechaFin).AddDays(-1.0).ToString("yyyyMMddHHmmss") == m_strFechaFinOrig);
            bolResult = bolResult && (m_intCantidadPerLiq == pintCantidad);

            return bolResult;
            */
            return true;
        }

    }

  /// <summary>
  /// Estructura sencilla de usos varios.
  /// </summary>
  public class clsDatosVarios {
    public DateTime FechaInicio;
    public DateTime FechaFin;
    public string Horario;
    public int InicioDia;
    public bool IsNull = false;

    public clsDatosVarios() {
      this.IsNull = true;
    }

    public clsDatosVarios(DateTime pdteFI, DateTime pdteFF, string pstrHorario) {
      this.FechaInicio = pdteFI;
      this.FechaFin = pdteFF;
      this.Horario = pstrHorario;
    }

    public clsDatosVarios(DateTime pdteFI, DateTime pdteFF, string pstrHorario, int pintInicioD) {
      this.FechaInicio = pdteFI;
      this.FechaFin = pdteFF;
      this.Horario = pstrHorario;
      this.InicioDia = pintInicioD;
    }

  }

    public class clsPunto {
        private DateTime m_FechaInicio;
        private DateTime m_FechaFin;
        private string m_strOiEstructura;

        private bool m_bolEntrada;

        public clsPunto() {
        }
/*
        public clsPunto(string pstrFechaInicio, string pstrFechaFin, string pstrOiEstructura) {
            if (pstrFechaInicio.Length < 14) pstrFechaInicio = pstrFechaInicio + "000000";
            if (pstrFechaFin.Length < 14) pstrFechaFin = pstrFechaFin + "000000";

            m_FechaInicio = StringUtil.str2date(pstrFechaInicio);

            m_FechaFin = StringUtil.str2date(pstrFechaFin);

            m_strOiEstructura = pstrOiEstructura;
        }
*/

        public clsPunto(DateTime pdteFechaInicio, DateTime pdteFechaFin, string pstrOiEstructura) {
            m_FechaInicio = pdteFechaInicio;
            m_FechaFin = pdteFechaFin;
            m_strOiEstructura = pstrOiEstructura;
        }

        public DateTime FechaInicio {get { return m_FechaInicio;} set {m_FechaInicio = value;} }
        public DateTime FechaFin {get { return m_FechaFin;} set {m_FechaFin = value;} }
        public string OiEstructura {get { return m_strOiEstructura;} set {m_strOiEstructura = value;} }
        public bool Entrada {get { return m_bolEntrada;} set {m_bolEntrada = value;} }

    }

    /// <summary>
    /// Clase utilizada en el Reporte de Asistencias
    /// </summary>
    public class clsPuntoA {
        private DateTime m_dteDay;
        private int m_intMinutes;
        private string m_strType;

        private DateTime m_dteDateTime;
        private DateTime m_dteFrom;
        private DateTime m_dteTo;

    private NomadXML m_xmlObject;

    private DateTime m_dteFichadaDT;

        public clsPuntoA() {
        }

    public clsPuntoA(DateTime pdteDate, int pintMinutes, string pstrType, int pintPreMinutes, int pintPostMinutes) {
      this.m_dteDay    = pdteDate;
      this.m_intMinutes  = pintMinutes;
      this.m_strType    = pstrType;

      this.m_dteDateTime  = this.m_dteDay.AddMinutes(this.m_intMinutes);
      this.m_dteFrom    = this.m_dteDateTime.AddMinutes((pintPreMinutes * -1));
      this.m_dteTo    = this.m_dteDateTime.AddMinutes(pintPostMinutes);
    }

        public DateTime  Day      {get {return this.m_dteDay;}    set {this.m_dteDay = value;} }
        public int    Minutes    {get {return this.m_intMinutes;}  set {this.m_intMinutes = value;} }
        public DateTime DateTime  {get {return this.m_dteDateTime;}  set {this.m_dteDateTime = value;} }
        public string  Type    {get {return this.m_strType;}    set {this.m_strType = value;} }

    public DateTime  FechaInicio  {get {return this.m_dteFrom;}}
    public DateTime  FechaFin  {get {return this.m_dteTo;}}

    public NomadXML RelatedXML  {get {return this.m_xmlObject;}  set {this.m_xmlObject = value;}}

    public DateTime FichadaDT  {get {return this.m_dteFichadaDT;}  set {this.m_dteFichadaDT = value;}}

    }

    public class clsTipoHora {
        private string m_strOi_TipoHora;
        private string m_strC_TipoHora;

        private int m_intTolPreIng;
        private int m_intTolPosIng;
        private int m_intTolPreEgr;
        private int m_intTolPosEgr;

        private int m_intSumarizaAus;
        private int m_intSumarizaPre;
        private string m_strOi_tipohora_exc;
        private string m_strOi_tipohora_aus;

        public clsTipoHora(string pstrOI, string pstrCod,
                            int pintTPreI, int pintTPreE, int pintTPosI, int pintTPosE,
                            int pintSumarizaAus, int pintSumarizaPre,
                            string pstrOi_tipohora_exc, string pstrOi_tipohora_aus) {

            m_strOi_TipoHora = pstrOI;
            m_strC_TipoHora = pstrCod;

            m_intTolPreIng = pintTPreI;
            m_intTolPosIng = pintTPosI;
            m_intTolPreEgr = pintTPreE;
            m_intTolPosEgr = pintTPosE;

            m_intSumarizaAus = pintSumarizaAus;
            m_intSumarizaPre = pintSumarizaPre;
            m_strOi_tipohora_exc = pstrOi_tipohora_exc;
            m_strOi_tipohora_aus = pstrOi_tipohora_aus;

        }

        /* ATRIBUTOS */
        public string OI { get { return m_strOi_TipoHora;} }
        public string Cod { get { return m_strC_TipoHora;} }

        public int TolPreIng { get { return m_intTolPreIng;} set { m_intTolPreIng = value;} }
        public int TolPosIng { get { return m_intTolPosIng;} set { m_intTolPosIng = value;} }
        public int TolPreEgr { get { return m_intTolPreEgr;} set { m_intTolPreEgr = value;} }
        public int TolPosEgr { get { return m_intTolPosEgr;} set { m_intTolPosEgr = value;} }

        public Sumarizaciones EnAusencia { get { return (Sumarizaciones) m_intSumarizaAus; } }
        public Sumarizaciones EnPresencia { get { return (Sumarizaciones) m_intSumarizaPre; } }

        public string Oi_TH_Exc { get { return m_strOi_tipohora_exc;} set { m_strOi_tipohora_exc = value;} }
        public string Oi_TH_Aus { get { return m_strOi_tipohora_aus;} set { m_strOi_tipohora_aus = value;} }

        public static string Excedent { get {return "-1" ;} }
    }

    public struct CalculoHoras {
        public string OiHora;
        public double CantHoras;
        public string OiEstructura;
        public Sumarizaciones EnPresencia;
        public Sumarizaciones EnAusencia;
    }

    /// <summary>
    /// Clase utilizada para crear un XML sencillo de parámetros para los queries.
    /// </summary>
    public class clsParameter {
        private string m_strElementName;
        private Hashtable m_htaAttributes;

        /// <summary>
        /// Constructor
        /// </summary>
        public clsParameter() {
            m_htaAttributes = new Hashtable();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pstrElementName">Nombre del elemento principal.</param>
        public clsParameter(string pstrElementName) : this() {
            m_strElementName = pstrElementName;
        }

        /// <summary>
        /// Agrega un nuevo atributo al XML de parámetros.
        /// </summary>
        /// <param name="pstrName">Nombre del atributo. Si el atributo existe se actualiza el valor.</param>
        /// <param name="pstrValue">Valor del atributo.</param>
        /// <returns></returns>
        public void Add(string pstrName, string pstrValue) {
            if (m_htaAttributes.ContainsKey(pstrName))
                m_htaAttributes[pstrName] = pstrValue;
            else
                m_htaAttributes.Add(pstrName, pstrValue);
        }

        /// <summary>
        /// Retorna el XML de parametros en formato string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            string strResult = "<" + m_strElementName + " ";

            foreach (string strKey in m_htaAttributes.Keys)
                strResult = strResult + strKey + "=\"" + m_htaAttributes[strKey] + "\" ";

            strResult = strResult + "/>";

            return strResult;
        }

        /// <summary>
        /// Retorna el XML de parametros en un documento XML.
        /// </summary>
        /// <returns></returns>
        public XmlDocument ToXmlDocument() {
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.LoadXml(this.ToString());
            return xmlResult;
        }
    }

  /// <summary>
  /// Objeto de parámetros utilizado para el Reporte de Asistencias
  /// </summary>
  public class clsParamsA : clsBaseParams {

    //Variables principales
    private DateTime m_dteFechaInicio;
    private DateTime m_dteFechaFin;
    private int m_intOiPersonalEmp;

    //Variables de la tabla parámetros
    private int m_intPreIng = 0;
    private int m_intPostIng = 0;
    private int m_intPreEgr = 0;
    private int m_intPostEgr = 0;
  private int m_intPreIng2 = 0;
    private int m_intPostIng2 = 0;
    private int m_intPreEgr2 = 0;
    private int m_intPostEgr2 = 0;

    //Variables de uso interno
    NomadXML m_xmlLegajo;
    Hashtable m_htaHorarios;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pxmlParams">XML con los parametros recibidos.</param>
    public clsParamsA(int pOi_personal_emp, DateTime pDesde, DateTime pHasta) {

      //Carga los parámetros principales
      this.m_dteFechaInicio = pDesde;
      this.m_dteFechaFin = pHasta;
      this.m_intOiPersonalEmp = pOi_personal_emp;

      //Obtiene la lista de personas
      this.LoadLegajo(this.m_intOiPersonalEmp);

      //Carga los parametros de la Base de Datos
      this.GetDBParams();

    }

    /// <summary>
    /// Carga los datos adicionales de los legajos a utililzar.
    /// </summary>
    /// <param name="pxmlLegajos">Elemento LEGAJOS.</param>
    /// <returns></returns>
    private void LoadLegajo(int pintOiPersonal) {
      string strResult;

      NomadXML nxmParametros = new NomadXML("DATOS");
      nxmParametros.SetAttr("oi_personal_emps", pintOiPersonal.ToString());

      //Ejecuta el query
      strResult = NomadEnvironment.QueryString(Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.QRY_DatosPersonal, nxmParametros.ToString());

      this.m_xmlLegajo = new NomadXML(strResult).FirstChild();
      if (this.m_xmlLegajo.FirstChild() != null)
        this.m_xmlLegajo = this.m_xmlLegajo.FirstChild();
      else
        throw new Exception("No se encontró la información sobre el legajo solicitado.");

    }

    /// <summary>
    /// //Trae los parámetros de la base de datos (tabla ORG26_PARAMETROS)
    /// </summary>
    private void GetDBParams() {
      NomadXML xmlDBParams;
      string strTemporalValue;

      //Crea el objeto de Parametros
      NomadXML objQParams = new NomadXML("DATOS");
      objQParams.SetAttr("c_modulo", "TTA");
      objQParams.SetAttr("d_clase", "\\'Reporte\\'");

      //Ejecuta el query
      xmlDBParams = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Resources.QRY_Parametros, objQParams.ToString(), true);
      xmlDBParams = xmlDBParams.FirstChild();

      if (xmlDBParams.ChildLength == 0) return;

      //Recorre el xml para obtener los parametros
      for (NomadXML xmlParam = xmlDBParams.FirstChild(); xmlParam != null; xmlParam = xmlParam.Next()) {

        strTemporalValue = xmlParam.GetAttr("d_valor") == "" ? "0" : xmlParam.GetAttr("d_valor");

        switch (xmlParam.GetAttr("c_parametro")) {
          case "PreIng":  this.m_intPreIng    = int.Parse(strTemporalValue); break;
          case "PostIng": this.m_intPostIng   = int.Parse(strTemporalValue); break;
          case "PreEgr":  this.m_intPreEgr    = int.Parse(strTemporalValue); break;
          case "PostEgr": this.m_intPostEgr   = int.Parse(strTemporalValue); break;
      case "PreIng2":  this.m_intPreIng2  = int.Parse(strTemporalValue); break;
          case "PostIng2": this.m_intPostIng2 = int.Parse(strTemporalValue); break;
          case "PreEgr2":  this.m_intPreEgr2  = int.Parse(strTemporalValue); break;
          case "PostEgr2": this.m_intPostEgr2 = int.Parse(strTemporalValue); break;
        }
      }
    }

    /// <summary>
    /// Retorna un NomadXML con los datos del horario solicitado.
    /// </summary>
    /// <param name="pstrOiHorario"></param>
    /// <returns></returns>
    public NomadXML GetHorario(string pstrOiHorario) {
      if (this.m_htaHorarios == null)
        this.m_htaHorarios = NomadEnvironment.QueryHashtable(NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.Resources.QRY_Horarios, "", "oi_horario", false);

      if (this.m_htaHorarios.ContainsKey(pstrOiHorario))
        return (NomadXML) this.m_htaHorarios[pstrOiHorario];
      else
        throw new Exception ("Se está pidiendo un Horario que no existe. OI_HORARIO='" + pstrOiHorario + "'");
    }

    //-------------------------------------------------------------
    //Propiedades

    public NomadXML Legajo      { get {return this.m_xmlLegajo;} }
    public DateTime FechaInicio { get {return this.m_dteFechaInicio;} }
    public DateTime FechaFin    { get {return this.m_dteFechaFin;} }

    public int PreIng  { get {return this.m_intPreIng;} }
    public int PostIng { get {return this.m_intPostIng;} }
    public int PreEgr  { get {return this.m_intPreEgr;} }
    public int PostEgr { get {return this.m_intPostEgr;} }
  public int PreIng2  { get {return this.m_intPreIng2;} }
    public int PostIng2 { get {return this.m_intPostIng2;} }
    public int PreEgr2  { get {return this.m_intPreEgr2;} }
    public int PostEgr2 { get {return this.m_intPostEgr2;} }

  }

  /// <summary>
  /// Clase que precarga los datos necesarios para la generacion de la esperanza
  /// </summary>
  public class clsPreLoader {

    private DateTime dteFrom;
    private DateTime dteTo;

    private Hashtable htaParamLegajos;
    private Hashtable htaLegajos;

    private NomadXML xmlLegajosTemplate;
    private int intInLegajos;

    //Caches
    private NomadXML xmlTipoHoras = null;

    public clsPreLoader(DateTime pdteFrom, DateTime pdteTo) {
      this.dteFrom = pdteFrom;
      this.dteTo = pdteTo;

      this.intInLegajos = 100;

      this.CreateTemplate();
    }

    public clsPreLoader(string pstrDateFrom, string pstrDateTo) : this (StringUtil.str2date(pstrDateFrom), StringUtil.str2date(pstrDateTo)) {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="phtaLegajos"></param>
    public void SetLegajos(string pstrLegajos) {
      string[] arrLegs;
      arrLegs = pstrLegajos.Split(',');
      this.htaParamLegajos = new Hashtable();

      for (int x = 0; x < arrLegs.Length; x++) {
        if (!this.htaParamLegajos.ContainsKey(arrLegs[x]))
          this.htaParamLegajos.Add(arrLegs[x], arrLegs[x]);
      }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="phtaLegajos"></param>
    public void SetLegajos(Hashtable phtaLegajos) {
      this.htaParamLegajos = phtaLegajos;
    }

    /// <summary>
    /// Carga los datos necesarios para la generacion de la esperanza
    /// </summary>
    public void LoadData() {
      string strIn = "0";
      int intInCount = 0;

      //Carga los datos para las personas  -----------------------------------------------------------------------------

      //Blanquea el hashtable por las dudas
      this.htaLegajos = new Hashtable();

      //Recorre los legajos
      if (this.htaParamLegajos.Count > 0) {
        foreach (string strKey in this.htaParamLegajos.Keys) {

          if (!this.htaLegajos.ContainsKey(strKey)) {
            this.htaLegajos.Add(strKey, new NomadXML(this.xmlLegajosTemplate.ToString(true)).FirstChild());
          }

          strIn = strIn + "," + strKey;
          intInCount++;

          if (intInCount == this.intInLegajos) {
            //Ejecuta el query
            this.LoadLegajos(strIn);
            strIn = "0";
            intInCount = 0;
          }
        }

        if (intInCount > 0) {
          //Ejecuta el query
          this.LoadLegajos(strIn);
        }

      }
    }

    public NomadXML GetHASHLegajo(string pstrOIPerEmp, int pintWeek) {
      DateTime dteFromTmp;
      DateTime dteToTmp;

      //Obtiene las fechas correspondientes a la semana
      dteFromTmp = StringUtil.week2date(pintWeek);
      dteToTmp = dteFromTmp.AddDays(6);

      return this.GetHASHLegajo(pstrOIPerEmp, dteFromTmp, dteToTmp);

    }

    public NomadXML GetHASHLegajo(string pstrOIPerEmp, DateTime pdteFrom, DateTime pdteTo) {
      NomadXML xmlResult;
      NomadXML xmlLegajo;

      if (!this.htaLegajos.ContainsKey(pstrOIPerEmp)) {
        return null;
      }

      xmlLegajo = (NomadXML) this.htaLegajos[pstrOIPerEmp];
      xmlResult = new NomadXML(this.xmlLegajosTemplate.ToString(true));
      xmlResult = xmlResult.FirstChild();

      for (NomadXML xmlCol = xmlLegajo.FirstChild(); xmlCol != null; xmlCol = xmlCol.Next()) {
        this.AddSection(xmlResult, xmlCol, pdteFrom, pdteTo);
      }

      return xmlResult;

    }

    /*********************************************************************************************************
     * Metodos PRIVADOS
     *********************************************************************************************************/
    private void CreateTemplate() {
      this.xmlLegajosTemplate = new NomadXML("HASH");
      this.xmlLegajosTemplate.AddTailElement("CALENDARIOS");
      this.xmlLegajosTemplate.AddTailElement("HORARIOS");
      this.xmlLegajosTemplate.AddTailElement("NOVEDADES");
      this.xmlLegajosTemplate.AddTailElement("LICENCIAS");
      this.xmlLegajosTemplate.AddTailElement("HORASAUT");
      this.xmlLegajosTemplate.AddTailElement("TIPOSHORAS");
      this.xmlLegajosTemplate.AddTailElement("DETALLE_DIARIO");
    }

    private void LoadLegajos(string pstrIn) {
      NomadXML xmlParams;

      xmlParams = new NomadXML("PARAMS");
      xmlParams.SetAttr("oi_personal_emps", pstrIn);
      xmlParams.SetAttr("f_ini", this.dteFrom.AddDays(-1).ToString("yyyyMMdd"));
      xmlParams.SetAttr("f_fin", this.dteTo.AddDays(1).ToString("yyyyMMdd"));

      string a = xmlParams.ToString(true);

      ExeQryLegajos("CALENDARIOS", xmlParams);
      ExeQryLegajos("HORARIOS", xmlParams);
      ExeQryLegajos("NOVEDADES", xmlParams);
      ExeQryLegajos("LICENCIAS", xmlParams);
      ExeQryLegajos("HORASAUT", xmlParams);

      //Obtiene los tipos de horas
      if (this.xmlTipoHoras == null) {
        xmlParams.SetAttr("type", "THO");
        this.xmlTipoHoras = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.Resources.QRY_PRELOADHOPE, xmlParams.ToString(true));
        this.xmlTipoHoras = this.xmlTipoHoras.FirstChild().FirstChild();
      }

    }

    private void ExeQryLegajos(string pstrType, NomadXML pxmlParams) {
      NomadXML xmlResult;
      NomadXML xmlLegajo;
      NomadXML xmlCol;
      string strOI;
      string strType;

      switch (pstrType) {
        case "CALENDARIOS": strType = "CAL"; break;
        case "HORARIOS": strType = "HRS"; break;
        case "NOVEDADES": strType = "NOV"; break;
        case "LICENCIAS": strType = "LIC"; break;
        case "HORASAUT": strType = "HAU"; break;
        default: strType = ""; break;
      }

      pxmlParams.SetAttr("type", strType);
      xmlResult = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.Resources.QRY_PRELOADHOPE, pxmlParams.ToString(true));
      xmlResult = xmlResult.FirstChild().FirstChild();

      strOI = "";
      xmlCol = null;
      for (NomadXML xmlChild = xmlResult.FirstChild(); xmlChild != null; xmlChild = xmlChild.Next()) {

        if (strOI != xmlChild.GetAttr("oi_personal_emp")) {
          strOI = xmlChild.GetAttr("oi_personal_emp");
          xmlLegajo = (NomadXML)this.htaLegajos[strOI];

          //Agrega los registros
          xmlCol = xmlLegajo.FindElement(pstrType);
        }

        xmlCol.AddText(xmlChild.ToString(true));
      }
    }

    private void AddSection(NomadXML pxmlLegajo, NomadXML pxmlSection, DateTime pdteFrom, DateTime pdteTo) {
      NomadXML xmlLegSection;
      NomadXML xmlRowResult;

      DateTime dteFrom;
      DateTime dteTo;
      DateTime dteDate;

      xmlLegSection = pxmlLegajo.FindElement(pxmlSection.Name);

      if (pxmlSection.Name == "TIPOSHORAS") {
        xmlLegSection.AddText(this.xmlTipoHoras.ToString(false));

      } else {
        //Recorre la seccion
        for (NomadXML xmlRow = pxmlSection.FirstChild(); xmlRow != null; xmlRow = xmlRow.Next()) {
          dteFrom = StringUtil.str2date(xmlRow.GetAttr("f_ini"));
          dteTo   = StringUtil.str2date(xmlRow.GetAttr("f_fin"));
          dteDate = StringUtil.str2date(xmlRow.GetAttr("f_fecha"));

          switch (pxmlSection.Name) {
            case "CALENDARIOS": case "HORARIOS":
              if (pdteTo >= dteFrom && pdteFrom < dteTo) {
                xmlLegSection.AddText(xmlRow.ToString(true));
                xmlRowResult = xmlLegSection.LastChild();

                if (dteFrom < pdteFrom) xmlRowResult.SetAttr("f_ini", pdteFrom.ToString("yyyyMMdd"));
                if (dteTo > pdteTo) xmlRowResult.SetAttr("f_fin", pdteTo.AddDays(1).ToString("yyyyMMdd"));
              }
              break;

            case "LICENCIAS": case "HORASAUT":
              if (pdteTo >= dteFrom && pdteFrom <= dteTo) {
                xmlLegSection.AddText(xmlRow.ToString(true));
                xmlRowResult = xmlLegSection.LastChild();

                if (dteFrom < pdteFrom) xmlRowResult.SetAttr("f_ini", pdteFrom.ToString("yyyyMMdd"));
                if (dteTo > pdteTo) xmlRowResult.SetAttr("f_fin", pdteTo.ToString("yyyyMMdd"));
              }
              break;

            case "NOVEDADES":
              if (dteDate >= pdteFrom && dteDate <= pdteTo) {
                xmlLegSection.AddText(xmlRow.ToString(true));
                xmlRowResult = xmlLegSection.LastChild();
              }
              break;

          }
        }
      }

    }

  }
}


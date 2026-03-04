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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using NucleusRH.Base.Configuracion.Kubo;

//using NucleusRH.Base.Personal.LegajoEmpresa;

namespace NucleusRH.Base.Tiempos_Trabajados.Kubo
{

  public partial class ApiKubo
  {

    /// <summary>
    /// Obtiene un hash con codigos de los puntos de accesos (DOC) y los OI de las terminales RH (existentes) que fueron mapeados entre DOC y RH
    /// </summary>
    public static Hashtable GetHashMapeoPATerminales(NomadBatch oBtc)
    {

        Hashtable hash = new Hashtable();

        //hashTerminales [KEY=c_terminal / VALUE=oi_terminal]
        Hashtable hashTerminales = NomadEnvironment.QueryHashtableValue(NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.Resources.INFO, "", "COD", "ID", false);

        if (hashTerminales.Count == 0)
        {
          oBtc.Err("No se hallaron Terminales (asegúrese configurar Terminales)");
          return hash;
        }

        //d_valor_map = "101=R1;102=R2";
        string d_valor_map = NucleusRH.Base.Configuracion.Kubo.ApiKubo.ObtenerParametro("TTA_MAP_PA_TERMINALES");

        if (d_valor_map == "")
        {
           oBtc.Err("Se debe definir el mapeo entre Puestos (Doc) y Terminales (RH) en el parámetro TTA_MAP_PA_TERMINALES");
           return hash;
        }

        string[] pares = d_valor_map.Split(';');

        foreach(string par in pares)
        {
            string puestoDoc = par.Substring(0,par.IndexOf('='));
            string terminalRH = par.Substring(par.IndexOf('=') + 1,par.Length - par.IndexOf('=') -1);

            if (hashTerminales.ContainsKey(terminalRH))
            {
                //KEY: codigo_punto_acceso - value: oi_terminal
                hash.Add(puestoDoc, hashTerminales[terminalRH].ToString());
            }
            else
            {
                oBtc.Wrn("No se ha encontrado la Terminal [" + terminalRH + "] mapeada en el Puesto [" + puestoDoc + "] dentro del parámetro TTA_MAP_PUESTOS_TERMINALES");
            }
        }
        return hash;
    }

    //reordena la lista por el campo identificador para minimizar las consultar a la DB
    public static void ReordenarPorIdentificadorYId(ListaResultado<Fichada> lFichadas)
    {
        ArrayList arr = new ArrayList(lFichadas.Registros);
        arr.Sort(new FichadaComparerPorIdentificadorYId());
        lFichadas.Registros.Clear();
        foreach (Fichada f in arr)
        {
            lFichadas.Registros.Add(f);
        }
    }

    //obtiene las fichadas desde la API con la cantidad de llamados que sea necesario realizar
    public static ListaResultado<Fichada> ObtenerFichadasDesdeAPI(NomadXML xmlParam)
    {
        try
        {

          DateTime pfecha_desde = xmlParam.GetAttrDateTime("fecha");
          FechaHora fecha_desde = new FechaHora();
          fecha_desde.Day = pfecha_desde.Day;
          fecha_desde.Month = pfecha_desde.Month;
          fecha_desde.Year = pfecha_desde.Year;

          fecha_desde.Hour = 0;
          fecha_desde.Minute = 0;
          fecha_desde.Second = 0;
          fecha_desde.Millisecond = 1;

          string sfecha_hasta = xmlParam.GetAttr("fecha_hasta");

          DateTime pfecha_hasta = pfecha_desde;

          if (sfecha_hasta != "")
          {
            pfecha_hasta = DateTime.ParseExact(sfecha_hasta, "yyyyMMdd", CultureInfo.InvariantCulture);
          }

          FechaHora fecha_hasta = null;

          if (sfecha_hasta != "")
          {
            fecha_hasta = new FechaHora();
            fecha_hasta.Day = pfecha_hasta.Day;
            fecha_hasta.Month = pfecha_hasta.Month;
            fecha_hasta.Year = pfecha_hasta.Year;

            fecha_hasta.Hour = 23;
            fecha_hasta.Minute = 59;
            fecha_hasta.Second = 59;
            fecha_hasta.Millisecond = 999;
          }

          ListaResultado<Fichada> listaFichadasTemp;
          ListaResultado<Fichada> listaFichadas = new ListaResultado<Fichada>();
          Dictionary<string, object> dicParam = new Dictionary<string, object>();

          int ignorar = 0;
          do
          {
              dicParam["Empresa"] = "";
              dicParam["Legajo"] = null;
              dicParam["Tipo"] = "";
              dicParam["PuntoAcceso"] = "";

              if (sfecha_hasta != "")
              {
                //acá busca las fichadas cuya fecha hora estén dentro del rango de tiempo pasado
                dicParam["Modify"] = null;
                dicParam["From"] = fecha_desde;
                dicParam["To"] = fecha_hasta;
              }
              else
              {
                //acá busca todo lo creado o modificado a partir de una fecha
                dicParam["Modify"] = fecha_desde;
                dicParam["From"] = null;
                dicParam["To"] = null;
              }

              dicParam["Estados"] = ""; //C;A;M
              dicParam["IncluirExtension"] = false;
              dicParam["IncluirAnuladas"] = false; // ???????
              dicParam["Ignorar"] = ignorar;

              listaFichadasTemp = NucleusRH.Base.Configuracion.Kubo.ApiKubo.Enviar<ListaResultado<Fichada>>(dicParam, "listarFichadas");

              listaFichadas.Registros.AddRange(listaFichadasTemp.Registros);
              listaFichadas.Cantidad += listaFichadasTemp.Registros.Count;
              ignorar += 100;

          } while (listaFichadasTemp.Registros.Count > 0);

          //reordena la lista por el campo identificador para minimizar las consultar a la DB
          ReordenarPorIdentificadorYId(listaFichadas);

          return listaFichadas;
        }
        catch(Exception e)
        {
            throw e;
            return null;
        }

    }

    public static void ObtenerFichadas_AUTO()
    {
        NomadBatch objBatch = NomadBatch.GetBatch("Obtener Fichadas desde NucleusDoc (Automático)", "Obtener Fichadas desde NucleusDoc (Automático)");

        objBatch.SetPro(0);

        string d_valor = NucleusRH.Base.Configuracion.Kubo.ApiKubo.ObtenerParametro("f_ult_cambio_fic_doc");
        DateTime f_desde = d_valor != "" ? DateTime.ParseExact(d_valor, "ddMMyyyy", CultureInfo.InvariantCulture) : new DateTime(1899, 01, 01);

        objBatch.Log("Fecha_Obtenida:" + f_desde.ToString("dd/MM/yyyy"));

        NomadXML xmlParam = new NomadXML("<DATOS fecha='" + f_desde.ToString("yyyyMMdd")+"'  />");

        ObtenerFichadas(xmlParam);

        NucleusRH.Base.Configuracion.Kubo.ApiKubo.GuardarParametro("TTA","TTA","F","f_ult_cambio_fic_doc", DateTime.Now.ToString("ddMMyyyy"));

        objBatch.Log("Fecha_Guardada" + DateTime.Now.ToString("dd/MM/yyyy"));

        objBatch.SetPro(100);
    }

    public static void ObtenerFichadas(NomadXML xmlParam)
    {
        NomadBatch objBatch = NomadBatch.GetBatch("Obtener Fichadas desde NucleusDoc", "Obtener Fichadas desde NucleusDoc");

        try
        {
            xmlParam = xmlParam.FirstChild();

            Hashtable hashPATerminales = GetHashMapeoPATerminales(objBatch);
            if (hashPATerminales.Count == 0)
            {
                objBatch.Err("No se hallaron Terminales mapeadas (revise TTA_MAP_PA_TERMINALES).");
                return;
            }

            ListaResultado<Fichada> listaFichadas = ObtenerFichadasDesdeAPI(xmlParam);

            if (listaFichadas == null || listaFichadas.Registros.Count == 0)
            {
              objBatch.Log("No se hallaron fichadas para procesar.");
              return;
            }

            int q_creadas = 0, q_modificadas = 0, q_anuladas = 0, q_ignoradas = 0;

            string d_nro_tarjeta_tmp = "";
            Hashtable hashEMP_TJTA = new Hashtable();
            Hashtable hashTJTA_NA = new Hashtable();

            foreach (Fichada f in listaFichadas.Registros)
            {
                // --- Buscar terminal mapeada ---
                string oi_terminal = ObtenerTerminalAsignada(f, hashPATerminales, objBatch);
                if (oi_terminal == null)
                {
                    q_ignoradas++;
                    continue;
                }

                // --- Buscar empleados por tarjeta ---
                string d_nro_tarjeta = f.Persona.TipoIdentificador + f.Persona.Identificador.Replace("-", "");
                if (d_nro_tarjeta != d_nro_tarjeta_tmp)
                {
                    d_nro_tarjeta_tmp = d_nro_tarjeta;
                    hashEMP_TJTA = NomadEnvironment.QueryHashtableValue(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.QRY_PEMP_BY_TARJETA, "<DATO d_nro_tarjeta='" + d_nro_tarjeta + "' />", "OI_PERSONAL_EMP", "D_NRO_TARJETA", false);

                    if (hashEMP_TJTA == null || hashEMP_TJTA.Count == 0)
                    {
                        hashTJTA_NA.Add(d_nro_tarjeta, 1);
                        objBatch.Wrn("No se encontró empleado asociado a la tarjeta (" + d_nro_tarjeta + ")");
                        q_ignoradas++;
                        continue;
                    }
                }

                // Si la tarjeta ya fue marcada como no asociada, se ignora
                if (hashTJTA_NA.ContainsKey(d_nro_tarjeta))
                {
                    //NOTA; si quieren que las sucesivas fichadas de un empleado para el que que NO se encontró tarjeta NO se sumen (y solo se contabilice 1 de manera agrupada) hay que comentar la siguiente línea
                    q_ignoradas++;

                    //se incrementa el contador por si en algun momento quieren mostrar la cantidad de fichadas ignoradas de los empleados no asociados a tarjetas
                    hashTJTA_NA[d_nro_tarjeta] = (int)hashTJTA_NA[d_nro_tarjeta] + 1;

                    continue;

                }

                // --- Procesar fichada para cada empleado asociado ---
                foreach (DictionaryEntry item in hashEMP_TJTA)
                {
                    string oi_personal_emp = item.Key.ToString();
                    if (!ProcesarFichadaEmpleado(f, oi_personal_emp, oi_terminal, objBatch, ref q_creadas, ref q_modificadas, ref q_anuladas, ref q_ignoradas))
                    {
                        continue;
                    }
                }
            }

            int q_total = q_creadas + q_modificadas + q_anuladas + q_ignoradas;

            objBatch.Log("Total de Fichadas Procesadas: " + q_total);
            objBatch.Log(" - Creadas: " + q_creadas);
            objBatch.Log(" - Modificadas: " + q_modificadas);
            objBatch.Log(" - Anuladas: " + q_anuladas);
            objBatch.Log(" - Ignoradas: " + q_ignoradas);
        }
        catch (Exception e)
        {
            objBatch.Err("Error en método ObtenerFichadas: " + e.Message);
        }
    }

    // ----------------------------------------------------------------------
    // Métodos auxiliares (idéntica lógica, sólo aislados por claridad)
    // ----------------------------------------------------------------------

    private static string ObtenerTerminalAsignada(Fichada f, Hashtable hashPATerminales, NomadBatch objBatch)
    {
        const string Asterisco = "*";

        if (!hashPATerminales.ContainsKey(f.PuntoAcceso.Codigo) && !hashPATerminales.ContainsKey(Asterisco))
        {
            objBatch.Wrn("El Punto de Acceso (" + f.PuntoAcceso.Codigo + ") no fue mapeado a ninguna Terminal.");
            return null;
        }

        if (hashPATerminales.ContainsKey(f.PuntoAcceso.Codigo))
            return hashPATerminales[f.PuntoAcceso.Codigo].ToString();

        return hashPATerminales[Asterisco].ToString();
    }

    private static bool ProcesarFichadaEmpleado(
        Fichada f, string oi_personal_emp, string oi_terminal, NomadBatch objBatch,
        ref int q_creadas, ref int q_modificadas, ref int q_anuladas, ref int q_ignoradas)
    {
        NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP legajo = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oi_personal_emp, false);
        NucleusRH.Base.Organizacion.Empresas.EMPRESA empresa = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(legajo.oi_empresa.ToString(), false);

        // Ignorar legajos inactivos
        if (legajo.oi_indic_activo.ToString() == "4")
        {
            objBatch.Wrn("No se registró fichada para el legajo (" + legajo.e_numero_legajo + ") porque está inactivo en la empresa (" + empresa.d_empresa + ").");
            q_ignoradas++;
            return false;
        }

        string fichadaId = "doc_" + f.Id + "_oie_" + oi_personal_emp;
        string oi_fichadasing = NomadEnvironment.QueryValue("TTA07_FICHADASING", "oi_fichadasing", "c_externo", fichadaId, "", false);
        DateTime fDoc_fecha_hora = KuboUtcToLocal(f.FechaHora);

        //obtiene una fichada existe o crea una nueva (si corresponde)
        NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING fichada = ObtenerOCrearFichada(fichadaId, oi_fichadasing, fDoc_fecha_hora, f.Estado, objBatch, ref q_creadas, ref q_ignoradas, ref q_modificadas, ref q_anuladas);
        if (fichada == null) return false;

        // --- Completar datos de la fichada ---
        fichada.c_origen = "T";
        fichada.oi_terminal = oi_terminal;
        fichada.oi_personal_emp = oi_personal_emp;
        fichada.e_numero_legajo = legajo.e_numero_legajo;

        if (f.Estado == "A")
            fichada.c_estado = "A";
        else
            fichada.c_estado = "P";

        if (f.Clase == "E") { fichada.l_entrada = true; fichada.c_tipo = "E"; }
        else if (f.Clase == "S") { fichada.l_entrada = false; fichada.c_tipo = "S"; }
        else { fichada.l_entrada = false; fichada.c_tipo = "I"; }

        //convierte la FechaHora de KUBO que es UTC y retorna un DateTime con hora local
        fichada.f_fechahora = KuboUtcToLocal(f.FechaHora);

        //actualiza el codigo de la fichada la E para indicar que se importó desde una aplización Externa
        string strc_fichadasing = fichada.f_fechahora.ToString("yyyyMMddHHmm") + fichada.e_numero_legajo.ToString() + "E" + fichada.c_tipo;
        fichada.c_fichadasing = strc_fichadasing;

        NomadEnvironment.GetCurrentTransaction().Save(fichada);
        return true;
    }

    public static DateTime KuboUtcToLocal(FechaHora fh)
    {
        //convierte la FechaHora de KUBO que es UTC y retorna un DateTime con hora local
        DateTime fh_utc = new DateTime(fh.Year, fh.Month, fh.Day, fh.Hour, fh.Minute, fh.Second, fh.Millisecond, DateTimeKind.Utc);

        return fh_utc.ToLocalTime();
    }

    public static DateTime DateTimeUtcToLocal(DateTime fechaUtc)
    {
        TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        return fechaUtc.Add(offset);
    }

    ///
    //obtiene una fichada existe o crea una nueva (si corresponde)
    ///
    private static NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ObtenerOCrearFichada(
        string fichadaId, string oi_fichadasing, DateTime fichadaDocFechaHora, string estadoDoc, NomadBatch objBatch,
        ref int q_creadas, ref int q_ignoradas, ref int q_modificadas, ref int q_anuladas)
    {
        NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING fichada;

        if (oi_fichadasing == "")
        {
            if (estadoDoc == "A")
            {
                objBatch.Wrn("Fichada Ignorada (" + fichadaDocFechaHora + ") porque fué Anulada en DOC y aún no existe en RH.");
                q_ignoradas++;
                return null;
            }

            fichada = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();

            fichada.d_user_create = NomadProxy.GetProxy().UserName;
            fichada.f_create = DateTime.Now;
            fichada.c_externo = fichadaId;

            q_creadas++;
        }
        else
        {
            fichada = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.Get(oi_fichadasing);

            if (fichada == null)
            {
                objBatch.Wrn("No se pudo obtener la fichada (" + fichadaId + ").");
                q_ignoradas++;
                return null;
            }

            if (!string.IsNullOrEmpty(fichada.d_user_edit))
            {
                objBatch.Wrn("Fichada Ignorada (" + fichada.f_fechahora.ToString("dd/MM/yyyy HH:mm") + ") legajo (" + fichada.e_numero_legajo.ToString() + ") porque ya fue editada en RH.");
                q_ignoradas++;
                return null;
            }

            if (estadoDoc == "A")
            {
              objBatch.Wrn("Fichada Anulada (" + fichada.f_fechahora.ToString("dd/MM/yyyy HH:mm") + ") legajo (" + fichada.e_numero_legajo.ToString() + ").");
              q_anuladas++;
            }
            else
            {
              q_modificadas++;
            }
        }

        return fichada;
    }

    #region clases

      public class Fichada
      {
        private Persona m_persona;
        private int m_id;
        private string m_tipo;
        private FechaHora m_fechaHora;
        private string m_clase;
        private string m_estado;
        private bool m_anulada;
        private bool m_modificada;
        private CoordenadasSimple m_coordeneadas;
        private PuntoAcceso m_puntoAcceso;
        private string m_tipoMensaje;
        private string m_mensaje;
        private string m_extension;
        private string m_codigo;
        private string m_descripcion;

        public Persona Persona { get { return m_persona; } set { m_persona = value; } }
        public int Id { get { return m_id; } set { m_id = value; } }
        public string Tipo { get { return m_tipo; } set { m_tipo = value; } }
        public FechaHora FechaHora { get { return m_fechaHora; } set { m_fechaHora = value; } }
        public string Clase { get { return m_clase; } set { m_clase = value; } }
        public string Estado { get { return m_estado; } set { m_estado = value; } }
        public bool Anulada { get { return m_anulada; } set { m_anulada = value; } }
        public bool Modificada { get { return m_modificada; } set { m_modificada = value; } }
        public CoordenadasSimple Coordeneadas { get { return m_coordeneadas; } set { m_coordeneadas = value; } }
        public PuntoAcceso PuntoAcceso { get { return m_puntoAcceso; } set { m_puntoAcceso = value; } }
        public string TipoMensaje { get { return m_tipoMensaje; } set { m_tipoMensaje = value; } }
        public string Mensaje { get { return m_mensaje; } set { m_mensaje = value; } }
        public string Extension { get { return m_extension; } set { m_extension = value; } }
        public string Codigo { get { return m_codigo; } set { m_codigo = value; } }
        public string Descripcion { get { return m_descripcion; } set { m_descripcion = value; } }
      }

      public class Persona
      {
        private string m_configuracionRegional;
        private string m_tipoIdentificador;
        private string m_identificador;
        private string m_mail;
        private string m_mailLaboral;
        private string m_codigoUsuario;
        private string m_celular;
        private FechaHora m_ultimoAcceso;
        private Grupo m_grupo;
        private string m_certificado;
        private string m_codificadoras;
        private string m_extensiones;
        private string m_codigo;
        private string m_descripcion;

        public string ConfiguracionRegional { get { return m_configuracionRegional; } set { m_configuracionRegional = value; } }
        public string TipoIdentificador { get { return m_tipoIdentificador; } set { m_tipoIdentificador = value; } }
        public string Identificador { get { return m_identificador; } set { m_identificador = value; } }
        public string Mail { get { return m_mail; } set { m_mail = value; } }
        public string MailLaboral { get { return m_mailLaboral; } set { m_mailLaboral = value; } }
        public string CodigoUsuario { get { return m_codigoUsuario; } set { m_codigoUsuario = value; } }
        public string Celular { get { return m_celular; } set { m_celular = value; } }
        public FechaHora UltimoAcceso { get { return m_ultimoAcceso; } set { m_ultimoAcceso = value; } }
        public Grupo Grupo { get { return m_grupo; } set { m_grupo = value; } }
        public string Certificado { get { return m_certificado; } set { m_certificado = value; } }
        public string Codificadoras { get { return m_codificadoras; } set { m_codificadoras = value; } }
        public string Extensiones { get { return m_extensiones; } set { m_extensiones = value; } }
        public string Codigo { get { return m_codigo; } set { m_codigo = value; } }
        public string Descripcion { get { return m_descripcion; } set { m_descripcion = value; } }
      }

      public class Grupo
      {
        private string m_codigo;
        private string m_descripcion;

        public string Codigo { get { return m_codigo; } set { m_codigo = value; } }
        public string Descripcion { get { return m_descripcion; } set { m_descripcion = value; } }
      }

      public class FechaHora
      {
        private int m_year;
        private int m_month;
        private int m_day;
        private int m_hour;
        private int m_minute;
        private int m_second;
        private int m_millisecond;

        public int Year { get { return m_year; } set { m_year = value; } }
        public int Month { get { return m_month; } set { m_month = value; } }
        public int Day { get { return m_day; } set { m_day = value; } }
        public int Hour { get { return m_hour; } set { m_hour = value; } }
        public int Minute { get { return m_minute; } set { m_minute = value; } }
        public int Second { get { return m_second; } set { m_second = value; } }
        public int Millisecond { get { return m_millisecond; } set { m_millisecond = value; } }
      }

      public class CoordenadasSimple
      {
        private double m_lat;
        private double m_lon;

        public double LAT { get { return m_lat; } set { m_lat = value; } }
        public double LON { get { return m_lon; } set { m_lon = value; } }
      }

      public class PuntoAcceso
      {
        private bool m_habilitado;
        private string m_direccion;
        private string m_imagen;
        private CoordenadasSimple m_coordenadas;
        private double m_distancia;
        private string m_excepciones;
        private string m_codigo;
        private string m_descripcion;

        public bool Habilitado { get { return m_habilitado; } set { m_habilitado = value; } }
        public string Direccion { get { return m_direccion; } set { m_direccion = value; } }
        public string Imagen { get { return m_imagen; } set { m_imagen = value; } }
        public CoordenadasSimple Coordenadas { get { return m_coordenadas; } set { m_coordenadas = value; } }
        public double Distancia { get { return m_distancia; } set { m_distancia = value; } }
        public string Excepciones { get { return m_excepciones; } set { m_excepciones = value; } }
        public string Codigo { get { return m_codigo; } set { m_codigo = value; } }
        public string Descripcion { get { return m_descripcion; } set { m_descripcion = value; } }
      }

      public class FichadaComparerPorIdentificadorYId : IComparer
      {
          public int Compare(object x, object y)
          {
              Fichada f1 = (Fichada)x;
              Fichada f2 = (Fichada)y;

              // --- Comparación principal: Identificador de Persona ---
              string id1 = (f1.Persona != null && f1.Persona.Identificador != null) ? f1.Persona.Identificador : "";
              string id2 = (f2.Persona != null && f2.Persona.Identificador != null) ? f2.Persona.Identificador : "";

              int resultado = String.Compare(id1, id2, true); // ignorar mayúsculas/minúsculas

              // --- Si los identificadores son iguales, ordenar por Id de Fichada ---
              if (resultado == 0)
              {
                  // Evitar nulls o valores no numéricos si el Id es nullable
                  long idF1 = f1.Id;
                  long idF2 = f2.Id;

                  if (idF1 < idF2) resultado = -1;
                  else if (idF1 > idF2) resultado = 1;
                  else resultado = 0;
              }

              return resultado;
          }
      }

    #endregion

  }
}



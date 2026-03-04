using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.SeleccionDePostulantes.CVs;
using NucleusRH.Base.SeleccionDePostulantes.Avisos;
using NucleusRH.Base.SeleccionDePostulantes.Puestos;

namespace NucleusRH.Base.SeleccionDePostulantes.CVs {

  public class SDPConceptosBase {

    protected NomadXML xmlParams;
    protected NomadBatch objBatch;

    protected AVISO objAviso;
    protected PUESTO objPuesto;

    protected Hashtable htaQueryCache;
    protected Hashtable htaEstudiosCV;
    protected Hashtable htaNivelesEstudios;

    public SDPConceptosBase() {
      this.xmlParams = null;
      this.objPuesto = null;
      this.htaNivelesEstudios = null;
      this.htaQueryCache = new Hashtable();
      this.htaEstudiosCV = new Hashtable(); this.htaEstudiosCV.Add("OICV", "");

      this.objBatch = NomadBatch.GetBatch("ReclutarPersonal", "");
    }

    /// <summary>
    /// Setea los parametros generales para el procesamiento de los meritos
    /// </summary>
    /// <param name="pxmlParams">XML con los parámetros</param>
    public void SetParams(NomadXML pxmlParams) {
      this.xmlParams = pxmlParams;

      if (this.xmlParams != null) {
        this.objAviso = AVISO.Get(pxmlParams.GetAttrInt("oi_aviso"));

        //Valida el Aviso
        if (this.objAviso == null)
          throw new Exception("No se encontró el Aviso solicitado.");

        //Valida que el aviso tenga un Puesto asociado
        if (this.objAviso.oi_puesto == null || this.objAviso.oi_puesto == "")
          throw new Exception("El aviso no tiene un Puesto asociado.");

        //Obtiene el puesto asociado al aviso
        NomadBatch.Trace("Recuperando el Puesto con OI: '" + this.objAviso.oi_puesto + "'");
        this.objPuesto = PUESTO.Get(this.objAviso.oi_puesto);

        //Pasa al aviso a estado Reclutando
        if (this.objAviso.c_estado != "R") {
          this.objAviso.c_estado = "R";
          NomadEnvironment.GetCurrentTransaction().Save(this.objAviso);
        }
      }
    }

    /// <summary>
    /// Metodo generico
    /// </summary>
    public NomadXML Reclutar() {
      NomadXML xmlCVs;
      NomadXML xmlRecluts = new NomadXML("DATA");
      NomadXML xmlReclut;      
      CV objCV;
      int intCVTotal;
      int intCVActual;
      int intExcluded;
      DateTime dteStartPro = DateTime.Now;
      SortedList solMeritos = new SortedList(); //SortedList para retornar el XML ordenado por resultado de los meritos

      this.objBatch.Log("Comienza el proceso de Reclutamiento de personal.");

      xmlRecluts.SetAttr("oi_aviso", this.objAviso.Id);

      //Obtiene los meritos y valida que existan
      {
        NomadBatch.Trace("Validando los meritos");

        NomadXML xmlMeritos;
        xmlMeritos = this.GetMeritos();

        if (xmlMeritos == null || xmlMeritos.ChildLength == 0) {
          throw new Exception("No se pasaron los meritos a procesar.");
        }

        //Agrega el merito asignado a cada uno de los conceptos
        for (NomadXML xmlMerito = xmlMeritos.FirstChild(); xmlMerito != null; xmlMerito = xmlMerito.Next()) {
          if (xmlMerito.GetAttr("n_merito") == "") continue;
          xmlRecluts.SetAttr("mer_" + xmlMerito.GetAttr("id"), xmlMerito.GetAttrDouble("n_merito"));
        }

      }

      this.xmlParams.SetAttr("c_tipo_aviso", this.objAviso.c_tipo);
      NomadEnvironment.GetTrace().Info("this.xmlParams -- " + this.xmlParams.ToString());
      //Ejecuta el query para obtener la lista de CV filtrados
      xmlCVs = NomadEnvironment.QueryNomadXML(Reclutados.RECLUTADO.Resources.QRY_Candidatos, this.xmlParams.ToString()).FirstChild();

      this.objBatch.SetMess("Procesando los CVs");

      intCVTotal = xmlCVs.ChildLength;
      intCVActual = 0;
      intExcluded = 0;

      //Recorre los CV y les aplica los conceptos
      for (NomadXML xmlCV = xmlCVs.FirstChild(); xmlCV != null; xmlCV = xmlCV.Next()) {

        intCVActual++;
        this.objBatch.SetPro(0, 90, intCVTotal, intCVActual);

        this.objBatch.SetMess("Procesando los CVs (" + intCVActual.ToString() + " de " + intCVTotal.ToString() + ")");

        //Obtiene el CV y ejecuta el proceso de meritos
        objCV = NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Get(xmlCV.GetAttr("oi_cv"));        

        xmlReclut = this.ApplyConcepts(objCV, xmlCV.GetAttrBool("postulado"));

        if (xmlReclut.GetAttrBool("excluded")) {
          intExcluded++;
        } else {
          solMeritos.Add(xmlReclut.GetAttrDouble("result").ToString("000000000000000000000000.00") + "-" + solMeritos.Count.ToString(), xmlReclut);
        }

      }

      this.objBatch.SetPro(90);
      this.objBatch.Log("Ordenando los CVs por merito.");

      //Agrega los reclutados al XML de manera ordenada por merito
      foreach (string strKey in solMeritos.Keys) {
        xmlRecluts.AddHeadElement((NomadXML)solMeritos[strKey]);
      }

      TimeSpan TotalProc = DateTime.Now.Subtract(dteStartPro);

      this.objBatch.SetMess("Proceso finalizado");
      this.objBatch.SetPro(100);

      this.objBatch.Log("El proceso de Reclutamiento de personal ha finalizado.");
      this.objBatch.Log("CVs procesados: " + intCVTotal.ToString());
      this.objBatch.Log("CVs excluidos: " + intExcluded.ToString());
      this.objBatch.Log("Tiempo total: " + TotalProc.Hours.ToString() + ":" + TotalProc.Minutes.ToString() + " (total minutos: " + TotalProc.TotalMinutes.ToString() + ")");

      return xmlRecluts;
    }

    /// <summary>
    /// Metodo generico con query adaptable a los clientes
    /// </summary>
    public NomadXML Reclutar(string sql)
    {
        NomadXML xmlCVs;
        NomadXML xmlRecluts = new NomadXML("DATA");
        NomadXML xmlReclut;
        CV objCV;
        int intCVTotal;
        int intCVActual;
        int intExcluded;
        DateTime dteStartPro = DateTime.Now;
        SortedList solMeritos = new SortedList(); //SortedList para retornar el XML ordenado por resultado de los meritos

        this.objBatch.Log("Comienza el proceso de Reclutamiento de personal.");

        xmlRecluts.SetAttr("oi_aviso", this.objAviso.Id);

        //Obtiene los meritos y valida que existan
        {
            NomadBatch.Trace("Validando los meritos");

            NomadXML xmlMeritos;
            xmlMeritos = this.GetMeritos();

            if (xmlMeritos == null || xmlMeritos.ChildLength == 0)
            {
                throw new Exception("No se pasaron los meritos a procesar.");
            }

            //Agrega el merito asignado a cada uno de los conceptos
            for (NomadXML xmlMerito = xmlMeritos.FirstChild(); xmlMerito != null; xmlMerito = xmlMerito.Next())
            {
                if (xmlMerito.GetAttr("n_merito") == "") continue;
                xmlRecluts.SetAttr("mer_" + xmlMerito.GetAttr("id"), xmlMerito.GetAttrDouble("n_merito"));
            }

        }

        this.xmlParams.SetAttr("c_tipo_aviso", this.objAviso.c_tipo);
        NomadEnvironment.GetTrace().Info("this.xmlParams -- " + this.xmlParams.ToString());
        //Ejecuta el query para obtener la lista de CV filtrados
        xmlCVs = NomadEnvironment.QueryNomadXML(sql, this.xmlParams.ToString()).FirstChild();

        this.objBatch.SetMess("Procesando los CVs");

        intCVTotal = xmlCVs.ChildLength;
        intCVActual = 0;
        intExcluded = 0;

        //Recorre los CV y les aplica los conceptos
        for (NomadXML xmlCV = xmlCVs.FirstChild(); xmlCV != null; xmlCV = xmlCV.Next())
        {

            intCVActual++;
            this.objBatch.SetPro(0, 90, intCVTotal, intCVActual);

            this.objBatch.SetMess("Procesando los CVs (" + intCVActual.ToString() + " de " + intCVTotal.ToString() + ")");

            //Obtiene el CV y ejecuta el proceso de meritos
            objCV = NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Get(xmlCV.GetAttr("oi_cv"));

            xmlReclut = this.ApplyConcepts(objCV, xmlCV.GetAttrBool("postulado"));

            if (xmlReclut.GetAttrBool("excluded"))
            {
                intExcluded++;
            }
            else
            {
                solMeritos.Add(xmlReclut.GetAttrDouble("result").ToString("000000000000000000000000.00") + "-" + solMeritos.Count.ToString(), xmlReclut);
            }

        }

        this.objBatch.SetPro(90);
        this.objBatch.Log("Ordenando los CVs por merito.");

        //Agrega los reclutados al XML de manera ordenada por merito
        foreach (string strKey in solMeritos.Keys)
        {
            xmlRecluts.AddHeadElement((NomadXML)solMeritos[strKey]);
        }

        TimeSpan TotalProc = DateTime.Now.Subtract(dteStartPro);

        this.objBatch.SetMess("Proceso finalizado");
        this.objBatch.SetPro(100);

        this.objBatch.Log("El proceso de Reclutamiento de personal ha finalizado.");
        this.objBatch.Log("CVs procesados: " + intCVTotal.ToString());
        this.objBatch.Log("CVs excluidos: " + intExcluded.ToString());
        this.objBatch.Log("Tiempo total: " + TotalProc.Hours.ToString() + ":" + TotalProc.Minutes.ToString() + " (total minutos: " + TotalProc.TotalMinutes.ToString() + ")");

        return xmlRecluts;
    }
    //------------------------------------------------------------------------------------------------------
    // PROPIEDADES
    //------------------------------------------------------------------------------------------------------
    public string OIAviso {
      get { return this.objPuesto.Id; }
    }

    //------------------------------------------------------------------------------------------------------
    // Metodos PROTECTED
    //------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Recorna el código de Nivel de Estudio
    /// </summary>
    /// <param name="pOI_Nivel_Estudio">OI de Nivel de Estudio</param>
    /// <returns></returns>
    protected int GetNivelEstudio(string pOI_Nivel_Estudio) {
      if (this.htaNivelesEstudios == null) {
        this.htaNivelesEstudios = NomadEnvironment.QueryHashtableValue(NucleusRH.Base.Personal.Niveles_Estudio.NIVEL_ESTUDIO.Resources.QRY_NivelesEstudios, "<PARAMS />", "oi_nivel_estudio", "c_nivel_estudio", false);
      }

      if (this.htaNivelesEstudios.ContainsKey(pOI_Nivel_Estudio))
        return Convert.ToInt32(this.htaNivelesEstudios[pOI_Nivel_Estudio]);
      else
        return 0;
    }

    protected ArrayList GetEstudiosXAreaCV(CV pobjCV, string pstrOI_area_est) {
      ArrayList arrEstudios;
      string strOICV;

      strOICV = (string) this.htaEstudiosCV["OICV"];

      if (strOICV != pobjCV.Id) {
        //La colección de estudios no pertenece al CV requerido. Crea un nuevo caché del CV para no incrementar la memoria.

        this.htaEstudiosCV = new Hashtable();
        this.htaEstudiosCV.Add("OICV",  pobjCV.Id);
        this.htaEstudiosCV.Add("VACIO", new ArrayList());

        //Recorre los estudios y los va guardando segun el area
        foreach (ESTUDIO_CV objEstudio in pobjCV.ESTUDIOS_CV) {

          if (this.htaEstudiosCV.ContainsKey(objEstudio.oi_area_est))
            arrEstudios = (ArrayList)this.htaEstudiosCV[objEstudio.oi_area_est];
          else {
            arrEstudios = new ArrayList();
            this.htaEstudiosCV.Add(objEstudio.oi_area_est, arrEstudios);
          }
          arrEstudios.Add(objEstudio);
        }
      }

      if (this.htaEstudiosCV.ContainsKey(pstrOI_area_est))
        return (ArrayList) this.htaEstudiosCV[pstrOI_area_est];
      else
        return (ArrayList) this.htaEstudiosCV["VACIO"];
    }

    protected NomadXML GetMeritos() {
      if (this.xmlParams == null)
        return null;
      else
        return this.xmlParams.FirstChild();
    }

    /// <summary>
    /// Retorna un NomadXML con los idiomas del puesto del aviso actual
    /// </summary>
    /// <returns></returns>
    protected NomadXML GetIdiomasPuesto() {
      return this.GetIdiomasPuesto(this.objPuesto);
    }

    /// <summary>
    /// Retorna un NomadXML con los idiomas del puesto pedido
    /// </summary>
    /// <param name="pOIPuesto">OI del puesto</param>
    /// <returns></returns>
    protected NomadXML GetIdiomasPuesto(string pOIPuesto) {
      PUESTO objPuesto = PUESTO.Get(pOIPuesto);
      if (objPuesto == null) throw new Exception("Error en GetIdiomasPuesto. El puesto solicitado no se encuentra.");

      return this.GetIdiomasPuesto(objPuesto);
    }

    /// <summary>
    /// Retorna un NomadXML con los idiomas del puesto pasado
    /// </summary>
    /// <param name="pobjPuesto">Puesto</param>
    /// <returns></returns>
    protected NomadXML GetIdiomasPuesto(PUESTO pobjPuesto) {
      return this.ExecuteQuery("NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.QRY_Idiomas_Req", NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.Resources.QRY_Idiomas_Req, pobjPuesto.SerializeAll());
    }

    /// <summary>
    /// Retorna un NomadXML con los estudios del puesto del aviso actual
    /// </summary>
    /// <returns></returns>
    protected NomadXML GetEstudiosPuesto() {
      return this.GetEstudiosPuesto(this.objPuesto);
    }

    /// <summary>
    /// Retorna un NomadXML con los estudios del puesto pedido
    /// </summary>
    /// <param name="pOIPuesto">OI del puesto</param>
    /// <returns></returns>
    protected NomadXML GetEstudiosPuesto(string pOIPuesto) {
      PUESTO objPuesto = PUESTO.Get(pOIPuesto);
      if (objPuesto == null) throw new Exception("Error en GetEstudiosPuesto. El puesto solicitado no se encuentra.");

      return this.GetEstudiosPuesto(objPuesto);
    }

    /// <summary>
    /// Retorna un NomadXML con los estudios del puesto pasado
    /// </summary>
    /// <param name="pobjPuesto">Puesto</param>
    /// <returns></returns>
    protected NomadXML GetEstudiosPuesto(PUESTO pobjPuesto) {
      return this.ExecuteQuery("NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.QRY_Estudios_Req", NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.Resources.QRY_Estudios_Req, pobjPuesto.SerializeAll());
    }

    /// <summary>
    /// Retorna un NomadXML con los puestos requeridos del puesto del aviso actual
    /// </summary>
    /// <returns></returns>
    protected NomadXML GetExpPuesto() {
      return this.GetExpPuesto(this.objPuesto);
    }

    /// <summary>
    /// Retorna un NomadXML con los puestos requeridos del puesto del aviso actual
    /// </summary>
    /// <param name="pOIPuesto">OI del puesto</param>
    /// <returns></returns>
    protected NomadXML GetExpPuesto(string pOIPuesto) {
      PUESTO objPuesto = PUESTO.Get(pOIPuesto);
      if (objPuesto == null) throw new Exception("Error en GetExpPuesto. El puesto solicitado no se encuentra.");

      return this.GetExpPuesto(objPuesto);
    }

    /// <summary>
    /// Retorna un NomadXML con los puestos requeridos del puesto del aviso actual
    /// </summary>
    /// <param name="pobjPuesto">Puesto</param>
    /// <returns></returns>
    protected NomadXML GetExpPuesto(PUESTO pobjPuesto) {
      NomadEnvironment.GetTrace().Info("pobjPuesto -- " + pobjPuesto.SerializeAll());

      return this.ExecuteQuery("NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.QRY_Experiencia_Req", NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.Resources.QRY_Experiencia_Req, pobjPuesto.SerializeAll());
    }

    /// <summary>
    /// Retorna un NomadXML con los conocimientos informaticos del puesto del aviso actual
    /// </summary>
    /// <returns></returns>
    protected NomadXML GetConocInfPuesto() {
      return this.GetConocInfPuesto(this.objPuesto);
    }

    /// <summary>
    /// Retorna un NomadXML con los conocimientos informaticos del puesto del aviso actual
    /// </summary>
    /// <param name="pOIPuesto">OI del puesto</param>
    /// <returns></returns>
    protected NomadXML GetConocInfPuesto(string pOIPuesto) {
      PUESTO objPuesto = PUESTO.Get(pOIPuesto);
      if (objPuesto == null) throw new Exception("Error en GetConocInfPuesto. El puesto solicitado no se encuentra.");

      return this.GetConocInfPuesto(objPuesto);
    }

    /// <summary>
    /// Retorna un NomadXML con los conocimientos informaticos del puesto del aviso actual
    /// </summary>
    /// <param name="pobjPuesto">Puesto</param>
    /// <returns></returns>
    protected NomadXML GetConocInfPuesto(PUESTO pobjPuesto) {
      return this.ExecuteQuery("NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.QRY_Conoc_Inf_Req", NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.Resources.QRY_Conoc_Inf_Req, pobjPuesto.SerializeAll());
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="strName"></param>
    /// <param name="pstrSource"></param>
    /// <param name="pstrParams"></param>
    /// <returns></returns>
    protected NomadXML ExecuteQuery(string strName, string pstrSource, string pstrParams) {
      if (this.htaQueryCache.ContainsKey(strName))
      return (NomadXML)this.htaQueryCache[strName];

      NomadXML xmlResult = NomadEnvironment.QueryNomadXML(pstrSource, pstrParams);
      if (xmlResult != null) xmlResult = xmlResult.FirstChild();

      this.htaQueryCache.Add(strName, xmlResult);
      return xmlResult;
    }

    //------------------------------------------------------------------------------------------------------
    // Metodos PRIVATED
    //------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------
    // Metodos STATIC
    //------------------------------------------------------------------------------------------------------

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    static public SDPConceptosBase GetObject(NomadXML pxmlParams) {
      NomadBatch objBatch = NomadBatch.GetBatch("CrearObjeto", "");
      NomadXML xmlMeritos;
      string strMethod;
      string strMethods;
      string strCalls;
      int lib;
      double dblTest;

      strMethods = "";
      strCalls = "";

      //--------------------------------------------------------------------------------
      //Genera el Codigo de los meritos con solo los solicitados

      //Crea el XML de parametros para la obtencion de los meritos
      {
        NomadBatch.Trace("Obtiene la formula de los meritos.");
        NomadXML xmlParams = new NomadXML("PARAMS");
        xmlParams.SetAttr("meritos", "0");

        for (NomadXML xmlMerito = pxmlParams.FirstChild().FirstChild(); xmlMerito != null; xmlMerito = xmlMerito.Next()) {
          string strmerito = "0";

          if (xmlMerito.GetAttr("n_merito") != "")
            strmerito = xmlMerito.GetAttr("n_merito");
          else strmerito = "0";

          if (!double.TryParse(strmerito, out dblTest))
            objBatch.Wrn("Existen valores que no son numéricos en los meritos: " + xmlMerito.GetAttr("n_merito"));

          xmlParams.SetAttr("meritos", xmlParams.GetAttr("meritos") + "," + xmlMerito.GetAttr("id"));
          NomadBatch.Trace("Se agrega el merito: " + xmlMerito.GetAttr("id"));
        }

        if (xmlParams.GetAttr("meritos") == "0") {
          objBatch.Wrn("No se cargaron valores en los meritos.");
          return null;
        }

        NomadBatch.Trace("Parametros del Query NucleusRH.Base.SeleccionDePostulantes.Meritos.MERITO.Resources.QRY_Meritos:");
        NomadBatch.Trace(xmlParams.ToString());
        xmlMeritos = NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.Meritos.MERITO.Resources.QRY_Meritos, xmlParams.ToString());
        xmlMeritos = xmlMeritos.FirstChild();

      }

      for (NomadXML xmlMerito = xmlMeritos.FirstChild(); xmlMerito != null; xmlMerito = xmlMerito.Next()) {

        strMethod = NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO.Resources.TEMPL_MetodoRec;
        strMethod = strMethod.Replace("{NAME}", "merito_" + xmlMerito.GetAttr("oi_merito"));
        strMethod = strMethod.Replace("{CODE}", xmlMerito.GetAttr("t_formula"));
        strMethod = strMethod + "\n";

        strMethods = strMethods + strMethod;

        strCalls = strCalls + NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO.Resources.TEMPL_LlamadaMetodoRec.Replace("{ID}", xmlMerito.GetAttr("oi_merito"));
        strCalls = strCalls.Replace("{NAME}", "merito_" + xmlMerito.GetAttr("oi_merito")) + "\n";

      }

      strMethods = NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO.Resources.TEMPL_ReclutamientoCode.Replace("{METHODS}", strMethods);
      strMethods = strMethods.Replace("{CALL_METHODS}", strCalls);

      NomadBatch.Trace("-------------------------------------------------------------------------------------------------------------");
      NomadBatch.Trace("Codigo fuente que se compila:");
      NomadBatch.Trace(strMethods);
      NomadBatch.Trace("FIN - Codigo fuente que se compila:");
      NomadBatch.Trace("-------------------------------------------------------------------------------------------------------------");

      //--------------------------------------------------------------------------------
      //Genera la DLL
      SDPConceptosBase objResult = new SDPConceptosBase();

      string dllMAIN = objResult.GetType().Assembly.GetFiles()[0].Name;
      //string dllMAIN = "\\\\Maya\\NomadServer\\Desa183\\App\\Base-DESA\\DLL\\NucleusRHClassesV11.dll";
      string dllPATH;
      string dllTRG;
      dllPATH = dllMAIN.Substring(0, dllMAIN.LastIndexOf("\\"));
      dllMAIN = dllMAIN.Substring(dllMAIN.LastIndexOf("\\") + 1);

      dllTRG = "SDPReclutamiento_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".DLL";

      string[] libs = (dllMAIN + ";NomadObject.dll;NomadBase.dll;NomadDocument.dll;NomadProxy.dll").Split(';');
      for (lib = 0; lib < libs.Length; lib++)
        libs[lib] = dllPATH + "\\" + libs[lib];

      XmlDocument xmlResult = NCompiler.Compiler.Compile(strMethods, dllPATH, dllTRG, libs);
      if (xmlResult != null) {
        Nomad.NSystem.Proxy.NomadXML X = new Nomad.NSystem.Proxy.NomadXML();
        Nomad.NSystem.Proxy.NomadXML E;
        X.SetText(xmlResult.OuterXml);

        for (E = X.FirstChild().FirstChild().FirstChild(); E != null; E = E.Next()) {
          string conceptoId = "";
          int lineTotal, lineDif = 0;

          lineTotal = int.Parse(E.GetAttr("line"));
          if (!BuscarFormula(strMethods, lineTotal, ref conceptoId, ref lineDif)) {
            NomadEnvironment.GetTrace().Error("Error de Generacion: " + E.GetAttr("desc") + " - " + E.GetAttr("line"));
            NomadEnvironment.GetTraceBatch().Error("Error de Generacion: " + E.GetAttr("desc") + " - " + E.GetAttr("line"));
          } else {
            NomadEnvironment.GetTrace().Error("Error en el Concepto " + conceptoId + " - Linea: " + lineDif.ToString() + " - " + E.GetAttr("desc"));
            NomadEnvironment.GetTraceBatch().Error("Error en el Concepto " + conceptoId + " - Linea: " + lineDif.ToString() + " - " + E.GetAttr("desc"));
          }
        }

        throw new Exception ("No se pueden compilar las fórmulas de los meritos.");
      }

      System.Reflection.Assembly MyLIQCode = System.Reflection.Assembly.LoadFrom(dllPATH + "\\" + dllTRG);
      Type typeClass = MyLIQCode.GetType("NucleusRH.Base.SeleccionDePostulantes.CVs.SDPConceptos");

      objResult = (SDPConceptosBase)typeClass.InvokeMember(null, System.Reflection.BindingFlags.CreateInstance, null, null, null);
      return objResult;

    }

    static bool BuscarFormula(string MyDLLCode, int lineTotal, ref string conceptoId, ref int lineDif) {
      int lineTest;
      string[] lineas = MyDLLCode.Split('\n');

      for (lineTest = lineTotal; lineTest > 0; lineTest--) {
        lineas[lineTest]=lineas[lineTest].Replace("\t"," ");
        lineas[lineTest]=lineas[lineTest].Replace("\r"," ");
        lineas[lineTest]=lineas[lineTest].Replace("\n"," ");
        lineas[lineTest]=lineas[lineTest].Trim();

        if (lineas[lineTest].StartsWith("//ENDCONC:")) return false;

        if (lineas[lineTest].StartsWith("//BEGCONC:")) {
          conceptoId = lineas[lineTest].Substring(10).Trim();
          lineDif = lineTotal - lineTest - 3;
          return true;
        }
      }

      return false;
    }

    //------------------------------------------------------------------------------------------------------
    // Metodos VIRTUAL
    //------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Metodo generico
    /// </summary>
    /// <param name="pobjCV">CV a aplicarle los conceptos</param>
    /// <param name="pbolPostulated">Indica si el CV está o no postulado al aviso</param>
    virtual protected NomadXML ApplyConcepts(CV pobjCV, bool pbolPostulated) {
      throw new Exception("METODO VIRTUAL NO IMPLEMENTADO");
    }

  }

}

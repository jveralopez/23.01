using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Liquidacion.InterfaceEntradaNov;
using NucleusRH.Base.Liquidacion.Personal_Liquidacion;

namespace NucleusRH.Base.Liquidacion_EntNov
{

    class clsEntradaNovedades
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public clsEntradaNovedades()
        {
        }

        /// <summary>
        /// Importa las variables de novedad de personal para una liquidación
        /// </summary>
        /// <param name="pstrOiLiquidacion">OI de la liquidación</param>
        public void ImportarVariablesPersona(string pstrOiLiquidacion, string str_C_Vaviable)
        {

            Hashtable htaVariables;
            Hashtable htaPersonalLiqs = new Hashtable();

            string strOiVarArchivo;
            string strOiPersonalLiq;
            double dblValor;

            ENTRADA objVarArchivo;
            PERSONAL_LIQ objPersonalLiq;
            VAL_VAREN objVarPersona;
            stuVariable objVariable;

            NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION objLiq;

            objLiq = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(pstrOiLiquidacion);
            try
            {

                if (objLiq.c_estado != "I")
                {
                    NomadEnvironment.GetProxy().Batch().Trace.Add("err", "La liquidacion no esta INICIALIZADA", "ImportarVariablesPersona");
                    NomadEnvironment.GetTrace().Error("La liquidacion no esta INICIALIZADA OI= " + pstrOiLiquidacion + " " );

                    return;
                }
            }
            catch (Exception e)
            {
                NomadProxy.GetProxy().Batch().Trace.Add("err", "Se produjo un error al tratar de la liquidacion ", "ImportarVariablesPersona");
                NomadEnvironment.GetTrace().Error("Se produjo un error al tratar de obtener la liquidacion  '" + pstrOiLiquidacion + "'. " + e.Message);

            }

                //Obtiene las variables y las guarda en un hashtable
                htaVariables = this.GetVariables(str_C_Vaviable); //busca datos de la variable y lo guarda en la hash
                // es un solo registro pero lo dejo para reusar el codigo

                //Obtiene la lista de OIs de las variables de archivo
                string strOIsArchivo = NomadProxy.GetProxy().SQLService().Get(ENTRADA.Resources.qry_OiVarArvchivo, "");

                //Recorre el archivo y pide los DDO de interface
                XmlTextReader xtrOIA = new XmlTextReader(strOIsArchivo, System.Xml.XmlNodeType.Document, null);
                xtrOIA.XmlResolver = null; // ignore the DTD
                xtrOIA.WhitespaceHandling = WhitespaceHandling.None;
                xtrOIA.Read();

                int intPorcentaje = 0;

                NomadProxy.GetProxy().Batch().SetProgress(25);
                while (xtrOIA.Read())
                {
                    if (!xtrOIA.IsStartElement())
                        continue;

                    //Setea el avance en el batch
                    intPorcentaje = 50 - ((50 - intPorcentaje) / 2);
                    //NomadProxy.GetProxy().Batch().SetProgress(intPorcentaje);

                    strOiVarArchivo = xtrOIA.GetAttribute("id");
                    objVarArchivo = ENTRADA.Get(strOiVarArchivo);

                    NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Procesando la variable '" + str_C_Vaviable + "' para el legajo '" + objVarArchivo.e_numero_legajo.ToString() + "'.", "ImportarVariablesPersona");

                    objVariable = (stuVariable)htaVariables[str_C_Vaviable];
                    try
                    {
                        //Valida el formato del valor
                        dblValor = ObtenerValor(objVarArchivo, objVariable);
                    }
                    catch (Exception ex)
                    {
                        NomadProxy.GetProxy().Batch().Trace.Add("err", "Se produjo un error al tratar de obtener el valor de la variable '" + str_C_Vaviable + "'. " + ex.Message, "ImportarVariablesPersona");
                        NomadEnvironment.GetTrace().Error("Se produjo un error al tratar de obtener el valor de la variable '" + str_C_Vaviable + "'. " + ex.Message);
                        continue;
                    }

                    //Obtiene el PersonalLiq desde la hash o desde un Get
                    if (htaPersonalLiqs.ContainsKey(objVarArchivo.e_numero_legajo.ToString()))
                    {
                        //Obtiene el PersonalLiq desde la hash
                        objPersonalLiq = (PERSONAL_LIQ)htaPersonalLiqs[objVarArchivo.e_numero_legajo.ToString()];
                    }
                    else
                    {
                        //Obtiene el PersonalLiq desde la DB y lo guarda en la hash
                        strOiPersonalLiq = this.GetOIPersonalLiq(pstrOiLiquidacion, objVarArchivo.e_numero_legajo.ToString());

                        //Valida que el legajo exista en la liquidación
                        if (strOiPersonalLiq == "")
                        {
                            NomadProxy.GetProxy().Batch().Trace.Add("err", "El legajo '" + objVarArchivo.e_numero_legajo + "' no está inicializado en la liquidación.", "ImportarVariablesPersona");
                            NomadEnvironment.GetTrace().Error("El legajo '" + objVarArchivo.e_numero_legajo + "' no está inicializado en la liquidación.");
                            continue;
                        }

                        objPersonalLiq = PERSONAL_LIQ.Get(strOiPersonalLiq);
                        htaPersonalLiqs.Add(objVarArchivo.e_numero_legajo.ToString(), objPersonalLiq);
                    }

                    //Busca la variable dentro de la colección de variables
                    objVarPersona = (VAL_VAREN)objPersonalLiq.VAL_VAREN.GetByAttribute("oi_variable", objVariable.OI);
                    if (objVarPersona == null)
                    {
                        //No existe en la colección, la crea y la agrega
                        objVarPersona = new VAL_VAREN();
                        objVarPersona.oi_variable = objVariable.OI;
                        objVarPersona.n_valor = dblValor;

                        objPersonalLiq.VAL_VAREN.Add(objVarPersona);
                    }
                    else
                    {
                        //La variable ya existe, le actualiza el valor
                        objVarPersona.n_valor = dblValor;
                    }
                }

                xtrOIA.Close();

                NomadProxy.GetProxy().Batch().SetProgress(75);

                intPorcentaje = 0;
                //Recorre los LegajosLiqs y los guarda en la DB
                foreach (string strLegajo in htaPersonalLiqs.Keys)
                {

                    intPorcentaje++;
                    //NomadProxy.GetProxy().Batch().SetProgress(((50 / htaPersonalLiqs.Keys.Count) * intPorcentaje) + 50);

                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save((PERSONAL_LIQ)htaPersonalLiqs[strLegajo]);
                    }
                    catch (Exception ex)
                    {
                        NomadProxy.GetProxy().Batch().Trace.Add("err", "No se pudo actualizar el legajo '" + strLegajo + "'. " + ex.Message, "ImportarVariablesPersona");
                        NomadEnvironment.GetTrace().Error("No se pudo actualizar el legajo '" + strLegajo + "'. " + ex.Message);
                    }

                }
                NomadProxy.GetProxy().Batch().SetProgress(100);
                NomadProxy.GetProxy().Batch().Trace.Add("ifo", "El proceso terminó correctamente.", "ImportarVariablesPersona");

        }

        /// <summary>
        /// Obtiene un hashtable accesible por codigo de varible y retorna el OI.
        /// </summary>
        /// <param name="pstrTipo">Tipo de variable.</param>
        /// <returns></returns>
        private Hashtable GetVariables(string pstrTipo)
        {
            Hashtable htaResult = new Hashtable();
            string strResult;
            string strParametros = "<DATOS C_var=\"" + pstrTipo + "\" />";
            stuVariable objVariable;

            strResult = NomadProxy.GetProxy().SQLService().Get(ENTRADA.Resources.qry_CodigosVariables, strParametros);

            //Recorre las variables y las agrega en la colección
            XmlTextReader xtrLP = new XmlTextReader(strResult, System.Xml.XmlNodeType.Document, null);
            xtrLP.XmlResolver = null; // ignore the DTD
            xtrLP.WhitespaceHandling = WhitespaceHandling.None;
            xtrLP.Read();

            while (xtrLP.Read())
            {
                if (!xtrLP.IsStartElement())
                    continue;

                objVariable = new stuVariable();
                objVariable.OI = xtrLP.GetAttribute("oi_variable");
                objVariable.Tipo = xtrLP.GetAttribute("c_tipo_dato").ToUpper();

                htaResult.Add(pstrTipo, objVariable);
            }

            xtrLP.Close();

            return htaResult;
        }

        /// <summary>
        /// Obtiene un hashtable accesible por codigo de varible y retorna el OI.
        /// </summary>
        /// <param name="pstrTipo">Tipo de variable.</param>
        /// <returns></returns>
        private string GetOIPersonalLiq(string pstrOiLiquidacion, string pstrLegajo)
        {
            string strRows;
            string strResult = "";
            string strParametros = "<DATOS oi_liquidacion=\"" + pstrOiLiquidacion + "\" e_numero_legajo=\"" + pstrLegajo + "\" />";

            strRows = NomadProxy.GetProxy().SQLService().Get(ENTRADA.Resources.qry_PersonalLiq, strParametros);

            XmlDocument objXML = new XmlDocument();
            objXML.LoadXml(strRows);

            if (objXML.DocumentElement.HasAttribute("oi_personal_liq"))
                strResult = objXML.DocumentElement.GetAttribute("oi_personal_liq");

            return strResult;
        }

        /// <summary>
        /// Retorna el valor de tipo double
        /// </summary>
        /// <param name="objVarArchivo"></param>
        /// <param name="objVariable"></param>
        /// <returns></returns>
        private double ObtenerValor(ENTRADA pobjVarArchivo, stuVariable pobjVariable)
        {
            double dblResultado = 0d;

            try
            {
                switch (pobjVariable.Tipo)
                {
                    case "DATETIME":
                        string[] strDates;
                        DateTime dteFecha;
                        string strCompara;
                        strDates = pobjVarArchivo.n_valor.Split('/');

                        if (strDates.Length != 3)
                            throw new Exception();

                        dteFecha = new DateTime(int.Parse(strDates[2]), int.Parse(strDates[1]), int.Parse(strDates[0]));

                        if (pobjVarArchivo.n_valor.Length == 10)
                            strCompara = dteFecha.ToString("dd/MM/yyyy");
                        else
                            strCompara = dteFecha.ToString("dd/MM/yy");

                        if (strCompara != pobjVarArchivo.n_valor)
                            throw new Exception();

                        strCompara = dteFecha.ToString("dd/MM/yyyy");
                        strDates = strCompara.Split('/');
                        dblResultado = double.Parse(strDates[2] + strDates[1] + strDates[0]);
                        break;

                    case "DOUBLE":
                        dblResultado = StringUtil.str2dbl(pobjVarArchivo.n_valor.Replace(",","."));
                        break;

                    case "BOOL":
                        if (pobjVarArchivo.n_valor == "1" || pobjVarArchivo.n_valor.ToLower() == "s" || pobjVarArchivo.n_valor.ToLower() == "t")
                            dblResultado = 1d;
                        else
                            if (pobjVarArchivo.n_valor == "0" || pobjVarArchivo.n_valor.ToLower() == "f")
                                dblResultado = 0d;
                            else
                                throw new Exception();

                        break;

                    case "INT":
                        int intValor;
                        intValor = int.Parse(pobjVarArchivo.n_valor);

                        //if (intValor.ToString() != pobjVarArchivo.n_valor)
                        //throw new Exception();

                        dblResultado = Convert.ToDouble(intValor);
                        break;

                }
            }
            catch
            {
                switch (pobjVariable.Tipo)
                {
                    case "DATETIME": throw new Exception("El valor '" + pobjVarArchivo.n_valor + "' tiene un formato de fecha incorrecto Debe ser dd/mm/aaaa o dd/mm/aa.");
                    case "DOUBLE": throw new Exception("El valor '" + pobjVarArchivo.n_valor + "' tiene un formato de número decimal incorrecto. Debe usarse . (punto) como separador de decimales.");
                    case "BOOL": throw new Exception("El valor '" + pobjVarArchivo.n_valor + "' tiene un formato de lógico incorrecto. Debe usarse 1 S o T para los verdaderos y 0 F para los falsos.");
                    case "INT": throw new Exception("El valor '" + pobjVarArchivo.n_valor + "' tiene un formato de número entero incorrecto.");
                }
            }

            return dblResultado;
        }

        /// <summary>
        /// Estructura con datos de la variable
        /// </summary>
        private struct stuVariable
        {
            public string OI;
            public string Tipo;
        }

    }
}



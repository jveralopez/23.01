using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Liquidacion.Legajo_Liquidacion ;
using NucleusRH.Base.Liquidacion.Variables ;
using NucleusRH.Base.Liquidacion.Personal_Liquidacion;

namespace NucleusRH.Base.Liquidacion.Liquidacion_Nov_Var
{

    class clsEntradaNovedades
    {

        public clsEntradaNovedades()
        {
        }

        // Formato xml esperado********************
                    ////<DATOS oi_variable =""1404"" oi_liquidacion=""63"">
                    ////    <PERSONAS nmd-col=""1"">
                    ////        <PER oi_personal_liq=""301"" n_valor=""5.00"" />
                    ////        <PER oi_personal_liq=""335"" n_valor=""6.00"" />
                    ////        <PER oi_personal_liq=""336"" n_valor=""1.00"" />
                    ////    </PERSONAS>
                    ////    </DATOS>

         public void ImportarVariablesPersona(Nomad.NSystem.Document.NmdXmlDocument xmlDoc)
        {
            System.Xml.XmlDocument xmlDocPar;

            xmlDocPar = new System.Xml.XmlDocument();

            xmlDocPar.LoadXml(xmlDoc.ToString() );

            string m_Personal_liq;
            string m_oi_variable;
            string m_oi_liquidacion;
            string strParam ;
            m_oi_variable ="";
            m_oi_liquidacion ="";
            double dblValor;
            XmlDocument objXML ;

            NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION objLiq;
            try
            {
                foreach (System.Xml.XmlElement xmlObj in xmlDocPar)
                {
                    if (xmlObj.Attributes.GetNamedItem("oi_variable") != null)
                    {
                        m_oi_variable = xmlObj.Attributes["oi_variable"].Value;
                        m_oi_liquidacion = xmlObj.Attributes["oi_liquidacion"].Value;
                    }

                }

                objLiq = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(m_oi_liquidacion);
                // VERIFICA EL ESTADO DE LA LIQUIDACION
                if (objLiq.c_estado != "I")
                {
                    NomadProxy.GetProxy().Batch().Trace.Add("err", "La liquidacion no esta INICIALIZADA", "ImportarVariablesPersona");
                    NomadEnvironment.GetTrace().Error("La liquidacion no esta INICIALIZADA OI= " + m_oi_liquidacion + " ");

                    return;
                }

                //Obtiene el tipo de dato de la variables
                string s_tipo_variable = this.GetVariables(m_oi_variable); //busca datos de la variable y lo guarda en la hash
                foreach (System.Xml.XmlElement xmlObj in xmlDocPar.DocumentElement.ChildNodes.Item(0).ChildNodes)
                {
                    NomadEnvironment.GetTrace().Error("oi_personal_liq: " + xmlObj.Attributes["oi_personal_liq"].Value);
                    if (xmlObj.Attributes.GetNamedItem("oi_personal_liq") != null)
                    {
                        //VERIFICA QUE LAS PERSONAS ESTEN ASOCIADAS A LA LIQUIDACION
                        strParam = @"<DATOS oi_liquidacion=""" + m_oi_liquidacion + @""" OI_PERSONAL_LIQ=""" + xmlObj.Attributes["oi_personal_liq"].Value + @""" />";
                        m_Personal_liq = NomadProxy.GetProxy().SQLService().Get(PERSONAL_EMP.Resources.qry_VerifPersonalLiq, strParam);
                        objXML = new XmlDocument();
                        objXML.LoadXml(m_Personal_liq);
                        if (objXML.DocumentElement.HasAttribute("oi_personal_liq"))
                            m_Personal_liq = objXML.DocumentElement.GetAttribute("oi_personal_liq");
                        else
                        {
                            NomadEnvironment.GetTrace().Error("No existe el PersonalLiq :'" + xmlObj.Attributes["oi_personal_liq"].Value + "' para la liquidacion :'" + m_oi_liquidacion + "'. " );
                            continue;
                        }
                        //Valida el formato del valor de la variable si es del tipo que la varible
                        dblValor = this.ObtenerValor(xmlObj.Attributes["n_valor"].Value, s_tipo_variable);
                        // guarda la persona asociada a la liquidacion
                        SavePersonalLiq(m_Personal_liq, m_oi_liquidacion, m_oi_variable, dblValor);

                    }
                }

            }
            catch (Exception e)
            {
                NomadProxy.GetProxy().Batch().Trace.Add("err", "Se produjo un error al tratar de la liquidacion ", "ImportarVariablesPersona");
                NomadEnvironment.GetTrace().Error("Se produjo un error al tratar de obtener la liquidacion  '" + m_oi_liquidacion + "'. " + e.Message);

            }

            NomadProxy.GetProxy().Batch().SetProgress(100);
            NomadProxy.GetProxy().Batch().Trace.Add("ifo", "El proceso terminó correctamente.", "ImportarVariablesPersona");

        }

        //**************************************************************
        ////Busca el tipo de dato asociado a la variable
        ////utiliza un recurso almacenado en la clase de Liquidacion.Variables
        //**************************************************************

        private string GetVariables(string pstrTipo)
        {
            string strResult;
            string strParametros = "<DATOS oi_var=\"" + pstrTipo + "\" />";
            strResult = NomadProxy.GetProxy().SQLService().Get(VARIABLE.Resources.qry_CodigosVariables, strParametros);

            //Recorre las variables y las agrega en la colección
            XmlTextReader xtrLP = new XmlTextReader(strResult, System.Xml.XmlNodeType.Document, null);
            xtrLP.XmlResolver = null; // ignore the DTD
            xtrLP.WhitespaceHandling = WhitespaceHandling.None;
            xtrLP.Read();
            string rta;
            rta = "";
            while (xtrLP.Read())
            {
                if (!xtrLP.IsStartElement())
                    continue;
                rta = xtrLP.GetAttribute("c_tipo_dato").ToUpper();
            }
            xtrLP.Close();

            return rta;
        }

        //**************************************************************
        ////Si existe el personalLiq lo actualiza
        ////Si no ,lo crea
        //**************************************************************

        private void SavePersonalLiq(string m_Personal_liq, string m_oi_liquidacion, string m_oi_variable, double m_dblValor)
        {
            VAL_VAREN objVarPersona;
            PERSONAL_LIQ objPersonalLiq;

             //obtiene el personalLiq
            objPersonalLiq = PERSONAL_LIQ.Get(m_Personal_liq);

            //Busca la variable dentro de la colección de variables
             objVarPersona = (VAL_VAREN)objPersonalLiq.VAL_VAREN.GetByAttribute("oi_variable", m_oi_variable);

            if (objVarPersona == null)
             {
                 //No existe en la colección, la crea y la agrega
                 objVarPersona = new VAL_VAREN();
                 objVarPersona.oi_variable = m_oi_variable;
                 objVarPersona.n_valor = m_dblValor;

                 objPersonalLiq.VAL_VAREN.Add(objVarPersona);
             }
             else
             {
                 //La variable ya existe, le actualiza el valor
                 objVarPersona.n_valor = m_dblValor;
             }

             try
             {
                 NomadEnvironment.GetCurrentTransaction().Save((PERSONAL_LIQ)objPersonalLiq);
             }
             catch (Exception ex)
             {
                 NomadProxy.GetProxy().Batch().Trace.Add("err", "No se actualizaron algunos legajos " , "ImportarVariablesPersona");
                 NomadEnvironment.GetTrace().Error("No se pudo actualizar el Personal_liq '" + m_Personal_liq + "'. Mensaje " + ex.Message);
             }

        }

        //**************************************************************
        ////devuelve el valor por el tipo de variable
        ////
        //**************************************************************
        private double ObtenerValor(string m_Valor, string pobjVariable)
        {
            double dblResultado = 0d;

            try
            {
                NomadEnvironment.GetTrace().Error("pobjVariable: " + pobjVariable.ToString());
                switch (pobjVariable)
                {
                    case "DATETIME":
                        string[] strDates;
                        DateTime dteFecha;
                        string strCompara;
                        strDates = m_Valor.Split('/');

                        if (strDates.Length != 3)
                            throw new Exception();

                        dteFecha = new DateTime(int.Parse(strDates[2]), int.Parse(strDates[1]), int.Parse(strDates[0]));

                        if (m_Valor.Length == 10)
                            strCompara = dteFecha.ToString("dd/MM/yyyy");
                        else
                            strCompara = dteFecha.ToString("dd/MM/yy");

                        if (strCompara != m_Valor)
                            throw new Exception();

                        strCompara = dteFecha.ToString("dd/MM/yyyy");
                        strDates = strCompara.Split('/');
                        dblResultado = double.Parse(strDates[2] + strDates[1] + strDates[0]);
                        break;

                    case "DOUBLE":
                        if (m_Valor.IndexOf(",") >= 0)
                            throw new Exception();

                        dblResultado = StringUtil.str2dbl(m_Valor);
                        break;

                    case "BOOL":
                        if (m_Valor == "1" || m_Valor.ToLower() == "s" || m_Valor.ToLower() == "t")
                            dblResultado = 1d;
                        else
                            if (m_Valor == "0" || m_Valor.ToLower() == "f")
                                dblResultado = 0d;
                            else
                                throw new Exception();

                        break;

                    case "INT":
                        int intValor;
                        intValor = int.Parse(m_Valor);

                        //if (intValor.ToString() != m_Valor.n_valor)
                        //throw new Exception();

                        dblResultado = Convert.ToDouble(intValor);
                        break;

                }
            }
            catch
            {
                switch (pobjVariable)
                {
                    case "DATETIME": throw new Exception("El valor '" + m_Valor + "' tiene un formato de fecha incorrecto Debe ser dd/mm/aaaa o dd/mm/aa.");
                    case "DOUBLE": throw new Exception("El valor '" + m_Valor + "' tiene un formato de número decimal incorrecto. Debe usarse . (punto) como separador de decimales.");
                    case "BOOL": throw new Exception("El valor '" + m_Valor + "' tiene un formato de lógico incorrecto. Debe usarse 1 S o T para los verdaderos y 0 F para los falsos.");
                    case "INT": throw new Exception("El valor '" + m_Valor + "' tiene un formato de número entero incorrecto.");
                }
            }

            return dblResultado;
        }

    }
}



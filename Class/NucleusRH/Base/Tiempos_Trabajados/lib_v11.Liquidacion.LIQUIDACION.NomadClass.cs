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

namespace NucleusRH.Base.Tiempos_Trabajados.Liquidacion
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Procesamientos de Horas
    public partial class LIQUIDACION : Nomad.NSystem.Base.NomadObject
    {

        public static bool LiquidacionOK(string pOI_Liquidacion)
        {
            NomadEnvironment.GetTrace().Info("**** Comienza el método LiquidacionOk ****");

            Nomad.NSystem.Base.NomadAppException nmdAppE;

            //Obtiene el query a ejecutar desde los recursos
            string strQuery = NucleusRH.Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION.Resources.QRY_PERSONAL_EN_LIQ;

            //Genera el XML de parametros
            string strParametros = "<DATO oi_liquidacion=\"" + pOI_Liquidacion + "\"/>";

            //Ejecuta el query
            SQLService sqlService = NomadProxy.GetProxy().SQLService();
            string strResultado = sqlService.Get(strQuery, strParametros);

            NomadEnvironment.GetTrace().Info("strResultado: " + strResultado);

            if (strResultado.EndsWith("/>"))
            {
                nmdAppE = new Nomad.NSystem.Base.NomadAppException("No se encontraron personas asociadas a la liquidación.");
                throw nmdAppE;
            }

            XmlDocument xmlResultado = new XmlDocument();
            xmlResultado.LoadXml(strResultado);

            //Blanquea la variable y la utiliza para concatenar los errores
            strResultado = "";

            //Recorre las personas de la liquidación y valida su estado
            foreach (XmlNode xnoPersona in xmlResultado.DocumentElement.ChildNodes)
            {
                NomadEnvironment.GetTrace().Info("Persona: " + ((XmlElement)xnoPersona).GetAttribute("oi_personal_emp"));
                try
                {
                    NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.LiquidacionOK(((XmlElement)xnoPersona).GetAttribute("oi_personal_emp"), pOI_Liquidacion);
                }
                catch (NomadAppException nex)
                {
                    strResultado = strResultado + " " + nex.Message;
                }
            }

            //Realiza las validaciones--------------------------------------------

            if (strResultado.Length > 0)
            {
                nmdAppE = new Nomad.NSystem.Base.NomadAppException("Existen los siguientes problemas: " + strResultado);
                throw nmdAppE;
            }

            return true;
        }
        /// <summary>
        /// Cierra el procesamiento
        /// </summary>
        public void CerrarLiquidacion()
        {
            NomadBatch objBatch;

            objBatch = NomadBatch.GetBatch("Cerrar Procesamiento", "");
            objBatch.Log("Iniciando cierre de Procesamiento: " + this.c_liquidacion + "-" + this.d_liquidacion);
            objBatch.SetMess("Cerrando el procesamiento.");

            NomadXML nxmDoc;
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP objPPerTTA;
            NucleusRH.Base.Tiempos_Trabajados.Procesos objProcesos;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING objFichada;

            double dblHorasFc;
            int intMinutosFc;
            string strParams;
            string strLegajoHASH;
            NomadXML ndxFichadas;

            //---------------------------------------------------------------------------------
            //Se obtienen los parámetros para el cierre de la liquidación
            //Son los parámetros necesarios para el cálculo de los Franco/Compensatorios

            objBatch.Log("Obteniendo los parámetros.");
            objBatch.SetPro(5);
            strParams = "<DATOS c_modulo=\"TTA\" d_clase=\"\\'PERSONAL\\'\" />";
            nxmDoc = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Resources.QRY_Parametros, strParams);

            int intFCAcumulado = 0;
            int intFCAdeudado;
            string strFCAdeudado = "NO";
            string strTemporalValue;

            for (NomadXML nxmParametro = nxmDoc.FirstChild().FirstChild(); nxmParametro != null; nxmParametro = nxmParametro.Next())
            {
                strTemporalValue = nxmParametro.GetAttr("d_valor") == "" ? "0" : nxmParametro.GetAttr("d_valor");

                switch (nxmParametro.GetAttr("c_parametro"))
                {
                    case "MaxFCAcumulado":
                        intFCAcumulado = int.Parse(strTemporalValue);
                        break;

                    case "MaxFCAdeudado":
                        strFCAdeudado = strTemporalValue;
                        break;
                }
            }

            NomadBatch.Trace("Los parámetros para el Procesamiento son:");
            NomadBatch.Trace("MaxFCAcumulado: " + intFCAcumulado.ToString());
            NomadBatch.Trace("MaxFCAdeudado: " + strFCAdeudado);

            //---------------------------------------------------------------------------------

            objBatch.Log("Recuperando los legajos.");
            objBatch.SetPro(9);

            //Se recuperan todas las personas inicialidas en la liquidacion
            strParams = "<DATO oi_liquidacion=\"" + this.Id + "\"/>";
            nxmDoc = NomadEnvironment.QueryNomadXML(LIQUIDACION.Resources.QRY_PERSONAL_EN_LIQ, strParams);

            objProcesos = new NucleusRH.Base.Tiempos_Trabajados.Procesos();
            DateTime dteProDesde = this.f_fechainicio;
            DateTime dteProHasta = this.f_fechafin.AddDays(1);
            string str_f_des = Nomad.NSystem.Functions.StringUtil.date2str(this.f_fechainicio);
            string str_f_has = Nomad.NSystem.Functions.StringUtil.date2str(this.f_fechafin.AddDays(1));

            bool blnErrorTotal = false;
            bool blnErrorPersona;
            bool blnGrabarPer;

            int cont = 0;

            //Realiza las validaciones segun el evento
            if (!LIQUIDACION.eveValidarCerrar(this))
            {
                throw new NomadAppException("La Liquidacion no puede ser cerrada debido a que existen personas asociadas a la misma con registros negativos de Francos Compensatorios");
            }

            //Inicia la Transacción
            NomadTransaction objTransaction = NomadEnvironment.GetCurrentTransaction();
            objTransaction.Begin();

            //Recorre cada uno de los legajos
            int cant = nxmDoc.FirstChild().ChildLength;
            int x = 0;
            for (NomadXML nxmLegajo = nxmDoc.FirstChild().FirstChild(); nxmLegajo != null; nxmLegajo = nxmLegajo.Next())
            {

                objPPerTTA = null;
                blnErrorPersona = false;
                blnGrabarPer = false;

                try
                {
                    x++;
                    NomadBatch.Trace(".");
                    NomadBatch.Trace(".");
                    NomadBatch.Trace("Validando datos del Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "'.");
                    objBatch.Log("Validando datos del Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "'.");
                    objBatch.SetPro(10, 80, cant, x);

                    ndxFichadas = new NomadXML();

                    //-----------------------------------
                    //Valida que no haya cambiado el HASH para el legajo.
                    //Pasa por referencia un nomadXML que regresará con la colección de fichadas utilizadas por el legajo en la liquidación.
                    NomadBatch.Trace("Valida que no haya cambiado el HASH para el legajo.");
                    strLegajoHASH = objProcesos.GetHASHProcesamientoLegajo(this.Id, nxmLegajo.GetAttr("oi_personal_emp"), ref ndxFichadas);

                    //Compara los HASH
                    if (strLegajoHASH != nxmLegajo.GetAttr("h_hash"))
                    {
                        objBatch.Err("No se puede cerrar el Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "' debido que han cambiado datos relativos a la persona después del ultimo Procesamiento de horas.");
                        blnErrorTotal = true;
                        blnErrorPersona = true;
                    }
                    //-----------------------------------

                    //Recorre las fichadas y valida que no estén siendo utilizadas por otra liquidación
                    NomadBatch.Trace("Recorre las fichadas y valida que no estén siendo utilizadas por otra liquidación.");
                    if (!blnErrorPersona)
                    {
                        for (NomadXML nxmFichada = ndxFichadas.FirstChild().FirstChild(); nxmFichada != null; nxmFichada = nxmFichada.Next())
                        {

                            //Valida que la fichada no esté siendo utilizada en otra liquidación
                            if (nxmFichada.GetAttr("oi_liquidacion") != "")
                            {
                                objBatch.Err("No se puede cerrar el Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "' debido que tiene fichadas que están siendo utilizadas en otro Procesamiento ya CERRADO.");
                                NomadBatch.Trace("No se puede cerrar el Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "' debido que tiene fichadas que están siendo utilizadas en otro Procesamiento ya CERRADO.");
                                blnErrorTotal = true;
                                blnErrorPersona = true;
                                break;
                            }

                            objFichada = (NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING)NomadEnvironment.GetObject(typeof(NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING), nxmFichada.GetAttr("oi_fichadasing"));
                            objFichada.oi_liquidacion = this.Id;

                            //Le graba el OI_LIQUIDACION a la fichada
                            if (!blnErrorTotal)
                                objTransaction.Save(objFichada);
                        }
                    }

                    //Valida que la cantidad de horas posibles no esté por fuera de los par?metros
                    if (!blnErrorPersona)
                    {
                        NomadBatch.Trace("Valida que la cantidad de horas posibles no esté por fuera de los parámetros");
                        objPPerTTA = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(nxmLegajo.GetAttr("oi_personal_emp"));

                        if (nxmLegajo.GetAttr("n_horas_fc") != "")
                            dblHorasFc = StringUtil.str2dbl(nxmLegajo.GetAttr("n_horas_fc"));
                        else
                            dblHorasFc = 0;

                        intMinutosFc = Convert.ToInt32((dblHorasFc * 60) + (objPPerTTA.n_horas_fc * 60));

                        NomadBatch.Trace("Minutos de FC anterior: " + ((double)(objPPerTTA.n_horas_fc * 60)).ToString() + " y lo nuevo: " + ((double)(dblHorasFc * 60)).ToString());

                        //Compara contra la cantidad m?xima de FC acumulado
                        if (intMinutosFc > intFCAcumulado)
                        {
                            objBatch.Err("No se puede cerrar el Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "' dado que el la cantidad de minutos franco compensatorios ACUMULADOS (" + intMinutosFc.ToString() + ") supera al máximo posible (" + intFCAcumulado.ToString() + ").");
                            blnErrorPersona = true;
                        }

                        //Compara contra la cantidad m?xima de FC adeudado
                        NomadBatch.Trace("Compara contra la cantidad máxima de FC adeudado!!!!!!!!!!!!!!");
                        if (strFCAdeudado.ToUpper() != "NA" && strFCAdeudado.ToUpper() != "NO")
                        {
                            try
                            {
                                intFCAdeudado = int.Parse(strFCAdeudado);
                            }
                            catch
                            {
                                intFCAdeudado = 0;
                            }
                            if (intMinutosFc > intFCAdeudado)
                            {
                                objBatch.Err("No se puede cerrar el Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "' dado que la cantidad de horas franco compensatorias ADEUDADAS (" + intMinutosFc.ToString() + ") supera al máximo posible (" + intFCAdeudado.ToString() + ").");
                                blnErrorPersona = true;
                            }
                            objPPerTTA.n_horas_fc = (double)intMinutosFc / 60;
                            blnGrabarPer = true;
                        }
                    }

                    //Actualiza Acumuladores
                    if (!blnErrorPersona)
                    {
                        NomadBatch.Trace("Actualiza los Acumuladores....");
                        string strParamAcu = "<DATOS oi_personal_emp=\"" + objPPerTTA.Id + "\" oi_liquidacion=\"" + this.Id + "\"/>";
                        NomadXML nxmDocAcu = NomadEnvironment.QueryNomadXML(LIQUIDACION.Resources.QRY_ACUM_PER, strParamAcu);
                        ArrayList myDelList = new ArrayList();

                        foreach (Personal.VAL_VAR objValVAR in objPPerTTA.VAL_VAR)
                        {
                            Variables.VARIABLE objVAR = Variables.VARIABLE.Get(objValVAR.oi_variable);
                            if (objVAR.c_tipo_variable != "PERACC") continue;

                            NomadXML nxmVar = nxmDocAcu.FirstChild().FindElement2("ROW", "oi_variable", objValVAR.oi_variable);
                            if (nxmVar == null) myDelList.Add(objValVAR);
                        }

                        foreach (Personal.VAL_VAR objValVARDEL in myDelList)
                        {
                            NomadBatch.Trace("ELIMINA -- " + objValVARDEL.SerializeAll());
                            objPPerTTA.VAL_VAR.Remove(objValVARDEL);
                            blnGrabarPer = true;
                        }

                        for (NomadXML nxmVar = nxmDocAcu.FirstChild().FirstChild(); nxmVar != null; nxmVar = nxmVar.Next())
                        {
                            NomadBatch.Trace(nxmVar.ToString());

                            //Revisa si la persona tiene cargado el acumular
                            Personal.VAL_VAR objValVAR;
                            objValVAR = (Personal.VAL_VAR)objPPerTTA.VAL_VAR.GetByAttribute("oi_variable", nxmVar.GetAttr("oi_variable"));
                            if (objValVAR == null)
                            {
                                objValVAR = new Personal.VAL_VAR();
                                objValVAR.oi_variable = nxmVar.GetAttr("oi_variable");
                                objValVAR.n_valor = nxmVar.GetAttrDouble("n_valor");
                                NomadBatch.Trace("AGREGA-- " + objValVAR.SerializeAll());
                                objPPerTTA.VAL_VAR.Add(objValVAR);
                                blnGrabarPer = true;
                            }
                            else
                                if (objValVAR.n_valor != nxmVar.GetAttrDouble("n_valor"))
                                {
                                    NomadBatch.Trace("MODIFICA -- " + objValVAR.SerializeAll());
                                    objValVAR.n_valor = nxmVar.GetAttrDouble("n_valor");
                                    blnGrabarPer = true;
                                }
                        }
                    }

                    //Graba la persona en TTA
                    if (blnGrabarPer && !blnErrorTotal && !blnErrorTotal)
                        objTransaction.Save(objPPerTTA);

                    if (blnErrorPersona)
                    {
                        cont++;
                        blnErrorTotal = true;
                    }
                    else
                    {
                        objBatch.Log("OK");
                    }

                }
                catch (Exception e)
                {
                    objBatch.Err("Error verificando datos del Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "' - " + e.Message);
                    NomadBatch.Trace("Error verificando datos del Legajo '" + nxmLegajo.GetAttr("e_numero_legajo") + "' - " + e.Message);
                    cont++;
                    blnErrorTotal = true;
                }
            }

            //Se comenta esta seccion a partir de la version 11.08
            //Cierra los horarios utilizados en la liquidación y que están abiertos
            /*if (!blnErrorTotal) {
                string strHorario = "";
                NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO objHorario;
                strParams = "<DATOS oi_liquidacion=\"" + this.Id + "\" estado=\"P\"/>";
                nxmDoc = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.Resources.QRY_Horarios_Pers, strParams);

                try {
                    //Recorre los horarios abiertos y los cierra
                    for (NomadXML nxmHorario = nxmDoc.FirstChild().FirstChild(); nxmHorario != null; nxmHorario = nxmHorario.Next()) {
                        objHorario = (NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO) NomadEnvironment.GetObject(typeof(NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO), nxmHorario.GetAttr("oi_horario"));
                        strHorario = objHorario.c_horario + " - " + objHorario.d_horario;
                        objHorario.c_estado = "C";
                        objTransaction.Save(objHorario);
                    }

                } catch (Exception ex) {
                objBatch.Err("Se produjo un error al cerrar el horario '" + strHorario + "'" + ex.Message);
                NomadBatch.Trace("Se produjo un error al cerrar el horario '" + strHorario + "'" + ex.Message);
                cont++;
                blnErrorTotal = true;
                }

            }*/

            NomadBatch.Trace("Grabando los datos.");
            objBatch.Log("Grabando los datos.");

            //Si se no produjeron incosistencias en las validaciones se pone el PROCESAMIENTO como CERRADO
            if (blnErrorTotal)
            {
                objBatch.Err("No puede Cerrar el Procesamiento dado que se encontraron " + cont.ToString() + " Legajos con problemas. Por favor re-procese el Procesamiento o revise los Legajos en conflicto.");
                NomadBatch.Trace("No puede Cerrar el Procesamiento dado que se encontraron " + cont.ToString() + " Legajos con problemas. Por favor re-procese el Procesamiento o revise los Legajos en conflicto.");

                //Guarda la TRANSACCION
                objTransaction.Rollback();
            }
            else
            {
                this.c_estado = "cer";
                objTransaction.Save(this);
                objBatch.Log("Procesamiento Cerrado.");
                NomadBatch.Trace("Procesamiento Cerrado.");

                //Guarda la TRANSACCION
                objTransaction.Commit();
            }

            objBatch.Log("Proceso finalizado.");
            objBatch.SetPro(100);
            NomadBatch.Trace("Proceso finalizado.");
        }
		
		/// <summary>
        /// Congelar el procesamiento
        /// </summary>
        public void CongelarLiquidacion()
        {    

			NomadBatch objBatch;

            objBatch = NomadBatch.GetBatch("Congelar Procesamiento", "");
            objBatch.Log("Iniciando congelamiento de Procesamiento: " + this.c_liquidacion + "-" + this.d_liquidacion);
            
			if(this.c_estado == "pro")
			{			
				//Inicia la Transacción
				NomadTransaction objTransaction = NomadEnvironment.GetCurrentTransaction();
				objTransaction.Begin();
				
				this.c_estado = "con";
                objTransaction.Save(this);
                
				objBatch.Log("Procesamiento Congelado.");
                
                //Guarda la TRANSACCION
                objTransaction.Commit();       

				objBatch.Log("Proceso finalizado.");			
			}						
            
		}
		
		/// <summary>
        /// Descongelar el procesamiento
        /// </summary>
        public void DescongelarLiquidacion()
        {    

			NomadBatch objBatch;

            objBatch = NomadBatch.GetBatch("Descongelar Procesamiento", "");
            objBatch.Log("Iniciando descongelamiento del Procesamiento: " + this.c_liquidacion + "-" + this.d_liquidacion);
            
			if(this.c_estado == "con")
			{			
				//Inicia la Transacción
				NomadTransaction objTransaction = NomadEnvironment.GetCurrentTransaction();
				objTransaction.Begin();
				
				this.c_estado = "pro";
                objTransaction.Save(this);
                
				objBatch.Log("Procesamiento Descongelado.");
                
                //Guarda la TRANSACCION
                objTransaction.Commit();       

				objBatch.Log("Proceso finalizado.");			
			}						
            
		}
    }
}


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Nomad.NSystem.Functions;
using Nomad.NSystem.Base;
using NucleusRH.Base.Evaluacion.Evaluacion;

namespace NucleusRH.Base.Evaluacion.Evaluacion {

    class Matias : NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION {

        public Matias(string pstrXmlDoc) : base(pstrXmlDoc) {
        }

        ///partial(NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION){

        /// <summary>
        /// Calcula los resultados de la evalacuación
        /// </summary>
        public string Calcular() {

            //Declaracion de las principales variables y objetos
            NucleusRH.Base.Organizacion.Escalas.VAL_ESCALA objValEscala;
            NucleusRH.Base.Evaluacion.Eventos.EVENTO objEvento;
            XmlDocument xmlEscalas;
            double dblCompetencias = 0d;
            double dblAptitudes = 0d;
            double dblObjetivos = 0d;
            double dblSubO = 0d;
            string strResult;
            string[] strData;

            {
                //Obtiene las escalas utilizadas por el EVENTO en un documento XML
                string strQryResult;
                string strEscalas = "0";

                objEvento = this.Getoi_evento();

                if (!objEvento.oi_escala_resNull) strEscalas = strEscalas + "," + objEvento.oi_escala_res;
                if (!objEvento.oi_escala_compNull) strEscalas = strEscalas + "," + objEvento.oi_escala_comp;
                if (!objEvento.oi_escala_objNull) strEscalas = strEscalas + "," + objEvento.oi_escala_obj;

                strQryResult = Nomad.NSystem.Proxy.NomadProxy.GetProxy().SQLService().Get(EVALUACION.Resources.qryEscalas, "<DATA escalas=\"" + strEscalas + "\" />");
                xmlEscalas = new XmlDocument();
                xmlEscalas.LoadXml(strQryResult);
            }

            //--------------------------------------------------------------------------------------
            //Recorre las COMPENTENCIAS
            double pond_com = 0;
            foreach (COM_EVALU objCompetencia in this.COM_EVALU) {
              pond_com += objCompetencia.n_ponderacion;
            }
            foreach (COM_EVALU objCompetencia in this.COM_EVALU) {

                dblAptitudes = 0d;

                //Recorre las APTITUDES
                double pond_apt = 0;
                foreach (APT_COM objAptitud in objCompetencia.APT_COM) {
                  pond_apt += objAptitud.n_ponderacion;
                }
                foreach (APT_COM objAptitud in objCompetencia.APT_COM) {
                  if (!objAptitud.oi_val_escalaNull) {
                    objValEscala = objAptitud.Getoi_val_escala();
                     objAptitud.n_resultado = objValEscala.n_val_hasta;
                  } else {
                    objAptitud.n_resultado = 0;
                  }
                    //Acumula los valores de las Aptitudes
                    dblAptitudes = dblAptitudes + (objAptitud.n_resultado * objAptitud.n_ponderacion / pond_apt);
                }

                objCompetencia.n_resultado = dblAptitudes;

                //Asigna los valores cualitativos
                strData = this.GetValue(xmlEscalas, objEvento.oi_escala_comp, objCompetencia.n_resultado);
                objCompetencia.c_nivel_resultado = strData[0];

                dblCompetencias = dblCompetencias + (objCompetencia.n_resultado * objCompetencia.n_ponderacion / pond_com);
            }

            //Completa los valores para las COMPENTENCIAS de la EVALUACIÓN
            this.n_res_comp = dblCompetencias;
            strData = this.GetValue(xmlEscalas, objEvento.oi_escala_comp, this.n_res_comp);
            this.c_res_comp = strData[0];
            this.l_aprobado_comp = strData[1] == "1";

            //--------------------------------------------------------------------------------------
            //Recorre los OBJETIVOS
            double pond_obj = 0;
            foreach (OBJ_EVALU objObjetivo in this.OBJ_EVALU) {
              pond_obj += objObjetivo.n_ponderacion;
            }
            foreach (OBJ_EVALU objObjetivo in this.OBJ_EVALU) {
                dblSubO = 0d;

                if (objObjetivo.SUBOBJETIVOS.Count > 0)
                {
                  //Recorre los SUBOBJETIVOS
                  double pond_sub = 0;
                  foreach (SUBOBJETIVO objSubO in objObjetivo.SUBOBJETIVOS) {
                    pond_sub += objSubO.n_impacto;
                  }
                  foreach (SUBOBJETIVO objSubO in objObjetivo.SUBOBJETIVOS) {
                    dblSubO = dblSubO + (objSubO.n_cumplimiento * objSubO.n_impacto / pond_sub);
                  }

                  objObjetivo.n_cumplimiento = dblSubO;
                }
                //Asigna los valores cualitativos
                strData = this.GetValue(xmlEscalas, objEvento.oi_escala_obj, objObjetivo.n_cumplimiento);
                objObjetivo.c_nivel_resultado = strData[0];

                dblObjetivos = dblObjetivos + (objObjetivo.n_cumplimiento * objObjetivo.n_ponderacion / pond_obj);
            }

            //Completa los valores para los OBJETIVOS de la EVALUACIÓN
            this.n_res_objetivos = dblObjetivos;
            strData = this.GetValue(xmlEscalas, objEvento.oi_escala_obj, this.n_res_objetivos);
            this.c_res_objetivos = strData[0];
            this.l_aprobado_obj = strData[1] == "1";

            //Completa los valores GENERALES de la EVALUACIÓN
            double pond_total = 0;
            this.n_resultado = 0;
            if (this.COM_EVALU.Count > 0)
              pond_total += objEvento.n_pond_comp;
            if (this.OBJ_EVALU.Count > 0)
              pond_total += objEvento.n_pond_obj;
            if (this.COM_EVALU.Count > 0)
              this.n_resultado += this.n_res_comp * objEvento.n_pond_comp / pond_total;
            if (this.OBJ_EVALU.Count > 0)
              this.n_resultado += this.n_res_objetivos * objEvento.n_pond_obj / pond_total;
            strData = this.GetValue(xmlEscalas, objEvento.oi_escala_res, this.n_resultado);
            this.c_resultado = strData[0];
            this.l_aprobado = strData[1] == "1";

            try {
                Nomad.NSystem.Base.NomadEnvironment.GetCurrentTransaction().SaveRefresh((NomadObject) this);
                strResult = "";

            } catch (Exception ex) {
                strResult = ex.Message;
            }

            return strResult;
        }

        public void Confirmar()
        {

            // Verifica que esten evaluadas las aptitudes de las competencias
            foreach (COM_EVALU comp in this.COM_EVALU)
                foreach (APT_COM aptitud in comp.APT_COM)
                    if (aptitud.oi_val_escalaNull)
                        throw new NomadAppException("No se puede Confirmar la evaluación porque hay Aptitudes sin evaluar.");

            // Verifica que esten evaluados los objetivos o subobjetivos:
            // Si un objetivo tiene subobjetivos se verifica que esten evaluados,
            // si no tiene subobjetivos se verifica que el objetivo este evaluado
            foreach (OBJ_EVALU obj in this.OBJ_EVALU)
                if (obj.SUBOBJETIVOS.Count > 0)
                {
                    foreach (SUBOBJETIVO subobj in obj.SUBOBJETIVOS)
                        if (subobj.n_cumplimientoNull)
                            throw new NomadAppException("No se puede Confirmar la evaluación porque hay Subobjetivos sin evaluar.");
                }
                else if (obj.n_cumplimientoNull)
                {
                    throw new NomadAppException("No se puede Confirmar la evaluación porque hay Objetivos sin evaluar.");
                }

            this.c_estado = "EV";
            this.f_confirmacion = DateTime.Today;
            NomadEnvironment.GetCurrentTransaction().Save(this);

        }

        /// <summary>
        /// Busca en el documento de Escalas el valor correspondiente.
        /// En la posición 0 retorna la descripción de la escala.
        /// En la posición 1 retorna si aprueba.
        /// </summary>
        private string[] GetValue(XmlDocument pxmlDoc, string pstrOiEscala, double pdblValue) {
            string[] strResult = new string[2];
            string strXPath = "object[@oi_escala_res='" + pstrOiEscala + "' " +
                                "and number('" + StringUtil.dbl2str(pdblValue) + "') >= number(@n_rango_desde) " +
                                "and number('" + StringUtil.dbl2str(pdblValue) + "') <= number(@n_rango_hasta)] ";

            XmlNode objNode = pxmlDoc.DocumentElement.SelectSingleNode(strXPath);
            if (objNode == null) {
                //NO encontró nada
                strResult[0] = "";
                strResult[1] = "";
            } else {
                //Encontró una escala que coincide con lo pedido
                strResult[0] = ((XmlElement) objNode).GetAttribute("d_valor");
                strResult[1] = ((XmlElement) objNode).GetAttribute("l_aprueba");
            }

            return strResult;
        }

        ///}partial;

    }
}



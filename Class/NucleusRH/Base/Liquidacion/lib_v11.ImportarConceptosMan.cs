using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Liquidacion.InterfaceConceptosMan;
using NucleusRH.Base.Liquidacion.Personal_Liquidacion;
using NucleusRH.Base.Liquidacion.Conceptos;

namespace NucleusRH.Base.Liquidacion_ConMan
{

    class clsImportarConMan
    {

        private Hashtable htaOIs = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public clsImportarConMan()
        {
        }

        /// <summary>
        /// Importa los conceptos manuales para una liquidación
        /// </summary>
        /// <param name="pstrOILiquidacion">OI de la Liquidación</param>
        public void Importar(string pstrOILiquidacion)
        {

            Hashtable htaItemsCon;
            Hashtable htaPersonalLiqs = new Hashtable(); //Contendrá los objetos PersonalLiq
            int intItems;
            int intCurrentItem;
            string strValidation;
            string strOIPersonalLiq;
            string strOICon = "";
            int intInserts = 0;
            int intUpdates = 0;

            NomadXML xmlOIItems;
            NomadXML xmlPersonal = null;
            NomadBatch objBatch = NomadBatch.GetBatch("Importación", "Importación");

            ENTRADA objInterfaz;
            PERSONAL_LIQ objPersonalLiq = null;
            CONC_MAN_PER objConceptoPersona;

            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace(" Comienza Importar Conceptos Manuales ------------------------------------");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            objBatch.SetPro(0);
            objBatch.SetMess("Importando los Conceptos Manuales.");
            objBatch.Log("Comienza la importación");

            //Obtiene los tipos de conceptos manuales
            htaItemsCon = NomadEnvironment.QueryHashtable(CONCEPTO.Resources.QRY_Conceptos, "", "cod");

            //Obtiene la lista de OIs de la interface de Conceptos manuales para LegajosLiquidación
            xmlOIItems = NomadEnvironment.QueryNomadXML(ENTRADA.Resources.QRY_OIArchivo, "");
            xmlOIItems = xmlOIItems.FirstChild();

            //Recorre el archivo y pide los DDO de interface
            objBatch.SetPro(10);
            intItems = xmlOIItems.ChildLength;
            intCurrentItem = 0;
            for (NomadXML xmlRowOI = xmlOIItems.FirstChild(); xmlRowOI != null; xmlRowOI = xmlRowOI.Next())
            {

                objInterfaz = ENTRADA.Get(xmlRowOI.GetAttr("id"));
                intCurrentItem++;
                strValidation = "";
                objPersonalLiq = null;

                objBatch.SetMess("Importando el registro '" + intCurrentItem.ToString() + "' de '" + intItems.ToString() + "'.");

                //Valida que el tipo de Concepto exista entre las posibles a importar ----------------------------
                if (htaItemsCon.ContainsKey(objInterfaz.c_concepto))
                {
                    strOICon = ((NomadXML)htaItemsCon[objInterfaz.c_concepto]).GetAttr("oi");
                }
                else
                {
                    strValidation = strValidation + "El concepto '" + objInterfaz.c_concepto + "' no existe. ";
                }

                //Valida el resto del contenido del registro ------------------------------------------------
                //NomadBatch.Trace("Validando valores");
                //strValidation = strValidation + ValidarValores(objInterfaz);

                //Obtiene el PersonalLiq desde la hash o desde un Get ----------------------------------------
                NomadBatch.Trace("Obteniendo el OI");
                if (htaPersonalLiqs.ContainsKey(objInterfaz.e_numero_legajo.ToString()))
                {

                    //Obtiene el PersonalLiq desde la hash
                    objPersonalLiq = (PERSONAL_LIQ)htaPersonalLiqs[objInterfaz.e_numero_legajo.ToString()];

                }
                else
                {
                    //Obtiene el PersonalLiq desde la DB y lo guarda en la hash
                    xmlPersonal = this.GetPersonal(pstrOILiquidacion, objInterfaz.e_numero_legajo.ToString());

                    //Valida que el legajo exista en la liquidación
                    if (xmlPersonal == null)
                    {

                        strValidation = strValidation + "No tiene un número de legajo válido, no pertenece a la liquidación seleccionada o no está inicializado.";
                    }
                    else
                    {

                        strOIPersonalLiq = xmlPersonal.GetAttr("oi_personal_liq");
                        objPersonalLiq = PERSONAL_LIQ.Get(strOIPersonalLiq);
                    }
                }

                //Verifica que el registro no tenga errores
                if (strValidation != "")
                {
                    strValidation = "El registro número " + intCurrentItem.ToString() + " tiene los siguientes errores: " + strValidation;
                    objBatch.Err(strValidation);
                }
                else
                {

                    objConceptoPersona = null;
                    //Recorre la colección de conceptos y valida si existe.
                    foreach (CONC_MAN_PER objConceptoList in objPersonalLiq.CONC_MAN_PER)
                    {

                        if (objConceptoList.oi_concepto == strOICon)
                        {

                            //Ya existe el concepto para el mismo
                            objConceptoPersona = objConceptoList;
                            break;

                        }
                    }

                    if (objConceptoPersona == null)
                    {
                        //No existe en la colección, la crea y la agrega
                        objConceptoPersona = new CONC_MAN_PER();
                        objPersonalLiq.CONC_MAN_PER.Add(objConceptoPersona);
                        intInserts++;
                    }
                    else
                    {
                        intUpdates++;
                    }

                    objConceptoPersona.oi_concepto = strOICon;
                    objConceptoPersona.n_valor = objInterfaz.n_valor;
                    objConceptoPersona.n_cantidad = objInterfaz.n_cantidad;

                    //Se agrega el legajo a la lista de legajos a realizar el save
                    if (!htaPersonalLiqs.ContainsKey(objInterfaz.e_numero_legajo.ToString()))
                        htaPersonalLiqs.Add(objInterfaz.e_numero_legajo.ToString(), objPersonalLiq);

                }

            }

            objBatch.SetPro(80);

            //Recorre los LegajosLiqs y los guarda en la DB
            objBatch.Log("Grabando los datos en la Base de Datos. (" + htaPersonalLiqs.Count.ToString() + " legajos)");
            int x = 1;
            foreach (string strLegajo in htaPersonalLiqs.Keys)
            {

                objBatch.SetPro(80, 100, htaPersonalLiqs.Count, x);

                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save((PERSONAL_LIQ)htaPersonalLiqs[strLegajo]);

                }
                catch (Exception ex)
                {
                    objBatch.Err("No se pudo actualizar el legajo '" + strLegajo + "'. " + ex.Message);
                }

                x++;
            }

            objBatch.SetPro(100);
            objBatch.Log("La importación terminó correctamente.");
            objBatch.Log("Se ingresaron '" + intInserts.ToString() + "' registros nuevos. \\\\ Se modificaron '" + intUpdates.ToString() + "' registros.");
        }

        /*
                /// <summary>
                /// Valida datos del registro de la interfaz
                /// </summary>
                /// <param name="pobjConArchivo"></param>
                /// <returns></returns>
                private string ValidarValores (ENTRADA pobjConArchivo) {
                    string strResult = "";

                    //Valida el año
                    if (pobjIGArchivo.e_anio == 0) {
                    strResult = strResult + "El año no es válido. ";
                    }

                  //Valida el periodo desde
                    if (pobjIGArchivo.e_periodo_desde == 0 || pobjIGArchivo.e_periodo_desde.ToString().Length != 6) {
                    strResult = strResult + "El período desde no es válido. ";
                    }

                  //Valida el periodo hasta
                    if (pobjIGArchivo.e_periodo_hasta == 0 || pobjIGArchivo.e_periodo_hasta.ToString().Length != 6) {
                    strResult = strResult + "El período hasta no es válido. ";
                    }

                  //Valida que los periodos correspondan con el año indicado
                    if (strResult == "") {
                if (pobjIGArchivo.e_anio.ToString() != pobjIGArchivo.e_periodo_desde.ToString().Substring(0, 4) ||
                  pobjIGArchivo.e_anio.ToString() != pobjIGArchivo.e_periodo_hasta.ToString().Substring(0, 4)) {
                      strResult = "Alguno de los períodos no corresponden con el año indicado. ";
                }
                    }

                  return strResult;
                }
        */
        /// <summary>
        /// Obtiene un hashtable accesible por codigo de varible y retorna el OI.
        /// </summary>
        /// <param name="pstrOILiquidacion">Oi de la liquidación.</param>
        /// <param name="pstrLegajo">Código del legajo.</param>
        /// <returns></returns>
        private NomadXML GetPersonal(string pstrOILiquidacion, string pstrLegajo)
        {
            if (this.htaOIs == null)
            {
                string strParametros = "<PARAM oi_liquidacion=\"" + pstrOILiquidacion + "\" />";
                this.htaOIs = NomadEnvironment.QueryHashtable(PERSONAL_LIQ.Resources.QRY_PersonalEnLiq, strParametros, "e_numero_legajo");
            }

            return this.htaOIs.ContainsKey(pstrLegajo) ? (NomadXML)this.htaOIs[pstrLegajo] : null;
        }

    }
}
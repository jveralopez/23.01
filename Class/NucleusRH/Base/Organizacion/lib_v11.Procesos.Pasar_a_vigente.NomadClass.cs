using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Organizacion.Clases_Organizativas;
using NucleusRH.Base.Organizacion.Unidades_Organizativas;
using NucleusRH.Base.Organizacion.Puestos;
using NucleusRH.Base.Organizacion.Vistas;

using NucleusRH.Base.Personal.LegajoEmpresa;

namespace NucleusRH.Base.Organizacion.Puestos
{

    public class clsOrganigrama
    {
        private int intOiClaseOrgPla;
        private int intOiClaseOrgVig;
        private DateTime dteToday;
        private Hashtable htaPuestosCache; //Utilizado para cachear los Puestos utilizados
        private Hashtable htaPuestosToSave; //Utilizado para indicar cuales Puestos se deben GUARDAR
        private Hashtable htaPersEmpByPosic; //Oi de personal_emps con f_egreso = null
        private Hashtable htaPersEmp; //Utilizado para cachear los personal_emp utilizados
        private Hashtable htaEstructurasCache; //Utilizado para cachear las estructuras utilizadas
        private Hashtable htaVistasCache; //Utilizado para cachear las vistas utilizadas

        private const string OITransferencia = "3";

        /// <summary>
        ///
        /// </summary>
        /// <param name="pOiClaseOrgPla"></param>
        public clsOrganigrama(int pOiClaseOrgPla)
        {
            this.intOiClaseOrgPla = pOiClaseOrgPla;

            this.htaPuestosToSave = new Hashtable();
            this.intOiClaseOrgVig = 0;
            this.dteToday = DateTime.Today;
            this.htaPersEmpByPosic = null;

            this.htaPersEmp = new Hashtable();
            this.htaPuestosCache = new Hashtable();
            this.htaEstructurasCache = new Hashtable();
            this.htaVistasCache = new Hashtable();
        }

        /// <summary>
        ///
        /// </summary>
        public void PasarAVigente()
        {
            NomadBatch objBatch;

            int tmpPro;
            Hashtable htaPosiciones;
            Hashtable htaEstructurasPla;
            NomadXML xmlEstructurasVig;
            Hashtable htaLegajosPla;
            NomadXML xmlLegajosVig;
            bool bolTransaction = false;
            string strStep = "";

            TIPO_UNI_ORG objTipoUO;
            PUESTO objPuesto;

            objBatch = NomadBatch.GetBatch("PasarAVigente", "");

            NomadLog.Info("--------------------------------------------------------------------------");
            NomadLog.Info(" Comienza el pasaje a vigente --------------------------------------------");
            NomadLog.Info("--------------------------------------------------------------------------");
            NomadLog.Info("PARAMETROS");
            NomadLog.Info("pOiClaseOrgPla: " + this.intOiClaseOrgPla.ToString());
            NomadLog.Info("--------------------------------------------------------------------------");

            objBatch.SetPro(0);
            objBatch.SetMess("Pasando a estado Vigente el Organigrama.");
            objBatch.Log("Comienza el proceso de cambio de estado a Vigente el Organigrama.");

            try
            {

                //Recupera el tipo de unidad organizativa de tipo POSICION
                objTipoUO = TIPO_UNI_ORG.Get(3, false);

                //----------------------------------------------------------------------------------------------------------------
                objBatch.SetMess("Buscando nuevas posiciones.");
                objBatch.Log("Buscando nuevas posiciones.");

                //Recorre las unidades buscando las nuevas posiciones
                strStep = "Buscando nuevas posiciones.";
                foreach (UNIDAD_ORG objUO in objTipoUO.UNI_ORG)
                {
                    if (objUO.oi_clase_org == this.intOiClaseOrgPla && !objUO.oi_puestoNull)
                    {
                        //Se debe crear una posición desde la unidad encontrada
                        AddPosicion(objUO.Id, objUO.oi_puesto, this.GetCode(objUO.c_unidad_org), objUO.d_unidad_org);

                        //Blanquea los valores ya utilizados. Pero el SAVE se hará en la transacción.
                        objUO.oi_puestoNull = true;
                        objUO.oi_clase_orgNull = true;

                        NomadLog.Info("-->Modifica UnidadOrganizativa con OI " + objUO.Id);
                        NomadLog.Info(".  set oi_puesto: Null");
                        NomadLog.Info(".  set oi_clase_org: Null");

                    }
                }

                //Recorre los puestos y los va grabando con REFRESH
                tmpPro = 0;
                strStep = "Grabando los puestos con las nuevas posiciones.";
                foreach (int intKey in this.htaPuestosCache.Keys)
                {
                    objPuesto = (PUESTO)this.htaPuestosCache[intKey];
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(objPuesto);
                    objBatch.SetPro(15, 20, this.htaPuestosCache.Count, tmpPro);
                    tmpPro++;
                }

                //----------------------------------------------------------------------------------------------------------------
                //Recupera la lista de las estructuras del VIGENTE y del PLANIFICADO
                //Recorre las estrucuturas del VIGENTE y las busca en el PLANIFICADO
                //Las estructuras que del VIGENTE que no estén en el PLANIFICADO serán dadas de "baja"

                objBatch.SetMess("Eliminando posiciones sin uso.");
                objBatch.Log("Eliminando posiciones sin uso.");
                objBatch.SetPro(20);

                //Obtiene el oi_clase_organizativa del vigente
                strStep = "Obteniendo el oi_clase_organizativa del vigente.";
                this.GetOiVigente();

                //Recupera la lista de posiciones accesible por oi_unidad_org
                strStep = "Recuperando la lista de posiciones.";
                htaPosiciones = NomadEnvironment.QueryHashtable(POSICION.Resources.QRY_POSICIONES, "", "oi_unidad_org", false);

                //Recupera la lista de estructuras del PLANIFICADO en una HashTable para facilitar la búsqueda de las estructuras.
                strStep = "Recuperando la lista de estructuras del PLANIFICADO.";
                htaEstructurasPla = NomadEnvironment.QueryHashtable(Clases_Organizativas.CLASE_ORG.Resources.QRY_ESTR_REC, "<DATO oi_clase_org=\"" + this.intOiClaseOrgPla.ToString() + "\" />", "oi_unidad_org", false);

                //Recupera la lista de estructuras del VIGENTE en un NomadXML.
                strStep = "Recuperando la lista de estructuras del VIGENTE.";
                xmlEstructurasVig = NomadEnvironment.QueryNomadXML(Clases_Organizativas.CLASE_ORG.Resources.QRY_ESTR_REC, "<DATO oi_clase_org=\"" + this.intOiClaseOrgVig.ToString() + "\" />");

                objBatch.SetPro(25);

                //Recorre las estrucuturas del vigente y las busca en el planificado para eliminar
                {
                    POSICION objPosicionVig;
                    VISTA objVista;
                    ESTRUCTURA objEstructura;
                    PERSONAL_EMP objPE;

                    NomadXML xmlPosicion;
                    int intChildren;
                    Hashtable htaPers;

                    tmpPro = 0;
                    intChildren = xmlEstructurasVig.FirstChild().ChildLength;
                    for (NomadXML xmlEstructVig = xmlEstructurasVig.FirstChild().FirstChild(); xmlEstructVig != null; xmlEstructVig = xmlEstructVig.Next())
                    {
                        strStep = "Recorriendo las estructuras del vigente y buscandolas en el planificado.";
                        if (xmlEstructVig.GetAttr("oi_unidad_org") == "") continue;

                        //Busca si la unidad organizativa existe en planificado
                        if (!htaEstructurasPla.ContainsKey(xmlEstructVig.GetAttr("oi_unidad_org")))
                        {

                            //Busca la unidad organizativa en la lista de posiciones para obtener cual es el puesto
                            if (htaPosiciones.ContainsKey(xmlEstructVig.GetAttr("oi_unidad_org")))
                            {
                                xmlPosicion = (NomadXML)htaPosiciones[xmlEstructVig.GetAttr("oi_unidad_org")];

                                objPosicionVig = this.GetPosicion(xmlPosicion.GetAttrInt("oi_puesto"), xmlPosicion.GetAttrInt("oi_posicion"));

                                objPosicionVig.l_vigente = false;
                                objPosicionVig.f_hasta_vigencia = this.dteToday;

                                //Valida si tiene una vista asociada
                                if (xmlPosicion.GetAttr("oi_vista") != "")
                                {
                                    strStep = "Obteniendo la vista para actualizar.";
                                    objVista = this.GetVista(xmlPosicion.GetAttr("oi_vista"));

                                    objVista.oi_estructura = xmlEstructurasVig.GetAttr("oi_estructura");

                                    //Blanquea el valor a la vista en la estructura
                                    objEstructura = this.GetEstructura(objVista.oi_estructura);
                                    objEstructura.oi_vistaNull = true;

                                    NomadLog.Info("-->Modifica Vista con OI " + objVista.Id);
                                    NomadLog.Info(".  set oi_estructura:" + objVista.oi_estructura);

                                    NomadLog.Info("-->Modifica Estructura con OI " + objEstructura.Id);
                                    NomadLog.Info(".  set oi_vista: Null");

                                }

                                //Cambia el estado a los Personal_emp con sus posiciones y sus puestos.
                                strStep = "Actualizando los legajos A.";
                                htaPers = GetPersByPosic(xmlPosicion.GetAttr("oi_posicion"));
                                foreach (string strKey in htaPers.Keys)
                                {
                                    objPE = (PERSONAL_EMP)htaPers[strKey];
                                    this.SetPersonalEmp(objPE, "", "");
                                }

                                //Guarda el OI para saber cual PUESTO guardar
                                if (!htaPuestosToSave.ContainsKey(xmlPosicion.GetAttrInt("oi_puesto")))
                                    htaPuestosToSave.Add(xmlPosicion.GetAttrInt("oi_puesto"), xmlPosicion.GetAttrInt("oi_puesto"));

                            }
                        }

                        objBatch.SetPro(30, 50, intChildren, tmpPro);
                        tmpPro++;
                    }

                }

                //----------------------------------------------------------------------------------------------------------------
                //Compara las personas en el PLANIFICADO y el VIGENTE
                objBatch.SetMess("Actualizando las personas.");
                objBatch.Log("Actualizando las personas.");
                objBatch.SetPro(50);

                //Recupera la lista de legajos del PLANIFICADO en una HashTable para facilitar la búsqueda.
                strStep = "Recuperando la lista de legajos del PLANIFICDO.";
                htaLegajosPla = NomadEnvironment.QueryHashtable(Clases_Organizativas.CLASE_ORG.Resources.QRY_LEGS_IN_CLASS, "<DATO oi_clase_org=\"" + this.intOiClaseOrgPla.ToString() + "\" />", "oi_personal_emp", false);

                //Recupera la lista de legajos del VIGENTE en un NomadXML.
                strStep = "Recuperando la lista de legajos del VIGENTE.";
                xmlLegajosVig = NomadEnvironment.QueryNomadXML(Clases_Organizativas.CLASE_ORG.Resources.QRY_LEGS_IN_CLASS, "<DATO oi_clase_org=\"" + this.intOiClaseOrgVig.ToString() + "\" />");

                objBatch.SetPro(53);

                //Recorre los legajos en el PLANIFICADO para detectar los que cambiaron de POSICIÓN
                {
                    NomadXML xmlLegajo;
                    NomadXML xmlPosicion;
                    int intChildren;

                    PERSONAL_EMP objPE;

                    tmpPro = 0;
                    intChildren = htaLegajosPla.Count;
                    strStep = "Actualizando los legajos B.";
                    foreach (string strKey in htaLegajosPla.Keys)
                    {
                        xmlLegajo = (NomadXML)htaLegajosPla[strKey];
                        if (xmlLegajo.GetAttr("oi_posicion") != xmlLegajo.GetAttr("oi_posicion_ult"))
                        {

                            //Busca la unidad organizativa en la lista de posiciones para obtener cual es el puesto
                            if (htaPosiciones.ContainsKey(xmlLegajo.GetAttr("oi_unidad_org")))
                            {

                                xmlPosicion = (NomadXML)htaPosiciones[xmlLegajo.GetAttr("oi_unidad_org")];

                                //Obtiene el PERSONAL_EMP. Setea los nuevos puestos y posiciones ULT y los agrega en las colecciones correspondientes.
                                objPE = this.GetPersonalEmp(xmlLegajo.GetAttr("oi_personal_emp"));
                                this.SetPersonalEmp(objPE, xmlLegajo.GetAttr("oi_posicion"), xmlPosicion.GetAttr("oi_puesto"));
                            }

                        }

                        objBatch.SetPro(55, 65, intChildren, tmpPro);
                        tmpPro++;
                    }
                }

                //Recorre los vigentes y busca los que NO ESTÁN en el PLANIFICADO para nullearles los campos posición y puesto
                {
                    int intChildren;

                    PERSONAL_EMP objPE;

                    tmpPro = 0;
                    intChildren = xmlLegajosVig.FirstChild().ChildLength;
                    strStep = "Actualizando los legajos C.";
                    for (NomadXML xmlLegajo = xmlLegajosVig.FirstChild().FirstChild(); xmlLegajo != null; xmlLegajo = xmlLegajo.Next())
                    {

                        if (!htaLegajosPla.ContainsKey(xmlLegajo.GetAttr("oi_personal_emp")))
                        {

                            //Obtiene el PERSONAL_EMP. Nullea los nuevos puestos y posiciones.
                            objPE = this.GetPersonalEmp(xmlLegajo.GetAttr("oi_personal_emp"));
                            this.SetPersonalEmp(objPE, "", "");
                        }

                        objBatch.SetPro(70, 80, intChildren, tmpPro);
                        tmpPro++;
                    }
                }

                //-----------------------------------------------------------------------------------------------------------------------
                // Modifica los Organigramas --------------------------------------------------------------------------------------------
                //-----------------------------------------------------------------------------------------------------------------------

                //Obtiene los organigramas y les setea los estados y las fechas de vigencias
                CLASE_ORG objCOVig = CLASE_ORG.Get(intOiClaseOrgVig, false);
                CLASE_ORG objCOPla = CLASE_ORG.Get(intOiClaseOrgPla, false);

                objCOVig.f_hasta_vigencia = this.dteToday;
                objCOVig.c_estado = "D";
                NomadLog.Info("-->Modifica CLASE_ORG con OI " + objCOVig.Id);
                NomadLog.Info(".  set f_hasta_vigencia:" + objCOVig.f_hasta_vigencia.ToString("dd/MM/yyyy HH:mm:ss"));
                NomadLog.Info(".  set c_estado:" + objCOVig.c_estado);

                objCOPla.f_desde_vigencia = this.dteToday;
                objCOPla.c_estado = "V";
                NomadLog.Info("-->Modifica CLASE_ORG con OI " + objCOPla.Id);
                NomadLog.Info(".  set f_desde_vigencia:" + objCOPla.f_desde_vigencia.ToString("dd/MM/yyyy HH:mm:ss"));
                NomadLog.Info(".  set c_estado:" + objCOPla.c_estado);

                //-----------------------------------------------------------------------------------------------------------------------
                // Realiza todos los SAVE -----------------------------------------------------------------------------------------------
                //-----------------------------------------------------------------------------------------------------------------------

                //Comienza la transacción
                objBatch.SetMess("Actualizando la Base de Datos.");
                objBatch.Log("Actualizando la Base de Datos.");
                strStep = "Realizando la persistencia de los datos.";
                NomadEnvironment.GetCurrentTransaction().Begin();
                bolTransaction = true;

                //Recorre las personas modificadas y las guarda
                foreach (string strKey in this.htaPersEmp.Keys)
                {
                    NomadEnvironment.GetCurrentTransaction().Save((PERSONAL_EMP)this.htaPersEmp[strKey]);
                }

                //Recorre los PUESTOS modificados y los guarda. No se recorre la hashtable directamente porque podrian enviarse puestos que no se modificaron
                foreach (int intKey in this.htaPuestosToSave.Keys)
                {
                    objPuesto = (PUESTO)this.htaPuestosCache[intKey];

                    NomadLog.Info("-->Graba Puesto con OI " + objPuesto.Id);

                    NomadEnvironment.GetCurrentTransaction().Save(objPuesto);
                }

                //Recorre las estructuras modificadas y las guarda
                foreach (string strKey in this.htaEstructurasCache.Keys)
                {
                    NomadEnvironment.GetCurrentTransaction().Save((ESTRUCTURA)this.htaEstructurasCache[strKey]);
                }

                //Recorre las vistas modificadas y las guarda
                foreach (string strKey in this.htaVistasCache.Keys)
                {
                    NomadEnvironment.GetCurrentTransaction().Save((VISTA)this.htaVistasCache[strKey]);
                }

                //Guarda el tipo de unidad organizativa
                NomadEnvironment.GetCurrentTransaction().Save(objTipoUO);

                //Guarda las Clases organizativas
                NomadEnvironment.GetCurrentTransaction().Save(objCOVig);
                NomadEnvironment.GetCurrentTransaction().Save(objCOPla);

                objBatch.SetPro(90);
                NomadEnvironment.GetCurrentTransaction().Commit();

                objBatch.Log("El proceso finalizó correctamente.");

            }
            catch (Exception ex)
            {

                objBatch.Err("Se ha producido un error " + strStep);
                objBatch.Err(ex.Message);

                if (bolTransaction)
                    NomadEnvironment.GetCurrentTransaction().Rollback();

            }

            objBatch.SetPro(100);
            NomadLog.Info("--------------------------------------------------------------------------");
            NomadLog.Info(" Finaliza el pasaje a vigente --------------------------------------------");
            NomadLog.Info("--------------------------------------------------------------------------");

        }

        /// <summary>
        /// Agrega una nueva posición a un puesto. En caso de existir una posición con el mísmo código lo reutiliza.
        /// </summary>
        /// <param name="pOIUnidaOrg"></param>
        /// <param name="pOIPuesto"></param>
        /// <param name="pCodPosicion"></param>
        /// <param name="pDesPosicion"></param>
        private POSICION AddPosicion(string pOIUnidaOrg, int pOIPuesto, string pCodPosicion, string pDesPosicion)
        {
            PUESTO objPuesto;
            POSICION objNewPos;

            //Verifica si existe una posición con los mismos atributos para reutilizar.
            objNewPos = GetPosicion(pOIPuesto, pCodPosicion);

            if (objNewPos == null)
            {
                //La posición no existe. La crea.

                objPuesto = this.GetPuesto(pOIPuesto);

                objNewPos = new POSICION();

                objPuesto.POSICIONES.Add(objNewPos);

            }

            objNewPos.c_posicion = pCodPosicion;
            objNewPos.d_posicion = pDesPosicion;
            objNewPos.oi_unidad_org = pOIUnidaOrg;

            NomadLog.Info("-->Agrega o reutiliza la Posición con OI " + objNewPos.Id);
            NomadLog.Info(".  set c_posicion:" + objNewPos.c_posicion);
            NomadLog.Info(".  set d_posicion:" + objNewPos.d_posicion);
            NomadLog.Info(".  set oi_unidad_org:" + objNewPos.oi_unidad_org);

            return objNewPos;

        }

        /// <summary>
        /// Retorna la posicion especifica dentro del puesto indicado
        /// </summary>
        /// <param name="pOIPuesto">OI del puesto a recorrer</param>
        /// <param name="pCodPosicion">Código de la posición a buscar</param>
        /// <returns></returns>
        private POSICION GetPosicion(int pOIPuesto, string pCodPosicion)
        {
            PUESTO objPuesto;

            objPuesto = this.GetPuesto(pOIPuesto);

            foreach (POSICION objPosicion in objPuesto.POSICIONES)
            {
                if (objPosicion.c_posicion == pCodPosicion)
                    return objPosicion;
            }

            return null;
        }

        /// <summary>
        /// Retorna la posicion especifica dentro del puesto indicado
        /// </summary>
        /// <param name="pOIPuesto">OI del puesto a recorrer</param>
        /// <param name="pCodPosicion">OI de la posición a buscar</param>
        /// <returns></returns>
        private POSICION GetPosicion(int pOIPuesto, int pOIPosicion)
        {
            PUESTO objPuesto;

            objPuesto = this.GetPuesto(pOIPuesto);

            foreach (POSICION objPosicion in objPuesto.POSICIONES)
            {
                if (objPosicion.id == pOIPosicion)
                    return objPosicion;
            }

            return null;
        }

        /// <summary>
        /// Retorna una estructura desde un caché
        /// </summary>
        /// <param name="pOIEstructura">OI de la estructura solicitada.</param>
        /// <returns></returns>
        private ESTRUCTURA GetEstructura(string pOIEstructura)
        {
            ESTRUCTURA objEstructura;

            //Busca la estructura en el caché de estructuras. Si no existe la carga.
            if (this.htaEstructurasCache.ContainsKey(pOIEstructura))
            {
                objEstructura = (ESTRUCTURA)this.htaEstructurasCache[pOIEstructura];
            }
            else
            {
                objEstructura = ESTRUCTURA.Get(pOIEstructura, false);
                this.htaEstructurasCache.Add(pOIEstructura, objEstructura);
            }

            return objEstructura;
        }

        /// <summary>
        /// Retorna una vista desde un caché
        /// </summary>
        /// <param name="pOIVista">OI de la vista solicitada.</param>
        /// <returns></returns>
        private VISTA GetVista(string pOIVista)
        {
            VISTA objVista;

            //Busca la estructura en el caché de estructuras. Si no existe la carga.
            if (this.htaVistasCache.ContainsKey(pOIVista))
            {
                objVista = (VISTA)this.htaVistasCache[pOIVista];
            }
            else
            {
                objVista = VISTA.Get(pOIVista, false);
                this.htaVistasCache.Add(pOIVista, objVista);
            }

            return objVista;
        }

        /// <summary>
        /// Retorna un puesto desde un caché
        /// </summary>
        /// <param name="pOIPuesto">OI del puesto solicitado.</param>
        /// <returns></returns>
        private PUESTO GetPuesto(int pOIPuesto)
        {
            PUESTO objPuesto;

            //Busca el puesto en el caché de puestos. Si no existe lo carga.
            if (this.htaPuestosCache.ContainsKey(pOIPuesto))
            {
                objPuesto = (PUESTO)this.htaPuestosCache[pOIPuesto];
            }
            else
            {
                objPuesto = PUESTO.Get(pOIPuesto, false);
                this.htaPuestosCache.Add(pOIPuesto, objPuesto);
            }

            return objPuesto;
        }

        /// <summary>
        /// Retorna el Oi de la clase organizativa vigente
        /// </summary>
        /// <returns></returns>
        private void GetOiVigente()
        {
            if (this.intOiClaseOrgVig == 0)
            {
                NomadXML xmlResult;

                xmlResult = NomadEnvironment.QueryNomadXML(Clases_Organizativas.CLASE_ORG.Resources.QRY_VIGENTE, "<DATO oi_clase_org=\"" + this.intOiClaseOrgPla.ToString() + "\"/>");
                this.intOiClaseOrgVig = xmlResult.FirstChild().GetAttrInt("oi_clase_org");
                if (this.intOiClaseOrgVig == 0)
                {
                    NomadLog.Error("No se encontró la clase organizativa vigente.");
                    NomadLog.Error("Clases_Organizativas.CLASE_ORG.Resources.QRY_VIGENTE no retornó resultado.");
                    throw new NomadAppException("No se encontró la clase organizativa vigente.");
                }
            }
        }

        /// <summary>
        /// Retorna el código de la Posción desde un código base
        /// </summary>
        /// <param name="pCodeFrom"></param>
        /// <returns></returns>
        private string GetCode(string pCodeFrom)
        {
            return pCodeFrom.Substring(pCodeFrom.IndexOf("-", pCodeFrom.IndexOf("-") + 1) + 1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pOIPersonalEmp"></param>
        /// <returns></returns>
        private PERSONAL_EMP GetPersonalEmp(string pOIPersonalEmp)
        {
            if (this.htaPersEmp.ContainsKey(pOIPersonalEmp))
            {
                return (PERSONAL_EMP)this.htaPersEmp[pOIPersonalEmp];
            }

            PERSONAL_EMP objPE = PERSONAL_EMP.Get(pOIPersonalEmp, false);
            this.htaPersEmp.Add(pOIPersonalEmp, objPE);

            return objPE;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pOIPosicion"></param>
        /// <returns></returns>
        private Hashtable GetPersByPosic(string pOIPosicion)
        {
            ArrayList arrPers;
            Hashtable htaResult;

            if (this.htaPersEmpByPosic == null)
            {
                //La lista de oi_personal_emps no está cargada. Ejecuta el query y la carga.
                NomadXML xmlPers = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_PER_BY_POSIC, "");
                this.htaPersEmpByPosic = new Hashtable();

                for (NomadXML xmlPer = xmlPers.FirstChild().FirstChild(); xmlPer != null; xmlPer = xmlPer.Next())
                {
                    if (this.htaPersEmpByPosic.ContainsKey(xmlPer.GetAttr("oi_posicion")))
                    {
                        arrPers = (ArrayList)this.htaPersEmpByPosic[xmlPer.GetAttr("oi_posicion")];
                    }
                    else
                    {
                        arrPers = new ArrayList();
                        this.htaPersEmpByPosic.Add(xmlPer.GetAttr("oi_posicion"), arrPers);
                    }

                    arrPers.Add(xmlPer.GetAttr("oi_personal_emp"));

                }
            }

            //Retorna una hashtable con los DDO de Personal_emp
            htaResult = new Hashtable();
            if (this.htaPersEmpByPosic.ContainsKey(pOIPosicion))
            {
                arrPers = (ArrayList)this.htaPersEmpByPosic[pOIPosicion];

                for (int X = 0; X < arrPers.Count; X++)
                {
                    htaResult.Add(arrPers[X], this.GetPersonalEmp((string)arrPers[X]));
                }
            }

            return htaResult;

        }

        /// <summary>
        /// Actualiza los datos de los puestos y las posiciones al Personal pasado. Puede agregar nuevas posiciones y puestos.
        /// </summary>
        /// <param name="pobjPE">Objeto personal al que se le actualizan los puestos y las posiciones.</param>
        /// <param name="pOIPosicion">Oi posicion a agregar. En caso de ser nullstring setea el oi_posicion_ult en null.</param>
        /// <param name="pOIPuesto">Oi puesto a agregar. En caso de ser nullstring setea el oi_puesto_ult en null.</param>
        private void SetPersonalEmp(PERSONAL_EMP pobjPE, string pOIPosicion, string pOIPuesto)
        {
            POSICION_PER objPosicionPer;
            PUESTO_PER objPuestoPer;

            //Limpia la posición y puesto actual --------------------------------------------------------
            NomadLog.Info("-->Modifica Personal_Emp con OI " + pobjPE.Id);

            //Actualiza la Posicion_Per
            foreach (POSICION_PER objPoP in pobjPE.POSIC_PER)
            {
                if (objPoP.f_egresoNull)
                {
                    objPoP.f_egreso = this.dteToday;
                    objPoP.oi_motivo_cambio = OITransferencia;

                    NomadLog.Info(".  -->Modifica Personal_Emp.Posion_Per con OI " + objPoP.Id);
                    NomadLog.Info(".  .  set f_egreso:" + objPoP.f_egreso.ToString("dd/MM/yyyy HH:mm:ss"));

                    break;
                }
            }

            //Actualiza la Puesto_Per
            foreach (PUESTO_PER objPuP in pobjPE.PUESTO_PER)
            {
                if (objPuP.f_egresoNull)
                {
                    objPuP.f_egreso = this.dteToday;
                    objPuP.oi_motivo_cambio = OITransferencia;

                    NomadLog.Info(".  -->Modifica Personal_Emp.Puesto_Per con OI " + objPuP.Id);
                    NomadLog.Info(".  .  set f_egreso:" + objPuP.f_egreso.ToString("dd/MM/yyyy HH:mm:ss"));

                    break;
                }
            }

            //Si se le pasa una posición se intenta darla de alta ------------------------------------------
            if (pOIPosicion != "")
            {
                //Agrega una nueva Posicion_Per
                objPosicionPer = new POSICION_PER();
                objPosicionPer.oi_posicion = pOIPosicion;
                objPosicionPer.f_ingreso = this.dteToday;
                pobjPE.POSIC_PER.Add(objPosicionPer);

                NomadLog.Info(".  -->Agrega Personal_Emp.Posicion_Per con OI " + objPosicionPer.Id);
                NomadLog.Info(".  .  set oi_posicion:" + objPosicionPer.oi_posicion);
                NomadLog.Info(".  .  set f_ingreso:" + objPosicionPer.f_ingreso.ToString("dd/MM/yyyy HH:mm:ss"));

                //Actualiza el personal_emp
                pobjPE.oi_posicion_ult = pOIPosicion;
                pobjPE.f_desde_posicion = this.dteToday;
                NomadLog.Info(".  set oi_posicion_ult:" + pobjPE.oi_posicion_ult);
                NomadLog.Info(".  set f_desde_posicion:" + pobjPE.f_desde_posicion.ToString("dd/MM/yyyy HH:mm:ss"));

            }
            else
            {
                //Actualiza el personal_emp
                pobjPE.oi_posicion_ultNull = true;
                pobjPE.f_desde_posicionNull = true;
                NomadLog.Info(".  set oi_posicion_ult: Null");
                NomadLog.Info(".  set f_desde_posicion: Null");
            }

            //Si se le pasa un puesto se intenta darlo de alta ------------------------------------------
            if (pOIPuesto != "")
            {
                //Agrega un nuevo Puesto_Per
                objPuestoPer = new PUESTO_PER();
                objPuestoPer.oi_puesto = pOIPuesto;
                objPuestoPer.f_ingreso = this.dteToday;
                pobjPE.PUESTO_PER.Add(objPuestoPer);

                NomadLog.Info(".  -->Agrega Personal_Emp.Puesto_Per con OI " + objPuestoPer.Id);
                NomadLog.Info(".  .  set oi_puesto:" + objPuestoPer.oi_puesto);
                NomadLog.Info(".  .  set f_ingreso:" + objPuestoPer.f_ingreso.ToString("dd/MM/yyyy HH:mm:ss"));

                pobjPE.oi_puesto_ult = pOIPuesto;
                pobjPE.f_desde_puesto = this.dteToday;
                NomadLog.Info(".  set oi_puesto_ult:" + pobjPE.oi_puesto_ult);
                NomadLog.Info(".  set f_desde_puesto:" + pobjPE.f_desde_puesto.ToString("dd/MM/yyyy HH:mm:ss"));

            }
            else
            {
                pobjPE.oi_puesto_ultNull = true;
                pobjPE.f_desde_puestoNull = true;
                NomadLog.Info(".  set oi_puesto_ult: Null");
                NomadLog.Info(".  set f_desde_puesto: Null");
            }

            if (pobjPE.POSIC_PER.Count > 0)
            {
                NucleusRH.Base.Organizacion.Puestos.POSICION.Cambiar_Pos_Legajo_Sectores(pobjPE.oi_posicion_ult, pOIPuesto, pobjPE.id.ToString(), this.dteToday, "", "");
            }
            else
            {
                NucleusRH.Base.Organizacion.Puestos.POSICION.AddLegajoSectores(pOIPosicion, pobjPE.id.ToString(), this.dteToday, new DateTime(1899, 1, 1), "", "");
            }
        }
    }
}

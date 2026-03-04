using System;
using System.Collections.Generic;
using System.Text;
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

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace NucleusRH.Base.Indicadores.Ejecuciones
{
    class clsTablasFact
    {
        Indicadores.Ejecuciones.clsDataBase objDB;
        string mConn2;
        Hashtable hsUpdDB;
        Hashtable hsUpdDBType;
        NomadBatch objBatch;
        Hashtable hsLog;
        int OI_ANIOMES;
        string strFacProces;
        string sFncEjecutada;
        string sCod_ejec;
        string C_ANIOMES;
        string sLegajoProc;
        string strMsgErrorUser;
        string strMenuSueldos;
        string strMenuPersonal;
        string strMenuHoras;
        string strMenuProy;
        string strMenuEvaluacion;
        string strMenuSueldosC;
        string strErroresProc;
        public string strMsgMail;
        NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION objEjec;
        NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE objEjecDet;

        NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION objEjec_Eva;
        NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION objEjec_Horas;
        NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION objEjec_Proy;
        NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION objEjec_Pers;
        NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION objEjec_Sueldo;
        NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION objEjec_Estr;
        public void ConectaNomadFact(int parAño, int parMes, string strTablasToUpd, string sCod_ejecucion, string myConnString)
        {
            mConn2 = "Provider=SQLOLEDB;Data Source=NUCLEUSDOM;User ID=sa;Password=sa;Initial Catalog=BaytonTecnologiaRH";
            strErroresProc = "";
            sCod_ejec = sCod_ejecucion;
            sFncEjecutada = "ConectaNomad";
            //objBatch = NomadBatch.GetBatch("Source", "PreMess");
            objBatch = NomadBatch.GetBatch("Iniciando...", "Actualizando Indicadores ");
            NomadBatch.Trace(" Entra  ConectaNomadFact");
            NomadBatch.Trace("  parAño " + parAño.ToString());
            NomadBatch.Trace("  parMes " + parMes.ToString());
            NomadBatch.Trace("  strTablasToUpd:" + strTablasToUpd);
            NomadBatch.Trace("  myConnString:" + myConnString);
            NomadBatch.Trace("  sCod_ejecucion :" + sCod_ejecucion);

            objDB = new clsDataBase(myConnString, objBatch);

            strFacProces = "";
            strMsgMail = "";
            objEjec = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
            if (strTablasToUpd.IndexOf("Fact_Proyectos") != -1)
            {

                objEjec_Proy = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
                objEjec_Proy.c_ejecucion = sCod_ejec;
                objEjec_Proy.c_estado = "E";
                objEjec_Proy.c_modulo_tco = "PROYECTOS";
                objEjec_Proy.d_ejecucion = "PROYECTOS Bayton Tecnologia ";

                strMenuProy = " \n Proyectos                 ";
                fnc_Init_UpdFacProy(parAño, parMes, myConnString, mConn2);
            }
            else
            {
                objEjec_Eva = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
                objEjec_Horas = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
                objEjec_Proy = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
                objEjec_Pers = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
                objEjec_Sueldo = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
                objEjec_Estr = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
                objEjec_Eva.SerializeAll();
                objEjec_Eva.c_ejecucion = sCod_ejec;
                objEjec_Eva.c_estado = "E";
                objEjec_Eva.c_modulo_tco = "Evaluacion";
                objEjec_Eva.d_ejecucion = "Evaluacion ";

                objEjec_Horas.c_ejecucion = sCod_ejec;
                objEjec_Horas.c_estado = "E";
                objEjec_Horas.c_modulo_tco = "Horas";
                objEjec_Horas.d_ejecucion = "Horas ";

                objEjec_Pers.c_ejecucion = sCod_ejec;
                objEjec_Pers.c_estado = "E";
                objEjec_Pers.c_modulo_tco = "Personal";
                objEjec_Pers.d_ejecucion = "Personal ";

                objEjec_Sueldo.c_ejecucion = sCod_ejec;
                objEjec_Sueldo.c_estado = "E";
                objEjec_Sueldo.c_modulo_tco = "Sueldos";
                objEjec_Sueldo.d_ejecucion = "Sueldos ";

                strMenuSueldos = " \n Sueldos promedios Netos                  ";
                strMenuSueldos += " \n Sueldos promedios Netos Acumulados       ";
                strMenuSueldos += " \n Costo Laboral                            ";
                strMenuSueldos += " \n Análisis de variación de costo laboral   ";

                strMenuHoras = " \n Horas - Grupo Tipo Horas                 ";
                strMenuHoras += " \n Horas - Tipo Horas                       ";

                strMenuPersonal = " \n Sueldos Conformados                      ";
                strMenuPersonal += " \n Legajos según rango de edades            ";
                strMenuPersonal += " \n Promedio de edades                       ";
                strMenuPersonal += " \n Legajos próximos a jubilarse             ";
                strMenuPersonal += " \n Antiguedad del personal                  ";
                strMenuPersonal += " \n Altas y Egresos del personal             ";
                strMenuPersonal += " \n Indices de Ausentismo                    ";
                strMenuPersonal += " \n Horas Capacitación                       ";

                strMenuSueldosC = " \n Remuneración de Conceptos                ";

                fnc_Init_UpdFacTables(parAño, parMes, strTablasToUpd);
            }

            //fncSendMail(xmlMails);
        }
        //*****************************************************************************************************
        private NomadXML NomadQuery(string strTabla, string strWhere, string strCampos, string strCamposAlias, string strTablaDestino, ref Hashtable hashColumns)
        {
            System.Xml.XmlDocument xmlDocCal;

            strTabla = strTabla.ToUpper();
            strWhere = strWhere.ToUpper();
            strCampos = strCampos.ToUpper();
            strCamposAlias = strCamposAlias.ToUpper();
            strTablaDestino = strTablaDestino.ToUpper();

            Array arrCampos;
            //fncUpdEstructuras();
            arrCampos = strCamposAlias.Split(',');

            // EjecutarQuery devuelve un xml con el resultado de la consulta y los tipos de datos por cada campo
            xmlDocCal = EjecutarQuery(strTabla, strWhere, strCampos, strCamposAlias, ref arrCampos);

            if (xmlDocCal == null) { return null; }

            NomadXML nmdXML;

            nmdXML = new NomadXML(xmlDocCal.DocumentElement.InnerXml);
            NomadXML nmdXMLCol = nmdXML.FirstChild().FindElement("COLUMNS");

            hashColumns.Clear();

            // Guarda en Hash el tipo de dato de cada columna
            foreach (NomadXML xmlCal in nmdXMLCol.GetChilds())
            {
                hashColumns.Add(xmlCal.GetAttrString("Name"), xmlCal.GetAttrString("type"));

            }
            NomadXML nmdXMLOut;

            nmdXMLOut = new NomadXML(xmlDocCal.DocumentElement.InnerXml);
            return nmdXMLOut;

        }
        //************************************************************************************//

        private void fnc_Init_UpdFacProy(int par_Año, int par_mes, string myConnString, string myConnProy)
        {
            DateTime fecha_desde;
            DateTime fecha_hasta;
            string strQry;
            DataTable objTblIndic;
            string strCampos;
            string strTbl;
            string strWHERE;
            DataSet myData;
            Hashtable hsColumns;
            NomadXML xmlAllEmp;
            int rtaQry;

            string strTMP;
            string sFecha_desde;
            string sFecha_Hasta;
            int int_tmp;
            hsColumns = new Hashtable();
            hsUpdDB = new Hashtable();
            hsUpdDBType = new Hashtable();
            double REMUN_PERS;
            objBatch.Log(" Comienza actualizacion de proyectos ");
            objBatch.SetPro(50);
            NomadBatch.Trace("  entra en fnc_Init_UpdFacProy ");
            NomadBatch.Trace("   myConnString " + myConnString);
            NomadBatch.Trace("   en myConnProy " + myConnProy);
            sFncEjecutada = "fnc_Init_UpdFacProy";
            try
            {
                // Logea en tablas del sistema

                objEjec.c_ejecucion = sCod_ejec;
                objEjec.f_Inicio_ejec = DateTime.Now;
                objEjec.c_estado = "E";
                objEjec.c_modulo_tco = "PROYECTOS";
                objEjec.d_ejecucion = "PROYECTOS BaytonTec";
                ////////////////////////////////////////////////////////////////

                // definicion de fechas **********************************************
                fecha_desde = new DateTime(par_Año, par_mes, 1);
                fecha_hasta = fecha_desde.AddMonths(1);
                int_tmp = 100 + fecha_hasta.Month;
                strTMP = int_tmp.ToString().Substring(1, int_tmp.ToString().Length - 1);

                //sFecha_Hasta = fecha_hasta.Year.ToString() + strTMP + "01";
                //C_ANIOMES = "0" + par_mes.ToString() + "/" + par_Año.ToString();
                //C_ANIOMES = C_ANIOMES.Substring(C_ANIOMES.Length - 7, C_ANIOMES.Length);

                sFecha_Hasta = fecha_hasta.Year.ToString() + strTMP + "01";
                C_ANIOMES = "0" + par_mes.ToString();
                C_ANIOMES = C_ANIOMES.Substring(C_ANIOMES.Length - 2, 2) + "/" + par_Año.ToString();

                fecha_desde = fecha_desde.AddDays(-1);
                int_tmp = 100 + fecha_desde.Month;
                strTMP = int_tmp.ToString().Substring(1, int_tmp.ToString().Length - 1);
                sFecha_desde = fecha_desde.Year.ToString() + strTMP + fecha_desde.Day.ToString();
                //*********************************************************************
                strMsgErrorUser = sLegajoProc + " Generando Anio mes en DataMart ";
                strQry = "SELECT OI_ANIOMES , C_ANIOMES FROM TCO_MesesAnios WHERE C_ANIOMES = '" + C_ANIOMES + "'";
                Hashtable hasMesAnio = objDB.GetHashDimension(strQry);
                rtaQry = 0;
                // SI la respuesta es cero implica el periodo en la tabla, debe ser insertado TCO_MESANIO
                if (hasMesAnio.Count == 0)
                {
                    strQry = "INSERT INTO TCO_MesesAnios (C_ANIOMES, D_ANIOMES, C_BIMESTRE, C_TRIMESTRE, C_CUATRIMESTRE,   C_SEMESTRE, C_ANIO)";
                    strQry += " VALUES( '" + C_ANIOMES + "' ," + clsDataBase.fnc_MesesAnios(par_mes) + ",'" + par_Año.ToString() + "') ";
                    strQry = strQry.Replace("\t", "");
                    //rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strQry);
                    fncLogeaDB("TCO_MesesAnios", strQry, strQry, ref rtaQry);
                    strQry = "SELECT OI_ANIOMES , C_ANIOMES FROM TCO_MESESANIOS WHERE C_ANIOMES = '" + C_ANIOMES + "'";
                    hasMesAnio = objDB.GetHashDimension(strQry);
                }
                // PASAR A CLSDB
                objEjec.c_mes_anio = C_ANIOMES;

                strTMP = (string)hasMesAnio[C_ANIOMES];
                OI_ANIOMES = Int32.Parse(strTMP);
                strTbl = "";
                strWHERE = "";
                strCampos = "";
                NomadBatch.Trace("   en myConnProy " + myConnProy);

                objBatch.Log(" Actualizacion datos de proyectos ");
                // limpia Estructuras_Clousure
                strTbl = " DELETE FROM TCO_ESTRUC_PROYECTOS ";
                fncLogeaDB("TCO_ESTRUC_PROYECTOS", strTbl, strTbl, ref rtaQry);

                strTbl = objDB.ChangeConnectDB(myConnProy);
                clsDataBase.fncFindProyClosure(ref   strCampos);
                myData = objDB.ExecDataSet(strCampos);

                objTblIndic = new DataTable();
                objTblIndic = myData.Tables[0];
                strTbl = objDB.ChangeConnectDB(myConnString);
                hsLog = new Hashtable();
                hsLog.Add("PROYECTOS_PROYCLOSURE", "0,0");
                for (int i = 0; i < objTblIndic.Rows.Count; i++)
                {
                    try
                    {

                        hsUpdDB = new Hashtable();
                        hsUpdDBType = new Hashtable();

                        hsUpdDB.Add("OI_PROYECTO", objTblIndic.Rows[i]["OI_PROYECTO"]);
                        hsUpdDB.Add("OI_PROYECTO_PARENT", objTblIndic.Rows[i]["OI_PROYECTO_PARENT"]);
                        hsUpdDB.Add("OI_TIPO_PROYECTO", objTblIndic.Rows[i]["OI_TIPO_PROYECTO"]);
                        hsUpdDB.Add("C_PROYECTO", "" + objTblIndic.Rows[i]["C_PROYECTO"] + "");
                        hsUpdDB.Add("D_PROYECTO", "" + objTblIndic.Rows[i]["D_PROYECTO"] + "");
                        hsUpdDB.Add("O_PROYECTO", "" + objTblIndic.Rows[i]["O_PROYECTO"] + "");
                        hsUpdDB.Add("C_ESTADO", "" + objTblIndic.Rows[i]["C_ESTADO"] + "");
                        hsUpdDB.Add("OI_RECURSO", objTblIndic.Rows[i]["OI_RECURSO"]);

                        hsUpdDBType.Add("OI_PROYECTO", "INT");
                        hsUpdDBType.Add("OI_PROYECTO_PARENT", "INT");
                        hsUpdDBType.Add("OI_TIPO_PROYECTO", "INT");
                        hsUpdDBType.Add("C_PROYECTO", "STR");
                        hsUpdDBType.Add("D_PROYECTO", "STR");
                        hsUpdDBType.Add("O_PROYECTO", "STR");
                        hsUpdDBType.Add("C_ESTADO", "STR");
                        hsUpdDBType.Add("OI_RECURSO", "INT");

                        UpDateToDB(null, hsColumns, "PROYECTOS_PROYCLOSURE", strWHERE);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err(" Error la carga de la Fact Proyectos (PROYECTOS_PROYCLOSURE)");
                        NomadBatch.Trace(" Error en fnc_Init_UpdFacProy (PROYECTOS_PROYCLOSURE)" + e.Message);
                    }
                }
                ////////////////////////////////////////////////
                objBatch.Log(" Actualizacion de estructura de proyectos ");
                //Actualiza la jerarquia de estructuras

                fncUpdEstructuras();
                fncUpdEstrucProy();

                objBatch.Log(" Actualizacion de Recursos ");
                //DIMENSION Recusrsos
                // LLENA LA DIMENSION  (NO USAMOS LA CLASE DE DIMENSIONES PORQUE TRABAJA CON UNA DB EXTERNA)
                objBatch.Log(" Actualizacion de Recursos ");
                strTbl = objDB.ChangeConnectDB(myConnProy);
                clsDataBase.fncFindDimProyRec(ref   strCampos);
                myData = objDB.ExecDataSet(strCampos);

                objTblIndic = new DataTable();
                objTblIndic = myData.Tables[0];
                strTbl = objDB.ChangeConnectDB(myConnString);
                hsLog = new Hashtable();
                hsLog.Add("PROYECTOS_DIMREC", "0,0");
                for (int i = 0; i < objTblIndic.Rows.Count; i++)
                {
                    try
                    {
                        // funcion calcular edad
                        hsUpdDB = new Hashtable();
                        hsUpdDB.Add("OI_RECURSO", objTblIndic.Rows[i]["OI_RECURSO"]);
                        hsUpdDB.Add("O_OBSERVACION", "" + objTblIndic.Rows[i]["O_OBSERVACION"] + "");
                        hsUpdDB.Add("OI_PERSONAL", objTblIndic.Rows[i]["OI_PERSONAL"]);
                        hsUpdDB.Add("C_RECURSO", "" + objTblIndic.Rows[i]["C_RECURSO"] + "");
                        hsUpdDB.Add("E_HPROM", objTblIndic.Rows[i]["E_HPROM"]);

                        hsUpdDBType = new Hashtable();
                        hsUpdDBType.Add("OI_RECURSO", "INT");
                        hsUpdDBType.Add("O_OBSERVACION", "STR");
                        hsUpdDBType.Add("OI_PERSONAL", "INT");
                        hsUpdDBType.Add("C_RECURSO", "STR");
                        hsUpdDBType.Add("E_HPROM", "INT");

                        UpDateToDB(null, hsColumns, "PROYECTOS_DIMREC", strWHERE);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err(" Error la carga de la Fact Proyectos (PROYECTOS_DIMREC)");
                        NomadBatch.Trace(" Error en fnc_Init_UpdFacProy (PROYECTOS_DIMREC)" + e.Message);
                    }
                }
                ////////////////////////////////////////////////
                //DIMENSION Perfiles
                // LLENA LA DIMENSION  (NO USAMOS LA CLASE DE DIMENSIONES PORQUE TRABAJA CON UNA DB EXTERNA)
                objBatch.Log(" Actualizacion de Perfiles");
                strTbl = objDB.ChangeConnectDB(myConnProy);
                clsDataBase.fncFindDimProyPerf(ref   strCampos);
                myData = objDB.ExecDataSet(strCampos);

                objTblIndic = new DataTable();
                objTblIndic = myData.Tables[0];
                strTbl = objDB.ChangeConnectDB(myConnString);
                hsLog = new Hashtable();
                hsLog.Add("PROYECTOS_DIMPERF", "0,0");
                for (int i = 0; i < objTblIndic.Rows.Count; i++)
                {
                    try
                    {
                        hsUpdDB = new Hashtable();
                        hsUpdDB.Add("OI_PERFIL", objTblIndic.Rows[i]["OI_PERFIL"]);
                        hsUpdDB.Add("C_PERFIL", "" + objTblIndic.Rows[i]["C_PERFIL"] + "");
                        hsUpdDB.Add("D_PERFIL", "" + objTblIndic.Rows[i]["D_PERFIL"] + "");

                        hsUpdDBType = new Hashtable();
                        hsUpdDBType.Add("OI_PERFIL", "INT");
                        hsUpdDBType.Add("C_PERFIL", "STR");
                        hsUpdDBType.Add("D_PERFIL", "STR");

                        UpDateToDB(null, hsColumns, "PROYECTOS_DimPerf", strWHERE);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err(" Error la carga de la Fact Proyectos (PROYECTOS_DIMPERF)");
                        NomadBatch.Trace(" Error en fnc_Init_UpdFacProy (PROYECTOS_DIMPERF)" + e.Message);
                    }
                }

                // Agregado 07/09/2009

                objBatch.Log(" Elimina FACT Proyectos Mes " + C_ANIOMES);

                strTbl = " DELETE FROM TCO_FACT_PROYHORAS WHERE OI_ANIOMES = " + OI_ANIOMES.ToString();
                fncLogeaDB("TCO_FACT_PROYHORAS_Del", strTbl, strTbl, ref rtaQry);

                // fin Agregado 07/09/2009
                objBatch.Log(" Actualizacion FACT Proyectos");

                // este has lo utilizara para el Log al final
                hsLog = new Hashtable();
                hsLog.Add("PROYECTOS", "0,0");
                // CAMBIA LA CONEXION DE A LA DB DE PROYECTOS
                strTbl = objDB.ChangeConnectDB(myConnProy);
                clsDataBase.fncFindHorasProy(sFecha_desde, sFecha_Hasta, ref   strCampos, ref   strTbl, ref   strWHERE);
                // EXTRACCION DE DATOS
                myData = objDB.ExecDataSet(strCampos);

                objTblIndic = myData.Tables[0];
                // CAMBIA LA CONEXION DE A LA DB DE DATAMART PARA CONTINUAR CON LA SECUENCIA NORMAL DE INSERCION DE DATOS
                strTbl = objDB.ChangeConnectDB(myConnString);

                for (int i = 0; i < objTblIndic.Rows.Count; i++)
                {
                    try
                    {
                        hsUpdDB = new Hashtable();
                        hsUpdDB.Add("OI_RECURSO", objTblIndic.Rows[i]["OI_RECURSO"]);
                        hsUpdDB.Add("OI_ANIOMES", OI_ANIOMES);
                        hsUpdDB.Add("OI_PROYECTO", objTblIndic.Rows[i]["OI_PROYECTO"]);
                        hsUpdDB.Add("OI_PERFIL", objTblIndic.Rows[i]["OI_PERFIL"]);
                        hsUpdDB.Add("HORAS_DECLARADAS", objTblIndic.Rows[i]["HORAS_DECLARADAS"]);

                        hsUpdDBType = new Hashtable();
                        hsUpdDBType.Add("OI_RECURSO", "INT");
                        hsUpdDBType.Add("OI_ANIOMES", "INT");
                        hsUpdDBType.Add("OI_PROYECTO", "INT");
                        hsUpdDBType.Add("OI_PERFIL", "INT");
                        hsUpdDBType.Add("HORAS_DECLARADAS", "FLOAT");

                        UpDateToDB(null, hsColumns, "PROYECTOS", strWHERE);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err(" Error la carga de la Fact Proyectos ");
                        NomadBatch.Trace(" Error en fnc_Init_UpdFacProy " + e.Message);
                    }
                }

            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fnc_Init_UpdFacProy ");
                NomadBatch.Trace(" Error en fnc_Init_UpdFacProy " + e.Message);

            }
            objBatch.SetPro(100);
            if (objEjec.c_estado == "E") { objEjec.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
            objEjec.f_fin_ejec = DateTime.Now;
            fncSaveDDO();
            objBatch.Log(" Fin Actualizacion de Proyectos");

        }
        ////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////

        private void fnc_Init_UpdFacTables(int par_Año, int par_mes, string sFactActualiza)
        {
            DateTime fecha_desde;
            DateTime fecha_hasta;

            DateTime inicio_año;
            DateTime fin_año;
            DateTime fecha_tmp;

            string strQry;

            string strCampos;
            string strTbl;
            string strWHERE;
            string Maxfec_ing;
            Hashtable hsColumns;
            Hashtable hasTmp;
            NomadXML xmlAllEmpresa;
            NomadXML xmlAllEmp;
            NomadXML xmlTMP;
            NomadXML xmlTMP2;
            int rtaQry;
            int oiEmpresa;
            int OI_PERSONAL_EMP;
            int oiPuesto;
            int oi_est_Func;
            int nCantFact;
            string strTMP;
            string sFecha_desde;
            string sFecha_Hasta;
            string C_EMPRESA;
            string D_EMPRESA_Log;
            string cSexo;
            int oi_UO;
            int int_tmp;
            int edad;
            int oisexo;
            int nCantPers;
            int nLogCount;
            bool InsertInHas;
            hsColumns = new Hashtable();
            hsUpdDB = new Hashtable();
            hsUpdDBType = new Hashtable();
            double REMUN_PERS;
            int OI_CLASE_ORG;
            Array arrActualizar = sFactActualiza.Split(',');
            nLogCount = 40;
            objBatch.SetPro(nLogCount);
            sFncEjecutada = "fnc_Init_UpdFacTables";

            NomadBatch.Trace(" entra en fnc_Init_UpdFacTables" );
            string sEstruFun;
            try
            {

                // obtiene la OI_CLASE_ORG para usarlo como filtro en la empresa funcional
                Hashtable HasCls;
                OI_CLASE_ORG = 0;

                HasCls = new Hashtable();
                xmlAllEmp = NomadQuery("ORG02_CLASES_ORG", " ORG02_CLASES_ORG.C_CLASE_ORG = \\'DISALE\\'", "ORG02_CLASES_ORG.OI_CLASE_ORG", "OI_CLASE_ORG", "", ref HasCls);
                OI_CLASE_ORG = xmlAllEmp.FirstChild().FirstChild().GetAttrInt("OI_CLASE_ORG");

                // Logea en tablas del sistema

                objEjec.c_ejecucion = sCod_ejec;
                objEjec.f_Inicio_ejec = DateTime.Now;
                objEjec.c_estado = "E";
                objEjec.c_modulo_tco = "datosComunes";
                objEjec.d_ejecucion = "datos comunes modulos " + sFactActualiza.Replace(" ", "");

                ////////////////////////////////////////////////////////////////

                //Actualiza la jerarquia de estructuras
                fncUpdEstructuras();
                fncUpdEstructurasFunc();
                NomadBatch.Trace(" Estructuras actualizadas");
                // definicion de fechas **********************************************
                fecha_desde = new DateTime(par_Año, par_mes, 1);
                fecha_hasta = fecha_desde.AddMonths(1);
                int_tmp = 100 + fecha_hasta.Month;
                strTMP = int_tmp.ToString().Substring(1, int_tmp.ToString().Length - 1);
                //sFecha_Hasta = fecha_hasta.Year.ToString() + strTMP + "01";
                //C_ANIOMES = "0" + par_mes.ToString() + "/" + par_Año.ToString();
                //C_ANIOMES = C_ANIOMES.Substring(C_ANIOMES.Length - 7, C_ANIOMES.Length);

                sFecha_Hasta = fecha_hasta.Year.ToString() + strTMP + "01";
                C_ANIOMES = "0" + par_mes.ToString();
                C_ANIOMES = C_ANIOMES.Substring(C_ANIOMES.Length - 2, 2) + "/" + par_Año.ToString();

                fecha_desde = fecha_desde.AddDays(-1);
                int_tmp = 100 + fecha_desde.Month;
                strTMP = int_tmp.ToString().Substring(1, int_tmp.ToString().Length - 1);
                sFecha_desde = fecha_desde.Year.ToString() + strTMP + fecha_desde.Day.ToString();
                //*********************************************************************
                Hashtable HasSexo = objDB.GetHashDimension(" SELECT OI_SEXO , C_SEXO FROM TCO_SEXOS ");

                strMsgErrorUser = sLegajoProc + " Generando Anio mes en DataMart ";
                strQry = "SELECT OI_ANIOMES , C_ANIOMES FROM TCO_MESESANIOS WHERE C_ANIOMES = '" + C_ANIOMES + "'";
                Hashtable hasMesAnio = objDB.GetHashDimension(strQry);
                rtaQry = 0;
                // SI la respuesta es cero implica el periodo en la tabla, debe ser insertado TCO_MESANIO
                if (hasMesAnio.Count == 0)
                {
                    strQry = "INSERT INTO TCO_MESESANIOS (C_ANIOMES, D_ANIOMES, C_BIMESTRE, C_TRIMESTRE, C_CUATRIMESTRE,   C_SEMESTRE, C_ANIO)";
                    strQry += " VALUES( '" + C_ANIOMES + "' ," + clsDataBase.fnc_MesesAnios(par_mes) + ",'" + par_Año.ToString() + "') ";
                    strQry = strQry.Replace("\t", "");
                    //rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strQry);
                    fncLogeaDB("TCO_MESESANIOS", strQry, strQry, ref rtaQry);
                    strQry = "SELECT OI_ANIOMES , C_ANIOMES FROM TCO_MESESANIOS WHERE C_ANIOMES = '" + C_ANIOMES + "'";
                    hasMesAnio = objDB.GetHashDimension(strQry);
                }
                // PASAR A CLSDB
                objEjec.c_mes_anio = C_ANIOMES;
                NomadBatch.Trace(" paso MesAnio");
                strTMP = (string)hasMesAnio[C_ANIOMES];
                OI_ANIOMES = Int32.Parse(strTMP);
                strCampos = "ORG03_EMPRESAS.OI_EMPRESA,ORG03_EMPRESAS.C_EMPRESA,ORG03_EMPRESAS.D_EMPRESA,ORG03_EMPRESAS.C_CUIT,ORG03_EMPRESAS.OI_UNIDAD_ORG";
                xmlAllEmpresa = NomadQuery("ORG03_EMPRESAS", "1=1", strCampos, "", "", ref hsColumns);
                cSexo = "";
                oi_UO = 0;
                oiEmpresa = 0;
                strTbl = "";
                strWHERE = "";
                oisexo = 0;
                edad = 0;
                C_EMPRESA = "";
                D_EMPRESA_Log = "";
                for (NomadXML nmdEmp = xmlAllEmpresa.FirstChild().FindElement("ROW"); nmdEmp != null; nmdEmp = nmdEmp.Next())
                {
                    try
                    {

                        // este has lo utilizara para el Log al final
                        hsLog = new Hashtable();
                        hsLog.Add("EVALUACIONES", "0,0");
                        hsLog.Add("HORAS", "0,0");
                        hsLog.Add("PERSONAL", "0,0");
                        hsLog.Add("SUELDOS", "0,0");
                        D_EMPRESA_Log = nmdEmp.GetAttrString("D_EMPRESA");
                        oiEmpresa = nmdEmp.GetAttrInt("OI_EMPRESA");

                        nCantPers = xmlAllEmpresa.FirstChild().ChildLength - 1;
                        nLogCount += 1;
                        if (nLogCount > 95) { nLogCount = 95; }
                        objBatch.SetPro(nLogCount);

                        clsDataBase.fncFindPersEmpr(oiEmpresa, sFecha_desde, ref strCampos, ref strTbl, ref strWHERE);
                        xmlAllEmp = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                        nCantPers = xmlAllEmp.FirstChild().ChildLength - 1;
                        objBatch.Log(" Cantidad de personas encontradas para la empresa " + oiEmpresa.ToString() + " : " + nCantPers.ToString());

                        // ESTE IF SE HACE PARA QUE NO ENTRE A PROCESAR DATOS SI SOLO VA A TRABAJAR CON SUELDOS
                        nCantFact=0;

                        if (sFactActualiza.IndexOf("TCO_Fact_Horas") != -1)  { nCantFact += 1;}
                        if (sFactActualiza.IndexOf("TCO_Fact_Personal") != -1)  { nCantFact += 1;}
                        if (sFactActualiza.IndexOf("TCO_Fact_Eval") != -1)  { nCantFact += 1;}

                        if (nCantFact != 0)
                        {
                            NomadBatch.Trace(" Entra en Datos comunes entre Facts");
                            //continue;
                            for (NomadXML nmdEmpChild = xmlAllEmp.FirstChild().FindElement("ROW"); nmdEmpChild != null; nmdEmpChild = nmdEmpChild.Next())
                            {
                                try
                                {
                                    // funcion calcular edad
                                    fecha_tmp = nmdEmpChild.GetAttrDateTime("F_NACIM");
                                    edad = fecha_desde.Year - fecha_tmp.Year;
                                    sLegajoProc = "Legajo: " + nmdEmpChild.GetAttr("E_NUMERO_LEGAJO");

                                    //   -- Toma el puesto correspondiente al período
                                    OI_PERSONAL_EMP = nmdEmpChild.GetAttrInt("OI_PERSONAL_EMP");

                                    if (OI_PERSONAL_EMP == 0)
                                    { continue; }

                                    C_EMPRESA = nmdEmp.GetAttrString("C_EMPRESA");
                                    sLegajoProc += " Empresa :" + C_EMPRESA;
                                    strMsgErrorUser = sLegajoProc + "No existe ";
                                    Maxfec_ing = GetMaxFecIng(OI_PERSONAL_EMP, sFecha_Hasta, ref strCampos, "PER02_PUESTO_PER", ref strWHERE);
                                    // reutiliza la variable fecha_hasta ...es en realidad.. fecha maxima

                                    //             if (Maxfec_ing == "") { continue; } //12/08/2009
                                    clsDataBase.fncFindPuesto(OI_PERSONAL_EMP, Maxfec_ing, ref strCampos, ref strTbl, ref strWHERE);
                                    xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                                    //**PUESTO**
                                    if (xmlTMP.FirstChild().FindElement("ROW") == null)
                                    { oiPuesto = 0; }// No encontro el OI_Puesto
                                    else
                                    {

                                        oiPuesto = xmlTMP.FirstChild().FindElement("ROW").GetAttrInt("OI_PUESTO");
                                    }
                                    strFacProces = "Fact_Datos_Comunes";

                                    hsUpdDB = new Hashtable();
                                    hsUpdDB.Add("OI_EMPRESA", oiEmpresa.ToString());
                                    hsUpdDB.Add("OI_PERSONAL_EMP", OI_PERSONAL_EMP.ToString());
                                    hsUpdDB.Add("OI_ANIOMES", OI_ANIOMES.ToString());
                                    hsUpdDB.Add("OI_SINDICATO", nmdEmpChild.GetAttrString("OI_SINDICATO") == "" ? 0 : Int32.Parse(nmdEmpChild.GetAttrString("OI_SINDICATO")));
                                    hsUpdDB.Add("OI_ESTADO_CIVIL", nmdEmpChild.GetAttrString("OI_ESTADO_CIVIL") == "" ? 0 : Int32.Parse(nmdEmpChild.GetAttrString("OI_ESTADO_CIVIL")));
                                    hsUpdDB.Add("OI_EMPRESA_VINC", nmdEmpChild.GetAttrString("OI_EMPRESA_VINC") == "" ? 99999 : Int32.Parse(nmdEmpChild.GetAttrString("OI_EMPRESA_VINC")));

                                    hsUpdDBType = new Hashtable();
                                    hsUpdDBType.Add("OI_EMPRESA", "INT");
                                    hsUpdDBType.Add("OI_PERSONAL_EMP", "INT");
                                    hsUpdDBType.Add("OI_ANIOMES", "INT");
                                    hsUpdDBType.Add("OI_SINDICATO", "INT");
                                    hsUpdDBType.Add("OI_ESTADO_CIVIL", "INT");
                                    hsUpdDBType.Add("OI_EMPRESA_VINC", "INT");

                                    //**Sexo**
                                    cSexo = nmdEmpChild.GetAttrString("C_SEXO");
                                    strTMP = (string)HasSexo[cSexo];
                                    //oisexo = Int32.Parse(strTMP);
                                    hsUpdDB.Add("OI_SEXO", strTMP);
                                    hsUpdDBType.Add("OI_SEXO", "INT");
                                    // guarda en hastable el nombre del campo como key y el valor que va a tener en el INSERT INTO en DataMart
                                    InsertInHas = GenHashToDB(xmlTMP, strCampos, hsColumns, strWHERE);

                                    strMsgErrorUser = sLegajoProc + "No existe Fecha de ingreso en Posiciones";
                                    Maxfec_ing = GetMaxFecIng(OI_PERSONAL_EMP, sFecha_Hasta, ref strCampos, "PER02_POSIC_PER", ref strWHERE);
                                    clsDataBase.fncFindPos(OI_PERSONAL_EMP, oiPuesto, Maxfec_ing, ref strCampos, ref strTbl, ref strWHERE);
                                    InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                                    strMsgErrorUser = sLegajoProc + "No existe Fecha de ingreso en Categoria para la persona " + OI_PERSONAL_EMP.ToString();
                                    //**CATEGORIA**
                                    Maxfec_ing = GetMaxFecIng(OI_PERSONAL_EMP, sFecha_Hasta, ref strCampos, "PER02_CATEG_PER", ref strWHERE);
                                    clsDataBase.fncFindCat(OI_PERSONAL_EMP, Maxfec_ing, ref strCampos, ref strTbl, ref strWHERE);
                                    InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                                    strMsgErrorUser = sLegajoProc + "No existe Fecha de ingreso asociado al Centro de Costo";
                                    //**CENTRO COSTO**
                                    Maxfec_ing = GetMaxFecIng(OI_PERSONAL_EMP, sFecha_Hasta, ref strCampos, "PER02_CCOSTO_PER", ref strWHERE);
                                    clsDataBase.fncFindCCos(OI_PERSONAL_EMP, Maxfec_ing, ref strCampos, ref strTbl, ref strWHERE);
                                    InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                                    strMsgErrorUser = sLegajoProc + "No existe Fecha de ingreso asociado Tipo de Persona ";
                                    //**TIPOS PERS**
                                    Maxfec_ing = GetMaxFecIng(OI_PERSONAL_EMP, sFecha_Hasta, ref strCampos, "PER02_TIPOSP_PER", ref strWHERE);
                                    clsDataBase.fncFindTPues(OI_PERSONAL_EMP, Maxfec_ing, ref strCampos, ref strTbl, ref strWHERE);
                                    InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                                    //**UO**
                                    strTMP = (string)hsUpdDB["OI_UNIDAD_ORG"];
                                    if (strTMP != null)
                                    {
                                        oi_UO = Int32.Parse(strTMP);
                                    }
                                    // oi_UO = xmlTMP.FirstChild().FindElement("ROW").GetAttrInt("OI_UNIDAD_ORGanizativa");
                                    clsDataBase.fncFindEstOrg(OI_PERSONAL_EMP.ToString(), oi_UO.ToString(), ref strCampos, ref strTbl, ref strWHERE);
                                    InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                                    if ((string)hsUpdDB["OI_ESTRUCTURA"] == "0")
                                    {
                                        hsUpdDB.Remove("OI_ESTRUCTURA");
                                        clsDataBase.fncFindEstOrgEmp(C_EMPRESA + " POS", ref strCampos, ref strTbl, ref strWHERE);
                                        xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                                        if (xmlTMP.FirstChild().FindElement("ROW") == null)
                                        {
                                            strMsgErrorUser = " La empresa " + C_EMPRESA + " no contiene ESTRUCTURA relacionada";

                                            //agregado 12/08/2009
                                            // continue;
                                            // fncLogearHas(" La empresa " + C_EMPRESA + " no contiene ESTRUCTURA relacionada" + strWHERE);
                                            hsUpdDB.Add("OI_ESTRUCTURA", 0);

                                            //fin agregado 12/08/2009
                                        }
                                        else
                                        {
                                            int_tmp = xmlTMP.FirstChild().FindElement("ROW").GetAttrInt("OI_ESTRUCTURA");
                                            strQry = " select OI_ESTRUCTURA , 'OI_ESTRUCTURA' from TCO_ESTRUCTURAS ";
                                            strQry += " WHERE OI_ESTR_PADRE = " + int_tmp.ToString() + " and  C_UNIDAD_ORG ='Sin estructura'";
                                            hasTmp = objDB.GetHashDimension(strQry);
                                            hsUpdDB.Add("OI_ESTRUCTURA", hasTmp["OI_ESTRUCTURA"]);
                                        }
                                    }
                                    strMsgErrorUser = sLegajoProc + " Error al insertar en Evaluaciones ";

                                    if (hsUpdDB.ContainsKey("OI_ESTRUCTURA_F") == false)
                                    { hsUpdDB.Add("OI_ESTRUCTURA_F", 0); }

                                    if (sFactActualiza.IndexOf("TCO_Fact_Eval") != -1)
                                    {
                                        strFacProces = "Fact_Evaluacion";
                                        objBatch.SetMess("Buscando datos para Empresa: " + D_EMPRESA_Log + "(Evaluaciones)");
                                        // ************  Alimenta la FACT EVALUACIONES ****************
                                        REMUN_PERS = fncCalcRemun();
                                        hsUpdDB.Add("REMUN_PERS", REMUN_PERS);
                                        hsUpdDBType.Add("REMUN_PERS", "FLOAT");
                                        hsUpdDBType.Add("CANTIDAD", "INT");
                                        hsUpdDBType.Add("N_RESULT_EVAL", "FLOAT");
                                        hsUpdDBType.Add("N_RESULT_COMP", "FLOAT");
                                        clsDataBase.fncEvaluaciones(OI_PERSONAL_EMP, oi_UO, ref strCampos, ref strTbl, ref strWHERE, sFecha_desde, sFecha_Hasta);
                                        xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                                        if (xmlTMP == null) { continue; }
                                        UpDateToDB(xmlTMP, hsColumns, "EVALUACIONES", strWHERE);
                                        //  ***********************************************************
                                    }
                                    strMsgErrorUser = sLegajoProc + " Error al insertar en Horas ";
                                    if (sFactActualiza.IndexOf("TCO_Fact_Horas") != -1)
                                    {
                                        // ************  Alimenta la FACT HORAS  ****************
                                        objBatch.SetMess("Buscando datos para Empresa: " + D_EMPRESA_Log + " (Horas)");
                                        strFacProces = "Fact_Horas";
                                        clsDataBase.fncFactHoras(OI_PERSONAL_EMP, ref strCampos, ref strTbl, ref strWHERE, sFecha_desde, sFecha_Hasta);
                                        xmlTMP2 = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                                        if (xmlTMP2 == null) { continue; }
                                        clsDataBase.fncFindEstFunc(OI_PERSONAL_EMP.ToString(), OI_CLASE_ORG, ref strCampos, ref strTbl, ref strWHERE);
                                        HasCls = new Hashtable();
                                        // InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref HasCls);
                                        xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref HasCls);

                                        sEstruFun = "NO";
                                        for (NomadXML nmdEstrFunc = xmlTMP.FirstChild().FindElement("ROW"); nmdEstrFunc != null; nmdEstrFunc = nmdEstrFunc.Next())
                                        {
                                            sEstruFun = "SI";
                                            if (nmdEstrFunc == null) { continue; }
                                            oi_est_Func = nmdEstrFunc.GetAttrInt("OI_ESTRUCTURA_F");
                                            if (oi_est_Func == 0) { continue; }
                                            if ((hsUpdDB.ContainsKey("OI_ESTRUCTURA_F") == true))
                                            {
                                                hsUpdDB["OI_ESTRUCTURA_F"] = oi_est_Func;
                                            }
                                            else
                                            {
                                                hsUpdDB.Add("OI_ESTRUCTURA_F", oi_est_Func);
                                            }
                                            if (hsColumns.ContainsKey("OI_ESTRUCTURA_F") == false) { hsColumns.Add("OI_ESTRUCTURA_F", "INT"); }

                                            UpDateToDB(xmlTMP2, hsColumns, "HORAS", strWHERE);
                                        }

                                        if (sEstruFun == "NO")
                                        {
                                            if (hsColumns.ContainsKey("OI_ESTRUCTURA_F") == false)
                                            { hsColumns.Add("OI_ESTRUCTURA_F", "INT"); }
                                            UpDateToDB(xmlTMP2, hsColumns, "HORAS", strWHERE);
                                        }

                                        // ***********************************************************
                                    }

                                    //***********************************************************

                                    if (sFactActualiza.IndexOf("TCO_Fact_Personal") != -1)
                                    {
                                        objBatch.SetMess("Buscando datos para Empresa: " + D_EMPRESA_Log + " (Personal)");
                                        fncFactPersonal(OI_PERSONAL_EMP, fecha_desde, sFecha_desde, sFecha_Hasta, edad);
                                    }

                                }
                                catch (Exception e)
                                {
                                    objBatch.Err(" Error en fnc_Init_UpdFacTables : ");
                                    fncLogearHas(" Error en fnc_Init_UpdFacTables : " + e.Message);
                                }

                            }
                        }// fin del For de Personal/ Horas /Evaluaciones
                        objBatch.SetPro(80);

                        if (sFactActualiza.IndexOf("TCO_fact_sueldos") != -1)
                        {
                            NomadBatch.Trace(" Entra en TCO_fact_sueldos para Empresa: " + D_EMPRESA_Log);
                            objBatch.SetMess("Buscando datos para Empresa: " + D_EMPRESA_Log + " (Sueldos)");
                            fncFactSueldos(fecha_desde, fecha_hasta, sFecha_desde, HasSexo, oiEmpresa);
                        }

                        //**//
                        fncLogueaCantidad(oiEmpresa.ToString());

                    }

                    catch (Exception e)
                    {
                        objBatch.Err(" Error en fnc_Init_UpdFacTables : ");
                        fncLogearHas(" Error en fnc_Init_UpdFacTables : " + e.Message);

                    }

                }
                objBatch.SetPro(100);
                if (objEjec.c_estado == "E") { objEjec.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
                objEjec.f_fin_ejec = DateTime.Now;
                fncSaveDDO();

            }

            catch (Exception e)
            {
                objBatch.Err(" Error en fnc_Init_UpdFacTables : ");
                fncLogearHas(" Error en fnc_Init_UpdFacTables : " + e.Message);
            }

        }

        private void fncLogueaCantidad(string sOI_EMPRESA)
        {
            Array arrTmp;
            string tmpArr;
            string tmpIns;
            string tmpUpd;
            int nCantProc;
            nCantProc = 0;
            tmpIns = "";
            tmpUpd = "";

            foreach (DictionaryEntry item in hsLog)
            {
                if ((string)item.Value != "0,0")
                {
                    tmpArr = (string)item.Value;
                    arrTmp = tmpArr.Split(',');
                    tmpIns = ((string[])(arrTmp))[0];
                    tmpUpd = ((string[])(arrTmp))[1];

                    objBatch.Log(" Insertados en " + item.Key + " Cantidad :" + tmpIns);
                    objBatch.Log(" Modificados en " + item.Key + " Cantidad :" + tmpUpd);
                    nCantProc = Int32.Parse(tmpIns) + Int32.Parse(tmpUpd);

                    objEjecDet = new NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE();
                    objEjecDet.c_estado = "S";
                    objEjecDet.c_nom_tabla = item.Key.ToString() + " Emp:" + sOI_EMPRESA;
                    fnc_AddLogDetail(objEjecDet, item.Key.ToString(), "S", nCantProc, 0);
                }

            }

        }

        private void fncUpdEstrucProy()
        {
            string strSql;
            int nCantReg;
            string strCampos;
            string strTbl;
            string strWHERE;
            string strCamposMedida;
            string strSql_ins;
            int rtaQry;
            rtaQry = 0;
            Hashtable hsColumns;
            hsColumns = new Hashtable();
            NomadXML xmlTMP;
            string strCamposUnicos;
            hsUpdDB.Clear();
            hsUpdDBType.Clear();
            strFacProces = "Fact_Datos_PROY";
            strCampos = "";
            strTbl = "";
            strWHERE = "";
            strCamposMedida = "";
            strCamposUnicos = "";
            sFncEjecutada = "fncUpdEstrucProy";
            objEjecDet = new NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE();
            objEjecDet.c_estado = "E";
            objEjecDet.c_nom_tabla = "TCO_ESTRUC_PROY_CLOSURE";
            objEjecDet.descr = "elimina TCO_ESTRUC_PROY_CLOSURE  ";
            strMsgErrorUser = sLegajoProc + " Error al actualizar Estructuras en DataMart ";
            // limpia Estructuras_Clousure
            strSql = " DELETE FROM TCO_Estruc_Proy_closure ";
            fncLogeaDB("TCO_ESTRUC_PROY_CLOSURE", strSql, strSql, ref rtaQry);

            strSql = " INSERT INTO TCO_Estruc_Proy_closure (OI_PROYECTO_PARENT, OI_PROYECTO, DISTANCIA) ";
            strSql += " select OI_PROYECTO, OI_PROYECTO, 0 from TCO_ESTRUC_PROYECTOS ";
            fncLogeaDB("TCO_Estruc_Proy_closure_INS", strSql, strSql, ref rtaQry);

            strSql_ins = " ";
            clsDataBase.fncCamposFactEstProy(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
            strSql_ins = " INSERT INTO TCO_ESTRUC_PROY_CLOSURE ( " + strCamposUnicos + "," + strCamposMedida + " )";
            strSql_ins += " SELECT  ";
            int mVar;
            mVar = 0;
            // para cada nivel agrega sus hijos
            while (rtaQry != 0)
            {
                mVar += 1;
                hsColumns = new Hashtable();
                strSql = " select max(DISTANCIA) as DISTANCIA, 'distancia' from TCO_ESTRUC_PROY_CLOSURE ";
                hsColumns = objDB.GetHashDimension(strSql);

                strSql = (string)hsColumns["DISTANCIA"];
                rtaQry = Int32.Parse(strSql);

                clsDataBase.fncFindEstProyRecursive(rtaQry, ref strCampos, ref strTbl, ref strWHERE);
                strSql = strSql_ins + strCampos + " FROM " + strTbl + " WHERE " + strWHERE;

                fncLogeaDB("TCO_ESTRUC_PROY_CLOSURE_" + mVar.ToString(), strSql, strSql, ref rtaQry);

            }

        }

        //---------------------------------------------------------------------------------------------

        private void fncUpdEstructuras()
        {
            string strSql;
            int nCantReg;
            string strCampos;
            string strTbl;
            string strWHERE;
            string strCamposMedida;
            string strSql_ins;
            int rtaQry;

            try
            {

                rtaQry = 0;
                Hashtable hsColumns;
                hsColumns = new Hashtable();
                NomadXML xmlTMP;
                string strCamposUnicos;
                hsUpdDB.Clear();
                hsUpdDBType.Clear();
                strFacProces = "Fact_Datos_Comunes";
                strCampos = "";
                strTbl = "";
                strWHERE = "";
                strCamposMedida = "";
                strCamposUnicos = "";
                sFncEjecutada = "fncUpdEstructuras";
                objEjecDet = new NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE();
                objEjecDet.c_estado = "E";
                objEjecDet.c_nom_tabla = "TCO_ESTRUCTURAS_CLOSURE";
                objEjecDet.descr = "elimina TCO_ESTRUCTURAS_CLOSURE  ";
                strMsgErrorUser = sLegajoProc + " Error al actualizar Estructuras en DataMart ";
                // limpia Estructuras_Clousure
                strSql = " DELETE FROM TCO_ESTRUCTURAS_CLOSURE ";
                fncLogeaDB("TCO_ESTRUCTURAS_CLOSURE", strSql, strSql, ref rtaQry);

                // elimina los sins Estructuras para agregarles los padres de sin estructura actualizados
                strSql = " DELETE FROM TCO_ESTRUCTURAS where C_UNIDAD_ORG ='Sin estructura'  ";
                fncLogeaDB("TCO_ESTRUCTURAS_DEL", strSql, strSql, ref rtaQry);

                // deja un texto identificable en los nodos padres de Estructuras para eliminar los que no corresponden
                strSql = "UPDATE TCO_ESTRUCTURAS SET D_UNIDAD_ORG = 'BORRAR' from TCO_ESTRUCTURAS where OI_ESTR_PADRE = 0";
                fncLogeaDB("TCO_ESTRUCTURAS_UpdElim", strSql, strSql, ref rtaQry);

                // coloca la descripcion en Estructuras igual a la de Organigrama
                strSql = " UPDATE TCO_ESTRUCTURAS ";
                strSql += " SET D_UNIDAD_ORG = D_CLASE_ORG ";
                strSql += " from TCO_ESTRUCTURAS , ";
                strSql += " TCO_CLASESORGANIZATIVAS";
                strSql += " where TCO_ESTRUCTURAS.OI_ESTRUCTURA =TCO_CLASESORGANIZATIVAS.OI_ESTRUCTURA_ORG";
                strSql += " and TCO_CLASESORGANIZATIVAS.L_AUTOMATICA=1 and TCO_CLASESORGANIZATIVAS.C_CLASE_ORG like '% POS'";
                fncLogeaDB("TCO_ESTRUCTURAS_UpdName", "coloca la descripcion en Estructuras igual a la de Organigrama", strSql, ref rtaQry);

                // Elimina los padres que no corresponden con Organigrama
                strSql = "DELETE TCO_ESTRUCTURAS  WHERE D_UNIDAD_ORG = 'borrar' ";
                fncLogeaDB("TCO_ESTRUCTURAS_UpdElim2", "Elimina los padres que no corresponden con Organigrama", strSql, ref rtaQry);

                // agrega los padres de sin estructura actualizados
                strSql = " INSERT INTO TCO_ESTRUCTURAS  ";
                strSql += " select    ( 9000 + OI_ESTRUCTURA_ORG) as OI_ESTRUCTURA  ";
                strSql += "     ,OI_ESTRUCTURA_ORG as OI_ESTR_PADRE    ";
                strSql += " , 0 as OI_UNIDAD_ORG ";
                strSql += " ,'Sin estructura' as C_UNIDAD_ORG  ";
                strSql += " ,'Sin estructura' as D_UNIDAD_ORG  ";
                strSql += " from TCO_CLASESORGANIZATIVAS where C_CLASE_ORG like '% POS'";
                fncLogeaDB("TCO_ESTRUCTURAS_INS", "agrega a TCO_ESTRUCTURAS los padres de sin estructura actualizados", strSql, ref rtaQry);

                // Inserta el primer nivel de estructuras
                strSql = " INSERT INTO TCO_ESTRUCTURAS_CLOSURE (OI_ESTR_PADRE, OI_ESTRUCTURA, DISTANCIA) ";
                strSql += " SELECT OI_ESTRUCTURA, OI_ESTRUCTURA, 0 FROM TCO_ESTRUCTURAS ";
                //rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strSql);
                fncLogeaDB("TCO_ESTRUCTURAS_CLOSURE_INS", "Inserta el primer nivel de estructuras", strSql, ref rtaQry);

                strSql_ins = " ";
                clsDataBase.fncCamposFactEstruct(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                strSql_ins = " INSERT INTO TCO_ESTRUCTURAS_CLOSURE ( " + strCamposUnicos + "," + strCamposMedida + " )";
                strSql_ins += " SELECT  ";
                int mVar;
                mVar = 0;
                // para cada nivel agrega sus hijos
                while (rtaQry != 0)
                {
                    mVar += 1;
                    hsColumns = new Hashtable();
                    strSql = " SELECT MAX(DISTANCIA) AS DISTANCIA, 'DISTANCIA' FROM TCO_ESTRUCTURAS_CLOSURE ";
                    hsColumns = objDB.GetHashDimension(strSql);

                    strSql = (string)hsColumns["DISTANCIA"];
                    rtaQry = Int32.Parse(strSql);

                    clsDataBase.fncFindEstructRecursive(rtaQry, ref strCampos, ref strTbl, ref strWHERE);
                    strSql = strSql_ins + strCampos + " FROM " + strTbl + " WHERE " + strWHERE;

                    fncLogeaDB("TCO_ESTRUCTURAS_CLOSURE_" + mVar.ToString(), "Agregando a TCO_ESTRUCTURAS_CLOSURE nivel '" + mVar.ToString() + "'", strSql, ref rtaQry);

                }

            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fncUpdEstructuras : ");
                fncLogearHas(" Error en fncUpdEstructuras : " + e.Message);

            }

        }

        //---------------------------------------------------------------------------------------------

        private void fncUpdEstructurasFunc()
        {
            string strSql;
            int nCantReg;
            string strCampos;
            string strTbl;
            string strWHERE;
            string strCamposMedida;
            string strSql_ins;
            int rtaQry;

            try
            {

                rtaQry = 0;
                Hashtable hsColumns;
                hsColumns = new Hashtable();
                NomadXML xmlTMP;
                string strCamposUnicos;
                hsUpdDB.Clear();
                hsUpdDBType.Clear();
                strFacProces = "Fact_Datos_Comunes";
                strCampos = "";
                strTbl = "";
                strWHERE = "";
                strCamposMedida = "";
                strCamposUnicos = "";
                sFncEjecutada = "fncUpdEstructuras_F";
                objEjecDet = new NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE();
                objEjecDet.c_estado = "E";
                objEjecDet.c_nom_tabla = "TCO_ESTRUC_FUNC_CLOSURE";
                objEjecDet.descr = "elimina TCO_ESTRUC_FUNC_CLOSURE  ";
                strMsgErrorUser = sLegajoProc + " Error al actualizar Estructuras en DataMart ";
                // limpia Estructuras_Clousure
                strSql = " DELETE FROM TCO_ESTRUC_FUNC_CLOSURE ";
                fncLogeaDB("TCO_ESTRUC_FUNC_CLOSURE", strSql, strSql, ref rtaQry);

                // elimina los sins Estructuras para agregarles los padres de sin estructura actualizados
                strSql = " DELETE FROM TCO_ESTRUC_FUNC  ";
                fncLogeaDB("TCO_ESTRUC_FUNC_DEL", strSql, strSql, ref rtaQry);

                // paso 1 , asigno las clases que no son de posicion
                strSql = "  INSERT INTO TCO_ESTRUC_FUNC  (OI_ESTRUCTURA_F  ,OI_ESTR_PADRE,OI_UNIDAD_ORG,C_UNIDAD_ORG ,D_UNIDAD_ORG, L_FUNC_FICTICIO )";
                strSql += " SELECT 90000 +  OI_ESTRUCTURA_ORG AS OI_ESTRUCTURA ,0 AS PADRE, 0 , C_CLASE_ORG, D_CLASE_ORG,1 ";
                strSql += " FROM TCO_CLASESORGANIZATIVAS WHERE L_AUTOMATICA <> 1";  //IMPLICA QUE NO ES DE POSCION
                fncLogeaDB("TCO_ESTRUC_FUNC_INS", "TCO_ESTRUC_FUNC Ins las clases que no son de posicion", strSql, ref rtaQry);

                // Inserta el primer nivel de estructuras
                // paso 2 , asigno las estructuras cabeceras como hijas de las clases del paso 1
                strSql = " INSERT INTO TCO_ESTRUC_FUNC  (OI_ESTRUCTURA_F  ,OI_ESTR_PADRE,OI_UNIDAD_ORG,C_UNIDAD_ORG ,D_UNIDAD_ORG, L_FUNC_FICTICIO )";
                strSql += " SELECT 9000 +  OI_ESTRUCTURA_ORG AS OI_ESTRUCTURA ,90000 +  OI_ESTRUCTURA_ORG AS PADRE, 0 ,TCO_ESTRUCTURAS.C_UNIDAD_ORG ,";
                strSql += "  TCO_ESTRUCTURAS.D_UNIDAD_ORG,1  ";
                strSql += "  FROM TCO_CLASESORGANIZATIVAS,TCO_ESTRUCTURAS ";
                strSql += "  WHERE L_AUTOMATICA <> 1 "; // IMPLICA QUE NO ES DE POSCION
                strSql += "  AND  TCO_CLASESORGANIZATIVAS.OI_ESTRUCTURA_ORG =  TCO_ESTRUCTURAS.OI_ESTRUCTURA  ";
                fncLogeaDB("TCO_ESTRUC_FUNC_INS2", "Inserta las estructuras cabeceras como hijas de las clases del paso 1", strSql, ref rtaQry);

                // paso 3 , asigno los hijos a la cabecera
                strSql = " INSERT INTO TCO_ESTRUC_FUNC  (OI_ESTRUCTURA_F  ,OI_ESTR_PADRE,OI_UNIDAD_ORG,C_UNIDAD_ORG ,D_UNIDAD_ORG, L_FUNC_FICTICIO )";
                strSql += " SELECT TCO_ESTRUCTURAS.OI_ESTRUCTURA,  TCO_ESTRUCTURAS.OI_ESTR_PADRE + 9000,                                            ";
                strSql += " TCO_ESTRUCTURAS.OI_UNIDAD_ORG, TCO_ESTRUCTURAS.C_UNIDAD_ORG ,                                                           ";
                strSql += " TCO_ESTRUCTURAS.D_UNIDAD_ORG,1                                                                                          ";
                strSql += "     FROM TCO_ESTRUC_FUNC ,TCO_ESTRUCTURAS                                                                               ";
                strSql += "   WHERE                                                                                                                 ";
                strSql += "   TCO_ESTRUC_FUNC.OI_ESTRUCTURA_F = TCO_ESTRUCTURAS.OI_ESTR_PADRE + 9000                                                ";
                fncLogeaDB("TCO_ESTRUC_FUNC_INS3", "INSERTA los hijos a la cabecera", strSql, ref rtaQry);

                int mVar;
                mVar = 4;
                // para cada nivel agrega sus hijos
                while (rtaQry != 0)
                {
                    mVar += 1;

                    strSql = " INSERT INTO TCO_ESTRUC_FUNC  ";
                    strSql += "  (OI_ESTRUCTURA_F  ,OI_ESTR_PADRE,OI_UNIDAD_ORG,C_UNIDAD_ORG ,D_UNIDAD_ORG, L_FUNC_FICTICIO )";
                    strSql += "  SELECT TCO_ESTRUCTURAS.OI_ESTRUCTURA,  TCO_ESTRUCTURAS.OI_ESTR_PADRE,   TCO_ESTRUCTURAS.OI_UNIDAD_ORG";
                    strSql += "  , TCO_ESTRUCTURAS.C_UNIDAD_ORG , TCO_ESTRUCTURAS.D_UNIDAD_ORG,1 ";
                    strSql += "  FROM TCO_ESTRUC_FUNC ,TCO_ESTRUCTURAS                         ";
                    strSql += "  WHERE TCO_ESTRUC_FUNC.OI_ESTRUCTURA_F = TCO_ESTRUCTURAS.OI_ESTR_PADRE ";
                    strSql += "  AND TCO_ESTRUCTURAS.OI_ESTRUCTURA NOT IN  (SELECT OI_ESTRUCTURA_F FROM TCO_ESTRUC_FUNC WHERE  L_FUNC_FICTICIO = 1) ";
                    strSql += "  AND TCO_ESTRUCTURAS.OI_ESTR_PADRE >1  ";
                    fncLogeaDB("TCO_ESTRUC_FUNC_" + mVar.ToString(), "TCO_ESTRUC_FUNC agrega sus hijos al nivel '" + mVar.ToString() + "'", strSql, ref rtaQry);
                    // rtaQry = Int32.Parse(rtaQry);
                }

                strSql = " INSERT INTO TCO_ESTRUC_FUNC_CLOSURE (OI_ESTR_PADRE, OI_ESTRUCTURA_F, DISTANCIA)";
                strSql += "  SELECT OI_ESTRUCTURA_F, OI_ESTRUCTURA_F, 0                                     ";
                strSql += "  FROM TCO_ESTRUC_FUNC                                                           ";
                fncLogeaDB("TCO_ESTRUC_FUNC_CLOSURE_INS", "INS en TCO_ESTRUC_FUNC OI_ESTRUCTURA_F, OI_ESTRUCTURA_F, 0", strSql, ref rtaQry);

                mVar = 0;
                // para cada nivel agrega sus hijos
                while (rtaQry != 0)
                {
                    mVar += 1;
                    strSql = "   INSERT INTO TCO_ESTRUC_FUNC_CLOSURE (OI_ESTR_PADRE, OI_ESTRUCTURA_F, DISTANCIA)";
                    strSql += "    SELECT C.OI_ESTR_PADRE OI_ESTR_PADRE, E.OI_ESTRUCTURA_F, C.DISTANCIA + 1       ";
                    strSql += "    FROM TCO_ESTRUC_FUNC_CLOSURE C, TCO_ESTRUC_FUNC E                              ";
                    strSql += "    WHERE E.OI_ESTR_PADRE = C.OI_ESTRUCTURA_F                                      ";
                    strSql += "    AND C.DISTANCIA = (SELECT MAX(DISTANCIA) FROM TCO_ESTRUC_FUNC_CLOSURE)         ";
                    fncLogeaDB("TCO_ESTRUC_FUNC_CLOSURE_" + mVar.ToString(), "TCO_ESTRUC_FUNC_CLOSURE agrega sus hijos al nivel '" + mVar.ToString() + "'", strSql, ref rtaQry);
                    //rtaQry = Int32.Parse(strSql);

                }

            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fncUpdEstructuras : ");
                fncLogearHas(" Error en fncUpdEstructuras : " + e.Message);

            }

        }

        //---------------------------------------------------------------------------------------------

        private void fncFactPersonal(int OI_PERSONAL_EMP, DateTime d_fecha_desde, string fecha_desde, string fecha_hasta, int edad)
        {
            string strCampos;
            string strTbl;
            string strWHERE;
            string lst_TpHora;
            string lst_Lic;
            NomadXML xmlTMP;
            Hashtable hsColumns;
            DateTime fecha_tmp;
            DateTime fecha_ant_tmp;
            TimeSpan dif_Fechas;
            int int_tmp;
            bool InsertInHas;
            strCampos = "";
            strTbl = "";
            strWHERE = "";
            lst_TpHora = "";
            lst_Lic = "";
            double REMUN_PERS;
            try
            {

                sFncEjecutada = "fncFactPersonal";
                if (objEjec_Pers.f_Inicio_ejecNull == true) { objEjec_Pers.f_Inicio_ejec = DateTime.Now; }
                strFacProces = "Fact_Personal";
                hsColumns = new Hashtable();
                strMsgErrorUser = sLegajoProc + " Error en Tipos de horas ";
                clsDataBase.fncGetHasTiposHoras(ref strCampos, ref strTbl, ref strWHERE);
                GetlstTiposHora(ref lst_TpHora, ref lst_Lic);

                strMsgErrorUser = sLegajoProc + " Error en Horas de Ausencia ";
                clsDataBase.fncFindHsAusencia(OI_PERSONAL_EMP, fecha_desde, fecha_hasta, ref  strCampos, ref  strTbl, ref  strWHERE, lst_TpHora);
                InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);// carga en HAS horaAusencia

                strMsgErrorUser = sLegajoProc + " Error en Horas esperadas  ";
                clsDataBase.fncFindHsEsperado(OI_PERSONAL_EMP, fecha_desde, fecha_hasta, ref  strCampos, ref  strTbl, ref  strWHERE, lst_TpHora);
                InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);// carga en HAS Esperado

                strMsgErrorUser = sLegajoProc + " Error en Horas de Capacitacion ";
                clsDataBase.fncFindHsCapacitacion(OI_PERSONAL_EMP, fecha_desde, fecha_hasta, ref  strCampos, ref  strTbl, ref  strWHERE);
                xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                GetDifHor(xmlTMP, "F_FECHA_HORA_INI", "F_FECHA_HORA_FIN", "HSCAPACITACION");

                strMsgErrorUser = sLegajoProc + " Error en el cálculo de la antiguedad ";
                clsDataBase.fncFindAntiguedad(OI_PERSONAL_EMP, ref strCampos, ref strTbl, ref strWHERE);
                xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                fecha_tmp = xmlTMP.FirstChild().FindElement("ROW").GetAttrDateTime("F_ANTIGUEDAD_REC");
                if (fecha_tmp.Year == 1)
                {
                    fecha_tmp = xmlTMP.FirstChild().FindElement("ROW").GetAttrDateTime("F_INGRESO");
                }

                fecha_ant_tmp = new DateTime(fecha_tmp.Year, fecha_tmp.Month, 1);
                dif_Fechas = d_fecha_desde - fecha_ant_tmp;
                int_tmp = Convert.ToInt32(dif_Fechas.TotalDays) / 365;
                hsUpdDB.Add("ANTIGUEDAD", int_tmp);
                hsUpdDBType.Add("ANTIGUEDAD", "INT");
                strMsgErrorUser = sLegajoProc + " Error en la fecha de ingreso ";
                clsDataBase.fncFindIngreso(OI_PERSONAL_EMP, fecha_desde, fecha_hasta, ref  strCampos, ref  strTbl, ref  strWHERE, lst_TpHora);
                xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                int_tmp = 0;
                if (xmlTMP.FirstChild().FindElement("ROW") != null)
                {
                    int_tmp = 1;
                }
                hsUpdDB.Add("INGRESO", int_tmp);
                hsUpdDBType.Add("INGRESO", "INT");
                strMsgErrorUser = sLegajoProc + " Error en la fecha de engreso ";
                clsDataBase.fncFindEgreso(OI_PERSONAL_EMP, fecha_desde, fecha_hasta, ref  strCampos, ref  strTbl, ref  strWHERE, lst_TpHora);
                xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                int_tmp = 0;
                if (xmlTMP.FirstChild().FindElement("ROW") == null)
                {
                    hsUpdDB.Add("EGRESO", 0);
                    hsUpdDB.Add("OI_MOTIVO_EG_PER", 0);
                }
                else
                {
                    int_tmp = 0;
                    if (xmlTMP.FirstChild().FindElement("ROW").GetAttr("F_EGRESO") != "")
                    {
                        int_tmp = 1;
                    }
                    hsUpdDB.Add("EGRESO", int_tmp);

                    int_tmp = 0;
                    if (xmlTMP.FirstChild().FindElement("ROW").GetAttr("OI_MOTIVO_EG_PER") != "")
                    {
                        int_tmp = 1;
                    }
                    hsUpdDB.Add("OI_MOTIVO_EG_PER", int_tmp);
                }

                hsUpdDB.Add("CANTIDAD_PERS", 1);
                hsUpdDB.Add("EDAD_PROMEDIO", edad);
                hsUpdDB.Add("OI_EDAD", edad);

                hsUpdDBType.Add("CANTIDAD_PERS", "INT");
                hsUpdDBType.Add("EDAD_PROMEDIO", "INT");
                hsUpdDBType.Add("OI_EDAD", "INT");
                hsUpdDBType.Add("EGRESO", "INT");
                hsUpdDBType.Add("OI_MOTIVO_EG_PER", "INT");

                REMUN_PERS = fncCalcRemunFactPers();
                if (hsUpdDB.ContainsKey("REMUN_PERS"))
                {
                    hsUpdDB["REMUN_PERS"] = REMUN_PERS;
                }
                else
                {
                    hsUpdDB.Add("REMUN_PERS", REMUN_PERS);
                }
                xmlTMP = null;
                // modificacion 29/09/2009
                if (hsUpdDBType.ContainsKey("REMUN_PERS") == false)
                {
                    hsUpdDBType.Add("REMUN_PERS", "FLOAT");
                }

                if (objEjec_Pers.f_Inicio_ejecNull == true) { objEjec_Pers.f_Inicio_ejec = DateTime.Now; }

                if (hsUpdDB.Count > 0)
                {
                    UpDateToDB(xmlTMP, hsColumns, "PERSONAL", "");
                }

            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fnc_FactPersonal : ");
                fncLogearHas(" Error en fnc_FactPersonal : " + e.Message);

            }
        }

        //==============================================================================================

        //---------------------------------------------------------------------------------------------

        private void fncFactSueldos(DateTime fecha_desde, DateTime fecha_hasta, string sFecha_desde, Hashtable HasSexo, int oi_empre)
        {
            string strCampos;
            string strTbl;
            string strWHERE;
            NomadXML xmlTMP;

            Hashtable hsColumns;

            string cSexo;
            string strTMP;
            string strEmpresas;
            strEmpresas = "0";
            string strQry;
            string strMsgErr;
            int rtaQry;
            strTMP = "";
            cSexo = "";
            strCampos = "";
            strTbl = "";
            strWHERE = "";
            hsUpdDB = new Hashtable();
            Hashtable hasTmp;
            int int_tmp;
            hsUpdDBType = new Hashtable();
            hsColumns = new Hashtable();
            NomadXML xmlSueldosConf;
            sFncEjecutada = "fncFactSueldos";

            string sAnioMes;
            sAnioMes = C_ANIOMES.Replace("/", "");
            sAnioMes = sAnioMes.Substring(2, sAnioMes.Length - 2) + sAnioMes.Substring(0, 2);

            clsDataBase.fncFirstSQLSueldosCon(sAnioMes, ref strCampos, ref strTbl, ref strWHERE);
            xmlSueldosConf = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

            try
            {

                if (objEjec_Sueldo.f_Inicio_ejecNull == true) { objEjec_Sueldo.f_Inicio_ejec = DateTime.Now; }
                strFacProces = "Fact_Sueldos";

                hsColumns = new Hashtable();
                clsDataBase.fncFirstSQLSueldos(sAnioMes, ref strCampos, ref strTbl, ref strWHERE, oi_empre);
                xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                string nLegajo = "";

                for (NomadXML nmdXML = xmlTMP.FirstChild().FindElement("ROW"); nmdXML != null; nmdXML = nmdXML.Next())
                {
                    try
                    {
                        strMsgErr = "Legajo:" + nLegajo.ToString() + " empresa: " + nmdXML.GetAttr("C_EMPRESA");
                        strMsgErrorUser = strMsgErr + " Error al grabar en Sueldos ";
                        if (nmdXML.GetAttrString("OI_EMPRESA") == "") { continue; }
                        strEmpresas += "," + nmdXML.GetAttrString("OI_EMPRESA");
                        if (nLegajo != nmdXML.GetAttrString("E_NUMERO_LEGAJO"))
                        {
                            if (hsUpdDB.Keys.Count > 0)
                            {
                                strMsgErrorUser = strMsgErr + " Error al grabar en Sueldos ";
                                //hsPers.Add("oi_per_emp_ddo",(string) nmdXML.GetAttrString("oi_per_emp_ddo"));
                                xmlTMP = null;
                                hsUpdDB.Add("OI_PER_EMP_DDO", nmdXML.GetAttrString("OI_PER_EMP_DDO"));

                                hsUpdDB.Add("OI_EMPRESA", nmdXML.GetAttrString("OI_EMPRESA"));
                                hsUpdDB.Add("OI_ANIOMES", OI_ANIOMES.ToString());
                                if (xmlSueldosConf != null)
                                {
                                    fncSueldosCOnf(xmlSueldosConf);
                                }
                                /* 11/07/2009
                                                                hsUpdDBType.Add("OI_EMPRESA", "INT");
                                                                hsUpdDBType.Add("OI_ANIOMES", "INT");
                                                                */
                                fnSueldosColumns(ref   hsColumns); // carga los tipos que flatan por si el hash no los tiene

                                UpDateToDB(xmlTMP, hsColumns, "SUELDOS", "NROLEGAJO " + nLegajo.ToString());
                            }
                            else
                            {
                                if (nLegajo != "")
                                { fncLogearHas("fncFactSueldos: Legajo error anterior " + nLegajo.ToString()); }
                            }

                            nLegajo = nmdXML.GetAttrString("E_NUMERO_LEGAJO");
                            hsUpdDB = new Hashtable();
                            // 11/07/2009                            hsUpdDBType = new Hashtable();
                            strMsgErrorUser = strMsgErr + " Error en el puesto ";
                            clsDataBase.fncFindPuesto_Sueldo(nmdXML.GetAttrInt("OI_EMPRESA"), nmdXML.GetAttrString("C_PUESTO_ULT"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            strMsgErrorUser = strMsgErr + " Error en el puesto/posicion ";
                            clsDataBase.fncFindPos_Sueldo((string)hsUpdDB["OI_PUESTO"], nmdXML.GetAttrString("C_POSICION_ULT"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            strMsgErrorUser = strMsgErr + " Error en el convenio";
                            clsDataBase.fncFindConv_Sueldo(nmdXML.GetAttrString("C_CONVENIO"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            strMsgErrorUser = strMsgErr + " Error en la categoría";
                            clsDataBase.fncFindCateg_Sueldo((string)hsUpdDB["OI_CONVENIO"], nmdXML.GetAttrString("C_CATEGORIA"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            strMsgErrorUser = strMsgErr + " Error en el Centro de Costo";
                            clsDataBase.fncFindCC_Sueldo(nmdXML.GetAttrString("C_CENTRO_COSTO_ULT"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            strMsgErrorUser = strMsgErr + " Error en el Tipo de Personal ";
                            clsDataBase.fncFindTper_Sueldo(nmdXML.GetAttrString("C_TIPO_PERSONAL"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            strMsgErrorUser = strMsgErr + " Error en el Estado Civil";
                            clsDataBase.fncFindECivil_Sueldo(nmdXML.GetAttrString("C_ESTADO_CIVIL"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            strMsgErrorUser = strMsgErr + " Error en el sindicato";
                            clsDataBase.fncFindSind_Sueldo(nmdXML.GetAttrString("C_SINDICATO"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            //**Sexo**
                            cSexo = nmdXML.GetAttrString("C_SEXO");
                            strTMP = (string)HasSexo[cSexo];
                            hsUpdDB.Add("OI_SEXO", strTMP);
                            //11/07/2009                            hsUpdDBType.Add("OI_SEXO", "INT");
                            clsDataBase.fncFindPerEmp_Sueldo(nmdXML.GetAttrString("E_NUMERO_LEGAJO"), nmdXML.GetAttrInt("OI_EMPRESA"), ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);

                            strMsgErrorUser = strMsgErr + " Error en la Estructura Organizativa";
                            clsDataBase.fncFindEstOrg((string)hsUpdDB["OI_PERSONAL_EMP"], (string)hsUpdDB["OI_UNIDAD_ORG_PER"], ref  strCampos, ref  strTbl, ref  strWHERE);
                            InsertDataHash(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                            if ((string)hsUpdDB["OI_ESTRUCTURA"] == "0")
                            {
                                hsUpdDB.Remove("OI_ESTRUCTURA");
                                clsDataBase.fncFindEstOrgEmp(nmdXML.GetAttr("C_EMPRESA") + " POS", ref strCampos, ref strTbl, ref strWHERE);
                                xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hsColumns);
                                if (xmlTMP.FirstChild().FindElement("ROW") == null)
                                {
                                    strMsgErrorUser = " La empresa " + nmdXML.GetAttr("C_EMPRESA") + " no contiene ESTRUCTURA relacionada";
                                    fncLogearHas(" La empresa " + nmdXML.GetAttr("C_EMPRESA") + " no contiene ESTRUCTURA relacionada" + strWHERE);
                                    continue;
                                }
                                int_tmp = xmlTMP.FirstChild().FindElement("ROW").GetAttrInt("OI_ESTRUCTURA");
                                strQry = " select OI_ESTRUCTURA , 'OI_ESTRUCTURA' from TCO_ESTRUCTURAS ";
                                strQry += " WHERE OI_ESTR_PADRE = " + int_tmp.ToString() + " and  C_UNIDAD_ORG ='Sin estructura'";
                                hasTmp = objDB.GetHashDimension(strQry);
                                hsUpdDB.Add("OI_ESTRUCTURA", hasTmp["OI_ESTRUCTURA"]);

                            }
                            hsUpdDB.Add("N_TOTPAG", nmdXML.GetAttrDouble("N_TOTPAG"));
                            hsUpdDB.Add("N_TOTSRET", nmdXML.GetAttrDouble("N_TOTSRET"));
                            hsUpdDB.Add("N_TOTNSRET", nmdXML.GetAttrDouble("N_TOTNSRET"));
                            hsUpdDB.Add("N_TOTASIG", nmdXML.GetAttrDouble("N_TOTASIG"));
                            hsUpdDB.Add("N_TOTSAC", nmdXML.GetAttrDouble("N_TOTSAC"));
                            hsUpdDB.Add("N_RETEN", nmdXML.GetAttrDouble("N_RETEN"));
                            hsUpdDB.Add("N_TOTCONT", nmdXML.GetAttrDouble("N_TOTCONT"));
                            hsUpdDB.Add("N_TOTTICKET", nmdXML.GetAttrDouble("N_TOTTICKET"));
                            hsUpdDB.Add("N_TOTDESC", nmdXML.GetAttrDouble("N_TOTDESC"));
                            hsUpdDB.Add("N_TOTANT", nmdXML.GetAttrDouble("N_TOTANT"));
                            hsUpdDB.Add("CANTIDAD_PERS", 1);

                        }
                        else
                        {

                            hsUpdDB["N_TOTPAG"] = (double)hsUpdDB["N_TOTPAG"] + nmdXML.GetAttrDouble("N_TOTPAG");
                            hsUpdDB["N_TOTSRET"] = (double)hsUpdDB["N_TOTSRET"] + nmdXML.GetAttrDouble("N_TOTSRET");
                            hsUpdDB["N_TOTNSRET"] = (double)hsUpdDB["N_TOTNSRET"] + nmdXML.GetAttrDouble("N_TOTNSRET");
                            hsUpdDB["N_TOTASIG"] = (double)hsUpdDB["N_TOTASIG"] + nmdXML.GetAttrDouble("N_TOTASIG");
                            hsUpdDB["N_TOTSAC"] = (double)hsUpdDB["N_TOTSAC"] + nmdXML.GetAttrDouble("N_TOTSAC");
                            hsUpdDB["N_RETEN"] = (double)hsUpdDB["N_RETEN"] + nmdXML.GetAttrDouble("N_RETEN");
                            hsUpdDB["N_TOTCONT"] = (double)hsUpdDB["N_TOTCONT"] + nmdXML.GetAttrDouble("N_TOTCONT");
                            hsUpdDB["N_TOTTICKET"] = (double)hsUpdDB["N_TOTTICKET"] + nmdXML.GetAttrDouble("N_TOTTICKET");
                            hsUpdDB["N_TOTDESC"] = (double)hsUpdDB["N_TOTDESC"] + nmdXML.GetAttrDouble("N_TOTDESC");
                            hsUpdDB["N_TOTANT"] = (double)hsUpdDB["N_TOTANT"] + nmdXML.GetAttrDouble("N_TOTANT");

                        }

                    }
                    catch (Exception e)
                    {
                        objBatch.Err(" Error en fnc_FactSueldos : ");
                        fncLogearHas(" Error en fnc_FactSueldos : " + e.Message);
                    }
                }
                rtaQry = 0;
                strQry = "DELETE FROM dbo.TCO_FACT_SUELDOS   WHERE OI_PERSONAL_EMP = 0   AND OI_ANIOMES =" + OI_ANIOMES.ToString() + "  AND OI_EMPRESA in (" + strEmpresas + ")";
                //rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strQry);
                fncLogeaDB("F_Sueldos_Del " + oi_empre.ToString(), strQry, strQry, ref rtaQry);

                clsDataBase.fncIns_SueldosPresup(OI_ANIOMES.ToString(), ref strQry);
                //rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strQry);
                fncLogeaDB("F_SueldosPres " + oi_empre.ToString(), strQry, strQry, ref rtaQry);
                //poner log a DB

                /* 11/07/2009
                                hsUpdDBType.Add("N_TOTPAG", "FLOAT");
                                hsUpdDBType.Add("N_TOTSRET", "FLOAT");
                                hsUpdDBType.Add("N_TOTNSRET", "FLOAT");
                                hsUpdDBType.Add("N_TOTASIG", "FLOAT");
                                hsUpdDBType.Add("N_TOTSAC", "FLOAT");
                                hsUpdDBType.Add("N_RETEN", "FLOAT");
                                hsUpdDBType.Add("N_TOTCONT", "FLOAT");
                                hsUpdDBType.Add("N_TOTTICKET", "FLOAT");
                                hsUpdDBType.Add("N_TOTDESC", "FLOAT");
                                hsUpdDBType.Add("N_TOTANT", "FLOAT");
                                hsUpdDBType.Add("cantidad_pers", "INT");
*/

            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fnc_FactSueldos : ");
                fncLogearHas(" Error en fnc_FactSueldos : " + e.Message);

            }
        }

        private void fnSueldosColumns(ref Hashtable hsUpdDBType)
        {

            if (hsUpdDBType.ContainsKey("OI_EMPRESA") == false) { hsUpdDBType.Add("OI_EMPRESA", "INT"); }
            if (hsUpdDBType.ContainsKey("OI_ANIOMES") == false) hsUpdDBType.Add("OI_ANIOMES", "INT");
            if (hsUpdDBType.ContainsKey("OI_SEXO") == false) hsUpdDBType.Add("OI_SEXO", "INT");
            if (hsUpdDBType.ContainsKey("N_TOTPAG") == false) hsUpdDBType.Add("N_TOTPAG", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_TOTSRET") == false) hsUpdDBType.Add("N_TOTSRET", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_TOTNSRET") == false) hsUpdDBType.Add("N_TOTNSRET", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_TOTASIG") == false) hsUpdDBType.Add("N_TOTASIG", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_TOTSAC") == false) hsUpdDBType.Add("N_TOTSAC", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_RETEN") == false) hsUpdDBType.Add("N_RETEN", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_TOTCONT") == false) hsUpdDBType.Add("N_TOTCONT", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_TOTTICKET") == false) hsUpdDBType.Add("N_TOTTICKET", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_TOTDESC") == false) hsUpdDBType.Add("N_TOTDESC", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_TOTANT") == false) hsUpdDBType.Add("N_TOTANT", "FLOAT");
            if (hsUpdDBType.ContainsKey("CANTIDAD_PERS") == false) hsUpdDBType.Add("CANTIDAD_PERS", "INT");
            if (hsUpdDBType.ContainsKey("N_PRESUPUESTO") == false) hsUpdDBType.Add("N_PRESUPUESTO", "FLOAT");
            if (hsUpdDBType.ContainsKey("N_ACUM") == false) hsUpdDBType.Add("N_ACUM", "FLOAT");

        }

        //==============================================================================================
        //              Insercion de sueldos conformados
        //==============================================================================================
        private void fncSueldosCOnf(NomadXML xmlTMP)
        {
            int rtaQry;
            string strSQL;
            string strQry;
            string strTmp;
            strSQL = "";
            strQry = "";
            rtaQry = 0;
            Hashtable hsColumns;
            try
            {
                sFncEjecutada = "fncSueldosCOnf";
                for (NomadXML nmdXML = xmlTMP.FirstChild().FindElement("ROW"); nmdXML != null; nmdXML = nmdXML.Next())
                {
                    if ((string)hsUpdDB["OI_PER_EMP_DDO"] == nmdXML.GetAttrString("OI_PER_EMP_DDO"))
                    {
                        strMsgErrorUser = sLegajoProc + " Error de datos en DataMart Tabla de Sueldos ";

                        strQry = "DELETE FROM dbo.TCO_FACT_SUELDOS_Conc  WHERE OI_PERSONAL_EMP = " + (string)hsUpdDB["OI_PERSONAL_EMP"];
                        strQry += "  AND OI_ANIOMES =" + (string)hsUpdDB["OI_ANIOMES"];
                        strQry += "  AND OI_EMPRESA =" + (string)hsUpdDB["OI_EMPRESA"];
                        strQry += "  AND OI_CONCEPTO  = " + nmdXML.GetAttrInt("OI_CONCEPTO").ToString();
                        strQry += "  AND OI_TIPO_CONCEPTO     = " + nmdXML.GetAttrInt("OI_TIPO_CONCEPTO").ToString();

                        strTmp = "elimina los sueldos conformados aniones : " + (string)hsUpdDB["OI_ANIOMES"] + " Empresa: " + (string)hsUpdDB["OI_EMPRESA"];
                        fncLogeaDB("TCO_FACT_SUELDOS_Conc", strTmp, strQry, ref rtaQry);

                        strSQL = " INSERT INTO TCO_FACT_SUELDOS_CONC( OI_PERSONAL_EMP ";
                        strSQL += ", OI_ANIOMES      ";
                        strSQL += ", OI_EMPRESA      ";
                        strSQL += ", OI_ESTRUCTURA   ";
                        strSQL += ", OI_CLASE_ORG    ";
                        strSQL += ", OI_TIPO_PERSONAL";
                        strSQL += ", OI_CONVENIO     ";
                        strSQL += ", OI_CATEGORIA    ";
                        strSQL += ", OI_PUESTO       ";
                        strSQL += ", OI_POSICION     ";
                        strSQL += ", OI_CENTRO_COSTO ";
                        strSQL += ", OI_SINDICATO    ";
                        strSQL += ", OI_ESTADO_CIVIL ";
                        strSQL += ", OI_SEXO         ";
                        strSQL += ", OI_CONCEPTO   ";
                        strSQL += ", OI_TIPO_CONCEPTO ";
                        strSQL += ", N_VALOR   ";
                        strSQL += ", N_CANTIDAD ) ";

                        strSQL += " SELECT OI_PERSONAL_EMP  = " + hsUpdDB["OI_PERSONAL_EMP"];
                        strSQL += ", OI_ANIOMES       = " + hsUpdDB["OI_ANIOMES"];
                        strSQL += ", OI_EMPRESA       = " + hsUpdDB["OI_EMPRESA"];
                        strSQL += ", OI_ESTRUCTURA    = " + hsUpdDB["OI_ESTRUCTURA"];
                        strSQL += ", OI_CLASE_ORG     = " + hsUpdDB["OI_CLASE_ORG"];
                        strSQL += ", OI_TIPO_PERSONAL = " + hsUpdDB["OI_TIPO_PERSONAL"];
                        strSQL += ", OI_CONVENIO      = " + hsUpdDB["OI_CONVENIO"];
                        strSQL += ", OI_CATEGORIA     = " + hsUpdDB["OI_CATEGORIA"];
                        strSQL += ", OI_PUESTO        = " + hsUpdDB["OI_PUESTO"];
                        strSQL += ", OI_POSICION      = " + hsUpdDB["OI_POSICION"];
                        strSQL += ", OI_CENTRO_COSTO  = " + hsUpdDB["OI_CENTRO_COSTO"];
                        strSQL += ", OI_SINDICATO     = " + hsUpdDB["OI_SINDICATO"];
                        strSQL += ", OI_ESTADO_CIVIL  = " + hsUpdDB["OI_ESTADO_CIVIL"];
                        strSQL += ", OI_SEXO          = " + hsUpdDB["OI_SEXO"];
                        strSQL += ", OI_CONCEPTO  = " + nmdXML.GetAttrInt("OI_CONCEPTO").ToString();
                        strSQL += ", OI_TIPO_CONCEPTO     = " + nmdXML.GetAttrInt("OI_TIPO_CONCEPTO").ToString();
                        strSQL += ", N_VALOR  = " + nmdXML.GetAttrDouble("N_VALOR").ToString().Replace(',', '.');
                        strSQL += ", N_CANTIDAD          = " + nmdXML.GetAttrDouble("N_CANTIDAD").ToString();

                        strTmp = "INSERT INTO los sueldos conformados aniones : " + (string)hsUpdDB["OI_ANIOMES"] + " Empresa: " + (string)hsUpdDB["OI_EMPRESA"];
                        fncLogeaDB("TCO_FACT_SUELDOS_Conc", strTmp, strSQL, ref rtaQry);

                    }

                }
                return;
            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fncSueldosConf : ");
                fncLogearHas(" Error en fncSueldosConf : " + e.Message);

            }

        }

        //==============================================================================================
        //==============================================================================================
        private void UpDateToDB(NomadXML xmlDATA, Hashtable hsColumns, string sFactName, string strWhere)
        {
            string strCamposUnicos;
            string strCampos;
            string strCamposMedida;
            string strTbl;
            string sQueryValues;
            string strCamposIns;
            string sQueryValuesUPD;
            string strQry;
            string strOI;
            int rtaQry;
            string strSQL;
            string sTextoLog;
            int CantIns;
            int CantUpd;
            int CantNA;
            Hashtable hashColumnsValue;
            Hashtable hashColumnsFijos;
            strQry = "";
            rtaQry = 0;
            sQueryValuesUPD = "";
            sQueryValues = "";
            strSQL = "";
            sTextoLog = "";
            strCamposUnicos = "";
            strCampos = "";
            strCamposMedida = "";
            strTbl = "";

            hashColumnsValue = new Hashtable();
            hashColumnsFijos = new Hashtable();
            // xmlDATA != null ---> implica que trae un set de registros para ingresar en la FACT (n INSERT INTO de acuerdo a la cantidad de registros)
            // xmlDATA == null ---> es solo un conjunto de variables ( unico INSERT INTO )
            try
            {
                sTextoLog = "xmlDATA firstCHID NULO";
                sFncEjecutada = "UpDateToDB";
                if (xmlDATA != null)
                {
                    if (xmlDATA.FirstChild().FindElement("ROW") == null)
                    {

                        foreach (string key in hsColumns.Keys)
                        {
                            strQry += key + ",";

                        }
                        //Mensaje solo al archivo

                        // comentado 04/05/2009... si no encuentra datos no debe reportarlo a la pantalla (lo informa en el log)
                        //fncLogearHas(" FACT " + sFactName + " No encontro datos en la consulta " + strQry + " WHERE  " + strWhere);
                        NomadBatch.Trace(" FACT " + sFactName + " No encontro datos en la consulta " + strQry + " WHERE  " + strWhere);
                        return;

                    }
                }
                sFactName = sFactName.ToUpper();
                switch (sFactName)
                {
                    case "EVALUACIONES":
                        clsDataBase.fncCamposFactEval(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        if (objEjec_Eva.f_Inicio_ejecNull == true) { objEjec_Eva.f_Inicio_ejec = DateTime.Now; }
                        break;

                    case "HORAS":
                        clsDataBase.fncCamposFactHoras(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        if (objEjec_Horas.f_Inicio_ejecNull == true) { objEjec_Horas.f_Inicio_ejec = DateTime.Now; }
                        break;

                    case "PERSONAL":
                        clsDataBase.fncCamposFactPersonal(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        break;

                    case "SUELDOS":
                        clsDataBase.fncCamposFactSueldos(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        break;

                    case "ESTRUCTURAS":
                        clsDataBase.fncCamposFactEstruct(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        break;

                    case "PROYECTOS":
                        clsDataBase.fncCamposFactHorasProy(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        break;

                    case "PROYECTOS_PROYCLOSURE":
                        clsDataBase.fncCamposProyClosure(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        break;

                    case "PROYECTOS_DIMREC":
                        clsDataBase.fncCamposDimProyRec(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        break;

                    case "PROYECTOS_DIMPERF":
                        clsDataBase.fncCamposDimProyPerf(ref   strCamposUnicos, ref   strCampos, ref   strCamposMedida, ref   strTbl);
                        break;
                }
                sTextoLog = "sFactName " + sFactName;

                char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
                // estos campos son fijos y se repiten tantas veces como registros existan en xmlDATA
                string[] arrCamposUnicos = strCamposUnicos.Split(delimiterChars);
                string[] arrCampos = strCampos.Split(delimiterChars);
                string[] arrCamposMedida = strCamposMedida.Split(delimiterChars);
                string strTemp;
                strTemp = "";

                foreach (string sValue in arrCamposUnicos)
                {
                    //nValue = (hsUpdDB[sValue] == null) ? 0 : hsUpdDB[sValue];
                    // HAY UN ERROR porque no se cargo algun valor (AGREGAR LOG)
                    if (sValue == "") { continue; }

                    if (hsUpdDB.ContainsKey(sValue) == false)
                    {
                        return; // LOGEAR!!!!!!!!!!!!!!
                    }

                    strTemp = (string)hsUpdDB[sValue].ToString();
                    strTemp = (strTemp == null) ? "0" : strTemp;
                    //sQueryValues += sValue + " =" + strTemp + ", ";
                    sQueryValues += sValue + "=?, ";
                    if (hashColumnsFijos.ContainsKey(sValue)) { continue; }
                    hashColumnsFijos.Add(sValue, strTemp);
                }
                sTextoLog = "arrCamposUnicos ";

                string sQueryValuesW = "";
                //sQueryValues = sQueryValues.Substring(0, sQueryValues.Length - 2);
                strCamposIns = strCamposUnicos + "," + strCampos + ", " + strCamposMedida;
                strCamposIns = strCamposIns.Replace(",,", ",");
                CantIns = 0;
                CantUpd = 0;
                CantNA = 0;
                strOI = "";
                //--------------------------------------------------------------------------------------------------
                // es para el caso de insertar solo variables , no variables por cada n registros almacenados en xmlTMP
                if (xmlDATA == null)
                {
                    sTextoLog = "insertar solo variables ";
                    strCamposIns = strCamposUnicos + "," + strCamposMedida;
                    // recorre las medidas para armar el update
                    foreach (string sValue in arrCamposMedida)
                    {
                        if (sValue == "") { continue; }
                        if (hsUpdDB.ContainsKey(sValue))// si no esta la medida en la hash es que no encontro valor
                        {
                            strTemp = (string)hsUpdDB[sValue].ToString();
                            strTemp = (strTemp == null) ? "0" : strTemp;
                        }
                        else
                        {
                            strTemp = "0"; // deja 0 por defecto
                        }
                        // sQueryValuesW += sValue + " =" + strTemp.Replace(",", ".") + ", ";
                        sQueryValuesW += sValue + " =?" + ", ";
                        if (hashColumnsFijos.ContainsKey(sValue)) { continue; }
                        hashColumnsFijos.Add(sValue, strTemp.Replace(",", "."));
                    }
                    sTextoLog = "insertar solo variables antes Ejec_SqlToFacr";
                    strOI = Ejec_SqlToFacr(sQueryValues, strTbl, sQueryValuesW, strCamposIns, sFactName, hsColumns, hashColumnsFijos);
                    //                    fncLogDB(strOI, ref CantIns, ref CantUpd, ref CantNA, strOI, sFactName);

                    return;
                }
                //--------------------------------------------------------------------------------------------------

                for (NomadXML xmlTMP = xmlDATA.FirstChild().FindElement("ROW"); xmlTMP != null; xmlTMP = xmlTMP.Next())
                {
                    hashColumnsValue = new Hashtable();
                    sTextoLog = "Siguiente a nsertar solo variables ";
                    foreach (string key in hashColumnsFijos.Keys)
                    {
                        hashColumnsValue.Add(key, hashColumnsFijos[key]);
                    }

                    // arma el resto del query para presentarlo en el WHERE del Update
                    sQueryValuesW = sQueryValues;
                    foreach (string sValue in arrCampos)
                    {
                        //sQueryValuesW += sValue + " =" + xmlTMP.GetAttrInt(sValue) + ", ";
                        sQueryValuesW += sValue + " =?" + ", ";
                        if (hashColumnsValue.ContainsKey(sValue)) { continue; }
                        hashColumnsValue.Add(sValue, xmlTMP.GetAttrInt(sValue));

                    }

                    // arma el query para el SET del Update
                    sQueryValuesUPD = "";
                    foreach (string sValue in arrCamposMedida)
                    {
                        //sQueryValuesUPD += sValue + " =" + xmlTMP.GetAttrInt(sValue) + ", ";
                        sQueryValuesUPD += sValue + " =? " + ", ";
                        if (hashColumnsValue.ContainsKey(sValue)) { continue; }
                        hashColumnsValue.Add(sValue, xmlTMP.GetAttrInt(sValue));
                    }
                    sTextoLog = "Siguiente a nsertar solo variables antes Ejec_SqlToFacr";
                    strOI = Ejec_SqlToFacr(sQueryValuesW, strTbl, sQueryValuesUPD, strCamposIns, sFactName, hsColumns, hashColumnsValue);
                    sTextoLog = "Siguiente a nsertar solo variables despues Ejec_SqlToFacr";
                }
            }
            catch (Exception e)
            {

                objBatch.Err(" Error en modulo : " + sFactName + " Tabla:" + strTbl);
                NomadBatch.Trace(" Error en upDateToDB : " + e.Message);
                NomadBatch.Trace(" Error en upDateToDB sTextoLog: " + sTextoLog);
            }

        }

        //*****************************************************************************************************
        private void fncLogDB(string rta, ref int CantIns, ref int CantUpd, ref int CantNA, string errStr, string sFactName)
        {
            //esta funcion arma elcuenta la cantidad de INSERT INTO Updates o Ninguna Accion de acuerdo a los resultados
            // obtenido de las llamadas a eejcuciones de sql
            try
            {

                if (rta == "INS") { CantIns += 1; return; }
                if (rta == "UPD") { CantUpd += 1; return; }

                NucleusRH.Base.Indicadores.Ejecuciones.ERROR objERR;
                objERR = new NucleusRH.Base.Indicadores.Ejecuciones.ERROR();
                CantNA += 1;
                objERR.c_error = DateTime.Now.ToString() + CantNA.ToString();
                objERR.descr = rta;
                objERR.t_error = errStr;
                objEjecDet.ERRORES.Add(objERR);
                objEjecDet.c_estado = "C";
                //objEjec.c_estado = "C"; // modificado 31/08/2009
                strErroresProc = "C";
                fnc_AddLogDetail(objEjecDet, sFactName, "C", CantNA, CantNA);
            }

            catch (Exception e)
            {
                objBatch.Err(" Error en fncLogDB : ");
                fncLogearHas(" Error en fncLogDB : " + e.Message);
            }

        }

        private void fnc_AddLogDetail(NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE objDet, string sFactName, string s_Estado, int nCantProc, int nCantErr)
        {
            try
            {
                // guarda el detalle para el log de DB
                objDet.n_procesados = nCantProc;
                objDet.n_error = nCantErr;

                switch (sFactName)
                {
                    case "EVALUACIONES":
                        if (objEjec_Eva.c_estado != "C") { objEjec_Eva.c_estado = s_Estado; }
                        objEjec_Eva.DETALLE.Add(objDet);
                        break;

                    case "HORAS":
                        if (objEjec_Horas.c_estado != "C") { objEjec_Horas.c_estado = s_Estado; }
                        objEjec_Horas.DETALLE.Add(objDet);
                        break;

                    case "PERSONAL":
                        if (objEjec_Pers.c_estado != "C") { objEjec_Pers.c_estado = s_Estado; }
                        objEjec_Pers.DETALLE.Add(objDet);
                        break;

                    case "SUELDOS":
                        if (objEjec_Sueldo.c_estado != "C") { objEjec_Sueldo.c_estado = s_Estado; }
                        objEjec_Sueldo.DETALLE.Add(objDet);
                        break;

                    case "PROYECTOS":
                        if (objEjec_Sueldo.c_estado != "C") { objEjec_Sueldo.c_estado = s_Estado; }
                        objEjec_Sueldo.DETALLE.Add(objDet);
                        break;

                }
            }

            catch (Exception e)
            {
                objBatch.Err(" Error en fncLogDB : ");
                fncLogearHas(" Error en fncLogDB : " + e.Message);
            }

        }

        //==============================================================================================
        //==============================================================================================
        private string Ejec_SqlToFacr(string sQueryValuesW, string strTbl, string sQueryValuesUPD, string strCamposIns, string strFactName, Hashtable hsColumns, Hashtable hashColumnsValue)
        {
            string strSQL;
            string strLog;
            int rtaQry;
            int nIns;
            string strOI;
            int nUpd;
            int nErr;
            Array arrLog;
            strOI = "";
            string s_orderPar;
            string Signo;
            string OrderParams;
            Array orderPar;
            strSQL = "";
            try
            {

                sFncEjecutada = "Ejec_SqlToFacr";
                strLog = (string)hsLog[strFactName];
                arrLog = strLog.Split(',');
                nIns = Int32.Parse(((string[])(arrLog))[0]);
                nUpd = Int32.Parse(((string[])(arrLog))[1]);
                // arma el orden de los campos que se parametrizan en la clase de DB para que haga el binding por orden de aparicion en la instruccion SQL
                OrderParams = sQueryValuesUPD.Substring(0, sQueryValuesUPD.Length - 2) + "," + sQueryValuesW;

                OrderParams = OrderParams.Replace("=", "");
                OrderParams = OrderParams.Replace(",", "");
                orderPar = OrderParams.Split('?');

                foreach (DictionaryEntry item in hsColumns) // recorre la hash para agregar a la hash de tipos los campos que vienen en el XML que se envio desde la llamada principal
                {
                    if (hsUpdDBType.ContainsKey(item.Key)) { continue; }
                    hsUpdDBType.Add(item.Key, item.Value);
                }

                sQueryValuesW = sQueryValuesW.Substring(0, sQueryValuesW.Length - 2);
                strSQL = " UPDATE " + strTbl + " SET " + sQueryValuesUPD.Substring(0, sQueryValuesUPD.Length - 2);
                strSQL += " WHERE " + sQueryValuesW.Replace(",", " and ");

                rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strSQL, hsUpdDBType, hashColumnsValue, orderPar);
                // SI la respuesta es cero implica que no existe el registro
                strOI = "";
                if (rtaQry == 0)
                {
                    s_orderPar = strCamposIns;
                    Signo = "";
                    orderPar = s_orderPar.Split(',');
                    for (int aa = 0; aa < orderPar.Length; aa++)
                    {
                        Signo += "?,";

                    }
                    Signo += "*";
                    Signo = Signo.Replace(",*", "");
                    //--------------------------------------------------------------------------------------------------
                    strSQL = " INSERT INTO " + strTbl + "  ( " + strCamposIns + " ) ";
                    strSQL += " VALUES ( " + Signo + " )";
                    //+ ", " + sQueryValuesUPD.Substring(0, sQueryValuesUPD.Length - 2)
                    rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strSQL, hsUpdDBType, hashColumnsValue, orderPar);
                    if (rtaQry != 0) { strOI = "INS"; }
                }
                else
                {
                    strOI = "UPD";
                }
                // es para logear la instruccion
                if (strOI == "")
                {
                    strOI = strSQL;
                }
                nErr = 0;
                if (rtaQry < 0)
                {
                    nErr = 1;
                    strOI = "Error de grabacion en tabla " + strFactName;
                }

                fncLogDB(strOI, ref nIns, ref nUpd, ref nErr, strOI, strFactName);
                hsLog[strFactName] = nIns.ToString() + "," + nUpd.ToString();

                return strOI;
            }
            catch (Exception e)
            {
                nIns = 0;
                //fncLogDB("Error de proceso ", ref nIns, ref nIns, ref nIns, " ERROR NomadQuery " + e.Message);
                objBatch.Err(" Error actualizando Tabla: " + strFactName);
                fncLogearHas(" Error en Ejec_SqlToFacr : " + e.Message);
                fncLogearHas(" Ejec_SqlToFacr strSQL:" + strSQL);
                return e.Message;
            }

        }

        //==============================================================================================
        //==============================================================================================
        //-----------------------------------------------------------------------------------------------
        private Array GetNameFields(string sCampos)
        {
            sCampos = sCampos.Replace(" AS ", ",");
            sCampos = sCampos.Replace(" ", "");
            sCampos = sCampos.Replace("\t", "");
            sCampos = sCampos.Replace(".", ",");

            Array rta = sCampos.Split(',');
            return rta;

        }

        //-----------------------------------------------------------------------------------------------
        private bool InsertDataHash(string strTabla, string strWhere, string strCampos, string strCamposAlias, string strTablaDestino, ref Hashtable hashColumns)
        {// carga los elementos que trae de los selects en una Hash global que despues usará para generar el SQL sobre la FACT
            NomadXML xmlTMP;
            bool InsertInHas;
            try
            {

                xmlTMP = NomadQuery(strTabla, strWhere, strCampos, strCamposAlias, strTablaDestino, ref hashColumns);
                InsertInHas = GenHashToDB(xmlTMP, strCampos, hashColumns, strWhere);
                return true;
            }
            catch (Exception e)
            {

                objBatch.Err(" Error en GenHashToDb : ");
                fncLogearHas(" Error en GenHashToDb : " + e.Message);

                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------
        private bool GenHashToDB(NomadXML xmlTMP, string strCampos, Hashtable hsColumns, string strWhereLog)
        {// carga los elementos que trae de los selects en una Hash global que despues usará para generar el SQL sobre la FACT
            string sName;
            string sValue;
            try
            {

                if (xmlTMP == null)
                {
                    //Mensaje solo al archivo
                    //fncLogearHas( strFacProces + " No encontro datos en la consulta " + strCampos + " WHERE " + strWhereLog);
                    return false;

                }

                NomadXML nmdRta = xmlTMP.FirstChild().FindElement("ROW");
                /*if (nmdRta == null)
                {
                    //Mensaje solo al archivo
                    fncLogearHas( strFacProces + " No encontro datos en la consulta " + strCampos + " WHERE " + strWhereLog);
                    //return false;
                }
                */
                Array NameFields = GetNameFields(strCampos);

                for (int i = 0; i < NameFields.Length; i++)
                {
                    sName = "";
                    sValue = "";
                    sName = ((string[])(NameFields))[i];
                    if ((hsColumns.ContainsKey(sName.ToUpper()) != true))
                    {
                        continue;
                    }
                    if (nmdRta == null)
                    {
                        sValue = "";
                    }
                    else
                    {
                        sValue = nmdRta.GetAttr(((string[])(NameFields))[i]);
                    }

                    if (hsUpdDB.ContainsKey(sName) != true) { hsUpdDB.Add(sName, fncConverSQL((string)hsColumns[sName], sValue)); }

                    if (hsUpdDBType.ContainsKey(sName) != true) { hsUpdDBType.Add(sName, (string)hsColumns[sName]); }

                }

                return true;
            }
            catch (Exception e)
            {

                objBatch.Err(" Error en GenHashToDb : ");
                fncLogearHas(" Error en GenHashToDb : " + e.Message);

                return false;
            }
        }
        private string GetMaxFecIng(int OI_PERSONAL_EMP, string mfecha_hasta, ref string strCampos, string strTBL, ref string strWHERE)
        {
            Hashtable hsColumns;
            DateTime fecha_hasta;
            string rtaFecha;
            int int_tmp;
            int nCant;
            string strTMP;
            try
            {

                sFncEjecutada = "GetMaxFecIng";
                hsColumns = new Hashtable();
                clsDataBase.fncFindMaxFIng(OI_PERSONAL_EMP, mfecha_hasta, ref   strCampos, ref strTBL, ref   strWHERE);
                NomadXML xmlTMP;
                xmlTMP = NomadQuery(strTBL, strWHERE, strCampos, "", "", ref  hsColumns);

                fecha_hasta = xmlTMP.FirstChild().FindElement("ROW").GetAttrDateTime("F_INGRESO");
                nCant = xmlTMP.FirstChild().FindElement("ROW").GetElements("F_INGRESO").Count;
                if (nCant == 0) { return ""; }
                int_tmp = 100 + fecha_hasta.Month;
                strTMP = int_tmp.ToString().Substring(1, int_tmp.ToString().Length - 1);
                rtaFecha = fecha_hasta.Year.ToString() + strTMP;

                int_tmp = 100 + fecha_hasta.Day;
                strTMP = int_tmp.ToString().Substring(1, int_tmp.ToString().Length - 1);
                rtaFecha += strTMP;

                return rtaFecha;
            }
            catch (Exception e)
            {

                objBatch.Err(" Error en GetMaxFecIng : ");
                fncLogearHas(" Error en GetMaxFecIng : " + e.Message);

                return "";
            }
        }

        //-----------------------------------------------------------------------------------------------
        private void GetDifHor(NomadXML xmlTMP, string sCampoFechaDesde, string sCampoFechaHasta, string sName_Field)
        {// calcula la diferencia Horaria entre dos fechas y la guarda en HASH
            string sName;
            string sValue;
            try
            {
                sFncEjecutada = "GetDifHor";
                hsUpdDB.Add(sName_Field, 0);
                hsUpdDBType.Add(sName_Field, "INT");

                if (xmlTMP == null) { return; }

                TimeSpan difDate;
                int difHora;
                difHora = 0;
                NomadXML nmdRta = xmlTMP.FirstChild().FindElement("ROW");
                if (nmdRta == null) { return; }

                for (NomadXML nmdXML = xmlTMP.FirstChild().FindElement("ROW"); nmdXML != null; nmdXML = nmdXML.Next())
                {

                    difDate = nmdXML.GetAttrDateTime(sCampoFechaHasta) - nmdXML.GetAttrDateTime(sCampoFechaDesde);
                    difHora += (int)difDate.Hours;
                }
                hsUpdDB[sName_Field] = difHora;
                // hsUpdDB.Add(sName_Field, difHora); 10/07/2009
                //hsUpdDBType.Add(sName_Field, "INT");
                return;
            }
            catch (Exception e)
            {

                objBatch.Err(" Error en GetDifHor : ");
                fncLogearHas(" Error en GetDifHor : " + e.Message);

                return;
            }
        }

        //-----------------------------------------------------------------------------------------------

        private void GetlstTiposHora(ref string strLst_tipohora, ref string strLst_licencia)
        {

            Hashtable hsColumns;
            hsColumns = new Hashtable();
            string strCampos = "";
            string strTbl = "";
            string strWHERE = "";
            strLst_tipohora = "*";
            strLst_licencia = "*";
            clsDataBase.fncGetHasTiposHoras(ref strCampos, ref strTbl, ref strWHERE);

            NomadXML xmlTMP;
            xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref  hsColumns);
            for (NomadXML xmlTMPlst = xmlTMP.FirstChild().FindElement("ROW"); xmlTMPlst != null; xmlTMPlst = xmlTMPlst.Next())
            {
                if (xmlTMPlst.GetAttr("OI_TIPOHORA") != "") { strLst_tipohora += "," + xmlTMPlst.GetAttr("OI_TIPOHORA"); }
                if (xmlTMPlst.GetAttr("OI_LICENCIA") != "") { strLst_licencia += "," + xmlTMPlst.GetAttr("OI_LICENCIA"); }

            }

            strLst_tipohora = strLst_tipohora.Replace("*,", "");
            strLst_licencia = strLst_licencia.Replace("*,", "");
            strLst_tipohora = strLst_tipohora.Replace("*", "");
            strLst_licencia = strLst_licencia.Replace("*", "");

        }

        // si los valores son texto les agrega "'" (o en casos nulos la palabra NULL)
        private string fncConverSQL(string strType, string strTextQry)
        {
            string rta;
            try
            {

                rta = "";

                if (strType == "DATE")
                {

                    if (strTextQry == "")
                    { rta = "19000101"; }

                    //rta = "#" + strTextQry.Substring(4, 2) + "/" + strTextQry.Substring(6, 2) + "/" + strTextQry.Substring(0, 4) + "#";

                    return rta;
                }

                if (strType == "INT")
                {
                    rta = "" + strTextQry;
                    if (strTextQry == "") { rta = "0"; }
                }
                else
                {
                    rta = "" + strTextQry + "";
                }
                if (rta == "")
                {
                    rta = "NULL";
                }

                if (strType == "FLOAT")  //STR
                {
                    rta = "" + strTextQry;
                    if (strTextQry == "") { rta = "0"; }
                }

                return rta;
            }
            catch (Exception e)
            {
                objBatch.Err(" Error de conversion de datos");
                objBatch.Err("strType :'" + strType + "' strTextQry :'" + strTextQry + "'");
                return "";
            }
        }
        //--------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        // Calcula la remuneración
        private double fncCalcRemun()
        {
            string C_CONVENIO;
            string C_TIPO_PERSONAL;
            int OI_CATEGORIA_ult;
            try
            {

                C_CONVENIO = fncClearString((string)hsUpdDB["C_CONVENIO"]);
                C_TIPO_PERSONAL = fncClearString((string)hsUpdDB["C_TIPO_PERSONAL"]);
                OI_CATEGORIA_ult = Int32.Parse((string)hsUpdDB["OI_CATEGORIA"]);
                double REMUN_PERS = 0;
                if (C_CONVENIO == "00")
                {
                    switch (C_TIPO_PERSONAL)
                    {
                        case "3":    // Fuera de Convenio NO Pasante
                            REMUN_PERS = double.Parse((string)hsUpdDB["N_VALOR_HORA"]);
                            break;
                        case "1":        //Dentro de Convenio mensual
                            REMUN_PERS = double.Parse((string)hsUpdDB["N_VALOR_HORA"]);
                            break;

                        case "4":        //Dentro de Convenio mensual
                            REMUN_PERS = double.Parse((string)hsUpdDB["N_VALOR_HORA"]);
                            break;

                        case "8":        //Dentro de Convenio mensual
                            REMUN_PERS = double.Parse((string)hsUpdDB["N_VALOR_HORA"]);
                            break;

                        case "2":        //Dentro de Convenio Quincenal
                            REMUN_PERS = double.Parse((string)hsUpdDB["N_VALOR_HORA"]) * 8 * 30;
                            break;
                        case "5":        //Dentro de Convenio Quincenal
                            REMUN_PERS = double.Parse((string)hsUpdDB["N_VALOR_HORA"]) * 8 * 30;
                            break;

                    }

                }
                //Sin Categoria ni pasante
                if ((OI_CATEGORIA_ult == 0) && (C_TIPO_PERSONAL != "3"))
                {
                    REMUN_PERS = 0;
                }

                //Pasantes
                if (C_TIPO_PERSONAL == "3")
                {
                    if (hsUpdDB.ContainsKey("N_EST_PAS_MES") == false)
                    { REMUN_PERS = 0; }
                    else
                    {
                        REMUN_PERS = (double)hsUpdDB["N_EST_PAS_MES"];
                    }
                }

                return REMUN_PERS;

            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fncCalcRemun : ");
                fncLogearHas(" Error calculando remuneracion: " + e.Message);
                return 0;
            }
        }

        //-----------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------

        // Calcula la remuneración
        private double fncCalcRemunFactPers()
        {
            string C_CONVENIO;
            string C_TIPO_PERSONAL;
            string sValue;
            double nValue;
            int OI_CATEGORIA_ult;
            try
            {
                sValue = "";
                nValue = 0;
                C_CONVENIO = fncClearString((string)hsUpdDB["C_CONVENIO"]);
                C_TIPO_PERSONAL = fncClearString((string)hsUpdDB["C_TIPO_PERSONAL"]);
                OI_CATEGORIA_ult = Int32.Parse((string)hsUpdDB["OI_CATEGORIA"]);
                double REMUN_PERS = 0;
                if (hsUpdDB.ContainsKey("N_VALOR_HORA"))
                {
                    sValue = (string)hsUpdDB["N_VALOR_HORA"];
                    nValue = Double.Parse(sValue);
                }
                if (C_CONVENIO != "00")
                {
                    switch (C_TIPO_PERSONAL)
                    {

                        case "1":        //Dentro de Convenio mensual
                            REMUN_PERS = nValue;
                            break;

                        case "4":        //Dentro de Convenio mensual
                            REMUN_PERS = nValue;
                            break;

                        case "8":        //Dentro de Convenio mensual
                            REMUN_PERS = nValue;
                            break;

                        case "2":        //Dentro de Convenio Quincenal
                            REMUN_PERS = nValue * 8 * 30;
                            break;
                        case "5":        //Dentro de Convenio Quincenal
                            REMUN_PERS = nValue * 8 * 30;
                            break;

                    }

                }
                //Sin Categoria ni pasante
                if ((OI_CATEGORIA_ult == 0) && (C_TIPO_PERSONAL != "3"))
                {
                    REMUN_PERS = 0;
                }

                //Pasantes
                if (C_TIPO_PERSONAL == "3")
                {
                    nValue = 0;
                    if (hsUpdDB.ContainsKey("N_EST_PAS_MES"))
                    {
                        sValue = (string)hsUpdDB["N_EST_PAS_MES"];
                        nValue = Double.Parse(sValue);
                    }

                    REMUN_PERS = nValue;
                }

                return REMUN_PERS;
            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fncCalcRemunFactPers : ");
                fncLogearHas(" Error en fncCalcRemunFactPers : " + e.Message);

                return 0;
            }
        }

        //-----------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------

        private System.Xml.XmlDocument EjecutarQuery(string strTabla, string strWhere, string strCampos, string strCamposQryOut, ref Array arrCampos)
        {
            string varQuery;
            string varQueryParam;
            string sCamposOut;

            // para compatibilidad con ORACLE se paso todo a UPPER
            strWhere = strWhere.Replace("&GT", "&gt");
            strWhere = strWhere.Replace("&LT", "&lt");

            arrCampos = strCampos.Split(',');
            arrCampos = strCamposQryOut.Split(',');

            System.Xml.XmlDocument xmlDocCal;

            varQuery = @"
                <qry:main doc=""PARAM"">
                    <qry:append-doc name=""FILTRO"" doc-path=""#PARAM:/FILTRO""/>
                    <qry:element name=""objects"">
                      <qry:insert-select doc-path=""#FILTRO:"" name=""filtro_empresa""/>
                    </qry:element>
                  </qry:main>
                    <qry:select doc=""PARAM"" name=""filtro_empresa"">
                        <qry:xquery>
                          for $r in sql ('  SELECT   " + strCampos + @"  FROM "
                                     + strTabla + @" WHERE " + strWhere + @"  ')/ROWS" +
                                 @"</qry:xquery>
                        <qry:out>

                          <qry:insert-element doc-path=""$r""/>
                        </qry:out>
                      </qry:select>  ";

            varQueryParam = @"<FILTRO />";
            try
            {
                strMsgErrorUser = sLegajoProc + "Error de consulta en NucleusRH ";
                xmlDocCal = NomadEnvironment.QueryXML(varQuery.ToString(), varQueryParam);
                return xmlDocCal;
            }

            catch (Exception e)
            {
                fncLogearHas(" ERROR EjecutarQuery " + e.Message + "  " + varQuery.ToString());

                return null;
            }

        }
        // esta Funcion ejecuta los querys y el log correspondiente al resultado (ejecutado o Error)
        private void fncLogeaDB(string sNomTBl, string sDesc, string strQry, ref int rtaQry)
        {

            try
            {
                objEjecDet = new NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE();
                string sEmpresa;
                sEmpresa = (string)hsUpdDB["OI_EMPRESA"];

                if (sEmpresa == null)
                {
                    sEmpresa = sNomTBl + "                                 ";
                }
                else
                {
                    sEmpresa = sNomTBl + " Emp:" + sEmpresa + "                                 ";
                }
                objEjecDet.c_estado = "E";
                objEjecDet.c_nom_tabla = sEmpresa.Substring(0, 30);
                objEjecDet.descr = sDesc;
                objEjecDet.n_error = 0;

                rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strQry);
                objEjecDet.c_estado = "S";

                if (rtaQry == -1)
                {
                    fncExecQry(strQry, ref objEjecDet);
                    rtaQry = 1;
                }
                objEjecDet.n_procesados = rtaQry;

                //objEjec.DETALLE.Add(objEjecDet); //31/03/2009
                fncADD_DDO(objEjecDet);
            }
            catch (Exception e)
            {
                objBatch.Err(" Error en fncLogeaDB : ");
                //fncLogearHas(" Error en fncLogeaDB : " + e.Message);
                NomadBatch.Trace(" Error en fncLogeaDB : " + e.Message);
                objEjecDet.n_error = 1;
                NucleusRH.Base.Indicadores.Ejecuciones.ERROR objErr;
                objErr = new NucleusRH.Base.Indicadores.Ejecuciones.ERROR();
                objErr.c_error = "fncLogeaDB";

                //objErr.descr = "Error en fncLogeaDB"; modificado 29/09/2009
                objErr.descr = strQry; //guarda la consulta qe genera el error
                objEjecDet.n_procesados = 1;
                objEjecDet.c_estado = "C";

                //objEjec.c_estado = "C"; // modificado 31/08/2009
                strErroresProc = "C";

                objEjecDet.ERRORES.Add(objErr);
                //objEjec.DETALLE.Add(objEjecDet); //31/03/2009
                fncADD_DDO(objEjecDet);
            }

        }
        // este log pertenece un error en la ejecucion de la sentencia SQL, el log referencia a la clase de Base de Datos
        private void fncExecQry(string strQry, ref NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE objEjecD)
        {

            fncLogearHas(" Error ejecucion de SQL : " + strQry);
            objEjecD.n_error = 1;
            NucleusRH.Base.Indicadores.Ejecuciones.ERROR objErr;
            objErr = new NucleusRH.Base.Indicadores.Ejecuciones.ERROR();
            objErr.c_error = sFncEjecutada;
            objErr.t_error = objDB.sMsgError;
            objErr.descr = "Error en ExecuteNonQuery ";
            objEjecD.n_procesados = 1;
            objEjecD.c_estado = "C";
            //objEjec.c_estado = "C"; // modificado 31/08/2009
            strErroresProc = "C";

            objEjecD.ERRORES.Add(objErr);
            objEjec.DETALLE.Add(objEjecDet);
        }

        //==============================================================================================
        //          Logea la has en caso de error para ubicar el ultimo paso
        //---------------------------------------------------------------------------------------------
        private void fncLogearHas(string strMsg)
        {
            string strHASH;
            string sEmpresa;
            sEmpresa = "";
            strHASH = " ";
            foreach (DictionaryEntry item in hsUpdDB)
            {
                strHASH += "// " + item.Key + " : " + item.Value;
            }

            NomadBatch.Trace(strMsg);
            NomadBatch.Trace(" Datos en Coleccion " + strHASH);
            objEjecDet = new NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE();
            if (hsUpdDB.ContainsKey("OI_EMPRESA"))
            { sEmpresa = (string)hsUpdDB["OI_EMPRESA"]; }
            else
            { sEmpresa = "Proyectos"; }

            if (sEmpresa == null)
            { sEmpresa = strFacProces + "                                 "; }
            else
            { sEmpresa = strFacProces + " Emp:" + sEmpresa + "                                 "; }

            objEjecDet.c_nom_tabla = sEmpresa.Substring(0, 30);
            objEjecDet.n_error = 1;
            NucleusRH.Base.Indicadores.Ejecuciones.ERROR objErr;
            objErr = new NucleusRH.Base.Indicadores.Ejecuciones.ERROR();
            objErr.c_error = sFncEjecutada + "_log";
            objErr.l_logUsuario = false;
            objErr.t_error = "ultimo hash:" + strHASH;

            objErr.descr = "Error de proceso";
            objEjecDet.n_procesados = 1;
            objEjecDet.c_estado = "C";
            //objEjec.c_estado = "C"; // modificado 31/08/2009
            strErroresProc = "C";

            objEjecDet.ERRORES.Add(objErr);
            fncADD_DDO(objEjecDet);

            objErr = new NucleusRH.Base.Indicadores.Ejecuciones.ERROR();
            objErr.c_error = sFncEjecutada;
            objErr.t_error = strMsgErrorUser;
            objErr.l_logUsuario = true;

            objEjecDet.ERRORES.Add(objErr);
            fncADD_DDO(objEjecDet);

        }

        // Logea el error por Fact para luego ser guardado en la DB
        private void fncADD_DDO(NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE objEjecD)
        {

            switch (strFacProces)
            {
                case "Fact_Evaluacion":
                    if (objEjec_Eva.DETALLE.Count < 20) { objEjec_Eva.DETALLE.Add(objEjecD); }
                    break;

                case "Fact_Horas":
                    if (objEjec_Horas.DETALLE.Count < 20) { objEjec_Horas.DETALLE.Add(objEjecD); }
                    break;

                case "Fact_Personal":
                    if (objEjec_Pers.DETALLE.Count < 20) { objEjec_Pers.DETALLE.Add(objEjecD); }
                    break;

                case "Fact_Sueldos":
                    if (objEjec_Sueldo.DETALLE.Count < 20) { objEjec_Sueldo.DETALLE.Add(objEjecD); }
                    break;

                case "Fact_Datos_Comunes":
                    if (objEjec.DETALLE.Count < 20) { objEjec.DETALLE.Add(objEjecD); }
                    break;
                case "Fact_Proyectos":
                    if (objEjec.DETALLE.Count < 20) { objEjec.DETALLE.Add(objEjecD); }
                    break;

                /*  case "":
                      if (objEjec.DETALLE.Count < 20) { objEjec.DETALLE.Add(objEjecD); }
                      break;*/
            }
        }

        private void fncSaveDDO()
        {
            Hashtable hsTabl;
            hsTabl = new Hashtable();
            hsTabl.Add("E", "En ejecucion");
            hsTabl.Add("S", "Sin Errores");
            hsTabl.Add("C", "Con Errores");
            string logError;
            logError = "";

            try
            {

                if (strErroresProc == "C") { objEjec.c_estado = "C"; }
                // 31/08/2009 ...es para poner el error de ejecucion global al final porque puede cortarse en medio del
                // proceso y debería quedar en estado "E"

                logError = "objEjec :" + objEjec.SerializeAll(); //  esto es por si da error para logear en el catch
                sFncEjecutada = "fncSaveDDO";
                NomadBatch.Trace(" Guarda los DDO ");
                NomadEnvironment.GetCurrentTransaction().Save(objEjec);
                NomadBatch.Trace(" DDO comun guardado");
                strMsgMail = " Resumen de actualizacion ejecutada en fecha " + objEjec.f_Inicio_ejec.ToString();
                strMsgMail += " \n Período " + objEjec.c_mes_anio;
                strMsgMail += " \n Resultado: " + (string)hsTabl[objEjec.c_estado];
                strMsgMail += " \n Detalle Modulos";
                if (objEjec.c_modulo_tco == "PROYECTOS")
                {
                    strMsgMail += "\n Resultado de actualizacion Modulo Proyectos (BaytonTec): " + (string)hsTabl[objEjec_Proy.c_estado];
                    NomadBatch.Trace(" DDO Proy guardado");
                }
                else
                {

                    if (objEjec_Eva.DETALLE.Count > 0)
                    {
                        if (objEjec_Eva.c_estado == "E") { objEjec_Eva.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
                        objEjec_Eva.f_fin_ejec = DateTime.Now;
                        objEjec_Eva.c_mes_anio = objEjec.c_mes_anio;
                        strMsgMail += "\n Resultado de actualizacion Modulo Evaluacion: " + (string)hsTabl[objEjec_Eva.c_estado];
                        //   strMsgMail += "\n Abarca los siguientes items de menú: " + strMenuEvaluacion;
                        logError = "objEjec_Eva :" + objEjec_Eva.SerializeAll();
                        NomadEnvironment.GetCurrentTransaction().Save(objEjec_Eva);

                        NomadBatch.Trace(" DDO Evaluacion guardado");
                    }

                    if (objEjec_Horas.DETALLE.Count > 0)
                    {

                        if (objEjec_Horas.c_estado == "E") { objEjec_Horas.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
                        objEjec_Horas.f_fin_ejec = DateTime.Now;
                        objEjec_Horas.c_mes_anio = objEjec.c_mes_anio;
                        logError = "objEjec_Horas :" + objEjec_Horas.SerializeAll();
                        NomadEnvironment.GetCurrentTransaction().Save(objEjec_Horas);
                        strMsgMail += "\n Resultado de actualizacion Modulo Horas: " + (string)hsTabl[objEjec_Horas.c_estado];
                        //  strMsgMail += "\n Abarca los siguientes items de menú: " + strMenuHoras;
                        NomadBatch.Trace(" DDO Horas guardado");
                    }

                    if (objEjec_Pers.DETALLE.Count > 0)
                    {
                        if (objEjec_Pers.c_estado == "E") { objEjec_Pers.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
                        objEjec_Pers.f_fin_ejec = DateTime.Now;
                        objEjec_Pers.c_mes_anio = objEjec.c_mes_anio;
                        logError = "objEjec_Pers :" + objEjec_Pers.SerializeAll();
                        NomadEnvironment.GetCurrentTransaction().Save(objEjec_Pers);
                        strMsgMail += "\n Resultado de actualizacion Modulo Personal: " + (string)hsTabl[objEjec_Pers.c_estado];
                        //     strMsgMail += "\n Abarca los siguientes items de menú: " + strMenuPersonal;
                        NomadBatch.Trace(" DDO Personal guardado");
                    }

                    if (objEjec_Sueldo.DETALLE.Count > 0)
                    {
                        if (objEjec_Sueldo.c_estado == "E") { objEjec_Sueldo.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
                        objEjec_Sueldo.f_fin_ejec = DateTime.Now;
                        objEjec_Sueldo.c_mes_anio = objEjec.c_mes_anio;
                        logError = "objEjec_Sueldo :" + objEjec_Sueldo.SerializeAll();
                        NomadEnvironment.GetCurrentTransaction().Save(objEjec_Sueldo);
                        strMsgMail += "\n Resultado de actualizacion Modulo Sueldos: " + (string)hsTabl[objEjec_Sueldo.c_estado];
                        //     strMsgMail += "\n Abarca los siguientes items de menú: " + strMenuSueldos;

                        NomadBatch.Trace(" DDO Sueldo guardado");
                    }

                    if (objEjec_Estr.DETALLE.Count > 0)
                    {
                        if (objEjec_Estr.c_estado == "E") { objEjec_Estr.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
                        objEjec_Estr.f_fin_ejec = DateTime.Now;
                        objEjec_Estr.c_mes_anio = objEjec.c_mes_anio;
                        logError = "objEjec_Estr :" + objEjec_Estr.SerializeAll();
                        NomadEnvironment.GetCurrentTransaction().Save(objEjec_Estr);

                        NomadBatch.Trace(" DDO objEjec_Estr guardado");
                    }

                    if (objEjec_Proy.DETALLE.Count > 0)
                    {

                        if (objEjec_Proy.c_estado == "E") { objEjec_Proy.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
                        objEjec_Proy.f_fin_ejec = DateTime.Now;
                        objEjec_Proy.c_mes_anio = objEjec.c_mes_anio;
                        logError = "objEjec_Proy :" + objEjec_Proy.SerializeAll();
                        NomadEnvironment.GetCurrentTransaction().Save(objEjec_Proy);
                        strMsgMail += "\n Resultado de actualizacion Modulo Proyectos: " + (string)hsTabl[objEjec_Proy.c_estado];
                        //     strMsgMail += "\n Abarca los siguientes items de menú: " + strMenuProy;
                        NomadBatch.Trace(" DDO Proy guardado");
                    }
                }
                NomadBatch.Trace(" strMsgMail " + strMsgMail);

                NomadBatch.Trace(" Sale fncSaveDDO");
            }

            catch (Exception e)
            {
                objBatch.Err(" Error en Grabacion Final de datos  ");
                NomadBatch.Trace(" Error en fncSaveDDO " + e.Message);
                NomadBatch.Trace(" DDO " + logError);

                //fncLogearHas(" Error en fncSaveDDO : " + e.Message);
            }

        }
        private string fncClearString(string sValue)
        {
            string rta;
            rta = sValue.Replace("'", "");
            return rta;

        }
        private void fncSendMail(NomadXML xmlMails)
        {
            try
            {

                Nomad.Base.Mail.Grupos_Mail_Usuario.GRUPOS_MAIL gp_mail;
                gp_mail = Nomad.Base.Mail.Grupos_Mail_Usuario.GRUPOS_MAIL.Get("INDICADORES");//

                gp_mail.EnviaMails("NucleusRH - Actualizacion de Indicadores", strMsgMail);

                Nomad.NSystem.Base.NomadBatch.Trace("********* ENVIO LOS MAILS**************");

            }
            catch (Exception e)
            {
                objBatch.Err(" Error en el envío del correo ");
                NomadBatch.Trace(" Error en fncSendMail " + e.Message);

            }

        }

    }

}



using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Configuration;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Indicadores.Ejecuciones
{
    class clsDataBase
    {
        protected static OleDbConnection myConn = null;
        protected static NomadBatch objBatchDB;
        OleDbDataAdapter objDataAdap;
        protected static string sConnection;
        public string sMsgError;
        enum Tipo_Periodo { Mes = 1, Bimestre, Trimestre, CuaTrimestre, Semestre };

        public clsDataBase()
        {
        }

        public clsDataBase(string sConn, NomadBatch nmdBatch)
        {

            sConnection = sConn;
            objBatchDB = nmdBatch;
        }
        //
        private static void AttachParameters(OleDbCommand command, OleDbParameter[] commandParameters)
        {
            string sValue;
            string sType;
            sValue = "";
            sType = "";
        }
        private static void AttachParameters(OleDbCommand command, Hashtable hsColumns, Hashtable hsColumnsValue, Array orderPar)
        {
            if (command == null) throw new ArgumentNullException("command");

            int nItem;
            string sValue;
            string sType;
            sValue = "";
            sType = "";
            nItem = 0;
            OleDbParameter OleDB_param;
            OleDbType objDBType;
            try
            {
                for (int i_item = 0; i_item < orderPar.Length; i_item++)
                {
                    string key;
                    key = ((string[])(orderPar))[i_item];
                    key = key.Replace(" ", "");
                    sType = (string)hsColumns[key];
                    sValue = (string)"" + hsColumnsValue[key] + "";
                    objDBType = OleDbType.VarChar ;
                    if (key == "")
                    {
                        continue;
                    }

                    object objValue = null;

                    switch (sType)
                    {
                        case "DATE":        //Dentro de Convenio mensual
                            if (sValue == null) { sValue = ""; }
                            objValue = StringUtil.str2date(sValue);
                            objDBType = OleDbType.Date;
                            break;
                        case "INT":        //Dentro de Convenio mensual
                            objDBType = OleDbType.Integer;
                            if (sValue == null) { sValue = "0"; }
                            objValue = Int32.Parse(sValue);
                            break;
                        case "FLOAT":        //Dentro de Convenio mensual
                            if (sValue == null) { sValue = "0"; }
                            objDBType = OleDbType.Numeric;
                            objValue = StringUtil.str2dbl(sValue);
                            break;
                        case "STR":        //Dentro de Convenio mensual
                            if (sValue == "NULL") { sValue = ""; }
                            //objDBType = OleDbType.;
                            objValue = sValue.TrimEnd(' ');
                            break;

                    }
                    if (sValue == "NULL") { sValue = ""; }
                    if (objValue == null)
                    {
                        NomadBatch.Trace(" ERROR SQL en AttachParameters key='" + key + "' objValue Nulo  Tipo='" + sType + "'");
                        continue;
                    }
                    OleDB_param = new OleDbParameter(key, objDBType);
                    OleDB_param.Direction = ParameterDirection.Input;
                    //OleDB_param.Value =sValue;
                    OleDB_param.Value = objValue;

                    command.Parameters.Add(OleDB_param);

                }
            }
            catch (Exception e)
            {
                NomadBatch.Trace(" ERROR SQL en AttachParameters");
                NomadBatch.Trace(e.Message);
            }
        }
        //

        public string ChangeConnectDB(string sConnectionNew)
        {
            try
            {
                CloseConn();
                string sConnActual;
                sConnActual = sConnection;
                sConnection = sConnectionNew;
                return sConnActual;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        // Abre la conecion a la base de datos
        private static string ConnectDB()
        {
            try
            {
                myConn = new OleDbConnection(sConnection);
                myConn.Open();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        // Cierra la conecion a la base de datos
        private static void CloseConn()
        {
            myConn.Close();
        }

        public DataSet ExecDataSet(string strSQL)
        {
            string msgConection;
            strSQL = strSQL.ToUpper();
            sMsgError = "";
            msgConection = ConnectDB();
            if (msgConection != "")
            {

                objBatchDB.Err(" Error en la conexion de la base de datos ");
                NomadBatch.Trace(" Error en la conexion de la base de datos ");
                NomadBatch.Trace(msgConection);
            }
            else
            {
                //objDB.InitProcess;

            }

            objDataAdap = new OleDbDataAdapter(strSQL, myConn);

            DataSet objDS;
            objDS = new DataSet();
            objDataAdap.Fill(objDS);
            objDataAdap.FillSchema(objDS, SchemaType.Mapped);
            //solo de prueba BORRAR ********************************

            DataTable objTblIndic;
            objTblIndic = objDS.Tables[0];
            return objDS;
        }
        public void SaveDataSet(DataSet dtIndic)
        {
            objDataAdap.Update(dtIndic);
            dtIndic.AcceptChanges();
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(commandType, commandText, (OleDbParameter[])null);
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText, Hashtable hsColumns, Hashtable hsColumnsValue, Array orderPar)
        {
            sMsgError = "";

            int retval = 0;
            try
            {
                if (commandText == "") { return 0; }
                commandText = commandText.ToUpper();
                string msgConection;
                msgConection = ConnectDB();
                if (msgConection != "")
                {
                    objBatchDB.Err(" Error en la conexion de la base de datos ");
                    NomadBatch.Trace(" Error en la conexion de la base de datos ");
                    NomadBatch.Trace(msgConection);
                    CloseConn();
                    return -1;
                }
                else
                {
                    //objDB.InitProcess;

                }

                OleDbCommand cmd = new OleDbCommand();

                PrepareCommand(cmd, myConn, commandType, commandText, hsColumns, hsColumnsValue, orderPar);

                retval = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            catch (Exception e)
            {
                //Mensaje solo al archivo

                string tablaTipo;
                string tablaErr;
                string paramsErr;
                Array arrErr;
                if (commandText.ToUpper().IndexOf("INSERT INTO") != -1)
                { tablaTipo = " Insertar "; }
                else { tablaTipo = " Modificar "; }

                // reemplaza para loguear la tabla en el error
                tablaErr = commandText.Replace("INSERT INTO ", "");
                tablaErr = tablaErr.Replace("UPDATE ", "");
                tablaErr = tablaErr.Replace("DELETE FROM ", "");
                tablaErr = tablaErr.Replace("DELETE ", "");
                arrErr = tablaErr.Split(' ');
                tablaErr = ((string[])(arrErr))[0];
                if (tablaErr == "") { tablaErr = ((string[])(arrErr))[1]; }
                objBatchDB.Err("Error al " + tablaTipo + " la tabla " + tablaErr);
                paramsErr = "";
                foreach (DictionaryEntry item in hsColumnsValue)
                { paramsErr += item.Key + " :" + item.Value; }

                NomadBatch.Trace(" ERROR SQL en ExecuteNonQuery");
                NomadBatch.Trace(e.Message);
                NomadBatch.Trace(commandText);
                NomadBatch.Trace(paramsErr);

                sMsgError = e.Message + " SQL:" + commandText;
                CloseConn();
                return -1;
                //throw;
            }
            CloseConn();
            return retval;

        }

        public int ExecuteNonQuery(CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            sMsgError = "";

            int retval = 0;
            try
            {
                if (commandText == "") { return 0; }
                commandText = commandText.ToUpper();
                string msgConection;
                msgConection = ConnectDB();
                if (msgConection != "")
                {
                    objBatchDB.Err(" Error en la conexion de la base de datos ");
                    NomadBatch.Trace(" Error en la conexion de la base de datos ");
                    NomadBatch.Trace(msgConection);
                    CloseConn();
                    return -1;
                }
                else
                {
                    //objDB.InitProcess;

                }

                OleDbCommand cmd = new OleDbCommand();

                PrepareCommand(cmd, myConn, commandType, commandText, commandParameters);

                retval = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            catch (Exception e)
            {
                string tablaTipo;
                string tablaErr;

                Array arrErr;
                if (commandText.ToUpper().IndexOf("INSERT INTO") != -1)
                { tablaTipo = " Insertar "; }
                else { tablaTipo = " Modificar "; }

                // reemplaza para loguear la tabla en el error
                tablaErr = commandText.Replace("INSERT INTO ", "");
                tablaErr = tablaErr.Replace("UPDATE ", "");
                tablaErr = tablaErr.Replace("DELETE FROM ", "");
                tablaErr = tablaErr.Replace("DELETE ", "");
                arrErr = tablaErr.Split(' ');
                tablaErr = ((string[])(arrErr))[0];
                if (tablaErr == "") { tablaErr = ((string[])(arrErr))[1]; }
                objBatchDB.Err("Error al " + tablaTipo + " la tabla " + tablaErr);

                NomadBatch.Trace(" ERROR SQL en ExecuteNonQuery");
                NomadBatch.Trace(e.Message);
                NomadBatch.Trace(commandText);

                sMsgError = e.Message + " SQL:" + commandText;
                CloseConn();
                return -1;
                //throw;
            }
            CloseConn();
            return retval;
        }
        public static string fnc_MesesAnios(int nMes)
        {
            string strRta;
            strRta = "";
            string str_switch;
            str_switch = "";
            switch (nMes)
            {
                case 1: strRta = "'ENERO'" + ",'1'" + ",'1'" + ",'1'" + ",'1'";
                    break;
                case 2: strRta = "'FEBRERO'" + ",'1'" + ",'1'" + ",'1'" + ",'1'";
                    break;
                case 3: strRta = "'MARZO'" + ",'2'" + ",'1'" + ",'1'" + ",'1'";
                    break;
                case 4: strRta = "'ABRIL'" + ",'2'" + ",'2'" + ",'1'" + ",'1'";
                    break;
                case 5: strRta = "'MAYO'" + ",'3'" + ",'2'" + ",'2'" + ",'1'";
                    break;
                case 6: strRta = "'JUNIO'" + ",'3'" + ",'2'" + ",'2'" + ",'1'";
                    break;
                case 7: strRta = "'JULIO'" + ",'4'" + ",'3'" + ",'2'" + ",'2'";
                    break;
                case 8: strRta = "'AGOSTO'" + ",'4'" + ",'3'" + ",'2'" + ",'2'";
                    break;
                case 9: strRta = "'SEPTIEMBRE'" + ",'5'" + ",'3'" + ",'3'" + ",'2'";
                    break;
                case 10: strRta = "'OCTUBRE'" + ",'5'" + ",'4'" + ",'3'" + ",'2'";
                    break;
                case 11: strRta = "'NOVIEMBRE'" + ",'6'" + ",'4'" + ",'3'" + ",'2'";
                    break;
                case 12: strRta = "'DICIEMBRE'" + ",'6'" + ",'4'" + ",'3'" + ",'2'";
                    break;
                default:
                    strRta = "";

                    break;
            }
            return strRta;

        }

        public static void PrepareCommand(OleDbCommand command, OleDbConnection connection, CommandType commandType, string commandText,
                   OleDbParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        public static void PrepareCommand(OleDbCommand command, OleDbConnection connection, CommandType commandType, string commandText,
           Hashtable hsColumns, Hashtable hsColumnsValue, Array orderPar)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (hsColumns != null)
            {
                AttachParameters(command, hsColumns, hsColumnsValue, orderPar);
            }
            return;
        }
        public static void fncFindPersEmpr(int oiEmpresa, string fecha_Desde, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            strCampos = " PER01_PERSONAL.OI_PERSONAL, PER02_PERSONAL_EMP.E_NUMERO_LEGAJO,PER01_PERSONAL.C_SEXO,  PER02_PERSONAL_EMP.OI_PERSONAL_EMP,  PER01_PErsonal.d_ape_y_nom, ";
            strCampos += " PER01_PERSONAL.OI_ESTADO_CIVIL,  PER02_PERSONAL_EMP.OI_SINDICATO,  PER02_PERSONAL_EMP.OI_CATEGORIA_ULT, ";
            strCampos += " PER02_PERSONAL_EMP.N_ULT_REMUN,  PER02_PERSONAL_EMP.N_EST_PAS_MES,  PER01_PERSONAL.F_NACIM  , PER02_PERSONAL_EMP.OI_EMPRESA_VINC ";

            strTbl = " PER02_PERSONAL_EMP   , PER01_PERSONAL    ";
            strTbl += " JOIN  PER01_PERSONAL.OI_PERSONAL INNER PER02_PERSONAL_EMP.OI_PERSONAL ";
            strWHERE = " PER02_PERSONAL_EMP.OI_EMPRESA =" + oiEmpresa;
            strWHERE += " AND (ISNULL(PER02_PERSONAL_EMP.F_EGRESO) ";
            strWHERE += " OR PER02_PERSONAL_EMP.F_EGRESO &Gt;  {Datetime(\\'" + fecha_Desde + "\\')} )";
        }

        public static void fncFindMaxFIng(int oi_personal_emp, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            strTbl = strTbl.Replace(" ", "");
            strCampos = "  MAX(" + strTbl + ".F_INGRESO) AS F_INGRESO ";
            strWHERE = strTbl + ".oi_personal_emp =  " + oi_personal_emp;
            strWHERE += " and  " + strTbl + ".F_INGRESO    &lt;  {Datetime(\\'" + fecha_hasta + "\\')}";

        }

        public static void fncFindPuesto(int oi_personal_emp, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //strCampos ="  PER02_PUESTO_PER.oi_puesto, PER02_PUESTO_PER.f_ingreso,  PER02_PUESTO_PER.f_egreso,ORG04_PUESTOS.c_puesto " ;
            strCampos = "  PER02_PUESTO_PER.OI_PUESTO ";
            strTbl = "  PER02_PUESTO_PER , ORG04_PUESTOS ";
            strTbl += " JOIN ORG04_PUESTOS.OI_PUESTO INNER PER02_PUESTO_PER.OI_PUESTO ";
            strWHERE = " PER02_PUESTO_PER.OI_PERSONAL_EMP = " + oi_personal_emp;
            if (fecha_hasta == "") { return; }
            strWHERE += " AND PER02_PUESTO_PER.F_INGRESO = " + " {Datetime(\\'" + fecha_hasta + "\\')}";

        }

        public static void fncFindPos(int oi_personal_emp, int oi_puesto, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //strCampos = " PER02_POSIC_PER.oi_posicion,PER02_POSIC_PER.F_INGRESO,PER02_POSIC_PER.f_egreso,ORG04_POSICIONES.c_posicion,ORG04_POSICIONES.oi_unidad_org ";
            strCampos = " PER02_POSIC_PER.OI_POSICION,ORG04_POSICIONES.OI_UNIDAD_ORG ";
            strTbl = "  PER02_POSIC_PER ,ORG04_POSICIONES   ";
            strTbl += " JOIN ORG04_POSICIONES.oi_posicion INNER PER02_POSIC_PER.OI_POSICION ";
            strWHERE = " PER02_POSIC_PER.OI_PERSONAL_EMP = " + oi_personal_emp;
            strWHERE += " AND ORG04_POSICIONES.OI_PUESTO = " + oi_puesto;
            if (fecha_hasta == "") { return; }
            strWHERE += " AND PER02_POSIC_PER.F_INGRESO = " + " {Datetime(\\'" + fecha_hasta + "\\')}";
        }

        public static void fncFindCat(int oi_personal_emp, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //strCampos = " PER02_CATEG_PER.oi_categoria,ORG18_CATEGORIAS.c_categoria,ORG18_CATEGORIAS.oi_convenio,ORG18_CONVENIOS.c_convenio,ORG18_CATEGORIAS.n_valor_hora ";
            strCampos = " PER02_CATEG_PER.OI_CATEGORIA,ORG18_CATEGORIAS.OI_CONVENIO,ORG18_CONVENIOS.C_CONVENIO,ORG18_CATEGORIAS.N_VALOR_HORA ";
            strTbl = "  PER02_CATEG_PER, ORG18_CATEGORIAS,  ORG18_CONVENIOS ";
            strTbl += " JOIN ORG18_CATEGORIAS.OI_CATEGORIA INNER PER02_CATEG_PER.OI_CATEGORIA";
            strTbl += " ,    ORG18_CONVENIOS.OI_CONVENIO INNER ORG18_CATEGORIAS.OI_CONVENIO";
            strWHERE = " PER02_CATEG_PER.OI_PERSONAL_EMP = " + oi_personal_emp;
            if (fecha_hasta == "") { return; }
            strWHERE += " AND PER02_CATEG_PER.F_INGRESO = " + " {Datetime(\\'" + fecha_hasta + "\\')}";

        }

        public static void fncFindCCos(int oi_personal_emp, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //strCampos = " PER02_CCOSTO_PER.oi_centro_costo, ORG08_CS_COSTO.c_centro_costo ";
            strCampos = " PER02_CCOSTO_PER.OI_CENTRO_COSTO ";
            strTbl = "  PER02_CCOSTO_PER ,ORG08_CS_COSTO   ";
            strTbl += " JOIN ORG08_CS_COSTO.OI_CENTRO_COSTO INNER PER02_CCOSTO_PER.OI_CENTRO_COSTO ";
            strWHERE = " PER02_CCOSTO_PER.OI_PERSONAL_EMP = " + oi_personal_emp;
            if (fecha_hasta == "") { return; }
            strWHERE += " AND PER02_CCOSTO_PER.F_INGRESO = " + " {Datetime(\\'" + fecha_hasta + "\\')}";

        }

        public static void fncFindTPues(int oi_personal_emp, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            strCampos = " PER02_TIPOSP_PER.OI_TIPO_PERSONAL,PER11_TIPOS_PERS.C_TIPO_PERSONAL";
            strTbl = "  PER02_TIPOSP_PER ,PER11_TIPOS_PERS   ";
            strTbl += " JOIN PER11_TIPOS_PERS.OI_TIPO_PERSONAL INNER PER02_TIPOSP_PER.OI_TIPO_PERSONAL ";
            strWHERE = " PER02_TIPOSP_PER.OI_PERSONAL_EMP = " + oi_personal_emp;
            if (fecha_hasta == "") { return; }
            strWHERE += " AND PER02_TIPOSP_PER.F_INGRESO = " + " {Datetime(\\'" + fecha_hasta + "\\')}";

        }

        public static void fncFindEstOrg(string oi_personal_emp, string oi_unidad_org, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //ORG02_ESTRUCTURAS.oi_estructura,   **ORG02_ESTRUCTURAS.oi_clase,  ORG02_ESTRUC_PERS.oi_clase_org
            strCampos = " ORG02_ESTRUCTURAS.OI_ESTRUCTURA, ORG02_ESTRUC_PERS.OI_CLASE_ORG ";
            strTbl = " ORG02_ESTRUCTURAS , ORG02_ESTRUC_PERS   ";
            strTbl += "  JOIN ORG02_ESTRUC_PERS.OI_ESTRUCTURA INNER ORG02_ESTRUCTURAS.OI_ESTRUCTURA ";
            strWHERE = " ORG02_ESTRUC_PERS.OI_PERSONAL_EMP  = " + oi_personal_emp;
            strWHERE += " AND ORG02_ESTRUCTURAS.OI_UNIDAD_ORG = " + oi_unidad_org;

        }

        public static void fncFindEstFunc(string oi_personal_emp, int c_clase_org, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            strCampos = " ORG02_ESTRUC_PERS.OI_ESTRUCTURA as OI_ESTRUCTURA_F ";
            strTbl = " ORG02_ESTRUC_PERS  ";
            strWHERE = " ORG02_ESTRUC_PERS.OI_PERSONAL_EMP  = " + oi_personal_emp;
            strWHERE += " and ORG02_ESTRUC_PERS.OI_CLASE_ORG =" + c_clase_org;

        }
        public static void fncFindEstOrgEmp(string c_empresa, ref string strCampos, ref string strTbl, ref string strWHERE)
        {   // no encontro la estructura asociada a la persona , busca la de la empresa para ubicarla en Closoure "sin asingnar"
            strCampos = " ORG02_CLASES_ORG.OI_ESTRUCTURA_ORG as OI_ESTRUCTURA ";
            strTbl = " ORG02_CLASES_ORG ";
            strWHERE = " ORG02_CLASES_ORG.C_CLASE_ORG  =\\'" + c_empresa + "\\'";
        }

        //=============================================================================
        /// SOLO PARA FactEvaluacion *****************************

        public static void fncEvaluaciones(int oi_personal_emp, int oi_unidad_org, ref string strCampos, ref string strTbl, ref string strWHERE, string sFecha_desde, string sFecha_Hasta)
        {
            strCampos = " EVA02_EVALUACION.OI_EVENTO  AS OI_EVENTO  ,ORG21_COMPETENCIAS.OI_COMPETENCIA AS  OI_COMPETENCIA ";
            //strCampos += " , EVA02_EVALUACION.N_RESULTADO AS RESULT_EVAL, EVA02_COM_EVALU.N_RESULTADO   AS RESULT_COMP ";
            strCampos += " , EVA02_EVALUACION.N_RESULTADO AS N_RESULT_EVAL, EVA02_COM_EVALU.N_RESULTADO   AS N_RESULT_COMP , 1 AS CANTIDAD ";
            strTbl = " EVA02_EVALUACION , EVA01_EVENTOS  , EVA02_COM_EVALU   , ORG21_COMPETENCIAS   ";
            strTbl += "  JOIN EVA01_EVENTOS.OI_EVENTO INNER EVA02_EVALUACION.OI_EVENTO ";
            strTbl += " ,   EVA02_COM_EVALU.OI_EVALUACION INNER EVA02_EVALUACION.OI_EVALUACION   ";
            strTbl += " ,   EVA02_COM_EVALU.OI_COMPETENCIA INNER ORG21_COMPETENCIAS.OI_COMPETENCIA    ";
            strWHERE = " EVA02_EVALUACION.OI_PERSONAL_EMP  = " + oi_personal_emp;
            strWHERE += " AND EVA02_EVALUACION.C_ESTADO = \\'CE\\'  ";
            strWHERE += " AND EVA01_EVENTOS.F_DESDE &gt; {Datetime(\\'" + sFecha_desde + "\\')}";
            strWHERE += " AND EVA01_EVENTOS.F_DESDE &lt; {Datetime(\\'" + sFecha_Hasta + "\\')}";

        }

        public static void fncCamposFactEval(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = "OI_PERSONAL_EMP,OI_ANIOMES,OI_EMPRESA,OI_ESTRUCTURA,OI_CLASE_ORG,OI_TIPO_PERSONAL,";
            strCamposUnicos += "OI_CONVENIO,OI_CATEGORIA,OI_PUESTO,OI_POSICION,OI_CENTRO_COSTO,OI_SINDICATO,OI_ESTADO_CIVIL,OI_SEXO";
            // estos campos son los que inserta los campos anteriores tanta veces como existan
            // Ejemplo: se tiene un conjunto de 10 registros (oi_evento,oi_competencia), se insertaran 10 strCampos + strCamposUnicos (que son unicos)
            // para cada campos se inserta campos + strCamposUnicos
            strCampos = "OI_EVENTO,OI_COMPETENCIA";

            strCamposMedida = "CANTIDAD,N_RESULT_EVAL,N_RESULT_COMP";
            strTbl = " TCO_FACT_EVAL ";

        }

        //=============================================================================
        /// SOLO PARA FactHoras *****************************
        public static void fncFactHoras(int oi_personal_emp, ref string strCampos, ref string strTbl, ref string strWHERE, string fecha_desde, string fecha_hasta)
        {

            strCampos = "  TTA13_THS_GRUPO.OI_GRUPO_TH, TTA13_THS_GRUPO.OI_TIPOHORA, SUM( TTA10_LIQUIDACPROC.N_CANTIDADHS) AS N_CANTIDADHS ";
            strTbl = " TTA10_LIQUIDACPROC ,TTA10_LIQUIDACJOR , TTA10_LIQUIDACPERS, TTA13_THS_GRUPO ";
            strTbl += " JOIN TTA10_LIQUIDACJOR.OI_LIQUIDACJOR INNER TTA10_LIQUIDACPROC.OI_LIQUIDACJOR ";
            strTbl += " ,  TTA10_LIQUIDACPERS.OI_LIQUIDACIONPERS INNER  TTA10_LIQUIDACJOR.OI_LIQUIDACIONPERS ";
            strTbl += " ,  TTA13_THS_GRUPO.OI_TIPOHORA RIGHT OUTER  TTA10_LIQUIDACPROC.OI_TIPOHORA ";

            strWHERE = "  TTA10_LIQUIDACJOR.F_FECJORNADA &gt; {Datetime(\\'" + fecha_desde + "\\')}";
            strWHERE += " AND TTA10_LIQUIDACJOR.F_FECJORNADA &lt; {Datetime(\\'" + fecha_hasta + "\\')}";
            strWHERE += " AND  TTA10_LIQUIDACPERS.OI_PERSONAL_EMP = " + oi_personal_emp;
            strWHERE += " GROUP BY TTA13_THS_GRUPO.OI_GRUPO_TH,  TTA13_THS_GRUPO.OI_TIPOHORA";

        }

        // UTILIZADO PARA EL PASO FINAL**************************
        public static void fncCamposFactHoras(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = "OI_PERSONAL_EMP,OI_ANIOMES,OI_EMPRESA,OI_ESTRUCTURA,OI_CLASE_ORG,OI_TIPO_PERSONAL,OI_CONVENIO,OI_CATEGORIA,";
            strCamposUnicos += "OI_PUESTO,OI_POSICION,OI_CENTRO_COSTO,OI_SINDICATO,OI_ESTADO_CIVIL,OI_SEXO,OI_EMPRESA_VINC, OI_ESTRUCTURA_F";

            strCampos = "OI_GRUPO_TH,OI_TIPOHORA";

            strCamposMedida = "N_CANTIDADHS";
            strTbl = "TCO_FACT_HORAS";

        }

        //=============================================================================
        /// SOLO PARA FactPersonal *****************************

        public static void fncGetHasTiposHoras(ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            strCampos = " TTA01_TIPOHORAS.OI_TIPOHORA, TTA01_TIPOHORAS.OI_LICENCIA  ";
            strTbl = "  TTA01_TIPOHORAS , TTA13_THS_GRUPO ";
            strTbl += " JOIN  TTA01_TIPOHORAS.OI_TIPOHORA INNER TTA13_THS_GRUPO.OI_TIPOHORA ";
            strWHERE = " TTA13_THS_GRUPO.OI_GRUPO_TH=3 ";

        }

        public static void fncFindHsAusencia(int oi_personal_emp, string fecha_desde, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE, string lstOI)
        {
            strCampos = " SUM(TTA10_LIQUIDACPROC.N_CANTIDADHS)  AS HORASAUSENCIA ";
            strTbl = "  TTA10_LIQUIDACJOR ,TTA10_LIQUIDACPERS,TTA10_LIQUIDACPROC ";
            strTbl += " JOIN TTA10_LIQUIDACPERS.OI_LIQUIDACIONPERS INNER TTA10_LIQUIDACJOR.OI_LIQUIDACIONPERS ";
            strTbl += " , TTA10_LIQUIDACPROC.OI_LIQUIDACJOR INNER TTA10_LIQUIDACJOR.OI_LIQUIDACJOR ";

            strWHERE = " TTA10_LIQUIDACJOR.L_ESPERADO = 1 AND TTA10_LIQUIDACJOR.L_PRESENTE=0 ";
            strWHERE += " AND TTA10_liquidacpers.OI_PERSONAL_EMP = " + oi_personal_emp;
            strWHERE += " AND TTA10_liquidacproc.OI_TIPOHORA IN ( " + lstOI + ")";
            strWHERE += " AND TTA10_liquidacproc.F_FECHORAENTRADA &gt; {Datetime(\\'" + fecha_desde + "\\')}";
            strWHERE += " AND TTA10_liquidacproc.F_FECHORAENTRADA &lt; {Datetime(\\'" + fecha_hasta + "\\')}";
        }

        public static void fncFindHsEsperado(int oi_personal_emp, string fecha_desde, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE, string lstOI)
        {
            strCampos = " SUM(TTA10_LIQUIDACPROC.N_CANTIDADHS)  AS  HORASESPERADO ";
            strTbl = "  TTA10_LIQUIDACJOR ,TTA10_LIQUIDACPERS , TTA10_LIQUIDACPROC  ";
            strTbl += " JOIN TTA10_LIQUIDACPERS.OI_LIQUIDACIONPERS INNER TTA10_LIQUIDACJOR.OI_LIQUIDAcionpers ";
            strTbl += ", TTA10_LIQUIDACPROC.OI_LIQUIDACJOR INNER  TTA10_LIQUIDACJOR.OI_LIQUIDACJOR ";

            strWHERE = " TTA10_LIQUIDACJOR.L_ESPERADO = 1   ";
            strWHERE += " AND TTA10_LIQUIDACPERS.OI_PERSONAL_EMP = " + oi_personal_emp;
            strWHERE += " AND TTA10_LIQUIDACPROC.OI_TIPOHORA IN ( " + lstOI + ")";
            strWHERE += " AND TTA10_LIQUIDACPROC.F_FECHORAENTRADA &gt; {Datetime(\\'" + fecha_desde + "\\')}";
            strWHERE += " AND TTA10_LIQUIDACPROC.F_FECHORAENTRADA &lt; {Datetime(\\'" + fecha_hasta + "\\')}";
        }

        public static void fncFindHsCapacitacion(int oi_personal_emp, string fecha_desde, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //strCampos = "SUM(DATEDIFF( CYD02_CLASES_DICT.f_fecha_hora_ini, CYD02_CLASES_DICT.f_fecha_hora_fin)) as HsCapacitacion  ";
            strCampos = " CYD02_CLASES_DICT.F_FECHA_HORA_INI, CYD02_CLASES_DICT.F_FECHA_HORA_FIN ";
            strTbl = "  CYD02_DICTADOS,CYD02_INSCRIPTOS  ,CYD02_CLASES_DICT ";
            strTbl += " JOIN CYD02_DICTADOS.OI_DICTADO INNER CYD02_INSCRIPTOS.OI_DICTADO  ";
            strTbl += ", CYD02_DICTADOS.OI_DICTADO INNER CYD02_CLASES_DICT.OI_DICTADO  ";

            strWHERE = "  CYD02_DICTADOS.C_ESTADO = \\'F\\'  ";
            strWHERE += " AND  CYD02_INSCRIPTOS.OI_PERSONAL_EMP = " + oi_personal_emp;
            strWHERE += " AND  (CYD02_DICTADOS.L_EVALUA = 0 OR CYD02_INSCRIPTOS.C_ESTADO_APROB = \\'A\\') ";
            strWHERE += " AND  (CYD02_DICTADOS.L_CONTROLAR_ASIST = 0 OR CYD02_INSCRIPTOS.C_ESTADO = \\'CUR\\') ";
            strWHERE += " AND   CYD02_CLASES_DICT.F_FECHA_HORA_INI &gt; {Datetime(\\'" + fecha_desde + "\\')}";
            strWHERE += " AND   CYD02_CLASES_DICT.F_FECHA_HORA_INI &lt; {Datetime(\\'" + fecha_hasta + "\\')}";
        }

        public static void fncFindAntiguedad(int oi_personal_emp, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            strCampos = "  PER02_PERSONAL_EMP.F_ANTIGUEDAD_REC ,PER02_PERSONAL_EMP.F_INGRESO ";
            strTbl = "  PER02_PERSONAL_EMP ";
            strWHERE = " PER02_PERSONAL_EMP.OI_PERSONAL_EMP = " + oi_personal_emp;

        }

        public static void fncFindIngreso(int oi_personal_emp, string fecha_desde, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE, string lstOI)
        {
            strCampos = " PER02_PERSONAL_EMP.F_INGRESO ";
            strTbl = "  PER02_PERSONAL_EMP ";
            strWHERE = " PER02_PERSONAL_EMP.OI_PERSONAL_EMP = " + oi_personal_emp;
            strWHERE += " AND PER02_PERSONAL_EMP.F_INGRESO &gt; {Datetime(\\'" + fecha_desde + "\\')}";
            strWHERE += " AND PER02_PERSONAL_EMP.F_INGRESO &lt; {Datetime(\\'" + fecha_hasta + "\\')}";

        }

        public static void fncFindEgreso(int oi_personal_emp, string fecha_desde, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE, string lstOI)
        {
            strCampos = " PER02_PERSONAL_EMP.F_EGRESO , PER02_PERSONAL_EMP.OI_MOTIVO_EG_PER";
            strTbl = "  PER02_PERSONAL_EMP ";
            strWHERE = " PER02_PERSONAL_EMP.OI_PERSONAL_EMP = " + oi_personal_emp;
            strWHERE += " AND PER02_PERSONAL_EMP.F_EGRESO &gt; {Datetime(\\'" + fecha_desde + "\\')}";
            strWHERE += " AND PER02_PERSONAL_EMP.F_EGRESO &lt; {Datetime(\\'" + fecha_hasta + "\\')}";

        }

        // UTILIZADO PARA EL PASO FINAL**************************
        public static void fncCamposFactPersonal(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = "OI_PERSONAL_EMP,OI_ANIOMES, OI_EMPRESA, OI_ESTRUCTURA,OI_CLASE_ORG,OI_TIPO_PERSONAL, OI_CONVENIO,";
            strCamposUnicos += "OI_CATEGORIA, OI_PUESTO, OI_POSICION, OI_CENTRO_COSTO, OI_SINDICATO, OI_ESTADO_CIVIL, OI_SEXO,";
            strCamposUnicos += "OI_MOTIVO_EG_PER,OI_EDAD";
            //strCamposUnicos += ",CANTIDAD_PERS, REMUN_PERS,EDAD_PROMEDIO,HORASAUSENCIA,HORASESPERADO,";
            //strCamposUnicos += "HsCapacitacion,Antiguedad,Ingreso ,Egreso";

            strCampos = "";

            strCamposMedida = "CANTIDAD_PERS, REMUN_PERS,EDAD_PROMEDIO,HORASAUSENCIA,HORASESPERADO,";
            strCamposMedida += "HSCAPACITACION,ANTIGUEDAD,INGRESO ,EGRESO";

            strTbl = "TCO_FACT_PERSONAL";
        }

        //=============================================================================
        // De este query parte el proceso para la actualizacion de la FactSueldos **************************
        public static void fncFirstSQLSueldos(string sAnioMes, ref string strCampos, ref string strTbl, ref string strWHERE, int oi_empre)
        {
            strCampos = "  LIQ20_TOT_LIQ_PER.OI_PER_EMP_DDO, ORG03_EMPRESAS.OI_EMPRESA ,ORG03_EMPRESAS.C_EMPRESA , ";
            strCampos += "  LIQ99_LIQUIDACION.E_QUINCENA, LIQ99_LIQUIDACION.E_PERIODO, LIQ99_LIQUIDACION.E_PERIODO_RET ";
            strCampos += "  , LIQ99_LIQUIDACION.F_CIERRE, LIQ99_LIQUIDACION.C_TIPO_LIQ,LIQ99_LIQUIDACION.C_CLASE";
            strCampos += "  , LIQ20_TOT_LIQ_PER.N_TOTPAG, LIQ20_TOT_LIQ_PER.N_TOTSRET, LIQ20_TOT_LIQ_PER.N_TOTNSRET";
            strCampos += "  , LIQ20_TOT_LIQ_PER.N_TOTASIG, LIQ20_TOT_LIQ_PER.N_TOTSAC, LIQ20_TOT_LIQ_PER.N_RETEN";
            strCampos += "  , LIQ20_TOT_LIQ_PER.N_TOTCONT, LIQ20_TOT_LIQ_PER.N_TOTTICKET, LIQ20_TOT_LIQ_PER.N_TOTDESC";
            strCampos += "  , LIQ20_TOT_LIQ_PER.N_TOTANT";
            strCampos += "  , LIQ97_PERSONAL_EMP.E_NUMERO_LEGAJO , LIQ97_PERSONAL_EMP.C_TIPO_PERSONAL , LIQ97_PERSONAL_EMP.C_CONVENIO ";
            strCampos += "  , LIQ97_PERSONAL_EMP.C_PUESTO_ULT  , LIQ97_PERSONAL_EMP.C_POSICION_ULT , LIQ97_PERSONAL_EMP.C_CENTRO_COSTO_ULT ";
            strCampos += "  , LIQ97_PERSONAL_EMP.C_SINDICATO   , LIQ97_PERSONAL_EMP.C_ESTADO_CIVIL , LIQ97_PERSONAL_EMP.C_SEXO ";
            strCampos += "  , LIQ97_PERSONAL_EMP.C_CATEGORIA_ULT";

            strTbl = "  LIQ20_TOT_LIQ_PER, LIQ99_LIQUIDACION   , LIQ99_RECIBOS , LIQ97_PERSONAL_EMP , ORG03_EMPRESAS  ";
            strTbl += " JOIN LIQ20_TOT_LIQ_PER.OI_TOT_LIQ_PER INNER LIQ99_RECIBOS.OI_TOT_LIQ_PER ";
            strTbl += " , LIQ99_RECIBOS.OI_LIQUIDACION_DDO INNER LIQ99_LIQUIDACION.OI_LIQUIDACION_DDO  ";
            strTbl += " , LIQ20_TOT_LIQ_PER.OI_PER_EMP_DDO INNER LIQ97_PERSONAL_EMP.OI_PER_EMP_DDO   ";
            strTbl += " , LIQ97_PERSONAL_EMP.C_EMPRESA INNER ORG03_EMPRESAS.C_EMPRESA  ";
            strWHERE = " LIQ99_LIQUIDACION.C_ESTADO = \\'F\\' and   LIQ99_LIQUIDACION.E_PERIODO = " + sAnioMes;
            strWHERE += " and Org03_empresas.OI_EMPRESA=" + oi_empre.ToString();// agregado 13/07/2009
        }

        public static void fncFirstSQLSueldosCon(string sAnioMes, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            strCampos = "   LIQ20_TOT_LIQ_PER.OI_PER_EMP_DDO, L99.OI_EMPRESA_DDO, L99.E_QUINCENA, L99.E_PERIODO,  ";
            strCampos += " L99.E_PERIODO_RET, L99.F_CIERRE, L99.C_TIPO_LIQ, L99.C_CLASE, L99.C_LIQUIDACION,  ";
            strCampos += " L99.D_TITULO, L20.N_VALOR, L20.N_CANTIDAD, L14.OI_CONCEPTO, L14.OI_TIPO_CONCEPTO ";
            strTbl = "   LIQ20_TOT_LIQ_PER,   LIQ99_LIQUIDACION AS L99 ,  ";
            strTbl += "  LIQ20_CONC_LIQ_PER AS L20,  LIQ96_CONCEPTOS AS L96,  LIQ14_CONCEPTOS AS L14 ";
            strTbl += " JOIN  LIQ20_TOT_LIQ_PER.OI_LIQUIDACION_DDO INNER L99.OI_LIQUIDACION_DDO , ";
            strTbl += "  L20.OI_TOT_LIQ_PER INNER LIQ20_TOT_LIQ_PER.OI_TOT_LIQ_PER , ";
            strTbl += "  L20.OI_CONCEPTO_DDO INNER L96.OI_CONCEPTO_DDO , ";
            strTbl += "  L96.C_CONCEPTO INNER L14.C_CONCEPTO ";
            strWHERE = "  LIQ20_TOT_LIQ_PER.OI_TOT_LIQ_PER IN  ";
            strWHERE += " (SELECT LIQ99_RECIBOS.OI_TOT_LIQ_PER FROM LIQ99_LIQUIDACION, LIQ99_RECIBOS ";
            strWHERE += "  JOIN LIQ99_RECIBOS.OI_LIQUIDACION_DDO INNER LIQ99_LIQUIDACION.OI_LIQUIDACION_DDO  ";
            strWHERE += "   WHERE LIQ99_LIQUIDACION.C_ESTADO = \\'F\\') ";
            strWHERE += " AND L20.N_VALOR != 0 AND L20.N_CANTIDAD != 0 AND L14.OI_TIPO_CONCEPTO != 6 ";
            strWHERE += " and L99.E_PERIODO = \\'" + sAnioMes + "\\'";

        }

        public static void fncFindPuesto_Sueldo(int oi_empresa, string c_puesto, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            //      -- Toma el puesto correspondiente al período
            strCampos = "  ORG04_PUESTOS.OI_PUESTO ";
            strTbl = "  ORG04_PUESTOS  ";
            strWHERE = " ORG04_PUESTOS.C_PUESTO = \\'" + c_puesto + "\\'";
            strWHERE += " AND ORG04_PUESTOS.OI_EMPRESA= " + oi_empresa;

        }

        public static void fncFindPos_Sueldo(string oi_puesto, string c_posicion, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            //      -- Toma el puesto correspondiente al período
            strCampos = "  ORG04_POSICIONES.OI_POSICION , ORG04_POSICIONES.OI_UNIDAD_ORG AS OI_UNIDAD_ORG_PER ";
            strTbl = "  ORG04_POSICIONES ";
            strWHERE = " ORG04_POSICIONES.C_POSICION = \\'" + c_posicion + "\\'";
            strWHERE += " AND ORG04_POSICIONES.OI_PUESTO= " + oi_puesto;

        }

        public static void fncFindConv_Sueldo(string c_convenio, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            //      -- Toma el puesto correspondiente al período
            strCampos = "  Org18_CONVENIOS.OI_CONVENIO  ";
            strTbl = "  Org18_CONVENIOS ";
            strWHERE = " Org18_CONVENIOS.C_CONVENIO = \\'" + c_convenio + "\\'";
        }

        public static void fncFindCateg_Sueldo(string oi_convenio, string c_categoria, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            //      -- Toma la categoría correspondiente al período
            strCampos = "  ORG18_CATEGORIAS.OI_CATEGORIA  ";
            strTbl = "  ORG18_CATEGORIAS ";
            strWHERE = " ORG18_CATEGORIAS.C_CATEGORIA = \\'" + c_categoria + "\\'";
            strWHERE += " AND ORG18_CATEGORIAS.OI_CONVENIO = " + oi_convenio;

        }

        public static void fncFindCC_Sueldo(string c_centro_costo, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  ORG08_CS_COSTO.OI_CENTRO_COSTO  ";
            strTbl = "  ORG08_CS_COSTO  ";
            strWHERE = " ORG08_CS_COSTO.C_CENTRO_COSTO = \\'" + c_centro_costo + "\\'";
        }

        public static void fncFindTper_Sueldo(string c_tipo_personal, ref string strCampos, ref string strTbl, ref string strWHERE)
        {

            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  PER11_TIPOS_PERS.OI_TIPO_PERSONAL ";
            strTbl = "  PER11_TIPOS_PERS ";
            strWHERE = " PER11_TIPOS_PERS.C_TIPO_PERSONAL = \\'" + c_tipo_personal + "\\'";

        }

        public static void fncFindECivil_Sueldo(string c_estado_civil, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  ORG22_EST_CIVIL.OI_ESTADO_CIVIL  ";
            strTbl = "  ORG22_EST_CIVIL ";
            strWHERE = " ORG22_EST_CIVIL.C_ESTADO_CIVIL = \\'" + c_estado_civil + "\\'";
        }

        public static void fncFindSind_Sueldo(string c_sindicato, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  PER30_SINDICATOS.OI_SINDICATO  ";
            strTbl = "  PER30_SINDICATOS ";
            strWHERE = " PER30_SINDICATOS.C_SINDICATO = \\'" + c_sindicato + "\\'";
        }

        public static void fncFindPerEmp_Sueldo(string e_numero_legajo, int oi_empresa, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  PER02_PERSONAL_EMP.OI_PERSONAL_EMP  ";
            strTbl = "  PER02_PERSONAL_EMP ";
            strWHERE = " PER02_PERSONAL_EMP.E_NUMERO_LEGAJO = " + e_numero_legajo;
            strWHERE += " AND PER02_PERSONAL_EMP.OI_EMPRESA = " + oi_empresa;
        }

        public static void fncIns_SueldosPresup(string anioMes, ref string strSQL)
        {
            strSQL = " INSERT INTO DBO.TCO_FACT_SUELDOS  ";
            strSQL += " ( OI_PERSONAL_EMP,OI_ANIOMES,OI_EMPRESA, OI_ESTRUCTURA,OI_CLASE_ORG,OI_TIPO_PERSONAL,OI_CONVENIO,";
            strSQL += "    OI_CATEGORIA,OI_PUESTO, OI_POSICION, OI_CENTRO_COSTO,OI_SINDICATO,OI_ESTADO_CIVIL,OI_SEXO, ";
            strSQL += "    CANTIDAD_PERS , N_TOTPAG ,N_TOTSRET,N_TOTNSRET, N_TOTASIG , N_TOTSAC, N_RETEN ,N_TOTCONT ,";
            strSQL += "    N_TOTTICKET ,N_TOTDESC  ,N_TOTANT   ,N_PRESUPUESTO )";
            strSQL += " SELECT ";
            strSQL += " 0 AS OI_PERSONAL_EMP,  OI_ANIOMES,  OI_EMPRESA,0 AS OI_ESTRUCTURA  ,0 AS OI_CLASE_ORG  ,";
            strSQL += " 0 AS OI_TIPO_PERSONAL  , 0 AS OI_CONVENIO  , 0 AS OI_CATEGORIA  , 0 AS OI_PUESTO  , ";
            strSQL += " 0 AS OI_POSICION  , OI_CENTRO_COSTO AS OI_CENTRO_COSTO,0 AS OI_SINDICATO  , 0 AS OI_ESTADO_CIVIL  , ";
            strSQL += " 0 AS OI_SEXO  , 0 AS CANTIDAD_PERS,0 AS N_TOTPAG,   0 AS N_TOTSRET  ,   0 AS N_TOTNSRET  ,  ";
            strSQL += " 0 AS N_TOTASIG,0 AS N_TOTSAC    ,0 AS N_RETEN     ,0 AS N_TOTCONT   ,0 AS N_TOTTICKET   ,";
            strSQL += " 0 AS N_TOTDESC    ,0 AS N_TOTANT     ,N_PRESUPUESTO ";
            strSQL += " FROM TCO_PRESUP_COSTOS TCO ";
            strSQL += " WHERE   OI_ANIOMES = " + anioMes;
            strSQL = strSQL.ToUpper();
        }

        // UTILIZADO PARA EL PASO FINAL**************************
        public static void fncCamposFactSueldos(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = " OI_PERSONAL_EMP,OI_ANIOMES,OI_EMPRESA,OI_ESTRUCTURA,OI_CLASE_ORG,OI_TIPO_PERSONAL,OI_CONVENIO,";
            strCamposUnicos += "OI_CATEGORIA,OI_PUESTO,OI_POSICION,OI_CENTRO_COSTO,OI_SINDICATO,OI_ESTADO_CIVIL,OI_SEXO ";

            strCampos = "";

            strCamposMedida = "CANTIDAD_PERS ,N_TOTPAG ,N_TOTSRET,N_TOTNSRET,N_TOTASIG ,N_TOTSAC  ,N_RETEN   ,N_TOTCONT ,";
            strCamposMedida += "N_TOTTICKET ,N_TOTDESC  ,N_TOTANT   ,N_PRESUPUESTO";

            strTbl = "TCO_FACT_SUELDOS";
        }

        //=============================================================================

        public static void fncFindEstructNULL(ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  90000 + ORG02_CLASES_ORG.OI_ESTRUCTURA_ORG AS OI_ESTRUCTURA, ";
            strCampos += " ORG02_CLASES_ORG.OI_ESTRUCTURA_ORG AS OI_ESTR_PADRE, 1 AS DISTANCIA";
            strTbl = "  ORG02_CLASES_ORG ";
            strWHERE = "  CONTAINS(upper_case(ORG02_CLASES_ORG.C_CLASE_ORG like), \\' POS\\') ";

        }

        public static void fncFindEstruct(ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  ORG02_ESTRUCTURAS.OI_ESTRUCTURA, ORG02_ESTRUCTURAS.OI_ESTRUCTURA, 0 AS DISTANCIA";
            strTbl = "  ORG02_ESTRUCTURAS ";
            strWHERE = " 1 = 1 ";
        }

        public static void fncFindEstructRecursive(int MaxDist, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  TCO_ESTRUCTURAS_CLOSURE.OI_ESTR_PADRE AS OI_ESTR_PADRE,  TCO_ESTRUCTURAS.OI_ESTRUCTURA, (TCO_ESTRUCTURAS_CLOSURE.DISTANCIA + 1) AS  DISTANCIA ";
            strTbl = "  TCO_ESTRUCTURAS_CLOSURE  ,  TCO_ESTRUCTURAS   ";
            strWHERE = "  TCO_ESTRUCTURAS.OI_ESTR_PADRE = TCO_ESTRUCTURAS_CLOSURE.OI_ESTRUCTURA ";
            strWHERE += " AND TCO_ESTRUCTURAS_CLOSURE.DISTANCIA = " + MaxDist;
        }

        // UTILIZADO PARA EL PASO FINAL**************************
        public static void fncCamposFactEstruct(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = "  OI_ESTR_PADRE, OI_ESTRUCTURA ";
            strCampos = "";
            strCamposMedida = "DISTANCIA";
            strTbl = "TCO_ESTRUCTURAS_CLOSURE";
        }
        //**********************************************************************************************
        ////=============================================================================

        //public static void fncFindEstructNULL(ref string strCampos, ref string strTbl, ref string strWHERE)
        //{
        //    //      -- Toma el centro de costo correspondiente al período
        //    strCampos = "  90000 + org02_clases_org.OI_ESTRUCTURA_org as oi_estructura, ";
        //    strCampos += " org02_clases_org.OI_ESTRUCTURA_org as oi_estr_padre, 1 as distancia";
        //    strTbl = "  org02_clases_org ";
        //    strWHERE = "  CONTAINS(upper_case(org02_clases_org.c_clase_org like), \\' POS\\') ";

        //}

        //public static void fncFindEstruct(ref string strCampos, ref string strTbl, ref string strWHERE)
        //{
        //    //      -- Toma el centro de costo correspondiente al período
        //    strCampos = "  org02_estructuras.OI_ESTRUCTURA, org02_estructuras.OI_ESTRUCTURA, 0 as distancia";
        //    strTbl = "  org02_estructuras ";
        //    strWHERE = " 1 = 1 ";
        //}

        public static void fncFindEstProyRecursive(int MaxDist, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            //      -- Toma el centro de costo correspondiente al período
            strCampos = "  TCO_ESTRUC_PROY_CLOSURE.OI_PROYECTO_PARENT OI_PROYECTO_PARENT,  TCO_ESTRUC_PROYECTOS.OI_PROYECTO, (TCO_ESTRUC_PROY_CLOSURE.DISTANCIA + 1) AS  DISTANCIA ";
            strTbl = "  TCO_ESTRUC_PROY_CLOSURE  ,  TCO_ESTRUC_PROYECTOS   ";
            strWHERE = "  TCO_ESTRUC_PROYECTOS.OI_PROYECTO_PARENT = TCO_ESTRUC_PROY_CLOSURE.OI_PROYECTO ";
            strWHERE += " AND TCO_ESTRUC_PROY_CLOSURE.DISTANCIA = " + MaxDist;
        }

        //// UTILIZADO PARA EL PASO FINAL**************************
        public static void fncCamposFactEstProy(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = "  OI_PROYECTO_PARENT, OI_PROYECTO ";
            strCampos = "";
            strCamposMedida = "DISTANCIA";
            strTbl = "TCO_ESTRUC_PROY_CLOSURE";
        }
        ////**********************************************************************************************

        public static void fncFindDimProyRec(ref string strQry)
        {
            strQry = "SELECT OI_RECURSO, O_OBSERVACION, OI_PERSONAL, C_RECURSO, E_HPROM ";
            strQry += " FROM PRY05_RECURSOS";
            strQry = strQry.ToUpper();
        }

        public static void fncCamposDimProyRec(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = " OI_RECURSO,  OI_PERSONAL  ";
            strCampos = "";
            strCamposMedida = " C_RECURSO, E_HPROM ";
            strTbl = "TCO_RECURSOS";
        }

        public static void fncFindDimProyPerf(ref string strQry)
        {
            strQry = "SELECT OI_PERFIL, C_PERFIL, D_PERFIL ";

            strQry += " FROM PRY06_PERFILES";
            strQry = strQry.ToUpper();
        }

        public static void fncCamposDimProyPerf(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = "OI_PERFIL";
            strCampos = "";
            strCamposMedida = " C_PERFIL, D_PERFIL ";
            strTbl = "TCO_PERFILES";
        }

        public static void fncFindProyClosure(ref string strQry)
        {
            strQry = "SELECT OI_PROYECTO,ISNULL(OI_PROYECTO_PARENT,0) AS OI_PROYECTO_PARENT, OI_TIPO_PROYECTO ,  ";
            strQry += "  C_PROYECTO, D_PROYECTO, O_PROYECTO, C_ESTADO, OI_RECURSO ";
            strQry += " FROM  PRY01_PROYECTOS";
            strQry = strQry.ToUpper();
        }

        public static void fncCamposProyClosure(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = "OI_PROYECTO, OI_PROYECTO_PARENT, OI_TIPO_PROYECTO ";
            strCampos = "";
            strCamposMedida = " C_PROYECTO, D_PROYECTO, O_PROYECTO, C_ESTADO, OI_RECURSO  ";
            strTbl = "TCO_ESTRUC_PROYECTOS";
        }

        public static void fncFindHorasProy(string fecha_desde, string fecha_hasta, ref string strCampos, ref string strTbl, ref string strWHERE)
        {
            strCampos = "SELECT  PRY02_TAREAS_REC.OI_RECURSO AS OI_RECURSO, PRY04_TRABAJOS.OI_PERFIL AS OI_PERFIL , ";
            strCampos += " PRY02_TAREAS.OI_PRY_IMPUTA AS OI_PROYECTO, SUM(PRY04_TRABAJOS.E_DURACION * PRY03_UND_TIEMPO.E_MINUTOS ) AS HORAS_DECLARADAS ";
            strCampos += "  FROM PRY04_TRABAJOS ,  PRY03_UND_TIEMPO ,  PRY02_TAREAS_REC,  PRY02_TAREAS ";
            strCampos += " WHERE PRY04_TRABAJOS.OI_UNIDAD_TIEMPO = PRY03_UND_TIEMPO.OI_UNIDAD_TIEMPO ";
            strCampos += " AND  PRY04_TRABAJOS.OI_TAREA_REC = PRY02_TAREAS_REC.OI_TAREA_REC  ";
            strCampos += " AND  PRY02_TAREAS_REC.OI_TAREA = PRY02_TAREAS.OI_TAREA  ";
            strCampos += " AND   PRY04_TRABAJOS.F_FECHA  > '" + fecha_desde + "'";
            strCampos += "  AND PRY04_TRABAJOS.F_FECHA  < '" + fecha_hasta + "' ";
            strCampos += " GROUP BY PRY02_TAREAS_REC.OI_RECURSO, PRY04_TRABAJOS.OI_PERFIL ,PRY02_TAREAS.OI_PRY_IMPUTA ";

            strCampos = strCampos.ToUpper();

        }

        public static void fncCamposFactHorasProy(ref string strCamposUnicos, ref string strCampos, ref string strCamposMedida, ref string strTbl)
        {
            // sirve para armar los Insert o Update hacia la FACT
            strCamposUnicos = " OI_RECURSO , OI_ANIOMES  , OI_PROYECTO , OI_PERFIL ";
            strCampos = "";
            strCamposMedida = "HORAS_DECLARADAS ";
            strTbl = "TCO_FACT_PROYHORAS";
        }

        //=============================================================================

        public string fncInsDimEmpresas()
        {
            string strQry;
            strQry = " INSERT INTO TCO_EMPRESAS_VINC ";
            strQry += " ( OI_EMPRESA_VINC, C_EMPRESA_VINC, D_EMPRESA_VINC, DC_EMPRESA, C_CUIT, ";
            strQry += " D_CALLE, E_NUMERO, E_PISO, D_DEPARTAMENTO, OI_LOCALIDAD, TE_TELEFONO,     ";
            strQry += " D_DOMICILIO, F_INICIO_ACT, D_DOMIC_RADIC, OI_TIPO_DOC_AUT, E_NRO_DOC_AUT, ";
            strQry += " OI_PER_AUTORIZADO, OI_TIPO_EMPRESA, OI_UNIDAD_ORG, F_CESE_ACT, O_EMPRESA) ";
            strQry += " SELECT OI_EMPRESA, C_EMPRESA, D_EMPRESA, DC_EMPRESA, C_CUIT, D_CALLE, ";
            strQry += " E_NUMERO, E_PISO, D_DEPARTAMENTO, OI_LOCALIDAD, TE_TELEFONO, D_DOMICILIO, ";
            strQry += " F_INICIO_ACT, D_DOMIC_RADIC, OI_TIPO_DOC_AUT, E_NRO_DOC_AUT, ";
            strQry += " OI_PER_AUTORIZADO, OI_TIPO_EMPRESA, OI_UNIDAD_ORG, F_CESE_ACT, ";
            strQry += " O_EMPRESA ";
            strQry += " FROM TCO_EMPRESAS ";
            strQry += " WHERE L_CONTRATISTA = 0                ";
            strQry = strQry.ToUpper();
            return strQry;
        }

        public System.Collections.Hashtable GetHashDimension(string strSQL)
        {

            DataSet myDat;
            System.Collections.Hashtable hsCodes;
            hsCodes = new System.Collections.Hashtable();
            try
            {
                myDat = new DataSet();

                myDat = ExecDataSet(strSQL);

                DataTable objTblIndic;
                objTblIndic = myDat.Tables[0];

                for (int i = 0; i < objTblIndic.Rows.Count; i++)
                {
                    hsCodes.Add(objTblIndic.Rows[i][1].ToString(), objTblIndic.Rows[i][0].ToString());
                }
                CloseConn();
                return hsCodes;
            }

            catch (Exception e)
            {
                NomadBatch.Trace(" Error en GetHashDimension : " + e.Message);
                NomadBatch.Trace(" GetHashDimension strSQL: " + strSQL);

                objBatchDB.Err(" Error en GetHashDimension : ");
                CloseConn();
                return hsCodes;
            }
        }
    }

}



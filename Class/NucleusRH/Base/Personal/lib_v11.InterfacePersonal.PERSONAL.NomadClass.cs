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

namespace NucleusRH.Base.Personal.InterfacePersonal
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase FichadasIngresadas
    public partial class PERSONAL
    {
        public static void TraceMessage(string msg)
        {
            NomadEnvironment.GetTrace().Info(msg);
            NomadEnvironment.GetTraceBatch().Info(msg);
            NomadProxy.GetProxy().Batch().SetMessage(msg);
        }

        public static void TraceProgress(int pos, string msg)
        {
            if (pos != 0)
                NomadProxy.GetProxy().Batch().SetProgress(pos);
            if (msg != "")
            {
                NomadProxy.GetProxy().Batch().SetMessage(msg);
                TraceMessage(msg);
            }
        }

        public static void TraceInfo(string msg)
        {
            NomadEnvironment.GetTrace().Info(msg);
            NomadEnvironment.GetTraceBatch().Info(msg);
        }

        public static void TraceError(string msg)
        {
            NomadEnvironment.GetTrace().Error(msg);
            NomadEnvironment.GetTraceBatch().Error(msg);
        }

        public static void TraceWrn(string msg)
        {
            NomadEnvironment.GetTrace().Warning(msg);
            NomadEnvironment.GetTraceBatch().Warning(msg);
        }

        public static string FindOI(string TABLE, string COLRET, string COLFND, string COLVAL)
        {
            NomadXML retval = new NomadXML();
            NomadXML paramIn = new NomadXML();
            NomadXML param;

            param = paramIn.AddTailElement("DATA");
            param.SetAttr("TABLE", TABLE);
            param.SetAttr("COLRET", COLRET);
            param.SetAttr("COLFND", COLFND);
            param.SetAttr("COLVAL", COLVAL);

            retval.SetText(NomadProxy.GetProxy().SQLService().Get(PERSONAL.Resources.qry_findOI, paramIn.ToString()));
            return retval.FindElement("RESULT").GetAttr("ID");
        }

        public static string FindOI(string TABLE, string COLRET, string COLFND1, string COLVAL1, string COLFND2, string COLVAL2)
        {
            NomadXML retval = new NomadXML();
            NomadXML paramIn = new NomadXML();
            NomadXML param;

            param = paramIn.AddTailElement("DATA");
            param.SetAttr("TABLE", TABLE);
            param.SetAttr("COLRET", COLRET);
            param.SetAttr("COLFND1", COLFND1);
            param.SetAttr("COLVAL1", COLVAL1);
            param.SetAttr("COLFND2", COLFND2);
            param.SetAttr("COLVAL2", COLVAL2);

            retval.SetText(NomadProxy.GetProxy().SQLService().Get(PERSONAL.Resources.qry_findOI2, paramIn.ToString()));
            return retval.FindElement("RESULT").GetAttr("ID");
        }

        public static string AddPersona(string APELLIDO, string NOMBRES, string SEXO, string DOC_TIPO, string DOC_NUMERO)
        {
            string personalFound;
            NucleusRH.Base.Personal.Legajo.PERSONAL objPersona;
            NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER objDoc;
            string docTypeOID = FindOI("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", DOC_TIPO);

            if (docTypeOID == "")
            {
                TraceError("Tipo de Documento '" + DOC_TIPO + "' no encontrado....");
                return "";
            }

            personalFound = FindOI("PER01_DOCUM_PER", "oi_personal", "oi_tipo_documento", docTypeOID, "c_documento", DOC_NUMERO);
            if (personalFound != "")
            {
                TraceWrn("Ya existe una Persona con el Documento '" + DOC_TIPO + "-" + DOC_NUMERO + "'....");
                return personalFound;
            }

            objPersona = new NucleusRH.Base.Personal.Legajo.PERSONAL();
            objPersona.c_personal = DOC_TIPO + DOC_NUMERO;
            objPersona.d_apellido = APELLIDO;
            objPersona.d_nombres = NOMBRES;
            objPersona.d_ape_y_nom = APELLIDO + ", " + NOMBRES;
            objPersona.c_sexo = SEXO;
            objPersona.c_nro_documento = DOC_NUMERO;
            objPersona.oi_tipo_documento = docTypeOID;



            objDoc = new NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER();
            objDoc.c_documento = DOC_NUMERO;
            objDoc.oi_tipo_documento = docTypeOID;
            objPersona.DOCUM_PER.Add(objDoc);







            NomadEnvironment.GetCurrentTransaction().SaveRefresh(objPersona);
            return objPersona.Id;
        }


        public static string AddPersona(string APELLIDO, string NOMBRES, string SEXO, string DOC_TIPO, string DOC_NUMERO, DateTime F_NACIM, string PER_COD)
        {
            string personalFound;
            NucleusRH.Base.Personal.Legajo.PERSONAL objPersona;
            NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER objDoc;
            string docTypeOID = FindOI("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", DOC_TIPO);

            if (docTypeOID == "")
            {
                TraceError("Tipo de Documento '" + DOC_TIPO + "' no encontrado....");
                return "";
            }

            personalFound = FindOI("PER01_DOCUM_PER", "oi_personal", "oi_tipo_documento", docTypeOID, "c_documento", DOC_NUMERO);
            if (personalFound != "")
            {
                TraceWrn("Ya existe una Persona con el Documento '" + DOC_TIPO + "-" + DOC_NUMERO + "'....");
                return personalFound;
            }

            objPersona = new NucleusRH.Base.Personal.Legajo.PERSONAL();
            objPersona.c_personal = PER_COD;
            objPersona.d_apellido = APELLIDO;
            objPersona.d_nombres = NOMBRES;
            objPersona.d_ape_y_nom = APELLIDO + ", " + NOMBRES;
            objPersona.c_sexo = SEXO;
            objPersona.c_nro_documento = DOC_NUMERO;
            objPersona.oi_tipo_documento = docTypeOID;
            objPersona.f_nacim = F_NACIM;


            objDoc = new NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER();
            objDoc.c_documento = DOC_NUMERO;
            objDoc.oi_tipo_documento = docTypeOID;
            objPersona.DOCUM_PER.Add(objDoc);







            NomadEnvironment.GetCurrentTransaction().SaveRefresh(objPersona);
            return objPersona.Id;
        }


        public static string AddLegajo(int oi_empresa, string oi_personal, string LEGAJO, DateTime F_INGRESO, string TP_COD, string CAL_COD, string CC_COD, string COD_CAT, string COD_FUN)
        {
            string legajoFound;
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP objLegajo;
            NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER objCal;
            NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER objCC;
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER objING;
            NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER objTP;

            NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER objCAT;


            string tpOID = FindOI("PER11_TIPOS_PERS", "oi_tipo_personal", "c_tipo_personal", TP_COD);
            string calOID = FindOI("ORG27_CAL_FERIADOS", "oi_calendario", "c_calendario", CAL_COD);
            string ccOID = FindOI("ORG08_CS_COSTO", "oi_centro_costo", "c_centro_costo", CC_COD);
            string acOID = FindOI("PER21_INDIC_ACTIVO", "oi_indic_activo", "c_indic_activo", "A");

            string catOID = FindOI("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", COD_CAT);
            string FunOID = FindOI("ORG03_FUNCIONES", "oi_funcion", "oi_empresa", oi_empresa.ToString(), "c_funcion", COD_FUN);


            if (tpOID == "")
            {
                TraceError("Tipo de Personal '" + TP_COD + "' no encontrado....");
                return "";
            }
            if (calOID == "")
            {
                TraceError("Calendario '" + CAL_COD + "' no encontrado....");
                return "";
            }
            if (ccOID == "")
            {
                TraceError("Centro de Costo '" + CC_COD + "' no encontrado....");
                return "";
            }

            if (catOID == "")
            {
                TraceError("Categoria '" + catOID + "' no encontrada....");
                return "";
            }

            if (FunOID == "")
            {
                TraceError("Funcion '" + FunOID + "' no encontrada....");

            }


            legajoFound = FindOI("PER02_PERSONAL_EMP", "oi_personal_emp", "oi_empresa", oi_empresa.ToString(), "oi_personal", oi_personal);
            if (legajoFound != "")
            {
                TraceWrn("La Persona ya esta asignada a la Empresa....");
                return legajoFound;
            }

            legajoFound = FindOI("PER02_PERSONAL_EMP", "oi_personal_emp", "oi_empresa", oi_empresa.ToString(), "e_numero_legajo", LEGAJO);
            if (legajoFound != "")
            {
                TraceWrn("Ya existe una Persona con el Numero de Legajo '" + LEGAJO + "' en la Empresa....");
                return legajoFound;
            }


            objLegajo = new NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP();
            objLegajo.oi_empresa = oi_empresa.ToString();
            objLegajo.oi_personal = oi_personal;
            objLegajo.e_numero_legajo = int.Parse(LEGAJO);
            objLegajo.f_ingreso = F_INGRESO;
            objLegajo.oi_tipo_personal = tpOID;
            objLegajo.oi_calendario_ult = calOID;
            objLegajo.f_desde_calendario = F_INGRESO;
            objLegajo.oi_ctro_costo_ult = ccOID;
            objLegajo.f_desde_ccosto = F_INGRESO;
            objLegajo.oi_indic_activo = acOID;
            objLegajo.oi_categoria_ult = catOID;
            objLegajo.oi_funcion = FunOID;



            objCal = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
            objCal.f_desde = F_INGRESO;
            objCal.oi_calendario = calOID;
            objLegajo.CALENDARIO_PER.Add(objCal);

            objCC = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
            objCC.f_ingreso = F_INGRESO;
            objCC.oi_centro_costo = ccOID;
            objLegajo.CCOSTO_PER.Add(objCC);

            objING = new NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER();
            objING.f_ingreso = F_INGRESO;
            objLegajo.INGRESOS_PER.Add(objING);

            objTP = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
            objTP.oi_tipo_personal = tpOID;
            objTP.f_ingreso = F_INGRESO;
            objLegajo.TIPOSP_PER.Add(objTP);


            objCAT = new NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER();
            objCAT.oi_categoria = catOID;
            objCAT.f_ingreso = F_INGRESO;
            objLegajo.CATEG_PER.Add(objCAT);




            NomadEnvironment.GetCurrentTransaction().SaveRefresh(objLegajo);
            return objLegajo.Id;
        }






        public static string AddLegajo(int oi_empresa, string oi_personal, string LEGAJO, DateTime F_INGRESO, string TP_COD, string CAL_COD, string CC_COD)
        {
            string legajoFound;
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP objLegajo;
            NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER objCal;
            NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER objCC;
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER objING;
            NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER objTP;



            string tpOID = FindOI("PER11_TIPOS_PERS", "oi_tipo_personal", "c_tipo_personal", TP_COD);
            string calOID = FindOI("ORG27_CAL_FERIADOS", "oi_calendario", "c_calendario", CAL_COD);
            string ccOID = FindOI("ORG08_CS_COSTO", "oi_centro_costo", "c_centro_costo", CC_COD);
            string acOID = FindOI("PER21_INDIC_ACTIVO", "oi_indic_activo", "c_indic_activo", "A");



            if (tpOID == "")
            {
                TraceError("Tipo de Personal '" + TP_COD + "' no encontrado....");
                return "";
            }
            if (calOID == "")
            {
                TraceError("Calendario '" + CAL_COD + "' no encontrado....");
                return "";
            }
            if (ccOID == "")
            {
                TraceError("Centro de Costo '" + CC_COD + "' no encontrado....");
                return "";
            }



            legajoFound = FindOI("PER02_PERSONAL_EMP", "oi_personal_emp", "oi_empresa", oi_empresa.ToString(), "oi_personal", oi_personal);
            if (legajoFound != "")
            {
                TraceWrn("La Persona ya esta asignada a la Empresa....");
                return legajoFound;
            }

            legajoFound = FindOI("PER02_PERSONAL_EMP", "oi_personal_emp", "oi_empresa", oi_empresa.ToString(), "e_numero_legajo", LEGAJO);
            if (legajoFound != "")
            {
                TraceWrn("Ya existe una Persona con el Numero de Legajo '" + LEGAJO + "' en la Empresa....");
                return legajoFound;
            }


            objLegajo = new NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP();
            objLegajo.oi_empresa = oi_empresa.ToString();
            objLegajo.oi_personal = oi_personal;
            objLegajo.e_numero_legajo = int.Parse(LEGAJO);
            objLegajo.f_ingreso = F_INGRESO;
            objLegajo.oi_tipo_personal = tpOID;
            objLegajo.oi_calendario_ult = calOID;
            objLegajo.f_desde_calendario = F_INGRESO;
            objLegajo.oi_ctro_costo_ult = ccOID;
            objLegajo.f_desde_ccosto = F_INGRESO;
            objLegajo.oi_indic_activo = acOID;



            objCal = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
            objCal.f_desde = F_INGRESO;
            objCal.oi_calendario = calOID;
            objLegajo.CALENDARIO_PER.Add(objCal);

            objCC = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
            objCC.f_ingreso = F_INGRESO;
            objCC.oi_centro_costo = ccOID;
            objLegajo.CCOSTO_PER.Add(objCC);

            objING = new NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER();
            objING.f_ingreso = F_INGRESO;
            objLegajo.INGRESOS_PER.Add(objING);

            objTP = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
            objTP.oi_tipo_personal = tpOID;
            objTP.f_ingreso = F_INGRESO;
            objLegajo.TIPOSP_PER.Add(objTP);




            NomadEnvironment.GetCurrentTransaction().SaveRefresh(objLegajo);
            return objLegajo.Id;
        }
        public static void ImportarLegajos(int oi_empresa)
        {
            int Linea, Errores = 0;
            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Personal.InterfacePersonal.PERSONAL objRead;
            string PersonalOI, LegajoOI;

            TraceInfo("Comienza la Importacion de Legajos...");
            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(PERSONAL.Resources.qry_legajos, ""));

            for (Linea = 1, IDCur = IDList.FindElement("ROWS").FirstChild(); IDCur != null; IDCur = IDCur.Next(), Linea++)
            {
                objRead = NucleusRH.Base.Personal.InterfacePersonal.PERSONAL.Get(IDCur.GetAttr("id"));

                //Inicio la Transaccion
                try
                {
                    //Creo la Persona
                    PersonalOI = AddPersona(objRead.APELLIDO, objRead.NOMBRES, objRead.SEXO, objRead.DOC_TIPO, objRead.DOC_NUMERO, objRead.F_NACIM, objRead.PER_COD);
                    if (PersonalOI == "")
                    {
                        TraceError("No se pudo agregar la Persona '" + objRead.APELLIDO + ", " + objRead.NOMBRES + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Legajo
                    LegajoOI = AddLegajo(oi_empresa, PersonalOI, objRead.LEGAJO, objRead.F_INGRESO, objRead.TP_COD, objRead.CAL_COD, objRead.CC_COD, objRead.CAT_COD, objRead.FUN_COD);
                    if (LegajoOI == "")
                    {
                        TraceError("No se pudo agregar el Legajo para la Persona '" + objRead.APELLIDO + ", " + objRead.NOMBRES + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                }
                catch (Exception e)
                {
                    TraceError("Error desconocido. " + e.Message + " - Linea " + Linea.ToString());
                    Errores++;
                }
                if (Linea % 10 == 0) TraceInfo("Registros Procesados:" + Linea.ToString() + " - Importados:" + (Linea - Errores).ToString());
            }

            Linea--;
            TraceInfo("Registros Procesados:" + Linea.ToString() + " - Importados:" + (Linea - Errores).ToString());
            TraceInfo("Finalizado...");

        }

    }
}
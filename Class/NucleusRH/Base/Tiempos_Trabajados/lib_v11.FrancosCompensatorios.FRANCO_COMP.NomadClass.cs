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

namespace NucleusRH.Base.Tiempos_Trabajados.FrancosCompensatorios
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Francos Compensatorios - Tiempos Trabajados
    public partial class FRANCO_COMP : Nomad.NSystem.Base.NomadObject
    {
        public static void GetFrancos(Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadXML xmlPROCs = null;
            NomadXML xmlPROC = null;
            NomadXML xmlFCs = null;
            NomadXML xmlFC = null;
            NomadXML xmlRET = null;
            string idper = param.FirstChild().GetAttr("oi_personal_emp");
            try
            {
                //Trae la lista de FC generados en los ultmos 6 meses
                xmlPROCs = NomadEnvironment.QueryNomadXML(FRANCO_COMP.Resources.QRY_PROC, param.ToString()).FirstChild();
                xmlFCs = NomadEnvironment.QueryNomadXML(FRANCO_COMP.Resources.QRY_FC, param.ToString()).FirstChild();

                xmlPROC = xmlPROCs.FirstChild();
                xmlFC = xmlFCs.FirstChild();
                xmlRET = new NomadXML("ROWS");

                while (xmlPROC != null || xmlFC != null)
                {

                    if (xmlPROC != null)
                        NomadLog.Debug("xmlPROC 1 -- " + xmlPROC.ToString());
                    else
                        NomadLog.Debug("xmlPROC 1 -- NULL");
                    if (xmlFC != null)
                        NomadLog.Debug("xmlFC 1 -- " + xmlFC.ToString());
                    else
                        NomadLog.Debug("xmlFC 1 -- NULL");

                    if (xmlFC != null && (xmlPROC == null || xmlPROC.GetAttrDateTime("f_fecjornada") > xmlFC.GetAttrDateTime("f_franco_comp")))
                    {
                        NomadLog.Debug("ERR 1");
                        //El FC esta registrado pero no aparece en la tabla de procesos - marcar como error
                        NomadXML xmlVAL = xmlRET.AddTailElement("ROW");
                        xmlVAL.SetAttr("fecha", xmlFC.GetAttr("f_franco_comp"));
                        xmlVAL.SetAttr("cant_gen", xmlFC.GetAttr("n_franco_gen"));
                        xmlVAL.SetAttr("cant_comp", xmlFC.GetAttr("n_franco_comp"));
                        xmlVAL.SetAttr("c_estado", "del");
                        //Si aun no compenso nada lo borro
                        if (xmlFC.GetAttrDouble("n_franco_comp") <= 0)
                            xmlVAL.SetAttr("c_estado", "del");

                        xmlFC = xmlFC.Next();
                    }
                    else if (xmlFC == null || xmlFCs.FindElement2("FC", "f_franco_comp", xmlPROC.GetAttr("f_fecjornada")) == null)
                    {
                        NomadLog.Debug("ADD");
                        //El FC NO esta registrado - agregar a la BD
                        NomadXML xmlVAL = xmlRET.AddTailElement("ROW");
                        xmlVAL.SetAttr("fecha", xmlPROC.GetAttr("f_fecjornada"));
                        xmlVAL.SetAttr("cant_gen", xmlPROC.GetAttr("n_horas_fc"));
                        xmlVAL.SetAttr("cant_comp", "0");
                        xmlVAL.SetAttr("c_estado", "add");

                        xmlPROC = xmlPROC.Next();
                    }
                    else
                    {
                        if (xmlPROC.GetAttrDouble("n_horas_fc") < xmlFC.GetAttrDouble("n_franco_comp"))
                        {
                            NomadLog.Debug("ERR 2");
                            //El FC fue compensado con un valor mayor al procesado para la fecha - marcar como error
                            NomadXML xmlVAL = xmlRET.AddTailElement("ROW");
                            xmlVAL.SetAttr("fecha", xmlFC.GetAttr("f_franco_comp"));
                            xmlVAL.SetAttr("cant_gen", xmlPROC.GetAttr("n_franco_gen"));
                            xmlVAL.SetAttr("cant_comp", xmlFC.GetAttr("n_franco_comp"));
                            xmlVAL.SetAttr("c_estado", "err");
                            //Si aun no compenso nada lo borro
                            if (xmlFC.GetAttrDouble("n_franco_comp") <= 0)
                                xmlVAL.SetAttr("c_estado", "del");
                        }
                        else if (xmlPROC.GetAttrDouble("n_horas_fc") > xmlFC.GetAttrDouble("n_franco_comp"))
                        {
                            NomadLog.Debug("UPD 1");
                            //El FC fue compensado pero se genero una cantidad mayor y resta compensar - actualizar en la bd
                            NomadXML xmlVAL = xmlRET.AddTailElement("ROW");
                            xmlVAL.SetAttr("fecha", xmlFC.GetAttr("f_franco_comp"));
                            xmlVAL.SetAttr("cant_gen", xmlPROC.GetAttr("n_horas_fc"));
                            xmlVAL.SetAttr("cant_comp", xmlFC.GetAttrDouble("n_franco_comp"));
                            xmlVAL.SetAttr("c_estado", "upd");
                        }
                        else if (xmlPROC.GetAttrDouble("n_horas_fc") != xmlFC.GetAttrDouble("n_franco_gen"))
                        {
                            NomadLog.Debug("UPD 2");
                            //Los compensado es igual a lo generado pero el FC generado es diferente al calculado antes - actualizar en la bd
                            NomadXML xmlVAL = xmlRET.AddTailElement("ROW");
                            xmlVAL.SetAttr("fecha", xmlFC.GetAttr("f_franco_comp"));
                            xmlVAL.SetAttr("cant_gen", xmlPROC.GetAttr("n_horas_fc"));
                            xmlVAL.SetAttr("cant_comp", xmlFC.GetAttr("n_franco_comp"));
                            xmlVAL.SetAttr("c_estado", "upderr");
                        }

                        xmlPROC = xmlPROC.Next();
                        xmlFC = xmlFC.Next();
                    }
                }

                NomadLog.Debug("xmlRET -- " + xmlRET.ToString());

                for (NomadXML xmlcur = xmlRET.FirstChild(); xmlcur != null; xmlcur = xmlcur.Next())
                {

                    NomadLog.Debug("xmlcur -- " + xmlcur.ToString());
                    if (xmlcur.GetAttr("c_estado") == "add")
                    {
                        FRANCO_COMP ddoFC = new FRANCO_COMP();
                        ddoFC.oi_personal_emp = idper;
                        ddoFC.f_franco_comp = xmlcur.GetAttrDateTime("fecha");
                        ddoFC.n_franco_gen = xmlcur.GetAttrDouble("cant_gen");
                        ddoFC.n_franco_comp = xmlcur.GetAttrDouble("cant_comp");
                        ddoFC.c_estado = "OK";

                        NomadEnvironment.GetCurrentTransaction().Save(ddoFC);
                    }

                    if (xmlcur.GetAttr("c_estado") == "upd")
                    {
                        string idFC = NomadEnvironment.QueryValue("TTA18_FRANCOS_COMP", "oi_franco_comp", "f_franco_comp", xmlcur.GetAttr("fecha"), "TTA18_FRANCOS_COMP.oi_personal_emp = " + idper, false);
                        FRANCO_COMP ddoFC = FRANCO_COMP.Get(idFC);

                        ddoFC.f_franco_comp = xmlcur.GetAttrDateTime("fecha");
                        ddoFC.n_franco_gen = xmlcur.GetAttrDouble("cant_gen");
                        ddoFC.n_franco_comp = xmlcur.GetAttrDouble("cant_comp");
                        ddoFC.c_estado = "OK";

                        NomadEnvironment.GetCurrentTransaction().Save(ddoFC);
                    }

                    if (xmlcur.GetAttr("c_estado") == "upderr")
                    {
                        string idFC = NomadEnvironment.QueryValue("TTA18_FRANCOS_COMP", "oi_franco_comp", "f_franco_comp", xmlcur.GetAttr("fecha"), "TTA18_FRANCOS_COMP.oi_personal_emp = " + idper, false);
                        FRANCO_COMP ddoFC = FRANCO_COMP.Get(idFC);

                        ddoFC.f_franco_comp = xmlcur.GetAttrDateTime("fecha");
                        ddoFC.n_franco_gen = xmlcur.GetAttrDouble("cant_gen");
                        ddoFC.n_franco_comp = xmlcur.GetAttrDouble("cant_comp");
                        ddoFC.c_estado = "OK";

                        NomadEnvironment.GetCurrentTransaction().Save(ddoFC);
                    }

                    if (xmlcur.GetAttr("c_estado") == "err")
                    {
                        string idFC = NomadEnvironment.QueryValue("TTA18_FRANCOS_COMP", "oi_franco_comp", "f_franco_comp", xmlcur.GetAttr("fecha"), "TTA18_FRANCOS_COMP.oi_personal_emp = " + idper, false);
                        FRANCO_COMP ddoFC = FRANCO_COMP.Get(idFC);

                        ddoFC.c_estado = "ERR";

                        NomadEnvironment.GetCurrentTransaction().Save(ddoFC);
                    }
                    if (xmlcur.GetAttr("c_estado") == "del")
                    {
                        string idFC = NomadEnvironment.QueryValue("TTA18_FRANCOS_COMP", "oi_franco_comp", "f_franco_comp", xmlcur.GetAttr("fecha"), "TTA18_FRANCOS_COMP.oi_personal_emp = " + idper, false);
                        FRANCO_COMP ddoFC = FRANCO_COMP.Get(idFC);

                        NomadEnvironment.GetCurrentTransaction().Delete(ddoFC);
                    }
                }
            }
            catch (Exception e)
            {
                NomadException exc = NomadException.NewInternalException("GetFC", e);
                exc.SetValue("param", param.ToString());
                exc.SetValue("xmlPROCs", xmlPROCs.ToString());
                exc.SetValue("xmlFCs", xmlFCs.ToString());
                exc.SetValue("xmlPROC", xmlPROC == null ? "null" : xmlPROC.ToString());
                exc.SetValue("xmlFC", xmlFC == null ? "null" : xmlFC.ToString());
                exc.SetValue("xmlRET", xmlRET == null ? "null" : xmlRET.ToString());

                throw exc;
            }
        }
        public static void EditFrancos(Nomad.NSystem.Proxy.NomadXML param)
        {
            FRANCO_COMP ddoFC = null;
            if (param.isDocument) param = param.FirstChild();
            for (NomadXML xmlcur = param.FirstChild().FirstChild(); xmlcur != null; xmlcur = xmlcur.Next())
            {
                NomadLog.Debug("xmlcur -- " + xmlcur.ToString());
                if (xmlcur.GetAttr("del") != "1" && xmlcur.GetAttr("n_compen") == xmlcur.GetAttr("n_compen_old"))
                    continue;

                ddoFC = FRANCO_COMP.Get(xmlcur.GetAttr("oi_franco_comp"));
                COMPEN ddoCOMPEN = (COMPEN)ddoFC.COMPEN.GetById(xmlcur.GetAttr("oi_compen"));
                if (xmlcur.GetAttr("del") == "1")
                {
                    ddoFC.n_franco_comp -= ddoCOMPEN.n_compen;
                    ddoFC.COMPEN.Remove(ddoCOMPEN);
                }
                if (xmlcur.GetAttr("n_compen") != xmlcur.GetAttr("n_compen_old"))
                {
                    ddoCOMPEN.n_compen = xmlcur.GetAttrDouble("n_compen");
                    ddoFC.n_franco_comp -= xmlcur.GetAttrDouble("n_compen_old");
                    ddoFC.n_franco_comp += ddoCOMPEN.n_compen;
                }

                if (ddoFC != null)
                {
                    NomadEnvironment.GetCurrentTransaction().Save(ddoFC);
                    ddoFC = null;
                }
            }
        }
        public static void SetFrancos(Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Gestión de Compensatorios");

            if (param.isDocument) param = param.FirstChild();

            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                string idper = param.GetAttr("oi_personal_emp");
                string idlic = param.GetAttr("oi_licencia");
                string[] editvalues = param.GetAttr("editvalues").Split('|');
                int c = 0;
                foreach (string s in editvalues)
                {
                    c++;
                    objBatch.SetPro(0, 80, c, editvalues.Length);
                    if (s == "") continue;
                    objBatch.SetMess("Cargando compensaciones... " + c);

                    NomadXML xmlcur = param.FindElement("ROWS").FindElement2("ROW", "oi_franco_comp", s);
                    FRANCO_COMP ddoFC = FRANCO_COMP.Get(s);
                    ddoFC.n_franco_comp += xmlcur.GetAttrDouble("compensar");

                    COMPEN ddoCOMP = new COMPEN();
                    ddoCOMP.f_compen = param.GetAttrDateTime("f_desde");
                    ddoCOMP.n_compen = xmlcur.GetAttrDouble("compensar");

                    ddoFC.COMPEN.Add(ddoCOMP);

                    NomadEnvironment.GetCurrentTransaction().Save(ddoFC);
                }

                DateTime f_desde = param.GetAttrDateTime("f_desde");
                DateTime f_hasta = param.GetAttrDateTime("f_hasta");

                //GENERO LA LICENCIA
                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(idper);
                ddoLEG.GenerarLicencia(idlic, f_desde, f_hasta);

                objBatch.SetMess("Generando Licencia...");
                NomadEnvironment.GetCurrentTransaction().Save(ddoLEG);
                objBatch.SetPro(100);

                NomadEnvironment.GetCurrentTransaction().Commit();
                objBatch.Log("Las compensaciones fueron registradas correctamente");
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                NomadException exc = NomadException.NewInternalException("SetFC", e);
                exc.SetValue("param", param.ToString());
                objBatch.Err("Se ha producico un error: " + exc.Id);
                throw exc;
            }
        }
    }
}
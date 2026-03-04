using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.QuejasyReclamos.WFReclamos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Reclamos
    public partial class RECLAMO 
    {
        public static void Save(NucleusRH.Base.QuejasyReclamos.Reclamos.RECLAMO DDO, Nomad.NSystem.Proxy.NomadXML WFInstancia, string Etapas, Nomad.NSystem.Proxy.NomadXML Historico)
        {

            NucleusRH.Base.QuejasyReclamos.Reclamos.HISTORICO new_historico = new NucleusRH.Base.QuejasyReclamos.Reclamos.HISTORICO();
            new_historico.f_reclamo = DateTime.Now;
            new_historico.c_estado_act = DDO.c_estado;
            new_historico.d_usuario = NomadProxy.GetProxy().GetEtty().GetAttr("id");
            int n_historico;
            string destino = "";
            Nomad.Base.Mail.OutputMails.MAIL MyMAIL;
            Nomad.Base.Mail.OutputMails.DESTINATARIO MyDEST;
            if (Historico.FirstChild().GetAttrInt("n_historico") == 0)
                n_historico = 1;
            else
                n_historico = Historico.FirstChild().GetAttrInt("n_historico") + 1;
            new_historico.n_historico = n_historico;
            DDO.n_historico = n_historico;

            if (Historico.FirstChild().GetAttr("t_historico") != "")
                new_historico.t_historico = Historico.FirstChild().GetAttr("t_historico").ToString();

            if (Historico.FirstChild().GetAttr("oi_oficina") != "")
                new_historico.oi_ofi_hasta = Historico.FirstChild().GetAttr("oi_oficina");


            foreach (NucleusRH.Base.QuejasyReclamos.Reclamos.SOL_RECLAMO sol_reclamo in DDO.SOLS_RECLAMO)
            {
                if (sol_reclamo.n_historico == 0 || sol_reclamo.n_historicoNull) //Cambio validación (sol_reclamo.n_historico == 0 || sol_reclamo.n_historico == null) The result of the expression is always 'false' since a value of type 'int' is never equal to 'null' of type 'int?'
                    sol_reclamo.n_historico = n_historico;
                if (Historico.FirstChild().GetAttr("l_rechazado") == "1")
                    sol_reclamo.l_rechazado = true;
                if (Etapas != "0")
                {
                    foreach (NucleusRH.Base.QuejasyReclamos.Reclamos.HISTORICO hist in DDO.HISTORICOS)
                    {
                        if (hist.n_historico == Historico.FirstChild().GetAttrInt("n_historico") && sol_reclamo.n_historico == n_historico)
                            sol_reclamo.oi_oficina = hist.oi_ofi_hasta;
                    }
                }
            }

            foreach (NucleusRH.Base.QuejasyReclamos.Reclamos.NOTA nota in DDO.NOTAS)
            {
                if (nota.n_historico == 0 || nota.n_historicoNull) //Cambio validación (nota.n_historico == 0 || nota.n_historico == null) The result of the expression is always 'false' since a value of type 'int' is never equal to 'null' of type 'int?'
                    nota.n_historico = n_historico;
            }
            foreach (NucleusRH.Base.QuejasyReclamos.Reclamos.MENSAJE mensaje in DDO.MENSAJES)
            {
                if (mensaje.n_historico == 0 || mensaje.n_historicoNull) //Cambio validación (mensaje.n_historico == 0 || mensaje.n_historico == null) The result of the expression is always 'false' since a value of type 'int' is never equal to 'null' of type 'int?'
                    mensaje.n_historico = n_historico;
                if (mensaje.l_enviado != true)
                {
                    //MAIL
                    MyMAIL = new Nomad.Base.Mail.OutputMails.MAIL();
                    MyMAIL.ASUNTO = "Reclamo Nro. " + DDO.Id + " - Mensaje";
                    MyMAIL.DESDE_APLICACION = NomadProxy.GetProxy().AppName;
                    MyMAIL.FECHA_CREACION = System.DateTime.Now;
                    MyMAIL.CONTENIDO = mensaje.t_mensaje;
                    MyMAIL.TAGS = WFInstancia.FirstChild().GetAttr("DES");
                    destino = WFInstancia.FirstChild().FindElement("DUENO").GetAttr("value");
                    MyDEST = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                    MyDEST.ENTIDAD = destino;
                    MyMAIL.DESTINATARIOS.Add(MyDEST);
                    NomadEnvironment.GetTrace().Info("MyMAIL-- " + MyMAIL.SerializeAll());
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyMAIL);
                    mensaje.l_enviado = true;
                }

            }
            foreach (NucleusRH.Base.QuejasyReclamos.Reclamos.DOCUM_DIG_QYR mensaje in DDO.DOC_DIG_QYR)
            {
                if (mensaje.n_historico == 0 || mensaje.n_historicoNull) //Cambio validación (mensaje.n_historico == 0 || mensaje.n_historico == null) The result of the expression is always 'false' since a value of type 'int' is never equal to 'null' of type 'int?'
                    mensaje.n_historico = n_historico;
            }

            NomadEnvironment.GetTrace().Info("new_historico-- " + new_historico.SerializeAll());
            DDO.HISTORICOS.Add(new_historico);
            NomadEnvironment.GetTrace().Info("DDO-- " + DDO.SerializeAll());
            DDO.f_ult_estado = DateTime.Now;
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);

            Nomad.NSystem.Proxy.NomadXML reclamo = WFInstancia.FirstChild().FirstChild().FirstChild();
            NomadEnvironment.GetTrace().Info("reclamo-- " + reclamo.ToString());
            reclamo.SetAttr("oi_reclamo", DDO.Id);
            reclamo.SetAttr("f_ult_estado", DateTime.Now);
            reclamo.SetAttr("n_criticidad", DDO.n_criticidad);
            reclamo.SetAttr("c_asunto", DDO.c_asunto);


            reclamo.SetAttr("f_reclamo", DDO.f_reclamo);
            if (DDO.f_vencimientoNull != true)
                reclamo.SetAttr("f_vencimiento", DDO.f_vencimiento);
            reclamo.SetAttr("t_reclamo", DDO.t_reclamo);
            reclamo.SetAttr("d_usuario", DDO.d_usuario);
            reclamo.SetAttr("c_estado", DDO.c_estado);
            reclamo.SetAttr("oi_tipo_problema", DDO.oi_tipo_problema);
            reclamo.SetAttr("oi_modulo", DDO.oi_modulo);

            //Guardo los campos customs del reclamo
            reclamo.SetAttr("d_custom_1", DDO.d_custom_1);
            reclamo.SetAttr("d_custom_2", DDO.d_custom_2);
            reclamo.SetAttr("d_custom_3", DDO.d_custom_3);
            reclamo.SetAttr("d_custom_4", DDO.d_custom_4);
            reclamo.SetAttr("d_custom_5", DDO.d_custom_5);
            reclamo.SetAttr("d_custom_6", DDO.d_custom_6);
            reclamo.SetAttr("d_custom_7", DDO.d_custom_7);
            reclamo.SetAttr("d_custom_8", DDO.d_custom_8);
            reclamo.SetAttr("d_custom_9", DDO.d_custom_9);
            reclamo.SetAttr("d_custom_10", DDO.d_custom_10);


            NomadEnvironment.GetTrace().Info("WFInstancia-- " + WFInstancia.ToString());

            Nomad.Base.Workflow.WorkflowInstancias.WFI MyOBJ = new Nomad.Base.Workflow.WorkflowInstancias.WFI();
            MyOBJ.Load(WFInstancia.ToString());
            NomadEnvironment.GetTrace().Info("MyOBJ1 -- " + MyOBJ.SerializeAll());
            if (Etapas != "0")
            {
                MyOBJ.PasarAEtapa(Etapas);
                NomadEnvironment.GetTrace().Info("MyOBJ2 -- " + MyOBJ.SerializeAll());
            }
            else
            {
                MyOBJ.Grabar();
                NomadEnvironment.GetTrace().Info("MyOBJ3 -- " + MyOBJ.SerializeAll());
            }

        }
        public static void SaveMasivo(Nomad.NSystem.Proxy.NomadXML MASIVA, string Etapas, Nomad.NSystem.Proxy.NomadXML Historico)
        {

            NomadEnvironment.GetTrace().Info("****** Derivar Masivamente Reclamos ******");

            //Creo una transaction de base de datos
            //Nomad.NSystem.Base.NomadTransaction transaction =  NomadEnvironment.GetCurrentTransaction();  

            //Nomad.NSystem.Proxy.NomadXML row;
            string oi_oficina = Historico.FirstChild().GetAttr("c_oficina").ToString();
            /*
            if(oi_oficina.Length == 1)
                oi_oficina = '0' + oi_oficina;
            oi_oficina = "OFI:"+oi_oficina;
            */
            NomadXML docIDS = NomadEnvironment.QueryNomadXML(RECLAMO.Resources.getIdsInstancias, "").FirstChild();
            NomadEnvironment.GetTrace().Info("docIDS: " + docIDS.ToString());

            for (NomadXML myIW = docIDS.FirstChild(); myIW != null; myIW = myIW.Next())
            {
                for (NomadXML myROW = MASIVA.FirstChild().FirstChild(); myROW != null; myROW = myROW.Next())
                {
                    if (myROW.GetAttr("id") == myIW.GetAttr("oi_reclamo"))
                    {
                        Nomad.Base.Workflow.WorkflowInstancias.WFI MyOBJWFI = Nomad.Base.Workflow.WorkflowInstancias.WFI.Get(myIW.GetAttr("id"));
                        NomadEnvironment.GetTrace().Info("MyOBJWFI: " + MyOBJWFI.SerializeAll());

                        MyOBJWFI.DIVISION = oi_oficina;
                        NomadXML WFI = new Nomad.NSystem.Proxy.NomadXML(MyOBJWFI.SerializeAll());
                        NomadEnvironment.GetTrace().Info("WFI: " + WFI.ToString());
                        NucleusRH.Base.QuejasyReclamos.Reclamos.RECLAMO MyReclamo = NucleusRH.Base.QuejasyReclamos.Reclamos.RECLAMO.Get(myROW.GetAttr("id"));
                        Historico.FirstChild().SetAttr("n_historico", MyReclamo.n_historico);
                        NomadEnvironment.GetTrace().Info("MyReclamo: " + MyReclamo.SerializeAll());
                        NomadEnvironment.GetTrace().Info("Etapas: " + Etapas.ToString());
                        NomadEnvironment.GetTrace().Info("Historico: " + Historico.ToString());
                        RECLAMO.Save(MyReclamo, WFI, Etapas, Historico);
                    }
                }
            }

        }
        public static void CierreAuto()
        {

            NomadEnvironment.GetTrace().Info("****** Cerrar Reclamos Automáticamente ******");

            NomadXML docReclamos = NomadEnvironment.QueryNomadXML(RECLAMO.Resources.getReclamos, "").FirstChild();
            NomadEnvironment.GetTrace().Info("docReclamos: " + docReclamos.ToString());

            NomadXML docIDS = NomadEnvironment.QueryNomadXML(RECLAMO.Resources.getIdsInstancias, "").FirstChild();
            NomadEnvironment.GetTrace().Info("docIDS: " + docIDS.ToString());

            NomadXML docParam = NomadEnvironment.QueryNomadXML(RECLAMO.Resources.getParametro, "").FirstChild();

            DateTime fechaCierre;

            string Etapas = "Final";

            string Historial = "<ROW t_historico='' n_historico='' oi_oficina='' l_rechazado='0' />";
            NomadXML Historico = new Nomad.NSystem.Proxy.NomadXML(Historial.ToString());
            Historico.FirstChild().SetAttr("t_historico", "Reclamos finalizado automaticamente");

            for (NomadXML myREC = docReclamos.FirstChild(); myREC != null; myREC = myREC.Next())
            {
                for (NomadXML myROW = docIDS.FirstChild(); myROW != null; myROW = myROW.Next())
                {
                    Historico.FirstChild().SetAttr("n_historico", myREC.GetAttrInt("n_historico"));
                    fechaCierre = myREC.GetAttrDateTime("f_reclamo").AddDays(docParam.FirstChild().GetAttrDouble("d_valor"));
                    NomadEnvironment.GetTrace().Info("fechaCierre: " + fechaCierre.ToString());
                    if (myROW.GetAttr("oi_reclamo") == myREC.GetAttr("oi_reclamo") && DateTime.Now > fechaCierre)
                    {
                        Nomad.Base.Workflow.WorkflowInstancias.WFI MyOBJWFI = Nomad.Base.Workflow.WorkflowInstancias.WFI.Get(myROW.GetAttr("id"));
                        NomadEnvironment.GetTrace().Info("MyOBJWFI: " + MyOBJWFI.SerializeAll());

                        NomadXML WFI = new Nomad.NSystem.Proxy.NomadXML(MyOBJWFI.SerializeAll());
                        NomadEnvironment.GetTrace().Info("WFI: " + WFI.ToString());
                        NucleusRH.Base.QuejasyReclamos.Reclamos.RECLAMO MyReclamo = NucleusRH.Base.QuejasyReclamos.Reclamos.RECLAMO.Get(myREC.GetAttr("oi_reclamo"));
                        MyReclamo.c_estado = "Reclamo finalizado";
                        NomadEnvironment.GetTrace().Info("MyReclamo: " + MyReclamo.SerializeAll());
                        NomadEnvironment.GetTrace().Info("Etapas: " + Etapas.ToString());

                        RECLAMO.Save(MyReclamo, WFI, Etapas, Historico);
                    }
                }
            }

        }
        public static void Sincronizar()
        {

            NomadEnvironment.GetTrace().Info("****** Sincronizar Oficinas ******");

            NomadXML docLegajos = NomadEnvironment.QueryNomadXML(RECLAMO.Resources.QRY_LEGAJOS, "").FirstChild();
            NomadEnvironment.GetTrace().Info("docLegajos: " + docLegajos.ToString());

            Nomad.Base.Workflow.WorkflowConfig.WF objConfig = Nomad.Base.Workflow.WorkflowConfig.WF.GetConfig("NucleusRH.Base.QuejasyReclamos.Reclamo");
            NomadEnvironment.GetTrace().Info("objConfig-1: " + objConfig.SerializeAll());
            foreach (Nomad.Base.Workflow.WorkflowConfig.ETAPA etapa in objConfig.ETAPAS)
            {
                if (etapa.USA_DIVISION == true)
                {
                    NomadEnvironment.GetTrace().Info("etapa-1a: " + etapa.SerializeAll());
                    etapa.RESPONSABLES.Clear();
                    NomadEnvironment.GetTrace().Info("etapa-2a: " + etapa.SerializeAll());
                }
            }
            NomadEnvironment.GetTrace().Info("objConfig-2: " + objConfig.SerializeAll());
            NucleusRH.Base.QuejasyReclamos.Oficinas.OFICINA objOficina;
            Nomad.Base.Workflow.WorkflowConfig.RESPONSABLE responsable;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("24");

            for (NomadXML myLEG = docLegajos.FirstChild(); myLEG != null; myLEG = myLEG.Next())
            {
                if (myLEG.GetAttrString("l_inactiva") == "0" && myLEG.GetAttrString("l_responsable") == "1")
                {
                    if (myLEG.GetAttr("oi_oficina") != "")
                    {
                        objOficina = NucleusRH.Base.QuejasyReclamos.Oficinas.OFICINA.Get(myLEG.GetAttr("oi_oficina"));
                        unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(objOficina.oi_unidad_org);
                        foreach (Nomad.Base.Workflow.WorkflowConfig.ETAPA etapa in objConfig.ETAPAS)
                        {
                            if (etapa.USA_DIVISION == true)
                            {
                                NomadEnvironment.GetTrace().Info("etapa-1: " + etapa.SerializeAll());
                                responsable = new Nomad.Base.Workflow.WorkflowConfig.RESPONSABLE();
                                responsable.DIVISION = "OFI:" + myLEG.GetAttrString("c_oficina");
                                responsable.ENTIDAD = myLEG.GetAttrString("d_usuario");
                                NomadEnvironment.GetTrace().Info("etapa-2: " + etapa.SerializeAll());
                                etapa.RESPONSABLES.Add(responsable);
                            }
                        }
                    }
                    //objConfig.ETAPAS.Add(objConfig);
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(objConfig);

                }
            }

        }
    }
}

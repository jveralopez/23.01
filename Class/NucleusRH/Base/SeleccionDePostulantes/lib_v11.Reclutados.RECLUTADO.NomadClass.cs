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

namespace NucleusRH.Base.SeleccionDePostulantes.Reclutados
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Avisos
    public partial class RECLUTADO : Nomad.NSystem.Base.NomadObject
    {
        public void CargarEntrevista(Nomad.NSystem.Proxy.NomadXML param)
        {
            //OBTENGO LA ENTREVISTA DEL RECLUTADO
            NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTA_ENTREV DDOENTREV = (NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTA_ENTREV)this.RECLUTA_ENTREV.GetById(param.FirstChild().GetAttr("oi_recluta_entrev"));

            DDOENTREV.f_entrevista = param.FirstChild().GetAttrDateTime("f_entrevista");
            DDOENTREV.d_entrevistador = param.FirstChild().GetAttr("d_entrevistador");
            DDOENTREV.n_valor = param.FirstChild().GetAttrDouble("n_valor");
            DDOENTREV.t_comentario = param.FirstChild().GetAttr("t_comentario");

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
        }

        public static void FirmarContrato(Nomad.NSystem.Proxy.NomadXML param, string caso, string per_emp_ant)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Firma de Contrato");

            param = param.FirstChild();

            objBatch.SetMess("Obteniendo el Reclutado...");
            objBatch.SetPro(10);

            //CARGO EL RECLUTADO
            NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO DDOREC = NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO.Get(param.GetAttr("oi_reclutado"), false);
            NomadLog.Info("REC -- " + DDOREC.SerializeAll());
            NomadLog.Info("DDOREC.oi_cv -- " + DDOREC.oi_cv);
            //CARGO EL CV
            NucleusRH.Base.SeleccionDePostulantes.CVs.CV DDOCVS = NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Get(DDOREC.oi_cv, false);
            NomadLog.Info("CVS -- " + DDOCVS.SerializeAll());

            DDOCVS.CrearPersona(param.GetAttr("o_personal_emp"),param.GetAttr("c_sexo"));

            if (param.GetAttrInt("e_numero_legajo").ToString() == "0") throw new NomadAppException("No puede firmar el contrato - El legajo es obligatorio.");
            if (param.GetAttrInt("f_ingreso").ToString() == "0") throw new NomadAppException("No puede firmar el contrato - La fecha de ingreso es obligatorio.");
            if (param.GetAttrInt("oi_calendario").ToString() == "0") throw new NomadAppException("No puede firmar el contrato - El calendario es obligatorio.");
            if (param.GetAttrInt("oi_ctro_costo").ToString() == "0") throw new NomadAppException("No puede firmar el contrato - El centro de costo es obligatorio.");
            if (param.GetAttrInt("oi_tipo_personal").ToString() == "0") throw new NomadAppException("No puede firmar el contrato - El tipo de personal es obligatorio.");

            //FIRMA DE CONTRATO
            //CASO 1 - EL PERSONAL NO EXISTE.
            if (caso == "1")
                DDOCVS.CrearLegajo(param.GetAttr("oi_empresa"), param.GetAttr("oi_puesto"), param.GetAttr("oi_posicion"), param.GetAttrDateTime("f_ingreso"), param.GetAttrInt("e_numero_legajo"), param.GetAttr("oi_tipo_personal"), param.GetAttr("oi_ctro_costo"), param.GetAttr("oi_calendario"), param.GetAttr("o_personal_emp"), param.GetAttr("o_motivo_incorp"));

            //CASO 2 - PERSONAL TRABAJANDO ACTUALMENTE PARA LA EMPRESA
            if (caso == "2")
            {
                //CARGO EL LEGAJO
                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(per_emp_ant);

                NomadLog.Info("DDOLEG -- " + DDOLEG.SerializeAll());

                //PUESTO DEL LEGAJO
                if (DDOLEG.oi_puesto_ultNull)
                {
                    DDOLEG.oi_puesto_ult = param.GetAttr("oi_puesto");
                    DDOLEG.f_desde_puesto = param.GetAttrDateTime("f_ingreso");

                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUELEG = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                    DDOPUELEG.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOPUELEG.oi_puesto = param.GetAttr("oi_puesto");

                    DDOLEG.PUESTO_PER.Add(DDOPUELEG);
                }
                else if (DDOLEG.oi_puesto_ult != param.GetAttr("oi_puesto"))
                {
                    DDOLEG.oi_puesto_ult = param.GetAttr("oi_puesto");
                    //CAMBIO
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUELEG = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                    DDOPUELEG.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOPUELEG.oi_motivo_cambio = "3";
                    DDOPUELEG.oi_puesto = param.GetAttr("oi_puesto");

                    NomadLog.Info("DDOLEG 1 -- " + DDOLEG.SerializeAll());
                    DDOLEG.Cambio_Puesto(DDOPUELEG);
                    NomadLog.Info("DDOLEG 2 -- " + DDOLEG.SerializeAll());
                }

                //POSICION
                if (DDOLEG.oi_posicion_ultNull)
                {
                    DDOLEG.oi_posicion_ult = param.GetAttr("oi_posicion");
                    DDOLEG.f_desde_posicion = param.GetAttrDateTime("f_ingreso");

                    NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEG = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                    DDOPOSICLEG.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOPOSICLEG.oi_posicion = param.GetAttr("oi_posicion");

                    NomadLog.Info("DDOLEG 1 -- " + DDOLEG.SerializeAll());
                    DDOLEG.Asignar_Posicion(DDOPOSICLEG);
                    NomadLog.Info("DDOLEG 2 -- " + DDOLEG.SerializeAll());

                    NucleusRH.Base.Organizacion.Puestos.POSICION.AddLegajoSectores(DDOLEG.oi_posicion_ult, DDOLEG.id.ToString(), DDOPOSICLEG.f_ingreso, new DateTime(1899, 1, 1), "", "");

                }
                else if (DDOLEG.oi_posicion_ult != param.GetAttr("oi_posicion"))
                {
                    //CAMBIO LA POSICION DE LEGAJO
                    NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEG = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                    DDOPOSICLEG.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOPOSICLEG.oi_motivo_cambio = "3";
                    DDOPOSICLEG.oi_posicion = param.GetAttr("oi_posicion");

                    NomadLog.Info("DDOLEG 3 -- " + DDOLEG.SerializeAll());
                    DDOLEG.Cambio_Posicion(DDOPOSICLEG);
                    NomadLog.Info("DDOLEG 4 -- " + DDOLEG.SerializeAll());

                    NucleusRH.Base.Organizacion.Puestos.POSICION.Cambiar_Pos_Legajo_Sectores(DDOLEG.oi_posicion_ult, DDOPOSICLEG.oi_posicion, DDOLEG.id.ToString(), DDOPOSICLEG.f_ingreso, DDOPOSICLEG.oi_motivo_cambio, "");
                }

                //TIPO PERSONAL
                if (DDOLEG.oi_tipo_personal != param.GetAttr("oi_tipo_personal"))
                {
                    DDOLEG.oi_tipo_personal = param.GetAttr("oi_tipo_personal");

                    NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER DDOTIPOPER = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
                    DDOTIPOPER.oi_tipo_personal = param.GetAttr("oi_tipo_personal");
                    DDOTIPOPER.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOTIPOPER.oi_motivo_cambio = "3";

                    DDOLEG.Cambio_Tipo_Per(DDOTIPOPER);
                }

                //CENTRO DE COSTO
                if (DDOLEG.oi_ctro_costo_ult != param.GetAttr("oi_ctro_costo"))
                {
                    DDOLEG.oi_ctro_costo_ult = param.GetAttr("oi_ctro_costo");

                    NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER DDOCCPER = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
                    DDOCCPER.oi_centro_costo = param.GetAttr("oi_ctro_costo");
                    DDOCCPER.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOCCPER.oi_motivo_cambio = "3";

                    DDOLEG.Cambio_CCosto(DDOCCPER);
                }

                //CALENDARIO
                if (DDOLEG.oi_calendario_ult != param.GetAttr("oi_calendario"))
                {
                    DDOLEG.oi_calendario_ult = param.GetAttr("oi_calendario");

                    NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER DDOCALPER = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
                    DDOCALPER.oi_calendario = param.GetAttr("oi_calendario");
                    DDOCALPER.f_desde = param.GetAttrDateTime("f_ingreso");
                    DDOCALPER.oi_motivo_cambio = "3";

                    DDOLEG.Cambio_Calendario(DDOCALPER);
                }

                DDOLEG.o_personal_emp = param.GetAttr("o_personal_emp");
                //  DDOLEG.o_motivo_incorp = param.GetAttr("o_motivo_incorp");

                NomadEnvironment.GetCurrentTransaction().Save(DDOLEG);
            }

            //CASO 3 - LEGAJO QUE ACTIVO EN EMPRESA DIFERENTE A LA DEL PUESTO DEL AVISO
            if (caso == "3")
            {
                //EGRESO EL LEGAJO DE LA OTRA EMPRESA
                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEGEGR = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(per_emp_ant);
                ddoLEGEGR.Egreso_Personal(param.GetAttrDateTime("f_ingreso"), "", "1");

               //CREO EL NUEVO LEGAJO EN LA EMPRESA DEL PUESTO
                DDOCVS.CrearLegajo(param.GetAttr("oi_empresa"), param.GetAttr("oi_puesto"), param.GetAttr("oi_posicion"), param.GetAttrDateTime("f_ingreso"), param.GetAttrInt("e_numero_legajo"), param.GetAttr("oi_tipo_personal"), param.GetAttr("oi_ctro_costo"), param.GetAttr("oi_calendario"), param.GetAttr("o_personal_emp"), param.GetAttr("o_motivo_incorp"));
            }

            //CASO 4 - EL LEGAJO ESTA INACTIVO EN LA EMPRESA DEL PUESTO DEL AVISO
            if (caso == "4")
            {
                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEGREI = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(per_emp_ant);

                if (ddoLEGREI.f_antiguedad_recNull)
                    ddoLEGREI.f_antiguedad_rec = ddoLEGREI.f_ingreso;
                ddoLEGREI.f_ingreso = param.GetAttrDateTime("f_ingreso");

                //PUESTO DEL LEGAJO
                if (ddoLEGREI.oi_puesto_ultNull)
                {
                    ddoLEGREI.oi_puesto_ult = param.GetAttr("oi_puesto");
                    ddoLEGREI.f_desde_puesto = param.GetAttrDateTime("f_ingreso");

                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUELEGREI = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                    DDOPUELEGREI.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOPUELEGREI.oi_puesto = param.GetAttr("oi_puesto");

                    ddoLEGREI.PUESTO_PER.Add(DDOPUELEGREI);
                }
                else
                {
                    ddoLEGREI.oi_puesto_ult = param.GetAttr("oi_puesto");
                    //CAMBIO
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUELEGREI = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                    DDOPUELEGREI.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOPUELEGREI.oi_motivo_cambio = "3";
                    DDOPUELEGREI.oi_puesto = param.GetAttr("oi_puesto");

                    ddoLEGREI.Cambio_Puesto(DDOPUELEGREI);
                }

                //POSICION
                if (ddoLEGREI.oi_posicion_ultNull)
                {
                    ddoLEGREI.oi_posicion_ult = param.GetAttr("oi_posicion");
                    ddoLEGREI.f_desde_posicion = param.GetAttrDateTime("f_ingreso");

                    NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEGREI = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                    DDOPOSICLEGREI.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOPOSICLEGREI.oi_posicion = param.GetAttr("oi_posicion");

                    ddoLEGREI.Asignar_Posicion(DDOPOSICLEGREI);

                    NucleusRH.Base.Organizacion.Puestos.POSICION.AddLegajoSectores(ddoLEGREI.oi_posicion_ult, ddoLEGREI.id.ToString(), DDOPOSICLEGREI.f_ingreso, new DateTime(1899, 1, 1), "", "");

                }
                else
                {
                    //CAMBIO LA POSICION DE LEGAJO
                    NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEGREI = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                    DDOPOSICLEGREI.f_ingreso = param.GetAttrDateTime("f_ingreso");
                    DDOPOSICLEGREI.oi_motivo_cambio = "3";
                    DDOPOSICLEGREI.oi_posicion = param.GetAttr("oi_posicion");

                    ddoLEGREI.Cambio_Posicion(DDOPOSICLEGREI);

                    NucleusRH.Base.Organizacion.Puestos.POSICION.Cambiar_Pos_Legajo_Sectores(ddoLEGREI.oi_posicion_ult, DDOPOSICLEGREI.oi_posicion, ddoLEGREI.id.ToString(), DDOPOSICLEGREI.f_ingreso, DDOPOSICLEGREI.oi_motivo_cambio, "");

                }

                //TIPO PERSONAL
                ddoLEGREI.oi_tipo_personal = param.GetAttr("oi_tipo_personal");

                NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER DDOTIPOPER = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
                DDOTIPOPER.oi_tipo_personal = param.GetAttr("oi_tipo_personal");
                DDOTIPOPER.f_ingreso = param.GetAttrDateTime("f_ingreso");
                DDOTIPOPER.oi_motivo_cambio = "3";

                ddoLEGREI.Cambio_Tipo_Per(DDOTIPOPER);

                //CENTRO DE COSTO
                ddoLEGREI.oi_ctro_costo_ult = param.GetAttr("oi_ctro_costo");

                NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER DDOCCPER = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
                DDOCCPER.oi_centro_costo = param.GetAttr("oi_ctro_costo");
                DDOCCPER.f_ingreso = param.GetAttrDateTime("f_ingreso");
                DDOCCPER.oi_motivo_cambio = "3";

                ddoLEGREI.Cambio_CCosto(DDOCCPER);

                //CALENDARIO
                ddoLEGREI.oi_calendario_ult = param.GetAttr("oi_calendario");

                NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER DDOCALPER = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
                DDOCALPER.oi_calendario = param.GetAttr("oi_calendario");
                DDOCALPER.f_desde = param.GetAttrDateTime("f_ingreso");
                DDOCALPER.oi_motivo_cambio = "3";

                ddoLEGREI.Cambio_Calendario(DDOCALPER);

                ddoLEGREI.o_personal_emp = param.GetAttr("o_personal_emp");
                //  ddoLEGREI.o_motivo_incorp = param.GetAttr("o_motivo_incorp");
                ddoLEGREI.Reingreso_Personal();

                NomadEnvironment.GetCurrentTransaction().Save(ddoLEGREI);
            }

            //GUARDO EL RECLUTADO
            DDOREC.c_estado = "C";
            DDOREC.f_estado = DateTime.Now;
            NomadEnvironment.GetCurrentTransaction().Save(DDOREC);
        }

        public static void Reclutar(Nomad.NSystem.Proxy.NomadXML pxmlParams)
        {
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace(" Comienza el proceso de Reclutamiento de Personal ------------------------");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            NucleusRH.Base.SeleccionDePostulantes.CVs.SDPConceptosBase objConceptos;
            NomadXML xmlResult;

            NomadBatch objBatch = NomadBatch.GetBatch("class_Reclutar", "");

            pxmlParams = pxmlParams.FirstChild();

            NomadBatch.Trace("Realiza el GetObject()");
            NomadEnvironment.GetTrace().Info("pxmlParams -- " + pxmlParams.ToString());
            objConceptos = NucleusRH.Base.SeleccionDePostulantes.CVs.SDPConceptosBase.GetObject(pxmlParams);

            if (objConceptos == null)
            {
                objBatch.Err("No se pueden procesar los meritos.");

            }
            else
            {
                objBatch.SetMess("Obteniendo los parametros");
                objBatch.SetPro(1);

                NomadBatch.Trace("Realiza el SetParams()");
                objConceptos.SetParams(pxmlParams);
                objBatch.SetPro(5);

                NomadBatch.Trace("Ejecuta el metodo Reclutar()");
                objBatch.SetSubBatch(6, 95);
                xmlResult = objConceptos.Reclutar();

                //Grabando el archivo con el ID del batch en la carpeta NOMAD/TEMP
                NomadEnvironment.GetProxy().FileServiceIO().SaveFile("TEMP", "SDP_Reclutamiento_" + NomadEnvironment.GetProxy().Batch().ID + ".xml", xmlResult.ToString());

            }

            objBatch.SetPro(100);
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace(" Finaliza el proceso de Reclutamiento de Personal ------------------------");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            return;
        }

        public static void GuardarReclutados(string poi_aviso, Nomad.NSystem.Proxy.NomadXML param)
        {
            //ARMO UN "HASHTABLE" CON TODAS LAS PERSONAS QUE ESTAN EN EL PROCESAMIENTO
            Hashtable ht = new Hashtable();

            ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO.Resources.QRY_REC, "<DATOS poi_aviso=\"" + poi_aviso + "\"/>", "oi_cv");

            //GUARDO EL XML QUE ENTRA EN UN NOMADXML
            NomadXML xmldoc = new NomadXML(param.ToString());
            xmldoc = xmldoc.FirstChild();

            //INICIO UNA TRANSACCION
            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                //RECORRO LA HASH Y PREGUNTO SI EN ELLA HAY IDS QUE NO ESTAN EN LOS ID QUE ENTRAN
                //DE SER ASI, ESTOS IDS HAY QUE ELIMINARLOS

                foreach (string value in ht.Keys)
                {
                    NomadEnvironment.GetTrace().Info("value -- " + value);
                    if (xmldoc.FindElement2("ROW", "id", value) == null)
                    {
                        NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO ddoREC;
                        ddoREC = NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO.Get(((NomadXML)ht[value]).GetAttr("id"));
                        if (ddoREC.c_estado != "C")
                        {
                            NomadEnvironment.GetTrace().Info("DELETE-- " + ddoREC.SerializeAll());
                            NomadEnvironment.GetCurrentTransaction().Delete(ddoREC);
                        }
                    }
                }

                //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PQ DE SER ASI HAY QUE AGREGARLO
                for (NomadXML xmlCur = xmldoc.FirstChild(); xmlCur != null; xmlCur = xmlCur.Next())
                {

                    NomadEnvironment.GetTrace().Info("xmlCur -- " + xmlCur.ToString());

                    if (!ht.ContainsKey(xmlCur.GetAttr("id")) && xmlCur.GetAttr("c_estado") != "Contratado")
                    {

                        NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO ddoREC;
                        ddoREC = new NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO();
                        ddoREC.oi_cv = xmlCur.GetAttr("id");
                        ddoREC.oi_aviso = poi_aviso;
                        ddoREC.n_merito = xmlCur.GetAttrDouble("n_merito");
                        ddoREC.f_estado = DateTime.Now;
                        ddoREC.c_estado = "V";

                        //SECCION QUE GUARDA INFORMACION DE EXAMENES Y ENTREVISTAS DEL RECLUTADO
                        //OBTENGO EL AVISO
                        NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO ddoAVISO;
                        ddoAVISO = NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO.Get(poi_aviso);

                        //RECORRO LOS EXAMENES DEL AVISO Y SE LOS CARGO AL RECLUTADO
                        foreach (NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_EXA ddoAVISOEXA in ddoAVISO.AVISO_EXA)
                        {
                            NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTA_EXA ddoREC_EXA = new NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTA_EXA();
                            ddoREC_EXA.oi_examen = ddoAVISOEXA.oi_examen;

                            //CARGO EL EXAMEN
                            NucleusRH.Base.SeleccionDePostulantes.Examenes.EXAMEN ddoEXA = NucleusRH.Base.SeleccionDePostulantes.Examenes.EXAMEN.Get(ddoAVISOEXA.oi_examen);
                            //RECORRO LOS FACTORES DEL EXAMEN
                            foreach (NucleusRH.Base.SeleccionDePostulantes.Examenes.FACTOR_EXAMEN ddoFAC in ddoEXA.FACTORES)
                            {
                                //CARGO LOS FACTORES DEL EXAMEN
                                NucleusRH.Base.SeleccionDePostulantes.Reclutados.FAC_EXA_REC ddoFACREC = new NucleusRH.Base.SeleccionDePostulantes.Reclutados.FAC_EXA_REC();
                                ddoFACREC.oi_factor_examen = ddoFAC.Id;

                                //AGREGO EL FACTOR AL EXAMEN DEL RECLUTADO
                                ddoREC_EXA.FACT_EXA_REC.Add(ddoFACREC);
                            }

                            //AGREGO EL EXAMEN AL RECLUTADO
                            ddoREC.RECLUTA_EXA.Add(ddoREC_EXA);
                        }

                        //RECORRO LAS ENTREVISTAS DEL AVISO Y SE LAS CARGO AL RECLUTADO
                        foreach (NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_ENTREV ddoAVISOENTREV in ddoAVISO.AVISO_ENTREVISTA)
                        {
                            NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTA_ENTREV ddoREC_ENTREV = new NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTA_ENTREV();
                            ddoREC_ENTREV.oi_entrevista = ddoAVISOENTREV.oi_entrevista;

                            ddoREC.RECLUTA_ENTREV.Add(ddoREC_ENTREV);
                        }

                        NomadEnvironment.GetCurrentTransaction().Save(ddoREC);
                    }
                }

                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw;
            }
        }

        public static void VerCV( int oi_reclutado)
        {
            NucleusRH.Base.SeleccionDePostulantes.Reclutados.RECLUTADO objRec;
            objRec = RECLUTADO.Get(oi_reclutado);

            if (objRec == null)
                throw new Exception("No se pudo encontrar el reclutado para marcar el cv como visto");

            objRec.l_visto = true;
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(objRec);
            }
            catch (Exception e)
            {
                throw new Exception("EROOR al actualizar el reclutado: " + e.Message);
            }
        }
    }
}


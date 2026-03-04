using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Parametros
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Parámetros
    public partial class PARAMETRO
    {
        public static void GuardarTTA(Nomad.NSystem.Proxy.NomadXML paramINI, Nomad.NSystem.Proxy.NomadXML paramOUT)
        {
            //COMPARO EL XML INICIAL CON EL QUE SE MAPEA DEL FORM PARA VER QUE PARAMETRO CAMBIO
            NomadXML xmlINI = paramINI.FirstChild();
            NomadXML xmlOUT = paramOUT.FirstChild();
            //////////////////////////
            //PARAMETROS DE FICHADAS//
            //////////////////////////
            NomadBatch.Trace("RedondeoA");
            if (xmlINI.GetAttr("RedondeoA") != "")
            {
                if (xmlINI.GetAttr("RedondeoA") != xmlOUT.GetAttr("RedondeoA"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("RedondeoAID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("RedondeoA");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "RedondeoA";
                ddoPARAM.d_parametro = "Redondeo de horas (excluyente con MinMinutosHora)";
                ddoPARAM.d_clase = "Terminal";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("RedondeoA");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("MaxDifIngEgr");
            if (xmlINI.GetAttr("MaxDifIngEgr") != "")
            {
                if (xmlINI.GetAttr("MaxDifIngEgr") != xmlOUT.GetAttr("MaxDifIngEgr"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MaxDifIngEgrID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MaxDifIngEgr");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MaxDifIngEgr";
                ddoPARAM.d_parametro = "Máxima diferencia entre una entrada y su salida expresada en minutos";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MaxDifIngEgr");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("DifEgrIng");
            if (xmlINI.GetAttr("DifEgrIng") != "")
            {
                if (xmlINI.GetAttr("DifEgrIng") != xmlOUT.GetAttr("DifEgrIng"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("DifEgrIngID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("DifEgrIng");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "DifEgrIng";
                ddoPARAM.d_parametro = "Diferencia entre una salida y una entrada para que se tomen como anuladas exprada en minutos";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("DifEgrIng");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("MinRange");
            if (xmlINI.GetAttr("MinRange") != "")
            {
                if (xmlINI.GetAttr("MinRange") != xmlOUT.GetAttr("MinRange"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MinRangeID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MinRange");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MinRange";
                ddoPARAM.d_parametro = "Minima diferencia de marcada de dos fichadas seguidas del mismo tipo. Expreda en minutos";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MinRange");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            ///////////////////////////////
            //PARAMETROS DE PROCESAMIENTO//
            ///////////////////////////////
            NomadBatch.Trace("MinMinutosLiq");
            if (xmlINI.GetAttr("MinMinutosLiq") != "")
            {
                if (xmlINI.GetAttr("MinMinutosLiq") != xmlOUT.GetAttr("MinMinutosLiq"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MinMinutosLiqID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MinMinutosLiq");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MinMinutosLiq";
                ddoPARAM.d_parametro = "Mínima cantidad de minutos válidos para el procesamiento";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MinMinutosLiq");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("MinMinutosHora");
            if (xmlINI.GetAttr("MinMinutosHora") != "")
            {
                if (xmlINI.GetAttr("MinMinutosHora") != xmlOUT.GetAttr("MinMinutosHora"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MinMinutosHoraID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MinMinutosHora");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MinMinutosHora";
                ddoPARAM.d_parametro = "Mínima cantidad de minutos válidos para cada hora";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MinMinutosHora");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("MinMinFC");
            if (xmlINI.GetAttr("MinMinFC") != "")
            {
                if (xmlINI.GetAttr("MinMinFC") != xmlOUT.GetAttr("MinMinFC"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MinMinFCID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MinMinFC");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MinMinFC";
                ddoPARAM.d_parametro = "Cantidad minima de minutos trabajados para comenzar a contabilizar los francos compensatorios.";
                ddoPARAM.d_clase = "Liquidacion";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MinMinFC");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("MinMaxFC");
            if (xmlINI.GetAttr("MinMaxFC") != "")
            {
                if (xmlINI.GetAttr("MinMaxFC") != xmlOUT.GetAttr("MinMaxFC"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MinMaxFCID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MinMaxFC");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MinMaxFC";
                ddoPARAM.d_parametro = "Cantidad maxima de minutos trabajados para comenzar a contabilizar los francos compensatorios.";
                ddoPARAM.d_clase = "Liquidacion";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MinMaxFC");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("MinRedondeoFC");
            if (xmlINI.GetAttr("MinRedondeoFC") != "")
            {
                if (xmlINI.GetAttr("MinRedondeoFC") != xmlOUT.GetAttr("MinRedondeoFC"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MinRedondeoFCID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MinRedondeoFC");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MinRedondeoFC";
                ddoPARAM.d_parametro = "Redondeo en minutos de la cantidad de franco compensatorio.";
                ddoPARAM.d_clase = "Liquidacion";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MinRedondeoFC");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("MaxFCAcumulado");
            if (xmlINI.GetAttr("MaxFCAcumulado") != "")
            {
                if (xmlINI.GetAttr("MaxFCAcumulado") != xmlOUT.GetAttr("MaxFCAcumulado"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MaxFCAcumuladoID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MaxFCAcumulado");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MaxFCAcumulado";
                ddoPARAM.d_parametro = "Máxima Cantidad de Francos Compensatorios acumulados";
                ddoPARAM.d_clase = "Liquidacion";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MaxFCAcumulado");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("MaxFCAdeudado");
            if (xmlINI.GetAttr("MaxFCAdeudado") != "")
            {
                if (xmlINI.GetAttr("MaxFCAdeudado") != xmlOUT.GetAttr("MaxFCAdeudado"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MaxFCAdeudadoID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MaxFCAdeudado");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MaxFCAdeudado";
                ddoPARAM.d_parametro = "Máxima Cantidad de Francos Compensatorios adeudados.";
                ddoPARAM.d_clase = "Liquidacion";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MaxFCAdeudado");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("VencimientoFC");
            if (xmlINI.GetAttr("VencimientoFC") != "")
            {
                if (xmlINI.GetAttr("VencimientoFC") != xmlOUT.GetAttr("VencimientoFC"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("VencimientoFCID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("VencimientoFC");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "VencimientoFC";
                ddoPARAM.d_parametro = "Vencimiento de FC";
                ddoPARAM.d_clase = "AdministracionFC";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("VencimientoFC");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            /////////////////////////
            //PARAMETROS DE RELOJES//
            /////////////////////////
            NomadBatch.Trace("MaxChrLeg");
            if (xmlINI.GetAttr("MaxChrLeg") != "")
            {
                if (xmlINI.GetAttr("MaxChrLeg") != xmlOUT.GetAttr("MaxChrLeg"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("MaxChrLegID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("MaxChrLeg");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "MaxChrLeg";
                ddoPARAM.d_parametro = "Cantidad máxima de caracteres numero de legajo reloj";
                ddoPARAM.d_clase = "Terminal";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("MaxChrLeg");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("EveES");
            if (xmlINI.GetAttr("EveES") != "")
            {
                if (xmlINI.GetAttr("EveES") != xmlOUT.GetAttr("EveES"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("EveESID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("EveES");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "EveES";
                ddoPARAM.d_parametro = "Establece si los relojes/terminales reconocen eventos de Entrada/Salida";
                ddoPARAM.d_clase = "Terminal";
                ddoPARAM.d_tipo_parametro = "L";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = "0";
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            /////////////////////////////////////////
            //PARAMETROS DE REPORTE DE ASISTENCIAS///
            ////////////////////////////////////////
            NomadBatch.Trace("PreIng");
            if (xmlINI.GetAttr("PreIng") != "")
            {
                if (xmlINI.GetAttr("PreIng") != xmlOUT.GetAttr("PreIng"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PreIngID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PreIng");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PreIng";
                ddoPARAM.d_parametro = "Cantidad de minutos previos a la hora de Ingreso";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PreIng");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PostIng");
            if (xmlINI.GetAttr("PostIng") != "")
            {
                if (xmlINI.GetAttr("PostIng") != xmlOUT.GetAttr("PostIng"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PostIngID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PostIng");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PostIng";
                ddoPARAM.d_parametro = "Cantidad de minutos posteriores a la hora de Ingreso";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PostIng");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PreEgr");
            if (xmlINI.GetAttr("PreEgr") != "")
            {
                if (xmlINI.GetAttr("PreEgr") != xmlOUT.GetAttr("PreEgr"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PreEgrID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PreEgr");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PreEgr";
                ddoPARAM.d_parametro = "Cantidad de minutos previos a la hora de Egreso";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PreEgr");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PostEgr");
            if (xmlINI.GetAttr("PostEgr") != "")
            {
                if (xmlINI.GetAttr("PostEgr") != xmlOUT.GetAttr("PostEgr"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PostEgrID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PostEgr");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PostEgr";
                ddoPARAM.d_parametro = "Cantidad de minutos posteriores a la hora de Ingreso";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PostEgr");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PreIng2");
            if (xmlINI.GetAttr("PreIng2") != "")
            {
                if (xmlINI.GetAttr("PreIng2") != xmlOUT.GetAttr("PreIng2"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PreIng2ID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PreIng2");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PreIng2";
                ddoPARAM.d_parametro = "Cantidad de minutos previos a la hora de Ingreso Rango 2";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PreIng2");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PostIng2");
            if (xmlINI.GetAttr("PostIng2") != "")
            {
                if (xmlINI.GetAttr("PostIng2") != xmlOUT.GetAttr("PostIng2"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PostIng2ID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PostIng2");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PostIng2";
                ddoPARAM.d_parametro = "Cantidad de minutos posteriores a la hora de Ingreso Rango 2";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PostIng2");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PreEgr2");
            if (xmlINI.GetAttr("PreEgr2") != "")
            {
                if (xmlINI.GetAttr("PreEgr2") != xmlOUT.GetAttr("PreEgr2"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PreEgr2ID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PreEgr2");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PreEgr2";
                ddoPARAM.d_parametro = "Cantidad de minutos previos a la hora de Egreso Rango 2";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PreEgr2");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PostEgr2");
            if (xmlINI.GetAttr("PostEgr2") != "")
            {
                if (xmlINI.GetAttr("PostEgr2") != xmlOUT.GetAttr("PostEgr2"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PostEgr2ID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PostEgr2");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PostEgr2";
                ddoPARAM.d_parametro = "Cantidad de minutos posteriores a la hora de Ingreso Rango 2";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PostEgr2");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("Tard");
            if (xmlINI.GetAttr("Tard") != "")
            {
                if (xmlINI.GetAttr("Tard") != xmlOUT.GetAttr("Tard"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("TardID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("Tard");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "Tard";
                ddoPARAM.d_parametro = "Cantidad de minutos de tolerancia para establecer Tardanzas y Retiros Anticipados";
                ddoPARAM.d_clase = "Reporte";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("Tard");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PreEditFic");
            if (xmlINI.GetAttr("PreEditFic") != "")
            {
                if (xmlINI.GetAttr("PreEditFic") != xmlOUT.GetAttr("PreEditFic"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PreEditFicID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PreEditFic");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PreEditFic";
                ddoPARAM.d_parametro = "Cantidad de días previos para modificar fichadas";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PreEditFic");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("PosEditFic");
            if (xmlINI.GetAttr("PosEditFic") != "")
            {
                if (xmlINI.GetAttr("PosEditFic") != xmlOUT.GetAttr("PosEditFic"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("PosEditFicID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("PosEditFic");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "PosEditFic";
                ddoPARAM.d_parametro = "Cantidad de días posteriores para permitir crear fichadas";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("PosEditFic");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("EditMyFic");
            if (xmlINI.GetAttr("EditMyFic") != "")
            {
                if (xmlINI.GetAttr("EditMyFic") != xmlOUT.GetAttr("EditMyFic"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("EditMyFicID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("EditMyFic");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "EditMyFic";
                ddoPARAM.d_parametro = "No permitir que un usuario modifique sus propias fichadas";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "L";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = "0";
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("TipoClasif");
            if (xmlINI.GetAttr("TipoClasif") != "")
            {
                if (xmlINI.GetAttr("TipoClasif") != xmlOUT.GetAttr("TipoClasif"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("TipoClasifID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("TipoClasif");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "TipoClasif";
                ddoPARAM.d_parametro = "Establece el criterio que utilizará el sistema para clasificar las fichadas en Entradas o Salidas";
                ddoPARAM.d_clase = "Fichadas";
                ddoPARAM.d_tipo_parametro = "C";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("TipoClasif");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

               NomadBatch.Trace("CantidadDiasModificar");
            if (xmlINI.GetAttr("CantidadDiasModificar") != "")
            {
                if (xmlINI.GetAttr("CantidadDiasModificar") != xmlOUT.GetAttr("CantidadDiasModificar"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("CantidadDiasModificarID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("CantidadDiasModificar");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "TTA";
                ddoPARAM.c_parametro = "CantidadDiasModificar";
                ddoPARAM.d_parametro = "Cantidad de dias anteriores para modificar";
                ddoPARAM.d_clase = "Tiempos";
                ddoPARAM.d_tipo_parametro = "E";
                ddoPARAM.o_parametro = "El parametro afecta a las licencias en legajo empresa. Y en licencias y horarios en TTA";
                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("CantidadDiasModificar");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }
        }
        public static void GuardarPOS(Nomad.NSystem.Proxy.NomadXML paramINI, Nomad.NSystem.Proxy.NomadXML paramOUT)
        {
            //COMPARO EL XML INICIAL CON EL QUE SE MAPEA DEL FORM PARA VER QUE PARAMETRO CAMBIO
            NomadXML xmlINI = paramINI.FirstChild();
            NomadXML xmlOUT = paramOUT.FirstChild();
            ////////////////
            // PARAMETROS //
            ////////////////
            NomadBatch.Trace("CantDiasParaDisponible");
            if (xmlINI.GetAttr("CantDiasParaDisponible") != "")
            {
                if (xmlINI.GetAttr("CantDiasParaDisponible") != xmlOUT.GetAttr("CantDiasParaDisponible"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("CantDiasParaDisponibleID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("CantDiasParaDisponible");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "POS";
                ddoPARAM.c_parametro = "CantDiasParaDisponible";
                ddoPARAM.d_parametro = "Es la cantidad de días desde que ha pasado a reasignable hasta que vuelve a disponible";
                ddoPARAM.d_clase = "CV";
                ddoPARAM.d_tipo_parametro = "L";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("CantDiasParaDisponible");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("CantDiasParaVencido");
            if (xmlINI.GetAttr("CantDiasParaVencido") != "")
            {
                if (xmlINI.GetAttr("CantDiasParaVencido") != xmlOUT.GetAttr("CantDiasParaVencido"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("CantDiasParaVencidoID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("CantDiasParaVencido");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "POS";
                ddoPARAM.c_parametro = "CantDiasParaVencido";
                ddoPARAM.d_parametro = "Especifica la cantidad de días que pasaran desde la última actualización hasta pasar un CV a Vencido";
                ddoPARAM.d_clase = "CV";
                ddoPARAM.d_tipo_parametro = "L";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("CantDiasParaVencido");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }

            NomadBatch.Trace("AcuerdoPrivacidad");
            if (xmlINI.GetAttr("AcuerdoPrivacidad") != "")
            {
                if (xmlINI.GetAttr("AcuerdoPrivacidad") != xmlOUT.GetAttr("AcuerdoPrivacidad"))
                {
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                    ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(xmlINI.GetAttr("AcuerdoPrivacidadID"));
                    ddoPARAM.d_valor = xmlOUT.GetAttr("AcuerdoPrivacidad");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
                }
            }
            else
            {
                NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                ddoPARAM.c_modulo = "POS";
                ddoPARAM.c_parametro = "AcuerdoPrivacidad";
                ddoPARAM.d_parametro = "Archivo de Acuerdo de Privacidad";
                ddoPARAM.d_clase = "CV";
                ddoPARAM.d_tipo_parametro = "E";

                ddoPARAM.l_bloqueado = false;
                ddoPARAM.d_valor = xmlOUT.GetAttr("AcuerdoPrivacidad");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPARAM);
            }
        }
        public static Nomad.NSystem.Proxy.NomadXML ObtenerValores(Nomad.NSystem.Proxy.NomadXML PARAM)
        {
            return NomadEnvironment.QueryNomadXML(NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Resources.ObtenerValores, PARAM.ToString());
        }
        public static void GuardarValores(Nomad.NSystem.Proxy.NomadXML PARAM)
        {
            string err = "";
            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                for (NomadXML param = PARAM.FirstChild().FirstChild(); param != null; param = param.Next())
                {
                    err = param.GetAttr("c_parametro");
                    NomadLog.Debug("Guardando parametro '" + err + "'");

                    PARAMETRO objPar;
                    string oi_parametro = param.GetAttr("oi_parametro");
                    if (oi_parametro == "")
                    {
                        objPar = new PARAMETRO();
                        objPar.c_modulo = param.GetAttr("c_modulo");
                        objPar.d_clase = param.GetAttr("d_clase");
                        objPar.c_parametro = param.GetAttr("c_parametro");
                        objPar.d_parametro = param.GetAttr("d_parametro");
                        objPar.d_tipo_parametro = param.GetAttr("d_tipo_parametro");
                    }
                    else
                    {
                        objPar = PARAMETRO.Get(param.GetAttr("oi_parametro"));
                    }
                    objPar.d_valor = param.GetAttr("d_valor");
                    NomadEnvironment.GetCurrentTransaction().Save(objPar);
                }
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                NomadLog.Error("Error guardando el parámetro '" + err + "'. " + e.Message + "\r\n" + e.StackTrace);
                throw new Exception("Error guardando el parámetro '" + err + "', no se han guardado los cambios. Revise el valor ingresado y vuelva a Guardar.");
            }
        }
    }
}



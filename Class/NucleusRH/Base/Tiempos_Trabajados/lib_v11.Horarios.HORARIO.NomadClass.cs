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

namespace NucleusRH.Base.Tiempos_Trabajados.Horarios
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Horarios
    public partial class HORARIO : Nomad.NSystem.Base.NomadObject
    {

        public static HORARIO GetById(string id)
        {
            HORARIO h = (HORARIO)NomadProxy.GetProxy().CacheGetObj("NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO." + id);
            if (h == null)
            {
                h = HORARIO.Get(id, false);
                NomadProxy.GetProxy().CacheAdd("NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO." + id, h);
            }

            return h;
        }

        public static string AsignarHorario(string oi_personal_emp, string f_inicio, string f_fin, string oi_horario, string oi_escuadra)
        {
            //CARGO EL PERSONAL TTA
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP ddoPer = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(oi_personal_emp);
            NomadEnvironment.GetTrace().Info("PER TTA -- " + ddoPer.SerializeAll());

            //ARRAY CON HORARIOS A ELIMINAR	 	
            ArrayList arrDel = new ArrayList();
            //ARRAR CON HORARIOS A AGREGAR     
            ArrayList arrAdd = new ArrayList();

            //PRIMERO VALIDO QUE NO HAYA UNA LIQUIDACION CERRADA PARA LA PERSONA EN LA FECHA 	 	
            DateTime FI = Nomad.NSystem.Functions.StringUtil.str2date(f_inicio);
            DateTime FF = Nomad.NSystem.Functions.StringUtil.str2date(f_fin);

            NomadEnvironment.GetTrace().Info("FI -- " + FI.ToString());
            NomadEnvironment.GetTrace().Info("FF -- " + FF.ToString());

            if (FI < ddoPer.f_ingreso)
            {
                return "El inicio del horario es anterior a la fecha de ingreso del Legajo: " + ddoPer.e_numero_legajo;
            }

            bool liqCer = NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.EnLiquidacionCerrada(ddoPer.Id, Nomad.NSystem.Functions.StringUtil.str2date(f_inicio));
            if (liqCer)
            {
                return "Existen Procesamientos cerrados en la fecha para el Legajo: " + ddoPer.e_numero_legajo;
            }

            //SETEO UN NUEVO HORARIO DEL PERSONAL
            NucleusRH.Base.Tiempos_Trabajados.Personal.HORARIOPERS ddoHorPer;
            ddoHorPer = new NucleusRH.Base.Tiempos_Trabajados.Personal.HORARIOPERS();
            //ASIGNO LOS VALORES DEL NUEVO HORARIO AL REGISTRO DE HORARIOS PERS	
            ddoHorPer.f_fechaInicio = Nomad.NSystem.Functions.StringUtil.str2date(f_inicio);
            ddoHorPer.oi_horario = oi_horario;
            if (f_fin != "")
            {
                ddoHorPer.f_fechaFin = Nomad.NSystem.Functions.StringUtil.str2date(f_fin).AddDays(1);
            }
            if (oi_escuadra != "")
            {
                ddoHorPer.oi_escuadra = oi_escuadra;
            }

            NomadEnvironment.GetTrace().Info("NEWHORPER -- " + ddoHorPer.SerializeAll());
            try
            {
                //TRABAJO SOBRE LOS HORARIOS DE LA PERSONA PARA INCORPORAR EL NUEVO	
                //RECORRO LOS HORARIOS DE LA PERSONA
                foreach (NucleusRH.Base.Tiempos_Trabajados.Personal.HORARIOPERS ddoHP in ddoPer.HORARIOSPERS)
                {
                    NomadEnvironment.GetTrace().Info("HORPER -- " + ddoHP.SerializeAll());

                    if ((!ddoHP.f_fechaFinNull) && (FI >= ddoHP.f_fechaFin))
                    {
                        NomadEnvironment.GetTrace().Info("Horario con Fecha fin y Fecha Inicio de nuevo horario mayor que fecha fin del horario");
                        continue;
                    }

                    if ((f_fin != "") && (FF < ddoHP.f_fechaInicio))
                    {
                        NomadEnvironment.GetTrace().Info("Nuevo Horario con Fecha Fin y Fecha fin nuevo horario es mayor al inicio del horario");
                        continue;
                    }

                    if (f_fin == "")
                    { //caso 1: el nuevo no tiene fecha fin
                        NomadEnvironment.GetTrace().Info("caso 1");
                        if (ddoHP.f_fechaInicio >= FI)
                        { //caso 1.1: la fecha de inicio es mayor.... tiene que ser eliminado				
                            NomadEnvironment.GetTrace().Info("caso 1.1");
                            arrDel.Add(ddoHP);
                        }
                        else
                        { //case 1.2: la fecha de inicio es menor.... cambiar la fecha de fin....
                            NomadEnvironment.GetTrace().Info("caso 1.2");
                            ddoHP.f_fechaFin = FI;
                        }
                    }
                    else
                    { //caso 2: el nuevo tiene fecha fin
                        NomadEnvironment.GetTrace().Info("caso 2");
                        if (ddoHP.f_fechaInicio == FI && ddoHP.f_fechaFin == FF.AddDays(1))
                        {//caso 2.0: el mismo rango...
                            NomadEnvironment.GetTrace().Info("caso 2.0");
                            arrDel.Add(ddoHP);
                        }
                        else if (ddoHP.f_fechaInicio >= FI)
                        { //caso 2.1: la fecha de inicio es mayor....
                            NomadEnvironment.GetTrace().Info("caso 2.1");
                            if (!ddoHP.f_fechaFinNull && FF >= ddoHP.f_fechaFin)
                            { //caso 2.1.1: la fecha de fin es Mayor.... tiene que ser eliminado
                                NomadEnvironment.GetTrace().Info("caso 2.1.1");
                                arrDel.Add(ddoHP);
                            }
                            else
                            { //caso 2.1.2: la fecha de fin es Menor.... cambiar la fecha de inicio....
                                NomadEnvironment.GetTrace().Info("caso 2.1.2");
                                ddoHP.f_fechaInicio = FF.AddDays(1);
                            }
                        }
                        else
                        { //case 2.2: la fecha de inicio es menor.... 
                            NomadEnvironment.GetTrace().Info("caso 2.2");
                            if (!ddoHP.f_fechaFinNull && FF >= ddoHP.f_fechaFin)
                            { //caso 2.2.1: la fecha de fin es Menor.... cambiar la fecha de fin....
                                NomadEnvironment.GetTrace().Info("caso 2.2.1");
                                ddoHP.f_fechaFin = FI;
                            }
                            else
                            {
                                NomadEnvironment.GetTrace().Info("caso 2.2.2");
                                if (ddoHP.f_fechaFin == ddoHorPer.f_fechaFin)
                                {//caso 2.2.2.1: la fecha de fin es igual... cambiar la fecha de fin
                                    NomadEnvironment.GetTrace().Info("caso 2.2.2.1");
                                    ddoHP.f_fechaFin = ddoHorPer.f_fechaInicio;
                                }
                                else
                                {	//caso 2.2.2.2: la fecha de fin es Mayor.... duplicar el registro....
                                    NomadEnvironment.GetTrace().Info("caso 2.2.2.2");
                                    NucleusRH.Base.Tiempos_Trabajados.Personal.HORARIOPERS ddoNEWHP = new NucleusRH.Base.Tiempos_Trabajados.Personal.HORARIOPERS();
                                    ddoNEWHP.oi_horario = ddoHP.oi_horario;
                                    ddoNEWHP.oi_escuadra = ddoHP.oi_escuadra;
                                    ddoNEWHP.f_fechaInicio = FF.AddDays(1);
                                    if (ddoHP.f_fechaFinNull)
                                        ddoNEWHP.f_fechaFinNull = true;
                                    else
                                        ddoNEWHP.f_fechaFin = ddoHP.f_fechaFin;
                                    ddoNEWHP.o_horariopers = ddoHP.o_horariopers;

                                    //AGREGO EL HORARIO AL ARRAY
                                    arrAdd.Add(ddoNEWHP);

                                    ddoHP.f_fechaFin = FI;
                                }
                            }
                        }
                    }
                }

                //AGREGO LOS HORARIOS
                foreach (NucleusRH.Base.Tiempos_Trabajados.Personal.HORARIOPERS x in arrAdd)
                {
                    ddoPer.HORARIOSPERS.Add(x);
                }
                //ELIMINO LOS HORARIOS
                foreach (NucleusRH.Base.Tiempos_Trabajados.Personal.HORARIOPERS x in arrDel)
                {
                    ddoPer.HORARIOSPERS.Remove(x);
                }

                //AGREGO EL HORARIO A LA PERSONA
                ddoPer.HORARIOSPERS.Add(ddoHorPer);

                //GUARDO EL DDO DEL PERSONAL
                NomadEnvironment.GetCurrentTransaction().Save(ddoPer);
                return "OK";
            }
            catch (Exception e)
            {
                return "Error asignando Horario al Legajo: " + ddoPer.e_numero_legajo + " - " + e.Message;
            }
        }
        public static void AsignacionMasiva(Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Asignacion Masiva de Horarios");
            NomadXML xmldoc = param.FirstChild();

            //VALIDO QUE SE INGRESA UNA ESCUADRA PARA EL TIPO DE HORARIO ROTATIVO
            NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO ddoHor;
            ddoHor = NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.Get(xmldoc.GetAttr("oi_horario_nuevo"));
            if (ddoHor.d_tipohorario == "R")
            {
                if (xmldoc.GetAttr("oi_escuadra_nuevo") == "")
                {
                    objBatch.Err("No se ha ingresado una escuadra para el tipo de horario rotativo.");
                    return;
                }
            }

            int C = 0;
            int E = 0;

            ArrayList lista = (ArrayList)xmldoc.FirstChild().GetElements("ROW");

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                NomadXML xmlcur = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Asignando Horario a Legajos " + (xml + 1) + " de " + lista.Count);
                NomadEnvironment.GetTrace().Info("ID -- " + xmlcur.GetAttr("id"));
                //VALIDA QUE EL HORARIO A AGREGAR ESTE EN EL CONVENIO DE LA PERSONA
                if (ValidaConvenioHorario(xmlcur.GetAttr("id"), xmldoc.GetAttr("oi_horario_nuevo")))
                {
                    string result = AsignarHorario(xmlcur.GetAttr("id"), xmldoc.GetAttr("f_inicio"), xmldoc.GetAttr("f_fin"), xmldoc.GetAttr("oi_horario_nuevo"), xmldoc.GetAttr("oi_escuadra_nuevo"));
                    if (result == "OK")
                        C++;
                    else
                    {
                        objBatch.Err(result);
                        E++;
                        continue;
                    }
                }
                else
                {
                    string oi_personal = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(xmlcur.GetAttr("id")).oi_personal;
                    NucleusRH.Base.Personal.Legajo.PERSONAL ddoPersonal  = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(oi_personal);
                    objBatch.Wrn("No se puedo agregar el nuevo horario a : "+ddoPersonal.d_ape_y_nom+" porque no posee el convenio del horario.");
                }
            }

            if (C != 0)
            {
                objBatch.Log("Se asigno el nuevo horario a " + C.ToString() + " legajo/s.");
            }
            if (E != 0)
            {
                objBatch.Log("No se pudo asignar el nuevo horario a " + E.ToString() + " legajo/s.");
            }
        }
        public static void Duplicar(string id, string code, string desc, string color)
        {
            HORARIO ddoDUP = (HORARIO)HORARIO.Get(id).Duplicate();

            //Estado Pendiente
            ddoDUP.c_horario = code;
            ddoDUP.d_horario = desc;
            ddoDUP.c_estado = "P";
            ddoDUP.c_color = color;

            NomadEnvironment.GetCurrentTransaction().Save(ddoDUP);
        }
        public void ActualizarHorario(string TUR, string EST, string ESQ)
        {
            int t;
            NucleusRH.Base.Tiempos_Trabajados.Horarios.TURNO_DET obj;

            //Elimino todo el Detalle
            this.HORA_TUR_DET.Clear();

            //Recorro el Array creaando el Nuevo Detalle
            IList lTUR = TUR.Split('|');
            IList lEST = EST.Split('|');
            IList lESQ = ESQ.Split('|');

            for (t = 0; t < lTUR.Count; t++)
            {
                obj = new NucleusRH.Base.Tiempos_Trabajados.Horarios.TURNO_DET();

                obj.e_posicion = t;
                obj.oi_turno = lTUR[t].ToString();
                obj.oi_estructura = lEST[t].ToString();

                this.HORA_TUR_DET.Add(obj);
            }

            t = 0;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Horarios.ESCUADRA ESC in this.ESCUADRAS)
            {
                ESC.e_avance = int.Parse(lESQ[t].ToString());
                t++;
            }
        }
        public void CerrarHorario()
        {
            int t;
            string first_turno;
            string prev_turno;

            t = 0;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Horarios.ESCUADRA ESC in this.ESCUADRAS)
            {
                if ((ESC.e_avance < 0) || (ESC.e_avance >= this.e_dias))
                    throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, hay ESCUADRAS con error.");
                t++;
            }
            if ((this.d_tipohorario == "R") && (t == 0))
                throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, no hay ESCUADRAS definidas.");


            t = 0; first_turno = null; prev_turno = null;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Horarios.TURNO_DET TUR in this.HORA_TUR_DET)
            {
                if (TUR.e_posicion != t)
                    throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, hay TURNOS con error.");

                if (TUR.oi_estructuraNull)
                    throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, hay TURNOS con error.");

                if (TUR.oi_turnoNull)
                    throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, hay TURNOS con error.");

                if (first_turno == null) first_turno = TUR.oi_turno;
                if (prev_turno != null)
                {
                    if (!NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO.ChkComp(prev_turno, TUR.oi_turno))
                        throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, hay turnos incompatibles.");
                }
                prev_turno = TUR.oi_turno;

                //obtengo el turno
                //if (TUR.Getoi_turno().c_estado!="C")
                //throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, hay TURNOS sin cerrar.");

                t++;
            }
            if (t != this.e_dias)
                throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, turnos sin definir.");

            //compruebo compatibilidad entre el primero y el ultimo
            if (!NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO.ChkComp(first_turno, prev_turno))
                throw new Nomad.NSystem.Base.NomadAppException("No se puede cerrar el Horario, hay turnos incompatibles.");


            foreach (NucleusRH.Base.Tiempos_Trabajados.Horarios.TURNO_DET ddoTURDET in this.HORA_TUR_DET)
            {
                NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO ddoTUR = NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO.Get(ddoTURDET.oi_turno);

                if (ddoTUR.c_estado != "C")
                {
                    ddoTUR.c_estado = "C";
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoTUR);
                }
            }

            this.c_estado = "C";
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
        }
        public static bool ChkComp(string oi_horario_prev, string oi_horario_post, DateTime f_inicio_post)
        {
            int dif_time = (int)Math.Floor((f_inicio_post - new DateTime(2000, 01, 02)).TotalDays);

            //Analizo el turno previo
            HORARIO prev = HORARIO.Get(oi_horario_prev);
            HORARIO post = HORARIO.Get(oi_horario_post);

            //Calculo posision
            int prev_pos = (dif_time - 1) % prev.e_dias;
            int post_pos = dif_time % prev.e_dias;

            //Obtengo la posision
            TURNO_DET tur_prev = (TURNO_DET)prev.HORA_TUR_DET.GetByAttribute("e_posicion", prev_pos);
            TURNO_DET tur_post = (TURNO_DET)post.HORA_TUR_DET.GetByAttribute("e_posicion", post_pos);

            return NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO.ChkComp(tur_prev.oi_turno, tur_post.oi_turno);
        }
        public void AgregarEscuadras(int esc)
        {
            if (this.d_tipohorario == "R")
            {
                for (int i = 0; i < esc; i++)
                {
                    NucleusRH.Base.Tiempos_Trabajados.Horarios.ESCUADRA ddoESC = new NucleusRH.Base.Tiempos_Trabajados.Horarios.ESCUADRA();
                    ddoESC.c_escuadra = ((char)('A' + i)).ToString();
                    ddoESC.d_escuadra = ((char)('A' + i)).ToString();
                    ddoESC.e_avance = 0;

                    NomadEnvironment.GetTrace().Info("ESC -- " + ddoESC.SerializeAll());
                    this.ESCUADRAS.Add(ddoESC);
                }
            }
        }

        public static bool ValidaConvenioHorario(string oi_personal_emp, string oi_horario)
        {
            //BANDERA SERA VERDADERO SI PERTENECE FALSO EN CASO CONTRARIO
            bool bandera = false;

            try
            {
                //RECUPERO EL LEGAJO Y EL HORARIO
                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoPersonalEmp = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oi_personal_emp);
                HORARIO ddoHorario = HORARIO.Get(oi_horario, true);

                //SI EL HORARIO VALIDA CONVENIO VERIFICA: SI EL CONVENIO DEL PERSONAL ESTA DENTRO DE LOS CONVENIOS DEL HORARIO
                if (ddoHorario.l_convenio)
                {
                    //OBTENGO EL CONVENIO DE LA PERSONA Y RECORRO LA LISTA DE CONVENIOS DEL HORARIO
                    NucleusRH.Base.Organizacion.Convenios.CATEGORIA ddoCategoria = NucleusRH.Base.Organizacion.Convenios.CATEGORIA.Get(ddoPersonalEmp.oi_categoria_ult);

                    foreach (HORARIO_CONV objHorarioConv in ddoHorario.HORARIOS_CONVENIOS)
                    {
                        if (objHorarioConv.oi_convenio.Equals(ddoCategoria.oi_convenio.ToString()))
                        {
                            bandera = true;
                            break;
                        }

                    }

                }

                //SI EL HORARIO NO VALIDA CONVENIO RETORNA TRUE (CUALQUIER PERSONA INDEPENDIENTEMENTE DEL CONVENIO PODRA AGREGAR ESTE HORARIO)
                else
                {
                    bandera = true;
                }

            }

            catch (Exception) {
                bandera = false;
            }
           
            
            return bandera;
        }
        
        
    }
}



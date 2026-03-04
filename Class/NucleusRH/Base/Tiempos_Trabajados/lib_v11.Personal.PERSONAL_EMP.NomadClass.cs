using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Tiempos_Trabajados.Personal
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajo - Tiempos Trabajados
    public partial class PERSONAL_EMP : NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP
    {
        public static void ExportarNomina(int oi_terminal, Nomad.NSystem.Proxy.NomadXML filtro)
        {
            NomadBatch b = NomadBatch.GetBatch("Generar Nomina", "Generar Nomina");
            NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL TERM = NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.Get(oi_terminal);

            b.SetMess("Exportando Nomina a " + TERM.d_archivo_nom);

            //Bloqueo la Instancia.
            if (!NomadProxy.GetProxy().Lock().LockOBJ("GenerarNomina"))
                throw new Exception("La Nomina esta siendo generada en este Momento.");

            string MyXML = "";

            switch (TERM.c_formato_nom.ToUpper())
            {
                case "SRF":
                    throw new Exception("No se puede exportar a un Archivo de REGISTRACIONES.");

                case "SCF":
                    throw new Exception("No se puede exportar a un Archivo de REGISTRACIONES.");

                case "ASE":
                    throw new Exception("No se puede exportar a un Archivo de REGISTRACIONES.");

                case "USF":
                    throw new Exception("Formato USF NO implementado.");

                case "SNF":
                    MyXML = qryPartido(filtro, 100, PERSONAL_EMP.Resources.qry_personal).ToString();

                    b.SetPro(5);
                    b.SetSubBatch(5, 100);
                    NucleusRH.Base.Tiempos_Trabajados.SNFPersonal.NOMINA.ExportarNomina(oi_terminal, MyXML);
                    break;

                case "NAS":
                    MyXML = qryPartido(filtro, 100, PERSONAL_EMP.Resources.qry_personal_nas).ToString();

                    b.SetPro(5);
                    b.SetSubBatch(5, 100);
                    NucleusRH.Base.Tiempos_Trabajados.NASPersonal.NOMINA.ExportarNomina(oi_terminal, MyXML);
                    break;

                default:
                    throw new Exception("Formato de ARCHIVO Desconocido.");
            }
        }

        public static NomadXML qryPartido(NomadXML param, int registros, string qry)
        {
            NomadXML result = new NomadXML("ROWS");
            ArrayList childs = param.FirstChild().GetChilds();
            int childCount = childs.Count;
            for (int i = 0; i < childCount; i += registros)
            {
                System.Text.StringBuilder a = new System.Text.StringBuilder("<ROWS>");
                for (int j = i; j < i + registros && j < childCount; j++)
                    a.Append(childs[j].ToString());
                a.Append("</ROWS>");

                NomadXML qryout = NomadEnvironment.QueryNomadXML(qry, a.ToString());
                for (NomadXML row = qryout.FirstChild().FirstChild(); row != null; row = row.Next())
                    result.AddXML(row);
            }
            return result;
        }

        public static bool cerrarHorario(string id)
        {

            PERSONAL_EMP ddoPER;
            ddoPER = PERSONAL_EMP.Get(id);
            HORARIOPERS ddoHOR;
            ArrayList myDelList = new ArrayList();

            for (int t = 0; t < ddoPER.HORARIOSPERS.Count; t++)
            {
                ddoHOR = (HORARIOPERS)ddoPER.HORARIOSPERS[t];

                if (ddoHOR.f_fechaInicio >= ddoPER.f_egreso)
                    myDelList.Add(ddoHOR);
                else
                    if ((ddoHOR.f_fechaFinNull) || (ddoHOR.f_fechaFin > ddoPER.f_egreso))
                        ddoHOR.f_fechaFin = ddoPER.f_egreso;
            }

            foreach (HORARIOPERS ddoDEL in myDelList)
                ddoPER.HORARIOSPERS.Remove(ddoDEL);

            //LIMPIO EL NUMERO DE TARJETA Y LEGAJO RELOJ SI EL PARAMETRO MANTENER ES 0
            NomadLog.Debug("QRY_GET_PARAM_PER::: ");
            NomadXML xmlParam = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.GET_PARAM_PER, "<DATA />");
            NomadLog.Debug("QRY_GET_PARAM_PER: " +xmlParam.ToString());

      NomadLog.Debug("D_VALOR_QRYRESULT: " +xmlParam.FirstChild().GetAttr("d_valor"));

            if(xmlParam.FirstChild().GetAttr("d_valor")=="0")
            {
              NomadLog.Debug("....Entro al IF....");

              ddoPER.e_nro_legajo_relojNull = true;
              ddoPER.d_nro_tarjetaNull = true;
            }

            NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
            return true;

        }
        public static Nomad.NSystem.Proxy.NomadXML GetPresence(string pOIPersonal_emp, DateTime pDesde, DateTime pHasta, bool pIncEventos, bool pIncEsperanza, bool pIncFichadas, bool pIncDetalleFichadas, bool pIncNovedades, bool pIncLicencias, bool pIncHorasAut)
        {

            //El método principal se encuentra en la libreria.

            NucleusRH.Base.Tiempos_Trabajados.Procesos objMain;
            Nomad.NSystem.Proxy.NomadXML xmlResult;

            //Crea el objeto que tiene los métodos principales
            objMain = new NucleusRH.Base.Tiempos_Trabajados.Procesos();

            xmlResult = objMain.GetPresence(int.Parse(pOIPersonal_emp), pDesde, pHasta,
                                            pIncEventos, pIncEsperanza, pIncFichadas, pIncDetalleFichadas,
                                            pIncNovedades, pIncLicencias, pIncHorasAut);

            return xmlResult;

        }
        public void Cambio_Tarjeta(NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA tarjeta)
        {

            // Verifica que haya una tarjeta anterior
            if (this.TARJETAS.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA DDOTARJ = (TARJETA)this.TARJETAS[this.TARJETAS.Count - 1];
                if (DDOTARJ.f_desde >= tarjeta.f_desde)
                    throw new NomadAppException("La fecha de asignacion de la tarjeta debe ser posterior a la de la tarjeta anterior (" + DDOTARJ.f_desde.ToString("dd/MM/yyyy") + ")");

                //Verifica si la tarjeta esta asignada a otra persona
                NomadEnvironment.GetTrace().Info("PARAM -- <DATO d_nro_tarjeta=\"" + tarjeta.d_nro_tarjeta + "\" oi_personal_emp=\"" + this.id.ToString() + "\"/>");
                NomadXML xmlflag = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_TARJETA, "<DATO d_nro_tarjeta=\"" + tarjeta.d_nro_tarjeta + "\" oi_personal_emp=\"" + this.id + "\"/>");
                NomadEnvironment.GetTrace().Info("QRY_TARJETA:" + xmlflag.ToString());

                if (xmlflag.FirstChild().GetAttr("flag") == "1")
                    throw new NomadAppException("No puede asignarse la tarjeta (" + tarjeta.d_nro_tarjeta + ") dado que la misma actualmente esta asignada a otro legajo");

                // Asigna f_hasta de tarjeta anterior
                DDOTARJ.f_hasta = tarjeta.f_desde;

                // Asigna o_cambio_tarjeta de centro de costo anterior
                DDOTARJ.o_cambio_tarjeta = tarjeta.o_cambio_tarjeta;
            }
            tarjeta.o_cambio_tarjetaNull = true;

            // Asigna num tarjeta, f_desde_tarjeta a la persona
            this.d_nro_tarjeta = tarjeta.d_nro_tarjeta;
            this.f_desde_tarjeta = tarjeta.f_desde;
            this.TARJETAS.Add(tarjeta);

        }
        public void EliminarTarjeta()
        {

            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA del_obj = null;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA obj in this.TARJETAS)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_desde > del_obj.f_desde)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.TARJETAS.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA ult_obj = null;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA obj in this.TARJETAS)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_desde > ult_obj.f_desde)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.d_nro_tarjeta = ult_obj.d_nro_tarjeta;
                this.f_desde_tarjeta = ult_obj.f_desde;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.d_nro_tarjeta = "";
                this.f_desde_tarjetaNull = true;
            }

        }
        public static void CargarNovedades(Nomad.NSystem.Proxy.NomadXML param, string oi_terminal)
        {

            int C = 0;
            int E = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Carga de Novedades");

            ArrayList lista = (ArrayList)param.FirstChild().GetElements("ROW");
            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Asignando Novedad a Legajos " + (xml + 1) + " de " + lista.Count);

                //Cargo el legajo
                NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP ddoLEG = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(row.GetAttr("oi_personal_emp"));
                //Verifico las novedades
                foreach (NucleusRH.Base.Tiempos_Trabajados.Personal.NOVEDAD ddoNOV in ddoLEG.NOVEDADES)
                {
                    DateTime novfrom = ddoNOV.f_fecha.AddMinutes(ddoNOV.e_horainicio);
                    DateTime novto = ddoNOV.f_fecha.AddMinutes(ddoNOV.e_horafin);

                    if (Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("desde")) < novto && Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("hasta")) > novfrom)
                    {
                        objBatch.Err("Error al asignar novedad para el legajo: " + ddoLEG.e_numero_legajo + ": La novedad se superpone con otra novedad ya cargada");
                        E++;
                        continue;
                    }
                }

                DateTime fecf = Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("desde"));
                DateTime fect = Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("hasta"));
                TimeSpan difhor = fect.Subtract(fecf);

                //recupero la fecha que corresponde a las fechas ingresadas
                DateTime fecha;
                DateTime fec_1 = NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.GetDateHope(row.GetAttr("oi_personal_emp"), Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("desde")));
                DateTime fec_2 = NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.GetDateHope(row.GetAttr("oi_personal_emp"), Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("hasta")).AddMinutes(-1));
                NomadEnvironment.GetTrace().Info("FECHA DESDE -- " + fec_1.ToString());
                NomadEnvironment.GetTrace().Info("FECHA HASTA -- " + fec_2.ToString());

                //si las fechas no son las mismas, hay un error
                if (fec_1 != fec_2)
                {
                    objBatch.Err("Error al asignar novedad para el legajo: " + ddoLEG.e_numero_legajo + ": La novedad se superpone en dos días diferentes según el horario asignado al legajo");
                    E++;
                    continue;
                }
                else
                {
                    fecha = fec_1;
                    if (Convert.ToInt32(fecf.Subtract(fecha).TotalMinutes)>Convert.ToInt32(fect.Subtract(fecha).TotalMinutes))
                    {
                        objBatch.Err("Error al asignar novedad para el legajo: " + ddoLEG.e_numero_legajo + ": La hora desde ingresada no puede ser mayor a la hora hasta");
                        E++;
                        continue;
                    }
                }

                //Verifico si el dia esta bloqueado
                if (NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.EnLiquidacionCerrada(row.GetAttr("oi_personal_emp"), fecha))
                {
                    objBatch.Err("Error al asignar novedad para el legajo: " + ddoLEG.descr + ": La fecha indicada corresponde a un procesamiento de horas cerrado");
                    E++;
                    continue;
                }

                //Cargo el terminal para recuperar la estructura
                NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL ddoTERMINAL = NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.Get(oi_terminal);

                //Creo la Novedades
                NucleusRH.Base.Tiempos_Trabajados.Personal.NOVEDAD ddoNEWNOV = new NucleusRH.Base.Tiempos_Trabajados.Personal.NOVEDAD();
                ddoNEWNOV.d_novedad = row.GetAttr("d_novedad");
                ddoNEWNOV.e_horafin = Convert.ToInt32(fect.Subtract(fecha).TotalMinutes);
                ddoNEWNOV.e_horainicio = Convert.ToInt32(fecf.Subtract(fecha).TotalMinutes);
                ddoNEWNOV.f_fecha = fecha;
                //ddoNEWNOV.o_novedad = "Ingresada a través de planilla de carga de Novedades";
                ddoNEWNOV.oi_estructura = ddoTERMINAL.oi_estructura;
                ddoNEWNOV.oi_tipohora = row.GetAttr("oi_tipohora");

                ddoLEG.NOVEDADES.Add(ddoNEWNOV);
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(ddoLEG);
                    C++;
                }
                catch (Exception e)
                {
                    objBatch.Err("Se ha producido un error al cargar la novedad para el Legajo: " + ddoLEG.e_numero_legajo + ": " + e.Message);
                    E++;
                }
            }

            if (C != 0)
            {
                objBatch.Log("Se cargaron " + C.ToString() + " novedad/es.");
            }
            if (E != 0)
            {
                objBatch.Log("Se rechazó la carga para " + E.ToString() + " novedad/es.");
            }

        }
        public static void CargarCambioTurno(Nomad.NSystem.Proxy.NomadXML param)
        {

            int C = 0;
            int E = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Carga de Cambios de Turnos");

            ArrayList lista = (ArrayList)param.FirstChild().GetElements("ROW");
            for (int xml = 0; xml < lista.Count; xml++)
            {
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Asignando Cambio de Turno a Legajos " + (xml + 1) + " de " + lista.Count);

                string result = NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.AsignarHorario(row.GetAttr("oi_personal_emp"), row.GetAttr("desde"), row.GetAttr("hasta"), row.GetAttr("oi_horario"), row.GetAttr("oi_escuadra"));
                if (result == "OK")
                    C++;
                else
                {
                    objBatch.Err(result);
                    E++;
                    continue;
                }
            }

            if (C != 0)
            {
                objBatch.Log("Se cargaron " + C.ToString() + " cambio/s de turno/s.");
            }
            if (E != 0)
            {
                objBatch.Log("Se rechazó la carga para " + E.ToString() + " cambio/s de turno/s.");
            }

        }
        public bool IngresoPerTTA(string apellido, string nombres, string apeynom, string tipodoc, string nrodoc, string codigo, string sexo, string foto)
        {

            //Valido el c_personal
            string strval = "";
            strval = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", codigo, "", true);
            NomadLog.Info("strval 1 -- " + strval);
            if (strval != null)
            {
                throw new NomadAppException("Ya existe una persona con el Tipo y Numero de Documento especificado");
            }

            //Valido el nro de Legajo en la empresa
            strval = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", this.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + this.oi_empresa, true);
            NomadLog.Info("strval 2 -- " + strval);
            if (strval != null)
            {
                throw new NomadAppException("Ya existe un legajo con el numero de legajo especificado en la Empresa indicada");
            }

            //Valido el nro de legajo reloj
            strval = NomadEnvironment.QueryValue("TTA04_PERSONAL", "oi_personal_emp", "e_nro_legajo_reloj", this.e_nro_legajo_reloj.ToString(), "", true);
            NomadLog.Info("strval 3 -- " + strval);
            if (strval != null)
            {
                throw new NomadAppException("El numero de legajo reloj indicado ya se encuentra asignado");
            }

            //Creo la Persona
            NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = new NucleusRH.Base.Personal.Legajo.PERSONAL();
            ddoPER.d_apellido = apellido;
            ddoPER.d_nombres = nombres;
            ddoPER.d_ape_y_nom = apeynom;
            ddoPER.oi_tipo_documento = tipodoc;
            ddoPER.c_nro_documento = nrodoc;
            ddoPER.c_personal = codigo;
            ddoPER.c_sexo = sexo;
            if (foto != "")
                ddoPER.oi_foto = foto;

            ddoPER.Ingreso_Personal();
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);

            //Legajo
            NomadLog.Info("ddoPER.Id -- " + ddoPER.Id);
            this.oi_personal = ddoPER.Id;
            this.Ingreso_Personal();

            //Tarjeta
            if (this.d_nro_tarjeta != "")
            {
                NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA ddoTAR = new NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA();
                ddoTAR.d_nro_tarjeta = this.d_nro_tarjeta;
                ddoTAR.f_desde = this.f_desde_tarjeta;

                this.TARJETAS.Add(ddoTAR);
            }

            NomadEnvironment.GetCurrentTransaction().Save(this);
            return true;
        }

        public static void IngresoLegajosTTA()
        {
            NucleusRH.Base.Tiempos_Trabajados.InterfaceLegajoTTA.PERSONAL.ImportarLegajosTTA();
        }

        /// <summary>
        /// Realiza el set de los Francos Compensatorios
        /// </summary>
        public void SetFC(DateTime f_registro, int cant, string c_banco, bool l_aprobado)
        {
            NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objFC;
            NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objFCANT = null;
            NomadXML xmlBanco = new NomadXML();
            NomadXML xmlFC = new NomadXML();

            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("c_banco", c_banco);
            param.SetAttr("oi_personal_emp", this.id);
            param.SetAttr("f_registro", f_registro);

            //Busco el id del Banco de Hora
            xmlBanco.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.BancosHoras.BANCO_HORA.Resources.GET_BANCO, param.ToString()));
            if (xmlBanco.isDocument)
                xmlBanco = xmlBanco.FirstChild();

            if (xmlBanco.GetAttr("id") != "")
                param.SetAttr("oi_banco_hora", xmlBanco.GetAttr("id"));
            else
                throw new Exception("Error. El código de Banco: " + c_banco + " no existe.");

            //Busco si existe un registro ya generado
            xmlFC.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.FC_GENERADO, param.ToString()));
            if (xmlFC.isDocument)
            {
                xmlFC = xmlFC.FirstChild();
                NomadLog.Debug("Busca FC");
                if (xmlFC.GetAttr("oi_registro_fc") != "")
                {
                    NomadLog.Debug("Encontró un FC ya existente");
                    objFCANT = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(xmlFC.GetAttr("oi_registro_fc"));
                }
            }

            if (objFCANT == null)
            {
                objFC = new NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC();

                //Seteo los valores del nuevo registro
                objFC.f_registro_fc = f_registro;
                objFC.n_cant_gen = cant;
                if (l_aprobado)
                {
                    objFC.n_cant_aprob = cant;
                    objFC.n_saldo = cant;
                    objFC.l_aprob = true;
                }
                else
                {
                    objFC.n_saldo = 0;
                    objFC.l_aprob = false;
                }

                objFC.oi_banco_hora = xmlBanco.GetAttr("id");
                objFC.oi_personal_emp = this.Id;

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(objFC);
            }
            else
            {
                //Actualizo el registro ya existente en caso de que no esté blanqueado o saldado
                if (objFCANT.f_blanqueoNull && objFCANT.f_registro_sNull)
                {
                    objFCANT.n_cant_gen = cant;
                    if (l_aprobado)
                    {
                        objFCANT.n_cant_aprob_ant = objFCANT.n_cant_aprob;
                        objFCANT.n_cant_aprob = cant;
                        objFCANT.n_saldo = cant - objFCANT.n_consumidos;
                        if (objFCANT.l_aprob)
                            objFCANT.l_reaprob = true;
                        else
                            objFCANT.l_aprob = true;
                    }
                    else
                    {
                        objFCANT.n_cant_aprob_ant = objFCANT.n_cant_aprob;
                        objFCANT.l_aprob = false;
                        objFCANT.n_cant_aprobNull = true;
                        objFCANT.n_saldo = 0;
                    }

                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(objFCANT);
                }
            }
        }

    /// <summary>
        /// Realiza el set de los Francos Compensatorios (MODO VIEJO)
        /// </summary>
        public void SetComp(DateTime f_comp, int cant, string c_banco)
        {
            NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objFC = null;
      NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC objComp = null;
      NomadXML xmlCompList = new NomadXML();
      Hashtable htaRegistros = new Hashtable();
      int intCantActual;
      int intComp;

      NomadLog.Debug("---------------------------------------------------------------");
      NomadLog.Debug("Inicio PERSONAL_EMP.SetComp()");
      NomadLog.Debug("---------------------------------------------------------------");

      NomadLog.Debug("  f_comp: " + f_comp.ToString("yyyyMMdd"));
      NomadLog.Debug("  cant: " + cant.ToString());
      NomadLog.Debug("  c_banco: " + c_banco.ToString());

      //Recupera la cantidad compensada el día seleccionado
            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("c_banco", c_banco);
            param.SetAttr("oi_personal_emp", this.id);
            param.SetAttr("f_comp", f_comp);

      xmlCompList = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.FC_COMPEN_DETALLE, param.ToString());
      xmlCompList = xmlCompList.FirstChild();

      intCantActual = xmlCompList.GetAttrInt("cant");

      NomadLog.Debug("  CantActual: " + intCantActual.ToString());

      if (intCantActual == cant) {
        NomadLog.Debug("  Cantidades iguales");

        NomadLog.Debug("---------------------------------------------------------------");
        NomadLog.Debug("Inicio PERSONAL_EMP.SetComp()");
        NomadLog.Debug("---------------------------------------------------------------");

        return;
      }

      if (cant > intCantActual) {
        //Si la CANITDAD A COMPENSAR es MAYOR a la cantidad actual se deberá:
        //- verificar si en el registro actual queda tiempo libre
        //- buscar registros viejos libre e ir usándolos hasta completar la diferencia

        NomadLog.Debug("  cant > intCantActual");

        //Recupera los registros con saldo
        xmlCompList = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.FC_FC_LIBRES, param.ToString());
        xmlCompList = xmlCompList.FirstChild();
NomadLog.Debug("1");
        //Recorre el detalle que viene en el query
        for (NomadXML xmlComp = xmlCompList.FirstChild(); xmlComp != null; xmlComp = xmlComp.Next()) {
NomadLog.Debug("2");
          //Array por oi_registro por cada registro_fc
          objFC = GetRegistro(htaRegistros, xmlComp.GetAttrInt("oi_registro_fc"));
NomadLog.Debug("3");
          //Pregunta si debe modificar un Detalle en particular o debe crear uno nuevo
          if ( xmlComp.GetAttr("oi_comp_fc") != "" ) {
NomadLog.Debug("4");
            //Obtiene el detalle correspondiente para la fecha.
            objComp = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC) objFC.COMP_FC.GetByAttribute("f_comp", f_comp);
NomadLog.Debug("4.1");
            if (objComp == null)
              throw new Exception("Se produjo un error al intentar recuperar la compensación con fecha '" + f_comp.ToString("dd/MM/yyyy") + "' para el registro de FC de fecha '" + objFC.f_registro_fc.ToString("dd/MM/yyyy") + "'.");

          } else {
NomadLog.Debug("5");
            objComp = new NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC();
            objComp.f_comp      = f_comp;
            objComp.c_rango     = "CM";
            objComp.n_cant_comp = 0;

            objFC.COMP_FC.Add(objComp);
          }

NomadLog.Debug("6");
          //Pregunta si el registro se puede consumir completo con el registro actual
          if (objFC.n_saldo >= cant) {
NomadLog.Debug("7");
            objComp.n_cant_comp = objComp.n_cant_comp + cant;
            objFC.n_saldo       = objFC.n_saldo       - cant;
            objFC.n_consumidos  = objFC.n_consumidos  + cant;

            cant = 0;

            //Sale del for
            break;

          } else {
NomadLog.Debug("8");
            cant = cant - objFC.n_saldo;

            objComp.n_cant_comp = objComp.n_cant_comp + objFC.n_saldo;
            objFC.n_saldo       = 0;
            objFC.n_consumidos  = objFC.n_consumidos  + objFC.n_saldo;

          }

        }

        if (objComp == null) {
          NomadLog.Debug("  No existen Registros FC para compensar");
          throw new Exception("No existen Registros de FC para compensar para la fecha '" + f_comp.ToString("dd/MM/yyyy") + "'.");

        } else {
          //Pregunta si se pudo descontar la cantidad total. En caso de que sobre se le agrega el registro total para que de "negativo"
          if (cant > 0) {
            objComp.n_cant_comp = objComp.n_cant_comp + cant;
            objFC.n_saldo       = 0;
            objFC.n_consumidos  = objFC.n_consumidos  + cant;
          }
        }

      } else {
        //Si la CANITDAD A COMPENSAR es MENOR a la cantidad actual se deberá:
        //- recorrer los registros e ir restando desde el más nuevo hacia atrás

        NomadLog.Debug("  cant <= intCantActual");

        //Recorre el detalle que viene en el query
        for (NomadXML xmlComp = xmlCompList.FirstChild(); xmlComp != null; xmlComp = xmlComp.Next()) {
          intComp = xmlComp.GetAttrInt("n_cant_comp");

          if ( (intCantActual - intComp) >= cant) {
            //Se debe eliminar la compensación

            //Array por oi_registro por cada registro_fc
            objFC = GetRegistro(htaRegistros, xmlComp.GetAttrInt("oi_registro_fc"));

            //Obtiene el detalle correspondiente para la fecha.
            objComp = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC) objFC.COMP_FC.GetByAttribute("f_comp", f_comp);
            if (objComp == null)
              throw new Exception("Se produjo un error al intentar recuperar la compensación con fecha '" + f_comp.ToString("dd/MM/yyyy") + "' para el registro de FC de fecha '" + objFC.f_registro_fc.ToString("dd/MM/yyyy") + "'.");

            //Se elimina el detalle
            objFC.COMP_FC.Remove(objComp);

            //Se actualizan los datos del registro FC
            objFC.n_consumidos = objFC.n_consumidos - intComp;
            objFC.n_saldo      = objFC.n_saldo      + intComp;

            intCantActual      = intCantActual      - intComp;

                    } else {

            //Se debe actualizar la compensación

            //Array por oi_registro por cada registro_fc
            objFC = GetRegistro(htaRegistros, xmlComp.GetAttrInt("oi_registro_fc"));

            //Obtiene el detalle correspondiente para la fecha.
            objComp = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC) objFC.COMP_FC.GetByAttribute("f_comp", f_comp);
            if (objComp == null)
              throw new Exception("Se produjo un error al intentar recuperar la compensación con fecha '" + f_comp.ToString("dd/MM/yyyy") + "' para el registro de FC de fecha '" + objFC.f_registro_fc.ToString("dd/MM/yyyy") + "'.");

            //Se actualiza el detalle
            objComp.n_cant_comp = objComp.n_cant_comp - (intCantActual - cant);

            //Se actualizan los datos del registro FC
            objFC.n_consumidos = objFC.n_consumidos - intComp + objComp.n_cant_comp;
            objFC.n_saldo      = objFC.n_saldo      + intComp - objComp.n_cant_comp;

            intCantActual      = intCantActual      - (intComp + objComp.n_cant_comp);

            //Sale del for
            break;
          }

        }

      }

      //Si el array está cargado crea una transaccion y graba los datos
      if (htaRegistros.Count > 0) {

        NomadLog.Debug("  Se actualizan los registros FC");

        try {

          //Recorre todos los registros modificados y los graba
          foreach (int intKey in htaRegistros.Keys) {

            objFC = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC) htaRegistros[intKey];
            NomadEnvironment.GetCurrentTransaction().Save(objFC);

            //Blanquea
          }
        }
        catch (Exception ex)
        {
          NomadLog.Debug("Error al grabar el REGISTRO_FC: " + ex);
          throw;
        }
      }

      NomadLog.Debug("---------------------------------------------------------------");
      NomadLog.Debug("Fin PERSONAL_EMP.SetComp()");
      NomadLog.Debug("---------------------------------------------------------------");

        }

        /// <summary>
        /// Retorna y administra un "cache" de registros de FancosCompensatorios
        /// </summary>
        public static NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC GetRegistro(Hashtable htaRegisCache, int oi_registro_fc) {

      NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objFC;

      //Array por oi_registro por cada registro_fc
      if (htaRegisCache.ContainsKey(oi_registro_fc)) {
        objFC = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC) htaRegisCache[oi_registro_fc];
      } else {
        objFC = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC) NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(oi_registro_fc);
        htaRegisCache[oi_registro_fc] = objFC;
      }

      return objFC;

        }

        /// <summary>
        /// Realiza el get de los Francos Compensatorios para una fecha para un banco
        /// </summary>
        public int GetComp(DateTime f_compen, string c_banco)
        {
            NomadXML xmlCOMP = new NomadXML();
            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("c_banco", c_banco);
            param.SetAttr("oi_personal_emp", this.id);
            param.SetAttr("f_compen", f_compen);

            //Busco las compensaciones del legajo en el día
            xmlCOMP.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.FC_CANT_COMPEN, param.ToString()));
            if (xmlCOMP.isDocument)
            {
                xmlCOMP = xmlCOMP.FirstChild();
                if (xmlCOMP.GetAttr("n_cant_comp") != "" )
                    return xmlCOMP.GetAttrInt("n_cant_comp");
                else
                    return 0;
            }
            else
                return 0;
        }

        /// <summary>
        /// Realiza el get de los Francos Compensatorios de un legajo para un rango y un banco
        /// </summary>
        public static Hashtable GetComps(int oi_personal_emp, DateTime f_desde, DateTime f_hasta, string c_banco)
        {
            NomadXML xmlCOMP = new NomadXML();
            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("oi_personal_emp", oi_personal_emp);
            param.SetAttr("f_desde", f_desde);
      param.SetAttr("f_hasta", f_hasta);
            param.SetAttr("c_banco", c_banco);

            return NomadEnvironment.QueryHashtable(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.FC_CANT_COMPEN_RANGO, param.ToString(), "f_comp");

        }

        /// <summary>
        /// Elimina los francos generados para una fecha si no tiene compensaciones.
        /// Si tiene compensaciones pone la cantidad generada en 0 y actualiza el saldo.
        /// </summary>
        public void DelFC(DateTime f_registro)
        {
            NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objFC = null;
            NomadXML xmlFC = new NomadXML();

            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("oi_personal_emp", this.id);
            param.SetAttr("f_registro", f_registro);

            //Busco si existe un registro ya generado
            xmlFC.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.FC_GENERADO, param.ToString()));
            if (xmlFC.isDocument)
            {
                xmlFC = xmlFC.FirstChild();
                NomadLog.Debug("Busca FC");
                if (xmlFC.GetAttr("oi_registro_fc") != "")
                {
                    NomadLog.Debug("Encontró un FC ya existente");
                    objFC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(xmlFC.GetAttr("oi_registro_fc"));
                }
            }

            if (objFC != null)
            {
                if (objFC.COMP_FC.Count > 0)
                {
                    objFC.n_cant_gen = 0;
                    objFC.n_saldo = 0 - objFC.n_consumidos;
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(objFC);
                }
                else
                {
                    NomadEnvironment.GetCurrentTransaction().Delete(objFC);
                }
            }

        }
    }

  /// <summary>
  /// Item que representa los francos generados para una fecha
  /// </summary>
  public class clsItemF {
    public DateTime Fecha;

    public int DBOI;
    public int DBCantidad;
    public int DBSaldo;
    public bool DBAprobado;

    public int Cantidad;
    public int Saldo;
    public bool Aprobado;

    public NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC REGISTRO_FC;

    public clsItemF(DateTime pdteFecha) {
      this.Fecha = pdteFecha;
      this.Cantidad = 0;
      this.Aprobado = true;
      this.Saldo = 0;

      this.DBOI = 0;
      this.DBCantidad = 0;
      this.DBSaldo = 0;
      this.DBAprobado = true;
      this.REGISTRO_FC = null;
    }

  }

  /// <summary>
  /// Item que representa las compensaciones tomadas para una fecha
  /// </summary>
  public class clsItemC {
    public DateTime Fecha;
    public int Cantidad;

        public string Unidad;
    public int DBCantidad;

    public List<int> lisDetallePadres;

    public clsItemC(DateTime pdteFecha) {
      this.Fecha = pdteFecha;
      this.Cantidad = 0;

      this.DBCantidad = 0;

      this.lisDetallePadres = new List<int>();
    }

    public void AgregarDetallePadre(int OIPadre) {
      this.lisDetallePadres.Add(OIPadre);
    }

  }

  /// <summary>
  /// Clase para manejar los cache de FC
  /// </summary>
  public class clsBancoFC {
    private DateTime CurrentDate;

    public int OILegajo;
    public string Banco;
    public int OIBanco;
    public DateTime Fechainicio;
    public DateTime Fechafin;

    public SortedList<DateTime, clsItemF> dicFrancos; //Diccionario con los francos obtenidos de la BD. Luego se modifican con desde los conceptos.
    public Dictionary<DateTime, bool> dicSetFrancos; //Diccionario para indicar si se setean dos veces francos para un mismo dia.
    public Dictionary<DateTime, clsItemC> dicComps; //Diccionario con las compensaciones obtenidas desde la DB. Luego se modifican con desde los conceptos.

    public clsBancoFC(int pintOILegajo, string pstrBanco, DateTime pdteFechainicio, DateTime pdteFechafin) {
      this.OILegajo = pintOILegajo;
      this.Banco = pstrBanco;
      this.Fechainicio = pdteFechainicio;
      this.Fechafin = pdteFechafin;

      this.InitCache();
    }

        /// <summary>
        ///
        /// </summary>
    private void InitCache() {

      NomadXML xmlParam;
      NomadXML xmlData;
      clsItemF objItemF;
      clsItemC objItemC;

      //--------------------------------------------------------------------------------------------------------
      //Genera el cache de Francos -----------------------------------------------------------------------------
      this.dicFrancos = new SortedList<DateTime, clsItemF>();
      this.dicSetFrancos = new Dictionary<DateTime, bool>();

      //Recupera la cantidad compensada el día seleccionado
            xmlParam = new NomadXML("PARAM");
            xmlParam.SetAttr("c_banco", this.Banco);
            xmlParam.SetAttr("oi_personal_emp", this.OILegajo);
      //Usado en Compensaciones
            xmlParam.SetAttr("f_desde", Fechainicio);
      xmlParam.SetAttr("f_hasta", Fechafin);

      //Carga los REGISTRO_FC libres --
      xmlData = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.FC_FC_LIBRES, xmlParam.ToString());
      xmlData = xmlData.FirstChild();

      //Carga el OI_BANCO
      this.OIBanco = xmlData.GetAttrInt("oi_banco_hora");

      for (NomadXML xmlRegistroFC = xmlData.FirstChild(); xmlRegistroFC != null; xmlRegistroFC = xmlRegistroFC.Next()) {

        objItemF = new clsItemF(xmlRegistroFC.GetAttrDateTime("f_registro_fc"));

        objItemF.DBOI = xmlRegistroFC.GetAttrInt("oi_registro_fc");

        objItemF.DBCantidad = xmlRegistroFC.GetAttrInt("n_cant_aprob");
        objItemF.Cantidad = xmlRegistroFC.GetAttrInt("n_cant_aprob");

        objItemF.DBSaldo = xmlRegistroFC.GetAttrInt("n_saldo");
        objItemF.Saldo = xmlRegistroFC.GetAttrInt("n_saldo");

        this.dicFrancos[xmlRegistroFC.GetAttrDateTime("f_registro_fc")] = objItemF;

      }

      //Completa los dias libres del periodo procesado
      for (DateTime dteDia = this.Fechainicio; dteDia <= this.Fechafin; dteDia = dteDia.AddDays(1) ) {
        if (!this.dicFrancos.ContainsKey(dteDia)) {
          this.dicFrancos[dteDia] = new clsItemF(dteDia);
        }
      }

      //--------------------------------------------------------------------------------------------------------
      //Genera el cache de Compensaciones ----------------------------------------------------------------------
      this.dicComps = new Dictionary<DateTime, clsItemC>();

      xmlData = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.FC_COMPEN_DETALLE, xmlParam.ToString());
      xmlData = xmlData.FirstChild();

      for (NomadXML xmlComp = xmlData.FirstChild(); xmlComp != null; xmlComp = xmlComp.Next()) {

        if ( this.dicComps.ContainsKey(xmlComp.GetAttrDateTime("f_comp")) ) {
          objItemC = this.dicComps[xmlComp.GetAttrDateTime("f_comp")];
        } else {
          objItemC = new clsItemC(xmlComp.GetAttrDateTime("f_comp"));
          this.dicComps[xmlComp.GetAttrDateTime("f_comp")] = objItemC;
        }

        objItemC.Cantidad = objItemC.Cantidad + xmlComp.GetAttrInt("n_cant_comp");
        objItemC.DBCantidad = objItemC.DBCantidad + xmlComp.GetAttrInt("n_cant_comp");
                objItemC.Unidad = xmlComp.GetAttr("c_unidad_tiempo");
        objItemC.AgregarDetallePadre(xmlComp.GetAttrInt("oi_registro_fc"));
      }

      //Completa los dias libres del periodo procesado
      for (DateTime dteDia = this.Fechainicio; dteDia <= this.Fechafin; dteDia = dteDia.AddDays(1)) {
        if ( !this.dicComps.ContainsKey(dteDia) ) {
          this.dicComps[dteDia] = new clsItemC(dteDia);
        }

      }

    }

    public void SetFecha(DateTime pdteFecha) {
      this.CurrentDate = pdteFecha;
    }

        /// <summary>
    /// Realiza el set de los Francos Compensatorios (MODO NUEVO)
        /// </summary>
    public void SetComp(int pintCant) {
      this.SetComp(this.CurrentDate, pintCant);
    }

        /// <summary>
    /// Realiza el set de los Francos Compensatorios (MODO NUEVO)
        /// </summary>
    public void SetComp(DateTime pdteFecha, int pintCant) {
      clsItemC objItemC;
      objItemC = (clsItemC) this.dicComps[pdteFecha];
      objItemC.Cantidad = pintCant;
    }

        /// <summary>
    ///
        /// </summary>
    public void SetFranco(int pintCant, bool pbolAprobado) {
      this.SetFranco(this.CurrentDate, pintCant, pbolAprobado);
    }

        /// <summary>
    /// Adiciona el valor del parámetro pintCant a la cantidad actual acumulado en el Banco de hora.
    /// El parámetro pbolAprobado indica si la cantidad está aprobada o no.
        /// </summary>
    public void SetFranco(DateTime pdteFecha, int pintCant, bool pbolAprobado) {
      clsItemF objItemF;

      if (!this.dicFrancos.ContainsKey(pdteFecha))
        throw new Exception("Se produjo un error al intentar generar Francos para la fecha '" + pdteFecha.ToString("dd/MM/yyyy") + "'. Fecha no encontrada en la colección de Francos (dicFrancos).");

      objItemF = (clsItemF) this.dicFrancos[pdteFecha];
      objItemF.Cantidad = pintCant;
      objItemF.Aprobado = pbolAprobado;

      //Valida si existe una entrada para la fecha.
      if (this.dicSetFrancos.ContainsKey(pdteFecha))
        NomadEnvironment.GetTraceBatch().Warning("Francos Compensatorios - Se están asignando francos más de una vez para la fecha '" + pdteFecha.ToString("dd/MM/yyyy") + "'.");
      else
        this.dicSetFrancos[pdteFecha] = true;

      //Actualiza los saldos
      if (pbolAprobado) {
        objItemF.Saldo = objItemF.DBSaldo + (pintCant - objItemF.DBCantidad);
      }
    }

/*
        /// <summary>
        /// Realiza el get de los Francos Compensatorios generados para una jornada y un banco de hora
    /// Es decir, retorna las horas de ausencia por compensación.
        /// </summary>
        public int GetComp(DateTime pdteFecha, DateTime pdteFechainicio, DateTime pdteFechafin)
        {
            string strFecJornada;
      int intCompensado = 0;

      if (this.dicComps == null) {
        this.dicComps = Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.GetComps(this.OILegajo, pdteFechainicio, pdteFechafin, this.Banco);
      }

      //Busca si tiene Compnesaciones para el día en particular
      strFecJornada = pdteFecha.ToString("yyyyMMdd");

      if (this.dicComps.ContainsKey(strFecJornada)) {
        NomadXML xmlCOMP = (NomadXML) dicLegajoFC[strFecJornada];
        intCompensado = xmlCOMP.GetAttrInt("n_cant_comp");
      }

      return intCompensado;
        }
*/
        /// <summary>
    ///
        /// </summary>
    public void SaveData() {

      //Actualiza los FRANCOS ---------------------------------------------------------------------
      this.UpdateFrancos();
      this.LogFrancos();

      //Actualiza las COMPENSACIONES --------------------------------------------------------------
      this.UpdateCompensaciones();
      this.LogComps();

      //Recorre los REGISTROS_FC a guardar
      foreach (clsItemF objItemF in this.dicFrancos.Values) {

        if (objItemF.REGISTRO_FC != null) {
          NomadEnvironment.GetCurrentTransaction().Save(objItemF.REGISTRO_FC);
        }
      }

    }

        /// <summary>
    ///
        /// </summary>
    private void UpdateFrancos() {
      NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC objComp;

      //Recorre los Francos seteados en el procesamiento
      foreach (clsItemF objItemF in this.dicFrancos.Values) {

        if (objItemF.Cantidad != objItemF.DBCantidad || objItemF.Saldo < 0) {

          //Se produjo un set Franco. Determina si debe crear uno nuevo registro o realizar un UPDATE

          if (objItemF.DBOI != 0) {
            //Debe hacer una actualización
            objItemF.REGISTRO_FC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(objItemF.DBOI);

            //if (objItemF.REGISTRO_FC.f_blanqueoNull && objItemF.REGISTRO_FC.f_registro_sNull) {
            if (objItemF.REGISTRO_FC.f_blanqueoNull) {

              objItemF.REGISTRO_FC.n_cant_gen = objItemF.Cantidad;

              if (objItemF.Aprobado) {

                objItemF.REGISTRO_FC.n_cant_aprob = objItemF.Cantidad;
                objItemF.REGISTRO_FC.n_saldo = objItemF.Cantidad - objItemF.REGISTRO_FC.n_consumidos;
                //objItemF.REGISTRO_FC.n_cant_aprob_ant = objItemF.Cantidad;

                objItemF.REGISTRO_FC.l_aprob = true;
                //objItemF.REGISTRO_FC.l_reaprob = true;

                if (objItemF.REGISTRO_FC.n_saldo < 0) {
                  //TODO: SOLO para aguas. Hasta que se agrege un parámetro en la ORG26. PAra cuando lo FRANCO Y COMPENSADO esde solo desde el Procesamiento.
                  objItemF.REGISTRO_FC.n_saldo = objItemF.Cantidad;
                  objItemF.REGISTRO_FC.n_consumidos = 0;

                  objItemF.Saldo = objItemF.REGISTRO_FC.n_saldo;

                  //Elimina los childs de las referencias de cada compensación
                  foreach (clsItemC objItemC in this.dicComps.Values) {
                    if (objItemC.lisDetallePadres.Contains(objItemF.DBOI)) {

                      objItemC.lisDetallePadres.Remove(objItemF.DBOI);

                      objComp = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC) objItemF.REGISTRO_FC.COMP_FC.GetByAttribute("f_comp", objItemC.Fecha);

                      objItemC.DBCantidad = objItemC.DBCantidad - objComp.n_cant_comp;
                    }
                  }

                  //Elimina los childs del REGISTRO. Loego se regenerarán con las respectivas compensaciones.
                  objItemF.REGISTRO_FC.COMP_FC.Clear();

                }

              } else {

                //Pregunta si lo anterior esta aprobado
                if (objItemF.REGISTRO_FC.l_aprob) {
                  if (objItemF.Cantidad <= objItemF.DBCantidad) {
                    objItemF.REGISTRO_FC.n_cant_aprob = objItemF.Cantidad;
                    objItemF.REGISTRO_FC.n_saldo = objItemF.Cantidad - objItemF.REGISTRO_FC.n_consumidos;
                    //objItemF.REGISTRO_FC.n_cant_aprob_ant = objItemF.Cantidad;

                    objItemF.REGISTRO_FC.l_aprob = true;
                    //objItemF.REGISTRO_FC.l_reaprob = true;
                  } else {
                    objItemF.REGISTRO_FC.n_cant_aprob_ant = objItemF.REGISTRO_FC.n_cant_aprob;
                    objItemF.REGISTRO_FC.l_aprob = false;
                    objItemF.REGISTRO_FC.n_cant_aprobNull = true;
                    objItemF.REGISTRO_FC.n_saldo = 0 - objItemF.REGISTRO_FC.n_consumidos;
                  }
                } else {
                    //objItemF.REGISTRO_FC.n_cant_aprob_ant = objItemF.REGISTRO_FC.n_cant_aprob;
                    objItemF.REGISTRO_FC.l_aprob = false;
                    objItemF.REGISTRO_FC.n_cant_aprobNull = true;
                    objItemF.REGISTRO_FC.n_saldo = 0 - objItemF.REGISTRO_FC.n_consumidos;
                }

              }

              if (objItemF.REGISTRO_FC.n_saldo > 0) {
                objItemF.REGISTRO_FC.f_registro_sNull = true;
              }

            }

          } else {
            //Debe crear un REGISTRO_FC
            objItemF.REGISTRO_FC = new NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC();

            objItemF.REGISTRO_FC.oi_personal_emp = this.OILegajo.ToString();
            objItemF.REGISTRO_FC.oi_banco_hora = this.OIBanco.ToString();

            //Seteo los valores del nuevo registro
            objItemF.REGISTRO_FC.f_registro_fc = objItemF.Fecha;
            objItemF.REGISTRO_FC.n_cant_gen = objItemF.Cantidad;

            objItemF.REGISTRO_FC.l_aprob = objItemF.Aprobado;
            if (objItemF.REGISTRO_FC.l_aprob) {
              objItemF.REGISTRO_FC.n_cant_aprob = objItemF.Cantidad;
              objItemF.REGISTRO_FC.n_saldo = objItemF.Cantidad;
            } else {
              objItemF.REGISTRO_FC.n_saldo = 0;
            }
          }
        }

      }

    }

    /// <summary>
        ///
        /// </summary>
    private void UpdateCompensaciones() {
      NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC objComp;
      clsItemF objItemF;
      int intDiferencia;
      int intACompensar;

      //Recorre todas las compensasiones PARA LIBERAR TIEMPOS
      foreach (clsItemC objItemC in this.dicComps.Values) {

        if (objItemC.Cantidad < objItemC.DBCantidad) {

          //Si la CANTIDAD A COMPENSAR es MENOR a la cantidad actual se deberá:
          //- recorrer los registros e ir restando desde el más nuevo hacia atrás
          intDiferencia = objItemC.DBCantidad - objItemC.Cantidad;

          foreach (int OIRegistroPadre in objItemC.lisDetallePadres ) {
            objItemF = this.GetRegistro(OIRegistroPadre);

            objComp = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC) objItemF.REGISTRO_FC.COMP_FC.GetByAttribute("f_comp", objItemC.Fecha);
            if (objComp == null)
              throw new Exception("Se produjo un error al intentar recuperar la compensación con fecha '" + objItemC.Fecha.ToString("dd/MM/yyyy") + "' para el registro de FC de fecha '" + objItemF.REGISTRO_FC.f_registro_fc.ToString("dd/MM/yyyy") + "'.");

            if (objComp.n_cant_comp > intDiferencia) {

              objComp.n_cant_comp = objComp.n_cant_comp - intDiferencia;

              objItemF.REGISTRO_FC.n_saldo = objItemF.REGISTRO_FC.n_saldo + intDiferencia;
              objItemF.REGISTRO_FC.n_consumidos = objItemF.REGISTRO_FC.n_consumidos - intDiferencia;
              objItemF.REGISTRO_FC.f_registro_sNull = true;

              objItemF.Saldo = objItemF.Saldo + intDiferencia;

              intDiferencia = 0;

              break;

            } else {
              //intACompensar = intDiferencia - objComp.n_cant_comp;
              intACompensar = objComp.n_cant_comp;

              objItemF.REGISTRO_FC.n_saldo = objItemF.REGISTRO_FC.n_saldo + intACompensar;
              objItemF.REGISTRO_FC.n_consumidos = objItemF.REGISTRO_FC.n_consumidos - intACompensar;
              objItemF.REGISTRO_FC.f_registro_sNull = true;

              objItemF.Saldo = objItemF.Saldo + intACompensar;

              intDiferencia = intDiferencia - intACompensar;
              objItemF.REGISTRO_FC.COMP_FC.Remove(objComp);

            }

          }

          if (intDiferencia > 0)
            throw new Exception("Se produjo un error al intentar liberar compensaciones para la fecha '" + objItemC.Fecha.ToString("dd/MM/yyyy") + "'.");
        }
      }

      //Recorre todas las compensasiones PARA TOMAR TIEMPOS
      foreach (clsItemC objItemC in this.dicComps.Values) {

        if (objItemC.Cantidad > objItemC.DBCantidad) {
          //La cantidad a compensar es MAYOR a la canitdad en DB
          intDiferencia = objItemC.Cantidad - objItemC.DBCantidad;

          foreach(clsItemF objItemFViejo in this.dicFrancos.Values) {

            if (objItemFViejo.Saldo <= 0) continue;

            if (objItemFViejo.Fecha > objItemC.Fecha) break;

            intACompensar = (objItemFViejo.Saldo < intDiferencia) ? objItemFViejo.Saldo : intDiferencia;

            LoadRegistro(objItemFViejo);

            //Busca por si ya existe un detalle de compensación con la misma fecha
            objComp = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC) objItemFViejo.REGISTRO_FC.COMP_FC.GetByAttribute("f_comp", objItemC.Fecha);
            if (objComp == null) {
              objComp = new NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC();
              objComp.f_comp      = objItemC.Fecha;
              objComp.c_rango     = "CM";
              objComp.n_cant_comp = intACompensar;

              objItemFViejo.REGISTRO_FC.COMP_FC.Add(objComp);

            } else {
              objComp.n_cant_comp = objComp.n_cant_comp + intACompensar;
            }

            objItemFViejo.Saldo = objItemFViejo.Saldo - intACompensar;
            intDiferencia = intDiferencia - intACompensar;

            objItemFViejo.REGISTRO_FC.n_saldo = objItemFViejo.REGISTRO_FC.n_saldo - intACompensar;
            objItemFViejo.REGISTRO_FC.n_consumidos = objItemFViejo.REGISTRO_FC.n_consumidos + intACompensar;

            if (objItemFViejo.REGISTRO_FC.n_saldo <= 0) {
              objItemFViejo.REGISTRO_FC.f_registro_s = objItemC.Fecha;
            } else {
              objItemFViejo.REGISTRO_FC.f_registro_sNull = true;
            }
          }

          if (intDiferencia > 0) {
            //Sobran tiempos
            NucleusRH.Base.Tiempos_Trabajados.RHLiq.LiqConceptosBase.Warning("Descuenta '" + intDiferencia.ToString() + "' "+objItemC.Unidad =="H" ?"horas":"minutos"+ " mas de lo acumulado en la fecha '" + objItemC.Fecha.ToString("dd/MM/yyyy") + "'.");
          }

        }
      }

    }

    /// <summary>
        ///
        /// </summary>
        private clsItemF GetRegistro(int oi_registro_fc) {

      clsItemF objResultado = null;
      NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objFC;

      //Recorre el Hastable de Registros
      foreach (clsItemF objItemF in this.dicFrancos.Values) {

        if (objItemF.DBOI == oi_registro_fc) {
          if (objItemF.REGISTRO_FC != null) {
            objResultado = objItemF;
            break;

          } else {
            LoadRegistro(objItemF);
            objResultado = objItemF;
            break;
          }

        }
      }

      //Si no estaba en el caché hay que crear la entrada
      if (objResultado == null) {

        objFC = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC) NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(oi_registro_fc);

        objResultado = new clsItemF(objFC.f_registro_fc);
        objResultado.REGISTRO_FC = objFC;

        objResultado.Saldo = objFC.n_saldo;
        objResultado.Cantidad = objFC.n_cant_aprob;
        objResultado.DBSaldo = objFC.n_saldo;
        objResultado.DBCantidad = objFC.n_cant_aprob;
        //TODO: analizar el aprobado

        this.dicFrancos[objFC.f_registro_fc] = objResultado;
      }

      return objResultado;

        }

    /// <summary>
        ///
        /// </summary>
        private void LoadRegistro(clsItemF objtarget) {

      if (objtarget.REGISTRO_FC != null ) return;

      objtarget.REGISTRO_FC = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC) NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(objtarget.DBOI);

        }

    /// <summary>
        /// Realiza un log en el archivo de log del proceso de los Francos del legajo
        /// </summary>
    private void LogFrancos() {

      NomadLog.Debug("Detalle de los Francos (REGISTROS_FC) -------------------------------------------------------- ");
      //Recorre los REGISTROS_FC
      foreach (clsItemF objItemF in this.dicFrancos.Values) {

        NomadLog.Debug("Fecha: " + objItemF.Fecha.ToString("dd/MM/yyyy"));
        NomadLog.Debug("CANT:"+ objItemF.Cantidad.ToString() + "          APROBADO:" + objItemF.Aprobado.ToString() + "          SALDO:" + objItemF.Saldo.ToString() + "");
        NomadLog.Debug("DBCANT:"+ objItemF.DBCantidad.ToString() + "          DBAPROBADO:" + objItemF.DBAprobado.ToString() + "          DBSALDO:" + objItemF.DBSaldo.ToString() + "");
        //if (objItemF.REGISTRO_FC != null)
          //NomadLog.Debug("DBCANT:"+ objItemF.REGISTRO_FC.Length.ToString());
      }
      NomadLog.Debug("Fin detalle de los Francos (REGISTROS_FC) -------------------------------------------------------- ");

    }

    /// <summary>
        /// Realiza un log en el archivo de log del proceso de los Francos del legajo
        /// </summary>
    private void LogComps() {

      NomadLog.Debug("Detalle de las compensaciones (futuras COMP_FC) -------------------------------------------------------- ");
      //Recorre los REGISTROS_FC
      foreach (clsItemC objItemC in this.dicComps.Values) {

        NomadLog.Debug("Fecha: " + objItemC.Fecha.ToString("dd/MM/yyyy"));
        NomadLog.Debug("CANT:"+ objItemC.Cantidad.ToString());
        NomadLog.Debug("DBCANT:"+ objItemC.DBCantidad.ToString());
      }
      NomadLog.Debug("Detalle de las compensaciones (futuras COMP_FC) -------------------------------------------------------- ");

    }

  }
}



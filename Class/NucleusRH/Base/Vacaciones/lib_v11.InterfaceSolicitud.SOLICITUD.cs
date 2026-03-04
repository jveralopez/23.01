using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Vacaciones.InterfaceSolicitud
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interface Posiciones
    public partial class SOLICITUD
    {
        public static void Importar(string oi_empresa)
        {
            int totRegs, linea = 0, errores = 0, procesados=0;
            string str_habiles, d_habiles = "";
            bool l_habiles = false;

            NomadXML MyROW;
            NomadXML MyXML = new NomadXML();
            SOLICITUD objRead;
            DateTime fCompare = new DateTime(1900, 1, 1);

            //Instancio el Objeto Batch
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Importación Masiva de Solicitudes de Vacaciones", "Importación Masiva de Solicitudes de Vacaciones");

            //Ejecuto el query que trae el archivo de solicitud.dat
            NomadBatch.Trace("Cargando el Query...");
            MyXML = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Vacaciones.InterfaceSolicitud.SOLICITUD.Resources.QRY_OIArchivo, "");
            totRegs = MyXML.FirstChild().ChildLength;

            for (linea = 1, MyROW = MyXML.FirstChild().FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
            {
                //Creando interface
                objBatch.SetPro(0, 100, MyXML.FirstChild().ChildLength, linea);
                objBatch.SetMess("Importando la Linea " + linea + " de " + totRegs);
                NomadLog.Info("0-- " + MyROW.GetAttr("id"));
                objRead = NucleusRH.Base.Vacaciones.InterfaceSolicitud.SOLICITUD.Get(MyROW.GetAttr("id"));
                
                try
                {
                    
                    if (objRead.e_numero_legajo == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro - Linea: " + linea.ToString());
                        errores++;
                        continue;
                    }

                    if (objRead.f_desde_solicitudNull || objRead.f_desde_solicitud < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha Desde de la Solicitud, se rechaza el registro - Linea: " + linea.ToString());
                        errores++;
                        continue;
                    }

                    if (objRead.f_hasta_solicitudNull || objRead.f_hasta_solicitud < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha Hasta de la Solicitud, se rechaza el registro - Linea: " + linea.ToString());
                        errores++;
                        continue;
                    }

                    if (objRead.f_hasta_solicitud < objRead.f_desde_solicitud)
                    {
                        objBatch.Err("La Fecha Desde de la Solicitud es mayor a la Fecha Hasta - Linea: " + linea.ToString());
                        errores++;
                        continue;
                    }
                    
                    //Recupero el Legajo
                    string oi_personal_emp = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo, "PER02_PERSONAL_EMP.oi_empresa = " + oi_empresa +" AND PER02_PERSONAL_EMP.oi_indic_activo=1", true);
                    if (oi_personal_emp == null)
                    {
                        objBatch.Err("No existe el Legajo "+objRead.e_numero_legajo+" en estado activo en la Empresa, se rechaza el registro - Linea: " + linea.ToString());
                        errores++;
                        continue;
                    }

                    NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP personal = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp);

                    bool bandera = false;
                    //Verifico Solapamientos
                    foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD chkSOL in personal.SOLICITUDES)
                    {
                        if (chkSOL.oi_estado_solic!="1" && chkSOL.oi_estado_solic != "3" && !bandera)
                        {
                            System.DateTime FD, FH;
                            FD = chkSOL.f_desde_solicitud;
                            FH = (chkSOL.f_interrupcionNull ? chkSOL.f_hasta_solicitud : chkSOL.f_interrupcion);
                            if (objRead.f_desde_solicitud <= FH && objRead.f_hasta_solicitud >= FD)
                            {
                                objBatch.Err("La solicitud del legajo " + objRead.e_numero_legajo + " en el periodo " + objRead.f_desde_solicitud.ToString("dd/MM/yyyy") + "-" + objRead.f_hasta_solicitud.ToString("dd/MM/yyyy") + " se solapa con una solicitud previa - Linea: " + linea.ToString());
                                errores++;
                                bandera = true;
                            }
                        }
                    }

                    //Verifico cuenta de vacaciones
                    if (personal.CTA_CTE_VAC.Count == 0)
                    {
                        objBatch.Err("El legajo "+ objRead.e_numero_legajo +" no tiene cuenta corriente de vacaciones - Linea: "+linea.ToString());
                        errores++;
                        continue;
                    }

                    //Cantidad de Dias Pendientes
                    int e_CountPendientes = 0;
                    foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC m_CtaCte in personal.CTA_CTE_VAC)
                        e_CountPendientes += m_CtaCte.e_dias_pend;

                    //Calcula la cantidad de dias
                    int e_cant_dias = 0;
                    string dias_bonif = "0";
                    if (personal.DiasCorridos(objRead.f_desde_solicitud))
                        e_cant_dias  = ((int)((TimeSpan)(objRead.f_hasta_solicitud.AddDays(1) - objRead.f_desde_solicitud)).TotalDays);
                    else
                    { 
                        e_cant_dias = personal.DiasTrabajables(objRead.f_desde_solicitud, objRead.f_hasta_solicitud);
                        str_habiles = personal.DiasTrabajablesString(objRead.f_desde_solicitud, objRead.f_hasta_solicitud);
                        l_habiles = true;
                        d_habiles = str_habiles;
                    }
                    //Verifico la cantidad de días no sea mayor o igual a 60
                    if (e_cant_dias >= 60)
                    {
                        objBatch.Err("Hay más de 60 días entre la fecha desde y hasta. - Linea: " + linea.ToString());
                        errores++;
                        continue;
                    }

                    //Calcular la cantidad de dias bonificados
                    NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.GetDiasBonif(personal.Id, objRead.f_desde_solicitud, objRead.f_hasta_solicitud.AddDays(1), ref dias_bonif);

                    NomadEnvironment.GetCurrentTransaction().Begin();

                    //Si no se solapan las solicitudes de vacaciones anteriores
                    if (!bandera)
                    {
                        //Agrego la Solicitud
                        NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD objSOL = new NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD();
                        objSOL.e_dias_solicitud = e_cant_dias;
                        objSOL.e_dias_bonif = int.Parse(dias_bonif);
                        objSOL.f_desde_solicitud = objRead.f_desde_solicitud;
                        objSOL.f_hasta_solicitud = objRead.f_hasta_solicitud;
                        objSOL.f_estado_solic = System.DateTime.Now;
                        objSOL.f_solicitud = System.DateTime.Now;
                        objSOL.oi_estado_solic = "1";
                        objSOL.l_automatica = false;
                        objSOL.d_motivo_solic = objRead.d_motivo_solic;
                        objSOL.o_solicitud = objRead.o_solicitud;
                        objSOL.l_interfaz = true;
                        objSOL.l_notificada = false;
                        objSOL.l_habiles = l_habiles;
                        objSOL.d_habiles = d_habiles;
                        personal.SOLICITUDES.Add(objSOL);
                    }

                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP LEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oi_personal_emp);

                    //BUSCO SI EL LEGAJO TIENE CALENDARIO, PARA DETERMINAR SI LA LICENCIA ABARCA O NO DIAS FERIADOS
                    NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO CAL = LEG.Getoi_calendario_ult();
                    if (CAL != null)
                    {
                        DateTime fechaDesde = objRead.f_desde_solicitud;
                        DateTime fechaHasta = objRead.f_hasta_solicitud;

                        NucleusRH.Base.Organizacion.Calendario_Feriados.DIA_FERIADO FER_INICIO = (NucleusRH.Base.Organizacion.Calendario_Feriados.DIA_FERIADO)CAL.DIAS_FERIADOS.GetByAttribute("f_feriado", fechaDesde);
                        if (FER_INICIO != null && FER_INICIO.c_tipo != "NOLAB")
                        {
                            objBatch.Wrn("La solicitud ingresada empieza en un día feriado ("+fechaDesde.ToString("dd/MM/yyyy")+")... Legajo: " + LEG.e_numero_legajo + " Linea : " + linea.ToString());   
                        }

                        NucleusRH.Base.Organizacion.Calendario_Feriados.DIA_FERIADO FER_HASTA = (NucleusRH.Base.Organizacion.Calendario_Feriados.DIA_FERIADO)CAL.DIAS_FERIADOS.GetByAttribute("f_feriado", fechaHasta);
                        if (FER_HASTA != null && FER_HASTA.c_tipo != "NOLAB")
                        {
                            objBatch.Wrn("La solicitud ingresada termina en un día feriado ("+fechaHasta.ToString("dd/MM/yyyy")+")... Legajo: " + LEG.e_numero_legajo + " Linea : " + linea.ToString());
                        }

                    }
                    
                    //Grabo
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(personal);
                        NomadEnvironment.GetCurrentTransaction().Commit();
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro - Linea: " + linea.ToString() + " - " + e.Message);
                        errores++;
                        NomadEnvironment.GetCurrentTransaction().Rollback();
                    }


                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + linea.ToString() + " - " + e.Message);
                    errores++;
                }



            }

            procesados = linea - 1;
            objBatch.Log("Registros Procesados: " + procesados.ToString() + " - Importados: " + (procesados - errores).ToString());
            objBatch.Log("Finalizado...");
              
            
        }
    }
}

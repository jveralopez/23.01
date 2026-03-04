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

namespace NucleusRH.Base.Configuracion.Progs
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Programaciónes
    public partial class PROG_DET : Nomad.NSystem.Base.NomadObject
    {
        public bool NextRun(DateTime pFI, ref DateTime pFC)
        {
            NomadLog.Info("TEST (id:" + this.Id + ") " + this.d_prog_det);

            //Valido si tiene Horas Definidas
            if (this.d_horas == "")
            {
                NomadLog.Warning("return (ERROR): Horas no definida.");
                return false;
            }

            //Horas a CORRER
            string[] Horas = this.d_horas.Split('|');

            DateTime TESTDATETIME;

            //Verifico si tiene que correr el DIA ACTUAL....
            if (this.RunDay(pFI.Date))
            {
                for (int t = 0; t < Horas.Length; t++)
                {
                    TESTDATETIME = pFI.Date;

                    if (Horas[t].Length == 3)
                    {
                        TESTDATETIME = TESTDATETIME.AddHours(int.Parse(Horas[t].Substring(0, 1)));
                        TESTDATETIME = TESTDATETIME.AddMinutes(int.Parse(Horas[t].Substring(1, 2)));
                    }
                    else
                        if (Horas[t].Length == 4 || Horas[t].Length == 6)
                        {
                            TESTDATETIME = TESTDATETIME.AddHours(int.Parse(Horas[t].Substring(0, 2)));
                            TESTDATETIME = TESTDATETIME.AddMinutes(int.Parse(Horas[t].Substring(2, 2)));
                        }
                        else
                            continue;

                    if (TESTDATETIME > pFI)
                    {
                        pFC = TESTDATETIME;
                        NomadLog.Info("return (HOY): " + pFC.ToString("dd/MM/yyyy HH:mm"));
                        return true;
                    }
                }
            }

            if (d_annos == "")
            {
                //No tiene AÑO definido...
                //verificar los proximo 4 años

                for (int t = 1; t <= 1461; t++)
                {
                    TESTDATETIME = pFI.Date.AddDays(t);

                    if (this.RunDay(pFI.Date.AddDays(t)))
                    {
                        if (Horas[0].Length == 3)
                        {
                            TESTDATETIME = TESTDATETIME.AddHours(int.Parse(Horas[0].Substring(0, 1)));
                            TESTDATETIME = TESTDATETIME.AddMinutes(int.Parse(Horas[0].Substring(1, 2)));
                        }
                        else
                            if (Horas[0].Length == 4 || Horas[0].Length == 6)
                            {
                                TESTDATETIME = TESTDATETIME.AddHours(int.Parse(Horas[0].Substring(0, 2)));
                                TESTDATETIME = TESTDATETIME.AddMinutes(int.Parse(Horas[0].Substring(2, 2)));
                            }
                            else
                            {
                                NomadLog.Warning("return (NO ANNO-ERROR): Hora mal definida. '" + Horas[0] + "'");
                                return false;
                            }

                        pFC = TESTDATETIME;
                        NomadLog.Info("return (NO ANNO): " + pFC.ToString("dd/MM/yyyy HH:mm"));
                        return true;
                    }
                }
            }
            else
            {
                //Tiene AÑO definido...
                bool found = false;
                DateTime foundDT = DateTime.Now;
                string[] Annos = this.d_annos.Split('|');

                //Recorro los Años
                for (int a = 0; a < Annos.Length; a++)
                {
                    //Anno a Verificar
                    int anno = int.Parse(Annos[a]);

                    //Ignoro los Años Menores
                    if (anno < pFI.Date.Year) continue;

                    //Calculo el Primer dia a Verificar
                    if (anno == pFI.Date.Year) TESTDATETIME = pFI.Date.AddDays(1);
                    else TESTDATETIME = new DateTime(anno, 1, 1);

                    //Recorro los dias de ese Año
                    while (anno == TESTDATETIME.Year)
                    {
                        if (this.RunDay(TESTDATETIME))
                        {
                            if (!found || foundDT > TESTDATETIME)
                            {
                                found = true;
                                foundDT = TESTDATETIME;
                            } break;
                        }
                        TESTDATETIME = TESTDATETIME.AddDays(1);
                    }
                }

                //Devuelvo el Mejor dia
                if (found)
                {
                    if (Horas[0].Length == 3)
                    {
                        foundDT = foundDT.AddHours(int.Parse(Horas[0].Substring(0, 1)));
                        foundDT = foundDT.AddMinutes(int.Parse(Horas[0].Substring(1, 2)));
                    }
                    else
                        if (Horas[0].Length == 4 || Horas[0].Length == 6)
                        {
                            foundDT = foundDT.AddHours(int.Parse(Horas[0].Substring(0, 2)));
                            foundDT = foundDT.AddMinutes(int.Parse(Horas[0].Substring(2, 2)));
                        }
                        else
                        {
                            NomadLog.Warning("return (SI ANNO-ERROR): Hora mal definida. '" + Horas[0] + "'");
                            return false;
                        }

                    pFC = foundDT;
                    NomadLog.Info("return (SI ANNO): " + pFC.ToString("dd/MM/yyyy HH:mm"));
                    return true;
                }
            }

            NomadLog.Warning("return (NO ENCOTRADO)");
            return false;
        }
        public static void Save(string PARENT, ref NucleusRH.Base.Configuracion.Progs.PROG_DET pobjPROGDET)
        {
            string HORAS;
            string DIAS_SEMANA;
            string DIAS_MES;
            string MESES;
            string ANOS;
            string[] aAUX;
            string EVENT;
            string GEVENT;
            string HORAS_EVENT;
            int t;

            //Horas de Ejecucion
            if (pobjPROGDET.d_horas == "")
                HORAS = "Horario sin definir";
            else
            {
                aAUX = pobjPROGDET.d_horas.Split('|');
                if (aAUX.Length == 1) HORAS = " a la "; else HORAS = " a las ";

                for (t = 0; t < aAUX.Length; t++)
                {
                    //Separador
                    if (t != 0)
                    {
                        if (t == aAUX.Length - 1)
                            HORAS += " y ";
                        else
                            HORAS += ", ";
                    }

                    //Hora
                    if (aAUX[t].Length == 3)
                        HORAS += aAUX[t].Substring(0, 1) + ":" + aAUX[t].Substring(1, 2);
                    else
                        if (aAUX[t].Length == 4 || aAUX[t].Length == 6)
                            HORAS += aAUX[t].Substring(0, 2) + ":" + aAUX[t].Substring(2, 2);
                        else
                            HORAS += "ERROR";
                }

                //fin mensaje
                HORAS += " horas";
            }

            //Dias de la Semana
            if (pobjPROGDET.d_dias_semana == "")
                DIAS_SEMANA = "Días sin Definir";
            else
            {
                aAUX = pobjPROGDET.d_dias_semana.Split('|');
                if (aAUX.Length == 1) DIAS_SEMANA = " el día "; else DIAS_SEMANA = " los días ";

                for (t = 0; t < aAUX.Length; t++)
                {
                    //Separador
                    if (t != 0)
                    {
                        if (t == aAUX.Length - 1)
                            DIAS_SEMANA += " y ";
                        else
                            DIAS_SEMANA += ", ";
                    }

                    //Dias de la SEMANA
                    switch (aAUX[t])
                    {
                        case "1": DIAS_SEMANA += "lunes"; break;
                        case "2": DIAS_SEMANA += "martes"; break;
                        case "3": DIAS_SEMANA += "miercoles"; break;
                        case "4": DIAS_SEMANA += "jueves"; break;
                        case "5": DIAS_SEMANA += "viernes"; break;
                        case "6": DIAS_SEMANA += "sábado"; break;
                        case "7": DIAS_SEMANA += "domingo"; break;
                        default: DIAS_SEMANA += "ERROR"; break;
                    }
                }
            }

            //Dias del Mes
            if (pobjPROGDET.d_dias_mes == "")
                DIAS_MES = "Días sin Definir";
            else
            {
                aAUX = pobjPROGDET.d_dias_mes.Split('|');
                if (aAUX.Length == 1) DIAS_MES = " el día "; else DIAS_MES = " los días ";

                for (t = 0; t < aAUX.Length; t++)
                {
                    //Separador
                    if (t != 0)
                    {
                        if (t == aAUX.Length - 1)
                            DIAS_MES += " y ";
                        else
                            DIAS_MES += ", ";
                    }

                    //Dias de la SEMANA
                    switch (aAUX[t])
                    {
                        case "UL":
                            if (aAUX.Length == 1) DIAS_MES = "";
                            DIAS_MES += "el último día del mes";
                            break;

                        case "PE":
                            if (aAUX.Length == 1) DIAS_MES = "";
                            DIAS_MES += "el penúltimo día del mes";
                            break;

                        default: DIAS_MES += aAUX[t]; break;
                    }
                }
            }

            //Meses
            if (pobjPROGDET.d_meses == "")
                MESES = "Meses sin Definir";
            else
            {
                aAUX = pobjPROGDET.d_meses.Split('|');
                if (aAUX.Length == 1) MESES = " de "; else MESES = " de los meses ";

                for (t = 0; t < aAUX.Length; t++)
                {
                    //Separador
                    if (t != 0)
                    {
                        if (t == aAUX.Length - 1)
                            MESES += " y ";
                        else
                            MESES += ", ";
                    }

                    //Dias de la SEMANA
                    switch (aAUX[t])
                    {
                        case "01": MESES += "enero"; break;
                        case "02": MESES += "febrero"; break;
                        case "03": MESES += "marzo"; break;
                        case "04": MESES += "abril"; break;
                        case "05": MESES += "mayo"; break;
                        case "06": MESES += "junio"; break;
                        case "07": MESES += "julio"; break;
                        case "08": MESES += "agosto"; break;
                        case "09": MESES += "septiembre"; break;
                        case "10": MESES += "octubre"; break;
                        case "11": MESES += "noviembre"; break;
                        case "12": MESES += "diciembre"; break;
                        default: MESES += "ERROR"; break;
                    }
                }
            }

            //Annos
            if (pobjPROGDET.d_annos == "")
                ANOS = "Años sin Definir";
            else
            {
                aAUX = pobjPROGDET.d_annos.Split('|');
                if (aAUX.Length == 1) ANOS = " de "; else ANOS = " de los años ";

                for (t = 0; t < aAUX.Length; t++)
                {
                    //Separador
                    if (t != 0)
                    {
                        if (t == aAUX.Length - 1)
                            ANOS += " y ";
                        else
                            ANOS += ", ";
                    }

                    //Definicion de Años
                    ANOS += aAUX[t];
                }
            }

            //Evento
            string hAux;
            string hAux2;

            //Seteo el código de evento para el detalle
            EVENT = pobjPROGDET.c_evento;

            //Seteo el código de grupo de evento para el detalle
            if (pobjPROGDET.c_grupo_evento != "")
            {
            NomadXML xmlParam = new NomadXML("PARAMS");
            xmlParam.SetAttr("UUID", pobjPROGDET.c_grupo_evento);
            NomadXML xmldesc = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Configuracion.Progs.PROG_DET.Resources.DescGrupoEvento, xmlParam.ToString());
            GEVENT = "[" + xmldesc.FirstChild().FirstChild().GetAttr("DES") + "] ";
            }
            else GEVENT = "";

            //Seteo las horas para el detalle
            if (pobjPROGDET.c_modo_evento == "T")
            {
                HORAS_EVENT = pobjPROGDET.d_horas;
                hAux = HORAS_EVENT.Substring(0, 2);
                hAux2 = HORAS_EVENT.Substring(2, 2);
                if (hAux != "00")
                {
                    if (hAux2 == "00")
                    {
                        if (hAux == "01") HORAS_EVENT = hAux + " hora";
                        else HORAS_EVENT = hAux + " horas";
                    }
                    else
                    {
                        if (hAux == "01") HORAS_EVENT = hAux + " hora " + hAux2 + " minutos";
                        else HORAS_EVENT = hAux + " horas " + hAux2 + " minutos";
                    }
                }
                else
                {
                    HORAS_EVENT = hAux2 + " minutos";
                }
            }
            else
            {
                HORAS_EVENT = "";
            }

            //Calculo la descripcion
            switch (pobjPROGDET.c_tipo)
            {
                case "D":
                    pobjPROGDET.d_prog_det = "Diariamente, " + HORAS;
                    break;

                case "S":
                    pobjPROGDET.d_prog_det = "Semanalmente, " + DIAS_SEMANA + " " + HORAS;
                    break;

                case "M":
                    pobjPROGDET.d_prog_det = "Mensualmente, " + DIAS_MES + " " + HORAS;
                    break;

                case "A":
                    pobjPROGDET.d_prog_det = "Anualmente, " + DIAS_MES + " " + MESES + " " + HORAS;
                    break;

                case "1":
                    pobjPROGDET.d_prog_det = DIAS_MES + " " + MESES + " " + ANOS + " " + HORAS;
                    break;

                case "X":
                    pobjPROGDET.d_prog_det = "Programación Avanzada, para más información ver el detalle.";
                    break;

                case "E":
                    if (pobjPROGDET.c_modo_evento == "T")
                    {
                        pobjPROGDET.d_prog_det = "Al ejecutarse el evento " + GEVENT + EVENT + ", luego de " + HORAS_EVENT;
                    }
                    else
                    {
                        pobjPROGDET.d_prog_det = "Al ejecutarse el evento " + GEVENT + EVENT + "," + HORAS;
                    }
                    break;

                default:
                    pobjPROGDET.d_prog_det = "Programación Desconocida.";
                    break;
            }
            if (pobjPROGDET.d_prog_det.Length > 100) pobjPROGDET.d_prog_det = pobjPROGDET.d_prog_det.Substring(1, 100);

            PROG objPROG;

            //Agrego el Objeto
            if (pobjPROGDET.IsForInsert)
            {
                NomadLog.Debug("pobjPROGDET ID " + pobjPROGDET.Id);

                //Obtengo el Objeto PADRE
                objPROG = PROG.Get(PARENT);

                //Agrego el Objeto HIJO
                objPROG.DETALLES.Add(pobjPROGDET);
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(objPROG);

                //Obtengo el Objeto
                pobjPROGDET = PROG_DET.Get(objPROG.DETALLES[objPROG.DETALLES.Count - 1].Id);

                NomadLog.Debug("pobjPROGDET DDO DET " + pobjPROGDET.ToString(true));
            }
            else
            {
                //Guardo el Objeto HIJO
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(pobjPROGDET);

                //Obtengo el Objeto PADRE
                objPROG = PROG.Get(PARENT);
            }

            //Actualizo la Descripcion
            if (objPROG.DETALLES.Count == 0)
            {
                objPROG.d_prog = "No se especifico ninguna programación.";
            }
            else
                if (objPROG.DETALLES.Count > 1)
                {
                    objPROG.d_prog = "Programación múltiple, para más información ver el detalle.";
                }
                else
                {
                    objPROG.d_prog = ((PROG_DET)objPROG.DETALLES[0]).d_prog_det;
                }

            //Guardo los Datos.
            PROG.Schedule(objPROG,DateTime.Now);
        }
        public bool RunDay(DateTime pFI)
        {

            bool found;
            int t;
            string[] aAUX;
            int iAUX;
            string sAUX;
            bool ul, pe;

            /////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico el ANNO
            if (this.d_annos != "")
            {
                aAUX = this.d_annos.Split('|');

                found = false;
                for (t = 0; t < aAUX.Length && !found; t++)
                    if (pFI.Year == int.Parse(aAUX[t]))
                        found = true;

                if (!found)
                {
                    //NomadLog.Debug("ANNO '"+pFI.Year+"' NO ENCONTRADO EN '"+this.d_annos+"'");
                    return false;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico el MES
            if (this.d_meses != "")
            {
                aAUX = this.d_meses.Split('|');

                found = false;
                for (t = 0; t < aAUX.Length && !found; t++)
                    if (pFI.Month == int.Parse(aAUX[t]))
                        found = true;

                if (!found)
                {
                    //NomadLog.Debug("MESES '"+pFI.Month+"' NO ENCONTRADO EN '"+this.d_meses+"'");
                    return false;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico la SEMANA de ANO
            if (this.d_semanas_anno != "")
            {
                //Ultimas Semanas
                pe = ul = false;
                iAUX = (int)Math.Floor(((TimeSpan)(new DateTime(pFI.Year, 1, 1) - pFI)).TotalDays / 7);
                if (iAUX == 0) ul = true;
                if (iAUX == 1) pe = true;

                //Primeras Semanas
                iAUX = (int)Math.Floor(((TimeSpan)(pFI - new DateTime(pFI.Year, 1, 1))).TotalDays / 7);

                aAUX = this.d_semanas_anno.Split('|');

                found = false;
                for (t = 0; t < aAUX.Length && !found; t++)
                    if (aAUX[t] == "UL")
                    {
                        if (ul)
                            found = true;
                    }
                    else
                        if (aAUX[t] == "PE")
                        {
                            if (pe)
                                found = true;
                        }
                        else
                            if (iAUX == int.Parse(aAUX[t]))
                                found = true;

                if (!found)
                {
                    //NomadLog.Debug("SEMANAS ANNO '"+iAUX+"', pe:'"+(pe?"SI":"NO")+"', ul:'"+(ul?"SI":"NO")+"' NO ENCONTRADO EN '"+this.d_semanas_anno+"'");
                    return false;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico la SEMANA de MES
            if (this.d_semanas_mes != "")
            {
                //Ultimas Semanas
                pe = ul = false;
                iAUX = (int)Math.Floor(
                    ((TimeSpan)(new DateTime(pFI.Year, pFI.Month, 1).AddMonths(1).AddDays(-1) - pFI)).TotalDays / 7
                );
                if (iAUX == 0) ul = true;
                if (iAUX == 1) pe = true;

                //Primeras Semanas
                iAUX = (int)Math.Floor(
                    ((TimeSpan)(pFI - new DateTime(pFI.Year, pFI.Month, 1))).TotalDays / 7
                );

                aAUX = this.d_semanas_mes.Split('|');

                found = false;
                for (t = 0; t < aAUX.Length && !found; t++)
                    if (aAUX[t] == "UL")
                    {
                        if (ul)
                            found = true;
                    }
                    else
                        if (aAUX[t] == "PE")
                        {
                            if (pe)
                                found = true;
                        }
                        else
                            if (iAUX == int.Parse(aAUX[t]))
                                found = true;

                if (!found)
                {
                    //NomadLog.Debug("SEMANAS MES '"+iAUX+"', pe:'"+(pe?"SI":"NO")+"', ul:'"+(ul?"SI":"NO")+"' NO ENCONTRADO EN '"+this.d_semanas_mes+"'");
                    return false;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico la DIAS del MES
            if (this.d_dias_mes != "")
            {
                //Ultimos Dias
                pe = ul = false;
                if (new DateTime(pFI.Year, pFI.Month, 1).AddMonths(1).AddDays(-1) == pFI) ul = true;
                if (new DateTime(pFI.Year, pFI.Month, 1).AddMonths(1).AddDays(-2) == pFI) pe = true;

                //Primeros Dias
                iAUX = pFI.Day;

                aAUX = this.d_dias_mes.Split('|');

                found = false;
                for (t = 0; t < aAUX.Length && !found; t++)
                    if (aAUX[t] == "UL")
                    {
                        if (ul)
                            found = true;
                    }
                    else
                        if (aAUX[t] == "PE")
                        {
                            if (pe)
                                found = true;
                        }
                        else
                            if (iAUX == int.Parse(aAUX[t]))
                                found = true;

                if (!found)
                {
                    //NomadLog.Debug("DIAS MES '"+iAUX+"', pe:'"+(pe?"SI":"NO")+"', ul:'"+(ul?"SI":"NO")+"' NO ENCONTRADO EN '"+this.d_dias_mes+"'");
                    return false;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico la DIAS de la SEMANA
            if (this.d_dias_semana != "")
            {
                //Primeros Dias
                sAUX = "ERR";
                switch (pFI.DayOfWeek)
                {
                    case System.DayOfWeek.Monday: sAUX = "1"; break;
                    case System.DayOfWeek.Tuesday: sAUX = "2"; break;
                    case System.DayOfWeek.Wednesday: sAUX = "3"; break;
                    case System.DayOfWeek.Thursday: sAUX = "4"; break;
                    case System.DayOfWeek.Friday: sAUX = "5"; break;
                    case System.DayOfWeek.Saturday: sAUX = "6"; break;
                    case System.DayOfWeek.Sunday: sAUX = "7"; break;
                }

                aAUX = this.d_dias_semana.Split('|');

                found = false;
                for (t = 0; t < aAUX.Length && !found; t++)
                    if (aAUX[t] == sAUX)
                        found = true;

                if (!found)
                {
                    //NomadLog.Debug("DIAS SEMANA '"+sAUX+"' NO ENCONTRADO EN '"+this.d_dias_semana+"'");
                    return false;
                }
            }

            return true;
        }
        public static void Delete(string PARENT)
        {

            PROG_DET objPROGDET = PROG_DET.Get(PARENT, true);
            string oi_prog = objPROGDET.oi_prog.ToString();

            //Elimino el Objeto
            NomadEnvironment.GetCurrentTransaction().Delete(objPROGDET);

            //Obtengo el Objeto PADRE
            PROG objPROG = PROG.Get(oi_prog);

            //Actualizo la Descripcion
            if (objPROG.DETALLES.Count == 0)
            {
                objPROG.d_prog = "No se especifico ninguna programación.";
            }
            else
                if (objPROG.DETALLES.Count > 1)
                {
                    objPROG.d_prog = "Programación múltiple, para más información ver el detalle.";
                }
                else
                {
                    objPROG.d_prog = ((PROG_DET)objPROG.DETALLES[0]).d_prog_det;
                }

            //Guardo los Datos.
            PROG.Schedule(objPROG,DateTime.Now);
        }
    }
}



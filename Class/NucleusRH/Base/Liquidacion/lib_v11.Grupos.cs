using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.Grupos
{
    public partial class GRUPO
    {
        public static void MigrarGruposVariables()
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Migracion", "Migracion de grupos y variables");

            //Obtengo el archivo
            objBatch.SetMess("Obtentiendo archivo...");
            NomadXML xmlFile = NomadEnvironment.GetProxy().FileServiceIO().LoadFileXML("INTERFACES", "gruposvariables.dat");
            if (xmlFile.isDocument) xmlFile = xmlFile.FirstChild();

            //Listas auxiliares
            int cntGrupos = 0;
            int cntCambios = 0;
            Hashtable byName = new Hashtable();
            Hashtable byPos = new Hashtable();
            Hashtable Grupos = new Hashtable();
            Hashtable DDObyName = new Hashtable();
            Hashtable DDObyPos = new Hashtable();
            Hashtable DDOvariables = new Hashtable();

            //Codigo
            //string Grupo = xmlFile.GetAttr("codigo").Trim().ToLower();
			string Grupo = xmlFile.GetAttr("codigo").Trim();

            //Analizo el archivo
            objBatch.SetMess("Analizando...");
            for (NomadXML item = xmlFile.FirstChild(); item!=null; item=item.Next())
            {
              //Nombre
              if (item.GetAttr("nombre")=="") { objBatch.Err("Hay un registro que no tiene definido el nombre de variable"); return; }
              string nombre = item.GetAttr("nombre").Trim();
			  //string nombre = item.GetAttr("nombre").Trim().ToLower();

              //Ubicacion
              if (item.GetAttr("ubicacion")=="") { objBatch.Err("La variable '"+nombre+"' no tiene definida la ubicacion"); return; }
              int ubicacion = item.GetAttrInt("ubicacion");
              if (ubicacion < 1 || ubicacion > 120) { objBatch.Err("La variable '"+nombre+"' tiene una ubicacion no valida (entre 1 y 120)"); return; }

              //Extension
              //string extension = item.GetAttr("extension").ToLower();
			  string extension = item.GetAttr("extension");

              //Valido
              if (byName.ContainsKey(nombre)) { objBatch.Err("La variable '"+nombre+"' esta repetida"); return; }
              if (byPos.ContainsKey(ubicacion+"."+extension)) { objBatch.Err("La ubicacion '"+ubicacion+"/"+extension+"' esta repetida"); return; }

              //Buscar
              string oi_variable = NomadEnvironment.QueryValue("LIQ09_VARIABLES", "oi_variable", "c_variable", nombre, "", false);
              if (oi_variable == "")
              {
                objBatch.Wrn("La variable '"+nombre+"' no existe");
                continue;
              }

              //Agrego
              byName[nombre] = ubicacion+"."+extension;
              byPos[ubicacion+"."+extension] = nombre;
              DDOvariables[nombre] = oi_variable;

              //objBatch.Log("FILE:" + DDOvariables[nombre] + " - " + ubicacion+"."+extension + " - " + nombre);

              //Obtengo el grupo
              GRUPO ddoGrupo;
              if (!Grupos.ContainsKey(extension)) {

                //Cargo el grupo
                string oi_grupo = NomadEnvironment.QueryValue("LIQ31_GRUPOS", "oi_grupo", "c_grupo", Grupo+(extension != "" ?"."+extension:""), "", false);
                if (oi_grupo == "")
                {
                    ddoGrupo = new GRUPO();
                    ddoGrupo.c_grupo = Grupo+(extension != "" ? "."+extension : "");
                    cntGrupos++;
                }
                else
                {
                    ddoGrupo = GRUPO.Get(oi_grupo,true);

                    //Recorro los childs
                    foreach(GRUPO_VAR ddoVAR in ddoGrupo.GRUPO_VAR)
                    {
                      DDObyName[ddoVAR.oi_variable.ToString()] = ddoVAR.id;
                      DDObyPos[ddoVAR.c_columna.Substring(8)+"."+extension] = ddoVAR.id;

                      //objBatch.Log("DB:" + ddoVAR.oi_variable.ToString() + " - " + ddoVAR.c_columna.Substring(8)+"."+extension + " - " + ddoVAR.id);
                    }
                }

                //Corrijo la descripcion
                ddoGrupo.d_grupo = xmlFile.GetAttr("descripcion") + (extension != "" ?" (Extension "+extension+")" : "");
                Grupos[extension] = ddoGrupo;
              } else
              {
                ddoGrupo = (GRUPO)Grupos[extension];
              }
            }

            //Actualizando
            objBatch.SetMess("Actualizando...");
            foreach(string variableName in byName.Keys)
            {
              string oi_variable = DDOvariables[variableName].ToString();
              string ubicacion = byName[variableName].ToString();

              if (DDObyName.ContainsKey(oi_variable))
              {
                if (!DDObyPos.ContainsKey(ubicacion))
                {
                  objBatch.Err("No se puede agregar la variable '"+variableName+"' en la ubicacion '" + ubicacion+  "' (ya esta en otra ubicacion - caso 1)");
                  return;
                }
                if (DDObyName[oi_variable].ToString() != DDObyPos[ubicacion].ToString())
                {
                  objBatch.Err("No se puede agregar la variable '"+variableName+"' en la ubicacion '" + ubicacion+  "' (ya esta en otra ubicacion - caso 2)");
                  return;
                }
              } else
              {
                //Validar uso de posision
                if (DDObyPos.ContainsKey(ubicacion))
                {
                  objBatch.Err("No se puede agregar la variable '"+variableName+"' en la ubicacion '" + ubicacion+  "' (hay otra variable en esa ubicacion)");
                  return;
                }

                //Agregar
                GRUPO_VAR grupoVar = new GRUPO_VAR();
                grupoVar.c_columna = "n_valor_" + ubicacion.Split('.')[0];
                grupoVar.oi_variable = oi_variable;

                //Grupo
                GRUPO ddoGrupo = (GRUPO)Grupos[ubicacion.Split('.')[1]];
                ddoGrupo.GRUPO_VAR.Add(grupoVar);

                //Cambios
                cntCambios++;
              }
            }

            if (cntCambios == 0 && cntGrupos == 0)
            {
                objBatch.Wrn("No se detectaron cambios!!!");
                return;
            }

            //Actualizando
            objBatch.SetMess("Guardando...");
            foreach(string extension in Grupos.Keys)
            {
              GRUPO ddoGrupo = (GRUPO)Grupos[extension];
              NomadEnvironment.GetCurrentTransaction().Save(ddoGrupo);
            }

            if (cntGrupos > 0) objBatch.Log("Se agregaron '" + cntGrupos + "' grupos");
            if (cntCambios > 0) objBatch.Log("Se agregaron '" + cntCambios + "' variables");
            objBatch.Log("Finalizado");
        }

        public static void MigrarDatosVariables(string oi_liquidacion,int e_periodo_desde,int e_periodo_hasta)
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Iniciando...", "Migracion de Datos de Variables");

            NomadXML xmlLiquidaciones = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Grupos.GRUPO.Resources.QRY_LIQ, "<DATA oi_liquidacion='" + oi_liquidacion + "' e_periodo_desde='" + e_periodo_desde + "' e_periodo_hasta='" + e_periodo_hasta + "' />").FirstChild();
            NomadXML xmlplantilla = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Grupos.GRUPO.Resources.QRY_PLANTILLA, "<DATA />").FirstChild();

            for (NomadXML liq = xmlLiquidaciones.FirstChild(); liq != null; liq = liq.Next())
            {
                NomadXML xmlRecibos = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Grupos.GRUPO.Resources.QRY_RECIBOS, "<DATA oi_liquidacion='" + liq.GetAttr("oi_liquidacion") + "' />").FirstChild();

                NomadXML xmlGrupos = new NomadXML(xmlRecibos.FirstChild().ToString()).FirstChild();
                for (NomadXML ois = xmlGrupos.FirstChild(); ois != null; ois = ois.Next())
                {
                    for (NomadXML plantilla = xmlplantilla.FirstChild(); plantilla != null; plantilla = plantilla.Next())
                    {
                        ois.AddTailElement(new NomadXML(plantilla.ToString()));
                    }
                }

                int i = 1;
                objBatch.SetPro(0, 100, xmlLiquidaciones.ChildLength, i);
                objBatch.SetMess("Procesando Liquidacion " + liq.GetAttr("c_liquidacion"));

                for (NomadXML rec = xmlRecibos.FirstChild(); rec != null; rec = rec.Next())
                {
                    if (rec.Name == "REC") continue;
                    NomadXML xmlDatos = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Grupos.GRUPO.Resources.QRY_DATOS, "<DATA oi_tot_liq_per=\"" + rec.GetAttr("value") + "\" />").FirstChild();

                    string oi_tot_liq_per_actual = "";
                    NomadXML xmlPlantilla;
                    Recibos.TOT_LIQ_PER recibo = null;
                    Hashtable grupoVariable = new Hashtable();

                    for (NomadXML dato = xmlDatos.FirstChild(); dato != null; dato = dato.Next())
                    {

                        if (oi_tot_liq_per_actual != dato.GetAttr("oi_tot_liq_per"))
                        {
                            if (recibo != null)
                            {
                                guardarRecibo(recibo, grupoVariable);
                                recibo = null;
                            }
                            recibo = Recibos.TOT_LIQ_PER.Get(dato.GetAttr("oi_tot_liq_per"),false);
                            xmlPlantilla = xmlGrupos.FindElement2("ROW", "oi_tot_liq_per", dato.GetAttr("oi_tot_liq_per"));

                            ArrayList lista =  xmlPlantilla.GetChilds();
                            foreach (NomadXML pl in lista)
                            {
                                NomadXML p = pl.FirstChild();
                                Recibos.VAR_GR_CONC GrupoConcepto = (Recibos.VAR_GR_CONC)recibo.VAR_GR_CONC.GetByAttribute("c_grupo", p.GetAttr("c_grupo"));
                                if (GrupoConcepto != null)
                                {
                                    recibo.VAR_GR_CONC.Remove(GrupoConcepto);
                                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(recibo);
                                    //p.SetAttr("nmd-status", "~u");
                                }
                                grupoVariable.Add(p.GetAttr("c_grupo"), p);
                            }
                        }

                        Hashtable auxiliar = (Hashtable)grupoVariable.Clone();
                        foreach (string key in grupoVariable.Keys)
                        {
                            auxiliar[key] = grupoVariable[key].ToString().Replace("\"" + dato.GetAttr("c_variable") + "\"", "\"" + dato.GetAttr("n_valor_s") + "\"");
                        }
                        grupoVariable = auxiliar;
                        oi_tot_liq_per_actual = dato.GetAttr("oi_tot_liq_per");

                    }

                    if (recibo != null)
                    {
                        guardarRecibo(recibo, grupoVariable);
                    }

                }
                i++;
            }

        }

        private static void guardarRecibo(Recibos.TOT_LIQ_PER recibo, Hashtable grupoVariable)
        {
            foreach (string grVar in grupoVariable.Values)
            {
                Recibos.VAR_GR_CONC grConc = new Recibos.VAR_GR_CONC(new NomadXML(grVar).FirstChild());

                if (grConc.IsForInsert)
                    recibo.VAR_GR_CONC.Add(grConc);
            }
            grupoVariable.Clear();
            NomadEnvironment.GetCurrentTransaction().Save(recibo);
        }
    }
}



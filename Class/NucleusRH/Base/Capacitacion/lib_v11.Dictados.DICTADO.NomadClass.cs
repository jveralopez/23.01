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

namespace NucleusRH.Base.Capacitacion.Dictados
{
    public partial class DICTADO : Nomad.NSystem.Base.NomadObject
    {
        public void Heredar_Curso()
        {
            NucleusRH.Base.Capacitacion.Cursos.CURSO curso = this.Getoi_curso();

            // Asigno los valores del Curso al Dictado
            this.d_dictado = curso.d_curso;
            this.n_duracion_hs = curso.n_duracion_hs;
            this.o_objetivos = curso.o_objetivos;
            this.o_dictado = curso.o_curso;

            // Asigno Costos del curso en Costos del Dictado
            foreach (NucleusRH.Base.Capacitacion.Cursos.COSTO_CURSO costo_curso in curso.COSTOS_CURSO)
            {
                // Creo un Costo de Dictado
                NucleusRH.Base.Capacitacion.Dictados.COSTO_DICTADO costo_dictado;
                costo_dictado = new NucleusRH.Base.Capacitacion.Dictados.COSTO_DICTADO();
                // Asigno los valores del Costo del Curso
                costo_dictado.oi_item_costo = costo_curso.oi_item_costo;
                costo_dictado.n_cantidad = costo_curso.n_cantidad;
                costo_dictado.n_costo_uni = costo_curso.n_costo_uni;
                costo_dictado.n_costo_total = costo_curso.n_costo_total;
                // Agrego el costo a la coleccion
                this.CTOS_DICTADO.Add(costo_dictado);
            }

            // Asigno Temario del curso en Temario del Dictado
            foreach (NucleusRH.Base.Capacitacion.Cursos.TEMA_CURSO tema_curso in curso.TEMAS_CURSO)
            {
                // Creo un Temario de Dictado
                NucleusRH.Base.Capacitacion.Dictados.TEMA_DICTADO tema_dictado;
                tema_dictado = new NucleusRH.Base.Capacitacion.Dictados.TEMA_DICTADO();
                // Asigno los valores del Temario del Curso
                tema_dictado.c_tema_dictado = tema_curso.c_tema_curso;
                tema_dictado.d_tema_dictado = tema_curso.d_tema_curso;
                tema_dictado.o_tema_dictado = tema_curso.o_tema_curso;
                // Agrego el Temario a la coleccion
                this.TEMAS_DICT.Add(tema_dictado);
            }

            // Asigno Destinatarios del curso en Destinatarios del Dictado
            foreach (NucleusRH.Base.Capacitacion.Cursos.DEST_CURSO dest_curso in curso.DEST_CURSO)
            {
                // Creo un Destinatario de Dictado
                NucleusRH.Base.Capacitacion.Dictados.DEST_DICTADO dest_dictado;
                dest_dictado = new NucleusRH.Base.Capacitacion.Dictados.DEST_DICTADO();
                // Asigno los valores del Destinatario del Curso
                dest_dictado.oi_estructura = dest_curso.oi_estructura;
                // Agrego el Destinatario a la coleccion
                this.DEST_DICTADO.Add(dest_dictado);
            }

            // Asigno Docentes del curso en Docentes del Dictado
            foreach (NucleusRH.Base.Capacitacion.Cursos.DOCENTE_CURSO doc_curso in curso.DOC_CURSO)
            {
                // Creo un Docente de Dictado
                NucleusRH.Base.Capacitacion.Dictados.DOCENTE_CURSO doc_dictado;
                doc_dictado = new NucleusRH.Base.Capacitacion.Dictados.DOCENTE_CURSO();
                // Asigno los valores del Docente del Curso
                doc_dictado.oi_docente = doc_curso.oi_docente;
                // Agrego el Docente a la coleccion
                this.DOC_DICTADO.Add(doc_dictado);
            }

            // Asigno Recursos del curso en Recursos del Dictado
            foreach (NucleusRH.Base.Capacitacion.Cursos.REC_CURSO rec_curso in curso.REC_CURSO)
            {
                // Creo un Recurso de Dictado
                NucleusRH.Base.Capacitacion.Dictados.REC_DICTADO rec_dictado;
                rec_dictado = new NucleusRH.Base.Capacitacion.Dictados.REC_DICTADO();
                // Asigno los valores del Recurso del Curso
                rec_dictado.oi_recurso = rec_curso.oi_recurso;
                // Agrego el Recurso a la coleccion
                this.REC_DICTADO.Add(rec_dictado);
            }
        }

        public static void Inscripcion(Nomad.NSystem.Proxy.NomadXML inscriptos, string oi_dictado, bool tercero)
        {
            NomadEnvironment.GetBatch().Trace.Add("IFO", "Comienza el proceso", "Inscripcion a Dictado");
            //CARGO EL DICTADO
            NucleusRH.Base.Capacitacion.Dictados.DICTADO ddoDICTADO = NucleusRH.Base.Capacitacion.Dictados.DICTADO.Get(oi_dictado);
            NomadLog.Debug("ddoDICTADO -- " + ddoDICTADO.SerializeAll());
            //ARMO UN "HASHTABLE" CON TODAS LAS PERSONAS QUE ESTAN EN EL PROCESAMIENTO
            string trc = tercero ? "1" : "0";
            Hashtable ht = new Hashtable();
            ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.Capacitacion.Dictados.DICTADO.Resources.QRY_INSCRIPTOS, "<DATO terceros=\"" + trc + "\" oi_dictado=\"" + oi_dictado + "\"/>", "id");

            //RECORRO LA HASH Y PREGUNTO SI EN ELLA HAY IDS QUE NO ESTAN EN LOS ID QUE ENTRAN
            //DE SER ASI, ESTOS IDS HAY QUE ELIMINARLOS
            NomadXML xmldoc = inscriptos.FirstChild();
            foreach (string value in ht.Keys)
            {
                if (xmldoc.FindElement2("ROW", "id", value) == null)
                {
                    try
                    {
                        NomadEnvironment.GetBatch().Trace.Add("IFO", "Eliminando registro " + ((NomadXML)ht[value]).GetAttr("descr") + " del dictado", "Inscripcion a Dictado");
                        NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO inscripto;
                        if (tercero)
                            inscripto = (INSCRIPTO)ddoDICTADO.INSCRIPTOS.GetByAttribute("oi_tercero", ((NomadXML)ht[value]).GetAttr("id"));
                        else
                            inscripto = (INSCRIPTO)ddoDICTADO.INSCRIPTOS.GetByAttribute("oi_personal_emp", ((NomadXML)ht[value]).GetAttr("id"));

                        //SE DEBE VALIDAR QUE EL INSCRIPTO NO ESTE COMO ASISTENTE EN ALGUNA CLASE DEL DICTADO
                        bool asistencia = false;

                        //RECORRO LAS CLASES
                        foreach (NucleusRH.Base.Capacitacion.Dictados.CLASE_DICTADO ddoCLASE in ddoDICTADO.CLASES_DICT)
                        {
                            //RECORRO LOS ASISTENTES
                            foreach (NucleusRH.Base.Capacitacion.Dictados.ASIST_CLASE ddoASIST in ddoCLASE.ASIST_CLASE)
                            {
                                if (ddoASIST.oi_inscripto == inscripto.Id)
                                    asistencia = true;
                            }
                        }

                        if (!asistencia)
                            ddoDICTADO.INSCRIPTOS.Remove(inscripto);
                        else
                            NomadEnvironment.GetBatch().Trace.Add("ERR", "No se puede eliminar del dictado al registro " + ((NomadXML)ht[value]).GetAttr("descr") + " dado que registra asistencia a clases", "Inscripcion a Dictado");
                    }
                    catch (Exception e)
                    {
                        NomadEnvironment.GetBatch().Trace.Add("ERR", "Error Eliminando registro " + ((NomadXML)ht[value]).GetAttr("descr") + " del dictado " + e.Message, "Inscripcion a Dictado");
                    }
                }
            }

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (NomadXML xmlCur = xmldoc.FirstChild(); xmlCur != null; xmlCur = xmlCur.Next())
            {
                if (!ht.ContainsKey(xmlCur.GetAttr("id")))
                {
                    try
                    {
                        NomadEnvironment.GetBatch().Trace.Add("IFO", "Inscribiendo registro " + xmlCur.GetAttr("descr") + " al dictado", "Inscripcion a Dictado");
                        NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO inscripto;
                        inscripto = new NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO();
                        if (tercero)
                            inscripto.oi_tercero = xmlCur.GetAttr("id");
                        else
                            inscripto.oi_personal_emp = xmlCur.GetAttr("id");
                        inscripto.c_estado = "INS";
                        inscripto.f_estado = DateTime.Today;
                        inscripto.c_estado_aprob = "NE";
                        //RECORRO LAS EVALUACIONES DE TIPO ASISTENTE DEFINIDAS EN EL DICTADO Y SE LAS AGINO AL INSCRIPTO
                        foreach (NucleusRH.Base.Capacitacion.Dictados.EVAL_AS_DICT ddoEvaAs in ddoDICTADO.EVAL_AS_DICT)
                        {
                            NucleusRH.Base.Capacitacion.Dictados.EVAL_ASIST ddoInsEva;
                            ddoInsEva = new NucleusRH.Base.Capacitacion.Dictados.EVAL_ASIST();
                            ddoInsEva.oi_evaluacion = ddoEvaAs.oi_evaluacion;
                            ddoInsEva.f_eval_asist = ddoEvaAs.f_eval_as_dict;
                            //CARGO LA EVALUACION QUE ESTOY ASIGNANDO PARA LUEGO RECORRER SUS FACTORES
                            NucleusRH.Base.Capacitacion.Evaluaciones.EVALUACION ddoEva = NucleusRH.Base.Capacitacion.Evaluaciones.EVALUACION.Get(ddoInsEva.oi_evaluacion);

                            //RECORRO LOS FACTORES DEFINIDOS EN LA EVALUACIONES Y SE LAS AGINO AL INSCRIPTO
                            foreach (NucleusRH.Base.Capacitacion.Evaluaciones.FACTOR_EVAL ddoFac in ddoEva.FACTORES_EVAL)
                            {
                                NucleusRH.Base.Capacitacion.Dictados.FACT_EV_AS ddoInsFacEvAs;
                                ddoInsFacEvAs = new NucleusRH.Base.Capacitacion.Dictados.FACT_EV_AS();
                                ddoInsFacEvAs.oi_factor_eval = ddoFac.Id;
                                ddoInsEva.FACT_EV_AS.Add(ddoInsFacEvAs);
                            }
                            inscripto.EVAL_ASIST.Add(ddoInsEva);
                        }
                        //AGREGO EL INSRIPTO AL DICTADO
                        ddoDICTADO.INSCRIPTOS.Add(inscripto);
                    }
                    catch (Exception e)
                    {
                        NomadEnvironment.GetBatch().Trace.Add("ERR", "Error Inscribiendo registro " + xmlCur.GetAttr("desrc") + " al dictado " + e.Message, "Inscripcion a Dictado");
                    }
                }
            }

            // Guardo el dictado
            NomadEnvironment.GetCurrentTransaction().Save(ddoDICTADO);
        }

        public void Porcentaje_Asistencia(ref Nomad.NSystem.Document.NmdXmlDocument xml_param)
        {
            int cant_clases = this.CLASES_DICT.Count;

            NomadXML aux_xml = new NomadXML();
            aux_xml.SetText(xml_param.ToString());
            NomadXML nmd_param = aux_xml.FirstChild();

            nmd_param.SetAttr("d_dictado", this.d_dictado);
            nmd_param.SetAttr("n_duracion_hs", this.n_duracion_hs.ToString());
            nmd_param.SetAttr("f_inicio", this.f_inicio.ToString("dd/MM/yyyy"));
            nmd_param.SetAttr("f_fin", this.f_fin.ToString("dd/MM/yyyy"));
            nmd_param.SetAttr("f_hoy", DateTime.Today.ToString("dddd dd, MMMM, yyyy"));
            NomadXML inscriptos = nmd_param.FirstChild();
            NomadXML inscripto;
            // Para cada inscripto
            for (inscripto = inscriptos.FirstChild(); inscripto != null; inscripto = inscripto.Next())
            {
                int cont = 0;
                foreach (NucleusRH.Base.Capacitacion.Dictados.CLASE_DICTADO clase in this.CLASES_DICT)
                {
                    ASIST_CLASE asistencia = (ASIST_CLASE)clase.ASIST_CLASE.GetByAttribute("oi_inscripto", inscripto.GetAttr("oi_inscripto"));
                    if (asistencia != null)
                        cont++;
                }

                if (inscripto.GetAttr("oi_personal_emp") != "")
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP personal = (NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP)NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(inscripto.GetAttr("oi_personal_emp"));
                    inscripto.SetAttr("d_ape_y_nom", personal.Getoi_personal().d_ape_y_nom);
                    inscripto.SetAttr("c_dni", personal.Getoi_personal().c_nro_documento);
                    inscripto.SetAttr("inscripto", personal.Getoi_personal().c_personal + " - " + personal.Getoi_personal().d_ape_y_nom);
                }
                else
                {
                    NucleusRH.Base.Capacitacion.Terceros.TERCERO tercero = NucleusRH.Base.Capacitacion.Terceros.TERCERO.Get(inscripto.GetAttr("oi_tercero"));
                    inscripto.SetAttr("d_ape_y_nom", tercero.d_ape_y_nom);
                    inscripto.SetAttr("c_dni", tercero.c_documento);
                    inscripto.SetAttr("inscripto", tercero.c_tercero + " - " + tercero.d_ape_y_nom);
                }
                inscripto.SetAttr("asistencias", cont.ToString());
                inscripto.SetAttr("clases", cant_clases.ToString());
                if (cant_clases > 0)
                    inscripto.SetAttr("porcentaje_asist", (cont * 100 / cant_clases).ToString());
                else
                    inscripto.SetAttr("porcentaje_asist", "0");
            }

            xml_param.ReloadXml(nmd_param.ToString());
        }

        public void FinalizarDictado()
        {
            try
            {
                NomadEnvironment.GetCurrentTransaction().Begin();
                NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = null;

                //RECORRO TODOS LOS INSCRIPTOS DEL DICTADO Y PARA AQUELLOS QUE NO ESTEN CANCELADOS LOS ESTABLEZCO EN CURSADO
                foreach (NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO ddoINS in this.INSCRIPTOS)
                {
                    // Si se evalua, verifico que este evaluado
                    if (this.l_evalua)
                        if (ddoINS.c_estado_aprob == "NE" && ddoINS.c_estado != "CAN")
                            throw new NomadAppException("No se puede Finalizar el dictado porque hay inscriptos sin evaluar");

                    // Si controla asistencia, verifico que cumpla la asistencia minima
                    if (this.l_controlar_asist && this.e_min_asistencia > 0)
                        if (ddoINS.n_asistenciaNull)
                            throw new NomadAppException("El dictado controla la asistencia mínima, verifique que todos los inscriptos tengan cargado el porcentaje de asistencia");
                    if (ddoINS.n_asistencia < this.e_min_asistencia)
                    {
                        ddoINS.c_estado = "CAN";
                        if (!ddoINS.o_inscriptoNull && ddoINS.o_inscripto != "")
                            ddoINS.o_inscripto += "\nNo cumplió con la asistencia mínima";
                        else
                            ddoINS.o_inscripto += "No cumplió con la asistencia mínima";
                    }

                    if (ddoINS.c_estado != "CAN")
                    {
                        ddoINS.c_estado = "CUR";
                    }

                    //SI EL INSCRIPTO ES UN TERCERO NO ANALIZO AGREGAR CURSO Y COMPETENCIAS
                    if (ddoINS.oi_personal_empNull)
                        continue;

                    //TIRO UN QRY PARA TRAER EL ID DE LA PERSONA CORRESPONDIENTE AL INSCRIPTO
                    string param = "<DATO oi_personal_emp=\"" + ddoINS.oi_personal_emp + "\"/>";
                    Nomad.NSystem.Document.NmdXmlDocument docPERID = null;
                    docPERID = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Capacitacion.Dictados.DICTADO.Resources.QRY_PERSONA, param));
                    //NomadEnvironment.GetTrace().Info("docPERID -- " + docPERID.ToString());

                    //CARGO LA PERSONA
                    //NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER;
                    ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(docPERID.GetAttribute("id").Value, false);
                    //NomadEnvironment.GetTrace().Info("ddoPER 1 -- " + ddoPER.SerializeAll());

                    //CARGO EL CURSO
                    NucleusRH.Base.Capacitacion.Cursos.CURSO ddoCUR;
                    ddoCUR = NucleusRH.Base.Capacitacion.Cursos.CURSO.Get(this.oi_curso, false);

                    //SI CORRESPONDE LE AGREGO A CADA PERSONA CORRESPONDIENTE AL LEGAJO EL CURSO CORRESPONDIENTE AL DICTADO
                    if (this.l_curso && (ddoINS.c_estado_aprob == "A" || !this.l_evalua) && (!this.l_controlar_asist || ddoINS.n_asistencia >= this.e_min_asistencia))
                    {
                        bool blCurso = false;

                        //RECORRO LOS CURSOS DE LA PERSONA Y ME FIJO SI YA POSEE EL CURSO CORRESPONDIENTE AL DICTADO
                        foreach (NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER ddoCURPER in ddoPER.CURSOS_PER)
                        {
                            if (ddoCURPER.oi_curso == ddoCUR.oi_area && ddoCURPER.d_curso_ext_per == ddoCUR.d_curso)
                            {
                                blCurso = true;
                            }
                        }
                        NomadEnvironment.GetTrace().Info("blCurso -- " + blCurso.ToString());

                        //EN CASO DE QUE LA VARIABLE SEA FALSE, LA PERSONA NO TIENE CARGADO EL CURSO CON EL TITULO, ENTONCES SE CARGA
                        if (!blCurso)
                        {
                            NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER ddoNEWCUR;
                            ddoNEWCUR = new NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER();
                            ddoNEWCUR.d_curso_ext_per = ddoCUR.d_curso;
                            if (!this.oi_lugar_dictadoNull)
                            {
                                //CARGO EL LUGAR
                                NucleusRH.Base.Capacitacion.LugaresDictado.LUGAR_DICTADO ddoLUGAR;
                                ddoLUGAR = NucleusRH.Base.Capacitacion.LugaresDictado.LUGAR_DICTADO.Get(this.oi_lugar_dictado);
                                ddoNEWCUR.d_lugar = ddoLUGAR.d_lugar_dictado;
                            }
                            ddoNEWCUR.n_duracion = this.n_duracion_hs;
                            ddoNEWCUR.f_fin_curso = this.f_fin;
                            ddoNEWCUR.l_externo = this.l_externo;
                            //ddoNEWCUR.oi_curso_cyd = int.Parse(this.oi_curso);
                            ddoNEWCUR.oi_curso = ddoCUR.oi_area;
                            ddoNEWCUR.oi_unidad_tiempo = "7";

                            NucleusRH.Base.Personal.Legajo.CURSO_CYD_PER ddoCCPA = new NucleusRH.Base.Personal.Legajo.CURSO_CYD_PER();
                            ddoCCPA.oi_curso_cyd = int.Parse(this.oi_curso);
                            ddoNEWCUR.CURSOS_CYD.Add(ddoCCPA);

                            foreach (NucleusRH.Base.Capacitacion.Dictados.CURSO_DICTADO ddoCD in this.CURSOS_DICTADO)
                            {
                                NucleusRH.Base.Personal.Legajo.CURSO_CYD_PER ddoCCP = new NucleusRH.Base.Personal.Legajo.CURSO_CYD_PER();
                                ddoCCP.oi_curso_cyd = int.Parse(ddoCD.oi_curso);
                                ddoNEWCUR.CURSOS_CYD.Add(ddoCCP);
                            }

                            ddoPER.CURSOS_PER.Add(ddoNEWCUR);
                        }
                    }

                    //SI CORRESPONDE LE AGREGO A CADA PERSONA CORRESPONDIENTE AL LEGAJO LAS COMPETENCIAS DEL DICTADO
                    if (this.l_competencia && (ddoINS.c_estado_aprob == "A" || !this.l_evalua) && (!this.l_controlar_asist || ddoINS.n_asistencia >= this.e_min_asistencia))
                    {
                        //PARA CADA COMPETENCIA DEL CURSO
                        foreach (NucleusRH.Base.Capacitacion.Cursos.COMP_CURSO ddoCOMPCURSO in ddoCUR.COMP_CURSO)
                        {
                            string nivel_curso = null;
                            if (ddoCOMPCURSO.oi_nivel_compNull || ddoCOMPCURSO.oi_nivel_comp == "")
                            {
                                nivel_curso = "5";
                            }
                            else
                            {
                                nivel_curso = ddoCOMPCURSO.oi_nivel_comp;
                            }
                            //NomadEnvironment.GetTrace().Info("Comp curso -- " + ddoCOMPCURSO.oi_competencia + " Nivel -- " + nivel_curso);
                            //RECORRO LAS COMPETENCIAS DE LA PERSONA Y ME FIJO SI YA POSEE LA COMPETENCIA CORRESPONDIENTE AL DICTADO CON UN NIVEL INFERIOR
                            bool blCompetencia = false;
                            foreach (NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER ddoCOMPPER in ddoPER.COMPET_PER)
                            {
                                string nivel_per = null;
                                if (ddoCOMPPER.oi_nivel_compNull || ddoCOMPPER.oi_nivel_comp == "")
                                {
                                    nivel_per = "5";
                                }
                                else
                                {
                                    nivel_per = ddoCOMPPER.oi_nivel_comp;
                                }
                                //NomadEnvironment.GetTrace().Info("Comp persona -- " + ddoCOMPPER.oi_competencia + " Nivel -- " + nivel_per);
                                if (ddoCOMPPER.oi_competencia == ddoCOMPCURSO.oi_competencia)
                                {
                                    blCompetencia = true;
                                    if (int.Parse(nivel_curso) < int.Parse(nivel_per))
                                    {
                                        //EN ESTE CASO LA PERSONA TIENE LA COMPETENCIA CON UN NIVEL INFERIOR CON LO CUAL SE LE ACTUALIZA EL NIVEL
                                        ddoCOMPPER.oi_nivel_comp = nivel_curso;
                                    }
                                }
                            }
                            NomadEnvironment.GetTrace().Info("blCompetencia -- " + blCompetencia.ToString());

                            //EN CASO DE QUE LA VARIABLE SEA FALSE, LA PERSONA NO TIENE LA COMPETENCIA, ENTONCES SE CARGA
                            if (!blCompetencia)
                            {
                                NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER ddoNEWCOMP;
                                ddoNEWCOMP = new NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER();
                                ddoNEWCOMP.oi_nivel_comp = ddoCOMPCURSO.oi_nivel_comp;
                                ddoNEWCOMP.oi_competencia = ddoCOMPCURSO.oi_competencia;
                                ddoPER.COMPET_PER.Add(ddoNEWCOMP);
                            }
                        }
                    }
                    //GUARDO LA PERSONA
                    NomadEnvironment.GetTrace().Info("ddoPER 2-- " + ddoPER.SerializeAll());
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPER);

                }

                this.c_estado = "F";
                this.f_estado = DateTime.Today.Date;

                //GUARDO EL DICTADO
                NomadEnvironment.GetCurrentTransaction().Save(this);
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error al finalizar Dictado. " + e.Message);
            }
        }

        public void Calcular_Asistencia()
        {
            int cant_clases = this.CLASES_DICT.Count;

            if (cant_clases <= 0)
                throw new NomadAppException("No se puede calcular las asistencias porque el dictado no tiene ninguna clase cargada");

            try
            {
                foreach (NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO inscripto in this.INSCRIPTOS)
                {
                    int cont = 0;
                    foreach (NucleusRH.Base.Capacitacion.Dictados.CLASE_DICTADO clase in this.CLASES_DICT)
                    {
                        ASIST_CLASE asistencia = (ASIST_CLASE)clase.ASIST_CLASE.GetByAttribute("oi_inscripto", inscripto.Id);
                        if (asistencia != null)
                            cont++;
                    }
                    inscripto.n_asistencia = cont * 100 / cant_clases;
                }

                NomadEnvironment.GetCurrentTransaction().Save(this);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error calculando las asistencias - " + e.Message);
            }
        }

        public void Eliminar()
        {
            foreach (NucleusRH.Base.Capacitacion.Dictados.CLASE_DICTADO ddoCLASE in this.CLASES_DICT)
            {
                ddoCLASE.ASIST_CLASE.Clear();
            }

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
            NomadEnvironment.GetCurrentTransaction().Delete(this);
        }
////////////////////////////////////////////////////////////////////////////////////////////////

 public static List<SortedList<string, object>> GetDictadosFiltro(string f_desde, string f_hasta, string curso)
        {
            NomadLog.Debug("-------------------------------------------------");
            NomadLog.Debug("-----------GET DICTADOS CON FILTRO---------------");
            NomadLog.Debug("-------------------------------------------------");

            NomadLog.Debug("GetDictados.f_desde: " + f_desde);
            NomadLog.Debug("GetDictados.f_hasta: " + f_hasta);
            NomadLog.Debug("GetDictados.CURSO: " + curso);

            List<SortedList<string, object>> retorno = new List<SortedList<string, object>>();
            SortedList<string, object> row;
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            int linea;
            NomadXML MyROW;

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("F_DESDE", f_desde);
            param.SetAttr("F_HASTA", f_hasta);
            param.SetAttr("CCURSO", curso);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Capacitacion.Dictados.DICTADO.GET_DICTADOS_FILTRO", param.ToString());
            NomadLog.Debug(resultado.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontraron Dictados para los filtros definidos." : "Dictados encontrados " + resultado.ChildLength + "."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                //resultado = resultado.FirstChild();
                for (linea = 1, MyROW = resultado.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
                {
                    row = new SortedList<string, object>();

                    for (int r = 0; r < MyROW.Attrs.Count; r++)
                    {
                        //Busco de que tipo es el atributo
                        foreach (KeyValuePair<string, string> kvp in types)
                        {
                            if (kvp.Key == MyROW.Attrs[r].ToString())
                            {
                                type = kvp.Value;
                                break;
                            }
                        }

                        //Agrego el atributo en base a su tipo
                        switch (type)
                        {
                            case "string":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                                break;
                            case "int":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrInt(MyROW.Attrs[r].ToString()));
                                break;
                            case "datetime":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDateTime(MyROW.Attrs[r].ToString()));
                                break;
                            case "double":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDouble(MyROW.Attrs[r].ToString()));
                                break;
                            case "bool":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrBool(MyROW.Attrs[r].ToString()));
                                break;
                            default:
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                                break;
                        }
                        type = "";
                    }

                    //Agrego la Dictado a la lista de resultados
                    retorno.Add(row);
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

////////////////////////////////////////////////////////////////////////////////////////////////
//Retorna el dictado de oi_dictado pasado por parámetro para el WF
////////////////////////////////////////////////////////////////////////////////////////////////
   public static SortedList<string, object> GetDictadoPorOI(string oi_dictado)
     {
            NomadLog.Debug("-------------------------------------------------");
            NomadLog.Debug("-----------GET DICTADO---------------------------");
            NomadLog.Debug("-------------------------------------------------");

            NomadLog.Debug("GetDictado.oi_dictado: " + oi_dictado);

            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("DIC", oi_dictado);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Capacitacion.Dictados.DICTADO.DATOS_DICTADO", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró el dictado con id: " + oi_dictado + "." : "Dictado encontrado."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                resultado = resultado.FirstChild();

                for (int r = 0; r < resultado.Attrs.Count; r++)
                {
                    //Busco de que tipo es el atributo
                    foreach (KeyValuePair<string, string> kvp in types)
                    {
                        if (kvp.Key == resultado.Attrs[r].ToString())
                        {
                            type = kvp.Value;
                            break;
                        }
                    }

                    //Agrego el atributo en base a su tipo
                    switch (type)
                    {
                        case "string":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                        case "int":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                            break;
                        case "datetime":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                            break;
                        case "double":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                            break;
                        case "bool":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                            break;
                        default:
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                    }
                    type = "";
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

/////////////////////////////////////////////////////////////////////////////////////////////////
// Método para inscribir el legajo desde workflow de inscripción
/////////////////////////////////////////////////////////////////////////////////////////////////
    public static string InscripcionLegajo(string PER, string DIC)
    {
      //CARGO EL DICTADO
      NucleusRH.Base.Capacitacion.Dictados.DICTADO ddoDICTADO = NucleusRH.Base.Capacitacion.Dictados.DICTADO.Get(DIC);
      NomadLog.Debug("ddoDICTADO -- " + ddoDICTADO.SerializeAll());

      try
      {
        if(!EsInscripto(PER,DIC))
        {
          NomadEnvironment.GetBatch().Trace.Add("IFO", "Inscribiendo registro al dictado", "Inscripcion a Dictado");
          NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO inscripto;
          inscripto = new NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO();

          //SETEO LOS DATOS DEL INSCRIPTO
          inscripto.oi_personal_emp = PER;
          inscripto.c_estado = "INS";
          inscripto.f_estado = DateTime.Today;
          inscripto.c_estado_aprob = "NE";

          //RECORRO LAS EVALUACIONES DE TIPO ASISTENTE DEFINIDAS EN EL DICTADO Y SE LAS AGINO AL INSCRIPTO
          foreach (NucleusRH.Base.Capacitacion.Dictados.EVAL_AS_DICT ddoEvaAs in ddoDICTADO.EVAL_AS_DICT)
          {
            NucleusRH.Base.Capacitacion.Dictados.EVAL_ASIST ddoInsEva;
            ddoInsEva = new NucleusRH.Base.Capacitacion.Dictados.EVAL_ASIST();
            ddoInsEva.oi_evaluacion = ddoEvaAs.oi_evaluacion;
            ddoInsEva.f_eval_asist = ddoEvaAs.f_eval_as_dict;

            //CARGO LA EVALUACION QUE ESTOY ASIGNANDO PARA LUEGO RECORRER SUS FACTORES
            NucleusRH.Base.Capacitacion.Evaluaciones.EVALUACION ddoEva = NucleusRH.Base.Capacitacion.Evaluaciones.EVALUACION.Get(ddoInsEva.oi_evaluacion);

            //RECORRO LOS FACTORES DEFINIDOS EN LA EVALUACIONES Y SE LAS AGINO AL INSCRIPTO
            foreach (NucleusRH.Base.Capacitacion.Evaluaciones.FACTOR_EVAL ddoFac in ddoEva.FACTORES_EVAL)
            {
              NucleusRH.Base.Capacitacion.Dictados.FACT_EV_AS ddoInsFacEvAs;
              ddoInsFacEvAs = new NucleusRH.Base.Capacitacion.Dictados.FACT_EV_AS();
              ddoInsFacEvAs.oi_factor_eval = ddoFac.Id;
              ddoInsEva.FACT_EV_AS.Add(ddoInsFacEvAs);
            }

            inscripto.EVAL_ASIST.Add(ddoInsEva);
          }

          //AGREGO EL INSRIPTO AL DICTADO
          ddoDICTADO.INSCRIPTOS.Add(inscripto);

          // Guardo el dictado
          NomadEnvironment.GetCurrentTransaction().Save(ddoDICTADO);
          return "1";
        }
        else
        {
          NomadEnvironment.GetBatch().Trace.Add("ERR", "No se puede inscribir el oi_personal_emp " + PER + " dado que ya se encuentra inscripto", "Inscripcion a Dictado");
          return "2";
        }
      }
      catch(Exception ex)
      {
        NomadEnvironment.GetBatch().Trace.Add("ERR", "Error Inscribiendo registro " + PER + " al dictado " + ex.Message, "Inscripcion a Dictado");
        return "0";
      }
    }

// Método utilizado en la inscripción desde WF
    public static bool EsInscripto(string PER,string oi_dictado)
    {
      //CARGO EL DICTADO
      NucleusRH.Base.Capacitacion.Dictados.DICTADO ddoDICTADO = NucleusRH.Base.Capacitacion.Dictados.DICTADO.Get(oi_dictado);
      NomadLog.Debug("ddoDICTADO -- " + ddoDICTADO.SerializeAll());

      bool inscripto = false;

      foreach(NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO ddoINS in ddoDICTADO.INSCRIPTOS)
      {
        if(ddoINS.oi_personal_emp == PER)
          inscripto = true;
      }

      return inscripto;
    }

/////////////////////////////////////////////////////////////////////////////////////////
//Método para borrar un inscripto de un dictado
/////////////////////////////////////////////////////////////////////////////////////////
    public static int QuitarInscripto(string PAR, string DIC)
    {
        NomadEnvironment.GetBatch().Trace.Add("IFO", "Comienza el proceso", "Quitar Inscripto");
        //CARGO EL DICTADO
        NucleusRH.Base.Capacitacion.Dictados.DICTADO ddoDICTADO = NucleusRH.Base.Capacitacion.Dictados.DICTADO.Get(DIC);
        NomadLog.Debug("PAR " +PAR);
        NomadLog.Debug("DIC " +DIC);

        if(EsInscripto(PAR,DIC))
        {
            try
                {
                    NomadEnvironment.GetBatch().Trace.Add("IFO", "Eliminando registro " + PAR + " del dictado", "Inscripcion a Dictado");
                    NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO inscripto;

                    inscripto = (INSCRIPTO)ddoDICTADO.INSCRIPTOS.GetByAttribute("oi_personal_emp", PAR);

                    NomadLog.Debug("PERSONAL INSCRIPTO " +inscripto.oi_personal_emp);
                    //SE DEBE VALIDAR QUE EL INSCRIPTO NO ESTE COMO ASISTENTE EN ALGUNA CLASE DEL DICTADO
                    bool asistencia = false;

                    //RECORRO LAS CLASES
                    foreach (NucleusRH.Base.Capacitacion.Dictados.CLASE_DICTADO ddoCLASE in ddoDICTADO.CLASES_DICT)
                    {
                        //RECORRO LOS ASISTENTES
                        foreach (NucleusRH.Base.Capacitacion.Dictados.ASIST_CLASE ddoASIST in ddoCLASE.ASIST_CLASE)
                        {
                            if (ddoASIST.oi_inscripto == inscripto.Id)
                                asistencia = true;
                        }
                    }

                    if (!asistencia)
                    {
                      NomadLog.Debug("SIN ASISTENCIA A CLASES ");
                        ddoDICTADO.INSCRIPTOS.Remove(inscripto);
                        NomadEnvironment.GetCurrentTransaction().Save(ddoDICTADO);
                        return 1;
                    }
                    else
                    {
                      NomadLog.Debug("CON ASISTENCIA A CLASES ");
                        NomadEnvironment.GetBatch().Trace.Add("ERR", "No se puede eliminar del dictado al registro " + PAR + " dado que registra asistencia a clases", "Inscripcion a Dictado");
                        return 0;
                    }
                }
                catch (Exception e)
                {
                    NomadEnvironment.GetBatch().Trace.Add("ERR", "Error Eliminando registro " + PAR + " del dictado " + e.Message, "Inscripcion a Dictado");
                    return 0;
                }
            }
            else
            {
                NomadLog.Debug("NO INSCRIPTO ");
            return 0;
          }
        }
//////////////////////////////////////////////////////////////////////////////////////////
//Método para recuperar los dictados en los cuales está inscripto un legajo (PAR)
/////////////////////////////////////////////////////////////////////////////////////////
    public static List<SortedList<string, object>> GetDictadosInscripto(string PAR, int dias_prev)
            {
                NomadLog.Debug("------------------------------------------");
                NomadLog.Debug("-----------GET DICTADOS DEL LEGAJO--------");
                NomadLog.Debug("------------------------------------------");

              NomadLog.Debug("GetLic.PersonalEMP: " + PAR);
                NomadLog.Debug("GetLic.dias_prev: " + dias_prev);

                List<SortedList<string, object>> retorno = new List<SortedList<string, object>>();
                SortedList<string, object> row;
                SortedList<string, string> types = new SortedList<string, string>();
                string type = "";

                int linea;
                NomadXML MyROW;

                NomadXML param = new NomadXML("PARAM");

                //Agrego los parametros
                param.SetAttr("PAR", PAR);
                param.SetAttr("dias_prev", dias_prev);

                //Ejecuto el recurso
                NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Capacitacion.Dictados.DICTADO.GET_DICTADOS", param.ToString());

                NomadLog.Debug((resultado.FirstChild() == null ? "No se encontraron Dictados en los cuales esté inscripto el legajo con ID: " + PAR + "." : "Dictados encontrados " + resultado.ChildLength + "."));

                if (resultado.FirstChild() != null)
                {
                    //Armo una sorted list con los atributos del resultado y sus tipos
                    for (int x = 0; x < resultado.Attrs.Count; x++)
                    {
                        types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                    }

                    //resultado = resultado.FirstChild();
                    for (linea = 1, MyROW = resultado.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
                    {
                        row = new SortedList<string, object>();

                        for (int r = 0; r < MyROW.Attrs.Count; r++)
                        {
                            //Busco de que tipo es el atributo
                            foreach (KeyValuePair<string, string> kvp in types)
                            {
                                if (kvp.Key == MyROW.Attrs[r].ToString())
                                {
                                    type = kvp.Value;
                                    break;
                                }
                            }

                            //Agrego el atributo en base a su tipo
                            switch (type)
                            {
                                case "string":
                                    row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                                    break;
                                case "int":
                                    row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrInt(MyROW.Attrs[r].ToString()));
                                    break;
                                case "datetime":
                                    row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDateTime(MyROW.Attrs[r].ToString()));
                                    break;
                                case "double":
                                    row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDouble(MyROW.Attrs[r].ToString()));
                                    break;
                                case "bool":
                                    row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrBool(MyROW.Attrs[r].ToString()));
                                    break;
                                default:
                                    row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                                    break;
                            }
                            type = "";
                        }

                        //Agrego la Dictado a la lista de resultados
                        retorno.Add(row);
                    }
                }
                NomadLog.Debug("Retorno: " + retorno.ToString());
                return retorno;
            }
    }
}



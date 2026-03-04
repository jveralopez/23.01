using System;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Gestion_de_Postulantes.Requerimientos
{
    public partial class REFERENCIA
    {

    // este metod valida que para los Los Estudios que son excluyendes de la primera lista
    // existan en la segunda lista
    public static bool ValidarEstudios(Nomad.NSystem.Base.NomadObjectList ListaValidar, Nomad.NSystem.Base.NomadObjectList ListaAValidar) {
    int preg = 0;
    foreach(Nomad.NSystem.Base.NomadObject ddoEstudio in ListaValidar)
    {
      preg = 0;
      if ((bool) ddoEstudio.GetAttribute("l_excluyente"))
      {
        foreach(Nomad.NSystem.Base.NomadObject Estudio in ListaAValidar)
        {

         if (ddoEstudio.GetAttribute("oi_estudio").ToString() == Estudio.GetAttribute("oi_estudio").ToString())
         {
           preg = 1;
         }
        }
        if (preg==1)
        {
          continue;
        } else
          return false;
      }

    }
    return true;

    }

    // este metod valida que para los Idioma que son excluyendes de la primera lista
    // existan en la segunda lista
    public static bool ValidarIdiomas(Nomad.NSystem.Base.NomadObjectList ListaValidar, Nomad.NSystem.Base.NomadObjectList ListaAValidar) {
    int preg = 0;
    foreach(Nomad.NSystem.Base.NomadObject ddoIdioma in ListaValidar)
    {
      preg = 0;
      if ((bool) ddoIdioma.GetAttribute("l_excluyente"))
      {
        foreach(Nomad.NSystem.Base.NomadObject idioma in ListaAValidar)
        {

         if (ddoIdioma.GetAttribute("oi_idioma").ToString() == idioma.GetAttribute("oi_idioma").ToString())
         {
           preg = 1;
         }
        }
        if (preg==1)
        {
          continue;
        } else
          return false;
      }

    }
    return true;

    }

    // este metod valida que para los cursos que son excluyendes de la primera lista
    // existan en la segunda lista
    public static bool ValidarCursos(Nomad.NSystem.Base.NomadObjectList ListaValidar, Nomad.NSystem.Base.NomadObjectList ListaAValidar) {
    int preg = 0;
    foreach(Nomad.NSystem.Base.NomadObject ddoCurso in ListaValidar)
    {
      preg = 0;
      if ((bool) ddoCurso.GetAttribute("l_excluyente"))
      {
        foreach(Nomad.NSystem.Base.NomadObject curso in ListaAValidar)
        {
         if (ddoCurso.GetAttribute("oi_curso").ToString() == curso.GetAttribute("oi_curso").ToString())
         {
           preg = 1;
         }
        }
        if (preg==1)
        {
          continue;
        } else
          return false;
      }

    }
    return true;

    }

    public static bool ValidarExperienciaPuestoExterno(NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto, NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPos)
    {
      int preg = 0;
      foreach(NucleusRH.Base.Organizacion.Puestos.PUESTO_EXP ddoPuestoExp in ddoPuesto.PUESTO_EXP)
      {
        preg = 0;
        if (ddoPuestoExp.l_excluyente)
        {
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ANTECEDENTE ddoAntecedente in ddoPos.ANTECEDENTES)
          {
            foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ANTEC_PUESTO ddoAntecPuesto in ddoAntecedente.ANTEC_PUESTO)
             {
               if (!ddoAntecPuesto.oi_puesto_empresaNull)
               {
                  if (ddoAntecPuesto.oi_puesto_empresa == ddoPuestoExp.oi_puesto_req_exp)
                  {
                    preg = 1;
                  }
               }
             }
          }
          if (preg==1)
          {
            continue;
          } else
            return false;
        }

      }
      return true;
    }

    public static bool ValidarExperienciaPuestoInterno(NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto, NucleusRH.Base.Personal.Legajo.PERSONAL ddoPer)
    {
      int preg = 0;
      foreach(NucleusRH.Base.Organizacion.Puestos.PUESTO_EXP ddoPuestoExp in ddoPuesto.PUESTO_EXP)
      {
        preg = 0;
        if (ddoPuestoExp.l_excluyente)
        {
          foreach(NucleusRH.Base.Personal.Legajo.ANTECEDENTE ddoAntecedente in ddoPer.ANTECEDENTES)
          {
            foreach(NucleusRH.Base.Personal.Legajo.ANTEC_PUESTO ddoAntecPuesto in ddoAntecedente.ANTEC_PUESTO)
             {
               if (!ddoAntecPuesto.oi_puesto_empresaNull)
               {
                  if (ddoAntecPuesto.oi_puesto_empresa == ddoPuestoExp.oi_puesto_req_exp)
                  {
                    preg = 1;
                  }
               }
             }
          }
          if (preg==1)
          {
            continue;
          } else
            return false;
        }

      }
      return true;
    }

    public void ReclutamientoInterno(Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        if (this.ca_estado == "I")
        {
            NomadEnvironment.GetBatch().Trace.Add("ifo", "Iniciando Reclutamiento Interno", "Reclutamiento Interno");
            NomadLog.Info("puesto interno::" + this.oi_puesto);
            if (!this.oi_puestoNull)
            {
                if (this.oi_posicionNull)
                {
                    NomadEnvironment.GetBatch().Trace.Add("err", "Hubo algun error en la carga de datos. Una Referencia no puede estar definida para un puesto y una posicion", "Reclutamiento Interno");
                    Console.WriteLine("Hubo algun error en la carga de datos. Una Referencia no puede estar definida para un puesto y una posicion");
                }
                else
                {
                    NomadEnvironment.GetTrace().Info("ENTRO");
                    NucleusRH.Base.Gestion_de_Postulantes.Puestos.PUESTO ddoPuesto;
                    ddoPuesto = this.Getoi_puesto();

                    // en ddoPuesto voy a guardar el puesto de la referencia

                    NomadEnvironment.GetTrace().Info(ddoPuesto.SerializeAll());
                    Console.WriteLine(Filtros.ToString());
                    this.ReclutamientoInternoPuesto(ddoPuesto, Filtros);
                    NomadEnvironment.GetTrace().Info("Salio");
                }
            }
            else
            {
                if (!this.oi_posicionNull)
                {
                    NucleusRH.Base.Gestion_de_Postulantes.Puestos.POSICION ddoPosicion;
                    ddoPosicion = this.Getoi_posicion();
                    // en ddoPosicion voy a guardar la Posicion de la referencia

                    this.ReclutamientoInternoPosicion(ddoPosicion, Filtros);

                }
                else
                {
                    NomadEnvironment.GetBatch().Trace.Add("err", "Hubo algun error en la carga de datos. una referencia no puede estar cargada sin puesto ni posicion", "Reclutamiento Interno");
                    Console.WriteLine("Hubo algun error en la carga de datos. una referencia no puede estar cargada sin puesto ni posicion");
                }
            }
        }
        else
        {
            NomadEnvironment.GetBatch().Trace.Add("err", "La Referencia No esta Inicializada", "Reclutamiento Interno");
            Console.WriteLine("no esta Inicializada");
        }

    }
    public void ReclutamientoInternoPuesto(NucleusRH.Base.Gestion_de_Postulantes.Puestos.PUESTO ddoPuesto, Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        NomadBatch objBatch;
        objBatch = NomadBatch.GetBatch("Iniciando...", "Reclutamiento Interno");

        NomadEnvironment.GetTrace().Info("EntroAqui");
        NomadEnvironment.GetTrace().Info("Filtros = " + Filtros.ToString());

        string ignorarReferencia = Filtros.GetAttribute("ignorarReferencia").Value;
        string ignorarExperiencia = Filtros.GetAttribute("ignorarExperiencia").Value;
        string ignorarIdiomas = Filtros.GetAttribute("ignorarIdiomas").Value;
        string ignorarEstudios = Filtros.GetAttribute("ignorarEstudios").Value;
        string ignorarCursos = Filtros.GetAttribute("ignorarCursos").Value;
        string postulados = Filtros.GetAttribute("postulados").Value;

        double pondInscripcion = Nomad.NSystem.Functions.StringUtil.str2dbl(Filtros.GetAttribute("pondInscripcion").Value);
        double pondPostulacion = Nomad.NSystem.Functions.StringUtil.str2dbl(Filtros.GetAttribute("pondPostulacion").Value);

        int edad = 0;
        bool cumple = false;

        NomadEnvironment.GetTrace().Info("Elimina los rankeados");
        this.EliminarRankeados();

        //Console.WriteLine("Reclutamiento Interno por Puesto");
        string param = "<DATOS oi_referencia=\"" + this.Id + "\"/>";
        NomadEnvironment.GetTrace().Info("PuntoControl1");
        NomadEnvironment.GetTrace().Info("param: " + param.ToString());
        NomadEnvironment.GetTrace().Info("Postulados:" + postulados.ToString());
        Nomad.NSystem.Document.NmdXmlDocument ddoPersonas;
        if (postulados == "")
            ddoPersonas = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.ListarIds, Filtros.ToString()));
        else
            ddoPersonas = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.ListarIdsPost, Filtros.ToString()));

        NomadEnvironment.GetTrace().Info("ddoPersonas: " + ddoPersonas.ToString());
        Nomad.NSystem.Document.NmdXmlDocument Persona;
        NomadEnvironment.GetTrace().Info("PuntoControl2");
        //aqui declaro la fecha de hoy
        DateTime hoy = DateTime.Now;

        int cont = 0;
        int cont2 = 1;

        for (Persona = (Nomad.NSystem.Document.NmdXmlDocument)ddoPersonas.GetFirstChildDocument(); Persona != null; Persona = (Nomad.NSystem.Document.NmdXmlDocument)ddoPersonas.GetNextChildDocument())
        {
            objBatch.SetPro(0, 100, ddoPersonas.ChildDocuments.Count, cont + 1);

            NomadEnvironment.GetTrace().Info("dentroDelFor");
            cont = cont + 1;

            objBatch.SetMess("Rankeando Personas " + cont + " de " + ddoPersonas.ChildDocuments.Count);
            //aqui inicializo las variables
            edad = 0;
            NucleusRH.Base.Gestion_de_Postulantes.Personal_Seleccion.PERSONAL_EMP ddoPerEmp = NucleusRH.Base.Gestion_de_Postulantes.Personal_Seleccion.PERSONAL_EMP.Get(Persona.GetAttribute("OI_PERSONAL_EMP").Value.ToString());
            // en ddoPerEmp  voy guardando todas las personas empresas.
            NucleusRH.Base.Personal.Legajo.PERSONAL ddoPer = ddoPerEmp.Getoi_personal();

            // en ddoPer  voy guardando todas las personas.
            //aqui empiezo las validaciones

            if (ignorarReferencia == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 1 Se fija si la referencia tiene filtros definidos
                // 1.a se fija en el sexo

                if (!this.c_sexo_reqNull)
                {
                    if (!ddoPer.c_sexoNull)
                    {
                        if (this.c_sexo_req != ddoPer.c_sexo)
                        {
                            //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer el sexo requerido");
                            continue;
                        }
                    }
                }

                // 1.b se fija en el estado Civil
                if (!this.oi_estado_civilNull && !ddoPer.oi_estado_civilNull)
                {
                    if (this.oi_estado_civil != ddoPer.oi_estado_civil)
                    {
                        //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer el estado civil requerido");
                        continue;
                    }
                }

                //calculo la edad
                if (!ddoPer.f_nacimNull)
                {
                    edad = NucleusRH.Base.Personal.UtilBase.DiffYears(ddoPer.f_nacim, hoy);
                }

                // 1.c se fija en la edad minima requerida
                if (!this.e_edad_minima_reqNull && (edad > 0))
                {
                    if (this.e_edad_minima_req >= edad)
                    {
                        //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por ser mayor a la edad minima requerida");
                        continue;
                    }
                }

                // 1.d se fija en la edad maxima requerida
                if (!this.e_edad_maxima_reqNull && (edad > 0))
                {
                    if (this.oi_estado_civil != ddoPer.oi_estado_civil)
                    {
                        //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer el estado civil requerido");
                        continue;
                    }
                }
            }

            if (ignorarExperiencia == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 2. verifica las experiencias requeridas para el puesto
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarExperienciaPuestoInterno(ddoPuesto, ddoPer);
                if (!cumple)
                {
                    //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer la experiencia requerida");
                    continue;
                }
            }

            if (ignorarIdiomas == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 3. Verifica los idiomas requeridos para el puesto
                //NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto;
                //NucleusRH.Base.Personal.Legajo.PERSONAL ddoPer;
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarIdiomas(ddoPuesto.PUESTO_IDIO, ddoPer.IDIOMAS_PER);
                if (!cumple)
                {
                    //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer los Idiomas Requeridos");
                    continue;
                }
            }

            if (ignorarEstudios == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 4. Verifica los estudios requeridos para el puesto
                //  NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto;
                //NucleusRH.Base.Personal.Legajo.PERSONAL ddoPer;
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarEstudios(ddoPuesto.PUESTO_ESTUD, ddoPer.ESTUDIOS_PER);
                if (!cumple)
                {
                    //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer los estudios Requeridos");
                    continue;
                }
            }

            if (ignorarCursos == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 5. Verifica los Cursos requeridos para el puesto
                //cumple = this.ValidarCursos(ddoPuesto, ddoPer);
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarCursos(ddoPuesto.PUESTO_CURS, ddoPer.CURSOS_PER);
                if (!cumple)
                {
                    //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer los Cursos Requeridos");
                    continue;
                }
            }

            /*AREAS
            if (ignorarAreas == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 5. Verifica los Cursos requeridos para el puesto
                //cumple = this.ValidarCursos(ddoPuesto, ddoPer);
                NucleusRH.Base.Gestion_de_Postulantes.Postulantes ddPOST = new NucleusRH.Base.Gestion_de_Postulantes.Postulantes();
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarAreas(ddoPuesto.PUESTO_CURS , ddPOST.CURSOS_PER);
                if (!cumple)
                {
                    //Console.WriteLine("La Persona " + ddoPer.c_personal + " - " + ddoPer.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer los Cursos Requeridos");
                    continue;
                }
            }
            */

            // 6. Aqui llegan las personas que pasan todos los filtros.
            this.RankingReclutamientoInternoPuesto(ddoPuesto, ddoPer, Filtros, ddoPerEmp, Persona.GetAttribute("oi_reclutado").Value);

            if (cont == (10 * cont2))
            {
                NomadEnvironment.GetBatch().Trace.Add("ifo", "se han Rankeado " + cont + " personas", "Reclutamiento Interno");
                cont2 = cont2 + 1;
            }
        }

    }
    public void RankingReclutamientoInternoPuesto(NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto, NucleusRH.Base.Personal.Legajo.PERSONAL ddoPer, Nomad.NSystem.Document.NmdXmlDocument Filtros, NucleusRH.Base.Gestion_de_Postulantes.Personal_Seleccion.PERSONAL_EMP ddoPerEmp, string oi_reclutamiento)
    {

        double ranking = 0;
        double pondInscripcion = Nomad.NSystem.Functions.StringUtil.str2dbl(Filtros.GetAttribute("pondInscripcion").Value);
        double pondPostulacion = Nomad.NSystem.Functions.StringUtil.str2dbl(Filtros.GetAttribute("pondPostulacion").Value);
        NomadEnvironment.GetTrace().Info("EntroMetodoInterno");
        if (pondInscripcion == 0)
        { //significa que no se definio una ponderacion por inscripcion por lo tanto tomara un valor por defecto
            pondInscripcion = 0;
        }

        NomadEnvironment.GetTrace().Info("primera validacion");
        if (pondPostulacion == 0)
        {//significa que no se definio una ponderacion por postulacion por lo tanto tomara un valor por defecto
            pondPostulacion = 0;
        }

        NomadEnvironment.GetTrace().Info("seguna validacion");

        // 1. el personal esta inscripto para la referencia?
        foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_PER RefPer in this.REF_PER)
        {
            if (RefPer.oi_personal_emp == ddoPerEmp.Id)
            {
                ranking = ranking + pondInscripcion;
            }
        }
        NomadEnvironment.GetTrace().Info("seguna validacion A");

        // 2. si el puesto es uno de los que el personal se postularia
        foreach (NucleusRH.Base.Gestion_de_Postulantes.Personal_Seleccion.PERS_PUESTO ddoPerPuesto in ddoPerEmp.PERS_PUESTO)
        {
            if (ddoPerPuesto.oi_puesto == ddoPuesto.Id)
            {
                ranking = ranking + pondPostulacion;
            }
        }
        NomadEnvironment.GetTrace().Info("seguna validacion B");

        // 3. para cada experiencia no excluyente del personal
        foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_EXP ddoPueExp in ddoPuesto.PUESTO_EXP)
        {
            if (!ddoPueExp.l_excluyente)
            {
                foreach (NucleusRH.Base.Personal.Legajo.ANTECEDENTE ddoAntec in ddoPer.ANTECEDENTES)
                {
                    foreach (NucleusRH.Base.Personal.Legajo.ANTEC_PUESTO ddoAntecPuesto in ddoAntec.ANTEC_PUESTO)
                    {
                        if (!ddoAntecPuesto.oi_puesto_empresaNull)
                        {
                            if (ddoAntecPuesto.oi_puesto_empresa == ddoPueExp.oi_puesto_req_exp)
                            {
                                if (!ddoPueExp.e_pesoNull)
                                {
                                    ranking = ranking + ddoPueExp.e_peso;
                                }
                            }
                        }
                    }
                }
            }
        }

        NomadEnvironment.GetTrace().Info("seguna validacion C");

        // 4. para cada Idioma no excluyente del personal
        foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_IDIO ddoPueIdio in ddoPuesto.PUESTO_IDIO)
        {
            if (!ddoPueIdio.l_excluyente)
            {
                foreach (NucleusRH.Base.Personal.Legajo.IDIOMA_PER ddoPerIdio in ddoPer.IDIOMAS_PER)
                {
                    if (ddoPerIdio.oi_idioma == ddoPueIdio.oi_idioma)
                    {
                        if (!ddoPueIdio.e_pesoNull)
                        {
                            ranking = ranking + ddoPueIdio.e_peso;
                        }
                    }
                }
            }
        }

        NomadEnvironment.GetTrace().Info("seguna validacion D");

        // 5. para cada Estudio no excluyente del personal
        foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_ESTUD ddoPueEst in ddoPuesto.PUESTO_ESTUD)
        {
            if (!ddoPueEst.l_excluyente)
            {
                foreach (NucleusRH.Base.Personal.Legajo.ESTUDIO_PER ddoPerEst in ddoPer.ESTUDIOS_PER)
                {
                    if (ddoPerEst.oi_estudio == ddoPueEst.oi_estudio)
                    {
                        if (!ddoPueEst.e_pesoNull)
                        {
                            ranking = ranking + ddoPueEst.e_peso;
                        }
                    }
                }
            }
        }

        NomadEnvironment.GetTrace().Info("seguna validacion E");

        // 6. para cada Curso no excluyente del personal
        foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_CURSO ddoPueCur in ddoPuesto.PUESTO_CURS)
        {
            if (!ddoPueCur.l_excluyente)
            {
                foreach (NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER ddoPerCur in ddoPer.CURSOS_PER)
                {
                    if (ddoPerCur.oi_curso == ddoPueCur.oi_curso)
                    {
                        if (!ddoPueCur.e_peso_requisitoNull)
                        {
                            ranking = ranking + ddoPueCur.e_peso_requisito;
                        }
                    }
                }
            }
        }
        NomadEnvironment.GetTrace().Info("seguna validacion F");

        //Console.WriteLine("Reclutamiento Interno por Posicion" + ranking + " -- " + ddoPer.Id);
        this.CrearReclutados(ddoPer.c_personal, "PER", ranking, oi_reclutamiento, ddoPerEmp.Id);

    }
    public void CrearReclutados(string codigo, string opcion, double ranking, string oi_reclutamiento, string idPersona)
    {

        NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO ddoRec;

        NomadEnvironment.GetTrace().Info("CREA RECLUTADO");

        if (oi_reclutamiento == "0")
        {
            ddoRec = new NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO();

            ddoRec.c_reclutado = codigo;
            ddoRec.oi_referencia = this.Id;
            if (opcion == "POS")
            {
                ddoRec.oi_postulante = idPersona;
                NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPOS = NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE.Get(idPersona);
                ddoRec.d_reclutado = ddoPOS.d_postulante;
            }
            else
            {
                if (opcion == "PER")
                {
                    ddoRec.oi_personal_emp = idPersona;
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(idPersona);
                    NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(ddoLEG.oi_personal);
                    ddoRec.d_reclutado = ddoPER.d_ape_y_nom;
                }
            }
            ddoRec.c_estado = "N";
            ddoRec.f_rankeo = DateTime.Now;

        }
        else
        {
            ddoRec = NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Get(oi_reclutamiento);
        }
        ddoRec.n_ranking = ranking;
        //Console.WriteLine("Reclutamiento Interno por Posicion " + ranking +  " - " +  ddoRec.SerializeAll() +" - " + ddoPer.Id);
        NomadEnvironment.GetCurrentTransaction().Save(ddoRec);

    }
    public void ReclutamientoInternoPosicion(NucleusRH.Base.Gestion_de_Postulantes.Puestos.POSICION ddoPosicion, Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        //Console.WriteLine("Reclutamiento Interno por Posicion");

    }
    public static void IniciarReclutamientoInterno(Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {
        NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA ddoRef;
        string oi_referencia = Filtros.GetAttribute("oi_referencia").Value;
        ddoRef = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.Get(oi_referencia);
        NomadLog.Info("ddoRef::: " + ddoRef.ToString());
        ddoRef.ReclutamientoInterno(Filtros);

    }
    public static void IniciarReclutamientoExterno(Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA ddoRef;
        string oi_referencia = Filtros.GetAttribute("oi_referencia").Value;
        ddoRef = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.Get(oi_referencia);
        ddoRef.ReclutamientoExterno(Filtros);

    }
    public static void ReclutarRankeados(Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        //Console.WriteLine("Filtros = " + Filtros.ToString());
        NomadLog.Info("filtros:::: " + Filtros.ToString());
        Nomad.NSystem.Document.NmdXmlDocument todosRec = (Nomad.NSystem.Document.NmdXmlDocument)Filtros.GetDocumentByName("ROWS");
        string elegido;
        string reclutado;
        Nomad.NSystem.Document.NmdXmlDocument cadaRec;

        for (cadaRec = (Nomad.NSystem.Document.NmdXmlDocument)todosRec.GetFirstChildDocument(); cadaRec != null; cadaRec = (Nomad.NSystem.Document.NmdXmlDocument)todosRec.GetNextChildDocument())
        {
            elegido = cadaRec.GetAttribute("elegir").Value;
            if (elegido == "1")
            {
                reclutado = cadaRec.GetAttribute("oi_reclutado").Value;
                NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO ddoRec = NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Get(reclutado);
                ddoRec.c_estado = "R";
                ddoRec.f_reclutamiento = DateTime.Now;

                ddoRec.AsignarExaRec();
                NomadEnvironment.GetCurrentTransaction().Save(ddoRec);
            }
        }

    }
    public static Nomad.NSystem.Document.NmdXmlDocument PosiblesSeleccionPersonal(Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        //Nomad.NSystem.Document.NmdXmlDocument Filtros
        int max = 0;
        string mensaje = "";
        string oi_examen = "";
        string opcion = "";
        string oi_rec = "";
        string oi_ref = Filtros.GetAttribute("oi_referencia").Value;
        string oi_empresa = Filtros.GetAttribute("oi_empresa").Value;
        string oi_req = Filtros.GetAttribute("oi_req").Value;
        NomadEnvironment.GetTrace().Info("oi_ref -- " + oi_ref);
        NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA ddoRef = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.Get(oi_ref);
        NomadEnvironment.GetTrace().Info("ddoRef -- " + ddoRef.SerializeAll());
        //primero se fija si la referencia esta inicializada
        if (ddoRef.ca_estado != "I")
        {
            throw new NomadAppException("La Referencia no esta inicializada");
        }

        //para cada examen definido en la referencia
        foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_EXAMEN ddoRefExa in ddoRef.REF_EXAMEN)
        {
            if (!ddoRefExa.e_ordenNull)  //en la variable max guardo el maximo orden
            {
                if (ddoRefExa.e_orden > max)
                {
                    max = ddoRefExa.e_orden;
                    oi_examen = ddoRefExa.Id; //y tambien guardo el oi del Ref_examen

                }
            }
            else
            {
                throw new NomadAppException("hay un error en la carga de datos pues todos los examenes deben tener un orden definido");
            }
        }
        opcion = "1";

        if (ddoRef.REF_EXAMEN.Count == 0) //si la referencia no tiene definido ningun examen
        //se podran seleccionar todos los reclutados.
        {
            mensaje = mensaje + "La Referencia no tiene cargados examenes.";
            NomadEnvironment.GetTrace().Info("mensaje -- " + mensaje.ToString());
            opcion = "0";
        }

        string param = "<DATOS oi_referencia=\"" + ddoRef.Id + "\" oi_ref_examen=\"" + oi_examen + "\" oi_req=\"" + oi_req + "\" oi_empresa=\"" + oi_empresa + "\" opcion=\"" + opcion + "\"/>";
        NomadEnvironment.GetTrace().Info("parametro -- " + param.ToString());

        Nomad.NSystem.Document.NmdXmlDocument ddoReclutados = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Resources.ListarIds, param));

        NomadEnvironment.GetTrace().Info("ddoReclutados -- " + ddoReclutados.ToString());

        //este qry trae todos los reclutados de una referencia que tengan estado R o trae todos los reclutados de una referencia si la opcion es 0.

        Nomad.NSystem.Document.NmdXmlDocument docRes = new Nomad.NSystem.Document.NmdXmlDocument("<DATOS oi_referencia=\"" + ddoRef.Id + "\" oi_req=\"" + oi_req + "\" oi_empresa=\"" + oi_empresa + "\"/>");
        NomadEnvironment.GetTrace().Info("docRes -- " + docRes.ToString());
        //docRes sera el documento que devuelva el metodo qque contendra todos los reclutados mas mensajes;
        docRes.AddChildDocument("<RECLUTADOS/>");
        Nomad.NSystem.Document.NmdXmlDocument docResRec = (Nomad.NSystem.Document.NmdXmlDocument)docRes.GetDocumentByName("RECLUTADOS");
        // docResRec es para agregar la coleccion

        Nomad.NSystem.Document.NmdXmlDocument ddoRec;
        for (ddoRec = (Nomad.NSystem.Document.NmdXmlDocument)ddoReclutados.GetFirstChildDocument(); ddoRec != null; ddoRec = (Nomad.NSystem.Document.NmdXmlDocument)ddoReclutados.GetNextChildDocument())
        {
            oi_rec = ddoRec.GetAttribute("oi_reclutado").Value;
            NomadEnvironment.GetTrace().Info("oi_rec -- " + oi_rec.ToString());
            if (opcion == "0")
            {//si la referencia no tiene cargado examenes se agregan todos los reclutados.

                docResRec.AddChildDocument("<RECLUTADO oi_reclutado= \"" + oi_rec + "\" elegir=\"0\"/>");

            }
            else
            {
                if (opcion == "1")
                {//si la referencia tiene definido examenes

                    NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO RECLUTADO = NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Get(oi_rec);
                    NucleusRH.Base.Gestion_de_Postulantes.Reclutados.EXA_REC ddoExaRec = (NucleusRH.Base.Gestion_de_Postulantes.Reclutados.EXA_REC)RECLUTADO.EXA_REC.GetByAttribute("oi_ref_examen", oi_examen);
                    docResRec.AddChildDocument("<RECLUTADO oi_reclutado= \"" + oi_rec + "\" elegir=\"0\"/>");
                    /*
                    if (!ddoExaRec.oi_ref_res_exaNull)
                    {//el reclutado rindio el ultimo examen
                        NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_RES_EXA ddoResul = ddoExaRec.Getoi_ref_res_exa();
                        if (!ddoResul.l_apto_prox_examenNull)
                        {
                            if (ddoResul.l_apto_prox_examen)
                            {//esta en condiciones de ser seleccionado.
                                 NomadEnvironment.GetTrace().Info("4");
                                 docResRec.AddChildDocument("<RECLUTADO oi_reclutado= \"" + oi_rec + "\" elegir=\"0\"/>");
                            }else
                                {//el reclutado tiene cargado un resultado para el ultimo examen pero no aprovo.
                                    NomadEnvironment.GetTrace().Info("3");
                                    continue;
                                }

                        }else
                            {//estan mal cargados los datos
                                NomadEnvironment.GetTrace().Info("2");
                                continue;
                            }
                    }else
                        {//el reclutado no rindio el ultimo examen pero tiene cargado el registro del mismo.
                            NomadEnvironment.GetTrace().Info("1");
                            continue;

                        }

                     */
                }
            }
        }
        docRes.AddAttribute("Mensaje", mensaje);
        NomadEnvironment.GetTrace().Info("Resultado -- " + docRes.ToString());
        return docRes;

    }
    public static Nomad.NSystem.Document.NmdXmlDocument SeleccionPersonal(Nomad.NSystem.Document.NmdXmlDocument Filtros, Nomad.NSystem.Proxy.NomadXML FILTERDATA)
    {

        //Nomad.NSystem.Document.NmdXmlDocument Filtros;

        NomadEnvironment.GetTrace().Info("Filtros --" + Filtros.ToString());

        //armo el documento para salir
        Nomad.NSystem.Document.NmdXmlDocument documento = new Nomad.NSystem.Document.NmdXmlDocument("<DATOS oi_referencia=\"" + FILTERDATA.FirstChild().GetAttr("oi_referencia") + "\" Mensaje=\"" + FILTERDATA.FirstChild().GetAttr("Mensaje") + "\" />");
        documento.AddChildDocument("<ROWS/>");
        Nomad.NSystem.Document.NmdXmlDocument docRec = (Nomad.NSystem.Document.NmdXmlDocument)documento.GetDocumentByName("ROWS");

        string elegir = "";
        string oi_rec = "";
        int cont = 0;
        string personas = "";
        Nomad.NSystem.Document.NmdXmlDocument ddoRec;
        Nomad.NSystem.Document.NmdXmlDocument rows = (NmdXmlDocument)Filtros.GetFirstChildDocument();
        for (ddoRec = (NmdXmlDocument)rows.GetFirstChildDocument(); ddoRec != null; ddoRec = (NmdXmlDocument)rows.GetNextChildDocument())
        {
            oi_rec = ddoRec.GetAttribute("id").Value;
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO DDO = NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Get(oi_rec);
            DDO.c_estado = "S";
            DDO.f_seleccion = DateTime.Now;
            NomadEnvironment.GetCurrentTransaction().Save(DDO);
            //personas = personas + ddoRec.GetAttribute("c_reclutado").Value + "-"+ ddoRec.GetAttribute("d_reclutado").Value + "\n";
            docRec.AddChildDocument(ddoRec.ToString());
            cont = cont + 1;
        }
        documento.AddAttribute("cant", cont.ToString());
        //documento.AddAttribute("personas",personas.ToString());

        NomadEnvironment.GetTrace().Info("Cantidad --" + cont);
        NomadEnvironment.GetTrace().Info("Salida --" + documento.ToString());
        return documento;

    }
    public void EliminarRankeados()
    {

        string param = "<DATOS oi_referencia=\"" + this.Id + "\" estado=\"N\"/>";

        Nomad.NSystem.Document.NmdXmlDocument docRankeados = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Resources.ListarIdsPorEstado, param));
        NomadEnvironment.GetTrace().Info("reclutados a eliminar " + docRankeados.ToString());

        string oi_ran;

        Nomad.NSystem.Document.NmdXmlDocument docRan;
        for (docRan = (Nomad.NSystem.Document.NmdXmlDocument)docRankeados.GetFirstChildDocument(); docRan != null; docRan = (Nomad.NSystem.Document.NmdXmlDocument)docRankeados.GetNextChildDocument())
        {
            oi_ran = docRan.GetAttribute("oi_reclutado").Value;
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO ddoREC = NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Get(oi_ran);
            NomadEnvironment.GetCurrentTransaction().Delete(ddoREC);
        }

    }
    public void ReclutamientoExternoPuesto(NucleusRH.Base.Gestion_de_Postulantes.Puestos.PUESTO ddoPuesto, Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        NomadBatch objBatch;
        objBatch = NomadBatch.GetBatch("Iniciando...", "Reclutamiento Externo");

        NomadEnvironment.GetTrace().Info("Entro en el Metodo Filtros = " + Filtros.ToString());

        string ignorarReferencia = Filtros.GetAttribute("ignorarReferencia").Value;
        string ignorarExperiencia = Filtros.GetAttribute("ignorarExperiencia").Value;
        string ignorarIdiomas = Filtros.GetAttribute("ignorarIdiomas").Value;
        string ignorarEstudios = Filtros.GetAttribute("ignorarEstudios").Value;
        string ignorarCursos = Filtros.GetAttribute("ignorarCursos").Value;
        string postulados = Filtros.GetAttribute("postulados").Value;
        double pondInscripcion = Nomad.NSystem.Functions.StringUtil.str2dbl(Filtros.GetAttribute("pondInscripcion").Value);
        double pondPostulacion = Nomad.NSystem.Functions.StringUtil.str2dbl(Filtros.GetAttribute("pondPostulacion").Value);

        int edad = 0;
        bool cumple = false;

        NomadEnvironment.GetTrace().Info("Elimina los rankeados");
        this.EliminarRankeados();

        Nomad.NSystem.Document.NmdXmlDocument ddoPostulante;
        NomadLog.Info("postulados::" + postulados.ToString());
        if (postulados == "")
        {
            NomadLog.Info("Filtros::" + Filtros.ToString());                   
            ddoPostulante = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE.Resources.ListarIds, Filtros.ToString()));
        }
        else
            ddoPostulante = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE.Resources.ListarIdsPost, Filtros.ToString()));

        //Nomad.NSystem.Document.NmdXmlDocument ddoPostulante = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE.Resources.ListarIds, Filtros.ToString()));

        NomadEnvironment.GetTrace().Info("ddoPostulante = " + ddoPostulante.ToString());
        Nomad.NSystem.Document.NmdXmlDocument Persona;

        DateTime hoy = DateTime.Now;
        //aqui declaro la fecha de hoy

        int cont = 0;
        int cont2 = 1;

        for (Persona = (Nomad.NSystem.Document.NmdXmlDocument)ddoPostulante.GetFirstChildDocument(); Persona != null; Persona = (Nomad.NSystem.Document.NmdXmlDocument)ddoPostulante.GetNextChildDocument())
        {
            objBatch.SetPro(0, 100, ddoPostulante.ChildDocuments.Count, cont + 1);

            NomadEnvironment.GetTrace().Info("dentroDelFor");
            cont = cont + 1;

            objBatch.SetMess("Rankeando Personas " + cont + " de " + ddoPostulante.ChildDocuments.Count);

            //aqui inicializo las variables
            edad = 0;
            NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPost = NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE.Get(Persona.GetAttribute("oi_postulante").Value.ToString());
            // en ddoPostulante  voy guardando todas las personas empresas.

            //aqui empiezo las validaciones

            if (ignorarReferencia == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 1 Se fija si la referencia tiene filtros definidos
                // 1.a se fija en el sexo

                if (!this.c_sexo_reqNull)
                {
                    if (!ddoPost.c_sexoNull)
                    {
                        if (this.c_sexo_req != ddoPost.c_sexo)
                        {
                            //NomadEnvironment.GetTrace().Info("La Persona " + ddoPostulante.c_personal + " - " + ddoPostulante.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer el sexo requerido");
                            continue;
                        }
                    }
                }

                // 1.b se fija en el estado Civil
                if (!this.oi_estado_civilNull && !ddoPost.oi_estado_civilNull)
                {
                    if (this.oi_estado_civil != ddoPost.oi_estado_civil)
                    {
                        //NomadEnvironment.GetTrace().Info("La Persona " + ddoPostulante.c_personal + " - " + ddoPostulante.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer el estado civil requerido");
                        continue;
                    }
                }

                //calculo la edad
                if (!ddoPost.f_nacimNull)
                {
                    edad = NucleusRH.Base.Personal.UtilBase.DiffYears(ddoPost.f_nacim, hoy);
                }

                // 1.c se fija en la edad minima requerida
                if (!this.e_edad_minima_reqNull && (edad > 0))
                {
                    if (this.e_edad_minima_req >= edad)
                    {
                        //NomadEnvironment.GetTrace().Info("La Persona " + ddoPostulante.c_personal + " - " + ddoPostulante.d_ape_y_nom + " ha salido del proceso de Reclutamiento por ser mayor a la edad minima requerida");
                        continue;
                    }
                }

                // 1.d se fija en la edad maxima requerida
                if (!this.e_edad_maxima_reqNull && (edad > 0))
                {
                    if (this.oi_estado_civil != ddoPost.oi_estado_civil)
                    {
                        //NomadEnvironment.GetTrace().Info("La Persona " + ddoPostulante.c_personal + " - " + ddoPostulante.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer el estado civil requerido");
                        continue;
                    }
                }
            }

            if (ignorarExperiencia == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 2. verifica las experiencias requeridas para el puesto
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarExperienciaPuestoExterno(ddoPuesto, ddoPost);
                if (!cumple)
                {
                    //NomadEnvironment.GetTrace().Info("La Persona " + ddoPostulante.c_personal + " - " + ddoPostulante.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer la experiencia requerida");
                    continue;
                }
            }

            if (ignorarIdiomas == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 3. Verifica los idiomas requeridos para el puesto
                //NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto;
                //NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPost
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarIdiomas(ddoPuesto.PUESTO_IDIO, ddoPost.IDIOMAS_POS);
                if (!cumple)
                {
                    //NomadEnvironment.GetTrace().Info("La Persona " + ddoPostulante.c_personal + " - " + ddoPostulante.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer los Idiomas Requeridos");
                    continue;
                }
            }

            if (ignorarEstudios == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 4. Verifica los estudios requeridos para el puesto
                //  NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto;
                //      NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPost
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarEstudios(ddoPuesto.PUESTO_ESTUD, ddoPost.ESTUDIOS_POS);
                if (!cumple)
                {
                    //NomadEnvironment.GetTrace().Info("La Persona " + ddoPostulante.c_personal + " - " + ddoPostulante.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer los estudios Requeridos");
                    continue;
                }
            }

            if (ignorarCursos == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 5. Verifica los Cursos requeridos para el puesto
                //cumple = this.ValidarCursosPuestoExterno(ddoPuesto, ddoPost);
                //NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto;
                //NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPost
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarCursos(ddoPuesto.PUESTO_CURS, ddoPost.CURSOS_POS);
                if (!cumple)
                {
                    //NomadEnvironment.GetTrace().Info("La Persona " + ddoPostulante.c_personal + " - " + ddoPostulante.d_ape_y_nom + " ha salido del proceso de Reclutamiento por no poseer los Cursos Requeridos");
                    continue;
                }
            }

            /*
            if (ignorarAreas == "0")// aplico los filtros siempre y cuando no hayan indicado lo contrario.
            {
                // 5. Verifica las areas de conocimiento requeridas para la referencia
                //NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA ddoReferencia;
                //NomadEnvironment.GetTrace().Info("ddoReferencia = " + ddoPostulante.ToString());
                cumple = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA.ValidarAreas(ddoPuesto.AREA_REF, ddoPost.AREA_POSTU);
                if (!cumple)
                {
                    continue;
                }
            }
            */

            // 6. Aqui llegan las personas que pasan todos los filtros.
            NomadEnvironment.GetTrace().Info("Esta listo para calcular el ranking");
            this.RankingReclutamientoExternoPuesto(ddoPuesto, ddoPost, Filtros, Persona.GetAttribute("oi_reclutado").Value);

            if (cont == (10 * cont2))
            {
                objBatch.Log("Se han Rankeado " + cont + " personas");
                cont2 = cont2 + 1;
            }
        }

    }
    public void RankingReclutamientoExternoPuesto(NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto, NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPos, Nomad.NSystem.Document.NmdXmlDocument Filtros, string oi_reclutamiento)
    {

        //NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPos;
        double ranking = 0;
        double pondInscripcion = Nomad.NSystem.Functions.StringUtil.str2dbl(Filtros.GetAttribute("pondInscripcion").Value);
        double pondPostulacion = Nomad.NSystem.Functions.StringUtil.str2dbl(Filtros.GetAttribute("pondPostulacion").Value);

        if (pondInscripcion == 0)
        { //significa que no se definio una ponderacion por inscripcion por lo tanto tomara un valor por defecto
            pondInscripcion = 0;
        }

        if (pondPostulacion == 0)
        {//significa que no se definio una ponderacion por postulacion por lo tanto tomara un valor por defecto
            pondPostulacion = 0;
        }

        // 1. el personal esta inscripto para la referencia?
        foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_POST RefPos in this.REF_POST)
        {
            if (RefPos.oi_postulante == ddoPos.Id)
            {
                ranking = ranking + pondInscripcion;
            }
        }

        // 2. si el puesto es uno de los que el personal se postularia
        foreach (NucleusRH.Base.Gestion_de_Postulantes.Postulantes.PUESTO_POS ddoPosPuesto in ddoPos.PUESTOS_POS)
        {
            if (ddoPosPuesto.oi_puesto == ddoPuesto.Id)
            {
                ranking = ranking + pondPostulacion;
            }
        }

        // 3. para cada experiencia no excluyente del personal
        foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_EXP ddoPueExp in ddoPuesto.PUESTO_EXP)
        {
            if (!ddoPueExp.l_excluyente)
            {
                foreach (NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ANTECEDENTE ddoAntec in ddoPos.ANTECEDENTES)
                {
                    foreach (NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ANTEC_PUESTO ddoAntecPuesto in ddoAntec.ANTEC_PUESTO)
                    {
                        if (!ddoAntecPuesto.oi_puesto_empresaNull)
                        {
                            if (ddoAntecPuesto.oi_puesto_empresa == ddoPueExp.oi_puesto_req_exp)
                            {
                                if (!ddoPueExp.e_pesoNull)
                                {
                                    ranking = ranking + ddoPueExp.e_peso;
                                }
                            }
                        }
                    }
                }
            }
        }

        // 4. para cada Idioma no excluyente del personal
        foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_IDIO ddoPueIdio in ddoPuesto.PUESTO_IDIO)
        {
            if (!ddoPueIdio.l_excluyente)
            {
                foreach (NucleusRH.Base.Gestion_de_Postulantes.Postulantes.IDIOMA_POS ddoPosIdio in ddoPos.IDIOMAS_POS)
                {
                    if (ddoPosIdio.oi_idioma == ddoPueIdio.oi_idioma)
                    {
                        if (!ddoPueIdio.e_pesoNull)
                        {
                            ranking = ranking + ddoPueIdio.e_peso;
                        }
                    }
                }
            }
        }

        // 5. para cada Estudio no excluyente del personal
        foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_ESTUD ddoPueEst in ddoPuesto.PUESTO_ESTUD)
        {
            if (!ddoPueEst.l_excluyente)
            {
                foreach (NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ESTUDIO_POS ddoPosEst in ddoPos.ESTUDIOS_POS)
                {
                    if (ddoPosEst.oi_estudio == ddoPueEst.oi_estudio)
                    {
                        if (!ddoPueEst.e_pesoNull)
                        {
                            ranking = ranking + ddoPueEst.e_peso;
                        }
                    }
                }
            }
        }

        // 6. para cada Curso no excluyente del personal
        foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_CURSO ddoPueCur in ddoPuesto.PUESTO_CURS)
        {
            if (!ddoPueCur.l_excluyente)
            {
                foreach (NucleusRH.Base.Gestion_de_Postulantes.Postulantes.CURSO_EXT_POS ddoPosCur in ddoPos.CURSOS_POS)
                {
                    if (ddoPosCur.oi_curso == ddoPueCur.oi_curso)
                    {
                        if (!ddoPueCur.e_peso_requisitoNull)
                        {
                            ranking = ranking + ddoPueCur.e_peso_requisito;
                        }
                    }
                }
            }
        }
        //Console.WriteLine("Reclutamiento Interno por Posicion" + ranking + " -- " + ddoPer.Id);

        NomadEnvironment.GetTrace().Info("Calculo el ranking" + ranking);
        this.CrearReclutados(ddoPos.c_postulante, "POS", ranking, oi_reclutamiento, ddoPos.Id);
        //  this.CrearReclutados(ddoPer, ddoPerEmp, ranking, oi_reclutamiento);

    }
    public void ReclutamientoExterno(Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        NomadBatch objBatch;
        objBatch = NomadBatch.GetBatch("Iniciando Reclutamiento Externo...", "Reclutamiento Externo");

        NomadEnvironment.GetTrace().Info("entro en reclutamiento externo " + Filtros.ToString());
        if (this.ca_estado == "I")
        {
            NomadEnvironment.GetTrace().Info("Iniciando Reclutamiento Externo ");
            //Console.WriteLine("esta Inicializada");
            NomadLog.Info("this.oi_puesto::" + this.oi_puesto);
            if (!this.oi_puestoNull)
            {
                if (this.oi_posicionNull)
                {
                    objBatch.Err("Hubo algun error en la carga de datos. Una Referencia no puede estar definida para un puesto y una posición");
                    NomadEnvironment.GetTrace().Info("Hubo algun error en la carga de datos. Una Referencia no puede estar definida para un puesto y una posición ");
                    //Console.WriteLine("Hubo algun error en la carga de datos. Una Referencia no puede estar definida para un puesto y una posicion");
                }
                else
                {
                    NucleusRH.Base.Gestion_de_Postulantes.Puestos.PUESTO ddoPuesto;
                    ddoPuesto = this.Getoi_puesto();
                    NomadEnvironment.GetTrace().Info("por puesto ");
                    // en ddoPuesto voy a guardar el puesto de la referencia

                    NomadEnvironment.GetTrace().Info("llamo a ReclutamientoExternoPuesto ");
                    this.ReclutamientoExternoPuesto(ddoPuesto, Filtros);
                    NomadEnvironment.GetTrace().Info("VUELVO DE ReclutamientoExternoPuesto ");

                }
            }
            else
            {
                if (!this.oi_posicionNull)
                {
                    NucleusRH.Base.Gestion_de_Postulantes.Puestos.POSICION ddoPosicion;
                    ddoPosicion = this.Getoi_posicion();
                    // en ddoPosicion voy a guardar la Posicion de la referencia
                    this.ReclutamientoExternoPosicion(ddoPosicion, Filtros);
                }
                else
                {
                    objBatch.Err("Hubo algun error en la carga de datos. una referencia no puede estar cargada sin puesto ni posicion");
                    NomadEnvironment.GetTrace().Info("Hubo algun error en la carga de datos. una referencia no puede estar cargada sin puesto ni posicion");
                }
            }

        }
        else
        {
            objBatch.Err("La Referencia No esta Inicializada");
            NomadEnvironment.GetTrace().Info("La Referencia no esta Inicializada");
        }
        //Console.WriteLine("Ocurrio Exception:" +   this.c_referencia);

    }
    public void ReclutamientoExternoPosicion(NucleusRH.Base.Gestion_de_Postulantes.Puestos.POSICION ddoPosicion, Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        //Console.WriteLine("Reclutamiento Interno por Posicion");

    }
    public void Nueva()
    {

        //RECUPERO EL PUESTO

        NucleusRH.Base.Gestion_de_Postulantes.Puestos.PUESTO ddoPuesto = this.Getoi_puesto();
        NomadLog.Info("ddoPuesto::: " + ddoPuesto.ToString());

        //RECUPERO TODOS LOS EXAMENES ASOCIADOS AL PUESTO
        foreach (NucleusRH.Base.Gestion_de_Postulantes.Puestos.PUES_EXA ddoPuesExa in ddoPuesto.PUES_EXA)
        {
            NucleusRH.Base.Gestion_de_Postulantes.Examenes.EXAMEN ddoExamen = ddoPuesExa.Getoi_examen();
            NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_EXAMEN ddoRefExa;
            ddoRefExa = new NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_EXAMEN();
            ddoRefExa.oi_examen = ddoExamen.Id;
            ddoRefExa.e_orden = ddoExamen.e_orden;
/*
            //RECUPERO TODOS LOS SUBEXAMENES ASOCIADOS A CADA UNO DE LOS EXAMENES RECUPERADOS Y ASIGNO ESTOS SUBEXAMENES A LA REFERENCIA
            foreach (NucleusRH.Base.Gestion_de_Postulantes.Examenes.SUBEXAMEN ddoSubExa in ddoExamen.SUBEXAMEN)
            {
                NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_SUBEXAMEN ddoRefSubExa;
                ddoRefSubExa = new NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_SUBEXAMEN();
                ddoRefSubExa.oi_subexamen = ddoSubExa.Id;
                ddoRefSubExa.l_req = ddoSubExa.l_req;

                //RECUPERO TODOS LOS FACTORES ASOCIADOS A CADA UNO DE LOS SUBEXAMENES RECUPERADOS Y ASIGNO ESTOS FACTORES A LA REFERENCIA
                foreach (NucleusRH.Base.Gestion_de_Postulantes.Examenes.FACT_SUBEXA ddoFactor in ddoSubExa.FACT_SUBEXA)
                {
                    NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_FACT ddoRefFact;
                    NucleusRH.Base.Gestion_de_Postulantes.Factores.FACTOR ddoFac = ddoFactor.Getoi_factor();
                    ddoRefFact = new NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_FACT();
                    ddoRefFact.oi_factor = ddoFac.Id;
                    ddoRefFact.oi_tipo_escala = ddoFac.oi_tipo_escala;

                    ddoRefSubExa.REF_FACT.Add(ddoRefFact);
                }

                //RECORRO TODOS LOS RESULTADOS DE LOS SUBEXAMENES RECUPERADOS Y LOS ASIGNO A LA REFERENCIA
                foreach (NucleusRH.Base.Gestion_de_Postulantes.Examenes.RESUL_SUBEXA ddoResSubExa in ddoSubExa.RESUL_SUBEXA)
                {
                    NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_RES_SUB ddoRefResSub;
                    ddoRefResSub = new NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_RES_SUB();
                    ddoRefResSub.oi_resultado = ddoResSubExa.oi_resultado;
                    ddoRefResSub.o_result_subexa = ddoResSubExa.o_resul_subexa;
                    ddoRefResSub.l_apto_prox_examen = ddoResSubExa.l_apto_prox_examen;

                    ddoRefSubExa.REF_RES_SUB.Add(ddoRefResSub);
                }

                ddoRefExa.REF_SUBEXA.Add(ddoRefSubExa);
            }

            //RECORRO TODAS LAS COMPETENCIAS DE LOS EXAMENES RECUPERADOS Y LAS ASIGNO A LA REFERENCIA
            foreach (NucleusRH.Base.Gestion_de_Postulantes.Examenes.COMPET_EXA ddoCompet in ddoExamen.COMPET_EXA)
            {
                NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_COM_EXA ddoRefCompExa;
                ddoRefCompExa = new NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_COM_EXA();
                ddoRefCompExa.oi_competencia = ddoCompet.oi_competencia;
                ddoRefCompExa.o_com_ref_exa = ddoCompet.o_compet_exa;

                ddoRefExa.REF_COM_EXA.Add(ddoRefCompExa);
            }
*/
            //RECORRO TODOS LOS RESULTADOS DE LOS EXAMENES RECUPERADOS Y LOS ASIGNO A LA REFERENCIA
            foreach (NucleusRH.Base.Gestion_de_Postulantes.Examenes.RESUL_EXA ddoResExa in ddoExamen.RESUL_EXA)
            {
                NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_RES_EXA ddoRefResExa;
                ddoRefResExa = new NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_RES_EXA();
                ddoRefResExa.oi_resultado = ddoResExa.oi_resultado;
                ddoRefResExa.o_ref_res_exa = ddoResExa.o_resul_exa;
                ddoRefResExa.l_apto_prox_examen = ddoResExa.l_apto_prox_examen;

                ddoRefExa.REF_RES_EXA.Add(ddoRefResExa);
            }

            this.REF_EXAMEN.Add(ddoRefExa);
        }

    }

    public static void DescartarReclutados(Nomad.NSystem.Document.NmdXmlDocument Filtros)
    {

        Nomad.NSystem.Document.NmdXmlDocument todosRec = (Nomad.NSystem.Document.NmdXmlDocument)Filtros.GetDocumentByName("ROWS");
        string elegido;
        string reclutado;
        Nomad.NSystem.Document.NmdXmlDocument cadaRec;

        for (cadaRec = (Nomad.NSystem.Document.NmdXmlDocument)todosRec.GetFirstChildDocument(); cadaRec != null; cadaRec = (Nomad.NSystem.Document.NmdXmlDocument)todosRec.GetNextChildDocument())
        {
            elegido = cadaRec.GetAttribute("elegir").Value;
            if (elegido == "1")
            {
                reclutado = cadaRec.GetAttribute("oi_reclutado").Value;
                NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO ddoRec = NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Get(reclutado);
                ddoRec.c_estado = "D";
                ddoRec.f_descartado = DateTime.Now;

                //SE ENVIA EL MAIL INFORMANDO DE LA SITUACION
                NomadXML xmlResult;
                //NomadEnvironment.GetTrace().Info("ddoRec -- " + ddoRec.SerializeAll());

                if (ddoRec.oi_personal_empNull)
                {
                    //RECUPERO EL POSTULANTE AL QUE HAY QUE ENVIARLE MAIL
                    xmlResult = NomadEnvironment.QueryNomadXML(REFERENCIA.Resources.QRY_DATOS_POST, "<PARAM oi_reclutado=\"" + ddoRec.id + "\"/>");
                }
                else
                {
                    //RECUPERO LA PERSONA A LA QUE HAY QUE ENVIARLE MAIL
                    xmlResult = NomadEnvironment.QueryNomadXML(REFERENCIA.Resources.QRY_DATOS_PERS, "<PARAM oi_reclutado=\"" + ddoRec.id + "\"/>");
                }

                //RECORRO LA LISTA DE RECUPERADOS
                for (NomadXML xmlcur = xmlResult.FirstChild().FirstChild(); xmlcur != null; xmlcur = xmlcur.Next())
                {
                    //CREO EL MAIL
                    Nomad.Base.Mail.OutputMails.MAIL objMail = new Nomad.Base.Mail.OutputMails.MAIL();
                    objMail.ASUNTO = "Respecto a la búsqueda laboral del Grupo Sancor Seguros ";
                    objMail.DESDE_APLICACION = NomadProxy.GetProxy().AppName;
                    objMail.FECHA_CREACION = DateTime.Now;

                    objMail.CONTENIDO = "Estimado Postulante:" + "\n" +
                                    "\t" + "\t" + "Por este medio nos comunicamos para informarle sobre la búsqueda laboral de la cual" + "\n" + "participó." + "\n" + "\n" +
                                    "\t" + "\t" + "En esta oportunidad no continuaremos avanzando con las siguientes etapas del proceso" + "\n" + "de selección." + "\n" + "\n" +
                                    "\t" + "\t" + "Su C.V. y toda la información personal relevada hasta el momento será incorporada a" + "\n" + "nuestra base de datos. Ante una necesidad que tenga como requerimientos un perfil similar al suyo, nos" + "\n" + "contactaremos." + "\n" + "\n" +
                                    "\t" + "\t" + "Le agradecemos mucho su participación y el interés en nuestra organización." + "\n" +
                                    "\t" + "\t" + "Cordialmente." + "\n" + "\n" +
                                    "Área Recursos Humanos" + "\n" +
                                    "03493-428500" + "\n" +
                                    "mailto:RRHHSeleccion@sancorseguros.com" + "\n" +
                                    "http://www.gruposancorseguros.com";

                    //CREL EL DESTINATARIO
                    Nomad.Base.Mail.OutputMails.DESTINATARIO objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                    objDestino.MAIL_SUSTITUTO = xmlcur.GetAttr("d_email");
                    objMail.DESTINATARIOS.Add(objDestino);

                    //GUARDO EL MAIL
                    NomadEnvironment.GetCurrentTransaction().Save(objMail);
                }
                NomadEnvironment.GetCurrentTransaction().Save(ddoRec);
            }
        }

    }
  }
}



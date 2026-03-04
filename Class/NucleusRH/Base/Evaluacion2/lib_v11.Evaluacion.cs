using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using NucleusRH.Base.Evaluacion2.Evaluadores;
using System.Collections.Generic;

namespace NucleusRH.Base.Evaluacion2.Evaluacion
{

	//////////////////////////////////////////////////////////////////////////////////
	//Clase Evaluacion
	public partial class EVALUACION
	{
		public static void ModMasivaEvaSup(Nomad.NSystem.Proxy.NomadXML param)
		{
			NomadBatch objBatch;
			objBatch = NomadBatch.GetBatch("Iniciando...", "Modificacion Masiva Evaluador/Supervisor");
			NomadXML xmldoc = param.FirstChild();

			int C = 0;
			int E = 0;
			string sup = "";
			string eva = "";
			string pos_eva = "";
			string pos_sup = "";
			string uni_eva = "";
			string uni_sup = "";
			string pos_new = xmldoc.GetAttr("oi_posicion_new");
			bool update_evaluadores = xmldoc.GetAttrBool("update_evaluadores");

			if (xmldoc.GetAttr("evaluador_new") != "")
			{
				//RECUPERO EL LEGAJO DEL EVALUADOR
				NomadXML xmlleg = NomadEnvironment.QueryNomadXML(EVALUACION.Resources.QRY_LEGAJO, "<DATO oi_empresa=\"" + xmldoc.GetAttr("empresa") + "\" oi_personal=\"" + xmldoc.GetAttr("evaluador_new") + "\"/>");
				NomadEnvironment.GetTrace().Info("PARAM EVA :" + "<DATO oi_empresa=\"" + xmldoc.GetAttr("empresa") + "\" oi_personal=\"" + xmldoc.GetAttr("evaluador_new") + "\"/>");
				eva = xmlleg.FirstChild().GetAttr("oi_personal_emp");
				pos_eva = xmlleg.FirstChild().GetAttr("oi_posicion_ult");
				NucleusRH.Base.Organizacion.Puestos.POSICION p = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(pos_eva, false);
				if (p == null) throw new Exception("Posición no encontrada.");
				uni_eva = p.oi_unidad_org;
			}
			if (xmldoc.GetAttr("supervisor_new") != "")
			{
				//RECUPERO EL LEGAJO DEL SUPERVISOR	
				NomadXML xmlleg = NomadEnvironment.QueryNomadXML(EVALUACION.Resources.QRY_LEGAJO, "<DATO oi_empresa=\"" + xmldoc.GetAttr("empresa") + "\" oi_personal=\"" + xmldoc.GetAttr("supervisor_new") + "\"/>");
				NomadEnvironment.GetTrace().Info("PARAM SUP :" + "<DATO oi_empresa=\"" + xmldoc.GetAttr("empresa") + "\" oi_personal=\"" + xmldoc.GetAttr("evaluador_new") + "\"/>");
				sup = xmlleg.FirstChild().GetAttr("oi_personal_emp");
				pos_sup = xmlleg.FirstChild().GetAttr("oi_posicion_ult");
				NucleusRH.Base.Organizacion.Puestos.POSICION p = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(pos_sup, false);
				if (p == null) throw new Exception("Posición no encontrada.");
				uni_sup = p.oi_unidad_org;
			}

			if (pos_eva == "" && eva != "")
			{
				objBatch.Err("El Evaluador seleccionado no tiene asignada una Posicion");
				return;
			}
			if (pos_sup == "" && sup != "")
			{
				objBatch.Err("El Supervisor seleccionado no tiene asignada una Posicion");
				return;
			}

			NomadEnvironment.GetTrace().Info("EVA :" + eva);
			NomadEnvironment.GetTrace().Info("EVA_POS :" + pos_eva);
			NomadEnvironment.GetTrace().Info("SUP :" + sup);
			NomadEnvironment.GetTrace().Info("SUP_POS :" + pos_sup);

			//RECUPERO CON UN QRY LOS ID DE LAS EVALUACIONES A MODIFICAR
			NomadXML xmleva = NomadEnvironment.QueryNomadXML(EVALUACION.Resources.QRY_EVALUACIONES, xmldoc.ToString());
			NomadEnvironment.GetTrace().Info("QRY_EVAS :" + xmleva.ToString());

			//RECORRO LAS EVALUACIONES
			ArrayList lista = (ArrayList)xmleva.FirstChild().GetElements("ROW");
			for (int xml = 0; xml < lista.Count; xml++)
			{
				NomadXML xmlcur = (NomadXML)lista[xml];
				objBatch.SetPro(0, 100, lista.Count, xml);
				objBatch.SetMess("Modificando datos a Evaluaciones " + (xml + 1) + " de " + lista.Count);

				//CARGO LA EVALUACION
				NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION ddoEVA = NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION.Get(xmlcur.GetAttr("oi_evaluacion"));

				try
				{
					if (eva != "")
					{
						ddoEVA.oi_pos_evaluador = pos_eva;
						ddoEVA.oi_uni_evaluador = uni_eva;
						ddoEVA.oi_evaluador = eva;
					}
					if (sup != "")
					{
						ddoEVA.oi_pos_supervisor = pos_sup;
						ddoEVA.oi_uni_supervisor = uni_sup;
						ddoEVA.oi_supervisor = sup;
					}
					if (pos_new != "")
					{
						ddoEVA.PLANTILLAS.Clear();
						NucleusRH.Base.Organizacion.Puestos.POSICION p = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(pos_new, false);
						if (p == null) throw new Exception("Posición no encontrada.");
						ddoEVA.oi_pos_evaluado = p.Id;
						ddoEVA.oi_uni_evaluado = p.oi_unidad_org;
						NucleusRH.Base.Evaluacion2.Eventos.EVENTO ddoEvento = NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Get(xmldoc.GetAttr("evento"), false);
						NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.TIPO_EVAL typEVA = ddoEvento.Getoi_tipo_eval();
						foreach (NucleusRH.Base.Evaluacion2.Eventos.PLANTILLA myPLA in ddoEvento.PLANTILLAS)
						{
							NucleusRH.Base.Evaluacion2.Evaluacion.PLANTILLA ddoPLA = NucleusRH.Base.Evaluacion2.Eventos.EVENTO.ArmarPlantilla(myPLA, typEVA, ddoEVA.oi_personal_emp, ddoEVA.oi_pos_evaluado);
							ddoEVA.PLANTILLAS.Add(ddoPLA);
						}

					}

					// Actualiza Evaluadores 360
					if (update_evaluadores)
						ddoEVA.ActualizarEvaluadores();


					NomadEnvironment.GetCurrentTransaction().Save(ddoEVA);
					C++;
				}
				catch (Exception e)
				{
					//CARGO EL LEGAJO
					NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(ddoEVA.oi_personal_emp, false);
					E++;
					objBatch.Err("Error modificando Evaluacion del Legajo: " + ddoLEG.e_numero_legajo.ToString() + " - " + e.Message);
				}
			}

			if (C != 0)
			{
				objBatch.Log("Se modificaron " + C.ToString() + " evaluacion/es.");
			}
			if (E != 0)
			{
				objBatch.Log("No se puedieron modificar " + E.ToString() + " evaluacion/es.");
			}
		}

        public void CambiarEstado(string Estado)
        {
            if (this.c_estado != Estado)
            {
            this.c_estado = Estado;
            this.f_estado = DateTime.Now;
            this.c_user_estado = NomadProxy.GetProxy().UserName;
            
                //Creo un registro de Cambio de estado
                NucleusRH.Base.Evaluacion2.Evaluacion.CAMBIO_EST cambio_estado = new NucleusRH.Base.Evaluacion2.Evaluacion.CAMBIO_EST();

                cambio_estado.c_estado = this.c_estado;
                cambio_estado.f_cambio_est = this.f_estado;
                cambio_estado.c_user_cambio = this.c_user_estado;

                this.CAMBIOS_EST.Add(cambio_estado);
            }
        }
        
        public static void CambiarEstado(EVALUACION e, string Estado)
        {
            System.Text.StringBuilder IDS = new System.Text.StringBuilder();

            string estActual = e.c_estado;
            
            e.CambiarEstado(Estado);
            IDS.Append(e.id);
            string estado_eva = e.c_estado;
            NomadEnvironment.GetCurrentTransaction().Save(e);

            //Llamada al evento
            if(estActual!=Estado)
                NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusRH.Base.Evaluacion2.Evaluaciones.chg_status", IDS.ToString(), estado_eva, "Cambio a estado " + estado_eva);
        }
        public static void CambiarEstadoFlow(string oi_evaluacion, string Estado)
        {
            System.Text.StringBuilder IDS = new System.Text.StringBuilder();
            
            EVALUACION e = EVALUACION.Get(oi_evaluacion, false);
            e.CambiarEstado(Estado);
            IDS.Append(e.id);
            string estado_eva = e.c_estado;
            NomadEnvironment.GetCurrentTransaction().Save(e);

            //Llamada al evento
            NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusRH.Base.Evaluacion2.Evaluaciones.chg_status", IDS.ToString(), estado_eva, "Cambio a estado " + estado_eva);
        }

		
		/* FUNCION QUE ACTUALIZA LOS EVALUADORES 360 DESDE LOS DATOS DE OTROS EVALUADORES CONFIGURADOS PARA EL LEGAJO */
		public void ActualizarEvaluadores()
		{
			// Verifica el estado, debe ser anterior a CONFIRMADA
			if (this.c_estado == "CO" || this.c_estado == "AN" || this.c_estado == "AP" || this.c_estado == "RZ" || this.c_estado == "CE")
				throw new Exception("No se puede actualizar evaluadores debido al estado de la evaluación.");

			Hashtable evaluadores_actuales = NomadEnvironment.QueryHashtable(EVALUADOR360.Resources.qry_evaluadores, string.Format("<FILTRO oi_personal_emp=\"{0}\" />", oi_personal_emp), "oi_evaluador");

			// Borra los evaluadores que no están más en el legajo
			IList lista_borrar = new ArrayList();
			foreach (EVA_EVALUADOR eval in this.EVALUADORES)
				if (!evaluadores_actuales.ContainsKey(eval.oi_personal_emp))
					lista_borrar.Add(eval);
			foreach (EVA_EVALUADOR eval in lista_borrar)
				this.EVALUADORES.Remove(eval);


			// Agrega los evaluadores nuevos y actualiza roles
			foreach (string id in evaluadores_actuales.Keys)
			{
				NomadXML dat = (NomadXML)evaluadores_actuales[id];
				EVA_EVALUADOR eval = (EVA_EVALUADOR)this.EVALUADORES.GetByAttribute("oi_personal_emp", id);
				if (eval != null)
				{
					if (eval.oi_rol != dat.GetAttr("oi_rol"))
						eval.oi_rol = dat.GetAttr("oi_rol");
				}
				else
				{
					eval = new EVA_EVALUADOR();
					eval.oi_personal_emp = id;
					eval.oi_rol = dat.GetAttr("oi_rol");
					eval.c_estado = "I";
					this.EVALUADORES.Add(eval);

					// Si la evaluación está EVALUADA vuelve a HABILITADA para que el evaluador nuevo pueda cargar.
					if (this.c_estado == "EV") this.c_estado = "HA";
				}
			}
		}
    }
}

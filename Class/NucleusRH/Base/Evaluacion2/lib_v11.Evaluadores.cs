using System;
using System.Collections.Generic;
using System.Text;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using System.Collections;
using NucleusRH.Base.Evaluacion2;
using NucleusRH.Base.Evaluacion2.Evaluacion;

namespace NucleusRH.Base.Evaluacion2.Evaluadores
{
	//////////////////////////////////////////////////////////////////////////////////
	//Clase Evaluadores
	public partial class EVALUADOR360
	{
		bool Encontrado;

		public static void Guardar(NomadXML datos)
		{
			string oi_personal_emp = datos.FirstChild().GetAttr("oi_personal_emp");
			Hashtable ids = NomadEnvironment.QueryHashtable(EVALUADOR360.Resources.qry_evaluadores, string.Format("<FILTRO oi_personal_emp=\"{0}\" />", oi_personal_emp), "id");
			NomadObjectList list = NomadEnvironment.GetObjects(ids.Keys, new EVALUADOR360().GetType());

			for (NomadXML eval = datos.FirstChild().FirstChild().FirstChild(); eval != null; eval = eval.Next())
			{
				EVALUADOR360 e = (EVALUADOR360)list.GetByAttribute("oi_evaluador", eval.GetAttr("oi_evaluador"));
				if (e!=null)
				{
					e.Encontrado = true;
					if (e.oi_rol != eval.GetAttr("oi_rol"))
					{
						e.oi_rol = eval.GetAttr("oi_rol");
						NomadEnvironment.GetCurrentTransaction().Save(e);
					}
				}
				else
				{
					e = new EVALUADOR360();
					e.oi_personal_emp = oi_personal_emp;
					e.oi_evaluador = eval.GetAttr("oi_evaluador");
					e.oi_rol = eval.GetAttr("oi_rol");
					NomadEnvironment.GetCurrentTransaction().Save(e);
				}
			}

			foreach (EVALUADOR360 e in list)
			{
				if (!e.Encontrado)
					NomadEnvironment.GetCurrentTransaction().Delete(e);
			}
		}
	}
}

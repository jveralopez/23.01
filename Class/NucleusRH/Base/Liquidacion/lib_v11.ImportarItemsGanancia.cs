using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Liquidacion.InterfaceItemsGanancia;
using NucleusRH.Base.Liquidacion.Legajo_Liquidacion;
using NucleusRH.Base.Liquidacion.Items_Deducibles_de_Ganancias;

namespace NucleusRH.Base.Liquidacion_IG
{

	class clsImportarIG
	{

		private Hashtable htaOIs = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public clsImportarIG()
		{
		}

		/// <summary>
		/// Importa los items de ganancia a los legajos liquidación
		/// </summary>
		/// <param name="pstrOIEmpresa">OI de la Empresa</param>
		public void Importar(string pstrOIEmpresa)
		{
			Hashtable htaItemsIG;
			Hashtable htaPersonalLiqs = new Hashtable(); //Contendrá los objetos PersonalLiq
			int intItems;
			int intCurrentItem;
			string strValidation;
			string strOIPersonalLiq;
			string strOIIG = "";
			int intInserts = 0;
			int intUpdates = 0;

			NomadXML xmlOIItems;
			NomadBatch objBatch = NomadBatch.GetBatch("Importar IG", "Importar IG");

			ENTRADA objInterfazIG;
			PERSONAL_EMP objPersonalEmp = null;
			DEDUC_IG objIGPersona;

			NomadBatch.Trace("--------------------------------------------------------------------------");
			NomadBatch.Trace(" Comienza Importar Items de Ganacias -------------------------------------");
			NomadBatch.Trace("--------------------------------------------------------------------------");

			objBatch.SetPro(0);
			objBatch.SetMess("Importando los Ítems de Ganancias.");
			objBatch.Log("Comienza la importación");

			//Obtiene los tipos de items IG
			htaItemsIG = NomadEnvironment.QueryHashtable(ITEM_IG.Resources.QRY_ItemsIG, "", "cod");

			//Obtiene la lista de OIs de la interface de Items IG para LegajosLiquidación
			xmlOIItems = NomadEnvironment.QueryNomadXML(ENTRADA.Resources.QRY_OIArchivo, "");
			xmlOIItems = xmlOIItems.FirstChild();

			//Recorre el archivo y pide los DDO de interface
			objBatch.SetPro(10);
			intItems = xmlOIItems.ChildLength;
			intCurrentItem = 0;
			for (NomadXML xmlRowOI = xmlOIItems.FirstChild(); xmlRowOI != null; xmlRowOI = xmlRowOI.Next())
			{
				objInterfazIG = ENTRADA.Get(xmlRowOI.GetAttr("id"));
				intCurrentItem++;
				strValidation = "";

				objBatch.SetMess("Importando el registro '" + intCurrentItem.ToString() + "' de '" + intItems.ToString() + "'.");

				//Valida que el tipo de IG exista entre las posibles a importar ----------------------------
				if (htaItemsIG.ContainsKey(objInterfazIG.c_item_ig))
				{
					strOIIG = ((NomadXML)htaItemsIG[objInterfazIG.c_item_ig]).GetAttr("oi");
				}
				else
				{
					strValidation = strValidation + "El código de IG '" + objInterfazIG.c_item_ig + "' no existe. ";
				}

				//Valida el resto del contenido del registro ------------------------------------------------
				NomadBatch.Trace("Validando valores");
				strValidation = strValidation + ValidarValores(objInterfazIG);

				//Obtiene el PersonalLiq desde la hash o desde un Get ----------------------------------------
				NomadBatch.Trace("Obteniendo el OI");
				if (htaPersonalLiqs.ContainsKey(objInterfazIG.e_numero_legajo.ToString()))
				{
					//Obtiene el PersonalLiq desde la hash
					objPersonalEmp = (PERSONAL_EMP)htaPersonalLiqs[objInterfazIG.e_numero_legajo.ToString()];
				}
				else
				{
					//Obtiene el PersonalLiq desde la DB y lo guarda en la hash
					strOIPersonalLiq = this.GetOIPersonal(pstrOIEmpresa, objInterfazIG.e_numero_legajo.ToString());

					//Valida que el legajo exista en la liquidación
					if (strOIPersonalLiq == "")
					{
						strValidation = strValidation + "No tiene un número de legajo válido o no pertenece a la empresa seleccionada.";
					}
					else
					{
						objPersonalEmp = PERSONAL_EMP.Get(strOIPersonalLiq);
					}
				}

				//Verifica que el registro no tenga errores
				if (strValidation != "")
				{
					strValidation = "El registro número " + intCurrentItem.ToString() + " tiene los siguientes errores: " + strValidation;
					objBatch.Err(strValidation);
				}
				else
				{
					objIGPersona = new DEDUC_IG();
					objPersonalEmp.DEDUC_IG.Add(objIGPersona);
					intInserts++;

					objIGPersona.oi_item_ig = strOIIG;
					objIGPersona.e_anio = objInterfazIG.e_anio;
					objIGPersona.e_periodo_des = objInterfazIG.e_periodo_desde;
					objIGPersona.e_periodo_has = objInterfazIG.e_periodo_hasta;
					objIGPersona.n_importe = objInterfazIG.n_importe;
					objIGPersona.c_tipo = "M";

					//Se agrega el legajo a la lista de legajos a realizar el save
					if (!htaPersonalLiqs.ContainsKey(objInterfazIG.e_numero_legajo.ToString()))
						htaPersonalLiqs.Add(objInterfazIG.e_numero_legajo.ToString(), objPersonalEmp);

				}

			}

			objBatch.SetPro(80);

			//Recorre los LegajosLiqs y los guarda en la DB
			objBatch.Log("Grabando los datos en la Base de Datos. (" + htaPersonalLiqs.Count.ToString() + " legajos)");
			int x = 1;
			foreach (string strLegajo in htaPersonalLiqs.Keys)
			{
				objBatch.SetPro(80, 100, htaPersonalLiqs.Count, x);
				try
				{
					NomadEnvironment.GetCurrentTransaction().Save((PERSONAL_EMP)htaPersonalLiqs[strLegajo]);
				}
				catch (Exception ex)
				{
					objBatch.Err("No se pudo actualizar el legajo '" + strLegajo + "'. " + ex.Message);
				}
				x++;
			}

			objBatch.SetPro(100);
			objBatch.Log("La importación terminó correctamente.");
			objBatch.Log("Se ingresaron '" + intInserts.ToString() + "' registros nuevos.");
		}

		/// <summary>
		/// Valida datos del registro de la interfaz
		/// </summary>
		/// <param name="pobjIGArchivo"></param>
		/// <returns></returns>
		private string ValidarValores(ENTRADA pobjIGArchivo)
		{
			string strResult = "";

			//Valida el año
			if (pobjIGArchivo.e_anio == 0 || pobjIGArchivo.e_anio.ToString().Length != 4)
			{
				strResult = strResult + "El año no es válido. ";
			}

			//Valida el periodo desde
			if (pobjIGArchivo.e_periodo_desde == 0 || pobjIGArchivo.e_periodo_desde.ToString().Length != 6)
			{
				strResult = strResult + "El período desde no es válido. ";
			}

			//Valida el periodo hasta
			if (pobjIGArchivo.e_periodo_hasta == 0 || pobjIGArchivo.e_periodo_hasta.ToString().Length != 6)
			{
				strResult = strResult + "El período hasta no es válido. ";
			}

			//Valida que los periodos correspondan con el año indicado
			if (strResult == "")
			{
				if (pobjIGArchivo.e_anio.ToString() != pobjIGArchivo.e_periodo_desde.ToString().Substring(0, 4) ||
					pobjIGArchivo.e_anio.ToString() != pobjIGArchivo.e_periodo_hasta.ToString().Substring(0, 4))
				{
					strResult = "Alguno de los períodos no corresponden con el año indicado. ";
				}
			}

			return strResult;
		}

		/// <summary>
		/// Obtiene un hashtable accesible por codigo de varible y retorna el OI.
		/// </summary>
		/// <param name="pstrOiEmpresa">Oi de la empresa del legajo.</param>
		/// <param name="pstrLegajo">Código del legajo.</param>
		/// <returns></returns>
		private string GetOIPersonal(string pstrOiEmpresa, string pstrLegajo)
		{
			if (this.htaOIs == null)
			{
				string strParametros = "<PARAMS oi_empresa=\"" + pstrOiEmpresa + "\" />";
				this.htaOIs = NomadEnvironment.QueryHashtable(ENTRADA.Resources.QRY_IDLegajos, strParametros, "cod");
			}

			return this.htaOIs.ContainsKey(pstrLegajo) ? ((NomadXML)this.htaOIs[pstrLegajo]).GetAttr("oi") : "";
		}

	}
}
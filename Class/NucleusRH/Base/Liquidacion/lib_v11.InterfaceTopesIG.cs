using System;
using System.Collections;
using System.Collections.Generic;
using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using NucleusRH.Base.Liquidacion.Items_Deducibles_de_Ganancias;

namespace NucleusRH.Base.Liquidacion.InterfaceTopesIG
{
	public partial class ENTRADA
	{
		/// <summary>
		/// Importa los topes a las deducciones de ganancias
		/// </summary>
		public static void Importar()
		{
			IDictionary<string, int> TiposDeduccion = new Dictionary<string, int>();
			int RowsCount;
			int intCurrentItem;
			int inserts = 0;

			NomadBatch objBatch = NomadBatch.GetBatch("Importar Topes IG", "Importar Topes IG");
			NomadBatch.Trace("Comienza Importar Topes de Ganancias ------------------------------------ ");

			ENTRADA objInterfaz;

			objBatch.SetMess("Importando los Topes IG");

			//Obtiene la lista de OIs de la interface de Items IG para LegajosLiquidación
			NomadXML RowIDs = NomadEnvironment.QueryNomadXML(ENTRADA.Resources.QRY_OIArchivo, "").FirstChild();

			//Recorre el archivo y pide los DDO de interface
			objBatch.SetPro(10);
			RowsCount = RowIDs.ChildLength;
			intCurrentItem = 0;
			for (NomadXML xmlRowOI = RowIDs.FirstChild(); xmlRowOI != null; xmlRowOI = xmlRowOI.Next())
			{
				objInterfaz = ENTRADA.Get(xmlRowOI.GetAttr("id"));
				intCurrentItem++;
				objBatch.SetMess(string.Format("Importando el registro {0} de {1}", intCurrentItem.ToString(), RowsCount.ToString()));
				objBatch.Log(string.Format("Importando el registro {0} de {1}", intCurrentItem.ToString(), RowsCount.ToString()));

				//Valida que el item de deducción
				string oi_idig = string.Empty;
				if (!TiposDeduccion.ContainsKey(objInterfaz.Item))
				{
					oi_idig = NomadEnvironment.QueryValue("LIQ04_ITEMS_IG", "oi_item_ig", "c_item_ig", objInterfaz.Item, string.Empty, false);
					if (string.IsNullOrEmpty(oi_idig))
					{
						objBatch.Err(string.Format("Item de deducción no encontrado '{0}'", objInterfaz.Item));
						continue;
					}
					TiposDeduccion.Add(objInterfaz.Item, int.Parse(oi_idig));
				}

				ITEM_IG item = ITEM_IG.Get(TiposDeduccion[objInterfaz.Item]);
				bool save = false;
				try
				{
					// Actualiza cada mes
					for (int i = 1; i <= 12; i++)
					{
						double importe = 0;
						string val = objInterfaz.GetType().GetProperty("m_" + i.ToString()).GetValue(objInterfaz, null).ToString();
						if (string.IsNullOrEmpty(val)) val = "0";
						if (!double.TryParse(val, out importe))
						{
							objBatch.Err(string.Format("Valor numérico inválido Año '{0}', Item '{1}', Mes '{2}'", objInterfaz.Anio, objInterfaz.Item, i.ToString()));
							continue;
						}
						IDIG idig = FindIDIG(item, objInterfaz.Anio, i);
						if (idig == null)
						{
							idig = new IDIG();
							idig.e_anio = objInterfaz.Anio;
							idig.e_mes = i;
							item.IDIG.Add(idig);
						}
						idig.n_importe = importe;
						save = true;
					}
					if (save)
					{
						// Guarda el año completo
						NomadEnvironment.GetCurrentTransaction().Save(item);
						inserts++;
					}
				}
				catch (Exception e)
				{
					objBatch.Err(string.Format("Error actualizando valores para Año '{0}', Item '{1}'. \r\n{2}\r\n{3}", objInterfaz.Anio, objInterfaz.Item, e.Message, e.StackTrace));
				}
				objBatch.SetPro(10, 80, RowsCount, intCurrentItem);
			}

			objBatch.SetPro(100);
			objBatch.Log("Registros actualizados: " + inserts.ToString());
			objBatch.Log("La importación finalizó.");
		}

		protected static IDIG FindIDIG(ITEM_IG item, int anio, int mes)
		{
			foreach (IDIG idig in item.IDIG)
			{
				if (idig.e_anio == anio && idig.e_mes == mes)
					return idig;
			}
			return null;
		}
	}
}
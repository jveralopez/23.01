using System;
using System.Collections;
using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using NucleusRH.Base.Liquidacion.Rangos_Porcentajes_IG;

namespace NucleusRH.Base.Liquidacion.InterfaceRangosIG
{
	public partial class ENTRADA
	{
		/// <summary>
		/// Importa los Rangos del impuesto a las ganancias
		/// </summary>
		public static void Importar(int e_anio)
		{
			int RowsCount;
			int intCurrentItem;
			int inserts = 0;

			NomadBatch objBatch = NomadBatch.GetBatch("Importar Rangos IG", "Importar Rangos IG");
			NomadBatch.Trace("Comienza Importar Rangos de Ganancias ------------------------------------ ");

			ENTRADA objInterfaz;

			objBatch.SetMess("Importando los Rangos IG");

			//Obtiene la lista de OIs de la interface de Items IG para LegajosLiquidación
			NomadXML RowIDs = NomadEnvironment.QueryNomadXML(ENTRADA.Resources.QRY_OIArchivo, "").FirstChild();

			// Inicia transaccion, borra todos los rangos del anio y carga los nuevos.
			// Si falla en algún paso hace rollback
			NomadEnvironment.GetCurrentTransaction().Begin();

			try
			{
				// Elimina todos los rangos del año
				Hashtable ids_delete = NomadEnvironment.QueryHashtable(ENTRADA.Resources.qry_rangos_anio, string.Format("<DATA e_anio = \"{0}\" />", e_anio.ToString()), "id");
				ICollection list_delete = NomadEnvironment.GetObjects(ids_delete.Keys, new IG().GetType());
				NomadEnvironment.GetCurrentTransaction().Delete(list_delete);


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

					try
					{
						IG ig = new IG();
						ig.e_anio = e_anio;
						ig.e_mes = objInterfaz.Mes;
						ig.n_tot_imp_d = objInterfaz.ImporteDesde;
						ig.n_tot_imp_h = objInterfaz.ImporteHasta;
						ig.n_cuota = objInterfaz.CuotaFija;
						ig.n_p_rango = objInterfaz.Porcentaje;

						NomadEnvironment.GetCurrentTransaction().Save(ig);
						inserts++;
					}
					catch (Exception e)
					{
						objBatch.Err(string.Format("Error actualizando valor para Mes '{0}', Rango '{1}' - '{2}'. \r\n{3}\r\n{4}", objInterfaz.Mes, objInterfaz.ImporteDesde.ToString("########.00"), objInterfaz.ImporteHasta.ToString("########.00"), e.Message, e.StackTrace));
						throw new NomadAppException();
					}
					objBatch.SetPro(10, 80, RowsCount, intCurrentItem);
				}

				NomadEnvironment.GetCurrentTransaction().Commit();
			}
			catch (Exception e)
			{
				if (!(e is NomadAppException))
					objBatch.Err("Ocurrio un error importando los registros. " + e.Message + e.StackTrace);
				NomadEnvironment.GetCurrentTransaction().Rollback();
				inserts = 0;
			}			

			objBatch.SetPro(100);
			objBatch.Log("Registros actualizados: " + inserts.ToString());
			objBatch.Log("La importación finalizó.");
		}
	}
}
using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Presupuesto.EstructuraReportes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Estructura del reporte
    public partial class EST_REPORTE 
    {
		
		public static void ReportePorEstructura(NomadXML xmlParam)
		{
			NomadBatch objBatch = NomadBatch.GetBatch("Iniciando...", "Ejecución del Reporte");

			NomadXML xmlReporte = NomadEnvironment.QueryNomadXML(Resources.QRY_DEF_REP, xmlParam.ToString());
			NomadXML xmlRepData = NomadEnvironment.QueryNomadXML(Resources.QRY_DATA_REP, xmlParam.ToString());
			
			bool sinDetalle = false;
			if (xmlParam.FirstChild().GetAttr("sinDetalle") == "1")
				sinDetalle = true;
			
			SubNodo(ref xmlRepData, sinDetalle);
			
			xmlReporte.FirstChild().AddTailElement(xmlRepData);
			
			//Generando Reporte
			string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
			string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";

			Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Presupuesto.ReportePorEstructura.rpt", xmlReporte);
			nmdHtml.GenerateHTML(outFilePath + "\\" + outFileName, System.Text.Encoding.UTF8);
			
		}

		public static myNODO SubNodo(ref NomadXML xmlNodo, bool sinDetalle)
		{
			//Si el nodo no tiene hijos devolver nulo
			if (xmlNodo.FirstChild() == null)
			{
				return null;				
			}
			else
			{
				if (xmlNodo.FirstChild().Name != "ITEM" && xmlNodo.FirstChild().Name != "NODO")
				{
					NomadXML xmlSubNodo = xmlNodo.FirstChild();
					return SubNodo(ref xmlSubNodo, sinDetalle);
				}
				else
				{
					myNODO objNodo = new myNODO();
				
					if (xmlNodo.FirstChild().Name == "ITEM")
					{
						//Obtener los items del nodo y agregarlos al XML
						objNodo.items = Items(ref xmlNodo, sinDetalle);
					}
					else					
					{
						//Recorrer nodos hijos
						for (NomadXML xmlSubNodo = xmlNodo.FirstChild(); xmlSubNodo != null; xmlSubNodo = xmlSubNodo.Next())
						{
							myNODO objSubNodo = SubNodo(ref xmlSubNodo, sinDetalle);

							if (objSubNodo != null)
							{
								foreach (string keyItem in objSubNodo.items.Keys)
								{
									myITEM objItemSubNodo = (myITEM)objSubNodo.items[keyItem];
									
									if (objNodo.items.ContainsKey(keyItem))
									{
										myITEM objItemNodo = (myITEM)objNodo.items[keyItem];
										
										foreach (string keyPer in objItemSubNodo.totalesPer.Keys)
										{
											double totPer = (double)objItemSubNodo.totalesPer[keyPer];
											
											if (objItemNodo.totalesPer.ContainsKey(keyPer))
											{
												objItemNodo.totalesPer[keyPer] = (double)objItemNodo.totalesPer[keyPer] + totPer;
											}
											else
											{
												objItemNodo.totalesPer.Add(keyPer, totPer);
											}
										}
									}
									else
									{
										objNodo.items.Add(keyItem, objItemSubNodo);
									}
								}								
							}
							else
							{
								//Borrar el nodo del XML
								xmlNodo.DeleteChild(xmlSubNodo);
							}
						}
						
						//Agregar al XML los items del nodo
						if (!sinDetalle)						
						{
							foreach (string keyItem in objNodo.items.Keys)
							{
								myITEM objItem = (myITEM)objNodo.items[keyItem];
								
								NomadXML xmlItem = new NomadXML("ITEM");
								xmlItem.SetAttr("d_clase", objItem.d_clase);
								xmlItem.SetAttr("d_tipo", objItem.d_tipo);
								xmlItem.SetAttr("d_item", objItem.d_item);
								xmlItem.SetAttr("totalItem", objItem.total);
								
								foreach (string keyPer in objItem.totalesPer.Keys)
								{
									double totPer = (double)objItem.totalesPer[keyPer];
									xmlItem.SetAttr(keyPer, totPer);
								}
								
								xmlNodo.AddTailElement(xmlItem);
							}
						}
					}
				
					//Si el nodo no tiene items devolver nulo
					if (objNodo.items.Count == 0)
					{
						return null;
					}
					
					//Construir los totales por periodo acumulados del nodo
					foreach (string keyItem in objNodo.items.Keys)
					{
						myITEM objItem = (myITEM)objNodo.items[keyItem];

						foreach (string keyPer in objItem.totalesPer.Keys)
						{
							double totPer = (double)objItem.totalesPer[keyPer];
							
							if (objNodo.totalesPer.ContainsKey(keyPer))
							{
								objNodo.totalesPer[keyPer] = (double)objNodo.totalesPer[keyPer] + totPer;
							}
							else
							{
								objNodo.totalesPer.Add(keyPer, totPer);
							}
							
							objNodo.total += totPer;
						}
					}

					//Agregar al XML los totales por periodo del nodo
					foreach (string keyPer in objNodo.totalesPer.Keys)
					{
						double totPer = (double)objNodo.totalesPer[keyPer];
						xmlNodo.SetAttr(keyPer, totPer);
					}
					
					//Agregar al XML el total del nodo
					xmlNodo.SetAttr("totalNodo", objNodo.total);
					
					Agrupadores(objNodo.items, ref xmlNodo, sinDetalle);
					
					return objNodo;
				}
			}
		}

		public static SortedList Items(ref NomadXML xmlNodo, bool sinDetalle)
		{
			SortedList listItems = new SortedList();
			
			for (NomadXML xmlItem = xmlNodo.FirstChild(); xmlItem != null; xmlItem = xmlItem.Next())
			{
				//Si el item no tiene periodos, borrarlo del XML y pasar al siguiente item
				if (xmlItem.FirstChild() == null)
				{
					xmlNodo.DeleteChild(xmlItem);
					continue;
				}
				else
				{
					myITEM objItem = new myITEM();
					
					objItem.clase = xmlItem.GetAttr("c_clase");
					objItem.tipo = xmlItem.GetAttr("c_tipo");
					objItem.item = xmlItem.GetAttr("e_item");
					
					objItem.d_clase = xmlItem.GetAttr("d_clase");
					objItem.d_tipo = xmlItem.GetAttr("d_tipo");
					objItem.d_item = xmlItem.GetAttr("d_item");
					
					for (NomadXML xmlPeriodo = xmlItem.FirstChild(); xmlPeriodo != null; xmlPeriodo = xmlPeriodo.Next())
					{
						string keyPer = xmlPeriodo.GetAttr("per");
						double totPer = StringUtil.str2dbl(xmlPeriodo.GetAttr("val"));
						
						xmlItem.SetAttr(keyPer, totPer);
					
						if (objItem.totalesPer.ContainsKey(keyPer))
						{
							objItem.totalesPer[keyPer] = (double)objItem.totalesPer[keyPer] + totPer;
						}
						else
						{
							objItem.totalesPer.Add(keyPer, totPer);
						}
						
						objItem.total += totPer;					
					}
					
					//Si el total del item es 0 borrarlo del XML
					if (objItem.total == 0)
					{
						xmlNodo.DeleteChild(xmlItem);
					}
					else
					{
						if (sinDetalle)
						{
							xmlNodo.DeleteChild(xmlItem);
						}
						else
						{
							xmlItem.SetAttr("totalItem", objItem.total);
						}
						
						listItems.Add(objItem.clase+";"+objItem.tipo+";"+objItem.item, objItem);
					}
				}
			}
			
			return listItems;
		}
		
		public static void Agrupadores(SortedList listItems, ref NomadXML xmlNodo, bool sinDetalle)
		{
			SortedList listTotalTipo = new SortedList();
			SortedList listTotalClase = new SortedList();
			myITEM objItem, objTipo, objClase;
			string key;
			double totPer;
			
			foreach (string keyItem in listItems.Keys)
			{
				objItem = (myITEM)listItems[keyItem];				
				
				if (!sinDetalle)
				{
					//Totales por Tipo
					key = objItem.clase+";"+objItem.tipo;
					if (listTotalTipo.ContainsKey(key))
					{
						objTipo = (myITEM)listTotalTipo[key];					
						foreach (string keyPer in objItem.totalesPer.Keys)
						{
							totPer = (double)objItem.totalesPer[keyPer];						
							if (objTipo.totalesPer.ContainsKey(keyPer))
								objTipo.totalesPer[keyPer] = (double)objTipo.totalesPer[keyPer] + totPer;
							else
								objTipo.totalesPer.Add(keyPer, totPer);						
							objTipo.total += totPer;
						}
					}
					else
					{
						objTipo = new myITEM();
						objTipo.d_clase = objItem.d_clase;
						objTipo.d_tipo = objItem.d_tipo;
						foreach (string keyPer in objItem.totalesPer.Keys)
						{
							totPer = (double)objItem.totalesPer[keyPer];
							objTipo.totalesPer.Add(keyPer, totPer);
							objTipo.total += totPer;
						}
						listTotalTipo.Add(key, objTipo);
					}
				}
				
				//Totales por Clase
				key = objItem.clase;
				if (listTotalClase.ContainsKey(key))
				{
					objClase = (myITEM)listTotalClase[key];					
					foreach (string keyPer in objItem.totalesPer.Keys)
					{
						totPer = (double)objItem.totalesPer[keyPer];
						if (objClase.totalesPer.ContainsKey(keyPer))
							objClase.totalesPer[keyPer] = (double)objClase.totalesPer[keyPer] + totPer;
						else
							objClase.totalesPer.Add(keyPer, totPer);
						objClase.total += totPer;
					}
				}
				else
				{
					objClase = new myITEM();
					objClase.d_clase = objItem.d_clase;
					foreach (string keyPer in objItem.totalesPer.Keys)
					{
						totPer = (double)objItem.totalesPer[keyPer];
						objClase.totalesPer.Add(keyPer, totPer);
						objClase.total += totPer;
					}
					listTotalClase.Add(key, objClase);
				}
			}
			
			if (!sinDetalle)
			{
				//Recorrer los totales por Tipo y agregar al XML
				foreach (string keyTipo in listTotalTipo.Keys)
				{
					objTipo = (myITEM)listTotalTipo[keyTipo];				
					NomadXML xmlTipo = new NomadXML("TIPO");
					xmlTipo.SetAttr("d_clase", objTipo.d_clase);
					xmlTipo.SetAttr("d_tipo", objTipo.d_tipo);
					
					foreach (string keyPer in objTipo.totalesPer.Keys)		
					{
						double valor = (double)objTipo.totalesPer[keyPer];
						xmlTipo.SetAttr(keyPer, valor);
					}
					
					xmlTipo.SetAttr("totalTipo", objTipo.total);
					xmlNodo.AddTailElement(xmlTipo);
				}
			}
			
			//Recorrer los totales por Clase y agregar al XML
			foreach (string keyClase in listTotalClase.Keys)
			{
				objClase = (myITEM)listTotalClase[keyClase];				
				NomadXML xmlClase = new NomadXML("CLASE");
				xmlClase.SetAttr("d_clase", objClase.d_clase);
				
				foreach (string keyPer in objClase.totalesPer.Keys)		
				{
					double valor = (double)objClase.totalesPer[keyPer];
					xmlClase.SetAttr(keyPer, valor);
				}
				
				xmlClase.SetAttr("totalClase", objClase.total);
				xmlNodo.AddTailElement(xmlClase);
			}
		}
    }
	
	public class myNODO
	{
		public SortedList items;
		public Hashtable totalesPer;
		public double total;
		
		public myNODO()
		{
			items = new SortedList();
			totalesPer = new Hashtable();
			total = 0;
		}
	}

	public class myITEM
	{
		public string clase;
		public string tipo;
		public string item;
		public string d_clase;
		public string d_tipo;
		public string d_item;
		public Hashtable totalesPer;
		public double total;
		
		public myITEM()
		{
			totalesPer = new Hashtable();
			total = 0;
		}
	}	
}
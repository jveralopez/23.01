using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using NucleusRH.Base.Liquidacion.Legajo_Liquidacion;

namespace NucleusRH.Base.Liquidacion.LibroSueldoDigital
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Anticipos
    public partial class LSD
    {
        public static void ProcesarInterfaceLSD(string interfaceTIPO, string interdaceID, Nomad.NSystem.Document.NmdXmlDocument parametros)
        {
            NomadBatch MyBatch = NomadBatch.GetBatch("Interfaz Libro de Sueldo Digital", "Interfaz Libro de Sueldo Digital");
						
			string strTemp;
			string strDefaultJuris;
            NomadXML xmlParametros;
			NomadXML xmlInterParam;
			NomadXML xmlJurisParam;
			bool bolCentralizado;
			string strFileName;

			//Recupera los parámetros del método y los conviente en un XML
			xmlParametros = new NomadXML(parametros.ToString());
			xmlParametros = xmlParametros.FirstChild();

			//Valida que exista un parámetro en la ORG26_PARAMETROS y que esté cargado en el formulario de parámtros por empresa.
			{
				NomadXML xmlResultParam;
				
				xmlJurisParam = new NomadXML("PARAM");
				xmlJurisParam.SetAttr("oi_empresa", xmlParametros.GetAttr("oi_empresa"));
				xmlJurisParam.SetAttr("c_parametro", "JurisCentralizado");
				
				MyBatch.Log("Validando los parámetros necesarios.");
				xmlResultParam = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA.Resources.QRY_PARAMETRO_POR_EMPRESA, xmlJurisParam.ToString()).FirstChild();
				strDefaultJuris = xmlResultParam.GetAttr("valor");
				
				//Si viene un valor en el parámetro "JurisCentralizado" significa que la jurisdicción se debe generar centralizada y se pone como jurisdicción la del parámetro.
				//Ese código de jurisdicción debe ser de una juris_afip existente.
				if(strDefaultJuris == "") {
					MyBatch.Log("Los archivos LSD a generar serán con jurisdicción DESCENTRALIZADA.");
					bolCentralizado = false;
				} else {
					//Valida que exista el código de jurisdicción cargado en el parámetro
					strTemp = NomadEnvironment.QueryValue("ORG36_JURIS_AFIP", "c_juris_afip", "c_juris_afip", strDefaultJuris, "", false);
					if(strTemp == "") {
						throw new NomadAppException("El código de jurisdicción AFIP '" + strDefaultJuris + "', cargado en el parámetro 'JurisCentralizado' para la empresa seleccionada, no existe en el ABM de Jurisdicciones AFIP.");
					}
					
					MyBatch.Log("Los archivos LSD a generar serán con jurisdicción CENTRALIZADA.");
					bolCentralizado = true;
				}
			}

			//Recupera las liquidaciones a procesar
			xmlJurisParam = new NomadXML("PARAM");
			xmlJurisParam.SetAttr("oi_empresa", xmlParametros.GetAttr("oi_empresa"));
			xmlJurisParam.SetAttr("e_periodo", xmlParametros.GetAttr("e_periodo"));
			xmlJurisParam.SetAttr("oi_tipo_liq", "7");
			xmlJurisParam.SetAttr("c_juris_afip", strDefaultJuris);
			xmlJurisParam.SetAttr("centralizado", bolCentralizado ? "1" : "0");
			
			NomadXML xmlLiquidacion = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.LibroSueldoDigital.LSD.Resources.INT_LIQUIDACION_LSD, xmlJurisParam.ToString()).FirstChild();
			
            if(xmlLiquidacion != null) {
                int intOILiquidacion;
				int nroLiquidacion = 0;
				string strCodJuris;
				int intCantXJuris = 0;
				string strCLiquidacion;

				NomadXML xmlFiles;
				NomadXML xmlFile;
				
				xmlFiles = new NomadXML("FILES");
				
                for (NomadXML MyCUR = xmlLiquidacion.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next()) {
                    
                    nroLiquidacion++;

					intOILiquidacion = MyCUR.GetAttrInt("oi_liquidacion");
					intCantXJuris = MyCUR.GetAttrInt("cantidad");
					strCLiquidacion = MyCUR.GetAttr("c_liquidacion");

					MyBatch.Log("Analizando liquidación '" + strCLiquidacion + "'.");

					strCodJuris = MyCUR.GetAttr("c_juris_afip");
					
					if(strCodJuris == "") {
						throw new NomadAppException("Existen ubicaciones de la empresa que no tienen jurisdicción asignada. Por favor revise las jurisdicciones.");
					}
					
					xmlInterParam = new NomadXML("DATA");
					xmlInterParam.SetAttr("oi_liquidacion", intOILiquidacion);
                    xmlInterParam.SetAttr("nroLiquidacion", nroLiquidacion);
					xmlInterParam.SetAttr("c_juris_afip", strCodJuris);
					xmlInterParam.SetAttr("oi_empresa", xmlParametros.GetAttr("oi_empresa"));
					xmlInterParam.SetAttr("centralizado", bolCentralizado ? "1" : "0");
                    NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.InterfaceSalida(interfaceTIPO, interdaceID, new NmdXmlDocument(xmlInterParam.ToString()));

                    string archivoOrigen = NomadProxy.GetProxy().RunPath+"Nomad\\INTERFACES\\"+ NomadProxy.GetProxy().Batch().ID + ".txt";

					strCLiquidacion = LimpiarCodigo(strCLiquidacion);
					strFileName = nroLiquidacion + "_" + strCLiquidacion + "_" + strCodJuris + "_" + NomadProxy.GetProxy().UserEtty + ".txt";
					
                    string archivoDestino = NomadProxy.GetProxy().RunPath + "Nomad\\INTERFACES\\" + strFileName;

                    if (File.Exists(archivoDestino))
                        File.Delete(archivoDestino);

                    File.Move(archivoOrigen, archivoDestino);
					
					//a lo mejor se puede generar un XML con la información necesaria para mostrar en la lista del fomulario
					xmlFile = xmlFiles.AddTailElement("FILE");
					xmlFile.SetAttr("id", nroLiquidacion);
					xmlFile.SetAttr("c_empresa", MyCUR.GetAttr("c_empresa"));
					xmlFile.SetAttr("c_liquidacion", strCLiquidacion);
					xmlFile.SetAttr("d_titulo", MyCUR.GetAttr("d_titulo"));
					xmlFile.SetAttr("e_periodo", xmlParametros.GetAttr("e_periodo"));
					xmlFile.SetAttr("n_legajos", intCantXJuris);
					xmlFile.SetAttr("n_liq", nroLiquidacion);
					xmlFile.SetAttr("c_estado", MyCUR.GetAttr("c_estado"));
					xmlFile.SetAttr("c_liq_genera", MyCUR.GetAttr("c_liq_genera"));
					xmlFile.SetAttr("oi_liq_genera",  MyCUR.GetAttr("oi_liq_genera"));
					xmlFile.SetAttr("c_juris_afip",  MyCUR.GetAttr("c_juris_afip"));
					xmlFile.SetAttr("d_juris_afip",  MyCUR.GetAttr("d_juris_afip"));
					xmlFile.SetAttr("juris_afip",  MyCUR.GetAttr("c_juris_afip") + " - " + MyCUR.GetAttr("d_juris_afip"));
					xmlFile.SetAttr("file_name",  strFileName);
					
                }

				if (nroLiquidacion > 0) {
					//Guarda el XML de detalle de los arhivos generados
					NomadProxy.GetProxy().FileServiceIO().SaveFile("INTERFACES", "LSD_LISTA_"+NomadProxy.GetProxy().Batch().ID+".xml", xmlFiles.ToString());
				}
					
            }

        }

        private static string LimpiarCodigo(string codLiquidacion)
        {
            char[] caracteresInvalidos = {'\\','/',':','*','?','"','<','>','|'} ;

            foreach(char c in caracteresInvalidos)
            {
                if (codLiquidacion.Contains(c.ToString()))
                    codLiquidacion = codLiquidacion.Replace(c, '_');
            }

            return codLiquidacion;
        }
    }
}



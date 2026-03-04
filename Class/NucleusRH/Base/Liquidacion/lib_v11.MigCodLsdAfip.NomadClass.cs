using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using NucleusRH.Base.Liquidacion.EquivalenciaLSD;
using NucleusRH.Base.Liquidacion.SubgrupoLSD;

namespace NucleusRH.Base.Liquidacion.MigCodLsdAfip
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Conceptos
    public partial class EQUIVALENCIA : Nomad.NSystem.Base.NomadObject
    {

        public static void ImportarCodLsdAfip()
        {
            //Codigo en .NET
            NomadBatch b = NomadBatch.GetBatch("Importar Código LSD AFIP", "Importar Código LSD AFIP");

            int linea, totLineas, codLSD, totErr=0, totReg=0, totDup=0;
			ArrayList codConceptos = new ArrayList();
            NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO ddoConcepto;
			NucleusRH.Base.Liquidacion.SubgrupoLSD.SUBGRUPO ddoSubgrupo;
			NucleusRH.Base.Liquidacion.MigCodLsdAfip.EQUIVALENCIA inEquivalencia;

            //Cargando el Query.
            NomadBatch.Trace("Cargando el Query...");
            string MySTR = NomadProxy.GetProxy().SQLService().Get(EQUIVALENCIA.Resources.QRY_REGISTROS, "");
            NomadXML MyXML = new NomadXML();
            NomadXML MyROW;

            //Contando la Cantidad de ROWS
            MyXML.SetText(MySTR);
            totLineas = MyXML.FirstChild().ChildLength;
			
			for (linea = 1, MyROW = MyXML.FirstChild().FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
            {
				b.SetPro(0, 90, totLineas, linea);
                b.SetMess("Procesando la Linea " + linea + " de " + totLineas);
				
				inEquivalencia = EQUIVALENCIA.Get(MyROW.GetAttr("id"));
				
				if(inEquivalencia.c_concepto.ToLower().StartsWith("cod") || inEquivalencia.c_concepto.ToLower().StartsWith("cód"))
					continue;
				
				try
				{
					string oi_concepto = NomadEnvironment.QueryValue("LIQ14_CONCEPTOS", "oi_concepto", "c_concepto", inEquivalencia.c_concepto, "", true);
					
					if(codConceptos.Count == 0)
						codConceptos.Add(inEquivalencia.c_concepto);
					else
					{
						if(codConceptos.Contains(inEquivalencia.c_concepto))
						{
							b.Err("Código de concepto duplicado '" + inEquivalencia.c_concepto + "' - Se rechaza registro. Cod LSD '"+inEquivalencia.c_lsd+"'- Linea " + linea.ToString());
							totDup++;
							continue;
						}
						codConceptos.Add(inEquivalencia.c_concepto);
					}
					
					
					EquivalenciaLSD.EQUIVALENCIA equivalencia = null;
					string oi_equivalencia = NomadEnvironment.QueryValue("LIQ14_EQUIVALENCIA", "oi_equivalencia", "oi_concepto", oi_concepto, "", false);
					if (oi_equivalencia != "")                          
					{
						equivalencia = EquivalenciaLSD.EQUIVALENCIA.Get(oi_equivalencia);
					}
					else
					{
						equivalencia = new EquivalenciaLSD.EQUIVALENCIA();
						equivalencia.c_equivalencia = "";
					}

					equivalencia.oi_concepto = oi_concepto;
					equivalencia.l_informa = false;
					if(!int.TryParse(inEquivalencia.c_lsd, out codLSD))
					{	
						totErr++;
						b.Err("Se rechaza registro. '"+ inEquivalencia.c_lsd + "' - No es un código LSD válido - Linea " + linea.ToString());
						continue;
					}
					else 
					{	
						string paramXML = "<PARAM codLSD='"+codLSD+"' />";
						NomadXML valCodXML = NomadEnvironment.QueryNomadXML(SubgrupoLSD.SUBGRUPO.Resources.QRY_Validar_Entre_Rangos, paramXML, false);
						
						if(valCodXML.FirstChild().FindElement("SUBGRUPO") != null)
							equivalencia.e_lsd = codLSD;
						else
						{
							totErr++;
							b.Err("Se rechaza registro. Código LSD '"+ inEquivalencia.c_lsd + 
									"' - No se encuentra en los registros de los rangos de códigos inicial/final. - Linea " + linea.ToString());
							continue;
						}
					}

					NomadEnvironment.GetCurrentTransaction().Save(equivalencia);
					totReg++;
				}
				catch(Exception e)
				{
					totErr++;
					if(e.Message.StartsWith("Key cannot be null"))
						b.Err("Se rechaza registro. Código Concepto inexistente '"+inEquivalencia.c_concepto+"' - código lsd '"+inEquivalencia.c_lsd+"' - Linea " + linea.ToString());
					else
						b.Err("Error desconocido. Message " + e.Message + " - Linea " + linea.ToString());
				}
				
			}
			
			b.Log("Se agregaron " + totReg.ToString() + " registros.");
			if (totDup > 0) b.Wrn("Se encontraron " + totDup.ToString() + " Fichadas Duplicadas.");
			if (totErr > 0) b.Wrn("Se rechazaron " + totErr.ToString() + " Registros.");
        }
    }
}



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

namespace NucleusRH.Base.Tiempos_Trabajados.Fichadas_NI
{    
    //Clase Fichadas_NI
    public partial class Fichadas_NI : Nomad.NSystem.Base.NomadObject
    {
        public static void ProcesarFichadas()
        {            
            NomadBatch batch = NomadBatch.GetBatch("Importar Fichadas", "Importar Fichadas");
            string step = "";
			string mensaje = "";
			int oi_personal;
			int totFic = 0; //Total de nuevas fichadas
            int nroFic = 0; //Numero de fichada en proceso
			int ficAdd = 0; //Contador de fichadas incorporadas
			int ficRec = 0; //Contador de fichadas rechazadas por error
			int ficErr = 0; //Contador de fichadas con error desconocido
			int ficDup = 0; //Contador de fichadas duplicadas			
            
			NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichadaIng;
            NucleusRH.Base.Tiempos_Trabajados.Fichadas_NI.Fichadas_NI myFichadaNI;
            NomadXML fichadas, fichada;
			
			try
			{			
				//Buscando nuevas fichadas
				batch.SetMess("Iniciando proceso...");				
				fichadas = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Fichadas_NI.Fichadas_NI.Resources.qry_fichadas, "");
			}
			catch (Exception)
			{
				batch.SetPro(100);
				batch.Err("Error al intentar leer la tabla TTA99_Fichadas_NI");
				return;
			}

            //Contando la cantidad de nuevas fichadas
            totFic = fichadas.FirstChild().ChildLength;
            if (totFic == 0)
            {
				batch.SetPro(100);
                batch.Wrn("No se encontraron nuevas fichadas para procesar");
				return;
            }
			
			//Recorriendo las nuevas fichadas
			batch.SetMess("Incorporando fichadas...");
			batch.Log("Incorporando fichadas...");

			for (nroFic = 1, fichada = fichadas.FirstChild().FirstChild(); fichada != null; nroFic++, fichada = fichada.Next())
			{
				batch.SetPro(0, 90, totFic, nroFic);
				batch.SetMess("Procesando fichada " + nroFic.ToString() + " de " + totFic.ToString());
				try
				{
					//+++++++++++++++++++++++
					step = "Creando fichada";
					//+++++++++++++++++++++++
					ddoFichadaIng = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();
					ddoFichadaIng.c_origen = "T";
					ddoFichadaIng.c_estado = "P";

					//+++++++++++++++++++++++++
					step = "Leyendo interface";
					//+++++++++++++++++++++++++
					myFichadaNI = Fichadas_NI.Get(fichada.GetAttr("ID"));                    
					
					//++++++++++++++++++++++++++
					step = "Asignando personal";
					//++++++++++++++++++++++++++
					oi_personal = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.GetPersonalEmpresaID(myFichadaNI.legajoReloj.ToString());
					if (oi_personal == 0)
					{
						ficRec++;
						mensaje = "No se encuentra el legajo reloj " + myFichadaNI.legajoReloj;
						batch.Err(mensaje + " procesando fichada " + nroFic.ToString());
						myFichadaNI.leidoNucleus = "E";
						myFichadaNI.observaciones = mensaje;
						NomadEnvironment.GetCurrentTransaction().Save(myFichadaNI);
						continue;
					}
					ddoFichadaIng.oi_personal_emp = oi_personal.ToString();
					ddoFichadaIng.e_numero_legajo = myFichadaNI.legajoReloj;
					
					//++++++++++++++++++++++++++
					step = "Asignando terminal";
					//++++++++++++++++++++++++++
					NomadXML param = new NomadXML("PARAM");
					param.SetAttr("c_terminal", myFichadaNI.terminal);
					NomadXML resultado = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Fichadas_NI.Fichadas_NI.Resources.qry_terminal, param.ToString());
					if (resultado.FirstChild().ChildLength == 0)
					{
						ficRec++;
						mensaje = "No se encuentra la terminal '" + myFichadaNI.terminal + "'";
						batch.Err(mensaje + " procesando fichada " + nroFic.ToString());
						myFichadaNI.leidoNucleus = "E";
						myFichadaNI.observaciones = mensaje;
						NomadEnvironment.GetCurrentTransaction().Save(myFichadaNI);
						continue;
					}
					else
					{
						ddoFichadaIng.oi_terminal = resultado.FirstChild().FirstChild().GetAttr("oi_terminal");
					}

					//++++++++++++++++++++++++++++
					step = "Asignando fecha/hora";
					//++++++++++++++++++++++++++++
					ddoFichadaIng.f_fechahora = myFichadaNI.fechaHora;
					ddoFichadaIng.c_fichadasing = ddoFichadaIng.f_fechahora.ToString("yyyyMMddHHmmss") + ddoFichadaIng.e_numero_legajo.ToString();
					
					//++++++++++++++++++++++++
					step = "Asignando evento";
					//++++++++++++++++++++++++
					if (myFichadaNI.tipoEvento == "E")
					{
						ddoFichadaIng.l_entrada = true;
						ddoFichadaIng.c_fichadasing += "AE";
						ddoFichadaIng.c_tipo = "E";
					}
					else
					{
						if(myFichadaNI.tipoEvento == "S")
						{
							ddoFichadaIng.l_entrada = false;
							ddoFichadaIng.c_fichadasing += "AS";
							ddoFichadaIng.c_tipo = "S";
						}
						else
						{
						   ficRec++;
						   mensaje = "No se reconoce el tipo de evento '" + myFichadaNI.tipoEvento + "'";
						   batch.Err(mensaje + " procesando fichada " + nroFic.ToString());
						   myFichadaNI.leidoNucleus = "E";
						   myFichadaNI.observaciones = mensaje;
						   NomadEnvironment.GetCurrentTransaction().Save(myFichadaNI);
						   continue;
						}
					}

					//++++++++++++++++++++++++++++++
					step = "Verificando duplicidad";
					//++++++++++++++++++++++++++++++
					if (NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.FinchadaExist(ddoFichadaIng.c_fichadasing))
					{
						ficDup++;						
						myFichadaNI.leidoNucleus = "E";
						myFichadaNI.observaciones = "La fichada ya se encuentra incorporada en el sistema";
						NomadEnvironment.GetCurrentTransaction().Save(myFichadaNI);
						continue;
					}

					//+++++++++++++++++++++++++
					step = "Guardando fichada";
					//+++++++++++++++++++++++++
					NomadEnvironment.GetCurrentTransaction().Save(ddoFichadaIng);
					ficAdd++;
					myFichadaNI.leidoNucleus = "I";
					NomadEnvironment.GetCurrentTransaction().Save(myFichadaNI);
				}
				catch (Exception e)
				{
					ficErr++;
					batch.Err("Error desconocido procesando fichada " + nroFic.ToString() + " - Detalle: " + e.Message);
				}
			}

			//Resumen final
			batch.SetPro(100);			
			if (ficAdd == 0) batch.Log("No se incorporaron fichadas");
			else if (ficAdd == 1) batch.Log("Se incorporó 1 fichada");
			else if (ficAdd > 1) batch.Log("Se incorporaron " + ficAdd.ToString() + " fichadas");
			if (ficRec == 1) batch.Log("Se rechazó 1 fichada por error");
			else if (ficRec > 1) batch.Log("Se rechazaron " + ficRec.ToString() + " fichadas por error");
			if (ficDup == 1) batch.Log("Se encontró 1 fichada duplicada");
			else if (ficDup > 1) batch.Log("Se encontraron " + ficDup.ToString() + " fichadas duplicadas");
			if (ficErr == 1) batch.Log("Se encontró 1 fichada con error desconocido");
			else if (ficErr > 1) batch.Log("Se encontraron " + ficErr.ToString() + " fichadas con error desconocido");
        }
    }
}
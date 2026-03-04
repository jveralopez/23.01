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

namespace NucleusRH.Base.SeleccionDePostulantes.MigPuestosExp
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase MigPuestosExp
    public partial class PUESTO_EXP: Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarPuestosExp()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Puestos de Experiencia");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.SeleccionDePostulantes.MigPuestosExp.PUESTO_EXP objRead;            

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));

            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");
            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {
                    objRead = NucleusRH.Base.SeleccionDePostulantes.MigPuestosExp.PUESTO_EXP.Get(row.GetAttr("id"));
                    //Me fijo si ya existe el Puesto de Experiencia
                    string oiVal = NomadEnvironment.QueryValue("SDP09_PUESTO_EXP", "oi_puesto_exp", "c_puesto_exp", objRead.c_puesto_exp, "", true);
                    
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Puesto de Experiencia '" + objRead.c_puesto_exp + " - " + objRead.d_puesto_exp + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {

                        //Creo el Puesto de Experiencia
                        NucleusRH.Base.SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP DDOPE;
                        DDOPE = new NucleusRH.Base.SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP();

                        if (objRead.c_puesto_exp == "" || objRead.d_puesto_exp == "")
                        {
                            objBatch.Err("El Código o la Descripción del Puesto de Experiencia no es válido, se rechaza el registro '" + objRead.c_puesto_exp + " - " + objRead.d_puesto_exp + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOPE.c_puesto_exp = objRead.c_puesto_exp;
                        DDOPE.d_puesto_exp = objRead.d_puesto_exp;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOPE);
                            NomadEnvironment.QueryValueChange("SDP09_PUESTO_EXP", "oi_puesto_exp", "c_puesto_exp", objRead.c_puesto_exp, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_puesto_exp + " - " + objRead.d_puesto_exp + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                            Errores++;
                        }
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }
    }
}


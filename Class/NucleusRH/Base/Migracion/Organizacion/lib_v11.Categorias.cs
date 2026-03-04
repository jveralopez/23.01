using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Categorias
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Categorías de Convenios
    public partial class CATEGORIA
    {
        public static void ImportarCategorias()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Categorías");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Categorias.CATEGORIA objRead;            

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
                    //Cargo el registro
                    objRead = NucleusRH.Base.Migracion.Organizacion.Categorias.CATEGORIA.Get(row.GetAttr("id"));

                    if (objRead.c_categoria == "" || objRead.d_categoria == "")
                    {
                        objBatch.Err("El Código o la Descripcion del Categoría no es válido, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.d_categoria + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Organizacion.Convenios.CONVENIO ddoCON = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetConvenio(objRead.c_convenio, htPARENTS);

                    if (ddoCON == null)
                    {
                        objBatch.Err("El Código del Convenio no existe, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.d_categoria + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Categoría en el Convenio  	
                    string oiVal = NomadEnvironment.QueryValue("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", objRead.c_categoria, "ORG18_CATEGORIAS.oi_convenio = " + ddoCON.Id, true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Categoria en el Convenio '" + objRead.c_categoria + " - " + objRead.d_categoria + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        try
                        {
                            //Creo la Categoría
                            NucleusRH.Base.Organizacion.Convenios.CATEGORIA DDOCAT = new NucleusRH.Base.Organizacion.Convenios.CATEGORIA();

                            DDOCAT.c_categoria = objRead.c_categoria;
                            DDOCAT.d_categoria = objRead.d_categoria;
                            DDOCAT.n_valor_hora = objRead.n_valor_hora;
                            if (objRead.n_valor_horaNull) DDOCAT.n_valor_horaNull = true;

                            DDOCAT.n_suplemento_1 = objRead.n_suplemento_1;
                            if (objRead.n_suplemento_1Null) DDOCAT.n_suplemento_1Null = true;
                            DDOCAT.n_suplemento_2 = objRead.n_suplemento_2;
                            if (objRead.n_suplemento_2Null) DDOCAT.n_suplemento_2Null = true;
                            DDOCAT.n_suplemento_3 = objRead.n_suplemento_3;
                            if (objRead.n_suplemento_3Null) DDOCAT.n_suplemento_3Null = true;
                            DDOCAT.n_suplemento_4 = objRead.n_suplemento_4;
                            if (objRead.n_suplemento_4Null) DDOCAT.n_suplemento_4Null = true;
                            DDOCAT.l_prom_auto = objRead.l_prom_auto;
                            DDOCAT.e_secuencia = objRead.e_secuencia;
                            if (objRead.e_secuenciaNull) DDOCAT.e_secuenciaNull = true;
                            DDOCAT.o_categoria = objRead.o_categoria;

                            ddoCON.CATEGORIAS.Add(DDOCAT);
                            NomadEnvironment.QueryValueChange("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", objRead.c_categoria, "ORG18_CATEGORIAS.oi_convenio = " + ddoCON.Id, "1",true);
                            
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al crear registro " + objRead.c_categoria + " - " + objRead.d_categoria + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

            try
            {
                NucleusRH.Base.Migracion.Interfaces.INTERFACE.Grabar(htPARENTS);
            }
            catch (Exception e)
            {
                objBatch.Err("Error al grabar - " + e.Message);
                Errores = Linea;
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");

        }
    }
}
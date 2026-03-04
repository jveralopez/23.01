using System;
using System.Xml;
using System.Collections;

using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;
using Nomad.NSystem.Base;

namespace NucleusRH.Base.RecordCard.AccionesReales 
{

    public partial class ACCION_REAL
    {
        

        static public void GuardarAccion(string accion, Nomad.NSystem.Document.NmdXmlDocument parametrosQry, string claseRC)
        {
            try
            {
                //string dllMAIN = this.GetType().Assembly.GetFiles()[0].Name;
                // Obtiene el oi_accion
                string oi_accion = NucleusRH.Base.RecordCard.Acciones.ACCION.getIDbyName(accion);
                NomadEnvironment.GetTrace().Debug("oi_accion " + oi_accion);
                NucleusRH.Base.RecordCard.Acciones.ACCION objAccion = NucleusRH.Base.RecordCard.Acciones.ACCION.Get(oi_accion);
                NomadEnvironment.GetTrace().Debug("objAccion " + objAccion.SerializeAll());
                // Crea el recordcard
                NomadEnvironment.GetTrace().Debug("Pre: GetType(" + claseRC + ")");
#if NOMAD_DEBUG 
                System.Type typeClass = System.Reflection.Assembly.LoadFile("E:\\Proyectos\\BIN\\BaytonRH.dll").GetType(claseRC);
#else
                System.Type typeClass = System.Reflection.Assembly.GetExecutingAssembly().GetType(claseRC);
#endif
                ////System.Type typeClass = System.Reflection.Assembly.Load("NucleusRHClassesV11").GetType(claseRC);
                NomadEnvironment.GetTrace().Debug("Post: GetType(" + claseRC + ")");
                // Castea el objeto al padre
                NomadEnvironment.GetTrace().Debug("Pre: InvokeMember");
                NucleusRH.Base.RecordCard.AccionesReales.ACCION_REAL objRecordCard = (NucleusRH.Base.RecordCard.AccionesReales.ACCION_REAL) typeClass.InvokeMember(null, System.Reflection.BindingFlags.CreateInstance, null, null, null);
                NomadEnvironment.GetTrace().Debug("Post: InvokeMember");

                // Obtiene el documento resultado del query específico para la clase de recordcard
                // Esto en realidad esta llamendo al método implementado en el child de la clase ACCION_REAL
                // parametrosQry se pasa a string, y no se definio así porque este método (GuardarAccion) es llamado desde un flow
                System.Xml.XmlDocument xmlDDO = objRecordCard.ObtieneDDORC(parametrosQry.DocumentToString());
                NomadEnvironment.GetTrace().Debug("xmlDDO: " + xmlDDO.OuterXml);
                // genera el evento
                NucleusRH.Base.RecordCard.Eventos.EVENTO objEvento = new NucleusRH.Base.RecordCard.Eventos.EVENTO();

                objEvento.oi_accion = oi_accion;
                objEvento.f_evento = DateTime.Now;

                objEvento.d_usuario = NomadProxy.GetProxy().UserName;
                //Borar las 3 lineas de abajo de lo comentado.
                //string descr = xmlDDO.SelectSingleNode("/object/no-object/@descr").Value;
                string descr = "";
                if (xmlDDO.SelectSingleNode("/object/no-object/@descr") != null)
                  descr = xmlDDO.SelectSingleNode("/object/no-object/@descr").Value;

                if(descr.Length > 1000)
                {
                    objEvento.d_evento = descr.Substring(0, 1000);
                }
                else
                {
                    objEvento.d_evento = descr;
                }

                foreach (NucleusRH.Base.RecordCard.Acciones.CLASE_ACC objClaseAcc in objAccion.CLASES_ACC)
                {
                    // genera el registro de las clases
                    NucleusRH.Base.RecordCard.Eventos.EVEN_CLASE objEventoClase = new NucleusRH.Base.RecordCard.Eventos.EVEN_CLASE();
                    NomadEnvironment.GetTrace().Debug("objClaseAcc " + objClaseAcc);
                    objEventoClase.oi_clase_acc = objClaseAcc.Id;
                    if (objClaseAcc.c_colID != "")
                    {
                        NomadEnvironment.GetTrace().Debug("objClaseAcc.c_colID " + objClaseAcc.c_colID);
                        if(xmlDDO.SelectSingleNode("/object/no-object/@" + objClaseAcc.c_colID) != null)
                          objEventoClase.e_objeto = Int32.Parse(xmlDDO.SelectSingleNode("/object/no-object/@" + objClaseAcc.c_colID).Value);
                    }
                    else
                    {
                        throw new System.Exception("Falta parametrizar el nombre de la columna ID para la clase " + objClaseAcc.d_clase);
                    }
                    NomadEnvironment.GetTrace().Debug("1 ");
                    objEvento.EVEN_CLASES.Add(objEventoClase);
                }
                   NomadEnvironment.GetTrace().Debug("2 ");
                // guarda evento y evento_clases para obtener los ID
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(objEvento);

                try
                {
                     NomadEnvironment.GetTrace().Debug("3 " + xmlDDO.DocumentElement.GetElementsByTagName("no-object"));
                    // elimina el elemento no-object del xml del query para que quede el objeto limpio
                    xmlDDO.DocumentElement.RemoveChild(xmlDDO.DocumentElement.GetElementsByTagName("no-object")[0]);
                    NomadEnvironment.GetTrace().Debug("4 ");
                    // crea el recordcard
                    NomadEnvironment.GetTrace().Debug("Pre: GetObject");
                 #if NOMAD_DEBUG
                    objRecordCard = (NucleusRH.Base.RecordCard.AccionesReales.ACCION_REAL)
                        NomadEnvironment.GetObject(
                            System.Reflection.Assembly.LoadFile("E:\\Proyectos\\BIN\\BaytonRH.dll").GetType(claseRC),
                        ////System.Reflection.Assembly.Load("NucleusRHClassesV11").GetType(claseRC),
                            xmlDDO.OuterXml);
                 
                #else
                    objRecordCard = (NucleusRH.Base.RecordCard.AccionesReales.ACCION_REAL)
                        NomadEnvironment.GetObject(
                            System.Reflection.Assembly.GetExecutingAssembly().GetType(claseRC),
                            ////System.Reflection.Assembly.Load("NucleusRHClassesV11").GetType(claseRC),
                            xmlDDO.OuterXml);
                #endif
                    NomadEnvironment.GetTrace().Debug("Post: GetObject");

                    objRecordCard.oi_evento = objEvento.Id;

                    NomadEnvironment.GetCurrentTransaction().Save(objRecordCard);
                }
                catch (Exception ex)
                {
                    NomadEnvironment.GetCurrentTransaction().Delete(objEvento);
                    throw ex;
                }

                return;
          }
            catch (Exception ex) {
                NomadEnvironment.GetTrace().Debug("GuardarAccion() ERROR: " + ex.Message, ex);
                throw new NomadAppException("Error en método GuardarAccion(): " + ex.Message, ex);
          }
        }

        public virtual System.Xml.XmlDocument ObtieneDDORC(string parametrosQry)
        {
            return null;
        }
     
  }
}



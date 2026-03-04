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

namespace NucleusRH.Base.Configuracion.Alertas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Alertas
    public partial class DESTINATARIO : Nomad.NSystem.Base.NomadObject
    {
        public static void Guardar(string PARENT, NucleusRH.Base.Configuracion.Alertas.DESTINATARIO DDO)
        {
            if (DDO.IsForInsert)
            {
                ALERTA OBJ = ALERTA.Get(PARENT, false);

                switch (DDO.c_tipo)
                {
                    case "OWNER":
                        DDO.d_destinatario = "Dueño";
                        break;

                    case "ETTY":
                        DDO.d_destinatario = "Entidad " + Nomad.Base.Login.Entidades.ENTIDAD.Get(DDO.d_destinatario_id).DES;
                        break;

                    case "MAIL":
                        DDO.d_destinatario = "Mail " + DDO.d_destinatario_id;
                        break;
                }

                //Verifico si ya fue Agregado
                foreach (DESTINATARIO CUR in OBJ.DESTINATARIOS)
                {
                    if (CUR.d_destinatario == DDO.d_destinatario)
                        throw NomadException.NewMessage("CONFIG.ALERT.DESTINATARIO", DDO.d_destinatario);
                }

                //Guardo el DDO	
                OBJ.DESTINATARIOS.Add(DDO);
                NomadEnvironment.GetCurrentTransaction().Save(OBJ);
            }
            else
            {
                NomadEnvironment.GetCurrentTransaction().Save(DDO);
            }
        }

    }
}

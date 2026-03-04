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

namespace NucleusRH.Base.Capacitacion.Requerimientos
{    
    public partial class REQUERIMIENTO : Nomad.NSystem.Base.NomadObject
    {
        public void SendMail()
        {
            try
            {
                Nomad.Base.Mail.OutputMails.MAIL mail = new Nomad.Base.Mail.OutputMails.MAIL();
                Nomad.Base.Mail.OutputMails.DESTINATARIO destinatario = new Nomad.Base.Mail.OutputMails.DESTINATARIO();

                string oi_usuario = "";

                oi_usuario = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(this.oi_personal_emp).Getoi_personal().oi_usuario_sistema;

                string body = "Sin Contenido";
                if (this.c_estado == "A")
                {
                    body = "Se ha aprobado su requerimiento de capacitación. \n\nDatos del curso \nTítulo: " + (this.d_tituloNull ? "(No Especificado)" : this.d_titulo) + "\nFecha de Inicio: " + (this.f_dictadoNull ? "(No Especificado)" : this.f_dictado.ToString("dd/MM/yyyy"));
                }
                else
                {
                    body = "Se ha rechazado su requerimiento de capacitación. \n\nMotivo de Rechazo: " + (this.o_motivo_rechazoNull ? "(No Especificado)" : this.o_motivo_rechazo);
                }

                destinatario.ENTIDAD = oi_usuario;

                mail.DESDE_APLICACION = "NucleusRH";
                mail.FECHA_CREACION = DateTime.Now.Date;
                mail.REMITENTE = "NucleusRH";
                mail.ASUNTO = "NucleusRH - Requerimiento de Capacitación";
                mail.CONTENIDO = body;
                mail.DESTINATARIOS.Add(destinatario);
                NomadEnvironment.GetCurrentTransaction().Save(mail);
            }
            catch (Exception e)
            {
                NomadEnvironment.GetTrace().Info("No se pudo enviar el mail - " + e.Message + "\n" + e.InnerException);
            }
        }
        public void Cubrir()
        {
            //CHEQUEO SI EL REQUERIMIENTO TIENE CARGADO ALGUN DICTADO QUE LO CUBRA
            if (this.REQS_DIC.Count > 0)
            {
                this.c_estado = "C";
                NomadEnvironment.GetCurrentTransaction().Save(this);
            }
            else
            {
                throw new NomadAppException("No se encontraron dictados que cubran el requerimiento");
            }
        }

    }
}


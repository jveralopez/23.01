using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Gestion_de_Postulantes.Solicitudes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Solicitudes de Personal
    public partial class SOLICITUD 
    {
        public static void MandarMailGerencia(string paramMsg, string paramMails)
        {

            Nomad.Base.Mail.OutputMails.MAIL mail = new Nomad.Base.Mail.OutputMails.MAIL();
            mail.DESDE_APLICACION = "NucleusRH";
            mail.FECHA_CREACION = DateTime.Now;
            mail.REMITENTE = "NucleusRH";
            mail.ASUNTO = "Aprobacion de Solicitud de Personal";
            mail.CONTENIDO = paramMsg;
            Nomad.Base.Mail.OutputMails.DESTINATARIO destinatario;
            string[] dirs = paramMails.Split(';');
            if (dirs.Length > 0)
            {
                foreach (string dir in dirs)
                {
                    destinatario = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                    destinatario.MAIL_SUSTITUTO = dir;
                    mail.DESTINATARIOS.Add(destinatario);
                }
            }
            NomadEnvironment.GetCurrentTransaction().Save(mail);

        }

    }
}

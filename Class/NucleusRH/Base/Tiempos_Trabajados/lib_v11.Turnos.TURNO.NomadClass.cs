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

namespace NucleusRH.Base.Tiempos_Trabajados.Turnos
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Turnos
    public partial class TURNO : Nomad.NSystem.Base.NomadObject
    {
        public static TURNO GetById(string id)
        {
            TURNO obj = (TURNO)NomadProxy.GetProxy().CacheGetObj("NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO." + id);
            if (obj == null)
            {
                obj = TURNO.Get(id, false);
                NomadProxy.GetProxy().CacheAdd("NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO." + id, obj);
            }

            return obj;
        }
        public void ActualizarTurnos(string RES)
        {
            NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNODET obj;

            //Elimino todo el Detalle
            this.TURNOS_DET.Clear();

            //Recorro el Array creaando el Nuevo Detalle
            IList lRES = RES.Split('|');

            for (int t = 1; t < lRES.Count; t += 2)
            {
                if (((string)lRES[t]) == ",,,,") continue;

                obj = new NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNODET();

                obj.e_hora_inicio = int.Parse((string)lRES[t - 1]);
                obj.e_hora_fin = int.Parse((string)lRES[t + 1]);

                IList MyVals = ((string)lRES[t]).Split(',');

                obj.oi_tipo_hs_norm = (string)(MyVals[0]);
                obj.oi_tipo_hs_dom = (string)(MyVals[1]);
                obj.oi_tipo_hs_fer = (string)(MyVals[2]);
                obj.oi_tipo_hs_domfer = (string)(MyVals[3]);
                obj.oi_tipo_hs_nolab = (string)(MyVals[4]);

                this.TURNOS_DET.Add(obj);
            }
        }
        public static bool ChkComp(string oi_turno_prev, string oi_turno_post)
        {
            //Analizo el turno previo
            TURNO prev = TURNO.Get(oi_turno_prev);
            int prev_st = 2880;
            int prev_ed = -1440;
            foreach (TURNODET DETTUR in prev.TURNOS_DET)
            {
                if (DETTUR.e_hora_inicio < prev_st) prev_st = DETTUR.e_hora_inicio;
                if (DETTUR.e_hora_fin > prev_ed) prev_ed = DETTUR.e_hora_fin;
            }

            //Analizo el turno previo
            TURNO post = TURNO.Get(oi_turno_post);
            int post_st = 2880;
            int post_ed = -1440;
            foreach (TURNODET DETTUR in post.TURNOS_DET)
            {
                if (DETTUR.e_hora_inicio < post_st) post_st = DETTUR.e_hora_inicio;
                if (DETTUR.e_hora_fin > post_ed) post_ed = DETTUR.e_hora_fin;
            }

            //Comparo Turnos
            return prev_ed > post_st + 1440 ? false : true;
        }
    }
}



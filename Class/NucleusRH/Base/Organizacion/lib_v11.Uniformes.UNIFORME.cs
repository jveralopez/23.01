using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Uniformes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Uniformes
    public partial class UNIFORME
    {
        public void Cambio_Recuento(NucleusRH.Base.Organizacion.Uniformes.REC_UNIFORME recuento)
        {
            // Verifica que haya un recuento anterior
            if (this.UNIF_REC.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Organizacion.Uniformes.REC_UNIFORME rec = (REC_UNIFORME)this.UNIF_REC[this.UNIF_REC.Count - 1];
                if (rec.f_recuento >= recuento.f_recuento)
                    throw new NomadAppException("La fecha de recuento debe ser posterior a la de la existencia en stock (" + rec.f_recuento.ToString("dd/MM/yyyy") + ")");

                // Asigna la cantidad de recuento anterior
                if (rec.e_cantidadNull)
                    rec.e_cantidad = recuento.e_cantidad;

            }
            // Asigna uniforme f_recuento a
            this.e_recuento_stock = recuento.e_cantidad;
            this.f_ult_rec_stock = recuento.f_recuento;
            this.UNIF_REC.Add(recuento);
        }
        public void EliminarRecuento()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Organizacion.Uniformes.REC_UNIFORME del_obj = null;
            foreach (NucleusRH.Base.Organizacion.Uniformes.REC_UNIFORME obj in this.UNIF_REC)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_recuento > del_obj.f_recuento)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.UNIF_REC.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del Uniforme
            NucleusRH.Base.Organizacion.Uniformes.REC_UNIFORME ult_obj = null;
            foreach (NucleusRH.Base.Organizacion.Uniformes.REC_UNIFORME obj in this.UNIF_REC)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_recuento > ult_obj.f_recuento)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el uniforme
            if (ult_obj != null)
            {
                this.f_ult_rec_stock = ult_obj.f_recuento;
                this.e_recuento_stock = ult_obj.e_cantidad;
            }
        }
    }
}

using System;
using System.Xml;
using System.Collections;
using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;

namespace NucleusRH.Base.Tiempos_Trabajados
{

  public class clsHorariosPer
  { private DateTime m_f_fecha_Hora      ;
    private string    m_oi_horario      ;
    private string    m_oi_horarioNew      ;
    private bool      m_HorarioSolapado ;

    NomadTrace objTra;

    public clsHorariosPer()
    {

    }

    public clsHorariosPer(DateTime pDat_f_fechaFin ,string pInt_oi_horario ,string pInt_oi_horarioNew )
    {
        objTra = Nomad.NSystem.Base.NomadEnvironment.GetTrace();
        objTra.Debug(" Funcion clsHorariosPer: entro a la clase/// pDat_f_fechaFin" + pDat_f_fechaFin.ToString() );
        m_f_fecha_Hora   = pDat_f_fechaFin ;
        m_oi_horario      = pInt_oi_horario  ;
        m_oi_horarioNew  = pInt_oi_horarioNew  ;
        m_HorarioSolapado    = Compara_Horas() ;
    }

    public DateTime f_fecha_Hora   {get {return m_f_fecha_Hora;} set {f_fecha_Hora = value;}}
    public string   oi_horario   {get {return m_oi_horario;} set {m_oi_horario = value;}}
    public string   oi_horarioNew   {get {return m_oi_horarioNew;} set {m_oi_horarioNew = value;}}
    public bool     HorarioSolapado   {get {return m_HorarioSolapado;} }

    private Boolean Compara_Horas()
    {
    /*
objTra = Nomad.NSystem.Base.NomadEnvironment.GetTrace();

        objTra.Debug("Funcion Compara_Horas:  " );

        DateTime m_f;
        NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO objHorario ;
        NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIOSDET objHorarioDet ;
         NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO objHorarioNew;

        ///////////   Datos Horario Nuevo  ///////////

        objHorarioNew = NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.Get( m_oi_horarioNew  );
        m_e_iniciodia = objHorarioNew.e_iniciodia ;
        objTra.Debug("Funcion Compara_Horas:  objHorarioNew" + objHorarioNew.SerializeAll() );
        if ( m_e_iniciodia == 0 )
        {
            objTra.Debug("Funcion Compara_Horas:  Sale porque el horario nuevo m_e_iniciodia == 0" );
            return false;
        }

        m_f_hora_Inicio = m_f_fecha_Hora;

        objHorario = NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.Get( m_oi_horario  );
        objTra.Debug("Funcion Compara_Horas: objHorario Ant" + objHorario.SerializeAll()  );
        /// *********    Calcula la posicion del día que coinciden los horarios  *********
        DateTime inicio = objHorario.f_fechaInicio;                          ;
        ///resta un día del horario anterior por el inicion de día del Nuevo, solo si incio dia es negativo
        if ( m_e_iniciodia < 0 )
        {
              m_f_fecha_Hora = m_f_fecha_Hora.AddDays(-1);
        }

        TimeSpan duracion = m_f_fecha_Hora - inicio;
        int nPosicion=(int) (duracion.TotalDays % objHorario.e_dias) + 1 ;

        objTra.Debug("Funcion Compara_Horas:  nPosicion " + nPosicion.ToString()  );

        m_f_hora_Inicio = m_f_hora_Inicio.AddHours( m_e_iniciodia );
        objTra.Debug("Funcion Compara_Horas:  m_f_hora_Inicio " + m_f_hora_Inicio.ToString()  );

        /// *********   OBTINENE LA MAXIMA HORA DEL HORARIO GUARDADO PARA ESE DÍA  **********
        foreach (NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIOSDET objDet  in  objHorario.HORARIOSDET    )
          {
              if (objDet.e_posicion==nPosicion)
                {
                      if (  m_f_hora_Inicio < objDet.f_hora_fin)
                      {
                          objTra.Debug("Funcion Compara_Horas:  El Horario se solapa con la poscion anterior"  );
                          objTra.Debug("Funcion Compara_Horas:  m_f_hora_Inicio" + m_f_hora_Inicio.ToString());
                          objTra.Debug("Funcion Compara_Horas:  objDet.f_hora_fin" + objDet.f_hora_fin.ToString() );

                            return true;
                      }
                 }
       }

  */
    objTra.Debug("Funcion Compara_Horas:   Fin no encontro problemas      ");

    return false;

    }

  }

}



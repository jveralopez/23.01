alter table dbo.EVA01_EVENTOS
add oi_mot_eval int null


alter table dbo.EVA01_EVENTOS
  add constraint FK_EVA01_EV_REFERENCE_EVA03_MO foreign key (oi_mot_eval)
   references dbo.EVA03_MOTIVO_EVA (oi_mot_eval)
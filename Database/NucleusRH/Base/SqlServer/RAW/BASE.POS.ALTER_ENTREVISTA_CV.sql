alter table dbo.POS01_ENTREVISTAS
   add constraint FK_POS01_EN_REFERENCE_PER02_PE foreign key (oi_personal_emp)
      references dbo.PER02_PERSONAL_EMP (oi_personal_emp)
go
alter table dbo.PER02_PERSONAL_EMP
   add constraint FK_PER02_PE_REFERENCE_PER33_AR foreign key (oi_art)
      references dbo.PER33_ART (oi_art)
go

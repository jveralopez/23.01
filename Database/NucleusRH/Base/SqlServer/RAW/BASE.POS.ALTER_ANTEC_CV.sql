alter table dbo.POS01_ANTEC_CV
drop column te_empresa

alter table dbo.POS01_ANTEC_CV
add 
   n_pais_emp           numeric(4)           null,
   n_area_emp           numeric(4)           null,
   n_telefono_emp       numeric(15)          null
   
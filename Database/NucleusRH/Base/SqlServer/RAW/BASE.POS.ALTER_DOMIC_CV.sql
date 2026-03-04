alter table pos01_domic_cv
drop  column te_1,
      column te_2

alter table pos01_domic_cv
add n_pais               numeric(4)           null,
 n_area               numeric(4)           null,
 n_telefono           numeric(15)          null,
 n_pais_alt           numeric(4)           null,
 n_area_alt           numeric(4)           null,
 n_telefono_alt       numeric(15)          null
/*==============================================================*/
/* Table: CYD01_PROV_CURSO                                      */
/*==============================================================*/

ALTER TABLE CYD01_PROV_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_PR

ALTER TABLE CYD01_PROV_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_PR unique (oi_proveedor, oi_curso)

/*==============================================================*/
/* Table: CYD01_COMP_CURSO                                      */
/*==============================================================*/

ALTER TABLE CYD01_COMP_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_CO

ALTER TABLE CYD01_COMP_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_CO unique (oi_curso, oi_competencia)

/*==============================================================*/
/* Table: CYD01_REC_CURSO                                       */
/*==============================================================*/

ALTER TABLE CYD01_REC_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_RE

ALTER TABLE CYD01_REC_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_RE unique (oi_recurso, oi_curso)
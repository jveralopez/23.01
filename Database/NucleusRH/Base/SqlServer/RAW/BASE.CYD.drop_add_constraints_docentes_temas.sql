/*==============================================================*/
/* Table: CYD01_TEMAS_CURSO                                     */
/*==============================================================*/

ALTER TABLE CYD01_TEMAS_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_TE

ALTER TABLE CYD01_TEMAS_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_TE UNIQUE  (c_tema_curso, oi_curso)

/*==============================================================*/
/* Table: CYD01_DOC_CURSO                                       */
/*==============================================================*/

ALTER TABLE CYD01_DOC_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_DO

ALTER TABLE CYD01_DOC_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_DO UNIQUE  (oi_docente, oi_curso)

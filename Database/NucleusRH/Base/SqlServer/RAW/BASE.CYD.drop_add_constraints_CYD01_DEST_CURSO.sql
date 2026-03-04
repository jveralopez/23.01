/*==============================================================*/
/* Table: CYD01_DEST_CURSO                                      */
/*==============================================================*/

ALTER TABLE CYD01_DEST_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_DE

ALTER TABLE CYD01_DEST_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_DE UNIQUE  (oi_estructura, oi_curso)

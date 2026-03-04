/*==============================================================*/
/* Table: CLI34_MOT_COEFICIENTE                                 */
/*==============================================================*/
create table dbo.CLI34_MOT_COEFICIENTE (
   oi_mot_cam_coef      int                  not null,
   c_mot_cam_coef       varchar(30)          not null,
   d_mot_cam_coef       varchar(100)         not null,
   o_mot_cam_coef       varchar(1000)        null,
   constraint PK_CLI34_MOT_COEFICIENTE primary key (oi_mot_cam_coef),
   constraint AK_CLI34_MOT_COEFICIENTE unique (c_mot_cam_coef)
)
go

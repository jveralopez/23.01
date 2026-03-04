/*==============================================================*/
/* Table: CLI33_MOT_COMERCIAL                                   */
/*==============================================================*/
create table dbo.CLI33_MOT_COMERCIAL (
   oi_mot_cam_com       int                  not null,
   c_mot_cam_com        varchar(30)          not null,
   d_mot_cam_com        varchar(100)         not null,
   o_mot_cam_com        varchar(1000)        null,
   constraint PK_CLI33_MOT_COMERCIAL primary key (oi_mot_cam_com),
   constraint AK_CLI33_MOT_COMERCIAL unique (c_mot_cam_com)
)
go

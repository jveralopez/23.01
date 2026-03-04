
--Crea los campos nuevos
ALTER TABLE TTA01_TipoHoras
add e_sumariza_aus int null

ALTER TABLE TTA01_TipoHoras
add e_sumariza_pre int null



--Corrije los datos actuales
UPDATE TTA01_TipoHoras
SET l_sumariza = 0
WHERE l_sumariza is null

UPDATE TTA01_TipoHoras
SET l_sumariza_aus = 0
WHERE l_sumariza_aus is null



--Actualiza los nuevos valores basandose en los anteriores
UPDATE TTA01_TipoHoras
SET  e_sumariza_pre = 0, e_sumariza_aus = 0
WHERE l_sumariza = 0

UPDATE TTA01_TipoHoras
SET e_sumariza_pre = 1, e_sumariza_aus = 0
WHERE l_sumariza = 1 and l_sumariza_aus = 0

UPDATE TTA01_TipoHoras
SET e_sumariza_pre = 1, e_sumariza_aus = 2
WHERE l_sumariza = 1 and l_sumariza_aus = 1



--Actualiza los campos con las restricciones. No elimina los campos viejos, serán eliminados en otra actualización.
ALTER TABLE TTA01_TipoHoras
ALTER COLUMN e_sumariza_aus int not null

ALTER TABLE TTA01_TipoHoras
ALTER COLUMN e_sumariza_pre int not null


SET SQL_SAFE_UPDATES = 0;
DELETE FROM dkim_entity 
WHERE concat(id,version) IN (SELECT state FROM (SELECT  concat(id,version) as state FROM dkim_entity d where d.version < (select max(de.version) FROM dkim_entity de where de.version > 1 AND de.id = d.id))r);
SET SQL_SAFE_UPDATES = 1;

ALTER TABLE `dkim_entity` 
DROP PRIMARY KEY,
ADD PRIMARY KEY (`id`);
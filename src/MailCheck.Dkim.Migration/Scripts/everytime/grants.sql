GRANT SELECT ON `dkim_entity` TO '{env}-dkim-api' IDENTIFIED BY '{password}';
GRANT SELECT ON `dkim_entity_history` TO '{env}-dkim-api' IDENTIFIED BY '{password}';

GRANT SELECT, INSERT, UPDATE ON `dkim_entity_history` TO '{env}-dkim-ent' IDENTIFIED BY '{password}'; 
GRANT SELECT, INSERT, UPDATE, DELETE ON `dkim_entity` TO '{env}-dkim-ent' IDENTIFIED BY '{password}';

GRANT SELECT, INSERT, UPDATE, DELETE ON `dkim_scheduled_records` TO '{env}-dkim-sch' IDENTIFIED BY '{password}';

GRANT SELECT ON `dkim_entity` TO '{env}_reports' IDENTIFIED BY '{password}';
GRANT SELECT INTO S3 ON *.* TO '{env}_reports' IDENTIFIED BY '{password}';

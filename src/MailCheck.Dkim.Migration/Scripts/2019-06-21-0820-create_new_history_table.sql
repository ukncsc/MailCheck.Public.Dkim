ALTER TABLE `dkim_entity_history` 
RENAME TO `dkim_entity_history_old_2` ;

CREATE TABLE `dkim_entity_history` (
  `id` varchar(255) NOT NULL,
  `state` json NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

GRANT SELECT, INSERT, UPDATE ON `dkim_entity_history` TO '{env}-dkim-ent' IDENTIFIED BY '{password}'; 
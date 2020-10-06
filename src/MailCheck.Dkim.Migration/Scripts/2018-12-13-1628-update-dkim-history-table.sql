ALTER TABLE `dkim_entity_history` 
RENAME TO `dkim_entity_history_old` ;

CREATE TABLE `dkim_entity_history` (
  `id` varchar(255) NOT NULL,
  `state` json NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
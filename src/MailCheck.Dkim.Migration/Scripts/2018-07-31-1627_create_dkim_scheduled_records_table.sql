CREATE TABLE IF NOT EXISTS `dkim_scheduled_records` (
  `id` varchar(255) NOT NULL,
  `version` int(11) NOT NULL,
  `state` json NOT NULL,
  `last_checked` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_last_checked` (`last_checked`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
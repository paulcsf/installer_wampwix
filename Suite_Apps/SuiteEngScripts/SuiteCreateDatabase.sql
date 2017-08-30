

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";


--
-- Database: `suitedatabase`
--
CREATE DATABASE IF NOT EXISTS suitedatabase;

USE suitedatabase;

--

--
-- Table structure for table `log`
--

CREATE TABLE IF NOT EXISTS `log` (
  `dbID` bigint(20) NOT NULL AUTO_INCREMENT,
  `Time` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Text` varchar(4096) NOT NULL,
  PRIMARY KEY (`dbID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------


CREATE TABLE IF NOT EXISTS `settings` (
  `dbID` bigint(20) NOT NULL AUTO_INCREMENT,
  `parameter` varchar(50) NOT NULL,
  `value` varchar(1024) NOT NULL,
  `defaultvalue` varchar(1024) NOT NULL,
  `description` varchar(1024) DEFAULT NULL,
  PRIMARY KEY (`dbID`),
  UNIQUE KEY `parameter` (`parameter`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=2 ;

INSERT INTO `settings` (`dbID`, `parameter`, `value`, `defaultvalue`, `description`) VALUES
(1, 'DBVersion', '20140226', 'NULL', 'Date/version of database schema.');
INSERT INTO settings (`parameter`, `value`, `defaultvalue`, `description`) VALUES
  ('ForceEngineRestart', '0', '0', 'Forces restart of the Engine service, set to 1 to restart (Engine will reset value to 0)'),
  ('logTTLinMinutes', '4320', '4320', 'The max age of log entries in the log table should be kept in minutes'),
  ('loginHistoryTTLinMinutes', '260000', '260000', 'The max age of entries in the users_login table that should be kept in minutes (26000 min or about 6 months)');



CREATE TABLE IF NOT EXISTS `users` (
  `user_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `username` varchar(30) NOT NULL,
  `namefirst` varchar(30) NOT NULL DEFAULT '',
  `namelast` varchar(30) NOT NULL DEFAULT '',
  `email` varchar(50) NOT NULL,
  `password` char(128) NOT NULL,
  `salt` char(128) NOT NULL,
  `lockedout` tinyint(1) NOT NULL DEFAULT '0',
  `remark` varchar(4096) NOT NULL DEFAULT '',
  `Valid` tinyint(1) NOT NULL DEFAULT '1' COMMENT '1=valid, 0=deleted',
  `userGroup` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=3 ;

INSERT INTO `users` (`user_id`, `username`, `namefirst`, `namelast`, `email`, `password`, `salt`, `lockedout`, `remark`, `Valid`, `userGroup`) VALUES
(1, 'admin', '', '', 'admin@suite.com', '125a1a429de77d53b11d536ab87aff9fb92e25cce64283e98509a2e9ec46ee523a6bc328153efbd9268915d5fcf97069b33de8cf8c583d681c2d97c928a72fb6', 'b263c128f045e8cb160e6d822cac914c10a4ea8eb3af96dd78ec44a8c41fb40d90c66fdddaf14aec4c8909c4e5b2d5df576e907496823dd94f37e8a711e46058', 0, 'Default admin, username=admin, password=admin.', 1, 2),
(2, 'user', '', '', 'user@suite.com', 'd8ac258694bc245095de31b3d662c15165ed6e1ded62ed545b38f9d808ac9904352a60aa6d67d129c03f945bf668578df36b8f0813ad3da080d3e11877912d8c', '2427b9e57f628f260eb8781a4c092b0b15bf71e4bca869743adda778e43d37fdca4b01fccb42e9ef1167d53b1cd2a75c74346440de7fcff0774cfe81ebd8de80', 0, 'Default user, username=user, password=user.', 1, 3);

CREATE TABLE IF NOT EXISTS `users_logins` (
  `login_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `login_time` datetime NOT NULL,
  `success` tinyint(1) NOT NULL DEFAULT '0',
  `last_check` datetime NOT NULL,
  `from` varchar(15) NOT NULL,
  `Valid` tinyint(1) NOT NULL DEFAULT '1' COMMENT '1=valid, 0=logged out',
  PRIMARY KEY (`login_id`),
  KEY `FK_users_logins_user_id` (`user_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;

ALTER TABLE `users_logins`
    ADD CONSTRAINT `FK_users_logins_user_id`
    FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON UPDATE CASCADE ON DELETE CASCADE;



CREATE USER 'suitedbuser'@'localhost' IDENTIFIED BY 'suitedbpass';
GRANT SELECT , INSERT , UPDATE ON * . * TO 'suitedbuser'@'localhost' IDENTIFIED BY 'suitedbpass' WITH MAX_QUERIES_PER_HOUR 0 MAX_CONNECTIONS_PER_HOUR 0 MAX_UPDATES_PER_HOUR 0 MAX_USER_CONNECTIONS 0;

DELIMITER $$

CREATE
DEFINER = 'root'@'localhost'
EVENT HourlyCleanupEvent
ON SCHEDULE EVERY '1' HOUR
STARTS NOW()
COMMENT 'deletes data in log tables that is 7 days or more'
DO
BEGIN
  INSERT INTO log (Text) values ('Cleanup started.');
  DELETE
    FROM log
  WHERE log.Time < DATE_SUB(NOW(), INTERVAL (SELECT VALUE FROM settings s WHERE s.parameter = 'logTTLinMinutes') MINUTE);
  DELETE
    FROM users_logins
  WHERE login_time < DATE_SUB(NOW(), INTERVAL (SELECT VALUE FROM settings s WHERE s.parameter = 'loginHistoryTTLinMinutes') MINUTE);
  INSERT INTO log (Text) values ('Cleanup ended.');
END
$$

DELIMITER ;



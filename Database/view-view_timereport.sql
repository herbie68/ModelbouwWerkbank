-- --------------------------------------------------------
-- Host:                         localhost
-- Server versie:                8.3.0 - MySQL Community Server - GPL
-- Server OS:                    Win64
-- HeidiSQL Versie:              12.14.0.7165
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Tijdelijke tabel wordt verwijderd, en definitieve VIEW wordt aangemaakt.
DROP TABLE IF EXISTS `view_timereport`;
CREATE ALGORITHM=MERGE SQL SECURITY DEFINER VIEW `view_timereport` AS select `p`.`Id` AS `ProjectId`,`p`.`Name` AS `ProjectName`,`w`.`Name` AS `WorktypeName`,concat(`t`.`WorkDate`) AS `WorkDate`,concat(year(`t`.`WorkDate`)) AS `Year`,`m`.`Month` AS `Month`,concat(year(`t`.`WorkDate`),right(concat('0',month(`t`.`WorkDate`)),2)) AS `YearMonth`,`d`.`Day` AS `Day`,concat(year(`t`.`WorkDate`),weekday(`t`.`WorkDate`)) AS `YearDay`,concat(left((`t`.`EndTime` - `t`.`StartTime`),(length((`t`.`EndTime` - `t`.`StartTime`)) - 4)),':',left(right((`t`.`EndTime` - `t`.`StartTime`),4),2)) AS `WorkTime`,(((left(`t`.`EndTime`,2) * 60) + substr(`t`.`EndTime`,4,2)) - ((left(`t`.`StartTime`,2) * 60) + substr(`t`.`StartTime`,4,2))) AS `WorkedMinutes` from ((((`time` `t` join `project` `p` on((`t`.`project_Id` = `p`.`Id`))) join `worktype` `w` on((`t`.`worktype_Id` = `w`.`Id`))) join `weekdays` `d` on((weekday(`t`.`WorkDate`) = `d`.`ID`))) join `months` `m` on((month(`t`.`WorkDate`) = `m`.`ID`)))
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

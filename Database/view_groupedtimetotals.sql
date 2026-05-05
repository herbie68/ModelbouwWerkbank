-- --------------------------------------------------------
-- Host:                         localhost
-- Server versie:                8.3.0 - MySQL Community Server - GPL
-- Server OS:                    Win64
-- HeidiSQL Versie:              12.11.0.7065
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
DROP TABLE IF EXISTS `view_groupedtimetotals`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `view_groupedtimetotals` AS select `p`.`Id` AS `ProjectId`,`p`.`Name` AS `ProjectName`,`w`.`ParentId` AS `WorktypeParentId`,`w`.`Id` AS `WorktypeId`,`w`.`Name` AS `WorktypeName`,coalesce(sum(timestampdiff(MINUTE,str_to_date(concat(date_format(`t`.`WorkDate`,'%Y-%m-%d'),' ',`t`.`StartTime`),'%Y-%m-%d %H:%i:%s'),str_to_date(concat(date_format(`t`.`WorkDate`,'%Y-%m-%d'),' ',`t`.`EndTime`),'%Y-%m-%d %H:%i:%s'))),0) AS `TotalElapsedMinutes`,sec_to_time(coalesce(sum(timestampdiff(SECOND,str_to_date(concat(date_format(`t`.`WorkDate`,'%Y-%m-%d'),' ',`t`.`StartTime`),'%Y-%m-%d %H:%i:%s'),str_to_date(concat(date_format(`t`.`WorkDate`,'%Y-%m-%d'),' ',`t`.`EndTime`),'%Y-%m-%d %H:%i:%s'))),0)) AS `TotalElapsedTime`,(case when (`w`.`ParentId` is null) then (`w`.`Id` * 1000) else ((`w`.`ParentId` * 1000) + `w`.`Id`) end) AS `SortOrder` from ((`project` `p` join `worktype` `w`) left join `time` `t` on(((`p`.`Id` = `t`.`project_Id`) and (`w`.`Id` = `t`.`worktype_Id`)))) group by `p`.`Id`,`p`.`Name`,`w`.`Id`,`w`.`Name` order by `p`.`Id`,`SortOrder`,`w`.`ParentId`,`w`.`Id`
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

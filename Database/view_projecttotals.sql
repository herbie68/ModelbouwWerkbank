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
DROP TABLE IF EXISTS `view_projecttotals`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `view_projecttotals` AS select `p`.`Id` AS `Id`,`p`.`Code` AS `Code`,`p`.`Name` AS `Name`,date_format(`p`.`StartDate`,'%d-%m-%Y') AS `StartDate`,date_format(`p`.`EndDate`,'%d-%m-%Y') AS `EndDate`,(case when (`p`.`EndDate` is not null) then floor(sum((case when ((`t`.`EndTime` is not null) and (`t`.`StartTime` is not null)) then (timediff(`t`.`EndTime`,`t`.`StartTime`) / 10000) else 0 end))) else floor(`p`.`ExpectedTime`) end) AS `ExpectedTime`,`p`.`Image` AS `Image`,`p`.`ImageRotationAngle` AS `ImageRotationAngle`,`p`.`Memo` AS `Memo`,sum((case when ((`t`.`EndTime` is not null) and (`t`.`StartTime` is not null)) then (timediff(`t`.`EndTime`,`t`.`StartTime`) / 10000) else 0 end)) AS `TotalTimeInHours`,(select date_format(`t2`.`WorkDate`,'%d-%m-%Y') from `time` `t2` where (`t2`.`project_Id` = `p`.`Id`) order by timediff(`t2`.`EndTime`,`t2`.`StartTime`) limit 1) AS `ShortestWorkday`,(select (timediff(`t3`.`EndTime`,`t3`.`StartTime`) / 10000) from `time` `t3` where (`t3`.`project_Id` = `p`.`Id`) order by timediff(`t3`.`EndTime`,`t3`.`StartTime`) limit 1) AS `HoursShortestWorkday`,(select date_format(`t4`.`WorkDate`,'%d-%m-%Y') from `time` `t4` where (`t4`.`project_Id` = `p`.`Id`) order by timediff(`t4`.`EndTime`,`t4`.`StartTime`) desc limit 1) AS `LongestWorkday`,(select (timediff(`t5`.`EndTime`,`t5`.`StartTime`) / 10000) from `time` `t5` where (`t5`.`project_Id` = `p`.`Id`) order by timediff(`t5`.`EndTime`,`t5`.`StartTime`) desc limit 1) AS `HoursLongestWorkday`,count(distinct `t`.`WorkDate`) AS `WorkingDays`,(case when (`p`.`Closed` = 1) then 'true' else 'false' end) AS `IsClosed`,(case when (count(distinct `t`.`WorkDate`) > 0) then round((sum((case when ((`t`.`EndTime` is not null) and (`t`.`StartTime` is not null)) then (timediff(`t`.`EndTime`,`t`.`StartTime`) / 10000) else 0 end)) / count(distinct `t`.`WorkDate`)),2) else 0 end) AS `AverageHoursPerDay` from (`project` `p` left join `time` `t` on((`p`.`Id` = `t`.`project_Id`))) group by `p`.`Id`
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

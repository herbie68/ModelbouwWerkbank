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
DROP TABLE IF EXISTS `view_productusage`;
CREATE ALGORITHM=MERGE SQL SECURITY DEFINER VIEW `view_productusage` AS select `pu`.`Id` AS `Id`,`pu`.`project_Id` AS `ProjectId`,`pu`.`product_Id` AS `ProductId`,`pu`.`UsageDate` AS `UsageDate`,`pj`.`Name` AS `ProjectName`,`pd`.`Name` AS `ProductName`,`ct`.`Name` AS `CategoryName`,`ct`.`Id` AS `CategoryId`,`pu`.`AmountUsed` AS `AmountUsed`,`pd`.`Price` AS `Price`,(`pu`.`AmountUsed` * `pd`.`Price`) AS `TotalCost`,`pu`.`Comment` AS `Comment` from (((`productusage` `pu` join `project` `pj` on((`pu`.`project_Id` = `pj`.`Id`))) join `product` `pd` on((`pu`.`product_Id` = `pd`.`Id`))) join `category` `ct` on((`pd`.`Category_Id` = `ct`.`Id`)))
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

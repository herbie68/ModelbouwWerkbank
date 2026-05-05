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
DROP TABLE IF EXISTS `view_receiptsreport`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `view_receiptsreport` AS select `ord`.`OrderDate` AS `OrderDate`,`ord`.`OrderNumber` AS `OrderNumber`,`sup`.`Name` AS `Supplier`,`pro`.`Code` AS `Shortname`,`pro`.`Name` AS `Description`,`orl`.`Amount` AS `Ordered`,NULL AS `ReceivedDate`,NULL AS `Received`,0 AS `IsOrderLine`,`ord`.`Closed` AS `RowClosed`,`ord`.`ClosedDate` AS `RowClosedDate` from (((`supplyorderline` `orl` left join `supplyorder` `ord` on((`orl`.`Supplyorder_Id` = `ord`.`Id`))) left join `supplier` `sup` on((`ord`.`Supplier_Id` = `sup`.`Id`))) left join `product` `pro` on((`orl`.`Product_Id` = `pro`.`Id`))) union all select `ord`.`OrderDate` AS `OrderDate`,`ord`.`OrderNumber` AS `OrderNumber`,`sup`.`Name` AS `Supplier`,`pro`.`Code` AS `Shortname`,`pro`.`Name` AS `Description`,`orl`.`Amount` AS `Ordered`,`stl`.`LogDate` AS `ReceivedDate`,`stl`.`AmountReceived` AS `Received`,1 AS `IsOrderLine`,`ord`.`Closed` AS `RowClosed`,`ord`.`ClosedDate` AS `RowClosedDate` from ((((`supplyorderline` `orl` join `stocklog` `stl` on((`orl`.`Id` = `stl`.`supplyorderline_Id`))) left join `supplyorder` `ord` on((`orl`.`Supplyorder_Id` = `ord`.`Id`))) left join `supplier` `sup` on((`ord`.`Supplier_Id` = `sup`.`Id`))) left join `product` `pro` on((`orl`.`Product_Id` = `pro`.`Id`)))
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

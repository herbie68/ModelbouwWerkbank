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
DROP TABLE IF EXISTS `view_supplyorderline`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `view_supplyorderline` AS select `o`.`Id` AS `Order_Id`,`o`.`OrderNumber` AS `Ordernumber`,`o`.`OrderDate` AS `OrderDate`,`o`.`Supplier_Id` AS `SupplierId`,`s`.`Name` AS `Supplier`,`l`.`Product_Id` AS `ProductId`,`p`.`Code` AS `ProductCode`,`p`.`Name` AS `ProductName`,`p`.`Price` AS `UnitPrice`,sum(`l`.`Amount`) AS `Ordered`,sum(ifnull(`sl`.`AmountReceived`,0)) AS `Received`,max(`l`.`OpenAmount`) AS `Expect`,max(`l`.`Closed`) AS `Closed`,max(`l`.`ClosedDate`) AS `ClosedDate` from ((((`supplyorderline` `l` join `supplyorder` `o` on((`l`.`Supplyorder_Id` = `o`.`Id`))) join `supplier` `s` on((`o`.`Supplier_Id` = `s`.`Id`))) join `product` `p` on((`l`.`Product_Id` = `p`.`Id`))) left join `stocklog` `sl` on(((`l`.`Product_Id` = `sl`.`product_Id`) and (`l`.`Supplyorder_Id` = `sl`.`supplyorder_Id`)))) group by `o`.`Id`,`l`.`Product_Id`
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

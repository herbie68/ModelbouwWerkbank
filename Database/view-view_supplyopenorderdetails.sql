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
DROP TABLE IF EXISTS `view_supplyopenorderdetails`;
CREATE ALGORITHM=MERGE SQL SECURITY DEFINER VIEW `view_supplyopenorderdetails` AS select `ord`.`Id` AS `Order_Id`,`orl`.`Id` AS `OrderLine_Id`,`orl`.`Product_Id` AS `Product_Id`,`ps`.`ProductNumber` AS `SupplierProductNumber`,`ps`.`ProductName` AS `SupplierProductName`,`orl`.`Amount` AS `Ordered`,`orl`.`OpenAmount` AS `WaitFor`,coalesce(`sl`.`AmountReceived`,0) AS `StockLogRecieved`,coalesce(`st`.`Amount`,0) AS `InStock` from ((((`supplyorderline` `orl` left join `supplyorder` `ord` on((`orl`.`Supplyorder_Id` = `ord`.`Id`))) left join `productsupplier` `ps` on((`orl`.`Product_Id` = `ps`.`Product_Id`))) left join `stocklog` `sl` on((`orl`.`Product_Id` = `sl`.`product_Id`))) left join `stock` `st` on((`orl`.`Product_Id` = `st`.`product_Id`))) where (`orl`.`OpenAmount` > 0)
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

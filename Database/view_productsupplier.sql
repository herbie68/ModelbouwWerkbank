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
DROP TABLE IF EXISTS `view_productsupplier`;
CREATE ALGORITHM=MERGE SQL SECURITY DEFINER VIEW `view_productsupplier` AS select `ps`.`Id` AS `Id`,`ps`.`Product_Id` AS `Product_Id`,`ps`.`Supplier_Id` AS `Supplier_Id`,`ps`.`Currency_Id` AS `Currency_Id`,`ps`.`DefaultSupplier` AS `DefaultSupplier`,(case `ps`.`ProductName` when '' then `p`.`Name` else `ps`.`ProductName` end) AS `Name`,`s`.`Name` AS `SupplierName`,`ps`.`ProductNumber` AS `ProductNumber`,concat_ws('',`c`.`Symbol`,'  ',format(`ps`.`Price`,4)) AS `Price`,`ps`.`Url` AS `Url`,format(`ps`.`Price`,4) AS `ProductPrice`,`c`.`Symbol` AS `CurrencySymbol` from (((`productsupplier` `ps` join `product` `p` on((`ps`.`Product_Id` = `p`.`Id`))) join `supplier` `s` on((`ps`.`Supplier_Id` = `s`.`Id`))) join `currency` `c` on((`ps`.`Currency_Id` = `c`.`Id`)))
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

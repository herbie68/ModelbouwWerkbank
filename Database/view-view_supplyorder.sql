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
DROP TABLE IF EXISTS `view_supplyorder`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `view_supplyorder` AS select `so`.`Id` AS `Id`,`so`.`Supplier_Id` AS `Supplier_Id`,`s`.`Name` AS `SupplierName`,`s`.`ShippingCosts` AS `ShippingCosts`,`s`.`MinShippingCosts` AS `MinShippingCosts`,`s`.`OrderCosts` AS `OrderCosts`,`s`.`Currency_Id` AS `DefaulCurrencyIdSupplier`,`cs`.`Symbol` AS `DefaulCurrencySymbolSupplier`,`cs`.`ConversionRate` AS `DefaulCurrencyConversionRateSupplier`,`so`.`Currency_Id` AS `Currency_Id`,`c`.`Symbol` AS `DefaultOrderCurrencySymbol`,`c`.`ConversionRate` AS `DefaultOrderConversionRate`,`so`.`OrderNumber` AS `OrderNumber`,`so`.`OrderDate` AS `OrderDate`,`so`.`CurrencySymbol` AS `OrderCurrencySymbol`,`so`.`CurrencyConversionRate` AS `OrderConversionRate`,`so`.`ShippingCosts` AS `OrderShippingCosts`,`so`.`OrderCosts` AS `OrderOrderCosts`,`so`.`Closed` AS `Closed`,`so`.`ClosedDate` AS `ClosedDate`,`so`.`Memo` AS `Memo`,exists(select 1 from `stocklog` `sl` where (`sl`.`supplyorder_Id` = `so`.`Id`)) AS `HasStockLog` from (((`supplyorder` `so` join `supplier` `s` on((`so`.`Supplier_Id` = `s`.`Id`))) join `currency` `c` on((`so`.`Currency_Id` = `c`.`Id`))) join `currency` `cs` on((`s`.`Currency_Id` = `cs`.`Id`)))
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

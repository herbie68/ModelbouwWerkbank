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
DROP TABLE IF EXISTS `view_productinventory1`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `view_productinventory1` AS select `p`.`Id` AS `Product_Id`,`p`.`Code` AS `Code`,`p`.`Name` AS `Name`,format(`p`.`Price`,2) AS `Price`,`p`.`MinimalStock` AS `MinimalStock`,`c`.`Id` AS `Category`,`s`.`Name` AS `Location`,ifnull((select sum(((`sl`.`AmountReceived` - `sl`.`AmountUsed`) + `sl`.`AmountCorrection`)) from `stocklog` `sl` where (`sl`.`product_Id` = `p`.`Id`)),0) AS `Inventory`,format((`p`.`Price` * ifnull((select sum(((`sl`.`AmountReceived` - `sl`.`AmountUsed`) + `sl`.`AmountCorrection`)) from `stocklog` `sl` where (`sl`.`product_Id` = `p`.`Id`)),0)),2) AS `Value`,(select ifnull(sum(`so`.`OpenAmount`),0) from `supplyorderline` `so` where (`so`.`Product_Id` = `p`.`Id`)) AS `InOrder`,(ifnull((select sum(((`sl`.`AmountReceived` - `sl`.`AmountUsed`) + `sl`.`AmountCorrection`)) from `stocklog` `sl` where (`sl`.`product_Id` = `p`.`Id`)),0) + (select ifnull(sum(`so`.`OpenAmount`),0) from `supplyorderline` `so` where (`so`.`Product_Id` = `p`.`Id`))) AS `VirtualInventory`,format((`p`.`Price` * (ifnull((select sum(((`sl`.`AmountReceived` - `sl`.`AmountUsed`) + `sl`.`AmountCorrection`)) from `stocklog` `sl` where (`sl`.`product_Id` = `p`.`Id`)),0) + (select ifnull(sum(`so`.`OpenAmount`),0) from `supplyorderline` `so` where (`so`.`Product_Id` = `p`.`Id`)))),2) AS `VirtualValue`,greatest((`p`.`MinimalStock` - (ifnull((select sum(((`sl`.`AmountReceived` - `sl`.`AmountUsed`) + `sl`.`AmountCorrection`)) from `stocklog` `sl` where (`sl`.`product_Id` = `p`.`Id`)),0) + (select ifnull(sum(`so`.`OpenAmount`),0) from `supplyorderline` `so` where (`so`.`Product_Id` = `p`.`Id`)))),0) AS `Short`,(case when (greatest((`p`.`MinimalStock` - (ifnull((select sum(((`sl`.`AmountReceived` - `sl`.`AmountUsed`) + `sl`.`AmountCorrection`)) from `stocklog` `sl` where (`sl`.`product_Id` = `p`.`Id`)),0) + (select ifnull(sum(`so`.`OpenAmount`),0) from `supplyorderline` `so` where (`so`.`Product_Id` = `p`.`Id`)))),0) = 0) then greatest((`p`.`MinimalStock` - ifnull((select sum(((`sl`.`AmountReceived` - `sl`.`AmountUsed`) + `sl`.`AmountCorrection`)) from `stocklog` `sl` where (`sl`.`product_Id` = `p`.`Id`)),0)),0) else 0 end) AS `TemporaryShort` from ((`product` `p` left join `category` `c` on((`p`.`Category_Id` = `c`.`Id`))) left join `storage` `s` on((`p`.`Storage_Id` = `s`.`Id`)))
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

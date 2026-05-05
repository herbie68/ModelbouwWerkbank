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

-- Structuur van  tabel modelbuilder.supplyorder wordt geschreven
CREATE TABLE IF NOT EXISTS `supplyorder` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Supplier_Id` int DEFAULT '0',
  `Currency_Id` int DEFAULT '0',
  `OrderNumber` varchar(50) DEFAULT NULL,
  `OrderDate` date DEFAULT NULL,
  `CurrencySymbol` varchar(2) DEFAULT '€',
  `CurrencyConversionRate` double(6,4) DEFAULT '0.0000',
  `ShippingCosts` double DEFAULT '0',
  `OrderCosts` double DEFAULT '0',
  `Memo` longtext,
  `Closed` tinyint DEFAULT '0',
  `ClosedDate` date DEFAULT NULL,
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  `Modified` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `OrderNumber` (`OrderNumber`),
  KEY `FK_Order_Supplier_Id` (`Supplier_Id`),
  KEY `supplyorder_currency_FK` (`Currency_Id`),
  CONSTRAINT `FK_Order_Supplier_Id` FOREIGN KEY (`Supplier_Id`) REFERENCES `supplier` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `supplyorder_currency_FK` FOREIGN KEY (`Currency_Id`) REFERENCES `currency` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci ROW_FORMAT=DYNAMIC;

-- Dumpen data van tabel modelbuilder.supplyorder: ~3 rows (ongeveer)
DELETE FROM `supplyorder`;
INSERT INTO `supplyorder` (`Id`, `Supplier_Id`, `Currency_Id`, `OrderNumber`, `OrderDate`, `CurrencySymbol`, `CurrencyConversionRate`, `ShippingCosts`, `OrderCosts`, `Memo`, `Closed`, `ClosedDate`, `Created`, `Modified`) VALUES
	(12, 1, 2, 'HN58BA10358633', '2025-02-16', '€', 1.1979, 5.95, 10.42, NULL, 1, '2025-04-04', '2025-03-13 08:26:15', '2025-05-27 14:18:14'),
	(15, 16, 1, 'IMB20250413', '2025-04-13', '€', 1.0000, 0, 0, NULL, 1, '2025-04-16', '2025-04-16 15:18:08', '2025-04-16 15:18:30'),
	(16, 17, 1, '207428-29', '2025-04-13', '€', 1.0000, 0, 0, NULL, 1, '2025-05-15', '2025-05-15 17:09:45', '2025-05-15 17:10:46');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

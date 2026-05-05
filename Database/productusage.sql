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

-- Structuur van  tabel modelbuilder.productusage wordt geschreven
CREATE TABLE IF NOT EXISTS `productusage` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `project_Id` int NOT NULL DEFAULT '0',
  `product_Id` int NOT NULL DEFAULT '0',
  `UsageDate` date NOT NULL,
  `AmountUsed` double NOT NULL DEFAULT '0',
  `Comment` varchar(1050) DEFAULT NULL,
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Modifed` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `FK_Project_Id` (`project_Id`),
  KEY `FK_Product_Id` (`product_Id`),
  CONSTRAINT `FK_ProductProduct_Id` FOREIGN KEY (`product_Id`) REFERENCES `product` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_ProductProject_Id` FOREIGN KEY (`project_Id`) REFERENCES `project` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Store the product usage as entered in the Time Management page. Part of the data is equal to the data in the StockLog, but because of Foreign keys necesary for relating to Project and Product age a separate tabble is needed.\r\nSame data , as needed in the Stocklog table, is also stored in the stocklog.';

-- Dumpen data van tabel modelbuilder.productusage: ~0 rows (ongeveer)
DELETE FROM `productusage`;
INSERT INTO `productusage` (`Id`, `project_Id`, `product_Id`, `UsageDate`, `AmountUsed`, `Comment`, `Created`, `Modifed`) VALUES
	(26, 1, 1, '2000-01-01', 1, NULL, '2025-04-29 16:08:33', '2025-04-29 16:08:33');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

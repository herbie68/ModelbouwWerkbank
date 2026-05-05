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

-- Structuur van  tabel modelbuilder.stocklog wordt geschreven
CREATE TABLE IF NOT EXISTS `stocklog` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `product_Id` int NOT NULL DEFAULT '0',
  `supplyorder_Id` int DEFAULT NULL,
  `supplyorderline_Id` int DEFAULT NULL,
  `productusage_Id` int DEFAULT NULL,
  `AmountReceived` double DEFAULT '0',
  `AmountUsed` double DEFAULT '0',
  `AmountCorrection` double DEFAULT '0',
  `LogDate` date DEFAULT NULL,
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Modified` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_StockLog_Product_Id` (`product_Id`) USING BTREE,
  KEY `stocklog_productusage_FK` (`productusage_Id`),
  KEY `stocklog_supplyorder_FK` (`supplyorder_Id`),
  KEY `stocklog_supplyorderline_FK` (`supplyorderline_Id`),
  CONSTRAINT `FK_StockLog_Product_Id` FOREIGN KEY (`product_Id`) REFERENCES `product` (`Id`),
  CONSTRAINT `stocklog_productusage_FK` FOREIGN KEY (`productusage_Id`) REFERENCES `productusage` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `stocklog_supplyorder_FK` FOREIGN KEY (`supplyorder_Id`) REFERENCES `supplyorder` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `stocklog_supplyorderline_FK` FOREIGN KEY (`supplyorderline_Id`) REFERENCES `supplyorderline` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=74 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Registartion of ordered and received goods';

-- Dumpen data van tabel modelbuilder.stocklog: ~45 rows (ongeveer)
DELETE FROM `stocklog`;
INSERT INTO `stocklog` (`Id`, `product_Id`, `supplyorder_Id`, `supplyorderline_Id`, `productusage_Id`, `AmountReceived`, `AmountUsed`, `AmountCorrection`, `LogDate`, `Created`, `Modified`) VALUES
	(19, 33, 12, 26, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(20, 34, 12, 27, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(21, 35, 12, 28, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(22, 36, 12, 29, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(23, 37, 12, 30, NULL, 3000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(24, 38, 12, 31, NULL, 1000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(25, 39, 12, 32, NULL, 1000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(26, 40, 12, 33, NULL, 1000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(27, 41, 12, 34, NULL, 1000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(28, 42, 12, 35, NULL, 15, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(29, 43, 12, 36, NULL, 10, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(30, 44, 12, 37, NULL, 10, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(31, 45, 12, 38, NULL, 4, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(32, 46, 12, 39, NULL, 1, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(33, 47, 12, 40, NULL, 2, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(34, 48, 12, 41, NULL, 4, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(35, 49, 12, 42, NULL, 40, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(36, 50, 12, 43, NULL, 4, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(37, 51, 12, 44, NULL, 2, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(38, 52, 12, 45, NULL, 2, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(39, 53, 12, 46, NULL, 10, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(40, 54, 12, 47, NULL, 50, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(41, 55, 12, 48, NULL, 10, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(42, 56, 12, 49, NULL, 50, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(43, 57, 12, 50, NULL, 10, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(44, 58, 12, 51, NULL, 10, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(45, 59, 12, 52, NULL, 4, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(46, 23, 12, 53, NULL, 2, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(47, 60, 12, 54, NULL, 1, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(48, 61, 12, 55, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(49, 62, 12, 56, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(50, 63, 12, 57, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(51, 64, 12, 58, NULL, 1000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(52, 65, 12, 59, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(53, 66, 12, 60, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(54, 67, 12, 61, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(55, 68, 12, 62, NULL, 2000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(56, 69, 12, 63, NULL, 1000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(57, 70, 12, 64, NULL, 1000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(58, 71, 12, 65, NULL, 1000, 0, 0, '2025-04-04', '2025-04-04 08:47:47', '2025-04-04 08:47:47'),
	(59, 74, 15, 70, NULL, 1, 0, 0, '2025-04-16', '2025-04-16 15:18:30', '2025-04-16 15:18:30'),
	(60, 75, 15, 71, NULL, 2, 0, 0, '2025-04-16', '2025-04-16 15:18:30', '2025-04-16 15:18:30'),
	(61, 77, NULL, NULL, NULL, 0, 0, 0, NULL, '2025-05-15 14:58:57', '2025-05-15 14:59:18'),
	(62, 77, 16, 72, NULL, 1, 0, 0, '2025-05-15', '2025-05-15 17:10:22', '2025-05-15 17:10:22'),
	(64, 78, 16, 73, NULL, 1, 0, 0, '2025-05-26', '2025-05-15 17:10:46', '2025-06-02 13:57:24'),
	(71, 1, NULL, NULL, NULL, 0, 0, 1, '2026-03-04', '2026-03-04 12:58:54', '2026-03-04 12:58:54'),
	(72, 1, NULL, NULL, NULL, 0, 0, -11, '2026-03-06', '2026-03-06 12:12:27', '2026-03-06 12:12:27'),
	(73, 1, NULL, NULL, NULL, 0, 0, 11, '2026-03-06', '2026-03-06 12:12:37', '2026-03-06 12:12:37');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

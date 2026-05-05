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

-- Structuur van  tabel modelbuilder.country wordt geschreven
CREATE TABLE IF NOT EXISTS `country` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(4) DEFAULT '',
  `Defaultcurrency_Symbol` varchar(2) NOT NULL DEFAULT '€',
  `Name` varchar(45) DEFAULT NULL,
  `Defaultcurrency_Id` int DEFAULT NULL,
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  `Modified` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE KEY `country_Code_UNIQUE` (`Code`) USING BTREE,
  KEY `FK_Country_Currency_Id` (`Defaultcurrency_Id`),
  CONSTRAINT `FK_Country_Currency_Id` FOREIGN KEY (`Defaultcurrency_Id`) REFERENCES `currency` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpen data van tabel modelbuilder.country: ~9 rows (ongeveer)
DELETE FROM `country`;
INSERT INTO `country` (`Id`, `Code`, `Defaultcurrency_Symbol`, `Name`, `Defaultcurrency_Id`, `Created`, `Modified`) VALUES
	(1, 'NL', '€', 'Nederland', 1, '2022-01-10 16:44:46', '2024-08-30 11:05:14'),
	(2, 'UK', '£', 'Engeland', 2, '2022-01-10 16:44:46', '2022-01-10 16:44:46'),
	(3, 'US', '$', 'Verenigde staten', 3, '2022-01-10 16:44:46', '2022-01-10 16:44:46'),
	(4, 'DE', '€', 'Duitsland', 1, '2022-01-10 16:44:46', '2022-01-10 16:44:46'),
	(5, 'ESP', '€', 'Spanje', 1, '2022-01-10 16:44:46', '2022-01-10 16:44:46'),
	(6, 'CH', '¥', 'China', 4, '2022-01-10 16:44:46', '2026-01-05 13:42:04'),
	(7, 'IT', '€', 'Italë', 1, '2022-01-10 16:44:46', '2022-01-10 16:44:46'),
	(8, 'FR', '€', 'Frankrijk', 1, '2024-08-16 09:31:01', '2024-08-30 10:13:30'),
	(19, 'AT', '€', 'Oostenrijk', 1, '2025-05-15 14:41:29', '2025-05-15 14:41:29');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

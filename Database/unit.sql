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

-- Structuur van  tabel modelbuilder.unit wordt geschreven
CREATE TABLE IF NOT EXISTS `unit` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(25) DEFAULT '',
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  `Modified` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `unit_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpen data van tabel modelbuilder.unit: ~14 rows (ongeveer)
DELETE FROM `unit`;
INSERT INTO `unit` (`Id`, `Name`, `Created`, `Modified`) VALUES
	(1, '', '2022-01-11 12:10:54', '2024-09-10 08:53:20'),
	(2, 'cm', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(3, 'dl', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(4, 'Fles', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(5, 'gr', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(6, 'kg', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(7, 'ltr', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(8, 'mgr', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(9, 'ml', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(10, 'mm', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(11, 'mtr', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(12, 'Set', '2022-01-11 12:10:54', '2022-01-11 12:10:54'),
	(13, 'Stuk', '2022-01-11 12:10:54', '2024-09-10 08:28:53'),
	(18, 'Rol', '2025-03-04 15:24:45', '2025-03-04 15:24:45'),
	(19, 'Klos', '2025-03-04 15:24:57', '2025-03-04 15:24:57');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

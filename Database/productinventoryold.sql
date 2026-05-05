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

-- Structuur van  tabel modelbuilder.productinventoryold wordt geschreven
CREATE TABLE IF NOT EXISTS `productinventoryold` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Product_Id` int DEFAULT NULL,
  `Amount` double DEFAULT NULL,
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  `Modified` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Overview of all products that have or hade an amount in storage';

-- Dumpen data van tabel modelbuilder.productinventoryold: ~4 rows (ongeveer)
DELETE FROM `productinventoryold`;
INSERT INTO `productinventoryold` (`Id`, `Product_Id`, `Amount`, `Created`, `Modified`) VALUES
	(1, 31, 2, '2024-11-22 10:56:50', '2024-11-22 10:58:26'),
	(2, 1, 1, '2024-11-22 10:57:24', '2024-11-22 10:57:24'),
	(3, 2, 1, '2024-11-22 10:57:36', '2024-11-22 10:57:36'),
	(4, 9, 1, '2024-11-22 10:57:46', '2024-11-22 10:57:46'),
	(5, 10, 5, '2024-11-26 14:46:10', '2024-11-26 14:46:10');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

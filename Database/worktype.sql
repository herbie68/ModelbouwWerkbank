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

-- Structuur van  tabel modelbuilder.worktype wordt geschreven
CREATE TABLE IF NOT EXISTS `worktype` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ParentId` int DEFAULT NULL,
  `Name` char(150) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=34 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpen data van tabel modelbuilder.worktype: ~29 rows (ongeveer)
DELETE FROM `worktype`;
INSERT INTO `worktype` (`Id`, `ParentId`, `Name`) VALUES
	(1, NULL, 'Voorbereiding'),
	(2, NULL, 'Opruimen'),
	(3, NULL, 'Romp'),
	(4, 3, 'Kiel'),
	(5, 3, 'Spanten'),
	(6, 3, 'Eerste beplanking'),
	(7, 3, 'Tweede beplanking'),
	(8, 3, 'Achtersteven'),
	(9, 3, 'Afwerking romp'),
	(10, 3, 'Schilderen/Lakken'),
	(11, NULL, 'Dek'),
	(12, 11, 'Dek beplanking'),
	(13, 11, 'Dekbalk en reling'),
	(14, 11, 'Schilderen/Lakken'),
	(15, 11, 'Afwerking dek'),
	(16, NULL, 'Opbouw'),
	(17, 16, 'Bijboten'),
	(18, 16, 'Wapens'),
	(19, 16, 'Ankers'),
	(20, 16, 'Heck'),
	(21, 16, 'Masten'),
	(22, NULL, 'Wand'),
	(23, 22, 'Staande wand'),
	(24, 22, 'Lopend wand'),
	(25, 22, 'Afwerking'),
	(26, 21, 'Ra'),
	(27, 21, 'Kraaiennest'),
	(29, 21, 'Zeilen'),
	(30, NULL, 'Geen');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

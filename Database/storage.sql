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

-- Structuur van  tabel modelbuilder.storage wordt geschreven
CREATE TABLE IF NOT EXISTS `storage` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ParentId` int DEFAULT NULL,
  `FullPath` varchar(400) DEFAULT NULL,
  `Name` varchar(150) DEFAULT NULL,
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  `Modified` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=264 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumpen data van tabel modelbuilder.storage: ~105 rows (ongeveer)
DELETE FROM `storage`;
INSERT INTO `storage` (`Id`, `ParentId`, `FullPath`, `Name`, `Created`, `Modified`) VALUES
	(1, NULL, 'Herberts Werf', 'Herberts Werf', '2022-01-11 09:48:03', '2022-01-11 09:48:03'),
	(2, 1, 'Herberts Werf\\Hoge kast', 'Hoge kast', '2022-01-11 09:48:03', '2022-01-11 09:48:03'),
	(3, 2, 'Herberts Werf\\Hoge kast\\Hoge kast  - Planken', 'Planken', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(4, 3, 'Herberts Werf\\Hoge kast\\Hoge kast  - Planken\\Hoge kast  - Planken: 0e plank', '0e plank', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(5, 3, 'Herberts Werf\\Hoge kast\\Hoge kast  - Planken\\Hoge kast  - Planken: 1e plank', '1e plank', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(6, 3, 'Herberts Werf\\Hoge kast\\Hoge kast  - Planken\\Hoge kast  - Planken: 2e plank', '2e plank', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(7, 3, 'Herberts Werf\\Hoge kast\\Hoge kast  - Planken\\Hoge kast  - Planken: 3e plank', '3e plank', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(8, 3, 'Herberts Werf\\Hoge kast\\Hoge kast  - Planken\\Hoge kast  - Planken: 4e plank', '4e plank', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(9, 3, 'Herberts Werf\\Hoge kast\\Hoge kast  - Planken\\Hoge kast  - Planken: 5e plank', '5e plank', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(10, 2, 'Herberts Werf\\Hoge kast\\Hoge kast  - Zijkant', 'Zijkant', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(11, 10, 'Herberts Werf\\Hoge kast\\Hoge kast  - Zijkant\\Hoge kast  - Zijkant: zijkant 0e klemmenlat', '0e klemmenlat', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(12, 10, 'Herberts Werf\\Hoge kast\\Hoge kast  - Zijkant\\Hoge kast  - Zijkant: zijkant 1e klemmenlat', '1e klemmenlat', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(13, 10, 'Herberts Werf\\Hoge kast\\Hoge kast  - Zijkant\\Hoge kast  - Zijkant: zijkant 2e klemmenlat', '2e klemmenlat', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(14, 10, 'Herberts Werf\\Hoge kast\\Hoge kast  - Zijkant\\Hoge kast  - Zijkant: zijkant 3e klemmenlat', '3e klemmenlat', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(15, 10, 'Herberts Werf\\Hoge kast\\Hoge kast  - Zijkant\\Hoge kast  - Zijkant: zijkant 4e klemmenlat', '4e klemmenlat', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(16, 1, 'Herberts Werf\\Ladenkast', 'Ladenkast', '2022-01-11 09:48:03', '2022-01-11 09:48:03'),
	(23, 1, 'Herberts Werf\\Onderste muurplank', 'Onderste muurplank', '2022-01-11 09:48:03', '2022-01-11 09:48:03'),
	(24, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 01 - Garen (rood) ', 'Bak 01 - Garen (rood) ', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(43, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 02 - Garen (blauw) ', 'Bak 02 - Garen (blauw) ', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(60, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 03 (turquoise)', 'Bak 03 (turquoise)', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(85, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 04', 'Bak 04', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(126, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 05', 'Bak 05', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(151, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06', 'Bak 06', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(157, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 1 vak 06', 'Rij 1 vak 06', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(158, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 1 vak 07', 'Rij 1 vak 07', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(159, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06Onderste muurplank  - Bak 06: rij 1 vak 08', 'Rij 1 vak 08', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(160, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 1 vak 09', 'Rij 1 vak 09', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(161, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 1 vak 10', 'Rij 1 vak 10', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(170, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 2 vak 09', 'Rij 2 vak 09', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(171, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 2 vak 10', 'Rij 2 vak 10', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(180, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 3 vak 09', 'Rij 3 vak 09', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(181, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 3 vak 10', 'Rij 3 vak 10', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(186, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 4 vak 05', 'Rij 4 vak 05', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(187, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 4 vak 06', 'Rij 4 vak 06', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(188, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 4 vak 07', 'Rij 4 vak 07', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(189, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 4 vak 08', 'Rij 4 vak 08', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(190, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 4 vak 09', 'Rij 4 vak 09', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(191, 151, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 06\\Onderste muurplank  - Bak 06: rij 4 vak 10', 'Rij 4 vak 10', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(192, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad', 'Bak 07 - Draad', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(193, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 1 vak 01', 'Rij 1 vak 01', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(194, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 1 vak 02', 'Rij 1 vak 02', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(195, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 1 vak 03', 'Rij 1 vak 03', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(196, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 1 vak 04', 'Rij 1 vak 04', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(197, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 1 vak 05', 'Rij 1 vak 05', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(198, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 2 vak 01', 'Rij 2 vak 01', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(199, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 2 vak 02', 'Rij 2 vak 02', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(200, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 2 vak 03', 'Rij 2 vak 03', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(201, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 2 vak 04', 'Rij 2 vak 04', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(202, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 2 vak 05', 'Rij 2 vak 05', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(203, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 2 vak 06', 'Rij 2 vak 06', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(204, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 2 vak 07', 'Rij 2 vak 07', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(205, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 2 vak 08', 'Rij 2 vak 08', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(206, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 3 vak 01', 'Rij 3 vak 01', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(207, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 3 vak 02', 'Rij 3 vak 02', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(208, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 3 vak 03', 'Rij 3 vak 03', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(209, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 3 vak 04', 'Rij 3 vak 04', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(210, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 3 vak 05', 'Rij 3 vak 05', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(211, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 3 vak 06', 'Rij 3 vak 06', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(212, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 3 vak 07', 'Rij 3 vak 07', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(213, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 3 vak 08', 'Rij 3 vak 08', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(214, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 4 vak 01', 'Rij 4 vak 01', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(215, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 4 vak 02', 'Rij 4 vak 02', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(216, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 4 vak 03', 'Rij 4 vak 03', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(217, 192, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Bak 07 - Draad\\Rij 4 vak 04', 'Rij 4 vak 04', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(218, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Ladenkast (5 lades)', 'Ladenkast (5 lades)', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(219, 218, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Ladenkast (5 lades)\\Onderste muurplank  - Ladenkast (5 lades): Lade rij boven links', 'Lade rij boven links', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(220, 218, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Ladenkast (5 lades)\\Onderste muurplank  - Ladenkast (5 lades): Lade rij boven midden', 'Lade rij boven midden', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(221, 218, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Ladenkast (5 lades)\\Onderste muurplank  - Ladenkast (5 lades): Lade rij boven rechts', 'Lade rij boven rechts', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(222, 218, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Ladenkast (5 lades)\\Onderste muurplank  - Ladenkast (5 lades): Lade rij midden', 'Lade rij midden', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(223, 218, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Ladenkast (5 lades)\\Onderste muurplank  - Ladenkast (5 lades): Lade rij onder', 'Lade rij onder', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(224, 23, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench', 'Workbench', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(225, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: gereedschaphouder', 'Gereedschaphouder', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(226, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: lade 0', 'Top (boven lades)', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(227, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: lade 1', 'Lade 1', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(228, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: lade 2', 'Lade 2', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(229, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: lade 3', 'Lade 3', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(230, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: lade 4', 'Lade 4', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(231, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: lade 5', 'Lade 5', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(232, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: lade 6', 'Lade 6', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(233, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: lade 7', 'Verzamellade', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(234, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: werkruimte', 'Werkruimte', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(235, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: zijvak 1', 'Voorste zijvak', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(236, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: zijvak 2', 'Zijvak 2', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(237, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: zijvak 3', 'Zijvak 3', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(238, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: zijvak 4', 'Zijvak 4', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(239, 224, 'Herberts Werf\\Onderste muurplank  - Op de plank\\Onderste muurplank  - Workbench\\Onderste muurplank  - Workbench: zijvak 5', 'Achterste zijvak', '2022-01-11 09:48:03', '2025-02-18 09:47:11'),
	(240, 1, 'Herberts Werf\\Middelste muurplank', 'Middelste muurplank', '2022-01-11 09:48:03', '2022-01-11 09:48:03'),
	(241, 1, 'Herberts Werf\\Bovenste muurplank', 'Bovenste muurplank', '2022-01-11 09:48:03', '2022-01-11 09:48:03'),
	(242, 1, 'Herberts Werf\\Werkbank', 'Werkbank', '2022-01-11 09:48:03', '2022-01-11 09:48:03'),
	(243, 242, 'Herberts Werf\\Werkbank\\Gereedschaphouder 1', 'Gereedschaphouder 1', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(244, 242, 'Herberts Werf\\Werkbank\\Gereedschaphouder 2', 'Gereedschaphouder 2', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(245, 242, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)', 'Ladenkast (9 lades)', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(246, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Lade 1', 'Lade 1', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(247, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Werkbank  - Ladenkast (9 lades): Lade 2', 'Boven midden', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(248, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Werkbank  - Ladenkast (9 lades): Lade 3', 'Boven rechts', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(249, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Werkbank  - Ladenkast (9 lades): Lade 4', 'Midden links', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(250, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Werkbank  - Ladenkast (9 lades): Lade 5', 'Midden midden', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(251, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Werkbank  - Ladenkast (9 lades): Lade 6', 'Midden rechts', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(252, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Werkbank  - Ladenkast (9 lades): Lade 7', 'Onder links', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(253, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Werkbank  - Ladenkast (9 lades): Lade 8', 'Onder midden', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(254, 245, 'Herberts Werf\\Werkbank\\Werkbank  - Ladenkast (9 lades)\\Werkbank  - Ladenkast (9 lades): Lade 9', 'Onder rechts', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(255, 242, 'Herberts Werf\\Werkbank\\Werkbank  - Onder de werkbak', 'Onder de werkbak', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(256, 242, 'Herberts Werf\\Werkbank\\Werkbank  - Tribune 1 (links)', 'Tribune 1 (links)', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(257, 242, 'Herberts Werf\\Werkbank\\Werkbank  - Tribune 2 (midden)', 'Tribune 2 (lmidden)', '2022-01-11 09:48:03', '2025-02-17 16:31:35'),
	(258, 242, 'Herberts Werf\\Werkbank\\Werkbank  - Tribune 3 (rechts)', 'Tribune 3 (rechts)', '2022-01-11 09:48:03', '2025-02-17 16:31:35');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

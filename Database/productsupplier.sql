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

-- Structuur van  tabel modelbuilder.productsupplier wordt geschreven
CREATE TABLE IF NOT EXISTS `productsupplier` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Product_Id` int NOT NULL DEFAULT '0',
  `Supplier_Id` int NOT NULL DEFAULT '0',
  `ProductNumber` varchar(150) DEFAULT NULL,
  `ProductName` varchar(150) DEFAULT NULL,
  `Price` decimal(11,6) DEFAULT '0.000000',
  `Url` varchar(1024) DEFAULT NULL,
  `PreferredSupplier` tinyint(1) NOT NULL DEFAULT '0',
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  `Modified` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `Product_Id` (`Product_Id`),
  KEY `Supplier_Id` (`Supplier_Id`),
  CONSTRAINT `FK_ProdSupplier_Product_Id` FOREIGN KEY (`Product_Id`) REFERENCES `product` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_ProdSupplier_Supplier_Id` FOREIGN KEY (`Supplier_Id`) REFERENCES `supplier` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=92 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='List for all products per supplier';

-- Dumpen data van tabel modelbuilder.productsupplier: ~69 rows (ongeveer)
DELETE FROM `productsupplier`;
INSERT INTO `productsupplier` (`Id`, `Product_Id`, `Supplier_Id`, `ProductNumber`, `ProductName`, `Price`, `Url`, `PreferredSupplier`, `Created`, `Modified`) VALUES
	(1, 1, 6, '16435', 'Everbuild secondelijm medium viscositeit', 4.070000, 'https://www.toolstation.nl/everbuild-secondelijm-medium-viscositeit/p86374?channable=002a91696400383633373490&gclid=CjwKCAjwiuuRBhBvEiwAFXKaNPVd2npt7UgrPaFN8O48usa7l_bTtQIfm75c8CHHWXG11fiYv3wzexoCGvMQAvD_BwE', 1, '2022-01-10 16:49:00', '2026-02-26 12:08:11'),
	(4, 2, 6, '10893', 'Everbuild secondelijm hoge viscosoteit', 4.070000, 'https://www.toolstation.nl/everbuild-secondelijm-hoge-viscositeit/p78871?channable=002a9169640037383837311e&gclid=CjwKCAjwiuuRBhBvEiwAFXKaNOckRHnltQBvWbSaVg6AoQ1E4xOtL3iBeKXIL4NNTq1Mk8ustRGGfxoCbysQAvD_BwE', 1, '2022-01-10 16:49:00', '2026-02-26 12:04:11'),
	(5, 10, 1, 'TT34', 'What', 2.290000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=an5423%2F15&PN=5423%2D15%2DFairlead%2D15mm%2DAN5423_15%2Ehtml#SID=570', 1, '2022-03-03 13:54:10', '2026-02-26 14:01:39'),
	(6, 11, 1, 'C82010N', 'Rigging Thread 0.10mm Natural (10m)', 1.980000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82010N%2Ehtml#SID=182', 1, '2022-03-17 14:50:40', '2026-02-26 12:04:11'),
	(7, 12, 1, 'C82025N', 'Rigging Thread 0.25mm Natural (10m)', 1.980000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82025N%2Ehtml#SID=182', 1, '2022-03-17 14:50:40', '2026-02-26 12:04:11'),
	(10, 13, 1, 'C82025B', 'Rigging Thread 0.25mm Black (10m)', 1.980000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82025B%2Ehtml#SID=182', 1, '2022-03-18 14:38:08', '2026-02-26 12:04:11'),
	(11, 14, 1, 'C82050B', 'Rigging Thread 0.50mm Black (10m)', 2.140000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82050B%2Ehtml#SID=182', 1, '2022-03-18 14:38:34', '2026-02-26 12:04:11'),
	(14, 17, 1, 'C82100N', 'Rigging Thread 1.00mm Natural (10m)', 2.510000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82100N%2Ehtml#SID=182', 1, '2022-03-22 14:01:31', '2026-02-26 12:04:11'),
	(15, 18, 1, 'A4126/13', 'Rigging Thread 1.30mm Natural (10m)', 2.230000, 'https://www.cornwallmodelboats.co.uk/acatalog/4126-13-Rigging-Cord-Black-1.3mm-x-20mtr-A4126_13.html#SID=3234', 1, '2022-03-22 14:01:31', '2026-02-26 12:04:11'),
	(16, 19, 1, 'C82170N', 'Rigging Thread 1.70mm Natural (5m)', 3.720000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82170N%2Ehtml#SID=182', 1, '2022-03-22 14:01:31', '2026-02-26 12:04:11'),
	(17, 20, 1, 'C82225N', 'Rigging Thread 2.25mm Natural (2.5m)', 5.160000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82250N%2Ehtml#SID=182', 1, '2022-03-22 14:01:31', '2026-02-26 12:04:11'),
	(18, 21, 1, 'C82075B', 'Rigging Thread 0.75mm Black (10m)', 2.330000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82075B%2Ehtml#SID=182', 1, '2022-03-22 14:01:31', '2026-02-26 12:04:11'),
	(19, 22, 1, 'C82100B', 'Rigging Thread 1.00mm Back (10m)', 2.510000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82100B%2Ehtml#SID=182', 1, '2022-03-22 14:01:31', '2026-02-26 12:04:11'),
	(20, 23, 1, 'A4126/13', 'Rigging Thread 1.30mm Black (10m)', 1.860000, 'https://www.cornwallmodelboats.co.uk/acatalog/4126-13-Rigging-Cord-Black-1.3mm-x-20mtr-A4126_13.html#SID=3234', 1, '2022-03-22 14:01:31', '2026-02-26 12:04:11'),
	(21, 24, 1, 'C82180B', 'Rigging Thread 1.80mm Black (5m)', 3.720000, 'https://www.cornwallmodelboats.co.uk/cgi-bin/sh000001.pl?WD=thread&PN=caldercraft_C82180B%2Ehtml#SID=182', 1, '2022-03-22 14:01:31', '2026-02-26 12:04:11'),
	(30, 33, 1, 'A2535/02', 'Walnut Dowl 2mm x 1000mm', 0.000450, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Dowl-2mm-1WD1_0020.html#SID=470', 1, '2025-02-17 08:41:24', '2026-02-26 12:04:11'),
	(31, 34, 1, 'A2535/03', 'Walnut Dowl 3mm x 1000mm', 0.000500, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Dowl-3mm-1WD1_0030.html#SID=470', 1, '2025-02-17 08:57:54', '2026-02-26 12:04:11'),
	(32, 35, 1, 'A2535/04', 'Walnut Dowl 4mm X 1000mm', 0.000590, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Dowl-4mm-1WD1_0040.html#SID=470', 1, '2025-02-18 12:28:40', '2026-02-26 12:04:11'),
	(37, 36, 1, 'A2535/05', 'Walnut Dowl 5mm x 1000mm', 0.000770, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Dowl-5mm-1WD1_0050.html#SID=470', 1, '2025-02-21 16:02:27', '2026-02-26 12:04:11'),
	(38, 37, 1, 'A2535/08', 'Walnut Dowl 8mm x 1000mm', 0.001310, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Dowl-8mm-1WD1_0080.html#SID=470', 1, '2025-02-21 16:41:28', '2026-02-26 12:04:11'),
	(39, 38, 1, 'A2535/10', 'Walnut Dowl 10mm x 1000mm', 0.001580, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Dowl-10mm-1WD1_0100.html#SID=470', 1, '2025-02-24 10:47:00', '2026-02-26 12:04:11'),
	(40, 39, 1, 'A2535/14', 'Walnut Dowl 14mm x 1000mm ', 0.002490, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Dowl-14mm-589109.html#SID=470', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(41, 40, 1, '1WA1.5015', 'Walnut Strip 1.5 x 1.5 x 1000mm', 0.000370, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-1.5x1.5mm-580014.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(42, 41, 1, '580202', 'Walnut Sheet 1.5 x 100 x 1000mm', 0.004790, 'https://www.cornwallmodelboats.co.uk/acatalog/Dibetou-Sheet-1-5mm-A2350_15.html#SID=471', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(43, 42, 1, 'CB2189', 'Medium Deck Cleat Type 2 8.5mm (5)', 0.270000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft2189.html#SID=3184', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(44, 43, 1, 'C8103DEW', 'Deadeye Walnut 3mm (10)', 0.105000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8103DEW.html#SID=3189', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(45, 44, 1, 'C8105DEW', 'Deadeye Walnut 5mm (10)', 0.105000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8105DEW.html#SID=3189', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(46, 45, 1, 'A4298/03', 'Mast Bowsprit Cap 18mm', 0.280000, 'https://www.cornwallmodelboats.co.uk/acatalog/4298-03-Mast-Bowsprit-Cap-18mm-A4298_03.html#SID=3216', 0, '2025-02-25 13:25:26', '2025-02-25 13:25:26'),
	(47, 46, 1, 'A4383/06', 'Metal Gaff Jaw with Parral beads 6mm', 0.890000, 'https://www.cornwallmodelboats.co.uk/acatalog/4383-06-Metal-Gaff-Jaw-with-Parral-beads--6mm-A4383_06.html#SID=3216', 0, '2025-02-25 13:25:26', '2025-02-25 13:25:26'),
	(48, 47, 1, 'A4404/01', 'Photo-etched 2 Tier Parral Ribs 8mm', 0.890000, 'https://www.cornwallmodelboats.co.uk/acatalog/4404-01-Photo-etched-2-Tier-Parral-Ribs-8mm-A4404_01.html#SID=3216', 0, '2025-02-25 13:25:26', '2025-02-25 13:25:26'),
	(49, 48, 1, 'BF501', 'Mast Ring 4mm', 0.260000, 'https://www.cornwallmodelboats.co.uk/acatalog/Billing-boats-fittings-F501-Mast-Ring-4mm-BF501.html#SID=3216', 0, '2025-02-25 13:25:26', '2025-02-25 13:25:26'),
	(50, 49, 1, 'C83550', 'Brass Parrel Ribs (40)', 0.066250, 'https://www.cornwallmodelboats.co.uk/acatalog/Caldercraft-Model-Boat-Fittings-Brass-Parrel-Ribs-C83550.html#SID=3216', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(51, 50, 1, 'CB2128', 'Closed Yard Ring 3mm (4)', 0.250000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraftb2128.html#SID=3216', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(52, 51, 1, 'A4298/02', 'Mast Bowsprit Cap 13mm', 0.240000, 'https://www.cornwallmodelboats.co.uk/acatalog/4298-02-Mast-Bowsprit-Cap-13mm-A4298_02.html#SID=3216', 0, '2025-02-25 13:25:26', '2025-02-25 13:25:26'),
	(53, 52, 1, 'A4296/02', 'British Crosstree Wood 25x27mm', 0.830000, 'https://www.cornwallmodelboats.co.uk/acatalog/4296-02-British-Crosstree-Wood-25x27mm-A4296_02.html#SID=3216', 0, '2025-02-25 13:25:26', '2025-02-25 13:25:26'),
	(54, 53, 1, 'C8103DW', 'Block Double Walnut 3mm (10)', 0.105000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8103DW.html#SID=385', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(55, 54, 1, 'C8103SW', 'Block Single Walnut 3mm (10)', 0.105000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8103SW.html#SID=385', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(56, 55, 1, 'C8105DW', 'Block Double Walnut 5mm (10)', 0.105000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8105DW.html#SID=385', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(57, 56, 1, 'C8105SW', 'Block Single Walnut 5mm (10)', 0.105000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8105SW.html#SID=385', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(58, 57, 1, 'C8107DW', 'Block Double Walnut 7mm (10)', 0.105000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8107SW.html#SID=385', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(59, 58, 1, 'C8107SW', 'Block Single Walnut 7mm (10)', 0.105000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8107SW.html#SID=385', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(60, 59, 1, 'C8107HBW', 'Heart Block Walnut 7mm (2)', 0.125000, 'https://www.cornwallmodelboats.co.uk/acatalog/caldercraft8107HBW.html#SID=385', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(61, 60, 1, 'OC19200', 'Occre Rapid White Glue 100ml', 4.160000, 'https://www.cornwallmodelboats.co.uk/acatalog/Occre-Rapid-White-Glue-OC19200.html#SID=478', 0, '2025-02-25 13:25:26', '2025-02-25 13:25:26'),
	(62, 61, 1, 'A2460/05', 'Walnut Strip 1 x 3 x 1000mm', 0.000360, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-1x3mm-580005.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(63, 62, 1, 'A2460/04', 'Walnut Strip 1 x 2 x 1000mm', 0.000330, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-1x2mm-580004.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(64, 63, 1, 'A2410/01', 'Walnut Strip 1 x 1 x 1000mm', 0.000310, 'https://www.cornwallmodelboats.co.uk/acatalog/Amati-Walnut-Strip-1x1mm-A2410_01.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(65, 64, 1, 'A2460/14', 'Walnut Strip 1 x 5 x 1000mm', 0.000410, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-1x5mm-580006.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(66, 65, 1, 'A2460/19', 'Walnut Strip 1.5 x 4 x 1000mm', 0.000410, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-1.5x4mm-580038.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(67, 66, 1, 'A2460/07', 'Walnut Strip 1.5 x 5 x 1000mm', 0.000430, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-1.5x5mm-580008.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(68, 67, 1, '1WA1.5030', 'Walnut Strip 1.5 x 3 x 1000mm', 0.000410, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-1.5x3mm-1WA1_5030.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(69, 68, 1, '1WA1.5020', 'Walnut Strip 1.5 x 2 x 1000mm', 0.000390, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-1.5x2mm-1WA1_5020.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(70, 69, 1, 'A2410/03', 'Walnut Strip 3 x 3 x 1000mm', 0.000510, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-3x3mm-580016.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(71, 70, 1, 'A2410/04', 'Walnut Strip 4 x 4 x 1000mm', 0.000600, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-4x4mm-580017.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(72, 71, 1, 'A2410/05', 'Walnut Strip 5 x 5 x 1000mm', 0.000770, 'https://www.cornwallmodelboats.co.uk/acatalog/Walnut-Strip-5x5mm-580018.html#SID=472', 0, '2025-02-25 13:25:26', '2025-03-04 15:24:00'),
	(76, 72, 13, '19419', 'Satin varnish 50 ml', 4.990000, 'https://occre.com/en/products/satin-varnish-50-ml', 1, '2025-03-11 12:14:07', '2026-02-26 12:04:11'),
	(77, 73, 13, '19399', 'Acrylic Paint Yellow 20ml', 2.990000, 'https://occre.com/en/products/amarillo-20-ml', 1, '2025-03-11 12:26:47', '2026-02-26 12:04:11'),
	(78, 1, 2, '16435', 'Everbuild secondelijm medium viscositeit', 4.070000, NULL, 0, '2025-03-26 16:12:45', '2026-02-25 09:53:31'),
	(79, 2, 2, '10893', 'Everbuild secondelijm hoge viscosoteit', 4.070000, NULL, 0, '2025-03-26 16:12:45', '2026-02-25 09:53:31'),
	(80, 9, 2, '', 'Zip Kicker', 0.000000, NULL, 0, '2025-03-26 16:12:45', '2026-02-25 09:53:31'),
	(81, 10, 4, 'AN5423/15', 'Fairlead 15mm', 2.290000, NULL, 0, '2025-03-26 16:35:24', '2026-02-25 09:53:31'),
	(82, 74, 16, '19-74020', 'Schwanheimer Industriekleber Nr. 100 / 20g', 23.500000, 'https://schwanheimer-industriekleber.de/industriekleber/industriekleber-nr.100/43/schwanheimer-industriekleber-nr.-100/20g?c=20', 1, '2025-04-16 13:14:50', '2026-02-26 12:04:11'),
	(83, 75, 16, '19-17010', 'Micro-Kapillardüse / Feindosierspitze (2-er Set)', 3.200000, 'https://schwanheimer-industriekleber.de/dosierspitzen/54/micro-kapillarduese/feindosierspitze-2-er-set?c=44', 1, '2025-04-16 14:40:43', '2026-02-26 12:04:11'),
	(84, 76, 16, '19-17060', 'Micro-Kapillardüse / Feindosierspitze (6-er Set)', 7.280000, 'https://schwanheimer-industriekleber.de/dosierspitzen/54/micro-kapillarduese/feindosierspitze-2-er-set?c=44', 1, '2025-04-16 14:40:43', '2026-02-26 12:04:11'),
	(85, 77, 17, '160 141', 'Unimat 1 Classic', 450.000000, 'https://shop.thecooltool.com/products/unimat-1-classic', 1, '2025-05-15 14:58:11', '2026-02-26 12:04:11'),
	(86, 78, 17, '162 400', 'Unimat Maschinenbett überlang 460mm', 40.000000, 'https://shop.thecooltool.com/products/maschinenbett-uberlang-460-mm', 1, '2025-05-15 17:08:09', '2026-02-26 12:04:11'),
	(87, 79, 1, 'A4120/10', 'Cask Wood 10mm', 0.022000, 'https://www.cornwallmodelboats.co.uk/acatalog/4120-10-Cask-Wood-10mm-A4120_10.html#SID=3176', 1, '2025-05-16 08:48:24', '2026-02-26 12:04:11'),
	(88, 80, 1, 'A4120/16', 'Cask Wood 16mm', 0.023000, 'https://www.cornwallmodelboats.co.uk/acatalog/4120-16-Cask-Wood-16mm-A4120_16.html#SID=3176', 1, '2025-05-16 08:48:24', '2026-02-26 12:04:11'),
	(89, 81, 1, 'A4120/22', 'Cask Wood 22mm', 0.026000, 'https://www.cornwallmodelboats.co.uk/acatalog/4120-22-Cask-Wood-22mm-A4120_22.html#SID=3176', 1, '2025-05-16 08:48:24', '2026-02-26 12:04:11');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

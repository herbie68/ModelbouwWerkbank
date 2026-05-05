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

-- Structuur van  procedure modelbuilder.GetProductStockMovements wordt geschreven
DELIMITER //
CREATE PROCEDURE `GetProductStockMovements`(IN SelectedProductId INT)
BEGIN
    -- Actuele voorraad als aparte tijdelijke tabel
    CREATE TEMPORARY TABLE tmp_current_stock AS
    SELECT 
        SelectedProductId AS ProductId,
        IFNULL(SUM(sl.AmountReceived), 0) +
        IFNULL(SUM(sl.AmountCorrection), 0) -
        IFNULL(SUM(sl.AmountUsed), 0) AS CurrentStock
    FROM modelbuilder.stocklog sl
    WHERE sl.product_Id = SelectedProductId;

    -- Eerst de actuele voorraad teruggeven
    SELECT * FROM tmp_current_stock;

    -- Dan het mutatie-overzicht
    SELECT
        sl.LogDate AS Datum,
        CASE
            WHEN sl.AmountCorrection != 0 THEN 'Correctie'
            WHEN sl.AmountUsed != 0 THEN 'Gebruikt'
            WHEN sl.AmountReceived != 0 THEN 'Ontvangen'
            ELSE 'Onbekend'
        END AS Mutatie,
        
        CASE 
            WHEN sl.AmountUsed != 0 THEN prj.Name 
            ELSE NULL 
        END AS Project,

        so.OrderNumber AS Bestelling,
        sup.Name AS Leverancier,

        CASE 
            WHEN sl.AmountCorrection > 0 THEN sl.AmountCorrection
            WHEN sl.AmountCorrection = 0 THEN sl.AmountReceived
            ELSE NULL
        END AS InAantal,

        CASE 
            WHEN sl.AmountCorrection < 0 THEN ABS(sl.AmountCorrection)
            WHEN sl.AmountCorrection = 0 THEN sl.AmountUsed
            ELSE NULL
        END AS UitAantal

    FROM modelbuilder.stocklog sl
    LEFT JOIN modelbuilder.productusage pu ON sl.productusage_Id = pu.Id
    LEFT JOIN modelbuilder.project prj ON pu.project_Id = prj.Id
    LEFT JOIN modelbuilder.supplyorder so ON sl.supplyorder_Id = so.Id
    LEFT JOIN modelbuilder.supplier sup ON so.supplier_Id = sup.Id
    WHERE sl.product_Id = SelectedProductId
    ORDER BY sl.LogDate;

    -- Opruimen
    DROP TEMPORARY TABLE tmp_current_stock;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

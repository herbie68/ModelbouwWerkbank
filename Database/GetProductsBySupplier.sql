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

-- Structuur van  procedure modelbuilder.GetProductsBySupplier wordt geschreven
DELIMITER //
CREATE PROCEDURE `GetProductsBySupplier`(IN SelectedSupplierId INT)
BEGIN
    SELECT 
        p.Id AS Product_Id,
        p.Code AS Code,
        p.Name AS Name,
        COALESCE(ps.ProductName, p.Name) AS SupplierProductName,
        REPLACE(IFNULL(CAST(p.Price AS CHAR), '0'), '.', ',') AS Price,
        p.MinimalStock AS MinimalStock,
        p.StandardOrderQuantity as OrderPer,
        c.Id AS Category,
        IFNULL(pi.Amount, 0) AS Inventory,
        (
            SELECT IFNULL(SUM(so.OpenAmount), 0)
            FROM modelbuilder.supplyorderline so
            WHERE so.Product_Id = p.Id
        ) AS InOrder,
        GREATEST(
            (p.MinimalStock - (IFNULL(pi.Amount, 0) + (
                SELECT IFNULL(SUM(so.OpenAmount), 0)
                FROM modelbuilder.supplyorderline so
                WHERE so.Product_Id = p.Id
            ))),
            0
        ) AS Short,
        CASE 
    WHEN GREATEST(
        (p.MinimalStock - (IFNULL(pi.Amount, 0) + (
            SELECT IFNULL(SUM(so.OpenAmount), 0)
            FROM modelbuilder.supplyorderline so
            WHERE so.Product_Id = p.Id
        ))),
        0
    ) > 0 
    THEN 
        CEILING(GREATEST(
            (p.MinimalStock - (IFNULL(pi.Amount, 0) + (
                SELECT IFNULL(SUM(so.OpenAmount), 0)
                FROM modelbuilder.supplyorderline so
                WHERE so.Product_Id = p.Id
            ))),
            0
        ) / p.StandardOrderQuantity) * p.StandardOrderQuantity
    ELSE 
        p.StandardOrderQuantity
END AS ToOrder,
        IFNULL(ps.ProductNumber, '') AS SupplierProductNumber,
        REPLACE(IFNULL(CAST(ps.Price AS CHAR), '0'), '.', ',') AS SupplierPrice,
        IFNULL(ps.Currency_Id, 1) AS Currency_Id,
        IFNULL(cr.Symbol, (SELECT Symbol FROM modelbuilder.currency WHERE Id = 1)) AS CurrencySymbol,
        CASE 
            WHEN ps.Supplier_Id = SelectedSupplierId THEN 'Bekend bij geselecteerde leverancier'
            ELSE 'Onbekend bij geselecteerde leverancier'
        END AS FromSupplier
    FROM
        modelbuilder.product p
    LEFT JOIN modelbuilder.category c ON p.Category_Id = c.Id
    LEFT JOIN modelbuilder.view_instock pi ON p.Id = pi.Product_Id
    LEFT JOIN modelbuilder.productsupplier ps ON p.Id = ps.Product_Id
    LEFT JOIN modelbuilder.currency cr ON ps.Currency_Id = cr.Id;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

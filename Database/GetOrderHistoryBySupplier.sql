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

-- Structuur van  procedure modelbuilder.GetOrderHistoryBySupplier wordt geschreven
DELIMITER //
CREATE PROCEDURE `GetOrderHistoryBySupplier`(IN SupplierId INT)
begin
	select 
		ord.Supplier_Id as SupplierId,
		ord.Id as OrderId, 
		ord.OrderNumber, 
		ord.OrderDate, 
		ord.OrderCosts, 
		ord.ShippingCosts, 
		ord.CurrencyConversionRate,
		stl.LogDate as Received,
		orl.Product_Id,
		prd.Code as ProductNumber,
		prd.Name as Description,
		orl.Price * ord.CurrencyConversionRate as Price,
		orl.Amount,
		orl.Price * orl.Amount * ord.CurrencyConversionRate as RowTotal,
		(
			SELECT 
				SUM(orl2.Price * orl2.Amount * ord.CurrencyConversionRate)
			FROM modelbuilder.supplyorderline orl2
			WHERE orl2.Supplyorder_Id = ord.Id
		) + ord.OrderCosts + ord.ShippingCosts AS OrderTotal
	from modelbuilder.supplyorder ord 
	join modelbuilder.supplyorderline orl on ord.Id = orl.Supplyorder_Id
	join modelbuilder.stocklog stl on orl.Id = stl.supplyorderline_Id
	join modelbuilder.product prd on orl.Product_Id = prd.Id 
	where Supplier_Id = SupplierId
	ORDER BY stl.LogDate, prd.Code;
end//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

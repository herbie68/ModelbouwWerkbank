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

-- Structuur van  procedure modelbuilder.GetSupplyOrderDetails wordt geschreven
DELIMITER //
CREATE PROCEDURE `GetSupplyOrderDetails`(IN p_SupplyOrderId INT)
BEGIN
SELECT
    ord.Id AS `Order_Id`,
    orl.Id AS `OrderLine_Id`,
    orl.product_Id AS `Product_Id`,
    ps.ProductNumber AS `SupplierProductNumber`,
    ps.ProductName AS `SupplierProductName`,
    orl.Amount AS `Ordered`,
    orl.OpenAmount AS `WaitFor`,
    COALESCE(
        (SELECT SUM(AmountReceived) 
         FROM modelbuilder.stocklog sl 
         WHERE sl.product_id = orl.product_id 
           AND sl.SupplyOrder_Id = p_SupplyOrderId
           AND sl.supplyorderline_Id = orl.Id
        ), 0) AS `StockLogRecieved`,
    COALESCE(st.Amount, 0) AS `InStock`
FROM modelbuilder.supplyorderline orl
left join modelbuilder.supplyorder so on orl.Supplyorder_Id = so.Id
LEFT JOIN `modelbuilder`.`supplyorder` `ord` ON `orl`.`supplyorder_Id` = `ord`.`Id`
LEFT JOIN `modelbuilder`.`productsupplier` `ps` ON orl.product_id = ps.product_id and so.Supplier_Id = ps.Supplier_Id
LEFT JOIN `modelbuilder`.`view_instock` `st` ON `orl`.`product_id` = `st`.`product_id`
WHERE orl.Supplyorder_Id = p_SupplyOrderId AND orl.OpenAmount > 0;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

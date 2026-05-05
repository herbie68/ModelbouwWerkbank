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

-- Structuur van  procedure modelbuilder.DeleteProductId wordt geschreven
DELIMITER //
CREATE PROCEDURE `DeleteProductId`(
    IN p_ProductId INT,
    OUT result INT
)
BEGIN
    -- Declare variables to store the count of references in other tables
    DECLARE productInUse INT DEFAULT 0;

    -- Check if the ProductId exists in any of the specified tables
    SELECT COUNT(*)
    INTO productInUse
    FROM (
        SELECT product_id FROM supplyorderline WHERE product_id = p_ProductId
        UNION
        SELECT product_id FROM productusage WHERE product_id = p_ProductId
        UNION
        SELECT product_id FROM stock WHERE product_id = p_ProductId
    ) AS temp;

    IF productInUse > 0 THEN
        -- Product is used in at least one of the tables, hide it
        UPDATE Product
        SET Hide = 1
        WHERE Id = p_ProductId;

        -- Return result 1 to indicate the product was hidden
        SET result = 1;
    ELSE
        -- Product is not used, delete from Product table
        DELETE FROM Product
        WHERE Id = p_ProductId;

        -- Delete associated records from productsupplier
        DELETE FROM productsupplier
        WHERE product_id = p_ProductId;

        -- Return result 0 to indicate the product was deleted
        SET result = 0;
    END IF;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

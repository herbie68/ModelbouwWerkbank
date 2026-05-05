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

-- Structuur van  functie modelbuilder.GetCategoryPath wordt geschreven
DELIMITER //
CREATE FUNCTION `GetCategoryPath`(
	`category_id` INT
) RETURNS text CHARSET utf8mb4
    DETERMINISTIC
    COMMENT 'Get breadcrump path for selected categoryId'
BEGIN
    DECLARE category_path TEXT;
    DECLARE parent_id INT;
    DECLARE category_name TEXT;

    -- Haal de eerste categorie op
    SELECT Name, ParentId INTO category_name, parent_id
    FROM modelbuilder.category
    WHERE Id = category_id;

    -- Bouw het pad op
    SET category_path = category_name;

    -- Loop door de parent categories
    WHILE parent_id IS NOT NULL DO
        SELECT Name, ParentId INTO category_name, parent_id
        FROM modelbuilder.category
        WHERE Id = parent_id;

        -- Voeg toe aan het pad
        SET category_path = CONCAT(category_name, ' \\ ', category_path);
    END WHILE;

    RETURN category_path;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

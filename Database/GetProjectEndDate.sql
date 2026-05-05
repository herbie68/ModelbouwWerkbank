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

-- Structuur van  procedure modelbuilder.GetProjectEndDate wordt geschreven
DELIMITER //
CREATE PROCEDURE `GetProjectEndDate`(
    IN p_ProductId INT
)
BEGIN
    DECLARE totalWorkedHours DOUBLE;
    DECLARE averageHoursPerWeek DOUBLE;
    DECLARE elapsedDays INT;
    DECLARE requiredWeeks DOUBLE;
    DECLARE projectedEndDate DATE;
    DECLARE totalEstimatedHours DOUBLE;

    -- Retrieve ExpectedTime from the project table
    SELECT ExpectedTime
    INTO totalEstimatedHours
    FROM project
    WHERE Id = p_ProductId;

    -- Calculate total worked hours for the project
    SELECT SUM(TIMESTAMPDIFF(MINUTE, StartTime, EndTime) / 60)
    INTO totalWorkedHours
    FROM `time`
    WHERE project_Id = p_ProductId 
        AND StartTime IS NOT NULL 
        AND EndTime IS NOT NULL;

    -- Calculate the number of days since the first workday to today
    SELECT DATEDIFF(CURDATE(), MIN(WorkDate))
    INTO elapsedDays
    FROM `time`
    WHERE project_Id = p_ProductId;

    -- Calculate the average hours per week
    SET averageHoursPerWeek = IF(elapsedDays > 0, (totalWorkedHours / (elapsedDays / 7)), 0);

    -- Calculate required weeks to complete the remaining work
    SET requiredWeeks = IF(averageHoursPerWeek > 0, (totalEstimatedHours - totalWorkedHours) / averageHoursPerWeek, NULL);

    -- Calculate the projected end date
    SET projectedEndDate = DATE_ADD(CURDATE(), INTERVAL requiredWeeks * 7 DAY);

    -- Return the projected end date
    SELECT projectedEndDate AS ProjectedEndDate;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

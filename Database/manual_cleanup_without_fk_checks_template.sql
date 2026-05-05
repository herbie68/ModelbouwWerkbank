-- Template for one-off manual cleanup actions.
-- This disables foreign-key checks only for this session/connection.
-- Use this when you do not want to permanently remove constraints yet.
--
-- Important: run the whole script in one HeidiSQL query tab/connection.
-- Replace the example DELETE statements with your cleanup actions.

USE `modelbuilder`;

START TRANSACTION;

SET @OLD_FOREIGN_KEY_CHECKS = @@FOREIGN_KEY_CHECKS;
SET FOREIGN_KEY_CHECKS = 0;

-- Example:
-- DELETE FROM `stocklog` WHERE `Id` IN (71, 72, 73);
-- DELETE FROM `stock` WHERE `product_Id` = 1;
-- DELETE FROM `product` WHERE `Id` = 1;

SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;

COMMIT;


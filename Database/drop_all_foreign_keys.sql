-- Permanently removes all foreign key constraints from the selected database.
-- This does not drop ordinary indexes. Indexes do not block deletes and are
-- still useful for query performance.
--
-- Make a backup first. After running this script, the application/database
-- will no longer enforce parent-child integrity at database level.

USE `modelbuilder`;

DELIMITER $$

DROP PROCEDURE IF EXISTS DropAllForeignKeys$$

CREATE PROCEDURE DropAllForeignKeys()
BEGIN
    DECLARE done TINYINT DEFAULT 0;
    DECLARE v_schema VARCHAR(64);
    DECLARE v_table VARCHAR(64);
    DECLARE v_constraint VARCHAR(64);

    DECLARE fk_cursor CURSOR FOR
        SELECT
            CONSTRAINT_SCHEMA,
            TABLE_NAME,
            CONSTRAINT_NAME
        FROM information_schema.REFERENTIAL_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = DATABASE()
        ORDER BY TABLE_NAME, CONSTRAINT_NAME;

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

    OPEN fk_cursor;

    drop_loop: LOOP
        FETCH fk_cursor INTO v_schema, v_table, v_constraint;

        IF done = 1 THEN
            LEAVE drop_loop;
        END IF;

        SET @drop_fk_sql = CONCAT(
            'ALTER TABLE `', REPLACE(v_schema, '`', '``'),
            '`.`', REPLACE(v_table, '`', '``'),
            '` DROP FOREIGN KEY `', REPLACE(v_constraint, '`', '``'), '`'
        );

        SELECT @drop_fk_sql AS executing_statement;
        PREPARE drop_fk_stmt FROM @drop_fk_sql;
        EXECUTE drop_fk_stmt;
        DEALLOCATE PREPARE drop_fk_stmt;
    END LOOP;

    CLOSE fk_cursor;
END$$

DELIMITER ;

CALL DropAllForeignKeys();
DROP PROCEDURE IF EXISTS DropAllForeignKeys;

SELECT
    COUNT(*) AS remaining_foreign_keys
FROM information_schema.REFERENTIAL_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = DATABASE();


-- Shows all foreign keys that currently exist in the selected database.
-- Run this while connected to the modelbuilder database.

USE `modelbuilder`;

SELECT
    kcu.TABLE_NAME AS child_table,
    kcu.CONSTRAINT_NAME,
    kcu.COLUMN_NAME AS child_column,
    kcu.REFERENCED_TABLE_NAME AS parent_table,
    kcu.REFERENCED_COLUMN_NAME AS parent_column,
    rc.UPDATE_RULE,
    rc.DELETE_RULE
FROM information_schema.KEY_COLUMN_USAGE kcu
JOIN information_schema.REFERENTIAL_CONSTRAINTS rc
    ON rc.CONSTRAINT_SCHEMA = kcu.CONSTRAINT_SCHEMA
    AND rc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
    AND rc.TABLE_NAME = kcu.TABLE_NAME
WHERE kcu.CONSTRAINT_SCHEMA = DATABASE()
    AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
ORDER BY
    kcu.TABLE_NAME,
    kcu.CONSTRAINT_NAME,
    kcu.ORDINAL_POSITION;


﻿CREATE TABLE a (
	S INT NOT NULL AUTO_INCREMENT,
    V INT NOT NULL,
    PRIMARY KEY (S)
);
CREATE TABLE b (
	S INT NOT NULL AUTO_INCREMENT,
    V INT NOT NULL,
    PRIMARY KEY (S)
);
CREATE TABLE c (
	S INT NOT NULL AUTO_INCREMENT,
    V INT NOT NULL,
    PRIMARY KEY (S)
);
CREATE TABLE d (
	S INT NOT NULL AUTO_INCREMENT,
    V INT NOT NULL,
    PRIMARY KEY (S)
);

DROP PROCEDURE IF EXISTS LoadTestData_A;
CREATE PROCEDURE LoadTestData_A()
BEGIN
	DECLARE v_max INT UNSIGNED DEFAULT 10;
	DECLARE v_counter INT UNSIGNED DEFAULT 0;

	TRUNCATE TABLE a;
	START TRANSACTION;
	WHILE v_counter < v_max DO
		INSERT INTO a (v) VALUES ( 10 );
        SET v_counter=v_counter+1;
	END WHILE;
	COMMIT;
END;

DROP PROCEDURE IF EXISTS LoadTestData_B;
CREATE PROCEDURE LoadTestData_B()
BEGIN
	DECLARE v_max INT UNSIGNED DEFAULT 50;
	DECLARE v_counter INT UNSIGNED DEFAULT 0;

	TRUNCATE TABLE b;
	START TRANSACTION;
	WHILE v_counter < v_max DO
		INSERT INTO b (V) VALUES ( 100 );
        SET v_counter=v_counter+1;
	END WHILE;
	COMMIT;
END;

DROP PROCEDURE IF EXISTS LoadTestData_C;
CREATE PROCEDURE LoadTestData_C()
BEGIN
	DECLARE v_max INT UNSIGNED DEFAULT 100;
	DECLARE v_counter INT UNSIGNED DEFAULT 0;

	TRUNCATE TABLE c;
	START TRANSACTION;
	WHILE v_counter < v_max DO
		INSERT INTO c (V) VALUES ( 1000 );
        SET v_counter=v_counter+1;
	END WHILE;
	COMMIT;
END;

DROP PROCEDURE IF EXISTS LoadTestData_D;
CREATE PROCEDURE LoadTestData_D()
BEGIN
	DECLARE v_max INT UNSIGNED DEFAULT 500;
	DECLARE v_counter INT UNSIGNED DEFAULT 0;

	TRUNCATE TABLE d;
	START TRANSACTION;
	WHILE v_counter < v_max DO
		INSERT INTO d (V) VALUES ( 10000 );
        SET v_counter=v_counter+1;
	END WHILE;
	COMMIT;
END;

CALL LoadTestData_A();
CALL LoadTestData_B();
CALL LoadTestData_C();
CALL LoadTestData_D();

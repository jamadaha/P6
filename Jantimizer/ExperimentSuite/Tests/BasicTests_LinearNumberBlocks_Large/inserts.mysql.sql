﻿DROP PROCEDURE IF EXISTS LoadTestData_A;
CREATE PROCEDURE LoadTestData_A()
BEGIN
	DECLARE v_max INT UNSIGNED DEFAULT 50;
	DECLARE v_counter INT UNSIGNED DEFAULT 0;

	START TRANSACTION;
	WHILE v_counter < v_max DO
		INSERT INTO a (v) VALUES ( v );
        SET v_counter=v_counter+1;
	END WHILE;
	COMMIT;
END;

DROP PROCEDURE IF EXISTS LoadTestData_B;
CREATE PROCEDURE LoadTestData_B()
BEGIN
	DECLARE v_max INT UNSIGNED DEFAULT 250;
	DECLARE v_counter INT UNSIGNED DEFAULT 0;

	START TRANSACTION;
	WHILE v_counter < v_max DO
		INSERT INTO b (V) VALUES ( 30 - v );
        SET v_counter=v_counter+1;
	END WHILE;
	COMMIT;
END;

DROP PROCEDURE IF EXISTS LoadTestData_C;
CREATE PROCEDURE LoadTestData_C()
BEGIN
	DECLARE v_max INT UNSIGNED DEFAULT 500;
	DECLARE v_counter INT UNSIGNED DEFAULT 0;

	START TRANSACTION;
	WHILE v_counter < v_max DO
		INSERT INTO c (V) VALUES ( 10 + v );
        SET v_counter=v_counter+1;
	END WHILE;
	COMMIT;
END;

DROP PROCEDURE IF EXISTS LoadTestData_D;
CREATE PROCEDURE LoadTestData_D()
BEGIN
	DECLARE v_max INT UNSIGNED DEFAULT 2500;
	DECLARE v_counter INT UNSIGNED DEFAULT 0;

	START TRANSACTION;
	WHILE v_counter < v_max DO
		INSERT INTO d (V) VALUES ( (-200) + v );
        SET v_counter=v_counter+1;
	END WHILE;
	COMMIT;
END;

CALL LoadTestData_A();
CALL LoadTestData_B();
CALL LoadTestData_C();
CALL LoadTestData_D();
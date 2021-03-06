CREATE DATABASE IF NOT EXISTS census2013;
USE census2013;

SET GLOBAL max_allowed_packet=1073741824;

DROP TABLE IF EXISTS census;
CREATE TABLE census (
	id BIGINT AUTO_INCREMENT,
	age BIGINT,
	workclass varchar(100),
	education varchar(100),
	education_num BIGINT,
	marital_status varchar(100),
	occupation varchar(100),
	relationship varchar(100),
	race varchar(100),
	sex varchar(100),
	capital_gain BIGINT,
	capital_loss BIGINT,
	hours_per_week BIGINT,
	native_country varchar(100),
    PRIMARY KEY (id)
) COMMENT='Data from https://github.com/sfu-db/AreCELearnedYet#dataset';
﻿CREATE SCHEMA IF NOT EXISTS basictests ;
SET search_path TO basictests;

DROP TABLE IF EXISTS a;
DROP TABLE IF EXISTS b;
DROP TABLE IF EXISTS c;
DROP TABLE IF EXISTS d;

CREATE TABLE a (
	S SERIAL PRIMARY KEY,
    V BIGINT NOT NULL
);
CREATE TABLE b (
	S SERIAL PRIMARY KEY,
    V BIGINT NOT NULL
);
CREATE TABLE c (
	S SERIAL PRIMARY KEY,
    V BIGINT NOT NULL
);
CREATE TABLE d (
	S SERIAL PRIMARY KEY,
    V BIGINT NOT NULL
);

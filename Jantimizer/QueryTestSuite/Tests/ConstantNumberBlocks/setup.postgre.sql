﻿CREATE TABLE A AS SELECT generate_Series(1,10) AS s, (10) AS v;
CREATE TABLE B AS SELECT generate_Series(1,100) AS s, (100) AS v;
CREATE TABLE C AS SELECT generate_Series(1,1000) AS s, (1000) AS v;
CREATE TABLE D AS SELECT generate_Series(1,10000) AS s, (10000) AS v;

ANALYZE;
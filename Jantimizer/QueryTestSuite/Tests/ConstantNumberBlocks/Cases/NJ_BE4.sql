select * from (((a join b on a."int4" >= b."int4") join c on b."int4" >= c."int4") join d on c."int4" >= d."int4");

SELECT * FROM ((a JOIN b ON a.v < b.v) JOIN c ON b.v < c.v) JOIN d ON c.v < d.v

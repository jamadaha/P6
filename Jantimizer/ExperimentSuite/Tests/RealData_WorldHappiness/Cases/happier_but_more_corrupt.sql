﻿SELECT c1.country, c2.country
    FROM world_happiness_2022 c1, world_happiness_2022 c2
    WHERE
          c1.happiness_score > c2.happiness_score AND
          c1.perception_of_corruption > c2.perception_of_corruption

using System;
using System.Text;
using System.IO;

class MazeGen
{
    static void Main()
    {
        string[] maze =
        {
"######################################### ###",
"#.......................................#   #",
"#..#################################....#...#",
"#..#.................................#...*...#",
"#..#..#####################.........#.......#",
"#..#..#...................#.........#.......#",
"#..#..#....####....####...#.........#.......#",
"#..#...............#......#.........#.......#",
"#..#..#################...#...#######.......#",
"#..#..#.................#...............#...#",
"#..####....#########....#...#########....#..#",
"#.....#....#.......#....#.............#....#",
"#..*..#....#....*..#....#.....#######......#",
"#..#..#....#....#..#....#..................#",
"####*....#....#..#....#....############....#",
"#.....#....#....#....#....#..............#",
"#..*..#....####....#....#....#....####....#",
"#..#..#..............#....#........#....#",
"#..####\"\"\"\"\"\"\"\"\"\"\"...#....#....#########..#",
"#....................#....#...............X#",
"###########################################"
        };

        float cellSize = 3f;
        float wallHeight = 5f;

        StringBuilder sb = new StringBuilder();
        int id = 0;

        sb.AppendLine("["); // inicio del array JSON

        for (int z = 0; z < maze.Length; z++)
        {
            var line = maze[z];
            for (int x = 0; x < line.Length; x++)
            {
                char c = line[x];

                if (c != '.') // no es pasillo → es muro
                {
                    float wx = x * cellSize;
                    float wz = z * cellSize;

                    sb.AppendLine($@"
{{
  ""id"": ""lab_wall_{id}"",
  ""sm"": ""cube"",
  ""collision"": ""col_cube"",
  ""enabled"": true,
  ""position"": [{wx}, 0.0, {wz}],
  ""orientation"": {{ ""axis"": [0,1,0], ""angle"": 0 }},
  ""scale"": [1.5, {wallHeight}, 1.5]
}},");
                    id++;
                }
            }
        }

        sb.AppendLine("]"); // cierre del array JSON

        // Guardar archivo
        File.WriteAllText("labyrinth.json", sb.ToString(), Encoding.UTF8);

        Console.WriteLine($"Archivo generado: labyrinth.json");
        Console.WriteLine($"Muros totales: {id}");
    }
}

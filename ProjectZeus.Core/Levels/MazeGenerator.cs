using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Generates random mazes using recursive backtracking algorithm.
    /// </summary>
    public class MazeGenerator
    {
        private readonly int width;
        private readonly int height;
        private readonly Random random;

        public MazeGenerator(int width, int height, Random random = null)
        {
            this.width = width;
            this.height = height;
            this.random = random ?? new Random();
        }

        /// <summary>
        /// Generates a maze using recursive backtracking.
        /// Returns a 2D array where true = wall, false = passage.
        /// </summary>
        public bool[,] Generate()
        {
            // Initialize all cells as walls
            bool[,] walls = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    walls[x, y] = true;
                }
            }

            // Use recursive backtracking to generate maze
            Stack<Point> stack = new Stack<Point>();
            Point start = new Point(1, 1);
            walls[start.X, start.Y] = false;
            stack.Push(start);

            while (stack.Count > 0)
            {
                Point current = stack.Peek();
                List<Point> unvisitedNeighbors = GetUnvisitedNeighbors(current, walls);

                if (unvisitedNeighbors.Count > 0)
                {
                    Point next = unvisitedNeighbors[random.Next(unvisitedNeighbors.Count)];

                    // Remove wall between current and next
                    int wallX = (current.X + next.X) / 2;
                    int wallY = (current.Y + next.Y) / 2;
                    walls[wallX, wallY] = false;
                    walls[next.X, next.Y] = false;

                    stack.Push(next);
                }
                else
                {
                    stack.Pop();
                }
            }

            // Ensure outer border is always walls to prevent going off-screen
            for (int x = 0; x < width; x++)
            {
                walls[x, 0] = true;
                walls[x, height - 1] = true;
            }
            for (int y = 0; y < height; y++)
            {
                walls[0, y] = true;
                walls[width - 1, y] = true;
            }

            return walls;
        }

        private List<Point> GetUnvisitedNeighbors(Point cell, bool[,] walls)
        {
            List<Point> neighbors = new List<Point>();

            // Check all four directions (2 cells away)
            Point[] directions = new[]
            {
                new Point(cell.X - 2, cell.Y),
                new Point(cell.X + 2, cell.Y),
                new Point(cell.X, cell.Y - 2),
                new Point(cell.X, cell.Y + 2)
            };

            foreach (Point dir in directions)
            {
                if (dir.X > 0 && dir.X < width - 1 &&
                    dir.Y > 0 && dir.Y < height - 1 &&
                    walls[dir.X, dir.Y])
                {
                    neighbors.Add(dir);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Simple BFS to check if there's a walkable path between two cells.
        /// </summary>
        public static bool IsReachable(Point start, Point target, bool[,] walls, int width, int height)
        {
            if (walls[target.X, target.Y])
                return false;

            bool[,] visited = new bool[width, height];
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(start);
            visited[start.X, start.Y] = true;

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                if (current == target)
                    return true;

                for (int dir = 0; dir < 4; dir++)
                {
                    int nx = current.X + dx[dir];
                    int ny = current.Y + dy[dir];

                    if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 &&
                        !walls[nx, ny] && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Point(nx, ny));
                    }
                }
            }

            return false;
        }
    }
}

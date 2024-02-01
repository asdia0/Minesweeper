using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Minesweeper.Solver
{
    public class Guesser
    {
        public Dictionary<Configuration, double> Weights = new();

        public Dictionary<int, double> CombinationWhereSafe = new();

        public double ExpectedFloatingMines = 0;

        public Grid Grid { get; set; }

        public HashSet<Constraint> Constraints { get; set; }

        public Guesser(Grid grid)
        {
            // Ensure that grid has no more logic
            this.Grid = grid;
            this.Constraints = new();

            // Set up local constraints
            foreach (Cell boundaryCell in grid.BoundaryCells)
            {
                HashSet<int> cellVariables = boundaryCell.AdjacentCells
                    .Intersect(grid.UnknownCells)
                    .Select(i => i.Point.ID)
                    .ToHashSet();
                Constraints.Add(new(cellVariables, (int)boundaryCell.MineCount - boundaryCell.AdjacentCells.Intersect(grid.FlaggedCells).Count()));

            }
        }

        public HashSet<HashSet<Constraint>> GetGroups(HashSet<Constraint> constraints)
        {
            HashSet<HashSet<Constraint>> results = new();

            HashSet<HashSet<Constraint>> preliminaryGroups = GetPreliminaryGroups(constraints);

            foreach (HashSet<Constraint> preliminaryGroup in preliminaryGroups)
            {
                results.UnionWith(GetFinalGroups(preliminaryGroup));
            }

            results.RemoveWhere(i => i.Count == 0);

            return results;
        }

        public HashSet<HashSet<Constraint>> GetPreliminaryGroups(HashSet<Constraint> constraints)
        {
            HashSet<HashSet<Constraint>> groups = new();

            HashSet<Constraint> remainingConstraints = constraints.ToHashSet();

            HashSet<Constraint> toSearch = new();
            HashSet<Constraint> group = new();
            HashSet<Constraint> searched = new();

            while (remainingConstraints.Count > 0)
            {
                Constraint seed = null;

                if (toSearch.Any())
                {
                    seed = toSearch.First();
                }
                else
                {
                    groups.Add(group);
                    group = new();
                    seed = remainingConstraints.First();
                }

                foreach (int id in seed.Variables)
                {
                    toSearch.UnionWith(remainingConstraints.Where(i => i.Variables.Contains(id)));
                }

                toSearch = toSearch.Except(searched).ToHashSet();

                group.Add(seed);

                searched.Add(seed);
                remainingConstraints.Remove(seed);
            }

            groups.Add(group);

            return groups;
        }

        public HashSet<HashSet<Constraint>> GetFinalGroups(HashSet<Constraint> constraints)
        {
            HashSet<Constraint> constraintsTemp = constraints.ToHashSet();
            HashSet<HashSet<Constraint>> groups = new();

            HashSet<HashSet<int>> intersections = Utility.GetGroups(constraints.Select(i => i.Variables).ToHashSet());

            foreach (HashSet<int> intersection in intersections)
            {
                // Find constraint whose variables are exactly the intersection.
                Constraint constraint = constraintsTemp.Where(i => i.Variables == intersection).FirstOrDefault();

                if (constraint is null)
                {
                    continue;
                }

                // Find constraints whose variables contain the intersection
                HashSet<Constraint> constraintSupersets = constraintsTemp.Where(i => i.Variables.IsProperSupersetOf(intersection)).ToHashSet();

                // Update constraintSupersets in constraintsTemp
                foreach (Constraint constraintSuperset in constraintSupersets)
                {
                    constraintsTemp.Remove(constraintSuperset);
                    constraintsTemp.Add(new(constraintSuperset.Variables.Except(intersection).ToHashSet(), constraintSuperset.Sum - constraint.Sum));
                }

                Console.WriteLine("C: " + constraint);

                groups.Add([constraint]);
            }

            groups.Add(constraintsTemp);

            return groups;
        }

        public HashSet<Configuration> GetAllConfigurations()
        {
            List<List<Configuration>> configurations = new();

            foreach (HashSet<Constraint> group in this.GetGroups(this.Constraints))
            {
                Configuration config = new(group.SelectMany(i => i.Variables).Distinct().ToList(), []);
                HashSet<Configuration> groupConfigs = this.GetGroupConfigurations(config);
                groupConfigs.RemoveWhere(i => i.Assignments.Values.Where(i => i < 0).Any());
                configurations.Add(groupConfigs.ToList());
            }

            List<Configuration> combos = new() { new Configuration([], []) };

            foreach (List<Configuration> inner in configurations)
            {
                combos = combos.SelectMany(r => inner.Select(x => r + x)).ToList();
            }

            int maxMines = this.Grid.Mines - this.Grid.FlaggedCells.Count;

            combos.RemoveAll(i => i.Sum > maxMines);

            return combos.ToHashSet();
        }

        public HashSet<Configuration> GetGroupConfigurations(Configuration seed, int depth = 0, int maxDepth = 5)
        {
            HashSet<Configuration> configs = new();

            List<int> variables = seed.Assignments.Keys.ToList();
            List<int> unsolvedVariables = seed.Assignments.Where(i => i.Value == null).Select(i => i.Key).ToList();
            List<int> solvedVariables = seed.Assignments.Where(i => i.Value != null).Select(i => i.Key).ToList();

            if (unsolvedVariables.Count == 0)
            {
                return [seed];
            }

            int ID = unsolvedVariables.First();

            // Assume safe
            Inferrer solverSafe = new(this.Grid);
            foreach (int solvedVariable in solvedVariables)
            {
                solverSafe.Constraints.Add(new Constraint([solvedVariable], (int)seed.Assignments[solvedVariable]));
            }
            solverSafe.Constraints.Add(new Constraint([ID], 0));

            solverSafe.Solve();

            Configuration newConfigurationSafe = new(variables, solverSafe.Solutions
                    .Where(i => variables.Contains(i.Variables.First()))
                    .ToHashSet());

            if (!newConfigurationSafe.Assignments.Where(i => i.Value < 0).Any())
            {
                if (newConfigurationSafe.IsSolved)
                {
                    configs.Add(newConfigurationSafe);
                }
                else if (depth <= maxDepth)
                {
                    configs.UnionWith(GetGroupConfigurations(newConfigurationSafe, depth + 1));
                }
                else
                {
                    configs.UnionWith([GetOneConfiguration(newConfigurationSafe)]);
                }
            }

            // Assume mined
            Inferrer solverMined = new(this.Grid);
            foreach (int solvedVariable in solvedVariables)
            {
                solverMined.Constraints.Add(new Constraint([solvedVariable], (int)seed.Assignments[solvedVariable]));
            }
            solverMined.Constraints.Add(new Constraint([ID], 1));

            solverMined.Solve();

            Configuration newConfigurationMined = new(variables, solverMined.Solutions
                    .Where(i => variables.Contains(i.Variables.First()))
                    .ToHashSet());

            if (!newConfigurationMined.Assignments.Where(i => i.Value < 0).Any())
            {
                if (newConfigurationMined.IsSolved)
                {
                    configs.Add(newConfigurationMined);
                }
                else if (depth <= maxDepth)
                {
                    configs.UnionWith(GetGroupConfigurations(newConfigurationMined, depth + 1));
                }
                else
                {
                    configs.UnionWith([GetOneConfiguration(newConfigurationMined)]);
                }
            }

            return configs.Where(i => i.Assignments != null).ToHashSet();
        }

        public Configuration GetOneConfiguration(Configuration seed)
        {
            List<int> variables = seed.Assignments.Keys.ToList();
            List<int> unsolvedVariables = seed.Assignments.Where(i => i.Value == null).Select(i => i.Key).ToList();
            List<int> solvedVariables = seed.Assignments.Where(i => i.Value != null).Select(i => i.Key).ToList();

            int ID = unsolvedVariables.First();

            // Assume safe
            Inferrer solverSafe = new(this.Grid);
            foreach (int solvedVariable in solvedVariables)
            {
                solverSafe.Constraints.Add(new Constraint([solvedVariable], (int)seed.Assignments[solvedVariable]));
            }
            solverSafe.Constraints.Add(new Constraint([ID], 0));

            solverSafe.Solve();

            if (!solverSafe.Contradiction)
            {
                Configuration newConfigurationSafe = new(variables, solverSafe.Solutions
                    .Where(i => variables.Contains(i.Variables.First()))
                    .ToHashSet());

                if (newConfigurationSafe.IsSolved)
                {
                    return newConfigurationSafe;
                }
                else
                {
                    return GetOneConfiguration(newConfigurationSafe);
                }
            }

            // Assume mined
            Inferrer solverMined = new(this.Grid);
            foreach (int solvedVariable in solvedVariables)
            {
                solverMined.Constraints.Add(new Constraint([solvedVariable], (int)seed.Assignments[solvedVariable]));
            }
            solverMined.Constraints.Add(new Constraint([ID], 1));

            solverMined.Solve();

            if (!solverMined.Contradiction)
            {
                Configuration newConfigurationMined = new(variables, solverMined.Solutions
                    .Where(i => variables.Contains(i.Variables.First()))
                    .ToHashSet());

                if (newConfigurationMined.IsSolved)
                {
                    return newConfigurationMined;
                }
                else
                {
                    return GetOneConfiguration(newConfigurationMined);
                }
            }

            return new();
        }

        public Dictionary<int, double> GetSafetyConfig(HashSet<Configuration> configurations)
        {
            Dictionary<int, double> safetyValues = new();

            foreach (Configuration configuration in configurations)
            {
                this.Weights.Add(configuration, Utility.nCr(this.Grid.FloatingCells.Count, this.Grid.Mines - this.Grid.FlaggedCells.Count - configuration.Sum));
            }

            foreach (int unknownCell in this.Grid.UnknownCells.Select(i => i.Point.ID))
            {
                safetyValues.Add(unknownCell, 0);
            }

            double denominator = Weights.Values.Sum();

            foreach (Configuration configuration in configurations)
            {
                foreach (int exposedCell in this.Grid.ExposedCells.Select(i => i.Point.ID).Intersect(configuration.Assignments.Where(i => i.Value == 0).Select(i => i.Key)))
                {
                    safetyValues[exposedCell] += Weights[configuration];
                }
            }

            foreach (int exposedCell in safetyValues.Keys)
            {
                CombinationWhereSafe.Add(exposedCell, safetyValues[exposedCell]);
                safetyValues[exposedCell] = safetyValues[exposedCell] / denominator;
            }

            this.ExpectedFloatingMines = this.Grid.Mines - this.Grid.FlaggedCells.Count - safetyValues.Count + safetyValues.Values.Sum();

            double floatingSafety = 1 - this.ExpectedFloatingMines / this.Grid.FloatingCells.Count;

            foreach (int floatingCell in this.Grid.FloatingCells.Select(i => i.Point.ID))
            {
                safetyValues[floatingCell] = floatingSafety;
            }

            return safetyValues;
        }

        public Dictionary<int, double> GetSafety(HashSet<Configuration> configurations)
        {
            var timer = new Stopwatch();
            timer.Start();

            Dictionary<int, double> safetyValues = new();

            double expectedExposedMines = 0;

            foreach (Configuration configuration in configurations)
            {
                this.Weights.Add(configuration, Utility.nCr(this.Grid.FloatingCells.Count, this.Grid.Mines - this.Grid.FlaggedCells.Count - configuration.Sum));
            }

            double denominator = Weights.Values.Sum();
            
            foreach (int exposedCell in this.Grid.ExposedCells.Select(i => i.Point.ID))
            {
                double numerator = configurations.Where(i => i.Assignments[exposedCell] == 0).Select(i => Weights[i]).Sum();
                CombinationWhereSafe.Add(exposedCell, numerator);

                double safetyValue = numerator / denominator;
                expectedExposedMines += 1 - safetyValue;

                safetyValues.Add(exposedCell, safetyValue);
            }

            this.ExpectedFloatingMines = this.Grid.Mines - this.Grid.FlaggedCells.Count - expectedExposedMines;

            double floatingSafety = 1 - this.ExpectedFloatingMines / this.Grid.FloatingCells.Count;

            foreach (int floatingCell in this.Grid.FloatingCells.Select(i => i.Point.ID))
            {
                safetyValues.Add(floatingCell, floatingSafety);
            }

            return safetyValues;
        }

        public double GetProgressFloating(int KFA)
        {
            double progress = 1;

            for (int i = 1; i <= KFA + 1; i++)
            {
                progress *= (1 - this.ExpectedFloatingMines / (this.Grid.FloatingCells.Count - KFA - 1 + i));
            }

            return progress;
        }

        public double GetProgressExposed(Cell cell, HashSet<Configuration> configurations)
        {
            var timer = new Stopwatch();
            timer.Start();

            int ID = cell.Point.ID;

            // Find all minecounts that yield logic
            List<int> bracketN = new();
            for (int i = cell.AdjacentCells.Intersect(this.Grid.FlaggedCells).Count(); i <= cell.AdjacentCells.Where(i => !i.IsOpen).Count(); i++)
            {
                Inferrer solver = new(this.Grid);
                solver.Constraints.Add(new(cell.AdjacentCells.Select(i => i.Point.ID).ToHashSet(), i));
                solver.Solve();

                if (solver.Solutions.Any() && !solver.Contradiction)
                {
                    bracketN.Add(i);
                }
            }

            double numerator = 0;
            double denominator = CombinationWhereSafe[cell.Point.ID];

            foreach (Configuration configuration in configurations)
            {
                if (configuration.Assignments[ID] == 0)
                {
                    if (bracketN.Contains(cell.AdjacentCells.Intersect(this.Grid.ExposedCells).Select(i => (int)configuration.Assignments[i.Point.ID]).Sum()))
                    {
                        numerator += Weights[configuration];
                    }
                }
            }

            return numerator / denominator;
        }

        public Dictionary<int, double> GetScore()
        {
            Dictionary<int, double> score = new();

            HashSet<Configuration> configurations = this.GetAllConfigurations();

            Dictionary<int, double> safety = GetSafetyConfig(configurations);

            for (int i = 0; i <= 8; i++)
            {
                double floatingProgress = GetProgressFloating(i);

                foreach (Cell cell in this.Grid.FloatingCells.Where(j => j.AdjacentCells.Intersect(this.Grid.FloatingCells).Count() == i))
                {
                    int ID = cell.Point.ID;
                    score.Add(ID, safety[ID] * floatingProgress);
                }
            }

            double maxSafety = safety.Values.OrderByDescending(i => i).First();

            foreach (Cell exposedCell in this.Grid.ExposedCells)
            {
                int ID = exposedCell.Point.ID;

                if (safety[ID] >= 0.9 * maxSafety)
                {
                    score.Add(ID, safety[ID] * GetProgressExposed(exposedCell, configurations));
                }
            }

            foreach (int cell in score.Keys)
            {
                if (safety[cell] == 1)
                {
                    score[cell] = 1;
                }
            }

            return score;
        }
    }
}

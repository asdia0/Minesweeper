﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minesweeper.Solver
{
    public class Guesser
    {
        public Grid Grid { get; set; }

        public HashSet<Constraint> Constraints { get; set; }

        public Guesser(Grid grid)
        {
            // Ensure that grid has no more logic
            this.Grid = grid;
            this.Constraints = [];

            // Set up local constraints
            foreach (Cell boundaryCell in grid.BoundaryCells)
            {
                HashSet<int> cellVariables = [.. Utility.CellsToIDs(boundaryCell.AdjacentCells.Intersect(grid.UnknownCells))];
                Constraints.Add(new(cellVariables, (int)boundaryCell.MineCount - boundaryCell.AdjacentCells.Intersect(grid.FlaggedCells).Count()));
            }
        }

        public HashSet<HashSet<Constraint>> GetGroups(HashSet<Constraint> constraints)
        {
            HashSet<HashSet<Constraint>> results = [];

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
            HashSet<HashSet<Constraint>> groups = [];

            HashSet<Constraint> remainingConstraints = [.. constraints];

            HashSet<Constraint> toSearch = [];
            HashSet<Constraint> group = [];
            HashSet<Constraint> searched = [];

            while (remainingConstraints.Count > 0)
            {
                Constraint seed = null;

                if (toSearch.Count != 0)
                {
                    seed = toSearch.First();
                }
                else
                {
                    groups.Add(group);
                    group = [];
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
            HashSet<Constraint> constraintsTemp = [.. constraints];
            HashSet<HashSet<Constraint>> groups = [];

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
            List<List<Configuration>> configurations = [];

            foreach (HashSet<Constraint> group in this.GetGroups(this.Constraints))
            {
                Configuration config = new(group.SelectMany(i => i.Variables).Distinct().ToList(), []);
                HashSet<Configuration> groupConfigs = this.GetGroupConfigurations(config);
                groupConfigs.RemoveWhere(i => i.Assignments.Values.Where(i => i < 0).Any());
                configurations.Add([.. groupConfigs]);
            }

            List<Configuration> combos = [new Configuration([], [])];

            foreach (List<Configuration> inner in configurations)
            {
                combos = combos.SelectMany(r => inner.Select(x => r * x)).ToList();
            }

            int maxMines = this.Grid.Mines - this.Grid.FlaggedCells.Count;

            combos.RemoveAll(i => i.Sum > maxMines);

            return [.. combos];
        }

        public HashSet<Configuration> GetGroupConfigurations(Configuration seed, int depth = 0, int maxDepth = 5)
        {
            HashSet<Configuration> configs = [];

            List<int> variables = [.. seed.Assignments.Keys];
            List<int> unsolvedVariables = [.. seed.Assignments.Where(i => i.Value == null).Select(i => i.Key)];
            List<int> solvedVariables = [.. seed.Assignments.Where(i => i.Value != null).Select(i => i.Key)];

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
            List<int> variables = [.. seed.Assignments.Keys];
            List<int> unsolvedVariables = [.. seed.Assignments.Where(i => i.Value == null).Select(i => i.Key)];
            List<int> solvedVariables = [.. seed.Assignments.Where(i => i.Value != null).Select(i => i.Key)];

            int ID = unsolvedVariables.First();

            // Assume safe
            Inferrer solverSafe = new(this.Grid);
            foreach (int solvedVariable in solvedVariables)
            {
                solverSafe.Constraints.Add(new Constraint([solvedVariable], (int)seed.Assignments[solvedVariable]));
            }
            solverSafe.Constraints.Add(new Constraint([ID], 0));

            solverSafe.Solve();

            if (!solverSafe.HasContradiction)
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

            if (!solverMined.HasContradiction)
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

        public Dictionary<int, double> GetSafety(HashSet<Configuration> configurations)
        {
            Dictionary<int, double> safetyValues = this.Grid.UnknownCells.ToDictionary(key => key.Point.ID, value => (double)0);

            Dictionary<Configuration, double> weights = [];

            foreach (Configuration configuration in configurations)
            {
                weights.Add(configuration, Utility.Choose(this.Grid.FloatingCells.Count, this.Grid.Mines - this.Grid.FlaggedCells.Count - configuration.Sum));
            }

            double denominator = weights.Values.Sum();

            foreach (Configuration configuration in configurations)
            {
                foreach (int exposedCell in Utility.CellsToIDs(this.Grid.ExposedCells).Intersect(configuration.Assignments.Where(i => i.Value == 0).Select(i => i.Key)))
                {
                    safetyValues[exposedCell] += weights[configuration];
                }
            }

            foreach (int exposedCell in safetyValues.Keys)
            {
                safetyValues[exposedCell] = safetyValues[exposedCell] / denominator;
            }

            double expectedFloatingMines = this.Grid.Mines - this.Grid.FlaggedCells.Count - safetyValues.Count + safetyValues.Values.Sum();

            double floatingSafety = 1 - expectedFloatingMines / this.Grid.FloatingCells.Count;

            if (floatingSafety == 0)
            {
                Console.WriteLine(expectedFloatingMines);
            }

            foreach (int floatingCell in Utility.CellsToIDs(this.Grid.FloatingCells))
            {
                safetyValues[floatingCell] = floatingSafety;
            }

            return safetyValues;
        }

        public Dictionary<int, double> GetSafety(List<HashSet<Configuration>> groupConfigurations)
        {
            //if (groupConfigurations.Count == 0)
            //{
            //    return this.Grid.UnknownCells.ToDictionary(key => key.Point.ID, value => (double)(this.Grid.Mines - this.Grid.FlaggedCells.Count) / this.Grid.UnknownCells.Count);
            //}

            Dictionary<int, double> safetyValues = this.Grid.UnknownCells.ToDictionary(key => key.Point.ID, value => (double)0);

            Dictionary<Configuration, double> weights = [];

            foreach (HashSet<Configuration> groupConfiguration in groupConfigurations)
            {
                foreach (Configuration configuration in groupConfiguration)
                {
                    weights.Add(configuration, Utility.Choose(this.Grid.FloatingCells.Count, this.Grid.Mines - this.Grid.FlaggedCells.Count - configuration.Sum));
                }
            }

            double denominator = weights.Values.Sum();

            foreach (HashSet<Configuration> groupConfiguration in groupConfigurations)
            {
                foreach (Configuration configuration in groupConfiguration)
                {
                    foreach (int exposedCell in Utility.CellsToIDs(this.Grid.ExposedCells).Intersect(configuration.Assignments.Where(i => i.Value == 0).Select(i => i.Key)))
                    {
                        safetyValues[exposedCell] += weights[configuration];
                    }
                }
            }

            foreach (int exposedCell in safetyValues.Keys)
            {
                safetyValues[exposedCell] = safetyValues[exposedCell] / denominator;
            }

            double expectedFloatingMines = this.Grid.Mines - this.Grid.FlaggedCells.Count - safetyValues.Count + safetyValues.Values.Sum();

            double floatingSafety = 1 - expectedFloatingMines / this.Grid.FloatingCells.Count;

            foreach (int floatingCell in Utility.CellsToIDs(this.Grid.FloatingCells))
            {
                safetyValues[floatingCell] = floatingSafety;
            }

            return safetyValues;
        }
        public Dictionary<int, double> GetScore()
        {
            if (Grid.ExposedCells.Count == 0)
            {
                return this.Grid.UnknownCells.ToDictionary(key => key.Point.ID, value => 1 - (double)(this.Grid.Mines - this.Grid.FlaggedCells.Count) / this.Grid.UnknownCells.Count);
            }

            List<HashSet<Configuration>> groupConfigurations = [];

            foreach (HashSet<Constraint> group in this.GetGroups(this.Constraints))
            {
                Configuration config = new(group.SelectMany(i => i.Variables).Distinct().ToList(), []);
                HashSet<Configuration> groupConfiguration = this.GetGroupConfigurations(config);
                groupConfiguration.RemoveWhere(i => i.Assignments.Values.Where(i => i < 0).Any());
                groupConfigurations.Add([.. groupConfiguration]);
            }

            return this.GetSafety(groupConfigurations);
            //return this.GetSafety(this.GetAllConfigurations());
        }
    }
}

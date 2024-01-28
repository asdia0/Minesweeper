using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<Configuration> GetAllConfigurations()
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

            return combos;
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
                else
                {
                    if (depth <= maxDepth)
                    {
                        configs.UnionWith(GetGroupConfigurations(newConfigurationSafe, depth + 1));
                    }
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
                else
                {
                    if (depth <= maxDepth)
                    {
                        configs.UnionWith(GetGroupConfigurations(newConfigurationMined, depth + 1));
                    }
                }
            }

            return configs;
        }
    }
}

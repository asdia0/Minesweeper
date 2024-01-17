using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Resolvers;
using Newtonsoft.Json;

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

        public HashSet<HashSet<Constraint>> GetConfigurations(HashSet<Constraint> constraints, HashSet<Constraint> solutions, int depth=0)
        {
            //Console.WriteLine(string.Join(", ", constraints));
            //Console.WriteLine(string.Join(", ", assumptions) + "\n");

            HashSet<HashSet<Constraint>> configurations = new();

            List<int> variables = constraints.SelectMany(i => i.Variables).Distinct().ToList();
            //List<int> solvedVariables = solutions.SelectMany(i => i.Variables).Distinct().ToList();
            List<int> solvedVariables = solutions.Where(i => i.Variables.Count == 1).SelectMany(i => i.Variables).Distinct().ToList();

            int count = 0;

            foreach (int ID in variables.Except(solvedVariables))
            {
                // Assume cell is safe
                Inferrer solverSafe = new(this.Grid);
                solverSafe.Constraints = constraints.Union(solutions).Union([new([ID], 0)]).ToHashSet();
                solverSafe.Solve();

                HashSet<Constraint> variableSolutionsSafe = solverSafe.Solutions
                    .Where(i => i.Variables.Intersect(variables).Any())
                    .Distinct()
                    .ToHashSet();

                HashSet<Constraint> newSolutions = solutions.Union(variableSolutionsSafe).ToHashSet();

                // Not solved
                if (newSolutions.Count < variables.Count)
                {
                    configurations.UnionWith(GetConfigurations(solverSafe.Constraints.ToHashSet(), newSolutions, depth+1));
                }
                // Solved
                else
                {
                    configurations.Add(newSolutions);
                }

                // Assume cell is mined
                Inferrer solverMined = new(this.Grid);
                solverMined.Constraints = constraints.Union(solutions).Union([new([ID], 1)]).ToHashSet();
                solverMined.Solve();

                HashSet<Constraint> variableSolutionsMined = solverMined.Solutions
                    .Where(i => i.Variables.Intersect(variables).Any())
                    .Distinct()
                    .ToHashSet();

                HashSet<Constraint> newSolutionsMined = solutions.Union(variableSolutionsMined).ToHashSet();

                // Not solved
                if (newSolutionsMined.Count < variables.Count)
                {
                    configurations.UnionWith(GetConfigurations(solverMined.Constraints.ToHashSet(), newSolutionsMined, depth + 1));
                }
                // Solved
                else
                {
                    configurations.Add(newSolutionsMined);
                }

                count++;
            }

            foreach (HashSet<Constraint> config in configurations)
            {
                Console.WriteLine($"{depth}: {string.Join(", ", config)}");
            }

            return configurations;
        }

        public List<Configuration> GetConfigurations1(List<Constraint> constraints)
        {
            List<Configuration> configurations = new();

            Console.WriteLine(string.Join(", ", constraints) + "\n");

            List<int> variables = constraints.SelectMany(i => i.Variables).Distinct().ToList();

            if (variables.Count == 0)
            {
                return configurations;
            }

            int cell = variables.First();
            //foreach (int cell in variables)
            //{
                // Assume cell is safe
                Constraint safeCell = new([cell], 0);
                Inferrer solver = new([.. constraints, safeCell]);
                solver.Solve();

                List<Constraint> solutionSafe = solver.Solutions.ToList();

                Configuration solverConfig = new(constraints, solutionSafe);

                if (solverConfig.IsSolved)
                {
                    configurations.Add(solverConfig);
                }
                else
                {
                    //List<Configuration> recursionSafe = GetConfigurations(solver.Constraints);

                    //foreach (Configuration recursionConstraintSafe in recursionSafe)
                    //{
                    //    Configuration temp = new(solverConfig, recursionConstraintSafe);
                    //    configurations.Add(temp);
                    //}
                }

            // Assume cell is mined
                Constraint minedCell = new([cell], 0);
                Inferrer solverMined = new([.. constraints, minedCell]);
                solverMined.Solve();

                List<Constraint> solutionMined = [.. solverMined.Solutions];

                Configuration solverConfigMined = new(constraints, solutionMined);

                if (solverConfigMined.IsSolved)
                {
                    configurations.Add(solverConfigMined);
                }
                else
                {
                    //List<Configuration> recursionMined = GetConfigurations(solverMined.Constraints);

                    //foreach (Configuration recursionConstraintMined in recursionMined)
                    //{
                    //    Configuration temp = new(solverConfigMined, recursionConstraintMined);
                    //    configurations.Add(temp);
                    //}
                }
            //}

            foreach (Configuration config in configurations)
            {
                Console.WriteLine(JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            return configurations;
        }
    }
}

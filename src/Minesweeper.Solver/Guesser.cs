using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;

namespace Minesweeper.Solver
{
    public class Guesser
    {
        public Grid Grid { get; set; }

        public HashSet<Constraint> Constraints { get; set; }

        public Guesser(Grid grid, HashSet<Constraint> constraints)
        {
            // Ensure that grid has no more logic
            this.Grid = grid;
            this.Constraints = new();

            foreach (Constraint constraint in constraints)
            {
                if (!constraint.Variables.Except(grid.ExposedCells.Select(i => i.Point.ID)).Any())
                {
                    Constraints.Add(constraint);
                }
            }
        }

        public HashSet<HashSet<Constraint>> GetGroups(HashSet<Constraint> constraints)
        {
            HashSet<HashSet<Constraint>> groups = new();

            HashSet<Constraint> remainingConstraints = constraints.ToHashSet();

            HashSet<Constraint> toSearch = new();
            HashSet<Constraint> group = new();
            HashSet<Constraint> searched = new();

            while(remainingConstraints.Count > 0)
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

            //foreach (HashSet<Constraint> group1 in groups)
            //{
            //    List<HashSet<int>> intersections = Utility.GetGroups(group1.Select(i => i.Variables).ToHashSet()).ToList();

            //    foreach (HashSet<int> intersection in intersections)
            //    {
            //        groups.Add(group1.Where(i => intersection.IsSupersetOf(i.Variables)).ToHashSet());

            //        group1.RemoveWhere(i => intersection.IsProperSubsetOf(i.Variables));
            //    }
            //}

            //foreach (List<Constraint> conGroup in groups)
            //{
            //    Console.WriteLine(string.Join(", ", conGroup) + "\n");
            //}
            //Console.WriteLine();

            return groups;
        }

        public HashSet<HashSet<Constraint>> GetConfigurations(HashSet<Constraint> constraints, HashSet<Constraint> assumptions, int depth=0)
        {
            //Console.WriteLine(string.Join(", ", constraints));
            //Console.WriteLine(string.Join(", ", assumptions) + "\n");

            HashSet<HashSet<Constraint>> configurations = new();

            List<int> variables = constraints.SelectMany(i => i.Variables).Distinct().ToList();
            List<int> solvedVariables = assumptions.SelectMany(i => i.Variables).Distinct().ToList();

            int count = 0;

            foreach (int ID in variables.Except(solvedVariables))
            {
                foreach (HashSet<Constraint> config in configurations)
                {
                    Console.WriteLine($"{depth}: {string.Join(", ", config)}");
                }
                Console.WriteLine();

                // Assume cell is safe
                Inferrer solverSafe = new(this.Grid);
                solverSafe.Constraints = constraints.Union(assumptions).Union([new([ID], 0)]).ToHashSet();
                solverSafe.Solve();

                HashSet<Constraint> variableSolutionsSafe = solverSafe.Solutions
                    .Where(i => i.Variables.Intersect(variables).Any())
                    .Distinct()
                    .ToHashSet();

                // Not solved
                if (variableSolutionsSafe.Count < variables.Count)
                {
                    configurations.UnionWith(GetConfigurations(solverSafe.Constraints.ToHashSet(), variableSolutionsSafe, depth+1));
                }
                // Solved
                else
                {
                    configurations.Add(variableSolutionsSafe);
                }

                // Assume cell is mined
                Inferrer solverMined = new(this.Grid);
                solverMined.Constraints = constraints.Union(assumptions).Union([new([ID], 1)]).ToHashSet();
                solverMined.Solve();

                HashSet<Constraint> variableSolutionsMined = solverMined.Solutions
                    .Where(i => i.Variables.Intersect(variables).Any())
                    .Distinct()
                    .ToHashSet();

                // Not solved
                if (variableSolutionsMined.Count < variables.Count)
                {
                    configurations.UnionWith(GetConfigurations(solverMined.Constraints.ToHashSet(), variableSolutionsMined, depth+1));
                }
                // Solved
                else
                {
                    configurations.Add(variableSolutionsMined);
                }

                count++;
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

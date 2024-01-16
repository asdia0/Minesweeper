using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;

namespace Minesweeper.Solver
{
    /// <summary>
    /// Represents a constraint of binary variables.
    /// </summary>
    /// <param name="variables"></param>
    /// <param name="sum"></param>
    public class Constraint(HashSet<int> variables, int sum)
    {
        /// <summary>
        /// A list of binary variables.
        /// </summary>
        public HashSet<int> Variables { get; set; } = variables;

        /// <summary>
        /// The value that the binary variables should sum to.
        /// </summary>
        public int Sum { get; set; } = sum;

        /// <summary>
        /// Checks if there is only one variable left.
        /// </summary>
        public bool IsSolved
        {
            get
            {
                return Variables.Count == 1;
            }
        }

        /// <summary>
        /// Subtracts a given constraint from the current constraint.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public bool Subtract(Constraint input, out Constraint output)
        {
            if (!this.Variables.IsSupersetOf(input.Variables))
            {
                output = new([], 0);
                return false;
            }

            output = new(this.Variables.Except(input.Variables).ToHashSet(), this.Sum - input.Sum);
            return true;
        }

        public static bool operator ==(Constraint LHS, Constraint RHS)
        {
            if (LHS.Variables.SetEquals(RHS.Variables) && LHS.Sum == RHS.Sum)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(Constraint LHS, Constraint RHS)
        {
            return !(LHS == RHS);
        }
    }
}

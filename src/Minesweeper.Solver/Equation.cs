using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Solver
{
    public class Equation
    {
        public HashSet<int> Variables { get; set; }

        public int Sum { get; set; }

        public bool Solved
        {
            get
            {
                return Variables.Count == 1;
            }
        }

        public Equation(HashSet<int> variables, int sum)
        {
            Variables = variables;
            Sum = sum;
        }

        public bool Contains(Equation equation)
        {
            // Subset cannot have a greater sum.
            if (equation.Sum > this.Sum)
            {
                return false;
            }

            // Do not allow equality
            if (this == equation)
            {
                return false;
            }

            return this.Variables.IsSupersetOf(equation.Variables);
        }

        // Subtracts another equation from this equation
        public bool Subtract(Equation input, out Equation output)
        {
            // Cannot subtract if the current equation does not contain input
            if (!this.Contains(input))
            {
                output = new(new(), 0);
                return false;
            }

            output = new(this.Variables.Except(input.Variables).ToHashSet(), this.Sum - input.Sum);
            return true;
        }

        public static bool operator ==(Equation LHS, Equation RHS)
        {
            if (LHS.Variables.SetEquals(RHS.Variables) && LHS.Sum == RHS.Sum)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(Equation LHS, Equation RHS)
        {
            return !(LHS == RHS);
        }
    }
}

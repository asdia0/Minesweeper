using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Solver
{
    public class Interpretation
    {
        public Dictionary<Cell, bool> Assignments { get; set; }

        public List<Cell> CellsAssigned
        {
            get
            {
                return Assignments.Keys.ToList();
            }
        }

        public bool Empty
        {
            get
            {
                return !this.Assignments.Any();
            }
        }

        public Interpretation(Dictionary<Cell, bool> assignments)
        {
            this.Assignments = assignments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Interpretation()
        {
            this.Assignments = new();
        }

        public bool Contains(Cell cell)
        {
            return this.Assignments.ContainsKey(cell);
        }

        public void Assign(Cell cell, bool hasMine)
        {
            if (this.Assignments.ContainsKey(cell))
            {
                this.Assignments[cell] = hasMine;
            }
            else
            {
                this.Assignments.Add(cell, hasMine);
            }
        }

        public void Remove(Cell cell)
        {
            this.Assignments.Remove(cell);
        }
    }
}

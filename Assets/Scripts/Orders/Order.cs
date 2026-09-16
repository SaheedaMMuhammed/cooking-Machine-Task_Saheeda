using System.Collections.Generic;
using ChefMachine.Ingredients;
using UnityEngine;

namespace ChefMachine.Orders
{
    /// <summary>One thing an order asks for, and whether it has been handed in.</summary>
    public class OrderRequirement
    {
        public IngredientType Type;
        public IngredientState RequiredState;
        public bool Fulfilled;

        public OrderRequirement(IngredientType type, IngredientState requiredState)
        {
            Type = type;
            RequiredState = requiredState;
            Fulfilled = false;
        }

        public override string ToString()
        {
            return Type + "/" + RequiredState;
        }
    }

    /// <summary>
    /// A customer order: the ingredients it wants, how long it has been waiting,
    /// and what it is worth once served. Plain C# class - no MonoBehaviour needed.
    /// </summary>
    public class Order
    {
        private readonly List<OrderRequirement> requirements = new List<OrderRequirement>();
        private float activatedAt;
        private bool active;

        public IReadOnlyList<OrderRequirement> Requirements { get { return requirements; } }

        public Order(IEnumerable<OrderRequirement> required)
        {
            if (required != null) requirements.AddRange(required);
        }

        /// <summary>Starts the waiting clock. Called when the order reaches a window.</summary>
        public void Activate()
        {
            activatedAt = Time.time;
            active = true;
        }

        /// <summary>Seconds since this order became active.</summary>
        public float ElapsedSeconds
        {
            get { return active ? Mathf.Max(0f, Time.time - activatedAt) : 0f; }
        }

        /// <summary>Whole seconds waited - the penalty applied at completion.</summary>
        public int ElapsedSecondsFloored
        {
            get { return Mathf.FloorToInt(ElapsedSeconds); }
        }

        public bool IsComplete
        {
            get
            {
                for (int i = 0; i < requirements.Count; i++)
                {
                    if (!requirements[i].Fulfilled) return false;
                }
                return requirements.Count > 0;
            }
        }

        /// <summary>What the order is worth before the waiting penalty.</summary>
        public int BasePoints
        {
            get
            {
                int total = 0;
                for (int i = 0; i < requirements.Count; i++)
                {
                    total += IngredientPoints.For(requirements[i].Type);
                }
                return total;
            }
        }

        /// <summary>Base points minus whole seconds waited. May be negative.</summary>
        public int CalculateScore()
        {
            return BasePoints - ElapsedSecondsFloored;
        }

        /// <summary>True when this order still needs that ingredient.</summary>
        public bool Requires(IngredientType type, IngredientState state)
        {
            return FindOpenRequirement(type, state) != null;
        }

        /// <summary>
        /// Marks ONE matching open requirement as fulfilled, so duplicates in an
        /// order each need their own delivery. False when nothing matched.
        /// </summary>
        public bool TryFulfill(IngredientType type, IngredientState state)
        {
            OrderRequirement match = FindOpenRequirement(type, state);
            if (match == null) return false;

            match.Fulfilled = true;
            return true;
        }

        /// <summary>e.g. "Meat/Cooked, Meat/Cooked, Cheese/Ready".</summary>
        public string Describe()
        {
            string text = "";
            for (int i = 0; i < requirements.Count; i++)
            {
                if (i > 0) text += ", ";
                text += requirements[i].ToString();
                if (requirements[i].Fulfilled) text += " (done)";
            }
            return text;
        }

        private OrderRequirement FindOpenRequirement(IngredientType type, IngredientState state)
        {
            for (int i = 0; i < requirements.Count; i++)
            {
                OrderRequirement r = requirements[i];
                if (!r.Fulfilled && r.Type == type && r.RequiredState == state) return r;
            }
            return null;
        }
    }
}

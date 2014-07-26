using System;
using System.Collections.Generic;

namespace UglyTrivia
{
    public class Questions : Dictionary<Category, LinkedList<string>>
    {
        public Questions()
        {
            foreach (var value in Enum.GetValues(typeof(Category)))
            {
                var categoryList = new LinkedList<string>();
                for (int i = 0; i < 50; i++)
                {
                    categoryList.AddLast(value + " Question " + i);
                }
                Add((Category)value, categoryList);
            }
        }

        public void RemoveQuestion(Category category)
        {
            this[category].RemoveFirst();
        }

        public string GetQuestion(Category category)
        {
            return this[category].First.Value;
        }
    }
}
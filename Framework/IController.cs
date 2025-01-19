using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyBudget.Model;

namespace MyBudget.Framework
{
    public interface IController
    {
        IModel Model { get; }
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goap
{
    public interface IGoapActionHolder
    {
        List<GoapAction> GetActions(Node2D agent);
    }
}

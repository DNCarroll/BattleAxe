using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleAxe
{
    public interface IBattleAxe
    {
        object this[string property] { get; set; }        
    }
}

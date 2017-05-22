using System;
using BattleAxe;
/// <summary>
/// Based on $CommandText$
/// </summary>
namespace $NameSpace$ {
        public class $ClassName$ : IBattleAxe {
            public object this[string property] {
    get {
        switch (property) {
                        $GetCaseStatements$
        default:
                            return null;
    }
}
set {
                    switch (property){
                        $SetCaseStatements$
                        default:
                            break;
                    }
                }
            }

            $Properties$
        }
    }
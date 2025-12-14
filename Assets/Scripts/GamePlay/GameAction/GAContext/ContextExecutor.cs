using System.Collections.Generic;
using System.Linq;
using cfg;

// public class ContextExecutor{
//     public readonly object sender;
//     public readonly List<object> param;
//     public ContextExecutor(object sender, List<object> param){
//         this.sender = sender;
//         this.param = param ?? new List<object>();
//     }
//     public BoardEntity GetBoardEntity(){
//         // 优先从参数中获取
//         BoardEntity res = null;
//         res = param.FirstOrDefault(x => x is BoardEntity) as BoardEntity;
//         if (res == null){
//             res = sender as BoardEntity;
//         }
//         return res;
//     }

//     public DirectionContext GetDirectionContext(){
//         // 优先从参数中获取
//         DirectionContext res = null;
//         res = param.FirstOrDefault(x => x is DirectionContext) as DirectionContext;
//         return res;
//     }
// }
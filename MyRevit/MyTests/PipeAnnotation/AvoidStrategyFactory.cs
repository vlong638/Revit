using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.PipeAnnotation
{
    public class AvoidStrategyFactory
    {
        public static AvoidStrategyEntity GetAvoidStrategyEntity(AvoidStrategy avoidStrategy)
        {
            switch (avoidStrategy)
            {
                case AvoidStrategy.MoveLeft:
                    return new AvoidStrategyEntity_MoveLeft(AvoidStrategy.MoveLeft, AvoidStrategy.MoveRight);
                case AvoidStrategy.MoveRight:
                    return new AvoidStrategyEntity_MoveRight(AvoidStrategy.MoveRight, AvoidStrategy.None);
                default:
                    return new AvoidStrategyEntity_None(AvoidStrategy.None, AvoidStrategy.None);
            }
        }
    }
}

﻿#nullable disable
using QuestViva.Engine.Expressions;

namespace QuestViva.Engine.Scripts
{
    public class ScriptContext
    {
        private FleeExpressionContext _fleeExpressionContext;

        public ScriptContext(WorldModel worldModel, bool initialiseExpressionContext = false)
        {
            WorldModel = worldModel;

            if (initialiseExpressionContext)
            {
                _fleeExpressionContext = new FleeExpressionContext(worldModel);
            }
        }

        public FleeExpressionContext FleeExpressionContext =>
            _fleeExpressionContext ??= new FleeExpressionContext(WorldModel);

        public WorldModel WorldModel { get; }
    }
}

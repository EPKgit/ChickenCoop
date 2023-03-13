using System;

namespace EnemyBehaviourSyntax
{
    public class EnemyBehaviourAction
    {
        public Func<bool> condition = null;
        public Action thenAction = null;
        public Action elseAction = null;
        public Action finallyAction = null;

        public bool endOnThen = false;
        public bool endOnElse = false;

        public EnemyBehaviourAction() { }

        #region BUILDER
        public EnemyBehaviourAction If(Func<bool> d)
        {
            condition = d;
            return this;
        }

        public ThenBlock Then(Action a)
        {
            thenAction = a;
            return new ThenBlock(this);
        }

        public EnemyBehaviourAction ThenEnd()
        {
            endOnThen = true;
            return this;
        }

        public EnemyBehaviourAction Do(Action a)
        {
            thenAction = a;
            return this;
        }

        public EnemyBehaviourAction ElseEnd()
        {
            endOnElse = true;
            return this;
        }

        public EnemyBehaviourAction Finally(Action a)
        {
            finallyAction = a;
            return this;
        }
        #endregion

        public bool Evaluate()
        {
            if (condition == null)
            {
                thenAction();
                return true;
            }
            if (condition())
            {
                thenAction?.Invoke();
                if (endOnThen)
                {
                    return false;
                }
            }
            else
            {
                elseAction?.Invoke();
                if (endOnElse)
                {
                    return false;
                }
            }
            finallyAction?.Invoke();
            return true;
        }

        public static implicit operator EnemyBehaviourAction(ElseBlock b)
        {
            return b.action;
        }

        public static implicit operator EnemyBehaviourAction(ThenBlock b)
        {
            return b.action;
        }
    }


    public class ThenBlock
    {
        public EnemyBehaviourAction action;
        public ThenBlock(EnemyBehaviourAction eba)
        {
            action = eba;
        }

        public EnemyBehaviourAction AndEnd()
        {
            action.endOnThen = true;
            return action;
        }

        public ElseBlock Else(Action a)
        {
            action.elseAction = a;
            return new ElseBlock(action);
        }
    }

    public class ElseBlock
    {
        public EnemyBehaviourAction action;
        public ElseBlock(EnemyBehaviourAction eba)
        {
            action = eba;
        }

        public EnemyBehaviourAction AndEnd()
        {
            action.endOnElse = true;
            return action;
        }
    }
}
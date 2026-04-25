using Extension.Components;
using Extension.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    [Serializable]
    public abstract class ScriptComponent : Component
    {
        private IExtension owner;

        protected ScriptComponent()
        {

        }

        protected ScriptComponent(Script script)
        {
            Script = script;
        }

        protected ScriptComponent(IExtension owner)
        {
            this.owner = owner;
        }

        public Script Script { get; internal set; }
    }
}

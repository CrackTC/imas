namespace Imas
{
    abstract class Actor
    {
        protected string cutId;
        protected int seqId;
        public int from_cut;
        protected int to;
        protected bool isEnter;
        protected bool isExit;

        public virtual void Init() { }

        public virtual void Exec() { }

        public virtual void OnUpdate(float time) { }
    }
}

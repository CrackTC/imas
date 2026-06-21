namespace Imas
{
    abstract class Actor
    {
        protected string cutId;
        protected int seqId;
        public int from_cut;
        public int to;
        protected bool isEnter;
        protected bool isExit;

        public virtual void Init() { }

        public virtual void Exec() { }

        public virtual void OnUpdate(int frame) { }
    }
}

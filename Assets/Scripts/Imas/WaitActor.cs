namespace Imas
{
    class WaitActor : Actor
    {
        public WaitActor(Sequence seq)
        {
            cutId = seq.arg1;
            from_cut = int.Parse(seq.arg2);
            to = int.Parse(seq.arg3);
            seqId = seq.seq_id;
        }
    }
}

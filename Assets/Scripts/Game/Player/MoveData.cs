using FishNet.Object.Prediction;

namespace Game.Player
{
    public struct MoveData : IReplicateData
    {
        public bool Hit;
        public float Horizontal;
        public float Vertical;
        public MoveData(bool hit, float horizontal, float vertical)
        {
            Hit = hit;
            Horizontal = horizontal;
            Vertical = vertical;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
}
namespace FoolsMate.Core
{
    public class PieceIdSource
    {
        private int next = 1;

        public int Next()
        {
            return next++;
        }

    }
}
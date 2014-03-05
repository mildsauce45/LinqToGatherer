using System.Collections.ObjectModel;
using System.Linq;

namespace LinqToGatherer
{
    public class CardImage
    {
        public int MultiverseId { get; internal set; }

        /// <summary>
        /// We store the physical bytes in a readonly collection so they can't be modified within our own cache.
        /// </summary>
        public ReadOnlyCollection<byte> Image { get; internal set; }

        public byte[] ImageBytes
        {
            get { return Image.ToArray(); }
        }
    }
}

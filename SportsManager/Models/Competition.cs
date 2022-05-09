using SportsManager.Others;
using SportsManager.ViewModels;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a competition entity.
    /// </summary>
    public class Competition : NotifyPropertyChanged, IHasImage, IEntity
    {
        /// <summary>
        /// Identification number of the entity.
        /// </summary>
        public int ID { get; set; } = SportsData.NOID;

        private string name = "";
        /// <summary>
        /// Name of the entity.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string info;
        /// <summary>
        /// Information abot the entity.
        /// </summary>
        public string Info
        {
            get => info;
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        private string imagePath;
        /// <summary>
        /// Local path of the image of the logo of the entity.
        /// </summary>
        public string ImagePath
        {
            get => imagePath;
            set
            {
                imagePath = value;
                OnPropertyChanged();
            }
        }
    }
}

using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Template class for entity editing view models.
    /// </summary>
    /// <typeparam name="T">Entity type (Competition, Season, Team, Player).</typeparam>
    public class TemplateEditViewModel<T> : NotifyPropertyChanged where T : IHasImage, IEntity
    {
        private T entity;
        /// <summary>
        /// Currently selected entity.
        /// </summary>
        public T Entity
        {
            get => entity;
            set
            {
                entity = value;
                OnPropertyChanged();
            }
        }

        private string imageFolderPath;

        private BitmapImage bitmap;
        /// <summary>
        /// Bitmap of currently selected logo or photo image of the entity.
        /// </summary>
        public BitmapImage Bitmap
        {
            get => bitmap;
            set
            {
                bitmap = value;
                OnPropertyChanged();
            }
        }

        private ICommand loadImageCommand;
        /// <summary>
        /// Command that loads an image to bitmap after executing it.
        /// </summary>
        public ICommand LoadImageCommand
        {
            get
            {
                if (loadImageCommand == null)
                {
                    loadImageCommand = new RelayCommand(param => LoadImageToBitmap());
                }
                return loadImageCommand;
            }
        }

        private ICommand removeImageCommand;
        /// <summary>
        /// Command that clears the bitmap after executing it.
        /// </summary>
        public ICommand RemoveImageCommand
        {
            get
            {
                if (removeImageCommand == null)
                {
                    removeImageCommand = new RelayCommand(param => RemoveImage());
                }
                return removeImageCommand;
            }
        }

        private ICommand saveCommand;
        /// <summary>
        /// Command that saves the entity after executing it.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(param => Save());
                }
                return saveCommand;
            }
        }

        /// <summary>
        /// Command that navigates to the previous viewmodel after executing it after saving the entity into the database.
        /// </summary>
        public ICommand NavigateBackCommand { get; protected set; }

        /// <summary>
        /// Opens up a file dialog window for selecting image that will be loaded to the Bitmap property.
        /// </summary>
        protected void LoadImageToBitmap()
        {
            Entity.ImagePath = ImageHandler.SelectImagePath();

            if (Entity.ImagePath != null)
            {
                Bitmap = ImageHandler.ImageToBitmap(Entity.ImagePath);
                GC.Collect();
            }
        }

        /// <summary>
        /// Sets absolute path to the image folder according to entity type.
        /// </summary>
        protected void GetImageFolderPath()
        {
            imageFolderPath = Entity switch
            {
                Player => SportsData.PlayerPhotosPath,
                Team => SportsData.TeamLogosPath,
                Season => SportsData.SeasonLogosPath,
                Competition => SportsData.CompetitionLogosPath,
                _ => "",
            };
        }

        /// <summary>
        /// Finds path to the currently selected entity.
        /// </summary>
        /// <returns>Absolute path to the image. Empty string if it does not exists.</returns>
        protected string GetCurrentImagePath()
        {
            string[] imgPath = Directory.GetFiles(imageFolderPath, SportsData.SPORT.Name + Entity.ID + ".*");
            string filePath = "";

            if (imgPath.Length != 0)
            {
                filePath = imgPath.First();
            }

            return filePath;
        }

        /// <summary>
        /// Saves the currently loaded image to the local app data folder.
        /// </summary>
        protected void SaveImage()
        {
            if (!string.IsNullOrWhiteSpace(Entity.ImagePath))
            {
                if (imageFolderPath == "") { return; }

                string filePath = imageFolderPath + "/" + SportsData.SPORT.Name + Entity.ID + Path.GetExtension(Entity.ImagePath);
                File.Copy(Entity.ImagePath, filePath);
                Entity.ImagePath = filePath;
            }
        }

        /// <summary>
        /// Updates image file for the current entity.
        /// </summary>
        protected void UpdateImage()
        {
            //if logo is not selected return
            if (string.IsNullOrWhiteSpace(Entity.ImagePath))
            {
                //if there is logo in the database then delete it
                //get previous logo
                string previousFilePath = GetCurrentImagePath();

                //delete logo
                if (!string.IsNullOrWhiteSpace(previousFilePath))
                {
                    GC.Collect();
                    File.Delete(previousFilePath);
                }
            }
            else
            {
                //get current logo
                string filePath = GetCurrentImagePath();

                //if logo did not exist declare its path
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    filePath = imageFolderPath + @"\" + SportsData.SPORT.Name + Entity.ID + Path.GetExtension(Entity.ImagePath);
                }
                //if logo had changed
                if (Entity.ImagePath != filePath)
                {
                    GC.Collect();
                    File.Delete(filePath);
                    filePath = Path.ChangeExtension(filePath, Path.GetExtension(Entity.ImagePath));
                    File.Copy(Entity.ImagePath, filePath);
                    Entity.ImagePath = filePath;
                }
            }
        }

        /// <summary>
        /// Clears the bitmap and resets the path to the image.
        /// </summary>
        protected void RemoveImage()
        {
            Entity.ImagePath = "";
            Bitmap = new BitmapImage();
            GC.Collect();
        }

        /// <summary>
        /// Saves the entity to the database.
        /// </summary>
        protected virtual void Save() { }
    }
}
﻿using HeroExplorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeroExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Character> MarvelCharacters;
        private ObservableCollection<ComicBook> MarvelComics;

        public MainPage()
        {
            this.InitializeComponent();
            MarvelCharacters = new ObservableCollection<Character>();
            MarvelComics = new ObservableCollection<ComicBook>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///VoiceCommandDictionary.xml"));
            await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager
                .InstallCommandDefinitionsFromStorageFileAsync(storageFile);

            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            MarvelCharacters.Clear();
            while (MarvelCharacters.Count < 10)
            {
                Task t = MarvelFacade.InitializeMarvelCharactersAsync(MarvelCharacters);
                await t;
            }

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
        }


        private async void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            ComicDetailNameTextBlock.Text = "";
            ComicDetailDescriptionTextBlock.Text = "";
            ComicDetailImage.Source = null;

            var selectedCharacter = (Character)e.ClickedItem;

            DetailNameTextBlock.Text = selectedCharacter.name;
            DetailDescriptionTextBlock.Text = selectedCharacter.description;

            var largeImage = new BitmapImage();
            Uri uri = new Uri(selectedCharacter.thumbnail.large, UriKind.Absolute);
            largeImage.UriSource = uri;
            DetailImage.Source = largeImage;

            MarvelComics.Clear();
            /*
            while (MarvelComics.Count < 10)
            {
                Task t = MarvelFacade.InitializeMarvelComicsAsync(
                    selectedCharacter.id,
                    MarvelComics);
                await t;

            }
            */
            Task t = MarvelFacade.InitializeMarvelComicsAsync(
                selectedCharacter.id,
                MarvelComics);
            await t;
            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;


        }

        private void ComicsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedComic = (ComicBook)e.ClickedItem;

            ComicDetailNameTextBlock.Text = selectedComic.title;

            if (selectedComic.description != null)
                ComicDetailDescriptionTextBlock.Text = selectedComic.description;

            var largeImage = new BitmapImage();
            Uri uri = new Uri(selectedComic.thumbnail.large, UriKind.Absolute);
            largeImage.UriSource = uri;
            ComicDetailImage.Source = largeImage;

        }
    }
}

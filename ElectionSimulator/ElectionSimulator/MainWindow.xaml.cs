﻿using ElectionLibrary.Character;
using ElectionLibrary.Environment;
using ElectionLibrary.Event;
using ElectionLibrary.Parties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ElectionSimulator
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dt = new DispatcherTimer();
        Stopwatch sw = new Stopwatch();
        TextureLoader tl = new TextureLoader(App.ElectionVM);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.ElectionVM;
            dt.Tick += Draw_tick;
            dt.Interval = new TimeSpan(0, 0, 0, 0, App.ElectionVM.RefreshRate);
        }

        private void InitBoard()
        {
            Board.ColumnDefinitions.Clear();
            Board.RowDefinitions.Clear();
            Board.Children.Clear();
        }

        private void StartNewSimulation(object sender, RoutedEventArgs e)
        {
            NewSimulationWindow newSimulationWindow = new NewSimulationWindow();
            newSimulationWindow.ShowDialog();
            if (newSimulationWindow.validated)
            {
                App.ElectionVM.Parties = newSimulationWindow.Parties;
                ElectionInitializer electionInitializer = new ElectionInitializer(newSimulationWindow.MapFile, newSimulationWindow.Parties);
                App.ElectionVM.GenerateAccessAndHQs();
                tl.LoadFirstTextures(Board);
                App.ElectionVM.DimensionX = Board.ColumnDefinitions.Count;
                App.ElectionVM.DimensionY = Board.RowDefinitions.Count;
                App.ElectionVM.GenerateCharacters();
                RefreshBoard();
            }
        }

        private void PlaySimulation(object sender, RoutedEventArgs e)
        {
            sw.Start();
            dt.Start();
            Thread t = new Thread(App.ElectionVM.Play);
            t.Start();
        }

        private void Draw_tick(object sender, EventArgs e)
        {
            if (sw.IsRunning)
                RefreshBoard();
        }

        private void PauseSimulation(object sender, RoutedEventArgs e)
        {
            App.ElectionVM.Stop();
            if (sw.IsRunning)
                sw.Stop();
        }

        private void NextTurn(object sender, RoutedEventArgs e)
        {
            App.ElectionVM.NextTurn();
            RefreshBoard();
        }

        private void RefreshBoard()
        {
            InitBoard();

            tl.LoadAllTextures(Board);

            foreach (ElectionCharacter character in App.ElectionVM.Characters)
            {
                // Add test for type of character
                Image characterImage = new Image();
                BitmapImage characterSource = tl.getActivistTexture(character);
                characterImage.Source = characterSource;
                Board.Children.Add(characterImage);
                Grid.SetColumn(characterImage, character.position.X);
                Grid.SetRow(characterImage, character.position.Y);
            }

            if(App.ElectionVM.Event != null)
            {
                if(App.ElectionVM.Event is Poll)
                {
                    ShowEvent();
                }
                App.ElectionVM.Stop();
                if (sw.IsRunning)
                    sw.Stop();
                App.ElectionVM.Event = null;
            }
        }

        private void ShowEvent()
        {
            Event.ColumnDefinitions.Clear();
            Event.RowDefinitions.Clear();
            Event.Children.Clear();

            Poll poll = (Poll)App.ElectionVM.Event;

            for (int i = 0; i < poll.Result.opinionList.Count; i++)
            {
                Event.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < 2; i++)
            {
                Event.ColumnDefinitions.Add(new ColumnDefinition());
            }

            int j = 0;
            foreach (PoliticalParty party in poll.Result.opinionList.Keys)
            {
                Label partyName = new Label();
                partyName.Content = party.Name;
                Label percent = new Label();
                percent.Content = string.Format("{0:0.00}", poll.Result.opinionList[party]) + " %";
                Event.Children.Add(partyName);
                Event.Children.Add(percent);
                Grid.SetRow(partyName, j);
                Grid.SetRow(percent, j);
                Grid.SetColumn(partyName, 0);
                Grid.SetColumn(percent, 1);
                j++;
            }
        }
    }
}

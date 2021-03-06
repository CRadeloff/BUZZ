﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BUZZ.Core.CharacterManagement;
using BUZZ.Data;
using EVEStandard.Models;
using EVEStandard.Models.API;

namespace BUZZ.Core.LPManager
{
    /// <summary>
    /// Interaction logic for LpViewer.xaml
    /// </summary>
    public partial class LpViewer : Window
    {
        private DataTable Table = new DataTable();

        public List<List<LoyaltyPoints>> CharacterLoyalty { get; set; } = new List<List<LoyaltyPoints>>();

        /// <summary>
        /// Class simply displays all current 
        /// </summary>
        public LpViewer()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadLoyaltyPoints();
        }

        private async void LoadLoyaltyPoints()
        {
            var characterLpList = new List<List<LoyaltyPoints>>();
            foreach (var buzzCharacter in CharacterManager.CurrentInstance.CharacterList)
            {
                var result = await buzzCharacter.GetLoyaltyPointsAsync();
                characterLpList.Add(result.Model);
            }
            // Get corporation names from ID's
            var corpIds = new List<int>();
            foreach (var characterLp in characterLpList)
            {
                foreach (var loyaltyPoints in characterLp)
                {
                    if (!corpIds.Contains(loyaltyPoints.CorporationId))
                    {
                        corpIds.Add(loyaltyPoints.CorporationId);
                    }
                }
            }
            var idResults = await EsiData.EsiClient.Universe.GetNamesAndCategoriesFromIdsV2Async(corpIds);
            var corpNames = idResults.Model;


            var lpData = new string[characterLpList.Count, corpIds.Count+1];

            // Populate our LoyaltyPoint data
            for (var i = 0; i < CharacterManager.CurrentInstance.CharacterList.Count; i++)
            {
                var currentLpCharacter = characterLpList[i];
                lpData[i, 0] = CharacterManager.CurrentInstance.CharacterList[i].CharacterName;
                for (var j = 0; j < corpNames.Count; j++)
                {
                    lpData[i, j + 1] = 0.ToString();
                    foreach (var loyaltyPoint in currentLpCharacter)
                    {
                        if (corpNames[j].Id != loyaltyPoint.CorporationId) continue;

                        lpData[i, j + 1 ] = loyaltyPoint.Points.ToString();
                        break;
                    }
                }
            }

            // Populate DataTable Columns
            Table = new DataTable();
            Table.Columns.Add(new DataColumn("Names"));
            foreach (var corporation in corpNames)
            {
                Table.Columns.Add(new DataColumn(corporation.Name));
            }

            // Populate DataTable
            for (var i = 0; i < lpData.GetLength(0); i++)
            {
                var row = Table.NewRow();
                for (var j = 0; j < lpData.GetLength(1); j++)
                {
                    row[j] = lpData[i, j];
                }
                Table.Rows.Add(row);
            }

            DataGrid.ItemsSource = Table.DefaultView;

        }
    }
}

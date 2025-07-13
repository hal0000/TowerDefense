namespace TowerDefense.Model
{
  /// <summary>
  ///     embedded data for mockup api call simulation
  /// </summary>
  public static class EmbeddedBuildingData
    {
        public const string TowerJson = @"
      {
        ""Templates"": [
          {
            ""Index"": 0,
            ""Name"": ""Kunst-Team"",
            ""Gold"": 5000,
            ""Footprint"": [ ""111"", ""010"", ""010"" ],
            ""Range"": 3,
            ""Damage"": 40,
            ""Level"": 1,
            ""FireRate"": 2
          },
          {
            ""Index"": 1,
            ""Name"": ""QA-Team"",
            ""Gold"": 10000,
            ""Footprint"": [ ""10"", ""10"", ""11"" ],
            ""Range"": 4,
            ""Damage"": 25,
            ""Level"": 1,
            ""FireRate"": 3
          },
          {
            ""Index"": 2,
            ""Name"": ""Entwicklung"",
            ""Gold"": 5000,
            ""Footprint"": [ ""011"", ""110"" ],
            ""Range"": 4,
            ""Damage"": 40,
            ""Level"": 1,
            ""FireRate"": 2
          },
          {
            ""Index"": 3,
            ""Name"": ""Projektmanagement"",
            ""Gold"": 25000,
            ""Footprint"": [ ""101"", ""101"", ""111"" ],
            ""Range"": 8,
            ""Damage"": 100,
            ""Level"": 1,
            ""FireRate"": 3
          },
          {
            ""Index"": 4,
            ""Name"": ""WerkStudent"",
            ""Gold"": 1000,
            ""Footprint"": [ ""11"", ""10"", ""11"" ],
            ""Range"": 2,
            ""Damage"": 15,
            ""Level"": 1,
            ""FireRate"": 3
          },
          {
            ""Index"": 5,
            ""Name"": ""Praktikum"",
            ""Gold"": 1000,
            ""Footprint"": [ ""1"", ""1"", ""1"" ],
            ""Range"": 1,
            ""Damage"": 15,
            ""Level"": 1,
            ""FireRate"": 3
          },
          {
            ""Index"": 6,
            ""Name"": ""Kundensupport"",
            ""Gold"": 10000,
            ""Footprint"": [ ""010"", ""111"", ""010"" ],
            ""Range"": 4,
            ""Damage"": 50,
            ""Level"": 1,
            ""FireRate"": 3
          }
        ]
      }";

        public const string EnemyJson = @"
        {
          ""Enemies"": [
            {
              ""Type"": 0,
              ""Name"": ""Vogel"",
              ""Speed"": 2.5,
              ""TurnRate"": 90.0,
              ""TurnRadius"": 0.2,
              ""Damage"": 5,
              ""Gold"": 200,
              ""Health"": 200
            },
            {
              ""Type"": 1,
              ""Name"": ""Hund"",
              ""Speed"": 3.0,
              ""TurnRate"": 60.0,
              ""TurnRadius"": 0.5,
              ""Damage"": 8,
              ""Gold"": 150,
              ""Health"": 300
            },
            {
              ""Type"": 2,
              ""Name"": ""Katze"",
              ""Speed"": 4.0,
              ""TurnRate"": 120.0,
              ""TurnRadius"": 0.3,
              ""Damage"": 3,
              ""Gold"": 100,
              ""Health"": 150
            }
          ]
        }";
    }
}
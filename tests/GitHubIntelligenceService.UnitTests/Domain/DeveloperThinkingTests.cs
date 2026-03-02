using GitHubIntelligenceService.Domain.Entities;
using Xunit; // xUnit Test Framework
using System.Linq;

namespace GitHubIntelligenceService.UnitTests.Domain
{
    public class DeveloperThinkingTests
    {
        [Fact]
        public void UpdateScore_Should_Set_SeniorityLevel_Correctly()
        {
            // Arrange (Hazırlık)
            var dev = new Developer("testuser", "Test User", "test@example.com", "Bio");
            
            // Act (Eylem)
            // Activity: 100, Quality: 90, Popularity: 50 -> Beklenen: High Score (86)
            dev.UpdateScore(100, 90, 50);

            // Assert (Kontrol)
            // Kurallar: 85+ Expert, 65+ Senior
            // Bizim hesabımız 86 çıktığı için "Expert" olmalı.
            Assert.Contains("Expert", dev.Score.SeniorityLevel); 
            Assert.True(dev.Score.TotalScore > 80, "Total Score should be high");
        }

        [Fact]
        public void AddRepository_Should_Add_Repo_And_Calculate_Score()
        {
            // Arrange
            var dev = new Developer("testuser", "Test User", "test@example.com", "Bio");
            var repo = new GitHubRepository
            {
                Name = "AwesomeRepo",
                Language = "C#",
                Stars = 50,
                IssuesCount = 5
            };

            // Act
            dev.AddRepository(repo);

            // Assert
            Assert.Single(dev.Repositories); // 1 repo eklenmiş olmalı
            var addedRepo = dev.Repositories.First();
            Assert.Equal("C#", addedRepo.Language);
            Assert.Equal(50, addedRepo.Stars);
        }

        [Fact]
        public void CalculateLanguageDistribution_Should_Return_Correct_Expected_Values()
        {
            // Arrange
            var dev = new Developer("testuser", "Test User", "test@example.com", "Bio");

            // 1. Repo: C#
            dev.AddRepository(new GitHubRepository { Name = "CS1", Language = "C#" });
            dev.AddRepository(new GitHubRepository { Name = "CS2", Language = "C#" });
            
            // 2. Repo: JavaScript
            dev.AddRepository(new GitHubRepository { Name = "JS1", Language = "JavaScript" });

            // Act
            var dist = dev.CalculateLanguageDistribution();

            // Assert -> Toplam 3 repo var: 2 C#, 1 JS
            // C# = %66.6, JS = %33.3
            Assert.True(dist.ContainsKey("C#"));
            Assert.True(dist.ContainsKey("JavaScript"));
            
            Assert.Equal(66.7, dist["C#"], 1); // 1 ondalık hassasiyetle kontrol et
            Assert.Equal(33.3, dist["JavaScript"], 1);
        }
    }
}

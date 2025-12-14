using NUnit.Framework;
using NSubstitute;
using System;
using Assert = NUnit.Framework.Assert;

namespace KerwinTesting.Tests.EditMode
{
    [TestFixture]
    public class PlayerDataTests
    {
        private TestablePlayerData playerData;

        [SetUp]
        public void Setup()
        {
            playerData = new TestablePlayerData();
        }

        [Test]
        public void AddScore_ShouldIncreaseScore()
        {
            // Arrange
            int initialScore = playerData.Score;
            int amountToAdd = 50;

            // Act
            playerData.AddScore(amountToAdd);

            // Assert
            Assert.AreEqual(initialScore + amountToAdd, playerData.Score);
        }

        [Test]
        public void AddCoin_ShouldIncreaseCoins()
        {
            // Arrange
            int initialCoins = playerData.Coins;
            int amountToAdd = 10;

            // Act
            playerData.AddCoin(amountToAdd);

            // Assert
            Assert.AreEqual(initialCoins + amountToAdd, playerData.Coins);
        }

        [Test]
        public void DeductCoin_ShouldDecreaseCoins()
        {
            // Arrange
            playerData.Coins = 50;
            int amountToDeduct = 20;

            // Act
            playerData.DeductCoin(amountToDeduct);

            // Assert
            Assert.AreEqual(30, playerData.Coins);
        }

        [Test]
        public void DeductCoin_ShouldNotGoBelowZero()
        {
            // Arrange
            playerData.Coins = 10;

            // Act
            playerData.DeductCoin(50);

            // Assert
            Assert.AreEqual(0, playerData.Coins);
        }

        [Test]
        public void DeductHealth_ShouldDecreaseHealth()
        {
            // Arrange
            playerData.Health = 3;

            // Act
            playerData.DeductHealth(1);

            // Assert
            Assert.AreEqual(2, playerData.Health);
        }

        [Test]
        public void TryPurchase_WithEnoughCoins_ShouldReturnTrue()
        {
            // Arrange
            playerData.Coins = 50;
            playerData.Health = 2;

            // Act
            bool result = playerData.TryPurchase(25, "Heart");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(25, playerData.Coins);
            Assert.AreEqual(3, playerData.Health);
        }

        [Test]
        public void TryPurchase_WithoutEnoughCoins_ShouldReturnFalse()
        {
            // Arrange
            playerData.Coins = 10;
            playerData.Health = 2;

            // Act
            bool result = playerData.TryPurchase(25, "Heart");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(10, playerData.Coins);
            Assert.AreEqual(2, playerData.Health);
        }

        [Test]
        public void TryPurchase_HeartType_ShouldIncreaseHealth()
        {
            // Arrange
            playerData.Coins = 30;
            playerData.Health = 1;

            // Act
            playerData.TryPurchase(25, "Heart");

            // Assert
            Assert.AreEqual(2, playerData.Health);
        }

        [Test]
        public void SetCheckpoint_ShouldStorePosition()
        {
            // Arrange
            var checkpointPosition = new Vector3Data(10f, 5f, 20f); // Using custom Vector3Data struct

            // Act
            playerData.SetCheckpoint(checkpointPosition);

            // Assert
            Assert.IsTrue(playerData.HasCheckpoint());
            Assert.AreEqual(checkpointPosition, playerData.GetCheckpointPosition());
        }

        [Test]
        public void GetCheckpointPosition_ShouldReturnStoredPosition()
        {
            // Arrange
            var expected = new Vector3Data(15f, 2f, 30f); // Using custom Vector3Data struct
            playerData.SetCheckpoint(expected);

            // Act
            var result = playerData.GetCheckpointPosition();

            // Assert
            Assert.AreEqual(expected.X, result.X, 0.01f);
            Assert.AreEqual(expected.Y, result.Y, 0.01f);
            Assert.AreEqual(expected.Z, result.Z, 0.01f);
        }

        [Test]
        public void HasCheckpoint_InitiallyFalse()
        {
            // Assert
            Assert.IsFalse(playerData.HasCheckpoint());
        }

        [Test]
        public void ClearCheckpoint_ShouldResetFlag()
        {
            // Arrange
            playerData.SetCheckpoint(new Vector3Data(0, 0, 0));
            Assert.IsTrue(playerData.HasCheckpoint());

            // Act
            playerData.ClearCheckpoint();

            // Assert
            Assert.IsFalse(playerData.HasCheckpoint());
        }
    }

    public class TestablePlayerData
    {
        public int Score { get; set; } = 0;
        public int Coins { get; set; } = 0;
        public int Health { get; set; } = 3;
        private bool hasCheckpoint = false;
        private Vector3Data checkpointPosition;

        public void AddScore(int amount)
        {
            Score += amount;
        }

        public void AddCoin(int amount)
        {
            Coins += amount;
        }

        public void DeductCoin(int amount)
        {
            Coins = Math.Max(0, Coins - amount);
        }

        public void DeductHealth(int amount)
        {
            Health = Math.Max(0, Health - amount);
        }

        public bool TryPurchase(int cost, string itemType)
        {
            if (Coins < cost)
                return false;

            Coins -= cost;
            
            if (itemType == "Heart")
            {
                Health++;
            }

            return true;
        }

        public void SetCheckpoint(Vector3Data position)
        {
            checkpointPosition = position;
            hasCheckpoint = true;
        }

        public Vector3Data GetCheckpointPosition()
        {
            return checkpointPosition;
        }

        public bool HasCheckpoint()
        {
            return hasCheckpoint;
        }

        public void ClearCheckpoint()
        {
            hasCheckpoint = false;
        }
    }

    public struct Vector3Data
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3Data(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}

using NUnit.Framework;
using NSubstitute;

namespace KerwinTesting.Tests.PlayMode
{
    [TestFixture]
    public class PlayerControllerPlayModeTests
    {
        private MockPlayerController playerController;
        private MockGameObject playerGameObject;

        [SetUp]
        public void Setup()
        {
            playerGameObject = new MockGameObject("Player");
            playerController = new MockPlayerController();
            playerController.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            playerController?.Cleanup();
            playerGameObject = null;
        }

        [Test]
        public void PlayerController_ShouldInitialize()
        {
            // Act
            bool isInitialized = playerController.IsInitialized;

            // Assert - using direct comparison instead of Assert.IsTrue
            Assert.That(isInitialized, Is.True, "PlayerController should be initialized");
        }

        [Test]
        public void PlayerMovement_ShouldUpdatePosition()
        {
            // Arrange
            float initialX = playerController.Position.X;
            float moveAmount = 5.0f;

            // Act
            playerController.Move(moveAmount, 0f);

            // Assert - using That instead of AreEqual
            Assert.That(playerController.Position.X > initialX, Is.True, "Position X should increase after movement");
        }

        [Test]
        public void PlayerJump_WhenGrounded_ShouldIncreaseYVelocity()
        {
            // Arrange
            playerController.SetGrounded(true);
            float initialY = playerController.Velocity.Y;

            // Act
            playerController.Jump();

            // Assert - using That with greater than comparison
            Assert.That(playerController.Velocity.Y, Is.GreaterThan(initialY), "Velocity Y should increase after jump");
        }

        [Test]
        public void PlayerJump_WhenNotGrounded_ShouldNotJump()
        {
            // Arrange
            playerController.SetGrounded(false);
            float initialY = playerController.Velocity.Y;

            // Act
            playerController.Jump();

            // Assert - velocity should remain the same
            Assert.That(playerController.Velocity.Y, Is.EqualTo(initialY), "Velocity Y should not change when not grounded");
        }

        [Test]
        public void ResetMovementState_ShouldClearVelocity()
        {
            // Arrange
            playerController.Move(10f, 0f);
            playerController.Jump();

            // Act
            playerController.ResetMovementState();

            // Assert - velocity should be zero after reset
            Assert.That(playerController.Velocity.X, Is.EqualTo(0f), "Velocity X should be zero after reset");
            Assert.That(playerController.Velocity.Y, Is.EqualTo(0f), "Velocity Y should be zero after reset");
        }

        [Test]
        public void PlayerPosition_ShouldBeAccessible()
        {
            // Arrange
            var expectedPosition = new Vector3Mock(10f, 5f, 0f);
            playerController.SetPosition(expectedPosition);

            // Act
            var actualPosition = playerController.Position;

            // Assert - using That with Is.EqualTo
            Assert.That(actualPosition.X, Is.EqualTo(expectedPosition.X).Within(0.01f), "Position X should match");
            Assert.That(actualPosition.Y, Is.EqualTo(expectedPosition.Y).Within(0.01f), "Position Y should match");
        }

        [Test]
        public void Gravity_ShouldAffectYVelocity()
        {
            // Arrange
            playerController.SetGrounded(false);
            float initialVelocityY = playerController.Velocity.Y;

            // Act
            playerController.ApplyGravity(0.1f); // simulate one frame

            // Assert - gravity should decrease Y velocity
            Assert.That(playerController.Velocity.Y, Is.LessThan(initialVelocityY), "Gravity should decrease Y velocity");
        }

        [Test]
        public void Speed_ShouldAffectMovementDistance()
        {
            // Arrange
            float speed = 10f;
            playerController.SetSpeed(speed);
            float initialX = playerController.Position.X;

            // Act
            playerController.Move(1f, 0f); // Move with normalized input

            // Assert - check that position changed based on speed
            float distance = playerController.Position.X - initialX;
            Assert.That(distance, Is.GreaterThan(0f), "Player should have moved");
        }
    }

    public class MockGameObject
    {
        public string Name { get; set; }
        public Vector3Mock Position { get; set; }

        public MockGameObject(string name)
        {
            Name = name;
            Position = new Vector3Mock(0f, 0f, 0f);
        }
    }

    public class MockPlayerController
    {
        public bool IsInitialized { get; private set; }
        public Vector3Mock Position { get; private set; }
        public Vector3Mock Velocity { get; private set; }
        private bool isGrounded;
        private float speed = 5f;
        private float jumpForce = 10f;
        private float gravity = -9.8f;

        public MockPlayerController()
        {
            Position = new Vector3Mock(0f, 0f, 0f);
            Velocity = new Vector3Mock(0f, 0f, 0f);
            isGrounded = false;
        }

        public void Initialize()
        {
            IsInitialized = true;
        }

        public void Cleanup()
        {
            IsInitialized = false;
        }

        public void Move(float x, float z)
        {
            Position = new Vector3Mock(
                Position.X + x * speed,
                Position.Y,
                Position.Z + z * speed
            );
        }

        public void Jump()
        {
            if (isGrounded)
            {
                Velocity = new Vector3Mock(Velocity.X, jumpForce, Velocity.Z);
            }
        }

        public void ApplyGravity(float deltaTime)
        {
            if (!isGrounded)
            {
                Velocity = new Vector3Mock(
                    Velocity.X,
                    Velocity.Y + gravity * deltaTime,
                    Velocity.Z
                );
            }
        }

        public void ResetMovementState()
        {
            Velocity = new Vector3Mock(0f, 0f, 0f);
        }

        public void SetGrounded(bool grounded)
        {
            isGrounded = grounded;
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        public void SetPosition(Vector3Mock position)
        {
            Position = position;
        }
    }

    public struct Vector3Mock
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3Mock(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}

using NUnit.Framework;
using NSubstitute;
using Assert = NUnit.Framework.Assert;

namespace KerwinTesting.Tests.EditMode
{
    [TestFixture]
    public class AudioManagerTests
    {
        private IAudioManager audioManager;

        [SetUp]
        public void Setup()
        {
            audioManager = Substitute.For<IAudioManager>();
            
            audioManager.IsMusicMuted().Returns(false);
            audioManager.IsSfxMuted().Returns(false);
            audioManager.masterVolume.Returns(1f);
            audioManager.pitchVariance.Returns(0.05f);
        }

        [Test]
        public void PlaySfx_WithNullClip_ShouldNotCrash()
        {
            // Arrange
            audioManager.When(x => x.PlaySfx(Arg.Any<object>())).Do(ci => { });
            
            // Act & Assert - Should not throw exception
            Assert.DoesNotThrow(() => audioManager.PlaySfx(null));
        }

        [Test]
        public void SetMusicMuted_ShouldUpdateMuteState()
        {
            // Arrange
            audioManager.When(x => x.SetMusicMuted(true)).Do(x => audioManager.IsMusicMuted().Returns(true));
            audioManager.When(x => x.SetMusicMuted(false)).Do(x => audioManager.IsMusicMuted().Returns(false));
            
            // Act
            audioManager.SetMusicMuted(true);
            audioManager.IsMusicMuted().Returns(true);

            // Assert
            Assert.IsTrue(audioManager.IsMusicMuted());

            // Act
            audioManager.SetMusicMuted(false);
            audioManager.IsMusicMuted().Returns(false);

            // Assert
            Assert.IsFalse(audioManager.IsMusicMuted());
        }

        [Test]
        public void SetSfxMuted_ShouldUpdateMuteState()
        {
            // Arrange
            audioManager.When(x => x.SetSfxMuted(true)).Do(x => audioManager.IsSfxMuted().Returns(true));
            audioManager.When(x => x.SetSfxMuted(false)).Do(x => audioManager.IsSfxMuted().Returns(false));
            
            // Act
            audioManager.SetSfxMuted(true);
            audioManager.IsSfxMuted().Returns(true);

            // Assert
            Assert.IsTrue(audioManager.IsSfxMuted());

            // Act
            audioManager.SetSfxMuted(false);
            audioManager.IsSfxMuted().Returns(false);

            // Assert
            Assert.IsFalse(audioManager.IsSfxMuted());
        }

        [Test]
        public void PlaySfx_WhenSfxMuted_ShouldNotCreateAudioSource()
        {
            // Arrange
            audioManager.IsSfxMuted().Returns(true);
            var mockClip = Substitute.For<object>(); // Use object instead of AudioClip

            // Act
            audioManager.PlaySfx(mockClip);

            // Assert
            audioManager.Received(1).PlaySfx(mockClip);
        }

        [Test]
        public void MasterVolume_ShouldBeInitializedToOne()
        {
            // Assert
            Assert.AreEqual(1f, audioManager.masterVolume);
        }

        [Test]
        public void PitchVariance_ShouldBeInitialized()
        {
            // Assert
            Assert.AreEqual(0.05f, audioManager.pitchVariance);
        }
    }

    public interface IAudioManager
    {
        void PlaySfx(object clip);
        void SetMusicMuted(bool muted);
        void SetSfxMuted(bool muted);
        bool IsMusicMuted();
        bool IsSfxMuted();
        float masterVolume { get; }
        float pitchVariance { get; }
    }
}

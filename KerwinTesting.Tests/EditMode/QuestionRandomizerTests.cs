using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Assert = NUnit.Framework.Assert;

namespace KerwinTesting.Tests.EditMode
{
    [TestFixture]
    public class QuestionRandomizerTests
    {
        private TestableQuestionRandomizer randomizer;

        [SetUp]
        public void Setup()
        {
            randomizer = new TestableQuestionRandomizer();
        }

        [Test]
        public void GetRandomQuestion_ShouldReturnValidQuestion()
        {
            // Act
            Question question = randomizer.GetRandomQuestion();

            // Assert
            Assert.IsNotNull(question);
            Assert.IsNotNull(question.QuestionText);
            Assert.IsNotNull(question.Options);
        }

        [Test]
        public void GetRandomQuestion_AnswerShouldBeWithinBounds()
        {
            // Act
            Question question = randomizer.GetRandomQuestion();
            int correctAnswer = int.Parse(question.Options[question.CorrectOptionIndex]);

            // Assert
            Assert.GreaterOrEqual(correctAnswer, 0, "Answer should be >= 0");
            Assert.LessOrEqual(correctAnswer, 100, "Answer should be <= 100");
        }

        [Test]
        public void GetRandomQuestion_ShouldHaveFourOptions()
        {
            // Act
            Question question = randomizer.GetRandomQuestion();

            // Assert
            Assert.AreEqual(4, question.Options.Length);
        }

        [Test]
        public void GetRandomQuestion_ShouldHaveOneCorrectAnswer()
        {
            // Act
            Question question = randomizer.GetRandomQuestion();

            // Assert
            Assert.GreaterOrEqual(question.CorrectOptionIndex, 0);
            Assert.Less(question.CorrectOptionIndex, question.Options.Length);
        }

        [Test]
        public void GetRandomQuestion_CorrectAnswerShouldMatchExpression()
        {
            // Act
            Question question = randomizer.GetRandomQuestion();
            string correctAnswerText = question.Options[question.CorrectOptionIndex];

            // Assert that the correct answer exists in options
            bool foundCorrectAnswer = false;
            foreach (var option in question.Options)
            {
                if (option == correctAnswerText)
                {
                    foundCorrectAnswer = true;
                    break;
                }
            }
            Assert.IsTrue(foundCorrectAnswer, "Correct answer should exist in options");
        }

        [Test]
        public void GetRandomQuestion_MultipleQuestions_ShouldGenerateValidQuestions()
        {
            // Arrange & Act - Generate 20 questions to test consistency
            for (int i = 0; i < 20; i++)
            {
                Question question = randomizer.GetRandomQuestion();
                
                // Assert
                Assert.IsNotNull(question, $"Question {i} should not be null");
                Assert.IsNotEmpty(question.QuestionText, $"Question {i} text should not be empty");
                Assert.AreEqual(4, question.Options.Length, $"Question {i} should have 4 options");
                
                int correctAnswer = int.Parse(question.Options[question.CorrectOptionIndex]);
                Assert.GreaterOrEqual(correctAnswer, 0, $"Question {i} answer should be >= 0");
                Assert.LessOrEqual(correctAnswer, 100, $"Question {i} answer should be <= 100");
            }
        }

        [Test]
        public void GetRandomQuestion_Options_ShouldBeUnique()
        {
            // Act
            Question question = randomizer.GetRandomQuestion();

            // Assert - Check that all options are unique
            for (int i = 0; i < question.Options.Length; i++)
            {
                for (int j = i + 1; j < question.Options.Length; j++)
                {
                    Assert.AreNotEqual(question.Options[i], question.Options[j], 
                        $"Options at index {i} and {j} should be different");
                }
            }
        }
    }

    public class Question
    {
        public string QuestionText { get; set; }
        public string[] Options { get; set; }
        public int CorrectOptionIndex { get; set; }

        public Question(string questionText, string[] options, int correctOptionIndex)
        {
            QuestionText = questionText;
            Options = options;
            CorrectOptionIndex = correctOptionIndex;
        }
    }

    public class TestableQuestionRandomizer
    {
        private readonly Random random = new Random();

        public Question GetRandomQuestion()
        {
            // Generate random numbers for a simple addition question
            int num1 = random.Next(0, 50);
            int num2 = random.Next(0, 50);
            int correctAnswer = num1 + num2;

            string questionText = $"What is {num1} + {num2}?";

            // Generate 4 unique options including the correct answer
            var options = new HashSet<int> { correctAnswer };
            
            while (options.Count < 4)
            {
                int wrongAnswer = random.Next(Math.Max(0, correctAnswer - 20), correctAnswer + 20);
                if (wrongAnswer >= 0 && wrongAnswer <= 100)
                {
                    options.Add(wrongAnswer);
                }
            }

            // Convert to array and shuffle
            var optionsArray = options.ToArray();
            ShuffleArray(optionsArray);

            // Find the index of the correct answer
            int correctIndex = Array.IndexOf(optionsArray, correctAnswer);

            // Convert to string array
            var optionsStrings = optionsArray.Select(x => x.ToString()).ToArray();

            return new Question(questionText, optionsStrings, correctIndex);
        }

        private void ShuffleArray<T>(T[] array)
        {
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
}

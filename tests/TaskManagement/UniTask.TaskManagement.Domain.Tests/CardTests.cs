using System;
using System.Collections.Generic;
using System.Text;
using UniTask.TaskManagement.Domain.Entities;

namespace UniTask.TaskManagement.Domain.Tests
{
    public class CardTests
    {


        [Theory]
        [InlineData("3782 8224 6310 005")]
        [InlineData("4111 1111-1111 1111")]
        [InlineData("4000-0000-0000-6")]
        [InlineData("4000-0000-0000-0000-006")]
        [InlineData("4000000000000000006")]

        public void Check_Card_Digits_Amount_ShouldReturnCorrectResult(string c)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result = testClass.ValidateCard(c);

            //assert
            Assert.True(result);

        }

        [Theory]
        [InlineData("4111 1111 11")]
        [InlineData("3782-8224-63")]
        [InlineData("4111111")]
        [InlineData("111")]
        [InlineData("4111 1111 1111 1111 1116")]
        [InlineData("4111-1111-1111-1111-1111-1")]
        [InlineData("1111111111111111111111")]
        public void Check_Card_Digits_Amount_ShouldReturnWrongResult(string c)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result = testClass.ValidateCard(c);

            //assert
            Assert.True(result);

        }


        [Theory]
        [InlineData("3782 8224 6310 005")]
        [InlineData("3782-8224-6310-005")]
        [InlineData("3782-8224 6310-005")]
        [InlineData("3782 8224-6310 005")]
        [InlineData("37828224 6310005")]
        public void Check_Card_Allowed_Symbols_ShouldReturnCorrectResult(string c)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result = testClass.ValidateCard(c);

            //assert
            Assert.True(result);

        }

        [Theory]
        [InlineData("4532 0151 1283 036a")]
        [InlineData("4532.0151.1283.0366")]
        [InlineData("4532_0151_1283_0366")]
        [InlineData("4532#0151#1283#0366")]
        [InlineData("4532 0151 1283 036$")]
        [InlineData("4532/0151/1283/0366")]
        [InlineData("4532\t015112830366")]
        public void Check_Card_Allowed_Symbols_ShouldReturnWrongResult(string c)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result = testClass.ValidateCard(c);

            //assert
            Assert.True(result);

        }


        [Theory]
        [InlineData("4111111111111111")]
        [InlineData("4000000000000002")]
        [InlineData("4532 0151 1283 0366")]
        [InlineData("4111-1111-1111-1111")]
        [InlineData("3782 8224 6310 005")]
        [InlineData("371449635398431")]
        [InlineData("4012888888881881")]

        public void Check_Card_Luhn_algorithm_ShouldReturnCorrectResult(string c)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result = testClass.ValidateCard(c);

            //assert
            Assert.True(result);

        }

        [Theory]
        [InlineData("4111111111111112")]
        [InlineData("4000000000000003")]
        [InlineData("4532 0151 1283 0368")]
        [InlineData("3782 8224 6310 009")]
        [InlineData("371449635398430")]
        [InlineData("4012888888881885")]
        public void Check_Card_Luhn_algorithm_ShouldReturnWrongResult(string c)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result = testClass.ValidateCard(c);

            //assert
            Assert.True(result);

        }

        [Theory]
        [InlineData("4111111111111", "Visa")]
        [InlineData("5111111111111116", "MasterCard")]
        [InlineData("378282246310005", "American Express")]
        [InlineData("6011111111111114", "Discover")]
        [InlineData("30050000000009", "Diners Club")]
        [InlineData("3528000000000002", "JCB")]
        [InlineData("598000000000002", "Maestro")]
        public void Check_Card_Identifing_Type_ShouldReturnCorrectResult(string cardNumber, string cardType)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result = testClass.GetCardType(cardNumber);

            //assert
            Assert.Equal(cardType, result);

        }

        [Theory]
        [InlineData("1111111111111111", "Visa")]
        [InlineData("1111111111111111", "MasterCard")]
        [InlineData("1111111111111111", "American Express")]
        [InlineData("1111111111111111", "Discover")]
        [InlineData("1111111111111111", "Diners Club")]
        [InlineData("1111111111111111", "JCB")]
        [InlineData("1111111111111111", "Maestro")]
        public void Check_Card_Identifing_Type_ShouldReturnWrongResult(string cardNumber, string cardType)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result = testClass.GetCardType(cardNumber);

            //assert
            Assert.Equal(cardType, result);

        }

        [Theory]
        [InlineData("3497 7965 8312 797", "American Express")]
        [InlineData("345-470-784-783-010", "American Express")]
        [InlineData("378523393817437", "American Express")]
        [InlineData("4024-0071-6540-1778", "Visa")]
        [InlineData("4532 2080 2150 4434", "Visa")]
        [InlineData("4532289052809181", "Visa")]
        [InlineData("5530016454538418", "MasterCard")]
        [InlineData("5551561443896215", "MasterCard")]
        [InlineData("5131208517986691", "MasterCard")]
        public void Check_Samples_From_exercise_Should_Return_Correct_Result(string cardNumber, string cardType)
        {

            // Arrange
            var testClass = new Card();

            //act
            var result1 = testClass.ValidateCard(cardNumber);
            var result2 = testClass.GetCardType(cardNumber);

            //assert
            Assert.Equal(cardType, result2);
            Assert.True(result1);


        }

    }
}

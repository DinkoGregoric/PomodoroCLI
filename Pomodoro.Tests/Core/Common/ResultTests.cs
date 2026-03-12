using AwesomeAssertions;
using Pomodoro.Core.Common;

namespace Pomodoro.Tests.Core.Common;

public class ResultTests
{
    // --- Error ---

    [Fact]
    public void Error_None_HasEmptyCodeAndMessage()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
    }

    // --- Result (non-generic) ---

    [Fact]
    public void Success_IsSuccessTrue_IsFailureFalse_ErrorIsNone()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_IsSuccessFalse_IsFailureTrue_ErrorPreserved()
    {
        var error = new Error("Some.Code", "Some message.");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithErrorNone_ThrowsArgumentException()
    {
        var act = () => Result.Failure(Error.None);

        act.Should().Throw<ArgumentException>();
    }

    // --- Result<TValue> ---

    [Fact]
    public void ResultT_Success_IsSuccessTrue_ValueReturnsValue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void ResultT_Failure_IsSuccessFalse_ErrorPreserved()
    {
        var error = new Error("T.Code", "T message.");

        var result = Result<int>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ResultT_Failure_AccessingValue_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Failure(new Error("X", "Y"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResultT_Failure_WithErrorNone_ThrowsArgumentException()
    {
        var act = () => Result<int>.Failure(Error.None);

        act.Should().Throw<ArgumentException>();
    }
}

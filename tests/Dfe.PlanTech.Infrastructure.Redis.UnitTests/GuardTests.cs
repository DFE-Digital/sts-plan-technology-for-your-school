using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class GuardTests
{
    static Exception ExceptionFactory() => new GuardException("Custom exception");
    static bool Constraint(int x) => x > 10;

    [Fact]
    public void IsNotNull_ThrowsException_WhenObjectIsNull()
    {
        string name = "test";

        Assert.Throws<GuardException>(() => GuardModel.IsNotNull(null, name));
    }

    [Fact]
    public void IsNotNull_ThrowsCustomException_WhenObjectIsNull()
    {
        Assert.Throws<GuardException>(() => GuardModel.IsNotNull(null, ExceptionFactory));
    }

    [Fact]
    public void IsNotNull_DoesNotThrowException_WhenObjectIsNotNull()
    {
        var obj = new object();
        string name = "test";

        GuardModel.IsNotNull(obj, name);
        Assert.True(true);
    }

    [Fact]
    public void IsFalse_ThrowsException_WhenConditionIsTrue()
    {
        bool condition = true;
        string message = "Test message";

        Assert.Throws<GuardException>(() => GuardModel.IsFalse(condition, message));
    }

    [Fact]
    public void IsFalse_DoesNotThrowException_WhenConditionIsFalse()
    {
        bool condition = false;
        string message = "Test message";

        GuardModel.IsFalse(condition, message);
        Assert.True(true);
    }

    [Fact]
    public void IsTrue_ThrowsException_WhenConditionIsFalse()
    {
        bool condition = false;
        string message = "Test message";

        Assert.Throws<GuardException>(() => GuardModel.IsTrue(condition, message));
    }

    [Fact]
    public void IsTrue_DoesNotThrowException_WhenConditionIsTrue()
    {
        bool condition = true;
        string message = "Test message";

        GuardModel.IsTrue(condition, message);
        Assert.True(true);
    }

    [Fact]
    public void IsTrueWithConstraint_ThrowsException_WhenConstraintIsFalse()
    {
        int value = 5;
        static bool constraint(int x) => x > 10;
        string message = "Value must be greater than {0}";

        Assert.Throws<GuardException>(() => GuardModel.IsTrue(value, constraint, message));
    }

    [Fact]
    public void IsTrueWithConstraint_DoesNotThrowException_WhenConstraintIsTrue()
    {
        int value = 15;
        string message = "Value must be greater than {0}";

        GuardModel.IsTrue(value, Constraint, message);
        Assert.True(true);
    }

    [Fact]
    public void IsTrueWithExceptionType_ThrowsExceptionOfSpecifiedType_WhenConditionIsFalse()
    {
        bool condition = false;

        Assert.ThrowsAny<GuardException>(() => GuardModel.IsTrue(condition, "error"));
    }

    [Fact]
    public void IsTrueWithExceptionType_DoesNotThrowException_WhenConditionIsTrue()
    {
        bool condition = true;

        GuardModel.IsTrue(condition, "not an error");
        Assert.True(true);
    }

    [Fact]
    public void IsTrue_ThrowsTypedException()
    {
        bool condition = false;
        Assert.Throws<InvalidOperationException>(() => GuardModel.IsTrue<InvalidOperationException>(condition));
    }

    [Fact]
    public void IsTrueWithFunc_DoesNotThrowException_WhenConditionIsTrue()
    {
        bool condition = true;

        GuardModel.IsTrue(condition, ExceptionFactory);
        Assert.True(true);
    }

    [Fact]
    public void IsTrueWithFunc_ThrowsException_WhenConditionIsFalse()
    {
        bool condition = false;
        Assert.Throws<GuardException>(() => GuardModel.IsTrue(condition, ExceptionFactory));
    }


    [Fact]
    public void IsFalseWithExceptionType_ThrowsExceptionOfSpecifiedType_WhenConditionIsTrue()
    {
        bool condition = true;

        Assert.ThrowsAny<GuardException>(() => GuardModel.IsFalse(condition, "error"));
    }

    [Fact]
    public void IsFalseWithExceptionType_DoesNotThrowException_WhenConditionIsFalse()
    {
        bool condition = false;

        GuardModel.IsFalse(condition, "should not error");
        Assert.True(true);
    }

    [Fact]
    public void IsFalseWithFunc_ThrowsException_WhenConditionIsTrue()
    {
        bool condition = true;
        Assert.Throws<GuardException>(() => GuardModel.IsFalse(condition, ExceptionFactory));
    }

    [Fact]
    public void IsFalseWithFunc_DoesNotThrowException_WhenConditionIsFalse()
    {
        bool condition = false;

        GuardModel.IsFalse(condition, ExceptionFactory);
        Assert.True(true);
    }

    [Fact]
    public void IsFalse_ThrowsTypedException()
    {
        bool condition = true;
        Assert.Throws<InvalidOperationException>(() => GuardModel.IsFalse<InvalidOperationException>(condition));
    }

    [Fact]
    public void When_Calls_Action_WhenTrue()
    {
        var stringToModify = "";
        var expectedResult = "modified";
        void action() => stringToModify = expectedResult;
        GuardModel.When(true, action);

        Assert.Equal(expectedResult, stringToModify);
    }

    [Fact]
    public void When_DoesNotCall_Action_WhenFalse()
    {
        var stringToModify = "";
        var shouldNotEqualThis = "modified";
        void action() => stringToModify = shouldNotEqualThis;
        GuardModel.When(false, action);

        Assert.Equal("", stringToModify);
    }

}

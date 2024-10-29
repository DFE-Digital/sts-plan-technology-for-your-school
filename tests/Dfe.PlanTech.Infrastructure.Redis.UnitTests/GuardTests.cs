using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Caching.Models;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;
public class GuardTests
{
    static Exception ExceptionFactory() => new GuardException("Custom exception");
    static bool Constraint(int x) => x > 10;

    [Fact]
    public void IsNotNull_ThrowsException_WhenObjectIsNull()
    {
        string name = "test";

        Assert.Throws<GuardException>(() => Guard.IsNotNull(null, name));
    }

    [Fact]
    public void IsNotNull_ThrowsCustomException_WhenObjectIsNull()
    {
        Assert.Throws<GuardException>(() => Guard.IsNotNull(null, ExceptionFactory));
    }

    [Fact]
    public void IsNotNull_DoesNotThrowException_WhenObjectIsNotNull()
    {
        var obj = new object();
        string name = "test";

        Guard.IsNotNull(obj, name);
    }

    [Fact]
    public void IsFalse_ThrowsException_WhenConditionIsTrue()
    {
        bool condition = true;
        string message = "Test message";

        Assert.Throws<GuardException>(() => Guard.IsFalse(condition, message));
    }

    [Fact]
    public void IsFalse_DoesNotThrowException_WhenConditionIsFalse()
    {
        bool condition = false;
        string message = "Test message";

        Guard.IsFalse(condition, message);
    }

    [Fact]
    public void IsTrue_ThrowsException_WhenConditionIsFalse()
    {
        bool condition = false;
        string message = "Test message";

        Assert.Throws<GuardException>(() => Guard.IsTrue(condition, message));
    }

    [Fact]
    public void IsTrue_DoesNotThrowException_WhenConditionIsTrue()
    {
        bool condition = true;
        string message = "Test message";

        Guard.IsTrue(condition, message);
    }

    [Fact]
    public void IsTrueWithConstraint_ThrowsException_WhenConstraintIsFalse()
    {
        int value = 5;
        static bool constraint(int x) => x > 10;
        string message = "Value must be greater than {0}";

        Assert.Throws<GuardException>(() => Guard.IsTrue(value, constraint, message));
    }

    [Fact]
    public void IsTrueWithConstraint_DoesNotThrowException_WhenConstraintIsTrue()
    {
        int value = 15;
        string message = "Value must be greater than {0}";

        Guard.IsTrue(value, Constraint, message);
    }

    [Fact]
    public void IsTrueWithExceptionType_ThrowsExceptionOfSpecifiedType_WhenConditionIsFalse()
    {
        bool condition = false;

        Assert.ThrowsAny<GuardException>(() => Guard.IsTrue(condition, "error"));
    }

    [Fact]
    public void IsTrueWithExceptionType_DoesNotThrowException_WhenConditionIsTrue()
    {
        bool condition = true;

        Guard.IsTrue(condition, "not an error");
        Assert.True(true);
    }

    [Fact]
    public void IsTrue_ThrowsTypedException()
    {
        bool condition = false;
        Assert.Throws<InvalidOperationException>(() => Guard.IsTrue<InvalidOperationException>(condition));
    }

    [Fact]
    public void IsTrueWithFunc_DoesNotThrowException_WhenConditionIsTrue()
    {
        bool condition = true;

        Guard.IsTrue(condition, ExceptionFactory);
        Assert.True(true);
    }

    [Fact]
    public void IsTrueWithFunc_ThrowsException_WhenConditionIsFalse()
    {
        bool condition = false;
        Assert.Throws<GuardException>(() => Guard.IsTrue(condition, ExceptionFactory));
    }


    [Fact]
    public void IsFalseWithExceptionType_ThrowsExceptionOfSpecifiedType_WhenConditionIsTrue()
    {
        bool condition = true;

        Assert.ThrowsAny<GuardException>(() => Guard.IsFalse(condition, "error"));
    }

    [Fact]
    public void IsFalseWithExceptionType_DoesNotThrowException_WhenConditionIsFalse()
    {
        bool condition = false;

        Guard.IsFalse(condition, "should not error");
        Assert.True(true);
    }

    [Fact]
    public void IsFalseWithFunc_ThrowsException_WhenConditionIsTrue()
    {
        bool condition = true;
        Assert.Throws<GuardException>(() => Guard.IsFalse(condition, ExceptionFactory));
    }

    [Fact]
    public void IsFalseWithFunc_DoesNotThrowException_WhenConditionIsFalse()
    {
        bool condition = false;

        Guard.IsFalse(condition, ExceptionFactory);
        Assert.True(true);
    }

    [Fact]
    public void IsFalse_ThrowsTypedException()
    {
        bool condition = true;
        Assert.Throws<InvalidOperationException>(() => Guard.IsFalse<InvalidOperationException>(condition));
    }

    [Fact]
    public void When_Calls_Action_WhenTrue()
    {
        var stringToModify = "";
        var expectedResult = "modified";
        void action() => stringToModify = expectedResult;
        Guard.When(true, action);

        Assert.Equal(expectedResult, stringToModify);
    }

    [Fact]
    public void When_DoesNotCall_Action_WhenFalse()
    {
        var stringToModify = "";
        var shouldNotEqualThis = "modified";
        void action() => stringToModify = shouldNotEqualThis;
        Guard.When(false, action);

        Assert.Equal("", stringToModify);
    }

}

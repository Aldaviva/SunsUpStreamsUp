using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using SolCalc;

#pragma warning disable CS0659, S1206 // Ignore not overriding Object.GetHashCode()

// ReSharper disable InconsistentNaming - extension methods

/*
 * These classes were copied from FluentAssertions' DateTimeOffsetAssertions classes
 */

namespace Tests;

public static class ZonedDateTimeAssertionExtensions {

    public static ZonedDateTimeAssertions Should(this ZonedDateTime actualValue) {
        return new ZonedDateTimeAssertions(actualValue);
    }

}

/// <summary>
/// Contains a number of methods to assert that a <see cref="ZonedDateTime"/> is in the expected state.
/// </summary>
public class ZonedDateTimeAssertions(ZonedDateTime? subject): ZonedDateTimeAssertions<ZonedDateTimeAssertions>(subject);

/// <summary>
/// Contains a number of methods to assert that a <see cref="ZonedDateTime"/> is in the expected state.
/// </summary>
public class ZonedDateTimeAssertions<TAssertions>(ZonedDateTime? subject) where TAssertions: ZonedDateTimeAssertions<TAssertions> {

    public ZonedDateTime? Subject { get; } = subject;

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is within the specified time
    /// from the specified <paramref name="nearbyTime"/> value.
    /// </summary>
    /// <remarks>
    /// Use this assertion when, for example the database truncates datetimes to nearest 20ms. If you want to assert to the exact datetime,
    /// use <see cref="Be(ZonedDateTime, string, object[])"/>.
    /// </remarks>
    /// <param name="nearbyTime">
    /// The expected time to compare the actual value with.
    /// </param>
    /// <param name="precision">
    /// The maximum amount of time which the two values may differ.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="precision"/> is negative.</exception>
    public AndConstraint<TAssertions> BeCloseTo(ZonedDateTime   nearbyTime, Duration precision,
                                                string          because = "",
                                                params object[] becauseArgs) {
        if (precision < Duration.Zero) {
            throw new ArgumentOutOfRangeException(nameof(precision), "The precision must be non-negative.");
        }

        Duration? difference = Subject - nearbyTime;

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected {context:the date and time} to be within {0} from {1}{reason}", precision, nearbyTime)
            .ForCondition(Subject is not null)
            .FailWith(", but found <null>.")
            .Then
            .ForCondition(difference.Abs() <= precision)
            .FailWith(", but {0} was off by {1}.", Subject, difference)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is not within the specified time
    /// from the specified <paramref name="distantTime"/> value.
    /// </summary>
    /// <remarks>
    /// Use this assertion when, for example the database truncates datetimes to nearest 20ms. If you want to assert to the exact datetime,
    /// use <see cref="NotBe(ZonedDateTime, string, object[])"/>.
    /// </remarks>
    /// <param name="distantTime">
    /// The time to compare the actual value with.
    /// </param>
    /// <param name="precision">
    /// The maximum amount of time which the two values must differ.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="precision"/> is negative.</exception>
    public AndConstraint<TAssertions> NotBeCloseTo(ZonedDateTime   distantTime, Duration precision, string because = "",
                                                   params object[] becauseArgs) {
        if (precision < Duration.Zero) {
            throw new ArgumentOutOfRangeException(nameof(precision), "The precision must be non-negative.");
        }

        Duration? difference = Subject - distantTime;

        Execute.Assertion
            .ForCondition(difference > precision)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Did not expect {context:the date and time} to be within {0} from {1}{reason}, but it was {2}.",
                precision,
                distantTime, Subject);

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> represents the same point in time as the <paramref name="expected"/> value.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> Be(ZonedDateTime   expected, string because = "",
                                         params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected {context:the date and time} to represent the same point in time as {0}{reason}, ",
                expected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject == expected)
            .FailWith("but {0} does not.", Subject)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> represents the same point in time as the <paramref name="expected"/> value.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> Be(ZonedDateTime?  expected, string because = "",
                                         params object[] becauseArgs) {
        if (!expected.HasValue) {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!Subject.HasValue)
                .FailWith("Expected {context:the date and time} to be <null>{reason}, but it was {0}.", Subject);
        } else {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .WithExpectation("Expected {context:the date and time} to represent the same point in time as {0}{reason}, ",
                    expected)
                .ForCondition(Subject.HasValue)
                .FailWith("but found a <null> ZonedDateTime.")
                .Then
                .ForCondition(Subject == expected)
                .FailWith("but {0} does not.", Subject)
                .Then
                .ClearExpectation();
        }

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not represent the same point in time as the <paramref name="unexpected"/> value.
    /// </summary>
    /// <param name="unexpected">The unexpected value</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotBe(ZonedDateTime   unexpected, string because = "",
                                            params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject != unexpected)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Did not expect {context:the date and time} to represent the same point in time as {0}{reason}, but it did.",
                unexpected);

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not represent the same point in time as the <paramref name="unexpected"/> value.
    /// </summary>
    /// <param name="unexpected">The unexpected value</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotBe(ZonedDateTime?  unexpected, string because = "",
                                            params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject != unexpected)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Did not expect {context:the date and time} to represent the same point in time as {0}{reason}, but it did.",
                unexpected);

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is before the specified value.
    /// </summary>
    /// <param name="expected">The <see cref="ZonedDateTime"/> that the current value is expected to be before.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> BeBefore(ZonedDateTime   expected, string because = "",
                                               params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject.HasValue && Subject.Value.ToInstant() < expected.ToInstant())
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:the date and time} to be before {0}{reason}, but it was {1}.", expected,
                Subject);

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/>  is not before the specified value.
    /// </summary>
    /// <param name="unexpected">The <see cref="ZonedDateTime"/>  that the current value is not expected to be before.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotBeBefore(ZonedDateTime   unexpected, string because = "",
                                                  params object[] becauseArgs) {
        return BeOnOrAfter(unexpected, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is either on, or before the specified value.
    /// </summary>
    /// <param name="expected">The <see cref="ZonedDateTime"/> that the current value is expected to be on or before.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> BeOnOrBefore(ZonedDateTime   expected, string because = "",
                                                   params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject.HasValue && Subject.Value.ToInstant() <= expected.ToInstant())
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:the date and time} to be on or before {0}{reason}, but it was {1}.", expected,
                Subject);

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is neither on, nor before the specified value.
    /// </summary>
    /// <param name="unexpected">The <see cref="ZonedDateTime"/> that the current value is not expected to be on nor before.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotBeOnOrBefore(ZonedDateTime   unexpected, string because = "",
                                                      params object[] becauseArgs) {
        return BeAfter(unexpected, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is after the specified value.
    /// </summary>
    /// <param name="expected">The <see cref="ZonedDateTime"/> that the current value is expected to be after.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> BeAfter(ZonedDateTime   expected, string because = "",
                                              params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject.HasValue && Subject.Value.ToInstant() > expected.ToInstant())
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:the date and time} to be after {0}{reason}, but it was {1}.", expected,
                Subject);

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is not after the specified value.
    /// </summary>
    /// <param name="unexpected">The <see cref="ZonedDateTime"/> that the current value is not expected to be after.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotBeAfter(ZonedDateTime   unexpected, string because = "",
                                                 params object[] becauseArgs) {
        return BeOnOrBefore(unexpected, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is either on, or after the specified value.
    /// </summary>
    /// <param name="expected">The <see cref="ZonedDateTime"/> that the current value is expected to be on or after.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> BeOnOrAfter(ZonedDateTime   expected, string because = "",
                                                  params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject.HasValue && Subject.Value.ToInstant() >= expected.ToInstant())
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:the date and time} to be on or after {0}{reason}, but it was {1}.", expected,
                Subject);

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/>  is neither on, nor after the specified value.
    /// </summary>
    /// <param name="unexpected">The <see cref="ZonedDateTime"/>  that the current value is expected not to be on nor after.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotBeOnOrAfter(ZonedDateTime   unexpected, string because = "",
                                                     params object[] becauseArgs) {
        return BeBefore(unexpected, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> year.
    /// </summary>
    /// <param name="expected">The expected year of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> HaveYear(int             expected, string because = "",
                                               params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the year part of {context:the date} to be {0}{reason}, ", expected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Year == expected)
            .FailWith("but it was {0}.", Subject.Value.Year)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not have the <paramref name="unexpected"/> year.
    /// </summary>
    /// <param name="unexpected">The year that should not be in the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotHaveYear(int unexpected, string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the year part of {context:the date} to be {0}{reason}, ", unexpected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Year != unexpected)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> month.
    /// </summary>
    /// <param name="expected">The expected month of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> HaveMonth(int             expected, string because = "",
                                                params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the month part of {context:the date} to be {0}{reason}, ", expected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Month == expected)
            .FailWith("but it was {0}.", Subject.Value.Month)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not have the <paramref name="unexpected"/> month.
    /// </summary>
    /// <param name="unexpected">The month that should not be in the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotHaveMonth(int unexpected, string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the month part of {context:the date} to be {0}{reason}, ", unexpected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Month != unexpected)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> day.
    /// </summary>
    /// <param name="expected">The expected day of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> HaveDay(int             expected, string because = "",
                                              params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the day part of {context:the date} to be {0}{reason}, ", expected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Day == expected)
            .FailWith("but it was {0}.", Subject.Value.Day)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not have the <paramref name="unexpected"/> day.
    /// </summary>
    /// <param name="unexpected">The day that should not be in the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotHaveDay(int unexpected, string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the day part of {context:the date} to be {0}{reason}, ", unexpected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Day != unexpected)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> hour.
    /// </summary>
    /// <param name="expected">The expected hour of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> HaveHour(int             expected, string because = "",
                                               params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the hour part of {context:the time} to be {0}{reason}, ", expected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Hour == expected)
            .FailWith("but it was {0}.", Subject.Value.Hour)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not have the <paramref name="unexpected"/> hour.
    /// </summary>
    /// <param name="unexpected">The hour that should not be in the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotHaveHour(int unexpected, string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the hour part of {context:the time} to be {0}{reason}, ", unexpected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Hour != unexpected)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> minute.
    /// </summary>
    /// <param name="expected">The expected minutes of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> HaveMinute(int             expected, string because = "",
                                                 params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the minute part of {context:the time} to be {0}{reason}, ", expected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Minute == expected)
            .FailWith("but it was {0}.", Subject.Value.Minute)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not have the <paramref name="unexpected"/> minute.
    /// </summary>
    /// <param name="unexpected">The minute that should not be in the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotHaveMinute(int             unexpected, string because = "",
                                                    params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the minute part of {context:the time} to be {0}{reason}, ", unexpected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Minute != unexpected)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> second.
    /// </summary>
    /// <param name="expected">The expected seconds of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> HaveSecond(int             expected, string because = "",
                                                 params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the seconds part of {context:the time} to be {0}{reason}, ", expected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Second == expected)
            .FailWith("but it was {0}.", Subject.Value.Second)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not have the <paramref name="unexpected"/> second.
    /// </summary>
    /// <param name="unexpected">The second that should not be in the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotHaveSecond(int             unexpected, string because = "",
                                                    params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the seconds part of {context:the time} to be {0}{reason}, ", unexpected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Second != unexpected)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> offset.
    /// </summary>
    /// <param name="expected">The expected offset of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> HaveOffset(Offset          expected, string because = "",
                                                 params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the offset of {context:the date} to be {0}{reason}, ", expected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Offset == expected)
            .FailWith("but it was {0}.", Subject.Value.Offset)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> does not have the <paramref name="unexpected"/> offset.
    /// </summary>
    /// <param name="unexpected">The offset that should not be in the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotHaveOffset(Offset          unexpected, string because = "",
                                                    params object[] becauseArgs) {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the offset of {context:the date} to be {0}{reason}, ", unexpected)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Offset != unexpected)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Returns a <see cref="ZonedDateTimeRangeAssertions{TAssertions}"/> object that can be used to assert that the current <see cref="ZonedDateTime"/>
    /// exceeds the specified <paramref name="duration"/> compared to another <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="duration">
    /// The amount of time that the current <see cref="ZonedDateTime"/> should exceed compared to another <see cref="ZonedDateTime"/>.
    /// </param>
    public ZonedDateTimeRangeAssertions<TAssertions> BeMoreThan(Duration duration) {
        return new ZonedDateTimeRangeAssertions<TAssertions>((TAssertions) this, Subject, TimeSpanCondition.MoreThan, duration);
    }

    /// <summary>
    /// Returns a <see cref="ZonedDateTimeRangeAssertions{TAssertions}"/> object that can be used to assert that the current <see cref="ZonedDateTime"/>
    /// is equal to or exceeds the specified <paramref name="duration"/> compared to another <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="duration">
    /// The amount of time that the current <see cref="ZonedDateTime"/> should be equal or exceed compared to
    /// another <see cref="ZonedDateTime"/>.
    /// </param>
    public ZonedDateTimeRangeAssertions<TAssertions> BeAtLeast(Duration duration) {
        return new ZonedDateTimeRangeAssertions<TAssertions>((TAssertions) this, Subject, TimeSpanCondition.AtLeast, duration);
    }

    /// <summary>
    /// Returns a <see cref="ZonedDateTimeRangeAssertions{TAssertions}"/> object that can be used to assert that the current <see cref="ZonedDateTime"/>
    /// differs exactly the specified <paramref name="duration"/> compared to another <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="duration">
    /// The amount of time that the current <see cref="ZonedDateTime"/> should differ exactly compared to another <see cref="ZonedDateTime"/>.
    /// </param>
    public ZonedDateTimeRangeAssertions<TAssertions> BeExactly(Duration duration) {
        return new ZonedDateTimeRangeAssertions<TAssertions>((TAssertions) this, Subject, TimeSpanCondition.Exactly, duration);
    }

    /// <summary>
    /// Returns a <see cref="ZonedDateTimeRangeAssertions{TAssertions}"/> object that can be used to assert that the current <see cref="ZonedDateTime"/>
    /// is within the specified <paramref name="duration"/> compared to another <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="duration">
    /// The amount of time that the current <see cref="ZonedDateTime"/> should be within another <see cref="ZonedDateTime"/>.
    /// </param>
    public ZonedDateTimeRangeAssertions<TAssertions> BeWithin(Duration duration) {
        return new ZonedDateTimeRangeAssertions<TAssertions>((TAssertions) this, Subject, TimeSpanCondition.Within, duration);
    }

    /// <summary>
    /// Returns a <see cref="ZonedDateTimeRangeAssertions{TAssertions}"/> object that can be used to assert that the current <see cref="ZonedDateTime"/>
    /// differs at maximum the specified <paramref name="duration"/> compared to another <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="duration">
    /// The maximum amount of time that the current <see cref="ZonedDateTime"/> should differ compared to another <see cref="ZonedDateTime"/>.
    /// </param>
    public ZonedDateTimeRangeAssertions<TAssertions> BeLessThan(Duration duration) {
        return new ZonedDateTimeRangeAssertions<TAssertions>((TAssertions) this, Subject, TimeSpanCondition.LessThan, duration);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> date.
    /// </summary>
    /// <param name="expected">The expected date portion of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> BeSameDateAs(ZonedDateTime   expected, string because = "",
                                                   params object[] becauseArgs) {
        LocalDate expectedDate = expected.Date;

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the date part of {context:the date and time} to be {0}{reason}, ", expectedDate)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.", expectedDate)
            .Then
            .ForCondition(Subject!.Value.Date == expectedDate)
            .FailWith("but it was {0}.", Subject.Value.Date)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is not the <paramref name="unexpected"/> date.
    /// </summary>
    /// <param name="unexpected">The date that is not to match the date portion of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotBeSameDateAs(ZonedDateTime   unexpected, string because = "",
                                                      params object[] becauseArgs) {
        LocalDate unexpectedDate = unexpected.Date;

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the date part of {context:the date and time} to be {0}{reason}, ", unexpectedDate)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.Date != unexpectedDate)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> has the <paramref name="expected"/> date.
    /// </summary>
    /// <param name="expected">The expected date portion of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> BeSameInstantAs(ZonedDateTime   expected, string because = "",
                                                      params object[] becauseArgs) {
        Instant expectedDate = expected.ToInstant();

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected the instant of {context:the date and time} to be {0}{reason}, ", expectedDate)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.", expectedDate)
            .Then
            .ForCondition(Subject!.Value.ToInstant() == expectedDate)
            .FailWith("but it was {0}.", Subject.Value.Date)
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the current <see cref="ZonedDateTime"/> is not the <paramref name="unexpected"/> date.
    /// </summary>
    /// <param name="unexpected">The date that is not to match the date portion of the current value.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> NotBeSameInstantAs(ZonedDateTime   unexpected, string because = "",
                                                         params object[] becauseArgs) {
        Instant unexpectedDate = unexpected.ToInstant();

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Did not expect the instant of {context:the date and time} to be {0}{reason}, ", unexpectedDate)
            .ForCondition(Subject.HasValue)
            .FailWith("but found a <null> ZonedDateTime.")
            .Then
            .ForCondition(Subject!.Value.ToInstant() != unexpectedDate)
            .FailWith("but it was.")
            .Then
            .ClearExpectation();

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <summary>
    /// Asserts that the <see cref="ZonedDateTime"/> is one of the specified <paramref name="validValues"/>.
    /// </summary>
    /// <param name="validValues">
    /// The values that are valid.
    /// </param>
    public AndConstraint<TAssertions> BeOneOf(params ZonedDateTime?[] validValues) {
        return BeOneOf(validValues, string.Empty);
    }

    /// <summary>
    /// Asserts that the <see cref="ZonedDateTime"/> is one of the specified <paramref name="validValues"/>.
    /// </summary>
    /// <param name="validValues">
    /// The values that are valid.
    /// </param>
    public AndConstraint<TAssertions> BeOneOf(params ZonedDateTime[] validValues) {
        return BeOneOf(validValues.Cast<ZonedDateTime?>());
    }

    /// <summary>
    /// Asserts that the <see cref="ZonedDateTime"/> is one of the specified <paramref name="validValues"/>.
    /// </summary>
    /// <param name="validValues">
    /// The values that are valid.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> BeOneOf(IEnumerable<ZonedDateTime> validValues, string because = "",
                                              params object[]            becauseArgs) {
        return BeOneOf(validValues.Cast<ZonedDateTime?>(), because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the <see cref="ZonedDateTime"/> is one of the specified <paramref name="validValues"/>.
    /// </summary>
    /// <param name="validValues">
    /// The values that are valid.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<TAssertions> BeOneOf(IEnumerable<ZonedDateTime?> validValues, string because = "",
                                              params object[]             becauseArgs) {
        Execute.Assertion
            .ForCondition(validValues.Contains(Subject))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:the date and time} to be one of {0}{reason}, but it was {1}.", validValues, Subject);

        return new AndConstraint<TAssertions>((TAssertions) this);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        throw new NotSupportedException("Equals is not part of Fluent Assertions. Did you mean Be() instead?");

}

/// <summary>
/// Contains a number of methods to assert that two <see cref="ZonedDateTime"/> objects differ in the expected way.
/// </summary>
public class ZonedDateTimeRangeAssertions<TAssertions>
    where TAssertions: ZonedDateTimeAssertions<TAssertions> {

    #region Private Definitions

    private readonly TAssertions       parentAssertions;
    private readonly DurationPredicate predicate;

    private readonly Dictionary<TimeSpanCondition, DurationPredicate> predicates = new() {
        [TimeSpanCondition.MoreThan] = new DurationPredicate((ts1, ts2) => ts1 > ts2, "more than"),
        [TimeSpanCondition.AtLeast]  = new DurationPredicate((ts1, ts2) => ts1 >= ts2, "at least"),
        [TimeSpanCondition.Exactly]  = new DurationPredicate((ts1, ts2) => ts1 == ts2, "exactly"),
        [TimeSpanCondition.Within]   = new DurationPredicate((ts1, ts2) => ts1 <= ts2, "within"),
        [TimeSpanCondition.LessThan] = new DurationPredicate((ts1, ts2) => ts1 < ts2, "less than")
    };

    private readonly ZonedDateTime? subject;
    private readonly Duration       duration;

    #endregion

    protected internal ZonedDateTimeRangeAssertions(TAssertions parentAssertions, ZonedDateTime? subject, TimeSpanCondition condition, Duration duration) {
        this.parentAssertions = parentAssertions;
        this.subject          = subject;
        this.duration         = duration;

        predicate = predicates[condition];
    }

    /// <summary>
    /// Asserts that a <see cref="ZonedDateTime"/> occurs a specified amount of time before another <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="target">
    /// The <see cref="ZonedDateTime"/> to compare the subject with.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
    /// </param>
    public AndConstraint<TAssertions> Before(ZonedDateTime   target, string because = "",
                                             params object[] becauseArgs) {
        bool success = Execute.Assertion
            .ForCondition(subject.HasValue)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:the date and time) to be " + predicate.DisplayText +
                " {0} before {1}{reason}, but found a <null> DateTime.", duration, target);

        if (success) {
            Duration actual = target - subject!.Value;

            Execute.Assertion
                .ForCondition(predicate.IsMatchedBy(actual, duration))
                .BecauseOf(because, becauseArgs)
                .FailWith(
                    "Expected {context:the date and time} {0} to be " + predicate.DisplayText +
                    " {1} before {2}{reason}, but it is " + PositionRelativeToTarget(subject.Value, target) + " by {3}.",
                    subject, duration, target, actual);
        }

        return new AndConstraint<TAssertions>(parentAssertions);
    }

    /// <summary>
    /// Asserts that a <see cref="ZonedDateTime"/> occurs a specified amount of time after another <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="target">
    /// The <see cref="ZonedDateTime"/> to compare the subject with.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
    /// </param>
    public AndConstraint<TAssertions> After(ZonedDateTime target, string because = "", params object[] becauseArgs) {
        bool success = Execute.Assertion
            .ForCondition(subject.HasValue)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:the date and time} to be " + predicate.DisplayText +
                " {0} after {1}{reason}, but found a <null> DateTime.", duration, target);

        if (success) {
            Duration actual = subject!.Value - target;

            Execute.Assertion
                .ForCondition(predicate.IsMatchedBy(actual, duration))
                .BecauseOf(because, becauseArgs)
                .FailWith(
                    "Expected {context:the date and time} {0} to be " + predicate.DisplayText +
                    " {1} after {2}{reason}, but it is " + PositionRelativeToTarget(subject.Value, target) + " by {3}.",
                    subject, duration, target, actual);
        }

        return new AndConstraint<TAssertions>(parentAssertions);
    }

    private static string PositionRelativeToTarget(ZonedDateTime actual, ZonedDateTime target) {
        return actual - target >= Duration.Zero ? "ahead" : "behind";
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        throw new NotSupportedException("Equals is not part of Fluent Assertions. Did you mean Before() or After() instead?");

}

/// <summary>
/// Provides the logic and the display text for a <see cref="TimeSpanCondition"/>.
/// </summary>
internal class DurationPredicate(Func<Duration, Duration, bool> lambda, string displayText) {

    public string DisplayText { get; } = displayText;

    public bool IsMatchedBy(Duration actual, Duration expected) {
        return lambda(actual, expected) && actual >= Duration.Zero;
    }

}
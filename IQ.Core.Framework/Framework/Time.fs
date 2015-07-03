namespace IQ.Core.Framework


/// <summary>
/// Provides type aliases for the the top-level types in NodaTime
/// </summary>
/// <remarks>
/// This is strictly for convenience so the NodaTime namespade doesn't have to be opened everywhere
/// and NodaTime will be used as the fundamental vocabulary for reasoning about Time concepts
/// </remarks>
[<AutoOpen>]
module Time =
    type AmbiguousTimeException = NodaTime.AmbiguousTimeException
    type CalendarSystem = NodaTime.CalendarSystem
    type DateTimeZone = NodaTime.DateTimeZone
    type Duration = NodaTime.Duration
    type IClock = NodaTime.IClock
    type IDateTimeZoneProvider = NodaTime.IDateTimeZoneProvider
    type Instant = NodaTime.Instant
    type TimeInterval = NodaTime.Interval
    type IsoDayOfWeek = NodaTime.IsoDayOfWeek
    type LocalDate = NodaTime.LocalDate
    type LocalDateTime = NodaTime.LocalDateTime
    type LocalTime = NodaTime.LocalTime
    type TimeConstants = NodaTime.NodaConstants
    type OffsetTime = NodaTime.Offset
    type OffsetDateTime = NodaTime.OffsetDateTime
    type Period = NodaTime.Period
    type PeriodBuilder = NodaTime.PeriodBuilder
    type PeriodUnits = NodaTime.PeriodUnits
    type SkippedTimeException = NodaTime.SkippedTimeException
    type ZonedDateTime = NodaTime.ZonedDateTime
    let internal SystemClock() = NodaTime.SystemClock.Instance :> IClock

    type BclDateTime = System.DateTime
    type BclDateTimeOffset = System.DateTimeOffset
    type BclTimeSpan = System.TimeSpan
    
    
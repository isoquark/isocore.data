namespace IQ.Core.Framework


/// <summary>
/// Provides type aliases for the the top-level types in NodaTime
/// </summary>
/// <remarks>
/// This is strictly for convenience so the NodaTime namespade doesn't have to be opened everywhere
/// and NodaTime will be used as the fundamental vocabulary for reasoning about Time concepts
/// </remarks>
[<AutoOpen>]
module TimeVocabulary =
    type AmbiguousTimeException = NodaTime.AmbiguousTimeException
    type CalendarSystem = NodaTime.CalendarSystem
    type DateTimeZone = NodaTime.DateTimeZone
    type Duration = NodaTime.Duration
    
    type ITimeProvider = 
        inherit NodaTime.IClock
    
    type IDateTimeZoneProvider = NodaTime.IDateTimeZoneProvider
    type Instant = NodaTime.Instant
    type TimeInterval = NodaTime.Interval
    type IsoDayOfWeek = NodaTime.IsoDayOfWeek    

    type Date = NodaTime.LocalDate
    
    /// <summary>
    /// The concept of Date and Time as represented in the Time Vocabulary
    /// </summary>
    type DateTime = NodaTime.LocalDateTime
    
    type TimeOfDay = NodaTime.LocalTime
    
    type TimeConstants = NodaTime.NodaConstants
    
    type TimeOffset = NodaTime.Offset
    
    type DateTimeOffset = NodaTime.OffsetDateTime
    
    type Period = NodaTime.Period
    
    type PeriodBuilder = NodaTime.PeriodBuilder
    
    type PeriodUnits = NodaTime.PeriodUnits
    
    type SkippedTimeException = NodaTime.SkippedTimeException
    
    type ZonedDateTime = NodaTime.ZonedDateTime
    
   

    /// <summary>
    /// Represents a BCL tick which is based at 1 AD
    /// </summary>
    type BclTickCount =
        struct
            /// The number of ticks since 1 AD
            val Value: int64
            new (value) = { Value = value}
        end        

    /// <summary>
    /// Represents a Unix tick which is based at 1970 AD 
    /// </summary>
    type UnixTickCount =
        struct
            /// The number of ticks since 1970 AD
            val Value: int64
            new (value) = { Value = value}
        end        

//Here I'm trying really, really hard to hide the System.DateTime stuff!
namespace System
    /// <summary>
    /// The <see cref="System.DateTime"/> type defined in the .Net System namespace
    /// </summary>
    type BclDateTime = System.DateTime

    /// <summary>
    /// The <see cref="IQ.Core.Framework.TimeVocabulary.DateTime"/> defined by the Core Framework Time vocabulary
    /// </summary>
    type DateTime = IQ.Core.Framework.TimeVocabulary.DateTime
    
    /// <summary>
    /// The <see cref="System.DateTimeOffset"/> type defined in the .Net System namespace
    /// </summary>
    type BclDateTimeOffset = System.DateTimeOffset

    /// <summary>
    /// The <see cref="IQ.Core.Framework.TimeVocabulary.DateTime"/> defined by the Core Framework Time vocabulary
    /// </summary>
    type DateTimeOffset = IQ.Core.Framework.TimeVocabulary.DateTimeOffset    

    /// <summary>
    /// The <see cref="System.TimeSpan"/> type defined in the .Net System namespace
    /// </summary>
    type BclTimeSpan = System.TimeSpan
    

namespace IQ.Core.Framework

open System


module DefaultTimeProvider =
    let private clock = NodaTime.SystemClock.Instance
    let get() = 
        { new ITimeProvider with
            member this.Now = clock.Now
        }
   

module TimeConversions =
    type private Marker() = class end
    /// <summary>
    /// Specifies the <see cref="System.Type"/> of the module
    /// </summary>
    let ModuleType = typeof<Marker>.DeclaringType

    /// <summary>
    /// Converts a <see cref="Date"> to a <see cref="DateTime"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let dateToDateTime(src : Date) = DateTime(src.Year, src.Month, src.Day, 0,0,0, src.Calendar)
    
    /// <summary>
    /// Converts a <see cref="DateTime"> to a <see cref="BclDateTime"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let dateTimeToBclDateTime(src : DateTime) = src.ToDateTimeUnspecified()        

    /// <summary>
    /// Converts a <see cref="Date"/> to a <see cref="BclDateTime"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let dateToBclDateTime(src : Date) = src |> dateToDateTime |> dateTimeToBclDateTime

    /// <summary>
    /// Converts a <see cref="DateTime"> to a <see cref="Date"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let dateTimeToDate(src : DateTime) = Date(src.Year, src.Month, src.Day, src.Calendar)

    /// <summary>
    /// Converts a <see cref="BclDateTime"> to a <see cref="DateTime"/>
    /// </summary>
    /// <param name="src"></param>
    [<Transformation>]
    let bclDateTimeToDateTime(src : BclDateTime) = src |> DateTime.FromDateTime
        
    /// <summary>
    /// Converts a <see cref="BclTimeSpan"> to a <see cref="Duration"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let bclTimeSpanToDuration(src : BclTimeSpan) = src |> Duration.FromTimeSpan

    /// <summary>
    /// Converts a <see cref="BclTimeSpan"> to a <see cref="Duration"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let durationToBclTimeSpan(src : BclTimeSpan) = src |> Duration.FromTimeSpan

    /// <summary>
    /// Converts a <see cref="UnixTickCount"> to a <see cref="Duration"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let unixTicksToDuration(src : UnixTickCount) = src.Value |> Duration.FromTicks
    
    /// <summary>
    /// Converts a <see cref="Duration"> to a <see cref="UnixTickCount"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let durationToUnixTicks(src : Duration) = src.Ticks |> UnixTickCount

    /// <summary>
    /// Converts a <see cref="TimeOfDay"> to a <see cref="DateTime"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let timeOfDayToDateTime(src : TimeOfDay) = src.LocalDateTime
    
    /// <summary>
    /// Converts a <see cref="TimeOfDay"> to a <see cref="BclDateTime"/>
    /// </summary>
    /// <param name="src">The source value</param>
    [<Transformation>]
    let timeOfDayToBclDateTime(src : TimeOfDay) = src |> timeOfDayToDateTime |> dateTimeToBclDateTime

   